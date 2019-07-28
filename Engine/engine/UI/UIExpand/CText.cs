using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

using UEngine.Data;
using UEngine.FreeType;
using UEngine.UI.UILuaBehaviour;

namespace UEngine.UIExpand
{
    [ExecuteInEditMode]
    public class CText : TextGraphic
    {
        public bool IsDirty = true;

        public Font Font;

        public string fontPath;

        static string InputString = "<\\$([a-z]+)=(.+?)\\$>(.+?)<\\$\\/\\1\\$>";

        static string SpriteString = "<\\$([a-z]+)=([^<>]+?)\\/\\$>";

        static string RichString = "<\\$([a-z]+)\\$>(.+?)<\\$\\/\\1\\$>";

        // 用正则取富文本格式(超链接,颜色,字体大小,描边大小,描边颜色)
        private static readonly Regex _InputTagRegex = new Regex( InputString, RegexOptions.Singleline );
        // 图文混排
        private static readonly Regex SpriteRegex = new Regex( SpriteString, RegexOptions.Singleline );
        // 加粗
        private static readonly Regex RichRegex = new Regex( RichString, RegexOptions.Singleline );

        //更新后的文本
        private string _OutputText = "";

        public override string text
        {
            get
            {
                return m_Text;
            }
            set
            {
                if (String.IsNullOrEmpty( value ))
                {
                    if (String.IsNullOrEmpty( m_Text ))
                        return;
                    m_Text = "";
                    _OutputText = "";
                    lines.Clear();
                    foreach (var item in ChildTextDic.Values)
                    {
                        item.mCharInfoList.Clear();
                    }
                    SetAllDirty();
                }
                else if (m_Text != value)
                {
                    if (m_Font != null)
                    {
                        _atlasPack = m_Font.GetAtlas( fontdata );
                    }
                    m_Text = value;
                    IsDirty = true;
                }
            }
        }

        public override Color color
        {
            get
            {
                return base.color;
            }
            set
            {
                base.color = value;
                IsDirty = true;
            }
        }

        public class HrefClickEvent : UnityEvent<int> { }
        //点击事件监听
        public HrefClickEvent OnHrefClick = new HrefClickEvent();

        public readonly Dictionary<Texture, TextGraphic> ChildTextDic = new Dictionary<Texture, TextGraphic>();

        public List<TextChild> ChildTextList = new List<TextChild>();

        public List<HrefInfo> mHreInfoList = new List<HrefInfo>();

        public List<CharInfo> totalTextCharInfoList = new List<CharInfo>();

        public bool RichText = false;

        public bool isUseColorLibray = false;

        public bool isUseSizeLibray = false;

        [HideInInspector]
        public string mColorName = "Default";

        [HideInInspector]
        public string mSizeName = "";

        public bool VCenter = false;

        public bool HCenter = false;

        [FormerlySerializedAs( "alignment" )]
        public TextAnchor Alignment = TextAnchor.UpperLeft;

        public bool VerticalFit = false;

        public bool HorizontalFit = false;

        public float textSpace = 0;

        public float lineSpace = 0;

        public Color OutLineColor = Color.black;

        public bool IsUseGradualColor = false;

        public Color TopColor = Color.black;

        public Color BottomColor = Color.white;

        private Dictionary<string, Texture> NetTextureMap = new Dictionary<string, Texture>();

        CFont _font;
        CFont m_Font
        {
            get
            {
                if (!string.IsNullOrEmpty( UIManager.fontPath ))
                {
                    if (_font == null)
                    {
                        _font = FontFactory.GetFontData( UIManager.fontPath );
                    }
                    if (_font != null)
                    {
                        _atlasPack = _font.GetAtlas( fontdata );
                    }
                }
                return _font;
            }
        }
        TextAtlasPacker _atlasPack;
        TextAtlasPacker atlasPack
        {
            get
            {
                if (_atlasPack != null)
                {
                    return _atlasPack;
                }
                else
                {
                    if (m_Font != null)
                    {
                        _atlasPack = m_Font.GetAtlas( fontdata );
                    }
                }
                return _atlasPack;
            }
        }

        PanelManager _panelManager;

        public PanelManager panelManager
        {
            get
            {
                if (!_panelManager)
                {
                    _panelManager = rectTransform.GetComponentInParent<PanelManager>();
                }

                return _panelManager;
            }
        }

        protected override void Awake ( )
        {
            if (isUseColorLibray && UIManager.mColorLibray != null && UIManager.mColorLibray.ContainsKey( mColorName ))
            {
                color = UIManager.mColorLibray[mColorName];
            }

            if (isUseSizeLibray && UIManager.mFontSizeLibray != null && UIManager.mFontSizeLibray.ContainsKey( mSizeName ))
            {
                FontSize = UIManager.mFontSizeLibray[mSizeName];
            }
            base.Awake();
        }

        protected override void OnEnable ( )
        {
            //supportRichText = false;
            //IsDirty = true;

            Dictionary<Texture, TextGraphic>.Enumerator it = ChildTextDic.GetEnumerator();
            while (it.MoveNext())
            {
                var item = it.Current.Value;
                if (item != this)
                    item.enabled = true;
            }

            base.OnEnable();
        }

        protected override void Start ( )
        {
            
            base.Start();
        }

        protected override void OnDisable ( )
        {
            Dictionary<Texture, TextGraphic>.Enumerator it = ChildTextDic.GetEnumerator();
            while (it.MoveNext())
            {
                var item = it.Current.Value;
                if (item != this)
                    item.enabled = false;
            }
            base.OnDisable();
        }

        private float mTempTime = 0f;
        void Update ( )
        {
            if (IsDirty)
            {
                if (!string.IsNullOrEmpty( UIManager.fontPath ))
                {
                    if (_font == null)
                    {
                        _font = FontFactory.GetFontData( UIManager.fontPath );
                    }
                    if (_font != null)
                    {
                        _atlasPack = _font.GetAtlas( fontdata );
                    }
                }

                if (!string.IsNullOrEmpty( m_Text ))
                {
                    _OutputText = UpdateText( m_Text );
                    UpdateTextInfo();
                }

                foreach (var item in ChildTextDic.Values)
                {
                    item.SetAllDirty();
                }

                IsDirty = false;
            }

            if (TextIsPlaying && mCharInfoList.Count > 0)
            {
                if (TextIsStop)
                {
                    mTempTime = Time.realtimeSinceStartup;
                }

                float TempTime = Time.realtimeSinceStartup - mTempTime;
                if (TempTime >= mTimeInterval)
                {
                    int index = (int)(TempTime / mTimeInterval);

                    TextAniIndex += index;
                    mTempTime = Time.realtimeSinceStartup - (TempTime - index * mTimeInterval);
                    IsDirty = true;
                }

                if (TextAniIndex > mCharInfoList.Count)
                {
                    StopTextAni( );
                    if (mAniCallBack != null)
                    {
                        mAniCallBack();
                    }
                }
            }
        }

