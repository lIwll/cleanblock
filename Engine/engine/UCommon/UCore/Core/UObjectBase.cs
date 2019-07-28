using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

namespace UEngine
{
    public abstract class UObjectBase
    {
        internal GameObject mObject;

        public GameObject GameEntity
        {
            get { return mObject; }

            protected set { }
        }

        internal int mInstanceID = -1;

        internal Animator mAnimator;
        internal Animation mAnimation;

        internal Vector3 mLocalTrans = Vector3.zero;
        internal Vector3 mLocalScale = Vector3.one;
        internal Quaternion mLocalRotation = Quaternion.identity;

        internal int mLayer = -1;

        internal UObjectBase mParent = null;

        internal Dictionary<string, GameObject> mBones = null;
        internal Dictionary<string, GameObject> Bones
        {
            get
            {
                if (null == mBones)
                    mBones = new Dictionary<string, GameObject>();

                return mBones;
            }
        }

        protected bool mCrossFadeAlpha = false;
        protected float mCrossFadeAlphaAddon = 1f;
        protected float mCrossFadeAlphaTarget = 0f;
        protected float mCrossFadeAlphaCurrent = 1f;
        protected float mCrossFadeAlphaTickline = 0f;
        protected float mCrossFadeAlphaDuration = 0f;


        protected struct SFloatValue
        {
            string
                mName;
            float
                mValue;
            Material
                mMaterial;

            public SFloatValue(string _n, Material _m, float _v)
            {
                mName = _n;
                mValue = _v;
                mMaterial = _m;
            }

            public void Set()
            {
                if (null != mMaterial)
                    mMaterial.SetFloat(mName, mValue);
            }
        }
        protected Dictionary<string, List<SFloatValue>> mFloatValueCache = null;
        protected Dictionary<string, List<SFloatValue>> FloatValueCache
        {
            get
            {
                if (null == mFloatValueCache)
                    mFloatValueCache = new Dictionary<string, List<SFloatValue>>();

                return mFloatValueCache;
            }
        }

        protected struct SMaterialValue
        {
            public Renderer
                mRenderer;
            public Material[]
                mMaterials;

            public void Set()
            {
                if (null != mRenderer)
                    mRenderer.materials = mMaterials;
            }
        }
        protected Dictionary<string, List<SMaterialValue>> mMaterialValueCache = null;
        protected Dictionary<string, List<SMaterialValue>> MaterialValueCache
        {
            get
            {
                if (null == mMaterialValueCache)
                    mMaterialValueCache = new Dictionary<string, List<SMaterialValue>>();

                return mMaterialValueCache;
            }
        }

        protected struct SShaderValue
        {
            int mQueue;
            Material
                mMaterial;
            Shader
                mShader;

            public SShaderValue(Material _m)
            {
                mMaterial = _m;
                mShader = _m.shader;
                mQueue = _m.renderQueue;
            }

            public void Set()
            {
                if (null != mMaterial)
                {
                    mMaterial.shader = mShader;
                    mMaterial.renderQueue = mQueue;
                }
            }
        }
        protected Dictionary<string, List<SShaderValue>> mShaderValueCache = null;
        protected Dictionary<string, List<SShaderValue>> ShaderValueCache
        {
            get
            {
                if (null == mShaderValueCache)
                    mShaderValueCache = new Dictionary<string, List<SShaderValue>>();

                return mShaderValueCache;
            }
        }



        protected static readonly string kDefaultMaterial = "__root__";

        protected bool mDestroySelf = false;

        protected float mAniSpeed = 1.0f;

        public float AniSpeed
        {
            get { return mAniSpeed; }
        }

		bool mAniPropChanged = false;

