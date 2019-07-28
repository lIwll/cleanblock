using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UEngine.UIExpand;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaScrollRect : ULuaUIBase
    {
        ScrollRect _scrollrect;
        ScrollRect scrollrect
        {
            get
            {
                if (!_scrollrect)
                {
                    _scrollrect = GetComponent<ScrollRect>();
                }
                return _scrollrect;
            }
        }

        RectTransform ViewRect 
        {
            get 
            {
                if (scrollrect)
                {
                    return scrollrect.viewport;
                }
                return recttransform;
            }
        }

        UScrollRect uscrollrect 
        {
            get 
            {
                if (scrollrect.GetType() == typeof(UScrollRect))
                    return (UScrollRect)scrollrect;
                else
                    return null;
            }
        }

        ULuaBehaviourBase _content;
        ULuaBehaviourBase content 
        {
            get 
            {
                if (!_content)
                {
                    _content = scrollrect.content.GetComponent<ULuaBehaviourBase>();
                    if (!_content)
                    {
                        if (scrollrect.content.GetComponent<ToggleGroup>())
                        {
                            _content = scrollrect.content.gameObject.AddComponent<ULuaToggleGroup>();
                        }
                        else
                        {
                            _content = scrollrect.content.gameObject.AddComponent<ULuaBehaviourBase>();
                        }
                        _content.panelmanager = panelmanager;
                    }
                }
                return _content;
            }
        }

        UScrollList m_ScrollList;
        UScrollList ScrollList
        {
            get
            {
                if (!m_ScrollList)
                {
                    m_ScrollList = scrollrect.content.GetComponent<UScrollList>();
                }
                return m_ScrollList;
            }
        }

        LayoutGroup m_layoutGroup;

        LayoutGroup LayoutGroup 
        {
            get 
            {
                if (!m_layoutGroup)
                {
                    m_layoutGroup = scrollrect.content.GetComponent<LayoutGroup>();
                }
                return m_layoutGroup;
            }
        }

        Vector2 targetVec2;
        Vector2 currentVec2;

        float time = 0;
        float velocity;

        public override PanelManagerPort AddItem(string path, bool isCache , bool active )
        {
            return content.AddItem( path, isCache, active );
        }

        public override PanelManagerPort[] AddItem ( string path, int num, bool isCache, bool active )
        {
            return content.AddItem( path, num, isCache, active );
        }

        public override void RemoveAllChild()
        {
            content.RemoveAllChild();
        }

        public void SetContentLayoutEnable ( bool IsEnable ) 
        {
            ContentSizeFitter contentSF = content.GetComponent<ContentSizeFitter>();
            if (contentSF != null)
            {
                contentSF.enabled = IsEnable;
            }

            if (LayoutGroup != null)
            {
                LayoutGroup.enabled = IsEnable;
            }
        }

        public RectOffset GetContentLayoutPadding ( ) 
        {
            if (LayoutGroup != null)
            {
                return LayoutGroup.padding;
            }
            else
            {
                return new RectOffset();
            }
        }

        public void SetContentPos( int index, bool isElastic )
        {
            scrollrect.StopMovement();
            
            if (uscrollrect && uscrollrect.isPlay)
            {
                uscrollrect.isPlay = false;
            }
            if (scrollrect.vertical && !scrollrect.horizontal)
            {
                int count = 0;
                for (int i = 0; i < content.transform.childCount; i++)
                {
                    Transform child = content.transform.GetChild(i);
                    if (child.gameObject.activeSelf)
                        count++;
                }

                if (index > count)
                {
                    ULogger.Error("设置的滑块位置越界");
                    return;
                }

                float currentPos = content.recttransform.anchoredPosition.y;

                List<float> childheight = new List<float>();

                GridLayoutGroup gridgroup = content.GetComponent<GridLayoutGroup>();
                HorizontalOrVerticalLayoutGroup horvgroup = content.GetComponent<HorizontalOrVerticalLayoutGroup>();

                float top = 0;
                float spacing = 0;

                if (gridgroup)
                {
                    int constrainCount = 1;
                    if (gridgroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
                    {
                        constrainCount = gridgroup.constraintCount;
                    }
                    count = count / constrainCount + count % constrainCount;
                    top = gridgroup.padding.top;
                    spacing = gridgroup.spacing.y;
                    float totalHeight = gridgroup.cellSize.y * count + spacing * (count - 1) + top;
                    if (totalHeight <= ViewRect.rect.height)
                    {
                        return;
                    }
                    float result = gridgroup.cellSize.y * (index - 1) + spacing * (index - 1) + top;

                    if (result > currentPos && result + gridgroup.cellSize.y < currentPos + ViewRect.rect.height)
                    {
                        return;
                    }

                    if (result <= totalHeight - ViewRect.rect.height)
                    {
                        if (isElastic && uscrollrect)
                        {
                            targetVec2 = new Vector2(content.recttransform.anchoredPosition.x, result);
                            currentVec2 = content.recttransform.anchoredPosition;
                            uscrollrect.isPlay = true;
                            float distance = Mathf.Abs(targetVec2.y - currentVec2.y);
                            CalcuTime(distance);
                        }
                        else
                        {
                            content.recttransform.anchoredPosition = new Vector2(content.recttransform.anchoredPosition.x, result);
                        }
                    }
                    else
                    {
                        if (isElastic && uscrollrect)
                        {
                            targetVec2 = new Vector2( content.recttransform.anchoredPosition.x, totalHeight - ViewRect.rect.height );
                            currentVec2 = content.recttransform.anchoredPosition;
                            uscrollrect.isPlay = true;
                            float distance = Mathf.Abs(targetVec2.y - currentVec2.y);
                            CalcuTime(distance);
                        }
                        else
                        {
                            content.recttransform.anchoredPosition = new Vector2( content.recttransform.anchoredPosition.x, totalHeight - ViewRect.rect.height );
                        }
                    }
                }

                if (horvgroup)
                {
                    for (int i = 0; i < content.transform.childCount; i++)
                    {
                        Transform child = content.transform.GetChild(i);
                        if (child.gameObject.activeSelf)
                        {
                            RectTransform recttrans = child as RectTransform;
                            childheight.Add(recttrans.sizeDelta.y);
                        }
                    }
                    top = horvgroup.padding.top;
                    spacing = horvgroup.spacing;
                    float totalHeight = top - spacing;

                    for (int i = 0; i < count; i++)
                    {
                        totalHeight += childheight[i] + spacing;
                    }
                    
                    if (totalHeight <= ViewRect.rect.height)
                    {
                        return;
                    }
                    float result = top - spacing;

                    for (int i = 0; i < index - 1; i++)
                    {
                        result += childheight[i] + spacing;
                    }

                    if (result > currentPos && result + 100 < currentPos + ViewRect.rect.height)
                    {
                        return;
                    }

                    if (result <= totalHeight - ViewRect.rect.height)
                    {
                        if (isElastic && uscrollrect)
                        {
                            targetVec2 = new Vector2(content.recttransform.anchoredPosition.x, result);
                            currentVec2 = content.recttransform.anchoredPosition;
                            uscrollrect.isPlay = true;
                            float distance = Mathf.Abs(targetVec2.y - currentVec2.y);
                            CalcuTime(distance);
                        }
                        else
                        {
                            content.recttransform.anchoredPosition = new Vector2(content.recttransform.anchoredPosition.x, result);
                        }
                    }
                    else
                    {
                        if (isElastic && uscrollrect)
                        {
                            targetVec2 = new Vector2( content.recttransform.anchoredPosition.x, totalHeight - ViewRect.rect.height );
                            currentVec2 = content.recttransform.anchoredPosition;
                            uscrollrect.isPlay = true;
                            float distance = Mathf.Abs(targetVec2.y - currentVec2.y);
                            CalcuTime(distance);
                        }
                        else
                        {
                            content.recttransform.anchoredPosition = new Vector2( content.recttransform.anchoredPosition.x, totalHeight - ViewRect.rect.height );
                        }
                    }
                }
            }
            else if (!scrollrect.vertical && scrollrect.horizontal)
            {
                int count = 0;
                for (int i = 0 ; i < content.transform.childCount ; i++)
                {
                    Transform child = content.transform.GetChild( i );
                    if (child.gameObject.activeSelf)
                        count++;
                }

                if (index > count)
                {
                    ULogger.Error( "设置的滑块位置越界" );
                    return;
                }

                float currentPos = content.recttransform.anchoredPosition.x;

                List<float> childheight = new List<float>();

                GridLayoutGroup gridgroup = content.GetComponent<GridLayoutGroup>();
                HorizontalOrVerticalLayoutGroup horvgroup = content.GetComponent<HorizontalOrVerticalLayoutGroup>();

                float left = 0;
                float spacing = 0;

                if (gridgroup)
                {
                    int constrainCount = 1;
                    if (gridgroup.constraint == GridLayoutGroup.Constraint.FixedRowCount)
                    {
                        constrainCount = gridgroup.constraintCount;
                    }
                    count = count / constrainCount + count % constrainCount;

                    left = gridgroup.padding.left;
                    spacing = gridgroup.spacing.x;
                    float totalHeight = gridgroup.cellSize.x * count + spacing * ( count - 1 ) + left;
                    if (totalHeight <= ViewRect.rect.width)
                    {
                        return;
                    }
                    float result = gridgroup.cellSize.x * ( index - 1 ) + spacing * ( index - 1 ) + left;

                    if (result < currentPos && result + gridgroup.cellSize.x < currentPos + ViewRect.rect.width)
                    {
                        return;
                    }

                    if (result <= totalHeight - ViewRect.rect.width)
                    {
                        if (isElastic && uscrollrect)
                        {
                            targetVec2 = new Vector2( result, content.recttransform.anchoredPosition.y );
                            currentVec2 = content.recttransform.anchoredPosition;
                            uscrollrect.isPlay = true;
                            float distance = Mathf.Abs( targetVec2.x - currentVec2.x );
                            CalcuTime( distance );
                        }
                        else
                        {
                            content.recttransform.anchoredPosition = new Vector2( result, content.recttransform.anchoredPosition.y );
                        }
                    }
                    else
                    {
                        if (isElastic && uscrollrect)
                        {
                            targetVec2 = new Vector2( totalHeight - ViewRect.rect.width, content.recttransform.anchoredPosition.y );
                            currentVec2 = content.recttransform.anchoredPosition;
                            uscrollrect.isPlay = true;
                            float distance = Mathf.Abs( targetVec2.x - currentVec2.x );
                            CalcuTime( distance );
                        }
                        else
                        {
                            content.recttransform.anchoredPosition = new Vector2( ViewRect.rect.width - totalHeight, content.recttransform.anchoredPosition.y );
                        }
                    }
                }

                if (horvgroup)
                {
                    for (int i = 0 ; i < content.transform.childCount ; i++)
                    {
                        Transform child = content.transform.GetChild( i );
                        if (child.gameObject.activeSelf)
                        {
                            RectTransform recttrans = child as RectTransform;
                            childheight.Add( recttrans.sizeDelta.x );
                        }
                    }
                    left = horvgroup.padding.left;
                    spacing = horvgroup.spacing;
                    float totalHeight = left - spacing;

                    for (int i = 0 ; i < count ; i++)
                    {
                        totalHeight += childheight[i] + spacing;
                    }
                    if (totalHeight <= ViewRect.rect.width)
                    {
                        return;
                    }
                    float result = left;

                    for (int i = 0 ; i < index - 1 ; i++)
                    {
                        result += childheight[i] + spacing;
                    }

                    if (result < currentPos && result + 100 < currentPos + ViewRect.rect.width)
                    {
                        return;
                    }

                    if (result <= totalHeight - ViewRect.rect.width)
                    {
                        if (isElastic && uscrollrect)
                        {
                            targetVec2 = new Vector2( result, content.recttransform.anchoredPosition.y );
                            currentVec2 = content.recttransform.anchoredPosition;
                            uscrollrect.isPlay = true;
                            float distance = Mathf.Abs( targetVec2.x - currentVec2.x );
                            CalcuTime( distance );
                        }
                        else
                        {
                            content.recttransform.anchoredPosition = new Vector2( result, content.recttransform.anchoredPosition.y );
                        }
                    }
                    else
                    {
                        if (isElastic && uscrollrect)
                        {
                            targetVec2 = new Vector2( totalHeight - ViewRect.rect.width, content.recttransform.anchoredPosition.y );
                            currentVec2 = content.recttransform.anchoredPosition;
                            uscrollrect.isPlay = true;
                            float distance = Mathf.Abs( targetVec2.x - currentVec2.x );
                            CalcuTime( distance );
                        }
                        else
                        {
                            content.recttransform.anchoredPosition = new Vector2( ViewRect.rect.width - totalHeight, content.recttransform.anchoredPosition.y );
                        }
                    }
                }
            }
        }

        public void SetContentPos ( float pos, bool isElastic ) 
        {
            scrollrect.StopMovement();
            if (uscrollrect && uscrollrect.isPlay)
            {
                uscrollrect.isPlay = false;
            }
            if (scrollrect.vertical && !scrollrect.horizontal) 
            {
                if (isElastic && uscrollrect)
                {
                    targetVec2 = new Vector2( content.recttransform.anchoredPosition.x, pos );
                    currentVec2 = content.recttransform.anchoredPosition;
                    uscrollrect.isPlay = true;
                    float distance = Mathf.Abs( targetVec2.y - currentVec2.y );
                    CalcuTime( distance );
                }
                else
                {
                    content.recttransform.anchoredPosition = new Vector2( content.recttransform.anchoredPosition.x, pos );
                }
            }
            else if (!scrollrect.vertical && scrollrect.horizontal)
            {
                if (isElastic && uscrollrect)
                {
                    targetVec2 = new Vector2( pos, content.recttransform.anchoredPosition.y );
                    currentVec2 = content.recttransform.anchoredPosition;
                    uscrollrect.isPlay = true;
                    float distance = Mathf.Abs( targetVec2.x - currentVec2.x );
                    CalcuTime( distance );
                }
                else
                {
                    content.recttransform.anchoredPosition = new Vector2( pos, content.recttransform.anchoredPosition.y );
                }
            }
        }

        public void SetLoopContentPos ( int index, bool isElastic ) 
        {
            scrollrect.StopMovement();

            if (uscrollrect && uscrollrect.isPlay)
            {
                uscrollrect.isPlay = false;
            }
            if (scrollrect.vertical && !scrollrect.horizontal && ScrollList) 
            {
                int count = ScrollList.ItemCount;
                if (index > count)
                {
                    ULogger.Error( "设置的滑块位置越界" );
                    return;
                }

                float currentPos = content.recttransform.anchoredPosition.y;
                float top = 0;
                float spacing = 0;
                top = ScrollList.Padding.top;
                spacing = ScrollList.CellOffset.y;
                float totalHeight = ScrollList.CellSize.y * count + spacing * ( count - 1 ) + top;
                if (totalHeight <= ViewRect.rect.height)
                {
                    return;
                }
                float result = ScrollList.CellSize.y * ( index - 1 ) + spacing * ( index - 1 ) + top;

                if (result > currentPos && result + ScrollList.CellSize.y < currentPos + ViewRect.rect.height)
                {
                    return;
                }

                if (result <= totalHeight - ViewRect.rect.height)
                {
                    if (isElastic && uscrollrect)
                    {
                        targetVec2 = new Vector2( content.recttransform.anchoredPosition.x, result );
                        currentVec2 = content.recttransform.anchoredPosition;
                        uscrollrect.isPlay = true;
                        float distance = Mathf.Abs( targetVec2.y - currentVec2.y );
                        CalcuTime( distance );
                    }
                    else
                    {
                        content.recttransform.anchoredPosition = new Vector2( content.recttransform.anchoredPosition.x, result );
                        if (uscrollrect.UpdateContent != null)
                            uscrollrect.UpdateContent( content.recttransform.anchoredPosition.y, false );
                    }
                }
                else
                {
                    if (isElastic && uscrollrect)
                    {
                        targetVec2 = new Vector2( content.recttransform.anchoredPosition.x, totalHeight - ViewRect.rect.height );
                        currentVec2 = content.recttransform.anchoredPosition;
                        uscrollrect.isPlay = true;
                        float distance = Mathf.Abs( targetVec2.y - currentVec2.y );
                        CalcuTime( distance );
                    }
                    else
                    {
                        content.recttransform.anchoredPosition = new Vector2( content.recttransform.anchoredPosition.x, totalHeight - ViewRect.rect.height );
                        if (uscrollrect.UpdateContent != null)
                            uscrollrect.UpdateContent( content.recttransform.anchoredPosition.y, false );
                    }
                }
            }
        }

        void CalcuTime(float distance) 
        {
            if (distance > 1000)
            {
                time = distance / 10000;
            }
            else if (distance > 500 && distance <= 1000)
            {
                time = distance / 6000;
            }
            else if (distance > 300 && distance <= 500)
            {
                time = distance / 1500;
            }
            else if (distance > 100 && distance <= 300)
            {
                time = distance / 1000;
            }
            else
            {
                time = distance / 700;
            }
        }

        void Update()
        {
            if (scrollrect.vertical && !scrollrect.horizontal && uscrollrect && uscrollrect.isPlay)
            {
                if (Mathf.Abs(targetVec2.y - content.recttransform.anchoredPosition.y) >= 0.01f)
                {
                    float y = Mathf.SmoothDamp(content.recttransform.anchoredPosition.y, targetVec2.y, ref velocity, time);
                    content.recttransform.anchoredPosition = new Vector2(content.recttransform.anchoredPosition.x, y);
                    if (uscrollrect.UpdateContent != null)
                        uscrollrect.UpdateContent( content.recttransform.anchoredPosition.y, false );
                }
                else
                {
                    uscrollrect.isPlay = false;
                }
            }

            if (!scrollrect.vertical && scrollrect.horizontal && uscrollrect && uscrollrect.isPlay)
            {
                if (Mathf.Abs( targetVec2.x - content.recttransform.anchoredPosition.x ) >= 0.01f)
                {
                    float x = Mathf.SmoothDamp( content.recttransform.anchoredPosition.x, -targetVec2.x, ref velocity, time );
                    content.recttransform.anchoredPosition = new Vector2( x, content.recttransform.anchoredPosition.y );
                    //if (uscrollrect.UpdateContent != null)
                    //    uscrollrect.UpdateContent( content.recttransform.anchoredPosition.y, false );
                }
                else
                {
                    uscrollrect.isPlay = false;
                }
            }
        }

        public void AddDragEvent(Action<float> luafunc)
        {
            if (uscrollrect)
            {
                uscrollrect.onDrag = luafunc;
            }
        }

        public override void RemoveAllDragEvent()
        {
        }

        public override void RemoveDragEvent(int index)
        {
        }

    }
}
