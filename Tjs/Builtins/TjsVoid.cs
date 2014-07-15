using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Builtins
{
	public sealed class TjsVoid
	{
		public static readonly TjsVoid Value = new TjsVoid();

		TjsVoid() { }

		public override string ToString() { return "void"; }
	}
}