        public void Init ( )
        {
            fontdata.outlineColor = OutLineColor;
            //IsDirty = true;
        }

#if UNITY_2017
        protected void OnValidate ( )
#else
        //protected override void OnValidate ( )
        protected void OnValidate()
#endif
        {
            if (isUseColorLibray && !Application.isPlaying && UIManager.mColorLibray.ContainsKey( mColorName ))
            {
                color = UIManager.mColorLibray[mColorName];
            }

            if (isUseSizeLibray && !Application.isPlaying && UIManager.mFontSizeLibray.ContainsKey( mSizeName ))
            {
                FontSize = UIManager.mFontSizeLibray[mSizeName];
            }

            //if (m_Font != null)
            //{
            //    _atlasPack = m_Font.GetAtlas( fontdata );
            //}

            //if (!string.IsNullOrEmpty( m_Text ))
            //{
            //    _OutputText = UpdateText( m_Text, true );
            //}

            if (USystemConfig.Instance.IsDevelopMode)
            {
                IsDirty = true;
                ChildTextDic.Clear();
                mCharInfoList.Clear();
                SetAllDirty();
            }

#if !UNITY_2017
            //base.OnValidate();
#endif
        }

        public int FontSize
        {
            get
            {
                return fontdata.size;
            }
            set
            {
                fontdata.size = value;
                if (m_Font != null)
                {
                    _atlasPack = m_Font.GetAtlas( fontdata );
                }
            }
        }

        public FontStyle FontStyle
        {
            get
            {
                return fontdata.FontStyle;
            }
            set
            {
                fontdata.FontStyle = value;
                if (m_Font != null)
                {
                    _atlasPack = m_Font.GetAtlas( fontdata );
                }
            }
        }

        public int OutLineSize
        {
            get
            {
                return fontdata.outlinesize;
            }
            set
            {
                fontdata.outlinesize = value;
                if (m_Font != null && _atlasPack == null)
                {
                    _atlasPack = m_Font.GetAtlas( fontdata );
                }
            }
        }

        ToKen toKen;

        List<TextLine> lines = new List<TextLine>();

        private Rect inputRect
        {
            get { return rectTransform.rect; }
        }

        public int TextAniIndex = 0;
        public bool TextIsPlaying = false;
        private bool TextIsStop = false;
        private float mTimeInterval = 0f;
        private Action mAniCallBack;

        public void PlayTextAni ( float timeInterval, Action AniCallBack = null ) 
        {
            TextIsPlaying = true;
            TextIsStop = false;
            TextAniIndex = 0;
            mTimeInterval = timeInterval;
            mAniCallBack = AniCallBack;
            mTempTime = Time.realtimeSinceStartup;
        }

        public void StopTextAni ( bool IsComplete = true ) 
        {
            if (IsComplete)
            {
                TextIsPlaying = false;
                TextAniIndex = 0;
                mTempTime = 0f;
                IsDirty = true;
            }
            mTempTime = Time.realtimeSinceStartup;
            TextIsStop = true;
        }

        protected override void OnPopulateMesh ( VertexHelper vh )
        {
            mHreInfoList.Clear();
            vh.Clear();
            if (m_Font == null || string.IsNullOrEmpty( _OutputText ) || fontdata.size == 0)
            {
                return;
            }

            UpdateLinesInfo();

            for (int i = 0 ; i < mCharInfoList.Count ; i++)
            {
                if (TextIsPlaying)
                {
                    int index = totalTextCharInfoList.LastIndexOf( mCharInfoList[i] );
                    if (index >= TextAniIndex)
                    {
                        break;
                    }
                }

                CharacterInfo ch = mCharInfoList[i].ch;
                Vector2 textPos = mCharInfoList[i].TextPos;
                Color mColor = mCharInfoList[i].charColor;
                Color topColor;
                Color bottomColor;
                Vector2 lowerLeft = new Vector2( textPos.x + ch.bearing, textPos.y - ( ch.glyphHeight - ch.maxY ) );
                Vector2 topRight = new Vector2( textPos.x + ch.glyphWidth + ch.bearing, textPos.y + ch.maxY );
                if (mCharInfoList[i].IsSprite)
                {
                    topColor = Color.white;
                    bottomColor = Color.white;
                }
                else
                {
                    if (mCharInfoList[i].IsUseGradual)
                    {
                        topColor = new Color( TopColor.r, TopColor.g, TopColor.b, TopColor.a * color.a );
                        bottomColor = new Color( BottomColor.r, BottomColor.g, BottomColor.b, BottomColor.a * color.a );
                    }
                    else
                    {
                        if (mCharInfoList[i].IsRichColor)
                        {
                            topColor = new Color( mColor.r, mColor.g, mColor.b, mColor.a * color.a );
                            bottomColor = topColor;
                        }
                        else
                        {
                            topColor = color;
                            bottomColor = color;
                        }
                    }
                }

                m_TempVerts[0].position = new Vector3( lowerLeft.x, lowerLeft.y );
                m_TempVerts[0].color = bottomColor;
                m_TempVerts[0].uv0 = ch.uvBottomLeft;
                m_TempVerts[1].position = new Vector3( topRight.x, lowerLeft.y );
                m_TempVerts[1].color = bottomColor;
                m_TempVerts[1].uv0 = ch.uvBottomRight;
                m_TempVerts[2].position = new Vector3( topRight.x, topRight.y );
                m_TempVerts[2].color = topColor;
                m_TempVerts[2].uv0 = ch.uvTopRight;
                m_TempVerts[3].position = new Vector3( lowerLeft.x, topRight.y );
                m_TempVerts[3].color = topColor;
                m_TempVerts[3].uv0 = ch.uvTopLeft;
                vh.AddUIVertexQuad( m_TempVerts );
                if (mCharInfoList[i].IsLink)
                {
                    var pos = m_TempVerts[0].position;
                    var bounds = new Bounds( pos, Vector3.zero );
                    bounds.Encapsulate( m_TempVerts[2].position );
                    mHreInfoList.Add( new HrefInfo { tag = mCharInfoList[i].tag, boxes = new Rect( bounds.min, bounds.size ) } );

                    m_TempVerts[0].position = new Vector3( textPos.x, textPos.y - 4 );
                    m_TempVerts[0].uv0 = new Vector2( 1023.5f / 1024f, 1023.5f / 1024f );
                    m_TempVerts[1].position = new Vector3( textPos.x + ch.advance + textSpace, textPos.y - 4 );
                    m_TempVerts[1].uv0 = new Vector2( 1f, 1023.5f / 1024f );
                    m_TempVerts[2].position = new Vector3( textPos.x + ch.advance + textSpace, textPos.y - 2 );
                    m_TempVerts[2].uv0 = new Vector2( 1f, 1f );
                    m_TempVerts[3].position = new Vector3( textPos.x, textPos.y - 2 );
                    m_TempVerts[3].uv0 = new Vector2( 1023.5f / 1024f, 1f );
                    vh.AddUIVertexQuad( m_TempVerts );
                }
            }
        }

