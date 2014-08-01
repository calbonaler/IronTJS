using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Utils;

namespace IronTjs.Runtime.Binding
{
	class TjsSetIndexBinder : SetIndexBinder
	{
		public TjsSetIndexBinder(TjsContext context, CallInfo callInfo) : base(callInfo) { _context = context; }

		readonly TjsContext _context;

		public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
		{
			var arguments = ArrayUtils.Append(ArrayUtils.Insert(target, indexes));
			var result = _context.Binder.SetIndex(new TjsOverloadResolverFactory(_context.Binder), arguments);
			if (result != null)
			{
				if (result.Expression.Type.IsValueType)
					return new DynamicMetaObject(Expression.Convert(result.Expression, typeof(object)), result.Restrictions);
				else
					return result;
			}
			if (indexes[0].LimitType == typeof(string))
			{
				result = target.BindSetMember(new CompatibilitySetMemberBinder(_context, (string)indexes[0].Value, false), value);
				return new DynamicMetaObject(result.Expression, result.Restrictions.Merge(
					BindingRestrictions.GetInstanceRestriction(indexes[0].Expression, indexes[0].Value)
				).Merge(
					BindingRestrictions.GetTypeRestriction(indexes[0].Expression, indexes[0].LimitType)
				));
			}
			return errorSuggestion ?? new DynamicMetaObject(
				Expression.Throw(Expression.Constant(new MissingMemberException(indexes[0].Value.ToString())), typeof(object)),
				BindingRestrictions.Combine(arguments).Merge(BindingRestrictions.GetTypeRestriction(indexes[0].Expression, indexes[0].LimitType))
			);
		}
	}
}
