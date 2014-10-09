using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Compiler.Ast
{
	public interface IContextHolder
	{
		System.Linq.Expressions.Expression Context { get; }
	}
}
