using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaToggleGroupPort : ULuaUIBasePort
    {
        ULuaToggleGroup _togglegroup;
        ULuaToggleGroup togglegroup
        {
            get
            {
                if (_togglegroup == null)
                {
                    if (behbase != null)
                    {
                        _togglegroup = (ULuaToggleGroup)behbase;
                    }
                }
                return _togglegroup;
            }
        }

        public override PanelManagerPort AddItem ( string path, bool isCache = true, bool active = true )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaToggleGroupPort_AddItem_对象被销毁仍调用接口" );
                return new PanelManagerPort();
            }
            return togglegroup.AddItem( path, isCache, active );
        }

        public override PanelManagerPort[] AddItem ( string path, int num, bool isCache = true, bool active = true )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaToggleGroupPort_AddItem_对象被销毁仍调用接口" );
                return new PanelManagerPort[0];
            }
            return togglegroup.AddItem( path, num, isCache, active );
        }

        public void SetAllTogglesOff()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaToggleGroupPort_SetAllTogglesOff_对象被销毁仍调用接口" );
                return;
            }
            togglegroup.SetAllTogglesOff();
        }

        public override int GetUIType()
        {
            return (int)UIType.ToggleGroup;
        }
    }
}
