using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Runtime.Binding
{
	using AstUtils = Microsoft.Scripting.Ast.Utils;

	class TjsSetMemberBinder : DynamicMetaObjectBinder
	{
		public TjsSetMemberBinder(TjsContext context, string name, bool ignoreCase, bool forceCreate)
		{
			_context = context;
			Name = name;
			IgnoreCase = ignoreCase;
			ForceCreate = forceCreate;
		}

		readonly TjsContext _context;

		public string Name { get; private set; }

		public bool IgnoreCase { get; private set; }

		public bool ForceCreate { get; private set; }

		public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
		{
			if (ForceCreate || target.GetDynamicMemberNames().Contains(Name))
				return target.BindSetMember(new CompatibilitySetMemberBinder(_context, Name, IgnoreCase), args[0]);
			else
				return new DynamicMetaObject(
					System.Linq.Expressions.Expression.Throw(System.Linq.Expressions.Expression.Constant(new MissingMemberException()), typeof(object)),
					BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType).Merge(target.Restrictions));
		}
	}

	class CompatibilitySetMemberBinder : SetMemberBinder
	{
		public CompatibilitySetMemberBinder(TjsContext context, string name, bool ignoreCase) : base(name, ignoreCase) { _context = context; }

		readonly TjsContext _context;

		public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
		{
			var result = _context.Binder.SetMember(Name, target, value, errorSuggestion, new TjsOverloadResolverFactory(_context.Binder));
			if (result.Expression.Type.IsValueType)
				result = new DynamicMetaObject(AstUtils.Convert(result.Expression, typeof(object)), result.Restrictions);
			return result;
		}
	}
}
