using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Hosting.Shell;
using Microsoft.Scripting.Utils;

namespace IronTjs.Hosting
{
	public sealed class TjsConsole : BasicConsole
	{
		struct Cursor
		{
			int _anchorTop;
			int _anchorLeft;

			public void Anchor()
			{
				_anchorTop = Console.CursorTop;
				_anchorLeft = Console.CursorLeft;
			}

			public void Reset()
			{
				Console.CursorTop = _anchorTop;
				Console.CursorLeft = _anchorLeft;
			}

			public void Place(int index)
			{
				Console.CursorLeft = (_anchorLeft + index) % Console.BufferWidth;
				int cursorTop = _anchorTop + (_anchorLeft + index) / Console.BufferWidth;
				if (cursorTop >= Console.BufferHeight)
				{
					_anchorTop -= cursorTop - Console.BufferHeight + 1;
					cursorTop = Console.BufferHeight - 1;
				}
				Console.CursorTop = cursorTop;
			}
		}

		public TjsConsole(bool colorful, int tabWidth) : base(colorful) { _tabWidth = tabWidth; }

		StringBuilder _input = new StringBuilder();
		int _current;
		int _rendered;
		Cursor _cursor;
		int[] _inputToOutputTable = new[] { 0 };
		int _tabWidth;

		void Initialize()
		{
			_cursor.Anchor();
			_input.Length = 0;
			_current = 0;
			_rendered = 0;
		}

		void OnBackspace()
		{
			if (_input.Length > 0 && _current > 0)
			{
				_input.Remove(--_current, 1);
				Render();
			}
		}

		void OnDelete()
		{
			if (_input.Length > 0 && _current < _input.Length)
			{
				_input.Remove(_current, 1);
				Render();
			}
		}

		void Insert(char c)
		{
			_input.Insert(_current++, c);
			Render();
		}

		void Render()
		{
			_inputToOutputTable = new int[_input.Length + 1];
			_cursor.Reset();
			StringBuilder output = new StringBuilder();
			for (int i = 0; i < _input.Length; i++)
			{
				_inputToOutputTable[i] = output.Length;
				if (_input[i] == '\t')
				{
					for (int j = _tabWidth - output.Length % _tabWidth; j > 0; j--)
						output.Append(' ');
				}
				else if (char.IsControl(_input[i]))
				{
					if (_input[i] <= 26)
						output.AppendFormat("^{0}", (char)(_input[i] + 'A' - 1));
					else
						output.Append("^?");
				}
				else
					output.Append(_input[i]);
			}
			_inputToOutputTable[_input.Length] = output.Length;
			var text = output.ToString();
			Output.Write(text);
			if (text.Length < _rendered)
				Output.Write(new string(' ', _rendered - text.Length));
			_rendered = text.Length;
			_cursor.Place(_inputToOutputTable[_current]);
		}

		void MoveRight()
		{
			if (_current < _input.Length)
				_cursor.Place(_inputToOutputTable[++_current]);
		}

		void MoveLeft()
		{
			if (_current > 0)
				_cursor.Place(_inputToOutputTable[--_current]);
		}

		public override string ReadLine(int autoIndentSize)
		{
			Initialize();
			for (int i = 0; i < autoIndentSize / _tabWidth; i++)
				Insert('\t');
			while (true)
			{
				var key = Console.ReadKey(true);
				switch (key.Key)
				{
					case ConsoleKey.Backspace:
						OnBackspace();
						break;
					case ConsoleKey.Delete:
						OnDelete();
						break;
					case ConsoleKey.Enter:
						return OnEnter();
					case ConsoleKey.RightArrow:
						MoveRight();
						break;
					case ConsoleKey.LeftArrow:
						MoveLeft();
						break;
					default:
						if (key.KeyChar == '\r')
							goto case ConsoleKey.Enter;      // Ctrl-M
						if (key.KeyChar == '\b')
							goto case ConsoleKey.Backspace;  // Ctrl-H
						if (key.KeyChar != '\0')
							Insert(key.KeyChar);
						break;
				}
			}
		}

		string OnEnter()
		{
			Output.Write("\n");
			var line = _input.ToString();
			if (line == FinalLineText)
				return null;
			return line;
		}

		string FinalLineText { get { return Environment.OSVersion.Platform != PlatformID.Unix ? "\x1A" : "\x04"; } }
	}
}