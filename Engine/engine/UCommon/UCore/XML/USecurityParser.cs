using System;
using System.Collections;
using System.IO;
using System.Security;

namespace UEngine
{
    public class USecurityParser : USimpleXmlParser, USimpleXmlParser.IContentHandler
    {
        private SecurityElement mRoot;
        private SecurityElement mCurrent;
        private Stack           mStack;

        public USecurityParser()
            : base()
        {
            mStack = new Stack();
        }

        public void LoadXml(string xml)
        {
            mRoot = null;

            mStack.Clear();

            Parse(new StringReader(xml), this);
        }

        public SecurityElement ToXml()
        {
            return mRoot;
        }

        public void OnStartParsing(USimpleXmlParser parser)
        { }

        public void OnProcessingInstruction(string name, string text)
        { }

        public void OnIgnorableWhitespace(string s)
        { }

        public void OnStartElement(string name, USimpleXmlParser.IAttrList attrs)
        {
            SecurityElement newel = new SecurityElement(name);
            if (mRoot == null)
            {
                mRoot = newel;

                mCurrent = newel;
            } else
            {
                SecurityElement parent = (SecurityElement)mStack.Peek();

                parent.AddChild(newel);
            }
            mStack.Push(newel);
            mCurrent = newel;

            int n = attrs.Length;
            for (int i = 0; i < n; i++)
                mCurrent.AddAttribute(attrs.GetName(i), attrs.GetValue(i));
        }

        public void OnEndElement(string name)
        {
            mCurrent = (SecurityElement)mStack.Pop();
        }

        public void OnChars(string ch)
        {
            mCurrent.Text = ch;
        }

        public void OnEndParsing(USimpleXmlParser parser)
        { }
    }
}
