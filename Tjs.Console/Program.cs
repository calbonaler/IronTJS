using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronTjs.Hosting;
using IronTjs.Runtime;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Shell;

sealed class TjsConsoleHost : ConsoleHost
{
	protected override Type Provider { get { return typeof(TjsContext); } }

	protected override CommandLine CreateCommandLine() { return new TjsCommandLine(InitializeScope); }

	void InitializeScope(ScriptScope scope)
	{
		scope.SetVariable("print", new TjsFunction((context, args) =>
		{
			if (args.Length <= 0)
				Console.WriteLine();
			else if (args.Length <= 1)
				Console.WriteLine(args[0]);
			else if (args[0] != null)
				Console.WriteLine(args[0].ToString(), Microsoft.Scripting.Utils.ArrayUtils.RemoveFirst(args));
			return IronTjs.Builtins.TjsVoid.Value;
		}, null));
		scope.SetVariable("scan", new TjsFunction((context, args) =>
		{
			if (args.Length > 0)
				Console.Write(args[0]);
			return Console.ReadLine();
		}, null));
	}

	static int Main(string[] args)
	{
		return new TjsConsoleHost().Run(args);
	}
}
