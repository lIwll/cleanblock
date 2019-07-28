using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaToggleGroup : ULuaUIBase
    {
        ToggleGroup _group;
        ToggleGroup group 
        {
            get
            {
                if (!_group)
                {
                    _group = GetComponent<ToggleGroup>();
                }
                return _group;
            }
        }

        List<ULuaToggle> toggle_List = new List<ULuaToggle>();

        public override PanelManagerPort AddItem(string path, bool isCache, bool active)
        {
            PanelManagerPort managerport = base.AddItem( path, isCache, active );
            var toggle = managerport.panelmanager.GetComponent<Toggle>();
            if (toggle)
            {
                toggle.group = group;
            }
            else
            {
                ULogger.Warn( "加载的Panel模板上没有Toggle" );
            }
            return managerport;
        }

        public override PanelManagerPort[] AddItem(string path, int num, bool isCache, bool active)
        {
            if (num <= 0)
            {
                return new PanelManagerPort[0];
            }

            PanelManagerPort[] managerports = base.AddItem( path, num, isCache, active );
            for (int i = 0 ; i < managerports .Length; i++)
            {
                var toggle = managerports[i].panelmanager.GetComponent<Toggle>();
                if (toggle)
                {
                    toggle.group = group;
                }
                else
                {
                    ULogger.Warn( "加载的Panel模板上没有Toggle" );
                }
            }
            return managerports;
        }

        public void SetAllTogglesOff() 
        {
            group.SetAllTogglesOff();
        }


    }
}
