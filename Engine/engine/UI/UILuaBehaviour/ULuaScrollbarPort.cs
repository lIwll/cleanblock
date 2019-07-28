using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaScrollbarPort : ULuaUIBasePort
    {
        ULuaScrollbar _scrollbar;
        ULuaScrollbar scrollbar
        {
            get
            {
                if (_scrollbar == null)
                {
                    if (behbase != null)
                    {
                        _scrollbar = (ULuaScrollbar)behbase;
                    }
                }
                return _scrollbar;
            }
        }

        public void SetBarValue(float value)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaScrollbarPort_SetBarValue_对象被销毁仍调用接口" );
                return;
            }
            scrollbar.SetBarValue(value);
        }

        public void AddValueChangeEvent(Action<float> luafunc)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaScrollbarPort_AddValueChangeEvent_对象被销毁仍调用接口" );
                return;
            }
            scrollbar.AddValueChangeEvent(luafunc);
        }

        public void RemoveValueChangeEvent(int index)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaScrollbarPort_RemoveValueChangeEvent_对象被销毁仍调用接口" );
                return;
            }
            scrollbar.RemoveValueChangeEvent(index);
        }

        public void RemoveAllValueChangeEvent()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaScrollbarPort_RemoveAllValueChangeEvent_对象被销毁仍调用接口" );
                return;
            }
            scrollbar.RemoveAllValueChangeEvent();
        }

        public override int GetUIType()
        {
            return (int)UIType.Scrollbar;
        }
    }
}
