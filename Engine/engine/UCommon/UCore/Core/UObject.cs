using System;
using System.Collections.Generic;

using UnityEngine;

namespace UEngine
{
    public class UObject : UObjectBase
    {

        protected struct SMaterialCache
        {
            Renderer
                mRenderer;
            public SMaterialCache(Renderer _r)
            {
                mRenderer = _r;
            }

            public void Set()
            {
                if (null != mRenderer)
                    mRenderer.materials = new Material[] { mRenderer.materials[0] };
            }
        }
        protected Dictionary<string, List<SMaterialCache>> mMaterialCache = null;
        protected Dictionary<string, List<SMaterialCache>> MaterialCache
        {
            get
            {
                if (null == mMaterialCache)
                    mMaterialCache = new Dictionary<string, List<SMaterialCache>>();

                return mMaterialCache;
            }
        }

        struct SColorValue
        {
            string
                mName;
            Color
                mValue;
            Material
                mMaterial;

            public SColorValue(string _n, Material _m, Color _v)
            {
                mName = _n;
                mValue = _v;
                mMaterial = _m;
            }

            public void Set()
            {
                if (null != mMaterial)
                    mMaterial.SetColor(mName, mValue);
            }
        }
        private Dictionary<string, List<SColorValue>> mColorValueCache = null;
        private Dictionary<string, List<SColorValue>> ColorValueCache
        {
            get
            {
                if (null == mColorValueCache)
                    mColorValueCache = new Dictionary<string, List<SColorValue>>();

                return mColorValueCache;
            }
        }


        public UObject()
            : base()
        {
        }

        public UObject(UnityEngine.Object obj, bool destroySelf)
            : base(obj, destroySelf)
        {
        }

        ~UObject()
        {
        }


        public override void Init(UnityEngine.Object obj)
        {
            base.Init(obj);
        }

        public override void Dispose()
        {
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        public override void Dispose(bool releaseRes)
        {
            base.Dispose(releaseRes);
        }

        public override void Destroy()
        {
            base.Destroy();

            if (null != mMaterialCache)
            {
                Dictionary<string, List<SMaterialCache>>.Enumerator it = mMaterialCache.GetEnumerator();
                while (it.MoveNext())
                {
                    var lst = it.Current.Value;
                    for (int i = lst.Count - 1; i >= 0; --i)
                        lst[i].Set();
                }

                mMaterialCache.Clear();
            }

            if (null != mColorValueCache)
            {
                Dictionary<string, List<SColorValue>>.Enumerator it = mColorValueCache.GetEnumerator();
                while (it.MoveNext())
                {
                    var lst = it.Current.Value;
                    for (int i = lst.Count - 1; i >= 0; --i)
                        lst[i].Set();
                }

                mColorValueCache.Clear();
            }
            Dispose(true);
        }

        public override void DontDestroyOnLoad()
        {
            base.DontDestroyOnLoad();
        }

        public override bool IsActive()
        {
            return base.IsActive();
        }

        public override void SetActive(bool active)
        {
            base.SetActive(active);
        }

        public override Component AddComponent(Type componentType)
        {
            return base.AddComponent(componentType);
        }

        public override Component GetComponent(Type componentType)
        {
            return base.GetComponent(componentType);
        }

        public override Component[] GetComponents(Type componentType)
        {
            return base.GetComponents(componentType);
        }

        public override Component GetComponentInChildren(Type componentType, bool includeInactive = false)
        {
            return base.GetComponentInChildren(componentType, includeInactive);
        }

        public override Component[] GetComponentsInChildren(Type componentType, bool includeInactive = false)
        {
            return base.GetComponentsInChildren(componentType, includeInactive);
        }

        public override void RmvComponent(Type componentType)
        {
            base.RmvComponent(componentType);
        }

        protected override bool CompareLayer(int layer)
        {
            return base.CompareLayer(layer);
        }

        public override void Update(float dTime)
        {
            base.Update(dTime);
        }

        public override void Tick(int dTick)
        {
        }

        public override void PlayAni(string aniName, float blendTime = 0f, bool stopAll = false, float startTime = float.NegativeInfinity)
        {
            base.PlayAni(aniName, blendTime, stopAll, startTime);
        }

        public override bool PlayQueuedAni(string aniName, bool immediately = false, bool stopAll = false)
        {
            return base.PlayQueuedAni(aniName, immediately, stopAll);
        }

        public override void StopAni()
        {
            base.StopAni();
        }

        public override void SetAniSpeedScale(float speed)
        {
            base.SetAniSpeedScale(speed);
        }

        public override bool IsAniFinished(string stateName)
        {
            return base.IsAniFinished(stateName);
        }

        public override void AddChild(UObjectBase obj, string bone, Vector3 offset, Vector3 scale, Vector3 rotation)
        {
            if (null != obj)
            {
                Bones.Clear();

                base.AddChild(obj, bone, offset, scale, rotation);
            }
        }

        public override void RmvChild(UObjectBase obj)
        {
            Bones.Clear();

            base.RmvChild(obj);
        }

        public override void SetParent(UObjectBase obj)
        {
            Bones.Clear();

            base.SetParent(obj);
        }

        public override void SetParent(GameObject obj)
        {
            Bones.Clear();

            base.SetParent(obj);
        }

        public override void SetParent(Transform obj)
        {
            Bones.Clear();

            base.SetParent(obj);
        }

        public override GameObject GetBone(string name)
        {
            return base.GetBone(name);
        }

        public override void SetMaterialProperty(string name, float value)
        {
            base.SetMaterialProperty(name, value);
        }

        public override void SetMaterialProperty(string name, Color value)
        {
            if (mObject)
            {
                if (!ColorValueCache.ContainsKey(name))
                    ColorValueCache[name] = new List<SColorValue>();

                var renderers = mObject.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; ++i)
                {
                    var r = renderers[i];

                    for (int k = 0; k < r.materials.Length; ++k)
                    {
                        var m = r.materials[k];
                        if (null == m)
                            continue;

                        var cache = ColorValueCache[name];
                        if (m.HasProperty(name))
                        {
                            cache.Add(new SColorValue(name, m, m.GetColor(name)));

                            m.SetColor(name, value);
                        }
                    }
                }
            }
        }

