using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Object = UnityEngine.Object;

namespace UEngine
{
    public class UBundleMetaData
    {
        public string Name { get; set; }

        public string RelativePath { get; set; }

        public string[] Dependencies { get; set; }
    }

    public class UBundle : URefObject, IResource
    {
        AssetBundle mBundle = null;
        public AssetBundle Bundle
        {
            get { return mBundle; }
            set { mBundle = value; }
        }

        public virtual bool IsDone
        { get; set; }

        public virtual bool IsMain
        {
            get
            {
                return (null == mHolder);
            }
        }

        protected bool mIsSceneAsset = false;
        public bool IsSceneAsset
        {
            get { return mIsSceneAsset; }
        }

        protected Object mRes = null;
        public virtual Object Res
        {
            get { return mRes; }
        }

        public virtual int InstanceID
        {
            get { return mRes != null ? mRes.GetInstanceID() : -1; }
        }

        public virtual string RelativePath
        { get; set; }

        //bool mIsShader = false;

        protected IEnumerator mLoadTask = null;

        protected bool mIsCancelLoadTask = true;

        protected List<IResource> mDepends = new List<IResource>();

        public UBundle()
        {
            IsDone = false;
        }

        public UBundle(string path)
        {
            IsDone = false;
            RelativePath = path;
        }

        public override int AddRef()
        {
            return base.AddRef();
        }

        public override int Release()
        {
            return base.Release();
        }

