using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Builtins
{
	public class Array : IEnumerable<object>
	{
		public Array() { _buffer = new CircularBuffer<object>(); }

		public Array(IEnumerable<object> collection) { _buffer = new CircularBuffer<object>(collection); }

		CircularBuffer<object> _buffer = new CircularBuffer<object>();

		public object this[int index]
		{
			get
			{
				if (index < 0)
					return _buffer[_buffer.Count - index];
				else
					return _buffer[index];
			}
			set
			{
				if (index < 0)
					_buffer[_buffer.Count - index] = value;
				else if (index < _buffer.Count)
					_buffer[index] = value;
				else
				{
					while (_buffer.Count < index)
						_buffer.Add(Void.Value);
					_buffer.Add(value);
				}
			}
		}

		public int count
		{
			get { return _buffer.Count; }
			set
			{
				if (value > _buffer.Count)
				{
					while (_buffer.Count < value)
						_buffer.Add(Void.Value);
				}
				else if (value < _buffer.Count)
				{
					while (_buffer.Count > value)
						_buffer.RemoveAt(_buffer.Count - 1);
				}
			}
		}

		public Array load(string fileName, string mode = "") { throw new NotImplementedException(); }

		public Array save(string fileName, string mode = "") { throw new NotImplementedException(); }

		public void split(object patternOrDelimiters, string text, object reserved = null, bool removeEmptyEntries = false)
		{
			if (text != null)
			{
				var delimiters = patternOrDelimiters as string;
				if (delimiters != null)
				{
					_buffer.Clear();
					foreach (var item in text.Split(delimiters.ToCharArray(), removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None))
						_buffer.Add(item);
				}
			}
		}

		public string join(string delimiter, object reserved = null, bool removeEmptyEntries = false)
		{
			IEnumerable<object> target = _buffer;
			if (removeEmptyEntries)
				target = _buffer.Where(x => !ReferenceEquals(x, Void.Value));
			return string.Join(delimiter, target);
		}

		public void reverse()
		{
			object temp;
			for (int i = 0; i < _buffer.Count / 2; i++)
			{
				temp = _buffer[i];
				_buffer[i] = _buffer[_buffer.Count - i - 1];
				_buffer[_buffer.Count - i - 1] = temp;
			}
		}

		public void sort(object comparer = null, bool stable = false)
		{
			if (comparer == null || comparer == Void.Value)
				comparer = "+";
			throw new NotImplementedException();
		}

		public Array saveStruct(string fileName, string mode = "")
		{
			System.IO.File.WriteAllText(fileName, Utils.ConvertToExpression(this, 0));
			return this;
		}

		public void assign(IEnumerable source)
		{
			_buffer.Clear();
			var dict = source as IDictionary;
			if (dict != null)
			{
				var enumerator = dict.GetEnumerator();
				while (enumerator.MoveNext())
				{
					_buffer.Add(enumerator.Key);
					_buffer.Add(enumerator.Value);
				}
			}
			else
			{
				foreach (var item in source)
					_buffer.Add(item);
			}
		}

		public void assignStruct(IEnumerable source)
		{
			_buffer.Clear();
			var dictSource = source as IDictionary;
			if (dictSource != null)
			{
				var de = dictSource.GetEnumerator();
				while (de.MoveNext())
				{
					_buffer.Add(de.Key);
					var subSource = de.Value as IEnumerable;
					if (subSource != null)
					{
						var subArray = new Array();
						subArray.assignStruct(subSource);
						_buffer.Add(subArray);
					}
					else
						_buffer.Add(de.Value);
				}
			}
			else
			{
				foreach (var item in source)
				{
					var subSource = item as IEnumerable;
					if (subSource != null)
					{
						var subArray = new Array();
						subArray.assignStruct(subSource);
						_buffer.Add(subArray);
					}
					else
						_buffer.Add(item);
				}
			}
		}

		public void clear() { _buffer.Clear(); }

		public void erase(int index) { _buffer.RemoveAt(index); }

		public void remove(object item, bool removeAll = true)
		{
			for (int i = _buffer.Count - 1; i >= 0; i--)
			{
				if (EqualityComparer<object>.Default.Equals(_buffer[i], item))
				{
					_buffer.RemoveAt(i);
					if (!removeAll)
						break;
				}
			}
		}

		public void insert(int index, object item) { _buffer.Insert(index, item); }

		public int add(object item)
		{
			_buffer.Add(item);
			return _buffer.Count - 1;
		}

		public int find(object item, int startIndex = 0)
		{
			for (int i = startIndex; i < _buffer.Count; i++)
			{
				if (EqualityComparer<object>.Default.Equals(_buffer[i], item))
					return i;
			}
			return -1;
		}

		public int push(params object[] items)
		{
			foreach (var item in items)
				_buffer.Add(item);
			return _buffer.Count;
		}

		public object pop()
		{
			if (_buffer.Count > 0)
			{
				var last = _buffer[_buffer.Count - 1];
				_buffer.RemoveAt(_buffer.Count - 1);
				return last;
			}
			return Void.Value;
		}

		public int unshift(params object[] items)
		{
			for (int i = items.Length - 1; i >= 0; i--)
				_buffer.Insert(0, items[i]);
			return _buffer.Count;
		}

		public object shift()
		{
			if (_buffer.Count > 0)
			{
				var first = _buffer[0];
				_buffer.RemoveAt(0);
				return first;
			}
			return Void.Value;
		}

		IEnumerator<object> IEnumerable<object>.GetEnumerator() { return _buffer.GetEnumerator(); }
		
		IEnumerator IEnumerable.GetEnumerator() { return _buffer.GetEnumerator(); }
	}
}