		bool mAniUpdatePreFrame = false;
		public bool AniUpdatePreFrame
		{
			get
			{
				return mAniUpdatePreFrame;
			}
			set
			{
				mAniPropChanged = true;

				if (null != mAnimator)
					mAnimator.cullingMode = value ? AnimatorCullingMode.AlwaysAnimate : AnimatorCullingMode.CullUpdateTransforms;
				else if (null != mAnimation)
					mAnimation.cullingType = value ? AnimationCullingType.AlwaysAnimate : AnimationCullingType.BasedOnRenderers;

				mAniUpdatePreFrame = value;
			}
		}

        public UObjectBase()
        {
            mDestroySelf = false;
        }

        public UObjectBase(UnityEngine.Object obj, bool destroySelf)
        {
            mDestroySelf = destroySelf;

            Init(obj);
        }

        ~UObjectBase()
        {
            //Dispose(true);
        }

        public virtual void Dispose()
        {
            Dispose(true);
        }

        public virtual void Dispose(bool releaseRes)
        {
            if (mDestroySelf && null != mObject)
                GameObject.DestroyImmediate(mObject);
            mObject = null;
        }

        public int Layer
        {
            get
            {
                return mLayer;
            }
            set
            {
                if (mLayer != value)
                {
                    mLayer = (value >= 0 && value <= 31) ? value : 0;
                    if (mObject)
                    {
                        UCoreUtil.TraverseChild(mObject, (go) =>
                        {
                            if (!CompareLayer(go.layer))
                            {
                                go.layer = mLayer;
                            }

                            return true;
                        });
                    }
                }
            }
        }

        protected int GameEffect1 = -999;
        protected int GameEffect2 = -999;
        protected int GameEffect3 = -999;

        public Transform LocalTransform
        {
            get
            {
                if (mObject)
                    return mObject.transform;

                return null;
            }
        }

        public Vector3 LocalTrans
        {
            get { return mLocalTrans; }
            set
            {
                mLocalTrans = value;

                if (mObject)
                    mObject.transform.localPosition = value;
            }
        }

        public Vector3 LocalScale
        {
            get { return mLocalScale; }
            set
            {
                mLocalScale = value;

                if (mObject)
                    mObject.transform.localScale = value;
            }
        }

        public Quaternion LocalRotation
        {
            get { return mLocalRotation; }
            set
            {
                mLocalRotation = value;

                if (mObject)
                    mObject.transform.localRotation = value;
            }
        }

        public int InstanceID
        {
            get { return mInstanceID; }
        }

        public string Name
        {
            get { if (mObject) return mObject.name; return "unknown"; }
            set { if (mObject) mObject.name = value; }
        }

        public virtual void Init(UnityEngine.Object obj)
        {
            mObject = obj as GameObject;
            if (mObject)
            {
                mObject.name = mObject.name.Replace("(Clone)", "");

                mInstanceID = mObject.GetInstanceID();

                if (mLayer != -1)
                    mObject.layer = Layer;
                else
                    mLayer = mObject.layer;

                mAnimator = mObject.GetComponentInChildren<Animator>();
                if (null != mAnimator)
                    mAniSpeed = mAnimator.speed;
                mAnimation = mObject.GetComponent<Animation>();

                mLocalTrans = mObject.transform.localPosition;
                mLocalScale = mObject.transform.localScale;
                mLocalRotation = mObject.transform.localRotation;

				if (mAniPropChanged)
					AniUpdatePreFrame = mAniUpdatePreFrame;

                GameObject.DontDestroyOnLoad(mObject);
                //DontDestroyOnLoad();
            }
            else
            {
                ULogger.Error("obj is not a game object.");
            }
        }

