using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Actions;

namespace IronTjs.Builtins
{
	public static class Exception
	{
		public static readonly ExtensionPropertyTracker messageProperty = new ExtensionPropertyTracker("message", typeof(System.Exception).GetMethod("get_Message"), null, null, typeof(System.Exception));
	}
}
