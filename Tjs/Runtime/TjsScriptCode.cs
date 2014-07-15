using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace IronTjs.Runtime
{
	class TjsScriptCode : ScriptCode
	{
		public TjsScriptCode(Expression<Func<object, object>> code, SourceUnit sourceUnit) : base(sourceUnit) { _code = code; }

		readonly Expression<Func<object, object>> _code;

		Func<object, object> _target;

		Func<object, object> Target
		{
			get
			{
				if (_target == null)
				{
					Func<object, object> compiledMethod;
					if (SourceUnit.LanguageContext.Options.NoAdaptiveCompilation)
						compiledMethod = _code.Compile();
					else
						compiledMethod = _code.LightCompile(SourceUnit.LanguageContext.Options.CompilationThreshold);
					System.Threading.Interlocked.CompareExchange(ref _target, compiledMethod, null);
				}
				return _target;
			}
		}

		public override object Run(Scope scope) { return Target(scope); }
	}
}
