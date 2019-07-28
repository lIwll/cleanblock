using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;

namespace UEngine
{
    internal class DefaultHandler : USimpleXmlParser.IContentHandler
    {
        public void OnStartParsing(USimpleXmlParser parser)
        {
        }

        public void OnEndParsing(USimpleXmlParser parser)
        {
        }

        public void OnStartElement(string name, USimpleXmlParser.IAttrList attrs)
        {
        }

        public void OnEndElement(string name)
        {
        }

        public void OnChars(string s)
        {
        }

        public void OnIgnorableWhitespace(string s)
        {
        }

        public void OnProcessingInstruction(string name, string text)
        {
        }
    }

    public class USimpleXmlParser
    {
        public interface IContentHandler
        {
            void OnStartParsing(USimpleXmlParser parser);

            void OnEndParsing(USimpleXmlParser parser);

            void OnStartElement(string name, IAttrList attrs);

            void OnEndElement(string name);

            void OnProcessingInstruction(string name, string text);

            void OnChars(string text);

            void OnIgnorableWhitespace(string text);
        }

        public interface IAttrList
        {
            int Length { get; }

            bool IsEmpty { get; }

            string GetName(int i);

            string GetValue(int i);

            string GetValue(string name);

            string[] Names { get; }

            string[] Values { get; }
        }

        class UAttrListImpl : IAttrList
        {
            ArrayList mAttrNames = new ArrayList();
            ArrayList mAttrValues = new ArrayList();

            public int Length
            {
                get { return mAttrNames.Count; }
            }

            public bool IsEmpty
            {
                get { return mAttrNames.Count == 0; }
            }

            public string GetName(int i)
            {
                return (string)mAttrNames[i];
            }

            public string GetValue(int i)
            {
                return (string)mAttrValues[i];
            }

            public string GetValue(string name)
            {
                for (int i = 0; i < mAttrNames.Count; i++)
                {
                    if ((string)mAttrNames[i] == name)
                        return (string)mAttrValues[i];
                }

                return null;
            }

            public string[] Names
            {
                get { return (string[])mAttrNames.ToArray(typeof(string)); }
            }
            public string[] Values
            {
                get { return (string[])mAttrValues.ToArray(typeof(string)); }
            }

            internal void Clear()
            {
                mAttrNames.Clear();
                mAttrValues.Clear();
            }

            internal void Add(string name, string value)
            {
                mAttrNames.Add(name);
                mAttrValues.Add(value);
            }
        }

        IContentHandler mHandler;
        TextReader      mReader;
        Stack           mElementNames = new Stack();
        Stack           mXMLSpaces = new Stack();
        string          mXMLSpace;
        StringBuilder   mBuffer = new StringBuilder(200);
        char[]          mNameBuffer = new char[30];
        bool            mIsWhitespace;

        UAttrListImpl   mAttributes = new UAttrListImpl();
        int             mLine = 1;
        int             mColumn;
        bool            mResetColumn;

        public USimpleXmlParser()
        {
        }

        private Exception Error(string msg)
        {
            return new USimpleXmlParserException(msg, mLine, mColumn);
        }

        private Exception UnexpectedEndError()
        {
            string[] arr = new string[mElementNames.Count];

            (mElementNames as ICollection).CopyTo(arr, 0);

            return Error(String.Format("Unexpected end of stream. Element stack content is {0}", String.Join(",", arr)));
        }

        private bool IsNameChar(char c, bool start)
        {
            switch (c)
            {
                case ':':
                case '_':
                    return true;
                case '-':
                case '.':
                    return !start;
            }

            if (c > 0x100)
            {
                switch (c)
                {
                    case '\u0559':
                    case '\u06E5':
                    case '\u06E6':
                        return true;
                }

                if ('\u02BB' <= c && c <= '\u02C1')
                    return true;
            }

            switch (Char.GetUnicodeCategory(c))
            {
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.LetterNumber:
                    return true;
                case UnicodeCategory.SpacingCombiningMark:
                case UnicodeCategory.EnclosingMark:
                case UnicodeCategory.NonSpacingMark:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.DecimalDigitNumber:
                    return !start;
                default:
                    return false;
            }
        }

        private bool IsWhitespace(int c)
        {
            switch (c)
            {
                case ' ':
                case '\r':
                case '\t':
                case '\n':
                    return true;
            }

            return false;
        }

        public void SkipWhitespaces()
        {
            SkipWhitespaces(false);
        }

        private void HandleWhitespaces()
        {
            while (IsWhitespace(Peek()))
                mBuffer.Append((char)Read());

            if (Peek() != '<' && Peek() >= 0)
                mIsWhitespace = false;
        }

