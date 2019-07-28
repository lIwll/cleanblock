using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UEngine;

namespace UnityEngine.UI.Extensions
{
    [Serializable]
    [ExecuteInEditMode]
    [AddComponentMenu("UI/Effects/UText")]
    public class UText : BaseMeshEffect
    {
        [SerializeField]
        private float m_spacing = 0f;

        // private string str_Char;
        protected UText() { }

        [SerializeField]
        public Color topColor = Color.white;

        public Color TopColor
        {
            set
            {
                IsChanged = true;
                topColor = value;
            }
        }

        [SerializeField]
        public Color bottomColor = Color.black;

        public Color BottomColor 
        {
            set 
            {
                IsChanged = true;
                bottomColor = value;
            }
        }

        public bool IsGradient = false;

        public bool IsLetterSpacing = false;

        public bool IsOutLine = false;

        public bool FourOutline = true;
        public bool EightOutline = false;

        public bool IsShadow = false;

        int[,] distance = new int[,] { { 1, 1 }, { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 }, { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } };

        private int begin = 0;
        private int CopyCount = 1;

        [SerializeField]
        private Color m_OutLineColor = new Color(0f, 0f, 0f, 0.5f);

        [SerializeField]
        public Vector2 m_OutLineDistance = new Vector2(0, 0);

        private const float kMaxEffectDistance = 600f;

        [SerializeField]
        private Color m_ShadowColor = new Color(0f, 0f, 0f, 0.75f);

        [SerializeField]
        private Vector2 m_ShadowDistance = new Vector2(1f, -1.732f);

        Text _text;
        Text text 
        {
            set 
            {
                _text = value;
            }
            get 
            {
                if (!_text)
                {
                    _text = GetComponent<Text>();
                }
                return _text;
            }
        }

        
        private float leftX, rightX, topY, bottomY, textWidth, textHight;
        public enum FitMode
        {
            Unconstrained,
            MinSize,
            PreferredSize
        }
        [SerializeField]
        public FitMode horizontalFit = FitMode.Unconstrained;
        [SerializeField]
        public FitMode verticalFit = FitMode.Unconstrained;

        public float spacing
        {
            get { return m_spacing; }
            set
            {
                if (m_spacing == value) return;
                m_spacing = value;
                if (graphic != null) graphic.SetVerticesDirty();
            }
        }

        public Color outlineColor
        {
            get { return m_OutLineColor; }
            set
            {
                m_OutLineColor = value;
                IsChanged = true;
                if (graphic != null)
                    graphic.SetVerticesDirty();
            }
        }

        public Vector2 outlineDistance
        {
            get { return m_OutLineDistance; }
            set
            {
                if (value.x > kMaxEffectDistance)
                    value.x = kMaxEffectDistance;
                if (value.x < -kMaxEffectDistance)
                    value.x = -kMaxEffectDistance;

                if (value.y > kMaxEffectDistance)
                    value.y = kMaxEffectDistance;
                if (value.y < -kMaxEffectDistance)
                    value.y = -kMaxEffectDistance;

                if (m_OutLineDistance == value)
                    return;

                m_OutLineDistance = value;

                if (graphic != null)
                    graphic.SetVerticesDirty();
            }
        }

        public Color shadowColor
        {
            get { return m_ShadowColor; }
            set
            {
                m_ShadowColor = value;
                if (graphic != null)
                    graphic.SetVerticesDirty();
            }
        }

        public Vector2 shadowDistance
        {
            get { return m_ShadowDistance; }
            set
            {
                if (value.x > kMaxEffectDistance)
                    value.x = kMaxEffectDistance;
                if (value.x < -kMaxEffectDistance)
                    value.x = -kMaxEffectDistance;

                if (value.y > kMaxEffectDistance)
                    value.y = kMaxEffectDistance;
                if (value.y < -kMaxEffectDistance)
                    value.y = -kMaxEffectDistance;

                if (m_ShadowDistance == value)
                    return;

                m_ShadowDistance = value;

                if (graphic != null)
                    graphic.SetVerticesDirty();
            }
        }

