using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Runtime.Binding
{
	class TjsInvokeBinder : InvokeBinder
	{
		public TjsInvokeBinder(TjsContext context, CallInfo callInfo) : base(callInfo) { _context = context; }
		
		readonly TjsContext _context;

		public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
		{
			return _context.Binder.Invoke(
				Binders.GetCallSignatureForCallInfo(CallInfo),
				errorSuggestion,
				new TjsOverloadResolverFactory(_context.Binder),
				target,
				args
			);
		}
	}
}
