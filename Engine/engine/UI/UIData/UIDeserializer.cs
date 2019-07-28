using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UEngine.Data;
using UEngine.UIExpand;
using UEngine.UI.UILuaBehaviour;

using Newtonsoft.Json;

namespace UEngine.UI
{
    public class UIDeserializer
    {
		public delegate object GetValueCB();

        
        private static bool IsCache = false;

        private static PanelManager manager;

        private static List< GuidType > WaitList = new List< GuidType >();

        private static List< Image > WaitImageList = new List< Image >(128);

		public static PanelManagerPort Deserializer(string path, byte[] bytes, bool active, Transform Parent, bool CacheGameObject = false, int edition = 0)
		{
			WaitList.Clear();
			WaitImageList.Clear();

			IsCache = USystemConfig.Instance.IsCaChe;

			float startTime = Time.realtimeSinceStartup;

			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer", path);

			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer manager", path);

			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer manager new and add component", path);

			PanelManagerPort managerport = new PanelManagerPort();

			GameObject root = new GameObject();

			int ReadPos = 4;

			//Root初始化
			manager = root.AddComponent< PanelManager >();

			if (edition >= 1)
			{
				manager.panelMode = (PanelExpandMode)DeserializerInt(bytes, ref ReadPos);
				manager.bottomValue = DeserializerFloat(bytes, ref ReadPos);
			}

			managerport.panelmanager = manager;
			manager.AnimationPath = path.Replace(".byte", "") + "_Animation.ani";
			manager.GameObjectList.Add(root);

			UProfile.EndSample();

			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer manager root node", path);

			int GameObjectCount = DeserializerInt(bytes, ref ReadPos);

			DeserializerGameObject(bytes, ref ReadPos, root);

			UProfile.EndSample();

			UProfile.EndSample();

			//实例化所有对象
			UProfile.BeginLoopSample("LoadUIData {0} PanelManagerPort.Deserializer Instantiate gameObject {1}", path, GameObjectCount);

			for (int i = 1; i < GameObjectCount; i ++)
			{
				UProfile.NewLoopSample(i);

				UProfile.BeginSample("new gameObject");

				GameObject go = new GameObject();
				manager.GameObjectList.Add(go);

				int index = DeserializerInt(bytes, ref ReadPos);

				go.transform.SetParent(manager.GameObjectList[index].transform, false);

				UProfile.EndSample();

				UProfile.BeginSample("deserializer gameObject");

				DeserializerGameObject(bytes, ref ReadPos, go);

				UProfile.EndSample();
			}

			UProfile.EndLoopSample();

			if (USystemConfig.Instance.UILoadResIsAsyn)
			{
				UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer UILoadResIsAsyn", path);

				for (int i = 0; i < WaitImageList.Count; i++)
				{
					Image image = WaitImageList[i];

					if (image.GetComponent< Mask >() == null)
					{
						if (alphaTex == null)
						{
							alphaTex = Texture2D.CreateExternalTexture(1, 1, TextureFormat.Alpha8, false, false, IntPtr.Zero);
							alphaTex.SetPixel(0, 0, new Color(0, 0, 0, 0));
							alphaTex.Apply();
						}

						if (alphaSpr == null)
						{
							alphaSpr = Sprite.Create(alphaTex, new Rect(0, 0, alphaTex.width, alphaTex.height), new Vector2(0.5f, 0.5f));
						}

						image.sprite = alphaSpr;
					}
				}
				WaitImageList.Clear();

				UProfile.EndSample();
			}

			//获取对象引用
			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer set value", path);

			for (int i = 0; i < WaitList.Count; i ++)
			{
				object obj = WaitList[i].obj;
				var gameobj = manager.GameObjectList[WaitList[i].index];
				var memberinfo = WaitList[i].memberinfo;
				var type = WaitList[i].type;

				object value = null;
				if (typeof(UnityEngine.Component).IsAssignableFrom(type) && gameobj)
					value = gameobj.GetComponent(type);
				else if (typeof(UnityEngine.GameObject).IsAssignableFrom(type) && gameobj)
					value = gameobj;

				if (memberinfo is FieldInfo)
					((FieldInfo)memberinfo).SetValue(obj, value);
				else
					((PropertyInfo)memberinfo).SetValue(obj, value, null);
			}

			UProfile.EndSample();

			//root.AddComponent<Canvas>();
			//root.AddComponent<GraphicRaycaster>();
			//控制整个面板的显示和父子物体关系

			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer manager init", path);

			manager.Init();

			UProfile.EndSample();

			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer set parent", path);

			if (IsCache)
			{
				root.transform.SetParent(UIManager.UICachePool.transform, false);
				root.SetActive(false);
			} else
			{
				root.transform.SetParent(Parent, false);
				root.SetActive(active);
			}

			UProfile.EndSample();

			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer add cache", path);

			byte[] tempRoot = UFileAccessor.ReadBinaryFile(path);
			if (IsCache)
			{
				//UIManager.UICacheObj.Add( path, root );
				if (tempRoot.Length >= USystemConfig.Instance.CutPoint)
					AddObj(UIManager.UIMaxObj, USystemConfig.Instance.LargeLimitCount, root, path);
				else
					AddObj(UIManager.UIMinObj, USystemConfig.Instance.LittleLimitCount, root, path);
			}

			UProfile.EndSample();

			//加载Guid引用关系
			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer update guid", path);

			string guidpath = path.Replace(".byte", "") + "_Guid.guid";

			string jsondic = UFileAccessor.ReadStringFile(guidpath);
			if (string.IsNullOrEmpty(jsondic))
			{
				//结束
				//ULogger.Info( "面板" + path + "加载成功...  Time : " + ( Time.realtimeSinceStartup - startTime ) * 1000f );

				if (IsCache)
					managerport = UILoader.LoadUIData(path, active, Parent, true);

				UProfile.EndSample();

				UProfile.EndSample();

				return managerport;
			}

			manager.GuidData = JsonConvert.DeserializeObject< List< UIGuidData > >(jsondic);

			UProfile.EndSample();

			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer init beahavir", path);

			//if (!IsCache)
			{
				for (int i = 0; i < manager.GuidData.Count; i ++)
				{
					UIGuidData guidData = manager.GuidData[i];

					GameObject go = manager.GameObjectList[guidData.GameObjectIndex];

					if (Application.isEditor)
					{
						manager.luaguidlist.Add(guidData.UIGuid);

						manager.ObjList.Add(go);

						UiWidgetGuid luaGuid = go.AddComponent< UiWidgetGuid >();
						luaGuid.UIGuid = guidData.UIGuid;
						luaGuid.uitype = (UIType)guidData.UIType;
					}

					LuaBeahavirInit(guidData, go);
				}
			}

			UProfile.EndSample();

			//ULogger.Info( "面板:" + path + "加载成功...  Time : " + ( Time.realtimeSinceStartup - startTime ) * 1000f );

			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer load ui data", path);

			if (IsCache)
				managerport = UILoader.LoadUIData(path, active, Parent, true);

			UProfile.EndSample();

			UProfile.EndSample();

			return managerport;
		}

