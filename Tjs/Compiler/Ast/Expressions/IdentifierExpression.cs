using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Compiler.Ast
{
	public class IdentifierExpression : Expression
	{
		public IdentifierExpression(string identifier) { Identifier = identifier; }

		public string Identifier { get; private set; }

		public override System.Linq.Expressions.Expression TransformRead()
		{
			for (var node = Parent; node != null; node = node.Parent)
			{
				var resolver = node as INameResolver;
				if (resolver != null)
				{
					var exp = resolver.ResolveForRead(Identifier);
					if (exp != null)
						return exp;
				}
			}
			throw Microsoft.Scripting.Utils.Assert.Unreachable;
		}

		public override System.Linq.Expressions.Expression TransformWrite(System.Linq.Expressions.Expression value)
		{
			for (var node = Parent; node != null; node = node.Parent)
			{
				var resolver = node as INameResolver;
				if (resolver != null)
				{
					var exp = resolver.ResolveForWrite(Identifier, value);
					if (exp != null)
						return exp;
				}
			}
			throw Microsoft.Scripting.Utils.Assert.Unreachable;
		}

		public override System.Linq.Expressions.Expression TransformVoid() { return System.Linq.Expressions.Expression.Empty(); }
	}
}
