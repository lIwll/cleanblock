using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Security;
using System.IO;

namespace UEngine
{
    public class LZMA
    {
#if !UNITY_EDITOR && UNITY_IPHONE
		const string LZMADLL = "__Internal";
#else
        const string LZMADLL = "lzma";
#endif
        [DllImport(LZMADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int LzmaDecJpzOrCopyKpk(string srcFilePath, int srcOffset, int srcLength, string distFilePath);

        [DllImport(LZMADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int LzmaDecompressJpz(string srcFilePath, int srcOffset, int srcLength, string distFilePath);

        [DllImport(LZMADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int LzmaCompressJpz(string srcFilePath, string dstFilePath);

        [DllImport(LZMADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetLzmaDecompressJpzTotalSize();

        [DllImport(LZMADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetLzmaDecompressJpzCurrentSize();
    }
}
