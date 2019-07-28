using System;
using System.Collections.Generic;

using ULuaState = System.IntPtr;

namespace UEngine.ULua
{
    public abstract class UDelegateBridgeBase : ULuaBase
    {
        private Type                            mFirstKey = null;
        private Delegate                        mFirstValue = null;
        private Dictionary< Type, Delegate >    mBindTo = null;
        protected int                           mErrorFuncRef;

        public UDelegateBridgeBase(int reference, ULuaEnv env) : base(reference, env)
        {
            mErrorFuncRef = env.mErrorFuncRef;
        }

        public bool TryGetDelegate(Type key, out Delegate value)
        {
            if(key == mFirstKey)
            {
                value = mFirstValue;

                return true;
            }

            if (mBindTo != null)
                return mBindTo.TryGetValue(key, out value);

            value = null;

            return false;
        }

        public void AddDelegate(Type key, Delegate value)
        {
            if (key == mFirstKey)
                throw new ArgumentException("An element with the same key already exists in the dictionary.");

            if (mFirstKey == null && mBindTo == null) // nothing 
            {
                mFirstKey   = key;
                mFirstValue = value;
            } else if (mFirstKey != null && mBindTo == null) // one key existed
            {
                mBindTo = new Dictionary<Type, Delegate>();
                mBindTo.Add(mFirstKey, mFirstValue);

                mFirstKey   = null;
                mFirstValue = null;
                mBindTo.Add(key, value);
            } else
            {
                mBindTo.Add(key, value);
            }
        }

        public virtual Delegate GetDelegateByType(Type type)
        {
            throw new InvalidCastException("This delegate must add to CSharpCallLua: " + type);
        }
    }

    public partial class UDelegateBridge : UDelegateBridgeBase
    {
        public UDelegateBridge(int reference, ULuaEnv env) : base(reference, env)
        {
        }

        public void Delegate_Null()
        {
            var _L = L;

            int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);

            int call_error = ULuaAPI.lua_pcall(_L, 0, 0, err_func);
            if (call_error != 0)
                mEnv.ThrowExceptionFromError(err_func - 1);

            ULuaAPI.lua_settop(_L, err_func - 1);
        }

        public void Delegate_String(string p0)
        {
            var _L = L;

            int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            ULuaAPI.lua_pushstring(_L, p0);

            int call_error = ULuaAPI.lua_pcall(_L, 1, 0, err_func);
            if (call_error != 0)
                mEnv.ThrowExceptionFromError(err_func - 1);

            ULuaAPI.lua_settop(_L, err_func - 1);
        }

        public void Delegate_UInteger ( uint p0 )
        {
            var _L = L;

            int err_func = ULuaAPI.luaex_load_error_func( _L, mErrorFuncRef );

            ULuaAPI.lua_getref( _L, mLuaReference );
            ULuaAPI.lua_pushint64( _L, p0 );

            int call_error = ULuaAPI.lua_pcall( _L, 1, 0, err_func );
            if (call_error != 0)
                mEnv.ThrowExceptionFromError( err_func - 1 );

            ULuaAPI.lua_settop( _L, err_func - 1 );
        }

        public void Delegate_Integer(int p0)
        {
            var _L = L;

            int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            ULuaAPI.lua_pushint64(_L, p0);

            int call_error = ULuaAPI.lua_pcall(_L, 1, 0, err_func);
            if (call_error != 0)
                mEnv.ThrowExceptionFromError(err_func - 1);

            ULuaAPI.lua_settop(_L, err_func - 1);
        }

        public void Delegate_Integer(int p0, int p1)
        {
            var _L = L;

            int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            ULuaAPI.lua_pushint64(_L, p0);
            ULuaAPI.lua_pushint64(_L, p1);

            int call_error = ULuaAPI.lua_pcall(_L, 2, 0, err_func);
            if (call_error != 0)
                mEnv.ThrowExceptionFromError(err_func - 1);

            ULuaAPI.lua_settop(_L, err_func - 1);
        }

        public void Delegate_Single(float p0)
        {
            var _L = L;

            int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            ULuaAPI.lua_pushnumber(_L, p0);

            int call_error = ULuaAPI.lua_pcall(_L, 1, 0, err_func);
            if (call_error != 0)
                mEnv.ThrowExceptionFromError(err_func - 1);

            ULuaAPI.lua_settop(_L, err_func - 1);
        }

