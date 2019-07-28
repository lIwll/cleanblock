using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UEngine.UIExpand;

using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaButton : ULuaUIBase
    {
        Button _button;
        Button button 
        {
            get 
            {
                if (!_button)
                {
                    _button = GetComponent<Button>();
                }
                return _button;
            }
        }

        public Action onClick;
        void Awake() 
        {
            button.onClick.AddListener(OnClickLuaFunc);
        }

        public void SetSwapSprite(string Prerespath, string Disrespath, bool IsAsyn)
        {
            if (!string.IsNullOrEmpty( Prerespath )) 
            {
                panelmanager.SynGetSprite( Prerespath, ( sprite ) => 
                {
                    var spriteState = button.spriteState;
                    spriteState.pressedSprite = sprite;
                    button.spriteState = spriteState;
                } , IsAsyn);
            }
            if (!string.IsNullOrEmpty( Disrespath )) 
            {
                panelmanager.SynGetSprite( Disrespath, ( sprite ) =>
                {
                    var spriteState = button.spriteState;
                    spriteState.disabledSprite = sprite;
                    button.spriteState = spriteState;
                }, IsAsyn );
            }
        }
        
        public override void AddClick(Action luafunc)
        {
            onClick = luafunc;
        }

        public override void RemoveAllClick()
        {
        }

        public override void RemoveClick(int index)
        {
        }

        void OnClickLuaFunc() 
        {
            if (onClick != null)
            {
                onClick();
            }
        }

        public override void OnClose ( )
        {
            base.OnClose();
            onClick = null;
        }
    }
}
