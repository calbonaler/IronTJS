using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Compiler.Ast
{
	public class ThrowStatement : Statement
	{
		public ThrowStatement(Expression expression)
		{
			Expression = expression;
			Expression.Parent = this;
		}

		public Expression Expression { get; private set; }

		public override System.Linq.Expressions.Expression Transform()
		{
			var exp = Expression.TransformRead();
			return System.Linq.Expressions.Expression.Throw(exp);
		}
	}
}
