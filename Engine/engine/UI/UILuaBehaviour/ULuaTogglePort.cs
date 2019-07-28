using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaTogglePort : ULuaUIBasePort
    {
        ULuaToggle _toggle;
        ULuaToggle toggle
        {
            get
            {
                if (_toggle == null)
                {
                    if (behbase != null)
                    {
                        _toggle = (ULuaToggle)behbase;
                    }
                }
                return _toggle;
            }
        }

        public bool GetIsOnState() 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaTogglePort_GetIsOnState_对象被销毁仍调用接口" );
                return false;
            }
            return toggle.GetIsOnState();
        }

        public void SetToggleState(bool ison) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaTogglePort_SetToggleState_对象被销毁仍调用接口" );
                return;
            }
            toggle.SetToggleState(ison);
        }

        public void AddToggleValueChangeEvent(Action<bool> func) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaTogglePort_AddToggleValueChangeEvent_对象被销毁仍调用接口" );
                return;
            }
            toggle.AddToggleValueChangeEvent(func);
        }

        public void RemoveAllValueChangeEvent() 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaTogglePort_RemoveAllValueChangeEvent_对象被销毁仍调用接口" );
                return;
            }
            toggle.RemoveAllValueChangeEvent();
        }

        public void RemoveValueChangeEvent(int index) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaTogglePort_RemoveValueChangeEvent_对象被销毁仍调用接口" );
                return;
            }
            toggle.RemoveValueChangeEvent(index);
        }

        public void SetBackGroundImage ( string respath, bool IsAsyn = UIManager.IsAsyn )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaTogglePort_SetBackGroundImage_对象被销毁仍调用接口" );
                return;
            }
            toggle.SetBackGroundImage( respath, IsAsyn );
        }

        public void SetCheckMarkImage ( string respath, bool IsAsyn = UIManager.IsAsyn )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaTogglePort_SetCheckMarkImage_对象被销毁仍调用接口" );
                return;
            }
            toggle.SetCheckMarkImage( respath, IsAsyn );
        }

        public void SetChangeColor ( uint color ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaTogglePort_SetChangeColor_对象被销毁仍调用接口" );
                return;
            }
            toggle.ChangeColor = UIManager.RGBA( color );
        }

        public override int GetUIType()
        {
            return (int)UIType.Toggle;
        }
    }
}
