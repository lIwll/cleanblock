using System;
using System.Collections.Generic;

using UnityEngine;

namespace UEngine
{
    [Serializable]
    public struct SRendererInfo
    {
        public Renderer mRenderer;
        public Terrain mTerrain;

        public int mLightmapIndex;
        public Vector4 mLightmapOffsetScale;
    }

    [DisallowMultipleComponent, ExecuteInEditMode]
    public class ULightmap : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        public SRendererInfo[] mRendererInfos;

        void Awake()
        {
            if (null != mRendererInfos)
                ApplyRendererInfo(mRendererInfos);
        }

        void Start()
        {
        }

        public static void ApplyRendererInfo(SRendererInfo[] rendererInfos)
        {
            for (int i = 0; i < rendererInfos.Length; i ++)
            {
                SRendererInfo info = rendererInfos[i];

                if (info.mRenderer != null)
                {
					var go = info.mRenderer.gameObject;

					var lodGroup = go.GetComponent< LODGroup >();
					if (null != lodGroup)
					{
						var lods = lodGroup.GetLODs();

						for (int k = 0; k < lods.Length; ++ k)
						{
							var lod = lods[k];

							for (int m = 0; m < lod.renderers.Length; ++ m)
							{
								var renderer = lod.renderers[m];
								if (null != renderer)
								{
									renderer.lightmapIndex = info.mLightmapIndex;
									renderer.lightmapScaleOffset = info.mLightmapOffsetScale;
								}
							}
						}
					} else
					{
						info.mRenderer.lightmapIndex = info.mLightmapIndex;
						info.mRenderer.lightmapScaleOffset = info.mLightmapOffsetScale;
					}
                }

                if (info.mTerrain != null)
                {
                    info.mTerrain.lightmapIndex         = info.mLightmapIndex;
                    info.mTerrain.lightmapScaleOffset   = info.mLightmapOffsetScale;
                }
            }
        }

        public static void ApplyRendererInfo(GameObject obj)
        {
            ULightmap lightmap = obj.GetComponent< ULightmap >();
            if (null != lightmap)
                ApplyRendererInfo(lightmap.mRendererInfos);
       }
    }
}
