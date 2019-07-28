using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UEngine.UIExpand;
using UEngine.UI.UILuaBehaviour;


namespace UEngine.FreeType
{
    [Serializable]
    public class TextAtlasPacker
    {
        public Dictionary<int, AtlasData> m_TextureDic = new Dictionary<int, AtlasData>();

        public List<AtlasData> atlasList = new List<AtlasData>();

        public int height;

        public int width;

        public SFontData mFontdata;

        public IntPtr Font;

        public TextAtlasPacker(IntPtr mFont, SFontData fontData)
        {
            Font = mFont;
            mFontdata = fontData;

            height = fontData.size + 3 + fontData.outlinesize * 2 - UIManager.OffsetSize;
            width = fontData.size + 3 + fontData.outlinesize * 2 - UIManager.OffsetSize;
        }

        public AtlasData SetCharCode(int code)
        {
            if (Font == IntPtr.Zero)
            {
                return null;
            }
            if (m_TextureDic.ContainsKey(code))
            {
                return m_TextureDic[code];
            }

            FreeTypeGlyph ftGlyph;
            var _data = FreeTypeFontApi.GetGlyph( Font, code, out ftGlyph, mFontdata.size, mFontdata.outlinesize, mFontdata.FontStyle == FontStyle.Bold || mFontdata.FontStyle == FontStyle.BoldAndItalic, mFontdata.FontStyle == FontStyle.Italic || mFontdata.FontStyle == FontStyle.BoldAndItalic );

            if (_data == IntPtr.Zero)
            {
                return SetCharCode(FontFactory.messyCode);
            }

            AtlasData data = CreateNewTexture(code, _data, ftGlyph);
            return data;
        }

        public AtlasData CreateNewTexture(int code, IntPtr data, FreeTypeGlyph ftGlyph)
        {
            for (int i = 0; i < atlasList.Count; i++)
            {
                if (!atlasList[i].isFull)
                {
                    atlasList[i].CreateNewCode(code, data, ftGlyph);
                    m_TextureDic.Add(code, atlasList[i]);
                    return atlasList[i];
                }
            }

            AtlasData atlasdata;
            if (atlasList.Count > 0 && Application.isPlaying)
            {
                atlasdata = new AtlasData( Font, mFontdata, width, height, true );
            }
            else
            {
                atlasdata = new AtlasData( Font, mFontdata, width, height, false );
            }

            atlasdata.CreateNewCode(code, data, ftGlyph);
            atlasList.Add(atlasdata);
            m_TextureDic.Add(code, atlasdata);
            return atlasdata;
        }

        public void ClearCodeTex ( float time ) 
        {
            for (int i = 0 ; i < atlasList.Count ; i++)
            {
                int[] clearCode = atlasList[i].ClearCodeTexture( time );
                for (int j = 0 ; j < clearCode.Length ; j++)
                {
                    if (m_TextureDic.ContainsKey(clearCode[j]))
                        m_TextureDic.Remove(clearCode[j]);
                }
            }
        }

        public void ClearAll ( ) 
        {
            for (int i = 0 ; i < atlasList.Count ; i++)
            {
                atlasList[i].ClearAll();
            }

            m_TextureDic.Clear();
            atlasList.Clear();
        }
    }

    public struct Glyph
    {
        public int index;
        public FreeTypeGlyph ftGlyph;
        public Rect uv;
    }

    [Serializable]
    public class AtlasData
    {
        public IntPtr m_fontContext;

        [SerializeField]
        public Texture2D m_texture;

        [SerializeField]
        public Material m_Material;

        SFontData fontData;

        public Material Material 
        {
            get 
            {
                if (!m_Material)
                {
                    if (fontData.outlinesize > 0)
                        m_Material = new Material( UShaderManager.FindShader( "UEngine/UI/TextOutlineRG16" ) );
                    else
                        m_Material = new Material( UShaderManager.FindShader( "UEngine/UI/UITextAlpha8" ) );

                    m_Material.name = "Size:" + fontData.size;
                    m_Material.SetTexture( "_MainTex", m_texture );
                }
                return m_Material;
            }
        }

        public List<Vector2> vertexList = new List<Vector2>();

        public List<int> codeList = new List<int>();

        public Dictionary<Color, Material> OutlineDic = new Dictionary<Color, Material>();

        public Dictionary<int, int> CodeIndexDic = new Dictionary<int, int>();

        public Queue<int> CodeIndexQue = new Queue<int>();

        public int Length;

        public int width;

        public int height;

        public int X;

        public int Y;

        public bool isFull = false;

        private const int s_canvasMinSize = 0x200;
        private const int s_canvasMaxSize = 0x1000;

        private const int s_glyphPadding = 2;

        public int m_canvasSize = 512;
        private int m_x;
        private int m_y;
        private byte[] m_rawData;

        private float[] CodeTime;
        private bool[] CodeUse;
        private int[] Code;
        private CharacterInfo[] characterInfos;

