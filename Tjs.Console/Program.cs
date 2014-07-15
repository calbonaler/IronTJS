using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronTjs.Hosting;
using IronTjs.Runtime;
using Microsoft.Scripting.Hosting.Shell;

sealed class TjsConsoleHost : ConsoleHost
{
	protected override Type Provider { get { return typeof(TjsContext); } }

	protected override CommandLine CreateCommandLine() { return new TjsCommandLine(); }

	static int Main(string[] args)
	{
		return new TjsConsoleHost().Run(args);
	}
}
