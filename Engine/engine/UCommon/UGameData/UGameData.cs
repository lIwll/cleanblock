using System;
using System.Reflection;
using System.Collections.Generic;

namespace UEngine.Data
{
    public abstract class UDataLoader
    {
        protected static readonly bool mIsPreloadData = true;
        protected readonly string mResourcePath;
        protected readonly string mFileExtention;
        protected readonly bool mIsUseOutterConfig;
        protected Action<int, int> mProgress;
        protected Action mFinished;

        protected UDataLoader()
        {
            mIsUseOutterConfig = USystemConfig.IsUseOutterConfig;
            if (mIsUseOutterConfig)
            {
                mResourcePath = String.Concat(USystemConfig.OutterPath, USystemConfig.kCfgSubFolder);
                mFileExtention = USystemConfig.kCfgExt;
            } else
            {
                mResourcePath = USystemConfig.kCfgSubFolder;
                mFileExtention = USystemConfig.kCfgFileExtension;
            }
        }
    }

    public class GameDataControler : UDataLoader
    {
        private List< Type > mDefaultData = new List< Type >() { typeof(USceneData), };

        private static GameDataControler mInstance;
        public static GameDataControler Instance
        {
            get { return mInstance; }
        }

        static GameDataControler()
        {
            mInstance = new GameDataControler();
        }

        public static void Init(Action< int, int > progress = null, Action finished = null)
        {
            mInstance.LoadData(mInstance.mDefaultData, mInstance.FormatXMLData, null);
            if (mIsPreloadData)
            {
                Action action = () => { mInstance.InitAsynData(mInstance.FormatXMLData, progress, finished); };

                if (!USystemConfig.Instance.IsDevelopMode)
                    action.BeginInvoke(null, null);
                else
                    action();
            } else
            {
                finished();
            }
        }

        private void InitAsynData(Func< string, Type, Type, object > formatData, Action< int, int > progress, Action finished)
        {
            try
            {
                List< Type > gameDataType = new List< Type >();

                var types = typeof(GameDataControler).Assembly.GetTypes();
                for(int i = 0; i < types.Length; ++i)
                {
                    var item = types[i];
                    if (item.Namespace == "UEngine.Data")
                    {
                        var type = item.BaseType;
                        while (type != null)
                        {
                            if (type == typeof(UGameData) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(UGameData<>)))
                            {
                                if (!mDefaultData.Contains(item))
                                    gameDataType.Add(item);

                                break;
                            } else
                            {
                                type = type.BaseType;
                            }
                        }
                    }
                }
                LoadData(gameDataType, formatData, progress);

                GC.Collect();

                if (finished != null)
                    finished();
            } catch (Exception ex)
            {
                ULogger.Error("InitData Error: " + ex.Message);
            }
        }

        private void LoadData(List< Type > gameDataType, Func< string, Type, Type, object > formatData, Action< int, int > progress)
        {
            var count = gameDataType.Count;

            var i = 1;
            for (int j = 0; j < gameDataType.Count; ++j )
            {
                var item = gameDataType[j];
                var p = item.GetProperty("mDataMap", ~BindingFlags.DeclaredOnly);
                var f = item.GetField("kFileName");
                if (p != null && f != null)
                {
                    var fileName = f.GetValue(null) as string;
                    var result = formatData(String.Concat(mResourcePath, fileName, mFileExtention), p.PropertyType, item);
                    p.GetSetMethod().Invoke(null, new object[] { result });
                }

                if (progress != null)
                    progress(i, count);

                i++;
            }
        }

        public object FormatData(string fileName, Type dicType, Type type)
        {
            return FormatXMLData(String.Concat(mResourcePath, fileName, mFileExtention), dicType, type);
        }

        private object FormatXMLData(string fileName, Type dicType, Type type)
        {
            object result = null;
            try
            {
                result = dicType.GetConstructor(Type.EmptyTypes).Invoke(null);

                Dictionary< int, Dictionary< string, string > > map;
                if (UXMLParser.LoadIntMap(fileName, mIsUseOutterConfig, out map))
                {
                    var props = type.GetProperties();
                    Dictionary<int, Dictionary<string, string>>.Enumerator it = map.GetEnumerator();
                    while(it.MoveNext())
                    {
                        var item = it.Current;
                        var t = type.GetConstructor(Type.EmptyTypes).Invoke(null);
                        for (int i = 0; i < props.Length; ++i )
                        {
                            var prop = props[i];
                            if (prop.Name == "ID")
                            {
                                prop.SetValue(t, item.Key, null);
                            }
                            else
                            {
                                if (item.Value.ContainsKey(prop.Name))
                                {
                                    var value = UDataParser.GetValue(item.Value[prop.Name], prop.PropertyType);

                                    prop.SetValue(t, value, null);
                                }
                            }
                        }
                        dicType.GetMethod("Add").Invoke(result, new object[] { item.Key, t });
                    }
                }
            } catch (Exception ex)
            {
                ULogger.Error("FormatData Error: " + fileName + "  " + ex.Message);
            }

            return result;
        }
    }

    public abstract class UGameData
    {
        public int ID { get; protected set; }

        protected static Dictionary< int, T > GetDataMap< T >()
        {
            Dictionary< int, T > dataMap;

            var type = typeof(T);

            var fileNameField = type.GetField("kFileName");
            if (fileNameField != null)
            {
                var fileName = fileNameField.GetValue(null) as string;

                var result = GameDataControler.Instance.FormatData(fileName, typeof(Dictionary< int, T >), type);

                dataMap = result as Dictionary< int, T >;
            } else
            {
                dataMap = new Dictionary< int, T >();
            }

            return dataMap;
        }
    }

    public abstract class UGameData< T > : UGameData where T : UGameData< T >
    {
        private static Dictionary< int, T > mDataMap;

        public static Dictionary< int, T > DataMap
        {
            get
            {
                if (mDataMap == null)
                    mDataMap = GetDataMap< T >();

                return mDataMap;
            }
            set { mDataMap = value; }
        }
    }
}
