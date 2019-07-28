using System;
using System.Collections.Generic;
using System.Reflection;
using ULuaState = System.IntPtr;

using UnityEngine;

namespace UEngine.ULua
{
    public class ULuaEnv : IDisposable
    {
        public delegate byte[] CustomLoader(ref string filepath);

        internal struct GCAction
        {
            public int  Reference;
            public bool IsDelegate;
        }
        Queue< GCAction >           mRefQueue = new Queue< GCAction >();
        internal ULuaState          mRawL;
        internal UObjectParser      mParser;
        internal int                mErrorFuncRef = -1;
        private ULuaTable           _G;
        private static List< Action< ULuaEnv, UObjectParser > >
                                    mIniters = null;
        internal List< CustomLoader >
                                    mCustomLoaders = new List< CustomLoader >();
        private bool                mDisposed = false;
        internal Dictionary< string, lua_CSFunction >
                                    mBuildinIniter = new Dictionary< string, lua_CSFunction >();

        public ULuaState L
        {
            get
            {
                if (mRawL == ULuaState.Zero)
                    throw new InvalidOperationException("this lua env had disposed!");

                return mRawL;
            }
        }

        public UObjectParser Parser
        {
            get
            {
                return mParser;
            }
        }

        public int ErrorFuncRef
        {
            get
            {
                return mErrorFuncRef;
            }
        }

        public Action<string> ErrorCallback
        { get; set; }

        internal HashSet< string > mLuaFileCache = null;
        internal Dictionary< string, ULuaBase > mLuaResCache = null;

        internal lua_CSFunction mTracebackFunction = null;

        const int kULuaLIB_VERSION_EXPECT = 102;
        const string kInitLuaex = @" 
            local metatable = { };
            local rawget = rawget;
            local setmetatable = setmetatable;
            local import_type = luaex.import_type;
            local load_assembly = luaex.load_assembly;

            function metatable:__index(key) 
                local fqn = rawget(self, '.fqn');
                fqn = ((fqn and fqn .. '.') or '') .. key;

                local obj = import_type(fqn);

                if obj == nil then
                    obj = { ['.fqn'] = fqn; }
                    setmetatable(obj, metatable);
                elseif obj == true then
                    return rawget(self, key);
                end

                rawset(self, key, obj);

                return obj;
            end

            function metatable:__call(...)
                error('no such type: ' .. rawget(self,'.fqn'), 2);
            end

            CS = CS or { };
            setmetatable(CS, metatable);

            typeof = function(t) return t.UnderlyingSystemType; end
            cast = luaex.cast;
            if not setfenv or not getfenv then
                local function getfunction(level)
                    local info = debug.getinfo(level + 1, 'f');

                    return info and info.func;
                end

                function setfenv(fn, env)
                  if type(fn) == 'number' then
                    fn = getfunction(fn + 1);
                  end

                  local i = 1;
                  while true do
                    local name = debug.getupvalue(fn, i);
                    if name == '_ENV' then
                      debug.upvaluejoin(fn, i, (function()
                        return env;
                      end), 1);

                      break;
                    elseif not name then
                      break;
                    end

                    i = i + 1;
                  end

                  return fn;
                end

                function getfenv(fn)
                  if type(fn) == 'number' then
                    fn = getfunction(fn + 1);
                  end

                  local i = 1;
                  while true do
                    local name, val = debug.getupvalue(fn, i);
                    if name == '_ENV' then
                      return val;
                    elseif not name then
                      break;
                    end

                    i = i + 1;
                  end
                end
            end
            ";

        [UMonoPInvokeCallbackAttribute(typeof(lua_CSFunction))]
        public static int Traceback(ULuaState L)
        {
            ULuaAPI.lua_getglobal(L, "debug");
            ULuaAPI.lua_getfield(L, -1, "traceback");
            ULuaAPI.lua_pushvalue(L, 1);
            ULuaAPI.lua_pushnumber(L, 2);
            ULuaAPI.lua_pcall(L, 2, 1, 0);

            return 1;
        }

        public string GetStack()
        {
            ULuaAPI.lua_getglobal(L, "i3k_global");
            ULuaAPI.lua_getfield(L, -1, "traceback");
            ULuaAPI.lua_pcall(L, 0, 1, 0);
            string result = ULuaAPI.lua_tostring(L, -1);
            ULuaAPI.lua_pop(L, 1);
            return result;
        }

