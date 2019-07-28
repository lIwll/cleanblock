using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Newtonsoft.Json;

using UEngine.Data;
using UEngine.UIExpand;
using UEngine.UI.UILuaBehaviour;

namespace UEngine.UI
{
    public class UILoader
    {
        private static List<GuidType> WaitList = new List<GuidType>();

        private static Dictionary<string, GameObject> m_ObjDic = new Dictionary<string, GameObject>();

        private static PanelManager manager;

        public static PanelManagerPort LoadUIData( string path, bool active, Transform Parent, bool isCache = false )
        {
            float startTime = Time.realtimeSinceStartup;

			UProfile.BeginSample("Begin LoadUIData {0}", path);

            manager = null;
            WaitList.Clear();
            m_ObjDic.Clear();
            PanelManagerPort managerport = new PanelManagerPort();
            if (string.IsNullOrEmpty(path))
            {
                ULogger.Error("路径为空");
                return managerport;
            }
            string _path = path.Replace("Assets/Resources/", "");

            if (USystemConfig.Instance.IsCaChe)
            {
                if (UIManager.UIMinObj.ContainsKey(_path))
                {
                   var ret = InsPanel(UIManager.UIMinObj[_path], Parent, path, startTime, managerport,active);

				   UProfile.EndSample();

				   return ret;
                }
                else if (UIManager.UIMaxObj.ContainsKey(_path))
                {
                    var ret = InsPanel(UIManager.UIMaxObj[_path], Parent, path, startTime, managerport, active);

				   UProfile.EndSample();

				   return ret;
                }
                //if ( UIManager.UICacheObj.ContainsKey( _path ) )
                //{
                //    GameObject panel = GameObject.Instantiate( UIManager.UICacheObj[_path], Parent, false );

				//	UProfile.EndSample();

                //    PanelManager panelMan = panel.GetComponent<PanelManager>();
                //    panelMan.PanelManagerInit();
                //    managerport.panelmanager = panelMan;
                //    panel.SetActive( active );

                //    //结束
                //    ULogger.Info( "面板" + path + "加载成功...  Time : " + ( Time.realtimeSinceStartup - startTime ) * 1000f );

				//    UProfile.EndSample();

                //    return managerport;
                //}
            }

            byte[] bytes = UFileAccessor.ReadBinaryFile( _path );

            //StatisticsResource.AddUIDataResource( _path );

            if (bytes == null || bytes.Length == 0)
            {
                ULogger.Error("加载路径" + _path + "结果为空");

				UProfile.EndSample();

                return managerport;
            }

			int edition = BitConverter.ToInt32(bytes, 0);
            if (edition == 0)
            {
				UProfile.BeginSample("LoadUIData {0} Deserializer", path);

				var ret = UIDeserializer.Deserializer(path, bytes, active, Parent, isCache);

				UProfile.EndSample();

				UProfile.EndSample();

				return ret;
            } else if (edition == 1)
            {
				UProfile.BeginSample("LoadUIData {0} Deserializer", path);

				var ret = UIDeserializer.Deserializer(path, bytes, active, Parent, isCache, edition);

				UProfile.EndSample();

				UProfile.EndSample();

                return ret;
            } else if (edition == 2)
			{
				UProfile.BeginSample("LoadUIData {0} Deserializer", path);

                UByteStream stream = new UByteStream( bytes );

				var ret = UIDeserializer.Deserializer(path, stream, active, Parent, isCache, edition);

				UProfile.EndSample();

				UProfile.EndSample();

				return ret;
			}

			UProfile.BeginSample("LoadUIData {0} json Deserializer", path);

            string json = System.Text.ASCIIEncoding.UTF8.GetString( bytes );

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.DefaultValueHandling = DefaultValueHandling.Ignore;
            settings.TypeNameHandling = TypeNameHandling.Auto;

            float desTime = Time.realtimeSinceStartup;
            UIPanelConfig table = JsonConvert.DeserializeObject<UIPanelConfig>(json, settings);
            //ULogger.Info( "面板" + path + "解析成功...  Time : " + ( Time.realtimeSinceStartup - desTime ) * 1000f );

            //实例化GameObject
            Dictionary<string, UIGameObject>.Enumerator it = table.GameObjectDic.GetEnumerator();
            while( it.MoveNext())
            {
                var gameobj = it.Current;
                GameObject go = new GameObject(gameobj.Value.objname);
                go.SetActive(gameobj.Value.IsActive);
                go.layer = LayerMask.NameToLayer("UI");
                m_ObjDic.Add(gameobj.Key, go);
                EditCreateGUID guidedit = go.AddComponent<EditCreateGUID>();
                guidedit.m_GUID = gameobj.Key;
            }

            GameObject panelroot = m_ObjDic[table.UIroot];
            manager = panelroot.AddComponent<PanelManager>();
            manager.AnimationPath = _path.Replace( ".txt", "" ) + "_Animation.ani";
            managerport.panelmanager = manager;

            //设置父子物体关系
            it = table.GameObjectDic.GetEnumerator();
            while (it.MoveNext())
            {
                var gameobj = it.Current;
                manager.EditorGuidObj_Dic.Add( gameobj.Key, m_ObjDic[gameobj.Key] );
                for (int i = 0; i < gameobj.Value.Child_List.Count; i++)
                {
                    m_ObjDic[gameobj.Value.Child_List[i]].transform.SetParent(m_ObjDic[gameobj.Key].transform, false);
                }
            }

            AddGameObject( panelroot, manager );

            desTime = Time.realtimeSinceStartup;
            //添加组件
            it = table.GameObjectDic.GetEnumerator();
            while (it.MoveNext())
            {
                var gameobj = it.Current;
                //float addTime = Time.realtimeSinceStartup;
                AddComponents(m_ObjDic[gameobj.Key], gameobj.Value);
                //ULogger.Info( m_ObjDic[gameobj.Key].name + ": " + "添加组件成功...  Time : " + ( Time.realtimeSinceStartup - addTime ) * 1000f );
            }
            //ULogger.Info( "面板" + path + "添加组件成功...  Time : " + ( Time.realtimeSinceStartup - desTime ) * 1000f );

            //获取对象引用
			for (int i = 0; i < WaitList.Count; ++ i)
			{
				var item = WaitList[i];

				object value = null;
				object obj = item.obj;
				var gameobj = GetGuidObj(item.guid);
				if (typeof(UnityEngine.Component).IsAssignableFrom(item.type) && gameobj)
				{
					value = gameobj.GetComponent(item.type);
				}
				else if (typeof(UnityEngine.GameObject).IsAssignableFrom(item.type) && gameobj)
				{
					value = gameobj;
				}

				if (item.memberinfo is FieldInfo)
					((FieldInfo)item.memberinfo).SetValue(obj, value);
				else
					((PropertyInfo)item.memberinfo).SetValue(obj, value, null);
			}

            //控制整个面板的显示和父子物体关系
            panelroot.SetActive(active);
            panelroot.transform.SetParent(Parent, false);

            //加载Guid引用关系
            string guidpath = _path.Replace(".txt", "") + "_Guid.guid";
            string jsondic = UFileAccessor.ReadStringFile(guidpath);
            if (string.IsNullOrEmpty(jsondic))
            {
                //结束
                //ULogger.Info( "面板" + path + "加载成功...  Time : " + ( Time.realtimeSinceStartup - startTime ) * 1000f );

				UProfile.EndSample();

				UProfile.EndSample();

                return managerport;
            }

            manager.GuidLua_Dic = JsonConvert.DeserializeObject<SortedDictionary<string, string>>(jsondic, settings);
            SortedDictionary<string, string>.Enumerator it2 = manager.GuidLua_Dic.GetEnumerator();
            while( it2.MoveNext())
            {
                var item = it2.Current;
                if (!string.IsNullOrEmpty(item.Key))
                {
                    manager.luaguidlist.Add(item.Key);
                    manager.guidlist.Add(item.Value);
                    manager.ObjList.Add(m_ObjDic[item.Value]);
                    UiWidgetGuid luaGuid = m_ObjDic[item.Value].GetComponent<UiWidgetGuid>();
                    if (!luaGuid)
                    {
                        luaGuid = m_ObjDic[item.Value].AddComponent<UiWidgetGuid>();
                        luaGuid.UIGuid = item.Key;
                        luaGuid.uitype = luaGuid.GetUIType();
                        LuaBeahavirInit(luaGuid, m_ObjDic[item.Value]);
                    }
                    else
                    {
                        luaGuid.UIGuid = item.Key;
                        luaGuid.uitype = luaGuid.GetUIType();
                        LuaBeahavirInit( luaGuid, m_ObjDic[item.Value] );
                    }
                }
            }
            //结束
            //ULogger.Info( "面板" + path + "加载成功...  Time : " + ( Time.realtimeSinceStartup - startTime ) * 1000f );
			UProfile.EndSample();

			UProfile.EndSample();

            return managerport;
        }

