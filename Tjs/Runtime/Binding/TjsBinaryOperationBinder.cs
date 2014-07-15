using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronTjs.Runtime.Binding
{
	class TjsBinaryOperationBinder : BinaryOperationBinder
	{
		public TjsBinaryOperationBinder(TjsContext context, ExpressionType operation) : base(operation) { _context = context; }

		readonly TjsContext _context;

		public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
		{
			var left = Expression.Convert(target.Expression, target.LimitType);
			var right = Expression.Convert(arg.Expression, arg.LimitType);
			Expression res = null;
			switch (Operation)
			{
				case ExpressionType.Add:
					res = Expression.Add(left, right);
					break;
				case ExpressionType.And:
					res = Expression.And(Expression.Convert(left, typeof(long)), Expression.Convert(right, typeof(long)));
					break;
				case ExpressionType.Divide:
					res = Expression.Divide(Expression.Convert(left, typeof(double)), Expression.Convert(right, typeof(double)));
					break;
				case ExpressionType.ExclusiveOr:
					res = Expression.ExclusiveOr(Expression.Convert(left, typeof(long)), Expression.Convert(right, typeof(long)));
					break;
				case ExpressionType.LeftShift:
					res = Expression.LeftShift(Expression.Convert(left, typeof(long)), Expression.Convert(right, typeof(long)));
					break;
				case ExpressionType.Modulo:
					res = Expression.Modulo(left, right);
					break;
				case ExpressionType.Multiply:
					res = Expression.Multiply(left, right);
					break;
				case ExpressionType.Or:
					res = Expression.Or(Expression.Convert(left, typeof(long)), Expression.Convert(right, typeof(long)));
					break;
				case ExpressionType.RightShift:
					res = Expression.RightShift(Expression.Convert(left, typeof(long)), Expression.Convert(right, typeof(long)));
					break;
				case ExpressionType.Subtract:
					res = Expression.Subtract(left, right);
					break;
			}
			var restrictions = BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target).Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(arg));
			if (res != null)
				return new DynamicMetaObject(Expression.Convert(res, typeof(object)), restrictions);
			else
				return errorSuggestion ?? new DynamicMetaObject(Expression.Throw(Expression.Constant(new InvalidOperationException("不正な二項演算です。")), typeof(object)), restrictions);
		}
	}
}
