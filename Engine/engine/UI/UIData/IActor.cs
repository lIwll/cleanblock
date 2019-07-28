using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UEngine.UI
{
    public interface IActor
    {
        void SetPos(Vector3 pos);

        Vector3 GetPos(string name = "");

        void SetLayer(string layer);

        GameObject GetGameObj();

        Mesh GetMesh();

        void UIDestroy ( );

        Action GetAddChlidEvent
        {
            get;
            set;
        }
    }
}
