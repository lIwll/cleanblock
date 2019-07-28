using System;
using System.Reflection;
using System.Collections.Generic;

namespace UEngine
{
    public static class UTypeCache
    {
        private static Dictionary< string, Type > mTypePool = new Dictionary< string, Type >();

        public static Type GetType(string type)
        {
            Type objType = null;

            if (mTypePool.ContainsKey(type))
                return mTypePool[type];

            Assembly[] assemblys = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblys.Length; ++ i)
            {
				var assembly = assemblys[i];

                objType = assembly.GetType(type, false, true);
                if (null != objType)
                    break;
            }

            mTypePool.Add(type, objType);

            return objType;
        }

        public static void Clear()
        {
            mTypePool.Clear();
        }
    }
}
