using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaInputFieldPort : ULuaUIBasePort
    {
        ULuaInputField _inputfield;
        ULuaInputField inputfield
        {
            get
            {
                if (_inputfield == null)
                {
                    if (behbase != null)
                    {
                        _inputfield = (ULuaInputField)behbase;
                    }
                }
                return _inputfield;
            }
        }
        public override void SetText(string value)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaInputFieldPort_SetText_对象被销毁仍调用接口" );
                return;
            }
            inputfield.SetText(value);
        }

        public override string GetText()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaInputFieldPort_GetText_对象被销毁仍调用接口" );
                return "";
            }
            return inputfield.GetText();
        }

        public void AddEndEditEvent(Action<string> luafunc)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaInputFieldPort_AddEndEditEvent_对象被销毁仍调用接口" );
                return;
            }
            inputfield.AddEndEditEvent(luafunc);
        }

        public void RemoveEndEditEvent(int index)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaInputFieldPort_RemoveEndEditEvent_对象被销毁仍调用接口" );
                return;
            }
            inputfield.RemoveEndEditEvent(index);
        }

        public void RemoveAllEndEditEvent()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaInputFieldPort_RemoveAllEndEditEvent_对象被销毁仍调用接口" );
                return;
            }
            inputfield.RemoveAllEndEditEvent();
        }

        public void AddValueChangeEvent(Action<string> luafunc)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaInputFieldPort_AddValueChangeEvent_对象被销毁仍调用接口" );
                return;
            }
            inputfield.AddValueChangeEvent(luafunc);
        }

        public void RemoveValueChangeEvent(int index) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaInputFieldPort_RemoveValueChangeEvent_对象被销毁仍调用接口" );
                return;
            }
            inputfield.RemoveValueChangeEvent(index);
        }

        public void RemoveAllValueChangeEvent() 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaInputFieldPort_RemoveAllValueChangeEvent_对象被销毁仍调用接口" );
                return;
            }
            inputfield.RemoveAllValueChangeEvent();
        }

		public void AddOnValidateInputEvent(ULuaInputField.OnValidateT luafunc) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaInputFieldPort_AddOnValidateInputEvent_对象被销毁仍调用接口" );
                return;
            }
            inputfield.AddOnValidateInputEvent(luafunc);
        }

        public void RemoveValidateInputEvent(int index) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaInputFieldPort_RemoveValidateInputEvent_对象被销毁仍调用接口" );
                return;
            }
            inputfield.RemoveValidateInputEvent(index);
        }

        public void RemoveAllValidateInputEvent() 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaInputFieldPort_RemoveAllValidateInputEvent_对象被销毁仍调用接口" );
                return;
            }
            inputfield.RemoveAllValidateInputEvent();
        }

        public void SetCharacterLimit ( int LimitCount ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaInputFieldPort_SetCharacterLimit_对象被销毁仍调用接口" );
                return;
            }
            inputfield.SetCharacterLimit( LimitCount );
        }

        public override int GetUIType()
        {
            return (int)UIType.InputField;
        }
    }
}
