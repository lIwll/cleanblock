using System;
using System.Reflection;
using System.Collections.Generic;

using ULuaState = System.IntPtr;

namespace UEngine.ULua
{
    public class UOverloadMethodWrap
    {
        UObjectParser   mParser;
        Type            mTargetType;
        MethodBase      mMethod;
        ObjectCheck[]   mCheckArray;
        ObjectCast[]    mCastArray;
        int[]           mInPosArray;
        int[]           mOutPosArray;
        bool[]          mIsOptionalArray;
        object[]        mDefaultValueArray;
        bool            mIsVoid = true;
        int             mLuaStackPosStart = 1;
        bool            mTargetNeeded = false;
        object[]        mArgs;
        int[]           mRefPos;
        Type            mParamsType = null;

        public bool HasDefalutValue{ get; private set; }

        public UOverloadMethodWrap(UObjectParser parser, Type targetType, MethodBase method)
        {
            mParser         = parser;
            mTargetType     = targetType;
            mMethod         = method;
            HasDefalutValue = false;
        }

        public void Init(UObjectChecker objChecker, UObjectCaster objCaster)
        {
            if (typeof(Delegate).IsAssignableFrom(mTargetType) || !mMethod.IsStatic || mMethod.IsConstructor)
            {
                mLuaStackPosStart = 2;
                if (!mMethod.IsConstructor)
                    mTargetNeeded = true;
            }

            var paramInfos = mMethod.GetParameters();
            mRefPos = new int[paramInfos.Length];

            List< int > inPosList = new List< int >();
            List< int > outPosList = new List< int >();

            List< ObjectCheck > paramsChecks = new List< ObjectCheck >();
            List< ObjectCast > paramsCasts = new List< ObjectCast >();
            List< bool > isOptionalList = new List< bool >();
            List< object > defaultValueList = new List< object >();

            for (int i = 0; i < paramInfos.Length; i ++)
            {
                mRefPos[i] = -1;

                if (!paramInfos[i].IsIn && paramInfos[i].IsOut)  // out parameter
				{
					outPosList.Add(i);
				} else
                {
                    if (paramInfos[i].ParameterType.IsByRef)
                    {
                        var ttype = paramInfos[i].ParameterType.GetElementType();
                        if (UEncoder.IsStruct(ttype) && ttype != typeof(decimal))
                            mRefPos[i] = inPosList.Count;
                        outPosList.Add(i);
                    }

                    inPosList.Add(i);
                    var paramType = paramInfos[i].IsDefined(typeof(ParamArrayAttribute), false) || (!paramInfos[i].ParameterType.IsArray && paramInfos[i].ParameterType.IsByRef ) ? 
                        paramInfos[i].ParameterType.GetElementType() : paramInfos[i].ParameterType;
                    paramsChecks.Add (objChecker.GetChecker(paramType));
                    paramsCasts.Add (objCaster.GetCaster(paramType));
                    isOptionalList.Add(paramInfos[i].IsOptional);
                    var defalutValue = paramInfos[i].DefaultValue;
                    if (paramInfos[i].IsOptional)
                    {
                        if (defalutValue != null && defalutValue.GetType() != paramInfos[i].ParameterType)
                        {
                            defalutValue = defalutValue.GetType() == typeof(Missing) ? (paramInfos[i].ParameterType.IsValueType ? Activator.CreateInstance(paramInfos[i].ParameterType) : Missing.Value) 
                                : Convert.ChangeType(defalutValue, paramInfos[i].ParameterType);
                        }
                        HasDefalutValue = true;
                    }
                    defaultValueList.Add(paramInfos[i].IsOptional ? defalutValue : null);
                }
            }
            mCheckArray         = paramsChecks.ToArray();
            mCastArray          = paramsCasts.ToArray();
            mInPosArray         = inPosList.ToArray();
            mOutPosArray        = outPosList.ToArray();
            mIsOptionalArray    = isOptionalList.ToArray();
            mDefaultValueArray  = defaultValueList.ToArray();

            if (paramInfos.Length > 0 && paramInfos[paramInfos.Length - 1].IsDefined(typeof(ParamArrayAttribute), false))
                mParamsType = paramInfos[paramInfos.Length - 1].ParameterType.GetElementType();

            mArgs = new object[paramInfos.Length];

            if (mMethod is MethodInfo) //constructor is not MethodInfo?
                mIsVoid = (mMethod as MethodInfo).ReturnType == typeof(void);
            else if(mMethod is ConstructorInfo)
                mIsVoid = false;
        }

