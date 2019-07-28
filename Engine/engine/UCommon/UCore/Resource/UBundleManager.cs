using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Security;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Object = UnityEngine.Object;

namespace UEngine
{
	class UCreateTaskManager : MonoBehaviour
	{ }

	public class UBundleManager
	{
		public delegate void HandleFinishDownload(string name, WWW res, bool succeeced);

		private static List< string >
			mLocalResources = new List< string >();

		private static AssetBundleManifest
			mManifest = null;

		private static Dictionary< string, UBundleMetaData >
			mBundleMetas = new Dictionary< string, UBundleMetaData >();

		private static Dictionary< string, string >
			mBundleNames = new Dictionary< string, string >();

		private static Dictionary< string, UBundle >
			mBundles = new Dictionary< string, UBundle >();

		private static float
			mTidyTick = 0.0f;
/*
		private static float
			mTidyAllTick = 0.0f;
*/

		public class SCreateTask
		{
			public IEnumerator
				mTask;
			public List< IEnumerator >
				mCreateJobs;
			public bool
				mIsFinished;
		}
		private static SCreateTask[] mCreateTasks = null;

		static UBundleManager()
		{
			mBundles.Clear();

			mCreateTasks = new SCreateTask[USystemConfig.Instance.MaxLoadTask];
		}

		public static IEnumerator CheckLocalAsset(Action< bool > check)
		{
			if (Application.genuineCheckAvailable)
			{
				if (!Application.genuine)
				{
					UCore.QuiteGame("游戏客户端发布后被修改过！");

					if (null != check)
						check(false);

					yield break;
				}
			}

			UCoreUtil.CreateFolder(USystemConfig.ResourceFolder);

			if (System.IO.Directory.Exists(USystemConfig.ResourceFolder + USystemConfig.kPkgFolder))
			{
				if (null != check)
					check(true);

				yield break;
			}

			if (System.IO.File.Exists(USystemConfig.ResourceFolder + USystemConfig.kResCollect.ToLower()))
			{
				if (null != check)
					check(true);

				yield break;
			}

			WWW www = new WWW(USystemConfig.AssetPath + USystemConfig.kResCollect.ToLower());
			while (www.isDone == false)
				yield return www;

			if (!string.IsNullOrEmpty(www.error))
			{
				ULogger.Warn(string.Format("load resource collect {0} failed {1}", USystemConfig.kResCollect, www.error));

				if (null != check)
					check(false);

				yield break;
			}

			UCoreUtil.CreateFile(USystemConfig.ResourceFolder + USystemConfig.kResCollect.ToLower(), www.bytes);

			var xml = UXMLParser.LoadXML(www.text);
			if (null != xml && null != xml.Children)
			{
				for (int i = 0; i < xml.Children.Count; ++ i)
				{
					System.Security.SecurityElement item = xml.Children[i] as System.Security.SecurityElement;

					System.Security.SecurityElement key = item.Children[0] as System.Security.SecurityElement;

					mLocalResources.Add(key.Text.ToLower());
				}
			}
			www.Dispose();
			www = null;

			int resCount = mLocalResources.Count;

			HandleFinishDownload loaded = delegate(string path, WWW res, bool succeeded)
			{
				if (null != res)
					UCoreUtil.CreateFile(USystemConfig.ResourceFolder + path, res.bytes);
			};

			int tasks = 0;

			for (int i = 0; i < resCount; ++ i)
			{
				if (!File.Exists(USystemConfig.ResourceFolder + mLocalResources[i]))
				{
					tasks ++;
					if (tasks >= USystemConfig.Instance.SynCreateRequestCount)
					{
						tasks = 0;

						yield return UCore.GameEnv.StartCoroutine(DownLoad(USystemConfig.AssetPath, mLocalResources[i], loaded));
					} else
					{
						UCore.GameEnv.StartCoroutine(DownLoad(USystemConfig.AssetPath, mLocalResources[i], loaded));
					}
				} else
				{
					loaded(mLocalResources[i], null, true);

					yield return null;
				}
			}

			if (null != check)
				check(true);
		}

