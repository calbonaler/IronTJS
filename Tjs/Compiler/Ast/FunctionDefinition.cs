using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Utils;

namespace IronTjs.Compiler.Ast
{
	public class FunctionDefinition : Node, INameResolver
	{
		public FunctionDefinition(string name, IEnumerable<ParameterDefinition> parameters, IEnumerable<Statement> body)
		{
			Name = name;
			Parameters = parameters.ToReadOnly();
			foreach (var param in Parameters)
				param.Parent = this;
			Body = body.ToReadOnly();
			foreach (var statement in Body)
				statement.Parent = this;
			ReturnLabel = System.Linq.Expressions.Expression.Label(typeof(object));
		}
		
		Dictionary<string, System.Linq.Expressions.ParameterExpression> variables = new Dictionary<string, System.Linq.Expressions.ParameterExpression>();
		// actual formal parameters 
		System.Linq.Expressions.ParameterExpression context = System.Linq.Expressions.Expression.Parameter(typeof(object), "__this__");
		System.Linq.Expressions.ParameterExpression parameters = System.Linq.Expressions.Expression.Parameter(typeof(object[]), "__params__");

		public string Name { get; private set; }

		public ReadOnlyCollection<ParameterDefinition> Parameters { get; private set; }

		public ReadOnlyCollection<Statement> Body { get; private set; }

		public System.Linq.Expressions.LabelTarget ReturnLabel { get; private set; }
		
		public System.Linq.Expressions.Expression ResolveForRead(string name)
		{
			var param = Parameters.Select(x => x.ParameterVariable).FirstOrDefault(x => x != null && x.Name == name);
			return param != null || variables.TryGetValue(name, out param) ? param : null;
		}

		public System.Linq.Expressions.Expression ResolveForWrite(string name, System.Linq.Expressions.Expression value)
		{
			var param = Parameters.Select(x => x.ParameterVariable).FirstOrDefault(x => x != null && x.Name == name);
			return param != null || variables.TryGetValue(name, out param) ? System.Linq.Expressions.Expression.Assign(param, value) : null;
		}

		public System.Linq.Expressions.Expression DeclareVariable(string name, System.Linq.Expressions.Expression value)
		{
			System.Linq.Expressions.ParameterExpression param;
			if (!variables.TryGetValue(name, out param))
				variables[name] = param = System.Linq.Expressions.Expression.Variable(typeof(object), name);
			return System.Linq.Expressions.Expression.Assign(param, value);
		}

		// Function Body Code Generation
		// object Func(object __this__, object[] __params__) {
		//     object p1 = ...;
		//     object p2 = ...;
		//     ...
		//     object pn = ...;
		//     (..Body..)
		// }
	}

	public class ParameterDefinition : Node
	{
		public ParameterDefinition(string name, object defaultValue) : this(name, false)
		{
			DefaultValue = defaultValue;
			HasDefaultValue = true;
		}

		public ParameterDefinition(string name, bool expandToArray)
		{
			if (name != null)
				ParameterVariable = System.Linq.Expressions.Expression.Variable(typeof(object), name);
			ExpandToArray = expandToArray;
		}

		public System.Linq.Expressions.ParameterExpression ParameterVariable { get; private set; }

		public bool HasDefaultValue { get; private set; }

		public object DefaultValue { get; private set; }

		public bool ExpandToArray { get; private set; }
	}
}