        public bool Check(ULuaState L)
        {
            int luaTop = ULuaAPI.lua_gettop(L);
            int luaStackPos = mLuaStackPosStart;

            for(int i = 0; i < mCheckArray.Length; i ++)
            {
                if ((i == (mCheckArray.Length - 1)) && (mParamsType != null))
                    break;

                if (luaStackPos > luaTop && !mIsOptionalArray[i])
                    return false;
                else if(luaStackPos <= luaTop && !mCheckArray[i](L, luaStackPos))
                    return false;

                if (luaStackPos <= luaTop || !mIsOptionalArray[i])
                    luaStackPos ++;
            }

            return mParamsType != null ? (luaStackPos < luaTop + 1 ? 
                mCheckArray[mCheckArray.Length - 1](L, luaStackPos) : true) : luaStackPos == luaTop + 1;
        }

        public int Call(ULuaState L)
        {
            try
            {
                object target = null;
                MethodBase toInvoke = mMethod;

                if (mLuaStackPosStart > 1)
                {
                    target = mParser.FastGetCSObj(L, 1);
                    if (target is Delegate)
                    {
                        Delegate delegateInvoke = (Delegate)target;

                        toInvoke = delegateInvoke.Method;
                    }
                }

                int luaTop = ULuaAPI.lua_gettop(L);
                int luaStackPos = mLuaStackPosStart;

                for (int i = 0; i < mCastArray.Length; i ++)
                {
                    if (luaStackPos > luaTop)
                    {
                        if (mParamsType != null && i == mCastArray.Length - 1)
                            mArgs[mInPosArray[i]] = Array.CreateInstance(mParamsType, 0);
                        else
                            mArgs[mInPosArray[i]] = mDefaultValueArray[i];
                    } else
                    {
                        if (mParamsType != null && i == mCastArray.Length - 1)
                            mArgs[mInPosArray[i]] = mParser.GetParams(L, luaStackPos, mParamsType);
                        else
                            mArgs[mInPosArray[i]] = mCastArray[i](L, luaStackPos, null);

                        luaStackPos ++;
                    }
                }

                object ret = null;

                ret = toInvoke.IsConstructor ? ((ConstructorInfo)mMethod).Invoke(mArgs) : mMethod.Invoke(mTargetNeeded ? target : null, mArgs);

                int nRet = 0;

                if (!mIsVoid)
                {
                    mParser.PushAny(L, ret);

                    nRet ++;
                }

                for (int i = 0; i < mOutPosArray.Length; i ++)
                {
                    if (mRefPos[mOutPosArray[i]] != -1)
                        mParser.Update(L, mLuaStackPosStart + mRefPos[mOutPosArray[i]], mArgs[mOutPosArray[i]]);

                    mParser.PushAny(L, mArgs[mOutPosArray[i]]);

                    nRet ++;
                }

                return nRet;
            } finally
            {
                for (int i = 0; i < mArgs.Length; i ++)
                {
                    mArgs[i] = null;
                }
            }
        }
    }

    public class UMethodWrap
    {
        private string
            mMethodName;
        private List< UOverloadMethodWrap >
            mOverloads = new List< UOverloadMethodWrap >();

        public UMethodWrap(string methodName, List< UOverloadMethodWrap > overloads)
        {
            mMethodName = methodName;
            mOverloads  = overloads;
        }

