using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UEngine.Data
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class EditCreateGUID : MonoBehaviour
    {
        [HideInInspector]
        public string m_GUID;

        public int m_Index;
    }
}
