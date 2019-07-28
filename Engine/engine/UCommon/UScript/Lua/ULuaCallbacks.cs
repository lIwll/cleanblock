using System;
using System.IO;
using System.Reflection;

using ULuaState = System.IntPtr;

namespace UEngine.ULua
{
    public partial class ULuaCallbacks
    {
        internal lua_CSFunction GCMeta;
        internal lua_CSFunction ToStringMeta;
        internal lua_CSFunction StaticCSFunctionWraper;
        internal lua_CSFunction FixCSFunctionWraper;

        public delegate bool TryArrayGet(Type type, ULuaState L, UObjectParser parser, object obj, int index);
        public delegate bool TryArraySet(Type type, ULuaState L, UObjectParser parser, object obj, int array_idx, int obj_idx);

        static TryArrayGet          GenTryArrayGetPtr = null;
        internal static TryArraySet GenTryArraySetPtr = null;

        public ULuaCallbacks()
        {
            GCMeta                  = new lua_CSFunction(ULuaCallbacks.LuaGC);
            ToStringMeta            = new lua_CSFunction(ULuaCallbacks.ToString);
            StaticCSFunctionWraper  = new lua_CSFunction(ULuaCallbacks.StaticCSFunction);
            FixCSFunctionWraper     = new lua_CSFunction(ULuaCallbacks.FixCSFunction);
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        static int StaticCSFunction(ULuaState L)
        {
            try
            {
                UObjectParser parser = UObjectParserPool.Instance.Find(L);

                lua_CSFunction func = (lua_CSFunction)parser.FastGetCSObj(L, ULuaAPI.luaex_upvalueindex(1));

                return func(L);
            } catch(Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in StaticCSFunction:" + e);
            }
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        static int FixCSFunction(ULuaState L)
        {
            try
            {
                UObjectParser parser = UObjectParserPool.Instance.Find(L);

                int idx = ULuaAPI.luaex_tointeger(L, ULuaAPI.luaex_upvalueindex(1));

                lua_CSFunction func = (lua_CSFunction)parser.GetFixCSFunction(idx);

                return func(L);
            } catch (Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in FixCSFunction:" + e);
            }
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        public static int DelegateCall(ULuaState L)
        {
            try
            {
                UObjectParser parser = UObjectParserPool.Instance.Find(L);

                object objDelegate = parser.FastGetCSObj(L, 1);
                if (objDelegate == null || !(objDelegate is Delegate))
                    return ULuaAPI.luaL_error(L, "trying to invoke a value that is not delegate nor callable");

                return parser.mMethodWrapsCache.GetDelegateWrap(objDelegate.GetType())(L);
            } catch (Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in DelegateCall:" + e);
            }
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        public static int LuaGC(ULuaState L)
        {
            try
            {
                int udata = ULuaAPI.luaex_tocsobj_safe(L, 1);
                if (udata != -1)
                {
                    UObjectParser parser = UObjectParserPool.Instance.Find(L);

                    parser.CollectObject(udata);
                }

                return 0;
            } catch (Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in LuaGC:" + e);
            }
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        public static int ToString(ULuaState L)
        {
            try
            {
                UObjectParser parser = UObjectParserPool.Instance.Find(L);

                object obj = parser.FastGetCSObj(L, 1);

                parser.PushAny(L, obj != null ? (obj.ToString() + ": " + obj.GetHashCode()) : "<invalid c# object>");

                return 1;
            } catch (Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in ToString:" + e);
            }
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        public static int DelegateCombine(ULuaState L)
        {
            try
            {
                var parser = UObjectParserPool.Instance.Find(L);

                Type type = parser.FastGetCSObj(L, ULuaAPI.lua_type(L, 1) == ELuaTypes.LUA_TUSERDATA ? 1 : 2).GetType();

                Delegate d1 = parser.GetObject(L, 1, type) as Delegate;
                Delegate d2 = parser.GetObject(L, 2, type) as Delegate;
                if (d1 == null || d2 == null)
                    return ULuaAPI.luaL_error(L, "one parameter must be a delegate, other one must be delegate or function");
                parser.PushAny(L, Delegate.Combine(d1, d2));

                return 1;
            } catch (Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in DelegateCombine:" + e);
            }
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        public static int DelegateRemove(ULuaState L)
        {
            try
            {
                var parser = UObjectParserPool.Instance.Find(L);
                Delegate d1 = parser.FastGetCSObj(L, 1) as Delegate;
                if (d1 == null)
                    return ULuaAPI.luaL_error(L, "#1 parameter must be a delegate");

                Delegate d2 = parser.GetObject(L, 2, d1.GetType()) as Delegate;
                if (d2 == null)
                    return ULuaAPI.luaL_error(L, "#2 parameter must be a delegate or a function ");

                parser.PushAny(L, Delegate.Remove(d1, d2));

                return 1;
            } catch (Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in DelegateRemove:" + e);
            }
        }

        static bool TryPrimitiveArrayGet(Type type, ULuaState L, object obj, int index)
        {
            bool ok = true;
            
            if (type == typeof(int[]))
            {
                int[] array = obj as int[];

                ULuaAPI.luaex_pushinteger(L, array[index]);
            } else if (type == typeof(float[]))
            {
                float[] array = obj as float[];

                ULuaAPI.lua_pushnumber(L, array[index]);
            } else if (type == typeof(double[]))
            {
                double[] array = obj as double[];

                ULuaAPI.lua_pushnumber(L, array[index]);
            } else if (type == typeof(bool[]))
            {
                bool[] array = obj as bool[];

                ULuaAPI.lua_pushboolean(L, array[index]);
            } else if (type == typeof(long[]))
            {
                long[] array = obj as long[];

                ULuaAPI.lua_pushint64(L, array[index]);
            } else if (type == typeof(ulong[]))
            {
                ulong[] array = obj as ulong[];

                ULuaAPI.lua_pushuint64(L, array[index]);
            } else if (type == typeof(sbyte[]))
            {
                sbyte[] array = obj as sbyte[];

                ULuaAPI.luaex_pushinteger(L, array[index]);
            } else if (type == typeof(short[]))
            {
                short[] array = obj as short[];

                ULuaAPI.luaex_pushinteger(L, array[index]);
            } else if (type == typeof(ushort[]))
            {
                ushort[] array = obj as ushort[];

                ULuaAPI.luaex_pushinteger(L, array[index]);
            } else if (type == typeof(char[]))
            {
                char[] array = obj as char[];

                ULuaAPI.luaex_pushinteger(L, array[index]);
            } else if (type == typeof(uint[]))
            {
                uint[] array = obj as uint[];

                ULuaAPI.luaex_pushuint(L, array[index]);
            } else if (type == typeof(IntPtr[]))
            {
                IntPtr[] array = obj as IntPtr[];

                ULuaAPI.lua_pushlightuserdata(L, array[index]);
            } else if (type == typeof(decimal[]))
            {
                decimal[] array = obj as decimal[];

                UObjectParser parser = UObjectParserPool.Instance.Find(L);
                parser.PushDecimal(L, array[index]);
            } else if (type == typeof(string[]))
            {
                string[] array = obj as string[];

                ULuaAPI.lua_pushstring(L, array[index]);
            } else
            {
                ok = false;
            }

            return ok;
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        public static int ArrayIndex(ULuaState L)
        {
            try
            {
                UObjectParser parser = UObjectParserPool.Instance.Find(L);

                System.Array array = (System.Array)parser.FastGetCSObj(L, 1);
                if (array == null)
                    return ULuaAPI.luaL_error(L, "#1 parameter is not a array!");

                int i = ULuaAPI.luaex_tointeger(L, 2);
                if (i >= array.Length)
                    return ULuaAPI.luaL_error(L, "index out of range! i =" + i + ", array.Length=" + array.Length);

                Type type = array.GetType();
                if (TryPrimitiveArrayGet(type, L, array, i))
                    return 1;

                if (GenTryArrayGetPtr != null)
                {
                    try
                    {
                        if (GenTryArrayGetPtr(type, L, parser, array, i))
                            return 1;
                    } catch (Exception e)
                    {
                        return ULuaAPI.luaL_error(L, "c# exception:" + e.Message + ",stack:" + e.StackTrace);
                    }
                }

                object ret = array.GetValue(i);
                parser.PushAny(L, ret);

                return 1;
            } catch (Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in ArrayIndexer:" + e);
            }
        }

        public static bool TryPrimitiveArraySet(Type type, ULuaState L, object obj, int array_idx, int obj_idx)
        {
            bool ok = true;

            ELuaTypes lua_type = ULuaAPI.lua_type(L, obj_idx);

            if (type == typeof(int[]) && lua_type == ELuaTypes.LUA_TNUMBER)
            {
                int[] array = obj as int[];

                array[array_idx] = ULuaAPI.luaex_tointeger(L, obj_idx);
            } else if (type == typeof(float[]) && lua_type == ELuaTypes.LUA_TNUMBER)
            {
                float[] array = obj as float[];

                array[array_idx] = (float)ULuaAPI.lua_tonumber(L, obj_idx);
            } else if (type == typeof(double[]) && lua_type == ELuaTypes.LUA_TNUMBER)
            {
                double[] array = obj as double[];

                array[array_idx] = ULuaAPI.lua_tonumber(L, obj_idx); ;
            } else if (type == typeof(bool[]) && lua_type == ELuaTypes.LUA_TBOOLEAN)
            {
                bool[] array = obj as bool[];

                array[array_idx] = ULuaAPI.lua_toboolean(L, obj_idx);
            } else if (type == typeof(long[]) && ULuaAPI.lua_isint64(L, obj_idx))
            {
                long[] array = obj as long[];

                array[array_idx] = ULuaAPI.lua_toint64(L, obj_idx);
            } else if (type == typeof(ulong[]) && ULuaAPI.lua_isuint64(L, obj_idx))
            {
                ulong[] array = obj as ulong[];

                array[array_idx] = ULuaAPI.lua_touint64(L, obj_idx);
            } else if (type == typeof(sbyte[]) && lua_type == ELuaTypes.LUA_TNUMBER)
            {
                sbyte[] array = obj as sbyte[];

                array[array_idx] = (sbyte)ULuaAPI.luaex_tointeger(L, obj_idx);
            } else if (type == typeof(short[]) && lua_type == ELuaTypes.LUA_TNUMBER)
            {
                short[] array = obj as short[];

                array[array_idx] = (short)ULuaAPI.luaex_tointeger(L, obj_idx);
            } else if (type == typeof(ushort[]) && lua_type == ELuaTypes.LUA_TNUMBER)
            {
                ushort[] array = obj as ushort[];

                array[array_idx] = (ushort)ULuaAPI.luaex_tointeger(L, obj_idx);
            } else if (type == typeof(char[]) && lua_type == ELuaTypes.LUA_TNUMBER)
            {
                char[] array = obj as char[];

                array[array_idx] = (char)ULuaAPI.luaex_tointeger(L, obj_idx);
            } else if (type == typeof(uint[]) && lua_type == ELuaTypes.LUA_TNUMBER)
            {
                uint[] array = obj as uint[];

                array[array_idx] = ULuaAPI.luaex_touint(L, obj_idx);
            } else if (type == typeof(IntPtr[]) && lua_type == ELuaTypes.LUA_TLIGHTUSERDATA)
            {
                IntPtr[] array = obj as IntPtr[];

                array[array_idx] = ULuaAPI.lua_touserdata(L, obj_idx);
            } else if (type == typeof(decimal[]))
            {
                decimal[] array = obj as decimal[];
                if (lua_type == ELuaTypes.LUA_TNUMBER)
                    array[array_idx] = (decimal)ULuaAPI.lua_tonumber(L, obj_idx);

                if (lua_type == ELuaTypes.LUA_TUSERDATA)
                {
                    UObjectParser parser = UObjectParserPool.Instance.Find(L);

                    if (parser.IsDecimal(L, obj_idx))
                        parser.Get(L, obj_idx, out array[array_idx]);
                    else
                        ok = false;
                } else
                {
                    ok = false;
                }
            } else if (type == typeof(string[]) && lua_type == ELuaTypes.LUA_TSTRING)
            {
                string[] array = obj as string[];

                array[array_idx] = ULuaAPI.lua_tostring(L, obj_idx);
            } else
            {
                ok = false;
            }

            return ok;
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        public static int ArrayNewIndex(ULuaState L)
        {
            try
            {
                UObjectParser parser = UObjectParserPool.Instance.Find(L);

                System.Array array = (System.Array)parser.FastGetCSObj(L, 1);
                if (array == null)
                    return ULuaAPI.luaL_error(L, "#1 parameter is not a array!");

                int i = ULuaAPI.luaex_tointeger(L, 2);
                if (i >= array.Length)
                    return ULuaAPI.luaL_error(L, "index out of range! i =" + i + ", array.Length=" + array.Length);

                Type type = array.GetType();
                if (TryPrimitiveArraySet(type, L, array, i, 3))
                    return 0;

                if (GenTryArraySetPtr != null)
                {
                    try
                    {
                        if (GenTryArraySetPtr(type, L, parser, array, i, 3))
                            return 0;
                    } catch (Exception e)
                    {
                        return ULuaAPI.luaL_error(L, "c# exception:" + e.Message + ",stack:" + e.StackTrace);
                    }
                }

                object val = parser.GetObject(L, 3, type.GetElementType());
                array.SetValue(val, i);

                return 0;
            } catch (Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in ArrayNewIndexer:" + e);
            }
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        public static int ArrayLength(ULuaState L)
        {
            try
            {
                UObjectParser parser = UObjectParserPool.Instance.Find(L);

                System.Array array = (System.Array)parser.FastGetCSObj(L, 1);
                if (array == null)
                    return ULuaAPI.luaL_error(L, "#1 parameter is not a array!");
                ULuaAPI.luaex_pushinteger(L, array.Length);

                return 1;
            } catch (System.Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in ArrayLength:" + e);
            }
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        public static int MetaFuncIndex(ULuaState L)
        {
            try
            {
                UObjectParser parser = UObjectParserPool.Instance.Find(L);

                Type type = parser.FastGetCSObj(L, 2) as Type;
                if (type == null)
                    return ULuaAPI.luaL_error(L, "#2 param need a System.Type!");

                parser.TryDelayWrapLoader(L, type);

                ULuaAPI.lua_pushvalue(L, 2);
                ULuaAPI.lua_rawget(L, 1);

                return 1;
            } catch (System.Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in MetaFuncIndex:" + e);
            }
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        internal static int Panic(ULuaState L)
        {
            string reason = String.Format("unprotected error in call to Lua API ({0})", ULuaAPI.lua_tostring(L, -1));

            throw new ULuaException(reason);
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        internal static int Print(ULuaState L)
        {
            try
            {
                int n = ULuaAPI.lua_gettop(L);

                string s = String.Empty;

                if (0 != ULuaAPI.luaex_getglobal(L, "tostring"))
                    return ULuaAPI.luaL_error(L, "can not get tostring in print:");

                for (int i = 1; i <= n; i ++)
                {
                    ULuaAPI.lua_pushvalue(L, -1);  /* function to be called */
                    ULuaAPI.lua_pushvalue(L, i);   /* value to print */
                    if (0 != ULuaAPI.lua_pcall(L, 1, 1, 0))
                        return ULuaAPI.lua_error(L);
                    s += ULuaAPI.lua_tostring(L, -1);

                    if (i != n)
                        s += "\t";

                    ULuaAPI.lua_pop(L, 1);  /* pop result */
                }
                UnityEngine.Debug.Log("LUA: " + s);

                return 0;
            } catch (System.Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in print:" + e);
            }
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        internal static int LoadSocketCore(ULuaState L)
        {
            return ULuaAPI.luaopen_socket_core(L);
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        internal static int LoadBuiltinLib(ULuaState L)
        {
            try
            {
                string builtin_lib = ULuaAPI.lua_tostring(L, 1);

                ULuaEnv self = UObjectParserPool.Instance.Find(L).mEnv;

                lua_CSFunction initer;

                if (self.mBuildinIniter.TryGetValue(builtin_lib, out initer))
                    ULuaAPI.lua_pushstdcallcfunction(L, initer);
                else
                    ULuaAPI.lua_pushstring(L, string.Format("\n\tno such builtin lib '{0}'", builtin_lib));

                return 1;
            } catch (System.Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in LoadBuiltinLib:" + e);
            }
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        internal static int LoadFromResource(ULuaState L)
        {
            try
            {
                string filename = ULuaAPI.lua_tostring(L, 1).Replace(".", "/") + ".lua";

                UnityEngine.TextAsset file = (UnityEngine.TextAsset)UnityEngine.Resources.Load(filename);
                if (file == null)
                {
                    ULuaAPI.lua_pushstring(L, string.Format("\n\tno such resource '{0}'", filename));
                } else
                {
                    if (ULuaAPI.luaexL_loadbuffer(L, file.bytes, file.bytes.Length, "@" + filename) != 0)
                        return ULuaAPI.luaL_error(L, String.Format("error loading module {0} from resource, {1}", ULuaAPI.lua_tostring(L, 1), ULuaAPI.lua_tostring(L, -1)));
                }

                return 1;
            } catch (System.Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in LoadFromResource:" + e);
            }
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        internal static int LoadFromStreamingAssetsPath(ULuaState L)
        {
            try
            {
                string filename = ULuaAPI.lua_tostring(L, 1).Replace(".", "/") + ".lua";

                var bytes = UFileAccessor.ReadBinaryScript(filename);

                if (null != bytes)
                {
                    if (ULuaAPI.luaexL_loadbuffer(L, bytes, bytes.Length, "@" + filename) != 0)
                    {
                        return ULuaAPI.luaL_error(L, String.Format("error loading module {0} from streamingAssetsPath, {1}",
                            ULuaAPI.lua_tostring(L, 1), ULuaAPI.lua_tostring(L, -1)));
                    }
                } else
                {
                    ULuaAPI.lua_pushstring(L, string.Format(
                        "\n\tno such file '{0}' in streamingAssetsPath!", filename));
                }

                return 1;
            } catch (System.Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in LoadFromStreamingAssetsPath:" + e);
            }
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        internal static int LoadFromCustomLoaders(ULuaState L)
        {
            try
            {
                string filename = ULuaAPI.lua_tostring(L, 1);

                ULuaEnv self = UObjectParserPool.Instance.Find(L).mEnv;

                for (int i = 0; i < self.mCustomLoaders.Count; ++ i)
                {
					var loader = self.mCustomLoaders[i];

                    string real_file_path = filename;

                    byte[] bytes = loader(ref real_file_path);
                    if (bytes != null)
                    {
                        if (ULuaAPI.luaexL_loadbuffer(L, bytes, bytes.Length, "@" + real_file_path) != 0)
                        {
                            return ULuaAPI.luaL_error(L, String.Format("error loading module {0} from CustomLoader, {1}",
                                ULuaAPI.lua_tostring(L, 1), ULuaAPI.lua_tostring(L, -1)));
                        }

                        return 1;
                    }
                }
                ULuaAPI.lua_pushstring(L, string.Format("\n\tno such file '{0}' in CustomLoaders!", filename));

                return 1;
            } catch (System.Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in LoadFromCustomLoaders:" + e);
            }
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        public static int LoadAssembly(ULuaState L)
        {
            try
            {
                UObjectParser parser = UObjectParserPool.Instance.Find(L);

                string assemblyName = ULuaAPI.lua_tostring(L, 1);

                Assembly assembly = null;

                try
                {
                    assembly = Assembly.Load(assemblyName);
                } catch (BadImageFormatException)
                {
                    // The assemblyName was invalid.  It is most likely a path.
                }

                if (assembly == null)
                    assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyName));

                if (assembly != null && !parser.mAssemblies.Contains(assembly))
                    parser.mAssemblies.Add(assembly);

                return 0;
            } catch (System.Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in luaex.load_assembly:" + e);
            }
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        public static int ImportType(ULuaState L)
        {
            try
            {
                UObjectParser parser = UObjectParserPool.Instance.Find(L);

                string className = ULuaAPI.lua_tostring(L, 1);
                Type type = parser.FindType(className);
                if (type != null)
                {
                    if (parser.TryDelayWrapLoader(L, type))
                        ULuaAPI.lua_pushboolean(L, true);
                    else
                        return ULuaAPI.luaL_error(L, "can not load type " + type);
                } else
                {
                    ULuaAPI.lua_pushnil(L);
                }

                return 1;
            } catch (System.Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in luaex.import_type:" + e);
            }
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        public static int Cast(ULuaState L)
        {
            try
            {
                UObjectParser parser = UObjectParserPool.Instance.Find(L);

                Type type;
                parser.Get(L, 2, out type);

                if (type == null)
                    return ULuaAPI.luaL_error(L, "#2 param[" + ULuaAPI.lua_tostring(L, 2) + "]is not valid type indicator");

                ULuaAPI.luaL_getmetatable(L, type.FullName);
                if (ULuaAPI.lua_isnil(L, -1))
                    return ULuaAPI.luaL_error(L, "no gen code for " + ULuaAPI.lua_tostring(L, 2));

                ULuaAPI.lua_setmetatable(L, 1);

                return 0;
            } catch (System.Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in luaex.cast:" + e);
            }
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        public static int LuaexAccess(ULuaState L)
        {
            try
            {
                UObjectParser parser = UObjectParserPool.Instance.Find(L);

                Type type = null;
                object obj = null;
                if (ULuaAPI.lua_type(L, 1) == ELuaTypes.LUA_TTABLE)
                {
                    ULuaTable tbl;
                    parser.Get(L, 1, out tbl);

                    type = tbl.Get< Type >("UnderlyingSystemType");
                } else if (ULuaAPI.lua_type(L, 1) == ELuaTypes.LUA_TSTRING)
                {
                    string className = ULuaAPI.lua_tostring(L, 1);

                    type = parser.FindType(className);
                } else if (ULuaAPI.lua_type(L, 1) == ELuaTypes.LUA_TUSERDATA)
                {
                    obj = parser.SafeGetCSObj(L, 1);
                    if (obj == null)
                        return ULuaAPI.luaL_error(L, "luaex.access, #1 parameter must a type/c# object/string");
                    type = obj.GetType();
                }

                if (type == null)
                    return ULuaAPI.luaL_error(L, "luaex.access, can not find c# type");

                string fieldName = ULuaAPI.lua_tostring(L, 2);

                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

                if (ULuaAPI.lua_gettop(L) > 2)
                {
                    var field = type.GetField(fieldName, bindingFlags);
                    if (field != null)
                    {
                        field.SetValue(obj, parser.GetObject(L, 3, field.FieldType));

                        return 0;
                    }

                    var prop = type.GetProperty(fieldName, bindingFlags);
                    if (prop != null)
                    {
                        prop.SetValue(obj, parser.GetObject(L, 3, prop.PropertyType), null);

                        return 0;
                    }
                } else
                {
                    var field = type.GetField(fieldName, bindingFlags);
                    if (field != null)
                    {
                        parser.PushAny(L, field.GetValue(obj));

                        return 1;
                    }

                    var prop = type.GetProperty(fieldName, bindingFlags);
                    if (prop != null)
                    {
                        parser.PushAny(L, prop.GetValue(obj, null));

                        return 1;
                    }
                }

                return ULuaAPI.luaL_error(L, "luaex.access, no field " + fieldName);
            } catch(Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in luaex.access: " + e);
            }
        }

        [UMonoPInvokeCallback(typeof(lua_CSFunction))]
        public static int LuaexPrivateAccessible(ULuaState L)
        {
            try
            {
                UObjectParser parser = UObjectParserPool.Instance.Find(L);

                Type type = null;
                if (ULuaAPI.lua_type(L, 1) == ELuaTypes.LUA_TTABLE)
                {
                    ULuaTable tbl;
                    parser.Get(L, 1, out tbl);

                    type = tbl.Get< Type >("UnderlyingSystemType");
                } else if (ULuaAPI.lua_type(L, 1) == ELuaTypes.LUA_TSTRING)
                {
                    string className = ULuaAPI.lua_tostring(L, 1);

                    type = parser.FindType(className);
                } else
                {
                    return ULuaAPI.luaL_error(L, "luaex.private_accessible, #1 parameter must a type/string");
                }

                if (type == null)
                    return ULuaAPI.luaL_error(L, "luaex.private_accessible, can not find c# type");

                Utils.MakePrivateAccessible(L, type);

                return 0;
            } catch (Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception in luaex.private_accessible: " + e);
            }
        }
    }
}
