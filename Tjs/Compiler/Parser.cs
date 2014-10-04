using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronTjs.Builtins;
using IronTjs.Compiler.Ast;
using IronTjs.Runtime.Binding;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronTjs.Compiler
{
	public class Parser
	{
		[Serializable]
		class InternalInvalidSyntaxException : Exception { }

		Tokenizer _tokenizer;
		CompilerContext _context;
		bool _parsedIncompleteIf = false;

		Token Accept(params TokenType[] types)
		{
			if (types.Contains(_tokenizer.NextToken.Type))
				return _tokenizer.Read();
			return null;
		}

		Token Expect(params TokenType[] types)
		{
			Assert.NotEmpty(types);
			if (types.Contains(_tokenizer.NextToken.Type))
				return _tokenizer.Read();
			var errorSpan = new SourceSpan(_tokenizer.NextToken.Span.Start, _tokenizer.NextToken.Span.Start);
			_tokenizer.ErrorSink.Add(_context.SourceUnit, "Unexpected token: " + _tokenizer.NextToken.Type, errorSpan, -1, Severity.Error);
			throw new InternalInvalidSyntaxException();
			//object dummyValue = null;
			//switch (types[0])
			//{
			//	case TokenType.Identifier:
			//	case TokenType.LiteralString:
			//		dummyValue = "__dummy__";
			//		break;
			//	case TokenType.LiteralInteger:
			//		dummyValue = 0;
			//		break;
			//	case TokenType.LiteralReal:
			//		dummyValue = 0.0;
			//		break;
			//}
			//return new Token(types[0], dummyValue, errorSpan);
		}

		public static int GetNextAutoIndentSize(string text, int autoIndentTabWidth)
		{
			ContractUtils.RequiresNotNull(text, "text");
			var lastLine = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
			if (lastLine == null)
				return 0;
			var autoIndentSize = lastLine.TakeWhile(x => x == ' ' || x == '\t').Aggregate(0, (x, y) => y == ' ' ? x + 1 : x + autoIndentTabWidth);
			if (lastLine.Trim().EndsWith("{"))
				autoIndentSize += autoIndentTabWidth;
			return autoIndentSize;
		}

		public SourceUnitTree Parse(CompilerContext context)
		{
			_tokenizer = new Tokenizer();
			_context = context;
			ErrorCounter counter = new ErrorCounter(context.Errors);
			_tokenizer.ErrorSink = counter;
			using (var reader = context.SourceUnit.GetReader())
			{
				_tokenizer.Initialize(null, reader, context.SourceUnit, SourceLocation.MinValue);
				SourceUnitTree ast;
				try { ast = Parse(); }
				catch (InternalInvalidSyntaxException) { ast = null; }
				finally
				{
					ScriptCodeParseResult props;
					if (counter.AnyError)
					{
						ast = null;
						if (_tokenizer.NextToken.Type == TokenType.EndOfStream)
							props = ScriptCodeParseResult.IncompleteStatement;
						else
							props = ScriptCodeParseResult.Invalid;
					}
					else if (_parsedIncompleteIf)
						props = ScriptCodeParseResult.IncompleteStatement;
					else
						props = ScriptCodeParseResult.Complete;
					context.SourceUnit.CodeProperties = props;
				}
				return ast;
			}
		}

		SourceUnitTree Parse()
		{
			List<ClassDefinition> classes = new List<ClassDefinition>();
			List<FunctionDefinition> functions = new List<FunctionDefinition>();
			List<PropertyDefinition> properties = new List<PropertyDefinition>();
			List<Statement> statements = new List<Statement>();
			while (_tokenizer.NextToken.Type != TokenType.EndOfStream)
			{
				if (Accept(TokenType.KeywordClass) != null)
				{
					classes.Add(ParseClassDefinition());
					_parsedIncompleteIf = false;
				}
				else if (Accept(TokenType.KeywordFunction) != null)
				{
					functions.Add(ParseFunctionDefinition());
					_parsedIncompleteIf = false;
				}
				else if (Accept(TokenType.KeywordProperty) != null)
				{
					properties.Add(ParsePropertyDefinition());
					_parsedIncompleteIf = false;
				}
				else
					statements.Add(ParseStatement());
			}
			return new SourceUnitTree(classes, functions, properties, statements, _context);
		}

		ClassDefinition ParseClassDefinition()
		{
			var name = Expect(TokenType.Identifier).Value.ToString();
			List<string> baseClasses = new List<string>();
			if (Accept(TokenType.KeywordExtends) != null)
			{
				do
					baseClasses.Add(Expect(TokenType.Identifier).Value.ToString());
				while (Accept(TokenType.SymbolComma) != null);
			}
			List<ClassDefinition> classes = new List<ClassDefinition>();
			List<FunctionDefinition> functions = new List<FunctionDefinition>();
			List<PropertyDefinition> properties = new List<PropertyDefinition>();
			List<VariableDeclarationExpression> variableDeclarations = new List<VariableDeclarationExpression>();
			Expect(TokenType.SymbolOpenBrace);
			while (true)
			{
				if (Accept(TokenType.KeywordClass) != null)
					classes.Add(ParseClassDefinition());
				else if (Accept(TokenType.KeywordFunction) != null)
					functions.Add(ParseFunctionDefinition());
				else if (Accept(TokenType.KeywordProperty) != null)
					properties.Add(ParsePropertyDefinition());
				else if (Accept(TokenType.KeywordVar) != null)
				{
					var varDec = ParseVariableDeclaration();
					Expect(TokenType.SymbolSemicolon);
					variableDeclarations.Add(varDec);
				}
				else
					break;
			}
			Expect(TokenType.SymbolCloseBrace);
			return new ClassDefinition(name, baseClasses, classes, functions, properties, variableDeclarations);
		}

		FunctionDefinition ParseFunctionDefinition()
		{
			var name = Expect(TokenType.Identifier).Value.ToString();
			List<ParameterDefinition> parameters = new List<ParameterDefinition>();
			if (Accept(TokenType.SymbolOpenParenthesis) != null)
			{
				while (true)
				{
					var token = Accept(TokenType.SymbolCloseParenthesis, TokenType.SymbolComma);
					if (token != null && token.Type == TokenType.SymbolCloseParenthesis)
						break;
					else if (Accept(TokenType.SymbolAsterisk) != null)
						parameters.Add(new ParameterDefinition(null, true));
					else
					{
						var paramName = Expect(TokenType.Identifier).Value.ToString();
						if (Accept(TokenType.SymbolEquals) != null)
							parameters.Add(new ParameterDefinition(paramName, ParseLiteral()));
						else if (Accept(TokenType.SymbolAsterisk) != null)
							parameters.Add(new ParameterDefinition(paramName, true));
						else
							parameters.Add(new ParameterDefinition(paramName, false));
					}
				}
			}
			List<Statement> statements = new List<Statement>();
			Expect(TokenType.SymbolOpenBrace);
			while (Accept(TokenType.SymbolCloseBrace) == null)
				statements.Add(ParseStatement());
			return new FunctionDefinition(name, parameters, statements);
		}

		PropertyDefinition ParsePropertyDefinition()
		{
			var name = Expect(TokenType.Identifier).Value.ToString();
			FunctionDefinition getter = null;
			FunctionDefinition setter = null;
			Expect(TokenType.SymbolOpenBrace);
			if (Accept(TokenType.KeywordGetter) != null)
			{
				getter = ParsePropertyGetter();
				if (Accept(TokenType.KeywordSetter) != null)
					setter = ParsePropertySetter();
			}
			else
			{
				Expect(TokenType.KeywordSetter);
				setter = ParsePropertySetter();
				if (Accept(TokenType.KeywordGetter) != null)
					getter = ParsePropertyGetter();
			}
			Expect(TokenType.SymbolCloseBrace);
			return new PropertyDefinition(name, getter, setter);
		}

		FunctionDefinition ParsePropertyGetter()
		{
			if (Accept(TokenType.SymbolOpenParenthesis) != null)
				Expect(TokenType.SymbolCloseParenthesis);
			List<Statement> body = new List<Statement>();
			Expect(TokenType.SymbolOpenBrace);
			while (Accept(TokenType.SymbolCloseBrace) == null)
				body.Add(ParseStatement());
			return new FunctionDefinition("getter", new ParameterDefinition[0], body);
		}

		FunctionDefinition ParsePropertySetter()
		{
			Expect(TokenType.SymbolOpenParenthesis);
			var name = Expect(TokenType.Identifier).Value.ToString();
			Expect(TokenType.SymbolCloseParenthesis);
			List<Statement> body = new List<Statement>();
			Expect(TokenType.SymbolOpenBrace);
			while (Accept(TokenType.SymbolCloseBrace) == null)
				body.Add(ParseStatement());
			return new FunctionDefinition("setter", new[] { new ParameterDefinition(name, false) } , body);
		}

		Statement ParseStatement()
		{
			if (Accept(TokenType.SymbolOpenBrace) != null)
			{
				List<Statement> statements = new List<Statement>();
				while (Accept(TokenType.SymbolCloseBrace) == null)
					statements.Add(ParseStatement());
				_parsedIncompleteIf = false;
				return new Block(statements);
			}
			else if (Accept(TokenType.KeywordIf) != null)
			{
				Expect(TokenType.SymbolOpenParenthesis);
				var test = ParseExpression();
				Expect(TokenType.SymbolCloseParenthesis);
				var ifTrue = ParseStatement();
				_parsedIncompleteIf = true;
				Statement ifFalse = null;
				if (Accept(TokenType.KeywordElse) != null)
				{
					ifFalse = ParseStatement();
					_parsedIncompleteIf = false;
				}
				return new IfStatement(test, ifTrue, ifFalse);
			}
			else if (Accept(TokenType.KeywordSwitch) != null)
			{
				Expect(TokenType.SymbolOpenParenthesis);
				var cond = ParseExpression();
				Expect(TokenType.SymbolCloseParenthesis);
				Expect(TokenType.SymbolOpenBrace);
				List<SwitchCase> cases = new List<SwitchCase>();
				List<Expression> testExpressions = new List<Expression>();
				List<Statement> statements = new List<Statement>();
				bool containsDefault = false;
				while (true)
				{
					var result = Accept(TokenType.KeywordCase, TokenType.KeywordDefault, TokenType.SymbolCloseBrace);
					if (result != null && statements.Count > 0)
					{
						cases.Add(new SwitchCase(testExpressions, containsDefault, statements));
						testExpressions.Clear();
						statements.Clear();
						containsDefault = false;
					}
					if (result == null)
						statements.Add(ParseStatement());
					else if (result.Type == TokenType.KeywordCase)
					{
						testExpressions.Add(ParseExpression());
						Expect(TokenType.SymbolColon);
					}
					else if (result.Type == TokenType.KeywordDefault)
					{
						containsDefault = true;
						Expect(TokenType.SymbolColon);
					}
					else
					{
						_parsedIncompleteIf = false;
						return new SwitchStatement(cond, cases);
					}
				}
			}
			else if (Accept(TokenType.KeywordWhile) != null)
			{
				Expect(TokenType.SymbolOpenParenthesis);
				var cond = ParseExpression();
				Expect(TokenType.SymbolCloseParenthesis);
				var body = ParseStatement();
				_parsedIncompleteIf = false;
				return new WhileStatement(cond, body);
			}
			else if (Accept(TokenType.KeywordDo) != null)
			{
				var body = ParseStatement();
				Expect(TokenType.KeywordWhile);
				Expect(TokenType.SymbolOpenParenthesis);
				var cond = ParseExpression();
				Expect(TokenType.SymbolCloseParenthesis);
				Expect(TokenType.SymbolSemicolon);
				_parsedIncompleteIf = false;
				return new DoWhileStatement(body, cond);
			}
			else if (Accept(TokenType.KeywordFor) != null)
			{
				Expression init = null;
				Expression condition = null;
				Expression update = null;
				Expect(TokenType.SymbolOpenParenthesis);
				if (Accept(TokenType.SymbolSemicolon) == null)
				{
					if (Accept(TokenType.KeywordVar) != null)
						init = ParseVariableDeclaration();
					else
						init = ParseExpression();
					Expect(TokenType.SymbolSemicolon);
				}
				if (Accept(TokenType.SymbolSemicolon) == null)
				{
					condition = ParseExpression();
					Expect(TokenType.SymbolSemicolon);
				}
				if (Accept(TokenType.SymbolCloseParenthesis) == null)
				{
					update = ParseExpression();
					Expect(TokenType.SymbolCloseParenthesis);
				}
				var body = ParseStatement();
				_parsedIncompleteIf = false;
				return new ForStatement(init, condition, update, body);
			}
			else if (Accept(TokenType.KeywordTry) != null)
			{
				var body = ParseStatement();
				Expect(TokenType.KeywordCatch);
				string catchVariableName = null;
				if (Accept(TokenType.SymbolOpenParenthesis) != null)
				{
					catchVariableName = Expect(TokenType.Identifier).Value.ToString();
					Expect(TokenType.SymbolCloseParenthesis);
				}
				var catchBody = ParseStatement();
				_parsedIncompleteIf = false;
				return new TryStatement(body, new CatchBlock(catchVariableName, catchBody));
			}
			else if (Accept(TokenType.KeywordWith) != null)
			{
				Expect(TokenType.SymbolOpenParenthesis);
				var exp = ParseExpression();
				Expect(TokenType.SymbolCloseParenthesis);
				var body = ParseStatement();
				_parsedIncompleteIf = false;
				return new WithStatement(exp, body);
			}
			else if (Accept(TokenType.KeywordBreak) != null)
			{
				Expect(TokenType.SymbolSemicolon);
				return new BreakStatement();
			}
			else if (Accept(TokenType.KeywordContinue) != null)
			{
				Expect(TokenType.SymbolSemicolon);
				return new ContinueStatement();
			}
			else if (Accept(TokenType.KeywordReturn) != null)
			{
				var exp = ParseExpression();
				Expect(TokenType.SymbolSemicolon);
				return new ReturnStatement(exp);
			}
			else if (Accept(TokenType.KeywordThrow) != null)
			{
				var exp = ParseExpression();
				Expect(TokenType.SymbolSemicolon);
				return new ThrowStatement(exp);
			}
			else if (Accept(TokenType.KeywordVar) != null)
			{
				var varDec = ParseVariableDeclaration();
				Expect(TokenType.SymbolSemicolon);
				return new ExpressionStatement(varDec);
			}
			else if (Accept(TokenType.SymbolSemicolon) != null)
				return new EmptyStatement();
			else
			{
				var exp = ParseExpression();
				Expect(TokenType.SymbolSemicolon);
				return new ExpressionStatement(exp);
			}
		}

		VariableDeclarationExpression ParseVariableDeclaration()
		{
			List<KeyValuePair<string, Expression>> initializers = new List<KeyValuePair<string, Expression>>();
			do
			{
				var name = Expect(TokenType.Identifier).Value.ToString();
				Expression exp = null;
				if (Accept(TokenType.SymbolEquals) != null)
					exp = ParseAssignmentExpression();
				initializers.Add(new KeyValuePair<string, Expression>(name, exp));
			} while (Accept(TokenType.SymbolComma) != null);
			return new VariableDeclarationExpression(initializers);
		}

		Expression ParseExpression()
		{
			var exp = ParseSequenceExpression();
			if (Accept(TokenType.KeywordIf) != null)
				return new IfExpression(exp, ParseSequenceExpression());
			return exp;
		}

		Expression ParseSequenceExpression()
		{
			List<Expression> exps = new List<Expression>();
			exps.Add(ParseAssignmentExpression());
			while (Accept(TokenType.SymbolComma) != null)
				exps.Add(ParseAssignmentExpression());
			if (exps.Count > 1)
				return new SequenceExpression(exps);
			else
				return exps[0];
		}

		Expression ParseAssignmentExpression()
		{
			var exp = ParseConditionalExpression();
			if (Accept(TokenType.SymbolEquals) != null)
				return new BinaryExpression(exp, ParseAssignmentExpression(), TjsOperationKind.Assign);
			else if (Accept(TokenType.SymbolAsteriskEquals) != null)
				return new BinaryExpression(exp, ParseAssignmentExpression(), TjsOperationKind.MultiplyAssign);
			else if (Accept(TokenType.SymbolSlashEquals) != null)
				return new BinaryExpression(exp, ParseAssignmentExpression(), TjsOperationKind.DivideAssign);
			else if (Accept(TokenType.SymbolBackSlashEquals) != null)
				return new BinaryExpression(exp, ParseAssignmentExpression(), TjsOperationKind.FloorDivideAssign);
			else if (Accept(TokenType.SymbolPercentEquals) != null)
				return new BinaryExpression(exp, ParseAssignmentExpression(), TjsOperationKind.ModuloAssign);
			else if (Accept(TokenType.SymbolPlusEquals) != null)
				return new BinaryExpression(exp, ParseAssignmentExpression(), TjsOperationKind.AddAssign);
			else if (Accept(TokenType.SymbolMinusEquals) != null)
				return new BinaryExpression(exp, ParseAssignmentExpression(), TjsOperationKind.SubtractAssign);
			else if (Accept(TokenType.SymbolDoubleLessThanEquals) != null)
				return new BinaryExpression(exp, ParseAssignmentExpression(), TjsOperationKind.LeftShiftAssign);
			else if (Accept(TokenType.SymbolDoubleGreaterThanEquals) != null)
				return new BinaryExpression(exp, ParseAssignmentExpression(), TjsOperationKind.RightShiftArithmeticAssign);
			else if (Accept(TokenType.SymbolTripleGreaterThanEquals) != null)
				return new BinaryExpression(exp, ParseAssignmentExpression(), TjsOperationKind.RightShiftLogicalAssign);
			else if (Accept(TokenType.SymbolAmpersandEquals) != null)
				return new BinaryExpression(exp, ParseAssignmentExpression(), TjsOperationKind.AndAssign);
			else if (Accept(TokenType.SymbolCircumflexEquals) != null)
				return new BinaryExpression(exp, ParseAssignmentExpression(), TjsOperationKind.ExclusiveOrAssign);
			else if (Accept(TokenType.SymbolVerticalLineEquals) != null)
				return new BinaryExpression(exp, ParseAssignmentExpression(), TjsOperationKind.OrAssign);
			else if (Accept(TokenType.SymbolDoubleAmpersandEquals) != null)
				return new BinaryExpression(exp, ParseAssignmentExpression(), TjsOperationKind.AndAlsoAssign);
			else if (Accept(TokenType.SymbolDoubleVerticalLineEquals) != null)
				return new BinaryExpression(exp, ParseAssignmentExpression(), TjsOperationKind.OrElseAssign);
			else if (Accept(TokenType.SymbolLessThanMinusGreaterThan) != null)
				return new BinaryExpression(exp, ParseAssignmentExpression(), TjsOperationKind.Exchange);
			else
				return exp;
		}

		Expression ParseConditionalExpression()
		{
			var exp = ParseLogicalOrExpression();
			if (Accept(TokenType.SymbolQuestion) != null)
			{
				var ifTrue = ParseAssignmentExpression();
				Expect(TokenType.SymbolColon);
				var ifFalse = ParseAssignmentExpression();
				return new ConditionalExpression(exp, ifTrue, ifFalse);
			}
			return exp;
		}

		Expression ParseLogicalOrExpression()
		{
			var exp = ParseLogicalAndExpression();
			while (Accept(TokenType.SymbolDoubleVerticalLine) != null)
				exp = new BinaryExpression(exp, ParseLogicalAndExpression(), TjsOperationKind.OrElse);
			return exp;
		}

		Expression ParseLogicalAndExpression()
		{
			var exp = ParseBitwiseOrExpression();
			while (Accept(TokenType.SymbolDoubleAmpersand) != null)
				exp = new BinaryExpression(exp, ParseBitwiseOrExpression(), TjsOperationKind.AndAlso);
			return exp;
		}

		Expression ParseBitwiseOrExpression()
		{
			var exp = ParseBitwiseXorExpression();
			while (Accept(TokenType.SymbolVerticalLine) != null)
				exp = new BinaryExpression(exp, ParseBitwiseXorExpression(), TjsOperationKind.Or);
			return exp;
		}

		Expression ParseBitwiseXorExpression()
		{
			var exp = ParseBitwiseAndExpression();
			while (Accept(TokenType.SymbolCircumflex) != null)
				exp = new BinaryExpression(exp, ParseBitwiseAndExpression(), TjsOperationKind.ExclusiveOr);
			return exp;
		}

		Expression ParseBitwiseAndExpression()
		{
			var exp = ParseEqualityExpression();
			while (Accept(TokenType.SymbolAmpersand) != null)
				exp = new BinaryExpression(exp, ParseEqualityExpression(), TjsOperationKind.And);
			return exp;
		}

		Expression ParseEqualityExpression()
		{
			var exp = ParseRelationalExpression();
			while (true)
			{
				if (Accept(TokenType.SymbolDoubleEquals) != null)
					exp = new BinaryExpression(exp, ParseRelationalExpression(), TjsOperationKind.Equal);
				else if (Accept(TokenType.SymbolExclamationEquals) != null)
					exp = new BinaryExpression(exp, ParseRelationalExpression(), TjsOperationKind.NotEqual);
				else if (Accept(TokenType.SymbolTripleEquals) != null)
					exp = new BinaryExpression(exp, ParseRelationalExpression(), TjsOperationKind.DistinctEqual);
				else if (Accept(TokenType.SymbolExclamationDoubleEquals) != null)
					exp = new BinaryExpression(exp, ParseRelationalExpression(), TjsOperationKind.DistinctNotEqual);
				else
					break;
			}
			return exp;
		}

		Expression ParseRelationalExpression()
		{
			var exp = ParseShiftExpression();
			while (true)
			{
				if (Accept(TokenType.SymbolLessThan) != null)
					exp = new BinaryExpression(exp, ParseShiftExpression(), TjsOperationKind.LessThan);
				else if (Accept(TokenType.SymbolLessThanEquals) != null)
					exp = new BinaryExpression(exp, ParseShiftExpression(), TjsOperationKind.LessThanOrEqual);
				else if (Accept(TokenType.SymbolGreaterThan) != null)
					exp = new BinaryExpression(exp, ParseShiftExpression(), TjsOperationKind.GreaterThan);
				else if (Accept(TokenType.SymbolGreaterThanEquals) != null)
					exp = new BinaryExpression(exp, ParseShiftExpression(), TjsOperationKind.GreaterThanOrEqual);
				else
					break;
			}
			return exp;
		}

		Expression ParseShiftExpression()
		{
			var exp = ParseAdditiveExpression();
			while (true)
			{
				if (Accept(TokenType.SymbolDoubleLessThan) != null)
					exp = new BinaryExpression(exp, ParseAdditiveExpression(), TjsOperationKind.LeftShift);
				else if (Accept(TokenType.SymbolDoubleGreaterThan) != null)
					exp = new BinaryExpression(exp, ParseAdditiveExpression(), TjsOperationKind.RightShiftArithmetic);
				else if (Accept(TokenType.SymbolTripleGreaterThan) != null)
					exp = new BinaryExpression(exp, ParseAdditiveExpression(), TjsOperationKind.RightShiftLogical);
				else
					break;
			}
			return exp;
		}

		Expression ParseAdditiveExpression()
		{
			var exp = ParseMultiplicativeExpression();
			while (true)
			{
				if (Accept(TokenType.SymbolPlus) != null)
					exp = new BinaryExpression(exp, ParseMultiplicativeExpression(), TjsOperationKind.Add);
				else if (Accept(TokenType.SymbolMinus) != null)
					exp = new BinaryExpression(exp, ParseMultiplicativeExpression(), TjsOperationKind.Subtract);
				else
					break;
			}
			return exp;
		}

		Expression ParseMultiplicativeExpression()
		{
			var exp = ParsePrefixExpression();
			while (true)
			{
				if (Accept(TokenType.SymbolAsterisk) != null)
					exp = new BinaryExpression(exp, ParsePrefixExpression(), TjsOperationKind.Multiply);
				else if (Accept(TokenType.SymbolSlash) != null)
					exp = new BinaryExpression(exp, ParsePrefixExpression(), TjsOperationKind.Divide);
				else if (Accept(TokenType.SymbolBackSlash) != null)
					exp = new BinaryExpression(exp, ParsePrefixExpression(), TjsOperationKind.FloorDivide);
				else if (Accept(TokenType.SymbolPercent) != null)
					exp = new BinaryExpression(exp, ParsePrefixExpression(), TjsOperationKind.Modulo);
				else
					break;
			}
			return exp;
		}

		Expression ParsePrefixExpression()
		{
			if (Accept(TokenType.SymbolExclamation) != null)
				return new UnaryExpression(ParsePrefixExpression(), TjsOperationKind.Not);
			else if (Accept(TokenType.SymbolTilde) != null)
				return new UnaryExpression(ParsePrefixExpression(), TjsOperationKind.OnesComplement);
			else if (Accept(TokenType.SymbolNumberSign) != null)
				return new UnaryExpression(ParsePrefixExpression(), TjsOperationKind.CharToCharCode);
			else if (Accept(TokenType.SymbolDollarSign) != null)
				return new UnaryExpression(ParsePrefixExpression(), TjsOperationKind.CharCodeToChar);
			else if (Accept(TokenType.SymbolPlus) != null)
				return new UnaryExpression(ParsePrefixExpression(), TjsOperationKind.UnaryPlus);
			else if (Accept(TokenType.SymbolMinus) != null)
				return new UnaryExpression(ParsePrefixExpression(), TjsOperationKind.Negate);
			else if (Accept(TokenType.SymbolAmpersand) != null)
				return new UnaryExpression(ParsePrefixExpression(), TjsOperationKind.AccessPropertyObject);
			else if (Accept(TokenType.SymbolAsterisk) != null)
				return new UnaryExpression(ParsePrefixExpression(), TjsOperationKind.InvokePropertyHandler);
			else if (Accept(TokenType.SymbolDoublePlus) != null)
				return new UnaryExpression(ParsePrefixExpression(), TjsOperationKind.PreIncrementAssign);
			else if (Accept(TokenType.SymbolDoubleMinus) != null)
				return new UnaryExpression(ParsePrefixExpression(), TjsOperationKind.PreDecrementAssign);
			else if (Accept(TokenType.KeywordNew) != null)
				return new UnaryExpression(ParsePrefixExpression(), TjsOperationKind.New);
			else if (Accept(TokenType.KeywordInvalidate) != null)
				return new UnaryExpression(ParsePrefixExpression(), TjsOperationKind.Invalidate);
			else if (Accept(TokenType.KeywordIsValid) != null)
				return new UnaryExpression(ParsePrefixExpression(), TjsOperationKind.IsValid);
			else if (Accept(TokenType.KeywordDelete) != null)
				return new UnaryExpression(ParsePrefixExpression(), TjsOperationKind.Delete);
			else if (Accept(TokenType.KeywordTypeOf) != null)
				return new UnaryExpression(ParsePrefixExpression(), TjsOperationKind.TypeOf);
			else if (Accept(TokenType.KeywordInt) != null)
				return new ConvertExpression(ParsePrefixExpression(), ConvertType.Integer);
			else if (Accept(TokenType.KeywordReal) != null)
				return new ConvertExpression(ParsePrefixExpression(), ConvertType.Real);
			else if (Accept(TokenType.KeywordString) != null)
				return new ConvertExpression(ParsePrefixExpression(), ConvertType.String);
			else
				return ParseClassInstanceExpression();
		}

		Expression ParseClassInstanceExpression()
		{
			var exp = ParseValidationExpression();
			while (Accept(TokenType.KeywordInstanceOf) != null)
				exp = new BinaryExpression(exp, ParseValidationExpression(), TjsOperationKind.InstanceOf);
			return exp;
		}

		Expression ParseValidationExpression()
		{
			var exp = ParseContextExpression();
			if (Accept(TokenType.KeywordIsValid) != null)
				return new UnaryExpression(exp, TjsOperationKind.IsValid);
			return exp;
		}

		Expression ParseContextExpression()
		{
			var exp = ParsePostfixExpression();
			while (Accept(TokenType.KeywordInContextOf) != null)
				exp = new BinaryExpression(exp, ParsePostfixExpression(), TjsOperationKind.InContextOf);
			return exp;
		}

		Expression ParsePostfixExpression()
		{
			var exp = ParseMemberExpression();
			while (true)
			{
				if (Accept(TokenType.SymbolDoublePlus) != null)
					exp = new UnaryExpression(exp, TjsOperationKind.PostIncrementAssign);
				else if (Accept(TokenType.SymbolDoubleMinus) != null)
					exp = new UnaryExpression(exp, TjsOperationKind.PostDecrementAssign);
				else if (Accept(TokenType.SymbolExclamation) != null)
					exp = new UnaryExpression(exp, TjsOperationKind.Evaluate);
				else
					break;
			}
			return exp;
		}

		Expression ParseMemberExpression()
		{
			Expression exp;
			if (Accept(TokenType.SymbolPeriod) != null)
				exp = new DirectMemberAccessExpression(null, Expect(TokenType.Identifier).Value.ToString());
			else
				exp = ParsePrimaryExpression();
			while (true)
			{
				if (Accept(TokenType.SymbolPeriod) != null)
					exp = new DirectMemberAccessExpression(exp, Expect(TokenType.Identifier).Value.ToString());
				else if (Accept(TokenType.SymbolOpenBracket) != null)
				{
					exp = new IndirectMemberAccessExpression(exp, ParseExpression());
					Expect(TokenType.SymbolCloseBracket);
				}
				else if (Accept(TokenType.SymbolOpenParenthesis) != null)
				{
					List<Expression> arguments = new List<Expression>();
					if (Accept(TokenType.SymbolCloseParenthesis) == null)
					{
						if (_tokenizer.NextToken.Type != TokenType.SymbolComma)
							arguments.Add(ParseAssignmentExpression());
						else
							arguments.Add(null);
						while (Accept(TokenType.SymbolComma) != null)
						{
							if (_tokenizer.NextToken.Type != TokenType.SymbolComma && _tokenizer.NextToken.Type != TokenType.SymbolCloseParenthesis)
								arguments.Add(ParseAssignmentExpression());
							else
								arguments.Add(null);
						}
						Expect(TokenType.SymbolCloseParenthesis);
					}
					exp = new InvokeExpression(exp, arguments);
				}
				else
					break;
			}
			return exp;
		}

		Expression ParsePrimaryExpression()
		{
			var identifier = Accept(TokenType.Identifier);
			if (identifier != null)
				return new IdentifierExpression(identifier.Value.ToString());
			else if (Accept(TokenType.KeywordSuper) != null)
				return new SuperExpression();
			else if (Accept(TokenType.KeywordGlobal) != null)
				return new GlobalExpression();
			else if (Accept(TokenType.KeywordThis) != null)
				return new ThisExpression();
			else if (Accept(TokenType.SymbolOpenParenthesis) != null)
			{
				var exp = ParseExpression();
				Expect(TokenType.SymbolCloseParenthesis);
				return exp;
			}
			else
				return new ConstantExpression(ParseLiteral());
		}

		object ParseLiteral()
		{
			Token token;
			if (Accept(TokenType.KeywordTrue) != null)
				return 1L;
			else if (Accept(TokenType.KeywordFalse) != null)
				return 0L;
			else if (Accept(TokenType.KeywordNull) != null)
				return null;
			else if (Accept(TokenType.KeywordVoid) != null)
				return IronTjs.Builtins.TjsVoid.Value;
			else if ((token = Accept(TokenType.LiteralInteger, TokenType.LiteralReal)) != null)
				return token.Value;
			else
			{
				token = Expect(TokenType.LiteralString);
				StringBuilder sb = new StringBuilder();
				do
					sb.Append(token.Value.ToString());
				while ((token = Accept(TokenType.LiteralString)) != null);
				return sb.ToString();
			}
		}
	}
}
