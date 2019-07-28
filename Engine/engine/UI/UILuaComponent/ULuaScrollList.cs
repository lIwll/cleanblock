using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaScrollList : ULuaUIBase
    {
        UScrollList m_ScrollList;
        UScrollList ScrollList
        {
            get
            {
                if (!m_ScrollList)
                {
                    m_ScrollList = GetComponent<UScrollList>();
                }
                return m_ScrollList;
            }
        }

        LayoutGroup layoutGroup;
        LayoutGroup LayoutGroup
        {
            get
            {
                if (!layoutGroup)
                {
                    layoutGroup = GetComponent<LayoutGroup>();
                }
                return layoutGroup;
            }
        }

        ContentSizeFitter sizeFitter;
        ContentSizeFitter SizeFitter
        {
            get
            {
                if (!sizeFitter)
                {
                    sizeFitter = GetComponent<ContentSizeFitter>();
                }
                return sizeFitter;
            }
        }

        public void SetTempCount ( int count )
        {
            ScrollList.ItemCache = count;
        }

        public void SetContentMode ( bool IsLoop )
        {
            if (IsLoop)
            {
                if (LayoutGroup)
                    LayoutGroup.enabled = false;

                if (SizeFitter)
                    SizeFitter.enabled = false;

                ScrollList.CaluContentHeight();
            }
            else
            {
                if (LayoutGroup)
                    LayoutGroup.enabled = true;

                if (SizeFitter)
                    SizeFitter.enabled = true;
            }
        }

        public void AddLoopItem ( string path, int count, int cache )
        {
            path = "Data/bytes/" + path + ".byte";
            PanelManagerPort tempmanager ;
            //if (panelmanager.ObjectPool.ContainsKey( path ))
            //{
            //    tempmanager = new PanelManagerPort();
            //    tempmanager.panelmanager = PanelManager.ManagerInit( panelmanager.ObjectPool[path], panelmanager );
            //}
            //else
            //{
            //    tempmanager = UILoader.LoadUIData( path, false, UIManager.UICachePool.transform );
            //    panelmanager.ObjectPool.Add( path, tempmanager.panelmanager.gameObject );
            //}

            tempmanager = UILoader.LoadUIData( path, false, transform, true );
            panelmanager.ChildPanel_List.Add( tempmanager.panelmanager );
            if (tempmanager.panelmanager)
            {
                tempmanager.panelmanager.IsClone = false;
                ScrollList.CreateItem( this, tempmanager.panelmanager.transform as RectTransform, count, cache );
            }
        }

        public void ChangeListCount ( int count ) 
        {
            ScrollList.ItemCount = count;
        }

        public void AddUpdateItemEvent ( Action<int, object> updateItem )
        {
            ScrollList.UpdateItem = updateItem;
        }

        public void RefreshList ( )
        {
            ScrollList.UpdateContent( recttransform.anchoredPosition.y, true );
        }

        public override void RemoveAllChild ( )
        {
            ScrollList.ItemQueue.Clear();
            base.RemoveAllChild();
        }

        public override void OnClose ( )
        {
            base.OnClose();
            ScrollList.UpdateItem = null;
        }
    }
}