        public void Delegate_Single(float p0, float p1)
        {
            var _L = L;

            int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            ULuaAPI.lua_pushnumber(_L, p0);
            ULuaAPI.lua_pushnumber(_L, p1);

            int call_error = ULuaAPI.lua_pcall(_L, 2, 0, err_func);
            if (call_error != 0)
                mEnv.ThrowExceptionFromError(err_func - 1);

            ULuaAPI.lua_settop(_L, err_func - 1);
        }

        public void Delegate_Number(double p0)
        {
            var _L = L;

            int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            ULuaAPI.lua_pushnumber(_L, p0);

            int call_error = ULuaAPI.lua_pcall(_L, 1, 0, err_func);
            if (call_error != 0)
                mEnv.ThrowExceptionFromError(err_func - 1);

            ULuaAPI.lua_settop(_L, err_func - 1);
        }

        public void Delegate_Number(double p0, double p1)
        {
            var _L = L;

            int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            ULuaAPI.lua_pushnumber(_L, p0);
            ULuaAPI.lua_pushnumber(_L, p1);

            int call_error = ULuaAPI.lua_pcall(_L, 2, 0, err_func);
            if (call_error != 0)
                mEnv.ThrowExceptionFromError(err_func - 1);

            ULuaAPI.lua_settop(_L, err_func - 1);
        }

        public void Delegate_Boolean(bool p0)
        {
            var _L = L;

            int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            ULuaAPI.lua_pushboolean(_L, p0);

            int call_error = ULuaAPI.lua_pcall(_L, 1, 0, err_func);
            if (call_error != 0)
                mEnv.ThrowExceptionFromError(err_func - 1);

            ULuaAPI.lua_settop(_L, err_func - 1);
        }

        public void Delegate_Boolean(bool p0, bool p1)
        {
            var _L = L;

            int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            ULuaAPI.lua_pushboolean(_L, p0);
            ULuaAPI.lua_pushboolean(_L, p1);

            int call_error = ULuaAPI.lua_pcall(_L, 2, 0, err_func);
            if (call_error != 0)
                mEnv.ThrowExceptionFromError(err_func - 1);

            ULuaAPI.lua_settop(_L, err_func - 1);
        }

		public void Delegate_Boolean(bool p0, ULuaTable p1)
		{
			var _L = L;

			UObjectParser parser = UObjectParserPool.Instance.Find(L);

			int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

			ULuaAPI.lua_getref(_L, mLuaReference);
			ULuaAPI.lua_pushboolean(_L, p0);
			parser.Push(_L, p1);

			int call_error = ULuaAPI.lua_pcall(_L, 2, 0, err_func);
			if (call_error != 0)
				mEnv.ThrowExceptionFromError(err_func - 1);

			ULuaAPI.lua_settop(_L, err_func - 1);
		}

        public void Delegate_Vector3(UnityEngine.Vector3 p0)
        {
            var _L = L;

            UObjectParser parser = UObjectParserPool.Instance.Find(L);

            int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            parser.Push(_L, p0);

            int call_error = ULuaAPI.lua_pcall(_L, 1, 0, err_func);
            if (call_error != 0)
                mEnv.ThrowExceptionFromError(err_func - 1);

            ULuaAPI.lua_settop(_L, err_func - 1);
        }

        public void Delegate_GameObject(UnityEngine.GameObject p0)
        {
            var _L = L;

            UObjectParser parser = UObjectParserPool.Instance.Find(L);

            int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            parser.Push(_L, p0);
            //ULuaAPI.luaex_pushcsobj(_L, -1, p0, false, -1);

            int call_error = ULuaAPI.lua_pcall(_L, 1, 0, err_func);
            if (call_error != 0)
                mEnv.ThrowExceptionFromError(err_func - 1);

            ULuaAPI.lua_settop(_L, err_func - 1);
        }

        public void Delegate_object(int p0, object p1)
        {
            var _L = L;

            UObjectParser parser = UObjectParserPool.Instance.Find(L);

            int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            ULuaAPI.lua_pushnumber(_L, p0);
            parser.Push(_L, p1);

            int call_error = ULuaAPI.lua_pcall(_L, 2, 0, err_func);
            if (call_error != 0)
                mEnv.ThrowExceptionFromError(err_func - 1);

            ULuaAPI.lua_settop(_L, err_func - 1);
        }

