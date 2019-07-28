using System;
using System.IO;

using UnityEngine;

namespace UEngine
{
	public enum QueryFileResult
	{
		eOK,        // OK
		eEmpty,     // empty file
		eFailed,    // bad file
	}

    public delegate byte[] delegate_ReadBinary(string path);
    public delegate string delegate_ReadString(string path);
    public delegate bool delegate_Exists(string path);
    public delegate QueryFileResult delegate_QueryFile(string path, out string realPath, out int offset);

    public static class UFileReaderProxy
    {
        private static delegate_ReadBinary HandlerReadBinaryRes;
        private static delegate_ReadString HandlerReadStringRes;

        private static delegate_ReadBinary HandlerReadBinaryFile;
        private static delegate_ReadString HandlerReadStringFile;

		private static delegate_Exists HandlerExists;

		private static delegate_QueryFile HandlerQueryFile;

        public static MemoryStream ReadFileAsMemoryStream(string filePath)
        {
            try
            {
                byte[] buffer = ReadBinaryFile(filePath);
                if (buffer == null)
                {
                    ULogger.Debug("ReadFileAsMemoryStream failed:{0}\n", filePath);

                    return null;
                }

                return new MemoryStream(buffer);
            } catch (Exception e)
            {
                ULogger.Debug("Exception:{0}\n", e.Message);
                ULogger.CallStack();

                return null;
            }
        }

        public static byte[] ReadBinaryRes(string filePath)
        {
            byte[] buffer = null;
            try
            {
                if (HandlerReadBinaryRes != null)
                    buffer = HandlerReadBinaryRes(filePath);
                else
                    buffer = LoadByteResource(filePath);
            } catch (Exception e)
            {
                ULogger.Debug("Exception:{0}\n", e.Message);
                ULogger.CallStack();

                return null;
            }

            return buffer;
        }

        public static string ReadStringRes(string filePath)
        {
            string buffer = string.Empty;
            try
            {
                if (HandlerReadStringRes != null)
                    buffer = HandlerReadStringRes(filePath);
                else
                    buffer = LoadResource(filePath);
            } catch (Exception e)
            {
                ULogger.Debug("Exception:{0}\n", e.Message);
                ULogger.CallStack();

                return null;
            }

            return buffer;
        }

        public static byte[] ReadBinaryFile(string filePath)
        {
            byte[] buffer = null;
            try
            {
                if (HandlerReadBinaryFile != null)
                    buffer = HandlerReadBinaryFile(filePath);
                else
                    buffer = LoadByteFile(filePath);
            } catch (Exception e)
            {
                ULogger.Debug("Exception:{0}\n", e.Message);
                ULogger.CallStack();

                return null;
            }

            return buffer;
        }

        public static string ReadStringFile(string filePath)
        {
            string buffer = string.Empty;
            try
            {
                if (HandlerReadStringFile != null)
                    buffer = HandlerReadStringFile(filePath);
                else
                    buffer = LoadFile(filePath);
            } catch (Exception e)
            {
                ULogger.Debug("Exception:{0}\n", e.Message);
                ULogger.CallStack();

                return null;
            }

            return buffer;
        }

		public static QueryFileResult QueryFile(string filePath, out string realPath, out int offset)
		{
			if (null != HandlerQueryFile)
				return HandlerQueryFile(filePath, out realPath, out offset);

			realPath	= filePath;
			offset		= 0;

			if (Exists(filePath))
				return QueryFileResult.eOK;

			return QueryFileResult.eFailed;
		}

        public static bool Exists(string filePath)
        {
			if (null != HandlerExists)
				return HandlerExists(filePath);

            return File.Exists(filePath);
        }

        public static void RegisterReadResHandler(delegate_Exists exists, delegate_ReadBinary readBinary, delegate_ReadString readString)
        {
			HandlerExists = exists;
            HandlerReadBinaryRes = readBinary;
            HandlerReadStringRes = readString;
        }

		public static void RegisterReadFileHandler(delegate_Exists exists, delegate_ReadBinary readBinary, delegate_ReadString readString, delegate_QueryFile queryFile)
        {
			HandlerExists = exists;
			HandlerQueryFile = queryFile;
            HandlerReadBinaryFile = readBinary;
            HandlerReadStringFile = readString;
        }

        // default reader
        private static string LoadFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                using (StreamReader sr = File.OpenText(fileName))
                    return sr.ReadToEnd();
            }

            return string.Empty;
        }

        private static byte[] LoadByteFile(string fileName)
        {
            if (File.Exists(fileName))
                return File.ReadAllBytes(fileName);

            return null;
        }

        private static string LoadResource(string fileName)
        {
            var text = Resources.Load(fileName);
            if (text != null)
            {
                var result = text.ToString();

                Resources.UnloadAsset(text);

                return result;
            }
            else
            {
                ULogger.Error("LoadResource: Can not find " + fileName);
            }

            return string.Empty;
        }

        private static byte[] LoadByteResource(string fileName)
        {
            TextAsset binAsset = Resources.Load(fileName, typeof(TextAsset)) as TextAsset;
            if (binAsset)
            {
                var result = binAsset.bytes;

                Resources.UnloadAsset(binAsset);

                return result;
            }

            return null;
        }
    }
}
