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

		public SourceUnitTree GlobalParent
		{
			get
			{
				var node = this;
				while (node.Parent != null)
					node = node.Parent;
				return (SourceUnitTree)node;
			}
		}

		public TjsContext LanguageContext { get { return (TjsContext)GlobalParent.CompilerContext.SourceUnit.LanguageContext; } }
	}
}