        public void SkipWhitespaces(bool expected)
        {
            while (true)
            {
                switch (Peek())
                {
                    case ' ':
                    case '\r':
                    case '\t':
                    case '\n':
                        Read();
                        if (expected)
                            expected = false;
                        continue;
                }

                if (expected)
                    throw Error("Whitespace is expected.");

                return;
            }
        }

        private int Peek()
        {
            return mReader.Peek();
        }

        private int Read()
        {
            int i = mReader.Read();
            if (i == '\n')
                mResetColumn = true;
            if (mResetColumn)
            {
                mLine ++;
                mColumn = 1;
                mResetColumn = false;
            } else
            {
                mColumn ++;
            }

            return i;
        }

        public void Expect(int c)
        {
            int p = Read();
            if (p < 0)
                throw UnexpectedEndError();
            else if (p != c)
                throw Error(String.Format("Expected '{0}' but got {1}", (char)c, (char)p));
        }

        private string ReadUntil(char until, bool handleReferences)
        {
            while (true)
            {
                if (Peek() < 0)
                    throw UnexpectedEndError();
                char c = (char)Read();
                if (c == until)
                    break;
                else if (handleReferences && c == '&')
                    ReadReference();
                else
                    mBuffer.Append(c);
            }

            string ret = mBuffer.ToString();
            mBuffer.Length = 0;

            return ret;
        }

        public string ReadName()
        {
            int idx = 0;
            if (Peek() < 0 || !IsNameChar((char)Peek(), true))
                throw Error("XML name start character is expected.");

            for (int i = Peek(); i >= 0; i = Peek())
            {
                char c = (char)i;
                if (!IsNameChar(c, false))
                    break;
                if (idx == mNameBuffer.Length)
                {
                    char[] tmp = new char[idx * 2];

                    Array.Copy(mNameBuffer, 0, tmp, 0, idx);

                    mNameBuffer = tmp;
                }
                mNameBuffer[idx++] = c;
                Read();
            }

            if (idx == 0)
                throw Error("Valid XML name is expected.");

            return new string(mNameBuffer, 0, idx);
        }

        public void Parse(TextReader input, IContentHandler handler)
        {
            mReader = input;
            mHandler = handler;

            mHandler.OnStartParsing(this);

            while (Peek() >= 0)
                ReadContent();
            HandleBufferedContent();
            if (mElementNames.Count > 0)
                throw Error(String.Format("Insufficient close tag: {0}", mElementNames.Peek()));

            mHandler.OnEndParsing(this);

            Cleanup();
        }

        private void Cleanup()
        {
            mLine = 1;
            mColumn = 0;
            mHandler = null;
            mReader = null;
            mElementNames.Clear();
            mXMLSpaces.Clear();
            mAttributes.Clear();
            mBuffer.Length = 0;
            mXMLSpace = null;
            mIsWhitespace = false;
        }

        public void ReadContent()
        {
            string name;
            if (IsWhitespace(Peek()))
            {
                if (mBuffer.Length == 0)
                    mIsWhitespace = true;
                HandleWhitespaces();
            }

            if (Peek() == '<')
            {
                Read();

                switch (Peek())
                {
                    case '!': // declarations
                        Read();

                        if (Peek() == '[')
                        {
                            Read();

                            if (ReadName() != "CDATA")
                                throw Error("Invalid declaration markup");
                            Expect('[');
                            ReadCDATASection();

                            return;
                        } else if (Peek() == '-')
                        {
                            ReadComment();

                            return;
                        } else if (ReadName() != "DOCTYPE")
                        {
                            throw Error("Invalid declaration markup.");
                        }
                        throw Error("This parser does not support document type.");
                    case '?': // PIs
                        HandleBufferedContent();
                        Read();
                        name = ReadName();
                        SkipWhitespaces();
                        string text = string.Empty;
                        if (Peek() != '?')
                        {
                            while (true)
                            {
                                text += ReadUntil('?', false);
                                if (Peek() == '>')
                                    break;
                                text += "?";
                            }
                        }
                        mHandler.OnProcessingInstruction(name, text);
                        Expect('>');

                        return;
                    case '/': // end tags
                        HandleBufferedContent();
                        if (mElementNames.Count == 0)
                            throw UnexpectedEndError();
                        Read();
                        name = ReadName();
                        SkipWhitespaces();
                        string expected = (string)mElementNames.Pop();

                        mXMLSpaces.Pop();
                        if (mXMLSpaces.Count > 0)
                            mXMLSpace = (string)mXMLSpaces.Peek();
                        else
                            mXMLSpace = null;

                        if (name != expected)
                            throw Error(String.Format("End tag mismatch: expected {0} but found {1}", expected, name));
                        mHandler.OnEndElement(name);
                        Expect('>');

                        return;
                    default:
                        HandleBufferedContent();
                        name = ReadName();
                        while (Peek() != '>' && Peek() != '/')
                            ReadAttribute(mAttributes);
                        mHandler.OnStartElement(name, mAttributes);
                        mAttributes.Clear();
                        SkipWhitespaces();
                        if (Peek() == '/')
                        {
                            Read();
                            mHandler.OnEndElement(name);
                        } else
                        {
                            mElementNames.Push(name);
                            mXMLSpaces.Push(mXMLSpace);
                        }
                        Expect('>');

                        return;
                }
            } else
            {
                ReadCharacters();
            }
        }

