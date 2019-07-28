using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace UEngine.Data
{
    [Serializable]
    public class ImageData
    {
        public SortedDictionary<string, BorderData> data_Dic = new SortedDictionary<string, BorderData>();
    }

    [Serializable]
    public struct BorderData
    {
        //public UnityEngine.Vector4 border;
        public float x;
        public float y;
        public float w;
        public float h;
        public int _x;
        public int _y;
        public int _w;
        public int _h;
        public string path;
    }

    public struct UVData 
    {
        public float x;
        public float y;
        public float w;
        public float h;
        public int _x;
        public int _y;
        public int _w;
        public int _h;
    }

    public class UiTextureConfig
    {
        private static ImageData imagedata;
        public static ImageData Imagedata
        {
            get
            {
                if (imagedata == null)
                {
                    imagedata = new ImageData();
                    string json = UFileAccessor.ReadStringFile("Data/TextureConfig.txt");
                    imagedata = JsonConvert.DeserializeObject<ImageData>(json);
                    return imagedata;
                }
                else
                {
                    return imagedata;
                }
            }
        }
    }

    public class UiGuidConfig 
    {
        public int Count;
    }
}

