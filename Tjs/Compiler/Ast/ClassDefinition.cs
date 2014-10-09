using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronTjs.Runtime;
using Microsoft.Scripting.Utils;
using MSAst = System.Linq.Expressions.Expression;

namespace IronTjs.Compiler.Ast
{
	public class ClassDefinition : Node, IContextHolder
	{
		public ClassDefinition(string name, IEnumerable<string> baseClasses, IEnumerable<ClassDefinition> classes, IEnumerable<FunctionDefinition> functions, IEnumerable<PropertyDefinition> properties, IEnumerable<VariableDeclarationExpression> variableDeclarations)
		{
			_context = System.Linq.Expressions.Expression.Parameter(typeof(object));
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

		System.Linq.Expressions.ParameterExpression _context;

		public string Name { get; private set; }

		public ReadOnlyCollection<string> BaseClasses { get; private set; }

		public ReadOnlyCollection<ClassDefinition> Classes { get; private set; }

		public ReadOnlyCollection<FunctionDefinition> Functions { get; private set; }

		public ReadOnlyCollection<PropertyDefinition> Properties { get; private set; }

		public ReadOnlyCollection<VariableDeclarationExpression> VariableDeclarations { get; private set; }

		public System.Linq.Expressions.Expression Context { get { return _context; } }

		public System.Linq.Expressions.Expression TransformClass()
		{
			var defaultContext = MSAst.Constant(null);
			var members = new Dictionary<string, MSAst>();
			foreach (var func in Functions)
				members[func.Name] = func.TransformFunction(defaultContext);
			foreach (var prop in Properties)
				members[prop.Name] = prop.TransformProperty(defaultContext);
			var membersArg = MSAst.Call(
				new Func<IEnumerable<string>, IEnumerable<object>, Func<string, object, KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>>(Enumerable.Zip<string, object, KeyValuePair<string, object>>).Method,
				MSAst.Constant(members.Keys),
				MSAst.NewArrayInit(typeof(object), members.Values),
				(System.Linq.Expressions.Expression<Func<string, object, KeyValuePair<string, object>>>)((x, y) => new KeyValuePair<string, object>(x, y))
			);
			var fields = new Dictionary<string, System.Linq.Expressions.Expression<Func<object, object>>>();
			foreach (var vd in VariableDeclarations)
			{
				foreach (var initializer in vd.Initializers)
				{
					if (initializer.Value != null)
						fields[initializer.Key] = System.Linq.Expressions.Expression.Lambda<Func<object, object>>(initializer.Value.TransformRead(), _context);
					else
						fields[initializer.Key] = System.Linq.Expressions.Expression.Lambda<Func<object, object>>(System.Linq.Expressions.Expression.Constant(Builtins.TjsVoid.Value), _context);
				}
			}
			var fieldsArg = MSAst.Call(
				new Func<IEnumerable<string>, IEnumerable<Func<object, object>>, Func<string, object, KeyValuePair<string, Func<object, object>>>, IEnumerable<KeyValuePair<string, Func<object, object>>>>(Enumerable.Zip<string, Func<object, object>, KeyValuePair<string, Func<object, object>>>).Method,
				MSAst.Constant(fields.Keys),
				MSAst.NewArrayInit(typeof(Func<object, object>), fields.Values),
				(System.Linq.Expressions.Expression<Func<string, Func<object, object>, KeyValuePair<string, Func<object, object>>>>)((x, y) => new KeyValuePair<string, Func<object, object>>(x, y))
			);
			var ctor = typeof(Class).GetConstructor(new[]
			{
				typeof(string),
				typeof(IEnumerable<Class>),
				typeof(IEnumerable<KeyValuePair<string, object>>),
				typeof(IEnumerable<KeyValuePair<string, Func<object, object>>>)
			});
			return MSAst.New(ctor, MSAst.Constant(Name), MSAst.Constant(Enumerable.Empty<Class>()), membersArg, fieldsArg);
		}

		public System.Linq.Expressions.Expression Register(System.Linq.Expressions.Expression registeredTo)
		{
			return MSAst.Dynamic(
				LanguageContext.CreateSetMemberBinder(Name, false, true, true),
				typeof(object),
				registeredTo,
				TransformClass()
			);
		}
	}
}