        public ULuaEnv(bool mono = true)
        {
            if (ULuaAPI.luaex_get_lib_version() != kULuaLIB_VERSION_EXPECT)
                throw new InvalidProgramException("wrong lib version expect:" + kULuaLIB_VERSION_EXPECT + " but got:" + ULuaAPI.luaex_get_lib_version());

            ULuaIndexes.LUA_REGISTRYINDEX = ULuaAPI.luaex_get_registry_index();

            // create state
            mRawL = ULuaAPI.luaL_newstate();

            // init extent libs
            ULuaAPI.luaopen_luaex(mRawL);
            ULuaAPI.luaopen_i64lib(mRawL);

            mLuaFileCache = new HashSet< string >();
            mLuaResCache = new Dictionary< string, ULuaBase >();

            mTracebackFunction = new lua_CSFunction(Traceback);

            mParser = new UObjectParser(this, mRawL);
            mParser.CreateFunctionMetatable(mRawL);
            mParser.OpenLib(mRawL);
            UObjectParserPool.Instance.Add(mRawL, mParser);

            ULuaAPI.lua_atpanic(mRawL, ULuaCallbacks.Panic);

            ULuaAPI.lua_pushstdcallcfunction(mRawL, ULuaCallbacks.Print);
            if (0 != ULuaAPI.luaex_setglobal(mRawL, "print"))
                throw new Exception("call luaex_setglobal fail!");

            UCodeGen.OpenLib(mRawL);

            AddSearcher(ULuaCallbacks.LoadBuiltinLib, 2);
            AddSearcher(ULuaCallbacks.LoadFromCustomLoaders, 3);

            AddSearcher(ULuaCallbacks.LoadFromResource, 4);
            AddSearcher(ULuaCallbacks.LoadFromStreamingAssetsPath, 5);

            DoString(kInitLuaex, "Init");

            AddBuildin("socket.core", ULuaCallbacks.LoadSocketCore);
            AddBuildin("socket", ULuaCallbacks.LoadSocketCore);

            ULuaAPI.lua_newtable(mRawL);
            ULuaAPI.luaex_pushasciistring(mRawL, "__index");
            ULuaAPI.lua_pushstdcallcfunction(mRawL, ULuaCallbacks.MetaFuncIndex);
            ULuaAPI.lua_rawset(mRawL, -3);

            ULuaAPI.luaex_pushasciistring(mRawL, Utils.kLuaIndexFieldName);
            ULuaAPI.lua_newtable(mRawL);
            ULuaAPI.lua_pushvalue(mRawL, -3);
            ULuaAPI.lua_setmetatable(mRawL, -2);
            ULuaAPI.lua_rawset(mRawL, ULuaIndexes.LUA_REGISTRYINDEX);

            ULuaAPI.luaex_pushasciistring(mRawL, Utils.kLuaNewIndexFieldName);
            ULuaAPI.lua_newtable(mRawL);
            ULuaAPI.lua_pushvalue(mRawL, -3);
            ULuaAPI.lua_setmetatable(mRawL, -2);
            ULuaAPI.lua_rawset(mRawL, ULuaIndexes.LUA_REGISTRYINDEX);

            ULuaAPI.luaex_pushasciistring(mRawL, Utils.kLuaClassIndexFieldName);
            ULuaAPI.lua_newtable(mRawL);
            ULuaAPI.lua_pushvalue(mRawL, -3);
            ULuaAPI.lua_setmetatable(mRawL, -2);
            ULuaAPI.lua_rawset(mRawL, ULuaIndexes.LUA_REGISTRYINDEX);

            ULuaAPI.luaex_pushasciistring(mRawL, Utils.kLuaClassNewIndexFieldName);
            ULuaAPI.lua_newtable(mRawL);
            ULuaAPI.lua_pushvalue(mRawL, -3);
            ULuaAPI.lua_setmetatable(mRawL, -2);
            ULuaAPI.lua_rawset(mRawL, ULuaIndexes.LUA_REGISTRYINDEX);

            ULuaAPI.lua_pop(mRawL, 1); // pop metatable of index and new index functions

            ULuaAPI.luaex_pushasciistring(mRawL, "luaex_main_thread");
            ULuaAPI.lua_pushthread(mRawL);
            ULuaAPI.lua_rawset(mRawL, ULuaIndexes.LUA_REGISTRYINDEX);

            if (mono)
                mParser.Alias(typeof(Type), "System.MonoType");

            if (0 != ULuaAPI.luaex_getglobal(mRawL, "_G"))
                throw new Exception("call luaex_getglobal fail!");
            mParser.Get(mRawL, -1, out _G);

            ULuaAPI.lua_pop(mRawL, 1);

            mErrorFuncRef = ULuaAPI.luaex_get_error_func_ref(mRawL);

            if (mIniters != null)
            {
                for (int i = 0; i < mIniters.Count; i ++)
                    mIniters[i](this, mParser);
            }

            mParser.CreateArrayMetatable(mRawL);
            mParser.CreateDelegateMetatable(mRawL);
        }