        public void Delegate_GameObject ( int p0, object p1 )
        {
            var _L = L;

            UObjectParser parser = UObjectParserPool.Instance.Find( L );

            int err_func = ULuaAPI.luaex_load_error_func( _L, mErrorFuncRef );

            ULuaAPI.lua_getref( _L, mLuaReference );
            ULuaAPI.lua_pushnumber( _L, p0 );
            parser.Push( _L, p1 );

            int call_error = ULuaAPI.lua_pcall( _L, 2, 0, err_func );
            if (call_error != 0)
                mEnv.ThrowExceptionFromError( err_func - 1 );

            ULuaAPI.lua_settop( _L, err_func - 1 );
        }

        public void Delegate_Object(object p0)
        {
            var _L = L;

            UObjectParser parser = UObjectParserPool.Instance.Find(L);

            int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            parser.Push(_L, p0);

            int call_error = ULuaAPI.lua_pcall(_L, 1, 0, err_func);
            if (call_error != 0)
                mEnv.ThrowExceptionFromError(err_func - 1);

            ULuaAPI.lua_settop(_L, err_func - 1);
        }

        public void Delegate_Object(UnityEngine.Object p0)
        {
            var _L = L;

            UObjectParser parser = UObjectParserPool.Instance.Find(L);

            int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            parser.Push(_L, p0);

            int call_error = ULuaAPI.lua_pcall(_L, 1, 0, err_func);
            if (call_error != 0)
                mEnv.ThrowExceptionFromError(err_func - 1);

            ULuaAPI.lua_settop(_L, err_func - 1);
        }

        public void Delegate_Sprite ( UnityEngine.Sprite p0 )
        {
            var _L = L;

            UObjectParser parser = UObjectParserPool.Instance.Find( L );

            int err_func = ULuaAPI.luaex_load_error_func( _L, mErrorFuncRef );

            ULuaAPI.lua_getref( _L, mLuaReference );
            parser.Push( _L, p0 );

            int call_error = ULuaAPI.lua_pcall( _L, 1, 0, err_func );
            if (call_error != 0)
                mEnv.ThrowExceptionFromError( err_func - 1 );

            ULuaAPI.lua_settop( _L, err_func - 1 );
        }

        public void Delegate_Object(string p0, int p1, UnityEngine.Object p2)
        {
            var _L = L;

            UObjectParser parser = UObjectParserPool.Instance.Find(L);

            int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            ULuaAPI.lua_pushstring(_L, p0);
            ULuaAPI.lua_pushnumber(_L, p1);
            parser.Push(_L, p2);

            int call_error = ULuaAPI.lua_pcall(_L, 3, 0, err_func);
            if (call_error != 0)
                mEnv.ThrowExceptionFromError(err_func - 1);

            ULuaAPI.lua_settop(_L, err_func - 1);
        }

		public void Delegate_Entity(UObjectBase p0)
		{
			var _L = L;

			UObjectParser parser = UObjectParserPool.Instance.Find(L);

			int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

			ULuaAPI.lua_getref(_L, mLuaReference);
			parser.Push(_L, p0);

			int call_error = ULuaAPI.lua_pcall(_L, 1, 0, err_func);
			if (call_error != 0)
				mEnv.ThrowExceptionFromError(err_func - 1);

			ULuaAPI.lua_settop(_L, err_func - 1);
		}

        public void Delegate_Entity(string p0, int p1, UObjectBase p2)
        {
            var _L = L;

            UObjectParser parser = UObjectParserPool.Instance.Find(L);

            int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            ULuaAPI.lua_pushstring(_L, p0);
            ULuaAPI.lua_pushnumber(_L, p1);
            parser.Push(_L, p2);

            int call_error = ULuaAPI.lua_pcall(_L, 3, 0, err_func);
            if (call_error != 0)
                mEnv.ThrowExceptionFromError(err_func - 1);

            ULuaAPI.lua_settop(_L, err_func - 1);
        }

