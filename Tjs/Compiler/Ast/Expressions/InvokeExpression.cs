using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronTjs.Builtins;
using Microsoft.Scripting.Utils;

namespace IronTjs.Compiler.Ast
{
	public class InvokeExpression : Expression
	{
		public InvokeExpression(Expression target, IEnumerable<Expression> arguments)
		{
			Target = target;
			Arguments = arguments.ToReadOnly();
			Target.Parent = this;
			foreach (var arg in Arguments)
			{
				if (arg != null)
					arg.Parent = this;
			}
		}

		public Expression Target { get; private set; }

		public ReadOnlyCollection<Expression> Arguments { get; private set; }

		public override System.Linq.Expressions.Expression TransformRead()
		{
			var targetExp = Target.TransformRead();
			var argumentsExp = Enumerable.Repeat(targetExp, 1).Concat(Arguments.Select(x => x != null ? x.TransformRead() : System.Linq.Expressions.Expression.Constant(IronTjs.Builtins.TjsVoid.Value))).ToArray();
			return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateInvokeBinder(new System.Dynamic.CallInfo(Arguments.Count)), typeof(object), argumentsExp);
		}

		public override System.Linq.Expressions.Expression TransformWrite(System.Linq.Expressions.Expression value) { throw new InvalidOperationException("関数呼び出しを左辺値とすることはできません。"); }

		public override System.Linq.Expressions.Expression TransformVoid() { return Microsoft.Scripting.Ast.Utils.Void(TransformRead()); }
	}
}
