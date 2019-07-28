using System;
using System.Security;
using System.Collections.Generic;

namespace UEngine.Data
{
	public static class ULanguage
	{
		static int mLanguage = -1;
		static Dictionary< string, string > mStringMap = new Dictionary< string, string >();

		static ULanguage()
		{
			mLanguage	= -1;
			mStringMap	= new Dictionary< string, string >();
		}

		public static bool Load(int lan)
		{
			if (mLanguage == lan)
				return true;

			mLanguage = lan;

			mStringMap.Clear();

			var text = UFileAccessor.ReadStringFile(USystemConfig.kLanguagePath + USystemConfig.kLanguagePrefix + lan.ToString() + ".xml");
			if (null == text)
			{
				ULogger.Error("error to load language {0} map.", lan);

				return false;
			}

			var xml = UXMLParser.LoadXML(text);
			if (null == xml)
			{
				ULogger.Error("error to load language {0} map.", lan);

				return false;
			}

			if (null == xml.Children)
				return false;

            for (int i = 0; i < xml.Children.Count; ++i )
            {
                SecurityElement language = (SecurityElement)xml.Children[i];
                var key = language.SearchForChildByTag("key");
                var value = language.SearchForChildByTag("value");

                mStringMap.Add(UCoreUtil.DecodeXmlText(key.Text), UCoreUtil.DecodeXmlText(value.Text));
            }

			return true;
		}

		public static string Get(string str)
		{
			string ret;
			if (mStringMap.TryGetValue(str, out ret))
				return ret;

			return str;
		}
	}
}
