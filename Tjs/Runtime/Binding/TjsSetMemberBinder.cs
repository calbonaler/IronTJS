using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Runtime.Binding
{
	using AstUtils = Microsoft.Scripting.Ast.Utils;

	class TjsSetMemberBinder : SetMemberBinder, IForceMemberCreatable, IDirectAccessible
	{
		public TjsSetMemberBinder(TjsContext context, string name, bool ignoreCase, bool forceCreate, bool direct) : base(name, ignoreCase)
		{
			_context = context;
			ForceCreate = forceCreate;
			DirectAccess = direct;
		}

		readonly TjsContext _context;

		public bool ForceCreate { get; private set; }

		public bool DirectAccess { get; private set; }

		public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
		{
			var result = _context.Binder.SetMember(Name, target, value, errorSuggestion, new TjsOverloadResolverFactory(_context.Binder));
			if (result.Expression.Type.IsValueType)
				result = new DynamicMetaObject(AstUtils.Convert(result.Expression, typeof(object)), result.Restrictions);
			return result;
		}
	}
}
