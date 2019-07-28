using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

namespace UEngine.UIExpand
{
    public class TextChild : TextGraphic
    {
        public CText ParentText;

        protected override void Start ( ) 
        {
            if (ParentText)
            {
                raycastTarget = ParentText.raycastTarget;

                ParentText.RegisterDirtyMaterialCallback( ( ) => 
                {
                    SetAllDirty();
                } );
            }
        }

        void Update ( ) 
        {
            if (ParentText)
            {
                if (rectTransform.localPosition != Vector3.zero || rectTransform.localEulerAngles != Vector3.zero || rectTransform.sizeDelta != ParentText.rectTransform.sizeDelta)
                {
                    rectTransform.localEulerAngles = Vector3.zero;
                    rectTransform.localPosition = Vector3.zero;
                    rectTransform.pivot = ParentText.rectTransform.pivot;
                    rectTransform.sizeDelta = new Vector2( ParentText.rectTransform.rect.width, ParentText.rectTransform.rect.height );
                    SetAllDirty();
                }
            }
        }

        protected override void OnPopulateMesh ( VertexHelper vh )
        {
            vh.Clear();

            if (ParentText)
            {
                ParentText.UpdateLinesInfo();
            }

            for (int i = 0; i < mCharInfoList.Count; i++)
            {
                if (ParentText.TextIsPlaying)
                {
                    int index = ParentText.totalTextCharInfoList.LastIndexOf( mCharInfoList[i] );
                    if (index >= ParentText.TextAniIndex)
                    {
                        break;
                    }
                }

                CharacterInfo ch = mCharInfoList[i].ch;
                Vector2 textPos = mCharInfoList[i].TextPos;
                Color mColor = mCharInfoList[i].charColor;
                Vector2 lowerLeft = new Vector2(textPos.x + ch.bearing, textPos.y - (ch.glyphHeight - ch.maxY));
                Vector2 topRight = new Vector2(textPos.x + ch.glyphWidth + ch.bearing, textPos.y + ch.maxY);

                Color topColor;
                Color bottomColor;

                if (mCharInfoList[i].IsSprite)
                {
                    topColor = Color.white;
                    bottomColor = Color.white;
                }
                else
                {
                    if (mCharInfoList[i].IsUseGradual)
                    {
                        topColor = ParentText.TopColor;
                        bottomColor = ParentText.BottomColor;
                        topColor = new Color( ParentText.TopColor.r, ParentText.TopColor.g, ParentText.TopColor.b, ParentText.TopColor.a * ParentText.color.a );
                        bottomColor = new Color( ParentText.BottomColor.r, ParentText.BottomColor.g, ParentText.BottomColor.b, ParentText.BottomColor.a * ParentText.color.a );
                    }
                    else
                    {
                        if (mCharInfoList[i].IsRichColor)
                        {
                            topColor = new Color( mColor.r, mColor.g, mColor.b, mColor.a * ParentText.color.a ); ;
                            bottomColor = topColor;
                        }
                        else
                        {
                            topColor = ParentText.color;
                            bottomColor = ParentText.color;
                        }
                    }
                }

                m_TempVerts[0].position = new Vector3(lowerLeft.x, lowerLeft.y);
                m_TempVerts[0].color = topColor;
                m_TempVerts[0].uv0 = ch.uvBottomLeft;
                m_TempVerts[1].position = new Vector3(topRight.x, lowerLeft.y);
                m_TempVerts[1].color = topColor;
                m_TempVerts[1].uv0 = ch.uvBottomRight;
                m_TempVerts[2].position = new Vector3(topRight.x, topRight.y);
                m_TempVerts[2].color = bottomColor;
                m_TempVerts[2].uv0 = ch.uvTopRight;
                m_TempVerts[3].position = new Vector3(lowerLeft.x, topRight.y);
                m_TempVerts[3].color = bottomColor;
                m_TempVerts[3].uv0 = ch.uvTopLeft;
                vh.AddUIVertexQuad(m_TempVerts);
                if (mCharInfoList[i].IsLink)
                {
                    var pos = m_TempVerts[0].position;
                    var bounds = new Bounds( pos, Vector3.zero );
                    bounds.Encapsulate( m_TempVerts[2].position );
                    ParentText.mHreInfoList.Add( new CText.HrefInfo { tag = mCharInfoList[i].tag, boxes = new Rect( bounds.min, bounds.size ) } );

                    m_TempVerts[0].position = new Vector3( textPos.x, textPos.y - 4 );
                    m_TempVerts[0].uv0 = new Vector2( 1023.5f / 1024f, 1023.5f / 1024f );
                    m_TempVerts[1].position = new Vector3( textPos.x + ch.advance + ParentText.textSpace, textPos.y - 4 );
                    m_TempVerts[1].uv0 = new Vector2( 1f, 1023.5f / 1024f );
                    m_TempVerts[2].position = new Vector3( textPos.x + ch.advance + ParentText.textSpace, textPos.y - 2 );
                    m_TempVerts[2].uv0 = new Vector2( 1f, 1f );
                    m_TempVerts[3].position = new Vector3( textPos.x, textPos.y - 2 );
                    m_TempVerts[3].uv0 = new Vector2( 1023.5f / 1024f, 1f );
                    vh.AddUIVertexQuad( m_TempVerts );
                }
            }
        }

        protected override void OnRectTransformDimensionsChange ( )
        {
            
            base.OnRectTransformDimensionsChange();
        }

    }
}
