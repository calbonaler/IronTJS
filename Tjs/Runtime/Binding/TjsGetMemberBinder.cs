using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Runtime.Binding
{
	using AstUtils = Microsoft.Scripting.Ast.Utils;

	class TjsGetMemberBinder : GetMemberBinder
	{
		public TjsGetMemberBinder(TjsContext context, string name, bool ignoreCase) : base(name, ignoreCase) { _context = context; }

		readonly TjsContext _context;

		public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
		{
			var result = _context.Binder.GetMember(Name, target, new TjsOverloadResolverFactory(_context.Binder), false, errorSuggestion);
			if (result.Expression.Type.IsValueType)
				result = new DynamicMetaObject(AstUtils.Convert(result.Expression, typeof(object)), result.Restrictions);
			return result;
		}
	}
}
