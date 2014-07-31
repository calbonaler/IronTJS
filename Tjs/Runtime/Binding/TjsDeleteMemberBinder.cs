using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Runtime.Binding
{
	class TjsDeleteMemberBinder : DynamicMetaObjectBinder
	{
		public TjsDeleteMemberBinder(TjsContext context, string name, bool ignoreCase)
		{
			_context = context;
			Name = name;
			IgnoreCase = ignoreCase;
		}

		readonly TjsContext _context;

		public string Name { get; private set; }

		public bool IgnoreCase { get; private set; }

		public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
		{
			var obj =  target.BindDeleteMember(new CompatibilityDeleteMemberBinder(_context, Name, IgnoreCase));
			return new DynamicMetaObject(Microsoft.Scripting.Ast.Utils.Try(
				obj.Expression,
				Expression.Constant(1L, typeof(object))
			).Catch(typeof(MissingMemberException),
				Expression.Constant(0L, typeof(object))
			), obj.Restrictions);
		}
	}

	class CompatibilityDeleteMemberBinder : DeleteMemberBinder
	{
		public CompatibilityDeleteMemberBinder(TjsContext context, string name, bool ignoreCase) : base(name, ignoreCase) { _context = context; }

		readonly TjsContext _context;

		public override DynamicMetaObject FallbackDeleteMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
		{
			return _context.Binder.DeleteMember(Name, target, new TjsOverloadResolverFactory(_context.Binder), errorSuggestion);
		}
	}
}