        public virtual void Destroy()
        {
            Bones.Clear();

            if (null != mFloatValueCache)
            {
                Dictionary<string, List<SFloatValue>>.Enumerator it = mFloatValueCache.GetEnumerator();
                while (it.MoveNext())
                {
                    var lst = it.Current.Value;
                    for (int i = lst.Count - 1; i >= 0; --i)
                        lst[i].Set();
                }

                mFloatValueCache.Clear();
            }

            if (null != mMaterialValueCache)
            {
                Dictionary<string, List<SMaterialValue>>.Enumerator it = mMaterialValueCache.GetEnumerator();
                while (it.MoveNext())
                {
                    var lst = it.Current.Value;
                    for (int i = lst.Count - 1; i >= 0; --i)
                        lst[i].Set();
                }

                mMaterialValueCache.Clear();
            }

            if (null != mShaderValueCache)
            {
                Dictionary<string, List<SShaderValue>>.Enumerator it = mShaderValueCache.GetEnumerator();
                while (it.MoveNext())
                {
                    var lst = it.Current.Value;
                    for (int i = lst.Count - 1; i >= 0; --i)
                        lst[i].Set();
                }

                mShaderValueCache.Clear();
            }
        }

        public virtual void DontDestroyOnLoad()
        {
#if xxx
            if (mObject)
                GameObject.DontDestroyOnLoad(mObject);
#endif
        }

        public virtual bool IsActive()
        {
            if (mObject)
                return mObject.activeSelf;

            return false;
        }

        public virtual void SetActive(bool active)
        {
            if (mObject)
                mObject.SetActive(active);
        }

        public virtual Component AddComponent(Type componentType)
        {
            return UCoreUtil.AddComponent(mObject, componentType);
        }

        public virtual Component GetComponent(Type componentType)
        {
            return UCoreUtil.GetComponent(mObject, componentType);
        }

        public virtual Component[] GetComponents(Type componentType)
        {
            return UCoreUtil.GetComponents(mObject, componentType);
        }

        public virtual Component GetComponentInChildren(Type componentType, bool includeInactive = false)
        {
            return UCoreUtil.GetComponentInChildren(mObject, componentType, includeInactive);
        }

        public virtual Component[] GetComponentsInChildren(Type componentType, bool includeInactive = false)
        {
            return UCoreUtil.GetComponentsInChildren(mObject, componentType, includeInactive);
        }

        public virtual void RmvComponent(Type componentType)
        {
            UCoreUtil.RmvComponent(mObject, componentType);
        }

        protected virtual bool CompareLayer(int layer)
        {
            if (GameEffect1 == -999)
            {
                GameEffect1 = LayerMask.NameToLayer("Game_Effect1");
            }
            if (GameEffect2 == -999)
            {
                GameEffect2 = LayerMask.NameToLayer("Game_Effect2");
            }
            if (GameEffect3 == -999)
            {
                GameEffect3 = LayerMask.NameToLayer("Game_Effect3");
            }
            if (layer == GameEffect1 || layer == GameEffect2 || layer == GameEffect3)
            {
                return true;
            }
            return false;
        }

        public virtual void Update(float dTime)
        {
            /*
                        if (null != mAnimator)
                        {
                            if (!mAniQueue.IsEmpty())
                            {
                                AnimatorStateInfo state = mAnimator.GetCurrentAnimatorStateInfo(0);
                                if (state.IsName(mAniQueue.AniName) && state.normalizedTime >= 1f)
                                {
                                    SAniInfo aniInfo = new SAniInfo();
                                    if (mAniQueue.Pop(ref aniInfo))
                                        mAnimator.CrossFade(aniInfo.mAniName, aniInfo.mBlendTime);
                                }
                            }
                        }
            */
            if (mCrossFadeAlpha)
            {
                mCrossFadeAlphaTickline += dTime;
                if (mCrossFadeAlphaTickline > mCrossFadeAlphaDuration)
                {
                    mCrossFadeAlpha = false;

                    mCrossFadeAlphaAddon = mCrossFadeAlphaTarget;
                    mCrossFadeAlphaTickline = mCrossFadeAlphaDuration;
                }

                UpdateAddonAlpha();
            }
        }

        public virtual void Tick(int dTick)
        {
        }

