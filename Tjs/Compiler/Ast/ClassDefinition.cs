using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Utils;

namespace IronTjs.Compiler.Ast
{
	public class ClassDefinition : Node
	{
		public ClassDefinition(string name, IEnumerable<string> baseClasses, IEnumerable<ClassDefinition> classes, IEnumerable<FunctionDefinition> functions, IEnumerable<PropertyDefinition> properties, IEnumerable<VariableDeclarationExpression> variableDeclarations)
		{
			Name = name;
			BaseClasses = baseClasses.ToReadOnly();
			Classes = classes.ToReadOnly();
			foreach (var cls in Classes)
				cls.Parent = this;
			Functions = functions.ToReadOnly();
			foreach (var function in Functions)
				function.Parent = this;
			Properties = properties.ToReadOnly();
			foreach (var prop in Properties)
				prop.Parent = this;
			VariableDeclarations = variableDeclarations.ToReadOnly();
			foreach (var variable in VariableDeclarations)
				variable.Parent = this;
		}

		public string Name { get; private set; }

		public ReadOnlyCollection<string> BaseClasses { get; private set; }

		public ReadOnlyCollection<ClassDefinition> Classes { get; private set; }

		public ReadOnlyCollection<FunctionDefinition> Functions { get; private set; }

		public ReadOnlyCollection<PropertyDefinition> Properties { get; private set; }

		public ReadOnlyCollection<VariableDeclarationExpression> VariableDeclarations { get; private set; }
	}
}
