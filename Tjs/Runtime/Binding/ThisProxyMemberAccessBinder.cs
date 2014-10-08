using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Runtime.Binding
{
	class ThisProxyMemberAccessBinder : DynamicMetaObjectBinder
	{
		public ThisProxyMemberAccessBinder(TjsContext context, string name, bool ignoreCase, MemberAccessKind accessKind)
		{
			_context = context;
			_name = name;
			_ignoreCase = ignoreCase;
			_accessKind = accessKind;
		}

		readonly TjsContext _context;
		readonly string _name;
		readonly bool _ignoreCase;
		readonly MemberAccessKind _accessKind;

		public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
		{
			switch (_accessKind & MemberAccessKind.AccessMask)
			{
				case MemberAccessKind.Get:
					return target.BindGetMember(new GetMemberBinderImpl(_context, _name, _ignoreCase, (_accessKind & MemberAccessKind.Direct) != 0, args[0]));
				case MemberAccessKind.Set:
					return target.BindSetMember(
						new SetMemberBinderImpl(
							_context, _name, _ignoreCase,
							(_accessKind & MemberAccessKind.Creatable) != 0,
							(_accessKind & MemberAccessKind.Direct) != 0,
							args[0]
						), args[1]
					);
				default:
					return TjsDeleteMemberBinder.Bind(target, new DeleteMemberBinderImpl(_context, _name, _ignoreCase, args[0]));
			}
		}

		class GetMemberBinderImpl : GetMemberBinder
		{
			public GetMemberBinderImpl(TjsContext context, string name, bool ignoreCase, bool direct, DynamicMetaObject fallback) : base(name, ignoreCase)
			{
				_context = context;
				_direct = direct;
				_fallback = fallback;
			}

			readonly TjsContext _context;
			readonly bool _direct;
			readonly DynamicMetaObject _fallback;

			public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
			{
				return _fallback.BindGetMember(_context.CreateGetMemberBinder(Name, IgnoreCase, _direct));
			}
		}

		class SetMemberBinderImpl : SetMemberBinder
		{
			public SetMemberBinderImpl(TjsContext context, string name, bool ignoreCase, bool forceCreate, bool direct, DynamicMetaObject fallback) : base(name, ignoreCase)
			{
				_context = context;
				_forceCreate = forceCreate;
				_direct = direct;
				_fallback = fallback;
			}

			readonly TjsContext _context;
			readonly bool _forceCreate;
			readonly bool _direct;
			readonly DynamicMetaObject _fallback;

			public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
			{
				return _fallback.BindSetMember(_context.CreateSetMemberBinder(Name, IgnoreCase, _forceCreate, _direct), value);
			}
		}

		class DeleteMemberBinderImpl : DeleteMemberBinder
		{
			public DeleteMemberBinderImpl(TjsContext context, string name, bool ignoreCase, DynamicMetaObject fallback) : base(name, ignoreCase)
			{
				_context = context;
				_fallback = fallback;
			}

			readonly TjsContext _context;
			readonly DynamicMetaObject _fallback;

			public override DynamicMetaObject FallbackDeleteMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
			{
				return TjsDeleteMemberBinder.Bind(_fallback, new CompatibilityDeleteMemberBinder(_context, Name, IgnoreCase));
			}
		}
	}

	[Flags]
	enum MemberAccessKind
	{
		Get = 0x00,
		Set = 0x01,
		Delete = 0x02,
		Creatable = 0x04,
		Direct = 0x08,
		AccessMask = 0x03,
	}
}
