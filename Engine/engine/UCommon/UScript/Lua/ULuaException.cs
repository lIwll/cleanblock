using System;

namespace UEngine.ULua
{
    [Serializable]
    public class ULuaException : Exception
    {
        public ULuaException(string message) : base(message)
        { }
    }
}