        public static void AddIniter(Action< ULuaEnv, UObjectParser > initer)
        {
            if (mIniters == null)
                mIniters = new List< Action< ULuaEnv, UObjectParser > >();

            mIniters.Add(initer);
        }

        public ULuaTable Global
        {
            get
            {
                return _G;
            }
        }

        public T LoadString< T >(byte[] chunk, string name = "UEngine", ULuaTable env = null)
        {
            if (typeof(T) != typeof(ULuaFunction) && !typeof(T).IsSubclassOf(typeof(Delegate)))
                throw new InvalidOperationException(typeof(T).Name + " is not a delegate type nor LuaFunction");

            var _L = L;

            int oldTop = ULuaAPI.lua_gettop(_L);

            if (ULuaAPI.luaexL_loadbuffer(_L, chunk, chunk.Length, name) != 0)
                ThrowExceptionFromError(oldTop);

            if (env != null)
            {
                env.Push(_L);

                ULuaAPI.lua_setfenv(_L, -2);
            }

            T result = (T)mParser.GetObject(_L, -1, typeof(T));

            ULuaAPI.lua_settop(_L, oldTop);

            return result;
        }

        public T LoadString< T >(string chunk, string name = "UEngine", ULuaTable env = null)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(chunk);

            return LoadString< T >(bytes, name, env);
        }

        public ULuaFunction LoadString(string chunk, string name = "UEngine", ULuaTable env = null)
        {
            return LoadString< ULuaFunction >(chunk, name, env);
        }

        public object[] DoString(byte[] chunk, string name = "UEngine", ULuaTable env = null)
        {
            var _L = L;

            int oldTop = ULuaAPI.lua_gettop(_L);

            int errFunc = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);
            if (ULuaAPI.luaexL_loadbuffer(_L, chunk, chunk.Length, name) == 0)
            {
                if (env != null)
                {
                    env.Push(_L);

                    ULuaAPI.lua_setfenv(_L, -2);
                }

                if (ULuaAPI.lua_pcall(_L, 0, -1, errFunc) == 0)
                {
                    ULuaAPI.lua_remove(_L, errFunc);

                    return mParser.PopValues(_L, oldTop);
                } else
                {
                    ThrowExceptionFromError(oldTop);
                }
            } else
            {
                ThrowExceptionFromError(oldTop);
            }

