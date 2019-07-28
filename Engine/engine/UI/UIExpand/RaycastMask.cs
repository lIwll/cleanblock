using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace UEngine.UIExpand
{
    public class RayCastMask : Graphic
    {
        public override void SetMaterialDirty ( )
        {

        }

        public override void SetVerticesDirty ( )
        {

        }

        protected override void OnPopulateMesh ( VertexHelper vh )
        {
            vh.Clear();
        }
    }
}
