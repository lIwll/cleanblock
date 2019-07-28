using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using ULuaState = System.IntPtr;

namespace UEngine.ULua
{
    public delegate bool ObjectCheck(ULuaState L, int idx);

    public delegate object ObjectCast(ULuaState L, int idx, object target);

    public class UObjectChecker
    {
        UObjectParser                   mParser;
        Dictionary< Type, ObjectCheck > mCheckerMap = new Dictionary< Type, ObjectCheck >();

        public UObjectChecker(UObjectParser parser)
        {
            mParser = parser;

            mCheckerMap[typeof(sbyte)]          = NumberCheck;
            mCheckerMap[typeof(byte)]           = NumberCheck;
            mCheckerMap[typeof(short)]          = NumberCheck;
            mCheckerMap[typeof(ushort)]         = NumberCheck;
            mCheckerMap[typeof(int)]            = NumberCheck;
            mCheckerMap[typeof(uint)]           = NumberCheck;
            mCheckerMap[typeof(long)]           = Int64Check;
            mCheckerMap[typeof(ulong)]          = UInt64Check;
            mCheckerMap[typeof(double)]         = NumberCheck;
            mCheckerMap[typeof(char)]           = NumberCheck;
            mCheckerMap[typeof(float)]          = NumberCheck;
            mCheckerMap[typeof(decimal)]        = DecimalCheck;
            mCheckerMap[typeof(bool)]           = BoolCheck;
            mCheckerMap[typeof(string)]         = StrCheck;
            mCheckerMap[typeof(object)]         = ObjCheck;
            mCheckerMap[typeof(byte[])]         = BytesCheck;
            mCheckerMap[typeof(IntPtr)]         = IntptrCheck;

            mCheckerMap[typeof(ULuaTable)]      = LuaTableCheck;
            mCheckerMap[typeof(ULuaFunction)]   = LuaFunctionCheck;
        }

        private static bool ObjCheck(ULuaState L, int idx)
        {
            return true;
        }

        private bool LuaTableCheck(ULuaState L, int idx)
        {
            return ULuaAPI.lua_isnil(L, idx) || ULuaAPI.lua_istable(L, idx) || (ULuaAPI.lua_type(L, idx) == ELuaTypes.LUA_TUSERDATA && mParser.SafeGetCSObj(L, idx) is ULuaTable);
        }

        private bool NumberCheck(ULuaState L, int idx)
        {
            return ULuaAPI.lua_type(L, idx) == ELuaTypes.LUA_TNUMBER;
        }

        private bool DecimalCheck(ULuaState L, int idx)
        {
            return ULuaAPI.lua_type(L, idx) == ELuaTypes.LUA_TNUMBER || mParser.IsDecimal(L, idx);
        }

        private bool StrCheck(ULuaState L, int idx)
        {
            return ULuaAPI.lua_type(L, idx) == ELuaTypes.LUA_TSTRING || ULuaAPI.lua_isnil(L, idx);
        }

        private bool BytesCheck(ULuaState L, int idx)
        {
            return ULuaAPI.lua_type(L, idx) == ELuaTypes.LUA_TSTRING || ULuaAPI.lua_isnil(L, idx) || (ULuaAPI.lua_type(L, idx) == ELuaTypes.LUA_TUSERDATA && mParser.SafeGetCSObj(L, idx) is byte[]);
        }

        private bool BoolCheck(ULuaState L, int idx)
        {
            return ULuaAPI.lua_type(L, idx) == ELuaTypes.LUA_TBOOLEAN;
        }

        private bool Int64Check(ULuaState L, int idx)
        {
            return ULuaAPI.lua_type(L, idx) == ELuaTypes.LUA_TNUMBER || ULuaAPI.lua_isint64(L, idx);
        }

        private bool UInt64Check(ULuaState L, int idx)
        {
            return ULuaAPI.lua_type(L, idx) == ELuaTypes.LUA_TNUMBER || ULuaAPI.lua_isuint64(L, idx);
        }

        private bool LuaFunctionCheck(ULuaState L, int idx)
        {
            return ULuaAPI.lua_isnil(L, idx) || ULuaAPI.lua_isfunction(L, idx) || (ULuaAPI.lua_type(L, idx) == ELuaTypes.LUA_TUSERDATA && mParser.SafeGetCSObj(L, idx) is ULuaFunction);
        }

        private bool IntptrCheck(ULuaState L, int idx)
        {
            return ULuaAPI.lua_type(L, idx) == ELuaTypes.LUA_TLIGHTUSERDATA;
        }

