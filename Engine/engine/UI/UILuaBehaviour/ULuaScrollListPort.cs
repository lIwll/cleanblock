using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaScrollListPort : ULuaUIBasePort
    {
        ULuaScrollList m_ScrollList;
        ULuaScrollList ScrollList 
        {
            get
            {
                if (m_ScrollList == null)
                {
                    if (behbase != null)
                    {
                        m_ScrollList = (ULuaScrollList)behbase;
                    }
                }
                return m_ScrollList;
            }
        }

        public void SetTempCount ( int count ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaScrollListPort_SetTempCount_对象被销毁仍调用接口" );
                return;
            }
            ScrollList.SetTempCount( count );
        }

        public void SetContentMode ( bool IsLoop ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaScrollListPort_SetContentMode_对象被销毁仍调用接口" );
                return;
            }
            ScrollList.SetContentMode( IsLoop );
        }

        public void AddLoopItem ( string path, int count, int cache ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaScrollListPort_AddLoopItem_对象被销毁仍调用接口" );
                return;
            }
            ScrollList.AddLoopItem( path, count, cache );
        }

        public void ChangeListCount ( int count ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaScrollListPort_ChangeListCount_对象被销毁仍调用接口" );
                return;
            }
            ScrollList.ChangeListCount( count );
        }

        public void AddUpdateItemEvent ( Action<int, object> updateItem ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaScrollListPort_AddUpdateItemEvent_对象被销毁仍调用接口" );
                return;
            }
            ScrollList.AddUpdateItemEvent( updateItem );
        }

        public void RefreshList ( ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaScrollListPort_RefreshList_对象被销毁仍调用接口" );
                return;
            }
            ScrollList.RefreshList();
        }

        public override int GetUIType ( )
        {
            return ( int )UIType.ScrollList;
        }
    }
}