        protected void OnValidate()
        {
            if (!Application.isPlaying)
            {
                IsChanged = true;
            }
            spacing = m_spacing;

#if !UNITY_2018
            //base.OnValidate();
#endif
        }

        protected override void Awake()
        {
            base.Awake();
            if (text)
            {
                textHight = text.rectTransform.rect.height;
                textWidth = text.rectTransform.rect.width;
            }
        }

        string textValue;
        public bool IsChanged = true;
        
        //[SerializeField]
        //UIVertex[] tempverts;

        void Update()
        {
            if (text)
            {
                if (textValue != text.text)
                {
                    textValue = text.text;
                    IsChanged = true;
                }
            }

            //if (!IsChanged)
            //{
            //    return;
            //}

            //if (horizontalFit != FitMode.Unconstrained)
            //{
            //    if (horizontalFit == FitMode.PreferredSize)
            //        text.horizontalOverflow = HorizontalWrapMode.Overflow;
            //}
            //if (verticalFit != FitMode.Unconstrained)
            //{
            //    if (verticalFit == FitMode.PreferredSize)
            //        text.verticalOverflow = VerticalWrapMode.Overflow;
            //}
            //if (IsLetterSpacing)
            //    text.horizontalOverflow = HorizontalWrapMode.Overflow;

        }

        VertexHelper tempVh;
        List<UIVertex> verts;
        public override void ModifyMesh(VertexHelper vh)
        {
            if (vh != tempVh)
            {
                IsChanged = true;
                tempVh = vh;
            }
            if (!IsActive())
                return;

			UProfile.BeginSample("CText ModifyMesh");

            if (verts == null)
            {
                int count = text.text.Length * 4;
                verts = new List<UIVertex>(count * 4);
            }

            vh.GetUIVertexStream(verts);
            if (!text || string.IsNullOrEmpty(text.text))
            {
				UProfile.EndSample();

                return;
            }

            if (true)
            {
                int count = verts.Count;
				if (count < 1)
				{
					UProfile.EndSample();

					return;
				}
                rightX = verts[2].position.x;
                leftX = verts[0].position.x;
                topY = verts[0].position.y;
                bottomY = verts[2].position.y;
                // ApplyGradient(verts, 0, count);
                if (IsLetterSpacing)
                {
                    if (horizontal == Horizontal.Wrap)
                        ModifyVerticesOverflow(verts);
                    else if (horizontal == Horizontal.Overflow)
                    {
                        ModifyVertices(verts);
                    }
                }

                PreffWidth(verts);
            
                PreffHight(verts);

                if (IsGradient)
                    ApplyGradient(verts, 0, count);

                if (IsOutLine)
                {
                    if (FourOutline)
                    {
                        EightOutline = false;
                        CopyCount = 5;
                    }
                    if (EightOutline)
                    {
                        FourOutline = false;
                        CopyCount = 9;
                    }
                    if (!FourOutline && !EightOutline)
                    {
                        CopyCount = 1;
                    }

                    begin = 1;

                }
                else
                {
                    CopyCount = 1;
                }
                if (IsShadow)
                {
                    begin = 0;
                }

                if (IsShadow || IsOutLine)
                    ApplyCopy(verts, 0, count);

                //tempverts.Clear();
                //if (verts.Count > 0)
                //{
                //    tempverts = new UIVertex[verts.Count];
                //    verts.CopyTo(tempverts);
                //}
                IsChanged = false;
            }
            else
            {
                //verts.Clear();
                //if (tempverts != null)
                //{
                //    for (int i = 0; i < tempverts.Length; i++)
                //    {
                //        verts.Add(tempverts[i]);
                //    }
                //}
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(verts);
            verts.Clear();

			UProfile.EndSample();
        }

        #region 颜色渐变
        private void ApplyGradient(List<UIVertex> vertexList, int start, int end)
        {
            float bottomY = vertexList[0].position.y;
            float topY = vertexList[0].position.y;
            for (int i = start; i < end; ++i)
            {
                float y = vertexList[i].position.y;
                if (y > topY)
                {
                    topY = y;
                }
                else if (y < bottomY)
                {
                    bottomY = y;
                }
            }

            float uiElementHeight = topY - bottomY;

            for (int i = start; i < end; ++i)
            {
                UIVertex uiVertex = vertexList[i];
                uiVertex.color = Color.Lerp(bottomColor, topColor, (uiVertex.position.y - bottomY) / uiElementHeight);
                vertexList[i] = uiVertex;
            }
        }
        #endregion

        #region  阴影、描边
        void ApplyCopy(List<UIVertex> verts, int start, int end)
        {
            bool isShadow = false;
            for (int i = begin; i < CopyCount; i++)
            {
                if (i != 0)
                {
                    if (isShadow)
                    {
                        start = end;
                        end = verts.Count;
                    }
                    ApplyShadowZeroAlloc(verts, 1, outlineColor, start, end, distance[i, 0] * outlineDistance.x, distance[i, 1] * outlineDistance.y);
                    isShadow = true;
                }
            }
            start = 0;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, 0, shadowColor, start, end, distance[0, 0] * shadowDistance.x, distance[0, 1] * shadowDistance.y);
        }

