using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Compiler.Ast
{
	public abstract class Statement : Node
	{
		public abstract System.Linq.Expressions.Expression Transform();
	}
}
