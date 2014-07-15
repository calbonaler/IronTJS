using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Compiler.Ast
{
	public class ConvertExpression : Expression
	{
		public ConvertExpression(Expression operand, ConvertType toType)
		{
			Operand = operand;
			ToType = toType;
			Operand.Parent = this;
		}

		public Expression Operand { get; private set; }

		public ConvertType ToType { get; private set; }

		public override System.Linq.Expressions.Expression TransformRead() { throw new NotImplementedException(); }

		public override System.Linq.Expressions.Expression TransformWrite(System.Linq.Expressions.Expression value) { throw new InvalidOperationException("型変換を左辺値とすることはできません。"); }

		public override System.Linq.Expressions.Expression TransformVoid() { return Operand.TransformVoid(); }
	}

	public enum ConvertType
	{
		Default = 0,
		Integer = 0,
		Real,
		String,
	}
}
