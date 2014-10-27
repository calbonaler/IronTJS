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
			for (var node = Parent; node != null; node = node.Parent)
			{
				var holdable = node as IContextHolder;
				if (holdable != null)
					return holdable.GlobalContext;
			}
			throw new InvalidOperationException("global コンテキストが見つかりませんでした。");
		}

		public override System.Linq.Expressions.Expression TransformVoid() { return System.Linq.Expressions.Expression.Empty(); }
	}
}
