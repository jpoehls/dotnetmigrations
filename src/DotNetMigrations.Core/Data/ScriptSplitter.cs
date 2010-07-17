/*
Copyright (c) 2005 - 2010, Phil Haack
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

    *	Redistributions of source code must retain the above copyright notice, 
		this list of conditions and the following disclaimer.
    *	Redistributions in binary form must reproduce the above copyright notice, 
		this list of conditions and the following disclaimer in the documentation 
		and/or other materials provided with the distribution.
    *	Neither the name of the Subtext nor the names of its contributors 
		may be used to endorse or promote products derived from this software 
		without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY 
OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE 
OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED 
OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNetMigrations.Core.Data
{
    public class ScriptSplitter : IEnumerable<string>
    {
        private readonly TextReader _reader;
        private StringBuilder _builder = new StringBuilder();
        private char _current;
        private char _lastChar;
        private ScriptReader _scriptReader;

        public ScriptSplitter(string script)
        {
            _reader = new StringReader(script);
            _scriptReader = new SeparatorLineReader(this);
        }

        internal bool HasNext
        {
            get { return _reader.Peek() != -1; }
        }

        internal char Current
        {
            get { return _current; }
        }

        internal char LastChar
        {
            get { return _lastChar; }
        }

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            while (Next())
            {
                if (Split())
                {
                    string script = _builder.ToString().Trim();
                    if (script.Length > 0)
                    {
                        yield return (script);
                    }
                    Reset();
                }
            }
            if (_builder.Length > 0)
            {
                string scriptRemains = _builder.ToString().Trim();
                if (scriptRemains.Length > 0)
                {
                    yield return (scriptRemains);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        internal bool Next()
        {
            if (!HasNext)
            {
                return false;
            }

            _lastChar = _current;
            _current = (char) _reader.Read();
            return true;
        }

        internal int Peek()
        {
            return _reader.Peek();
        }

        private bool Split()
        {
            return _scriptReader.ReadNextSection();
        }

        internal void SetParser(ScriptReader newReader)
        {
            _scriptReader = newReader;
        }

        internal void Append(string text)
        {
            _builder.Append(text);
        }

        internal void Append(char c)
        {
            _builder.Append(c);
        }

        private void Reset()
        {
            _current = _lastChar = char.MinValue;
            _builder = new StringBuilder();
        }
    }

    internal abstract class ScriptReader
    {
        protected readonly ScriptSplitter Splitter;

        protected ScriptReader(ScriptSplitter splitter)
        {
            Splitter = splitter;
        }

        /// <summary>
        /// This acts as a template method. Specific Reader instances 
        /// override the component methods.
        /// </summary>
        public bool ReadNextSection()
        {
            if (IsQuote)
            {
                ReadQuotedString();
                return false;
            }

            if (BeginDashDashComment)
            {
                return ReadDashDashComment();
            }

            if (BeginSlashStarComment)
            {
                ReadSlashStarComment();
                return false;
            }

            return ReadNext();
        }

        protected virtual bool ReadDashDashComment()
        {
            Splitter.Append(Current);
            while (Splitter.Next())
            {
                Splitter.Append(Current);
                if (EndOfLine)
                {
                    break;
                }
            }
            //We should be EndOfLine or EndOfScript here.
            Splitter.SetParser(new SeparatorLineReader(Splitter));
            return false;
        }

        protected virtual void ReadSlashStarComment()
        {
            if (ReadSlashStarCommentWithResult())
            {
                Splitter.SetParser(new SeparatorLineReader(Splitter));
                return;
            }
        }

        private bool ReadSlashStarCommentWithResult()
        {
            Splitter.Append(Current);
            while (Splitter.Next())
            {
                if (BeginSlashStarComment)
                {
                    ReadSlashStarCommentWithResult();
                    continue;
                }
                Splitter.Append(Current);

                if (EndSlashStarComment)
                {
                    return true;
                }
            }
            return false;
        }

        protected virtual void ReadQuotedString()
        {
            Splitter.Append(Current);
            while (Splitter.Next())
            {
                Splitter.Append(Current);
                if (IsQuote)
                {
                    return;
                }
            }
        }

        protected abstract bool ReadNext();

        #region Helper methods and properties

        protected bool HasNext
        {
            get { return Splitter.HasNext; }
        }

        protected bool WhiteSpace
        {
            get { return char.IsWhiteSpace(Splitter.Current); }
        }

        protected bool EndOfLine
        {
            get { return '\n' == Splitter.Current; }
        }

        protected bool IsQuote
        {
            get { return '\'' == Splitter.Current; }
        }

        protected char Current
        {
            get { return Splitter.Current; }
        }

        protected char LastChar
        {
            get { return Splitter.LastChar; }
        }

        private bool BeginDashDashComment
        {
            get { return Current == '-' && Peek() == '-'; }
        }

        private bool BeginSlashStarComment
        {
            get { return Current == '/' && Peek() == '*'; }
        }

        private bool EndSlashStarComment
        {
            get { return LastChar == '*' && Current == '/'; }
        }

        protected static bool CharEquals(char expected, char actual)
        {
            return Char.ToLowerInvariant(expected) == Char.ToLowerInvariant(actual);
        }

        protected bool CharEquals(char compare)
        {
            return CharEquals(Current, compare);
        }

        protected char Peek()
        {
            if (!HasNext)
            {
                return char.MinValue;
            }
            return (char) Splitter.Peek();
        }

        #endregion
    }

    internal class SeparatorLineReader : ScriptReader
    {
        private StringBuilder _builder = new StringBuilder();
        private bool _foundGo;
        private bool _gFound;

        public SeparatorLineReader(ScriptSplitter splitter)
            : base(splitter)
        {
        }

        private void Reset()
        {
            _foundGo = false;
            _gFound = false;
            _builder = new StringBuilder();
        }

        protected override bool ReadDashDashComment()
        {
            if (!_foundGo)
            {
                base.ReadDashDashComment();
                return false;
            }
            base.ReadDashDashComment();
            return true;
        }

        protected override void ReadSlashStarComment()
        {
            if (_foundGo)
            {
                throw new SqlParseException("Incorrect syntax near GO keyword.");
            }
            base.ReadSlashStarComment();
        }

        protected override bool ReadNext()
        {
            if (EndOfLine) //End of line or script
            {
                if (!_foundGo)
                {
                    _builder.Append(Current);
                    Splitter.Append(_builder.ToString());
                    Splitter.SetParser(new SeparatorLineReader(Splitter));
                    return false;
                }
                Reset();
                return true;
            }

            if (WhiteSpace)
            {
                _builder.Append(Current);
                return false;
            }

            if (!CharEquals('g') && !CharEquals('o'))
            {
                FoundNonEmptyCharacter(Current);
                return false;
            }

            if (CharEquals('o'))
            {
                if (CharEquals('g', LastChar) && !_foundGo)
                {
                    _foundGo = true;
                }
                else
                {
                    FoundNonEmptyCharacter(Current);
                }
            }

            if (CharEquals('g', Current))
            {
                if (_gFound || (!Char.IsWhiteSpace(LastChar) && LastChar != char.MinValue))
                {
                    FoundNonEmptyCharacter(Current);
                    return false;
                }

                _gFound = true;
            }

            if (!HasNext && _foundGo)
            {
                Reset();
                return true;
            }

            _builder.Append(Current);
            return false;
        }

        private void FoundNonEmptyCharacter(char c)
        {
            _builder.Append(c);
            Splitter.Append(_builder.ToString());
            Splitter.SetParser(new SqlScriptReader(Splitter));
        }
    }

    internal class SqlScriptReader : ScriptReader
    {
        public SqlScriptReader(ScriptSplitter splitter)
            : base(splitter)
        {
        }

        protected override bool ReadNext()
        {
            if (EndOfLine) //end of line
            {
                Splitter.Append(Current);
                Splitter.SetParser(new SeparatorLineReader(Splitter));
                return false;
            }

            Splitter.Append(Current);
            return false;
        }
    }
}