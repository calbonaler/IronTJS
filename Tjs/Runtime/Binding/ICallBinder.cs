using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Actions;

namespace IronTjs.Runtime.Binding
{
	public interface ICallBinder
	{
		CallSignature Signature { get; }
	}
}
