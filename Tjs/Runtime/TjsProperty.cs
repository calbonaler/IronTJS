using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Runtime
{
	public class TjsProperty : IContextChangeable
	{
		public TjsProperty(Func<object, object[], object> getter, Func<object, object[], object> setter, object context)
		{
			_getter = getter;
			_setter = setter;
			Context = context;
		}

		Func<object, object[], object> _getter;
		Func<object, object[], object> _setter;

		public object Context { get; private set; }

		public object GetValue()
		{
			if (_getter == null)
				throw new InvalidOperationException("プロパティに getter が存在しないため右辺値となることができません。");
			return _getter(Context, new object[0]);
		}

		public object SetValue(object value)
		{
			if (_setter == null)
				throw new InvalidOperationException("プロパティに setter が存在しないため左辺値となることができません。");
			_setter(Context, new[] { value });
			return value;
		}

		public TjsProperty ChangeContext(object context) { return new TjsProperty(_getter, _setter, context); }

		object IContextChangeable.ChangeContext(object context) { return ChangeContext(context); }
	}
}