        public virtual void PlayAni(string aniName, float blendTime = 0f, bool stopAll = false, float startTime = float.NegativeInfinity)
        {
            if (null != mAnimator)
            {
                if (!mObject.activeSelf)
                    return;

                if (!mAnimator.HasState(0, Animator.StringToHash(aniName)))
                    return;

                //mAnimator.speed = 0.5f;
                if (blendTime > 0.0f)
                {
                    var currState = mAnimator.GetCurrentAnimatorStateInfo(0);

                    var nextState = mAnimator.GetNextAnimatorStateInfo(0);
                    if (nextState.fullPathHash != 0)
                    {
                        if (nextState.loop && nextState.IsName(aniName))
                            return;
                    } else if (currState.fullPathHash != 0)
                    {
                        if (currState.loop && currState.IsName(aniName))
                            return;
                    }
                    mAnimator.CrossFade(aniName, blendTime, -1, startTime);
                } else
                {
                    mAnimator.Play(aniName, -1, startTime);
                }
            } else if (null != mAnimation)
            {
                mAnimation.Play(aniName, stopAll ? PlayMode.StopAll : PlayMode.StopSameLayer);
            }
        }

        public virtual bool PlayQueuedAni(string aniName, bool immediately = false, bool stopAll = false)
        {
            if (null != mAnimation)
            {
                mAnimation.PlayQueued(aniName, immediately ? QueueMode.PlayNow : QueueMode.CompleteOthers, stopAll ? PlayMode.StopAll : PlayMode.StopSameLayer);

                return true;
            }

            return false;
        }

        public virtual void StopAni()
        {
            if (null != mAnimator && mObject.activeSelf)
            {
                var state = mAnimator.GetCurrentAnimatorStateInfo(0);

                mAnimator.Update(state.length * (1 - state.normalizedTime));
            }
            else if (null != mAnimation)
            {
                mAnimation.Stop();
            }
        }

        public virtual void SetAniSpeedScale(float speed)
        {
            if (null != mAnimator)
                mAnimator.speed = mAniSpeed * speed;
        }

        public virtual bool IsAniFinished(string stateName)
        {
            if (null != mAnimator && mObject.activeSelf)
            {
                AnimatorStateInfo state = mAnimator.GetCurrentAnimatorStateInfo(0);
                if (state.IsName(stateName) && state.normalizedTime >= 1f)
                    return true;

                return false;
            }
            return true;
        }

        public virtual void AddChild(UObjectBase obj, string bone, Vector3 offset, Vector3 scale, Vector3 rotation)
        {
            if (string.IsNullOrEmpty(bone))
            {
                obj.SetParent(this);
            }
            else
            {
                GameObject parent = GetBone(bone);
                if (null != parent)
                    obj.SetParent(parent);
                else
                    obj.SetParent(this);
            }
            obj.LocalTrans = offset;
            obj.LocalScale = scale;
            obj.LocalRotation = Quaternion.Euler(rotation);
        }

        public virtual void RmvChild(UObjectBase obj)
        {
            if (null != obj && obj.mObject)
                obj.mObject.transform.parent = null;
        }

        public virtual void SetParent(UObjectBase obj)
        {
            if (null != mObject)
                mObject.transform.parent = (null != obj) ? ((null != obj.GameEntity) ? obj.GameEntity.transform : null) : null;
        }

        public virtual void SetParent(GameObject obj)
        {
            if (null != mObject)
                mObject.transform.parent = (null != obj) ? obj.transform : null;
        }

        public virtual void SetParent(Transform obj)
        {
            if (null != mObject)
                mObject.transform.parent = obj;
        }

        public virtual GameObject GetBone(string name)
        {
            GameObject obj = Bones.Get(name);
            if (!obj)
            {
                if (null != mObject)
                {
                    obj = mObject.GetChild(name);
                    if (obj)
                        Bones.Add(name, obj);
                }
            }

            return obj;
        }

