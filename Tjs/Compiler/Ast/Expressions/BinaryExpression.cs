using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronTjs.Runtime.Binding;

namespace IronTjs.Compiler.Ast
{
	public class BinaryExpression : Expression
	{
		public BinaryExpression(Expression left, Expression right, TjsOperationKind expressionType)
		{
			Left = left;
			Right = right;
			ExpressionType = expressionType;
			Left.Parent = Right.Parent = this;
		}

		public Expression Left { get; private set; }

		public Expression Right { get; private set; }

		public TjsOperationKind ExpressionType { get; private set; }

		// TODO: Operator Action

		public override System.Linq.Expressions.Expression TransformRead()
		{
			if (ExpressionType == TjsOperationKind.Exchange)
				throw new InvalidOperationException("交換操作の結果を得ることはできません。");
			System.Linq.Expressions.Expression exp;
			if (ExpressionType == TjsOperationKind.Assign)
				exp = Right.TransformRead();
			else
				exp = LanguageContext.DoBinaryOperation(Left.TransformRead(), Right.TransformRead(), ExpressionType & TjsOperationKind.ValueMask);
			if ((ExpressionType & TjsOperationKind.Assign) != TjsOperationKind.None)
				exp = Left.TransformWrite(exp);
			return exp;
		}

		public override System.Linq.Expressions.Expression TransformWrite(System.Linq.Expressions.Expression value) { throw new InvalidOperationException("二項演算は左辺値となることはできません。"); }

		public override System.Linq.Expressions.Expression TransformVoid()
		{
			if ((ExpressionType & TjsOperationKind.Assign) != TjsOperationKind.None)
				return Microsoft.Scripting.Ast.Utils.Void(TransformRead());
			switch (ExpressionType)
			{
				case TjsOperationKind.Add:
				case TjsOperationKind.RightShiftArithmetic:
				case TjsOperationKind.And:
				case TjsOperationKind.Or:
				case TjsOperationKind.ExclusiveOr:
				case TjsOperationKind.Divide:
				case TjsOperationKind.Equal:
				case TjsOperationKind.FloorDivide:
				case TjsOperationKind.GreaterThan:
				case TjsOperationKind.GreaterThanOrEqual:
				case TjsOperationKind.InContextOf:
				case TjsOperationKind.InstanceOf:
				case TjsOperationKind.LeftShift:
				case TjsOperationKind.LessThan:
				case TjsOperationKind.LessThanOrEqual:
				case TjsOperationKind.RightShiftLogical:
				case TjsOperationKind.Modulo:
				case TjsOperationKind.Multiply:
				case TjsOperationKind.NotEqual:
				case TjsOperationKind.StrictEqual:
				case TjsOperationKind.StrictNotEqual:
				case TjsOperationKind.Subtract:
					return System.Linq.Expressions.Expression.Block(Left.TransformVoid(), Right.TransformVoid());
				case TjsOperationKind.AndAlso:
					return System.Linq.Expressions.Expression.IfThen(Left.TransformReadAsBoolean(), Right.TransformVoid());
				case TjsOperationKind.OrElse:
					return System.Linq.Expressions.Expression.IfThen(System.Linq.Expressions.Expression.Not(Left.TransformReadAsBoolean()), Right.TransformVoid());
				case TjsOperationKind.Exchange:
					{
						var v = System.Linq.Expressions.Expression.Variable(typeof(object));
						return System.Linq.Expressions.Expression.Block(new[] { v },
							System.Linq.Expressions.Expression.Assign(v, Left.TransformRead()),
							Left.TransformWrite(Right.TransformRead()),
							Right.TransformWrite(v),
							System.Linq.Expressions.Expression.Empty()
						);
					}
				default:
					throw Microsoft.Scripting.Utils.Assert.Unreachable;
			}
		}
	}
}
