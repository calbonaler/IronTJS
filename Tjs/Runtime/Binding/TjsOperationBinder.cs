using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Actions;

namespace IronTjs.Runtime.Binding
{
	class TjsOperationBinder : DynamicMetaObjectBinder
	{
		public TjsOperationBinder(TjsContext context, TjsOperationKind operation)
		{
			_context = context;
			_operation = operation;
		}

		readonly TjsContext _context;
		readonly TjsOperationKind _operation;

		public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
		{
			Expression convertedTarget = Expression.Convert(target.Expression, target.LimitType);
			Expression[] convertedArgs = args.Select(x => Expression.Convert(x.Expression, x.LimitType)).ToArray();
			Expression exp = null;
			int usedArgs = 0;
			switch (_operation)
			{
				// Unary
				case TjsOperationKind.CharCodeToChar:
					exp = Expression.Call(_context.Convert(convertedTarget, typeof(char)), "ToString", null);
					usedArgs = 1;
					break;
				case TjsOperationKind.CharToCharCode:
					exp = Expression.Call(new Func<string, long>(TjsOperationHelper.CharToCharCode).Method, _context.Convert(convertedTarget, typeof(string)));
					usedArgs = 1;
					break;
				case TjsOperationKind.Evaluate:
				case TjsOperationKind.Invalidate:
				case TjsOperationKind.IsValid:
				case TjsOperationKind.New:
					break;
				case TjsOperationKind.TypeOf:
					if (target.RuntimeType == null)
						exp = Expression.Constant("Object");
					if (target.RuntimeType == typeof(IronTjs.Builtins.TjsVoid))
						exp = Expression.Constant("void");
					else if (target.RuntimeType == typeof(string))
						exp = Expression.Constant("String");
					else if (Binders.IsInteger(target.RuntimeType))
						exp = Expression.Constant("Integer");
					else if (Binders.IsFloatingPoint(target.RuntimeType))
						exp = Expression.Constant("Real");
					else
						exp = Expression.Constant("Object");
					usedArgs = 1;
					break;
				// Unary (Special)
				case TjsOperationKind.InvokePropertyHandler:
					if (target.LimitType == typeof(TjsProperty))
					{
						if (args.Length == 0)
						{
							exp = Expression.Call(convertedTarget, "GetValue", null);
							usedArgs = 1;
						}
						else if (args.Length == 1)
						{
							exp = Expression.Call(convertedTarget, "SetValue", null, args[0].Expression);
							usedArgs = 2;
						}
						else
							throw Microsoft.Scripting.Utils.Assert.Unreachable;
					}
					else
						exp = Expression.Throw(Expression.Constant(new InvalidOperationException("プロパティ以外に対して * 演算子が使用されました。")), typeof(object));
					break;
				// Binary (Arithmetic & Logical)
				case TjsOperationKind.FloorDivide:
					exp = Expression.Divide(_context.Convert(convertedTarget, typeof(long)), _context.Convert(convertedArgs[0], typeof(long)));
					usedArgs = 2;
					break;
				case TjsOperationKind.RightShiftLogical:
					exp = Expression.Convert(Expression.RightShift(_context.Convert(convertedTarget, typeof(ulong)), _context.Convert(convertedArgs[0], typeof(int))), typeof(long));
					usedArgs = 2;
					break;
				// Binary (Comparison)
				case TjsOperationKind.DistinctEqual:
					if (convertedTarget.Type == convertedArgs[0].Type)
						exp = Expression.Condition(Expression.Equal(convertedTarget, convertedArgs[0]), Expression.Constant(1L), Expression.Constant(0L));
					else
						exp = Expression.Constant(0L);
					usedArgs = 2;
					break;
				case TjsOperationKind.DistinctNotEqual:
					if (convertedTarget.Type == convertedArgs[0].Type)
						exp = Expression.Condition(Expression.Equal(convertedTarget, convertedArgs[0]), Expression.Constant(0L), Expression.Constant(1L));
					else
						exp = Expression.Constant(1L);
					usedArgs = 2;
					break;
				// Binary (Special)
				case TjsOperationKind.InstanceOf:
					exp = Expression.Condition(Expression.Equal(Expression.Constant(convertedTarget.Type.Name), _context.Convert(convertedArgs[0], typeof(string))),
						Expression.Constant(1L),
						Expression.Constant(0L)
					);
					usedArgs = 2;
					break;
				case TjsOperationKind.InContextOf:
					if (target.Value is IContextChangeable)
						exp = Expression.Call(Expression.Convert(convertedTarget, typeof(IContextChangeable)), "ChangeContext", null, args[0].Expression);
					else if (target.LimitType == typeof(BoundMemberTracker))
						exp = Expression.New(typeof(BoundMemberTracker).GetConstructor(new[] { typeof(MemberTracker), typeof(object) }), Expression.PropertyOrField(convertedTarget, "BoundTo"), args[0].Expression);
					usedArgs = 2;
					break;
			}
			if (exp == null)
				throw new NotImplementedException();
			if (exp.Type != typeof(object))
				exp = Expression.Convert(exp, typeof(object));
			var restrictions = target.Restrictions.Merge(BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
			for (int i = 0; i < usedArgs - 1; i++)
				restrictions = restrictions.Merge(args[i].Restrictions).Merge(BindingRestrictions.GetTypeRestriction(args[i].Expression, args[i].LimitType));
			return new DynamicMetaObject(exp, restrictions);
		}
	}

	static class TjsOperationHelper
	{
		public static long CharToCharCode(string s) { return s.Length > 0 ? s[0] : 0; }
	}
}
