using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using ULuaState = System.IntPtr;

namespace UEngine.ULua
{
    public class ULuaTable : ULuaBase
    {
        public ULuaTable(int reference, ULuaEnv env) : base(reference, env)
        {
        }

        public void Get< TKey, TValue >(TKey key, out TValue value)
        {
            var _L = L;

            var parser = mEnv.mParser;

            int oldTop = ULuaAPI.lua_gettop(_L);

            ULuaAPI.lua_getref(_L, mLuaReference);
            parser.PushByType(_L, key);

            if (0 != ULuaAPI.luaex_pgettable(_L, -2))
            {
                string err = ULuaAPI.lua_tostring(_L, -1);

                ULuaAPI.lua_settop(_L, oldTop);

                throw new Exception("get field [" + key + "] error:" + err);
            }

            Type type_of_value = typeof(TValue);

            ELuaTypes lua_type = ULuaAPI.lua_type(_L, -1);
            if (lua_type == ELuaTypes.LUA_TNIL && type_of_value.IsValueType)
                throw new InvalidCastException("can not assign nil to " + type_of_value);

            try
            {
                parser.Get(_L, -1, out value);
            } catch (Exception e)
            {
                throw e;
            } finally
            {
                ULuaAPI.lua_settop(_L, oldTop);
            }
        }

        public void Set< TKey, TValue >(TKey key, TValue value)
        {
            var _L = L;

            int oldTop = ULuaAPI.lua_gettop(_L);

            var parser = mEnv.mParser;

            ULuaAPI.lua_getref(_L, mLuaReference);
            parser.PushByType(_L, key);
            parser.PushByType(_L, value);

            if (0 != ULuaAPI.luaex_psettable(_L, -3))
                mEnv.ThrowExceptionFromError(oldTop);
            ULuaAPI.lua_settop(_L, oldTop);
        }

        public T GetInPath< T >(string path)
        {
            var _L = L;

            var parser = mEnv.mParser;

            int oldTop = ULuaAPI.lua_gettop(_L);

            ULuaAPI.lua_getref(_L, mLuaReference);
            if (0 != ULuaAPI.luaex_pgettable_bypath(_L, -1, path))
                mEnv.ThrowExceptionFromError(oldTop);
            ELuaTypes lua_type = ULuaAPI.lua_type(_L, -1);
            if (lua_type == ELuaTypes.LUA_TNIL && typeof(T).IsValueType)
                throw new InvalidCastException("can not assign nil to " + typeof(T));

            T value;
            try
            {
                parser.Get(_L, -1, out value);
            } catch (Exception e)
            {
                throw e;
            } finally
            {
                ULuaAPI.lua_settop(_L, oldTop);
            }

            return value;
        }

        public void SetInPath< T >(string path, T val)
        {
            var _L = L;

            int oldTop = ULuaAPI.lua_gettop(_L);

            ULuaAPI.lua_getref(_L, mLuaReference);
            mEnv.mParser.PushByType(_L, val);
            if (0 != ULuaAPI.luaex_psettable_bypath(_L, -2, path))
                mEnv.ThrowExceptionFromError(oldTop);

            ULuaAPI.lua_settop(_L, oldTop);
        }

        public void ForEach< TKey, TValue >(Action< TKey, TValue > action)
        {
            var _L = L;

            var parser = mEnv.mParser;

            int oldTop = ULuaAPI.lua_gettop(_L);
            try
            {
                ULuaAPI.lua_getref(_L, mLuaReference);
                ULuaAPI.lua_pushnil(_L);
                while (ULuaAPI.lua_next(_L, -2) != 0)
                {
                    if (parser.Assignable< TKey >(_L, -2))
                    {
                        TKey key;
                        TValue val;
                        parser.Get(_L, -2, out key);
                        parser.Get(_L, -1, out val);
                        action(key, val);
                    }
                    ULuaAPI.lua_pop(_L, 1);
                }
            } finally
            {
                ULuaAPI.lua_settop(_L, oldTop);
            }
        }

        public int Length
        {
            get
            {
                var _L = L;

                int oldTop = ULuaAPI.lua_gettop(_L);

                ULuaAPI.lua_getref(_L, mLuaReference);
                var len = (int)ULuaAPI.luaex_objlen(_L, -1);
                ULuaAPI.lua_settop(_L, oldTop);

                return len;
            }
        }

        public IEnumerable GetKeys()
        {
            var _L = L;

            var parser = mEnv.mParser;

            int oldTop = ULuaAPI.lua_gettop(_L);

            ULuaAPI.lua_getref(_L, mLuaReference);
            ULuaAPI.lua_pushnil(_L);
            while (ULuaAPI.lua_next(_L, -2) != 0)
            {
                yield return parser.GetObject(_L, -2);

                ULuaAPI.lua_pop(_L, 1);
            }
            ULuaAPI.lua_settop(_L, oldTop);
        }

        public IEnumerable< T > GetKeys< T >()
        {
            var _L = L;

            var parser = mEnv.mParser;

            int oldTop = ULuaAPI.lua_gettop(_L);

            ULuaAPI.lua_getref(_L, mLuaReference);
            ULuaAPI.lua_pushnil(_L);
            while (ULuaAPI.lua_next(_L, -2) != 0)
            {
                if (parser.Assignable< T >(_L, -2))
                {
                    T v;
                    parser.Get(_L, -2, out v);

                    yield return v;
                }
                ULuaAPI.lua_pop(_L, 1);
            }
            ULuaAPI.lua_settop(_L, oldTop);
        }

        public TValue Get< TKey, TValue >(TKey key)
        {
            TValue ret;
            Get(key, out ret);

            return ret;
        }

        public TValue Get< TValue >(string key)
        {
            TValue ret;
            Get(key, out ret);

            return ret;
        }

        public void SetMetaTable(ULuaTable metaTable)
        {
            var _L = L;

            Push(_L);
            metaTable.Push(_L);
            ULuaAPI.lua_setmetatable(_L, -2);
            ULuaAPI.lua_pop(_L, 1);
        }

        public T Cast< T >()
        {
            var _L = L;

            var parser = mEnv.mParser;

            Push(_L);
                T ret = (T)parser.GetObject(_L, -1, typeof(T));
            ULuaAPI.lua_pop(_L, 1);

            return ret;
        }

        internal override void Push(ULuaState L)
        {
            ULuaAPI.lua_getref(L, mLuaReference);
        }

        public override string ToString()
        {
            return "table :" + mLuaReference;
        }
    }
}
