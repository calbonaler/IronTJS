using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronTjs.Runtime;

namespace IronTjs.Compiler.Ast
{
	public abstract class Node
	{
		public Node Parent { get; internal set; }

		public TjsContext LanguageContext
		{
			get
			{
				var node = this;
				while (node.Parent != null)
					node = node.Parent;
				return (TjsContext)((SourceUnitTree)node).CompilerContext.SourceUnit.LanguageContext;
			}
		}
	}
}
