using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using Object			= UnityEngine.Object;
using Debug				= UnityEngine.Debug;

using ULoadProgress		= System.Action< float >;
using ULoadSceneTask	= System.Action< string, bool >;
using ULoadInstanceTask = System.Action< string, int, UEngine.UObjectBase >;
using ULoadResourceTask = System.Action< string, int, UEngine.IResource >;

namespace UEngine
{
	class UBundleLoadQueue
	{
		public UTask mLoadTask = null;

		public UBundle mResource = null;

		List< ULoadResourceTask > mLoadResourceTasks = new List< ULoadResourceTask >();

		public void AddTask(ULoadResourceTask task)
		{
			mLoadResourceTasks.Add(task);
		}

		public void Done(string path, IResource res)
		{
			for (int i = 0; i < mLoadResourceTasks.Count; ++ i)
			{
				var task = mLoadResourceTasks[i];
				if (null != task)
				{
					if (i == 0)
						res.AddRef();
					else
						UResourceManager.AddReferenceCount(res);

					task.Invoke(path, res.InstanceID, res);
				}
			}
		}
	}

	public class UBundleLoader : IResLoader
	{
		// bundle load task queue
		Dictionary< string, UBundleLoadQueue > mBundleLoadQueue = new Dictionary< string, UBundleLoadQueue >();

		// instance to path
		Dictionary< int, string > mLoadedInsPaths = new Dictionary< int, string >();

		// resource to path
		Dictionary< int, string > mLoadedResPaths = new Dictionary< int, string >();

		public virtual void Create(Action< bool > cb)
		{
			if (IsNeedCheckLocalAsset())
			{
				UCore.GameEnv.StartCoroutine(UBundleManager.CheckLocalAsset((res) =>
				{
					UBundleManager.LoadAssetManifest();

					if (null != cb)
						cb(res);
				}));
			} else
			{
				UBundleManager.LoadAssetManifest();

				if (null != cb)
					cb(true);
			}
		}

		public virtual bool IsAssetExist(string path)
		{
			var meta = UBundleManager.GetBundleMeta(path);

			return (null != meta);
		}

