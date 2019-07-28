using System;
using System.Security;
using System.Collections.Generic;

using UnityEngine;

namespace UEngine.Data
{
    public class USceneData
    {
        public enum ELightTime
        {
            Default,
            Dawn,
            Day,
            Dusk,
            Night,
        }

        public class UCameraData
        {
            public Color mColor;

            public float mFar;

			public class UBloomEffect
			{
				public enum EResolution
				{
					Low = 0,
					High = 1,
				}

				public bool mEnable;

				public float mThreshold = 0.25f;

				public float mIntensity = 0.75f;

				public float mBlurSize = 1.0f;

				public EResolution mResolution = EResolution.Low;

				public int mBlurIterations = 1;
			}
/*
			public UBloomEffect mBloomEffect = new UBloomEffect();
*/

			public class UFastBloomEffect
			{
				public bool mEnable;

				public float mBloomAmount = 1f;

				public float mBlurAmount = 2f;

				public float mFadeAmount = 0.2f;
			}
			public UFastBloomEffect mFastBloomEffect = new UFastBloomEffect();

            public class UColorCorrectionEffect
            {
                public bool mEnable;

                public float mColorTemp = 0.0f;

                public float mColorTint = 0.0f;

                public float mExposure = 1.0f;

                public float mSaturation = 1.0f;

                public AnimationCurve mCurveR = AnimationCurve.Linear(0, 0, 1, 1);
                public AnimationCurve mCurveG = AnimationCurve.Linear(0, 0, 1, 1);
                public AnimationCurve mCurveB = AnimationCurve.Linear(0, 0, 1, 1);

                public AnimationCurve mCurveRGB = AnimationCurve.Linear(0, 0, 1, 1);
            }
            public UColorCorrectionEffect mColorCorrectionEffect = new UColorCorrectionEffect();
        }
        public UCameraData mCamera;

        public class UEnvironmentData
        {
            public Color mAmbientLight;

            public bool mEnableFog;
            public Color mFogColor;
            public FogMode mFogMode;
            public float mLinearFogStart;
            public float mLinearFogEnd;
        }
        public UEnvironmentData mEnvironment;

        public class ULightmapData
        {
            public string ColorMap;
            public string DirMap;
            public string ShadowMap;
        }

        public string LightmapPath { get; set; }
        public ULightmapData[] Lightmap { get; set; }

        public Dictionary< string, bool > Objects { get; set; }

        public class UHeightmapData
        {
            public float mInterval;
            public float mStepX;
            public float mStepZ;
            public float mMinX;
            public float mMaxX;
            public float mMinY;
            public float mMaxY;
            public float mMinZ;
            public float mMaxZ;

            public List< Vector3 > mData = new List< Vector3 >();
        }
        public UHeightmapData mHeightmapData;

        public USceneData()
        {
            Objects = new Dictionary< string, bool >();
        }

		private static Dictionary< string, SecurityElement > mNodeMap = null;