        protected void ApplyShadowZeroAlloc(List<UIVertex> verts, int index, Color32 color, int start, int end, float x, float y)
        {
            UIVertex vt;

            var neededCapacity = verts.Count + end - start;
            if (verts.Capacity < neededCapacity)
                verts.Capacity = neededCapacity * 2;

            for (int i = start; i < end; ++i)
            {
                vt = verts[i];
                verts.Add(vt);

                Vector3 v = vt.position;
                v.x += x;
                v.y += y;
                vt.position = v;
                var newColor = color;
                //if (m_UseGraphicAlpha[index])
                newColor.a = (byte)((newColor.a * verts[i].color.a) / 255);
                vt.color = newColor;
                verts[i] = vt;
            }
        }

        #endregion

        #region Spacing

        private const string SupportedTagRegexPattersn = @"<b>|</b>|<i>|</i>|<size=.*?>|</size>|<color=.*?>|</color>|<material=.*?>|</material>";
        public void ModifyVertices(List<UIVertex> verts)
        {
            if (!IsActive()) return;

            string str = text.text;

            // Artificially insert line breaks for automatic line breaks.
            IList<UILineInfo> lineInfos = text.cachedTextGenerator.lines;
            for (int i = lineInfos.Count - 1; i > 0; i--)
            {
                // Insert a \n at the location Unity wants to automatically line break.
                // Also, remove any space before the automatic line break location.
                if (str[lineInfos[i].startCharIdx] != '\n')
                    str = str.Insert(lineInfos[i].startCharIdx, "\n");
            }

            string[] lines = str.Split('\n');

            if (text == null)
            {
                Debug.LogWarning("LetterSpacing: Missing Text component");
                return;
            }

            Vector3 pos;
            float letterOffset = spacing * (float)text.fontSize / 100f;
            float alignmentFactor = 0;
            int glyphIdx = 0;  // character index from the beginning of the text, including RichText tags and line breaks

            bool isRichText = text.supportRichText;
            IEnumerator matchedTagCollection = null; // when using RichText this will collect all tags (index, length, value)
            Match currentMatchedTag = null;

            switch (text.alignment)
            {
                case TextAnchor.LowerLeft:
                case TextAnchor.MiddleLeft:
                case TextAnchor.UpperLeft:
                    alignmentFactor = 0f;
                    break;

                case TextAnchor.LowerCenter:
                case TextAnchor.MiddleCenter:
                case TextAnchor.UpperCenter:
                    alignmentFactor = 0.5f;
                    break;

                case TextAnchor.LowerRight:
                case TextAnchor.MiddleRight:
                case TextAnchor.UpperRight:
                    alignmentFactor = 1f;
                    break;
            }

            for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
            {
                string line = lines[lineIdx];
                int lineLength = line.Length;
                if (line == "")
                {
                    glyphIdx++;
                    continue;
                }
                if (isRichText)
                {
                    matchedTagCollection = GetRegexMatchedTagCollection(line, out lineLength);
                    currentMatchedTag = null;
                    if (matchedTagCollection.MoveNext())
                    {
                        currentMatchedTag = (Match)matchedTagCollection.Current;
                    }
                }

                float lineOffset = (lineLength - 1) * letterOffset * alignmentFactor;

                for (int charIdx = 0, actualCharIndex = 0; charIdx < line.Length; charIdx++, actualCharIndex++)
                {
                    if (isRichText)
                    {
                        if (currentMatchedTag != null && currentMatchedTag.Index == charIdx)
                        {
                            // skip matched RichText tag
                            charIdx += currentMatchedTag.Length - 1;  // -1 because next iteration will increment charIdx
                            actualCharIndex--;                          // tag is not an actual character, cancel counter increment on this iteration
                            glyphIdx += currentMatchedTag.Length;      // glyph index is not incremented in for loop so skip entire length

                            // prepare next tag to detect
                            currentMatchedTag = null;
                            if (matchedTagCollection.MoveNext())
                            {
                                currentMatchedTag = (Match)matchedTagCollection.Current;
                            }

                            continue;
                        }
                    }

                    int idx1 = glyphIdx * 6 + 0;
                    int idx2 = glyphIdx * 6 + 1;
                    int idx3 = glyphIdx * 6 + 2;
                    int idx4 = glyphIdx * 6 + 3;
                    int idx5 = glyphIdx * 6 + 4;
                    int idx6 = glyphIdx * 6 + 5;

                    // Check for truncated text (doesn't generate verts for all characters)
                    if (idx6 > verts.Count - 1) return;

                    UIVertex vert1 = verts[idx1];
                    UIVertex vert2 = verts[idx2];
                    UIVertex vert3 = verts[idx3];
                    UIVertex vert4 = verts[idx4];
                    UIVertex vert5 = verts[idx5];
                    UIVertex vert6 = verts[idx6];

                    pos = Vector3.right * (letterOffset * actualCharIndex - lineOffset);

                    vert1.position += pos;
                    vert2.position += pos;
                    vert3.position += pos;
                    vert4.position += pos;
                    vert5.position += pos;
                    vert6.position += pos;

                    //rightX = rightX > vert4.position.x ? rightX : vert2.position.x;
                    //leftX = leftX < vert1.position.x ? leftX : vert1.position.x;
                    //topY = topY > vert1.position.y ? topY : vert1.position.y;
                    //bottomY = bottomY < vert4.position.y ? bottomY : vert4.position.y;

                    verts[idx1] = vert1;
                    verts[idx2] = vert2;
                    verts[idx3] = vert3;
                    verts[idx4] = vert4;
                    verts[idx5] = vert5;
                    verts[idx6] = vert6;

                    glyphIdx++;
                }

                // Offset for carriage return character that still generates verts
            }
        }