        public static PanelManagerPort InsPanel(Cache tempCach, Transform parent, string path, float startTime, PanelManagerPort port, bool active)
        {
            UProfile.BeginSample("LoadUIData {0} Intantiate", path);

            GameObject panel = GameObject.Instantiate(tempCach.go, parent, false);

            UProfile.BeginSample("LoadUIData {0} Intantiate init panel manager", path);

            PanelManager panelMan = panel.GetComponent< PanelManager >();
            panelMan.PanelManagerInit();
            port.panelmanager = panelMan;
            panel.SetActive(active);

			UProfile.EndSample();

            //结束
            //ULogger.Info("面板" + path + "加载成功...  Time : " + (Time.realtimeSinceStartup - startTime) * 1000f);

			UProfile.EndSample();

            tempCach.time = DateTime.Now.Ticks;

            return port;
        }
        private static void AddGameObject ( GameObject go, PanelManager manager ) 
        {
            manager.GameObjectList.Add( go );
            for (int i = 0 ; i < go.transform.childCount ; i++)
            {
                AddGameObject( go.transform.GetChild( i ).gameObject, manager );
            }
        }

        private static void AddComponents(GameObject go, UIGameObject godata)
        {
            Dictionary<string, UIClassConfig>.Enumerator it = godata.CompoentsDic.GetEnumerator();
            while(it.MoveNext())
            {
                var com = it.Current;
                Type type = UTypeCache.GetType(com.Key);
                if (type == null)
                    continue;
                
                Component component;

                //if (com.Key == "UnityEngine.ParticleSystem" || com.Key == "UnityEngine.ParticleSystemRenderer")
                //{
                //    continue;
                //}

                if (com.Key == "UnityEngine.ParticleSystemRenderer")
                {
                    component = go.GetComponent( type );
                }
                else
                {
                    component = go.AddComponent( type );
                }

                if (component is ParticleSystem) 
                    ( ( ParticleSystem )component ).Stop();

                SetComValue(component, com.Value, type);

                if (component is ParticleSystem)
                    ( ( ParticleSystem )component ).Play();
                
                if (component is CText) 
                {
                    CText ctext =( CText )component;
                    ctext.Init();
                }
            }
        }

