using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting.Shell;

namespace IronTjs.Hosting
{
	public class TjsCommandLine : CommandLine
	{
		protected override void ExecuteCommand(string command)
		{
			var result = ExecuteCommand(Engine.CreateScriptSourceFromString(command, SourceCodeKind.InteractiveCode));
			Console.WriteLine(result != null ? result.ToString() : "null", Style.Out);
		}
	}
}
