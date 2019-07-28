using System;
using System.Linq;

using UnityEngine;

using Object			= UnityEngine.Object;

using ULoadProgress		= System.Action< float>;
using ULoadSceneTask	= System.Action< string, bool >;
using ULoadInstanceTask = System.Action< string, int, UEngine.UObjectBase >;
using ULoadResourceTask = System.Action< string, int, UEngine.IResource >;

namespace UEngine
{
	public class UResourceManager
	{
		static bool msIsCreated = false;

		private static IResLoader msLoader = null;

		public static string[] kExportableFileTypes = { ".xml" };

		private static string[] mPackedExportableFileTypes = 
        {
            ".prefab",
            ".fbx",
            ".controller",
            ".png",
            ".exr",
            ".tga",
            ".tif",
            ".asset",
            ".jpg",
            ".jpeg",
            ".psd",
            ".mp3",
            ".wav",
            ".mat",
            ".unity",
            ".anim",
            ".shader",
            ".ttf",
			".dds",
			".ogg",
        };
		public static string[] PackedExportableFileTypes
		{
			get { return UResourceManager.mPackedExportableFileTypes; }
			set { UResourceManager.mPackedExportableFileTypes = value; }
		}

		static UResourceManager()
		{
			msLoader = null;
		}

		public static bool Create(bool useResource, Action< bool > onCreated)
		{
			if (msIsCreated)
			{
				if (null != onCreated)
					onCreated.Invoke(false);

				return false;
			}

			if (useResource)
				msLoader = new UResourceLoader();
			else
				msLoader = new UBundleLoader();

			msLoader.Create((res) =>
			{
				msIsCreated = true;

				if (null != onCreated)
					onCreated.Invoke(res);
			});

			return true;
		}

		public static void Destroy()
		{
			if (msIsCreated)
			{
				msLoader = null;

				msIsCreated = false;
			}
		}

		public static void Update()
		{
			if (!msIsCreated)
				return;

			msLoader.Update();
		}

		public static bool IsExportable(string path)
		{
			var extension = path.Substring(path.LastIndexOf('.'));
				extension = extension.ToLower();

			return kExportableFileTypes.Contains(extension);
		}

		public static bool IsPackedExportable(string path)
		{
			var extension = path.Substring(path.LastIndexOf('.'));
				extension = extension.ToLower();

			return PackedExportableFileTypes.Contains(".*") || PackedExportableFileTypes.Contains(extension);
		}

		public static bool IsAssetExist(string path)
		{
			if (!msIsCreated)
				return false;

			return msLoader.IsAssetExist(path);
		}

		public static void LoadScene(string scene, ULoadSceneTask task, ULoadProgress progress = null)
		{
			if (!msIsCreated)
			{
				if (null != progress)
					progress(1.0f);

				if (null != task)
					task(scene, false);

				return;
			}

			msLoader.LoadScene(scene, task, progress);
		}

		public static void UnloadScene(string scene)
		{
			if (!msIsCreated)
				return;

			msLoader.UnloadScene(scene);
		}

		public static void LoadInstance< T >(string path, ULoadInstanceTask task) where T : Object
		{
			if (!msIsCreated)
			{
				if (null != task)
					task(path, 0, null);

				return;
			}

			msLoader.LoadInstance< T >(path, task);
		}

		public static UObjectBase SynLoadInstance< T >(string path) where T : Object
		{
			if (!msIsCreated)
				return null;

			return msLoader.SynLoadInstance< T >(path);
		}

		public static void UnloadInstance(UObjectBase obj)
		{
			if (!msIsCreated)
				return;

			msLoader.UnloadInstance(obj);
		}

		public static void LoadResource< T >(string path, ULoadResourceTask task) where T : Object
		{
			if (!msIsCreated)
			{
				if (null != task)
					task(path, -1, null);

				return;
			}

			msLoader.LoadResource< T >(path, USystemConfig.Instance.ForceSyncLoad, task);
		}

		public static IResource SynLoadResource< T >(string path) where T : Object
		{
			if (!msIsCreated)
				return null;

			return msLoader.SynLoadResource< T >(path);
		}

		public static void UnloadResource(string path)
		{
			if (!msIsCreated)
				return;

			msLoader.UnloadResource(path);
		}

		public static void UnloadResource(IResource res)
		{
			if (!msIsCreated)
				return;

			msLoader.UnloadResource(res);
		}

		public static void UnloadResource(Object res)
		{
			if (!msIsCreated)
				return;

			msLoader.UnloadResource(res);
		}

		public static void LoadResources(string[] resourcesName, Action< IResource[] > loaded, Action< float > progress = null)
		{
			if (resourcesName == null || resourcesName.Length == 0)
			{
				if (loaded != null)
					loaded(null);

				return;
			}

			int loadCnt = 0;

			IResource[] objs = new IResource[resourcesName.Length];
			for (int i = 0; i < resourcesName.Length; i ++)
			{
				int index = i;

				string pref = resourcesName[index];

				Action actionProgress = null;
				if (progress != null)
					actionProgress = () => { progress((float)index / (float)resourcesName.Length); };

				LoadResource< Object >(pref, (path, guid, obj) =>
				{
					if (null != obj)
						objs[index] = obj;
					else
						objs[index] = null;

					loadCnt ++;
					if (loadCnt == resourcesName.Length)
					//if (index == resourcesName.Length - 1)
					{
						if (loaded != null)
							loaded(objs);
					}

					if (null != actionProgress)
						actionProgress();
				});
			}
		}

		public static void UnloadResources(string[] resourcesName)
		{
			if (resourcesName == null)
				return;

			for (int i = 0; i < resourcesName.Length; ++ i)
				UnloadResource(resourcesName[i]);
		}

		public static void AddReferenceCount(string path)
		{
			if (!msIsCreated)
				return;

			msLoader.AddReferenceCount(path);
		}

		public static void AddReferenceCount(IResource res)
		{
			if (!msIsCreated)
				return;

			msLoader.AddReferenceCount(res);
		}

		public static void AddReferenceCount(Object res)
		{
			if (!msIsCreated)
				return;

			msLoader.AddReferenceCount(res);
		}

		public static void DelReferenceCount(string path)
		{
			if (!msIsCreated)
				return;

			msLoader.DelReferenceCount(path);
		}

		public static void DelReferenceCount(IResource res)
		{
			if (!msIsCreated)
				return;

			msLoader.DelReferenceCount(res);
		}

		public static void DelReferenceCount(Object res)
		{
			if (!msIsCreated)
				return;

			msLoader.DelReferenceCount(res);
		}

		public static void UnloadUnusedResources(Action cb)
		{
			if (!msIsCreated)
				return;

			msLoader.UnloadUnusedResources(cb);
		}

		public static string Print()
		{
			return msLoader.Print();
		}
	}
}
