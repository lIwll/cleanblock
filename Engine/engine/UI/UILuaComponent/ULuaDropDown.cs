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
    public class ULuaDropDown : ULuaUIBase
    {
        Dropdown _dropdown;
        Dropdown dropdown
        {
            get
            {
                if (!_dropdown)
                {
                    _dropdown = GetComponent<Dropdown>();
                    if (!_dropdown)
                    {
                        _dropdown = GetComponent<TDropdown>();
                    }
                }
                return _dropdown;
            }
        }

        Action<int> onValueChangeEvent;
        void Awake()
        {
            dropdown.onValueChanged.AddListener(OnValueChangeLuaFunc);
        }

        void OnValueChangeLuaFunc(int index)
        {
            if (onValueChangeEvent != null)
            {
                onValueChangeEvent( index );
            }
        }

        public void AddValueChangeEvent(Action<int> luafunc)
        {
            onValueChangeEvent = luafunc;
        }

        public void RemoveValueChangeEvent(int index)
        {
        }

        public void RemoveAllValueChangeEvent()
        {
        }

        public void SetDropOption ( int index, string value, string SpritePath = "", bool IsAsyn  = true ) 
        {
            if (!string.IsNullOrEmpty(value) && index >= 0 && index < dropdown.options.Count)
            {
                dropdown.options[index].text = value;
            }
            if (!string.IsNullOrEmpty(SpritePath) && index >= 0 && index < dropdown.options.Count)
            {
                panelmanager.SynGetSprite( SpritePath, ( s ) => 
                {
                    dropdown.options[index].image = s;
                }, IsAsyn );
            }
        }

        public void AddDropOption ( string value, string SpritePath = "", bool IsAsyn = true ) 
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            if (!string.IsNullOrEmpty(value))
            {
                option.text = value;
            }
            else
            {
                ULogger.Error("添加下拉菜单字符串为空");
            }
            if (!string.IsNullOrEmpty(SpritePath))
            {
                panelmanager.SynGetSprite( SpritePath, ( sprite ) => 
                {
                    option.image = sprite;
                }, IsAsyn );
            }
            else
            {
                if (dropdown.itemImage != null)
                {
                    option.image = dropdown.itemImage.sprite;
                }
            }
            List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
            list.Add( option );
            dropdown.AddOptions( list );
        }

        public int GetValue()
        {
            return dropdown.value;
        }

        public void SetValue(int value) 
        {
            dropdown.value = value;
        }

        public string GetDropDownCaptionValue() 
        {
            return dropdown.captionText.text;
        }

        public void RemoveOption(int index) 
        {
            dropdown.options.RemoveAt(index);
        }

        public void RemoveAll() 
        {
            dropdown.options.Clear();
        }

        public int GetOptionCount() 
        {
            return dropdown.options.Count;
        }

        public override void OnClose ( )
        {
            base.OnClose();
            onValueChangeEvent = null;
        }
    }
}
