using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaMiniMapPort : ULuaUIBasePort
    {
        ULuaMiniMap _uiMap;
        ULuaMiniMap uiMap
        {
            get
            {
                if (_uiMap == null)
                {
                    if (behbase != null)
                    {
                        _uiMap = (ULuaMiniMap)behbase;
                    }
                }
                return _uiMap;
            }
        }

        public void LoadMap ( string MapPath, string SceneName, int X1, int Z1, int X2, int Z2, float Size = 1f, float offsetX = 0f, float offsetY = 0f, bool IsAsyn = UIManager.IsAsyn )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaMiniMapPort_LoadMap_对象被销毁仍调用接口" );
                return;
            }
            uiMap.LoadMap( MapPath, SceneName, X1, Z1, X2, Z2, Size, offsetX, offsetY, IsAsyn );
        }

        public Vector2 TransformPoint(Vector3 point)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaMiniMapPort_TransformPoint_对象被销毁仍调用接口" );
                return Vector2.zero;
            }
            return uiMap.TransformPoint(point);
        }

        public Vector3 TransformScreenToWorld(Vector2 ScreenPos)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaMiniMapPort_TransformScreenToWorld_对象被销毁仍调用接口" );
                return Vector3.zero;
            }
            return uiMap.TransformScreenToWorld(ScreenPos);
        }

        public override PanelManagerPort AddItem ( string path, bool isCache = true, bool active = true )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaMiniMapPort_AddItem_对象被销毁仍调用接口" );
                return new PanelManagerPort();
            }
            return uiMap.AddItem( path, isCache, active );
        }

        public override PanelManagerPort[] AddItem ( string path, int num, bool isCache = true, bool active = true )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaMiniMapPort_AddItem_对象被销毁仍调用接口" );
                return new PanelManagerPort[0];
            }
            return uiMap.AddItem( path, num, isCache, active );
        }

        public override int GetUIType()
        {
            return (int)UIType.MiniMap;
        }
    }
}
