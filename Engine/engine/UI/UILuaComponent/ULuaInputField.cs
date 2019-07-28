using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaInputField : ULuaUIBase
    {
        InputField _inputfield;
        InputField inputfield
        {
            get
            {
                if (!_inputfield)
                {
                    _inputfield = GetComponent<InputField>();
                    _inputfield.shouldHideMobileInput = false;
                }
                return _inputfield;
            }
        }

        Action<string> onEndEdit;
        Action<string> onValueChange;

        public delegate string OnValidateT(string s, int n, char c);
		OnValidateT onValidate;

        void Awake()
        {
            inputfield.onValueChanged.AddListener(OnValueChangeFunc);
            inputfield.onEndEdit.AddListener(OnEndEditLuaFunc);
        }

        public override void SetText(string value)
        {
            inputfield.text = value;
        }

        public override string GetText()
        {
            return inputfield.text;
        }

        void OnEndEditLuaFunc(string value)
        {
            if (onEndEdit != null)
            {
                onEndEdit(value);
            }
        }

        void OnValueChangeFunc(string value) 
        {
            if (onValueChange != null)
            {
                onValueChange( value );
            }
        }

        char OnValidateInput(string text, int Charindex, char addedChar) 
        {
            return (char)onValidate( text, Charindex, addedChar )[0];
        }

        public void AddEndEditEvent(Action<string> luafunc)
        {
            onEndEdit = luafunc;
        }

        public void RemoveEndEditEvent(int index)
        {
        }

        public void RemoveAllEndEditEvent()
        {
        }

        public void AddValueChangeEvent(Action<string> luafunc)
        {
            onValueChange = luafunc;
        }

        public void RemoveValueChangeEvent(int index) 
        {
        }

        public void RemoveAllValueChangeEvent() 
        {
        }

        public void AddOnValidateInputEvent(OnValidateT luafunc) 
        {
            inputfield.onValidateInput = OnValidateInput;
            onValidate = luafunc;
        }

        public void RemoveValidateInputEvent(int index) 
        {
        }

        public void RemoveAllValidateInputEvent() 
        {
        }

        public void SetCharacterLimit ( int LimitCount ) 
        {
            inputfield.characterLimit = LimitCount;
        }

        public override void OnClose ( )
        {
            base.OnClose();
            onEndEdit = null;
            onValueChange = null;
        }
    }
}
