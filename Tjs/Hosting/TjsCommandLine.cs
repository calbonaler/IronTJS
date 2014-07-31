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
			if (result == null)
				Console.WriteLine("null", Style.Out);
			else if (result.GetType() == typeof(string))
				Console.WriteLine(string.Format("\"{0}\"", result), Style.Out);
			else
				Console.WriteLine(result.ToString(), Style.Out);
		}
	}
}