            return null;
        }

        public object[] DoString(string chunk, string name = "UEngine", ULuaTable env = null)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(chunk);

            return DoString(bytes, name, env);
        }

        public bool DoFile(string fileName, out object[] objects, string name = "UEngine", ULuaTable env = null)
        {
            objects = null;

            if (!mLuaFileCache.Contains(fileName))
            {
                string text = UFileAccessor.ReadStringScript(fileName);
                if (text.Length > 0)
                {
					mLuaFileCache.Add(fileName);

                    objects = DoString(text, name, env);

                    return true;
                }

                return false;
            }

            return true;
        }

        public bool ReloadFile(string fileName, out object[] objects, string name = "UEngine", ULuaTable env = null)
        {
            objects = null;

            if (!mLuaFileCache.Contains(fileName))
                mLuaFileCache.Add(fileName);

            string text = UFileAccessor.ReadStringScript(fileName);
            if (text.Length > 0)
            {
                objects = DoString(text, name, env);

                return true;
            }

            return false;
        }

        private void AddSearcher(lua_CSFunction searcher, int index)
        {
            var _L = L;

            // insert the loader
            ULuaAPI.luaex_getloaders(_L);
            if (!ULuaAPI.lua_istable(_L, -1))
                throw new Exception("can not set searcher!");

            uint len = ULuaAPI.luaex_objlen(_L, -1);
            index = index < 0 ? (int)(len + index + 2) : index;
            for (int e = (int)len + 1; e > index; e --)
            {
                ULuaAPI.luaex_rawgeti(_L, -1, e - 1);
                ULuaAPI.luaex_rawseti(_L, -2, e);
            }
            ULuaAPI.lua_pushstdcallcfunction(_L, searcher);
            ULuaAPI.luaex_rawseti(_L, -2, index);
            ULuaAPI.lua_pop(_L, 1);
        }

        public void Alias(Type type, string alias)
        {
            mParser.Alias(type, alias);
        }

        int last_check_point = 0;

        int max_check_per_tick = 20;

        static bool ObjectValidCheck(object obj)
        {
            return (!(obj is UnityEngine.Object)) ||  ((obj as UnityEngine.Object) != null);
        }

        Func< object, bool > object_valid_checker = new Func< object, bool >(ObjectValidCheck);

        public void Tick()
        {
            var _L = L;

            lock (mRefQueue)
            {
                while (mRefQueue.Count > 0)
                {
                    GCAction gca = mRefQueue.Dequeue();

                    mParser.ReleaseLuaBase(_L, gca.Reference, gca.IsDelegate);
                }
            }
            last_check_point = mParser.mObjects.Check(last_check_point, max_check_per_tick, object_valid_checker, mParser.mReverseMap);
        }

        public void GC()
        {
            Tick();
        }

        public ULuaTable NewTable()
        {
            var _L = L;

            int oldTop = ULuaAPI.lua_gettop(_L);

            ULuaAPI.lua_newtable(_L);

            ULuaTable returnVal = (ULuaTable)mParser.GetObject(_L, -1, typeof(ULuaTable));

            ULuaAPI.lua_settop(_L, oldTop);

            return returnVal;
        }

        public void Dispose()
        {
            FullGC();

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            Dispose(true);

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }

        public virtual void Dispose(bool dispose)
        {
            if (mDisposed)
                return;

            Tick();

            if (!mParser.AllDelegateBridgeReleased())
                throw new InvalidOperationException("try to dispose a LuaEnv with C# callback!");

            ULuaAPI.lua_close(L);

            UObjectParserPool.Instance.Remove(L);
            if (mParser != null)
                mParser = null;

            mRawL = ULuaState.Zero;

            mDisposed = true;
        }

        public void ThrowExceptionFromError(int oldTop)
        {
            object err = mParser.GetObject(L, -1);
            ULuaAPI.lua_settop(L, oldTop);

            Exception ex = err as Exception;
            if (ex != null)
                throw ex;

            if (err == null)
                err = "Unknown Lua Error";

			string msg = err.ToString();
			if (string.IsNullOrEmpty(msg))
				msg = "unknown error:";

            if (null != ErrorCallback)
                ErrorCallback(msg);

			ULogger.ScriptError(msg);

            for (int i = 0; i < USystemConfig.Instance.Isthrow.Length; ++i)
            {
                bool isHave = msg.ToLower().Contains(USystemConfig.Instance.Isthrow[i].ToLower());
                if (isHave)
                    return;
            }
                  
            if (USystemConfig.Instance.IsClose)
            {
#pragma warning disable 0618
                if (!Application.isEditor)
                {
                    UnityEngine.Application.ForceCrash(0);
                }
#pragma warning restore 0618
                UnityEngine.Application.Quit();
            } else
            {
                throw new ULuaException(msg);
            }    
        }

        internal void EqueueGCAction(GCAction action)
        {
            lock (mRefQueue)
            {
                mRefQueue.Enqueue(action);
            }
        }

        public void AddLoader(CustomLoader loader)
        {
            mCustomLoaders.Add(loader);
        }

        public void AddBuildin(string name, lua_CSFunction initer)
        {
            if (!Utils.IsStaticPInvokeCSFunction(initer))
                throw new Exception("initer must be static and has MonoPInvokeCallback Attribute!");

            mBuildinIniter.Add(name, initer);
        }

		public int Memroy
		{
			get
			{
				return ULuaAPI.lua_gc(L, ELuaGCOptions.LUA_GCCOUNT, 0);
			}
		}

        public int GCPause
        {
            get
            {
                int val = ULuaAPI.lua_gc(L, ELuaGCOptions.LUA_GCSETPAUSE, 200);

                ULuaAPI.lua_gc(L, ELuaGCOptions.LUA_GCSETPAUSE, val);

                return val;
            }
            set
            {
                ULuaAPI.lua_gc(L, ELuaGCOptions.LUA_GCSETPAUSE, value);
            }
        }

		public int GCStep
		{
			set
			{
				ULuaAPI.lua_gc(L, ELuaGCOptions.LUA_GCSTEP, value);
			}
		}

        public int GCStepmul
        {
            get
            {
                int val = ULuaAPI.lua_gc(L, ELuaGCOptions.LUA_GCSETSTEPMUL, 200);

                ULuaAPI.lua_gc(L, ELuaGCOptions.LUA_GCSETSTEPMUL, val);

                return val;
            }
            set
            {
                ULuaAPI.lua_gc(L, ELuaGCOptions.LUA_GCSETSTEPMUL, value);
            }
        }

        public void FullGC()
        {
            ULuaAPI.lua_gc(L, ELuaGCOptions.LUA_GCCOLLECT, 0);
        }

        public void StopGC()
        {
            ULuaAPI.lua_gc(L, ELuaGCOptions.LUA_GCSTOP, 0);
        }

        public void RestartGC()
        {
            ULuaAPI.lua_gc(L, ELuaGCOptions.LUA_GCRESTART, 0);
        }
    }
}
