using System;
using UnityEngine;

namespace UEngine
{
    public class Bone2DInfno : ScriptableObject
    {
        public Vector3 pos;

        public string boneName;

        public Color color = Color.white;

        public Material material;

        public string sortingLayerName;

        public int sortingOrder;

        public string[] boneInfo;

    }
}
