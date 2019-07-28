using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UEngine.UIExpand;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaToggle : ULuaUIBase
    {
        Toggle _toggle;
        Toggle toggle
        {
            get
            {
                if (!_toggle)
                {
                    _toggle = GetComponent<Toggle>();
                }
                return _toggle;
            }
        }

        Action<bool> onValueChange;

        public Color ChangeColor;

        void Awake ( ) 
        {
            toggle.onValueChanged.AddListener(OnClickLuaFunc);
            ChangeColor = UIManager.ToggleChangeColor;
        }

        void OnEnable ( ) 
        {
            if (ChildText == null)
            {
                ChildText = GetComponentInChildren<Text>();
            }
            if (ChildText != null && ChildText.color != ChangeColor)
            {
                TempColor = ChildText.color;

                if (GetIsOnState())
                {
                    ChildText.color = ChangeColor;
                }
            }
        }

        void Start ( )
        {
            if (ChildText != null && ChildText.color != ChangeColor)
            {
                TempColor = ChildText.color;

                if (GetIsOnState())
                {
                    ChildText.color = ChangeColor;
                }
            }
        }

        public void SetBackGroundImage ( string respath, bool IsAsyn ) 
        {
            if (toggle.targetGraphic)
            {
                Image image = toggle.targetGraphic.GetComponent<Image>();
                if (image)
                {
                    panelmanager.SynGetSprite( respath, ( sprite ) =>
                    {
                        image.sprite = sprite;
                    }, IsAsyn );
                }
            }
            else
            {
                ULogger.Error(gameObject.name + ":上的toggle组件targetGraphic为空");
            }
        }

        public void SetCheckMarkImage ( string respath, bool IsAsyn ) 
        {
            if (toggle.graphic)
            {
                Image image = toggle.graphic.GetComponent<Image>();
                if (image)
                {
                    panelmanager.SynGetSprite( respath, ( sprite ) =>
                    {
                        image.sprite = sprite;
                    }, IsAsyn );
                }
            }
            else
            {
                ULogger.Error(gameObject.name + ":上的toggle组件Graphic为空");
            }
        }

        public bool GetIsOnState() 
        {
            return toggle.isOn;
        }

        private Text ChildText;
        private Color TempColor;
        void OnClickLuaFunc(bool ison)
        {
            if (onValueChange != null)
            {
                onValueChange( ison );
            }

            if (ChildText == null && this != null)
            {
                ChildText = GetComponentInChildren<Text>();
            }

            if (ChildText != null) 
            {
                if (ison && ChildText.color != ChangeColor)
                {
                    TempColor = ChildText.color;
                    ChildText.color = ChangeColor;
                }
                else if (!ison && ChildText.color == ChangeColor)
                {
                    ChildText.color = TempColor;
                }
            }
        }

        public void SetToggleState(bool ison) 
        {
            if (ison != toggle.isOn)
            {
                toggle.isOn = ison;
            }
        }

        public void AddToggleValueChangeEvent(Action<bool> luafunc)
        {
            onValueChange = luafunc;
        }

        public void RemoveAllValueChangeEvent()
        {
        }

        public void RemoveValueChangeEvent(int index)
        {
        }

        public override void OnClose ( )
        {
            base.OnClose();
            onValueChange = null;
        }
    }
}
