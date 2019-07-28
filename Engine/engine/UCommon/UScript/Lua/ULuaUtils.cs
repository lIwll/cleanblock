using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.Runtime.CompilerServices;

using ULuaState = System.IntPtr;

namespace UEngine.ULua
{
    public static partial class Utils
    {
        static Dictionary< string, string > kULuaSupportOP = new Dictionary< string, string >()
        {
            { "op_Addition",        "__add" },
            { "op_Subtraction",     "__sub" },
            { "op_Multiply",        "__mul" },
            { "op_Division",        "__div" },
            { "op_Equality",        "__eq"  },
            { "op_UnaryNegation",   "__unm" },
            { "op_LessThan",        "__lt"  },
            { "op_LessThanOrEqual", "__le"  },
            { "op_Modulus",         "__mod" }
        };

        public static readonly int kIDX_SETTER      = -1;
        public static readonly int kIDX_GETTER      = -2;
        public static readonly int kIDX_METHOD      = -3;
        public static readonly int kIDX_OBJ_META    = -4;

        public static readonly int kCLS_SETTER_IDX  = -1;
        public static readonly int kCLS_GETTER_IDX  = -2;
        public static readonly int kCLS_META_IDX    = -3;
        public static readonly int kCLS_IDX         = -4;

        public static string kLuaIndexFieldName             = "ULuaIndex";
        public static string kLuaNewIndexFieldName          = "ULuaNewIndex";
        public static string kLuaClassIndexFieldName        = "ULuaClassIndex";
        public static string kLuaClassNewIndexFieldName     = "ULuaClassNewIndex";

        static Dictionary< Type, IEnumerable< MethodInfo > >    mExtensionMethodMap = null;

        public static bool LoadField(ULuaState L, int idx, string field_name)
        {
            ULuaAPI.luaex_pushasciistring(L, field_name);
            ULuaAPI.lua_rawget(L, idx);

            return !ULuaAPI.lua_isnil(L, -1);
        }

        public static ULuaState GetMainState(ULuaState L)
        {
            ULuaState ret = default(ULuaState);

            ULuaAPI.luaex_pushasciistring(L, "luaex_main_thread");
            ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);
            if (ULuaAPI.lua_isthread(L, -1))
                ret = ULuaAPI.lua_tothread(L, -1);
            ULuaAPI.lua_pop(L, 1);

            return ret;
        }

        public static IEnumerable< Type > GetAllTypes(bool exclude_generic_definition = true)
        {
            return from assembly in AppDomain.CurrentDomain.GetAssemblies()
                //where !(assembly.ManifestModule is System.Reflection.Emit.ModuleBuilder)
                from type in assembly.GetTypes() where exclude_generic_definition ? !type.IsGenericTypeDefinition : true select type;
        }

        static lua_CSFunction GenFieldGetter(Type type, FieldInfo field)
        {
            if (field.IsStatic)
            {
                return (ULuaState L) =>
                {
                    UObjectParser parser = UObjectParserPool.Instance.Find(L);

                    parser.PushAny(L, field.GetValue(null));

                    return 1;
                };
            } else
            {
                return (ULuaState L) =>
                {
                    UObjectParser parser = UObjectParserPool.Instance.Find(L);

                    object obj = parser.FastGetCSObj(L, 1);
                    if (obj == null || !type.IsInstanceOfType(obj))
                        return ULuaAPI.luaL_error(L, "Expected type " + type + ", but got " + (obj == null ? "null" : obj.GetType().ToString()) + ", while get field " + field);
                    parser.PushAny(L, field.GetValue(obj));

                    return 1;
                };
            }
        }

        static lua_CSFunction GenFieldSetter(Type type, FieldInfo field)
        {
            if (field.IsStatic)
            {
                return (ULuaState L) =>
                {
                    UObjectParser parser = UObjectParserPool.Instance.Find(L);

                    object val = parser.GetObject(L, 1, field.FieldType);
                    if (val == null && field.FieldType.IsValueType)
                        return ULuaAPI.luaL_error(L, type.Name + "." + field.Name + " Expected type " + field.FieldType);
                    field.SetValue(null, val);

                    return 0;
                };
            } else
            {
                return (ULuaState L) =>
                {
                    UObjectParser parser = UObjectParserPool.Instance.Find(L);

                    object obj = parser.FastGetCSObj(L, 1);
                    if (obj == null || !type.IsInstanceOfType(obj))
                        return ULuaAPI.luaL_error(L, "Expected type " + type + ", but got " + (obj == null ? "null" : obj.GetType().ToString()) + ", while set field " + field);

                    object val = parser.GetObject(L, 2, field.FieldType);
                    if (val == null && field.FieldType.IsValueType)
                        return ULuaAPI.luaL_error(L, type.Name + "." + field.Name + " Expected type " + field.FieldType);
                    field.SetValue(obj, val);

                    return 0;
                };
            }
        }

        static lua_CSFunction GenPropGetter(Type type, PropertyInfo prop, bool is_static)
        {
            if (is_static)
            {
                return (ULuaState L) =>
                {
                    UObjectParser parser = UObjectParserPool.Instance.Find(L);
                    try
                    {
                        parser.PushAny(L, prop.GetValue(null, null));
                    } catch(Exception e)
                    {
                        return ULuaAPI.luaL_error(L, "try to get " + type + "." + prop.Name + " throw a exception:" + e + ",stack:" + e.StackTrace);
                    }

                    return 1;
                };
            } else
            {
                return (ULuaState L) =>
                {
                    UObjectParser parser = UObjectParserPool.Instance.Find(L);

                    object obj = parser.FastGetCSObj(L, 1);
                    if (obj == null || !type.IsInstanceOfType(obj))
                        return ULuaAPI.luaL_error(L, "Expected type " + type + ", but got " + (obj == null ? "null" : obj.GetType().ToString()) + ", while get prop " + prop);

                    try
                    {
                        parser.PushAny(L, prop.GetValue(obj, null));
                    } catch (Exception e)
                    {
                        return ULuaAPI.luaL_error(L, "try to get " + type + "." + prop.Name + " throw a exception:" + e + ",stack:" + e.StackTrace);
                    }

                    return 1;
                };
            }
        }

