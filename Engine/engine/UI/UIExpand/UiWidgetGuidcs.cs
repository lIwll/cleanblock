using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UEngine;
using UEngine.UIExpand;
using UEngine.UI.UILuaBehaviour;

namespace UEngine.UI
{
    [ExecuteInEditMode]
    public class UiWidgetGuid : MonoBehaviour
    {
        public string UIGuid;

        public UIType uitype = UIType.BaseTransform;

        void Awake()
        {
            uitype = GetUIType();
        }

        void OnEnable()
        {
            uitype = GetUIType();
        }

        public UIType GetUIType()
        {
            UIType _uitype = UIType.BaseTransform;
            Component[] components = transform.GetComponents<Component>();
            for (int i = 0; i < components.Length; ++ i)
            {
				var com = components[i];

                if (com is Image || com is RayCastMask)
                    _uitype = UIType.Image;
                else if (com.GetType() == typeof( Text ) || ( com.GetType() == typeof( CText ) && !( ( CText )com ).RichText ))
                    _uitype = UIType.Text;
                else if (( com.GetType() == typeof( CText ) && ( ( CText )com ).RichText ))
                {
                    _uitype = UIType.InlineText;
                    break;
                }
                else if (com is Button)
                {
                    _uitype = UIType.Button;
                }
                else if (com is Toggle)
                {
                    _uitype = UIType.Toggle;
                    break;
                }
                else if (com is Dropdown || com is TDropdown)
                {
                    _uitype = UIType.Dropdown;
                    break;
                }
                else if (com is ScrollRect)
                {
                    _uitype = UIType.ScrollRect;
                    break;
                }
                else if (com is InputField)
                {
                    _uitype = UIType.InputField;
                    break;
                }
                else if (com is Slider)
                {
                    _uitype = UIType.Slider;
                    break;
                }
                else if (com is Scrollbar)
                {
                    _uitype = UIType.Scrollbar;
                    break;
                }
                else if (com is UJoystick)
                {
                    _uitype = UIType.JoyStick;
                    break;
                }
                else if (com is ToggleGroup)
                {
                    _uitype = UIType.ToggleGroup;
                    break;
                }
                else if (com is RawImage)
                {
                    _uitype = UIType.RawImage;
                    break;
                }
                else if (com is MiniMap)
                {
                    _uitype = UIType.MiniMap;
                    break;
                }
                else if (com is UScrollList)
                {
                    _uitype = UIType.ScrollList;
                    break;
                }
                else if (com is Radar)
                {
                    _uitype = UIType.Radar;
                    break;
                }
            }
            return _uitype;
        }
    }

    public enum UIType
    {
        BaseTransform, //0
        Image,
        Text,
        Button,
        Toggle,
        Dropdown,
        ScrollRect,
        InputField,
        Slider,        //8
        Scrollbar,
        InlineText,
        ToggleGroup,
        JoyStick,
        RawImage,
        MiniMap,
        ScrollList,
        Radar,
    }
}
