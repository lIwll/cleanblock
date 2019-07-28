using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Collections;
using UEngine.UIExpand;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaDropDownPort : ULuaUIBasePort
    {
        ULuaDropDown _dropdown;
        ULuaDropDown dropdown
        {
            get
            {
                if (_dropdown == null)
                {
                    if (behbase != null)
                    {
                        _dropdown = (ULuaDropDown)behbase;
                    }
                }
                return _dropdown;
            }
        }
        public void AddValueChangeEvent(Action<int> luafunc)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaDropDownPort_AddValueChangeEvent_对象被销毁仍调用接口" );
                return;
            }
            dropdown.AddValueChangeEvent(luafunc);
        }

        public void RemoveValueChangeEvent(int index)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaDropDownPort_RemoveValueChangeEvent_对象被销毁仍调用接口" );
                return;
            }
            dropdown.RemoveValueChangeEvent(index);
        }

        public void RemoveAllValueChangeEvent()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaDropDownPort_RemoveAllValueChangeEvent_对象被销毁仍调用接口" );
                return;
            }
            dropdown.RemoveAllValueChangeEvent();
        }

        public void SetDropOption ( int index, string value, string SpritePath = "", bool IsAsyn = UIManager.IsAsyn ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaDropDownPort_SetDropOption_对象被销毁仍调用接口" );
                return;
            }
            dropdown.SetDropOption(index, value, SpritePath, IsAsyn);
        }

        public void AddDropOption ( string value, string SpritePath = "", bool IsAsyn = UIManager.IsAsyn ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaDropDownPort_AddDropOption_对象被销毁仍调用接口" );
                return;
            }
            dropdown.AddDropOption( value, SpritePath, IsAsyn );
        }

        public int GetValue()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaDropDownPort_GetValue_对象被销毁仍调用接口" );
                return 0;
            }
            return dropdown.GetValue();
        }

        public void SetValue(int value) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaDropDownPort_SetValue_对象被销毁仍调用接口" );
                return;
            }
            dropdown.SetValue(value);
        }

        public string GetDropDownCaptionValue() 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaDropDownPort_GetDropDownCaptionValue_对象被销毁仍调用接口" );
                return "";
            }
            return dropdown.GetDropDownCaptionValue();
        }

        public void RemoveOption(int index) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaDropDownPort_RemoveOption_对象被销毁仍调用接口" );
                return;
            }
            dropdown.RemoveOption(index);
        }

        public void RemoveAll() 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaDropDownPort_RemoveAll_对象被销毁仍调用接口" );
                return;
            }
            dropdown.RemoveAll();
        }

        public int GetOptionCount() 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaDropDownPort_GetOptionCount_对象被销毁仍调用接口" );
                return 0;
            }
            return dropdown.GetOptionCount();
        }

        public override int GetUIType()
        {
            return (int)UIType.Dropdown;
        }
    }
}