        static lua_CSFunction GenPropSetter(Type type, PropertyInfo prop, bool is_static)
        {
            if (is_static)
            {
                return (ULuaState L) =>
                {
                    UObjectParser parser = UObjectParserPool.Instance.Find(L);

                    object val = parser.GetObject(L, 1, prop.PropertyType);
                    if (val == null && prop.PropertyType.IsValueType)
                        return ULuaAPI.luaL_error(L, type.Name + "." + prop.Name + " Expected type " + prop.PropertyType);

                    try
                    { 
                        prop.SetValue(null, val, null);
                    } catch (Exception e)
                    {
                        return ULuaAPI.luaL_error(L, "try to set " + type + "." + prop.Name + " throw a exception:" + e + ",stack:" + e.StackTrace);
                    }

                    return 0;
                };
            } else
            {
                return (ULuaState L) =>
                {
                    UObjectParser parser = UObjectParserPool.Instance.Find(L);

                    object obj = parser.FastGetCSObj(L, 1);
                    if (obj == null || !type.IsInstanceOfType(obj))
                        return ULuaAPI.luaL_error(L, "Expected type " + type + ", but got " + (obj == null ? "null" : obj.GetType().ToString()) + ", while set prop " + prop);

                    object val = parser.GetObject(L, 2, prop.PropertyType);
                    if (val == null && prop.PropertyType.IsValueType)
                        return ULuaAPI.luaL_error(L, type.Name + "." + prop.Name + " Expected type " + prop.PropertyType);

                    try
                    {
                        prop.SetValue(obj, val, null);
                    } catch (Exception e)
                    {
                        return ULuaAPI.luaL_error(L, "try to set " + type + "." + prop.Name + " throw a exception:" + e + ",stack:" + e.StackTrace);
                    }

                    return 0;
                };
            }
        }

        static lua_CSFunction GenItemGetter(Type type, PropertyInfo[] props)
        {
            props = props.Where(prop => !prop.GetIndexParameters()[0].ParameterType.IsAssignableFrom(typeof(string))).ToArray();
            if (props.Length == 0)
                return null;

            Type[] params_type = new Type[props.Length];
            for (int i = 0; i < props.Length; i ++)
                params_type[i] = props[i].GetIndexParameters()[0].ParameterType;

            object[] arg = new object[1] { null };
            return (ULuaState L) =>
            {
                UObjectParser parser = UObjectParserPool.Instance.Find(L);

                object obj = parser.FastGetCSObj(L, 1);
                if (obj == null || !type.IsInstanceOfType(obj))
                    return ULuaAPI.luaL_error(L, "Expected type " + type + ", but got " + (obj == null ? "null" : obj.GetType().ToString()) + ", while get prop " + props[0].Name);

                for (int i = 0; i < props.Length; i ++)
                {
                    if (!parser.Assignable(L, 2, params_type[i]))
                    {
                        continue;
                    } else
                    {
                        PropertyInfo prop = props[i];
                        try
                        {
                            object index = parser.GetObject(L, 2, params_type[i]);

                            arg[0] = index;
                            object ret = prop.GetValue(obj, arg);

                            ULuaAPI.lua_pushboolean(L, true);

                            parser.PushAny(L, ret);

                            return 2;
                        } catch (Exception e)
                        {
                            return ULuaAPI.luaL_error(L, "try to get " + type + "." + prop.Name + " throw a exception:" + e + ",stack:" + e.StackTrace);
                        }
                    }
                }

                ULuaAPI.lua_pushboolean(L, false);

                return 1;
            };
        }

        static lua_CSFunction GenItemSetter(Type type, PropertyInfo[] props)
        {
            Type[] params_type = new Type[props.Length];
            for (int i = 0; i < props.Length; i ++)
                params_type[i] = props[i].GetIndexParameters()[0].ParameterType;

            object[] arg = new object[1] { null };
            return (ULuaState L) =>
            {
                UObjectParser parser = UObjectParserPool.Instance.Find(L);

                object obj = parser.FastGetCSObj(L, 1);
                if (obj == null || !type.IsInstanceOfType(obj))
                    return ULuaAPI.luaL_error(L, "Expected type " + type + ", but got " + (obj == null ? "null" : obj.GetType().ToString()) + ", while set prop " + props[0].Name);

                for (int i = 0; i < props.Length; i ++)
                {
                    if (!parser.Assignable(L, 2, params_type[i]))
                    {
                        continue;
                    } else
                    {
                        PropertyInfo prop = props[i];
                        try
                        {
                            arg[0] = parser.GetObject(L, 2, params_type[i]);

                            object val = parser.GetObject(L, 3, prop.PropertyType);
                            if (val == null)
                                return ULuaAPI.luaL_error(L, type.Name + "." + prop.Name + " Expected type " + prop.PropertyType);
                            prop.SetValue(obj, val, arg);

                            ULuaAPI.lua_pushboolean(L, true);
                            
                            return 1;
                        } catch (Exception e)
                        {
                            return ULuaAPI.luaL_error(L, "try to set " + type + "." + prop.Name + " throw a exception:" + e + ",stack:" + e.StackTrace);
                        }
                    }
                }

                ULuaAPI.lua_pushboolean(L, false);

                return 1;
            };
        }