        public static PanelManagerPort Deserializer(string path, UByteStream stream, bool active, Transform Parent, bool CacheGameObject = false, int edition = 0) 
        {
            WaitList.Clear();
            WaitImageList.Clear();

            IsCache = USystemConfig.Instance.IsCaChe;

            float startTime = Time.realtimeSinceStartup;

			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer", path);

			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer manager", path);

			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer manager new and add component", path);

            PanelManagerPort managerport = new PanelManagerPort();

            GameObject root = new GameObject();

            //Root初始化
            manager = root.AddComponent< PanelManager >();

            if (edition >= 1)
            {
				var panelMode = stream.GetI32();

				manager.panelMode = (PanelExpandMode)panelMode;
                manager.bottomValue = stream.GetF32();
            }

            managerport.panelmanager = manager;
            manager.AnimationPath = path.Replace( ".byte", "" ) + "_Animation.ani";
            manager.GameObjectList.Add( root );

			UProfile.EndSample();

			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer manager root node", path);

            int GameObjectCount = stream.GetI32();

            DeserializerGameObject(stream, root);
            
			UProfile.EndSample();

			UProfile.EndSample();

            //实例化所有对象
			UProfile.BeginLoopSample("LoadUIData {0} PanelManagerPort.Deserializer Instantiate gameObject {1}", path, GameObjectCount);

            for (int i = 1 ; i < GameObjectCount ; i ++)
            {
				UProfile.NewLoopSample(i);

				UProfile.BeginSample("new gameObject");

                GameObject go = new GameObject();
                manager.GameObjectList.Add(go);

				int index = stream.GetI32();

                go.transform.SetParent(manager.GameObjectList[index].transform, false);

				UProfile.EndSample();

				UProfile.BeginSample("deserializer gameObject");

                DeserializerGameObject(stream, go);

				UProfile.EndSample();
            }

			UProfile.EndLoopSample();

            if (USystemConfig.Instance.UILoadResIsAsyn)
            {
				UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer UILoadResIsAsyn", path);

                for (int i = 0 ; i < WaitImageList.Count ; i ++)
                {
                    Image image = WaitImageList[i];

                    if (image.GetComponent< Mask >() == null)
                    {
                        if (alphaTex == null)
                        {
                            alphaTex = Texture2D.CreateExternalTexture(1, 1, TextureFormat.Alpha8, false, false, IntPtr.Zero);
                            alphaTex.SetPixel(0, 0, new Color(0, 0, 0, 0));
                            alphaTex.Apply();
                        }

                        if (alphaSpr == null)
                        {
                            alphaSpr = Sprite.Create(alphaTex, new Rect(0, 0, alphaTex.width, alphaTex.height), new Vector2(0.5f, 0.5f));
                        }

                        image.sprite = alphaSpr;
                    }
                }
                WaitImageList.Clear();

				UProfile.EndSample();
            }

            //获取对象引用
			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer set value", path);

            for (int i = 0 ; i < WaitList.Count ; i ++)
            {
                object obj = WaitList[i].obj;
                var gameobj = manager.GameObjectList[WaitList[i].index];
                var memberinfo = WaitList[i].memberinfo;
                var type = WaitList[i].type;

                object value = null;
                if (typeof(UnityEngine.Component).IsAssignableFrom(type) && gameobj)
                    value = gameobj.GetComponent(type);
                else if (typeof(UnityEngine.GameObject).IsAssignableFrom(type) && gameobj)
                    value = gameobj;

                if (memberinfo is FieldInfo)
                    ((FieldInfo)memberinfo).SetValue(obj, value);
                else
                    ((PropertyInfo)memberinfo).SetValue(obj, value, null);
            }

			UProfile.EndSample();

            //root.AddComponent<Canvas>();
            //root.AddComponent<GraphicRaycaster>();
            //控制整个面板的显示和父子物体关系

			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer manager init", path);

            manager.Init();

			UProfile.EndSample();

			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer set parent", path);

            if (IsCache)
            {
                root.transform.SetParent(UIManager.UICachePool.transform, false);
                root.SetActive(false);
            } else 
            {
                root.transform.SetParent(Parent, false);
                root.SetActive(active);
            }

			UProfile.EndSample();

			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer add cache", path);

            byte[] tempRoot = UFileAccessor.ReadBinaryFile(path);
            if (IsCache)
            {     
                //UIManager.UICacheObj.Add( path, root );
                if (tempRoot.Length >= USystemConfig.Instance.CutPoint)
                    AddObj(UIManager.UIMaxObj, USystemConfig.Instance.LargeLimitCount, root, path);
                else
                    AddObj(UIManager.UIMinObj, USystemConfig.Instance.LittleLimitCount, root, path);
            }

			UProfile.EndSample();

            //加载Guid引用关系
			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer update guid", path);

            string guidpath = path.Replace(".byte", "") + "_Guid.guid";

            string jsondic = UFileAccessor.ReadStringFile(guidpath);
            if (string.IsNullOrEmpty(jsondic))
            {
                //结束
                //ULogger.Info( "面板" + path + "加载成功...  Time : " + ( Time.realtimeSinceStartup - startTime ) * 1000f );

                if (IsCache)
                    managerport = UILoader.LoadUIData(path, active, Parent, true);

				UProfile.EndSample();

				UProfile.EndSample();

                return managerport;
            }

            manager.GuidData = JsonConvert.DeserializeObject< List< UIGuidData > >(jsondic);

			UProfile.EndSample();
            
			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer init beahavir", path);

            //if (!IsCache)
            {
                for (int i = 0 ; i < manager.GuidData.Count ; i ++)
                {
                    UIGuidData guidData = manager.GuidData[i];

                    GameObject go = manager.GameObjectList[guidData.GameObjectIndex];

                    if (Application.isEditor)
                    {
                        manager.luaguidlist.Add(guidData.UIGuid);

                        manager.ObjList.Add(go);

                        UiWidgetGuid luaGuid = go.AddComponent< UiWidgetGuid >();
                        luaGuid.UIGuid = guidData.UIGuid;
                        luaGuid.uitype = (UIType)guidData.UIType;
                    }
                
                    LuaBeahavirInit(guidData, go);
                }
            }

			UProfile.EndSample();

            //ULogger.Info( "面板:" + path + "加载成功...  Time : " + ( Time.realtimeSinceStartup - startTime ) * 1000f );

			UProfile.BeginSample("LoadUIData {0} PanelManagerPort.Deserializer load ui data", path);

            if (IsCache)
				managerport = UILoader.LoadUIData(path, active, Parent, true);

			UProfile.EndSample();

			UProfile.EndSample();

            return managerport;
        }