        private static void SetComValue(object obj, UIClassConfig classconfig, Type script)
        {
            Dictionary<string, object>.Enumerator it = classconfig.MemberDic.GetEnumerator();
            while(it.MoveNext())
            {
                var item = it.Current.Key;
                FieldInfo fieldInfo = script.GetField( item );
                if (fieldInfo != null)
                {
                    object dicvalue = classconfig.MemberDic[fieldInfo.Name];
                    if (dicvalue is UIClassConfig)
                    {
                        var value = Activator.CreateInstance( fieldInfo.FieldType );
                        SetComValue( value, dicvalue as UIClassConfig, fieldInfo.FieldType );
                        fieldInfo.SetValue( obj, value );
                    }
                    else if (dicvalue is System.Object[])
                    {
                        object length = dicvalue.GetType().GetMethod( "GetLength" ).Invoke( dicvalue, new object[] { 0 } );
                        if (( int )length <= 0)
                        {
                            return;
                        }
                        MethodInfo _mi = dicvalue.GetType().GetMethod( "GetValue", new[] { typeof( int ) } );
                        object _arrayvalue = _mi.Invoke( dicvalue, new object[] { 0 } );
                        Type stype = _arrayvalue.GetType();
                        Type ftype = fieldInfo.FieldType;
                        if (stype == typeof( UIClassConfig ))
                        {
                            object value = null;
                            Type _type = null;
                            if (ftype.IsArray)
                            {
                                _type = fieldInfo.FieldType.GetElementType();
                                value = Array.CreateInstance( _type, ( int )length );
                            }
                            else if (ftype.IsGenericType && ftype.GetGenericTypeDefinition() == typeof( List<> ))
                            {
                                _type = GetListType( ftype );
                                value = fieldInfo.FieldType.GetConstructor( Type.EmptyTypes ).Invoke( null );
                            }
                            for (int j = 0 ; j < ( int )length ; j++)
                            {
                                MethodInfo mi = dicvalue.GetType().GetMethod( "GetValue", new[] { typeof( int ) } );
                                object arrayvalue = mi.Invoke( dicvalue, new object[] { j } );
                                if (ftype.IsArray)
                                {
                                    var valueindex = Activator.CreateInstance( _type );
                                    SetComValue( valueindex, arrayvalue as UIClassConfig, _type );
                                    ( value as Array ).SetValue( valueindex, j );
                                }
                                else if (ftype.IsGenericType && ftype.GetGenericTypeDefinition() == typeof( List<> ))
                                {
                                    var valueindex = Activator.CreateInstance( _type );
                                    SetComValue( valueindex, arrayvalue as UIClassConfig, _type );
                                    value.GetType().GetMethod( "Add" ).Invoke( value, new object[] { valueindex } );
                                }
                            }
                            fieldInfo.SetValue( obj, value );
                        }
                        else
                        {
                            if (ftype.IsArray)
                            {
                                fieldInfo.SetValue( obj, dicvalue );
                            }
                            else if (ftype.IsGenericType && ftype.GetGenericTypeDefinition() == typeof( List<> ))
                            {
                                for (int j = 0 ; j < ( int )length ; j++)
                                {
                                    MethodInfo mi = stype.GetMethod( "GetValue", new[] { typeof( int ) } );
                                    object arrayvalue = mi.Invoke( dicvalue, new object[] { j } );
                                    ftype.GetMethod( "Add" ).Invoke( obj, new object[] { arrayvalue } );
                                }
                            }
                        }
                    }
                    else
                    {
                        ChangeValue( obj, fieldInfo.FieldType, dicvalue, fieldInfo );
                    }
                }
                else
                {
                    PropertyInfo propertyInfo = script.GetProperty(item);
                    if (propertyInfo != null)
                    {
                        object dicvalue = classconfig.MemberDic[propertyInfo.Name];
                        if (dicvalue is UIClassConfig)
                        {
                            object prValue = propertyInfo.GetValue( obj, null );
                            if (prValue != null && !propertyInfo.CanWrite)
                            {
                                SetComValue( prValue, dicvalue as UIClassConfig, propertyInfo.PropertyType );
                            }
                            else
                            {
                                var value = Activator.CreateInstance( propertyInfo.PropertyType );
                                SetComValue( value, dicvalue as UIClassConfig, propertyInfo.PropertyType );
                                propertyInfo.SetValue( obj, value, null );
                            }
                        }
                        else if (dicvalue is System.Object[])
                        {
                            object length = dicvalue.GetType().GetMethod( "GetLength" ).Invoke( dicvalue, new object[] { 0 } );
                            if (( int )length <= 0)
                            {
                                return;
                            }
                            MethodInfo _mi = dicvalue.GetType().GetMethod( "GetValue", new[] { typeof( int ) } );
                            object _arrayvalue = _mi.Invoke( dicvalue, new object[] { 0 } );
                            Type stype = _arrayvalue.GetType();
                            Type ftype = propertyInfo.PropertyType;
                            if (stype == typeof( UIClassConfig ))
                            {
                                object value = null;
                                Type _type = null;

                                if (ftype.IsArray)
                                {
                                    _type = ftype.GetElementType();
                                    value = Array.CreateInstance( _type, ( int )length );
                                }
                                else if (ftype.IsGenericType && ftype.GetGenericTypeDefinition() == typeof( List<> ))
                                {
                                    _type = GetListType( ftype );
                                    value = ftype.GetConstructor( Type.EmptyTypes ).Invoke( null );
                                }
                                for (int j = 0 ; j < ( int )length ; j++)
                                {
                                    MethodInfo mi = dicvalue.GetType().GetMethod( "GetValue", new[] { typeof( int ) } );
                                    object arrayvalue = mi.Invoke( dicvalue, new object[] { j } );
                                    if (ftype.IsArray)
                                    {
                                        var valueindex = Activator.CreateInstance( _type );
                                        SetComValue( valueindex, arrayvalue as UIClassConfig, _type );
                                        ( value as Array ).SetValue( valueindex, j );
                                    }
                                    else if (ftype.IsGenericType && ftype.GetGenericTypeDefinition() == typeof( List<> ))
                                    {
                                        var valueindex = Activator.CreateInstance( _type );
                                        SetComValue( valueindex, arrayvalue as UIClassConfig, _type );
                                        value.GetType().GetMethod( "Add" ).Invoke( value, new object[] { valueindex } );
                                    }
                                }
                                propertyInfo.SetValue( obj, value, null );
                            }
                            else
                            {
                                if (ftype.IsArray)
                                {
                                    propertyInfo.SetValue( obj, dicvalue, null );
                                }
                                else if (ftype.IsGenericType && ftype.GetGenericTypeDefinition() == typeof( List<> ))
                                {
                                    for (int j = 0 ; j < ( int )length ; j++)
                                    {
                                        MethodInfo mi = stype.GetMethod( "GetValue", new[] { typeof( int ) } );
                                        object arrayvalue = mi.Invoke( dicvalue, new object[] { j } );
                                        ftype.GetMethod( "Add" ).Invoke( obj, new object[] { arrayvalue } );
                                    }
                                }
                            }
                        }
                        else
                        {
                            ChangeValue( obj, propertyInfo.PropertyType, dicvalue, propertyInfo );
                        }
                    }
                }
            }

            //FieldInfo[] fieldInfos = script.GetFields(~BindingFlags.Static & ~BindingFlags.DeclaredOnly);
            //PropertyInfo[] propertyInfos = script.GetProperties(~BindingFlags.Static & ~BindingFlags.DeclaredOnly);
            //for (int i = 0; i < fieldInfos.Length; i++)
            //{
            //    if (classconfig.MemberDic.ContainsKey(fieldInfos[i].Name))
            //    {
            //        object dicvalue = classconfig.MemberDic[fieldInfos[i].Name];
            //        if (dicvalue is UIClassConfig)
            //        {
            //            var value = Activator.CreateInstance(fieldInfos[i].FieldType);
            //            SetComValue(value, dicvalue as UIClassConfig, fieldInfos[i].FieldType);
            //            fieldInfos[i].SetValue(obj, value);
            //        }
            //        else if (dicvalue is System.Object[])
            //        {
            //            object length = dicvalue.GetType().GetMethod("GetLength").Invoke(dicvalue, new object[] { 0 });
            //            if ((int)length <= 0)
            //            {
            //                return;
            //            }
            //            MethodInfo _mi = dicvalue.GetType().GetMethod("GetValue", new[] { typeof(int) });
            //            object _arrayvalue = _mi.Invoke(dicvalue, new object[] { 0 });
            //            Type stype = _arrayvalue.GetType();
            //            Type ftype = fieldInfos[i].FieldType;
            //            if (stype == typeof(UIClassConfig))
            //            {
            //                object value = null;
            //                Type _type = null;
            //                if (ftype.IsArray)
            //                {
            //                    _type = fieldInfos[i].FieldType.GetElementType();
            //                    value = Array.CreateInstance( _type, ( int )length );
            //                }
            //                else if (ftype.IsGenericType && ftype.GetGenericTypeDefinition() == typeof(List<>))
            //                {
            //                    _type = GetListType(ftype);
            //                    value = fieldInfos[i].FieldType.GetConstructor(Type.EmptyTypes).Invoke(null);
            //                }
            //                for (int j = 0; j < (int)length; j++)
            //                {
            //                    MethodInfo mi = dicvalue.GetType().GetMethod("GetValue", new[] { typeof(int) });
            //                    object arrayvalue = mi.Invoke(dicvalue, new object[] { j });
            //                    if (ftype.IsArray)
            //                    {
            //                        var valueindex = Activator.CreateInstance(_type);
            //                        SetComValue(valueindex, arrayvalue as UIClassConfig, _type);
            //                        (value as Array).SetValue(valueindex, j);
            //                    }
            //                    else if (ftype.IsGenericType && ftype.GetGenericTypeDefinition() == typeof(List<>))
            //                    {
            //                        var valueindex = Activator.CreateInstance(_type);
            //                        SetComValue(valueindex, arrayvalue as UIClassConfig, _type);
            //                        value.GetType().GetMethod("Add").Invoke(value, new object[] { valueindex });
            //                    }
            //                }
            //                fieldInfos[i].SetValue(obj, value);
            //            }
            //            else
            //            {
            //                if (ftype.IsArray)
            //                {
            //                    fieldInfos[i].SetValue(obj, dicvalue);
            //                }
            //                else if (ftype.IsGenericType && ftype.GetGenericTypeDefinition() == typeof(List<>))
            //                {
            //                    for (int j = 0; j < (int)length; j++)
            //                    {
            //                        MethodInfo mi = stype.GetMethod("GetValue", new[] { typeof(int) });
            //                        object arrayvalue = mi.Invoke(dicvalue, new object[] { j });
            //                        ftype.GetMethod("Add").Invoke(obj, new object[] { arrayvalue });
            //                    }
            //                }
            //            }
            //        }
            //        else
            //        {
            //            ChangeValue(obj, fieldInfos[i].FieldType, dicvalue, fieldInfos[i]);
            //        }
            //    }
            //}

            if (obj is ParticleSystem)
            {
                var emiss = ( ( ParticleSystem )obj ).emission;
                if (classconfig.MemberDic.ContainsKey( "bursts" ))
                {
                    object dicvalue = classconfig.MemberDic["bursts"];
                    object length = dicvalue.GetType().GetMethod( "GetLength" ).Invoke( dicvalue, new object[] { 0 } );
                    if (( int )length <= 0)
                    {
                        return;
                    }
                    MethodInfo _mi = dicvalue.GetType().GetMethod( "GetValue", new[] { typeof( int ) } );
                    object _arrayvalue = _mi.Invoke( dicvalue, new object[] { 0 } );
                    Type stype = _arrayvalue.GetType();
                    Type ftype = typeof( ParticleSystem.Burst[] );
                    object value = null;
                    Type _type = null;
                    _type = ftype.GetElementType();
                    value = Array.CreateInstance( _type, ( int )length );

                    for (int j = 0 ; j < ( int )length ; j++)
                    {
                        MethodInfo mi = dicvalue.GetType().GetMethod( "GetValue", new[] { typeof( int ) } );
                        object arrayvalue = mi.Invoke( dicvalue, new object[] { j } );
                        if (ftype.IsArray)
                        {
                            var valueindex = Activator.CreateInstance( _type );
                            SetComValue( valueindex, arrayvalue as UIClassConfig, _type );
                            ( value as Array ).SetValue( valueindex, j );
                        }
                    }
                    emiss.SetBursts( value as ParticleSystem.Burst[] );
                }
            }

            //for (int i = 0; i < propertyInfos.Length; i++)
            //{
            //    if (classconfig.MemberDic.ContainsKey(propertyInfos[i].Name))
            //    {
            //        object dicvalue = classconfig.MemberDic[propertyInfos[i].Name];
            //        if (dicvalue is UIClassConfig)
            //        {
            //            object prValue = propertyInfos[i].GetValue( obj, null );
            //            if (prValue != null && !propertyInfos[i].CanWrite)
            //            {
            //                SetComValue( prValue, dicvalue as UIClassConfig, propertyInfos[i].PropertyType );
            //            }
            //            else
            //            {
            //                var value = Activator.CreateInstance(propertyInfos[i].PropertyType);
            //                SetComValue(value, dicvalue as UIClassConfig, propertyInfos[i].PropertyType);
            //                propertyInfos[i].SetValue(obj, value, null);
            //            }
            //        }
            //        else if (dicvalue is System.Object[])
            //        {
            //            object length = dicvalue.GetType().GetMethod("GetLength").Invoke(dicvalue, new object[] { 0 });
            //            if ((int)length <= 0)
            //            {
            //                return;
            //            }
            //            MethodInfo _mi = dicvalue.GetType().GetMethod("GetValue", new[] { typeof(int) });
            //            object _arrayvalue = _mi.Invoke(dicvalue, new object[] { 0 });
            //            Type stype = _arrayvalue.GetType();
            //            Type ftype = propertyInfos[i].PropertyType;
            //            if (stype == typeof(UIClassConfig))
            //            {
            //                object value = null;
            //                Type _type = null;

            //                if (ftype.IsArray)
            //                {
            //                    _type = ftype.GetElementType();
            //                    value = Array.CreateInstance( _type, ( int )length );
            //                }
            //                else if (ftype.IsGenericType && ftype.GetGenericTypeDefinition() == typeof(List<>))
            //                {
            //                    _type = GetListType(ftype);
            //                    value = ftype.GetConstructor( Type.EmptyTypes ).Invoke( null );
            //                }
            //                for (int j = 0; j < (int)length; j++)
            //                {
            //                    MethodInfo mi = dicvalue.GetType().GetMethod("GetValue", new[] { typeof(int) });
            //                    object arrayvalue = mi.Invoke(dicvalue, new object[] { j });
            //                    if (ftype.IsArray)
            //                    {
            //                        var valueindex = Activator.CreateInstance(_type);
            //                        SetComValue(valueindex, arrayvalue as UIClassConfig, _type);
            //                        (value as Array).SetValue(valueindex, j);
            //                    }
            //                    else if (ftype.IsGenericType && ftype.GetGenericTypeDefinition() == typeof(List<>))
            //                    {
            //                        var valueindex = Activator.CreateInstance(_type);
            //                        SetComValue(valueindex, arrayvalue as UIClassConfig, _type);
            //                        value.GetType().GetMethod("Add").Invoke(value, new object[] { valueindex });
            //                    }
            //                }
            //                propertyInfos[i].SetValue(obj, value, null);
            //            }
            //            else
            //            {
            //                if (ftype.IsArray)
            //                {
            //                    propertyInfos[i].SetValue(obj, dicvalue, null);
            //                }
            //                else if (ftype.IsGenericType && ftype.GetGenericTypeDefinition() == typeof(List<>))
            //                {
            //                    for (int j = 0; j < (int)length; j++)
            //                    {
            //                        MethodInfo mi = stype.GetMethod("GetValue", new[] { typeof(int) });
            //                        object arrayvalue = mi.Invoke(dicvalue, new object[] { j });
            //                        ftype.GetMethod("Add").Invoke(obj, new object[] { arrayvalue });
            //                    }
            //                }
            //            }
            //        }
            //        else
            //        {
            //            ChangeValue(obj, propertyInfos[i].PropertyType, dicvalue, propertyInfos[i]);
            //        }
            //    }
            //}
        }

