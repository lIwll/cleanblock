using ULuaState = System.IntPtr;

namespace UEngine.ULua
{
    public abstract class ULuaBase : System.IDisposable
    {
        protected ULuaEnv   mEnv;
        protected bool      mDisposed;
        protected int       mLuaReference;

        protected int ErrorFuncRef
        {
            get
            {
                return mEnv.mErrorFuncRef;
            }
        }

        protected ULuaState L
        {
            get
            {
                return mEnv.L;
            }
        }

        protected UObjectParser Parser
        {
            get
            {
                return mEnv.mParser;
            }
        }

        public ULuaBase(int reference, ULuaEnv env)
        {
            mEnv            = env;
            mDisposed       = false;
            mLuaReference   = reference;
        }

        ~ULuaBase()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);

            System.GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposeManagedResources)
        {
            if (!mDisposed)
            {
                if (mLuaReference != 0)
                {
                    bool is_delegate = this is UDelegateBridgeBase;
                    if (disposeManagedResources)
                        mEnv.mParser.ReleaseLuaBase(mEnv.L, mLuaReference, is_delegate);
                    else
                        mEnv.EqueueGCAction(new ULuaEnv.GCAction { Reference = mLuaReference, IsDelegate = is_delegate });

                    //ULuaAPI.lua_unref(L, mLuaReference);

                    mLuaReference = 0;
                }
                mEnv        = null;
                mDisposed   = true;
            }
        }

        public override bool Equals(object o)
        {
            if (this.GetType() == o.GetType())
            {
                ULuaBase rhs = (ULuaBase)o;

                var _L = L;
                if (_L != rhs.mEnv.L)
                    return false;

                int top = ULuaAPI.lua_gettop(_L);
                ULuaAPI.lua_getref(_L, rhs.mLuaReference);
                ULuaAPI.lua_getref(_L, mLuaReference);
                int equal = ULuaAPI.lua_rawequal(_L, -1, -2);
                ULuaAPI.lua_settop(_L, top);

                return (equal != 0);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return mLuaReference + ((mEnv != null) ? mEnv.L.ToInt32() : 0);
        }

        internal virtual void Push(ULuaState L)
        {
            ULuaAPI.lua_getref(L, mLuaReference);
        }
    }
}
