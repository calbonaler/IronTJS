using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Compiler.Ast
{
	public class DirectMemberAccessExpression : Expression
	{
		public DirectMemberAccessExpression(Expression target, string memberName)
		{
			Target = target;
			MemberName = memberName;
			if (Target != null)
				Target.Parent = this;
		}

		public Expression Target { get; private set; }

		public string MemberName { get; private set; }

		System.Linq.Expressions.Expression TargetExpression
		{
			get
			{
				if (Target != null)
					return Target.TransformRead();
				Node node = this;
				while (node.Parent != null)
				{
					var with = node as WithStatement;
					if (with != null)
						return with.AccessibleVariable;
					node = node.Parent;
				}
				return ((SourceUnitTree)node).GlobalObject;
			}
		}

		public override System.Linq.Expressions.Expression TransformRead()
		{
			return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateGetMemberBinder(MemberName, false), typeof(object), TargetExpression);
		}

		public override System.Linq.Expressions.Expression TransformWrite(System.Linq.Expressions.Expression value)
		{
			return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateSetMemberBinder(MemberName, false, true), typeof(object), TargetExpression, value);
		}

		public override System.Linq.Expressions.Expression TransformVoid() { return System.Linq.Expressions.Expression.Empty(); }
	}

	public class IndirectMemberAccessExpression : Expression
	{
		public IndirectMemberAccessExpression(Expression target, Expression member)
		{
			Target = target;
			Member = member;
			Target.Parent = Member.Parent = this;
		}

		public Expression Target { get; private set; }

		public Expression Member { get; private set; }

		public override System.Linq.Expressions.Expression TransformRead()
		{
			throw new NotImplementedException();
		}

		public override System.Linq.Expressions.Expression TransformWrite(System.Linq.Expressions.Expression value)
		{
			throw new NotImplementedException();
		}

		public override System.Linq.Expressions.Expression TransformVoid()
		{
			throw new NotImplementedException();
		}
	}
}
