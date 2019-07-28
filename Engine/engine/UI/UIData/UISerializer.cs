using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace UEngine.UI
{
    public class UISerializer
    {
        private static int index = -1;
        
        private static UIPanelConfig UIPanel;

        private static Dictionary< string, int > GuidMap = new Dictionary< string, int >();

		public static void Serializer(UIPanelConfig panel, MemoryStream memoryStream)
		{
			UIPanel = panel;

			GuidMap.Clear();

			byte[] bytes = BitConverter.GetBytes(1);
			memoryStream.Write(bytes, 0, bytes.Length);

			SerializerInt(memoryStream, panel.PanelMode);
			SerializerFloat(memoryStream, panel.BottomValue);

			SerializerInt(memoryStream, panel.GameObjectDic.Count);

			UIGameObject uiGameObj = panel.GameObjectDic[panel.UIroot];

			index = 0;

			MapInit(uiGameObj, panel.UIroot);

			index = -1;

			SerializerGameObj(memoryStream, uiGameObj, -1);
		}

        public static void Serializer(UIPanelConfig panel, UByteStream memoryStream) 
        {
            UIPanel = panel;

            GuidMap.Clear();

            memoryStream.AddI32(panel.PanelMode);
            memoryStream.AddF32(panel.BottomValue);

            memoryStream.AddI32(panel.GameObjectDic.Count);

            UIGameObject uiGameObj = panel.GameObjectDic[panel.UIroot];

            index = 0;
            
            MapInit(uiGameObj, panel.UIroot);

            index = -1;

            SerializerGameObj(memoryStream, uiGameObj, -1);
        }

        static void MapInit(UIGameObject gameObj, string guid) 
        {
            GuidMap.Add(guid, index);

            index ++;
            for (int i = 0 ; i < gameObj.Child_List.Count ; i ++)
            {
                string childGuid = gameObj.Child_List[i];

                MapInit(UIPanel.GameObjectDic[childGuid], childGuid);
            }
        }

		static void SerializerString(MemoryStream memory, string value)
		{
			byte[] stringbytes = System.Text.UnicodeEncoding.UTF8.GetBytes(value);
			byte[] bytes = BitConverter.GetBytes((short)stringbytes.Length);
			memory.Write(bytes, 0, bytes.Length);
			memory.Write(stringbytes, 0, stringbytes.Length);
		}

		static void SerializerInt(MemoryStream memory, int value)
		{
			byte[] bytes = BitConverter.GetBytes((short)value);
			memory.Write(bytes, 0, bytes.Length);
		}

		static void SerializerFloat(MemoryStream memory, float value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			memory.Write(bytes, 0, bytes.Length);
		}

		static void SerializerBool(MemoryStream memory, bool value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			memory.Write(bytes, 0, bytes.Length);
		}

		static void SerializerGameObj(MemoryStream memoryStream, UIGameObject uiGameObj, int _index)
		{
			if (_index != -1)
				SerializerInt(memoryStream, _index);

			SerializerUIGameObject(memoryStream, uiGameObj);

			index ++;

			int parentIndex = index;

			for (int i = 0; i < uiGameObj.Child_List.Count; i ++)
				SerializerGameObj(memoryStream, UIPanel.GameObjectDic[uiGameObj.Child_List[i]], parentIndex);
		}

        static void SerializerGameObj(UByteStream memoryStream, UIGameObject uiGameObj, int _index) 
        {
            if (_index != -1)
                memoryStream.AddI32(_index);

            SerializerUIGameObject(memoryStream, uiGameObj);

            index ++;

            int parentIndex = index;

            for (int i = 0 ; i < uiGameObj.Child_List.Count ; i ++)
                SerializerGameObj(memoryStream, UIPanel.GameObjectDic[uiGameObj.Child_List[i]], parentIndex);
        }

		static void SerializerUIGameObject(MemoryStream memory, UIGameObject gameObject)
		{
			SerializerString(memory, gameObject.objname);
			SerializerBool(memory, gameObject.IsActive);
			SerializerInt(memory, gameObject.CompoentsDic.Count);

			Dictionary< string, UIClassConfig >.Enumerator it = gameObject.CompoentsDic.GetEnumerator();
			while (it.MoveNext())
			{
				var item = it.Current;

				SerializerString(memory, item.Key);

				SerializerUIClass(memory, item.Value);
			}
		}

        static void SerializerUIGameObject(UByteStream memory, UIGameObject gameObject) 
        {
            memory.AddString(gameObject.objname);
            memory.AddBool(gameObject.IsActive);
            memory.AddI32(gameObject.CompoentsDic.Count);

            Dictionary< string, UIClassConfig >.Enumerator it = gameObject.CompoentsDic.GetEnumerator();
            while (it.MoveNext())
            {
                var item = it.Current;

                memory.AddString(item.Key);

                SerializerUIClass(memory, item.Value);
            }
        }

		static void SerializerUIClass(MemoryStream memory, UIClassConfig classConfig)
		{
			int count = 0;

			Dictionary< string, object >.Enumerator it1 = classConfig.MemberDic.GetEnumerator();
			while (it1.MoveNext())
			{
				var item = it1.Current.Value;
				if (item != null)
					count ++;
			}
			SerializerInt(memory, count);

			it1 = classConfig.MemberDic.GetEnumerator();
			while (it1.MoveNext())
			{
				var item = it1.Current;
				if (item.Value != null)
				{
					SerializerString(memory, item.Key);

					SerializerObject(memory, item.Value);
				}
			}
		}

        static void SerializerUIClass(UByteStream memory, UIClassConfig classConfig) 
        {
            int count = 0;

            Dictionary< string, object >.Enumerator it1 = classConfig.MemberDic.GetEnumerator();
            while (it1.MoveNext())
            {
                var item = it1.Current.Value;
                if (item != null)
                    count ++;
            }

            memory.AddI32(count);

            it1 = classConfig.MemberDic.GetEnumerator();
            while (it1.MoveNext())
            {
                var item = it1.Current;
                if (item.Value != null)
                {
                    memory.AddString(item.Key);

                    SerializerObject(memory, item.Value);
                }
            }
        }

		static void SerializerObject(MemoryStream memory, object obj) 
        {
            if (obj is long || obj is int || obj is Enum || obj is short)
            {
                SerializerInt(memory, 0);
                if (obj is short)
                {
                    SerializerInt(memory, (short)obj);
                } else
                {
                    if (obj is Enum)
                        obj = (int)obj;
                    SerializerInt(memory, int.Parse(obj.ToString()));
                }
            } else if (obj is double || obj is float)
            {
                SerializerInt(memory, 1);
                SerializerFloat(memory, float.Parse(obj.ToString()));
            } else if (obj is string)
            {
                string value = (string)obj;
                if (GuidMap.ContainsKey(value))
                {
                    SerializerInt(memory, 0);
                    SerializerInt(memory, (short)(GuidMap[value]));
                } else
                {
                    SerializerInt(memory, 2);
                    SerializerString(memory, (string)obj);
                }
            } else if (obj is byte)
            {
                SerializerInt(memory, 3);
                memory.WriteByte((byte)obj);
            } else if (obj is UIClassConfig)
            {
                SerializerInt(memory, 4);
                SerializerUIClass(memory, (UIClassConfig)obj);
            } else if (obj is System.Object[])
            {
                System.Object[] objs = (System.Object[])obj;

                SerializerInt(memory, 5);
                SerializerInt(memory, objs.Length);
                for (int i = 0 ; i < objs.Length ; i ++)
                    SerializerObject(memory, objs[i]);
            } else if (obj is bool)
            {
                SerializerInt(memory, 6);
                SerializerBool(memory, (bool)obj);
            }
        }

        static void SerializerObject(UByteStream memory, object obj) 
        {
            if (obj is long || obj is int || obj is Enum || obj is short)
            {
                memory.AddB8(0);

                if (obj is short)
                {
                    memory.AddI32((short)obj);
                } else
                {
                    if (obj is Enum)
                        obj = (int)obj;

                    memory.AddI32(int.Parse(obj.ToString()));
                }
            } else if (obj is double || obj is float)
            {
                memory.AddB8(1);
                memory.AddF32(float.Parse(obj.ToString()));
            } else if (obj is string)
            {
                string value = ( string )obj;
                if (GuidMap.ContainsKey( value ))
                {
                    memory.AddB8(0);
                    memory.AddI32(GuidMap[value]);
                } else
                {
                    memory.AddB8(2);
                    memory.AddString((string)obj);
                }
            } else if (obj is byte)
            {
                memory.AddB8(3);
                memory.AddB8((byte)obj);
            }
            else if (obj is UIClassConfig)
            {
                memory.AddB8(4);
                SerializerUIClass(memory, (UIClassConfig)obj);
            } else if (obj is System.Object[])
            {
                System.Object[] objs = ( System.Object[] )obj;

                memory.AddB8(5);
                memory.AddI32(objs.Length);
                for (int i = 0 ; i < objs.Length ; i ++)
                {
                    SerializerObject(memory, objs[i]);
                }
            } else if (obj is bool)
            {
                memory.AddB8(6);
                memory.AddBool((bool)obj);
            }
        }
    }
}