        static lua_CSFunction GenEnumCastFrom(Type type)
        {
            return (ULuaState L) =>
            {
                UObjectParser parser = UObjectParserPool.Instance.Find(L);

                return parser.TranslateToEnumToTop(L, type, 1);
            };
        }

        static IEnumerable< MethodInfo > GetExtensionMethodsOf(Type type_to_be_extend)
        {
            if (mExtensionMethodMap == null)
            {
                List< Type > type_def_extention_method = new List< Type >();

                IEnumerator< Type > enumerator = GetAllTypes().GetEnumerator();
                
                while (enumerator.MoveNext())
                {
                    Type type = enumerator.Current;
                    if (type.IsDefined(typeof(ExtensionAttribute), false)  && (type.IsDefined(typeof(UReflectionUseAttribute), false)))
                    {
                        type_def_extention_method.Add(type);
                    } else if(!type.IsInterface && typeof(IReflectionConfig).IsAssignableFrom(type))
                    {
                        var tmp = (Activator.CreateInstance(type) as IReflectionConfig).ReflectionUse;
                        if (tmp != null)
                        {
                            type_def_extention_method.AddRange(tmp.Where(t => t.IsDefined(typeof(ExtensionAttribute), false)));
                        }
                    }

                    if (!type.IsAbstract || !type.IsSealed)
                        continue;

                    var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    for (int i = 0; i < fields.Length; i ++)
                    {
                        var field = fields[i];
                        if ((field.IsDefined(typeof(UReflectionUseAttribute), false)) && (typeof(IEnumerable< Type >)).IsAssignableFrom(field.FieldType))
                        {
                            type_def_extention_method.AddRange((field.GetValue(null) as IEnumerable<Type>).Where(t => t.IsDefined(typeof(ExtensionAttribute), false)));
                        }
                    }

                    var props = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    for (int i = 0; i < props.Length; i ++)
                    {
                        var prop = props[i];
                        if ((prop.IsDefined(typeof(UReflectionUseAttribute), false)) && (typeof(IEnumerable<Type>)).IsAssignableFrom(prop.PropertyType))
                        {
                            type_def_extention_method.AddRange((prop.GetValue(null, null) as IEnumerable< Type >).Where(t => t.IsDefined(typeof(ExtensionAttribute), false)));
                        }
                    }
                }
                enumerator.Dispose();

                mExtensionMethodMap = (from type in type_def_extention_method
                                        from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public) where IsSupportedExtensionMethod(method)
                                        group method by GetExtendedType(method)).ToDictionary(g => g.Key, g => g as IEnumerable< MethodInfo >);
            }

            IEnumerable< MethodInfo > ret = null;
            mExtensionMethodMap.TryGetValue(type_to_be_extend, out ret);

