using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace UEngine
{
    public static class UDataParser
    {
        private const char kMAP_SPRITER = ',';
        private const char kLIST_SPRITER = ',';
        private const char kKEY_VALUE_SPRITER = '|';

        public static object GetValue(string value, Type type)
        {
            if (type == null)
                return null;
            else if (type == typeof(string))
                return value;
            else if (type == typeof(Int32))
                return Convert.ToInt32(Convert.ToDouble(value));
            else if (type == typeof(float))
                return float.Parse(value);
            else if (type == typeof(byte))
                return Convert.ToByte(Convert.ToDouble(value));
            else if (type == typeof(sbyte))
                return Convert.ToSByte(Convert.ToDouble(value));
            else if (type == typeof(UInt32))
                return Convert.ToUInt32(Convert.ToDouble(value));
            else if (type == typeof(Int16))
                return Convert.ToInt16(Convert.ToDouble(value));
            else if (type == typeof(Int64))
                return Convert.ToInt64(Convert.ToDouble(value));
            else if (type == typeof(UInt16))
                return Convert.ToUInt16(Convert.ToDouble(value));
            else if (type == typeof(UInt64))
                return Convert.ToUInt64(Convert.ToDouble(value));
            else if (type == typeof(double))
                return double.Parse(value);
            else if (type == typeof(bool))
            {
                if (value == "0")
                    return false;
                else if (value == "1")
                    return true;

                return bool.Parse(value);
            }
            else if (type.BaseType == typeof(Enum))
                return GetValue(value, Enum.GetUnderlyingType(type));
            else if (type == typeof(Vector3))
            {
                Vector3 result;
                ParseVector3(value, out result);

                return result;
            }
            else if (type == typeof(Quaternion))
            {
                Quaternion result;
                ParseQuaternion(value, out result);

                return result;
            }
            else if (type == typeof(Color))
            {
                Color result;
                ParseColor(value, out result);

                return result;
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                Type[] types = type.GetGenericArguments();
                var map = ParseMap(value);
                var result = type.GetConstructor(Type.EmptyTypes).Invoke(null);
                foreach (var item in map)
                {
                    var key = GetValue(item.Key, types[0]);
                    var v = GetValue(item.Value, types[1]);
                    type.GetMethod("Add").Invoke(result, new object[] { key, v });
                }

                return result;
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type t = type.GetGenericArguments()[0];
                var list = ParseList(value);
                var result = type.GetConstructor(Type.EmptyTypes).Invoke(null);
                foreach (var item in list)
                {
                    var v = GetValue(item, t);
                    type.GetMethod("Add").Invoke(result, new object[] { v });
                }

                return result;
            }
            else if (type.IsArray)
            {
                Type t = type.GetElementType();

                var list = ParseList(value);
                var result = Array.CreateInstance(t, list.Count);
                for (int i = 0; i < list.Count; ++ i)
                {
                    var v = GetValue(list[i], t);

                    (result as Array).SetValue(v, i);
                }

                return result;
            }

            return null;
        }

        public static bool ParseVector3(string s, out Vector3 result)
        {
            result = new Vector3();

            string trimString = s.Trim();
            if (trimString.Length < 7)
                return false;

            try
            {
                string[] detail = trimString.Split(kLIST_SPRITER);
                if (detail.Length != 3)
                    return false;

                result.x = float.Parse(detail[0]);
                result.y = float.Parse(detail[1]);
                result.z = float.Parse(detail[2]);

                return true;
            } catch (Exception e)
            {
                ULogger.Error("Parse Vector3 error: " + trimString + e.ToString());

                return false;
            }
        }

        public static bool ParseQuaternion(string s, out Quaternion result)
        {
            result = new Quaternion();

            string trimString = s.Trim();
            if (trimString.Length < 9)
                return false;

            try
            {
                string[] detail = trimString.Split(kLIST_SPRITER);
                if (detail.Length != 4)
                    return false;

                result.x = float.Parse(detail[0]);
                result.y = float.Parse(detail[1]);
                result.z = float.Parse(detail[2]);
                result.w = float.Parse(detail[3]);

                return true;
            } catch (Exception e)
            {
                ULogger.Error("Parse Quaternion error: " + trimString + e.ToString());

                return false;
            }
        }

        public static bool ParseColor(string s, out Color result)
        {
            result = Color.clear;

            string trimString = s.Trim();
            if (trimString.Length < 9)
                return false;

            try
            {
                string[] detail = trimString.Split(kLIST_SPRITER);
                if (detail.Length != 4)
                    return false;

                result = new Color(float.Parse(detail[0]) / 255, float.Parse(detail[1]) / 255, float.Parse(detail[2]) / 255, float.Parse(detail[3]) / 255);

                return true;
            } catch (Exception e)
            {
                ULogger.Error("Parse Color error: " + trimString + e.ToString());

                return false;
            }
        }

        public static Dictionary< string,  string > ParseMap(this string strMap, char keyValueSpriter = kKEY_VALUE_SPRITER, char mapSpriter = kMAP_SPRITER)
        {
            Dictionary< string,  string > result = new Dictionary< string,  string >();

            if (string.IsNullOrEmpty(strMap))
                return result;

            var map = strMap.Split(mapSpriter);
            for (int i = 0; i < map.Length; i++)
            {
                if (string.IsNullOrEmpty(map[i]))
                    continue;

                var keyValuePair = map[i].Split(keyValueSpriter);
                if (keyValuePair.Length == 2)
                {
                    if (!result.ContainsKey(keyValuePair[0]))
                        result.Add(keyValuePair[0], keyValuePair[1]);
                    else
                        ULogger.Warn("Key {0} already exist, index {1} of {2}.", keyValuePair[0], i, strMap);
                } else
                {
                    ULogger.Warn("KeyValuePair are not match: {0}, index {1} of {2}.", map[i], i, strMap);
                }
            }

            return result;
        }

        public static string PackMap< T, U >(this IEnumerable< KeyValuePair< T, U > > map, char keyValueSpriter = kKEY_VALUE_SPRITER, char mapSpriter = kMAP_SPRITER)
        {
            if (map.Count() == 0)
                return "";

            StringBuilder sb = new StringBuilder();
            foreach (var item in map)
                sb.AppendFormat("{0}{1}{2}{3}", item.Key, keyValueSpriter, item.Value, mapSpriter);

            return sb.ToString().Remove(sb.Length - 1, 1);
        }

        public static List< string > ParseList(this string strList, char listSpriter = kLIST_SPRITER)
        {
            var result = new List< string >();

            if (string.IsNullOrEmpty(strList))
                return result;

            var trimString = strList.Trim();
            if (string.IsNullOrEmpty(strList))
                return result;

            var detials = trimString.Split(listSpriter);
            foreach (var item in detials)
            {
                if (!string.IsNullOrEmpty(item))
                    result.Add(item.Trim());
            }

            return result;
        }

        public static string PackList< T >(this List< T > list, char listSpriter = kLIST_SPRITER)
        {
            if (list.Count == 0)
                return "";

            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
                sb.AppendFormat("{0}{1}", item, listSpriter);
            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        public static string PackArray< T >(this T[] array, char listSpriter = kLIST_SPRITER)
        {
            var list = new List< T >();
                list.AddRange(array);

            return PackList(list, listSpriter);
        }
    }
}
