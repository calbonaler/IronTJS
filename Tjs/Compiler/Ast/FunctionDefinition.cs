using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Utils;
using MSAst = System.Linq.Expressions;

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

		public System.Linq.Expressions.ParameterExpression Context { get { return context; } }

		public string Name { get; private set; }

		public ReadOnlyCollection<ParameterDefinition> Parameters { get; private set; }

		public ReadOnlyCollection<Statement> Body { get; private set; }

		public System.Linq.Expressions.LabelTarget ReturnLabel { get; private set; }
		
		public System.Linq.Expressions.Expression ResolveForRead(string name)
		{
			var param = Parameters.Select(x => x.ParameterVariable).FirstOrDefault(x => x != null && x.Name == name);
			if (param != null || variables.TryGetValue(name, out param))
				return param;
			else
				return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateGetMemberBinder(name, false), typeof(object), context);
		}

		public System.Linq.Expressions.Expression ResolveForWrite(string name, System.Linq.Expressions.Expression value)
		{
			var param = Parameters.Select(x => x.ParameterVariable).FirstOrDefault(x => x != null && x.Name == name);
			if (param != null || variables.TryGetValue(name, out param))
				return System.Linq.Expressions.Expression.Assign(param, value);
			else
				return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateSetMemberBinder(name, false, false), typeof(object), context, value);
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

		System.Linq.Expressions.Expression<Func<object, object[], object>> TransformLambda()
		{
			List<MSAst.Expression> body = new List<MSAst.Expression>();
			for (int i = 0; i < Parameters.Count; i++)
			{
				MSAst.Expression exp = MSAst.Expression.ArrayAccess(parameters, MSAst.Expression.Constant(i));
				if (Parameters[i].HasDefaultValue)
					exp = MSAst.Expression.Condition(MSAst.Expression.TypeEqual(exp, typeof(IronTjs.Builtins.TjsVoid)),
						MSAst.Expression.Constant(Parameters[i].DefaultValue, typeof(object)),
						exp
					);
				body.Add(MSAst.Expression.Assign(Parameters[i].ParameterVariable,
					MSAst.Expression.Condition(MSAst.Expression.LessThan(MSAst.Expression.Constant(i), MSAst.Expression.Property(parameters, "Length")),
						exp,
						MSAst.Expression.Constant(Parameters[i].HasDefaultValue ? Parameters[i].DefaultValue : IronTjs.Builtins.TjsVoid.Value, typeof(object))
					)
				));
			}
			foreach (var statement in Body)
			{
				body.Add(statement.Transform());
			}
			body.Add(MSAst.Expression.Label(ReturnLabel, MSAst.Expression.Constant(IronTjs.Builtins.TjsVoid.Value)));
			return MSAst.Expression.Lambda<Func<object, object[], object>>(MSAst.Expression.Block(variables.Values.Concat(Parameters.Select(x => x.ParameterVariable)), body), Name, new[] { context, parameters });
		}

		public System.Linq.Expressions.Expression Register(System.Linq.Expressions.Expression registeredTo)
		{
			var lambda = TransformLambda();
			return MSAst.Expression.Dynamic(LanguageContext.CreateSetMemberBinder(Name, false, true), typeof(object), registeredTo,
				MSAst.Expression.New(typeof(IronTjs.Runtime.TjsFunction).GetConstructor(new[] { typeof(Func<object, object[], object>), typeof(object) }),
					lambda,
					registeredTo
				)
			);
		}
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