		public virtual void LoadScene(string scene, ULoadSceneTask task, ULoadProgress progress = null)
		{
			Debug.Assert(Thread.CurrentThread.ManagedThreadId == UCore.ThreadID, "call in main thread.");

			UProfile.ResetLoadProfile();

			string path = UCoreUtil.AssetPathNormalize("levels/" + scene + ".unity");

			var meta = UBundleManager.GetBundleMeta(path);
			if (null == meta)
			{
				if (null != task)
					task(path, false);

				return;
			}

			ULoadResourceTask onLoaded = (p, g, r) =>
			{
				if (null != r)
				{
					float stTime = Time.realtimeSinceStartup;

					UBundle b = r as UBundle;

					var sceneName = b.GetAllAssetsNames()[0];
					if (null != UCore.GameEnv
#if ENABLE_SYNC_LOAD
						&& !USystemConfig.Instance.ForceSyncLoad
#endif
						)
					{
						UCore.GameEnv.StartCoroutine(AsyncLoadScene(sceneName, () =>
						{
							ULogger.Profile("BundleLoader: load scene use time {0}", Time.realtimeSinceStartup - stTime);

							//UnloadUnusedResources(() =>
							{
								ULogger.Profile("BundleLoader: load scene GC use time {0}", Time.realtimeSinceStartup - stTime);

								//UnloadSceneImpl(path);

								if (null != task)
									task(scene, true);
							}//);
						}));
					} else
					{
						SceneManager.LoadScene(sceneName);

						ULogger.Profile("BundleLoader: load scene use time {0}", Time.realtimeSinceStartup - stTime);

						//UnloadUnusedResources(() =>
						{
							ULogger.Profile("BundleLoader: load scene with GC use time {0}", Time.realtimeSinceStartup - stTime);

							//UnloadSceneImpl(path);

							if (null != task)
								task(scene, true);
						}//);
					}
				} else
				{
					if (null != task)
						task(scene, false);
				}
			};

			// exist in cache
			UBundle bundle = UBundleManager.GetBundle(path);
			if (null != bundle)
			{
				if (bundle.IsMain && !mLoadedResPaths.ContainsKey(bundle.InstanceID))
					mLoadedResPaths.Add(bundle.InstanceID, path);

				AddReferenceCount(bundle);

				if (null != task)
					onLoaded(path, bundle.InstanceID, bundle);
			} else
			{
#if ENABLE_SYNC_LOAD
				if (USystemConfig.Instance.ForceSyncLoad)
				{
					// load depend resources
					var depends = UBundleManager.GetAssetInfoEX(path, false);
					for (int i = 0; i < depends.Count; ++ i)
					{
						var depend = depends[i];

						IResource resDepend = UBundleManager.GetBundle(depend);
						if (null != resDepend)
							resDepend.AddRef();
						else if (!string.IsNullOrEmpty(depend))
							SynLoadImpl< Object >(depend, false);
					}

					res = SynLoadImpl< Object >(path, false);

					onLoaded(path, res.InstanceID, res);
				} else
#endif
				{
					bundle = UBundleManager.CreateBundle(path);

					float stTime = Time.realtimeSinceStartup;

					// load depend resources
					var dependencies = meta.Dependencies;
					for (int i = 0; i < dependencies.Length; ++ i)
					{
						var depend = dependencies[i];
#if SYNC_LOAD_SCENE
						SynLoadImpl< Object >(depend, false);
#else
						var depRes = LoadImpl< Object >(depend, false, false, (p, g, r) => { });
						if (null != depRes)
							bundle.AddDepend(depRes);
#endif
					}
					ULogger.Profile("BundleLoader: scene resource depend load use time {0}", Time.realtimeSinceStartup - stTime);

#if USE_MEMORY_BUNDLE
					ULoadThread.Instance.AddLoadTask(meta.RelativePath, (data, onCreated) =>
#else
					string filePath; int fileOffset;
					if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
						UFileReaderProxy.QueryFile(meta.RelativePath, out filePath, out fileOffset);
					else
						UFileAccessor.QueryFile(meta.RelativePath, out filePath, out fileOffset);
#endif
					{
#if USE_MEMORY_BUNDLE
#	if SYNC_LOAD_SCENE
						UBundleManager.SyncBuildBundle< Object >(bundle, data);
#	else
						UBundleManager.BuildBundle< Object >(bundle, data, (res) =>
						{
							bundle.AddRef();

							ULogger.Profile("BundleLoader:  scene resource load use time {0}", Time.realtimeSinceStartup - stTime);

							onLoaded(path, bundle.InstanceID, bundle);
						});
#	endif
#else
#	if SYNC_LOAD_SCENE
						UBundleManager.SyncBuildBundle< Object >(bundle, filePath, fileOffset);
#	else
						UBundleManager.BuildBundle< Object >(bundle, filePath, fileOffset, (res)=>
						{
							ULogger.Profile("BundleLoader: scene resource load use time {0}", Time.realtimeSinceStartup - stTime);

							bundle.AddRef();

							onLoaded(path, bundle.InstanceID, bundle);

							string loadProfile = "";

							UProfile.CollectLoadProfile(ref loadProfile);

							ULogger.Profile(loadProfile);

						});
#	endif
#endif

#if SYNC_LOAD_SCENE
						bundle.AddRef();

						onLoaded(path, bundle.InstanceID, bundle);
#endif

#if USE_MEMORY_BUNDLE
						if (null != onCreated)
							onCreated();
					});
#else
					}
#endif
				}
			}
		}

		public virtual void UnloadScene(string scene)
		{
			Debug.Assert(Thread.CurrentThread.ManagedThreadId == UCore.ThreadID, "call in main thread.");

			UnloadImpl(UCoreUtil.AssetPathNormalize("levels/" + scene + ".unity"));

			//SceneManager.UnloadSceneAsync(scene);

			UBundleManager.Tidy(true);
		}

		IEnumerator AsyncLoadScene(string sceneName, Action onLoaded)
		{
			AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
			while (!op.isDone)
				yield return null;

			if (null != onLoaded)
				onLoaded();

			yield return 0;
		}

