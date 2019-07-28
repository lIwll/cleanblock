using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using UnityEngine;

namespace UEngine
{
    public static class UCoreUtil
    {
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
        private static readonly string[] kEmptyPaths = new string[0];

        public static bool IsNumeric(this string str)
        {
			if (string.IsNullOrEmpty(str))
				return false;

            return Regex.IsMatch(str, @"^[+-]?\d*[.]?\d*$");
        }

        public static bool IsInt(this string str)
        {
			if (string.IsNullOrEmpty(str))
				return false;

            return Regex.IsMatch(str, @"^[+-]?\d*$");
        }

        public static bool IsUnsign(this string str)
        {
			if (string.IsNullOrEmpty(str))
				return false;

            return Regex.IsMatch(str, @"^\d*[.]?\d*$");
        }

        public static bool IsTel(this string str)
        {
			if (string.IsNullOrEmpty(str))
				return false;

            return Regex.IsMatch(str, @"\d{3}-\d{8}|\d{4}-\d{7}");
        }

        public static string GetFileName(string path, char separator = '/')
        {
			if (string.IsNullOrEmpty(path))
				return string.Empty;

            return path.Substring(path.LastIndexOf(separator) + 1);
        }

        public static string GetFileExtention(string path)
        {
			if (string.IsNullOrEmpty(path))
				return string.Empty;

			if (path.LastIndexOf('.') < 0)
				return string.Empty;

            return path.Substring(path.LastIndexOf("."));
        }

        public static string GetDirectoryName(string fileName)
        {
			if (string.IsNullOrEmpty(fileName))
				return string.Empty;

            return fileName.Substring(0, fileName.LastIndexOf('/'));
        }

        public static string PathNormalize(this string str)
        {
			if (string.IsNullOrEmpty(str))
				return string.Empty;

            return str.Replace("\\", "/").ToLower();
        }

        public static string ReplaceString(string s, string o, string n)
        {
			if (string.IsNullOrEmpty(s))
				return string.Empty;

            if( s.IndexOf(o) < 0 )
                return s;

            return s.Replace(o, n);
        }

        public static string ReplaceString(string s, char o, char n)
        {
			if (string.IsNullOrEmpty(s))
				return string.Empty;

            bool bFound = false;
            for (int i = 0; i < s.Length; ++ i)
            { 
                if( s[i] == o )
                {
                    bFound = true;

                    break;
                }
            }

            if (!bFound)
                return s;

            return s.Replace(o, n);
        }

		public static string AssetPathNormalize(string path)
		{
			if (string.IsNullOrEmpty(path))
				return string.Empty;

            bool upper = false;
            bool slash = false;
            for (int i = 0; i < path.Length; ++ i)
            {
                char c = path[i];
                if (!upper && Char.IsUpper(c))
                    upper = true;
                if (c == '\\')
                    slash = true;
                if (upper && slash)
                    break;
            }

            if( slash)
                path = path.Replace('\\', '/');
            if( upper)
                path = path.ToLower();
            if (path.Length >= 7 && path.Substring( 0, 7 ) == "assets/")
				path = path.Substring(7);
            if (path.Length >= 10 && path.Substring( 0, 10 ) == "resources/")
				path = path.Substring(10);

			return path;
		}

        public static string AssetPathNormalizeUpper(string path)
        {
			if (string.IsNullOrEmpty(path))
				return string.Empty;

            path = path.Replace("\\", "/");
            path = path.Replace("E:/ns/Editor/", "");

            return path;
        }

        public static string GetFilePathWithoutExtention(string fileName)
        {
			if (string.IsNullOrEmpty(fileName))
				return string.Empty;

            if (fileName.LastIndexOf('.') < 0)
                return fileName;

            return fileName.Substring(0, fileName.LastIndexOf('.'));
        }

        public static string GetFileNameWithoutExtention(string fileName, char separator = '/')
        {
			if (string.IsNullOrEmpty(fileName))
				return string.Empty;

            var name = GetFileName(fileName, separator);

            return GetFilePathWithoutExtention(name);
        }