        public int Call(ULuaState L)
        {
            try
            {
                if (mOverloads.Count == 1 && !mOverloads[0].HasDefalutValue) return mOverloads[0].Call(L);

                for (int i = 0; i < mOverloads.Count; ++ i)
                {
                    var overload = mOverloads[i];
                    if (overload.Check(L))
                        return overload.Call(L);
                }

                return ULuaAPI.luaL_error(L, "invalid arguments to " + mMethodName);
            } catch (System.Reflection.TargetInvocationException e)
            {
                return ULuaAPI.luaL_error(L, "c# exception:" + e.InnerException.Message + ",stack:" + e.InnerException.StackTrace);
            } catch (System.Exception e)
            {
                return ULuaAPI.luaL_error(L, "c# exception:" + e.Message + ",stack:" + e.StackTrace);
            }
        }
    }

    public class UMethodWrapsCache
    {
        UObjectParser
            mParser;
        UObjectChecker
            mObjChecker;
        UObjectCaster
            mObjCaster;
        Dictionary< Type, lua_CSFunction >
            mConstructorCache = new Dictionary< Type, lua_CSFunction >();
        Dictionary< Type, Dictionary< string, lua_CSFunction > >
            mMethodsCache = new Dictionary< Type, Dictionary< string, lua_CSFunction > >();
        Dictionary< Type, lua_CSFunction >
            mDelegateCache = new Dictionary< Type, lua_CSFunction >();

        public UMethodWrapsCache(UObjectParser parser, UObjectChecker objChecker, UObjectCaster objCaster)
        {
            mParser     = parser;
            mObjChecker = objChecker;
            mObjCaster  = objCaster;
        }

        public lua_CSFunction GetConstructorWrap(Type type)
        {
            if (!mConstructorCache.ContainsKey(type))
            {
                var constructors = type.GetConstructors();
                if (type.IsAbstract || constructors == null || constructors.Length == 0)
                {
                    if (type.IsValueType)
                    {
                        mConstructorCache[type] = (L) =>
                        {
                            mParser.PushAny(L, Activator.CreateInstance(type));

                            return 1;
                        };
                    } else
                    {
                        return null;
                    }
                } else
                {
                    lua_CSFunction ctor = GenMethodWrap(type, ".ctor", constructors).Call;
                    
                    if (type.IsValueType)
                    {
                        bool hasZeroParamsCtor = false;
                        for (int i = 0; i < constructors.Length; i ++)
                        {
                            if (constructors[i].GetParameters().Length == 0)
                            {
                                hasZeroParamsCtor = true;

                                break;
                            }
                        }

                        if (hasZeroParamsCtor)
                        {
                            mConstructorCache[type] = ctor;
                        } else
                        {
                            mConstructorCache[type] = (L) =>
                            {
                                if (ULuaAPI.lua_gettop(L) == 1)
                                {
                                    mParser.PushAny(L, Activator.CreateInstance(type));

                                    return 1;
                                } else
                                {
                                    return ctor(L);
                                }
                            };
                        }
                    } else
                    {
                        mConstructorCache[type] = ctor;
                    }
                }
            }

            return mConstructorCache[type];
        }