		public virtual void LoadInstance< T >(string path, ULoadInstanceTask task) where T : Object
		{
			Debug.Assert(Thread.CurrentThread.ManagedThreadId == UCore.ThreadID, "call in main thread.");

#if ENABLE_SYNC_LOAD
			if (USystemConfig.Instance.ForceSyncLoad)
			{
				UObject obj = SynLoadInstance< T >(path);

				if (null != task)
					task(path, obj != null ? obj.InstanceID : -1, obj);
			} else
#endif
			{
				float stTime = Time.realtimeSinceStartup;

				LoadResource< T >(path, USystemConfig.Instance.ForceSyncLoad, (p, g, r) =>
				{
					ULogger.Profile("UBundleLoader: load scene instance`s resource use time {0}", Time.realtimeSinceStartup - stTime);

					stTime = Time.realtimeSinceStartup;

					UObjectBase obj = null;
					if (null != r)
					{
						obj = r.Instantiate< T >();

						ULogger.Profile("UBundleLoader: load scene instance`s instantiate use time {0}", Time.realtimeSinceStartup - stTime);

						mLoadedInsPaths.Add(obj.InstanceID, path);
					}

					if (null != task)
						task(path, obj != null ? obj.InstanceID : -1, obj);
				});
			}
		}

		public virtual UObjectBase SynLoadInstance< T >(string path) where T : Object
		{
			Debug.Assert(Thread.CurrentThread.ManagedThreadId == UCore.ThreadID, "call in main thread.");

			UObjectBase obj = null;

			IResource res = SynLoadResource< T >(path);
			if (null != res)
			{
				obj = res.Instantiate< T >();

				mLoadedInsPaths.Add(obj.InstanceID, path);
			}

			return obj;
		}

		public virtual void UnloadInstance(UObjectBase obj)
		{
			Debug.Assert(Thread.CurrentThread.ManagedThreadId == UCore.ThreadID, "call in main thread.");

			if (null != obj)
			{
				string path = null;
				if (mLoadedInsPaths.TryGetValue(obj.InstanceID, out path))
				{
					mLoadedInsPaths.Remove(obj.InstanceID);

					UnloadResource(path);
				}

				obj.Destroy();
			}
		}

		public virtual void LoadResource< T >(string path, bool synCreate, ULoadResourceTask task) where T : Object
		{
			Debug.Assert(Thread.CurrentThread.ManagedThreadId == UCore.ThreadID, "call in main thread.");

			IResource res = null;
#if ENABLE_SYNC_LOAD
			if (USystemConfig.Instance.ForceSyncLoad)
			{
				res = SynLoadResource< T >(path);

				if (null != task)
					task(path, res.InstanceID, res);

				return;
			}
#endif

			path = UCoreUtil.AssetPathNormalize(path);

			var meta = UBundleManager.GetBundleMeta(path);
			if (null == meta)
			{
				ULogger.Error("can`t find resource`s meta data {0}", path);

				if (null != task)
					task(path, -1, null);

				return;
			}

			// exist in cache
			res = UBundleManager.GetBundle(path);
			if (null != res)
			{
				if (res.IsMain && !mLoadedResPaths.ContainsKey(res.InstanceID))
					mLoadedResPaths.Add(res.InstanceID, path);

				AddReferenceCount(res);

				if (null != task)
					task(path, res.InstanceID, res);
			} else
			{
				res = LoadImpl< T >(path, true, synCreate, task);
			}
		}

		public virtual IResource SynLoadResource< T >(string path) where T : Object
		{
			Debug.Assert(Thread.CurrentThread.ManagedThreadId == UCore.ThreadID, "call in main thread.");

			path = UCoreUtil.AssetPathNormalize(path);

			var meta = UBundleManager.GetBundlePath(path);
			if (null == meta)
			{
				ULogger.Error("can`t find resource`s meta data {0}", path);

				return null;
			}

			// exist in cache
			IResource res = UBundleManager.GetBundle(path);
			if (null != res)
			{
				if (res.IsMain && !mLoadedResPaths.ContainsKey(res.InstanceID))
					mLoadedResPaths.Add(res.InstanceID, path);

				AddReferenceCount(res);
			} else
			{
				res = SynLoadImpl< T >(path, true);
			}

			return res;
		}

