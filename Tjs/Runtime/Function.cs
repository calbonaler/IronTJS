using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Runtime
{
	public class Function : IDynamicMetaObjectProvider, IContextChangeable
	{
		public Function(Func<object, object[], object> functionBody, object context)
		{
			_functionBody = functionBody;
			Context = context;
		}

		Func<object, object[], object> _functionBody;

		public object Context { get; private set; }

		public object Invoke(params object[] args) { return _functionBody(Context, args); }

		public Function ChangeContext(object context) { return new Function(_functionBody, context); }

		IContextChangeable IContextChangeable.ChangeContext(object context) { return ChangeContext(context); }

		public DynamicMetaObject GetMetaObject(Expression parameter) { return new TjsMetaFunction(parameter, BindingRestrictions.GetTypeRestriction(parameter, typeof(Function)), this); }

		class TjsMetaFunction : DynamicMetaObject
		{
			public TjsMetaFunction(Expression expression, BindingRestrictions restrictions, Function value) : base(expression, restrictions, value) { }

			public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
			{
				return new DynamicMetaObject(
					Expression.Call(
						Expression.Convert(Expression, typeof(Function)),
						typeof(Function).GetMethod("Invoke", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
						Expression.NewArrayInit(typeof(object), args.Select(x => x.Expression))
					),
					Restrictions.Merge(BindingRestrictions.Combine(args))
				);
			}
		}
	}
}
