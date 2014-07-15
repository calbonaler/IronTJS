using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Compiler.Ast
{
	public class EmptyStatement : Statement
	{
		public override System.Linq.Expressions.Expression Transform() { return System.Linq.Expressions.Expression.Empty(); }
	}
}