		public virtual void UnloadResource(string path)
		{
			Debug.Assert(Thread.CurrentThread.ManagedThreadId == UCore.ThreadID, "call in main thread.");

			UnloadImpl(UCoreUtil.AssetPathNormalize(path));
		}

		public virtual void UnloadResource(IResource res)
		{
			Debug.Assert(Thread.CurrentThread.ManagedThreadId == UCore.ThreadID, "call in main thread.");

			if (null != res)
			{
				string path = null;
				if (mLoadedResPaths.TryGetValue(res.InstanceID, out path))
					UnloadResource(path);
			}
		}

		public virtual void UnloadResource(Object res)
		{
			Debug.Assert(Thread.CurrentThread.ManagedThreadId == UCore.ThreadID, "call in main thread.");

			if (null != res)
			{
				string path = null;
				if (mLoadedResPaths.TryGetValue(res.GetInstanceID(), out path))
					UnloadResource(path);
			}
		}

		public virtual void UnloadUnusedResources(Action cb)
		{
			UnityEngine.Debug.AssertFormat(Thread.CurrentThread.ManagedThreadId == UCore.ThreadID, "call in main thread.");

			UBundleManager.UnloadUnusedResources(() =>
			{
				if (null != cb)
					cb();
			});
		}

		public virtual void AddReferenceCount(string path)
		{
			path = UCoreUtil.AssetPathNormalize(path);

			var meta = UBundleManager.GetBundleMeta(path);
			if (null == meta)
				return;

			UBundle res = UBundleManager.GetBundle(path);
			if (null != res)
				res.AddRef();

			var dependencies = meta.Dependencies;
			for (int i = 0; i < dependencies.Length; ++ i)
				AddReferenceCount(dependencies[i]);
		}

		public virtual void AddReferenceCount(IResource res)
		{
			if (null != res)
				AddReferenceCount(res.RelativePath);
		}

		public virtual void AddReferenceCount(Object res)
		{
			if (null != res)
			{
				string path = null;
				if (mLoadedResPaths.TryGetValue(res.GetInstanceID(), out path))
				{
					AddReferenceCount(path);
				} else
				{
					IResource pres = res as IResource;
					if (null != pres)
						AddReferenceCount(pres.RelativePath);
				}
			}
		}

		public virtual void DelReferenceCount(string path)
		{
			path = UCoreUtil.AssetPathNormalize(path);

			var meta = UBundleManager.GetBundleMeta(path);
			if (null == meta)
				return;

			UBundle res = UBundleManager.GetBundle(path);
			if (null != res)
				res.Release();

			var dependencies = meta.Dependencies;
			for (int i = 0; i < dependencies.Length; ++ i)
				DelReferenceCount(dependencies[i]);
		}

		public virtual void DelReferenceCount(IResource res)
		{
			if (null != res)
				DelReferenceCount(res.RelativePath);
		}

		public virtual void DelReferenceCount(Object res)
		{
			if (null != res)
			{
				string path = null;
				if (mLoadedResPaths.TryGetValue(res.GetInstanceID(), out path))
				{
					DelReferenceCount(path);
				} else
				{
					IResource pres = res as IResource;
					if (null != pres)
						DelReferenceCount(pres.RelativePath);
				}
			}
		}

		public virtual void Update()
		{
			UBundleManager.Update();
		}

		public virtual string Print()
		{
			return string.Format("Total asset load task {0}.", mBundleLoadQueue.Count);
		}

		private bool IsNeedCheckLocalAsset()
		{
			if (USystemConfig.Instance.IsDevelopMode)
				return false;

			if (!USystemConfig.Instance.IsPublishMode)
				return false;

			switch (Application.platform)
			{
				case RuntimePlatform.Android:
				case RuntimePlatform.IPhonePlayer:
					return true;
				default:
					return false;
			}
		}

