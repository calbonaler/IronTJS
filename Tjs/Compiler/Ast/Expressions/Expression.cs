using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Compiler.Ast
{
	public abstract class Expression : Node
	{
		// Type: typeof(object)
		public abstract System.Linq.Expressions.Expression TransformRead();

		// Type: typeof(bool)
		public System.Linq.Expressions.Expression TransformReadAsBoolean() { return null; }

		// Type: typeof(object)
		public abstract System.Linq.Expressions.Expression TransformWrite(System.Linq.Expressions.Expression value);

		// Type: typeof(void)
		public abstract System.Linq.Expressions.Expression TransformVoid();
	}
}