        public lua_CSFunction GetMethodWrap(Type type, string methodName)
        {
            if (!mMethodsCache.ContainsKey(type))
                mMethodsCache[type] = new Dictionary< string, lua_CSFunction >();

            var methodsOfType = mMethodsCache[type];
            if (!methodsOfType.ContainsKey(methodName))
            {
                MemberInfo[] methods = type.GetMember(methodName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (methods == null || methods.Length == 0 ||
#if UNITY_WSA && !UNITY_EDITOR
                    methods[0] is MethodBase
#else
                    methods[0].MemberType != MemberTypes.Method
#endif
                    )
                {
                    return null;
                }

                methodsOfType[methodName] = GenMethodWrap(type, methodName, methods).Call;
            }

            return methodsOfType[methodName];
        }

        public lua_CSFunction GetMethodWrapInCache(Type type, string methodName)
        {
            if (!mMethodsCache.ContainsKey(type))
                mMethodsCache[type] = new Dictionary< string, lua_CSFunction >();
            var methodsOfType = mMethodsCache[type];

            return methodsOfType.ContainsKey(methodName) ? methodsOfType[methodName] : null;
        }

        public lua_CSFunction GetDelegateWrap(Type type)
        {
            if (!typeof(Delegate).IsAssignableFrom(type))
                return null;

            if (!mDelegateCache.ContainsKey(type))
                mDelegateCache[type] = GenMethodWrap(type, type.ToString(), new MethodBase[] { type.GetMethod("Invoke") }).Call;

            return mDelegateCache[type];
        }

        public lua_CSFunction GetEventWrap(Type type, string eventName)
        {
            if (!mMethodsCache.ContainsKey(type))
                mMethodsCache[type] = new Dictionary< string, lua_CSFunction >();

            var methodsOfType = mMethodsCache[type];
            if (!methodsOfType.ContainsKey(eventName))
            {
                EventInfo eventInfo = type.GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static);
                if (eventInfo == null)
                    throw new Exception(type.Name + " has no event named: " + eventName);

                int start_idx = 0;

                MethodInfo add = eventInfo.GetAddMethod();
                MethodInfo remove = eventInfo.GetRemoveMethod();

                bool is_static = add != null ? add.IsStatic : remove.IsStatic;
                if (!is_static)
                    start_idx = 1;

                methodsOfType[eventName] = (L) =>
                {
                    object obj = null;

                    if (!is_static)
                    {
                        obj = mParser.GetObject(L, 1, type);
                        if (obj == null)
                            return ULuaAPI.luaL_error(L, "invalid #1, needed:" + type);
                    }

                    try
                    {
                        Delegate handlerDelegate = mParser.CreateDelegateBridge(L, eventInfo.EventHandlerType, start_idx + 2);
                        if (handlerDelegate == null)
                            return ULuaAPI.luaL_error(L, "invalid #" + (start_idx + 2) + ", needed:" + eventInfo.EventHandlerType);

                        switch (ULuaAPI.lua_tostring(L, start_idx + 1))
                        {
                            case "+":
                                if (add == null)
                                {
                                    return ULuaAPI.luaL_error(L, "no add for event " + eventName);
                                }
                                add.Invoke(obj, new object[] { handlerDelegate });
                                break;
                            case "-":
                                if (remove == null)
                                    return ULuaAPI.luaL_error(L, "no remove for event " + eventName);
                                remove.Invoke(obj, new object[] { handlerDelegate });
                                break;
                            default:
                                return ULuaAPI.luaL_error(L, "invalid #" + (start_idx + 1) + ", needed: '+' or '-'" + eventInfo.EventHandlerType);
                        }
                    } catch (System.Exception e)
                    {
                        return ULuaAPI.luaL_error(L, "c# exception:" + e + ",stack:" + e.StackTrace);
                    }

                    return 0;
                };
            }
            return methodsOfType[eventName];
        }

        public UMethodWrap GenMethodWrap(Type type, string methodName, IEnumerable< MemberInfo > methodBases)
        { 
            List< UOverloadMethodWrap > overloads = new List< UOverloadMethodWrap >();
            foreach (var methodBase in methodBases)
            {
                var mb = methodBase as MethodBase;
                if (mb == null)
                    continue;

                if (mb.IsGenericMethodDefinition && !TryMakeGenericMethod(ref mb))
                    continue;

                var overload = new UOverloadMethodWrap(mParser, type, mb);
                overload.Init(mObjChecker, mObjCaster);
                overloads.Add(overload);
            }
            return new UMethodWrap(methodName, overloads);
        }

        private static bool TryMakeGenericMethod(ref MethodBase method)
        {
            try
            {
                var genericArguments = method.GetGenericArguments();
                var constraintedArgumentTypes = new Type[genericArguments.Length];
                for (var i = 0; i < genericArguments.Length; i ++)
                {
                    var argumentType = genericArguments[i];
                    var parameterConstraints = argumentType.GetGenericParameterConstraints();
                    if (parameterConstraints.Length == 0 || !parameterConstraints[0].IsClass)
                        return false;
                    constraintedArgumentTypes[i] = parameterConstraints[0];
                }
                method = ((MethodInfo)method).MakeGenericMethod(constraintedArgumentTypes);

                return true;
            } catch (Exception)
            {
                return false;
            }
        }
    }
}
