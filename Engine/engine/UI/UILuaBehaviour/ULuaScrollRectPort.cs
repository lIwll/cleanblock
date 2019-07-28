using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaScrollRectPort : ULuaUIBasePort
    {
        ULuaScrollRect _scroolrect;
        ULuaScrollRect scroolrect
        {
            get
            {
                if (_scroolrect == null)
                {
                    if (behbase != null)
                    {
                        _scroolrect = (ULuaScrollRect)behbase;
                    }
                }
                return _scroolrect;
            }
        }

        public override PanelManagerPort AddItem ( string path, bool isCache = true, bool active = true )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaScrollRectPort_AddItem_对象被销毁仍调用接口" );
                return new PanelManagerPort();
            }
            return scroolrect.AddItem( path, isCache, active );
        }

        public override PanelManagerPort[] AddItem ( string path, int num, bool isCache = true, bool active = true )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaScrollRectPort_AddItem_对象被销毁仍调用接口" );
                return new PanelManagerPort[0];
            }
            return scroolrect.AddItem( path, num, isCache, active );
        }

        public override void RemoveAllChild()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaScrollRectPort_RemoveAllChild_对象被销毁仍调用接口" );
                return;
            }
            scroolrect.RemoveAllChild();
        }

        public void SetContentLayoutEnable ( bool IsEnable ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaScrollRectPort_SetContentLayoutEnable_对象被销毁仍调用接口" );
                return;
            }
            scroolrect.SetContentLayoutEnable( IsEnable );
        }

        public RectOffset GetContentLayoutPadding ( ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaScrollRectPort_GetContentLayoutPadding_对象被销毁仍调用接口" );
                return new RectOffset();
            }
            return scroolrect.GetContentLayoutPadding( );
        }

        public void SetContentPos(int index, bool isElastic = false)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaScrollRectPort_SetContentPos_对象被销毁仍调用接口" );
                return;
            }
            scroolrect.SetContentPos(index, isElastic);
        }

        public void SetContentPos ( float pos, bool isElastic = false ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaScrollRectPort_SetContentPos_对象被销毁仍调用接口" );
                return;
            }
            scroolrect.SetContentPos( pos, isElastic );
        }

        public void SetLoopContentPos ( int index, bool isElastic = false ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaScrollRectPort_SetLoopContentPos_对象被销毁仍调用接口" );
                return;
            }
            scroolrect.SetLoopContentPos( index, isElastic );
        }

        public override int GetUIType()
        {
            return (int)UIType.ScrollRect;
        }

        public void AddDragEvent(Action<float> luafunc)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaScrollRectPort_AddDragEvent_对象被销毁仍调用接口" );
                return;
            }
            scroolrect.AddDragEvent(luafunc);
        }

        public override void RemoveAllDragEvent()
        {
        }

        public override void RemoveDragEvent(int index)
        {
        }
    }
}
