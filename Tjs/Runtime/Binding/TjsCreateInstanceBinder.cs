using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Runtime.Binding
{
	class TjsCreateInstanceBinder : CreateInstanceBinder
	{
		public TjsCreateInstanceBinder(TjsContext context, CallInfo callInfo) : base(callInfo) { Context = context; }

		public TjsContext Context { get; private set; }

		public override DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
		{
			return errorSuggestion ?? new DynamicMetaObject(Expression.Throw(Expression.Constant(new NotImplementedException()), typeof(object)), target.Restrictions.Merge(BindingRestrictions.Combine(args)));
		}
	}
}
