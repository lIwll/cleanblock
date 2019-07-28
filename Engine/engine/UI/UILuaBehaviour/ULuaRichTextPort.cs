using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UEngine.UI.ricktext;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaRichTextPort : ULuaUIBasePort
    {
        ULuaRichText _inlinetext;
        ULuaRichText inlinetext
        {
            get
            {
                if (_inlinetext == null)
                {
                    if (behbase != null)
                    {
                        _inlinetext = (ULuaRichText)behbase;
                    }
                }
                return _inlinetext;
            }
        }

        public void AddHrefClick(Action<int> luafunc)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaRichTextPort_AddHrefClick_对象被销毁仍调用接口" );
                return;
            }
            inlinetext.AddHrefClick(luafunc);
        }

        public void RemoveHrefClick(int index)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaRichTextPort_RemoveHrefClick_对象被销毁仍调用接口" );
                return;
            }
            inlinetext.RemoveHrefClick(index);
        }

        public void RemoveAllHrefClick()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaRichTextPort_RemoveAllHrefClick_对象被销毁仍调用接口" );
                return;
            }
            inlinetext.RemoveAllHrefClick();
        }

        public override int GetUIType()
        {
            return (int)UIType.InlineText;
        }
    }
}
