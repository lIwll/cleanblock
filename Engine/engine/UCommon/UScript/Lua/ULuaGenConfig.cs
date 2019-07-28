using System;
using System.Collections.Generic;

namespace UEngine.ULua
{
    public interface IGenConfig 
    {
        List< Type > LuaCallCSharp { get; }

        List< Type > CSharpCallLua { get; }

        List< List< string > > BlackList { get; }
    }

    public interface IGCOptimizeConfig
    {
        List< Type > TypeList { get; }

        Dictionary< Type, List< string > > AdditionalProperties { get; }
    }

    public interface IReflectionConfig
    {
        List< Type > ReflectionUse { get; }
    }
}