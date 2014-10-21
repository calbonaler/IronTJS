using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Builtins
{
	public sealed class Void
	{
		public static readonly Void Value = new Void();

		Void() { }

		public override string ToString() { return ""; }
	}
}
