using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Security;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UEngine
{
    public class UIgnoreSerializeAttribute : Attribute
    {
    }

    public class UXMLSerializer
    {
        public static bool SerializeToXML(object value, string name, string path)
        {
            SecurityElement root = new SecurityElement("root");

            if (SerializeToXML(value, name, root))
            {
                StringBuilder context = new StringBuilder();
                {
                    XmlDocument xd = new XmlDocument();
                    xd.LoadXml(root.ToString());

                    XmlTextWriter xtw = null;
                    try
                    {
                        xtw = new XmlTextWriter(new StringWriter(context));

                        xtw.Formatting = Formatting.Indented;
                        xtw.Indentation = 1;
                        xtw.IndentChar = '\t';

                        xd.WriteTo(xtw);
                    } finally
                    {
                        if (xtw != null)
                            xtw.Close();
                    }
                }

                string targetPath = System.IO.Path.GetDirectoryName(path);

                UCoreUtil.CreateFolder(targetPath);

                UXMLParser.SaveText(path, context.ToString());

                return true;
            }

            return false;
        }

        public static bool SerializeUnityNodeToXML(object value, string name, SecurityElement root)
        {
            Type stype = value.GetType();

            EVariableType type = UCore.ConvToVariableType(value);
            switch (type)
            {
                case EVariableType.eVT_Vec2:
                    {
                        Vector2 v = (Vector2)value;

                        SecurityElement node = new SecurityElement(name);
                        node.AddAttribute("type", type.ToString());
                        SecurityElement item = new SecurityElement("data");
                        item.AddAttribute("x", v.x.ToString());
                        item.AddAttribute("y", v.y.ToString());
                        node.AddChild(item);
                        root.AddChild(node);
                    }
                    return true;
                case EVariableType.eVT_Vec3:
                    {
                        Vector3 v = (Vector3)value;

                        SecurityElement node = new SecurityElement(name);
                        node.AddAttribute("type", type.ToString());
                        SecurityElement item = new SecurityElement("data");
                        item.AddAttribute("x", v.x.ToString());
                        item.AddAttribute("y", v.y.ToString());
                        item.AddAttribute("z", v.z.ToString());
                        node.AddChild(item);
                        root.AddChild(node);
                    }
                    return true;
                case EVariableType.eVT_Vec4:
                    {
                        Vector4 v = (Vector4)value;

                        SecurityElement node = new SecurityElement(name);
                        node.AddAttribute("type", type.ToString());
                        SecurityElement item = new SecurityElement("data");
                        item.AddAttribute("x", v.x.ToString());
                        item.AddAttribute("y", v.y.ToString());
                        item.AddAttribute("z", v.z.ToString());
                        item.AddAttribute("w", v.w.ToString());
                        node.AddChild(item);
                        root.AddChild(node);
                    }
                    return true;
                case EVariableType.eVT_Matrix:
                    {
                        Matrix4x4 v = (Matrix4x4)value;

                        SecurityElement node = new SecurityElement(name);
                        node.AddAttribute("type", type.ToString());
                        SecurityElement item = new SecurityElement("data");
                        for (int i = 0; i < 4; ++i)
                        {
                            for (int k = 0; k < 4; ++k)
                                item.AddAttribute("m" + i + k, v[i, k].ToString());
                        }
                        node.AddChild(item);
                        root.AddChild(node);
                    }
                    return true;
                case EVariableType.eVT_Quaternion:
                    {
                        Quaternion v = (Quaternion)value;

                        SecurityElement node = new SecurityElement(name);
                        node.AddAttribute("type", type.ToString());
                        SecurityElement item = new SecurityElement("data");
                        item.AddAttribute("x", v.x.ToString());
                        item.AddAttribute("y", v.y.ToString());
                        item.AddAttribute("z", v.z.ToString());
                        item.AddAttribute("w", v.w.ToString());
                        node.AddChild(item);
                        root.AddChild(node);
                    }
                    return true;
                case EVariableType.eVT_Color:
                    {
                        UnityEngine.Color v = (UnityEngine.Color)value;

                        SecurityElement node = new SecurityElement(name);
                        node.AddAttribute("type", type.ToString());
                        SecurityElement item = new SecurityElement("data");
                        item.AddAttribute("r", v.r.ToString());
                        item.AddAttribute("g", v.g.ToString());
                        item.AddAttribute("b", v.b.ToString());
                        item.AddAttribute("a", v.a.ToString());
                        node.AddChild(item);
                        root.AddChild(node);
                    }
                    return true;
                case EVariableType.eVT_Color32:
                    {
                        UnityEngine.Color32 v = (UnityEngine.Color32)value;

                        SecurityElement node = new SecurityElement(name);
                        node.AddAttribute("type", type.ToString());
                        SecurityElement item = new SecurityElement("data");
                        item.AddAttribute("r", v.r.ToString());
                        item.AddAttribute("g", v.g.ToString());
                        item.AddAttribute("b", v.b.ToString());
                        item.AddAttribute("a", v.a.ToString());
                        node.AddChild(item);
                        root.AddChild(node);
                    }
                    return true;
                case EVariableType.eVT_AniCurve:
                    {
                        UnityEngine.AnimationCurve v = (UnityEngine.AnimationCurve)value;

                        SecurityElement node = new SecurityElement(name);
                        node.AddAttribute("type", type.ToString());

                        SecurityElement item = new SecurityElement("data");
                        SerializeToXML(v.preWrapMode, "preWrapMode", item);
                        SerializeToXML(v.postWrapMode, "postWrapMode", item);
                        SecurityElement keys = new SecurityElement("keys");
                        for (int i = 0; i < v.keys.Length; ++i)
                            SerializeToXML(v.keys[i], "key", keys);
                        item.AddChild(keys);

                        node.AddChild(item);

                        root.AddChild(node);
                    }
                    return true;
                case EVariableType.eVT_KeyFrame:
                    {
                        UnityEngine.Keyframe v = (UnityEngine.Keyframe)value;

                        SecurityElement node = new SecurityElement(name);
                        node.AddAttribute("type", type.ToString());

                        SecurityElement item = new SecurityElement("data");
                        item.AddAttribute("inTangent", v.inTangent.ToString());
                        item.AddAttribute("outTangent", v.outTangent.ToString());
                        item.AddAttribute("tangentMode", v.tangentMode.ToString());
                        item.AddAttribute("time", v.time.ToString());
                        item.AddAttribute("value", v.value.ToString());
                        node.AddChild(item);

                        root.AddChild(node);
                    }
                    return true;
                case EVariableType.eVT_Array:
                case EVariableType.eVT_List:
                case EVariableType.eVT_Dictionary:
                case EVariableType.eVT_Custom:
                case EVariableType.eVT_Byte:
                case EVariableType.eVT_SByte:
                case EVariableType.eVT_Int16:
                case EVariableType.eVT_Int32:
                case EVariableType.eVT_Int64:
                case EVariableType.eVT_UInt16:
                case EVariableType.eVT_UInt32:
                case EVariableType.eVT_UInt64:
                case EVariableType.eVT_Boolean:
                case EVariableType.eVT_String:
                case EVariableType.eVT_Float:
                case EVariableType.eVT_Double:
                case EVariableType.eVT_Enum:
                    return SerializeToXML(value, name, root);
            }

            return false;
        }

        public static bool SerializeToXML(object value, string name, SecurityElement root)
        {
            Type stype = value.GetType();

            EVariableType type = UCore.ConvToVariableType(value);
            switch (type)
            {
                case EVariableType.eVT_Custom:
                    {
                        SecurityElement node = new SecurityElement(name);
                            node.AddAttribute("type", stype.ToString());

                        var fields = stype.GetFields(/*~BindingFlags.Static & */~BindingFlags.DeclaredOnly & ~BindingFlags.NonPublic);
                        for (int i = 0; i < fields.Length; ++ i)
                        {
							var field = fields[i];

                            if (!field.IsDefined(typeof(UIgnoreSerializeAttribute), false))
                            {
                                var v = field.GetValue(value);
                                if (null != v)
                                    SerializeToXML(v, field.Name, node);
                            }
                        }

                        var properties = stype.GetProperties(/*~BindingFlags.Static & */~BindingFlags.DeclaredOnly & ~BindingFlags.NonPublic);
                        for (int i = 0; i < properties.Length; ++ i)
                        {
							var prop = properties[i];

                            if (!prop.IsDefined(typeof(UIgnoreSerializeAttribute), false))
                            {
                                var v = prop.GetValue(value, null);
                                if (null != v)
                                    SerializeToXML(v, prop.Name, node);
                            }
                        }

                        root.AddChild(node);
                    }
                    return true;
                case EVariableType.eVT_Byte:
                case EVariableType.eVT_SByte:
                case EVariableType.eVT_Int16:
                case EVariableType.eVT_Int32:
                case EVariableType.eVT_Int64:
                case EVariableType.eVT_UInt16:
                case EVariableType.eVT_UInt32:
                case EVariableType.eVT_UInt64:
                case EVariableType.eVT_Boolean:
                case EVariableType.eVT_String:
                case EVariableType.eVT_Float:
                case EVariableType.eVT_Double:
                case EVariableType.eVT_Enum:
                    {
                        SecurityElement node = new SecurityElement(name, value.ToString());
                            node.AddAttribute("type", type.ToString());
                        root.AddChild(node);
                    }
                    return true;
                case EVariableType.eVT_Array:
                    {
                        SecurityElement node = new SecurityElement(name);
                            node.AddAttribute("type", type.ToString());

                        object length = stype.GetMethod("GetLength").Invoke(value, new object[] { 0 });
                        for (int i = 0; i < (int)length; ++ i)
                        {
                            MethodInfo mi = stype.GetMethod("GetValue", new[] { typeof(int) });
                            if (null != mi)
                                SerializeToXML(mi.Invoke(value, new object[] { i }), "element", node);
                        }

                        root.AddChild(node);
                    }
                    return true;
                case EVariableType.eVT_List:
                    {
                        SecurityElement node = new SecurityElement(name);
                            node.AddAttribute("type", type.ToString());

                        object length = stype.GetProperty("Count").GetValue(value, null);
                        for (int i = 0; i < (int)length; ++ i)
                        {
                            var v = typeof(UCoreUtil).GetMethod("ListGet").MakeGenericMethod(stype.GetGenericArguments()).Invoke(null, new object[] { value, i });

                            SerializeToXML(v, "element", node);
                        }

                        root.AddChild(node);
                    }
                    return true;
                case EVariableType.eVT_Dictionary:
                    {
                        SecurityElement node = new SecurityElement(name);
                            node.AddAttribute("type", type.ToString());

                        object length = stype.GetProperty("Count").GetValue(value, null);
                        for (int i = 0; i < (int)length; ++ i)
						{
							SecurityElement item_node = new SecurityElement("item");

							{
								var k = typeof(UCoreUtil).GetMethod("DictGetKey").MakeGenericMethod(stype.GetGenericArguments()).Invoke(null, new object[] { value, i });
								SerializeToXML(k, "key", item_node);
							}

							{
								var v = typeof(UCoreUtil).GetMethod("DictGetValue").MakeGenericMethod(stype.GetGenericArguments()).Invoke(null, new object[] { value, i });
								SerializeToXML(v, "value", item_node);
							}

							node.AddChild(item_node);
						}

                        root.AddChild(node);
                    }
                    return true;
                case EVariableType.eVT_Vec2:
                case EVariableType.eVT_Vec3:
                case EVariableType.eVT_Vec4:
                case EVariableType.eVT_Matrix:
                case EVariableType.eVT_Quaternion:
                case EVariableType.eVT_Color:
                case EVariableType.eVT_Color32:
                case EVariableType.eVT_AniCurve:
                case EVariableType.eVT_KeyFrame:
                    return SerializeUnityNodeToXML(value, name, root);
            }

            return false;
        }

        public static object SerializeFromXML(string path, Type objType = default(Type))
        {
            var root = UXMLParser.LoadOutter(path);
            if (null != root && root.Children.Count > 0)
                return SerializeFromXML(root.Children[0] as SecurityElement, objType);

            return null;
        }

		public static T SerializeFromXML< T >(string path)
		{
			var root = UXMLParser.LoadOutter(path);
			if (null != root && root.Children.Count > 0)
				return SerializeFromXML< T >(root.Children[0] as SecurityElement);

			return default(T);
		}

        public static T SerializeFromXML< T >(SecurityElement root)
        {
            object obj = SerializeFromXML(root, typeof(T));

            return (T)obj;
        }

        public static object SerializeFromXML(SecurityElement root, Type objType = default(Type))
        {
            object obj = null;

            var type = root.Attributes["type"];
            if (null != type)
            {
                Type cacheType = UTypeCache.GetType(type as string);

                if (null != cacheType)
                {
/*
                    if (cacheType.IsValueType)
                    {
                        ULogger.Error("the field obj type is ValueType please change it to Reference type");

                        return obj;
                    }
*/
                    obj = cacheType.GetConstructor(Type.EmptyTypes).Invoke(null);

                    if (null == root.Children)
                        return obj;

                    var fields = cacheType.GetFields(/*~BindingFlags.Static & */~BindingFlags.DeclaredOnly & ~BindingFlags.NonPublic);
                    var properties = cacheType.GetType().GetProperties(/*~BindingFlags.Static & */~BindingFlags.DeclaredOnly & ~BindingFlags.NonPublic);

                    Action< SecurityElement, FieldInfo > setFieldValue = (item, field) =>
                    {
                        object value = SerializeCommonObjectFromXML(field.FieldType, item);
                        if (null == value)
                        {
                            EVariableType fieldType = (item.Attributes["type"] as string).ToVariableType();
                            switch (fieldType)
                            {
                                case EVariableType.eVT_Array:
                                    {
                                        Type ftype = field.FieldType.GetElementType();

                                        if (null != item.Children)
                                        {
                                            value = Array.CreateInstance(ftype, item.Children.Count);

                                            for (int i = 0; i < item.Children.Count; ++ i)
                                                (value as Array).SetValue(SerializeCommonObjectFromXML(ftype, item.Children[i] as SecurityElement), i);
                                        }
                                    }
                                    break;
                                case EVariableType.eVT_List:
                                    {
                                        value = field.FieldType.GetConstructor(Type.EmptyTypes).Invoke(null);

                                        if (null != item.Children)
                                        {
                                            for (int i = 0; i < item.Children.Count; ++ i)
											{
												SecurityElement element = item.Children[i] as SecurityElement;

                                                field.FieldType.GetMethod("Add").Invoke(value, new object[] { SerializeCommonObjectFromXML(field.FieldType, element) });
											}
                                        }
                                    }
                                    break;
                                case EVariableType.eVT_Dictionary:
                                    {
                                        value = field.FieldType.GetConstructor(Type.EmptyTypes).Invoke(null);

                                        if (null != item.Children)
                                        {
                                            for (int i = 0; i < item.Children.Count; ++ i)
                                            {
												SecurityElement element = item.Children[i] as SecurityElement;

												// key, value
												if (element.Children.Count == 2)
												{
													var k_value = SerializeFromXML(element.Children[0] as SecurityElement);
													var v_value = SerializeFromXML(element.Children[1] as SecurityElement);

													field.FieldType.GetMethod("Add").Invoke(value, new object[] { k_value, v_value });
												}
                                            }
                                        }
                                    }
                                    break;
                            }
                        }

                        if (null != value)
                            field.SetValue(obj, value);
                    };

                    Action< SecurityElement, PropertyInfo > setPropValue = (item, prop) =>
                    {
                        object value = SerializeCommonObjectFromXML(prop.PropertyType, item);
                        if (null == value)
                        {
                            EVariableType fieldType = (item.Attributes["type"] as string).ToVariableType();
                            switch (fieldType)
                            {
                                case EVariableType.eVT_Array:
                                    {
                                        Type ptype = prop.PropertyType.GetElementType();

                                        if (null != item.Children)
                                        {
                                            value = Array.CreateInstance(ptype, item.Children.Count);

                                            for (int i = 0; i < item.Children.Count; ++ i)
                                                (value as Array).SetValue(SerializeCommonObjectFromXML(ptype, item.Children[i] as SecurityElement), i);
                                        }
                                    }
                                    break;
                                case EVariableType.eVT_List:
                                    {
                                        value = prop.PropertyType.GetConstructor(Type.EmptyTypes).Invoke(null);

                                        if (null != item.Children)
                                        {
											for (int i = 0; i < item.Children.Count; ++ i)
											{
												SecurityElement element = item.Children[i] as SecurityElement;

												prop.PropertyType.GetMethod("Add").Invoke(value, new object[] { SerializeCommonObjectFromXML(prop.PropertyType, element) });
											}
                                        }
                                    }
                                    break;
                                case EVariableType.eVT_Dictionary:
                                    {
                                        value = prop.PropertyType.GetConstructor(Type.EmptyTypes).Invoke(null);

                                        if (null != item.Children)
                                        {
											for (int i = 0; i < item.Children.Count; ++ i)
                                            {
												SecurityElement element = item.Children[i] as SecurityElement;

												// key, value
												if (element.Children.Count == 2)
												{
													var k_value = SerializeFromXML(element.Children[0] as SecurityElement);
													var v_value = SerializeFromXML(element.Children[1] as SecurityElement);

													prop.PropertyType.GetMethod("Add").Invoke(value, new object[] { k_value, v_value });
												}
                                            }
                                        }
                                    }
                                    break;
                            }
                        }

                        if (null != value)
                            prop.SetValue(obj, value, null);
                    };

                    bool found = false;

                    for (int i = 0; i < root.Children.Count; ++ i)
                    {
						SecurityElement item = root.Children[i] as SecurityElement;

                        for (int k = 0; k < fields.Length; ++ k)
                        {
							var field = fields[k];
                            try
                            {
                                if (item.Tag == field.Name)
                                {
                                    found = true;

                                    setFieldValue(item, field);

                                    break;
                                }
                            } catch (Exception e)
                            {
                                ULogger.Info("occur error while parser field, message = {0}", e.Message);
                            }
                        }

                        if (!found)
                        {
                            for (int k = 0; k < properties.Length; ++ k)
                            {
								var prop = properties[k];

                                try
                                {
                                    if (item.Tag == prop.Name)
                                    {
                                        setPropValue(item, prop);

                                        break;
                                    }
                                } catch (Exception e)
                                {
                                    ULogger.Info("occur error while parser property, message = {0}", e.Message);
                                }
                            }
                        }
                    }
                } else
                {
                    EVariableType fieldType = (root.Attributes["type"] as string).ToVariableType();
					if (UCore.IsCommonType(fieldType))
					{
						obj = SerializeCommonObjectFromXML(objType, root);
					} else if (UCore.IsCollectionType(fieldType))
					{
						if (null != objType)
						{
							switch (fieldType)
							{
								case EVariableType.eVT_Array:
									{
										Type ptype = objType.GetElementType();

										if (null != root.Children)
										{
											obj = Array.CreateInstance(ptype, root.Children.Count);

											for (int i = 0; i < root.Children.Count; ++ i)
												(obj as Array).SetValue(SerializeCommonObjectFromXML(ptype, root.Children[i] as SecurityElement), i);
										}
									}
									break;

								case EVariableType.eVT_List:
									{
										obj = objType.GetConstructor(Type.EmptyTypes).Invoke(null);

										if (null != root.Children)
										{
											for (int i = 0; i < root.Children.Count; ++ i)
												objType.GetMethod("Add").Invoke(obj, new object[] { SerializeCommonObjectFromXML(objType, root.Children[i] as SecurityElement) });
										}
									}
									break;

								case EVariableType.eVT_Dictionary:
									{
										var types = objType.GetGenericArguments();
										if (types.Length == 2)
										{
											obj = objType.GetConstructor(Type.EmptyTypes).Invoke(null);

											if (null != root.Children)
											{
												for (int i = 0; i < root.Children.Count; ++ i)
												{
													SecurityElement element = root.Children[i] as SecurityElement;

													// key, value
													if (element.Children.Count == 2)
													{
														var k_value = SerializeFromXML(element.Children[0] as SecurityElement, types[0]);
														var v_value = SerializeFromXML(element.Children[1] as SecurityElement, types[1]);

														objType.GetMethod("Add").Invoke(obj, new object[] { k_value, v_value });
													}
												}
											}
										}
									}
									break;
							}
						}
					}
                }
            }

            return obj;
        }

/*
        public static Color RGB(uint color)
        {
            uint r = 0xFF & color;
            uint g = 0xFF00 & color;
            g >>= 8;
            uint b = 0xFF0000 & color;
            b >>= 16;
            uint a = 0xFF000000 & color;
            a >>= 24;

            return new Color32((byte)r, (byte)g, (byte)b, (byte)a);
        }
*/

        public static object SerializeUnityObjectFromXML(Type objType, SecurityElement root)
        {
            object obj = null;

            EVariableType fieldType = (root.Attributes["type"] as string).ToVariableType();
            switch (fieldType)
            {
                case EVariableType.eVT_Vec2:
                    {
                        SecurityElement data = root.SearchForChildByTag("data");
                        if (null != data)
                        {
                            UnityEngine.Vector2 v = new UnityEngine.Vector2();

                            v.x = float.Parse(data.Attributes["x"] as string);
                            v.y = float.Parse(data.Attributes["y"] as string);

                            obj = v;
                        }
                    }
                    break;
                case EVariableType.eVT_Vec3:
                    {
                        SecurityElement data = root.SearchForChildByTag("data");
                        if (null != data)
                        {
                            UnityEngine.Vector3 v = new UnityEngine.Vector3();

                            v.x = float.Parse(data.Attributes["x"] as string);
                            v.y = float.Parse(data.Attributes["y"] as string);
                            v.z = float.Parse(data.Attributes["z"] as string);

                            obj = v;
                        }
                    }
                    break;
                case EVariableType.eVT_Vec4:
                    {
                        SecurityElement data = root.SearchForChildByTag("data");
                        if (null != data)
                        {
                            UnityEngine.Vector4 v = new UnityEngine.Vector4();

                            v.x = float.Parse(data.Attributes["x"] as string);
                            v.y = float.Parse(data.Attributes["y"] as string);
                            v.z = float.Parse(data.Attributes["z"] as string);
                            v.w = float.Parse(data.Attributes["w"] as string);

                            obj = v;
                        }
                    }
                    break;
                case EVariableType.eVT_Matrix:
                    {
                        SecurityElement data = root.SearchForChildByTag("data");
                        if (null != data)
                        {
                            UnityEngine.Matrix4x4 v = new UnityEngine.Matrix4x4();

                            for (int i = 0; i < 4; ++ i)
                            {
                                for (int k = 0; k < 4; ++ k)
                                    v[i, k] = float.Parse(data.Attributes["m" + i + k] as string);
                            }

                            obj = v;
                        }
                    }
                    break;
                case EVariableType.eVT_Quaternion:
                    {
                        SecurityElement data = root.SearchForChildByTag("data");
                        if (null != data)
                        {
                            UnityEngine.Quaternion v = new UnityEngine.Quaternion();

                            v.x = float.Parse(data.Attributes["x"] as string);
                            v.y = float.Parse(data.Attributes["y"] as string);
                            v.z = float.Parse(data.Attributes["z"] as string);
                            v.w = float.Parse(data.Attributes["w"] as string);

                            obj = v;
                        }
                    }
                    break;
                case EVariableType.eVT_Color:
                    {
                        SecurityElement data = root.SearchForChildByTag("data");
                        if (null != data)
                        {
                            UnityEngine.Color v = new UnityEngine.Color();

                            v.r = float.Parse(data.Attributes["r"] as string);
                            v.g = float.Parse(data.Attributes["g"] as string);
                            v.b = float.Parse(data.Attributes["b"] as string);
                            v.a = float.Parse(data.Attributes["a"] as string);

                            obj = v;
                        }
                    }
                    break;
                case EVariableType.eVT_Color32:
                    {
                        SecurityElement data = root.SearchForChildByTag("data");
                        if (null != data)
                        {
                            UnityEngine.Color32 v = new UnityEngine.Color32();

                            v.r = byte.Parse(data.Attributes["r"] as string);
                            v.g = byte.Parse(data.Attributes["g"] as string);
                            v.b = byte.Parse(data.Attributes["b"] as string);
                            v.a = byte.Parse(data.Attributes["a"] as string);

                            obj = v;
                        }
                    }
                    break;
                case EVariableType.eVT_AniCurve:
                    {
                        SecurityElement data = root.SearchForChildByTag("data");
                        if (null != data)
                        {
                            UnityEngine.AnimationCurve v = new UnityEngine.AnimationCurve();
                            v.preWrapMode	= SerializeFromXML< UnityEngine.WrapMode >(data.SearchForChildByTag("preWrapMode"));
                            v.postWrapMode	= SerializeFromXML< UnityEngine.WrapMode >(data.SearchForChildByTag("postWrapMode"));

                            SecurityElement keys = data.SearchForChildByTag("keys");
                            if (null != keys.Children)
                            {
								for (int i = 0; i < keys.Children.Count; ++ i)
								{
									SecurityElement key = keys.Children[i] as SecurityElement;

									v.AddKey((UnityEngine.Keyframe)SerializeFromXML(key));
								}
                            }

                            obj = v;
                        }
                    }
                    break;
                case EVariableType.eVT_KeyFrame:
                    {
                        SecurityElement data = root.SearchForChildByTag("data");
                        if (null != data)
                        {
                            UnityEngine.Keyframe v = new UnityEngine.Keyframe();

                            v.inTangent     = float.Parse(data.Attribute("inTangent"));
                            v.outTangent    = float.Parse(data.Attribute("outTangent"));
                            v.tangentMode   = int.Parse(data.Attribute("tangentMode"));
                            v.time          = float.Parse(data.Attribute("time"));
                            v.value         = float.Parse(data.Attribute("value"));

                            obj = v;
                        }
                    }
                    break;
                case EVariableType.eVT_Custom:
                case EVariableType.eVT_Byte:
                case EVariableType.eVT_SByte:
                case EVariableType.eVT_Int16:
                case EVariableType.eVT_Int32:
                case EVariableType.eVT_Int64:
                case EVariableType.eVT_UInt16:
                case EVariableType.eVT_UInt32:
                case EVariableType.eVT_UInt64:
                case EVariableType.eVT_Boolean:
                case EVariableType.eVT_String:
                case EVariableType.eVT_Float:
                case EVariableType.eVT_Double:
                case EVariableType.eVT_Enum:
                    return SerializeCommonObjectFromXML(objType, root);
            }

            return obj;
        }

        public static object SerializeCommonObjectFromXML(Type objType, SecurityElement root)
        {
            object obj = null;

            EVariableType fieldType = (root.Attributes["type"] as string).ToVariableType();
            switch (fieldType)
            {
                case EVariableType.eVT_Custom:
                    {
                        obj = SerializeFromXML(root);
                    }
                    break;

                case EVariableType.eVT_Byte:
                    {
                        obj = byte.Parse(root.Text);
                    }
                    break;
                case EVariableType.eVT_SByte:
                    {
                        obj = sbyte.Parse(root.Text);
                    }
                    break;
                case EVariableType.eVT_Int16:
                    {
                        obj = short.Parse(root.Text);
                    }
                    break;
                case EVariableType.eVT_Int32:
                    {
                        obj = int.Parse(root.Text);
                    }
                    break;
                case EVariableType.eVT_Int64:
                    {
                        obj = long.Parse(root.Text);
                    }
                    break;
                case EVariableType.eVT_UInt16:
                    {
                        obj = ushort.Parse(root.Text);
                    }
                    break;
                case EVariableType.eVT_UInt32:
                    {
                        obj = uint.Parse(root.Text);
                    }
                    break;
                case EVariableType.eVT_UInt64:
                    {
                        obj = ulong.Parse(root.Text);
                    }
                    break;
                case EVariableType.eVT_Boolean:
                    {
                        obj = bool.Parse(root.Text);
                    }
                    break;
                case EVariableType.eVT_String:
                    {
                        obj = root.Text;
                    }
                    break;
                case EVariableType.eVT_Float:
                    {
                        obj = float.Parse(root.Text);
                    }
                    break;
                case EVariableType.eVT_Double:
                    {
                        obj = double.Parse(root.Text);
                    }
                    break;
                case EVariableType.eVT_Enum:
                    {
                        obj = Enum.Parse(objType, root.Text);
                    }
                    break;
                case EVariableType.eVT_Vec2:
                case EVariableType.eVT_Vec3:
                case EVariableType.eVT_Vec4:
                case EVariableType.eVT_Matrix:
                case EVariableType.eVT_Quaternion:
                case EVariableType.eVT_Color:
                case EVariableType.eVT_Color32:
                case EVariableType.eVT_AniCurve:
                case EVariableType.eVT_KeyFrame:
                    return SerializeUnityObjectFromXML(objType, root);
            }

            return obj;
        }
    }
}