        public static void AddObj(Dictionary< string, Cache > dic, int limit, GameObject root, string path)
        {
            if (dic.Count > limit)
            {
                string index = "";
                long time = 0;

                Dictionary< string, Cache >.Enumerator it = dic.GetEnumerator();
                while (it.MoveNext())
                {
                    var item = it.Current;
                    if (index == "")
                    {
                        index = item.Key;
                        time = item.Value.time;
                    } else
                    {
                        if (time > item.Value.time)
                        {
                            index = item.Key;
                            time = item.Value.time;
                        }
                    }
                }

                GameObject.DestroyImmediate(dic[index].go);

                dic.Remove(index);               
            }

            Cache cache = new Cache();
            cache.go = root;
            dic.Add(path, cache);
        }

        #region 反序列化基础字段
        public static bool DeserializerBool(byte[] bytes, ref int pos, int count = 1) 
        {
            var value = BitConverter.ToBoolean(bytes, pos);
            pos += count;

            return value;
        }

        public static byte DeserializerByte(byte[] bytes, ref int pos, int count = 1) 
        {
            var value = bytes[pos];
            pos += count;

            return value;
        }

        public static int DeserializerInt(byte[] bytes, ref int pos, int count = 2) 
        {
            var value = BitConverter.ToInt16( bytes, pos );
            pos += count;

            return value;
        }

        public static float DeserializerFloat(byte[] bytes, ref int pos, int count = 4) 
        {
            var value = BitConverter.ToSingle(bytes, pos);
            pos += count;

            return value;
        }

        public static string DeserializerString(byte[] bytes, ref int pos) 
        {
            int StringLength = DeserializerInt(bytes, ref pos);
            if (StringLength == 0)
                return string.Empty;

            try
            {
                if (bytes.Length - pos < StringLength)
                    return string.Empty;

                var value = System.Text.UnicodeEncoding.UTF8.GetString(bytes, pos, StringLength);
                pos += StringLength;

                return value;
            } catch (Exception e)
            {
                Debug.Log( e.Message );

                throw;
            }
        }
        #endregion

        //GameObject
		public static void DeserializerGameObject(byte[] bytes, ref int pos, GameObject go)
		{
			UProfile.BeginSample("deserializer gameObject root");

			go.name = DeserializerString(bytes, ref pos);
			go.SetActive(DeserializerBool(bytes, ref pos));
			go.layer = LayerMask.NameToLayer("UI");

			UProfile.EndSample();

			UProfile.BeginLoopSample("deserializer gameObject component");

			int ComponentCount = DeserializerInt(bytes, ref pos);
			for (int i = 0; i < ComponentCount; i ++)
			{
				UProfile.NewLoopSample(i);

				UProfile.BeginSample("deserializer gameObject get type");

				string ComType = DeserializerString(bytes, ref pos);

				Type type = UTypeCache.GetType(ComType);

				UProfile.EndSample();

				Component component;

				UProfile.BeginSample("deserializer gameObject add or get component");
				if (ComType == "UnityEngine.ParticleSystemRenderer")
				{
					component = go.GetComponent(type);
					manager.Particles.Add((ParticleSystemRenderer)component);
				} else
				{
					component = go.AddComponent(type);
					if (ComType == "UnityEngine.Canvas")
					{
						manager.SubCanvaes.Add((Canvas)component);
					}
				}
				UProfile.EndSample();

				UProfile.BeginSample("deserializer gameObject RectTransform component");
				if (ComType == "UnityEngine.RectTransform")
				{
					component = go.GetComponent(type);

					if (component == null)
					{
						component = go.AddComponent(type);
					}
				}
				UProfile.EndSample();

				if (component is ParticleSystem)
					((ParticleSystem)component).Stop();

				UProfile.BeginSample("deserializer gameObject DeserializerClass");

				DeserializerClass(bytes, ref pos, component, type);

				UProfile.EndSample();

				if (component is ParticleSystem)
					((ParticleSystem)component).Play();

				UProfile.BeginSample("deserializer gameObject init CText");
				if (component is CText)
				{
					CText ctext = (CText)component;
					ctext.Init();
				}
				UProfile.EndSample();

				UProfile.BeginSample("deserializer gameObject init Text");
				if (component is Text)
				{
					Text text = (Text)component;
					text.text = ULanguage.Get(text.text);
				}
				UProfile.EndSample();

				UProfile.BeginSample("deserializer gameObject init DropDown");
				if (component is Dropdown)
				{
					Dropdown dropDown = (Dropdown)component;
					for (int j = 0; j < dropDown.options.Count; j++)
					{
						string translate = dropDown.options[j].text;

						dropDown.options[j].text = ULanguage.Get(translate);
					}
				}
				UProfile.EndSample();
			}

			UProfile.EndLoopSample();
		}

