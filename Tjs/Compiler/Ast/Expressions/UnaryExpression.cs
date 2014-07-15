using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Compiler.Ast
{
	public class UnaryExpression : Expression
	{
		public UnaryExpression(Expression operand, UnaryExpressionType expressionType)
		{
			Operand = operand;
			ExpressionType = expressionType;
			Operand.Parent = this;
		}

		public Expression Operand { get; private set; }

		public UnaryExpressionType ExpressionType { get; private set; }

		public override System.Linq.Expressions.Expression TransformRead()
		{
			throw new NotImplementedException();
		}

		public override System.Linq.Expressions.Expression TransformWrite(System.Linq.Expressions.Expression value)
		{
			throw new NotImplementedException();
		}

		public override System.Linq.Expressions.Expression TransformVoid()
		{
			throw new NotImplementedException();
		}
	}

	public enum UnaryExpressionType
	{
		Not,
		OnesComplement,
		CharToCharCode,
		CharCodeToChar,
		UnaryPlus,
		Negate,
		ReferProperty,
		DereferProperty,
		PreIncrementAssign,
		PreDecrementAssign,
		New,
		Invalidate,
		IsValid,
		Delete,
		TypeOf,
		PostIncrementAssign,
		PostDecrementAssign,
		Evaluate,
	}
}
