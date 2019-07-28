using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using UEngine.FreeType;
using UEngine.UI.UILuaBehaviour;

namespace UEngine.UIExpand
{
    [ExecuteInEditMode]
    public class TextGraphic : Text
    {
        [HideInInspector][SerializeField]
        public Texture mTexture;

        public List<CharInfo> mCharInfoList = new List<CharInfo>();

        protected readonly UIVertex[] m_TempVerts = new UIVertex[4];

        public bool isSprite = false;

        protected override void Awake ( )
        {
            if (font == null)
            {
                if (UIManager.emtyFont == null)
                {
                    UIManager.emtyFont = new Font();
                }
                font = UIManager.emtyFont;
            }
            base.Awake();
        }

        public override UnityEngine.Texture mainTexture
        {
            get
            {
                if (mTexture)
                {
                    return mTexture;
                }
                return base.mainTexture;
            }
        }

        [SerializeField]
        public SFontData fontdata = new SFontData { size = 14 };

        public override Material material
        {
            get
            {
                if (m_Material != null)
                {
                    if (isSprite)
                    {
                        m_Material = Canvas.GetDefaultCanvasMaterial();
                    }
                }
                else
                {
                    if (isSprite && m_Material != Canvas.GetDefaultCanvasMaterial())
                    {
                        m_Material = Canvas.GetDefaultCanvasMaterial();
                    }
                }
                return m_Material;
            }
            set
            {
                m_Material = value;
            }
        }
    }
}
