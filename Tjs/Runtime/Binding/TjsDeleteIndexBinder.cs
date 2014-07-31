using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Runtime.Binding
{
	class TjsDeleteIndexBinder : DynamicMetaObjectBinder
	{
		public TjsDeleteIndexBinder(TjsContext context, CallInfo callInfo)
		{
			_context = context;
			CallInfo = callInfo;
		}

		readonly TjsContext _context;

		public CallInfo CallInfo { get; private set; }

		public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
		{
			var obj = target.BindDeleteIndex(new CompatibilityDeleteIndexBinder(_context, CallInfo), args);
			return new DynamicMetaObject(Microsoft.Scripting.Ast.Utils.Try(
				obj.Expression,
				Expression.Constant(1L, typeof(object))
			).Catch(typeof(MissingMemberException),
				Expression.Constant(0L, typeof(object))
			), obj.Restrictions);
		}
	}

	class CompatibilityDeleteIndexBinder : DeleteIndexBinder
	{
		public CompatibilityDeleteIndexBinder(TjsContext context, CallInfo callInfo) : base(callInfo) { _context = context; }

		readonly TjsContext _context;

		public override DynamicMetaObject FallbackDeleteIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
		{
			var result = target.BindDeleteMember(new CompatibilityDeleteMemberBinder(_context, (string)indexes[0].Value, false));
			return new DynamicMetaObject(result.Expression, BindingRestrictions.Combine(new[] { result, indexes[0] }).Merge(BindingRestrictions.GetInstanceRestriction(indexes[0].Expression, indexes[0].Value)));
		}
	}
}
