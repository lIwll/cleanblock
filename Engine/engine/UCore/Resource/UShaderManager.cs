using System;
using System.Collections.Generic;

using UnityEngine;

namespace UEngine
{
    public class UShaderManager
    {
        static List< IResource > mAssets = new List< IResource >();

        static Dictionary< string, Shader > mShaders = new Dictionary< string, Shader >();

        static UShaderManager()
        {
        }

        public static void RegisterShader(IResource asset, Shader shader)
        {
            if (null == asset.Res)
                return;

			UBundleManager.AddReferenceCount(asset);

            if (!mAssets.Contains(asset))
                mAssets.Add(asset);

            if (!mShaders.ContainsKey(shader.name))
                mShaders.Add(shader.name, shader);
        }

        public static void RegisterShader(Shader shader)
        {
            if (null == shader)
                return;

            ULogger.Info("register shader {0}", shader.name);

            if (!mShaders.ContainsKey(shader.name))
                mShaders.Add(shader.name, shader);
        }

        public static void ClearCache()
        {
            mShaders.Clear();

            for (int i = 0; i < mAssets.Count; ++ i)
			{
                var asset = mAssets[i];

				var list = UBundleManager.GetAssetInfo(asset);
					list.Reverse();
				for (int k = 0; k < list.Count; ++ k)
					UBundleManager.ReleaseBundle(list[k] as UBundle);
			}
            mAssets.Clear();
        }

        public static Shader FindShader(string name)
        {
            Shader shader = null;

			if (mShaders.ContainsKey(name))
			{
				shader = mShaders[name];
			} else
			{
				if (null == shader)
				{
					shader = Shader.Find(name);
					if (shader)
						mShaders.Add(name, shader);
				}
			}

            return shader;
        }
    }
}
