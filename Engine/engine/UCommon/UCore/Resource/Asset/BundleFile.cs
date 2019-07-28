/*
Copyright (c) 2016 Radu

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

/*
DISCLAIMER
The reposiotory, code and tools provided herein are for educational purposes only.
The information not meant to change or impact the original code, product or service.
Use of this repository, code or tools does not exempt the user from any EULA, ToS or any other legal agreements that have been agreed with other parties.
The user of this repository, code and tools is responsible for his own actions.

Any forks, clones or copies of this repository are the responsability of their respective authors and users.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lz4;
using SevenZip.Compression.LZMA;
using UnityEngine;

namespace AssetStudio
{
    public class BundleFile
    {
        public int ver1;
        public string ver2;
        public string ver3;
        public int format;
        public string versionPlayer;
        public string versionEngine;
        public List<MemoryAssetsFile> MemoryAssetsFileList = new List<MemoryAssetsFile>();

        public List<byte[]> m_FileBytes = new List<byte[]>();
        public string filepath = "";
        public EndianStream fileES = null;

        public class MemoryAssetsFile
        {
            public string fileName;
            public MemoryStream memStream;
        }

        public BundleFile(string fileName)
        {
            filepath = fileName;
            if (Path.GetExtension(fileName) == ".lz4")
            {
                byte[] filebuffer;

                using (BinaryReader lz4Stream = new BinaryReader(File.OpenRead(fileName)))
                {
                    int version = lz4Stream.ReadInt32();
                    int uncompressedSize = lz4Stream.ReadInt32();
                    int compressedSize = lz4Stream.ReadInt32();
                    int something = lz4Stream.ReadInt32(); //1

                    byte[] lz4buffer = new byte[compressedSize];
                    lz4Stream.Read(lz4buffer, 0, compressedSize);

                    using (var inputStream = new MemoryStream(lz4buffer))
                    {
                        var decoder = new Lz4DecoderStream(inputStream);

                        filebuffer = new byte[uncompressedSize]; //is this ok?
                        for (;;)
                        {
                            int nRead = decoder.Read(filebuffer, 0, uncompressedSize);
                            if (nRead == 0)
                                break;
                        }
                    }
                }

                using (var b_Stream = new EndianStream(new MemoryStream(filebuffer), EndianType.BigEndian))
                {
                    readBundle(b_Stream);
                }
            }
            else
            {
                var bytes = UEngine.UFileAccessor.ReadBinaryFile(fileName);
				if (null != bytes)
				{
					System.IO.MemoryStream stream = new MemoryStream(bytes);
					using (var b_Stream = new EndianStream(stream, EndianType.BigEndian))
					{
						readBundle(b_Stream);
					}
				}
            }
        }

        private void readBundle(EndianStream b_Stream)
        {
            fileES = b_Stream;
            //UnityEngine.Debug.LogError("readBundle");
            var header = b_Stream.ReadStringToNull();
            //UnityEngine.Debug.LogError(header);
            m_FileBytes.Clear();

            if (header == "UnityWeb" || header == "UnityRaw" || header == "\xFA\xFA\xFA\xFA\xFA\xFA\xFA\xFA")
            {
                ver1 = b_Stream.ReadInt32();
                ver2 = b_Stream.ReadStringToNull();
                ver3 = b_Stream.ReadStringToNull();
                //UnityEngine.Debug.LogError(string.Format("ver1:{0}     ver2:{1}     ver3:{2}", ver1, ver2, ver3));
                if (ver1 < 6) { int bundleSize = b_Stream.ReadInt32(); }
                else
                {
                    long bundleSize = b_Stream.ReadInt64();
                    //UnityEngine.Debug.LogError("Retun");
                    return;
                }
                short dummy2 = b_Stream.ReadInt16();
                int offset = b_Stream.ReadInt16();
                int dummy3 = b_Stream.ReadInt32();
                int lzmaChunks = b_Stream.ReadInt32();

                //UnityEngine.Debug.LogError(string.Format("dummy2:{0}     offset:{1}     dummy3:{2}     lzmaChunks:{3}", dummy2, offset, dummy3, lzmaChunks));
                int lzmaSize = 0;
                long streamSize = 0;

                for (int i = 0; i < lzmaChunks; i++)
                {
                    lzmaSize = b_Stream.ReadInt32();
                    streamSize = b_Stream.ReadInt32();
                }

                b_Stream.Position = offset;
                switch (header)
                {
                    case "\xFA\xFA\xFA\xFA\xFA\xFA\xFA\xFA": //.bytes
                    case "UnityWeb":
                        {
                            byte[] lzmaBuffer = new byte[lzmaSize];
                            b_Stream.Read(lzmaBuffer, 0, lzmaSize);

                            //UnityEngine.Debug.LogError(lzmaBuffer.Length);
                            using (var lzmaStream = new EndianStream(SevenZip.Compression.LZMA.SevenZipHelper.StreamDecompress(new MemoryStream(lzmaBuffer)), EndianType.BigEndian))
                            {
                                //UnityEngine.Debug.LogError("getFiles");
                                getFiles(lzmaStream, 0);
                            }
                            break;
                        }
                    case "UnityRaw":
                        {
                            getFiles(b_Stream, offset);
                            break;
                        }
                }


            }
            else if (header == "UnityFS")
            {
                //ver1 = b_Stream.ReadInt32();
                //ver2 = b_Stream.ReadStringToNull();
                //ver3 = b_Stream.ReadStringToNull();
                //long bundleSize = b_Stream.ReadInt64();

                format = b_Stream.ReadInt32();
                versionPlayer = b_Stream.ReadStringToNull();
                versionEngine = b_Stream.ReadStringToNull();

                //Debug.LogError(string.Format("format:{0}    versionPlayer:{1}    versionEngine:{2}", format, versionPlayer, versionEngine));
                //if (format == 6)
                //{
                    ReadFormat6(b_Stream);
                //}
            }
        }
        private void ReadFormat6(EndianStream bundleReader, bool padding = false)
        {
            var bundleSize = bundleReader.ReadInt64();
            int compressedSize = bundleReader.ReadInt32();
            int uncompressedSize = bundleReader.ReadInt32();
            int flag = bundleReader.ReadInt32();
            if (padding)
                bundleReader.ReadByte();
            byte[] blocksInfoBytes;
            //Debug.LogError(string.Format("bundleSize:{0}     compressedSize:{1}     uncompressedSize:{2}     flag:{3}", bundleSize, compressedSize, uncompressedSize, flag));
            if ((flag & 0x80) != 0)//at end of file
            {
                var position = bundleReader.Position;
                bundleReader.Position = bundleReader.BaseStream.Length - compressedSize;
                blocksInfoBytes = bundleReader.ReadBytes(compressedSize);
                bundleReader.Position = position;
                ////Debug.LogError(string.Format("blocksInfoBytes:{0}", blocksInfoBytes.Length));
            }
            else
            {
                blocksInfoBytes = bundleReader.ReadBytes(compressedSize);
                ////Debug.LogError(string.Format("blocksInfoBytes2:{0}", blocksInfoBytes.Length));
            }

            MemoryStream blocksInfoStream;
            switch (flag & 0x3F)
            {
                default://None
                    {
                        blocksInfoStream = new MemoryStream(blocksInfoBytes);
                        break;
                    }
                case 1://LZMA
                    {
                        blocksInfoStream = SevenZip.Compression.LZMA.SevenZipHelper.StreamDecompress(new MemoryStream(blocksInfoBytes));
                        break;
                    }
                case 2://LZ4
                case 3://LZ4HC
                    {
                        byte[] uncompressedBytes = new byte[uncompressedSize];
                        using (var decoder = new Lz4DecoderStream(new MemoryStream(blocksInfoBytes)))
                        {
                            decoder.Read(uncompressedBytes, 0, uncompressedSize);
                        }
                        blocksInfoStream = new MemoryStream(uncompressedBytes);
                        break;
                    }
                //case 4:LZHAM?
            }
            using (var blocksInfo = new EndianBinaryReader(blocksInfoStream))
            {
                blocksInfo.Position = 0x10;
                int blockcount = blocksInfo.ReadInt32();
                var assetsDataStream = new MemoryStream();

                //Debug.LogError(string.Format("blockcount:{0}", blockcount));
                for (int i = 0; i < blockcount; i++)
                {
                    uncompressedSize = blocksInfo.ReadInt32();
                    compressedSize = blocksInfo.ReadInt32();
                    flag = blocksInfo.ReadInt16();

                    //Debug.LogError(string.Format("uncompressedSize:{0}     compressedSize:{1}     flag:{2}", uncompressedSize, compressedSize, flag));

                    var compressedBytes = bundleReader.ReadBytes(compressedSize);
                    switch (flag & 0x3F)
                    {
                        default://None
                            {
                                assetsDataStream.Write(compressedBytes, 0, compressedSize);
                                break;
                            }
                        case 1://LZMA
                            {
                                var uncompressedBytes = new byte[uncompressedSize];
                                using (var mstream = new MemoryStream(compressedBytes))
                                {
									var decoder = SevenZipHelper.StreamDecompress(mstream, uncompressedSize);//uncompressedSize
                                    decoder.Read(uncompressedBytes, 0, uncompressedSize);
                                    decoder.Dispose();
                                }
                                assetsDataStream.Write(uncompressedBytes, 0, uncompressedSize);
                                break;
                            }
                        case 2://LZ4
                        case 3://LZ4HC
                            {
                                var uncompressedBytes = new byte[uncompressedSize];
                                using (var decoder = new Lz4DecoderStream(new MemoryStream(compressedBytes)))
                                {
                                    decoder.Read(uncompressedBytes, 0, uncompressedSize);
                                }
                                assetsDataStream.Write(uncompressedBytes, 0, uncompressedSize);
                                break;
                            }
                        //case 4:LZHAM?
                    }
                }
                using (var assetsDataReader = new EndianBinaryReader(assetsDataStream))
                {
                    var entryinfo_count = blocksInfo.ReadInt32();
                    //Debug.LogError(string.Format("entryinfo_count:{0}", entryinfo_count));

                    for (int i = 0; i < entryinfo_count; i++)
                    {
                        var file = new MemoryAssetsFile();
                        var entryinfo_offset = blocksInfo.ReadInt64();
                        var entryinfo_size = blocksInfo.ReadInt64();
                        flag = blocksInfo.ReadInt32();

                        //Debug.LogError(string.Format("entryinfo_offset:{0}     entryinfo_size:{1}     flag:{2}", entryinfo_offset, entryinfo_size, flag));

                        file.fileName = Path.GetFileName(blocksInfo.ReadStringToNull());
                        //Debug.LogError(string.Format("name:{0}", file.fileName));
                        assetsDataReader.Position = entryinfo_offset;
                        var buffer = assetsDataReader.ReadBytes((int)entryinfo_size);

                        m_FileBytes.Add(buffer);

                        file.memStream = new MemoryStream(buffer);
                        MemoryAssetsFileList.Add(file);

                        //Debug.LogError(string.Format("buffer:{0}", buffer.Length));
                    }
                } 
            }
        }
        private void getFiles(EndianStream f_Stream, int offset)
        {
            int fileCount = f_Stream.ReadInt32();
            //UnityEngine.Debug.LogError(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                MemoryAssetsFile memFile = new MemoryAssetsFile();
                memFile.fileName = f_Stream.ReadStringToNull();
                int fileOffset = f_Stream.ReadInt32();
                fileOffset += offset;
                int fileSize = f_Stream.ReadInt32();
                long nextFile = f_Stream.Position;
                //UnityEngine.Debug.LogError(string.Format("fileOffset:{0}", fileOffset));
                f_Stream.Position = fileOffset;

                byte[] buffer = new byte[fileSize];
                f_Stream.Read(buffer, 0, fileSize);

                //Debug.LogError("buffer.Length ： " + buffer.Length);
                m_FileBytes.Add(buffer);

                memFile.memStream = new MemoryStream(buffer);
                MemoryAssetsFileList.Add(memFile);
                f_Stream.Position = nextFile;

            }
        }

        public List<byte[]> GetByte()
        {
            List<AssetsFile> assetsfileList = new List<AssetsFile>();
            List<unityFont> fonts = new List<unityFont>();

            for(int i = 0; i <this.MemoryAssetsFileList.Count;++i)
            {
                var file = this.MemoryAssetsFileList[i];
                var assetsFile = new AssetsFile(Path.GetDirectoryName(filepath) + "\\" + file.fileName, new EndianStream(file.memStream, EndianType.BigEndian));

                if (assetsFile.fileGen == 6) //2.6.x and earlier don't have a string version before the preload table
                {
                    //make use of the bundle file version
                    assetsFile.m_Version = versionEngine;
                    assetsFile.version = System.Text.RegularExpressions.Regex.Matches(versionEngine, @"\d").Cast<System.Text.RegularExpressions.Match>().Select(m => int.Parse(m.Value)).ToArray();
                    assetsFile.buildType = System.Text.RegularExpressions.Regex.Replace(versionEngine, @"\d", "").Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                }
                assetsfileList.Add(assetsFile);

                Dictionary<long, AssetPreloadData>.Enumerator it = assetsFile.preloadTable.GetEnumerator();
                while(it.MoveNext())
                {
                    var asset = it.Current.Value;
                    if (asset.Type == ClassIDReference.Font)
                    {
                        unityFont m_Font = new unityFont(asset, true);
                        fonts.Add(m_Font);
                    }
                }
            }

            List<byte[]> resultList = new List<byte[]>();
            if (fonts != null)
            {
                for (int i = 0; i < fonts.Count; i++)
                {
                    if (fonts[i] != null)
                    {
                        resultList.Add(fonts[i].m_FontData);
                    }
                }
            }
            return resultList;
        }
    }
}
