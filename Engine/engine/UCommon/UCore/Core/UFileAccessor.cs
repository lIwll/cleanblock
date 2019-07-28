using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace UEngine
{
    public class UFileAccessor
    {
        public static string[] GetFileNamesByDirectory(string path, bool recursive = false)
        {
            return Directory.GetFiles(UCoreUtil.CombinePath(USystemConfig.ResourceFolder, path), "*.*", recursive  ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        public static string[] GetFileNamesByDirectory(string path, string pattern, bool recursive = false)
        {
            return Directory.GetFiles(UCoreUtil.CombinePath(USystemConfig.ResourceFolder, path), pattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Data/TextureConfig.txt(Resoures目录下)
        /// </summary>
        public static string ReadStringFile(string fileName)
        {
            return UFileReaderProxy.ReadStringFile(UCoreUtil.CombinePath(USystemConfig.ResourceFolder, fileName).Replace('\\', '/'));
        }

        public static byte[] ReadBinaryFile(string fileName)
        {
            return UFileReaderProxy.ReadBinaryFile(UCoreUtil.CombinePath(USystemConfig.ResourceFolder, fileName).Replace('\\', '/'));
        }

        public static string ReadStringScript(string fileName)
        {
            return UFileReaderProxy.ReadStringFile(UCoreUtil.CombinePath(USystemConfig.ScriptPath, fileName).Replace('\\', '/'));
        }

        public static byte[] ReadBinaryScript(string fileName)
        {
            return UFileReaderProxy.ReadBinaryFile(UCoreUtil.CombinePath(USystemConfig.ScriptPath, fileName).Replace('\\', '/'));
        }

		public static QueryFileResult QueryFile(string fileName, out string filePath, out int offset)
		{
			return UFileReaderProxy.QueryFile(UCoreUtil.CombinePath(USystemConfig.ResourceFolder, fileName).Replace('\\', '/'), out filePath, out offset);
		}

        public static string GetPath(string path)
        {
            return UCoreUtil.CombinePath(USystemConfig.ResourceFolder, path).Replace('\\', '/');
        }

        public static bool IsFileExist(String fileName)
        {
			return UFileReaderProxy.Exists(UCoreUtil.CombinePath(USystemConfig.ResourceFolder, fileName).Replace('\\', '/'));
        }

        public static List< KeyValuePair< string, byte[] > > LoadFiles(List< KeyValuePair< string, string > > fileFullNames)
        {
            return LoadLocalFiles(fileFullNames);
        }

        private static List< KeyValuePair< string, byte[] > > LoadLocalFiles(List< KeyValuePair< string, string > > fileFullNames)
        {
            var result = new List< KeyValuePair< string, byte[] > >();
            for(int i = 0; i < fileFullNames.Count; ++i)
            {
                var item = fileFullNames[i];
                var data = UFileReaderProxy.ReadBinaryFile(UCoreUtil.CombinePath(USystemConfig.ResourceFolder, item.Value));
                if (null != data)
                    result.Add(new KeyValuePair< string, byte[] >(item.Key, data));
            }

            return result;
        }
    }
}
