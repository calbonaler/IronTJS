using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using IronTjs.Runtime.Binding;

namespace IronTjs.Runtime
{
	public class TjsStorage : IDynamicMetaObjectProvider
	{
		public TjsStorage() { }

		public TjsStorage(IEnumerable<KeyValuePair<string, object>> pairs)
		{
			foreach (var pair in pairs)
				_members.Add(pair.Key, pair.Value);
		}

		Dictionary<string, dynamic> _members = new Dictionary<string, dynamic>();
		long _version = 0;

		public dynamic GetMember(string name) { return _members[name]; }

		public dynamic SetMember(string name, dynamic value)
		{
			lock (_members)
			{
				if (!_members.ContainsKey(name))
					_version++;
				return _members[name] = value;
			}
		}

		public dynamic GetMemberIndirect(string name)
		{
			lock (_members)
			{
				var member = _members[name];
				var property = member as TjsProperty;
				if (property != null)
					return property.Value;
				else
					return member;
			}
		}

		public dynamic SetMemberIndirect(string name, dynamic value)
		{
			lock (_members)
			{
				dynamic member;
				TjsProperty property;
				if (!_members.TryGetValue(name, out member))
					_version++;
				else if ((property = member as TjsProperty) != null)
					return property.Value = value;
				return _members[name] = value;
			}
		}

		public bool TryGetMember(string name, out dynamic value) { return _members.TryGetValue(name, out value); }

		public bool TryDeleteMember(string name)
		{
			lock (_members)
			{
				var result = _members.Remove(name);
				if (result)
					_version++;
				return result;
			}
		}

		public bool HasMember(string name) { return _members.ContainsKey(name); }

		public string[] GetMemberNames() { lock (_members) return _members.Keys.ToArray(); }

		public DynamicMetaObject GetMetaObject(Expression parameter) { return new Meta(parameter, BindingRestrictions.GetTypeRestriction(parameter, typeof(TjsStorage)), this); }

		class Meta : DynamicMetaObject
		{
			public Meta(Expression expression, BindingRestrictions restrictions, TjsStorage value) : base(expression, restrictions, value) { }

			public new TjsStorage Value { get { return (TjsStorage)base.Value; } }

			Expression LimitedInstance { get { return Expression.Convert(Expression, LimitType); } }

			DynamicMetaObject Restrict(Expression expression, DynamicMetaObjectBinder binder, BindingRestrictions additional)
			{
				return new DynamicMetaObject(
					Expression.Condition(
						Expression.Equal(
							Expression.Field(LimitedInstance, typeof(TjsStorage).GetField("_version", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)),
							Expression.Constant(Value._version)
						),
						expression,
						binder.GetUpdateExpression(expression.Type)
					),
					BindingRestrictions.GetInstanceRestriction(Expression, Value).Merge(additional)
				);
			}

			DynamicMetaObject BindDeleteMember(object name, DynamicMetaObjectBinder binder, BindingRestrictions additional, Func<DynamicMetaObject> fallback)
			{
				Expression exp;
				if (name.GetType() == typeof(string))
				{
					var delete = Expression.Call(LimitedInstance, "TryDeleteMember", null, Expression.Constant(name));
					if (binder.ReturnType == typeof(void))
						exp = Expression.Condition(delete, Expression.Empty(), fallback().Expression);
					else if (binder.ReturnType == typeof(bool))
						exp = delete;
					else
						exp = Expression.Convert(Expression.Condition(delete, Expression.Constant(1L), Expression.Constant(0L)), binder.ReturnType);
				}
				else
				{
					var fb = fallback();
					exp = fb.Expression;
					additional = fb.Restrictions;
				}
				return Restrict(exp, binder, additional);
			}

			DynamicMetaObject BindGetMember(object name, DynamicMetaObjectBinder binder, BindingRestrictions additional, Func<DynamicMetaObject> fallback)
			{
				Expression exp;
				if (name.GetType() == typeof(string) && Value.HasMember((string)name))
				{
					var accessible = binder as IDirectAccessible;
					exp = Expression.Call(LimitedInstance, accessible != null && accessible.DirectAccess ? "GetMember" : "GetMemberIndirect", null, Expression.Constant(name));
				}
				else
				{
					var fb = fallback();
					exp = fb.Expression;
					additional = fb.Restrictions;
				}
				return Restrict(exp, binder, additional);
			}

			DynamicMetaObject BindSetMember(object name, Expression value, DynamicMetaObjectBinder binder, BindingRestrictions additional, Func<DynamicMetaObject> fallback)
			{
				Expression exp;
				IForceMemberCreatable creatable = binder as IForceMemberCreatable;
				if (name.GetType() == typeof(string) && (creatable == null || creatable.ForceCreate || Value.HasMember((string)name)))
				{
					var accessible = binder as IDirectAccessible;
					exp = Expression.Call(LimitedInstance, accessible != null && accessible.DirectAccess ? "SetMember" : "SetMemberIndirect", null, Expression.Constant(name), value);
				}
				else
				{
					var fb = fallback();
					exp = fb.Expression;
					additional = fb.Restrictions;
				}
				return Restrict(exp, binder, additional);
			}

			public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
			{
				return BindDeleteMember(indexes[0].Value, binder, BindingRestrictions.GetInstanceRestriction(indexes[0].Expression, indexes[0].Value), () => base.BindDeleteIndex(binder, indexes));
			}

			public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
			{
				return BindDeleteMember(binder.Name, binder, BindingRestrictions.Empty, () => base.BindDeleteMember(binder));
			}

			public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
			{
				return BindGetMember(indexes[0].Value, binder, BindingRestrictions.GetInstanceRestriction(indexes[0].Expression, indexes[0].Value), () => base.BindGetIndex(binder, indexes));
			}

			public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
			{
				return BindGetMember(binder.Name, binder, BindingRestrictions.Empty, () => base.BindGetMember(binder));
			}

			public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
			{
				return BindSetMember(indexes[0].Value, value.Expression, binder, BindingRestrictions.GetInstanceRestriction(indexes[0].Expression, indexes[0].Value), () => base.BindSetIndex(binder, indexes, value));
			}

			public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
			{
				return BindSetMember(binder.Name, value.Expression, binder, BindingRestrictions.Empty, () => base.BindSetMember(binder, value));
			}

			public override IEnumerable<string> GetDynamicMemberNames() { return Value.GetMemberNames(); }
		}
	}
}
