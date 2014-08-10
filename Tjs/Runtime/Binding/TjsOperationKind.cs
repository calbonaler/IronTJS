using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Runtime.Binding
{
	public enum TjsOperationKind
	{
		None,

		// Unary
		AccessPropertyObject,
		CharCodeToChar,
		CharToCharCode,
		Decrement,
		Delete,
		Evaluate,
		Increment,
		Invalidate,
		InvokePropertyHandler,
		IsValid,
		Negate,
		New,
		Not,
		OnesComplement,
		TypeOf,
		UnaryPlus,
		
		// Binary (Arithmetic & Logical)
		Add,
		And,
		AndAlso,
		Divide,
		ExclusiveOr,
		FloorDivide,
		LeftShift,
		Modulo,
		Multiply,
		Or,
		OrElse,
		RightShiftArithmetic,
		RightShiftLogical,
		Subtract,

		// Binary (Comparison)
		GreaterThan,
		GreaterThanOrEqual,
		LessThan,
		LessThanOrEqual,
		Equal,
		NotEqual,
		DistinctEqual,
		DistinctNotEqual,
		
		// Binary (Special)
		Exchange,
		InstanceOf,
		InContextOf,
		
		// Unary (Composite)
		PostDecrementAssign = Decrement | PostAssign,
		PostIncrementAssign = Increment | PostAssign,
		PreDecrementAssign = Decrement | Assign,
		PreIncrementAssign = Increment | Assign,

		// Binary (InPlace)
		AddAssign = Add | Assign,
		AndAssign = And | Assign,
		AndAlsoAssign = AndAlso | Assign,
		DivideAssign = Divide | Assign,
		ExclusiveOrAssign = ExclusiveOr | Assign,
		FloorDivideAssign = FloorDivide | Assign,
		LeftShiftAssign = LeftShift | Assign,
		ModuloAssign = Modulo | Assign,
		MultiplyAssign = Multiply | Assign,
		OrAssign = Or | Assign,
		OrElseAssign = OrElse | Assign,
		RightShiftArithmeticAssign = RightShiftArithmetic | Assign,
		RightShiftLogicalAssign = RightShiftLogical | Assign,
		SubtractAssign = Subtract | Assign,

		Assign = 0x10000000,
		PostAssign = 0x20000000,
		ValueMask = 0x0FFFFFFF,
		ModifierMask = unchecked((int)0xF0000000),
	}
}