        public static void CreateFile(string path, byte[] bytes, bool overwrite = true, bool icase = false)
        {
			if (string.IsNullOrEmpty(path))
				return;

            if (icase)
                path = path.ToLower();

            string targetPath = System.IO.Path.GetDirectoryName(path);

            CreateFolder(targetPath);

            System.IO.FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Create);
            if (null != bytes && bytes.Length > 0)
                stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
            stream.Close();
        }

        public static void AppendFile(string path, byte[] bytes)
        {
            System.IO.FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Append);
            if (null != bytes && bytes.Length > 0)
                stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
            stream.Close();
        }

        public static void CopyFile(string sourceFile, string targetFile, bool overwrite = true, bool icase = false)
        {
            if (!System.IO.File.Exists(sourceFile))
                return;

            string targetPath = System.IO.Path.GetDirectoryName(targetFile);
            if (icase)
                targetPath = targetPath.ToLower();

            CreateFolder(targetPath);

            if (icase)
                System.IO.File.Copy(sourceFile, targetFile.ToLower(), overwrite);
            else
                System.IO.File.Copy(sourceFile, targetFile, overwrite);
        }

        public static void DeleteFile(string fileName)
        {
            try
            {
                if (System.IO.File.Exists(fileName))
                    System.IO.File.Delete(fileName);
            } catch(Exception e)
            {
                ULogger.Warn(e.Message);
            }
        }

        public static bool IsEmptyFolder(string targetPath, bool ignoreMetaFile)
        {
            bool isEmpty = true;

            if (!System.IO.Directory.Exists(targetPath))
                return true;

            var files = System.IO.Directory.GetFiles(targetPath, "*.*", System.IO.SearchOption.AllDirectories);
            if (!ignoreMetaFile)
            {
                isEmpty = (files.Count() == 0);
            } else
            {
                foreach (var item1 in files)
                {
                    if (GetFileExtention(item1).ToLower() != ".meta")
                    {
                        isEmpty = false;

                        break;
                    }
                }
            }

            return isEmpty;
        }

        public static void CopyFolder(string targetPath, string sourcePath, string extention, bool recursive = false, bool icase = false)
        {
            if (!System.IO.Directory.Exists(sourcePath))
                return;

            var files = System.IO.Directory.GetFiles(sourcePath);
            foreach (var item1 in files)
            {
                var item = item1.Replace("\\", "/");
                if (item.EndsWith(extention, System.StringComparison.OrdinalIgnoreCase))
                    CopyFile(item, System.IO.Path.Combine(targetPath, System.IO.Path.GetFileName(item)), true, icase);
            }

            if (recursive)
            {
                var dirs = System.IO.Directory.GetDirectories(sourcePath);
                foreach (var dir in dirs)
                {
                    string dirName = System.IO.Path.GetFileName(dir);

                    CopyFolder(targetPath + "/" + dirName, dir, extention, recursive, icase);
                }
            }
        }

        public static void CreateFolder(string path)
        {
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
        }

        public static void DeleteFolder(string path)
        {
            try
            {
                if (System.IO.Directory.Exists(path))
                    System.IO.Directory.Delete(path, true);
            } catch(Exception e)
            {
                ULogger.Warn(e.Message);
            }
        }

        public static void EmptyFolder(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);

                return;
            }

            var files = System.IO.Directory.GetFiles(path, "*", System.IO.SearchOption.AllDirectories);
            foreach (var file in files)
            {
                try
                {
                    System.IO.File.Delete(file);
                } catch
                { }
            }

            var dirs = System.IO.Directory.GetDirectories(path, "*", System.IO.SearchOption.AllDirectories);
            foreach (var dir in dirs)
            {
                try
                {
                    System.IO.Directory.Delete(dir);
                }
                catch
                { }
            }
        }

        public static void MoveFolder(string targetPath, string sourcePath)
        {
            EmptyFolder(targetPath);
            CopyFolderEx(targetPath, sourcePath, new string[] { }, true);
            DeleteFolder(sourcePath);
        }

        public static void CopyFolderEx(string targetPath, string sourcePath, string[] excludeExts, bool recursive = false, bool icase = false)
        {
            if (!System.IO.Directory.Exists(sourcePath))
                return;

            var files = System.IO.Directory.GetFiles(sourcePath);
            foreach (var file in files)
            {
                var item = file.Replace("\\", "/");

                var ext = System.IO.Path.GetExtension(item);
                if (!excludeExts.Contains(ext))
                    CopyFile(item, System.IO.Path.Combine(targetPath, System.IO.Path.GetFileName(item)), true, icase);
            }

            if (recursive)
            {
                var dirs = System.IO.Directory.GetDirectories(sourcePath);
                foreach (var dir in dirs)
                {
                    string dirName = System.IO.Path.GetFileName(dir);

                    CopyFolderEx(targetPath + "/" + dirName, dir, excludeExts, recursive, icase);
                }
            }
        }

        public static string CombinePath(string p1, string p2)
        {
            return System.IO.Path.Combine(p1, USystemConfig.Instance.IsDevelopMode ? p2 : p2.ToLower()).Replace("\\", "/");
        }

        public static string ReplaceFirst(this string input, string oldValue, string newValue, int startAt = 0)
        {
            int pos = input.IndexOf(oldValue, startAt);
            if (pos < 0)
                return input;

            return string.Concat(input.Substring(0, pos), newValue, input.Substring(pos + oldValue.Length));
        }

        public static string ReplaceLast(this string input, string oldValue, string newValue)
        {
            int pos = input.LastIndexOf(oldValue);
            if (pos < 0)
                return input;

            return string.Concat(input.Substring(0, pos), newValue, input.Substring(pos + oldValue.Length));
        }

        public static T GetChild< T >(GameObject go, string subnode) where T : Component
        {
            if (null != go)
            {
                Transform sub = go.transform.Find(subnode);
                if (sub != null)
                    return sub.GetComponent< T >();
            }

            return null;
        }

        public static T GetChild< T >(Transform go, string subnode) where T : Component
        {
            if (null != go)
            {
                Transform sub = go.Find(subnode);
                if (sub != null)
                    return sub.GetComponent< T >();
            }

            return null;
        }

        public static T AddComponent< T >(GameObject go) where T : Component
        {
            if (null != go)
            {
                T[] ts = go.GetComponents< T >();
                for (int i = 0; i < ts.Length; i ++)
                {
                    if (ts[i] != null)
                        GameObject.Destroy(ts[i]);
                }

                return go.gameObject.AddComponent< T >();
            }

            return null;
        }

        public static Component AddComponent(GameObject go, Type componentType)
        {
            if (null != go)
            {
                Component[] ts = go.GetComponents(componentType);
                for (int i = 0; i < ts.Length; i ++)
                {
                    if (ts[i] != null)
                        GameObject.DestroyImmediate(ts[i]);
                }

                return go.gameObject.AddComponent(componentType);
            }

            return null;
        }

        public static T AddComponent< T >(Transform go) where T : Component
        {
            return AddComponent< T >(go.gameObject);
        }

        public static Component AddComponent(Transform go, Type componentType)
        {
            return AddComponent(go.gameObject, componentType);
        }

        public static T GetComponent< T >(GameObject go) where T : Component
        {
            if (null != go)
                return go.GetComponent< T >();

            return null;
        }

        public static Component GetComponent(GameObject go, Type componentType)
        {
            if (null != go)
                return go.GetComponent(componentType);

            return null;
        }

        public static T GetComponent< T >(Transform go) where T : Component
        {
            return GetComponent< T >(go.gameObject);
        }

        public static Component GetComponent(Transform go, Type componentType)
        {
            return GetComponent(go.gameObject, componentType);
        }

		public static T[] GetComponents< T >(GameObject go) where T : Component
		{
			if (null != go)
				return go.GetComponents< T >();

			return null;
		}

		public static Component[] GetComponents(GameObject go, Type componentType)
		{
			if (null != go)
				return go.GetComponents(componentType);

			return null;
		}

		public static T[] GetComponents< T >(Transform go) where T : Component
		{
			return GetComponents< T >(go.gameObject);
		}

		public static Component[] GetComponents(Transform go, Type componentType)
		{
			return GetComponents(go.gameObject, componentType);
		}

		public static T GetComponentInChildren< T >(GameObject go, bool includeInactive) where T : Component
        {
            if (null != go)
                return go.GetComponentInChildren< T >(includeInactive);

            return null;
        }

        public static Component GetComponentInChildren(GameObject go, Type componentType, bool includeInactive)
        {
            if (null != go)
                return go.GetComponentInChildren(componentType, includeInactive);

            return null;
        }

        public static T GetComponentInChildren< T >(Transform go, bool includeInactive) where T : Component
        {
            return GetComponentInChildren< T >(go.gameObject, includeInactive);
        }

		public static Component GetComponentInChildren(Transform go, Type componentType, bool includeInactive)
        {
            return GetComponentInChildren(go.gameObject, componentType, includeInactive);
        }

		public static T[] GetComponentsInChildren< T >(GameObject go, bool includeInactive) where T : Component
		{
			if (null != go)
				return go.GetComponentsInChildren< T >(includeInactive);

			return null;
		}

		public static Component[] GetComponentsInChildren(GameObject go, Type componentType, bool includeInactive)
		{
			if (null != go)
				return go.GetComponentsInChildren(componentType, includeInactive);

			return null;
		}

		public static T[] GetComponentsInChildren< T >(Transform go, bool includeInactive) where T : Component
		{
			return GetComponentsInChildren< T >(go.gameObject, includeInactive);
		}

		public static Component[] GetComponentsInChildren(Transform go, Type componentType, bool includeInactive)
		{
			return GetComponentsInChildren(go.gameObject, componentType, includeInactive);
		}

        public static void RmvComponent< T >(GameObject go) where T : Component
        {
            if (null != go)
            {
                T[] ts = go.GetComponents< T >();
                for (int i = 0; i < ts.Length; i ++)
                {
                    if (ts[i] != null)
						GameObject.DestroyImmediate(ts[i]);
                }
            }
        }

        public static void RmvComponent(GameObject go, Type componentType)
        {
            if (null != go)
            {
                Component[] ts = go.GetComponents(componentType);
                for (int i = 0; i < ts.Length; i ++)
                {
                    if (ts[i] != null)
						GameObject.DestroyImmediate(ts[i]);
                }
            }
        }

        public static void RmvComponent< T >(Transform go) where T : Component
        {
            RmvComponent< T >(go.gameObject);
        }

        public static void RmvComponent(Transform go, Type componentType)
        {
            RmvComponent(go.gameObject, componentType);
        }

        public static GameObject FindChild(GameObject go, string subnode)
        {
            return FindChild(go.transform, subnode);
        }

        public static GameObject FindChild(Transform go, string subnode)
        {
            Transform tran = go.Find(subnode);
            if (tran == null)
                return null;

            return tran.gameObject;
        }

        public static GameObject FindSibling(GameObject go, string subnode)
        {
            return FindSibling(go.transform, subnode);
        }

        public static GameObject FindSibling(Transform go, string subnode)
        {
            Transform tran = go.parent.Find(subnode);
            if (tran == null)
                return null;

            return tran.gameObject;
        }

        public delegate bool ChildCheck(GameObject obj);

        public static void TraverseChild(GameObject go, ChildCheck cb)
        {
			if (null != go)
				TraverseChild(go.transform, cb);
        }

        public static void TraverseChild(Transform go, ChildCheck cb)
        {
			if (null != go)
			{
				if (cb(go.gameObject))
				{
					for (int i = 0; i < go.childCount; ++ i)
						TraverseChild(go.GetChild(i), cb);
				}
			}
        }

        public static GameObject GetChild(this GameObject go, string name)
        {
            GameObject child = null;

            UCoreUtil.TraverseChild(go, (GameObject obj) =>
            {
                if (obj.name == name)
                {
                    child = obj;

                    return false;
                }

                return true;
            });

            return child;
        }

        public static GameObject FindObject(this UnityEngine.SceneManagement.Scene scene, string name)
        {
            var objs = scene.GetRootGameObjects();
			for (int i = 0; i < objs.Length; ++ i)
			{
				var obj = objs[i];
				if (obj.name == name)
					return obj;
			}

            return null;
        }

        public static GameObject FindObjectWithTag(this UnityEngine.SceneManagement.Scene scene, string tag)
        {
            GameObject child = null;

            var objs = scene.GetRootGameObjects();
			for (int i = 0; i < objs.Length; ++ i)
			{
				var obj = objs[i];

				UCoreUtil.TraverseChild(obj, (GameObject go) =>
				{
					if (go.tag == tag)
					{
						child = go;

						return false;
					}

					return true;
				});
			}

            return child;
        }

        public static void Vibrate()
        {
        }

        public static TValue ListGet< TValue >(this List< TValue > list, int idx)
        {
            return list[idx];
        }

        public static string ListPack< TValue >(this List< TValue > list)
        {
            if (list.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
				for (int i = 0; i < list.Count; ++ i)
					sb.AppendFormat("{0}{1}", list[i], ',');
                sb.Remove(sb.Length - 1, 1);

                return sb.ToString();
            }

            return "";
        }

        public static TValue GetValueOrDefault< TKey, TValue >(this IDictionary< TKey, TValue > dictionary, TKey key)
        {
            TValue value = default(TValue);

            dictionary.TryGetValue(key, out value);

            return value;
        }

        public static TValue GetValueOrDefault< TKey, TValue >(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            TValue value;

            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static TValue GetValueOrDefault< TKey, TValue >(this IDictionary< TKey, TValue > dictionary, TKey key, Func< TValue > provider)
        {
            TValue value;

            return dictionary.TryGetValue(key, out value) ? value : provider();
        }

        public static TValue Get< TKey, TValue >(this IDictionary< TKey, TValue > dictionary, TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);

            return value;
        }

        // 效率问题，运行时不要使用
		public static TKey DictGetKey< TKey, TValue >(this Dictionary< TKey, TValue > dict, int idx)
		{
			int i = 0;
			foreach (var v in dict)
			{
				if (i ++ == idx)
					return v.Key;
			}

			return default(TKey);
		}

        public static TValue DictGetValue< TKey, TValue >(this Dictionary< TKey, TValue > dict, int idx)
        {
            int i = 0;
            foreach (var v in dict)
            {
                if (i ++ == idx)
                    return v.Value;
            }

            return default(TValue);
        }

        public static void ForEach< T >(this IEnumerable< T > collection, Action< T > action)
        {
            if (null == collection)
                return;

            foreach (var item in collection)
                action(item);
        }

        public static IEnumerable< R > MapTo< T, R >(this IEnumerable< T > collection, Func< T, R > action)
        {
            if (null == collection)
                yield break;

            foreach (var item in collection)
                yield return action(item);
        }

        public static IEnumerable< T > Filter< T >(this IEnumerable< T > collection, Func< T, bool > action)
        {
            if (null == collection)
                yield break;

            foreach (var item in collection)
            {
                if (action(item))
                    yield return item;
            }
        }
