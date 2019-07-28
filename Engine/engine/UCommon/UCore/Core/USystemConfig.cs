using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;

using UnityEngine;
namespace UEngine
{

    public enum GameTypeE { Game2D,Game3D  }

    public partial class USystemConfig
    {
        public const string kAssetFileNameExport = "GameRes";
        public const string kAssetFileName = "meta/GameRes";
        public const string kAssetFileExtension = ".u";
        public const string kEncFileExtension = ".enc";
        public const string kCfgExt = ".xml";
        public const string kCfgSubFolder = "data/";
        public const string kCfgName = "SystemConfig";
        public const string kResCollect = "meta/Resources.collect";
        public const string kResNames = "Resources.name";
        public const string kResErrors = "Resources.error";
        public const string kBuiltinShaders = "Builtin.shaders";
        public const string kCopyResNames = "CopyRes.name";
        public const string kPreloadRes = "meta/Preload.Res";
        public const string kPkgFolder = "packages";
        public const string kPkgManifest = "version.txt";
        public const string kPkgExtension = ".kpk";
        public const string kMainCamera = "MainCamera";
        public const string kGameDBPath = "Data/xml/";
        public const string kLevelDBPath = "Data/xml/level/";
        public const string kGridSuffix = "_grid";
        public const string kSceneSuffix = "_scene";
        public const string kLightSuffix = "_light";
        public const string kSceneInfos = "levels";
        public const string kDesignRoot = "DesignRoot";
        public const string kDesignPrefix = "Design_";
        public const string kDesignLayerName = "Design_Layer";
        public const string kDesignPath = "Design";
        public const string kDungeonPrefix = "Dungeon_";
        public const string kProgramPath = "Program";
        public const string kSceneTag = "Scene";
        public const string kEngineRoot = "Engine";

        public const string kLuaGenFolder = "Scripts/Lua/Core/Gen/";
        public const string kLuaEditorGenFolder = "Scripts/Editor/Script/Lua/";
        public const string kAgentGenFolder = "Scripts/Logic/Core/Gen/Agent/";
        public const string kAgentEditorGenFolder = "Scripts/Editor/Core/Gen/Agent/";
        public const string kLightmapFolder = "Assets/Resources/Lightmap/";
        public const string kSkillDataFile = "Data/bytes/flyskill.bytes";
		public const string kLanguagePath = "Data/translate/";
		public const string kLanguagePrefix = "translate_";
        public const string KEngineBytesSuffix = ".bytes";

        public static readonly  GameTypeE GAMETYPE = GameTypeE.Game3D;  //2D 代表2D 游戏，3D 代表3D  游戏

        private static ULocalSetting mInstance;
        public static ULocalSetting Instance
        {
            get
            {
                if (mInstance == null)
                {
                    LoadConfig();

                    if (mInstance == null)
                        mInstance = new ULocalSetting();
                }

                return mInstance;
            }
            set
            {
                mInstance = value;
            }
        }

        public static string AndroidPath
        {
            get { return string.Concat(Application.persistentDataPath, "/GameRes/"); }
        }

        public static string PCPath
        {
            get { return Application.streamingAssetsPath + "/"; }
        }

        public static string IOSPath
        {
            get { return string.Concat(Application.persistentDataPath, "/GameRes/"); }
        }

        public static string AssetPath
        {
            get
            {
                if (Instance.IsPublishMode)
                {
                    switch (Application.platform)
                    {
                        case RuntimePlatform.Android:
                            return Application.streamingAssetsPath + "/";
                        case RuntimePlatform.IPhonePlayer:
                            return Application.streamingAssetsPath + "/";
                        default:
                            return "file://" + Application.streamingAssetsPath + "/";
                    }
                } else
                {
                    switch (Application.platform)
                    {
                        case RuntimePlatform.Android:
                            return Application.dataPath + "/AssetBundles/android/";
                        case RuntimePlatform.IPhonePlayer:
                            return Application.dataPath + "/AssetBundles/ios/";
                        default:
                            return "file://" + Application.dataPath + "/AssetBundles/win64/";
                    }
                }
            }
        }

