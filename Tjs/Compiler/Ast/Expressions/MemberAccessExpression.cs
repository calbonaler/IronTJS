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
			Target.Parent = this;
		}

		public Expression Target { get; private set; }

		public string MemberName { get; private set; }

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