        private ObjectCheck GenChecker(Type type)
        {
            ObjectCheck fixTypeCheck = (ULuaState L, int idx) =>
            {
                if (ULuaAPI.lua_type(L, idx) == ELuaTypes.LUA_TUSERDATA)
                {
                    object obj = mParser.SafeGetCSObj(L, idx);
                    if (obj != null)
                    {
                        return type.IsAssignableFrom(obj.GetType());
                    }
                    else
                    {
                        Type type_of_obj = mParser.GetTypeOf(L, idx);
                        if (type_of_obj != null)
                            return type.IsAssignableFrom(type_of_obj);
                    }
                }

                return false;
            };

            if (!type.IsAbstract && typeof(Delegate).IsAssignableFrom(type))
            {
                return (ULuaState L, int idx) =>
                {
                    return ULuaAPI.lua_isnil(L, idx) || ULuaAPI.lua_isfunction(L, idx) || fixTypeCheck(L, idx);
                };
            } else if (type.IsEnum)
            {
                return fixTypeCheck;
            } else if (type.IsInterface)
            {
                return (ULuaState L, int idx) =>
                {
                    return ULuaAPI.lua_isnil(L, idx) || ULuaAPI.lua_istable(L, idx) || fixTypeCheck(L, idx);
                };
            } else
            {
                if ((type.IsClass && type.GetConstructor(System.Type.EmptyTypes) != null)) //class has default construtor
                {
                    return (ULuaState L, int idx) =>
                    {
                        return ULuaAPI.lua_isnil(L, idx) || ULuaAPI.lua_istable(L, idx) || fixTypeCheck(L, idx);
                    };
                } else if (type.IsValueType)
                {
                    return (ULuaState L, int idx) =>
                    {
                        return ULuaAPI.lua_istable(L, idx) || fixTypeCheck(L, idx);
                    };
                } else if (type.IsArray)
                {
                    return (ULuaState L, int idx) =>
                    {
                        return ULuaAPI.lua_isnil(L, idx) || ULuaAPI.lua_istable(L, idx) || fixTypeCheck(L, idx);
                    };
                } else
                {
                    return (ULuaState L, int idx) =>
                    {
                        return ULuaAPI.lua_isnil(L, idx) || fixTypeCheck(L, idx);
                    };
                }
            }
        }

        public ObjectCheck GetChecker(Type type)
        {
            if (type.IsByRef)
                type = type.GetElementType();

            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                type = underlyingType;
            }

            ObjectCheck oc;
            if (!mCheckerMap.TryGetValue(type, out oc))
            {
                oc = GenChecker(type);

                mCheckerMap.Add(type, oc);
            }