        private static Type GetListType(Type t)
        {
            if (t.GetGenericArguments().Length > 0)
            {
                return t.GetGenericArguments()[0];
            }
            else
            {
                return null;
            }
        }

        private static void ChangeValue(object obj, Type stype, object value, MemberInfo memberinfo)
        {
            if (!typeof(UnityEngine.Component).IsAssignableFrom(stype) && !typeof(UnityEngine.GameObject).IsAssignableFrom(stype) && !typeof(UnityEngine.Object).IsAssignableFrom(stype))
            {
                if (stype == typeof(Single))
                    value = float.Parse(value.ToString());
                if (stype.IsEnum || stype == typeof(int))
                    value = int.Parse(value.ToString());
                if (stype == typeof( short ))
                    value = short.Parse( value.ToString() );
                if (stype == typeof( byte ))
                    value = byte.Parse( value.ToString() );

                if (memberinfo is FieldInfo)
                    ((FieldInfo)memberinfo).SetValue(obj, value);
                else
                    ((PropertyInfo)memberinfo).SetValue(obj, value, null);
            }
            else if (typeof(UnityEngine.Component).IsAssignableFrom(stype) || typeof(UnityEngine.GameObject).IsAssignableFrom(stype))
            {
                if (string.IsNullOrEmpty(value as string))
                {
                    return;
                }
                GuidType guidtype = new GuidType();
                guidtype.guid = value as string;
                guidtype.type = stype;
                guidtype.obj = obj;
                guidtype.memberinfo = memberinfo;
                WaitList.Add(guidtype);
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(stype))
            {
                GetAssetFromPath( value as string, stype, ( _obj ) => 
                {
                    value = _obj;
                    if (memberinfo != null && obj != null && value != null)
                    {
                        if (memberinfo is FieldInfo)
                            ( ( FieldInfo )memberinfo ).SetValue( obj, value );
                        else
                            ( ( PropertyInfo )memberinfo ).SetValue( obj, value, null );
                    }
                } );
            }
        }

