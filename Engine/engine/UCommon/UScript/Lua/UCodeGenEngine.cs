using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.Text;

using ULuaState = System.IntPtr;

namespace UEngine.ULua
{
    public enum ETokenType
    {
        eTT_Code, eTT_Eval, eTT_Text
    }

    public class UChunk
    {
        public ETokenType Type
        {
            get;
            private set;
        }

        public string Text
        {
            get;
            private set;
        }

        public UChunk(ETokenType type, string text)
        {
            Type = type;
            Text = text;
        }
    }

    class UCodeFormatException : Exception
    {
        public UCodeFormatException(string msg)
        {
        }
    }

    public class UCodeParser
    {
        public static string RegexString
        {
            get;
            private set;
        }

        static UCodeParser()
        {
            RegexString = GetRegexString();
        }

        static string EscapeString(string code)
        {
            var output = code
                .Replace("\\", @"\\")
                .Replace("\'", @"\'")
                .Replace("\"", @"\""")
                .Replace("\n", @"\n")
                .Replace("\t", @"\t")
                .Replace("\r", @"\r")
                .Replace("\b", @"\b")
                .Replace("\f", @"\f")
                .Replace("\a", @"\a")
                .Replace("\v", @"\v")
                .Replace("\0", @"\0");

            return output;
        }

        static string GetRegexString()
        {
            string regexBadUnopened = @"(?<error>((?!<%).)*%>)";
            string regexText = @"(?<text>((?!<%).)+)";
            string regexNoCode = @"(?<nocode><%=?%>)";
            string regexCode = @"<%(?<code>[^=]((?!<%|%>).)*)%>";
            string regexEval = @"<%=(?<eval>((?!<%|%>).)*)%>";
            string regexBadUnclosed = @"(?<error><%.*)";
            string regexBadEmpty = @"(?<error>^$)";

            return '(' + regexBadUnopened
                + '|' + regexText
                + '|' + regexNoCode
                + '|' + regexCode
                + '|' + regexEval
                + '|' + regexBadUnclosed
                + '|' + regexBadEmpty
                + ")*";
        }

        public static List< UChunk > Parse(string snippet)
        {
            Regex codeRegex = new Regex(RegexString, RegexOptions.ExplicitCapture | RegexOptions.Singleline);

            Match matches = codeRegex.Match(snippet);

            if (matches.Groups["error"].Length > 0)
                throw new UCodeFormatException("Messed up brackets");

            List< UChunk > chunks = matches.Groups["code"].Captures
                .Cast< Capture >()
                .Select(p => new { Type = ETokenType.eTT_Code, p.Value, p.Index })
                .Concat(matches.Groups["text"].Captures
                .Cast< Capture >()
                .Select(p => new { Type = ETokenType.eTT_Text, Value = EscapeString(p.Value), p.Index }))
                .Concat(matches.Groups["eval"].Captures
                .Cast< Capture >()
                .Select(p => new { Type = ETokenType.eTT_Eval, p.Value, p.Index }))
                .OrderBy(p => p.Index)
                .Select(m => new UChunk(m.Type, m.Value))
                .ToList();

            if (chunks.Count == 0)
                throw new UCodeFormatException("Empty code parser");

            return chunks;
        }
    }

    public class UCodeGen
    {
        public static string ComposeCode(List< UChunk > chunks)
        {
            StringBuilder code = new StringBuilder();

            code.Append("local __text_gen = { };\r\n");
            for (int i = 0; i < chunks.Count; ++ i)
            {
				var chunk = chunks[i];

                switch (chunk.Type)
                {
                    case ETokenType.eTT_Text:
                        code.Append("table.insert(__text_gen, \"" + chunk.Text + "\");\r\n");
                        break;
                    case ETokenType.eTT_Eval:
                        code.Append("table.insert(__text_gen, tostring(" + chunk.Text + "));\r\n");
                        break;
                    case ETokenType.eTT_Code:
                        code.Append(chunk.Text + "\r\n");
                        break;
                }
            }

            code.Append("return table.concat(__text_gen);\r\n");

            return code.ToString();
        }

        public static ULuaFunction Compile(ULuaEnv env, string snippet)
        {
            return env.LoadString(ComposeCode(UCodeParser.Parse(snippet)), "LuaGen");
        }

        public static string Execute(ULuaFunction compiled, ULuaTable parameters)
        {
            compiled.SetEnv(parameters);
            object[] result = compiled.Call();

            return result[0].ToString();
        }

        public static string Execute(ULuaFunction compiled)
        {
            object[] result = compiled.Call();

            return result[0].ToString();
        }

        [UMonoPInvokeCallbackAttribute(typeof(lua_CSFunction))]
        public static int Compile(ULuaState L)
        {
            string snippet = ULuaAPI.lua_tostring(L, 1);

            string code;
            try
            {
                code = ComposeCode(UCodeParser.Parse(snippet));
            } catch (Exception e)
            {
                return ULuaAPI.luaL_error(L, String.Format("code compile error:{0}\r\n", e.Message));
            }

            if (ULuaAPI.luaL_loadbuffer(L, code, "LuaGen") != 0)
                return ULuaAPI.lua_error(L);

            return 1;
        }

        [UMonoPInvokeCallbackAttribute(typeof(lua_CSFunction))]
        public static int Execute(ULuaState L)
        {
            if (!ULuaAPI.lua_isfunction(L, 1))
                return ULuaAPI.luaL_error(L, "invalid compiled code, function needed!\r\n");

            if (ULuaAPI.lua_istable(L, 2))
                ULuaAPI.lua_setfenv(L, 1);

            ULuaAPI.lua_pcall(L, 0, 1, 0);

            return 1;
        }

        public static void OpenLib(ULuaState L)
        {
            ULuaAPI.lua_newtable(L);
            ULuaAPI.luaex_pushasciistring(L, "compile");
            ULuaAPI.lua_pushstdcallcfunction(L, Compile);
            ULuaAPI.lua_rawset(L, -3);
            ULuaAPI.luaex_pushasciistring(L, "execute");
            ULuaAPI.lua_pushstdcallcfunction(L, Execute);
            ULuaAPI.lua_rawset(L, -3);

            if (0 != ULuaAPI.luaex_setglobal(L, "codeGen"))
                throw new Exception("call luaex_setglobal fail!");
        }
    }
}