        public virtual void SetMaterialProperty(string name, float value)
        {
            if (mObject)
            {
                if (!FloatValueCache.ContainsKey(name))
                    FloatValueCache[name] = new List<SFloatValue>();

                var renderers = mObject.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; ++i)
                {
                    var r = renderers[i];

                    for (int k = 0; k < r.materials.Length; ++k)
                    {
                        var m = r.materials[k];
                        if (null == m)
                            continue;

                        var cache = FloatValueCache[name];
                        if (m.HasProperty(name))
                        {
                            cache.Add(new SFloatValue(name, m, m.GetFloat(name)));

                            m.SetFloat(name, value);
                        }
                    }
                }
            }
        }

        public virtual void SetMaterialProperty(string name, Color value)
        {

        }

        public virtual void SetMaterialProperty(string part, string name, float value)
        {
            if (mObject)
            {
                var obj = mObject.GetChild(part);
                if (obj)
                {
                    if (!FloatValueCache.ContainsKey(name))
                        FloatValueCache[name] = new List<SFloatValue>();

                    var renderers = obj.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < renderers.Length; ++i)
                    {
                        var r = renderers[i];

                        for (int k = 0; k < r.materials.Length; ++k)
                        {
                            var m = r.materials[k];
                            if (null == m)
                                continue;

                            var cache = FloatValueCache[name];
                            if (m.HasProperty(name))
                            {
                                cache.Add(new SFloatValue(name, m, m.GetFloat(name)));

                                m.SetFloat(name, value);
                            }
                        }
                    }
                }
            }
        }

        public virtual void SetMaterialProperty(string part, string name, Color value)
        {

        }

        public virtual void ResetMaterialProperty(string name, bool resetAll = true)
        {
            if (FloatValueCache.ContainsKey(name))
            {
                var cache = FloatValueCache[name];

                if (resetAll)
                {
                    for (int i = cache.Count - 1; i >= 0; --i)
                        cache[i].Set();

                    cache.Clear();
                }
                else
                {
                    var count = cache.Count;
                    if (count > 0)
                    {
                        cache[count - 1].Set();

                        cache.RemoveAt(count - 1);
                    }
                }
            }
        }

        public virtual void ReplaceMaterial(Material material)
        {
            if (mObject)
            {
                ResetMaterial(kDefaultMaterial, false);

                if (!MaterialValueCache.ContainsKey(kDefaultMaterial))
                    MaterialValueCache[kDefaultMaterial] = new List<SMaterialValue>();

                var cache = MaterialValueCache[kDefaultMaterial];

                var renderers = mObject.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; ++i)
                {
                    var r = renderers[i];
                    if (null == r.materials)
                        continue;

                    var mv = new SMaterialValue() { mRenderer = r, mMaterials = new Material[r.materials.Length] };

                    Material[] materials = new Material[r.materials.Length];

                    for (int k = 0; k < r.materials.Length; ++k)
                    {
                        var m = r.materials[k];

                        mv.mMaterials[k] = m;

                        materials[k] = material;
                    }
                    cache.Add(mv);

                    r.materials = materials;
                }
            }
        }

        public virtual void ReplaceMaterial(string part, Material material)
        {
            if (mObject)
            {
                var obj = mObject.GetChild(part);
                if (obj)
                {
                    ResetMaterial(part, false);

                    if (!MaterialValueCache.ContainsKey(part))
                        MaterialValueCache[part] = new List<SMaterialValue>();

                    var cache = MaterialValueCache[part];

                    var renderers = obj.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < renderers.Length; ++i)
                    {
                        var r = renderers[i];
                        if (null == r.materials)
                            continue;

                        var mv = new SMaterialValue() { mRenderer = r, mMaterials = new Material[r.materials.Length] };

                        Material[] materials = new Material[r.materials.Length];

                        for (int k = 0; k < r.materials.Length; ++k)
                        {
                            var m = r.materials[k];

                            mv.mMaterials[k] = m;

                            materials[k] = (null == m) ? null : material;
                        }
                        cache.Add(mv);

                        r.materials = materials;
                    }
                }
            }
        }

        public virtual void ResetMaterial(bool resetKey = true)
        {
            ResetMaterial(kDefaultMaterial, resetKey);
        }

        public virtual void ResetMaterial(string part, bool resetKey = true)
        {
            if (MaterialValueCache.ContainsKey(part))
            {
                var cache = MaterialValueCache[part];

                for (int i = 0; i < cache.Count; ++i)
                    cache[i].Set();
                cache.Clear();

                if (resetKey)
                    MaterialValueCache.Remove(part);
            }
        }

        public virtual void AddMaterial(Material material)
        {

        }

        public virtual void AddMaterial(string part, Material material)
        {

        }

        public virtual void RmvMaterial(bool resetKey = true)
        {

        }

        public virtual void RmvMaterial(string part, bool resetKey = true)
        {

        }

        public virtual void ReplaceShader(Shader shader)
        {
            if (mObject)
            {
                ResetShader(kDefaultMaterial, false);

                if (!ShaderValueCache.ContainsKey(kDefaultMaterial))
                    ShaderValueCache[kDefaultMaterial] = new List<SShaderValue>();

                var renderers = mObject.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; ++i)
                {
                    var r = renderers[i];

                    for (int k = 0; k < r.materials.Length; ++k)
                    {
                        var m = r.materials[k];
                        if (null == m)
                            continue;

                        var cache = ShaderValueCache[kDefaultMaterial];
                        cache.Add(new SShaderValue(m));

                        m.shader = shader;
                    }
                }
            }
        }

        public virtual void ReplaceShader(string part, Shader shader)
        {
            if (mObject)
            {
                var obj = mObject.GetChild(part);
                if (obj)
                {
                    ResetShader(part, false);

                    if (!ShaderValueCache.ContainsKey(part))
                        ShaderValueCache[part] = new List<SShaderValue>();

                    var renderers = obj.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < renderers.Length; ++i)
                    {
                        var r = renderers[i];

                        for (int k = 0; k < r.materials.Length; ++k)
                        {
                            var m = r.materials[k];
                            if (null == m)
                                continue;

                            var cache = ShaderValueCache[part];
                            cache.Add(new SShaderValue(m));

                            m.shader = shader;
                        }
                    }
                }
            }
        }

        public virtual void ResetShader(bool resetKey = true)
        {
            ResetShader(kDefaultMaterial, resetKey);
        }

        public virtual void ResetShader(string part, bool resetKey = true)
        {
            if (ShaderValueCache.ContainsKey(part))
            {
                var cache = ShaderValueCache[part];

                for (int i = 0; i < cache.Count; ++i)
                    cache[i].Set();

                cache.Clear();

                if (resetKey)
                    ShaderValueCache.Remove(part);
            }

        }

        public virtual void EanbleShadow(bool enable)
        {
            var renderers = mObject.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; ++i)
                renderers[i].shadowCastingMode = enable ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        public virtual void CrossFadeAlpha(float alpha, float duration)
        {
            if (!mCrossFadeAlpha)
            {
                mCrossFadeAlphaTickline = 0f;
                mCrossFadeAlphaCurrent = mCrossFadeAlphaAddon;
            }

            mCrossFadeAlpha = true;
            mCrossFadeAlphaTarget = alpha;
            mCrossFadeAlphaDuration = duration;
        }

        public virtual bool IsCrossFadeAlpha()
        {
            return mCrossFadeAlpha;
        }

        protected virtual void UpdateAddonAlpha()
        {
        }


        public virtual void SetObjSorting(string name, int layer = 0)
        {
        }

        public virtual void SetObjSorting(string name, string layerName, int layer = 0)
        {
        }

        public virtual void SetSortingGroup(string name, int layer = 0)
        {
        }

        public virtual void SetSortingGroup(string name, string layerName, int layer = 0)
        {
        }

    }
}
