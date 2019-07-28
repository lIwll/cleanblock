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
    public class MeshFilter
    {
        public long preloadIndex;
        public PPtr m_GameObject = new PPtr();
        public PPtr m_Mesh = new PPtr();

        public MeshFilter(AssetPreloadData preloadData)
        {
            var sourceFile = preloadData.sourceFile;
            var a_Stream = preloadData.sourceFile.reader;
            a_Stream.Position = preloadData.Offset;

            if (sourceFile.platform == -2)
            {
                uint m_ObjectHideFlags = a_Stream.ReadUInt32();
                PPtr m_PrefabParentObject = sourceFile.ReadPPtr();
                PPtr m_PrefabInternal = sourceFile.ReadPPtr();
            }

            m_GameObject = sourceFile.ReadPPtr();
            m_Mesh = sourceFile.ReadPPtr();
        }
    }
}
