using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs
{
	static class Utils
	{
		public static Type[] GetDelegateSignature(Type delegateType)
		{
			var invokeMethod = delegateType.GetMethod("Invoke");
			return invokeMethod.GetParameters().Select(x => x.ParameterType).Concat(Enumerable.Repeat(invokeMethod.ReturnType, 1)).ToArray();
		}
	}
}
