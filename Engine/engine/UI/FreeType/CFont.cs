using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UEngine.FreeType
{
    [Serializable]
    public class CFont
    {
        public IntPtr mFont;
        
        //public Dictionary<SFontData, TextAtlasPacker> mAtlasMap = new Dictionary<SFontData, TextAtlasPacker>();

        public List<SFontData> fontDataList = new List<SFontData>();
        public List<TextAtlasPacker> atlasList = new List<TextAtlasPacker>();

        public TextAtlasPacker GetAtlas(SFontData fontdata)
        {
            int matchIndex = -1;
            for (int i = 0 ; i < fontDataList.Count ; i++)
            {
                if (fontdata.Equals( fontDataList[i] ))
                {
                    matchIndex = i;
                    break;
                }
            }
            //if (mAtlasMap.ContainsKey(fontdata))
            //{
            //    return mAtlasMap[fontdata];
            //}
            if (matchIndex != -1)
            {
                return atlasList[matchIndex];
            }
            TextAtlasPacker atlas = new TextAtlasPacker(mFont, fontdata);
            fontDataList.Add( fontdata );
            atlasList.Add( atlas );
            //mAtlasMap.Add(fontdata, atlas);
            return atlas;
        }

        public void ClearCodeTex ( float time ) 
        {
            for (int i = 0 ; i < atlasList.Count ; i++)
            {
                atlasList[i].ClearCodeTex( time );
            }

            //Dictionary<SFontData, TextAtlasPacker>.Enumerator it = mAtlasMap.GetEnumerator();
            //while( it.MoveNext() )
            //{
            //    var item = it.Current.Value;
            //    item.ClearCodeTex( time );
            //}
        }

        public void ClearAll ( ) 
        {
            for (int i = 0 ; i < atlasList.Count ; i++)
            {
                atlasList[i].ClearAll();
            }
            atlasList.Clear();
            fontDataList.Clear();
            //Dictionary<SFontData, TextAtlasPacker>.Enumerator it = mAtlasMap.GetEnumerator();
            //while (it.MoveNext())
            //{
            //    var item = it.Current.Value;
            //    item.ClearAll();
            //}
            //mAtlasMap.Clear();
        }
    }
}
