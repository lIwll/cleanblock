using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaButtonPort : ULuaUIBasePort
    {
        ULuaButton _button;

        ULuaButton button 
        {
            get
            {
                if (_button == null)
                {
                    if (behbase != null)
                    {
                        _button = (ULuaButton)behbase;
                    }
                }
                return _button;
            }
        }

        public void SetSwapSprite ( string Prerespath, string Disrespath, bool IsAsyn = UIManager.IsAsyn ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaButtonPort_SetSwapSprite_对象被销毁仍调用接口" );
                return;
            }
            button.SetSwapSprite( Prerespath, Disrespath, IsAsyn );
        }

        public override void AddClick(Action func) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaButtonPort_AddClick_对象被销毁仍调用接口" );
                return;
            }
            button.AddClick(func);
        }

        public override int GetUIType()
        {
            return (int)UIType.Button;
        }
    }
}