        public enum Horizontal { Wrap = 0, Overflow };

        public enum Vertical
        {
            Truncate,
            Overflow
        };
        [SerializeField]
        public Horizontal horizontal = Horizontal.Overflow;
        [SerializeField]
        public Vertical vertical = Vertical.Overflow;
        public void ModifyVerticesOverflow(List<UIVertex> verts)
        {
            if (!IsActive()) return;

            string str = text.text;

            int startCharIndex = 0;
            int startVerIndex = 0;
            int lineCount = 0;
            List<UIVertex> newList = new List<UIVertex>();

            for (int i = 0; i < verts.Count; i++)
            {
                UIVertex vert = verts[i];
                newList.Add(vert);
            }

            string[] lines = str.Split('\n');

            if (text == null)
            {
                Debug.LogWarning("LetterSpacing: Missing Text component");
                return;
            }

            Vector3 pos;
            float letterOffset = spacing * (float)text.fontSize / 100f;
            float alignmentFactor = 0;
            int glyphIdx = 0;  // character index from the beginning of the text, including RichText tags and line breaks
            bool isRichText = text.supportRichText;
            IEnumerator matchedTagCollection = null; // when using RichText this will collect all tags (index, length, value)
            Match currentMatchedTag = null;

            switch (text.alignment)
            {
                case TextAnchor.LowerLeft:
                case TextAnchor.MiddleLeft:
                case TextAnchor.UpperLeft:
                    alignmentFactor = 0f;
                    break;

                case TextAnchor.LowerCenter:
                case TextAnchor.MiddleCenter:
                case TextAnchor.UpperCenter:
                    alignmentFactor = 0.5f;
                    break;

                case TextAnchor.LowerRight:
                case TextAnchor.MiddleRight:
                case TextAnchor.UpperRight:
                    alignmentFactor = 1f;
                    break;
            }

            for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
            {
                startCharIndex = 0;
                startVerIndex = 6 * glyphIdx;
                string line = lines[lineIdx];
                int lineLength = line.Length;
                if (line == "")
                {
                    glyphIdx++;
                    lineCount++;
                    continue;
                }
                if (isRichText)
                {
                    matchedTagCollection = GetRegexMatchedTagCollection(line, out lineLength);
                    currentMatchedTag = null;
                    if (matchedTagCollection.MoveNext())
                    {
                        currentMatchedTag = (Match)matchedTagCollection.Current;
                    }
                }

                float lineOffset = (lineLength - 1) * letterOffset * alignmentFactor;
                int actualCharIndex = 0;
                for (int charIdx = 0; charIdx < line.Length; charIdx++)
                {

                    if (isRichText)
                    {
                        if (currentMatchedTag != null && currentMatchedTag.Index == charIdx)
                        {
                            // skip matched RichText tag
                            charIdx += currentMatchedTag.Length - 1;  // -1 because next iteration will increment charIdx
                            actualCharIndex += currentMatchedTag.Length;                          // tag is not an actual character, cancel counter increment on this iteration
                            glyphIdx += currentMatchedTag.Length;      // glyph index is not incremented in for loop so skip entire length

                            // prepare next tag to detect
                            currentMatchedTag = null;
                            if (matchedTagCollection.MoveNext())
                            {
                                currentMatchedTag = (Match)matchedTagCollection.Current;
                            }

                            continue;
                        }
                    }

                    int idx1 = glyphIdx * 6 + 0;
                    int idx2 = glyphIdx * 6 + 1;
                    int idx3 = glyphIdx * 6 + 2;
                    int idx4 = glyphIdx * 6 + 3;
                    int idx5 = glyphIdx * 6 + 4;
                    int idx6 = glyphIdx * 6 + 5;

                    // Check for truncated text (doesn't generate verts for all characters)
                    if (idx6 > verts.Count - 1) return;

                    UIVertex vert1 = verts[idx1];
                    UIVertex vert2 = verts[idx2];
                    UIVertex vert3 = verts[idx3];
                    UIVertex vert4 = verts[idx4];
                    UIVertex vert5 = verts[idx5];
                    UIVertex vert6 = verts[idx6];

                    pos = Vector3.right * (letterOffset * (charIdx - startCharIndex - actualCharIndex) - lineOffset);

                    vert1.position = newList[idx1].position - newList[startVerIndex + 0].position + pos +
                                     verts[startVerIndex + 0].position;
                    vert2.position = newList[idx2].position - newList[startVerIndex + 0].position + pos +
                                     verts[startVerIndex + 0].position;
                    vert3.position = newList[idx3].position - newList[startVerIndex + 0].position + pos +
                                     verts[startVerIndex + 0].position;
                    vert4.position = newList[idx4].position - newList[startVerIndex + 0].position + pos +
                                     verts[startVerIndex + 0].position;
                    vert5.position = newList[idx5].position - newList[startVerIndex + 0].position + pos +
                                     verts[startVerIndex + 0].position;
                    vert6.position = newList[idx6].position - newList[startVerIndex + 0].position + pos +
                                     verts[startVerIndex + 0].position;

                    if (vert2.position.x > text.rectTransform.rect.width * 0.5f || (charIdx == 0))
                    {
                        lineCount++;
                        Vector3 midPos = 0.5f * (newList[idx1].position + newList[idx3].position);
                        midPos -= new Vector3(0, text.cachedTextGenerator.lines[lineIdx].height * text.lineSpacing * (lineCount - 1 - lineIdx));
                        vert1.position = new Vector3(-text.rectTransform.rect.width * 0.5f, midPos.y + (newList[idx1].position.y - newList[idx3].position.y) * 0.5f);
                        vert2.position = vert1.position + newList[idx2].position - newList[idx1].position;
                        vert3.position = vert1.position + newList[idx3].position - newList[idx1].position;
                        vert4.position = vert1.position + newList[idx4].position - newList[idx1].position;
                        vert5.position = vert1.position + newList[idx5].position - newList[idx1].position;
                        vert6.position = vert1.position + newList[idx6].position - newList[idx1].position;

                        startCharIndex = charIdx;
                        startVerIndex = idx1;
                        actualCharIndex = 0;
                    }
                    verts[idx1] = vert1;
                    verts[idx2] = vert2;
                    verts[idx3] = vert3;
                    verts[idx4] = vert4;
                    verts[idx5] = vert5;
                    verts[idx6] = vert6;

                    glyphIdx++;
                }
                glyphIdx++;
                // Offset for carriage return character that still generates verts
            }
        }

