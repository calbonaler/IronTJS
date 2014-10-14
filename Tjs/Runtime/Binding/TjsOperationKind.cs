using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Runtime.Binding
{
	public enum TjsOperationKind
	{
		// Unary
		CharCodeToChar,
		CharToCharCode,
		Evaluate,
		Invalidate,
		InvokePropertyHandler,
		IsValid,
		TypeOf,
		
		// Binary (Arithmetic & Logical)
		FloorDivide,
		RightShiftLogical,

		// Binary (Comparison)
		DistinctEqual,
		DistinctNotEqual,
		
		// Binary (Special)
		InstanceOf,
		InContextOf,
	}
}
