using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Runtime.Binding
{
	interface ITjsOperable
	{
		DynamicMetaObject BindOperation(TjsOperationBinder binder, DynamicMetaObject[] args);
	}
}