        public void Delegate_Vector2(UnityEngine.Vector2 p0)
        {
            var _L = L;

            UObjectParser parser = UObjectParserPool.Instance.Find(L);

            int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            parser.Push(_L, p0);

            int call_error = ULuaAPI.lua_pcall(_L, 1, 0, err_func);
            if (call_error != 0)
                mEnv.ThrowExceptionFromError(err_func - 1);

            ULuaAPI.lua_settop(_L, err_func - 1);
        }
        public void Delegate_IntegerString(int p0, string p1)
        {
            var _L = L;

            int err_func = ULuaAPI.luaex_load_error_func(_L, mErrorFuncRef);

            ULuaAPI.lua_getref(_L, mLuaReference);
            ULuaAPI.lua_pushint64(_L, p0);
            ULuaAPI.lua_pushstring(_L, p1);

            int call_error = ULuaAPI.lua_pcall(_L, 2, 0, err_func);
            if (call_error != 0)
                mEnv.ThrowExceptionFromError(err_func - 1);

            ULuaAPI.lua_settop(_L, err_func - 1);
        }

        public override Delegate GetDelegateByType(Type type)
        {
            if (type == typeof(System.Action))
                return new System.Action(Delegate_Null);

            if (type == typeof(UnityEngine.Events.UnityAction))
                return new UnityEngine.Events.UnityAction(Delegate_Null);

            if (type == typeof(System.Action< string >))
                return new System.Action< string >(Delegate_String); 

            if (type == typeof( System.Action<uint> ))
                return new System.Action<uint>( Delegate_UInteger );

            if (type == typeof(System.Action< int >))
                return new System.Action< int >(Delegate_Integer);

            if (type == typeof(System.Action< int, int >))
                return new System.Action< int, int >(Delegate_Integer);

            if (type == typeof(System.Action< float >))
                return new System.Action< float >(Delegate_Single);

            if (type == typeof(System.Action< float, float >))
                return new System.Action< float, float >(Delegate_Single);

            if (type == typeof(System.Action< double >))
                return new System.Action< double >(Delegate_Number);

            if (type == typeof(System.Action< double, double >))
                return new System.Action< double, double >(Delegate_Number);

            if (type == typeof(System.Action< bool >))
                return new System.Action< bool >(Delegate_Boolean);

            if (type == typeof(System.Action< bool, bool >))
                return new System.Action< bool, bool >(Delegate_Boolean);

            if (type == typeof(System.Action< bool, ULuaTable >))
                return new System.Action< bool, ULuaTable >(Delegate_Boolean);

            if (type == typeof(System.Action< UnityEngine.Vector3 >))
                return new System.Action< UnityEngine.Vector3 >(Delegate_Vector3);

            if (type == typeof(System.Action< UnityEngine.GameObject >))
                return new System.Action< UnityEngine.GameObject >(Delegate_GameObject);

            if (type == typeof(System.Action< int, UnityEngine.GameObject >))
                return new System.Action<int, UnityEngine.GameObject>(Delegate_GameObject);

            if (type == typeof(System.Action< int, object >))
                return new System.Action<int, object>( Delegate_object );

            if (type == typeof(System.Action< UnityEngine.Object >))
                return new System.Action<UnityEngine.Object>(Delegate_Object);

            if (type == typeof( System.Action<UnityEngine.Sprite> ))
                return new System.Action<UnityEngine.Sprite>( Delegate_Sprite );

            if (type == typeof(System.Action< string, int, UnityEngine.Object >))
                return new System.Action< string, int, UnityEngine.Object >(Delegate_Object);

            if (type == typeof(System.Action< UObjectBase >))
                return new System.Action< UObjectBase >(Delegate_Entity);

            if (type == typeof(System.Action< string, int, UObjectBase >))
                return new System.Action< string, int, UObjectBase >(Delegate_Entity);

            if (type == typeof(System.Action< UnityEngine.Vector2 >))
                return new System.Action< UnityEngine.Vector2 >(Delegate_Vector2);

            if (type == typeof(System.Action< object >))
                return new System.Action< object >(Delegate_Object);

            if (type == typeof(System.Action<int, string>))
                return new System.Action<int, string>(Delegate_IntegerString);

            throw new InvalidCastException("This delegate must add to CSharpCallLua: " + type);
        }
    }
}