        public static void DeserializerGameObject(UByteStream stream, GameObject go)
        {
			UProfile.BeginSample("deserializer gameObject root");

            go.name = stream.GetString();
            go.SetActive(stream.GetBool());
            go.layer = LayerMask.NameToLayer("UI");

			UProfile.EndSample();

			UProfile.BeginLoopSample("deserializer gameObject component");

			int ComponentCount = stream.GetI32();
            for (int i = 0 ; i < ComponentCount ; i ++)
            {
				UProfile.NewLoopSample(i);

				UProfile.BeginSample("deserializer gameObject get type");

                string ComType = stream.GetString();

                Type type = UTypeCache.GetType(ComType);

				UProfile.EndSample();

                Component component;

				UProfile.BeginSample("deserializer gameObject add or get component");
                if (ComType == "UnityEngine.ParticleSystemRenderer")
                {
                    component = go.GetComponent(type);
                    manager.Particles.Add((ParticleSystemRenderer)component);
                } else
                {
                    component = go.AddComponent(type);
                    if (ComType == "UnityEngine.Canvas")
                    {
                        manager.SubCanvaes.Add((Canvas)component);
                    }
                }
				UProfile.EndSample();

				UProfile.BeginSample("deserializer gameObject RectTransform component");
                if (ComType == "UnityEngine.RectTransform")
                {
                    component = go.GetComponent(type);

                    if (component == null)
                    {
                        component = go.AddComponent(type);
                    }
                }
				UProfile.EndSample();

                if (component is ParticleSystem)
                    ((ParticleSystem)component).Stop();

				UProfile.BeginSample("deserializer gameObject DeserializerClass");

                DeserializerClass(stream, component, type);

				UProfile.EndSample();

                if (component is ParticleSystem)
                    ((ParticleSystem)component).Play();

                if (component is Image && ( ( Image )component ).color == Color.white)
                    WaitImageList.Add( ( Image )component );

				UProfile.BeginSample("deserializer gameObject init CText");
                if (component is CText)
                {
                    CText ctext = (CText)component;
                    ctext.Init();
                }
				UProfile.EndSample();

				UProfile.BeginSample("deserializer gameObject init Text");
                if (component is Text)
                {
                    Text text = (Text)component;
                    text.text = ULanguage.Get(text.text);
                }
				UProfile.EndSample();

				UProfile.BeginSample("deserializer gameObject init DropDown");
                if (component is Dropdown)
                {
                    Dropdown dropDown = (Dropdown)component;
                    for (int j = 0; j < dropDown.options.Count; j ++)
                    {
                        string translate = dropDown.options[j].text;

                        dropDown.options[j].text = ULanguage.Get(translate);
                    }
                }
				UProfile.EndSample();
            }

			UProfile.EndLoopSample();
        }

		enum EMemberType
		{
			eMT_Field,
			eMT_Property,
			eMT_Other
		}

		struct SMemberInfo
		{
			public EMemberType mType;

			public MemberInfo mInfo;
		}
		static Dictionary< Type, Dictionary< string, SMemberInfo > > msMemberInfos = new Dictionary< Type, Dictionary< string, SMemberInfo > >();

