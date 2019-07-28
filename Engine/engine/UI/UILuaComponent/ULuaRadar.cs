using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UEngine.UIExpand;

namespace UEngine.UI.UILuaBehaviour
{
    class ULuaRadar : ULuaBehaviourBase
    {
        Radar _radar;

        public Radar radar 
        {
            get
            {
                if (!_radar)
                {
                    _radar = GetComponent<Radar>();
                }
                return _radar;
            }
        }

        public void SetRadarWeight ( int index, float weigh ) 
        {
            if (radar != null)
            {
                radar.SetRadarWeight( index, weigh );
            }
        }
    }
}
