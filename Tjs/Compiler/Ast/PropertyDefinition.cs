using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Compiler.Ast
{
	public class PropertyDefinition : Node
	{
		public PropertyDefinition(string name, FunctionDefinition getter, FunctionDefinition setter)
		{
			Name = name;
			Getter = getter;
			Setter = setter;
			Getter.Parent = Setter.Parent = this;
		}

		public string Name { get; private set; }

		public FunctionDefinition Getter { get; private set; }

		public FunctionDefinition Setter { get; private set; }
	}
}
