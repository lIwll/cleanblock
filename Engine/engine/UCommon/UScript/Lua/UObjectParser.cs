using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using ULuaState = System.IntPtr;

namespace UEngine.ULua
{
    class UReferenceEqualsComparer : IEqualityComparer< object >
    {
        public new bool Equals(object o1, object o2)
        {
            return object.ReferenceEquals(o1, o2);
        }

        public int GetHashCode(object obj)
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }

    public class UMonoPInvokeCallbackAttribute : System.Attribute
    {
        private Type mType;

        public UMonoPInvokeCallbackAttribute(Type t)
        {
            mType = t;
        }
    }

    public partial class UObjectParser
	{
        public delegate void    PushCSObject(ULuaState L, object obj);
        public delegate object  GetCSObject(ULuaState L, int idx);
        public delegate void    UpdateCSObject(ULuaState L, int idx, object obj);
        public delegate void    GetFunc< T >(ULuaState L, int idx,  out T val);

        internal UMethodWrapsCache      mMethodWrapsCache;
        internal UObjectChecker         mObjectChecker;
        internal UObjectCaster          mObjectCaster;
        internal readonly UObjectPool   mObjects = new UObjectPool();
        internal readonly Dictionary< object, int >
                                        mReverseMap = new Dictionary< object, int >(new UReferenceEqualsComparer());
        internal ULuaEnv                mEnv;
        internal ULuaCallbacks          mMetaFunctions;
        internal List< Assembly >       mAssemblies;
        private lua_CSFunction          mImportTypeFunction;
        private lua_CSFunction          mLoadAssemblyFunction;
        private lua_CSFunction          mCastFunction;
        private readonly Dictionary< Type, Action< ULuaState > >
                                        mDelayWrap = new Dictionary< Type, Action< ULuaState > >();
        private readonly Dictionary< Type, Func< int, ULuaEnv, ULuaBase > >
                                        mInterfaceBridgeCreators = new Dictionary< Type, Func< int, ULuaEnv, ULuaBase > >();
        private readonly Dictionary<Type, Type>
                                        mAliasCfg = new Dictionary< Type, Type >();
        private Dictionary< Type, bool >
                                        mLoadedTypes = new Dictionary< Type, bool >();
        public int                      mCacheRef;
        private Type                    mDelegateBirdgeType;
        List< lua_CSFunction >          mFixCSFunctions = new List< lua_CSFunction >();
        Dictionary< object, int >       mEnumMap = new Dictionary< object, int >();
        Dictionary< Type, int >         mTypeIdMap = new Dictionary< Type, int >();
        Dictionary< int, Type >         mTypeMap = new Dictionary< int, Type >();
        private Dictionary< Type, PushCSObject >
                                        mCustomPushFuncs = new Dictionary< Type, PushCSObject >();
        private Dictionary< Type, GetCSObject >
                                        mCustomGetFuncs = new Dictionary< Type, GetCSObject >();
        private Dictionary< Type, UpdateCSObject >
                                        mCustomUpdateFuncs = new Dictionary< Type, UpdateCSObject >();
        private Dictionary< Type, Delegate >
                                        mPushFuncWithType = null;
        private Dictionary< Type, Delegate >
                                        mGetFuncWithType = null;
        int                             mDecimalTypeId = -1;

        public void DelayWrapLoader(Type type, Action< ULuaState > loader)
        {
            mDelayWrap[type] = loader;
        }

        public void AddInterfaceBridgeCreator(Type type, Func< int, ULuaEnv, ULuaBase > creator)
        {
            mInterfaceBridgeCreators.Add(type, creator);
        }

        public bool TryDelayWrapLoader(ULuaState L, Type type)
        {
            if (mLoadedTypes.ContainsKey(type))
                return true;
            mLoadedTypes.Add(type, true);

            ULuaAPI.luaL_newmetatable(L, type.FullName);
            ULuaAPI.lua_pop(L, 1);

            Action< ULuaState > loader;
            int top = ULuaAPI.lua_gettop(L);
            if (mDelayWrap.TryGetValue(type, out loader))
            {
                mDelayWrap.Remove(type);

                loader(L);
            } else
            {
                Utils.ReflectionWrap(L, type);
            }

            if (top != ULuaAPI.lua_gettop(L))
                throw new Exception("top change, before:" + top + ", after:" + ULuaAPI.lua_gettop(L));

			Type[] nested_types = type.GetNestedTypes(BindingFlags.Public);
            for (int i = 0; i < nested_types.Length; ++ i)
            {
				var nested_type = nested_types[i];

                if ((!nested_type.IsAbstract && typeof(Delegate).IsAssignableFrom(nested_type)) || nested_type.IsGenericTypeDefinition)
                    continue;

                TryDelayWrapLoader(L, nested_type);
            }
            
            return true;
        }
        
        public void Alias(Type type, string alias)
        {
            Type alias_type = FindType(alias);
            if (alias_type == null)
                throw new ArgumentException("Can not find " + alias);
            mAliasCfg[alias_type] = type;
        }

        void AddAssemblieByName(IEnumerable< Assembly > assemblies_usorted, string name)
        {
            foreach (var assemblie in assemblies_usorted)
            {
                if (assemblie.FullName.StartsWith(name) && !mAssemblies.Contains(assemblie))
                {
                    mAssemblies.Add(assemblie);

                    break;
                }
            }
        }

        public UObjectParser(ULuaEnv env, ULuaState L)
		{
            mAssemblies = new List< Assembly >();
            mAssemblies.Add(Assembly.GetExecutingAssembly());

            var assemblies_usorted = AppDomain.CurrentDomain.GetAssemblies();

            AddAssemblieByName(assemblies_usorted, "mscorlib,");
            AddAssemblieByName(assemblies_usorted, "System,");
            AddAssemblieByName(assemblies_usorted, "System.Core,");
            for (int i = 0; i < assemblies_usorted.Length; ++ i)
            {
				Assembly assembly = assemblies_usorted[i];

                if (!mAssemblies.Contains(assembly))
                    mAssemblies.Add(assembly);
            }

            mEnv                    = env;
            mObjectCaster           = new UObjectCaster(this);
            mObjectChecker          = new UObjectChecker(this);
            mMethodWrapsCache       = new UMethodWrapsCache(this, mObjectChecker, mObjectCaster);
			mMetaFunctions          = new ULuaCallbacks();

            mImportTypeFunction     = new lua_CSFunction(ULuaCallbacks.ImportType);
            mLoadAssemblyFunction   = new lua_CSFunction(ULuaCallbacks.LoadAssembly);
            mCastFunction           = new lua_CSFunction(ULuaCallbacks.Cast);

            ULuaAPI.lua_newtable(L);
            ULuaAPI.lua_newtable(L);
            ULuaAPI.luaex_pushasciistring(L, "__mode");
            ULuaAPI.luaex_pushasciistring(L, "v");
            ULuaAPI.lua_rawset(L, -3);
            ULuaAPI.lua_setmetatable(L, -2);

            mCacheRef = ULuaAPI.luaL_ref(L, ULuaIndexes.LUA_REGISTRYINDEX);

            InitCSharpCallLua();
        }

        void InitCSharpCallLua()
        {
            mDelegateBirdgeType = typeof(UDelegateBridge);
        }

