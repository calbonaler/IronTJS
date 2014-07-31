using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronTjs.Compiler
{
	public class Tokenizer : TokenizerService
	{
		TextReader _reader;
		SourceUnit _sourceUnit;
		string _line = string.Empty;
		int _columnIndex = 0;
		SourceLocation _baseLocation;
		Token _nextToken = null;

		public override void Initialize(object state, TextReader sourceReader, SourceUnit sourceUnit, SourceLocation initialLocation)
		{
			_reader = sourceReader;
			_sourceUnit = sourceUnit;
			_baseLocation = initialLocation;
			Read();
		}

		public Token Read()
		{
			var token = _nextToken;
			do
			{
				if (_line == null)
					return token;
				if (_columnIndex >= _line.Length)
				{
					_baseLocation = new SourceLocation(_baseLocation.Index + _line.Length, _baseLocation.Line + 1, 1);
					_line = _reader.ReadLine();
					if (_line == null)
					{
						_nextToken = new Token(TokenType.EndOfStream, null, new SourceSpan(_baseLocation, _baseLocation));
						return token;
					}
					_columnIndex = 0;
				}
				while (_columnIndex < _line.Length && char.IsWhiteSpace(_line[_columnIndex]))
					_columnIndex++;
				if (_columnIndex < _line.Length && _line.IndexOf("//", _columnIndex) == _columnIndex)
				{
					_columnIndex += 2;
					while (_columnIndex < _line.Length)
						_columnIndex++;
				}
			} while (_columnIndex >= _line.Length);
			var start = CurrentPosition;
			if (char.IsLetter(_line[_columnIndex]) || _line[_columnIndex] == '_')
			{
				StringBuilder sb = new StringBuilder();
				while (_columnIndex < _line.Length && (char.IsLetterOrDigit(_line[_columnIndex]) || _line[_columnIndex] == '_'))
					sb.Append(_line[_columnIndex++]);
				_nextToken = new Token(Token.GetTokenTypeForWord(sb.ToString()), sb.ToString(), new SourceSpan(start, CurrentPosition));
			}
			else if (char.IsDigit(_line[_columnIndex]))
			{
				long value = 0;
				while (_columnIndex < _line.Length && char.IsDigit(_line[_columnIndex]))
					value = value * 10 + _line[_columnIndex++] - '0';
				long fraction = 0;
				long divisor = 1;
				if (_columnIndex < _line.Length && _line[_columnIndex] == '.')
				{
					_columnIndex++;
					while (_columnIndex < _line.Length && char.IsDigit(_line[_columnIndex]))
					{
						fraction = fraction * 10 + _line[_columnIndex++] - '0';
						divisor *= 10;
					}
				}
				if (divisor > 1)
					_nextToken = new Token(TokenType.LiteralReal, (double)value + (double)fraction / divisor, new SourceSpan(start, CurrentPosition));
				else
					_nextToken = new Token(TokenType.LiteralInteger, value, new SourceSpan(start, CurrentPosition));
			}
			else if (_line[_columnIndex] == '"' || _line[_columnIndex] == '\'')
			{
				var quote = _line[_columnIndex++];
				StringBuilder sb = new StringBuilder();
				while (true)
				{
					if (_columnIndex >= _line.Length)
					{
						ErrorSink.Add(_sourceUnit, "文字列トークンが予期せず終了しました。", new SourceSpan(start, CurrentPosition), -1, Severity.Error);
						_nextToken = new Token(TokenType.Unknown, sb.ToString(), new SourceSpan(start, CurrentPosition));
						break;
					}
					else if (_line[_columnIndex] == quote)
					{
						_columnIndex++;
						_nextToken = new Token(TokenType.LiteralString, sb.ToString(), new SourceSpan(start, CurrentPosition));
						break;
					}
					sb.Append(_line[_columnIndex++]);
				}
			}
			else
			{
				StringBuilder sb = new StringBuilder();
				string completeSymbol = null;
				int index = 0;
				while (_columnIndex < _line.Length && !char.IsWhiteSpace(_line[_columnIndex]))
				{
					sb.Append(_line[_columnIndex++]);
					if (Token.GetTokenTypeForSymbol(sb.ToString()) != TokenType.Unknown)
					{
						completeSymbol = sb.ToString();
						index = _columnIndex;
					}
				}
				if (completeSymbol != null)
				{
					_columnIndex = index;
					_nextToken = new Token(Token.GetTokenTypeForSymbol(completeSymbol), completeSymbol, new SourceSpan(start, CurrentPosition));
				}
				else
					_nextToken = new Token(TokenType.Unknown, sb.ToString(), new SourceSpan(start, CurrentPosition));
			}
			return token;
		}

		public Token NextToken { get { return _nextToken; } }

		public override SourceLocation CurrentPosition { get { return new SourceLocation(_baseLocation.Index + _columnIndex, _baseLocation.Line, _baseLocation.Column + _columnIndex); } }

		public override object CurrentState { get { return null; } }

		public override ErrorSink ErrorSink { get; set; }

		public override bool IsRestartable { get { return true; } }

		public override TokenInfo ReadToken() { return Read().ToTokenInfo(); }
	}
}
