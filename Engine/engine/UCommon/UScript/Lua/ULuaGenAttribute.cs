using System;
using System.Collections.Generic;

namespace UEngine.ULua
{
    public class ULuaCallCSharpAttribute : Attribute
    {
        ELuaGenFlag mFlag;

        public ELuaGenFlag Flag
        {
            get
            {
                return mFlag;
            }
        }

        public ULuaCallCSharpAttribute(ELuaGenFlag flag = ELuaGenFlag.eGF_No)
        {
            mFlag = flag;
        }
    }

    public class UCSharpCallLuaAttribute : Attribute
    {
    }

    public class UBlackListAttribute : Attribute
    {
    }

    public class UGCOptimizeAttribute : Attribute
    {
    }

    public class UReflectionUseAttribute : Attribute
    {
    }

    public class UAdditionalPropertiesAttribute : Attribute
    {
    }

    public static class USysGenConfig
    {
        [UGCOptimize]
        static List< Type > GCOptimize
        {
            get
            {
                return new List< Type >()
                {
                    typeof(UnityEngine.Vector2),
                    typeof(UnityEngine.Vector3),
                    typeof(UnityEngine.Vector4),
                    typeof(UnityEngine.Color),
                    typeof(UnityEngine.Quaternion),
                    typeof(UnityEngine.Ray),
                    typeof(UnityEngine.Bounds),
                    typeof(UnityEngine.Ray2D),

					typeof(UObjectBase),
                };
            }
        }

        [UAdditionalProperties]
        static Dictionary< Type, List< string > > AdditionalProperties
        {
            get
            {
                return new Dictionary< Type, List< string > >()
                {
                    { typeof(UnityEngine.Ray), new List< string >() { "origin", "direction" } },
                    { typeof(UnityEngine.Ray2D), new List< string >() { "origin", "direction" } },
                    { typeof(UnityEngine.Bounds), new List< string >() { "center", "extents" } },
                };
            }
        }
    }
}
