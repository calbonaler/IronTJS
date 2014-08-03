using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using IronTjs.Compiler;
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

		public override ConvertBinder CreateConvertBinder(Type toType, bool? explicitCast) { return new TjsConvertBinder(this, toType, explicitCast ?? true); }

		public override InvokeBinder CreateInvokeBinder(CallInfo callInfo) { return new TjsInvokeBinder(this, callInfo); }

		public override UnaryOperationBinder CreateUnaryOperationBinder(System.Linq.Expressions.ExpressionType operation) { return new TjsUnaryOperationBinder(this, operation); }

		public override BinaryOperationBinder CreateBinaryOperationBinder(System.Linq.Expressions.ExpressionType operation) { return new TjsBinaryOperationBinder(this, operation); }

		public override GetMemberBinder CreateGetMemberBinder(string name, bool ignoreCase) { return new TjsGetMemberBinder(this, name, ignoreCase); }

		public override SetMemberBinder CreateSetMemberBinder(string name, bool ignoreCase) { return CreateSetMemberBinder(name, ignoreCase, true); }

		public SetMemberBinder CreateSetMemberBinder(string name, bool ignoreCase, bool forceCreate) { return new TjsSetMemberBinder(this, name, ignoreCase, forceCreate); }
		
		public override DeleteMemberBinder CreateDeleteMemberBinder(string name, bool ignoreCase) { return new CompatibilityDeleteMemberBinder(this, name, ignoreCase); }

		public DynamicMetaObjectBinder CreateDeleteMemberBinder(string name, bool ignoreCase, bool noThrow)
		{
			if (noThrow)
				return new TjsDeleteMemberBinder(this, name, ignoreCase);
			else
				return CreateDeleteMemberBinder(name, ignoreCase);
		}

		public GetIndexBinder CreateGetIndexBinder(CallInfo callInfo) { return new TjsGetIndexBinder(this, callInfo); }

		public SetIndexBinder CreateSetIndexBinder(CallInfo callInfo) { return new TjsSetIndexBinder(this, callInfo); }

		public DynamicMetaObjectBinder CreateDeleteIndexBinder(CallInfo callInfo) { return new TjsDeleteIndexBinder(this, callInfo); }

#if !DEBUG
		public override string FormatException(Exception exception)
		{
			if (exception is NotImplementedException)
				return base.FormatException(exception);
			if (exception is MissingMemberException)
				return string.Format("\"{0}\" という名前のオブジェクトは指定されたスコープに存在しません。", exception.Message);
			return exception.GetType().ToString() + ": " + exception.Message;
		}
#endif
	}
}