        //Class
		public static void DeserializerClass(byte[] bytes, ref int pos, object obj, Type script)
		{
			UProfile.BeginCallSample("DeserializerClass");

			int MemberCount = DeserializerInt(bytes, ref pos);
			for (int i = 0; i < MemberCount; i ++)
			{
				UProfile.BeginCallSample("DeserializerClass_SetMemberInfo");

				string typeName = DeserializerString(bytes, ref pos);

				Type type = null;

				object value = null;

				if (!msMemberInfos.ContainsKey(script))
					msMemberInfos.Add(script, new Dictionary<string, SMemberInfo>());

				var memberInfos = msMemberInfos[script];

				SMemberInfo info;
				if (memberInfos.TryGetValue(typeName, out info))
				{
					if (info.mType == EMemberType.eMT_Field)
					{
						var fieldInfo = info.mInfo as FieldInfo;

						type = fieldInfo.FieldType;

						value = DeserializerObject(bytes, ref pos, fieldInfo.FieldType);
					} else if (info.mType == EMemberType.eMT_Property)
					{
						var propertyInfo = info.mInfo as PropertyInfo;

						type = propertyInfo.PropertyType;
						if (script == typeof(ParticleSystem))
							value = DeserializerObject(bytes, ref pos, type, (ParticleSystem)obj, propertyInfo);
						else
							value = DeserializerObject(bytes, ref pos, type);
					} else
					{
						if (script == typeof(ParticleSystem) && typeName == "bursts")
						{
							int fieldType = DeserializerInt(bytes, ref pos);

							int Length = DeserializerInt(bytes, ref pos);

							ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[Length];
							for (int j = 0; j < Length; j ++)
							{
								bursts[j] = (ParticleSystem.Burst)DeserializerObject(bytes, ref pos, typeof(ParticleSystem.Burst));
							}
							((ParticleSystem)obj).emission.SetBursts(bursts);
						}
					}
				} else
				{
					info = new SMemberInfo();

					info.mInfo = script.GetField(typeName);

					FieldInfo fieldInfo = info.mInfo as FieldInfo;
					if (fieldInfo != null)
					{
						info.mType = EMemberType.eMT_Field;

						type = fieldInfo.FieldType;
						value = DeserializerObject(bytes, ref pos, fieldInfo.FieldType);
					} else
					{
						info.mInfo = script.GetProperty(typeName);

						PropertyInfo propertyInfo = info.mInfo as PropertyInfo;
						if (propertyInfo != null)
						{
							info.mType = EMemberType.eMT_Property;

							type = propertyInfo.PropertyType;
							if (script == typeof(ParticleSystem))
								value = DeserializerObject(bytes, ref pos, type, (ParticleSystem)obj, propertyInfo);
							else
								value = DeserializerObject(bytes, ref pos, type);
						} else
						{
							info.mType = EMemberType.eMT_Other;

							if (script == typeof(ParticleSystem) && typeName == "bursts")
							{
								int fieldType = DeserializerInt(bytes, ref pos);

								int Length = DeserializerInt(bytes, ref pos);

								ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[Length];
								for (int j = 0; j < Length; j ++)
								{
									bursts[j] = (ParticleSystem.Burst)DeserializerObject(bytes, ref pos, typeof(ParticleSystem.Burst));
								}
								((ParticleSystem)obj).emission.SetBursts(bursts);
							}
						}
					}

					memberInfos.Add(typeName, info);
				}

				UProfile.EndCallSample();

				UProfile.BeginCallSample("DeserializerClass_ChangeValue");

				if (type != null)
					ChangeValue(obj, type, value, info.mInfo);

				UProfile.EndCallSample();
			}

			UProfile.EndCallSample();

			//if (obj is ParticleSystem)
			//{
			//    var emiss = ( ( ParticleSystem )obj ).emission;
			//    if (classconfig.MemberDic.ContainsKey( "bursts" ))
			//    {
			//        object dicvalue = classconfig.MemberDic["bursts"];
			//        object length = dicvalue.GetType().GetMethod( "GetLength" ).Invoke( dicvalue, new object[] { 0 } );
			//        if (( int )length <= 0)
			//        {
			//            return;
			//        }
			//        MethodInfo _mi = dicvalue.GetType().GetMethod( "GetValue", new[] { typeof( int ) } );
			//        object _arrayvalue = _mi.Invoke( dicvalue, new object[] { 0 } );
			//        Type stype = _arrayvalue.GetType();
			//        Type ftype = typeof( ParticleSystem.Burst[] );
			//        object value = null;
			//        Type _type = null;
			//        _type = ftype.GetElementType();
			//        value = Array.CreateInstance( _type, ( int )length );

			//        for (int j = 0 ; j < ( int )length ; j++)
			//        {
			//            MethodInfo mi = dicvalue.GetType().GetMethod( "GetValue", new[] { typeof( int ) } );
			//            object arrayvalue = mi.Invoke( dicvalue, new object[] { j } );
			//            if (ftype.IsArray)
			//            {
			//                var valueindex = Activator.CreateInstance( _type );
			//                SetComValue( valueindex, arrayvalue as UIClassConfig, _type );
			//                ( value as Array ).SetValue( valueindex, j );
			//            }
			//        }
			//        emiss.SetBursts( value as ParticleSystem.Burst[] );
			//    }
			//}
		}

