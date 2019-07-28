using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaScrollbar : ULuaUIBase 
    {
        Scrollbar _scrollbar;
        Scrollbar scrollbar
        {
            get
            {
                if (!_scrollbar)
                {
                    _scrollbar = GetComponent<Scrollbar>();
                }
                return _scrollbar;
            }
        }

        Action<float> onValueChange;

        void Awake()
        {
            scrollbar.onValueChanged.AddListener(OnClickLuaFunc);
        }

        public void SetBarValue(float value)
        {
            if (value < 0 || value > 1)
            {
                ULogger.Error("设置的值应该介于0 - 1之间");
                return;
            }
            else
            {
                scrollbar.value = value;
            }
        }

        void OnClickLuaFunc(float value)
        {
            if (onValueChange != null)
            {
                onValueChange( value );
            }
        }

        public void AddValueChangeEvent(Action<float> luafunc)
        {
            onValueChange = luafunc;
        }

        public void RemoveValueChangeEvent(int index)
        {
        }

        public void RemoveAllValueChangeEvent()
        {
        }

        public override void OnClose ( )
        {
            base.OnClose();
            onValueChange = null;
        }
    }
}
