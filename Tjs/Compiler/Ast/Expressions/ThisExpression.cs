using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Compiler.Ast
{
	public class ThisExpression : Expression
	{
		public override System.Linq.Expressions.Expression TransformRead()
		{
			for (var node = Parent; node != null; node = node.Parent)
			{
				if (node is FunctionDefinition)
					return ((FunctionDefinition)node).Context;
				else if (node is SourceUnitTree)
					return ((SourceUnitTree)node).GlobalObject;
			}
			throw Microsoft.Scripting.Utils.Assert.Unreachable;
		}

		public override System.Linq.Expressions.Expression TransformWrite(System.Linq.Expressions.Expression value) { throw new InvalidOperationException("this を左辺値とすることはできません。"); }

		public override System.Linq.Expressions.Expression TransformVoid() { return System.Linq.Expressions.Expression.Empty(); }
	}
}
