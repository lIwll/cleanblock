using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UEngine
{
    public enum EScreenAspectRatio
    {
        eSAR_UNKNOW = 0,

        eSAR_16_9,
        eSAR_4_3,
    }

    public enum EVariableType : byte
    {
        eVT_Byte,
        eVT_SByte,
        eVT_Int16,
        eVT_Int32,
        eVT_Int64,
        eVT_UInt16,
        eVT_UInt32,
        eVT_UInt64,
        eVT_Boolean,
        eVT_String,
        eVT_Float,
        eVT_Double,
        eVT_List,
        eVT_Array,
        eVT_Dictionary,
        eVT_Enum,
        eVT_Vec2,
        eVT_Vec3,
        eVT_Vec4,
        eVT_Quaternion,
        eVT_Matrix,
        eVT_Color, // float
        eVT_Color32, // 4 byte

        eVT_AniCurve, // animation curve
        eVT_KeyFrame, // key frame

        eVT_Custom,
    }

    public struct UVariableType
    {
        public string mName;

        public EVariableType mType;

        public UVariableType(string name, EVariableType type)
        {
            mName = name;
            mType = type;
        }
    }

    public struct UCustomType
    {
    }

    public struct UListType
    {
    }

    public struct UDictionaryType
    {
    }

    public static class UCore
    {
        // 屏幕分辨率比例...
        public static EScreenAspectRatio kScreenAspectRatio = EScreenAspectRatio.eSAR_UNKNOW;

        // 初始化是否已经完成...
        public static bool kInitFinished = false;

        // 等待被加载的场景名字，加载场景时都先进loading场景，再根据这个名字异步加载...
        public static string kLoadLevelName = "test";

        // 是否开启手机调试模式...
        public static bool kMobileDebug = false;

        // 是否处于编辑模式
        public static bool editorMode = true;

        // 被击退时的摩擦力加速度...
        public static float kBeatBackA = 100.0f;

        // main game object.
		public static MonoBehaviour GameEnv
		{ get; set; }

		// main camera
		public static Camera MainCamera
		{ get; set; }

		// profile
		public static UProfile Profile
		{ get; set; }

		public static int ThreadID
		{ get; set; }

		public static uint CurrentFrame
		{ get; set; }

        public static void QuiteGame(string erro)
        {
            if (!string.IsNullOrEmpty(erro))
                ULogger.Error(erro);

            Application.Quit();
        }


        public static UVariableType ConvToVariableType(string type)
        {
            string _type = type.ToLower();
            switch (_type)
            {
                case "byte":    return new UVariableType(type, EVariableType.eVT_Byte);
                case "sbyte":   return new UVariableType(type, EVariableType.eVT_SByte);
                case "int16":   return new UVariableType(type, EVariableType.eVT_Int16);
                case "int32":   return new UVariableType(type, EVariableType.eVT_Int32);
                case "int64":   return new UVariableType(type, EVariableType.eVT_Int64);
                case "uint16":  return new UVariableType(type, EVariableType.eVT_UInt16);
                case "uint32":  return new UVariableType(type, EVariableType.eVT_UInt32);
                case "uint64":  return new UVariableType(type, EVariableType.eVT_UInt64);
                case "bool":    return new UVariableType(type, EVariableType.eVT_Boolean);
                case "string":  return new UVariableType(type, EVariableType.eVT_String);
                case "float":   return new UVariableType(type, EVariableType.eVT_Float);
                case "double":  return new UVariableType(type, EVariableType.eVT_Double);
                case "list":    return new UVariableType(type, EVariableType.eVT_List);
                case "array":   return new UVariableType(type, EVariableType.eVT_Array);
                case "dict":    return new UVariableType(type, EVariableType.eVT_Dictionary);
                case "enum":    return new UVariableType(type, EVariableType.eVT_Enum);
                case "vec2":    return new UVariableType(type, EVariableType.eVT_Vec2);
                case "vec3":    return new UVariableType(type, EVariableType.eVT_Vec3);
                case "vec4":    return new UVariableType(type, EVariableType.eVT_Vec4);
                case "quat":    return new UVariableType(type, EVariableType.eVT_Quaternion);
                case "mat4":    return new UVariableType(type, EVariableType.eVT_Matrix);
                case "color":   return new UVariableType(type, EVariableType.eVT_Color);
                case "color32": return new UVariableType(type, EVariableType.eVT_Color32);
                case "aniCurve":return new UVariableType(type, EVariableType.eVT_AniCurve);
                case "keyFrame":return new UVariableType(type, EVariableType.eVT_KeyFrame);
            }

            return new UVariableType(type, EVariableType.eVT_Custom);
        }

        public static EVariableType ConvToVariableType(object value)
        {
            Type type = value.GetType();

            if (type == typeof(byte))
                return EVariableType.eVT_Byte;
            else if (type == typeof(sbyte))
                return EVariableType.eVT_SByte;
            else if (type == typeof(short))
                return EVariableType.eVT_Int16;
            else if (type == typeof(int))
                return EVariableType.eVT_Int32;
            else if (type == typeof(long))
                return EVariableType.eVT_Int64;
            else if (type == typeof(ushort))
                return EVariableType.eVT_UInt16;
            else if (type == typeof(uint))
                return EVariableType.eVT_UInt32;
            else if (type == typeof(ulong))
                return EVariableType.eVT_UInt64;
            else if (type == typeof(bool))
                return EVariableType.eVT_Boolean;
            else if (type == typeof(string))
                return EVariableType.eVT_String;
            else if (type == typeof(float))
                return EVariableType.eVT_Float;
            else if (type == typeof(double))
                return EVariableType.eVT_Double;
            else if (type.IsArray)
                return EVariableType.eVT_Array;
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return EVariableType.eVT_List;
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                return EVariableType.eVT_Dictionary;
            else if (type.IsEnum)
                return EVariableType.eVT_Enum;
            else if (type == typeof(UnityEngine.Vector2))
                return EVariableType.eVT_Vec2;
            else if (type == typeof(UnityEngine.Vector3))
                return EVariableType.eVT_Vec3;
            else if (type == typeof(UnityEngine.Vector4))
                return EVariableType.eVT_Vec4;
            else if (type == typeof(UnityEngine.Quaternion))
                return EVariableType.eVT_Quaternion;
            else if (type == typeof(UnityEngine.Matrix4x4))
                return EVariableType.eVT_Matrix;
            else if (type == typeof(UnityEngine.Color))
                return EVariableType.eVT_Color;
            else if (type == typeof(UnityEngine.Color32))
                return EVariableType.eVT_Color32;
            else if (type == typeof(UnityEngine.AnimationCurve))
                return EVariableType.eVT_AniCurve;
            else if (type == typeof(UnityEngine.Keyframe))
                return EVariableType.eVT_KeyFrame;

            return EVariableType.eVT_Custom;
        }

        public static Type ConvToEngineType(EVariableType type)
        {
            switch (type)
            {
                case EVariableType.eVT_Byte:        return typeof(byte);
                case EVariableType.eVT_SByte:       return typeof(sbyte);
                case EVariableType.eVT_Int16:       return typeof(Int16);
                case EVariableType.eVT_Int32:       return typeof(Int32);
                case EVariableType.eVT_Int64:       return typeof(Int64);
                case EVariableType.eVT_UInt16:      return typeof(UInt16);
                case EVariableType.eVT_UInt32:      return typeof(UInt32);
                case EVariableType.eVT_UInt64:      return typeof(UInt64);
                case EVariableType.eVT_Boolean:     return typeof(bool);
                case EVariableType.eVT_String:      return typeof(string);
                case EVariableType.eVT_Float:       return typeof(float);
                case EVariableType.eVT_Double:      return typeof(double);
                case EVariableType.eVT_List:        return typeof(UListType);
                case EVariableType.eVT_Array:       return typeof(Array);
                case EVariableType.eVT_Dictionary:  return typeof(UDictionaryType);
                case EVariableType.eVT_Enum:        return typeof(Enum);
                case EVariableType.eVT_Vec2:        return typeof(UnityEngine.Vector2);
                case EVariableType.eVT_Vec3:        return typeof(UnityEngine.Vector3);
                case EVariableType.eVT_Vec4:        return typeof(UnityEngine.Vector4);
                case EVariableType.eVT_Quaternion:  return typeof(UnityEngine.Quaternion);
                case EVariableType.eVT_Matrix:      return typeof(UnityEngine.Matrix4x4);
                case EVariableType.eVT_Color:       return typeof(UnityEngine.Color);
                case EVariableType.eVT_Color32:     return typeof(UnityEngine.Color32);
                case EVariableType.eVT_AniCurve:    return typeof(UnityEngine.AnimationCurve);
                case EVariableType.eVT_KeyFrame:    return typeof(UnityEngine.Keyframe);
            }

            return typeof(UCustomType);
        }

        public static bool IsCommonType(UVariableType type)
        {
            return IsCommonType(type.mType);
        }

        public static bool IsCommonType(EVariableType type)
        {
            switch (type)
            {
                case EVariableType.eVT_Byte:
                case EVariableType.eVT_SByte:
                case EVariableType.eVT_Int16:
                case EVariableType.eVT_Int32:
                case EVariableType.eVT_Int64:
                case EVariableType.eVT_UInt16:
                case EVariableType.eVT_UInt32:
                case EVariableType.eVT_UInt64:
                case EVariableType.eVT_Boolean:
                case EVariableType.eVT_String:
                case EVariableType.eVT_Float:
                case EVariableType.eVT_Double:
                case EVariableType.eVT_Enum:
                case EVariableType.eVT_Vec2:
                case EVariableType.eVT_Vec3:
                case EVariableType.eVT_Vec4:
                case EVariableType.eVT_Quaternion:
                case EVariableType.eVT_Matrix:
                case EVariableType.eVT_Color:
                case EVariableType.eVT_Color32:
                case EVariableType.eVT_AniCurve:
                case EVariableType.eVT_KeyFrame:
                case EVariableType.eVT_Custom:
                    return true;
            }

            return false;
        }

		public static bool IsCollectionType(UVariableType type)
		{
			return IsCollectionType(type.mType);
		}

		public static bool IsCollectionType(EVariableType type)
		{
			switch (type)
			{
				case EVariableType.eVT_Array:
				case EVariableType.eVT_List:
				case EVariableType.eVT_Dictionary:
					return true;
			}

			return false;
		}

        public static EVariableType ToVariableType(this string type)
        {
            switch (type)
            {
                case "eVT_Byte":        return EVariableType.eVT_Byte;
                case "eVT_SByte":       return EVariableType.eVT_SByte;
                case "eVT_Int16":       return EVariableType.eVT_Int16;
                case "eVT_Int32":       return EVariableType.eVT_Int32;
                case "eVT_Int64":       return EVariableType.eVT_Int64;
                case "eVT_UInt16":      return EVariableType.eVT_UInt16;
                case "eVT_UInt32":      return EVariableType.eVT_UInt32;
                case "eVT_UInt64":      return EVariableType.eVT_UInt64;
                case "eVT_Boolean":     return EVariableType.eVT_Boolean;
                case "eVT_String":      return EVariableType.eVT_String;
                case "eVT_Float":       return EVariableType.eVT_Float;
                case "eVT_Double":      return EVariableType.eVT_Double;
                case "eVT_Array":       return EVariableType.eVT_Array;
                case "eVT_List":        return EVariableType.eVT_List;
                case "eVT_Dictionary":  return EVariableType.eVT_Dictionary;
                case "eVT_Enum":        return EVariableType.eVT_Enum;
                case "eVT_Vec2":        return EVariableType.eVT_Vec2;
                case "eVT_Vec3":        return EVariableType.eVT_Vec3;
                case "eVT_Vec4":        return EVariableType.eVT_Vec4;
                case "eVT_Quaternion":  return EVariableType.eVT_Quaternion;
                case "eVT_Matrix":      return EVariableType.eVT_Matrix;
                case "eVT_Color":       return EVariableType.eVT_Color;
                case "eVT_Color32":     return EVariableType.eVT_Color32;
                case "eVT_AniCurve":    return EVariableType.eVT_AniCurve;
                case "eVT_KeyFrame":    return EVariableType.eVT_KeyFrame;
            }

            return EVariableType.eVT_Custom;
        }

		public static void AddCallbackInFrames(Action cb, int frames = 3)
		{
			if (null != UCore.GameEnv)
				UCore.GameEnv.StartCoroutine(CallbackInFrames(cb, frames));
			else
				UTimer.AddTimer((uint)((float)frames / 30) * 1000, 0, cb);
		}

		public static void AddCallbackInFrames(Action< object > cb, object arg1, int frames = 3)
		{
			if (null != UCore.GameEnv)
				UCore.GameEnv.StartCoroutine(CallbackInFrames(cb, arg1, frames));
			else
				UTimer.AddTimer< object >((uint)((float)frames / 30) * 1000, 0, cb, arg1);
		}

		public static void AddCallbackInFrames(Action< object, object > cb, object arg1, object arg2, int frames = 3)
		{
			if (null != UCore.GameEnv)
				UCore.GameEnv.StartCoroutine(CallbackInFrames(cb, arg1, arg2, frames));
			else
				UTimer.AddTimer< object, object >((uint)((float)frames / 30) * 1000, 0, cb, arg1, arg2);
		}

		public static void AddCallbackInFrames(Action< object, object, object > cb, object arg1, object arg2, object arg3, int frames = 3)
		{
			if (null != UCore.GameEnv)
				UCore.GameEnv.StartCoroutine(CallbackInFrames(cb, arg1, arg2, arg3, frames));
			else
				UTimer.AddTimer< object, object, object >((uint)((float)frames / 30) * 1000, 0, cb, arg1, arg2, arg3);
		}

		public static void AddCallbackInFrames(Action< object, object, object, object > cb, object arg1, object arg2, object arg3, object arg4, int frames = 3)
		{
			if (null != UCore.GameEnv)
				UCore.GameEnv.StartCoroutine(CallbackInFrames(cb, arg1, arg2, arg3, arg4, frames));
			else
				UTimer.AddTimer< object, object, object, object >((uint)((float)frames / 30) * 1000, 0, cb, arg1, arg2, arg3, arg4);
		}

		static YieldInstruction mWaitForEndOfFrame = new WaitForEndOfFrame();
		static YieldInstruction mWaitForFixedUpdate = new WaitForFixedUpdate();

		static IEnumerator CallbackInFrames(Action cb, int frames)
		{
			for (int n = 0; n < frames; ++ n)
				yield return mWaitForEndOfFrame;

			cb();
		}

		static IEnumerator CallbackInFrames(Action< object > cb, object arg1, int frames)
		{
			for (int n = 0; n < frames; ++ n)
				yield return mWaitForEndOfFrame;

			cb(arg1);
		}

		static IEnumerator CallbackInFrames(Action< object, object > cb, object arg1, object arg2, int frames)
		{
			for (int n = 0; n < frames; ++ n)
				yield return mWaitForEndOfFrame;

			cb(arg1, arg2);
		}

		static IEnumerator CallbackInFrames(Action< object, object, object > cb, object arg1, object arg2, object arg3, int frames)
		{
			for (int n = 0; n < frames; ++ n)
				yield return mWaitForEndOfFrame;

			cb(arg1, arg2, arg3);
		}

		static IEnumerator CallbackInFrames(Action< object, object, object, object > cb, object arg1, object arg2, object arg3, object arg4, int frames)
		{
			for (int n = 0; n < frames; ++ n)
				yield return mWaitForEndOfFrame;

			cb(arg1, arg2, arg3, arg4);
		}
    }
}
