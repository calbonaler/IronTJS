using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Runtime
{
	public interface IContextChangeable
	{
		object Context { get; }

		IContextChangeable ChangeContext(object context);
	}
}
