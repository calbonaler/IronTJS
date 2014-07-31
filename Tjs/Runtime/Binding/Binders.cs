using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Actions;

namespace IronTjs.Runtime.Binding
{
	static class Binders
	{
		public static Expression Convert(this TjsContext context, Expression expression, Type toType) { return Expression.Dynamic(context.CreateConvertBinder(toType, true), toType, expression); }

		public static CallSignature GetCallSignatureForCallInfo(CallInfo callInfo)
		{
			return new CallSignature(Enumerable.Repeat(new Argument(ArgumentType.Simple), callInfo.ArgumentCount - callInfo.ArgumentNames.Count)
				.Concat(callInfo.ArgumentNames.Select(x => new Argument(ArgumentType.Named, x))).ToArray());
		}

		public static bool IsReal(Type type)
		{
			if (type.IsEnum)
				return false;
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Double:
				case TypeCode.Single:
					return true;
				default:
					return false;
			}
		}

		public static bool IsInteger(Type type)
		{
			if (type.IsEnum)
				return false;
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return true;
				default:
					return false;
			}
		}

		public static bool IsNumber(Type type)
		{
			if (type.IsEnum)
				return false;
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Byte:
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.Single:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return true;
				default:
					return false;
			}
		}

		public static Type GetNonNullableType(Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ? type.GetGenericArguments()[0] : type;
		}

		static ExpressionType? GetExpressionTypeForOperation(TjsOperationKind operation)
		{
			switch (operation)
			{
				case TjsOperationKind.Add:
					return ExpressionType.Add;
				case TjsOperationKind.AddAssign:
					return ExpressionType.AddAssign;
				case TjsOperationKind.And:
					return ExpressionType.And;
				case TjsOperationKind.AndAssign:
					return ExpressionType.AndAssign;
				case TjsOperationKind.Assign:
					return ExpressionType.Assign;
				case TjsOperationKind.Decrement:
					return ExpressionType.Decrement;
				case TjsOperationKind.Divide:
					return ExpressionType.Divide;
				case TjsOperationKind.DivideAssign:
					return ExpressionType.DivideAssign;
				case TjsOperationKind.Equal:
					return ExpressionType.Equal;
				case TjsOperationKind.ExclusiveOr:
					return ExpressionType.ExclusiveOr;
				case TjsOperationKind.ExclusiveOrAssign:
					return ExpressionType.ExclusiveOrAssign;
				case TjsOperationKind.GreaterThan:
					return ExpressionType.GreaterThan;
				case TjsOperationKind.GreaterThanOrEqual:
					return ExpressionType.GreaterThanOrEqual;
				case TjsOperationKind.Increment:
					return ExpressionType.Increment;
				case TjsOperationKind.LeftShift:
					return ExpressionType.LeftShift;
				case TjsOperationKind.LeftShiftAssign:
					return ExpressionType.LeftShiftAssign;
				case TjsOperationKind.LessThan:
					return ExpressionType.LessThan;
				case TjsOperationKind.LessThanOrEqual:
					return ExpressionType.LessThanOrEqual;
				case TjsOperationKind.Modulo:
					return ExpressionType.Modulo;
				case TjsOperationKind.ModuloAssign:
					return ExpressionType.ModuloAssign;
				case TjsOperationKind.Multiply:
					return ExpressionType.Multiply;
				case TjsOperationKind.MultiplyAssign:
					return ExpressionType.MultiplyAssign;
				case TjsOperationKind.Negate:
					return ExpressionType.Negate;
				case TjsOperationKind.Not:
					return ExpressionType.Not;
				case TjsOperationKind.NotEqual:
					return ExpressionType.NotEqual;
				case TjsOperationKind.OnesComplement:
					return ExpressionType.OnesComplement;
				case TjsOperationKind.Or:
					return ExpressionType.Or;
				case TjsOperationKind.OrAssign:
					return ExpressionType.OrAssign;
				case TjsOperationKind.RightShiftArithmetic:
					return ExpressionType.RightShift;
				case TjsOperationKind.RightShiftArithmeticAssign:
					return ExpressionType.RightShiftAssign;
				case TjsOperationKind.Subtract:
					return ExpressionType.Subtract;
				case TjsOperationKind.SubtractAssign:
					return ExpressionType.SubtractAssign;
				case TjsOperationKind.UnaryPlus:
					return ExpressionType.UnaryPlus;
			}
			return null;
		}

		public static Expression DoBinaryOperation(this TjsContext context, Expression left, Expression right, TjsOperationKind operation)
		{
			Debug.Assert((operation & TjsOperationKind.ModifierMask) == TjsOperationKind.None);
			if (operation == TjsOperationKind.AndAlso)
			{
				var v = Expression.Variable(typeof(object));
				return Expression.Block(new[] { v },
					Expression.Assign(v, left),
					Expression.Condition(Convert(context, v, typeof(bool)), right, v)
				);
			}
			else if (operation == TjsOperationKind.OrElse)
			{
				var v = Expression.Variable(typeof(object));
				return Expression.Block(new[] { v },
					Expression.Assign(v, left),
					Expression.Condition(Convert(context, v, typeof(bool)), v, right)
				);
			}
			var type = GetExpressionTypeForOperation(operation);
			if (type == null)
				throw new NotImplementedException();
			else
				return Expression.Dynamic(context.CreateBinaryOperationBinder(type.Value), typeof(object), left, right);
		}

		public static Expression DoUnaryOperation(this TjsContext context, Expression target, TjsOperationKind operation)
		{
			Debug.Assert((operation & TjsOperationKind.ModifierMask) == TjsOperationKind.None);
			var type = GetExpressionTypeForOperation(operation);
			if (type == null)
				throw new NotImplementedException();
			else
				return Expression.Dynamic(context.CreateUnaryOperationBinder(type.Value), typeof(object), target);
		}
	}
}
