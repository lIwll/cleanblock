using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UEngine.UIExpand;
using UEngine.UI.UILuaBehaviour;

public class UScrollList : MonoBehaviour
{
    public Action<int, object> UpdateItem;

    public RectOffset Padding;

    public Vector2 CellSize;

    public Vector2 CellOffset;

    int itemCount;
    public int ItemCount
    {
        get
        {
            return itemCount;
        }
        set
        {
            int offset = value - itemCount;
            if (offset > 0)
            {
                for (int i = itemCount ; i < value ; i++)
                {
                    AddPosToList( i );
                }
            }
            else if (offset < 0)
            {
                itemPosList.RemoveRange( value, -offset );
            }
            else
            {
                return;
            }
            
            countY = value / countX;
            ContentHeight = ( float )countY * ( CellSize.y + CellOffset.y ) - CellOffset.y + Padding.top + Padding.bottom;
            itemCount = value;
        }
    }

    int itemCache;
    public int ItemCache
    {
        get
        {
            return itemCache;
        }
        set
        {
            if (CellSize.x > 0 && CellSize.y > 0)
            {
                viewY = ( int )( ViewportHeight / ( CellSize.y + CellOffset.y ) );
                if (value > ( viewY + 2 ) * countX)
                {
                    itemCache = value;
                }
                else
                {
                    itemCache = ( viewY + 2 ) * countX;
                }
            }
        }
    }

    public int countX = 0;
    int countY = 0;
    int viewY = 0;

    RectTransform rectTransform;
    RectTransform mRect
    {
        get
        {
            if (!rectTransform)
            {
                rectTransform = transform as RectTransform;
            }
            return rectTransform;
        }
    }

    RectTransform mCell;

    UScrollRect parentScrollRect;
    UScrollRect ParentScrollRect
    {
        get
        {
            if (!parentScrollRect)
                parentScrollRect = GetComponentInParent<UScrollRect>();

            return parentScrollRect;
        }
    }

    Dictionary<int, Vector2> itemPosMap = new Dictionary<int, Vector2>();
    public List<Vector2> itemPosList = new List<Vector2>();

    public Queue<PanelManagerPort> ItemQueue = new Queue<PanelManagerPort>();

    float ContentWidth
    {
        get
        {
            return mRect.rect.width;
        }
    }

    float ContentHeight
    {
        get
        {
            return mRect.sizeDelta.y;
        }
        set
        {
            mRect.sizeDelta = new Vector2( mRect.sizeDelta.x, value );
        }
    }

    float ViewportHeight
    {
        get { return ParentScrollRect.viewport.rect.height; }
    }

    public void CreateItem ( ULuaScrollList luaScrollList, RectTransform Cell, int Count, int Cache )
    {
        mCell = Cell;
        mCell.anchorMin = new Vector2( 0, 1 );
        mCell.anchorMax = new Vector2( 0, 1 );
        mCell.pivot = new Vector2( 0, 1 );
        mCell.sizeDelta = CellSize;
        ItemCount = Count;
        ItemCache = Cache;
        ParentScrollRect.UpdateContent = UpdateContent;

        for (int i = 0 ; i < ItemCache ; i++)
        {
            RectTransform item = Instantiate( Cell, transform );
            item.gameObject.SetActive( true );
            PanelManagerPort port = new PanelManagerPort();
            port.panelmanager = item.GetComponent<PanelManager>();
            port.panelmanager.ParentPanel = luaScrollList.panelmanager;
            luaScrollList.panelmanager.ChildPanel_List.Add( port.panelmanager );
            port.panelmanager.PanelManagerInit();
            UpdatePostion( i, port );
            if (UpdateItem != null)
            {
                UpdateItem( i, port );
            }
        }
    }

    public void AddPosToList ( int index ) 
    {
        int row  = ( int )( ( float )index / ( float )countX );  //行
        int file = ( int )( ( float )index % ( float )countX );  //列
        float x = Padding.left + file * ( CellSize.x + CellOffset.x );
        float y = Padding.top + row * ( CellSize.y + CellOffset.y );
        itemPosList.Add( new Vector2( x, -y ) );
    }

    void UpdatePostion ( int index, PanelManagerPort port )
    {
        port.panelmanager.rectTransform.anchoredPosition = itemPosList[index];
        ItemQueue.Enqueue( port );
    }

    float tempPos = 0;
    public void UpdateContent ( float contentPos, bool IsManul = false )
    {
        if (ItemQueue.Count == 0)
            return;

        if (contentPos < 0)
            contentPos = 0;

        float maxPos = ContentHeight - ViewportHeight;
        if (contentPos > maxPos)
            contentPos = maxPos;

        if (Mathf.Abs( contentPos - tempPos ) < 5 && !IsManul)
            return;

        float contenPosOffset = contentPos - Padding.top;
        if (contenPosOffset < 0)
            contenPosOffset = 0;

        int cow = ( int )( contenPosOffset / ( CellSize.y + CellOffset.y ) );
        int count = ( cow + viewY + 2 ) * countX;
        int offsetCount = 0;
        if (count > ItemCount)
        {
            offsetCount = count - itemCount;
            count = ItemCount;
        }
        for (int i = cow * countX ; i < count ; i++)
        {
            PanelManagerPort port = ItemQueue.Dequeue();
            UpdatePostion( i, port );

            if (UpdateItem != null)
                UpdateItem( i, port );
        }
        for (int i = cow * countX - 1 ; i > cow * countX - 1 - offsetCount ; i--)
        {
            PanelManagerPort port = ItemQueue.Dequeue();
            UpdatePostion( i, port );
        }
        tempPos = contentPos;
    }

    public void CaluContentHeight ( ) 
    {
        countY = itemCount / countX;
        ContentHeight = ( float )countY * ( CellSize.y + CellOffset.y ) - CellOffset.y + Padding.top + Padding.bottom;
    }

    void Update ( )
    {
    }
}