        public override void SetMaterialProperty(string part, string name, float value)
        {
            base.SetMaterialProperty(part, name, value);
        }

        public override void SetMaterialProperty(string part, string name, Color value)
        {
            if (mObject)
            {
                var obj = mObject.GetChild(part);
                if (obj)
                {
                    if (!ColorValueCache.ContainsKey(name))
                        ColorValueCache[name] = new List<SColorValue>();

                    var renderers = obj.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < renderers.Length; ++i)
                    {
                        var r = renderers[i];

                        for (int k = 0; k < r.materials.Length; ++k)
                        {
                            var m = r.materials[k];
                            if (null == m)
                                continue;

                            var cache = ColorValueCache[name];
                            if (m.HasProperty(name))
                            {
                                cache.Add(new SColorValue(name, m, m.GetColor(name)));

                                m.SetColor(name, value);
                            }
                        }
                    }
                }
            }
        }

        public override void ResetMaterialProperty(string name, bool resetAll = true)
        {
            base.ResetMaterialProperty(name, resetAll);

            if (ColorValueCache.ContainsKey(name))
            {
                var cache = ColorValueCache[name];

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

        public override void ReplaceMaterial(Material material)
        {
            base.ReplaceMaterial(material);
        }

        public override void ReplaceMaterial(string part, Material material)
        {
            base.ReplaceMaterial(part, material);
        }

        public override void ResetMaterial(bool resetKey = true)
        {
            base.ResetMaterial(resetKey);
        }

        public override void ResetMaterial(string part, bool resetKey = true)
        {
            base.ResetMaterial(part, resetKey);
        }

        public override void AddMaterial(Material material)
        {
            if (mObject)
            {
                RmvMaterial(kDefaultMaterial, false);

                if (!MaterialCache.ContainsKey(kDefaultMaterial))
                    MaterialCache[kDefaultMaterial] = new List<SMaterialCache>();

                var renderers = mObject.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; ++i)
                {
                    var r = renderers[i];
                    if (null == r || r.materials.Length != 1) // 目前仅支持单材质
                        continue;

                    var cache = MaterialCache[kDefaultMaterial];
                    cache.Add(new SMaterialCache(r));

                    r.materials = new Material[] { r.materials[0], new Material(material) };
                }
            }
        }

        public override void AddMaterial(string part, Material material)
        {
            if (mObject)
            {
                var obj = mObject.GetChild(part);
                if (obj)
                {
                    RmvMaterial(part, false);

                    if (!MaterialCache.ContainsKey(part))
                        MaterialCache[part] = new List<SMaterialCache>();

                    var renderers = obj.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < renderers.Length; ++i)
                    {
                        var r = renderers[i];
                        if (null == r || r.materials.Length != 1) // 目前仅支持单材质
                            continue;

                        var cache = MaterialCache[part];
                        cache.Add(new SMaterialCache(r));

                        r.materials = new Material[] { r.materials[0], new Material(material) };
                    }
                }
            }
        }

        public override void RmvMaterial(bool resetKey = true)
        {
            RmvMaterial(kDefaultMaterial, resetKey);
        }

        public override void RmvMaterial(string part, bool resetKey = true)
        {
            if (MaterialCache.ContainsKey(part))
            {
                var cache = MaterialCache[part];

                for (int i = 0; i < cache.Count; ++i)
                    cache[i].Set();

                cache.Clear();

                if (resetKey)
                    MaterialCache.Remove(part);
            }
        }

        public override void ReplaceShader(Shader shader)
        {
            base.ReplaceShader(shader);
        }

        public override void ReplaceShader(string part, Shader shader)
        {
            base.ReplaceShader(part, shader);
        }

        public override void ResetShader(bool resetKey = true)
        {
            ResetShader(kDefaultMaterial, resetKey);
        }

        public override void ResetShader(string part, bool resetKey = true)
        {
            base.ResetShader(part, resetKey);
        }

        public override void EanbleShadow(bool enable)
        {
            base.EanbleShadow(enable);
        }

        public override void CrossFadeAlpha(float alpha, float duration)
        {
            base.CrossFadeAlpha(alpha, duration);
        }

        public override bool IsCrossFadeAlpha()
        {
            return base.IsCrossFadeAlpha();
        }

        protected override void UpdateAddonAlpha()
        {
            mCrossFadeAlphaCurrent = Mathf.Lerp(mCrossFadeAlphaAddon, mCrossFadeAlphaTarget, mCrossFadeAlphaTickline / mCrossFadeAlphaDuration);
            if (mObject)
            {
                var renderers = mObject.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; ++i)
                {
                    var r = renderers[i];

                    for (int k = 0; k < r.materials.Length; ++k)
                    {
                        var m = r.materials[k];
                        if (null == m)
                            continue;

                        m.color = new Color(m.color.r, m.color.g, m.color.b, m.color.a * mCrossFadeAlphaCurrent);
                    }
                }
            }
        }

    }
}
