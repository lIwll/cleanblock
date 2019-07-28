using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Object			= UnityEngine.Object;

using ULoadProgress		= System.Action< float >;
using ULoadSceneTask	= System.Action< string, bool >;
using ULoadInstanceTask	= System.Action< string, int, UEngine.UObjectBase >;
using ULoadResourceTask = System.Action< string, int, UEngine.IResource >;

namespace UEngine
{
	public interface IResLoader
	{
		void Create(Action< bool > cb);

		bool IsAssetExist(string path);

		void LoadScene(string scene, ULoadSceneTask task, ULoadProgress progress = null);
		void UnloadScene(string scene);

		void LoadInstance< T >(string path, ULoadInstanceTask task) where T : Object;
		UObjectBase SynLoadInstance< T >(string path) where T : Object;
		void UnloadInstance(UObjectBase obj);

		void LoadResource< T >(string path, bool synCreate, ULoadResourceTask task) where T : Object;
		IResource SynLoadResource< T >(string path) where T : Object;
		void UnloadResource(string path);
		void UnloadResource(IResource res);
		void UnloadResource(Object res);

		void UnloadUnusedResources(Action cb);

		void AddReferenceCount(string path);
		void AddReferenceCount(IResource res);
		void AddReferenceCount(Object res);

		void DelReferenceCount(string path);
		void DelReferenceCount(IResource res);
		void DelReferenceCount(Object res);

		void Update();

		string Print();
	}
}