            return oc;
        }
    }

    public class UObjectCaster
    {
        UObjectParser                   mParser;
        Dictionary< Type, ObjectCast >  mCasterMap = new Dictionary< Type, ObjectCast >();

        public UObjectCaster(UObjectParser parser)
        {
            mParser = parser;

            mCasterMap[typeof(char)]            = CharCaster;
            mCasterMap[typeof(sbyte)]           = SByteCaster;
            mCasterMap[typeof(byte)]            = ByteCaster;
            mCasterMap[typeof(short)]           = ShortCaster;
            mCasterMap[typeof(ushort)]          = UShortCaster;
            mCasterMap[typeof(int)]             = IntCaster;
            mCasterMap[typeof(uint)]            = UIntCaster;
            mCasterMap[typeof(long)]            = LongCaster;
            mCasterMap[typeof(ulong)]           = ULongCaster;
            mCasterMap[typeof(double)]          = GetDouble;
            mCasterMap[typeof(float)]           = FloatCaster;
            mCasterMap[typeof(decimal)]         = DecimalCaster;
            mCasterMap[typeof(bool)]            = GetBoolean;
            mCasterMap[typeof(string)]          = GetString;
            mCasterMap[typeof(object)]          = GetObject;
            mCasterMap[typeof(byte[])]          = GetBytes;
            mCasterMap[typeof(IntPtr)]          = GetIntptr;
            mCasterMap[typeof(ULuaTable)]       = GetLuaTable;
            mCasterMap[typeof(ULuaFunction)]    = GetLuaFunction;
        }

        private static object CharCaster(ULuaState L, int idx, object target)
        {
            return (char)ULuaAPI.luaex_tointeger(L, idx);
        }

        private static object SByteCaster(ULuaState L, int idx, object target)
        {
            return (sbyte)ULuaAPI.luaex_tointeger(L, idx);
        }

        private static object ByteCaster(ULuaState L, int idx, object target)
        {
            return (byte)ULuaAPI.luaex_tointeger(L, idx);
        }

        private static object ShortCaster(ULuaState L, int idx, object target)
        {
            return (short)ULuaAPI.luaex_tointeger(L, idx);
        }

        private static object UShortCaster(ULuaState L, int idx, object target)
        {
            return (ushort)ULuaAPI.luaex_tointeger(L, idx);
        }

        private static object IntCaster(ULuaState L, int idx, object target)
        {
            return ULuaAPI.luaex_tointeger(L, idx);
        }

        private static object UIntCaster(ULuaState L, int idx, object target)
        {
            return ULuaAPI.luaex_touint(L, idx);
        }

        private static object LongCaster(ULuaState L, int idx, object target)
        {
            return ULuaAPI.lua_toint64(L, idx);
        }

        private static object ULongCaster(ULuaState L, int idx, object target)
        {
            return ULuaAPI.lua_touint64(L, idx);
        }

        private static object GetDouble(ULuaState L, int idx, object target)
        {
            return ULuaAPI.lua_tonumber(L, idx);
        }

        private static object FloatCaster(ULuaState L, int idx, object target)
        {
            return (float)ULuaAPI.lua_tonumber(L, idx);
        }

        private object DecimalCaster(ULuaState L, int idx, object target)
        {
            decimal ret;
            mParser.Get(L, idx, out ret);

            return ret;
        }

        private static object GetBoolean(ULuaState L, int idx, object target)
        {
            return ULuaAPI.lua_toboolean(L, idx);
        }

        private static object GetString(ULuaState L, int idx, object target)
        {
            return ULuaAPI.lua_tostring(L, idx);
        }

        private object GetBytes(ULuaState L, int idx, object target)
        {
            return ULuaAPI.lua_type(L, idx) == ELuaTypes.LUA_TSTRING ? ULuaAPI.lua_tobytes(L, idx) : mParser.SafeGetCSObj(L, idx) as byte[];
        }

        private object GetIntptr(ULuaState L, int idx, object target)
        {
            return ULuaAPI.lua_touserdata(L, idx);
        }

        private object GetObject(ULuaState L, int idx, object target)
        {
            ELuaTypes type = (ELuaTypes)ULuaAPI.lua_type(L, idx);
            switch (type)
            {
                case ELuaTypes.LUA_TNUMBER:
                    {
                        if (ULuaAPI.lua_isint64(L, idx))
                            return ULuaAPI.lua_toint64(L, idx);
                        else if(ULuaAPI.lua_isinteger(L, idx))
                            return ULuaAPI.luaex_tointeger(L, idx);
                        else
                            return ULuaAPI.lua_tonumber(L, idx);
                    }
                case ELuaTypes.LUA_TSTRING:
                    {
                        return ULuaAPI.lua_tostring(L, idx);
                    }
                case ELuaTypes.LUA_TBOOLEAN:
                    {
                        return ULuaAPI.lua_toboolean(L, idx);
                    }
                case ELuaTypes.LUA_TTABLE:
                    {
                        return GetLuaTable(L, idx, null);
                    }
                case ELuaTypes.LUA_TFUNCTION:
                    {
                        return GetLuaFunction(L, idx, null);
                    }
                case ELuaTypes.LUA_TUSERDATA:
                    {
                        if (ULuaAPI.lua_isint64(L, idx))
                            return ULuaAPI.lua_toint64(L, idx);
                        else if(ULuaAPI.lua_isuint64(L, idx))
                            return ULuaAPI.lua_touint64(L, idx);
                        else
                            return mParser.SafeGetCSObj(L, idx);
                    }
                default:
                    return null;
            }
        }

        private object GetLuaTable(ULuaState L, int idx, object target)
        {
            if (ULuaAPI.lua_type(L, idx) == ELuaTypes.LUA_TUSERDATA)
            {
                object obj = mParser.SafeGetCSObj(L, idx);

                return (obj != null && obj is ULuaTable) ? obj : null;
            }

            if (!ULuaAPI.lua_istable(L, idx))
                return null;

            ULuaAPI.lua_pushvalue(L, idx);

            return new ULuaTable(ULuaAPI.luaL_ref(L), mParser.mEnv);
        }

        private object GetLuaFunction(ULuaState L, int idx, object target)
        {
            if (ULuaAPI.lua_type(L, idx) == ELuaTypes.LUA_TUSERDATA)
            {
                object obj = mParser.SafeGetCSObj(L, idx);

                return (obj != null && obj is ULuaFunction) ? obj : null;
            }

            if (!ULuaAPI.lua_isfunction(L, idx))
                return null;

            ULuaAPI.lua_pushvalue(L, idx);

            return new ULuaFunction(ULuaAPI.luaL_ref(L), mParser.mEnv);
        }

        public void AddCaster(Type type, ObjectCast oc)
        {
            mCasterMap[type] = oc;
        }

        private ObjectCast GenCaster(Type type)
        {
            ObjectCast fixTypeGetter = (ULuaState L, int idx, object target) =>
            {
                if (ULuaAPI.lua_type(L, idx) == ELuaTypes.LUA_TUSERDATA)
                {
                    object obj = mParser.SafeGetCSObj(L, idx);

                    return (obj != null && type.IsAssignableFrom(obj.GetType())) ? obj : null;
                }

                return null;
            }; 

            if (typeof(Delegate).IsAssignableFrom(type))
            {
                return (ULuaState L, int idx, object target) =>
                {
                    object obj = fixTypeGetter(L, idx, target);
                    if (obj != null)
                        return obj;

                    if (!ULuaAPI.lua_isfunction(L, idx))
                        return null;

                    return mParser.CreateDelegateBridge(L, type, idx);
                };
            } else if (type.IsInterface)
            {
                return (ULuaState L, int idx, object target) =>
                {
                    object obj = fixTypeGetter(L, idx, target);
                    if (obj != null)
                        return obj;

                    if (!ULuaAPI.lua_istable(L, idx))
                        return null;

                    return mParser.CreateInterfaceBridge(L, type, idx);
                };
            } else if (type.IsEnum)
            {
                return (ULuaState L, int idx, object target) =>
                {
                    object obj = fixTypeGetter(L, idx, target);
                    if (obj != null)
                        return obj;

                    ELuaTypes lua_type = ULuaAPI.lua_type(L, idx);
                    if (lua_type == ELuaTypes.LUA_TSTRING)
                        return Enum.Parse(type, ULuaAPI.lua_tostring(L, idx));
                    else if (lua_type == ELuaTypes.LUA_TNUMBER)
                        return Enum.ToObject(type, ULuaAPI.luaex_tointeger(L, idx));

                    throw new InvalidCastException("invalid value for enum " + type);
                };
            } else if (type.IsArray)
            {
                return (ULuaState L, int idx, object target) =>
                {
                    object obj = fixTypeGetter(L, idx, target);
                    if (obj != null)
                        return obj;

                    if (!ULuaAPI.lua_istable(L, idx))
                        return null;

                    uint len = ULuaAPI.luaex_objlen(L, idx);
                    int n = ULuaAPI.lua_gettop(L);
                    idx = idx > 0 ? idx : ULuaAPI.lua_gettop(L) + idx + 1;// abs of index
                    Type et = type.GetElementType();
                    ObjectCast elementCaster = GetCaster(et);
                    Array ary = target == null ? Array.CreateInstance(et, (int)len) : target as Array;

                    if (!ULuaAPI.lua_checkstack(L, 1))
                        throw new Exception("stack overflow while cast to Array");

                    for (int i = 0; i < len; ++ i)
                    {
                        ULuaAPI.lua_pushnumber(L, i + 1);
                        ULuaAPI.lua_rawget(L, idx);
                        if (et.IsPrimitive)
                        {
                            if (!ULuaCallbacks.TryPrimitiveArraySet(type, L, ary, i, n + 1))
                                ary.SetValue(elementCaster(L, n + 1, null), i);
                        } else
                        {
                            if (ULuaCallbacks.GenTryArraySetPtr == null || !ULuaCallbacks.GenTryArraySetPtr(type, L, mParser, ary, i, n + 1))
                                ary.SetValue(elementCaster(L, n + 1, null), i);
                        }
                        ULuaAPI.lua_pop(L, 1);
                    }

                    return ary;
                };
            } else if (typeof(IList).IsAssignableFrom(type) && type.IsGenericType)
            {
                Type elementType = type.GetGenericArguments()[0];
                ObjectCast elementCaster = GetCaster(elementType);

                return (ULuaState L, int idx, object target) =>
                {
                    object obj = fixTypeGetter(L, idx, target);
                    if (obj != null)
                        return obj;

                    if (!ULuaAPI.lua_istable(L, idx))
                        return null;

                    obj = target == null ? Activator.CreateInstance(type) : target;
                    int n = ULuaAPI.lua_gettop(L);
                    idx = idx > 0 ? idx : ULuaAPI.lua_gettop(L) + idx + 1;// abs of index
                    IList list = obj as IList;

                    uint len = ULuaAPI.luaex_objlen(L, idx);
                    if (!ULuaAPI.lua_checkstack(L, 1))
                        throw new Exception("stack overflow while cast to IList");

                    for (int i = 0; i < len; ++ i)
                    {
                        ULuaAPI.lua_pushnumber(L, i + 1);
                        ULuaAPI.lua_rawget(L, idx);
                        if (i < list.Count && target != null)
                        {
                            if (mParser.Assignable(L, n + 1, elementType))
                                list[i] = elementCaster(L, n + 1, list[i]); ;
                        } else
                        {
                            if (mParser.Assignable(L, n + 1, elementType))
                                list.Add(elementCaster(L, n + 1, null));
                        }
                        ULuaAPI.lua_pop(L, 1);
                    }

                    return obj;
                };
            } else if (typeof(IDictionary).IsAssignableFrom(type) && type.IsGenericType)
            {
                Type keyType = type.GetGenericArguments()[0];
                ObjectCast keyCaster = GetCaster(keyType);
                Type valueType = type.GetGenericArguments()[1];
                ObjectCast valueCaster = GetCaster(valueType);

                return (ULuaState L, int idx, object target) =>
                {
                    object obj = fixTypeGetter(L, idx, target);
                    if (obj != null)
                        return obj;

                    if (!ULuaAPI.lua_istable(L, idx))
                        return null;

                    IDictionary dic = (target == null ? Activator.CreateInstance(type) : target) as IDictionary;
                    int n = ULuaAPI.lua_gettop(L);
                    idx = idx > 0 ? idx : ULuaAPI.lua_gettop(L) + idx + 1;// abs of index

                    ULuaAPI.lua_pushnil(L);
                    if (!ULuaAPI.lua_checkstack(L, 1))
                        throw new Exception("stack overflow while cast to IDictionary");

                    while (ULuaAPI.lua_next(L, idx) != 0)
                    {
                        if (mParser.Assignable(L, n + 1, keyType) && mParser.Assignable(L, n + 2, valueType))
                        {
                            object k = keyCaster(L, n + 1, null);

                            dic[k] = valueCaster(L, n + 2, !dic.Contains(k) ? null : dic[k]);
                        }
                        ULuaAPI.lua_pop(L, 1); // removes value, keeps key for next iteration
                    }

                    return dic;
                };
            } else if ((type.IsClass && type.GetConstructor(System.Type.EmptyTypes) != null) || (type.IsValueType && !type.IsEnum)) //class has default construtor
            {
                return (ULuaState L, int idx, object target) =>
                {
                    object obj = fixTypeGetter(L, idx, target);
                    if (obj != null)
                        return obj;

                    if (!ULuaAPI.lua_istable(L, idx))
                        return null;

                    obj = target == null ? Activator.CreateInstance(type) : target;

                    int n = ULuaAPI.lua_gettop(L);
                    idx = idx > 0 ? idx : ULuaAPI.lua_gettop(L) + idx + 1;// abs of index
                    if (!ULuaAPI.lua_checkstack(L, 1))
                        throw new Exception("stack overflow while cast to " + type);

					FieldInfo[] fields = type.GetFields();

                    for (int i = 0; i < fields.Length; ++ i)
                    {
						FieldInfo field = fields[i];

                        ULuaAPI.luaex_pushasciistring(L, field.Name);
                        ULuaAPI.lua_rawget(L, idx);
                        if (!ULuaAPI.lua_isnil(L, -1))
                        {
                            try
                            {
                                field.SetValue(obj, GetCaster(field.FieldType)(L, n + 1, target == null || field.FieldType.IsPrimitive || field.FieldType == typeof(string) ? null : field.GetValue(obj)));
                            } catch (Exception e)
                            {
                                throw new Exception("exception in tran " + field.Name + ", msg=" + e.Message);
                            }
                        }
                        ULuaAPI.lua_pop(L, 1);
                    }

                    return obj;
                };
            } else
            {
                return fixTypeGetter;
            }
        }

        public ObjectCast GetCaster(Type type)
        {
            if (type.IsByRef)
                type = type.GetElementType();

            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
                type = underlyingType; 

            ObjectCast oc;
            if (!mCasterMap.TryGetValue(type, out oc))
            {
                oc = GenCaster(type);

                mCasterMap.Add(type, oc);
            }

            return oc;
        }
    }
}