        Dictionary< int, WeakReference > delegate_bridges = new Dictionary< int, WeakReference >();
        public Delegate CreateDelegateBridge(ULuaState L, Type delegateType, int idx)
        {
            ULuaAPI.lua_pushvalue(L, idx);
            ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);
            if (!ULuaAPI.lua_isnil(L, -1))
            {
                int referenced = ULuaAPI.luaex_tointeger(L, -1);
                ULuaAPI.lua_pop(L, 1);

                if (delegate_bridges[referenced].IsAlive)
                {
                    UDelegateBridgeBase exist_bridge = delegate_bridges[referenced].Target as UDelegateBridgeBase;

                    Delegate exist_delegate;
                    if (exist_bridge.TryGetDelegate(delegateType, out exist_delegate))
                    {
                        return exist_delegate;
                    } else
                    {
                        exist_delegate = exist_bridge.GetDelegateByType(delegateType);

                        exist_bridge.AddDelegate(delegateType, exist_delegate);

                        return exist_delegate;
                    }
                }
            } else
            {
                ULuaAPI.lua_pop(L, 1);
            }

            ULuaAPI.lua_pushvalue(L, idx);
            int reference = ULuaAPI.luaL_ref(L);
            ULuaAPI.lua_pushvalue(L, idx);
            ULuaAPI.lua_pushnumber(L, reference);
            ULuaAPI.lua_rawset(L, ULuaIndexes.LUA_REGISTRYINDEX);

            UDelegateBridgeBase bridge;
            try
            {
                bridge = new UDelegateBridge(reference, mEnv);
            } catch(Exception e)
            {
                ULuaAPI.lua_pushvalue(L, idx);
                ULuaAPI.lua_pushnil(L);
                ULuaAPI.lua_rawset(L, ULuaIndexes.LUA_REGISTRYINDEX);
                ULuaAPI.lua_pushnil(L);
                ULuaAPI.luaex_rawseti(L, ULuaIndexes.LUA_REGISTRYINDEX, reference);
                throw e;
            }

            try
            {
                var ret = bridge.GetDelegateByType(delegateType);
                bridge.AddDelegate(delegateType, ret);
                delegate_bridges[reference] = new WeakReference(bridge);

                return ret;
            } catch(Exception e)
            {
                bridge.Dispose();
                throw e;
            }
        }

        public bool AllDelegateBridgeReleased()
        {
            foreach (var kv in delegate_bridges.Values)
            {
                if (kv.IsAlive)
                    return false;
            }

            return true;
        }

