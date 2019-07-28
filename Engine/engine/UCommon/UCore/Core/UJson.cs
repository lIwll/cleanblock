using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace UEngine
{
    public class UJson
    {
        protected enum EToken
        {
            eTOKEN_NONE,
            eTOKEN_CURLY_OPEN,
            eTOKEN_CURLY_CLOSE,
            eTOKEN_SQUARED_OPEN,
            eTOKEN_SQUARED_CLOSE,
            eTOKEN_COLON,
            eTOKEN_COMMA,
            eTOKEN_STRING,
            eTOKEN_NUMBER,
            eTOKEN_TRUE,
            eTOKEN_FALSE,
            eTOKEN_NULL,
        }

        private const int kBUILDER_CAPACITY = 2000;

        protected static int mLastErrorIndex = -1;
        protected static string mLastDecode = "";

        public static object JsonDecode(string json)
        {
            mLastDecode = json;
            if (json != null)
            {
                char[] charArray = json.ToCharArray();

                int index = 0;
                bool success = true;
                object value = ParseValue(charArray, ref index, ref success);

                if (success)
                    mLastErrorIndex = -1;
                else
                    mLastErrorIndex = index;

                return value;
            } else
            {
                return null;
            }
        }

        public static string JsonEncode(object json)
        {
            var builder = new StringBuilder(kBUILDER_CAPACITY);

            var success = SerializeValue(json, builder);

            return (success ? builder.ToString() : null);
        }

        public static bool LastDecodeSuccessful()
        {
            return (mLastErrorIndex == -1);
        }

        public static int GetLastErrorIndex()
        {
            return mLastErrorIndex;
        }

        public static string GetLastErrorSnippet()
        {
            if (mLastErrorIndex == -1)
            {
                return "";
            } else
            {
                int startIndex = mLastErrorIndex - 5;
                if (startIndex < 0)
                    startIndex = 0;

                int endIndex = mLastErrorIndex + 15;
                if (endIndex >= mLastDecode.Length)
                    endIndex = mLastDecode.Length - 1;

                return mLastDecode.Substring(startIndex, endIndex - startIndex + 1);
            }
        }

        protected static Hashtable ParseObject(char[] json, ref int index)
        {
            Hashtable table = new Hashtable();

            NextToken(json, ref index);

            bool done = false;
            while (!done)
            {
                EToken token = LookAhead(json, index);
                if (token == EToken.eTOKEN_NONE)
                {
                    return null;
                } else if (token == EToken.eTOKEN_COMMA)
                {
                    NextToken(json, ref index);
                } else if (token == EToken.eTOKEN_CURLY_CLOSE)
                {
                    NextToken(json, ref index);

                    return table;
                } else
                {
                    string name = ParseString(json, ref index);
                    if (name == null)
                        return null;

                    token = NextToken(json, ref index);
                    if (token != EToken.eTOKEN_COLON)
                        return null;

                    bool success = true;
                    object value = ParseValue(json, ref index, ref success);
                    if (!success)
                        return null;

                    table[name] = value;
                }
            }

            return table;
        }

        protected static ArrayList ParseArray(char[] json, ref int index)
        {
            ArrayList array = new ArrayList();

            NextToken(json, ref index);

            bool done = false;
            while (!done)
            {
                EToken token = LookAhead(json, index);
                if (token == EToken.eTOKEN_NONE)
                {
                    return null;
                } else if (token == EToken.eTOKEN_COMMA)
                {
                    NextToken(json, ref index);
                } else if (token == EToken.eTOKEN_SQUARED_CLOSE)
                {
                    NextToken(json, ref index);

                    break;
                } else
                {
                    bool success = true;
                    object value = ParseValue(json, ref index, ref success);
                    if (!success)
                        return null;

                    array.Add(value);
                }
            }

            return array;
        }

        protected static object ParseValue(char[] json, ref int index, ref bool success)
        {
            switch (LookAhead(json, index))
            {
                case EToken.eTOKEN_STRING:
                    return ParseString(json, ref index);
                case EToken.eTOKEN_NUMBER:
                    return ParseNumber(json, ref index);
                case EToken.eTOKEN_CURLY_OPEN:
                    return ParseObject(json, ref index);
                case EToken.eTOKEN_SQUARED_OPEN:
                    return ParseArray(json, ref index);
                case EToken.eTOKEN_TRUE:
                    NextToken(json, ref index);

                    return Boolean.Parse("TRUE");
                case EToken.eTOKEN_FALSE:
                    NextToken(json, ref index);

                    return Boolean.Parse("FALSE");
                case EToken.eTOKEN_NULL:
                    NextToken(json, ref index);

                    return null;
                case EToken.eTOKEN_NONE:
                    break;
            }

            success = false;

            return null;
        }

        protected static string ParseString(char[] json, ref int index)
        {
            string res = "";

            SkipWhitespace(json, ref index);

            char c = json[index ++];

            bool complete = false;
            while (!complete)
            {
                if (index == json.Length)
                    break;

                c = json[index++];
                if (c == '"')
                {
                    complete = true;

                    break;
                } else if (c == '\\')
                {
                    if (index == json.Length)
                        break;

                    c = json[index ++];
                    if (c == '"')
                    {
                        res += '"';
                    } else if (c == '\\')
                    {
                        res += '\\';
                    } else if (c == '/')
                    {
                        res += '/';
                    } else if (c == 'b')
                    {
                        res += '\b';
                    } else if (c == 'f')
                    {
                        res += '\f';
                    } else if (c == 'n')
                    {
                        res += '\n';
                    } else if (c == 'r')
                    {
                        res += '\r';
                    } else if (c == 't')
                    {
                        res += '\t';
                    } else if (c == 'u')
                    {
                        int remainingLength = json.Length - index;
                        if (remainingLength >= 4)
                        {
                            char[] unicodeCharArray = new char[4];
                            Array.Copy(json, index, unicodeCharArray, 0, 4);

                            res += "&#x" + new string(unicodeCharArray) + ";";

                            index += 4;
                        } else
                        {
                            break;
                        }

                    }
                } else
                {
                    res += c;
                }
            }

            if (!complete)
                return null;

            return res;
        }

        protected static double ParseNumber(char[] json, ref int index)
        {
            SkipWhitespace(json, ref index);

            int lastIndex = GetLastIndexOfNumber(json, index);
            int charLength = (lastIndex - index) + 1;

            char[] numberCharArray = new char[charLength];
            Array.Copy(json, index, numberCharArray, 0, charLength);

            index = lastIndex + 1;

            return Double.Parse(new string(numberCharArray));
        }

        protected static int GetLastIndexOfNumber(char[] json, int index)
        {
            int lastIndex;
            for (lastIndex = index; lastIndex < json.Length; lastIndex ++)
            {
                if ("0123456789+-.eE".IndexOf(json[lastIndex]) == -1)
                    break;
            }

            return lastIndex - 1;
        }

        protected static void SkipWhitespace(char[] json, ref int index)
        {
            for (; index < json.Length; index ++)
            {
                if (" \t\n\r".IndexOf(json[index]) == -1)
                    break;
            }
        }

        protected static EToken LookAhead(char[] json, int index)
        {
            int saveIndex = index;

            return NextToken(json, ref saveIndex);
        }

        protected static EToken NextToken(char[] json, ref int index)
        {
            SkipWhitespace(json, ref index);

            if (index == json.Length)
                return EToken.eTOKEN_NONE;

            char c = json[index ++];
            switch (c)
            {
                case '{':
                    return EToken.eTOKEN_CURLY_OPEN;
                case '}':
                    return EToken.eTOKEN_CURLY_CLOSE;
                case '[':
                    return EToken.eTOKEN_SQUARED_OPEN;
                case ']':
                    return EToken.eTOKEN_SQUARED_CLOSE;
                case ',':
                    return EToken.eTOKEN_COMMA;
                case '"':
                    return EToken.eTOKEN_STRING;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                    return EToken.eTOKEN_NUMBER;
                case ':':
                    return EToken.eTOKEN_COLON;
            }
            index --;

            int remainingLength = json.Length - index;

            if (remainingLength >= 5)
            {
                if (json[index] == 'f' && json[index + 1] == 'a' && json[index + 2] == 'l' && json[index + 3] == 's' && json[index + 4] == 'e')
                {
                    index += 5;

                    return EToken.eTOKEN_FALSE;
                }
            }

            if (remainingLength >= 4)
            {
                if (json[index] == 't' && json[index + 1] == 'r' && json[index + 2] == 'u' && json[index + 3] == 'e')
                {
                    index += 4;

                    return EToken.eTOKEN_TRUE;
                }
            }

            if (remainingLength >= 4)
            {
                if (json[index] == 'n' && json[index + 1] == 'u' && json[index + 2] == 'l' && json[index + 3] == 'l')
                {
                    index += 4;

                    return EToken.eTOKEN_NULL;
                }
            }

            return EToken.eTOKEN_NONE;
        }

        protected static bool SerializeObjectOrArray(object obj, StringBuilder builder)
        {
            if (obj is Hashtable)
                return SerializeObject((Hashtable)obj, builder);
            else if (obj is ArrayList)
                return SerializeArray((ArrayList)obj, builder);

            return false;
        }

        protected static bool SerializeObject(Hashtable obj, StringBuilder builder)
        {
            bool first = true;

            builder.Append("{");

            IDictionaryEnumerator e = obj.GetEnumerator();
            while (e.MoveNext())
            {
                string key = e.Key.ToString();
                object value = e.Value;

                if (!first)
                    builder.Append(", ");

                SerializeString(key, builder);

                builder.Append(":");

                if (!SerializeValue(value, builder))
                    return false;

                first = false;
            }

            builder.Append("}");

            return true;
        }

        protected static bool SerializeDictionary(Dictionary< string, string > dict, StringBuilder builder)
        {
            bool first = true;

            builder.Append("{");

            foreach (var kv in dict)
            {
                if (!first)
                    builder.Append(", ");

                SerializeString(kv.Key, builder);

                builder.Append(":");

                SerializeString(kv.Value, builder);

                first = false;
            }

            builder.Append("}");

            return true;
        }

        protected static bool SerializeArray(ArrayList array, StringBuilder builder)
        {
            bool first = true;

            builder.Append("[");

            for (int i = 0; i < array.Count; i ++)
            {
                object value = array[i];

                if (!first)
                    builder.Append(", ");

                if (!SerializeValue(value, builder))
                    return false;

                first = false;
            }

            builder.Append("]");

            return true;
        }

        protected static bool SerializeValue(object value, StringBuilder builder)
        {
            if (value == null)
                builder.Append("null");
            else if (value.GetType().IsArray)
                SerializeArray(new ArrayList((ICollection)value), builder);
            else if (value is string)
                SerializeString((string)value, builder);
            else if (value is Char)
                SerializeString(Convert.ToString((char)value), builder);
            else if (value is Hashtable)
                SerializeObject((Hashtable)value, builder);
            else if (value is Dictionary< string, string >)
                SerializeDictionary((Dictionary< string, string >)value, builder);
            else if (value is ArrayList)
                SerializeArray((ArrayList)value, builder);
            else if ((value is Boolean) && ((Boolean)value == true))
                builder.Append("true");
            else if ((value is Boolean) && ((Boolean)value == false))
                builder.Append("false");
            else if (value.GetType().IsPrimitive)
                SerializeNumber(Convert.ToDouble(value), builder);
            else
                return false;

            return true;
        }

        protected static void SerializeString(string str, StringBuilder builder)
        {
            builder.Append("\"");

            char[] charArray = str.ToCharArray();
            for (int i = 0; i < charArray.Length; i ++)
            {
                char c = charArray[i];

                switch (c)
                {
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        {
                            int codepoint = Convert.ToInt32(c);
                            if ((codepoint >= 32) && (codepoint <= 126))
                                builder.Append(c);
                            else
                                builder.Append("\\u" + Convert.ToString(codepoint, 16).PadLeft(4, '0'));
                        }
                        break;
                }
            }

            builder.Append("\"");
        }

        protected static void SerializeNumber(double number, StringBuilder builder)
        {
            builder.Append(Convert.ToString(number));
        }
    }

    public static class UJsonUtil
    {
        public static string ToJson(this Hashtable obj)
        {
            return UJson.JsonEncode(obj);
        }

        public static string ToJson(this Dictionary< string, string > obj)
        {
            return UJson.JsonEncode(obj);
        }

        public static ArrayList ArrayListFromJson(this string json)
        {
            return UJson.JsonDecode(json) as ArrayList;
        }

        public static Hashtable HashtableFromJson(this string json)
        {
            return UJson.JsonDecode(json) as Hashtable;
        }
    }
}