        public AtlasData(IntPtr font, SFontData _fontdata,int _width, int _height, bool IsDouble)
        {
            if (IsDouble)
            {
                m_canvasSize = m_canvasSize * 2;
            }

            if (USystemConfig.Instance.IsDevelopMode)
            {
                m_canvasSize = 4096;
            }
            m_fontContext = font;
            fontData = _fontdata;
            width = _width;
            height = _height;
            X = m_canvasSize / width;
            Y = m_canvasSize / height;
            Length = X * Y;
            CodeTime = new float[Length];
            CodeUse = new bool[Length];
            Code = new int[Length];
            characterInfos = new CharacterInfo[Length];
            for (int i = 0 ; i < Length ; i++)
            {
                CodeIndexQue.Enqueue( i );
            }
        }

        public void CreateNewCode(int code, IntPtr data, FreeTypeGlyph ftGlyph)
        {
            if (!m_texture)
            {
                EnlargeTexture( fontData.outlinesize > 0 );
            }

            if (CodeIndexQue.Count > 0)
            {
                int index = CodeIndexQue.Dequeue();
                CodeUse[index] = true;
                CodeTime[index] = Time.realtimeSinceStartup;
                CreateTexture( code, index, data, ftGlyph );
                if (!CodeIndexDic.ContainsKey(code))
                {
                    CodeIndexDic.Add( code, index );
                }
                else
                {
                    CodeIndexDic[code] = index;
                }

                if (CodeIndexQue.Count == 0)
                {
                    isFull = true;
                }
            }

            //for ( int i = 0; i < Length; i++ )
            //{
            //    if ( !CodeUse[i] )
            //    {
            //        CreateTexture( code, i, data, ftGlyph );
            //        CodeUse[i] = true;
            //        CodeTime[i] = Time.realtimeSinceStartup;
            //        for (int j = i + 1 ; j < Length ; j++)
            //        {
            //            if (!CodeUse[j])
            //            {
            //                return;
            //            }
            //        }
            //        isFull = true;
            //        return;
            //    }
            //}
        }

        public void CreateTexture(int charCode, int i, IntPtr data, FreeTypeGlyph ftGlyph)
        {
            Glyph glyph = new Glyph();
            glyph.ftGlyph = ftGlyph;

            if (m_fontContext != IntPtr.Zero)
            {
                if (data != IntPtr.Zero)
                {
                    if (Alloc(ref glyph, i))
                    {
                        UpdateData( data, glyph.ftGlyph );
                        Apply();
                        //UCoreUtil.CreateFile("Assets/Tex.png", m_texture.EncodeToPNG());
                        CharacterInfo charInfo = new CharacterInfo();
                        var rect = glyph.uv;
                        charInfo = new CharacterInfo
                        {
                            index = glyph.ftGlyph.code,
                            advance = glyph.ftGlyph.advance,
                            bearing = glyph.ftGlyph.bearing,
                            minX = glyph.ftGlyph.minX,
                            maxX = glyph.ftGlyph.maxX,
                            minY = glyph.ftGlyph.minY,
                            maxY = glyph.ftGlyph.maxY,
                            size = fontData.size,
                            style = fontData.FontStyle,
                            glyphWidth = glyph.ftGlyph.width - fontData.outlinesize,
                            glyphHeight = glyph.ftGlyph.height - fontData.outlinesize,
                            uvBottomLeft = new Vector2(rect.xMin, rect.yMax),
                            uvBottomRight = new Vector2(rect.xMax, rect.yMax),
                            uvTopLeft = new Vector2(rect.xMin, rect.yMin),
                            uvTopRight = new Vector2(rect.xMax, rect.yMin),
                        };
                        characterInfos[i] = charInfo;
                        Code[i] = charCode;
                    }
                }
            }
        }

        unsafe public void UpdateData(IntPtr data, FreeTypeGlyph glyph)
        {
            if (fontData.outlinesize > 0)
            {
                fixed (byte* dstPtr = m_rawData)
                {
                    byte* srcPtr = (byte*)data.ToPointer();
                    for (int y = 0; y < glyph.height; y++)
                    {
                        int srcIndex = y * glyph.width * 2;
                        int dstIndex = ((m_y + y) * m_canvasSize + m_x) * 2;
                        for (int x = 0; x < glyph.width; x++)
                        {
                            dstPtr[dstIndex] = srcPtr[srcIndex];
                            dstPtr[dstIndex + 1] = srcPtr[srcIndex + 1];
                            srcIndex += 2;
                            dstIndex += 2;
                        }
                    }
                }
            }
            else
            {
                fixed (byte* dstPtr = m_rawData)
                {
                    byte* srcPtr = (byte*)data.ToPointer();
                    for (int y = 0; y < glyph.height; y++)
                    {
                        int srcIndex = y * glyph.width;
                        int dstIndex = ((m_y + y) * m_canvasSize + m_x);
                        for (int x = 0; x < glyph.width; x++)
                        {
                            dstPtr[dstIndex] = srcPtr[srcIndex];
                            srcIndex += 1;
                            dstIndex += 1;
                        }
                    }
                }
            }
        }

