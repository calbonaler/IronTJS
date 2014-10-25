using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs
{
	static class Utils
	{
		public static Type[] GetDelegateSignature(Type delegateType)
		{
			var invokeMethod = delegateType.GetMethod("Invoke");
			return invokeMethod.GetParameters().Select(x => x.ParameterType).Concat(Enumerable.Repeat(invokeMethod.ReturnType, 1)).ToArray();
		}

		public static MemberInfo GetMemberEx<TDelegate>(Expression<TDelegate> lambda)
		{
			Expression exp = lambda.Body;
			if (exp.NodeType == ExpressionType.Convert || exp.NodeType == ExpressionType.ConvertChecked)
				exp = ((UnaryExpression)exp).Operand;
			if (exp.NodeType == ExpressionType.New)
				return ((NewExpression)exp).Constructor;
			if (exp.NodeType == ExpressionType.Call)
				return ((MethodCallExpression)exp).Method;
			if (exp.NodeType == ExpressionType.MemberAccess)
				return ((MemberExpression)exp).Member;
			return null;
		}

		public static MemberInfo GetMember(Expression<Func<object>> lambda) { return GetMemberEx(lambda); }

		public static MemberInfo GetMember(Expression<Action> lambda) { return GetMemberEx(lambda); }

		public static MemberInfo GetMember<T>(Expression<Func<T, object>> lambda) { return GetMemberEx(lambda); }

		public static MemberInfo GetMember<T>(Expression<Action<T>> lambda) { return GetMemberEx(lambda); }
	}
}
