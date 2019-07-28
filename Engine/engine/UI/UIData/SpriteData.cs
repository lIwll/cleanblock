using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UEngine.UI
{
    public class SpriteData
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public TextureWrapMode wrapmode = TextureWrapMode.Clamp;
        public FilterMode filtermode = FilterMode.Bilinear;
    }

    public class SpriteConfig
    {
        public Dictionary<string, SpriteData> spriteConfig = new Dictionary<string, SpriteData>();
    }
}
