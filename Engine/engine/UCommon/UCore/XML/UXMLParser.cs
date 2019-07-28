using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Security;
using System.Collections.Generic;

namespace UEngine
{
    public class UXMLParser
    {
        public Dictionary< string, Dictionary< string, string > > LoadMap(string fileName, out string key)
        {
            key = Path.GetFileNameWithoutExtension(fileName);

            var xml = Load(fileName);

            return LoadMap(xml);
        }

        public bool LoadMap(string fileName, out Dictionary< string, Dictionary< string, string > > map)
        {
            try
            {
                var xml = Load(fileName);

                map = LoadMap(xml);

                return true;
            } catch (Exception)
            {
                map = null;

                return false;
            }
        }

        public static bool LoadIntMap(string fileName, bool isForceOutterRecoure, out Dictionary< int, Dictionary< string, string > > map)
        {
            try
            {
                SecurityElement xml;
                if (isForceOutterRecoure)
                    xml = LoadOutter(fileName);
                else
                    xml = Load(fileName);
                if (xml == null)
                {
                    ULogger.Error("File not exist: {0}", fileName);

                    map = null;

                    return false;
                } else
                {
                    map = LoadIntMap(xml, fileName);

                    return true;
                }
            } catch (Exception ex)
            {
                ULogger.Error("Load Int Map Error: {0} {1}", fileName, ex.Message);

                map = null;

                return false;
            }
        }

        public static Dictionary< int, Dictionary< string, string > > LoadIntMap(SecurityElement xml, string source)
        {
            var result = new Dictionary< int, Dictionary< string, string > >();

            var index = 0;
            for (int i = 0; i < xml.Children.Count; ++ i)
            {
				SecurityElement subMap = xml.Children[i] as SecurityElement;

                index ++;
                if (subMap.Children == null || subMap.Children.Count == 0)
                {
                    ULogger.Warn("empty row in row NO.{0} of {1}", index, source);

                    continue;
                }

                int key = int.Parse((subMap.Children[0] as SecurityElement).Text);
                if (result.ContainsKey(key))
                {
                    ULogger.Warn("Key {0} already exist, in {1}.", key, source);

                    continue;
                }

                var children = new Dictionary< string, string >();
                result.Add(key, children);
                for (int k = 1; k < subMap.Children.Count; k ++)
                {
                    var node = subMap.Children[k] as SecurityElement;

                    string tag;
                    if (node.Tag.Length < 3)
                    {
                        tag = node.Tag;
                    } else
                    {
                        var tagTial = node.Tag.Substring(node.Tag.Length - 2, 2);
                        if (tagTial == "_i" || tagTial == "_s" || tagTial == "_f" || tagTial == "_l" || tagTial == "_k" || tagTial == "_m")
                            tag = node.Tag.Substring(0, node.Tag.Length - 2);
                        else
                            tag = node.Tag;
                    }

                    if (node != null && !children.ContainsKey(tag))
                    {
                        if (String.IsNullOrEmpty(node.Text))
                            children.Add(tag, "");
                        else
                            children.Add(tag, node.Text.Trim());
                    } else
                    {
                        ULogger.Warn("Key {0} already exist, index {1} of {2}.", node.Tag, i, node.ToString());
                    }
                }
            }

            return result;
        }

        public static Dictionary< string, Dictionary< string, string > > LoadMap(SecurityElement xml)
        {
            var result = new Dictionary< string, Dictionary< string, string > >();

            for (int i = 0; i < xml.Children.Count; ++ i)
            {
				SecurityElement subMap = xml.Children[i] as SecurityElement;

                string key = (subMap.Children[0] as SecurityElement).Text.Trim();
                if (result.ContainsKey(key))
                {
                    ULogger.Warn("Key {0} already exist, in {1}.", key, xml.ToString());

                    continue;
                }

                var children = new Dictionary< string, string >();
                result.Add(key, children);
                for (int k = 1; k < subMap.Children.Count; k ++)
                {
                    var node = subMap.Children[k] as SecurityElement;
                    if (node != null && !children.ContainsKey(node.Tag))
                    {
                        if (String.IsNullOrEmpty(node.Text))
                            children.Add(node.Tag, "");
                        else
                            children.Add(node.Tag, node.Text.Trim());
                    } else
                    {
                        ULogger.Warn("Key {0} already exist, index {1} of {2}.", node.Tag, i, node.ToString());
                    }
                }
            }

            return result;
        }

        public static string LoadText(string fileName)
        {
            try
            {
                return USystemConfig.Instance.IsDevelopMode ? UFileReaderProxy.ReadStringRes(fileName) : UFileAccessor.ReadStringFile(fileName);
            } catch (Exception)
            {
                return "";
            }
        }

        public static byte[] LoadBytes(String fileName)
        {
            return USystemConfig.Instance.IsDevelopMode ? UFileReaderProxy.ReadBinaryRes(fileName) : UFileAccessor.ReadBinaryFile(fileName);
        }

        public static SecurityElement Load(string fileName)
        {
            string xmlText = LoadText(fileName);
            if (string.IsNullOrEmpty(xmlText))
                return null;

            return LoadXML(xmlText);
        }

        public static SecurityElement LoadOutter(string fileName)
        {
            string xmlText = UFileReaderProxy.ReadStringFile(fileName.Replace('\\', '/'));
            if (string.IsNullOrEmpty(xmlText))
                return null;

            return LoadXML(xmlText);
        }

        public static SecurityElement LoadXML(string xml)
        {
            try
            {
                USecurityParser securityParser = new USecurityParser();

                securityParser.LoadXml(xml);

                return securityParser.ToXml();
            } catch (Exception e)
            {
                ULogger.Warn("parser xml error {0}.", e);

                return null;
            }
        }

        public static void SaveBytes(string fileName, byte[] buffer)
        {
            UCoreUtil.CreateFolder(UCoreUtil.GetDirectoryName(fileName));

            UCoreUtil.DeleteFile(fileName);

            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                using (BinaryWriter sw = new BinaryWriter(fs))
                {
                    sw.Write(buffer);

                    sw.Flush();

                    sw.Close();
                }

                fs.Close();
            }
        }

        public static void SaveText(string fileName, string text)
        {
            UCoreUtil.CreateFolder(UCoreUtil.GetDirectoryName(fileName));

            UCoreUtil.DeleteFile(fileName);

            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(Format(text));

                    sw.Flush();

                    sw.Close();
                }

                fs.Close();
            }
        }

        public static string Format(string text)
        {
            StringBuilder context = new StringBuilder();
            {
                XmlDocument xd = new XmlDocument();
                    xd.LoadXml(text);

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

            return context.ToString();
        }
    }
}
