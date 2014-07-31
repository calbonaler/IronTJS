using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Compiler.Ast
{
	public class GlobalExpression : Expression
	{
		public override System.Linq.Expressions.Expression TransformRead()
		{
			Node node = this;
			while (node.Parent != null)
				node = node.Parent;
			return ((SourceUnitTree)node).GlobalObject;
		}

		public override System.Linq.Expressions.Expression TransformWrite(System.Linq.Expressions.Expression value) { throw new InvalidOperationException("global を左辺値とすることはできません。"); }

		public override System.Linq.Expressions.Expression TransformVoid() { return System.Linq.Expressions.Expression.Empty(); }
	}
}