		private UBundle LoadImpl< T >(string path, bool addToPool, bool synCreate, ULoadResourceTask task) where T : Object
		{
			Debug.Assert(Thread.CurrentThread.ManagedThreadId == UCore.ThreadID, "call in main thread.");

			UBundle bundle = null;

			var meta = UBundleManager.GetBundleMeta(path);
			if (null == meta)
			{
				ULogger.Error("can`t find resource`s meta data {0}", path);

				if (null != task)
					task(path, -1, null);

				return bundle;
			}

			bundle = UBundleManager.GetBundle(path);
			if (null != bundle)
			{
				if (bundle.IsMain && !mLoadedResPaths.ContainsKey(bundle.InstanceID))
					mLoadedResPaths.Add(bundle.InstanceID, path);

				AddReferenceCount(bundle);

				if (null != task)
					task(path, bundle.InstanceID, bundle);

				return bundle;
			}

			// check is in load task queue first.
			UBundleLoadQueue queue = null;
			if (mBundleLoadQueue.TryGetValue(path, out queue))
			{
				bundle = queue.mResource;

				queue.AddTask(task);
			} else // load from disk.
			{
				bundle = UBundleManager.CreateBundle(path);

				// load depend first
				var dependencies = meta.Dependencies;
				for (int i = 0; i < dependencies.Length; ++ i)
				{
					var depend = LoadImpl< Object >(dependencies[i], false, synCreate, (p, g, r) =>
					{ });

					bundle.AddDepend(depend);
				}

				queue = new UBundleLoadQueue();
					queue.AddTask(task);
					queue.mResource = bundle;
				mBundleLoadQueue.Add(path, queue);

#if USE_MEMORY_BUNDLE
				queue.mLoadTask = ULoadThread.Instance.AddLoadTask(meta.RelativePath, (data, onCreated) =>
#else
				string filePath; int fileOffset;
				if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
					UFileReaderProxy.QueryFile(meta.RelativePath, out filePath, out fileOffset);
				else
					UFileAccessor.QueryFile(meta.RelativePath, out filePath, out fileOffset);
#endif
				{
					if (synCreate)
					{
						mBundleLoadQueue.Remove(path);
#if USE_MEMORY_BUNDLE
						bool res = UBundleManager.SyncBuildBundle< T >(bundle, data);
#else
						bool res = UBundleManager.SyncBuildBundle< T >(bundle, filePath, fileOffset);
#endif
						if (res)
						{
							//if (addToPool)
							if (bundle.IsMain)
							{
								string _path = null;
								if (mLoadedResPaths.TryGetValue(bundle.InstanceID, out _path))
									ULogger.Error("contains key {0}, {1} exist path {2}", bundle.InstanceID, path, _path);
								else
									mLoadedResPaths.Add(bundle.InstanceID, path);
							}
						}

						if (null != queue)
							queue.Done(path, bundle);
					} else
					{
#if USE_MEMORY_BUNDLE
						bool res = UBundleManager.BuildBundle< T >(bundle, data, (asset) =>
#else
						bool res = UBundleManager.BuildBundle< T >(bundle, filePath, fileOffset, (asset) =>
#endif
						{
							mBundleLoadQueue.Remove(path);

							if (null != asset)
							{
								//if (addToPool)
								if (bundle.IsMain)
								{
									string _path = null;
									if (mLoadedResPaths.TryGetValue(asset.InstanceID, out _path))
										ULogger.Error("contains key {0}, {1} exist path {2}", asset.InstanceID, path, _path);
									else
										mLoadedResPaths.Add(asset.InstanceID, path);
								}

								queue.Done(path, asset);
							} else
							{
								queue.Done(path, null);
							}

#if USE_MEMORY_BUNDLE
							if (null != onCreated)
								onCreated();
#endif
						});
					}
#if USE_MEMORY_BUNDLE
				});
#else
				}
#endif
			}

			return bundle;
		}

