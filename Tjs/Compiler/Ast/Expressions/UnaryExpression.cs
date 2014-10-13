using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronTjs.Runtime.Binding;

namespace IronTjs.Compiler.Ast
{
	public class UnaryExpression : Expression
	{
		public UnaryExpression(Expression operand, TjsOperationKind expressionType)
		{
			Operand = operand;
			ExpressionType = expressionType;
			Operand.Parent = this;
		}

		public Expression Operand { get; private set; }

		public TjsOperationKind ExpressionType { get; private set; }

		public override System.Linq.Expressions.Expression TransformRead()
		{
			if (ExpressionType == TjsOperationKind.New)
			{
				var ie = Operand as InvokeExpression;
				if (ie != null)
					return System.Linq.Expressions.Expression.Dynamic(
						LanguageContext.CreateCreateBinder(new System.Dynamic.CallInfo(ie.Arguments.Count)),
						typeof(object),
						Microsoft.Scripting.Utils.ArrayUtils.Insert(ie.Target.TransformRead(), ie.Arguments.Select(x => x.TransformRead()).ToArray())
					);
				else
					throw new InvalidOperationException("new 演算子を適用できるのは関数呼び出しのみです。");
			}
			if (ExpressionType == TjsOperationKind.Delete)
				return Operand.TransformDelete();
			if (ExpressionType == TjsOperationKind.AccessPropertyObject)
				return Operand.TransformRead();
			var target = Operand.TransformRead();
			System.Linq.Expressions.ParameterExpression v = null;
			if ((ExpressionType & TjsOperationKind.PostAssign) != TjsOperationKind.None)
				v = System.Linq.Expressions.Expression.Variable(typeof(object));
			var exp = LanguageContext.DoUnaryOperation(v ?? target, ExpressionType & TjsOperationKind.ValueMask);
			if ((ExpressionType & TjsOperationKind.Assign) != TjsOperationKind.None)
				return Operand.TransformWrite(exp);
			else if ((ExpressionType & TjsOperationKind.PostAssign) != TjsOperationKind.None)
				return System.Linq.Expressions.Expression.Block(new[] { v },
					System.Linq.Expressions.Expression.Assign(v, target),
					Operand.TransformWrite(exp),
					v
				);
			return exp;
		}

		// & * は代入可能
		public override System.Linq.Expressions.Expression TransformWrite(System.Linq.Expressions.Expression value)
		{
			switch (ExpressionType)
			{
				case TjsOperationKind.AccessPropertyObject:
					return Operand.TransformWrite(value);
				case TjsOperationKind.InvokePropertyHandler:
					return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateOperationBinder(ExpressionType), typeof(object), Operand.TransformRead(), value);
				default:
					throw new InvalidOperationException(string.Format("単項演算子 '{0}' は左辺値になることはできません。", ExpressionType));
			}
		}

		public override System.Linq.Expressions.Expression TransformVoid()
		{
			if (ExpressionType == TjsOperationKind.Delete)
				return Microsoft.Scripting.Ast.Utils.Void(Operand.TransformDelete());
			switch (ExpressionType)
			{
				case TjsOperationKind.CharCodeToChar:
				case TjsOperationKind.CharToCharCode:
				case TjsOperationKind.InvokePropertyHandler:
				case TjsOperationKind.IsValid:
				case TjsOperationKind.Negate:
				case TjsOperationKind.Not:
				case TjsOperationKind.OnesComplement:
				case TjsOperationKind.AccessPropertyObject:
				case TjsOperationKind.TypeOf:
				case TjsOperationKind.UnaryPlus:
					return System.Linq.Expressions.Expression.Empty();
			}
			var exp = LanguageContext.DoUnaryOperation(Operand.TransformRead(), ExpressionType & TjsOperationKind.ValueMask);
			if ((ExpressionType & (TjsOperationKind.Assign | TjsOperationKind.PostAssign)) != TjsOperationKind.None)
				return Microsoft.Scripting.Ast.Utils.Void(Operand.TransformWrite(exp));
			return exp;
		}
	}
}
