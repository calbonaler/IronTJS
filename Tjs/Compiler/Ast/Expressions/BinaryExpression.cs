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
		public BinaryExpression(Expression left, Expression right, BinaryExpressionType expressionType)
		{
			Left = left;
			Right = right;
			ExpressionType = expressionType;
			Left.Parent = Right.Parent = this;
		}

		public Expression Left { get; private set; }

		public Expression Right { get; private set; }

		public BinaryExpressionType ExpressionType { get; private set; }

		// TODO: Operator Action

		public override System.Linq.Expressions.Expression TransformRead()
		{
			switch (ExpressionType)
			{
				case BinaryExpressionType.Assign:
					return Left.TransformWrite(Right.TransformRead());
				case BinaryExpressionType.Exchange:
					throw new InvalidOperationException("交換操作の結果を得ることはできません。");
				case BinaryExpressionType.Add:
					return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateBinaryOperationBinder(System.Linq.Expressions.ExpressionType.Add), typeof(object), Left.TransformRead(), Right.TransformRead());
				case BinaryExpressionType.ArithmeticRightShift:
					return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateBinaryOperationBinder(System.Linq.Expressions.ExpressionType.RightShift), typeof(object), Left.TransformRead(), Right.TransformRead());
				case BinaryExpressionType.BitwiseAnd:
					return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateBinaryOperationBinder(System.Linq.Expressions.ExpressionType.And), typeof(object), Left.TransformRead(), Right.TransformRead());
				case BinaryExpressionType.BitwiseOr:
					return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateBinaryOperationBinder(System.Linq.Expressions.ExpressionType.Or), typeof(object), Left.TransformRead(), Right.TransformRead());
				case BinaryExpressionType.BitwiseXor:
					return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateBinaryOperationBinder(System.Linq.Expressions.ExpressionType.ExclusiveOr), typeof(object), Left.TransformRead(), Right.TransformRead());
				case BinaryExpressionType.Divide:
					return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateBinaryOperationBinder(System.Linq.Expressions.ExpressionType.Divide), typeof(object), Left.TransformRead(), Right.TransformRead());
				case BinaryExpressionType.Equal:
					return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateBinaryOperationBinder(System.Linq.Expressions.ExpressionType.Equal), typeof(object), Left.TransformRead(), Right.TransformRead());
				case BinaryExpressionType.FloorDivide:
					throw new NotImplementedException();
				case BinaryExpressionType.GreaterThan:
					return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateBinaryOperationBinder(System.Linq.Expressions.ExpressionType.GreaterThan), typeof(object), Left.TransformRead(), Right.TransformRead());
				case BinaryExpressionType.GreaterThanOrEqual:
					return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateBinaryOperationBinder(System.Linq.Expressions.ExpressionType.GreaterThanOrEqual), typeof(object), Left.TransformRead(), Right.TransformRead());
				case BinaryExpressionType.InContextOf:
					throw new NotImplementedException();
				case BinaryExpressionType.InstanceOf:
					throw new NotImplementedException();
				case BinaryExpressionType.LeftShift:
					return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateBinaryOperationBinder(System.Linq.Expressions.ExpressionType.LeftShift), typeof(object), Left.TransformRead(), Right.TransformRead());
				case BinaryExpressionType.LessThan:
					return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateBinaryOperationBinder(System.Linq.Expressions.ExpressionType.LessThan), typeof(object), Left.TransformRead(), Right.TransformRead());
				case BinaryExpressionType.LessThanOrEqual:
					return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateBinaryOperationBinder(System.Linq.Expressions.ExpressionType.LessThanOrEqual), typeof(object), Left.TransformRead(), Right.TransformRead());
				case BinaryExpressionType.LogicalAnd:
					throw new NotImplementedException();
				case BinaryExpressionType.LogicalOr:
					throw new NotImplementedException();
				case BinaryExpressionType.LogicalRightShift:
					throw new NotImplementedException();
				case BinaryExpressionType.Modulo:
					return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateBinaryOperationBinder(System.Linq.Expressions.ExpressionType.Modulo), typeof(object), Left.TransformRead(), Right.TransformRead());
				case BinaryExpressionType.Multiply:
					return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateBinaryOperationBinder(System.Linq.Expressions.ExpressionType.Multiply), typeof(object), Left.TransformRead(), Right.TransformRead());
				case BinaryExpressionType.NotEqual:
					return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateBinaryOperationBinder(System.Linq.Expressions.ExpressionType.NotEqual), typeof(object), Left.TransformRead(), Right.TransformRead());
				case BinaryExpressionType.StrictEqual:
					throw new NotImplementedException();
				case BinaryExpressionType.StrictNotEqual:
					throw new NotImplementedException();
				case BinaryExpressionType.Subtract:
					return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateBinaryOperationBinder(System.Linq.Expressions.ExpressionType.Subtract), typeof(object), Left.TransformRead(), Right.TransformRead());
				default:
					throw new NotImplementedException();
			}
		}

		public override System.Linq.Expressions.Expression TransformWrite(System.Linq.Expressions.Expression value) { throw new InvalidOperationException("二項演算は左辺値となることはできません。"); }

		public override System.Linq.Expressions.Expression TransformVoid()
		{
			switch (ExpressionType)
			{
				case BinaryExpressionType.Add:
				case BinaryExpressionType.ArithmeticRightShift:
				case BinaryExpressionType.BitwiseAnd:
				case BinaryExpressionType.BitwiseOr:
				case BinaryExpressionType.BitwiseXor:
				case BinaryExpressionType.Divide:
				case BinaryExpressionType.Equal:
				case BinaryExpressionType.FloorDivide:
				case BinaryExpressionType.GreaterThan:
				case BinaryExpressionType.GreaterThanOrEqual:
				case BinaryExpressionType.InContextOf:
				case BinaryExpressionType.InstanceOf:
				case BinaryExpressionType.LeftShift:
				case BinaryExpressionType.LessThan:
				case BinaryExpressionType.LessThanOrEqual:
				case BinaryExpressionType.LogicalRightShift:
				case BinaryExpressionType.Modulo:
				case BinaryExpressionType.Multiply:
				case BinaryExpressionType.NotEqual:
				case BinaryExpressionType.StrictEqual:
				case BinaryExpressionType.StrictNotEqual:
				case BinaryExpressionType.Subtract:
					return System.Linq.Expressions.Expression.Block(Left.TransformVoid(), Right.TransformVoid());
				case BinaryExpressionType.LogicalAnd:
					return System.Linq.Expressions.Expression.IfThen(Left.TransformReadAsBoolean(), Right.TransformVoid());
				case BinaryExpressionType.LogicalOr:
					return System.Linq.Expressions.Expression.IfThen(System.Linq.Expressions.Expression.Not(Left.TransformReadAsBoolean()), Right.TransformVoid());
				case BinaryExpressionType.Assign:
					return Microsoft.Scripting.Ast.Utils.Void(Left.TransformWrite(Right.TransformRead()));
				case BinaryExpressionType.Exchange:
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
					throw new NotImplementedException();
			}
		}
	}

	public enum BinaryExpressionType
	{
		Multiply,
		Divide,
		FloorDivide,
		Modulo,
		Add,
		Subtract,
		LeftShift,
		ArithmeticRightShift,
		LogicalRightShift,
		LessThan,
		LessThanOrEqual,
		GreaterThan,
		GreaterThanOrEqual,
		Equal,
		NotEqual,
		StrictEqual,
		StrictNotEqual,
		BitwiseAnd,
		BitwiseXor,
		BitwiseOr,
		LogicalAnd,
		LogicalOr,
		InstanceOf,
		InContextOf,
		Assign,
		MultiplyAssign,
		DivideAssign,
		FloorDivideAssign,
		ModuloAssign,
		AddAssign,
		SubtractAssign,
		LeftShiftAssign,
		ArithmeticRightShiftAssign,
		LogicalRightShiftAssign,
		BitwiseAndAssign,
		BitwiseXorAssign,
		BitwiseOrAssign,
		LogicalAndAssign,
		LogicalOrAssign,
		Exchange,
	}
}