		private IResource SynLoadImpl< T >(string path, bool addToPool) where T : Object
		{
			Debug.Assert(Thread.CurrentThread.ManagedThreadId == UCore.ThreadID, "call in main thread.");

			var meta = UBundleManager.GetBundleMeta(path);
			if (null == meta)
			{
				ULogger.Error("can`t find resource`s meta data {0}", path);

				return null;
			}

			UBundle bundle = UBundleManager.GetBundle(path);
			if (null != bundle)
			{
				if (bundle.IsMain && !mLoadedResPaths.ContainsKey(bundle.InstanceID))
					mLoadedResPaths.Add(bundle.InstanceID, path);

				AddReferenceCount(bundle);

				return bundle;
			}

			bundle = UBundleManager.CreateBundle(path);

			var dependencies = meta.Dependencies;
			for (int i = 0; i < dependencies.Length; ++ i)
			{
				var depend = SynLoadImpl< Object >(dependencies[i], false);

				bundle.AddDepend(depend);
			}

			bool isCancelRes = true;

			// check is in load task queue first.
			UBundleLoadQueue queue = null;
			if (mBundleLoadQueue.TryGetValue(path, out queue))
			{
				if (null != queue.mLoadTask)
					queue.mLoadTask.Cancel();

				if (null != queue.mResource)
					isCancelRes = queue.mResource.Cancel();

				mBundleLoadQueue.Remove(path);
			}

			if (!isCancelRes)
			{
				bundle = UBundleManager.GetBundle(path);
				if (null != bundle)
				{
					if (bundle.IsMain && !mLoadedResPaths.ContainsKey(bundle.InstanceID))
						mLoadedResPaths.Add(bundle.InstanceID, path);

					AddReferenceCount(bundle);
					//bundle.AddRef();
				}
			} else
			{
				if (UFileAccessor.IsFileExist(meta.RelativePath))
				{
#if USE_MEMORY_BUNDLE
					bool res = UBundleManager.SyncBuildBundle< T >(bundle, UFileAccessor.ReadBinaryFile(meta.RelativePath));
#else
					string filePath; int fileOffset;
					if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
						UFileReaderProxy.QueryFile(meta.RelativePath, out filePath, out fileOffset);
					else
						UFileAccessor.QueryFile(meta.RelativePath, out filePath, out fileOffset);

					bool res = UBundleManager.SyncBuildBundle< T >(bundle, filePath, fileOffset);
#endif
					if (res)
					{
						bundle.AddRef();

						//if (addToPool)
						if (bundle.IsMain)
						{
							string _path = null;
							if (mLoadedResPaths.TryGetValue(bundle.InstanceID, out _path))
								ULogger.Error("contains key {0}, {1} exist path {2}", bundle.InstanceID, path, _path);
							else
								mLoadedResPaths.Add(bundle.InstanceID, path);
						}
					}

					if (null != queue)
						queue.Done(path, bundle);
				}
			}

			return bundle;
		}

		private void UnloadImpl(string path)
		{
			Debug.Assert(Thread.CurrentThread.ManagedThreadId == UCore.ThreadID, "call in main thread.");

			var meta = UBundleManager.GetBundleMeta(path);
			if (null == meta)
				return;

			UBundle res = UBundleManager.GetBundle(path);
			if (null != res)
			{
				if (UBundleManager.ReleaseBundle(res) && res.IsMain)
					mLoadedResPaths.Remove(res.InstanceID);
			}

			// unload depend resources
			var dependencies = meta.Dependencies;
			for (int i = 0; i < dependencies.Length; ++ i)
			{
				var depend = dependencies[i];

				if (!string.IsNullOrEmpty(depend))
					UnloadImpl(depend);
			}
		}

		private void UnloadSceneImpl(string path)
		{
			Debug.Assert(Thread.CurrentThread.ManagedThreadId == UCore.ThreadID, "call in main thread.");

			var meta = UBundleManager.GetBundleMeta(path);
			if (null == meta)
				return;

			UBundle res = UBundleManager.GetBundle(path);
			if (null != res)
			{
				if (UBundleManager.ReleaseBundle(res))
				{
					UBundleManager.UnloadBundle(path);
					UBundleManager.RmvBundle(path);

					if (res.IsMain)
						mLoadedResPaths.Remove(res.InstanceID);
				}
			}

			// unload depend resources
			var dependencies = meta.Dependencies;
			for (int i = 0; i < dependencies.Length; ++ i)
			{
				var depend = dependencies[i];

				if (!string.IsNullOrEmpty(depend))
					UnloadSceneImpl(depend);
			}
		}
	}
}
