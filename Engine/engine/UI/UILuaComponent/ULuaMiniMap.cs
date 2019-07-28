using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UEngine.UIExpand;
using UnityEngine;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaMiniMap : ULuaUIBase
    {
        MiniMap _miniMap;
        MiniMap miniMap
        {
            get
            {
                if (!_miniMap)
                {
                    _miniMap = GetComponent<MiniMap>();
                }
                return _miniMap;
            }
        }

        public void LoadMap ( string MapPath, string SceneName, int X1, int Z1, int X2, int Z2, float Size, float offsetX, float offsetY, bool IsAsyn )
        {
            panelmanager.SynGetSprite(MapPath, (s)=>
            {
                miniMap.mImage.sprite = s;
            }, IsAsyn);

            miniMap.LoadMap( SceneName, X1, Z1, X2, Z2, Size, offsetX, offsetY );
        }

        public Vector2 TransformPoint(Vector3 point) 
        {
            return miniMap.TransformPoint(point);
        }

        public Vector3 TransformScreenToWorld(Vector2 ScreenPos) 
        {
            return miniMap.TransformScreenToWorld(ScreenPos);
        }

        public override PanelManagerPort AddItem(string path, bool isCache, bool active )
        {
            PanelManagerPort managerport = base.AddItem( path, isCache, active );
            managerport.panelmanager.rectTransform.anchorMin = Vector2.zero;
            managerport.panelmanager.rectTransform.anchorMax = Vector2.zero;
            return managerport;
        }

        public override PanelManagerPort[] AddItem ( string path, int num, bool isCache , bool active )
        {
            if (num <= 0)
            {
                return new PanelManagerPort[0];
            }

            PanelManagerPort[] managerports = base.AddItem( path, num, isCache, active );
            for (int i = 0 ; i < managerports.Length ; i++)
            {
                PanelManagerPort managerport = managerports[i];
                managerport.panelmanager.rectTransform.anchorMin = Vector2.zero;
                managerport.panelmanager.rectTransform.anchorMax = Vector2.zero;
            }
            return managerports;
        }
    }
}
