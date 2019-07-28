using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaRadarPort : ULuaBehaviourBasePort
    {
        ULuaRadar _uiRadar;

        ULuaRadar uiRadar 
        {
            get
            {
                if (_uiRadar == null)
                {
                    if (behbase != null)
                    {
                        _uiRadar = ( ULuaRadar )behbase;
                    }
                }
                return _uiRadar;
            }
        }

        public void SetRadarWeight ( int index, float weigh ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaRadarPort_SetRadarWeight_对象被销毁仍调用接口" );
                return;
            }
            if (uiRadar != null)
            {
                uiRadar.SetRadarWeight( index, weigh );
            }
        }

        public override int GetUIType ( )
        {
            return ( int )UIType.Radar;
        }
    }
}