            return ret;
        }

        struct MethodKey
        {
            public string
                Name;
            public bool
                IsStatic;
        }

        static void MakeReflectionWrap(ULuaState L, Type type, int cls_field, int cls_getter, int cls_setter,
            int obj_field, int obj_getter, int obj_setter, int obj_meta, out lua_CSFunction item_getter, out lua_CSFunction item_setter, bool private_access = false)
        {
            UObjectParser parser = UObjectParserPool.Instance.Find(L);

            BindingFlags flag = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | (private_access ? BindingFlags.NonPublic : BindingFlags.Public);
            FieldInfo[] fields = type.GetFields(flag);
            EventInfo[] all_events = type.GetEvents(flag | BindingFlags.Public | BindingFlags.NonPublic);

            for (int i = 0; i < fields.Length; ++ i)
            {
                FieldInfo field = fields[i];

                string fieldName = field.Name;
                if (private_access)
                {
                    if (field.IsStatic && (field.Name.StartsWith("__Hitfix") || field.Name.StartsWith("_c__Hitfix")) && typeof(Delegate).IsAssignableFrom(field.FieldType))
                        continue;

                    if (all_events.Any(e => e.Name == fieldName))
                        fieldName = "&" + fieldName;
                }

                if (field.IsStatic && (field.IsInitOnly || field.IsLiteral))
                {
                    ULuaAPI.luaex_pushasciistring(L, fieldName);
                    parser.PushAny(L, field.GetValue(null));
                    ULuaAPI.lua_rawset(L, cls_field);
                } else
                {
                    ULuaAPI.luaex_pushasciistring(L, fieldName);
                    parser.PushFixCSFunction(L, GenFieldGetter(type, field));
                    ULuaAPI.lua_rawset(L, field.IsStatic ? cls_getter : obj_getter);

                    ULuaAPI.luaex_pushasciistring(L, fieldName);
                    parser.PushFixCSFunction(L, GenFieldSetter(type, field));
                    ULuaAPI.lua_rawset(L, field.IsStatic ? cls_setter : obj_setter);
                }
            }

            EventInfo[] events = type.GetEvents(flag);
            for (int i = 0; i < events.Length; ++ i)
            {
                EventInfo eventInfo = events[i];

                ULuaAPI.luaex_pushasciistring(L, eventInfo.Name);
                parser.PushFixCSFunction(L, parser.mMethodWrapsCache.GetEventWrap(type, eventInfo.Name));
                bool is_static = (eventInfo.GetAddMethod() != null) ? eventInfo.GetAddMethod().IsStatic : eventInfo.GetRemoveMethod().IsStatic;
                ULuaAPI.lua_rawset(L, is_static ? cls_field : obj_field);
            }

            Dictionary< string, PropertyInfo > prop_map = new Dictionary< string, PropertyInfo >();
            List< PropertyInfo > items = new List< PropertyInfo >();
            PropertyInfo[] props = type.GetProperties(flag);
            for (int i = 0; i < props.Length; ++ i)
            {
                PropertyInfo prop = props[i];

                if (prop.Name == "Item" && prop.GetIndexParameters().Length > 0)
                    items.Add(prop);
                else
                    prop_map.Add(prop.Name, prop);
            }

            var item_array = items.ToArray();
            item_getter = item_array.Length > 0 ? GenItemGetter(type, item_array) : null;
            item_setter = item_array.Length > 0 ? GenItemSetter(type, item_array) : null;

            MethodInfo[] methods = type.GetMethods(flag);
            Dictionary< MethodKey, List< MemberInfo > > pending_methods = new Dictionary< MethodKey, List< MemberInfo > >();
            for (int i = 0; i < methods.Length; ++ i)
            {
                MethodInfo method = methods[i];
                string method_name = method.Name;

                MethodKey method_key = new MethodKey { Name = method_name, IsStatic = method.IsStatic };
                List< MemberInfo > overloads;
                if (pending_methods.TryGetValue(method_key, out overloads))
                {
                    overloads.Add(method);

                    continue;
                }

                PropertyInfo prop = null;
                if (method_name.StartsWith("add_") || method_name.StartsWith("remove_") || method_name == "get_Item" || method_name == "set_Item")
                    continue;

                if (method_name.StartsWith("op_")) // operator
                {
                    if (kULuaSupportOP.ContainsKey(method_name))
                    {
                        if (overloads == null)
                        {
                            overloads = new List< MemberInfo >();

                            pending_methods.Add(method_key, overloads);
                        }
                        overloads.Add(method);
                    }
                    continue;
                } else if (method_name.StartsWith("get_") && prop_map.TryGetValue(method.Name.Substring(4), out prop)) // getter of property
                {
                    ULuaAPI.luaex_pushasciistring(L, prop.Name);
                    parser.PushFixCSFunction(L, GenPropGetter(type, prop, method.IsStatic));
                    ULuaAPI.lua_rawset(L, method.IsStatic ? cls_getter : obj_getter);
                } else if (method_name.StartsWith("set_") && prop_map.TryGetValue(method.Name.Substring(4), out prop)) // setter of property
                {
                    ULuaAPI.luaex_pushasciistring(L, prop.Name);
                    parser.PushFixCSFunction(L, GenPropSetter(type, prop, method.IsStatic));
                    ULuaAPI.lua_rawset(L, method.IsStatic ? cls_setter : obj_setter);
                } else if (method_name == ".ctor" && method.IsConstructor) // destruct
                {
                    continue;
                } else
                {
                    if (overloads == null)
                    {
                        overloads = new List< MemberInfo >();

                        pending_methods.Add(method_key, overloads);
                    }
                    overloads.Add(method);
                }
            }

            IEnumerable< MethodInfo > extend_methods = GetExtensionMethodsOf(type);
            if (extend_methods != null)
            {
                foreach (var extend_method in extend_methods)
                {
                    MethodKey method_key = new MethodKey { Name = extend_method.Name, IsStatic = false };
                    List< MemberInfo > overloads;
                    if (pending_methods.TryGetValue(method_key, out overloads))
                    {
                        overloads.Add(extend_method);

                        continue;
                    } else
                    {
                        overloads = new List< MemberInfo >() { extend_method };

                        pending_methods.Add(method_key, overloads);
                    }
                }
            }

            foreach (var kv in pending_methods)
            {
                if (kv.Key.Name.StartsWith("op_")) // operator
                {
                    ULuaAPI.luaex_pushasciistring(L, kULuaSupportOP[kv.Key.Name]);
                    parser.PushFixCSFunction(L, new lua_CSFunction(parser.mMethodWrapsCache.GenMethodWrap(type, kv.Key.Name, kv.Value.ToArray()).Call));
                    ULuaAPI.lua_rawset(L, obj_meta);
                } else
                {
                    ULuaAPI.luaex_pushasciistring(L, kv.Key.Name);
                    parser.PushFixCSFunction(L, new lua_CSFunction(parser.mMethodWrapsCache.GenMethodWrap(type, kv.Key.Name, kv.Value.ToArray()).Call));
                    ULuaAPI.lua_rawset(L, kv.Key.IsStatic ? cls_field : obj_field);
                }
            }
        }

        public static void LoadUpvalue(ULuaState L, Type type, string metafunc, int num)
        {
            UObjectParser parser = UObjectParserPool.Instance.Find(L);

            ULuaAPI.luaex_pushasciistring(L, metafunc);
            ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);
            parser.Push(L, type);
            ULuaAPI.lua_rawget(L, -2);

            for (int i = 1; i <= num; i ++)
            {
                ULuaAPI.lua_getupvalue(L, -i, i);
                if (ULuaAPI.lua_isnil(L, -1))
                {
                    ULuaAPI.lua_pop(L, 1);
                    ULuaAPI.lua_newtable(L);
                    ULuaAPI.lua_pushvalue(L, -1);
                    ULuaAPI.lua_setupvalue(L, -i - 2, i);
                }
            }

            for (int i = 0; i < num; i ++)
                ULuaAPI.lua_remove(L, -num - 1);
        }

        public static void MakePrivateAccessible(ULuaState L, Type type)
        {
            int oldTop = ULuaAPI.lua_gettop(L);

            ULuaAPI.luaL_getmetatable(L, type.FullName);
            if (ULuaAPI.lua_isnil(L, -1))
            {
                ULuaAPI.lua_settop(L, oldTop);

                throw new Exception("can not find the metatable for " + type);
            }
            int obj_meta = ULuaAPI.lua_gettop(L);

            LoadCSTable(L, type);
            if (ULuaAPI.lua_isnil(L, -1))
            {
                ULuaAPI.lua_settop(L, oldTop);

                throw new Exception("can not find the class for " + type);
            }
            int cls_field = ULuaAPI.lua_gettop(L);

            LoadUpvalue(L, type, kLuaIndexFieldName, 2);
            int obj_getter = ULuaAPI.lua_gettop(L);
            int obj_field = obj_getter - 1;

            LoadUpvalue(L, type, kLuaNewIndexFieldName, 1);
            int obj_setter = ULuaAPI.lua_gettop(L);

            LoadUpvalue(L, type, kLuaClassIndexFieldName, 1);
            int cls_getter = ULuaAPI.lua_gettop(L);

            LoadUpvalue(L, type, kLuaClassNewIndexFieldName, 1);
            int cls_setter = ULuaAPI.lua_gettop(L);

            lua_CSFunction item_getter;
            lua_CSFunction item_setter;
            MakeReflectionWrap(L, type, cls_field, cls_getter, cls_setter, obj_field, obj_getter, obj_setter, obj_meta, out item_getter, out item_setter, true);

            ULuaAPI.lua_settop(L, oldTop);
        }

        public static void ReflectionWrap(ULuaState L, Type type)
        {
            int top_enter = ULuaAPI.lua_gettop(L);

            UObjectParser parser = UObjectParserPool.Instance.Find(L);

            ULuaAPI.luaL_getmetatable(L, type.FullName);
            if (ULuaAPI.lua_isnil(L, -1))
            {
                ULuaAPI.lua_pop(L, 1);
                ULuaAPI.luaL_newmetatable(L, type.FullName);
            }
            ULuaAPI.lua_pushlightuserdata(L, ULuaAPI.luaex_tag());
            ULuaAPI.lua_pushnumber(L, 1);
            ULuaAPI.lua_rawset(L, -3);

            int obj_meta = ULuaAPI.lua_gettop(L);

            ULuaAPI.lua_newtable(L);
            int cls_meta = ULuaAPI.lua_gettop(L);

            ULuaAPI.lua_newtable(L);
            int obj_field = ULuaAPI.lua_gettop(L);

            ULuaAPI.lua_newtable(L);
            int obj_getter = ULuaAPI.lua_gettop(L);

            ULuaAPI.lua_newtable(L);
            int obj_setter = ULuaAPI.lua_gettop(L);

            ULuaAPI.lua_newtable(L);
            int cls_field = ULuaAPI.lua_gettop(L);

            ULuaAPI.lua_newtable(L);
            int cls_getter = ULuaAPI.lua_gettop(L);

            ULuaAPI.lua_newtable(L);
            int cls_setter = ULuaAPI.lua_gettop(L);

            lua_CSFunction item_getter;
            lua_CSFunction item_setter;
            MakeReflectionWrap(L, type, cls_field, cls_getter, cls_setter, obj_field, obj_getter, obj_setter, obj_meta, out item_getter, out item_setter);

            // init obj metatable
            ULuaAPI.luaex_pushasciistring(L, "__gc");
            ULuaAPI.lua_pushstdcallcfunction(L, parser.mMetaFunctions.GCMeta);
            ULuaAPI.lua_rawset(L, obj_meta);

            ULuaAPI.luaex_pushasciistring(L, "__tostring");
            ULuaAPI.lua_pushstdcallcfunction(L, parser.mMetaFunctions.ToStringMeta);
            ULuaAPI.lua_rawset(L, obj_meta);

            ULuaAPI.luaex_pushasciistring(L, "__index");
            ULuaAPI.lua_pushvalue(L, obj_field);
            ULuaAPI.lua_pushvalue(L, obj_getter);
            parser.PushFixCSFunction(L, item_getter);
            parser.PushAny(L, type.BaseType);
            ULuaAPI.luaex_pushasciistring(L, kLuaIndexFieldName);
            ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);
            ULuaAPI.lua_pushnil(L);
            ULuaAPI.luaex_gen_obj_index(L);
            //store in lua index function tables
            ULuaAPI.luaex_pushasciistring(L, kLuaIndexFieldName);
            ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);
            parser.Push(L, type);
            ULuaAPI.lua_pushvalue(L, -3);
            ULuaAPI.lua_rawset(L, -3);
            ULuaAPI.lua_pop(L, 1);
            ULuaAPI.lua_rawset(L, obj_meta); // set __index

            ULuaAPI.luaex_pushasciistring(L, "__newindex");
            ULuaAPI.lua_pushvalue(L, obj_setter);
            parser.PushFixCSFunction(L, item_setter);
            parser.Push(L, type.BaseType);
            ULuaAPI.luaex_pushasciistring(L, kLuaNewIndexFieldName);
            ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);
            ULuaAPI.lua_pushnil(L);
            ULuaAPI.luaex_gen_obj_newindex(L);
            //store in lua new index function tables
            ULuaAPI.luaex_pushasciistring(L, kLuaNewIndexFieldName);
            ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);
            parser.Push(L, type);
            ULuaAPI.lua_pushvalue(L, -3);
            ULuaAPI.lua_rawset(L, -3);
            ULuaAPI.lua_pop(L, 1);
            ULuaAPI.lua_rawset(L, obj_meta); // set __newindex
            //finish init obj metatable

            ULuaAPI.luaex_pushasciistring(L, "UnderlyingSystemType");
            parser.PushAny(L, type);
            ULuaAPI.lua_rawset(L, cls_field);

            if (type != null && type.IsEnum)
            {
                ULuaAPI.luaex_pushasciistring(L, "__CastFrom");
                parser.PushFixCSFunction(L, GenEnumCastFrom(type));
                ULuaAPI.lua_rawset(L, cls_field);
            }

            //set cls_field to namespace
            SetCSTable(L, type, cls_field);
            //finish set cls_field to namespace

            //init class meta
            ULuaAPI.luaex_pushasciistring(L, "__index");
            ULuaAPI.lua_pushvalue(L, cls_getter);
            ULuaAPI.lua_pushvalue(L, cls_field);
            parser.Push(L, type.BaseType);
            ULuaAPI.luaex_pushasciistring(L, kLuaClassIndexFieldName);
            ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);
            ULuaAPI.luaex_gen_cls_index(L);
            //store in lua index function tables
            ULuaAPI.luaex_pushasciistring(L, kLuaClassIndexFieldName);
            ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);
            parser.Push(L, type);
            ULuaAPI.lua_pushvalue(L, -3);
            ULuaAPI.lua_rawset(L, -3);
            ULuaAPI.lua_pop(L, 1);
            ULuaAPI.lua_rawset(L, cls_meta); // set __index 

            ULuaAPI.luaex_pushasciistring(L, "__newindex");
            ULuaAPI.lua_pushvalue(L, cls_setter);
            parser.Push(L, type.BaseType);
            ULuaAPI.luaex_pushasciistring(L, kLuaClassNewIndexFieldName);
            ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);
            ULuaAPI.luaex_gen_cls_newindex(L);
            //store in lua new index function tables
            ULuaAPI.luaex_pushasciistring(L, kLuaClassNewIndexFieldName);
            ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);
            parser.Push(L, type);
            ULuaAPI.lua_pushvalue(L, -3);
            ULuaAPI.lua_rawset(L, -3);
            ULuaAPI.lua_pop(L, 1);
            ULuaAPI.lua_rawset(L, cls_meta); // set __newindex

            lua_CSFunction constructor = parser.mMethodWrapsCache.GetConstructorWrap(type);
            if (constructor == null)
            {
                constructor = (ULuaState LL) =>
                {
                    return ULuaAPI.luaL_error(LL, "No constructor for " + type);
                };
            }

            ULuaAPI.luaex_pushasciistring(L, "__call");
            parser.PushFixCSFunction(L, constructor);
            ULuaAPI.lua_rawset(L, cls_meta);

            ULuaAPI.lua_pushvalue(L, cls_meta);
            ULuaAPI.lua_setmetatable(L, cls_field);

            ULuaAPI.lua_pop(L, 8);

            System.Diagnostics.Debug.Assert(top_enter == ULuaAPI.lua_gettop(L));
        }

        //meta: -4, method:-3, getter: -2, setter: -1
        public static void BeginObjectRegister(Type type, ULuaState L, UObjectParser parser, int meta_count, int method_count, int getter_count, int setter_count, int type_id = -1)
        {
            if (type == null)
            {
                if (type_id == -1)
                    throw new Exception("Fatal: must provide a type of type_id");

                ULuaAPI.luaex_rawgeti(L, ULuaIndexes.LUA_REGISTRYINDEX, type_id);
            } else
            {
                ULuaAPI.luaL_getmetatable(L, type.FullName);
                if (ULuaAPI.lua_isnil(L, -1))
                {
                    ULuaAPI.lua_pop(L, 1);
                    ULuaAPI.luaL_newmetatable(L, type.FullName);
                }
            }
            ULuaAPI.lua_pushlightuserdata(L, ULuaAPI.luaex_tag());
            ULuaAPI.lua_pushnumber(L, 1);
            ULuaAPI.lua_rawset(L, -3);

            if ((type == null || !parser.HasCustomOP(type)) && type != typeof(decimal))
            {
                ULuaAPI.luaex_pushasciistring(L, "__gc"); 
                ULuaAPI.lua_pushstdcallcfunction(L, parser.mMetaFunctions.GCMeta);
                ULuaAPI.lua_rawset(L, -3);
            }

            ULuaAPI.luaex_pushasciistring(L, "__tostring");
            ULuaAPI.lua_pushstdcallcfunction(L, parser.mMetaFunctions.ToStringMeta);
            ULuaAPI.lua_rawset(L, -3);

            if (method_count == 0)
                ULuaAPI.lua_pushnil(L);
            else
                ULuaAPI.lua_createtable(L, 0, method_count);

            if (getter_count == 0)
                ULuaAPI.lua_pushnil(L);
            else
                ULuaAPI.lua_createtable(L, 0, getter_count);

            if (setter_count == 0)
                ULuaAPI.lua_pushnil(L);
            else
                ULuaAPI.lua_createtable(L, 0, setter_count);
        }

        static int abs_idx(int top, int idx)
        {
            return idx > 0 ? idx : top + idx + 1;
        }

        public static void EndObjectRegister(Type type, ULuaState L, UObjectParser parser,
            lua_CSFunction csIndex, lua_CSFunction csNewIndex, Type base_type, lua_CSFunction arrayIndex, lua_CSFunction arrayNewIndex)
        {
            int top = ULuaAPI.lua_gettop(L);

            int meta_idx = abs_idx(top, kIDX_OBJ_META);
            int method_idx = abs_idx(top, kIDX_METHOD);
            int getter_idx = abs_idx(top, kIDX_GETTER);
            int setter_idx = abs_idx(top, kIDX_SETTER);

            //begin index gen
            ULuaAPI.luaex_pushasciistring(L, "__index");
            ULuaAPI.lua_pushvalue(L, method_idx);
            ULuaAPI.lua_pushvalue(L, getter_idx);

            if (csIndex == null)
                ULuaAPI.lua_pushnil(L);
            else
                ULuaAPI.lua_pushstdcallcfunction(L, csIndex);

            parser.Push(L, type == null ? base_type : type.BaseType);

            ULuaAPI.luaex_pushasciistring(L, kLuaIndexFieldName);
            ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);
            if (arrayIndex == null)
                ULuaAPI.lua_pushnil(L);
            else
                ULuaAPI.lua_pushstdcallcfunction(L, arrayIndex);

            ULuaAPI.luaex_gen_obj_index(L);

            if (type != null)
            {
                ULuaAPI.luaex_pushasciistring(L, kLuaIndexFieldName);
                ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);//store in lua index function tables
                parser.Push(L, type);
                ULuaAPI.lua_pushvalue(L, -3);
                ULuaAPI.lua_rawset(L, -3);
                ULuaAPI.lua_pop(L, 1);
            }

            ULuaAPI.lua_rawset(L, meta_idx);
            //end index gen

            //begin new index gen
            ULuaAPI.luaex_pushasciistring(L, "__newindex");
            ULuaAPI.lua_pushvalue(L, setter_idx);

            if (csNewIndex == null)
                ULuaAPI.lua_pushnil(L);
            else
                ULuaAPI.lua_pushstdcallcfunction(L, csNewIndex);

            parser.Push(L, type == null ? base_type : type.BaseType);

            ULuaAPI.luaex_pushasciistring(L, kLuaNewIndexFieldName);
            ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);

            if (arrayNewIndex == null)
                ULuaAPI.lua_pushnil(L);
            else
                ULuaAPI.lua_pushstdcallcfunction(L, arrayNewIndex);

            ULuaAPI.luaex_gen_obj_newindex(L);

            if (type != null)
            {
                ULuaAPI.luaex_pushasciistring(L, kLuaNewIndexFieldName);
                ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);   //store in lua new index function tables
                parser.Push(L, type);
                ULuaAPI.lua_pushvalue(L, -3);
                ULuaAPI.lua_rawset(L, -3);
                ULuaAPI.lua_pop(L, 1);
            }

            ULuaAPI.lua_rawset(L, meta_idx);
            //end new index gen

            ULuaAPI.lua_pop(L, 4);
        }

        public static void RegisterFunc(ULuaState L, int idx, string name, lua_CSFunction func)
        {
            idx = abs_idx(ULuaAPI.lua_gettop(L), idx);

            ULuaAPI.luaex_pushasciistring(L, name);
            ULuaAPI.lua_pushstdcallcfunction(L, func);
            ULuaAPI.lua_rawset(L, idx);
        }

        public static void RegisterObject(ULuaState L, UObjectParser parser, int idx, string name, object obj)
        {
            idx = abs_idx(ULuaAPI.lua_gettop(L), idx);

            ULuaAPI.luaex_pushasciistring(L, name);
            parser.PushAny(L, obj);
            ULuaAPI.lua_rawset(L, idx);
        }

        public static void BeginClassRegister(Type type, ULuaState L, lua_CSFunction creator, int class_field_count, int static_getter_count, int static_setter_count)
        {
            ULuaAPI.lua_createtable(L, 0, class_field_count);

            int cls_table = ULuaAPI.lua_gettop(L);

            SetCSTable(L, type, cls_table);

            ULuaAPI.lua_createtable(L, 0, 3);
            int meta_table = ULuaAPI.lua_gettop(L);
            if (creator != null)
            {
                ULuaAPI.luaex_pushasciistring(L, "__call");
                ULuaAPI.lua_pushstdcallcfunction(L, creator);
                ULuaAPI.lua_rawset(L, -3);
            }

            if (static_getter_count == 0)
                ULuaAPI.lua_pushnil(L);
            else
                ULuaAPI.lua_createtable(L, 0, static_getter_count);
            
            if (static_setter_count == 0)
                ULuaAPI.lua_pushnil(L);
            else
                ULuaAPI.lua_createtable(L, 0, static_setter_count);
            ULuaAPI.lua_pushvalue(L, meta_table);
            ULuaAPI.lua_setmetatable(L, cls_table);
        }

        public static void EndClassRegister(Type type, ULuaState L, UObjectParser parser)
        {
            int top = ULuaAPI.lua_gettop(L);

            int cls_idx = abs_idx(top, kCLS_IDX);
            int cls_getter_idx = abs_idx(top, kCLS_GETTER_IDX);
            int cls_setter_idx = abs_idx(top, kCLS_SETTER_IDX);
            int cls_meta_idx = abs_idx(top, kCLS_META_IDX);
   
            //begin class index
            ULuaAPI.luaex_pushasciistring(L, "__index");
            ULuaAPI.lua_pushvalue(L, cls_getter_idx);
            ULuaAPI.lua_pushvalue(L, cls_idx);
            parser.Push(L, type.BaseType);
            ULuaAPI.luaex_pushasciistring(L, kLuaClassIndexFieldName);
            ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);
            ULuaAPI.luaex_gen_cls_index(L);

            ULuaAPI.luaex_pushasciistring(L, kLuaClassIndexFieldName);
            ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);//store in lua index function tables
            parser.Push(L, type);
            ULuaAPI.lua_pushvalue(L, -3);
            ULuaAPI.lua_rawset(L, -3);
            ULuaAPI.lua_pop(L, 1);

            ULuaAPI.lua_rawset(L, cls_meta_idx);
            //end class index

            //begin class new index
            ULuaAPI.luaex_pushasciistring(L, "__newindex");
            ULuaAPI.lua_pushvalue(L, cls_setter_idx);
            parser.Push(L, type.BaseType);
            ULuaAPI.luaex_pushasciistring(L, kLuaClassNewIndexFieldName);
            ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);
            ULuaAPI.luaex_gen_cls_newindex(L);

            ULuaAPI.luaex_pushasciistring(L, kLuaClassNewIndexFieldName);
            ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);   //store in lua new index function tables
            parser.Push(L, type);
            ULuaAPI.lua_pushvalue(L, -3);
            ULuaAPI.lua_rawset(L, -3);
            ULuaAPI.lua_pop(L, 1);

            ULuaAPI.lua_rawset(L, cls_meta_idx);
            //end class new index

            ULuaAPI.lua_pop(L, 4);
        }

        static List< string > GetPathOfType(Type type)
        {
            List< string > path = new List< string >();

            if (type.Namespace != null)
                path.AddRange(type.Namespace.Split(new char[] { '.' }));

            string class_name = type.ToString().Substring(type.Namespace == null ? 0 : type.Namespace.Length + 1);

            if (type.IsNested)
                path.AddRange(class_name.Split(new char[] { '+' }));
            else
                path.Add(class_name);

            return path;
        }

        public static void LoadCSTable(ULuaState L, Type type)
        {
            int oldTop = ULuaAPI.lua_gettop(L);
            if (0 != ULuaAPI.luaex_getglobal(L, "CS"))
                throw new Exception("call luaex_getglobal fail!");

            List< string > path = GetPathOfType(type);

            for (int i = 0; i < path.Count; ++ i)
            {
                ULuaAPI.luaex_pushasciistring(L, path[i]);
                if (0 != ULuaAPI.luaex_pgettable(L, -2))
                {
                    ULuaAPI.lua_settop(L, oldTop);
                    ULuaAPI.lua_pushnil(L);

                    return;
                }

                if (!ULuaAPI.lua_istable(L, -1) && i < path.Count - 1)
                {
                    ULuaAPI.lua_settop(L, oldTop);
                    ULuaAPI.lua_pushnil(L);

                    return;
                }
                ULuaAPI.lua_remove(L, -2);
            }
        }

        public static void SetCSTable(ULuaState L, Type type, int cls_table)
        {
            int oldTop = ULuaAPI.lua_gettop(L);

            cls_table = abs_idx(oldTop, cls_table);
            if (0 != ULuaAPI.luaex_getglobal(L, "CS"))
                throw new Exception("call luaex_getglobal fail!");

            List< string > path = GetPathOfType(type);

            for (int i = 0; i < path.Count - 1; ++ i)
            {
                ULuaAPI.luaex_pushasciistring(L, path[i]);
                if (0 != ULuaAPI.luaex_pgettable(L, -2))
                {
                    ULuaAPI.lua_settop(L, oldTop);

                    throw new Exception("SetCSTable for [" + type + "] error: " + ULuaAPI.lua_tostring(L, -1));
                }

                if (ULuaAPI.lua_isnil(L, -1))
                {
                    ULuaAPI.lua_pop(L, 1);
                    ULuaAPI.lua_createtable(L, 0, 0);
                    ULuaAPI.luaex_pushasciistring(L, path[i]);
                    ULuaAPI.lua_pushvalue(L, -2);
                    ULuaAPI.lua_rawset(L, -4);
                } else if (!ULuaAPI.lua_istable(L, -1))
                {
                    ULuaAPI.lua_settop(L, oldTop);

                    throw new Exception("SetCSTable for [" + type + "] error: ancestors is not a table!");
                }
                ULuaAPI.lua_remove(L, -2);
            }

            ULuaAPI.luaex_pushasciistring(L, path[path.Count -1]);
            ULuaAPI.lua_pushvalue(L, cls_table);
            ULuaAPI.lua_rawset(L, -3);
            ULuaAPI.lua_pop(L, 1);
        }

        public static bool IsParamsMatch(MethodInfo delegateMethod, MethodInfo bridgeMethod)
        {
            if (delegateMethod == null || bridgeMethod == null)
                return false;

            if (delegateMethod.ReturnType != bridgeMethod.ReturnType)
                return false;

            ParameterInfo[] delegateParams = delegateMethod.GetParameters();
            ParameterInfo[] bridgeParams = bridgeMethod.GetParameters();
            if (delegateParams.Length != bridgeParams.Length)
                return false;

            for (int i = 0; i < delegateParams.Length; i ++)
            {
                if (delegateParams[i].ParameterType != bridgeParams[i].ParameterType || delegateParams[i].IsOut != bridgeParams[i].IsOut)
                    return false;
            }

            return true;
        }
        public static bool IsSupportedExtensionMethod(MethodBase method)
        {
            if (!method.IsDefined(typeof(ExtensionAttribute), false))
                return false;

            if (!method.ContainsGenericParameters)
                return true;

            var hasValidGenericParameter = false;

            var methodParameters = method.GetParameters();
            for (var i = 0; i < methodParameters.Length; i ++)
            {
                var parameterType = methodParameters[i].ParameterType;
                if (parameterType.IsGenericParameter)
                {
                    var parameterConstraints = parameterType.GetGenericParameterConstraints();
                    if (parameterConstraints.Length == 0 || !parameterConstraints[0].IsClass)
                        return false;

                    hasValidGenericParameter = true;
                }
            }

            return hasValidGenericParameter;
        }

        private static Type GetExtendedType(MethodInfo method)
        {
            var type = method.GetParameters()[0].ParameterType;
            if (!type.IsGenericParameter)
                return type;

            var parameterConstraints = type.GetGenericParameterConstraints();
            if (parameterConstraints.Length == 0)
                throw new InvalidOperationException();

            var firstParameterConstraint = parameterConstraints[0];
            if (!firstParameterConstraint.IsClass)
                throw new InvalidOperationException();

            return firstParameterConstraint;
        }

        public static bool IsStaticPInvokeCSFunction(lua_CSFunction csFunction)
        {
            return csFunction.Method.IsStatic && Attribute.IsDefined(csFunction.Method, typeof(UMonoPInvokeCallbackAttribute));
        }
    }
}
