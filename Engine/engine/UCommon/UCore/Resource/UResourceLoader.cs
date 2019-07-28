using System;

using UnityEngine;

using Object			= UnityEngine.Object;

using ULoadProgress		= System.Action< float >;
using ULoadSceneTask	= System.Action< string, bool >;
using ULoadInstanceTask = System.Action<string, int, UEngine.UObjectBase>;
using ULoadResourceTask = System.Action<string, int, UEngine.IResource >;

namespace UEngine
{
	public class UResourceLoader : IResLoader
	{
		public virtual void Create(Action< bool > cb)
		{
			if (null != cb)
				cb(true);
		}

		public virtual bool IsAssetExist(string path)
		{
			return false;
		}

		public virtual void LoadScene(string scene, ULoadSceneTask task, ULoadProgress progress = null)
		{
		}

		public virtual void UnloadScene(string scene)
		{
		}

		public virtual void LoadInstance< T >(string path, ULoadInstanceTask task) where T : Object
		{
		}

		public virtual UObjectBase SynLoadInstance< T >(string path) where T : Object
		{
			return null;
		}

		public virtual void UnloadInstance(UObjectBase obj)
		{
		}

		public virtual void LoadResource< T >(string path, bool synCreate, ULoadResourceTask task) where T : Object
		{
		}

		public virtual IResource SynLoadResource< T >(string path) where T : Object
		{
			return null;
		}

		public virtual void UnloadResource(string path)
		{
		}

		public virtual void UnloadResource(IResource res)
		{
		}

		public virtual void UnloadResource(Object res)
		{
		}

		public virtual void UnloadBundle(string path)
		{
		}

		public virtual void UnloadBundle(IResource res)
		{
		}

		public virtual void UnloadBundle(Object res)
		{
		}

		public virtual void UnloadUnusedResources(Action cb)
		{
		}

		public virtual void AddReferenceCount(string path)
		{
		}

		public virtual void AddReferenceCount(IResource res)
		{
		}

		public virtual void AddReferenceCount(Object res)
		{
		}

		public virtual void DelReferenceCount(string path)
		{
		}

		public virtual void DelReferenceCount(IResource res)
		{
		}

		public virtual void DelReferenceCount(Object res)
		{
		}

		public virtual void Update()
		{
		}

		public virtual string Print()
		{
			return "";
		}
	}
}
