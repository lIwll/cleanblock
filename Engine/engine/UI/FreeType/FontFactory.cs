using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AssetStudio;

using UnityEngine;

namespace UEngine.FreeType
{
    [Serializable]
    public class FontFactory
    {
        public static char messyCode = '?';

        public static Dictionary<string, CFont> fontDic = new Dictionary<string, CFont>();

        public static CFont GetFontData(string path)
        {
            path = path.ToLower();
            if (path.StartsWith( "assets/resources/" ))
            {
                path = path.Replace( "assets/resources/", "" );
            }
            if (!USystemConfig.Instance.IsDevelopMode)
            {
                path = path.Replace( ".ttf", "_ttf.u" );
            }
            if (fontDic.ContainsKey( path ))
            {
                return fontDic[path];
            }
            byte[] bytes = null;
            if (USystemConfig.Instance.IsDevelopMode)
            {
                bytes = UFileAccessor.ReadBinaryFile( path );
            }
            else
            {
                BundleFile b_File = new BundleFile( path );
                bytes = b_File.GetByte()[0];
                if (bytes == null || bytes.Length == 0)
                {
                    ULogger.Error( "字体加载为空" );
                }
            }

            if (bytes == null)
            {
                return null;
            }

            IntPtr fontInt = FreeTypeFontApi.CreateFontContext(bytes);//"C:/Users/joypie/AppData/LocalLow/DefaultCompany/SignedDistanceFieldsFont2\\Font\\fzjt"
            if (fontInt == IntPtr.Zero)
            {
                return null;
            }
            CFont cfont = new CFont();
            cfont.mFont = fontInt;
            fontDic.Add(path, cfont);
            return cfont;
        }

        public static void ClearCodeTex ( float time )
        {
            Dictionary<string, CFont>.Enumerator it = fontDic.GetEnumerator();
            while( it.MoveNext() )
            {
                var item = it.Current.Value;
                item.ClearCodeTex( time );
            }
        }

        public static void ClearAll ( ) 
        {
            Dictionary<string, CFont>.Enumerator it = fontDic.GetEnumerator();
            while (it.MoveNext())
            {
                var item = it.Current.Value;
                item.ClearAll();
            }
            fontDic.Clear();
        }
    }

    [Serializable]
    public struct SFontData 
    {
        public FontStyle FontStyle;
        public int size;
        public int outlinesize;
        public Color outlineColor;

        public override bool Equals ( object obj )
        {
            if (!(obj is SFontData))
                return false;

            var fontData = (SFontData)obj;
            if (FontStyle == fontData.FontStyle && size == fontData.size && outlinesize == fontData.outlinesize && outlineColor.Equals(fontData.outlineColor))
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode ( )
        {
            return base.GetHashCode();
        }
    }
}