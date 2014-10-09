using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSAst = System.Linq.Expressions;

namespace IronTjs.Compiler.Ast
{
	public class PropertyDefinition : Node
	{
		public PropertyDefinition(string name, FunctionDefinition getter, FunctionDefinition setter)
		{
			Name = name;
			Getter = getter;
			Setter = setter;
			if (Getter != null)
				Getter.Parent = this;
			if (Setter != null)
				Setter.Parent = this;
		}

		public string Name { get; private set; }

		public FunctionDefinition Getter { get; private set; }

		public FunctionDefinition Setter { get; private set; }

		public System.Linq.Expressions.Expression TransformProperty(System.Linq.Expressions.Expression context)
		{
			MSAst.Expression getter;
			if (Getter != null)
				getter = Getter.TransformLambda();
			else
				getter = MSAst.Expression.Constant(null, typeof(Func<object, object[], object>));
			MSAst.Expression setter;
			if (Setter != null)
				setter = Setter.TransformLambda();
			else
				setter = MSAst.Expression.Constant(null, typeof(Func<object, object[], object>));
			return MSAst.Expression.New(typeof(IronTjs.Runtime.Property).GetConstructor(new[] { typeof(Func<object, object[], object>), typeof(Func<object, object[], object>), typeof(object) }),
				getter,
				setter,
				context
			);
		}

		public System.Linq.Expressions.Expression Register(System.Linq.Expressions.Expression registeredTo)
		{
			return MSAst.Expression.Dynamic(LanguageContext.CreateSetMemberBinder(Name, false, true, true), typeof(object), registeredTo, TransformProperty(registeredTo));
		}
	}
}