		public static void LoadAssetManifest()
		{
			if (mManifest == null)
			{
				AssetBundle manifestBundle = AssetBundle.LoadFromMemory(UFileAccessor.ReadBinaryFile(USystemConfig.kAssetFileName));
				if (manifestBundle)
					mManifest = (AssetBundleManifest)manifestBundle.LoadAsset("AssetBundleManifest");
			}

			if (mManifest != null)
			{
				string[] bundles = mManifest.GetAllAssetBundles();
				for (int i = 0; i < bundles.Length; ++ i)
				{
					string path = bundles[i];
					if (string.IsNullOrEmpty(path))
						continue;

					string name = path.Substring(0, path.LastIndexOf(".u")).ReplaceLast("_", ".");

					var meta = new UBundleMetaData();
						meta.Name = name;
						meta.RelativePath = path;
						meta.Dependencies = mManifest.GetAllDependencies(path);

					mBundleNames[path] = name;
					mBundleMetas[name] = meta;
				}

                Dictionary<string, UBundleMetaData>.Enumerator it = mBundleMetas.GetEnumerator();
                while( it.MoveNext())
				{
                    var meta = it.Current.Value;
					for (int i = 0; i < meta.Dependencies.Length; ++ i)
						meta.Dependencies[i] = GetBundleName(meta.Dependencies[i]);
				}
			}
		}

		public static string GetBundleName(string path)
		{
			string name = null;
			if (mBundleNames.TryGetValue(path, out name))
				return name;

			return null;
		}

		public static string GetBundlePath(string path)
		{
			path = UCoreUtil.AssetPathNormalize(path);

			UBundleMetaData meta = null;
			if (mBundleMetas.TryGetValue(path, out meta))
				return meta.RelativePath;

			return null;
		}

		public static UBundleMetaData GetBundleMeta(string path)
		{
			path = UCoreUtil.AssetPathNormalize(path);

			UBundleMetaData meta = null;
			if (mBundleMetas.TryGetValue(path, out meta))
				return meta;

			return null;
		}

		public static UBundle GetBundle(string path)
		{
			path = UCoreUtil.AssetPathNormalize(path);

			if (mBundles.ContainsKey(path))
				return mBundles[path];

			return null;
		}

		public static void StartCreateTasks()
		{
			if (null != UCore.GameEnv)
			{
				for (int i = 0; i < Mathf.Min(USystemConfig.Instance.MaxLoadTask, mCreateTasks.Length); ++ i)
				{
					mCreateTasks[i] = new SCreateTask()
					{
						mCreateJobs = new List< IEnumerator >(128),
						mIsFinished = false
					};
					mCreateTasks[i].mTask = CreateBundleTask(mCreateTasks[i]);

					UCore.GameEnv.StartCoroutine(mCreateTasks[i].mTask);
				}
			}
		}

		public static void StopCreateTasks()
		{
			if (null != UCore.GameEnv)
			{
				for (int i = 0; i < Mathf.Min(USystemConfig.Instance.MaxLoadTask, mCreateTasks.Length); ++ i)
				{
					if (null != mCreateTasks[i])
						UCore.GameEnv.StopCoroutine(mCreateTasks[i].mTask);
				}
			}
		}

		public static void AddCreateTask(IEnumerator ct)
		{
			SCreateTask task = mCreateTasks[0];
			for (int i = 0; i < Mathf.Min(USystemConfig.Instance.MaxLoadTask, mCreateTasks.Length); ++ i)
			{
				if (mCreateTasks[i].mCreateJobs.Count < task.mCreateJobs.Count)
					task = mCreateTasks[i];
			}

			task.mCreateJobs.Add(ct);
		}

		public static void FinishCreateTask(string path)
		{
/*
			SCreateTask task;
			if (mCreateTask.TryGetValue(path, out task))
				task.mIsFinished = true;
*/
		}

