using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IronTjs.Runtime.Binding;

namespace IronTjs.Runtime
{
	public abstract class DynamicStorage : IDynamicMetaObjectProvider
	{
		protected DynamicStorage() { Version = 0; }

		protected long Version { get; set; }

		protected virtual bool TryGetValue(object key, bool direct, out object member)
		{
			member = null;
			return false;
		}

		protected virtual bool TrySetValue(object key, bool direct, bool forceCreate, object value) { return false; }

		protected virtual bool TryDeleteValue(object key) { return false; }

		protected virtual bool TryCreateInstance(object[] args, out object result)
		{
			result = null;
			return false;
		}

		protected virtual IEnumerable<string> GetMemberNames() { return Enumerable.Empty<string>(); }

		public DynamicMetaObject GetMetaObject(Expression parameter) { return new Meta(parameter, BindingRestrictions.GetTypeRestriction(parameter, GetType()), this); }

		class Meta : DynamicMetaObject
		{
			public Meta(Expression expression, BindingRestrictions restrictions, DynamicStorage value) : base(expression, restrictions, value) { }

			public new DynamicStorage Value { get { return (DynamicStorage)base.Value; } }

			Expression LimitedInstance { get { return Expression.Convert(Expression, LimitType); } }

			DynamicMetaObject Restrict(Expression expression, DynamicMetaObjectBinder binder, BindingRestrictions additional)
			{
				return new DynamicMetaObject(
					Expression.Condition(
						Expression.Equal(
							Expression.Property(LimitedInstance, (PropertyInfo)Utils.GetMember(() => Value.Version)),
							Expression.Constant(Value.Version)
						),
						expression,
						binder.GetUpdateExpression(expression.Type)
					),
					BindingRestrictions.GetInstanceRestriction(Expression, Value).Merge(additional)
				);
			}

			DynamicMetaObject BindDeleteMember(object key, DynamicMetaObjectBinder binder, BindingRestrictions additional, Func<DynamicMetaObject> fallback)
			{
				Expression exp;
				var delete = Expression.Call(LimitedInstance, (MethodInfo)Utils.GetMember(() => Value.TryDeleteValue(null)), Expression.Constant(key));
				if (binder.ReturnType == typeof(void))
					exp = Expression.Condition(delete, Expression.Empty(), fallback().Expression);
				else if (binder.ReturnType == typeof(bool))
					exp = delete;
				else
					exp = Expression.Convert(Expression.Condition(delete, Expression.Constant(1L), Expression.Constant(0L)), binder.ReturnType);
				return Restrict(exp, binder, additional);
			}

			DynamicMetaObject BindGetMember(object key, DynamicMetaObjectBinder binder, BindingRestrictions additional, Func<DynamicMetaObject> fallback)
			{
				var v = Expression.Variable(typeof(object));
				var accessible = binder as IDirectAccessible;
				var exp = Expression.Block(new[] { v },
					Expression.Condition(
						Expression.Call(
							LimitedInstance, 
							(MethodInfo)Utils.GetMember<object>(x => Value.TryGetValue(null, false, out x)),
							Expression.Constant(key), Expression.Constant(accessible != null && accessible.DirectAccess), v
						),
						v,
						fallback().Expression
					)
				);
				return Restrict(exp, binder, additional);
			}

			DynamicMetaObject BindSetMember(object key, Expression value, DynamicMetaObjectBinder binder, BindingRestrictions additional, Func<DynamicMetaObject> fallback)
			{
				IForceMemberCreatable creatable = binder as IForceMemberCreatable;
				var v = Expression.Variable(typeof(object));
				var accessible = binder as IDirectAccessible;
				var exp = Expression.Block(new[] { v },
					Expression.Assign(v, Expression.Convert(value, v.Type)),
					Expression.Condition(
						Expression.Call(
							LimitedInstance,
							(MethodInfo)Utils.GetMember(() => Value.TrySetValue(null, false, false, null)),
							Expression.Constant(key),
							Expression.Constant(accessible != null && accessible.DirectAccess),
							Expression.Constant(creatable == null || creatable.ForceCreate),
							v
						),
						v,
						fallback().Expression
					)
				);
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

			public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
			{
				var v = Expression.Variable(typeof(object));
				var exp = Expression.Block(new[] { v },
					Expression.Condition(
						Expression.Call(
							LimitedInstance,
							(MethodInfo)Utils.GetMember<object>(x => Value.TryCreateInstance(null, out x)),
							Expression.NewArrayInit(typeof(object), args.Select(x => Expression.Convert(x.Expression, typeof(object)))), v
						),
						v,
						base.BindCreateInstance(binder, args).Expression
					)
				);
				return Restrict(exp, binder, BindingRestrictions.Empty);
			}

			public override IEnumerable<string> GetDynamicMemberNames() { return Value.GetMemberNames(); }
		}
	}
}