        public void EnlargeTexture(bool isOutline)
        {
            if (isOutline)
            {
                if (!OutlineDic.ContainsKey( fontData.outlineColor ))
                {
                    var s = UShaderManager.FindShader( "UEngine/UI/TextOutlineRG16" );

                    m_rawData = new byte[m_canvasSize * m_canvasSize * 2];
                    m_rawData[m_canvasSize * m_canvasSize * 2 - 1] = 255;
                    m_texture = new Texture2D(m_canvasSize, m_canvasSize, TextureFormat.RG16, false);

                    if (null != s) 
                    {
                        Material outlineMaterial = new Material( s );
                        OutlineDic.Add( fontData.outlineColor, outlineMaterial );
                        if (null != outlineMaterial)
                        {
                            outlineMaterial.SetTexture( "_MainTex", m_texture );
                            outlineMaterial.name = "Size:" + fontData.size;
                            outlineMaterial.SetColor( "_OutlineColor", fontData.outlineColor );
                        }
                    }
                    else
                    {
                        ULogger.Warn( "TextOutlineRG16 加载失败" );
                    }
                }
            }
            else
            {
				var s = UShaderManager.FindShader("UEngine/UI/UITextAlpha8");

                m_rawData = new byte[m_canvasSize * m_canvasSize];
                m_rawData[m_canvasSize * m_canvasSize - 1] = 255;
                m_texture = new Texture2D(m_canvasSize, m_canvasSize, TextureFormat.Alpha8, false);
				if (null != s)
                {
					m_Material = new Material(s);
                }
                else
                {
                    ULogger.Warn( "TextAlpha8 加载失败" );
                }
			    if (null != m_Material)
			    {
				    m_Material.SetTexture("_MainTex", m_texture);
				    m_Material.name = "Size:" + fontData.size;
			    }
            }
            m_texture.name = "Size:" + fontData.size;
            m_texture.wrapMode = TextureWrapMode.Clamp;
            m_texture.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
        }

        public void Apply()
        {
            m_texture.LoadRawTextureData(m_rawData);
            
            m_texture.Apply();

            //byte[] bytes = m_texture.EncodeToPNG();
            //FileStream fs = new FileStream( "Assets/EncodeSize" + fontData.size, FileMode.Create );
            //fs.Write( bytes, 0, bytes.Length );
            //fs.Close();
        }

        public bool Alloc(ref Glyph glyph, int index)
        {
            bool result = true;

            m_x = (index % X) * width;

            m_y = (index / X) * height;

            if (m_x > m_canvasSize || m_y > m_canvasSize)
            {
                result = false;
            }

            if (result)
            {
                glyph.uv = new Rect(
                    ((float)m_x) / m_canvasSize,
                    ((float)m_y) / m_canvasSize,
                    ((float)glyph.ftGlyph.width) / m_canvasSize,
                    ((float)glyph.ftGlyph.height) / m_canvasSize
                );
            }

            return result;
        }

        public CharacterInfo GetCharInfo(int charCode)
        {
            if (CodeIndexDic.ContainsKey(charCode))
            {
                int index = CodeIndexDic[charCode];
                CodeTime[index] = Time.realtimeSinceStartup;
                return characterInfos[index];
            }
            else
            {
                if (CodeIndexDic.ContainsKey( FontFactory.messyCode )) 
                {
                    int index = CodeIndexDic[FontFactory.messyCode];
                    CodeTime[index] = Time.realtimeSinceStartup;
                    return characterInfos[index];
                }
                return new CharacterInfo();
            }

            //for (int i = 0 ; i < Code.Length ; i++)
            //{
            //    if (Code[i] == charCode)
            //    {
            //        CodeTime[i] = Time.realtimeSinceStartup;
            //        return characterInfos[i];
            //    }
            //}

            //for (int i = 0 ; i < Code.Length ; i++)
            //{
            //    if (Code[i] == FontFactory.messyCode)
            //    {
            //        CodeTime[i] = Time.realtimeSinceStartup;
            //        return characterInfos[i];
            //    }
            //}
            //return new CharacterInfo();
        }

        public int[] ClearCodeTexture ( float time ) 
        {
            List<int> clearCodeList = new List<int>();
            for (int i = 0 ; i < Length ; i++)
            {
                if (CodeTime[i] - Time.realtimeSinceStartup > time)
                {
                    CodeUse[i] = false;
                    isFull = false;
                    clearCodeList.Add( Code[i] );
                    CodeIndexQue.Enqueue( i );
                    CodeIndexDic.Remove( Code[i] );
                }
            }
            return clearCodeList.ToArray();
        }

        public void ClearAll ( ) 
        {
            if (m_texture != null)
            {
                UnityEngine.Object.Destroy( m_texture );
                m_texture = null;
            }

            if (m_Material != null)
            {
                UnityEngine.Object.Destroy( m_Material );
                m_Material = null;
            }

            CodeIndexQue.Clear();
            CodeIndexDic.Clear();

            if (OutlineDic.Count > 0)
            {
                Dictionary<Color, Material>.Enumerator it = OutlineDic.GetEnumerator();
                while( it.MoveNext() )
                {
                    var item = it.Current.Value;
                    UnityEngine.Object.Destroy( item );
                }
                OutlineDic.Clear();
            }
        }
    }
}
