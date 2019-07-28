﻿/*
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

namespace AssetStudio
{
    class TextAsset
    {
        public string m_Name;
        public byte[] m_Script;
        public string m_PathName;

        public TextAsset(AssetPreloadData preloadData, bool readSwitch)
        {
            var sourceFile = preloadData.sourceFile;
            var a_Stream = preloadData.sourceFile.reader;
            a_Stream.Position = preloadData.Offset;
            preloadData.extension = ".txt";

            if (sourceFile.platform == -2)
            {
                uint m_ObjectHideFlags = a_Stream.ReadUInt32();
                PPtr m_PrefabParentObject = sourceFile.ReadPPtr();
                PPtr m_PrefabInternal = sourceFile.ReadPPtr();
            }

            m_Name = a_Stream.ReadAlignedString(a_Stream.ReadInt32());

            int m_Script_size = a_Stream.ReadInt32();

            if (readSwitch) //asset is read for preview or export
            {
                m_Script = new byte[m_Script_size];
                a_Stream.Read(m_Script, 0, m_Script_size);

                if (m_Script[0] == 93) { m_Script = SevenZip.Compression.LZMA.SevenZipHelper.Decompress(m_Script); }
                if (m_Script[0] == 60 || (m_Script[0] == 239 && m_Script[1] == 187 && m_Script[2] == 191 && m_Script[3] == 60)) { preloadData.extension = ".xml"; }
            }
            else
            {
                byte lzmaTest = a_Stream.ReadByte();
                if (lzmaTest == 93)
                {
                    a_Stream.Position += 4;
                    //preloadData.exportSize = a_Stream.ReadInt32(); //actualy int64
                    a_Stream.Position -= 8;
                }
                //else { preloadData.exportSize = m_Script_size; }

                a_Stream.Position += m_Script_size - 1;

                //if (m_Name != "") { preloadData.Text = m_Name; }
                //else { preloadData.Text = preloadData.TypeString + " #" + preloadData.uniqueID; }
                //preloadData.SubItems.AddRange(new string[] { preloadData.TypeString, preloadData.exportSize.ToString() });
            }
            a_Stream.AlignStream(4);

            m_PathName = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
        }
    }
}