        private IEnumerator GetRegexMatchedTagCollection(string line, out int lineLengthWithoutTags)
        {
            MatchCollection matchedTagCollection = Regex.Matches(line, SupportedTagRegexPattersn);
            lineLengthWithoutTags = 0;
            int tagsLength = 0;

            if (matchedTagCollection.Count > 0)
            {
                for (int i = 0; i < matchedTagCollection.Count; ++i )
                {
                    tagsLength += matchedTagCollection[i].Length;
                }
            }
            lineLengthWithoutTags = line.Length - tagsLength;
            return matchedTagCollection.GetEnumerator();
        }
        #endregion

        #region  Content Size Fitter
        void PreffWidth(List<UIVertex> verts)
        {
            if (horizontalFit == FitMode.Unconstrained)
                return;
            else if (horizontalFit == FitMode.MinSize)
            {
                weilingx = textWidth;
                textWidth = 0;
            }
            else
            {
                GetRightValue(verts);
                //if (rightX > 0.5f*text.rectTransform.rect.width)
                //{
                textWidth = rightX - leftX + text.fontSize / 5;
                float diffwidth = textWidth - text.rectTransform.rect.width;
                //  text.rectTransform.sizeDelta=new Vector2(width,hight);

                for (int i = 0; i < verts.Count; i++)
                {
                    UIVertex vert = verts[i];
                    vert.position += new Vector3(-(diffwidth - text.fontSize / 5) * 0.5f, 0);
                    verts[i] = vert;
                }
                if (textWidth == 0)
                    textWidth = weilingx;
                // }
            }

            text.rectTransform.sizeDelta = new Vector2(textWidth, text.rectTransform.rect.height);
        }