        private void HandleBufferedContent()
        {
            if (mBuffer.Length == 0)
                return;
            if (mIsWhitespace)
                mHandler.OnIgnorableWhitespace(mBuffer.ToString());
            else
                mHandler.OnChars(mBuffer.ToString());
            mBuffer.Length = 0;
            mIsWhitespace = false;
        }

        private void ReadCharacters()
        {
            mIsWhitespace = false;

            while (true)
            {
                int i = Peek();
                switch (i)
                {
                    case -1:
                        return;
                    case '<':
                        return;
                    case '&':
                        Read();
                        ReadReference();
                        continue;
                    default:
                        mBuffer.Append((char)Read());
                        continue;
                }
            }
        }

        private void ReadReference()
        {
            if (Peek() == '#')
            {
                Read();
                ReadCharacterReference();
            } else
            {
                string name = ReadName();
                Expect(';');
                switch (name)
                {
                    case "amp":
                        mBuffer.Append('&');
                        break;
                    case "quot":
                        mBuffer.Append('"');
                        break;
                    case "apos":
                        mBuffer.Append('\'');
                        break;
                    case "lt":
                        mBuffer.Append('<');
                        break;
                    case "gt":
                        mBuffer.Append('>');
                        break;
                    default:
                        throw Error("General non-predefined entity reference is not supported in this parser.");
                }
            }
        }

        private int ReadCharacterReference()
        {
            int n = 0;
            if (Peek() == 'x')
            {
                Read();

                for (int i = Peek(); i >= 0; i = Peek())
                {
                    if ('0' <= i && i <= '9')
                        n = n << 4 + i - '0';
                    else if ('A' <= i && i <= 'F')
                        n = n << 4 + i - 'A' + 10;
                    else if ('a' <= i && i <= 'f')
                        n = n << 4 + i - 'a' + 10;
                    else
                        break;

                    Read();
                }
            } else
            {
                for (int i = Peek(); i >= 0; i = Peek())
                {
                    if ('0' <= i && i <= '9')
                        n = n << 4 + i - '0';
                    else
                        break;

                    Read();
                }
            }

            return n;
        }

        private void ReadAttribute(UAttrListImpl a)
        {
            SkipWhitespaces(true);

            if (Peek() == '/' || Peek() == '>')
                return;

            string name = ReadName();
            string value;
            SkipWhitespaces();
            Expect('=');
            SkipWhitespaces();
            switch (Read())
            {
                case '\'':
                    value = ReadUntil('\'', true);
                    break;
                case '"':
                    value = ReadUntil('"', true);
                    break;
                default:
                    throw Error("Invalid attribute value markup.");
            }

            if (name == "xml:space")
                mXMLSpace = value;

            a.Add(name, value);
        }

        private void ReadCDATASection()
        {
            int nBracket = 0;
            while (true)
            {
                if (Peek() < 0)
                    throw UnexpectedEndError();

                char c = (char)Read();
                if (c == ']')
                {
                    nBracket ++;
                } else if (c == '>' && nBracket > 1)
                {
                    for (int i = nBracket; i > 2; i--)
                        mBuffer.Append(']');

                    break;
                } else
                {
                    for (int i = 0; i < nBracket; i++)
                        mBuffer.Append(']');
                    nBracket = 0;
                    mBuffer.Append(c);
                }
            }

            string text = UCoreUtil.EncodeXmlText(mBuffer.ToString());
            mBuffer.Length = 0;

			for (int i = 0; i < text.Length; ++ i)
				mBuffer.Append(text[i]);
        }

        private void ReadComment()
        {
            Expect('-');
            Expect('-');
            while (true)
            {
                if (Read() != '-')
                    continue;
                if (Read() != '-')
                    continue;
                if (Read() != '>')
                    throw Error("'--' is not allowed inside comment markup.");
                break;
            }
        }
    }

    internal class USimpleXmlParserException : SystemException
    {
        int mLine;
        int mColumn;

        public USimpleXmlParserException(string msg, int line, int column)
            : base(String.Format("{0}. At ({1},{2})", msg, line, column))
        {
            mLine = line;
            mColumn = column;
        }

        public int Line
        {
            get { return mLine; }
        }

        public int Column
        {
            get { return mColumn; }
        }
    }
}