        public static void DeserializerClass(UByteStream stream, object obj, Type script) 
        {
			UProfile.BeginCallSample("DeserializerClass");

            int MemberCount = stream.GetI32();
            for (int i = 0 ; i < MemberCount ; i ++)
            {
				UProfile.BeginCallSample("DeserializerClass_SetMemberInfo");

                string typeName = stream.GetString();

                Type type = null;

                object value = null;

				if (!msMemberInfos.ContainsKey(script))
					msMemberInfos.Add(script, new Dictionary< string, SMemberInfo >());

				var memberInfos = msMemberInfos[script];

				SMemberInfo info;
				if (memberInfos.TryGetValue(typeName, out info))
				{
					if (info.mType == EMemberType.eMT_Field)
					{
						FieldInfo fieldInfo = info.mInfo as FieldInfo;

						type = fieldInfo.FieldType;

						value = DeserializerObject(stream, type, () =>
						{
							return fieldInfo.GetValue(obj);
						});
					} else if (info.mType == EMemberType.eMT_Property)
					{
						var propertyInfo = info.mInfo as PropertyInfo;

						type = propertyInfo.PropertyType;

						value = DeserializerObject(stream, type, () =>
						{
							return propertyInfo.GetValue(obj, null);
						});
					} else
					{
						if (script == typeof(ParticleSystem) && typeName == "bursts")
						{
							int fieldType = stream.GetB8();

							int Length = stream.GetI32();
							ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[Length];
							for (int j = 0; j < Length; j ++)
							{
								bursts[j] = (ParticleSystem.Burst)DeserializerObject(stream, typeof(ParticleSystem.Burst), null);
							}
							((ParticleSystem)obj).emission.SetBursts(bursts);
						}
					}
				} else
				{
					info = new SMemberInfo();

					info.mInfo = script.GetField(typeName);

					FieldInfo fieldInfo = info.mInfo as FieldInfo;
					if (fieldInfo != null)
					{
						info.mType = EMemberType.eMT_Field;

						type	= fieldInfo.FieldType;
						value	= DeserializerObject(stream, fieldInfo.FieldType, () =>
						{
							return fieldInfo.GetValue(obj);
						});
					} else
					{
						info.mInfo = script.GetProperty(typeName);

						PropertyInfo propertyInfo = info.mInfo as PropertyInfo;
						if (propertyInfo != null)
						{
							info.mType = EMemberType.eMT_Property;

							type = propertyInfo.PropertyType;

							value = DeserializerObject(stream, type, () =>
							{
								return propertyInfo.GetValue(obj, null);
							});
						} else
						{
							info.mType = EMemberType.eMT_Other; 

							if (script == typeof(ParticleSystem) && typeName == "bursts")
							{
								int fieldType = stream.GetB8();

								int Length = stream.GetI32();

								ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[Length];
								for (int j = 0; j < Length; j ++)
								{
									bursts[j] = (ParticleSystem.Burst)DeserializerObject(stream, typeof(ParticleSystem.Burst), null);
								}
								((ParticleSystem)obj).emission.SetBursts(bursts);
							}
						}
					}

					memberInfos.Add(typeName, info);
				}

				UProfile.EndCallSample();

				UProfile.BeginCallSample("DeserializerClass_ChangeValue");

                if (type != null)
					ChangeValue(obj, type, value, info.mInfo);

				UProfile.EndCallSample();
            }

			UProfile.EndCallSample();

            //if (obj is ParticleSystem)
            //{
            //    var emiss = ( ( ParticleSystem )obj ).emission;
            //    if (classconfig.MemberDic.ContainsKey( "bursts" ))
            //    {
            //        object dicvalue = classconfig.MemberDic["bursts"];
            //        object length = dicvalue.GetType().GetMethod( "GetLength" ).Invoke( dicvalue, new object[] { 0 } );
            //        if (( int )length <= 0)
            //        {
            //            return;
            //        }
            //        MethodInfo _mi = dicvalue.GetType().GetMethod( "GetValue", new[] { typeof( int ) } );
            //        object _arrayvalue = _mi.Invoke( dicvalue, new object[] { 0 } );
            //        Type stype = _arrayvalue.GetType();
            //        Type ftype = typeof( ParticleSystem.Burst[] );
            //        object value = null;
            //        Type _type = null;
            //        _type = ftype.GetElementType();
            //        value = Array.CreateInstance( _type, ( int )length );

            //        for (int j = 0 ; j < ( int )length ; j++)
            //        {
            //            MethodInfo mi = dicvalue.GetType().GetMethod( "GetValue", new[] { typeof( int ) } );
            //            object arrayvalue = mi.Invoke( dicvalue, new object[] { j } );
            //            if (ftype.IsArray)
            //            {
            //                var valueindex = Activator.CreateInstance( _type );
            //                SetComValue( valueindex, arrayvalue as UIClassConfig, _type );
            //                ( value as Array ).SetValue( valueindex, j );
            //            }
            //        }
            //        emiss.SetBursts( value as ParticleSystem.Burst[] );
            //    }
            //}

        }

        //Object
		public static object DeserializerObject(byte[] bytes, ref int pos, Type ftype, ParticleSystem particle = null, PropertyInfo propertyInfo = null)
		{
			int fieldType = DeserializerInt(bytes, ref pos);
			if (fieldType == 0)			// int
			{
				return DeserializerInt(bytes, ref pos);
			} else if (fieldType == 1)	// float
			{
				return DeserializerFloat(bytes, ref pos);
			} else if (fieldType == 2)	// string
			{
				return DeserializerString(bytes, ref pos);
			} else if (fieldType == 3)	// byte
			{
				return DeserializerByte(bytes, ref pos);
			} else if (fieldType == 4)	// UIClass
			{
				object value;

				if (particle)
					value = propertyInfo.GetValue(particle, null);
				else
					value = Activator.CreateInstance(ftype);

				DeserializerClass(bytes, ref pos, value, ftype);

				return value;
			} else if (fieldType == 5)
			{
				int Length = DeserializerInt(bytes, ref pos);

				object value = null;
				Type _type = null;

				if (ftype.IsArray)
				{
					_type = ftype.GetElementType();
					value = Array.CreateInstance(_type, Length);
					for (int i = 0; i < Length; i ++)
					{
						(value as Array).SetValue(DeserializerObject(bytes, ref pos, _type), i);
					}
				} else if (ftype.IsGenericType && ftype.GetGenericTypeDefinition() == typeof(List<>))
				{
					_type = GetListType(ftype);

					value = ftype.GetConstructor(Type.EmptyTypes).Invoke(null);
					for (int i = 0; i < Length; i ++)
					{
						value.GetType().GetMethod("Add").Invoke(value, new object[] { DeserializerObject(bytes, ref pos, _type) });
					}
				}

				return value;
			} else if (fieldType == 6)
			{
				return DeserializerBool( bytes, ref pos );
			}

			return null;
		}

        public static object DeserializerObject(UByteStream stream, Type ftype, GetValueCB getValue) 
        {
            int fieldType = stream.GetB8();
            if (fieldType == 0)			// int
            {
				return stream.GetI32();
            } else if (fieldType == 1)	// float
            {
				return stream.GetF32();
            } else if (fieldType == 2)	// string
            {
				return stream.GetString();
            } else if (fieldType == 3)	// byte
            {
				return stream.GetB8();
            } else if (fieldType == 4)	// UIClass
            {
                object value = null;

                if (null != getValue)
                    value = getValue();

				if (null == value)
                    value = Activator.CreateInstance(ftype);

                DeserializerClass(stream, value, ftype);

                return value;
            } else if (fieldType == 5)
            {
				int Length = stream.GetI32();

                object value = null;
                Type _type = null;
                
                if (ftype.IsArray)
                {
                    _type = ftype.GetElementType();
                    value = Array.CreateInstance(_type, Length);
                    for (int i = 0 ; i < Length ; i ++)
                    {
                        (value as Array).SetValue(DeserializerObject(stream, _type, null), i);
                    }
                } else if (ftype.IsGenericType && ftype.GetGenericTypeDefinition() == typeof(List<>))
                {
                    _type = GetListType(ftype);

                    value = ftype.GetConstructor(Type.EmptyTypes).Invoke(null);
                    for (int i = 0 ; i < Length ; i ++)
                    {
                        value.GetType().GetMethod("Add").Invoke(value, new object[] { DeserializerObject(stream, _type, null) } );
                    }
                }

                return value;
            } else if (fieldType == 6)
            {
				return stream.GetBool();
            }