/*
        public static T[] ToArray< T >(this IEnumerable< T > collection)
        {
            if (null == collection)
                return null;

            return new List< T >(collection).ToArray();
        }
*/

        public static T[] Sort< T >(this IEnumerable< T > collection, Comparison< T > action)
        {
            var array = collection.ToArray();
                Array.Sort(array, action);

            return array;
        }

        public static T[] OrderBy< T, R >(this IEnumerable< T > collection, Func< T, R > action) where R : IComparable< R >
        {
            var values = collection.ToArray();

            var keys = collection.MapTo(action).ToArray();
                Array.Sort(keys, values);

            return values;
        }

        public static int Count< T >(this IEnumerable< T > collection)
        {
            int len = 0;
            foreach (var item in collection)
                len ++;

            return len;
        }

        public static int Count< T >(this IEnumerable< T > collection, Func< T, bool > action)
        {
            int len = 0;
            foreach (var item in collection)
            {
                if (action(item))
                    len ++;
            }

            return len;
        }

        public static bool Any< T >(this IEnumerable< T > collection, Func< T, bool > action)
        {
            foreach (var item in collection)
            {
                if (action(item))
                    return true;
            }

            return false;
        }

        public static bool All< T >(this IEnumerable< T > collection, Func< T, bool > action)
        {
            foreach (var item in collection)
            {
                if (!action(item))
                    return false;
            }

            return true;
        }

        public static T FirstOrDefault< T >(this IEnumerable< T > collection, T defaultValue = default(T))
        {
            T result = defaultValue;
            foreach (var item in collection)
            {
                result = item;

                break;
            }

            return result;
        }

        public static T FirstOrDefault< T >(this IEnumerable< T > collection, Func< T, bool > predicate, T defaultValue = default(T))
        {
            T result = defaultValue;
            foreach (var item in collection)
            {
                if (predicate(item))
                {
                    result = item;

                    break;
                }
            }

            return result;
        }

        public static List< T > ToList< T >(this IEnumerable< T > collection)
        {
            return new List< T >(collection);
        }

        public static HashSet< T > ToHashSet< T >(this IEnumerable< T > collection)
        {
            return new HashSet< T >(collection);
        }

        public static double Sum(this IEnumerable< double > collection)
        {
            double result = 0;
            foreach (var item in collection)
                result += item;

            return result;
        }

        public static double Sum(this IEnumerable< float > collection)
        {
            double result = 0;
            foreach (var item in collection)
                result += item;

            return result;
        }

        public static long Sum(this IEnumerable< int > collection)
        {
            long result = 0;
            foreach (var item in collection)
                result += item;

            return result;
        }

        public static long Sum(this IEnumerable< long > collection)
        {
            long result = 0;
            foreach (var item in collection)
                result += item;

            return result;
        }

        public static long Sum(this IEnumerable< short > collection)
        {
            long result = 0;
            foreach (var item in collection)
                result += item;

            return result;
        }

        public static ulong Sum(this IEnumerable< ushort > collection)
        {
            ulong result = 0;
            foreach (var item in collection)
                result += item;

            return result;
        }

        public static ulong Sum(this IEnumerable< uint > collection)
        {
            ulong result = 0;
            foreach (var item in collection)
                result += item;

            return result;
        }

        public static ulong Sum(this IEnumerable< ulong > collection)
        {
            ulong result = 0;
            foreach (var item in collection)
                result += item;

            return result;
        }

        public static float Average(this IEnumerable< float > collection)
        {
            int len = 0;
            foreach (var item in collection)
                len++;

            if (len == 0)
                return 0;

            return (float)(collection.Sum() / len);
        }

        public static string[] Walk(string path, string ext)
        {
            if (System.IO.Directory.Exists(path))
            {
                var paths = System.IO.Directory.GetFiles(path, ext, System.IO.SearchOption.AllDirectories);

                return paths;
            }

            return kEmptyPaths;
        }

		public static Vector2 xz(this Vector3 vv)
		{
			return new Vector2(vv.x, vv.z);
		}

		public static float Distance2D(this Vector3 v1, Vector3 v2)
		{
			return Vector2.Distance(new Vector2(v1.x, v1.z), new Vector2(v2.x, v2.z));
		}

		public static bool Equals2D(this Vector3 v1, Vector3 v2)
		{
			return Vector2.Equals(new Vector2(v1.x, v1.z), new Vector2(v2.x, v2.z));
		}

        public static string EncodeXmlText(string text)
        {
            byte[] bytes = Encoding.Default.GetBytes(text);

            return Convert.ToBase64String(bytes);
        }

        public static string DecodeXmlText(string text)
        {
            byte[] bytes = Convert.FromBase64String(text);

            return Encoding.Default.GetString(bytes);
        }

        public static void Foreach(ArrayList a, Action< object > v)
        {
            if (null != a)
            {
				for (int i = 0; i < a.Count; ++ i)
					v(a[i]);
            }
        }

        public static string GetRealAgentType(string etype)
        {
            return etype.Substring(etype.LastIndexOf(".") + 1);
        }

		public static Color HexToColor(uint data)
		{
			byte r = (byte)((data >> 24) & 0xff);
			byte g = (byte)((data >> 16) & 0xff);
			byte b = (byte)((data >>  8) & 0xff);
			byte a = (byte)((data >>  0) & 0xff);

			return new Color32(r, g, b, a);
		}

        public static string Base64ToUTF8String(string str)
        {
            try
            {
                byte[] bpath = Convert.FromBase64String(str);
                return System.Text.Encoding.UTF8.GetString(bpath);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static long UnixTime(DateTime d)
        {
            TimeSpan ts = d.ToUniversalTime() - new DateTime(1970, 1, 1);
            return (long)ts.TotalSeconds;
        }
    }
}