        private static void GetAssetFromPath(string path, Type type, Action<UnityEngine.Object> GetAssetTask)
        {
            if (string.IsNullOrEmpty(path) || path == "none")
                return;

            if (type == typeof(Sprite))
            {
                manager.SynGetSprite( path, ( sprite ) => 
                {
                    GetAssetTask(sprite);
                }, USystemConfig.Instance.UILoadResIsAsyn );
            } else
            {
                manager.SynGetRes( path, ( IRes ) => 
                {
                    if (IRes != null)
                    {
                        GetAssetTask( IRes.Res );
                    }
                }, USystemConfig.Instance.UILoadResIsAsyn );
            }
        }

        static GameObject GetGuidObj(string guid)
        {
            if (guid != null && m_ObjDic.ContainsKey(guid))
            {
                return m_ObjDic[guid];
            }
            return null;
        }

        public class GuidType
        {
            public MemberInfo memberinfo;
            public string guid;
            public Type type;
            public object obj;
        }

        public static void LuaBeahavirInit(UiWidgetGuid guid, GameObject obj) 
        {
            ULuaBehaviourBase behbase = null;
            ULuaBehaviourBasePort port = null;
            switch (guid.uitype)
            {
                case UIType.BaseTransform:
                    behbase = obj.AddComponent<ULuaBehaviourBase>();
                    port = new ULuaBehaviourBasePort();
                    break;
                case UIType.Image:
                    behbase = obj.AddComponent<ULuaUIBase>();
                    port = new ULuaUIBasePort();
                    break;
                case UIType.Text:
                    behbase = obj.AddComponent<ULuaUIBase>();
                    port = new ULuaUIBasePort();
                    break;
                case UIType.Button:
                    behbase = obj.AddComponent<ULuaButton>();
                    port = new ULuaButtonPort();
                    break;
                case UIType.Toggle:
                    behbase = obj.AddComponent<ULuaToggle>();
                    port = new ULuaTogglePort();
                    break;
                case UIType.Dropdown:
                    behbase = obj.AddComponent<ULuaDropDown>();
                    port = new ULuaDropDownPort();
                    break;
                case UIType.ScrollRect:
                    behbase = obj.AddComponent<ULuaScrollRect>();
                    port = new ULuaScrollRectPort();
                    break;
                case UIType.InputField:
                    behbase = obj.AddComponent<ULuaInputField>();
                    port = new ULuaInputFieldPort();
                    break;
                case UIType.Slider:
                    behbase = obj.AddComponent<ULuaSlider>();
                    port = new ULuaSliderPort();
                    break;
                case UIType.Scrollbar:
                    behbase = obj.AddComponent<ULuaScrollbar>();
                    port = new ULuaScrollbarPort();
                    break;
                case UIType.InlineText:
                    behbase = obj.AddComponent<ULuaRichText>();
                    port = new ULuaRichTextPort();
                    break;
                case UIType.ToggleGroup:
                    behbase = obj.AddComponent<ULuaToggleGroup>();
                    port = new ULuaToggleGroupPort();
                    break;
                case UIType.JoyStick:
                    behbase = obj.AddComponent<ULuaJoystick>();
                    port = new ULuaJoystickPort();
                    break;
                case UIType.RawImage:
                    behbase = obj.AddComponent<ULuaRawImage>();
                    port = new ULuaRawImagePort();
                    break;
                case UIType.MiniMap:
                    behbase = obj.AddComponent<ULuaMiniMap>();
                    port = new ULuaMiniMapPort();
                    break;
                case UIType.ScrollList:
                    behbase = obj.AddComponent<ULuaScrollList>();
                    port = new ULuaScrollListPort();
                    break;
                case UIType.Radar:
                    behbase = obj.AddComponent<ULuaRadar>();
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
    }
}