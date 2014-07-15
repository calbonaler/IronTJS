using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Compiler.Ast
{
	public class ConstantExpression : Expression
	{
		public ConstantExpression(object value) { Value = value; }

		public object Value { get; private set; }

		public override System.Linq.Expressions.Expression TransformRead() { return System.Linq.Expressions.Expression.Constant(Value); }

		public override System.Linq.Expressions.Expression TransformWrite(System.Linq.Expressions.Expression value) { throw new InvalidOperationException("定数を左辺値にすることはできません。"); }

		public override System.Linq.Expressions.Expression TransformVoid() { return System.Linq.Expressions.Expression.Empty(); }
	}
}