		public static UBundle CreateBundle(string path)
 		{
			return new UBundle(UCoreUtil.AssetPathNormalize(path));
		}

#if USE_MEMORY_BUNDLE
		public static bool BuildBundle< T >(UBundle bundle, byte[] data, Action< IResource > onCreated) where T : Object
#else
		public static bool BuildBundle< T >(UBundle bundle, string filePath, int fileOffset, Action< IResource > onCreated) where T : Object
#endif
		{
			if (null == bundle)
				return false;

#if USE_MEMORY_BUNDLE
			bundle.Create< T >(data, (res)=>
#else
			bundle.Create< T >(filePath, (ulong)fileOffset, (res)=>
#endif
			{
				if (!res)
					ULogger.Error("failed load bundle {0}", bundle.RelativePath);

				AddBundle(bundle.RelativePath, bundle);

				if (null != onCreated)
					onCreated(bundle);
			});

			return true;
		}

#if USE_MEMORY_BUNDLE
		public static bool SyncBuildBundle< T >(UBundle bundle, byte[] data) where T : Object
#else
		public static bool SyncBuildBundle< T >(UBundle bundle, string filePath, int fileOffset) where T : Object
#endif
		{
			if (null == bundle)
				return false;
#if USE_MEMORY_BUNDLE
				bundle.SyncCreate< T >(data);
#else
				bundle.SyncCreate< T >(filePath, (ulong)fileOffset);
#endif
			AddBundle(bundle.RelativePath, bundle);

			return true;
		}

		public static bool ReleaseBundle(string path)
		{
			path = UCoreUtil.AssetPathNormalize(path);

			UBundle bundle = null;
			if (mBundles.TryGetValue(path, out bundle))
			{
				var refCnt = bundle.Release();
				if (refCnt <= 0)
				{
/*
					if (refCnt == 0)
						DelReferenceCount(bundle, false);
*/
					return true;
				}
			}

			return false;
		}

		public static bool ReleaseBundle(UBundle bundle)
		{
			if (null != bundle)
			{
				var refCnt = bundle.Release();
				if (refCnt <= 0)
				{
/*
					if (refCnt == 0)
						DelReferenceCount(bundle, false);
*/
					return true;
				}
			}

			return false;
		}

		public static void UnloadBundle(string path)
		{
			path = UCoreUtil.AssetPathNormalize(path);

			UBundle bundle = null;
			if (mBundles.TryGetValue(path, out bundle))
				bundle.Unload(false);
		}

		public static void AddBundle(string path, UBundle bundle)
		{
			path = UCoreUtil.AssetPathNormalize(path);

			if (!mBundles.ContainsKey(path))
				mBundles.Add(path, bundle);
		}

		public static UBundle RmvBundle(string path)
		{
			path = UCoreUtil.AssetPathNormalize(path);

			UBundle bundle = null;
			if (!mBundles.TryGetValue(path, out bundle))
				mBundles.Remove(path);

			return bundle;
		}

		public static void AddReferenceCount(IResource asset)
		{
			var resList = UBundleManager.GetAssetInfo(asset);
			for (int i = 0; i < resList.Count; ++ i)
			{
				var item = resList[i];
				if (null != item)
					item.AddRef();
			}
		}

		public static void DelReferenceCount(IResource asset, bool containSelf = false)
		{
			var resList = UBundleManager.GetAssetInfo(asset, containSelf);
			for (int i = 0; i < resList.Count; ++ i)
			{
				var item = resList[i];
				if (null != item)
					item.Release();
			}
		}

		public static List< IResource > GetAssetInfo(IResource asset, bool containSelf = true)
		{
			var list = new List< IResource >();
			if (!string.IsNullOrEmpty(asset.RelativePath))
				list.AddRange(GetDeepDependencies(asset.RelativePath));
			else
				ULogger.Error("there is a none path dependency");

			if (containSelf)
				list.Add(asset);

			list = list.Distinct().ToList();

			return list;
		}


		public static List< string > GetAssetInfoEX(string path, bool containSelf = true)
		{
			var list = new List< string >();
			if (!string.IsNullOrEmpty(path))
				list.AddRange(GetDeepDependenciesEX(path));
			else
				ULogger.Error("there is a none path dependency");

			if (containSelf)
				list.Add(path);

			list = list.Distinct().ToList();

			return list;
		}

		public static string Print()
		{
			return string.Format("Total asset bundle {0}", mBundles.Count);
		}

		private static List< IResource > GetDeepDependencies(string relativePath)
		{
			var result = new List< IResource >();
			var list = GetDependencies(relativePath);
			if (list != null)
			{
				for (int i = 0; i < list.Count; ++ i)
				{
					var item = list[i];

					if (!string.IsNullOrEmpty(item.RelativePath))
						result.AddRange(GetDeepDependencies(item.RelativePath));
					else
						ULogger.Error("there is a none path dependency");
				}
				result.AddRange(list);
			}

			return result;
		}

		private static List< string > GetDeepDependenciesEX(string relativePath)
		{
			var result = new List< string >();
			var list = GetDependenciesEX(relativePath);
			if (list != null)
			{
				for (int i = 0; i < list.Count; ++ i)
				{
					var item = list[i];

					if (!string.IsNullOrEmpty(item))
						result.AddRange(GetDeepDependenciesEX(item));
					else
						ULogger.Error("there is a none path dependency");
				}
				result.AddRange(list);
			}

			return result;
		}

		private static List< IResource > GetDependencies(string relativePath)
		{
			var meta = GetBundleMeta(relativePath);
			if (null == meta)
				return null;

			string name = meta.Name;

			var dependencyPaths = meta.Dependencies;

			if (dependencyPaths == null || dependencyPaths.Length == 0)
				return null;

			var dependencies = new List< IResource >();

			for (int i = 0; i < dependencyPaths.Length; ++ i)
			{
				var dependencyPath = dependencyPaths[i];

				if (!string.IsNullOrEmpty(dependencyPath))
				{
					var res = GetBundle(dependencyPath);
					if (null != res)
						dependencies.Add(res);
					else
						ULogger.Error("failed get depend {0}", dependencyPath);
				}
			}

			return dependencies;
		}

		private static List< string > GetDependenciesEX(string relativePath)
		{
			var meta = GetBundleMeta(relativePath);
			if (null == meta)
				return null;

			string name = meta.Name;

			var dependencyPaths = meta.Dependencies;

			if (dependencyPaths == null || dependencyPaths.Length == 0)
				return null;

			var dependencies = new List< string >();

			for (int i = 0; i < dependencyPaths.Length; ++ i)
			{
				var dependencyPath = dependencyPaths[i];

				if (!string.IsNullOrEmpty(dependencyPath))
					dependencies.Add(dependencyPath);
			}

			return dependencies;
		}

        private static List< string > mTmpListForRmove = new List< string >(128);

		public static void Tidy(bool releaseAll = true)
		{
			//List< string > rmvKeys = new List< string >();
            mTmpListForRmove.Clear();

            List< string > rmvKeys = mTmpListForRmove;

            Dictionary< string, UBundle >.Enumerator it = mBundles.GetEnumerator();
            while (it.MoveNext())
			{
                var v = it.Current;

				var bundle = v.Value;

				if (bundle.IsDone && bundle.RefCnt <= 0)
				{
					bundle.Unload(true);

					rmvKeys.Add(v.Key);
				}
			}

			for (int i = 0; i < rmvKeys.Count; ++ i)
				mBundles.Remove(rmvKeys[i]);

			if (releaseAll)
			{
				UnloadUnusedResources(() =>
				{
					GC.Collect();
					//GC.WaitForPendingFinalizers();
				});
			}
		}

		public static void Update()
		{
			// tidy per ten minute
			if ((Time.realtimeSinceStartup - mTidyTick) >= USystemConfig.Instance.BundleTidyTick)
			{
				mTidyTick = Time.realtimeSinceStartup;

				Tidy(false);
			}
/*
			if ((Time.realtimeSinceStartup - mTidyAllTick) >= USystemConfig.Instance.BundleTidyAllTick)
			{
				mTidyAllTick = Time.realtimeSinceStartup;

				Tidy(true);
			}
*/
		}

		public static void UnloadUnusedResources(Action cb)
		{
			UCore.GameEnv.StartCoroutine(UnloadUnusedAssets(() =>
			{
#if UNITY_2017
				Caching.ClearCache();
#else
                Caching.CleanCache();
#endif

				if (null != cb)
					cb();

				//GC.Collect();
				//GC.WaitForPendingFinalizers();
			}));
		}

		public static string Dump()
		{
			StringBuilder str = new StringBuilder(512);

			var dicSort = from objDic in mBundles orderby objDic.Key ascending select objDic;
			foreach (KeyValuePair< string, UBundle > v in dicSort)
			{
				var bundle = v.Value;

				str.AppendFormat("found bundle {0}, holder = {1}, refcnt = {2}\n", bundle.RelativePath, null != bundle.mHolder ? bundle.mHolder.RelativePath : "none", bundle.RefCnt);
			}

			return str.ToString();
		}

		private static IEnumerator DownLoad(string url, string name, HandleFinishDownload cb)
		{
			string path = url + name.ToLower();

			WWW www = new WWW(path);
			while (www.isDone == false)
				yield return www;

			if (www.error == null)
			{
				yield return new WaitForSeconds(0.05f);

				if (cb != null)
					cb(name, www, true);

				www.Dispose();
				www = null;
			} else
			{
				yield return new WaitForSeconds(0.5f);

				if (cb != null)
					cb(name, null, false);

				ULogger.Error("download error code = " + www.error);
			}
		}

		public static IEnumerator UnloadUnusedAssets(Action callBack)
		{
			yield return Resources.UnloadUnusedAssets();

			callBack();
		}

		static YieldInstruction mWaitForEndOfFrame = new WaitForEndOfFrame();
		static IEnumerator CreateBundleTask(SCreateTask task)
		{
			while (!task.mIsFinished)
			{
				if (task.mCreateJobs.Count > 0)
				{
					IEnumerator job = task.mCreateJobs[0];

					yield return job;

					task.mCreateJobs.RemoveAt(0);
				} else
				{
					yield return mWaitForEndOfFrame;
				}
			}
		}
	}
}
