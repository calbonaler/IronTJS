using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using IronTjs.Builtins;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;

namespace IronTjs.Runtime.Binding
{
	public sealed class TjsBinder : DefaultBinder
	{
		public override MemberGroup GetMember(MemberRequestKind action, Type type, string name)
		{
			if (type == typeof(string))
			{
				var method = typeof(IronTjs.Builtins.TjsString).GetMethod(name);
				if (method != null)
					return new MemberGroup(MemberTracker.FromMemberInfo(method, typeof(string)));
				var property = typeof(IronTjs.Builtins.TjsString).GetField(name + "Property");
				if (property != null && property.FieldType == typeof(ExtensionPropertyTracker))
					return new MemberGroup((ExtensionPropertyTracker)property.GetValue(null));
				return MemberGroup.EmptyGroup;
			}
			return base.GetMember(action, type, name);
		}

		internal static readonly object NoValue = new object();

		internal static object ConvertInternal(object obj, Type toType)
		{
			Type fromType;
			if (obj == null || (fromType = obj.GetType()) == typeof(TjsVoid))
			{
				if (toType == typeof(string))
					return obj == null ? "null" : string.Empty;
				else
					return toType.IsValueType ? toType.GetConstructor(Type.EmptyTypes).Invoke(null) : null;
			}
			if (toType == typeof(object) || toType == fromType)
				return obj;
			if (toType == typeof(string))
				return obj.ToString();
			var nonNullableTo = Binders.GetNonNullableType(toType);
			if (Binders.IsNumber(nonNullableTo))
			{
				if (Binders.IsNumber(fromType))
				{
					var converted = System.Convert.ChangeType(obj, nonNullableTo);
					if (nonNullableTo == toType)
						return converted;
					else
						return Activator.CreateInstance(toType, converted);
				}
				else if (fromType == typeof(string))
				{
					var tryParse = nonNullableTo.GetMethod("TryParse", new[] { typeof(string), nonNullableTo.MakeByRefType() });
					var argument = new[] { obj, null };
					if ((bool)tryParse.Invoke(null, argument))
					{
						if (nonNullableTo == toType)
							return argument[1];
						else
							return Activator.CreateInstance(toType, argument[1]);
					}
					else
						return Activator.CreateInstance(toType);
				}
			}
			else if (nonNullableTo == typeof(bool))
			{
				object converted;
				if (Binders.IsNumber(fromType))
					converted = System.Convert.ChangeType(obj, nonNullableTo);
				else if (fromType == typeof(string))
				{
					long value;
					converted = long.TryParse((string)obj, out value) && value != 0;
				}
				else if (!fromType.IsValueType)
					converted = obj != null;
				else if (fromType.IsGenericType && fromType.GetGenericTypeDefinition() == typeof(Nullable<>))
					converted = fromType.GetProperty("HasValue").GetValue(obj);
				else
					converted = true;
				if (nonNullableTo == toType)
					return converted;
				else
					return new Nullable<bool>((bool)converted);
			}
			return NoValue;
		}

		public override object Convert(object obj, Type toType)
		{
			var converted = ConvertInternal(obj, toType);
			if (converted == NoValue)
				return base.Convert(obj, toType);
			return converted;
		}

		public override Expression ConvertExpression(Expression expr, Type toType, ConversionResultKind kind, OverloadResolverFactory resolverFactory)
		{
			return TjsConvertBinder.TryConvertExpression(expr, toType, null) ?? base.ConvertExpression(expr, toType, kind, resolverFactory);
		}

		public override bool CanConvertFrom(Type fromType, Type toType, bool toNotNullable, NarrowingLevel level)
		{
			var nonNullable = Binders.GetNonNullableType(toType);
			return toType.IsAssignableFrom(fromType) ||
				fromType == typeof(IronTjs.Builtins.TjsVoid) ||
				toType == typeof(object) ||
				toType == typeof(string) ||
				nonNullable == typeof(bool) ||
				Binders.IsNumber(nonNullable) && (Binders.IsNumber(fromType) || fromType == typeof(string));
		}
	}
}