        private void UpdateTextInfo ( )
        {
            lines.Clear();
            if (m_Font == null || string.IsNullOrEmpty( _OutputText ) || fontdata.size == 0)
            {
                return;
            }

            if (isSprite && !material)
            {
                material = Canvas.GetDefaultCanvasMaterial();
                material.mainTexture = mTexture;
            }
            //mTexture = material.mainTexture;
            if (OutLineSize > 0 && material)
            {
                material.SetColor( "_OutlineColor", OutLineColor );
            }

            Vector2 pivot = rectTransform.pivot;
            Rect inputRect = rectTransform.rect;

            float posY = 0;

            float totalHeight = -lineSpace;

            float RectWidth = 0;

            if (toKen == null)
            {
                toKen = new ToKen( this );
            }
            toKen.Refresh();

            TextLine Line = new TextLine( 0, 0 );

            int charCount = 0;

            for (int i = 0 ; i < _OutputText.Length ; ++i)
            {
                char c = _OutputText[i];

                if (HorizontalFit)
                {
                    RectWidth = float.MaxValue;
                }
                else
                {
                    RectWidth = inputRect.width;
                }

                if (!toKen.AddChar( c, i, RectWidth, this ))
                {
                    if (Line.TryAdd( toKen, RectWidth, toKen.CalWidth ))
                    {
                        Line.AddToKen( toKen, textSpace );
                        if (!lines.Contains( Line ))
                        {
                            if (lines.Count == 0)
                            {
                                if (totalHeight + Line.MaxHeight + lineSpace > inputRect.height && !VerticalFit)
                                {
                                    RemoveLineCharInfo( Line, charCount );
                                    break;
                                }
                                else
                                {
                                    totalHeight += ( Line.MaxHeight + lineSpace + 10 );
                                }
                            }
                            else
                            {
                                if (totalHeight + Line.MaxMaxY > inputRect.height && !VerticalFit)
                                {
                                    RemoveLineCharInfo( Line, charCount );
                                    break;
                                }
                                else
                                {
                                    totalHeight += ( Line.MaxMaxY + lineSpace + 10 );
                                }
                            }
                            lines.Add( Line );
                            charCount += Line.mCharInfoList.Count;
                        }
                    }
                    else
                    {
                        posY -= ( Line.MaxMaxY + lineSpace + 10 );
                        Line = new TextLine( posY, atlasPack.height );
                        Line.AddToKen( toKen, textSpace );
                        if (lines.Count == 0)
                        {
                            if (totalHeight + Line.MaxHeight + lineSpace > inputRect.height && !VerticalFit)
                            {
                                RemoveLineCharInfo( Line, charCount );
                                break;
                            }
                            else
                            {
                                totalHeight += ( Line.MaxHeight + lineSpace + 10 );
                            }
                        }
                        else
                        {
                            if (totalHeight + Line.MaxMaxY > inputRect.height && !VerticalFit)
                            {
                                RemoveLineCharInfo( Line, charCount );
                                break;
                            }
                            else
                            {
                                totalHeight += ( Line.MaxMaxY + lineSpace + 10 );
                            }
                        }
                        lines.Add( Line );
                        charCount += Line.mCharInfoList.Count;
                    }
                    if (c == '\n' || c == '\r')
                    {
                        posY -= ( Line.MaxMaxY + lineSpace + 10 );
                        Line = new TextLine( posY, atlasPack.height );
                    }
                }

                if (i == _OutputText.Length - 1)
                {
                    if (toKen.mCharInfoList.Count > 0)
                    {
                        if (Line.TryAdd( toKen, RectWidth, toKen.CalWidth ))
                        {
                            Line.AddToKen( toKen, textSpace );
                            if (!lines.Contains( Line ))
                            {
                                if (lines.Count == 0)
                                {
                                    if (totalHeight + Line.MaxHeight + lineSpace > inputRect.height && !VerticalFit)
                                    {
                                        RemoveLineCharInfo( Line, charCount );
                                        break;
                                    }
                                    else
                                    {
                                        totalHeight += ( Line.MaxHeight + lineSpace + 10 );
                                    }
                                }
                                else
                                {
                                    if (totalHeight + Line.MaxMaxY > inputRect.height && !VerticalFit)
                                    {
                                        RemoveLineCharInfo( Line, charCount );
                                        break;
                                    }
                                    else
                                    {
                                        totalHeight += ( Line.MaxMaxY + lineSpace + 10 );
                                    }
                                }
                                lines.Add( Line );
                                charCount += Line.mCharInfoList.Count;
                            }
                        }
                        else
                        {
                            posY -= ( Line.MaxMaxY + lineSpace + 10 );
                            Line = new TextLine( posY, atlasPack.height );
                            Line.AddToKen( toKen, textSpace );
                            if (lines.Count == 0)
                            {
                                if (totalHeight + Line.MaxHeight + lineSpace > inputRect.height && !VerticalFit)
                                {
                                    RemoveLineCharInfo( Line, charCount );
                                    break;
                                }
                                else
                                {
                                    totalHeight += ( Line.MaxHeight + lineSpace + 10 );
                                }
                            }
                            else
                            {
                                if (totalHeight + Line.MaxMaxY > inputRect.height && !VerticalFit)
                                {
                                    RemoveLineCharInfo( Line, charCount );
                                    break;
                                }
                                else
                                {
                                    totalHeight += ( Line.MaxMaxY + lineSpace + 10 );
                                }
                            }
                            lines.Add( Line );
                            charCount += Line.mCharInfoList.Count;
                        }
                    }
                }
            }

            if (HorizontalFit)
            {
                float MaxWidth = 0;
                for (int i = 0 ; i < lines.Count ; i++)
                {
                    float realwidth = lines[i].LineWidth;
                    if (MaxWidth < realwidth)
                    {
                        MaxWidth = realwidth;
                    }
                }
                if (rectTransform)
                {
                    rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, MaxWidth );
                }
            }
            if (VerticalFit)
            {
                float _totalheight = -lineSpace - 10;
                if (string.IsNullOrEmpty( _OutputText ))
                {
                    _totalheight = 0;
                }
                else
                {
                    for (int i = 0 ; i < lines.Count ; i++)
                    {
                        if (i == 0)
                        {
                            _totalheight += ( lines[i].MaxHeight + lineSpace + 10 );
                        }
                        else
                        {
                            _totalheight += ( lines[i].MaxMaxY + lineSpace + 10 );
                        }
                    }
                    //_totalheight += 5;
                }
                if (rectTransform)
                {
                    rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, _totalheight );
                }
            }
        }

        public void UpdateLinesInfo ( )
        {
            float totalHeight = 0;
            float X = -rectTransform.pivot.x * rectTransform.rect.width;
            float Y = ( 1 - rectTransform.pivot.y ) * rectTransform.rect.height;

            for (int i = 0 ; i < lines.Count ; i++)
            {
                lines[i].Update( X, Y, textSpace );
            }
            if (Alignment == TextAnchor.LowerCenter || Alignment == TextAnchor.LowerRight || Alignment == TextAnchor.MiddleCenter || Alignment == TextAnchor.MiddleRight || Alignment == TextAnchor.UpperCenter || Alignment == TextAnchor.UpperRight)
            {
                for (int i = 0 ; i < lines.Count ; i++)
                {
                    lines[i].UpdateV( inputRect.width, Alignment );
                }
            }
            if (Alignment == TextAnchor.LowerCenter || Alignment == TextAnchor.LowerLeft || Alignment == TextAnchor.LowerRight || Alignment == TextAnchor.MiddleCenter || Alignment == TextAnchor.MiddleLeft || Alignment == TextAnchor.MiddleRight)
            {
                totalHeight = -10;
                for (int i = 0 ; i < lines.Count ; i++)
                {
                    if (i == 0)
                    {
                        totalHeight += ( lines[i].MaxHeight + lineSpace + 10 );
                    }
                    else
                    {
                        totalHeight += ( lines[i].MaxMaxY + lineSpace + 10 );
                    }
                }

                if (totalHeight < inputRect.height)
                {
                    float offsetY = 0;
                    switch (Alignment)
                    {
                        case TextAnchor.LowerCenter:
                            offsetY = ( inputRect.height - totalHeight );
                            break;
                        case TextAnchor.LowerLeft:
                            offsetY = ( inputRect.height - totalHeight );
                            break;
                        case TextAnchor.LowerRight:
                            offsetY = ( inputRect.height - totalHeight );
                            break;
                        case TextAnchor.MiddleCenter:
                            offsetY = ( inputRect.height - totalHeight ) / 2;
                            break;
                        case TextAnchor.MiddleLeft:
                            offsetY = ( inputRect.height - totalHeight ) / 2;
                            break;
                        case TextAnchor.MiddleRight:
                            offsetY = ( inputRect.height - totalHeight ) / 2;
                            break;
                        case TextAnchor.UpperCenter:
                            break;
                        case TextAnchor.UpperLeft:
                            break;
                        case TextAnchor.UpperRight:
                            break;
                    }
                    for (int i = 0 ; i < lines.Count ; i++)
                    {
                        lines[i].UpdateH( offsetY );
                    }
                }
            }
        }

        int offsetIndex = 0;
        private void RemoveLineCharInfo ( TextLine Line, int index )
        {
            int count = mCharInfoList.Count;
            for (int i = count - 1 ; i >= index ; i--)
            {
                mCharInfoList.Remove( mCharInfoList[i] );
            }

            //for (int i = Line.mCharInfoList.Count - 1 ; i >= 0 ; i--)
            //{
            //    if (mCharInfoList.Contains( Line.mCharInfoList[i] ))
            //    {
            //        mCharInfoList.Remove( Line.mCharInfoList[i] );
            //    }
            //}
            //for (int t = toKen.mCharInfoList.Count - 1 ; t >= 0 ; t--)
            //{
            //    if (mCharInfoList.Contains( toKen.mCharInfoList[t] ))
            //    {
            //        mCharInfoList.Remove( toKen.mCharInfoList[t] );
            //    }
            //}
        }

        private string UpdateText ( string outputText, bool isEditor = false )
        {
            if (outputText.Length > totalTextCharInfoList.Count)
            {
                int count = outputText.Length - totalTextCharInfoList.Count;
                for (int i = 0 ; i < count ; i++)
                {
                    totalTextCharInfoList.Add( new CharInfo() );
                }
            }

            for (int i = 0 ; i < totalTextCharInfoList.Count ; i++)
            {
                totalTextCharInfoList[i].Clear();
            }

            foreach (var item in ChildTextDic.Values)
            {
                item.mCharInfoList.Clear();
            }

            if (RichText)
            {
                //更新图
                int _textIndex = 0;
                offsetIndex = 0;

                outputText = UpdataCharInfo( m_Text, 0 ).value;

                offsetIndex = 0;
                StringBuilder _textBuilder = new StringBuilder();
                foreach (Match match in SpriteRegex.Matches( outputText ))
                {
                    if (match.Groups[1].Value == "img")
                    {
                        _textBuilder.Append( outputText.Substring( _textIndex, match.Index - _textIndex ) );

                        _textBuilder.Append( " " );

                        _textIndex = match.Index + match.Length;

                        totalTextCharInfoList[match.Index - offsetIndex].IsSprite = true;
                        totalTextCharInfoList[match.Index - offsetIndex].SpritePath = match.Groups[2].Value;

                        offsetIndex += ( match.Length - 1 );
                    }
                }
                _textBuilder.Append( outputText.Substring( _textIndex, outputText.Length - _textIndex ) );

                outputText = _textBuilder.ToString();
            }
            else
            {
                outputText = m_Text;
            }


            for (int i = 0 ; i < outputText.Length ; i++)
            {
                CharInfo charInfo = null;

                charInfo = totalTextCharInfoList[i];

                if (outputText[i] == '\n' || outputText[i] == '\r')
                {
                    continue;
                }
                if (RichText && charInfo.IsSprite)
                {
                    charInfo.charColor = new Color( 1, 1, 1, 1 );
                    if (UIManager.texturedic != null && UIManager.texturedic.ContainsKey( charInfo.SpritePath ))
                    {
                        BorderData bordata = UIManager.texturedic[charInfo.SpritePath];
                        panelManager.SynGetRes( bordata.path, ( IRes ) =>
                        {
                            if (IRes == null)
                            {
                                return;
                            }
                            var texture = IRes.Res as Texture;
                            int width = 0;
                            int height = 0;
                            int x = 0;
                            int y = 0;
                            if (texture)
                            {
                                AddCharInfo( charInfo, material, texture );
                                width = bordata._w;
                                height = bordata._h;
                                x = bordata._x;
                                y = bordata._y;
                            }
                            charInfo.ch.bearing = 0;
                            charInfo.ch.maxY = height;
                            charInfo.ch.glyphHeight = height;
                            charInfo.ch.glyphWidth = width;
                            charInfo.ch.advance = width;
                            charInfo.ch.uvBottomLeft =  new Vector2( ( float )x / ( float )texture.width,             ( float )y / ( float )texture.height );
                            charInfo.ch.uvBottomRight = new Vector2( ( float )( x + width ) / ( float )texture.width, ( float )y / ( float )texture.height );
                            charInfo.ch.uvTopLeft =     new Vector2( ( float )x / ( float )texture.width,             ( float )( y + height ) / ( float )texture.height );
                            charInfo.ch.uvTopRight =    new Vector2( ( float )( x + width ) / ( float )texture.width, ( float )( y + height ) / ( float )texture.height );
                        }, false );
                    }
                    else
                    {
                        panelManager.SynGetRes( charInfo.SpritePath, ( IRes ) =>
                        {
                            if (IRes == null)
                            {
                                return;
                            }
                            var texture = IRes.Res as Texture;
                            float width = 0;
                            float height = 0;
                            if (texture)
                            {
                                AddCharInfo( charInfo, material, texture );
                                width = texture.width;
                                height = texture.height;
                            }

                            charInfo.ch.bearing = 0;
                            charInfo.ch.maxY = ( int )height;
                            charInfo.ch.glyphHeight = ( int )height;
                            charInfo.ch.glyphWidth = ( int )width;
                            charInfo.ch.uvBottomLeft = new Vector2();
                            charInfo.ch.uvBottomRight = new Vector2( 1, 0 );
                            charInfo.ch.uvTopLeft = new Vector2( 0, 1 );
                            charInfo.ch.uvTopRight = new Vector2( 1, 1 );
                            charInfo.ch.advance = ( int )width;
                        }, false );
                    }
                }
                else if (charInfo.IsUesful && RichText)
                {
                    SFontData sfontdata = fontdata;

                    if (charInfo.outlineSize > 0)
                    {
                        sfontdata.outlinesize = charInfo.outlineSize;
                    }
                    if (charInfo.charSize != 0)
                    {
                        sfontdata.size = charInfo.charSize;
                    }
                    _atlasPack = m_Font.GetAtlas( sfontdata );
                    AtlasData atlasData = atlasPack.SetCharCode( outputText[i] );
                    CharacterInfo ch = atlasData.GetCharInfo( outputText[i] );
                    charInfo.ch = ch;
                    if (OutLineSize > 0)
                    {
                        if (atlasData.OutlineDic.ContainsKey(fontdata.outlineColor))
                        {
                            AddCharInfo( charInfo, atlasData.OutlineDic[fontdata.outlineColor], atlasData.m_texture );
                        }
                    }
                    else
                    {
                        AddCharInfo( charInfo, atlasData.Material, atlasData.m_texture );
                    }
                }
                else
                {
                    if (atlasPack != null)
                    {
                        _atlasPack = m_Font.GetAtlas( fontdata );
                        AtlasData atlasData = atlasPack.SetCharCode( outputText[i] );
                        if (atlasData != null)
                        {
                            charInfo.IsUseGradual = IsUseGradualColor;
                            CharacterInfo ch = atlasData.GetCharInfo( outputText[i] );
                            charInfo.ch = ch;
                            charInfo.charColor = color;

                            if (OutLineSize > 0)
                            {
                                AddCharInfo( charInfo, atlasData.OutlineDic[fontdata.outlineColor], atlasData.m_texture );
                            }
                            else
                            {
                                AddCharInfo( charInfo, atlasData.Material, atlasData.m_texture );
                            }
                        }
                    }
                }
            }

            return outputText;
        }

        private RichTextInfo UpdataCharInfo ( string outputText, int textIndex, bool isNest = false, int tempOffset = 0 )
        {
            int _textIndex = 0;
            StringBuilder _textBuilder = new StringBuilder();

            RichTextInfo richInfo = new RichTextInfo();

            string resultText = outputText;

            foreach (Match match in _InputTagRegex.Matches( outputText ))
            {
                if (match.Groups[1].Value == "link")
                {
                    TextLink.LoginLink( this );

                    int tempid = 0;

                    if (int.TryParse( match.Groups[2].Value, out tempid ))
                    {
                        offsetIndex += ( match.Groups[3].Index - match.Index );

                        RichTextInfo _richInfo = UpdataCharInfo( match.Groups[3].Value, match.Index, true, match.Groups[3].Index - match.Index );

                        if (isNest)
                        {
                            richInfo.otherLength += ( match.Length - match.Groups[3].Length );
                        }

                        _textBuilder.Append( outputText.Substring( _textIndex, match.Index - _textIndex ) );

                        _textBuilder.Append( _richInfo.value );

                        _textIndex = match.Index + match.Length;

                        int offset = offsetIndex - _richInfo.otherLength - tempOffset;

                        for (int i = textIndex + match.Groups[3].Index - offset ; i < textIndex + match.Groups[3].Index - offset + _richInfo.value.Length ; i++)
                        {
                            totalTextCharInfoList[i].IsLink = true;
                            totalTextCharInfoList[i].tag = tempid;
                            totalTextCharInfoList[i].IsUesful = true;
                        }
                        offsetIndex += ( match.Length - ( match.Groups[3].Index - match.Index ) - match.Groups[3].Length );
                    }
                }
                else if (match.Groups[1].Value == "color")
                {
                    Color textColor = colorHx16toRGBA( match.Groups[2].Value );

                    offsetIndex += ( match.Groups[3].Index - match.Index );

                    RichTextInfo _richInfo = UpdataCharInfo( match.Groups[3].Value, match.Index, true, match.Groups[3].Index - match.Index );

                    if (isNest)
                    {
                        richInfo.otherLength += ( match.Length - match.Groups[3].Length );
                    }

                    _textBuilder.Append( outputText.Substring( _textIndex, match.Index - _textIndex ) );

                    _textBuilder.Append( _richInfo.value );

                    _textIndex = match.Index + match.Length;

                    int offset = offsetIndex - _richInfo.otherLength - tempOffset;

                    for (int i = textIndex + match.Groups[3].Index - offset ; i < textIndex + match.Groups[3].Index - offset + _richInfo.value.Length ; i++)
                    {
                        totalTextCharInfoList[i].charColor = textColor;
                        totalTextCharInfoList[i].IsRichColor = true;
                        totalTextCharInfoList[i].IsUesful = true;
                    }
                    offsetIndex += ( match.Length - ( match.Groups[3].Index - match.Index ) - match.Groups[3].Length );
                }
                else if (match.Groups[1].Value == "size")
                {
                    int size = 0;

                    if (int.TryParse( match.Groups[2].Value, out size ))
                    {
                        RichTextInfo _richInfo = UpdataCharInfo( match.Groups[3].Value, match.Index, true, match.Groups[3].Index - match.Index );

                        if (isNest)
                        {
                            richInfo.otherLength += ( match.Length - match.Groups[3].Length );
                        }

                        _textBuilder.Append( outputText.Substring( _textIndex, match.Index - _textIndex ) );

                        _textBuilder.Append( _richInfo.value );

                        _textIndex = match.Index + match.Length;

                        int offset = offsetIndex - _richInfo.otherLength - tempOffset;

                        for (int i = textIndex + match.Groups[3].Index - offset ; i < textIndex + match.Groups[3].Index - offset + _richInfo.value.Length ; i++)
                        {
                            totalTextCharInfoList[i].charSize = size;
                            totalTextCharInfoList[i].IsUesful = true;
                        }
                        offsetIndex += ( match.Length - ( match.Groups[3].Index - match.Index ) - match.Groups[3].Length );
                    }
                }
                else if (match.Groups[1].Value == "outlinesize")
                {
                    int outlinesize = 0;

                    if (int.TryParse( match.Groups[2].Value, out outlinesize ))
                    {
                        RichTextInfo _richInfo = UpdataCharInfo( match.Groups[3].Value, match.Index, true, match.Groups[3].Index - match.Index );

                        if (isNest)
                        {
                            richInfo.otherLength += ( match.Length - match.Groups[3].Length );
                        }

                        _textBuilder.Append( outputText.Substring( _textIndex, match.Index - _textIndex ) );

                        _textBuilder.Append( _richInfo.value );

                        _textIndex = match.Index + match.Length;

                        int offset = offsetIndex - _richInfo.otherLength - tempOffset;

                        for (int i = textIndex + match.Groups[3].Index - offset ; i < textIndex + match.Groups[3].Index - offset + _richInfo.value.Length ; i++)
                        {
                            totalTextCharInfoList[i].outlineSize = outlinesize;
                            totalTextCharInfoList[i].IsUesful = true;
                        }
                        offsetIndex += ( match.Length - ( match.Groups[3].Index - match.Index ) - match.Groups[3].Length );
                    }
                }
                else if (match.Groups[1].Value == "outlinecolor")
                {
                    Color textColor = colorHx16toRGBA( match.Groups[2].Value );

                    RichTextInfo _richInfo = UpdataCharInfo( match.Groups[3].Value, match.Index, true, match.Groups[3].Index - match.Index );

                    if (isNest)
                    {
                        richInfo.otherLength += ( match.Length - match.Groups[3].Length );
                    }

                    _textBuilder.Append( outputText.Substring( _textIndex, match.Index - _textIndex ) );

                    _textBuilder.Append( _richInfo.value );

                    _textIndex = match.Index + match.Length;

                    int offset = offsetIndex - _richInfo.otherLength - tempOffset;

                    for (int i = textIndex + match.Groups[3].Index - offset ; i < textIndex + match.Groups[3].Index - offset + _richInfo.value.Length ; i++)
                    {
                        totalTextCharInfoList[i].outlineColor = textColor;
                        totalTextCharInfoList[i].IsUesful = true;
                    }
                    offsetIndex += ( match.Length - ( match.Groups[3].Index - match.Index ) - match.Groups[3].Length );
                }
            }

            _textBuilder.Append( outputText.Substring( _textIndex, outputText.Length - _textIndex ) );

            outputText = _textBuilder.ToString();

            _textIndex = 0;

            //offsetIndex = 0;

            foreach (Match match in RichRegex.Matches( outputText ))
            {
                if (match.Groups[1].Value == "b")
                {
                    offsetIndex += ( match.Groups[2].Index - match.Index );

                    string _tempTag = UpdataCharInfo( match.Groups[2].Value, match.Index ).value;

                    _textBuilder.Append( outputText.Substring( _textIndex, match.Index - _textIndex ) );

                    _textBuilder.Append( _tempTag );

                    _textIndex = match.Index + match.Length;

                    _textBuilder.Append( outputText.Substring( _textIndex, outputText.Length - _textIndex ) );

                    for (int i = textIndex + match.Index ; i < textIndex + match.Index + _tempTag.Length ; i++)
                    {
                        totalTextCharInfoList[i].IsBold = true;
                        totalTextCharInfoList[i].IsUesful = true;
                    }
                    offsetIndex += ( match.Length - ( match.Groups[2].Index - match.Index ) - match.Groups[2].Length );
                }
            }

            richInfo.value = _textBuilder.ToString();

            return richInfo;
        }

        void AddCharInfo ( CharInfo charInfo, Material material, Texture texture = null )
        {
            Texture tex = null;

            tex = texture;

            if (ChildTextDic.ContainsKey( tex ))
            {
                TextGraphic testGraphic = ChildTextDic[tex];
                if (charInfo.IsSprite && testGraphic)
                {
                    testGraphic.isSprite = true;
                }
                else
                {
                    testGraphic.material = material;
                }

                if (!testGraphic)
                    return;

                if (testGraphic is TextChild)
                {
                    testGraphic.rectTransform.localPosition = Vector2.zero;
                    testGraphic.rectTransform.pivot = rectTransform.pivot;
                    testGraphic.rectTransform.localScale = Vector3.one;
                    testGraphic.rectTransform.sizeDelta = new Vector2( rectTransform.rect.width, rectTransform.rect.height );
                }

                testGraphic.mCharInfoList.Add( charInfo );
                testGraphic.mTexture = tex;
            }
            else
            {
                if (!ChildTextDic.ContainsValue( this ) || ChildTextDic.Count == 0)
                {
                    ChildTextDic.Add( tex, this );
                    mTexture = tex;
                    if (charInfo.IsSprite)
                    {
                        isSprite = true;
                        this.material = Canvas.GetDefaultCanvasMaterial();
                    }
                    else
                    {
                        this.material = material;
                    }
                    mCharInfoList.Add( charInfo );
                }
                else
                {
                    TextChild textChild = null;
                    {
                        if (Application.isPlaying)
                        {
                            GameObject go = new GameObject( "TextChild" );
                            textChild = go.AddComponent<TextChild>();
                            go.transform.SetParent( rectTransform );
                            RectTransform spriteRect = go.transform as RectTransform;

                            Shadow shadow = GetComponent<Shadow>();
                            if (shadow)
                            {
                                Shadow s = go.AddComponent<Shadow>();
                                s.effectColor = shadow.effectColor;
                                s.effectDistance = shadow.effectDistance;
                            }

                            spriteRect.localPosition = Vector3.zero;
                            spriteRect.pivot = rectTransform.pivot;
                            spriteRect.localScale = Vector3.one;
                            spriteRect.sizeDelta = new Vector2( rectTransform.rect.width, rectTransform.rect.height );
                            textChild.mTexture = tex;
                            if (!charInfo.IsSprite)
                            {
                                textChild.material = material;
                            }
                            else
                            {
                                textChild.isSprite = true;
                            }
                            textChild.mCharInfoList.Add( charInfo );
                            textChild.ParentText = this;
                            ChildTextDic.Add( tex, textChild );
                            ChildTextList.Add( textChild );
                        }
                    }
                }
            }
        }

        class RichTextInfo
        {
            public string value;

            public int otherLength;
        }

        class ToKen
        {
            public int breaingX = 0;

            public List<CharInfo> mCharInfoList = new List<CharInfo>();

            public CText ctext;

            private string _Punctuation = "";//",.:;]})|>，。：；】｝）｜》 　";

            public int MaxMaxY = 0;

            public int MaxHeight = 0;

            public float CalWidth = 0;

            public float TotalWidth = 0;

            public int punWidth = 0;

            public CharInfo tempCharInfo;

            public ToKen ( CText _ctext )
            {
                ctext = _ctext;
                if (USystemConfig.Instance.IsLineFeed)
                {
                    _Punctuation = USystemConfig.Instance.Punctuation;
                }
                else
                {
                    _Punctuation = "";
                }
            }

            public void Refresh ( )
            {
                mCharInfoList.Clear();
                if (tempCharInfo != null)
                {
                    mCharInfoList.Add( tempCharInfo );
                    MaxMaxY = tempCharInfo.ch.maxY;
                    CalWidth = tempCharInfo.ch.glyphWidth;
                    TotalWidth = tempCharInfo.ch.advance + ctext.textSpace;
                    MaxHeight = tempCharInfo.ch.glyphHeight;
                    tempCharInfo = null;
                }
                else
                {
                    CalWidth = 0;
                    MaxHeight = 0;
                    MaxMaxY = 0;
                    TotalWidth = 0;
                }
                punWidth = 0;
            }

            public bool AddChar ( char Char, int CharIndex, float InputWidth, CText ctext )
            {
                if (Char == '\n' || Char == '\r')
                {
                    return false;
                }

                CharInfo charInfo = ctext.totalTextCharInfoList[CharIndex];

                CharacterInfo ch = charInfo.ch;

                {
                    float _CalWidth = ch.glyphWidth + TotalWidth + ch.bearing;

                    if (_CalWidth > InputWidth)
                    {
                        tempCharInfo = charInfo;
                        return false;
                    }

                    CalWidth = _CalWidth;
                    TotalWidth += ( ch.advance + ctext.textSpace );

                    if (ch.maxY > MaxMaxY)
                    {
                        MaxMaxY = ch.maxY;
                    }
                    if (ch.glyphHeight > MaxHeight)
                    {
                        MaxHeight = ch.glyphHeight;
                    }
                }

                mCharInfoList.Add( charInfo );
                if (_Punctuation.Contains( Char ))
                {
                    if (Char == ' ' || Char == '　')
                    {
                        punWidth = charInfo.ch.advance;
                    }
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        class TextLine
        {
            public TextLine ( float _posY, int _MaxMaxY )
            {
                posY = _posY;
                MaxMaxY = _MaxMaxY;
                breaingX = 0;
                MaxHeight = 0;
                LineWidth = 0;
                mCharInfoList = new List<CharInfo>();
            }

            public int breaingX;

            public float posY;

            public int MaxMaxY;

            public int MaxHeight;

            public float LineWidth;

            public List<CharInfo> mCharInfoList;

            public bool TryAdd ( ToKen toKen, float TotalWidth, float CalWidth )
            {
                if (TotalWidth - LineWidth >= CalWidth - toKen.punWidth)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void AddToKen ( ToKen toKen, float textSpace )
            {
                if (toKen.MaxMaxY > MaxMaxY || mCharInfoList.Count == 0)
                {
                    MaxMaxY = toKen.MaxMaxY;
                }

                if (toKen.MaxHeight > MaxHeight || mCharInfoList.Count == 0)
                {
                    MaxHeight = toKen.MaxHeight;
                }

                for (int i = 0 ; i < toKen.mCharInfoList.Count ; i++)
                {
                    LineWidth += ( toKen.mCharInfoList[i].ch.advance + textSpace );
                    mCharInfoList.Add( toKen.mCharInfoList[i] );
                }

                toKen.Refresh();
            }

            public void Update ( float X, float Y, float textSpace )
            {
                float posX = 0;
                for (int i = 0 ; i < mCharInfoList.Count ; i++)
                {
                    mCharInfoList[i].TextPos.x = posX + X;
                    mCharInfoList[i].TextPos.y = posY - MaxMaxY + Y;
                    posX += ( mCharInfoList[i].ch.advance + textSpace );
                }
            }

            public void UpdateV ( float TotalWidth, TextAnchor Alignment )
            {
                if (mCharInfoList.Count > 0)
                {
                    float centerOffset = 0;
                    switch (Alignment)
                    {
                        case TextAnchor.LowerCenter:
                            centerOffset = ( TotalWidth - LineWidth + ( mCharInfoList[mCharInfoList.Count - 1].ch.advance - mCharInfoList[mCharInfoList.Count - 1].ch.glyphWidth - mCharInfoList[mCharInfoList.Count - 1].ch.bearing ) - mCharInfoList[0].ch.bearing ) / 2;
                            break;
                        case TextAnchor.LowerLeft:
                            break;
                        case TextAnchor.LowerRight:
                            centerOffset = ( TotalWidth - LineWidth + ( mCharInfoList[mCharInfoList.Count - 1].ch.advance - mCharInfoList[mCharInfoList.Count - 1].ch.glyphWidth - mCharInfoList[mCharInfoList.Count - 1].ch.bearing ) );
                            break;
                        case TextAnchor.MiddleCenter:
                            centerOffset = ( TotalWidth - LineWidth + ( mCharInfoList[mCharInfoList.Count - 1].ch.advance - mCharInfoList[mCharInfoList.Count - 1].ch.glyphWidth - mCharInfoList[mCharInfoList.Count - 1].ch.bearing ) - mCharInfoList[0].ch.bearing ) / 2;
                            break;
                        case TextAnchor.MiddleLeft:
                            break;
                        case TextAnchor.MiddleRight:
                            centerOffset = ( TotalWidth - LineWidth + ( mCharInfoList[mCharInfoList.Count - 1].ch.advance - mCharInfoList[mCharInfoList.Count - 1].ch.glyphWidth - mCharInfoList[mCharInfoList.Count - 1].ch.bearing ) - mCharInfoList[0].ch.bearing );
                            break;
                        case TextAnchor.UpperCenter:
                            centerOffset = ( TotalWidth - LineWidth + ( mCharInfoList[mCharInfoList.Count - 1].ch.advance - mCharInfoList[mCharInfoList.Count - 1].ch.glyphWidth - mCharInfoList[mCharInfoList.Count - 1].ch.bearing ) - mCharInfoList[0].ch.bearing ) / 2;
                            break;
                        case TextAnchor.UpperLeft:
                            break;
                        case TextAnchor.UpperRight:
                            centerOffset = ( TotalWidth - LineWidth + ( mCharInfoList[mCharInfoList.Count - 1].ch.advance - mCharInfoList[mCharInfoList.Count - 1].ch.glyphWidth - mCharInfoList[mCharInfoList.Count - 1].ch.bearing ) - mCharInfoList[0].ch.bearing );
                            break;
                    }
                    for (int i = 0 ; i < mCharInfoList.Count ; i++)
                    {
                        mCharInfoList[i].TextPos = new Vector2( centerOffset + mCharInfoList[i].TextPos.x, mCharInfoList[i].TextPos.y );
                    }
                }
            }

            public void UpdateH ( float offsetY )
            {
                for (int i = 0 ; i < mCharInfoList.Count ; i++)
                {
                    mCharInfoList[i].TextPos = new Vector2( mCharInfoList[i].TextPos.x, mCharInfoList[i].TextPos.y - offsetY );
                }
            }

            public float GetRealWidth ( )
            {
                if (mCharInfoList.Count > 0)
                {
                    return LineWidth - ( mCharInfoList[mCharInfoList.Count - 1].ch.advance - mCharInfoList[mCharInfoList.Count - 1].ch.glyphWidth - mCharInfoList[mCharInfoList.Count - 1].ch.bearing );
                }
                else
                {
                    return 0;
                }
            }
        }

        public class HrefInfo
        {
            public int tag;

            public Rect boxes;
        }

        public static Color colorHx16toRGBA ( string strHxColor )
        {
            if (UIManager.mColorLibray != null && UIManager.mColorLibray.ContainsKey( strHxColor ))
            {
                return UIManager.mColorLibray[strHxColor];
            }

            if (strHxColor.StartsWith( "#" ) && strHxColor.Length == 9)
            {
                return new Color( ( float )System.Int32.Parse( strHxColor.Substring( 1, 2 ), System.Globalization.NumberStyles.AllowHexSpecifier ) / 255, ( float )System.Int32.Parse( strHxColor.Substring( 3, 2 ), System.Globalization.NumberStyles.AllowHexSpecifier ) / 255, ( float )System.Int32.Parse( strHxColor.Substring( 5, 2 ), System.Globalization.NumberStyles.AllowHexSpecifier ) / 255, ( float )System.Int32.Parse( strHxColor.Substring( 7, 2 ), System.Globalization.NumberStyles.AllowHexSpecifier ) / 255 );
            }
            else if (strHxColor.StartsWith( "#" ) && strHxColor.Length == 7)
            {
                return new Color( ( float )System.Int32.Parse( strHxColor.Substring( 1, 2 ), System.Globalization.NumberStyles.AllowHexSpecifier ) / 255, ( float )System.Int32.Parse( strHxColor.Substring( 3, 2 ), System.Globalization.NumberStyles.AllowHexSpecifier ) / 255, ( float )System.Int32.Parse( strHxColor.Substring( 5, 2 ), System.Globalization.NumberStyles.AllowHexSpecifier ) / 255 );
            }
            else
            {
                return Color.white;
            }
        }

        protected override void OnRectTransformDimensionsChange ( )
        {
            IsDirty = true;
            base.OnRectTransformDimensionsChange();
        }

        //待改(统一网络用图做缓存)
        private IEnumerator GetNetImage ( string URL )
        {
            if (NetTextureMap.ContainsKey( URL ))
            {
                yield return null;
            }

            //double startTime = ( double )Time.time;
            WWW www = WWW.LoadFromCacheOrDownload( URL, 0 );
            yield return www;
            if (www != null && string.IsNullOrEmpty( www.error ))
            {
                Texture2D texture = www.texture;
                if (texture && !NetTextureMap.ContainsKey( URL ))
                {
                    NetTextureMap.Add( URL, texture );
                    IsDirty = true;
                }
            }
            else
            {
                ULogger.Error( www.error );
            }
        }
    }

    public class CharInfo
    {
        public bool IsUesful = false;

        public CharacterInfo ch;

        public Vector2 TextPos;

        public int tag;

        public string SpritePath;

        public bool IsSprite;

        public bool IsBold;

        public bool IsLink;

        public int charSize;

        public int outlineSize;

        public Color charColor;

        public bool IsRichColor = false;

        public Color outlineColor;

        public bool IsUseGradual;

        public CharInfo ( ) { }

        public CharInfo ( int _charSize, int _outlineSize, Color _charColor, Color _outlineColor )
        {
            charSize = _charSize;
            outlineSize = _outlineSize;
            charColor = _charColor;
            outlineColor = _outlineColor;
        }

        public void Clear ( )
        {
            IsSprite = false;
            IsBold = false;
            IsLink = false;
            charSize = 0;
            outlineSize = 0;
            IsRichColor = false;
            SpritePath = "";
            tag = 0;
            IsUseGradual = false;
            IsUesful = false;
        }
    }

    public class ColorLibray
    {
        public string mColorName = "Default";

        public float r = 0;

        public float g = 0;

        public float b = 0;

        public float a = 0;
    }
}
