using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Actions;

namespace IronTjs.Runtime.Binding
{
	class TjsInvokeBinder : InvokeBinder, ICallBinder
	{
		public TjsInvokeBinder(TjsContext context, CallSignature signature) : base(Binders.GetCallInfoForCallSignature(signature))
		{
			_context = context;
			Signature = signature;
		}
		
		readonly TjsContext _context;
		
		public CallSignature Signature { get; private set; }

		public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
		{
			return _context.Binder.Invoke(
				Signature,
				errorSuggestion,
				new TjsOverloadResolverFactory(_context.Binder),
				target,
				args
			);
		}
	}
}