        public void ReleaseLuaBase(ULuaState L, int reference, bool is_delegate)
        {
            if (is_delegate)
            {
                ULuaAPI.luaex_rawgeti(L, ULuaIndexes.LUA_REGISTRYINDEX, reference);
                if (ULuaAPI.lua_isnil(L, -1))
                {
                    ULuaAPI.lua_pop(L, 1);
                } else
                {
                    ULuaAPI.lua_pushvalue(L, -1);
                    ULuaAPI.lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);
                    if (ULuaAPI.lua_type(L, -1) == ELuaTypes.LUA_TNUMBER && ULuaAPI.luaex_tointeger(L, -1) == reference)
                    {
                        ULuaAPI.lua_pop(L, 1);
                        ULuaAPI.lua_pushnil(L);
                        ULuaAPI.lua_rawset(L, ULuaIndexes.LUA_REGISTRYINDEX);
                    } else
                    {
                        ULuaAPI.lua_pop(L, 2);
                    }
                }

                ULuaAPI.lua_unref(L, reference);

                delegate_bridges.Remove(reference);
            } else
            {
                ULuaAPI.lua_unref(L, reference);
            }
        }

		public object CreateInterfaceBridge(ULuaState L, Type interfaceType, int idx)
        {
            Func< int, ULuaEnv, ULuaBase > creator;

            if (!mInterfaceBridgeCreators.TryGetValue(interfaceType, out creator))
            {
                throw new InvalidCastException("This interface must add to CSharpCallLua: " + interfaceType);
            }
            ULuaAPI.lua_pushvalue(L, idx);

            return creator(ULuaAPI.luaL_ref(L), mEnv);
        }

        int common_array_meta = -1;
        public void CreateArrayMetatable(ULuaState L)
        {
            Utils.BeginObjectRegister(null, L, this, 0, 0, 1, 0, common_array_meta);
            Utils.RegisterFunc(L, Utils.kIDX_GETTER, "Length", ULuaCallbacks.ArrayLength);
            Utils.EndObjectRegister(null, L, this, null, null, typeof(System.Array), ULuaCallbacks.ArrayIndex, ULuaCallbacks.ArrayNewIndex);
        }

        int common_delegate_meta = -1;
        public void CreateDelegateMetatable(ULuaState L)
        {
            Utils.BeginObjectRegister(null, L, this, 3, 0, 0, 0, common_delegate_meta);
            Utils.RegisterFunc(L, Utils.kIDX_OBJ_META, "__call", ULuaCallbacks.DelegateCall);
            Utils.RegisterFunc(L, Utils.kIDX_OBJ_META, "__add", ULuaCallbacks.DelegateCombine);
            Utils.RegisterFunc(L, Utils.kIDX_OBJ_META, "__sub", ULuaCallbacks.DelegateRemove);
            Utils.EndObjectRegister(null, L, this, null, null, typeof(System.MulticastDelegate), null, null);
        }

		public void OpenLib(ULuaState L)
		{
            if (0 != ULuaAPI.luaex_getglobal(L, "luaex"))
                throw new Exception("call luaex_getglobal fail!" + ULuaAPI.lua_tostring(L, -1));

            ULuaAPI.luaex_pushasciistring(L, "import_type");
			ULuaAPI.lua_pushstdcallcfunction(L, mImportTypeFunction);
			ULuaAPI.lua_rawset(L, -3);
            ULuaAPI.luaex_pushasciistring(L, "cast");
            ULuaAPI.lua_pushstdcallcfunction(L, mCastFunction);
            ULuaAPI.lua_rawset(L, -3);
            ULuaAPI.luaex_pushasciistring(L, "load_assembly");
			ULuaAPI.lua_pushstdcallcfunction(L, mLoadAssemblyFunction);
            ULuaAPI.lua_rawset(L, -3);
            ULuaAPI.luaex_pushasciistring(L, "access");
            ULuaAPI.lua_pushstdcallcfunction(L, ULuaCallbacks.LuaexAccess);
            ULuaAPI.lua_rawset(L, -3);
            ULuaAPI.luaex_pushasciistring(L, "private_accessible");
            ULuaAPI.lua_pushstdcallcfunction(L, ULuaCallbacks.LuaexPrivateAccessible);
            ULuaAPI.lua_rawset(L, -3);
            ULuaAPI.lua_pop(L, 1);

            ULuaAPI.lua_createtable(L, 1, 4);
            common_array_meta = ULuaAPI.luaL_ref(L, ULuaIndexes.LUA_REGISTRYINDEX);
            ULuaAPI.lua_createtable(L, 1, 4);
            common_delegate_meta = ULuaAPI.luaL_ref(L, ULuaIndexes.LUA_REGISTRYINDEX);
        }
		
		internal void CreateFunctionMetatable(ULuaState L)
		{
			ULuaAPI.lua_newtable(L);
			ULuaAPI.luaex_pushasciistring(L, "__gc");
			ULuaAPI.lua_pushstdcallcfunction(L, mMetaFunctions.GCMeta);
			ULuaAPI.lua_rawset(L, -3);
            ULuaAPI.lua_pushlightuserdata(L, ULuaAPI.luaex_tag());
            ULuaAPI.lua_pushnumber(L, 1);
            ULuaAPI.lua_rawset(L, -3);

            ULuaAPI.lua_pushvalue(L, -1);
            int type_id = ULuaAPI.luaL_ref(L, ULuaIndexes.LUA_REGISTRYINDEX);
            ULuaAPI.lua_pushnumber(L, type_id);
            ULuaAPI.luaex_rawseti(L, -2, 1);
            ULuaAPI.lua_pop(L, 1);

            mTypeIdMap.Add(typeof(lua_CSFunction), type_id);
        }
		
		internal Type FindType(string className, bool isQualifiedName = false)
		{
            for (int i = 0; i < mAssemblies.Count; ++ i)
			{
				Assembly assembly = mAssemblies[i];

                Type klass = assembly.GetType(className);

                if (klass!=null)
					return klass;
			}

            int p1 = className.IndexOf('[');
            if (p1 > 0 && !isQualifiedName)
            {
                string qualified_name = className.Substring(0, p1 + 1);
                string[] generic_params = className.Substring(p1 + 1, className.Length - qualified_name.Length - 1).Split(',');
                for(int i = 0; i < generic_params.Length; i ++)
                {
                    Type generic_param = FindType(generic_params[i].Trim());
                    if (generic_param == null)
                        return null;

                    if (i != 0 )
                        qualified_name += ", ";

                    qualified_name = qualified_name + "[" + generic_param.AssemblyQualifiedName + "]";
                }
                qualified_name += "]";

                return FindType(qualified_name, true);
            }

			return null;
		}

        bool HasMethod(Type type, string methodName)
        {
			var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            for (int i = 0; i < methods.Length; ++ i)
            {
				var method = methods[i];

                if (method.Name == methodName)
                    return true;
            }

            return false;
        }
		
		internal void CollectObject(int obj_index_to_collect)
		{
			object o = null;
			
			if (mObjects.TryGetValue(obj_index_to_collect, out o))
			{
				mObjects.Remove(obj_index_to_collect);
                
                if (o != null)
                {
                    int obj_index;
                    bool is_enum = o.GetType().IsEnum;

                    if ((is_enum ? mEnumMap.TryGetValue(o, out obj_index) : mReverseMap.TryGetValue(o, out obj_index)) && obj_index == obj_index_to_collect)
                    {
                        if (is_enum)
                            mEnumMap.Remove(o);
                        else
                            mReverseMap.Remove(o);
                    }
                }
			}
		}
		
		int AddObject(object obj, bool is_valuetype, bool is_enum)
		{
            int index = mObjects.Add(obj);
            if (is_enum)
                mEnumMap[obj] = index;
            else if (!is_valuetype)
                mReverseMap[obj] = index;
			
			return index;
		}
		
		internal object GetObject(ULuaState L, int index)
		{
            return (mObjectCaster.GetCaster(typeof(object))(L, index, null));
        }

        public Type GetTypeOf(ULuaState L, int idx)
        {
            Type type = null;

            int type_id = ULuaAPI.luaex_gettypeid(L, idx);
            if (type_id != -1)
                mTypeMap.TryGetValue(type_id, out type);

            return type;
        }

        public bool Assignable< T >(ULuaState L, int index)
		{
            return Assignable(L, index, typeof(T));
        }

        public bool Assignable(ULuaState L, int index, Type type)
        {
            if (ULuaAPI.lua_type(L, index) == ELuaTypes.LUA_TUSERDATA)
            {
                int udata = ULuaAPI.luaex_tocsobj_safe(L, index);

                object obj = null;
                if (udata != -1 && mObjects.TryGetValue(udata, out obj))
                    return type.IsAssignableFrom(obj.GetType());

                int type_id = ULuaAPI.luaex_gettypeid(L, index);

                Type type_of_struct;
                if (type_id != -1 && mTypeMap.TryGetValue(type_id, out type_of_struct)) // is struct
                    return type.IsAssignableFrom(type_of_struct);
            }

            return mObjectChecker.GetChecker(type)(L, index);
        }

        public object GetObject(ULuaState L, int index, Type type)
        {
            int udata = ULuaAPI.luaex_tocsobj_safe(L, index);

            if (udata != -1)
            {
                return mObjects.Get(udata);
            } else
            {
                if (ULuaAPI.lua_type(L, index) == ELuaTypes.LUA_TUSERDATA)
                {
                    int type_id = ULuaAPI.luaex_gettypeid(L, index);
                    if (type_id != -1 && type_id == mDecimalTypeId)
                    {
                        decimal d;
                        Get(L, index, out d);

                        return d;
                    }

                    GetCSObject getC; Type type_of_struct;
                    if (type_id != -1 && mTypeMap.TryGetValue(type_id, out type_of_struct) && type.IsAssignableFrom(type_of_struct) && mCustomGetFuncs.TryGetValue(type, out getC))
                        return getC(L, index);
                }

                return (mObjectCaster.GetCaster(type)(L, index, null));
            }
        }

        public void Get< T >(ULuaState L, int index, out T v)
        {
            Func< ULuaState, int, T > get_func;
            if (TryGetGetFuncByType(typeof(T), out get_func))
                v = get_func(L, index);
            else
                v = (T)GetObject(L, index, typeof(T));
        }

        public void PushByType< T >(ULuaState L,  T v)
        {
            Action< ULuaState, T > push_func;
            if (TryGetPushFuncByType(typeof(T), out push_func))
                push_func(L, v);
            else
                PushAny(L, v);
        }

        public T[] GetParams< T >(ULuaState L, int index)
        {
            T[] ret = new T[Math.Max(ULuaAPI.lua_gettop(L) - index + 1, 0)];
            for (int i = 0; i < ret.Length; i ++)
                Get(L, index + i, out ret[i]);

            return ret;
        }

        public Array GetParams(ULuaState L, int index, Type type)
        {
            Array ret = Array.CreateInstance(type, Math.Max(ULuaAPI.lua_gettop(L) - index + 1, 0));
            for (int i = 0; i < ret.Length; i ++)
            {
                ret.SetValue(GetObject(L, index + i, type), i); 
            }

            return ret;
        }

        public void PushParams(ULuaState L, Array ary)
        {
            if (ary != null)
            {
                for (int i = 0; i < ary.Length; i ++)
                    PushAny(L, ary.GetValue(i));
            }
        }

        public T GetDelegate< T >(ULuaState L, int index) where T :class
        {
            
            if (ULuaAPI.lua_isfunction(L, index))
                return CreateDelegateBridge(L, typeof(T), index) as T;
            else if (ULuaAPI.lua_type(L, index) == ELuaTypes.LUA_TUSERDATA)
                return (T)SafeGetCSObj(L, index);

            return null;
        }

        int GetTypeId(ULuaState L, Type type, out bool is_first, ELogLevel log_level = ELogLevel.eLV_WARN)
        {
            is_first = false;

            int type_id;
            if (!mTypeIdMap.TryGetValue(type, out type_id))
            {
                if (type.IsArray)
                {
                    if (common_array_meta == -1)
                        throw new Exception("Fatal exception! array meta table not init!");

                    return common_array_meta;
                }

                if (typeof(MulticastDelegate).IsAssignableFrom(type))
                {
                    if (common_delegate_meta == -1)
                        throw new Exception("Fatal exception! delegate meta table not init!");

                    return common_delegate_meta;
                }

                is_first = true;

                Type alias_type = null;
                mAliasCfg.TryGetValue(type, out alias_type);

                ULuaAPI.luaL_getmetatable(L, alias_type == null ? type.FullName : alias_type.FullName);

                if (ULuaAPI.lua_isnil(L, -1))
                {
                    ULuaAPI.lua_pop(L, 1);

                    if (TryDelayWrapLoader(L, alias_type == null ? type : alias_type))
                        ULuaAPI.luaL_getmetatable(L, alias_type == null ? type.FullName : alias_type.FullName);
                    else
                        throw new Exception("Fatal: can not load metatable of type:" + type);
                }

                if (mTypeIdMap.TryGetValue(type, out type_id))
                {
                    ULuaAPI.lua_pop(L, 1);
                } else
                {
                    ULuaAPI.lua_pushvalue(L, -1);
                    type_id = ULuaAPI.luaL_ref(L, ULuaIndexes.LUA_REGISTRYINDEX);
                    ULuaAPI.lua_pushnumber(L, type_id);
                    ULuaAPI.luaex_rawseti(L, -2, 1);
                    ULuaAPI.lua_pop(L, 1);

                    if (type.IsValueType)
                        mTypeMap.Add(type_id, type);

                    mTypeIdMap.Add(type, type_id);
                }
            }

            return type_id;
        }

        void PushPrimitive(ULuaState L, object o)
        {
            if (o is sbyte || o is byte || o is short || o is ushort || o is int)
            {
                int i = Convert.ToInt32(o);

                ULuaAPI.luaex_pushinteger(L, i);
            } else if (o is uint)
            {
                ULuaAPI.luaex_pushuint(L, (uint)o);
            } else if (o is float || o is double)
            {
                double d = Convert.ToDouble(o);

                ULuaAPI.lua_pushnumber(L, d);
            } else if (o is IntPtr)
            {
                ULuaAPI.lua_pushlightuserdata(L, (IntPtr)o);
            } else if (o is char)
            {
                ULuaAPI.luaex_pushinteger(L, (char)o);
            } else if (o is long)
            {
                ULuaAPI.lua_pushint64(L, Convert.ToInt64(o));
            } else if (o is ulong)
            {
                ULuaAPI.lua_pushuint64(L, Convert.ToUInt64(o));
            } else if (o is bool)
            {
                bool b = (bool)o;

                ULuaAPI.lua_pushboolean(L, b);
            } else
            {
                throw new Exception("No support type " + o.GetType());
            }
        }

        public void PushAny(ULuaState L, object o)
        {
            if (o == null)
            {
                ULuaAPI.lua_pushnil(L);

                return;
            }

            Type type = o.GetType();
            if (type.IsPrimitive)
            {
                PushPrimitive(L, o);
            } else if (o is string)
            {
                ULuaAPI.lua_pushstring(L, o as string);
            } else if (o is byte[])
            {
                ULuaAPI.lua_pushstring(L, o as byte[]);
            } else if (o is decimal)
            {
                PushDecimal(L, (decimal)o);
            } else if (o is ULuaBase)
            {
                ((ULuaBase)o).Push(L);
            } else if (o is lua_CSFunction)
            {
                Push(L, o as lua_CSFunction);
            } else if (o is ValueType)
            {
                PushCSObject push;
                if (mCustomPushFuncs.TryGetValue(o.GetType(), out push))
                    push(L, o);
                else
                    Push(L, o);
            } else
            {
                Push(L, o);
            }
        }

        public int TranslateToEnumToTop(ULuaState L, Type type, int idx)
        {
            object res = null;

            ELuaTypes lt = (ELuaTypes)ULuaAPI.lua_type(L, idx);
            if (lt == ELuaTypes.LUA_TNUMBER)
            {
                int ival = (int)ULuaAPI.lua_tonumber(L, idx);

                res = Enum.ToObject(type, ival);
            } else if (lt == ELuaTypes.LUA_TSTRING)
            {
                string sflags = ULuaAPI.lua_tostring(L, idx);
                string err = null;
                try
                {
                    res = Enum.Parse(type, sflags);
                } catch (ArgumentException e)
                {
                    err = e.Message;
                }

                if (err != null)
                    return ULuaAPI.luaL_error(L, err);
            } else
            {
                return ULuaAPI.luaL_error(L, "#1 argument must be a integer or a string");
            }
            PushAny(L, res);

            return 1;
        }

        public void Push(ULuaState L, lua_CSFunction o)
        {
            if (Utils.IsStaticPInvokeCSFunction(o))
            {
                ULuaAPI.lua_pushstdcallcfunction(L, o);
            } else
            {
                Push(L, (object)o);

                ULuaAPI.lua_pushstdcallcfunction(L, mMetaFunctions.StaticCSFunctionWraper, 1);
            }
        }

        public void Push(ULuaState L, ULuaBase o)
        {
            if (o == null)
                ULuaAPI.lua_pushnil(L);
            else
                o.Push(L);
        }

        public void Push(ULuaState L, object o)
        {
            if (o == null)
            {
                ULuaAPI.lua_pushnil(L);

                return;
            }

            Type type = o.GetType();
            bool is_enum = type.IsEnum;
            bool is_valuetype = type.IsValueType;
            bool needcache = !is_valuetype || is_enum;

            int index = -1;
            if (needcache && (is_enum ? mEnumMap.TryGetValue(o, out index) : mReverseMap.TryGetValue(o, out index)))
            {
                if (ULuaAPI.luaex_tryget_cachedud(L, index, mCacheRef) == 1)
                    return;

                //CollectObject(index);
            }

            bool is_first;
            int type_id = GetTypeId(L, type, out is_first);

            if (is_first && needcache && (is_enum ? mEnumMap.TryGetValue(o, out index) : mReverseMap.TryGetValue(o, out index))) 
            {
                if (ULuaAPI.luaex_tryget_cachedud(L, index, mCacheRef) == 1)
                    return;
            }

            index = AddObject(o, is_valuetype, is_enum);

            ULuaAPI.luaex_pushcsobj(L, index, type_id, needcache, mCacheRef);
        }

        public void PushObject(ULuaState L, object o, int type_id)
        {
            if (o == null)
            {
                ULuaAPI.lua_pushnil(L);

                return;
            }

            int index = -1;
            if (mReverseMap.TryGetValue(o, out index))
            {
                if (ULuaAPI.luaex_tryget_cachedud(L, index, mCacheRef) == 1)
                    return;
            }

            index = AddObject(o, false, false);

            ULuaAPI.luaex_pushcsobj(L, index, type_id, true, mCacheRef);
        }

        public void Update(ULuaState L, int index, object obj)
        {
            int udata = ULuaAPI.luaex_tocsobj_fast(L, index);

            if (udata != -1)
            {
                mObjects.Replace(udata, obj);
            } else
            {
                UpdateCSObject update;
                if (mCustomUpdateFuncs.TryGetValue(obj.GetType(), out update))
                    update(L, index, obj);
                else
                    throw new Exception("can not update [" + obj + "]");
            }
        }

        private object GetCSObj(ULuaState L, int index, int udata)
        {
            object obj = null;
            if (udata == -1)
            {
                if (ULuaAPI.lua_type(L, index) != ELuaTypes.LUA_TUSERDATA)
                    return null;

                Type type = GetTypeOf(L, index);
                if (type == typeof(decimal))
                {
                    decimal v;
                    Get(L, index, out v);

                    return v;
                }

                GetCSObject getC;
                if (type != null && mCustomGetFuncs.TryGetValue(type, out getC))
                    return getC(L, index);
                else
                    return null;
            } else if (mObjects.TryGetValue(udata, out obj))
            {
                return obj;
            }

            return null;
        }

        public object SafeGetCSObj(ULuaState L, int index)
        {
            return GetCSObj(L, index, ULuaAPI.luaex_tocsobj_safe(L, index));
        }

		public object FastGetCSObj(ULuaState L,int index)
		{
            return GetCSObj(L, index, ULuaAPI.luaex_tocsobj_fast(L,index));
        }

        public lua_CSFunction GetFixCSFunction(int index)
        {
            return mFixCSFunctions[index];
        }

        internal void PushFixCSFunction(ULuaState L, lua_CSFunction func)
        {
            if (func == null)
            {
                ULuaAPI.lua_pushnil(L);
            } else
            {
                ULuaAPI.luaex_pushinteger(L, mFixCSFunctions.Count);
                mFixCSFunctions.Add(func);
                ULuaAPI.lua_pushstdcallcfunction(L, mMetaFunctions.FixCSFunctionWraper, 1);
            }
        }

        internal object[] PopValues(ULuaState L, int oldTop)
		{
			int newTop = ULuaAPI.lua_gettop(L);
			if (oldTop == newTop)
				return null;

            ArrayList values = new ArrayList();
			for (int i = oldTop + 1; i <= newTop; i ++)
			    values.Add(GetObject(L, i));
			ULuaAPI.lua_settop(L, oldTop);

			return values.ToArray();
		}

		internal object[] PopValues(ULuaState L, int oldTop, Type[] popTypes)
		{
			int newTop = ULuaAPI.lua_gettop(L);
			if(oldTop == newTop)
				return null;

			int iTypes = 0;
			if (popTypes[0] == typeof(void))
			    iTypes = 1;

			ArrayList values = new ArrayList();
			for (int i = oldTop + 1; i <= newTop; i ++)
			{
				values.Add(GetObject(L, i, popTypes[iTypes]));

				iTypes ++;
			}
			ULuaAPI.lua_settop(L, oldTop);

			return values.ToArray();
		}

        void RegisterCustomOP(Type type, PushCSObject push, GetCSObject getC, UpdateCSObject update)
        {
            if (push != null)
                mCustomPushFuncs.Add(type, push);
            if (getC != null)
                mCustomGetFuncs.Add(type, getC);
            if (update != null)
                mCustomUpdateFuncs.Add(type, update);
        }

        public bool HasCustomOP(Type type)
        {
            return mCustomPushFuncs.ContainsKey(type);
        }
        
        bool TryGetPushFuncByType< T >(Type type, out T func) where T : class
        {
            if (mPushFuncWithType == null)
            {
                mPushFuncWithType = new Dictionary< Type, Delegate >()
                {
                    { typeof(int),      new Action< ULuaState, int >(ULuaAPI.luaex_pushinteger) },
                    { typeof(double),   new Action< ULuaState, double >(ULuaAPI.lua_pushnumber) },
                    { typeof(string),   new Action< ULuaState, string >(ULuaAPI.lua_pushstring) },
                    { typeof(byte[]),   new Action< ULuaState, byte[] >(ULuaAPI.lua_pushstring) },
                    { typeof(bool),     new Action< ULuaState, bool >(ULuaAPI.lua_pushboolean) },
                    { typeof(long),     new Action< ULuaState, long >(ULuaAPI.lua_pushint64) },
                    { typeof(ulong),    new Action< ULuaState, ulong >(ULuaAPI.lua_pushuint64) },
                    { typeof(IntPtr),   new Action< ULuaState, IntPtr >(ULuaAPI.lua_pushlightuserdata) },
                    { typeof(decimal),  new Action< ULuaState, decimal >(PushDecimal) },
                    { typeof(byte),     new Action< ULuaState, byte >((L, v) => ULuaAPI.luaex_pushinteger(L, v)) },
                    { typeof(sbyte),    new Action< ULuaState, sbyte >((L, v) => ULuaAPI.luaex_pushinteger(L, v)) },
                    { typeof(char),     new Action< ULuaState, char >((L, v) => ULuaAPI.luaex_pushinteger(L, v)) },
                    { typeof(short),    new Action< ULuaState, short >((L, v) => ULuaAPI.luaex_pushinteger(L, v)) },
                    { typeof(ushort),   new Action< ULuaState, ushort >((L, v) => ULuaAPI.luaex_pushinteger(L, v)) },
                    { typeof(uint),     new Action< ULuaState, uint >(ULuaAPI.luaex_pushuint) },
                    { typeof(float),    new Action< ULuaState, float >((L, v) => ULuaAPI.lua_pushnumber(L, v)) },
                };
            }

            Delegate obj;
            if (mPushFuncWithType.TryGetValue(type, out obj))
            {
                func = obj as T;

                return true;
            }

            func = null;

            return false;
        }

        bool TryGetGetFuncByType< T >(Type type, out T func) where T : class
        {
            if (mGetFuncWithType == null)
            {
                mGetFuncWithType = new Dictionary< Type, Delegate >()
                {
                    { typeof(int),      new Func< ULuaState, int, int >(ULuaAPI.luaex_tointeger) },
                    { typeof(double),   new Func< ULuaState, int, double >(ULuaAPI.lua_tonumber) },
                    { typeof(string),   new Func< ULuaState, int, string >(ULuaAPI.lua_tostring) },
                    { typeof(byte[]),   new Func< ULuaState, int, byte[] >(ULuaAPI.lua_tobytes) },
                    { typeof(bool),     new Func< ULuaState, int, bool >(ULuaAPI.lua_toboolean) },
                    { typeof(long),     new Func< ULuaState, int, long >(ULuaAPI.lua_toint64) },
                    { typeof(ulong),    new Func< ULuaState, int, ulong >(ULuaAPI.lua_touint64) },
                    { typeof(IntPtr),   new Func< ULuaState, int, IntPtr >(ULuaAPI.lua_touserdata) },
                    { typeof(decimal),  new Func< ULuaState, int, decimal >((L, idx) => {
                        decimal ret;
                        Get(L, idx, out ret);

                        return ret;
                    }) },
                    { typeof(byte),     new Func< ULuaState, int, byte >((L, idx) => (byte)ULuaAPI.luaex_tointeger(L, idx) ) },
                    { typeof(sbyte),    new Func< ULuaState, int, sbyte >((L, idx) => (sbyte)ULuaAPI.luaex_tointeger(L, idx) ) },
                    { typeof(char),     new Func< ULuaState, int, char >((L, idx) => (char)ULuaAPI.luaex_tointeger(L, idx) ) },
                    { typeof(short),    new Func< ULuaState, int, short >((L, idx) => (short)ULuaAPI.luaex_tointeger(L, idx) ) },
                    { typeof(ushort),   new Func< ULuaState, int, ushort >((L, idx) => (ushort)ULuaAPI.luaex_tointeger(L, idx) ) },
                    { typeof(uint),     new Func< ULuaState, int, uint >(ULuaAPI.luaex_touint) },
                    { typeof(float),    new Func< ULuaState, int, float >((L, idx) => (float)ULuaAPI.lua_tonumber(L, idx) ) },
                };
            }

            Delegate obj;
            if (mGetFuncWithType.TryGetValue(type, out obj))
            {
                func = obj as T;

                return true;
            }

            func = null;

            return false;
        }

        public void RegisterPushAndGetAndUpdate< T >(Action< ULuaState, T > push, GetFunc< T > getC, Action< ULuaState, int, T > update)
        {
            Type type = typeof(T);

            Action< ULuaState, T > org_push; Func< ULuaState, int, T > org_get;
            if (TryGetPushFuncByType(type, out org_push) || TryGetGetFuncByType(type, out org_get))
                throw new InvalidOperationException("push or get of " + type + " has register!");

            mPushFuncWithType.Add(type, push);
            mGetFuncWithType.Add(type, new Func< ULuaState, int, T >((L, idx) =>
            {
                T ret;
                getC(L, idx, out ret);

                return ret;
            }));

            RegisterCustomOP(type, 
                (ULuaState L, object obj) =>
                {
                    push(L, (T)obj);
                },
                (ULuaState L, int idx) =>
                {
                    T val;
                    getC(L, idx, out val);

                    return val;
                },
                (ULuaState L, int idx, object obj) =>
                {
                    update(L, idx, (T)obj);
                }
            );
        }

        public void RegisterCaster< T >(GetFunc< T > getC)
        {
            mObjectCaster.AddCaster(typeof(T), (L, idx, o) =>
            {
                T obj;
                getC(L, idx, out obj);

                return obj;
            });
        }

        public void PushDecimal(ULuaState L, decimal val)
        {
            if (mDecimalTypeId == -1)
            {
                bool is_first;

                mDecimalTypeId = GetTypeId(L, typeof(decimal), out is_first);
            }

            IntPtr buff = ULuaAPI.luaex_pushstruct(L, 16, mDecimalTypeId);
            if (!UEncoder.Encode(buff, 0, val))
                throw new Exception("pack fail for decimal ,value=" + val);
            
        }

        public bool IsDecimal(ULuaState L, int index)
        {
            if (mDecimalTypeId == -1)
                return false;

            return ULuaAPI.luaex_gettypeid(L, index) == mDecimalTypeId;
        }

        public void Get(ULuaState L, int index, out decimal val)
        {
            ELuaTypes lua_type = ULuaAPI.lua_type(L, index);
            if (lua_type == ELuaTypes.LUA_TUSERDATA)
            {
                if (ULuaAPI.luaex_gettypeid(L, index) != mDecimalTypeId)
                    throw new Exception("invalid user data for decimal!");

                IntPtr buff = ULuaAPI.lua_touserdata(L, index);

                if (!UEncoder.Decode(buff, 0, out val))
                    throw new Exception("unpack decimal fail!");
            } else if(lua_type == ELuaTypes.LUA_TNUMBER)
            {
                if (ULuaAPI.lua_isint64(L, index))
                    val = (decimal)ULuaAPI.lua_toint64(L, index);
                else
                    val = (decimal)ULuaAPI.lua_tonumber(L, index); // has gc
            } else
            {
                throw new Exception("invalid lua value for decimal, LuaType = " + lua_type);
            }
        }

        int UnityEngineVector2_TypeID = -1;
        public void Push(ULuaState L, UnityEngine.Vector2 val)
        {
            if (UnityEngineVector2_TypeID == -1)
            {
                bool is_first;

                UnityEngineVector2_TypeID = GetTypeId(L, typeof(UnityEngine.Vector2), out is_first);
            }

            IntPtr buff = ULuaAPI.luaex_pushstruct(L, 8, UnityEngineVector2_TypeID);
            if (!UEncoder.Encode(buff, 0, val))
                throw new Exception("encode fail fail for UnityEngine.Vector2, value = " + val);
        }

        public void Get(ULuaState L, int index, out UnityEngine.Vector2 val)
        {
            ELuaTypes type = ULuaAPI.lua_type(L, index);
            if (type == ELuaTypes.LUA_TUSERDATA)
            {
                if (ULuaAPI.luaex_gettypeid(L, index) != UnityEngineVector2_TypeID)
                    throw new Exception("invalid user data for UnityEngine.Vector2");

                IntPtr buff = ULuaAPI.lua_touserdata(L, index);
                if (!UEncoder.Decode(buff, 0, out val))
                    throw new Exception("decode fail for UnityEngine.Vector2");
            } else if (type == ELuaTypes.LUA_TTABLE)
            {
                UEncoder.Decode(this, L, index, out val);
            } else
            {
                val = (UnityEngine.Vector2)mObjectCaster.GetCaster(typeof(UnityEngine.Vector2))(L, index, null);
            }
        }

        public void Update(ULuaState L, int index, UnityEngine.Vector2 val)
        {
            if (ULuaAPI.lua_type(L, index) == ELuaTypes.LUA_TUSERDATA)
            {
                if (ULuaAPI.luaex_gettypeid(L, index) != UnityEngineVector2_TypeID)
                    throw new Exception("invalid user data for UnityEngine.Vector2");

                IntPtr buff = ULuaAPI.lua_touserdata(L, index);
                if (!UEncoder.Encode(buff, 0, val))
                    throw new Exception("encode fail for UnityEngine.Vector2, value = " + val);
            } else
            {
                throw new Exception("try to update a data with lua type:" + ULuaAPI.lua_type(L, index));
            }
        }

        int UnityEngineVector3_TypeID = -1;
        public void Push(ULuaState L, UnityEngine.Vector3 val)
        {
            if (UnityEngineVector3_TypeID == -1)
            {
                bool is_first;

                UnityEngineVector3_TypeID = GetTypeId(L, typeof(UnityEngine.Vector3), out is_first);
            }

            IntPtr buff = ULuaAPI.luaex_pushstruct(L, 12, UnityEngineVector3_TypeID);
            if (!UEncoder.Encode(buff, 0, val))
                throw new Exception("encode fail fail for UnityEngine.Vector3, value = " + val);
        }

        public void Get(ULuaState L, int index, out UnityEngine.Vector3 val)
        {
            ELuaTypes type = ULuaAPI.lua_type(L, index);
            if (type == ELuaTypes.LUA_TUSERDATA)
            {
                if (ULuaAPI.luaex_gettypeid(L, index) != UnityEngineVector3_TypeID)
                    throw new Exception("invalid user data for UnityEngine.Vector3");

                IntPtr buff = ULuaAPI.lua_touserdata(L, index);
                if (!UEncoder.Decode(buff, 0, out val))
                    throw new Exception("decode fail for UnityEngine.Vector3");
            } else if (type == ELuaTypes.LUA_TTABLE)
            {
                UEncoder.Decode(this, L, index, out val);
            } else
            {
                val = (UnityEngine.Vector3)mObjectCaster.GetCaster(typeof(UnityEngine.Vector3))(L, index, null);
            }
        }

        public void Update(ULuaState L, int index, UnityEngine.Vector3 val)
        {
            if (ULuaAPI.lua_type(L, index) == ELuaTypes.LUA_TUSERDATA)
            {
                if (ULuaAPI.luaex_gettypeid(L, index) != UnityEngineVector3_TypeID)
                    throw new Exception("invalid user data for UnityEngine.Vector3");

                IntPtr buff = ULuaAPI.lua_touserdata(L, index);
                if (!UEncoder.Encode(buff, 0, val))
                    throw new Exception("encode fail for UnityEngine.Vector3, value = " + val);
            } else
            {
                throw new Exception("try to update a data with lua type:" + ULuaAPI.lua_type(L, index));
            }
        }

        int UnityEngineVector4_TypeID = -1;
        public void Push(ULuaState L, UnityEngine.Vector4 val)
        {
            if (UnityEngineVector4_TypeID == -1)
            {
                bool is_first;

                UnityEngineVector4_TypeID = GetTypeId(L, typeof(UnityEngine.Vector4), out is_first);
            }

            IntPtr buff = ULuaAPI.luaex_pushstruct(L, 16, UnityEngineVector4_TypeID);
            if (!UEncoder.Encode(buff, 0, val))
                throw new Exception("encode fail fail for UnityEngine.Vector4, value = " + val);
        }

        public void Get(ULuaState L, int index, out UnityEngine.Vector4 val)
        {
            ELuaTypes type = ULuaAPI.lua_type(L, index);
            if (type == ELuaTypes.LUA_TUSERDATA)
            {
                if (ULuaAPI.luaex_gettypeid(L, index) != UnityEngineVector4_TypeID)
                    throw new Exception("invalid user data for UnityEngine.Vector4");

                IntPtr buff = ULuaAPI.lua_touserdata(L, index);
                if (!UEncoder.Decode(buff, 0, out val))
                    throw new Exception("decode fail for UnityEngine.Vector4");
            } else if (type == ELuaTypes.LUA_TTABLE)
            {
                UEncoder.Decode(this, L, index, out val);
            } else
            {
                val = (UnityEngine.Vector4)mObjectCaster.GetCaster(typeof(UnityEngine.Vector4))(L, index, null);
            }
        }

        public void Update(ULuaState L, int index, UnityEngine.Vector4 val)
        {
            if (ULuaAPI.lua_type(L, index) == ELuaTypes.LUA_TUSERDATA)
            {
                if (ULuaAPI.luaex_gettypeid(L, index) != UnityEngineVector4_TypeID)
                    throw new Exception("invalid user data for UnityEngine.Vector4");

                IntPtr buff = ULuaAPI.lua_touserdata(L, index);
                if (!UEncoder.Encode(buff, 0, val))
                    throw new Exception("encode fail for UnityEngine.Vector4, value = " + val);
            } else
            {
                throw new Exception("try to update a data with lua type:" + ULuaAPI.lua_type(L, index));
            }
        }

        int UnityEngineColor_TypeID = -1;
        public void Push(ULuaState L, UnityEngine.Color val)
        {
            if (UnityEngineColor_TypeID == -1)
            {
                bool is_first;

                UnityEngineColor_TypeID = GetTypeId(L, typeof(UnityEngine.Color), out is_first);
            }

            IntPtr buff = ULuaAPI.luaex_pushstruct(L, 16, UnityEngineColor_TypeID);
            if (!UEncoder.Encode(buff, 0, val))
                throw new Exception("encode fail fail for UnityEngine.Color, value = " + val);
        }

        public void Get(ULuaState L, int index, out UnityEngine.Color val)
        {
            ELuaTypes type = ULuaAPI.lua_type(L, index);
            if (type == ELuaTypes.LUA_TUSERDATA)
            {
                if (ULuaAPI.luaex_gettypeid(L, index) != UnityEngineColor_TypeID)
                    throw new Exception("invalid user data for UnityEngine.Color");

                IntPtr buff = ULuaAPI.lua_touserdata(L, index);
                if (!UEncoder.Decode(buff, 0, out val))
                    throw new Exception("unpack fail for UnityEngine.Color");
            } else if (type == ELuaTypes.LUA_TTABLE)
            {
                UEncoder.Decode(this, L, index, out val);
            } else
            {
                val = (UnityEngine.Color)mObjectCaster.GetCaster(typeof(UnityEngine.Color))(L, index, null);
            }
        }

        public void Update(ULuaState L, int index, UnityEngine.Color val)
        {
            if (ULuaAPI.lua_type(L, index) == ELuaTypes.LUA_TUSERDATA)
            {
                if (ULuaAPI.luaex_gettypeid(L, index) != UnityEngineColor_TypeID)
                    throw new Exception("invalid user data for UnityEngine.Color");

                IntPtr buff = ULuaAPI.lua_touserdata(L, index);
                if (!UEncoder.Encode(buff, 0, val))
                    throw new Exception("encode fail for UnityEngine.Color, value = " + val);
            } else
            {
                throw new Exception("try to update a data with lua type:" + ULuaAPI.lua_type(L, index));
            }
        }

        int UnityEngineQuaternion_TypeID = -1;
        public void Push(ULuaState L, UnityEngine.Quaternion val)
        {
            if (UnityEngineQuaternion_TypeID == -1)
            {
                bool is_first;

                UnityEngineQuaternion_TypeID = GetTypeId(L, typeof(UnityEngine.Quaternion), out is_first);
            }

            IntPtr buff = ULuaAPI.luaex_pushstruct(L, 16, UnityEngineQuaternion_TypeID);
            if (!UEncoder.Encode(buff, 0, val))
                throw new Exception("encode fail fail for UnityEngine.Quaternion, value = " + val);
        }

        public void Get(ULuaState L, int index, out UnityEngine.Quaternion val)
        {
            ELuaTypes type = ULuaAPI.lua_type(L, index);
            if (type == ELuaTypes.LUA_TUSERDATA)
            {
                if (ULuaAPI.luaex_gettypeid(L, index) != UnityEngineQuaternion_TypeID)
                    throw new Exception("invalid user data for UnityEngine.Quaternion");

                IntPtr buff = ULuaAPI.lua_touserdata(L, index);
                if (!UEncoder.Decode(buff, 0, out val))
                    throw new Exception("decode fail for UnityEngine.Quaternion");
            } else if (type == ELuaTypes.LUA_TTABLE)
            {
                UEncoder.Decode(this, L, index, out val);
            } else
            {
                val = (UnityEngine.Quaternion)mObjectCaster.GetCaster(typeof(UnityEngine.Quaternion))(L, index, null);
            }
        }

        public void Update(ULuaState L, int index, UnityEngine.Quaternion val)
        {
            if (ULuaAPI.lua_type(L, index) == ELuaTypes.LUA_TUSERDATA)
            {
                if (ULuaAPI.luaex_gettypeid(L, index) != UnityEngineQuaternion_TypeID)
                    throw new Exception("invalid user data for UnityEngine.Quaternion");

                IntPtr buff = ULuaAPI.lua_touserdata(L, index);
                if (!UEncoder.Encode(buff, 0, val))
                    throw new Exception("encode fail for UnityEngine.Quaternion, value = " + val);
            } else
            {
                throw new Exception("try to update a data with lua type:" + ULuaAPI.lua_type(L, index));
            }
        }

        int UnityEngineRay_TypeID = -1;
        public void Push(ULuaState L, UnityEngine.Ray val)
        {
            if (UnityEngineRay_TypeID == -1)
            {
                bool is_first;

                UnityEngineRay_TypeID = GetTypeId(L, typeof(UnityEngine.Ray), out is_first);
            }

            IntPtr buff = ULuaAPI.luaex_pushstruct(L, 24, UnityEngineRay_TypeID);
            if (!UEncoder.Encode(buff, 0, val))
                throw new Exception("encode fail fail for UnityEngine.Ray, value = " + val);
        }

        public void Get(ULuaState L, int index, out UnityEngine.Ray val)
        {
            ELuaTypes type = ULuaAPI.lua_type(L, index);
            if (type == ELuaTypes.LUA_TUSERDATA)
            {
                if (ULuaAPI.luaex_gettypeid(L, index) != UnityEngineRay_TypeID)
                    throw new Exception("invalid user data for UnityEngine.Ray");

                IntPtr buff = ULuaAPI.lua_touserdata(L, index);
                if (!UEncoder.Decode(buff, 0, out val))
                    throw new Exception("decode fail for UnityEngine.Ray");
            } else if (type == ELuaTypes.LUA_TTABLE)
            {
                UEncoder.Decode(this, L, index, out val);
            } else
            {
                val = (UnityEngine.Ray)mObjectCaster.GetCaster(typeof(UnityEngine.Ray))(L, index, null);
            }
        }

        public void Update(ULuaState L, int index, UnityEngine.Ray val)
        {
            if (ULuaAPI.lua_type(L, index) == ELuaTypes.LUA_TUSERDATA)
            {
                if (ULuaAPI.luaex_gettypeid(L, index) != UnityEngineRay_TypeID)
                    throw new Exception("invalid user data for UnityEngine.Ray");

                IntPtr buff = ULuaAPI.lua_touserdata(L, index);
                if (!UEncoder.Encode(buff, 0, val))
                    throw new Exception("encode fail for UnityEngine.Ray, value = " + val);
            } else
            {
                throw new Exception("try to update a data with lua type:" + ULuaAPI.lua_type(L, index));
            }
        }

        int UnityEngineBounds_TypeID = -1;
        public void Push(ULuaState L, UnityEngine.Bounds val)
        {
            if (UnityEngineBounds_TypeID == -1)
            {
                bool is_first;

                UnityEngineBounds_TypeID = GetTypeId(L, typeof(UnityEngine.Bounds), out is_first);
            }

            IntPtr buff = ULuaAPI.luaex_pushstruct(L, 24, UnityEngineBounds_TypeID);
            if (!UEncoder.Encode(buff, 0, val))
                throw new Exception("encode fail fail for UnityEngine.Bounds, value = " + val);
        }

        public void Get(ULuaState L, int index, out UnityEngine.Bounds val)
        {
            ELuaTypes type = ULuaAPI.lua_type(L, index);
            if (type == ELuaTypes.LUA_TUSERDATA)
            {
                if (ULuaAPI.luaex_gettypeid(L, index) != UnityEngineBounds_TypeID)
                    throw new Exception("invalid user data for UnityEngine.Bounds");

                IntPtr buff = ULuaAPI.lua_touserdata(L, index);
                if (!UEncoder.Decode(buff, 0, out val))
                    throw new Exception("decode fail for UnityEngine.Bounds");
            } else if (type == ELuaTypes.LUA_TTABLE)
            {
                UEncoder.Decode(this, L, index, out val);
            } else
            {
                val = (UnityEngine.Bounds)mObjectCaster.GetCaster(typeof(UnityEngine.Bounds))(L, index, null);
            }
        }

        public void Update(ULuaState L, int index, UnityEngine.Bounds val)
        {
            if (ULuaAPI.lua_type(L, index) == ELuaTypes.LUA_TUSERDATA)
            {
                if (ULuaAPI.luaex_gettypeid(L, index) != UnityEngineBounds_TypeID)
                    throw new Exception("invalid user data for UnityEngine.Bounds");

                IntPtr buff = ULuaAPI.lua_touserdata(L, index);
                if (!UEncoder.Encode(buff, 0, val))
                    throw new Exception("encode fail for UnityEngine.Bounds, value = " + val);
            } else
            {
                throw new Exception("try to update a data with lua type:" + ULuaAPI.lua_type(L, index));
            }
        }

        int UnityEngineRay2D_TypeID = -1;
        public void Push(ULuaState L, UnityEngine.Ray2D val)
        {
            if (UnityEngineRay2D_TypeID == -1)
            {
                bool is_first;

                UnityEngineRay2D_TypeID = GetTypeId(L, typeof(UnityEngine.Ray2D), out is_first);
            }

            IntPtr buff = ULuaAPI.luaex_pushstruct(L, 16, UnityEngineRay2D_TypeID);
            if (!UEncoder.Encode(buff, 0, val))
                throw new Exception("encode fail fail for UnityEngine.Ray2D, value = " + val);
        }

        public void Get(ULuaState L, int index, out UnityEngine.Ray2D val)
        {
            ELuaTypes type = ULuaAPI.lua_type(L, index);
            if (type == ELuaTypes.LUA_TUSERDATA)
            {
                if (ULuaAPI.luaex_gettypeid(L, index) != UnityEngineRay2D_TypeID)
                    throw new Exception("invalid user data for UnityEngine.Ray2D");

                IntPtr buff = ULuaAPI.lua_touserdata(L, index);
                if (!UEncoder.Decode(buff, 0, out val))
                    throw new Exception("decode fail for UnityEngine.Ray2D");
            } else if (type == ELuaTypes.LUA_TTABLE)
            {
                UEncoder.Decode(this, L, index, out val);
            } else
            {
                val = (UnityEngine.Ray2D)mObjectCaster.GetCaster(typeof(UnityEngine.Ray2D))(L, index, null);
            }
        }

        public void Update(ULuaState L, int index, UnityEngine.Ray2D val)
        {
            if (ULuaAPI.lua_type(L, index) == ELuaTypes.LUA_TUSERDATA)
            {
                if (ULuaAPI.luaex_gettypeid(L, index) != UnityEngineRay2D_TypeID)
                    throw new Exception("invalid user data for UnityEngine.Ray2D");

                IntPtr buff = ULuaAPI.lua_touserdata(L, index);
                if (!UEncoder.Encode(buff, 0, val))
                    throw new Exception("encode fail for UnityEngine.Ray2D, value = " + val);
            } else
            {
                throw new Exception("try to update a data with lua type:" + ULuaAPI.lua_type(L, index));
            }
        }
    }
}
