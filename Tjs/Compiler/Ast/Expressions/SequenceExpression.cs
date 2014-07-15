using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Utils;

namespace IronTjs.Compiler.Ast
{
	public class SequenceExpression : Expression
	{
		public SequenceExpression(IEnumerable<Expression> expressions)
		{
			Expressions = expressions.ToReadOnly();
			foreach (var exp in Expressions)
				exp.Parent = this;
		}

		public ReadOnlyCollection<Expression> Expressions { get; private set; }

		public override System.Linq.Expressions.Expression TransformRead()
		{
			var transformed = Expressions.Take(Expressions.Count - 1).Select(x => x.TransformVoid()).ToArray();
			var transformedLast = Expressions.Last().TransformRead();
			return System.Linq.Expressions.Expression.Block(transformed.Concat(Enumerable.Repeat(transformedLast, 1)));
		}

		public override System.Linq.Expressions.Expression TransformWrite(System.Linq.Expressions.Expression value) { throw new InvalidOperationException("カンマ式を左辺値とすることはできません。"); }

		public override System.Linq.Expressions.Expression TransformVoid()
		{
			var transformed = Expressions.Select(x => x.TransformVoid()).ToArray();
			return System.Linq.Expressions.Expression.Block(transformed);
		}
	}
}
