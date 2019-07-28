using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace UEngine.UI
{
    public class UIClassConfig
    {
        public Dictionary<string, object> MemberDic = new Dictionary<string, object>();
    }

    public class UIGameObject
    {
        public string objname;
        public bool IsActive;
        public List<string> Child_List = new List<string>();
        public Dictionary<string, UIClassConfig> CompoentsDic = new Dictionary<string, UIClassConfig>();
    }

    public class UIPanelConfig
    {
        public string UIName;
        public string UIroot;
        public int PanelMode;
        public float BottomValue;
        public Dictionary<string, UIGameObject> GameObjectDic = new Dictionary<string, UIGameObject>();
    }

    [Serializable]
    public class UIGuidData 
    {
        public string UIGuid;
        public int GameObjectIndex;
        public int UIType;
    }

    [Serializable]
    public class ResourcesTask 
    {
        public string ResPath;
        public string memberName;
        public UnityEngine.Object obj;
    }
}