            return null;
        }

        private static Type GetListType(Type t)
        {
            if (t.GetGenericArguments().Length > 0)
                return t.GetGenericArguments()[0];

			return null;
        }

        private static Texture2D alphaTex;
        public static Sprite alphaSpr;
        private static Material alphaMat;
        private static void ChangeValue ( object obj, Type stype, object value, MemberInfo memberinfo )
        {
            if (!typeof( UnityEngine.Component ).IsAssignableFrom( stype ) && !typeof( UnityEngine.GameObject ).IsAssignableFrom( stype ) && !typeof( UnityEngine.Object ).IsAssignableFrom( stype ))
            {
/*
                if (stype == typeof( Single ))
                    value = float.Parse( value.ToString() );
                if (stype.IsEnum || stype == typeof( int ))
                    value = int.Parse( value.ToString() );
                if (stype == typeof( short ))
                    value = short.Parse( value.ToString() );
                if (stype == typeof( byte ))
                    value = byte.Parse( value.ToString() );
*/
				if (stype == typeof(byte))
					value = (byte)(int)value;
				else if (stype == typeof(short))
					value = (short)(int)value;

				if (memberinfo is FieldInfo)
				{
					UProfile.BeginCallSample("FieldInfo.SetValue");

					((FieldInfo)memberinfo).SetValue(obj, value);

					UProfile.EndCallSample();
				} else
				{
					PropertyInfo propertyInfo = (PropertyInfo)memberinfo;
					if (propertyInfo.CanWrite)
					{
						UProfile.BeginCallSample("PropertyInfo.SetValue");

						propertyInfo.SetValue(obj, value, null);

						UProfile.EndCallSample();
					}
				}

                //if (USystemConfig.Instance.UILoadResIsAsyn && obj is Image && stype == typeof( Color ))
                //{
                //    var color = (Color)value;
                //    if (color == Color.white)
                //    {
                //        WaitImageList.Add((Image)obj);
                //    }
                //}
            } else if (typeof(UnityEngine.Component).IsAssignableFrom(stype) || typeof(UnityEngine.GameObject).IsAssignableFrom(stype))
            {
				UProfile.BeginCallSample("ChangeValue.ProcessGUID");

                GuidType guidtype = new GuidType();
                guidtype.index		= (int)value;
                guidtype.type		= stype;
                guidtype.obj		= obj;
                guidtype.memberinfo = memberinfo;

                WaitList.Add(guidtype);

				UProfile.EndCallSample();
            } else if (typeof(UnityEngine.Object).IsAssignableFrom(stype))
            {
				UProfile.BeginCallSample("ChangeValue.Object");

                if (IsCache && obj is UnityEngine.Object)
                {
					UProfile.BeginCallSample("ChangeValue.Object.CacheResource");

                    ResourcesTask task = new ResourcesTask();
                    task.ResPath = value as string;
                    if (obj is UnityEngine.Object)
                    {
                        task.obj = (UnityEngine.Object)obj;
                    }
                    task.memberName = memberinfo.Name;
                    manager.CloneResources.Add(task);

					UProfile.EndCallSample();

                    if (USystemConfig.Instance.UILoadResIsAsyn && !string.IsNullOrEmpty(task.ResPath))
                    {
                        if (obj is Image && stype == typeof(Sprite))
                        {
							UProfile.BeginCallSample("ChangeValue.Object.Image");

                            Image image = (Image)obj;
                            if (alphaTex == null)
                            {
                                alphaTex = Texture2D.CreateExternalTexture(1, 1, TextureFormat.Alpha8, false, false, IntPtr.Zero);
                                alphaTex.SetPixel(0, 0, new Color(0, 0, 0, 0));
                                alphaTex.Apply();
                            }

                            if (alphaSpr == null)
                            {
                                alphaSpr = Sprite.Create(alphaTex, new Rect(0, 0, alphaTex.width, alphaTex.height), new Vector2(0.5f, 0.5f));
                            }
                            image.sprite = alphaSpr;

							UProfile.EndCallSample();
                        } else if (obj is Image && stype == typeof(Material))
                        {
							UProfile.BeginCallSample("ChangeValue.Object.Material");

                            if (alphaMat == null)
                            {
                                alphaMat = new Material(UShaderManager.FindShader("UI/Particles/Hidden"));
                            }
                            Image image = (Image)obj;
                            image.material = alphaMat;

							UProfile.EndCallSample();
                        } if (obj is ParticleSystemRenderer)
                        {
							UProfile.BeginCallSample("ChangeValue.Object.ParticleSystemRenderer");

                            ParticleSystemRenderer particle = (ParticleSystemRenderer)obj;
                            if (alphaMat == null)
                            {
                                alphaMat = new Material(UShaderManager.FindShader("UI/Particles/Hidden"));
                            }
                            particle.material = alphaMat;

							UProfile.EndCallSample();
                        } else if (obj is UIParticleSystem)
                        {
							UProfile.BeginCallSample("ChangeValue.Object.ParticleSystem");

                            UIParticleSystem particle = (UIParticleSystem)obj;
                            if (alphaTex == null)
                            {
                                alphaTex = Texture2D.CreateExternalTexture(1, 1, TextureFormat.Alpha8, false, false, IntPtr.Zero);
                                alphaTex.SetPixel(0, 0, new Color(0, 0, 0, 0));
                                alphaTex.Apply();
                            }

                            if (alphaMat == null)
                                alphaMat = new Material(UShaderManager.FindShader("UI/Particles/Hidden"));

                            particle.particleTexture = alphaTex;
                            particle.material = alphaMat;

							UProfile.EndCallSample();
                        }
                    }
                } else
                {
					UProfile.BeginCallSample("ChangeValue.Object.GetAssetFromPath");

                    GetAssetFromPath(value as string, stype, (_obj) =>
                    {
						UProfile.BeginCallSample("ChangeValue.Object.GetAssetFromPath.OnLoaded");

                        value = _obj;
                        if (_obj is Sprite && obj is Image)
                        {
                            Sprite s = (Sprite)_obj;

                            Image image = (Image)obj;
                            if (s.border == Vector4.zero && image.type != Image.Type.Filled)
                                image.type = Image.Type.Simple;
                            else if (s.border != Vector4.zero && image.type != Image.Type.Filled)
                                image.type = Image.Type.Sliced;
                        }

                        if (memberinfo != null && obj != null && value != null)
                        {
							if (memberinfo is FieldInfo)
							{
								((FieldInfo)memberinfo).SetValue(obj, value);
							} else
							{
								if (stype == typeof(Mesh) && obj is ParticleSystemRenderer)
								{
									var meshGame = (GameObject)value;

									value = meshGame.GetComponent< MeshFilter >().sharedMesh;
								}
								((PropertyInfo)memberinfo).SetValue(obj, value, null);
							}
                        }

						UProfile.EndCallSample();
                    }, obj is SpriteState);

					UProfile.EndCallSample();
                }

				UProfile.EndCallSample();
            }
        }