        public static string ScriptPath
        {
            get
            {
                if (Instance.IsPublishMode)
                {
                    return ResourceFolder;
                } else
                {
                    return Application.dataPath + "/" + kProgramPath + "/";
                }
            }
        }

        private static string mResourceFolder = null;
        public static string ResourceFolder
        {
            get
            {
                if (mResourceFolder == null)
                {
                    if (Instance.IsPublishMode)
                        mResourceFolder = OutterPath;
                    else if (Instance.IsDevelopMode)
                        mResourceFolder = Application.dataPath + "/Resources";
                    else
                    {
                        string assetPath = new DirectoryInfo(Application.dataPath).Parent.FullName.Replace("\\", "/") + "/publish/AssetBundles/";
                        if (Application.isEditor)
                        {
                            // TODO ???
                            mResourceFolder = assetPath + "win64/";
/*
                            switch (Application.platform)
                            {
                                case RuntimePlatform.Android:       mResourceFolder = assetPath + "android/";   break;
                                case RuntimePlatform.IPhonePlayer:  mResourceFolder = assetPath + "ios/";       break;
                                default:                            mResourceFolder = assetPath + "win64/";     break;
                            }
*/
                        } else
                        {
                            switch (Application.platform)
                            {
                                case RuntimePlatform.Android:       mResourceFolder = assetPath + "android/";   break;
                                case RuntimePlatform.IPhonePlayer:  mResourceFolder = assetPath + "ios/";       break;
                                default:                            mResourceFolder = assetPath + "win64/";     break;
                            }
                        }
                    }
                }

                return mResourceFolder;
            }
        }

        public static string OutterPath
        {
            get
            {
                if (Application.platform == RuntimePlatform.Android)
                    return AndroidPath;
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                    return IOSPath;
                else if (Application.platform == RuntimePlatform.WindowsPlayer)
                    return PCPath;
                else if (Application.platform == RuntimePlatform.WindowsEditor)
                    return PCPath;
                else if (Application.platform == RuntimePlatform.OSXEditor)
                    return IOSPath;

                return string.Empty;
            }
        }

        public static bool IsUseOutterConfig
        {
            get
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    if (Directory.Exists(String.Concat(AndroidPath, kCfgSubFolder)))
                        return true;
                } else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    if (Directory.Exists(String.Concat(IOSPath, kCfgSubFolder)))
                        return true;
                } else if (Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    if (Directory.Exists(String.Concat(PCPath, kCfgSubFolder)))
                        return true;
                }