        private static Dictionary< string, USceneData > mDataMap = null;
        protected static Dictionary< string, USceneData > DataMap
        {
            get
            {
                if (null == mDataMap)
                {
                    var text = UFileAccessor.ReadStringFile(USystemConfig.kGameDBPath + USystemConfig.kSceneInfos + USystemConfig.kCfgExt);
                    if (null == text)
                    {
                        ULogger.Error("error to load scene map.");

                        return null;
                    }

                    var xml = UXMLParser.LoadXML(text);
                    if (null == xml)
                    {
                        ULogger.Error("error to load scene map.");

                        return null;
                    }

                    mDataMap = new Dictionary< string, USceneData >();
					mNodeMap = new Dictionary< string, SecurityElement >();

                    for (int i = 0; i < xml.Children.Count; ++ i)
                    {
						SecurityElement scene = xml.Children[i] as SecurityElement;
#if xxxx
                        USceneData data = new USceneData();

                        var info_n = scene.SearchForChildByTag("info");
                        if (null != info_n)
                        {
                            text = UFileAccessor.ReadStringFile(info_n.Text);
                            if (null != text)
                            {
                                var info_xml = UXMLParser.LoadXML(text);
                                if (null != info_xml)
                                {
                                    var camera_n = info_xml.SearchForChildByTag("camera");
                                    if (null != camera_n)
                                        data.mCamera = (UCameraData)UXMLSerializer.SerializeFromXML(camera_n);

                                    var environment_n = info_xml.SearchForChildByTag("environment");
                                    if (null != environment_n)
                                        data.mEnvironment = (UEnvironmentData)UXMLSerializer.SerializeFromXML(environment_n);

                                    var objects_n = info_xml.SearchForChildByTag("objects");
                                    if (null != objects_n)
                                    {
                                        if (null != objects_n.Children)
                                        {
                                            foreach (SecurityElement obj in objects_n.Children)
                                                data.Objects.Add(obj.Attribute("path"), bool.Parse(obj.Attribute("static")));
                                        }
                                    }
                                } else
                                {
                                    ULogger.Error("error to load scene info.");
                                }
                            } else
                            {
                                ULogger.Error("error to load scene info.");
                            }
                        }

                        var light_n = scene.SearchForChildByTag("light");
                        if (null != light_n)
                        {
                            text = UFileAccessor.ReadStringFile(light_n.Text);
                            if (null != text)
                            {
                                var light_xml = UXMLParser.LoadXML(text);
                                if (null != light_xml)
                                {
                                    data.LightmapPath = light_xml.Attribute("path").Substring("Assets/".Length);

                                    List< ULightmapData > lightMaps = new List< ULightmapData >();

                                    if (null != light_xml.Children)
                                    {
                                        foreach (System.Security.SecurityElement l in light_xml.Children)
                                        {
                                            ULightmapData lightData = new ULightmapData();

                                            var c_n = l.SearchForChildByTag("color");
                                            if (null != c_n)
                                                lightData.ColorMap = c_n.Text;

                                            var d_n = l.SearchForChildByTag("dir");
                                            if (null != d_n)
                                                lightData.DirMap = d_n.Text;

                                            var s_n = l.SearchForChildByTag("shadow");
                                            if (null != s_n)
                                                lightData.ShadowMap = s_n.Text;

                                            lightMaps.Add(lightData);
                                        }
                                    }
                                    data.Lightmap = lightMaps.ToArray();
                                }
                            }
                        }

                        var grid_n = scene.SearchForChildByTag("grid");
                        if (null != grid_n)
                        {
                            text = UFileAccessor.ReadStringFile(grid_n.Text);
                            if (null != text)
                            {
                                var grid_xml = UXMLParser.LoadXML(text);
                                if (null != grid_xml)
                                {
                                    data.mHeightmapData = new UHeightmapData();

                                    var desc_n = grid_xml.SearchForChildByTag("desc");
                                    if (null != desc_n)
                                    {
                                        data.mHeightmapData.mInterval = float.Parse(desc_n.SearchForChildByTag("interval").Text);

                                        if (null != desc_n.SearchForChildByTag("stepX"))
                                            data.mHeightmapData.mStepX = float.Parse(desc_n.SearchForChildByTag("stepX").Text);
                                        if (null != desc_n.SearchForChildByTag("stepZ"))
                                            data.mHeightmapData.mStepZ = float.Parse(desc_n.SearchForChildByTag("stepZ").Text);

                                        data.mHeightmapData.mMinX = float.Parse(desc_n.SearchForChildByTag("minX").Text);
                                        data.mHeightmapData.mMaxX = float.Parse(desc_n.SearchForChildByTag("maxX").Text);

                                        data.mHeightmapData.mMinY = float.Parse(desc_n.SearchForChildByTag("minY").Text);
                                        data.mHeightmapData.mMaxY = float.Parse(desc_n.SearchForChildByTag("maxY").Text);

                                        data.mHeightmapData.mMinZ = float.Parse(desc_n.SearchForChildByTag("minZ").Text);
                                        data.mHeightmapData.mMaxZ = float.Parse(desc_n.SearchForChildByTag("maxZ").Text);
                                    }

                                    var grids_n = grid_xml.SearchForChildByTag("grids");
                                    if (null != grids_n && null != grids_n.Children)
                                    {
                                        foreach (SecurityElement grid in grids_n.Children)
                                        {
                                            var str = grid.Text.Substring(1, grid.Text.Length - 2);
                                            var s = str.Split(',');
                                            if (s.Length == 3)
                                            {
                                                Vector3 v = Vector3.zero;

                                                v.x = float.Parse(s[0]);
                                                v.y = float.Parse(s[1]);
                                                v.z = float.Parse(s[2]);

                                                data.mHeightmapData.mData.Add(v);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        mDataMap.Add(scene.Attribute("name"), data);
#endif
                        mDataMap.Add(scene.Attribute("name"), null);
						mNodeMap.Add(scene.Attribute("name"), scene);
                    }
                }

                return mDataMap;
            }
        }

		public static bool IsExist(string scene)
		{
			return DataMap.ContainsKey(scene);
		}

		public static USceneData Get(string scene)
		{
			if (!DataMap.ContainsKey(scene))
				return null;

			USceneData data = mDataMap[scene];
			if (null != data)
				return data;

			var sceneN = mNodeMap.Get(scene);

			data = new USceneData();

			var info_n = sceneN.SearchForChildByTag("info");
			if (null != info_n)
			{
				string text = UFileAccessor.ReadStringFile(info_n.Text);
				if (null != text)
				{
					var info_xml = UXMLParser.LoadXML(text);
					if (null != info_xml)
					{
						var camera_n = info_xml.SearchForChildByTag("camera");
						if (null != camera_n)
							data.mCamera = (UCameraData)UXMLSerializer.SerializeFromXML(camera_n);

						var environment_n = info_xml.SearchForChildByTag("environment");
						if (null != environment_n)
							data.mEnvironment = (UEnvironmentData)UXMLSerializer.SerializeFromXML(environment_n);

						var objects_n = info_xml.SearchForChildByTag("objects");
						if (null != objects_n)
						{
							if (null != objects_n.Children)
							{
								for (int i = 0; i < objects_n.Children.Count; ++ i)
								{
									SecurityElement obj = objects_n.Children[i] as SecurityElement;

									data.Objects.Add(obj.Attribute("path"), bool.Parse(obj.Attribute("static")));
								}
							}
						}
					} else
					{
						ULogger.Error("error to load scene info.");
					}
				} else
				{
					ULogger.Error("error to load scene info.");
				}
			}

			var light_n = sceneN.SearchForChildByTag("light");
			if (null != light_n)
			{
				string text = UFileAccessor.ReadStringFile(light_n.Text);
				if (null != text)
				{
					var light_xml = UXMLParser.LoadXML(text);
					if (null != light_xml)
					{
						data.LightmapPath = light_xml.Attribute("path").Substring("Assets/".Length);

						List< ULightmapData > lightMaps = new List< ULightmapData >();

						if (null != light_xml.Children)
						{
							for (int i = 0; i < light_xml.Children.Count; ++ i)
							{
								System.Security.SecurityElement l = light_xml.Children[i] as System.Security.SecurityElement;

								ULightmapData lightData = new ULightmapData();

								var c_n = l.SearchForChildByTag("color");
								if (null != c_n)
									lightData.ColorMap = c_n.Text;

								var d_n = l.SearchForChildByTag("dir");
								if (null != d_n)
									lightData.DirMap = d_n.Text;

								var s_n = l.SearchForChildByTag("shadow");
								if (null != s_n)
									lightData.ShadowMap = s_n.Text;

								lightMaps.Add(lightData);
							}
						}
						data.Lightmap = lightMaps.ToArray();
					}
				}
			}

			var grid_n = sceneN.SearchForChildByTag("grid");
			if (null != grid_n)
			{
				string text = UFileAccessor.ReadStringFile(grid_n.Text);
				if (null != text)
				{
					var grid_xml = UXMLParser.LoadXML(text);
					if (null != grid_xml)
					{
						data.mHeightmapData = new UHeightmapData();

						var desc_n = grid_xml.SearchForChildByTag("desc");
						if (null != desc_n)
						{
							data.mHeightmapData.mInterval = float.Parse(desc_n.SearchForChildByTag("interval").Text);

							if (null != desc_n.SearchForChildByTag("stepX"))
								data.mHeightmapData.mStepX = float.Parse(desc_n.SearchForChildByTag("stepX").Text);
							if (null != desc_n.SearchForChildByTag("stepZ"))
								data.mHeightmapData.mStepZ = float.Parse(desc_n.SearchForChildByTag("stepZ").Text);

							data.mHeightmapData.mMinX = float.Parse(desc_n.SearchForChildByTag("minX").Text);
							data.mHeightmapData.mMaxX = float.Parse(desc_n.SearchForChildByTag("maxX").Text);

							data.mHeightmapData.mMinY = float.Parse(desc_n.SearchForChildByTag("minY").Text);
							data.mHeightmapData.mMaxY = float.Parse(desc_n.SearchForChildByTag("maxY").Text);

							data.mHeightmapData.mMinZ = float.Parse(desc_n.SearchForChildByTag("minZ").Text);
							data.mHeightmapData.mMaxZ = float.Parse(desc_n.SearchForChildByTag("maxZ").Text);
						}

						var grids_n = grid_xml.SearchForChildByTag("grids");
						if (null != grids_n && null != grids_n.Children)
						{
							for (int i = 0; i < grids_n.Children.Count; ++ i)
							{
								SecurityElement grid = grids_n.Children[i] as SecurityElement;

								var str = grid.Text.Substring(1, grid.Text.Length - 2);
								var s = str.Split(',');
								if (s.Length == 3)
								{
									Vector3 v = Vector3.zero;

									v.x = float.Parse(s[0]);
									v.y = float.Parse(s[1]);
									v.z = float.Parse(s[2]);

									data.mHeightmapData.mData.Add(v);
								}
							}
						}
					}
				}
			}

			mDataMap[sceneN.Attribute("name")] = data;

			return data;
		}

		public static bool Clear(string scene)
		{
			if (!DataMap.ContainsKey(scene))
				return false;

			USceneData data = mDataMap[scene];
			if (null != data)
				return false;
			data.mHeightmapData = null;

			mDataMap[scene] = null;

			return true;
		}
    }

    public class ULevelData
    {
        public class UEntity
        {
            //UAgentProxy mAgent = null;
        }
        List< UEntity > mEntities = new List< UEntity >();

        public string mScene;

        private static Dictionary< string, ULevelData > mDataMap = null;
        public static Dictionary< string, ULevelData > DataMap
        {
            get
            {
                if (null == mDataMap)
                {
                    mDataMap = new Dictionary< string, ULevelData >();

                    var files = UFileAccessor.GetFileNamesByDirectory(USystemConfig.kLevelDBPath, false);
                    for (int i = 0; i < files.Length; ++ i)
                    {
						var file = files[i];

                        if (!file.EndsWith(USystemConfig.kCfgExt))
                            continue;

                        ULevelData data = new ULevelData();

                        var text = UFileAccessor.ReadStringFile(file);
                        if (null != text)
                        {
                            var xml = UXMLParser.LoadXML(text);
                            if (null != xml)
                            {
                                data.mScene = xml.Attribute("scene");

                                var entities_n = xml.SearchForChildByTag("entities");
                                if (null != entities_n && null != entities_n.Children)
                                {
                                    for (int k = 0; k < entities_n.Children.Count; ++ k)
                                    {
										SecurityElement entity = entities_n.Children[k] as SecurityElement;

                                        var etype = entity.Attribute("type");

                                        var pos = UXMLSerializer.SerializeFromXML(entity.SearchForChildByTag("position"));
                                        var rot = UXMLSerializer.SerializeFromXML(entity.SearchForChildByTag("rotation"));
                                        var scl = UXMLSerializer.SerializeFromXML(entity.SearchForChildByTag("scale"));
                                    }
                                }
                            }
                        }

                        mDataMap.Add(UCoreUtil.GetFileNameWithoutExtention(file), data);
                    }
                }

                return mDataMap;
            }
        }
    }
}