        static int mTaskCount = 0;

#if USE_MEMORY_BUNDLE
		public void Create< T >(byte[] data, Action< bool > onLoaded) where T : Object
#else
        public void Create<T>(string filePath, ulong fileOffset, Action<bool> onLoaded) where T : Object
#endif
        {
#if USE_MEMORY_BUNDLE
			if (null != data)
			{
				if (USystemConfig.Instance.ForceSyncLoad)
				{
					SyncCreate< T >(data);

					if (null != onLoaded)
						onLoaded(null != mBundle);
				} else
				{
					mLoadTask = AsyncCreateImpl< T >(data, () =>
					{
						IsDone = true;

						mLoadTask = null;

						if (null != onLoaded)
							onLoaded(null != mBundle);
					});
					mIsCancelLoadTask = false;

					UCore.GameEnv.StartCoroutine(mLoadTask);

					//UCoroutineManager.AddTask(mLoadTask);

					//UBundleManager.AddCreateTask(mLoadTask);
				}
			} else
			{
				IsDone = true;

				ULogger.Warn("can`t load resource {0}", RelativePath);
			}
#else
            if (!string.IsNullOrEmpty(filePath))
            {
                if (USystemConfig.Instance.ForceSyncLoad)
                {
                    SyncCreate<T>(filePath, fileOffset);

                    if (null != onLoaded)
                        onLoaded(null != mBundle);
                }
                else
                {
                    mTaskCount++;

                    mLoadTask = AsyncCreateImpl<T>(filePath, fileOffset, () =>
                    {
                        IsDone = true;

                        mLoadTask = null;

                        if (null != onLoaded)
                            onLoaded(null != mBundle);
                    });
                    mIsCancelLoadTask = false;

                    UCore.GameEnv.StartCoroutine(mLoadTask);

                    //UCoroutineManager.AddTask(mLoadTask);

                    //UBundleManager.AddCreateTask(mLoadTask);
                }
            }
            else
            {
                IsDone = true;

                ULogger.Warn("can`t load resource {0}", RelativePath);

                if (null != onLoaded)
                    onLoaded(null != mBundle);
            }
#endif
        }

#if USE_MEMORY_BUNDLE
		public bool SyncCreate< T >(byte[] data) where T : Object
#else
        public bool SyncCreate<T>(string filePath, ulong fileOffset) where T : Object
#endif
        {
            float stTime = Time.realtimeSinceStartup;

#if USE_MEMORY_BUNDLE
			if (null != data)
			{
				mBundle = AssetBundle.LoadFromMemory(data);
#else
            if (!string.IsNullOrEmpty(filePath))
            {
                mBundle = AssetBundle.LoadFromFile(filePath, 0, fileOffset);

                UProfile.AddLoadProfile(RelativePath, 1, Time.realtimeSinceStartup - stTime);
#endif

                if (mBundle)
                {
                    mIsSceneAsset = mBundle.isStreamedSceneAssetBundle;

                    if (!mIsSceneAsset)
                    {
                        // if is shader or nav mesh or prefab then load asset.
                        if (null == mHolder || RelativePath.EndsWith(".shader") || RelativePath.EndsWith(".asset") || RelativePath.EndsWith(".prefab"))
                            LoadAsset<T>();

                        stTime = Time.realtimeSinceStartup;

                        var materials = mBundle.LoadAllAssets<Material>();
                        for (int i = 0; i < materials.Length; ++i)
                        {
                            var m = materials[i];
                            if (null == m)
                                continue;

                            var shaderName = m.shader.name;
                            if (shaderName.ToLower() == "standard")
                            {
                                ULogger.Error("use standard shader {0}", mRes.name);

                                continue;
                            }

                            var newShader = UShaderManager.FindShader(shaderName);
                            if (newShader != null)
                            {
                                var renderQueue = m.renderQueue;

                                m.shader = newShader;
                                if (m.renderQueue != renderQueue)
                                    m.renderQueue = renderQueue;
                            }
                        }
                        UProfile.AddLoadProfile(RelativePath, 5, Time.realtimeSinceStartup - stTime);
                    }
                }
            }
            else
            {
                ULogger.Error("can`t load resource {0}", RelativePath);
            }

            IsDone = true;

            return (null != mBundle);
        }

        public virtual bool Cancel()
        {
#if USE_MEMORY_BUNDLE
			if (UCoroutineManager.RmvTask(mLoadTask))
			{
				mIsCancelLoadTask = true;

				UCoroutineManager.WaitEndOfTask(mLoadTask);

				mIsCancelLoadTask = false;

				return false;
			} else if (UCoroutineManager.IsRunningTask(mLoadTask))
			{
				mIsCancelLoadTask = true;

				UCoroutineManager.WaitEndOfCurrentTask();

				mIsCancelLoadTask = false;

				return false;
			}

			return true;
#else
            mIsCancelLoadTask = true;

            if (null != mLoadTask)
            {
                while (mLoadTask.MoveNext())
                    ;
            }
            mLoadTask = null;

            return false;
#endif
        }

        public string[] GetAllAssetsNames()
        {
            if (mBundle)
            {
                if (mBundle.isStreamedSceneAssetBundle)
                    return mBundle.GetAllScenePaths();

                return mBundle.GetAllAssetNames();
            }

            return new string[] { };
        }

        public virtual T Load<T>(string name) where T : Object
        {
            if (null != mBundle && !mBundle.isStreamedSceneAssetBundle)
                return mBundle.LoadAsset<T>(name);

            return null;
        }

        public virtual void Unload(bool release)
        {
            if (release/* || !mIsShader*/)
            {
                if (null != mRes && mRes.GetType() != typeof(GameObject))
                    Resources.UnloadAsset(mRes);
                mRes = null;

                if (null != mBundle)
                    mBundle.Unload(release);
                mBundle = null;

                IsDone = false;
            }
        }

        public IResource mHolder = null;
        public virtual void AddDepend(IResource res)
        {
            (res as UBundle).mHolder = this;

            mDepends.Add(res);
        }

        public virtual UObjectBase Instantiate<T>() where T : Object
        {
            if (null != mRes)
            {
                if (USystemConfig.GAMETYPE == GameTypeE.Game2D)
                    return new UObject2D(GameObject.Instantiate<T>(mRes as T), true);
                else
                    return new UObject(GameObject.Instantiate<T>(mRes as T), true);

            }
            else
                ULogger.Error("failed load resource {0}", RelativePath);

            if (USystemConfig.GAMETYPE == GameTypeE.Game2D)
                return new UObject2D();
            else
                return new UObject();
        }

#if USE_MEMORY_BUNDLE
		IEnumerator AsyncCreateImpl< T >(byte[] data, Action onLoaded) where T : Object
#else
        IEnumerator AsyncCreateImpl<T>(string filePath, ulong fileOffset, Action onLoaded) where T : Object
#endif
        {
            float stTime = Time.realtimeSinceStartup;

            for (int i = 0; i < mDepends.Count; ++i)
            {
                var dep = mDepends[i];
                while (!dep.IsDone/* && !mIsCancelLoadTask*/)
                {
                    yield return null;
                }
            }

            UProfile.AddLoadProfile(RelativePath, 0, Time.realtimeSinceStartup - stTime);

            stTime = Time.realtimeSinceStartup;
#if USE_MEMORY_BUNDLE
			AssetBundleCreateRequest cr = AssetBundle.LoadFromMemoryAsync(data);
#else
            AssetBundleCreateRequest cr = AssetBundle.LoadFromFileAsync(filePath, 0, fileOffset);
#endif

            if (null == cr)
            {
                if (null != onLoaded)
                    onLoaded();

                yield break;
            }

            while (!cr.isDone && !mIsCancelLoadTask)
                yield return null;

            //yield return cr;

            UProfile.AddLoadProfile(RelativePath, 1, Time.realtimeSinceStartup - stTime);

            mBundle = cr.assetBundle;
            if (null != mBundle)
            {
                mIsSceneAsset = mBundle.isStreamedSceneAssetBundle;

                if (!mIsSceneAsset)
                {
                    // if is shader or nav mesh or prefab then load asset.
                    if (null == mHolder || RelativePath.EndsWith(".shader") || RelativePath.EndsWith(".asset") || RelativePath.EndsWith(".prefab"))
                        yield return LoadAssetAsync<T>();

                    stTime = Time.realtimeSinceStartup;

                    var materials = mBundle.LoadAllAssets<Material>();
                    for (int i = 0; i < materials.Length; ++i)
                    {
                        var m = materials[i];
                        if (null == m)
                            continue;

                        var shaderName = m.shader.name;

                        var newShader = UShaderManager.FindShader(shaderName);
                        if (newShader != null)
                        {
                            var renderQueue = m.renderQueue;

                            m.shader = newShader;
                            if (m.renderQueue != renderQueue)
                                m.renderQueue = renderQueue;
                        }
                    }
                    UProfile.AddLoadProfile(RelativePath, 5, Time.realtimeSinceStartup - stTime);
                }
            }

            if (null != onLoaded)
                onLoaded();

            --mTaskCount;

            //ULogger.Profile("UBundle total task {0}", mTaskCount);
        }

        void LoadAsset<T>() where T : Object
        {
            float stTime = Time.realtimeSinceStartup;

            var names = mBundle.GetAllAssetNames();
            if (names.Length > 0)
                mRes = mBundle.LoadAsset<T>(names[0]);

            UProfile.AddLoadProfile(RelativePath, 2, Time.realtimeSinceStartup - stTime);

            stTime = Time.realtimeSinceStartup;

            if (mRes is UnityEngine.AI.NavMeshData)
                UNavMeshManager.Add(this, mRes as UnityEngine.AI.NavMeshData);

            UProfile.AddLoadProfile(RelativePath, 3, Time.realtimeSinceStartup - stTime);

            if (mRes is Shader)
            {
                stTime = Time.realtimeSinceStartup;

                //mIsShader = true;

                Shader shader = mRes as Shader;
                if (!shader.isSupported)
                    ULogger.Warn("shader {0} is not support on this platform({1}).", mRes.name, Application.platform);

                UShaderManager.RegisterShader(this, shader);

                UProfile.AddLoadProfile(RelativePath, 4, Time.realtimeSinceStartup - stTime);
            }
        }

        IEnumerator LoadAssetAsync<T>() where T : Object
        {
            float stTime = Time.realtimeSinceStartup;

            var names = mBundle.GetAllAssetNames();
            if (names.Length > 0)
            {
                AssetBundleRequest ar = mBundle.LoadAssetAsync<T>(names[0]);

                while (!ar.isDone && !mIsCancelLoadTask)
                    yield return null;
                //yield return ar;

                mRes = ar.asset;
            }

            UProfile.AddLoadProfile(RelativePath, 2, Time.realtimeSinceStartup - stTime);

            stTime = Time.realtimeSinceStartup;

            if (mRes is UnityEngine.AI.NavMeshData)
                UNavMeshManager.Add(this, mRes as UnityEngine.AI.NavMeshData);

            UProfile.AddLoadProfile(RelativePath, 3, Time.realtimeSinceStartup - stTime);

            if (mRes is Shader)
            {
                //mIsShader = true;

                stTime = Time.realtimeSinceStartup;

                Shader shader = mRes as Shader;
                if (!shader.isSupported)
                    ULogger.Warn("shader {0} is not support on this platform({1}).", mRes.name, Application.platform);

                UShaderManager.RegisterShader(this, shader);

                UProfile.AddLoadProfile(RelativePath, 4, Time.realtimeSinceStartup - stTime);
            }
        }
    }
}