                return false;
            }
        }

        public static string kCfgFileExtension
        {
            get
            {
                return !Instance.IsDevelopMode ? kCfgExt : "";
            }
        }

        public static bool Init()
        {
            try
            {
                LoadConfig();
            } catch (Exception)
            {
            }

            return true;
        }

        public static void SaveConfig(string path)
        {
            var root = new System.Security.SecurityElement("root");
            var props = mInstance.GetType().GetProperties();
			for (int i = 0; i < props.Length; ++ i)
			{
				var item = props[i];

				if (item.CanRead && item.CanWrite)
				{
					var value = item.GetGetMethod().Invoke(mInstance, null);
					if (item.Name == "DefaultExportableFileType")
					{
						string str = "";
						for (int k = 0; k < mInstance.DefaultExportableFileType.Length; ++ k)
							str += mInstance.DefaultExportableFileType[k] + ",";
						str = str.Substring(0, str.Length - 1);

						root.AddChild(new System.Security.SecurityElement(item.Name, str));
					} else if (item.Name == "IgnoreBuiltinShaders")
					{
						string str = "";
						for (int k = 0; k < mInstance.IgnoreBuiltinShaders.Length; ++ k)
							str += mInstance.IgnoreBuiltinShaders[k] + ",";
						str = str.Substring(0, str.Length - 1);

						root.AddChild(new System.Security.SecurityElement(item.Name, str));
					} else
					{
						if (null != value)
							root.AddChild(new System.Security.SecurityElement(item.Name, value.ToString()));
						else
							root.AddChild(new System.Security.SecurityElement(item.Name, ""));
					}
				}
			}
            UXMLParser.SaveText(path, root.ToString());
        }

        private static void LoadConfig()
        {
            try
            {
                var instance = LoadXML< ULocalSetting >(kCfgName);
                if (instance != null)
                    Instance = instance;
            } catch (Exception)
            {
            }
        }

        private static T LoadXML< T >(string path)
        {
            var text = UFileReaderProxy.ReadStringRes(path);

            return LoadXMLText< T >(text);
        }

        private static T LoadXMLText< T >(string text)
        {
            var obj = typeof(T).GetConstructor(Type.EmptyTypes).Invoke(null);

            try
            {
                if (String.IsNullOrEmpty(text))
                    return default(T);

                var xml = UXMLParser.LoadXML(text);

                var props = typeof(T).GetProperties();
                for (int i = 0; i < xml.Children.Count; ++ i)
                {
					System.Security.SecurityElement item = xml.Children[i] as System.Security.SecurityElement;
					for (int k = 0; k < props.Length; ++ k)
					{
						var prop = props[k];

						if (item.Tag == prop.Name)
						{
							if (prop.CanWrite)
								prop.SetValue(obj, UDataParser.GetValue(item.Text, prop.PropertyType), null);

							break;
						}
					}
                }
            } catch (Exception e)
            {
                ULogger.Error("load xml error: " + e.ToString());
            }

            return (T)obj;
        }
    }
    public class ULocalSetting
    {
        public bool IsDevelopMode { get; set; }

        public bool IsPublishMode { get; set; }


        private int mSynCreateRequestCount = 5;
        public int SynCreateRequestCount
        {
            get { return mSynCreateRequestCount; }
            set
            {
                if (value > 0)
                    mSynCreateRequestCount = value;
            }
        }

        private int mLuaGCTick = 1000; // 1秒
        public int LuaGCTick
        {
            get { return mLuaGCTick; }
            set
            {
                if (value > 0)
                    mLuaGCTick = value;
            }
        }

        public string MainGame { get; set; }

        public string MainCamera { get; set; }

        public float ShiedAlpha { get; set; }

        public string ShiedShader { get; set; }

        public string[] DefaultExportableFileType { get; set; }

        public string[] IgnoreBuiltinShaders { get; set; }

        // res_build.xml tag define.
        public string UITag { get; set; }

        public string SceneTag { get; set; }

        public string ModelTag { get; set; }

        public string EffectTag { get; set; }

        public string ImageTag { get; set; }

        public string AudioTag { get; set; }

        public string CutsceneTag { get; set; }

        public string OtherTag { get; set; }

        public string PreloadTag { get; set; }

        public string LevelExportPath { get; set; }

        public bool IsEnableLogWindow { get; set; }

        public int ObjectPoolSize { get; set; }

        public float Anti_AliasingCoefficient { get; set; }

        public bool UILoadResIsAsyn { get; set; }

        public string ZonesURLPattern { get; set; }

        public string AnnouncementURLPattern { get; set; }

        public bool IsEnableSDK { get; set; }

        public string BuglyAppID_Android { get; set; }
        public string BuglyAppID_iOS { get; set; }

        public bool IsClose { get; set; }

        public string[] Isthrow { get; set; }

        public bool isAutoCamera { get; set; }

        public float autoCameraDelayTime { get; set; }

        public float rotateSpeed { get; set; }

        public float autoRoataeMaxSpeed { get; set; }

        public float autoRotateSpeed { get; set; }

        public bool lockRotateX { get; set; }
		
        public bool UseHttps { get; set; }
        public string DefaultDirAddressIDByTimeZone { get; set; }
        public string DirAddress_1  { get; set; }
        public string DirAddress_2 { get; set; }
        public string DirAddress_3 { get; set; }

        public int DirPort_1 { get; set; }
        public int DirPort_2 { get; set; }
        public int DirPort_3 { get; set; }

        public string CdnAddress_1 { get; set; }
        public string CdnAddress_2 { get; set; }
        public string CdnAddress_3 { get; set; }

        public string CdnDict { get; set; }

        public int InnerVersion { get; set; }

        public int TestOpID { get; set; }
        
        public int CdnPort_1 { get; set; }
        public int CdnPort_2 { get; set; }
        public int CdnPort_3 { get; set; }

        public float cameraDistance { get; set; }

        public float UICacheIntervalTime { get; set; }

        public float TextCheckIntervalTime { get; set; }

        public float TextCheckTime { get; set; }

		public bool EnableProfile { get; set; }

        public string DataChannelID { get; set; }

        public bool IsLineFeed { get; set; }

        public string Punctuation { get; set; }

        public int CutPoint { get; set; }
        public int LargeLimitCount { get; set; }
        public int LittleLimitCount { get; set; }
        public float CheckWifiTime { get; set; }
        public bool IsCaChe { get; set; }
		public bool ForceSyncLoad { get; set; }

		public float MainTaskLimit { get; set; }

		public float BundleTidyTick { get; set; }

		public float BundleTidyAllTick { get; set; }

		public int PoolManagerCacheSize { get; set; }

		public int PoolManagerSize { get; set; }

		public float PoolManagerTidyTick { get; set; }

		public int MaxLoadTask { get; set; }

		public string ModelLodSuffix { get; set; }
		public string PlayerModelLodSuffix { get; set; }
		public string MainPlayerModelLodSuffix { get; set; }

		int mCameraBlackCullMask = -1;
		public int CameraBlackCullMask
		{
			get { return mCameraBlackCullMask; }
		}

		public string[] mCameraBlackCullMaskNames;
		public string[] CameraBlackCullMaskNames
		{
			get { return mCameraBlackCullMaskNames; }
		}

		string mCameraBlackCullMaskName;
		public string CameraBlackCullMaskName
		{
			get
			{
				return mCameraBlackCullMaskName;
			}
			set
			{
				if (mCameraBlackCullMaskName != value)
				{
					mCameraBlackCullMaskName = value;

					if (string.IsNullOrEmpty(value))
					{
						mCameraBlackCullMask = -1;
						mCameraBlackCullMaskNames = new string[] { };
					} else
					{
						var suffix = mCameraBlackCullMaskName.Split(';');

						mCameraBlackCullMask = 0;

						mCameraBlackCullMaskNames = new string[suffix.Length];
						for (int i = 0; i < suffix.Length; ++ i)
						{
							mCameraBlackCullMaskNames[i] = suffix[i].Trim();

							mCameraBlackCullMask += (1 << LayerMask.NameToLayer(mCameraBlackCullMaskNames[i]));
						}
					}
				}
			}
		}

		public float Audio3D_RolloffMin { get; set; }
		public float Audio3D_RolloffMax { get; set; }

#if ENABLE_PROFILER
		public int ProfileTimeout
		{ get; set; }
#endif

        public ULocalSetting()
        {
            IsDevelopMode = true;
            IsPublishMode = false;
            MainGame = "Script/main.lua";
            MainCamera = "Prefab/System/MainCamera.prefab";
            ShiedAlpha = 0.5f;
            //ShiedShader = "Transparent/Diffuse";
			ShiedShader = "Legacy Shaders/Transparent/Diffuse";
            DefaultExportableFileType = new string[]
            {
                ".prefab",
                ".fbx",
                ".controller",
                ".png",
                ".exr",
                ".tga",
                ".tif",
				".dds",
                ".jpg",
                ".jpeg",
                ".psd",
                ".asset",
				".ogg",
                ".mp3",
                ".wav",
                ".mat",
                ".unity",
                ".anim",
                ".shader",
                ".ttf",
            };

            IgnoreBuiltinShaders = new string[]
            {
                "Standard",
            };

            UITag = "ui";
            SceneTag = "scene";
            ModelTag = "model";
            EffectTag = "effect";
            ImageTag = "image";
            AudioTag = "audio";
			CutsceneTag = "cutscene";
            OtherTag = "other";
            PreloadTag = "preload";

            LevelExportPath = "Export";

            IsEnableLogWindow = false;

            ObjectPoolSize = 32;

            Anti_AliasingCoefficient = 2f;
            UILoadResIsAsyn = false;

            ZonesURLPattern = "http://{0}/DirServer/getListsInfoServlet?versionId={1}&amp;openId={2}";
            AnnouncementURLPattern = "http://{0}/DirServer/getBeforeEnteringNoticeServlet?curVersion={1}";

            TestOpID = 2173;

            CheckWifiTime = 5;

            IsEnableSDK = false;
            BuglyAppID_Android = "3254187baa";
            BuglyAppID_iOS = "9abdfc66fe";

            IsClose = false;
            Isthrow = new string[]{};
            isAutoCamera = false;
            autoCameraDelayTime = 3f;
            rotateSpeed = 20f;
            autoRoataeMaxSpeed = 300f;
            lockRotateX = false;
            autoRotateSpeed = 2.5f;
            cameraDistance = 0f;

            UICacheIntervalTime = 1200f;
            TextCheckIntervalTime = 1200f;
            TextCheckTime = 600f;

			EnableProfile = false;

            IsLineFeed = false;

            Punctuation = "";

            DataChannelID = "1";

            CutPoint = 2;
            LargeLimitCount = 2;
            LittleLimitCount = 2;
            IsCaChe = true;
			ForceSyncLoad = false;
			MainTaskLimit = 9999f;

			BundleTidyTick		= 120;
			BundleTidyAllTick	= 600;

			PoolManagerSize			= 10;
			PoolManagerCacheSize	= 10;
			PoolManagerTidyTick		= 60;

			MaxLoadTask = 64;

			// level Fast
			// level Simple
			// level Good
			// level Beautiful
			// level Fantastic
			ModelLodSuffix				= "_lod2;_lod2;_lod1;_lod1;";
			PlayerModelLodSuffix		= "_lod2;_lod2;_lod1;_lod1;_lod1";
			MainPlayerModelLodSuffix	= "_lod2;_lod1;_lod1;;";

			CameraBlackCullMaskName = "Game_Player;Game_Effect1;Game_Effect2;Game_Effect3;Game_Monster;UI";

            DefaultDirAddressIDByTimeZone = "0;0;0;0;1;1;1;1;1;1;1;1;1;2;2;2;2;2;2;2;2;2;0;0";

			Audio3D_RolloffMin = 5f;
			Audio3D_RolloffMax = 15f;

#if ENABLE_PROFILER
			ProfileTimeout = 20;
#endif
        }

        public int GetDefaultDirID()
        {
            int hour = (TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours + 24) % 24;            
            var defaultIDs = DefaultDirAddressIDByTimeZone.Split(';');
            return Int32.Parse(defaultIDs[hour]);
        }

        public void AddTable(string errList)
        {
            List< string > tempList = new List< string >();

            if (Isthrow != null)
				tempList.AddRange(Isthrow);
            tempList.Add(errList);

            Isthrow = tempList.ToArray();
        }

		string[] mModelLodSuffix = null;
		public string GetModelSuffix(int level)
		{
			if (null == mModelLodSuffix)
			{
				var suffix = ModelLodSuffix.Split(';');

				mModelLodSuffix = new string[suffix.Length];
				for (int i = 0; i < suffix.Length; ++ i)
					mModelLodSuffix[i] = suffix[i].Trim();
			}

			if (level >= 0 && level < mModelLodSuffix.Length)
				return mModelLodSuffix[level];

			return string.Empty;
		}

		string[] mPlayerModelLodSuffix = null;
		public string GetPlayerModelSuffix(int level)
		{
			if (null == mPlayerModelLodSuffix)
			{
				var suffix = PlayerModelLodSuffix.Split(';');

				mPlayerModelLodSuffix = new string[suffix.Length];
				for (int i = 0; i < suffix.Length; ++ i)
					mPlayerModelLodSuffix[i] = suffix[i].Trim();
			}

			if (level >= 0 && level < mPlayerModelLodSuffix.Length)
				return mPlayerModelLodSuffix[level];

			return string.Empty;
		}

		string[] mMainPlayerModelLodSuffix = null;
		public string GetMainPlayerModelSuffix(int level)
		{
			if (null == mMainPlayerModelLodSuffix)
			{
				var suffix = MainPlayerModelLodSuffix.Split(';');

				mMainPlayerModelLodSuffix = new string[suffix.Length];
				for (int i = 0; i < suffix.Length; ++ i)
					mMainPlayerModelLodSuffix[i] = suffix[i].Trim();
			}

			if (level >= 0 && level < mMainPlayerModelLodSuffix.Length)
				return mMainPlayerModelLodSuffix[level];

			return string.Empty;
		}
    }
}
