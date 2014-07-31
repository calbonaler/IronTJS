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

		public System.Linq.Expressions.Expression ThisObject
		{
			get
			{
				for (var node = this; node != null; node = node.Parent)
				{
					if (node is FunctionDefinition)
						return ((FunctionDefinition)node).Context;
					else if (node is SourceUnitTree)
						return ((SourceUnitTree)node).GlobalObject;
				}
				throw Microsoft.Scripting.Utils.Assert.Unreachable;
			}
		}

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
