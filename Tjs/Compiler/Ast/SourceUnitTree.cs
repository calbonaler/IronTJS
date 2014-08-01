using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronTjs.Runtime;
using IronTjs.Runtime.Binding;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronTjs.Compiler.Ast
{
	public class SourceUnitTree : Node, INameResolver
	{
		public SourceUnitTree(IEnumerable<ClassDefinition> classes, IEnumerable<FunctionDefinition> functions, IEnumerable<PropertyDefinition> properties, IEnumerable<Statement> statements, CompilerContext context)
		{
			Classes = classes.ToReadOnly();
			foreach (var cls in Classes)
				cls.Parent = this;
			Functions = functions.ToReadOnly();
			foreach (var function in Functions)
				function.Parent = this;
			Properties = properties.ToReadOnly();
			foreach (var prop in Properties)
				prop.Parent = this;
			Statements = statements.ToReadOnly();
			foreach (var statement in Statements)
				statement.Parent = this;
			GlobalObject = System.Linq.Expressions.Expression.Parameter(typeof(object), "global");
			CompilerContext = context;
		}

		public CompilerContext CompilerContext { get; private set; }

		public ReadOnlyCollection<ClassDefinition> Classes { get; private set; }

		public ReadOnlyCollection<FunctionDefinition> Functions { get; private set; }

		public ReadOnlyCollection<PropertyDefinition> Properties { get; private set; }

		public ReadOnlyCollection<Statement> Statements { get; private set; }

		public System.Linq.Expressions.ParameterExpression GlobalObject { get; private set; }

		public System.Linq.Expressions.Expression<TDelegate> Transform<TDelegate>() where TDelegate : class
		{
			var signature = Utils.GetDelegateSignature(typeof(TDelegate));
			bool hasResult;
			if (signature.Length == 2 && signature[0] == typeof(object) && ((hasResult = signature[1] == typeof(object)) || signature[1] == typeof(void)))
			{
				List<System.Linq.Expressions.Expression> exps = new List<System.Linq.Expressions.Expression>();
				for (int i = 0; i < Functions.Count; i++)
					exps.Add(Functions[i].Register(GlobalObject));
				for (int i = 0; i < Statements.Count - 1; i++)
					exps.Add(Statements[i].Transform());
				if (Statements.Count > 0)
				{
					ExpressionStatement lastExp;
					if (!hasResult)
						exps.Add(Statements[Statements.Count - 1].Transform());
					else if ((lastExp = Statements[Statements.Count - 1] as ExpressionStatement) != null)
						exps.Add(lastExp.Expression.TransformRead());
					else
					{
						exps.Add(Statements[Statements.Count - 1].Transform());
						exps.Add(System.Linq.Expressions.Expression.Constant(Builtins.TjsVoid.Value));
					}
				}
				System.Linq.Expressions.Expression body;
				if (exps.Count > 1)
					body = System.Linq.Expressions.Expression.Block(exps);
				else if (exps.Count == 1)
					body = exps[0];
				else if (hasResult)
					body = System.Linq.Expressions.Expression.Constant(Builtins.TjsVoid.Value);
				else
					body = System.Linq.Expressions.Expression.Empty();
				return System.Linq.Expressions.Expression.Lambda<TDelegate>(body, GlobalObject);
			}
			throw new ArgumentException("無効なデリゲート型です。");
		}

		public System.Linq.Expressions.Expression ResolveForRead(string name)
		{
			return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateGetMemberBinder(name, false), typeof(object), GlobalObject);
		}

		public System.Linq.Expressions.Expression ResolveForWrite(string name, System.Linq.Expressions.Expression value)
		{
			return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateSetMemberBinder(name, false, false), typeof(object), GlobalObject, value);
		}

		public System.Linq.Expressions.Expression DeclareVariable(string name, System.Linq.Expressions.Expression value)
		{
			return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateSetMemberBinder(name, false, true), typeof(object), GlobalObject, value);
		}
	}
}