        private float weilingx, weilingy;
        void PreffHight(List<UIVertex> verts)
        {
            if (verticalFit == FitMode.Unconstrained)
                return;
            else if (verticalFit == FitMode.MinSize)
            {
                weilingy = textHight;
                textHight = 0;
            }
            else
            {
                GetRightValue(verts);
                //if (bottomY > 0.5f * text.rectTransform.rect.height)
                //{
                textHight = topY - bottomY + text.fontSize / 5;
                float diffhight = textHight - text.rectTransform.rect.height;
                //text.rectTransform.sizeDelta = new Vector2(width, hight);

                for (int i = 0; i < verts.Count; i++)
                {
                    UIVertex vert = verts[i];
                    vert.position += new Vector3(0, 0.5f * (diffhight + text.fontSize / 10));
                    verts[i] = vert;
                }
                if (textHight == 0)
                    textHight = weilingy;
            }

            text.rectTransform.sizeDelta = new Vector2(text.rectTransform.rect.width, textHight);
        }

        void GetRightValue(List<UIVertex> vertsList)
        {
            if (vertsList.Count == 0)
                return;
			for (int i = 0; i < vertsList.Count; ++ i)
			{
				UIVertex vertex = vertsList[i];

				rightX = rightX > vertex.position.x ? rightX : vertex.position.x;
				leftX = leftX < vertex.position.x ? leftX : vertex.position.x;
				topY = topY > vertex.position.y ? topY : vertex.position.y;
				bottomY = bottomY < vertex.position.y ? bottomY : vertex.position.y;
			}
        }

        #endregion

    }
}
