using System;
using System.Collections.Generic;

using ULuaState = System.IntPtr;

namespace UEngine.ULua
{
    public partial class ULuaFunction : ULuaBase
    {
        public ULuaFunction(int reference, ULuaEnv env) : base(reference, env)
        {
        }

        public void Action< T >(T a)
        {
            var _L = L;

            var parser = mEnv.mParser;

            int oldTop = ULuaAPI.lua_gettop(_L);
            int errFunc = ULuaAPI.luaex_load_error_func(_L, mEnv.mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            parser.PushByType(_L, a);
            int error = ULuaAPI.lua_pcall(_L, 1, 0, errFunc);
            if (error != 0)
                mEnv.ThrowExceptionFromError(oldTop);
            ULuaAPI.lua_settop(_L, oldTop);
        }

        public TResult Func< T, TResult >(T a)
        {
            var _L = L;

            var parser = mEnv.mParser;

            int oldTop = ULuaAPI.lua_gettop(_L);
            int errFunc = ULuaAPI.luaex_load_error_func(_L, mEnv.mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            parser.PushByType(_L, a);
            int error = ULuaAPI.lua_pcall(_L, 1, 1, errFunc);
            if (error != 0)
                mEnv.ThrowExceptionFromError(oldTop);
            TResult ret;
            try
            {
                parser.Get(_L, -1, out ret);
            } catch (Exception e)
            {
                throw e;
            } finally
            {
                ULuaAPI.lua_settop(_L, oldTop);
            }

            return ret;
        }

        public void Action< T1, T2 >(T1 a1, T2 a2)
        {
            var _L = L;

            var parser = mEnv.mParser;

            int oldTop = ULuaAPI.lua_gettop(_L);
            int errFunc = ULuaAPI.luaex_load_error_func(_L, mEnv.mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            parser.PushByType(_L, a1);
            parser.PushByType(_L, a2);
            int error = ULuaAPI.lua_pcall(_L, 2, 0, errFunc);
            if (error != 0)
                mEnv.ThrowExceptionFromError(oldTop);
            ULuaAPI.lua_settop(_L, oldTop);
        }

        public TResult Func< T1, T2, TResult >(T1 a1, T2 a2)
        {
            var _L = L;

            var parser = mEnv.mParser;

            int oldTop = ULuaAPI.lua_gettop(_L);
            int errFunc = ULuaAPI.luaex_load_error_func(_L, mEnv.mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            parser.PushByType(_L, a1);
            parser.PushByType(_L, a2);
            int error = ULuaAPI.lua_pcall(_L, 2, 1, errFunc);
            if (error != 0)
                mEnv.ThrowExceptionFromError(oldTop);
            TResult ret;
            try
            {
                parser.Get(_L, -1, out ret);
            } catch (Exception e)
            {
                throw e;
            } finally
            {
                ULuaAPI.lua_settop(_L, oldTop);
            }

            return ret;
        }

        public object[] Call(object[] args, Type[] returnTypes)
        {
            var _L = L;

            int nArgs = 0;
            var parser = mEnv.mParser;

            int oldTop = ULuaAPI.lua_gettop(_L);
            int errFunc = ULuaAPI.luaex_load_error_func(_L, mEnv.mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            if (args != null)
            {
                nArgs = args.Length;
                for (int i = 0; i < args.Length; i ++)
                    parser.PushAny(_L, args[i]);
            }
            int error = ULuaAPI.lua_pcall(_L, nArgs, -1, errFunc);
            if (error != 0)
                mEnv.ThrowExceptionFromError(oldTop);

            ULuaAPI.lua_remove(_L, errFunc);
            if (returnTypes != null)
                return parser.PopValues(_L, oldTop, returnTypes);
            else
                return parser.PopValues(_L, oldTop);
        }

        public object[] Call(params object[] args)
        {
            return Call(args, null);
        }

        public T Cast< T >()
        {
            if (!typeof(T).IsSubclassOf(typeof(Delegate)))
                throw new InvalidOperationException(typeof(T).Name + " is not a delegate type");

            var _L = L;

            var parser = mEnv.mParser;

            Push(L);
                T ret = (T)parser.GetObject(_L, -1, typeof(T));
            ULuaAPI.lua_pop(_L, 1);

            return ret;
        }

        public void SetEnv(ULuaTable env)
        {
            var _L = L;

            int oldTop = ULuaAPI.lua_gettop(_L);

            Push(_L);

            env.Push(_L);

            ULuaAPI.lua_setfenv(_L, -2);
            ULuaAPI.lua_settop(_L, oldTop);
        }

        internal override void Push(ULuaState L)
        {
            ULuaAPI.lua_getref(L, mLuaReference);
        }

        public override string ToString()
        {
            return "function :" + mLuaReference;
        }
    }
}
