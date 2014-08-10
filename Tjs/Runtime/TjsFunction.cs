using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Runtime
{
	public class TjsFunction : IDynamicMetaObjectProvider, IContextChangeable
	{
		public TjsFunction(Func<object, object[], object> functionBody, object context)
		{
			_functionBody = functionBody;
			Context = context;
		}

		Func<object, object[], object> _functionBody;

		public object Context { get; private set; }

		public object Invoke(params object[] args) { return _functionBody(Context, args); }

		public TjsFunction ChangeContext(object context) { return new TjsFunction(_functionBody, context); }

		object IContextChangeable.ChangeContext(object context) { return ChangeContext(context); }

		public DynamicMetaObject GetMetaObject(Expression parameter) { return new TjsMetaFunction(parameter, BindingRestrictions.GetTypeRestriction(parameter, typeof(TjsFunction)), this); }

		class TjsMetaFunction : DynamicMetaObject
		{
			public TjsMetaFunction(Expression expression, BindingRestrictions restrictions, TjsFunction value) : base(expression, restrictions, value) { }

			public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
			{
				return new DynamicMetaObject(
					Expression.Call(
						Expression.Convert(Expression, typeof(TjsFunction)),
						typeof(TjsFunction).GetMethod("Invoke", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
						Expression.NewArrayInit(typeof(object), args.Select(x => x.Expression))
					),
					Restrictions.Merge(BindingRestrictions.Combine(args))
				);
			}
		}
	}
}
