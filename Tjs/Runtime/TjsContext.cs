using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronTjs.Compiler.Parser;
using IronTjs.Runtime.Binding;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronTjs.Runtime
{
	public sealed class TjsContext : LanguageContext
	{
		public TjsContext(ScriptDomainManager manager, IDictionary<string, object> options) : base(manager)
		{
			Binder = new TjsBinder();
		}

		public override ScriptCode CompileSourceCode(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink)
		{
			var context = new CompilerContext(sourceUnit, options, errorSink);
			var ast = new Parser().Parse(context);
			if (ast == null)
				return null;
			var exp = ast.Transform<Func<object, object>>();
			return new TjsScriptCode(exp, sourceUnit);
		}

		public override CompilerOptions GetCompilerOptions() { return new CompilerOptions(); }

		public override Guid LanguageGuid { get { return new Guid("B6632EF3-555B-454A-816C-7430ADA873C8"); } }

		public override Version LanguageVersion { get { return new Version("1.0.0.0"); } }

		public TjsBinder Binder { get; private set; }

		public override UnaryOperationBinder CreateUnaryOperationBinder(System.Linq.Expressions.ExpressionType operation) { return base.CreateUnaryOperationBinder(operation); }

		public override BinaryOperationBinder CreateBinaryOperationBinder(System.Linq.Expressions.ExpressionType operation) { return new TjsBinaryOperationBinder(this, operation); }

		public override GetMemberBinder CreateGetMemberBinder(string name, bool ignoreCase) { return new TjsGetMemberBinder(this, name, ignoreCase); }

		public override SetMemberBinder CreateSetMemberBinder(string name, bool ignoreCase) { return new CompatibilitySetMemberBinder(this, name, ignoreCase); }

		internal TjsSetMemberBinder CreateSetMemberBinder(string name, bool ignoreCase, bool forceCreate) { return new TjsSetMemberBinder(this, name, ignoreCase, forceCreate); }

		public override string FormatException(Exception exception)
		{
			if (exception is NotImplementedException)
				return base.FormatException(exception);
			return exception.GetType().ToString() + ": " + exception.Message;
		}
	}
}