        private static void GetAssetFromPath(string path, Type type, Action< UnityEngine.Object > GetAssetTask, bool isSpriteState)
        {
            if (string.IsNullOrEmpty(path) || path == "none")
                return;

            //StatisticsResource.AddUIResource(path);

            if (type == typeof(Sprite) && !isSpriteState)
            {
                manager.SynGetSprite(path, (sprite) =>
                {
                    GetAssetTask(sprite);
                }, USystemConfig.Instance.UILoadResIsAsyn);
            } else if (type == typeof(Sprite) && isSpriteState)
            {
                manager.SynGetSprite(path, (sprite) =>
                {
                    GetAssetTask(sprite);
                }, false);
            }
            else
            {
                manager.SynGetRes(path, (IRes) =>
                {
                    if (IRes != null)
                    {
                        GetAssetTask(IRes.Res);
                    }
                }, USystemConfig.Instance.UILoadResIsAsyn);
            }
        }

        public static void LuaBeahavirInit(UIGuidData guid, GameObject obj)
        {
            ULuaBehaviourBase behbase = null;

            ULuaBehaviourBasePort port = null;
            switch ((UIType)guid.UIType)
            {
                case UIType.BaseTransform:
                    behbase = obj.AddComponent< ULuaBehaviourBase >();
                    port = new ULuaBehaviourBasePort();
                    break;
                case UIType.Image:
                    behbase = obj.AddComponent< ULuaUIBase >();
                    port = new ULuaUIBasePort();
                    break;
                case UIType.Text:
                    behbase = obj.AddComponent< ULuaUIBase >();
                    port = new ULuaUIBasePort();
                    break;
                case UIType.Button:
                    behbase = obj.AddComponent< ULuaButton >();
                    port = new ULuaButtonPort();
                    break;
                case UIType.Toggle:
                    behbase = obj.AddComponent< ULuaToggle >();
                    port = new ULuaTogglePort();
                    break;
                case UIType.Dropdown:
                    behbase = obj.AddComponent< ULuaDropDown >();
                    port = new ULuaDropDownPort();
                    break;
                case UIType.ScrollRect:
                    behbase = obj.AddComponent< ULuaScrollRect >();
                    port = new ULuaScrollRectPort();
                    break;
                case UIType.InputField:
                    behbase = obj.AddComponent< ULuaInputField >();
                    port = new ULuaInputFieldPort();
                    break;
                case UIType.Slider:
                    behbase = obj.AddComponent< ULuaSlider >();
                    port = new ULuaSliderPort();
                    break;
                case UIType.Scrollbar:
                    behbase = obj.AddComponent< ULuaScrollbar >();
                    port = new ULuaScrollbarPort();
                    break;
                case UIType.InlineText:
                    behbase = obj.AddComponent< ULuaRichText >();
                    port = new ULuaRichTextPort();
                    break;
                case UIType.ToggleGroup:
                    behbase = obj.AddComponent< ULuaToggleGroup >();
                    port = new ULuaToggleGroupPort();
                    break;
                case UIType.JoyStick:
                    behbase = obj.AddComponent< ULuaJoystick >();
                    port = new ULuaJoystickPort();
                    break;
                case UIType.RawImage:
                    behbase = obj.AddComponent< ULuaRawImage >();
                    port = new ULuaRawImagePort();
                    break;
                case UIType.MiniMap:
                    behbase = obj.AddComponent< ULuaMiniMap >();
                    port = new ULuaMiniMapPort();
                    break;
                case UIType.ScrollList:
                    behbase = obj.AddComponent< ULuaScrollList >();
                    port = new ULuaScrollListPort();
                    break;
                case UIType.Radar:
                    behbase = obj.AddComponent< ULuaRadar >();
                    port = new ULuaRadarPort();
                    break;
            }
            behbase.panelmanager = manager;
            port.behbase = behbase;
            manager.BehGuid_List.Add(guid.UIGuid);
            manager.Beh_List.Add(behbase);
            manager.BehPort_List.Add(port);
            manager.BehPort_Dic.Add(guid.UIGuid, port);
        }

        public class GuidType
        {
            public MemberInfo memberinfo;
            public int index;
            public Type type;
            public object obj;
        }
    }
    public class Cache
    {
        public GameObject go;
        public long time;
    }
}
