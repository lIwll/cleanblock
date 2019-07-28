using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UEngine.UIAnimation;
using UEngine.UI.UILuaBehaviour;

using Newtonsoft.Json;

namespace UEngine.UIAnimation
{
    public class UIAnimationData
    {
        public string uiName = "";

        public Dictionary<string, UIAnimator> PanelAnimatorDic = new Dictionary<string, UIAnimator>();
    }

    public class UIAnimator 
    {
        public int TotalFrame = 1000;

        public bool IsLoop = false;

        public int Times = 1;

        public List<int> AniEvents = new List<int>();

        public Dictionary<int, int> EventDic = new Dictionary<int, int>();

        public void UpdateEvent ( float time, Action<int> Event ) 
        {
            if (AniEvents.Count > 0 && time > AniEvents[0] / 30f)
            {
                if (Event != null)
                {
                    Event( EventDic[AniEvents[0]] );
                }
                AniEvents.RemoveAt(0);
            }
        }

        public void InitEvent () 
        {
            AniEvents.Clear();
            Dictionary<int, int>.Enumerator it = EventDic.GetEnumerator();
            while( it.MoveNext() )
            {
                var item = it.Current.Key;
                AniEvents.Add( item );
            }
        }

        public void Init ( PanelManager panelManger, AnimatorTask task ) 
        {
            InitEvent();
            Dictionary<int, UIAniGameObject>.Enumerator it = AniGameObjectDic.GetEnumerator();
            while( it.MoveNext() )
            {
                var item = it.Current;
                if (panelManger.GameObjectList.Count > item.Key )
                {
                    GameObject go = panelManger.GameObjectList[item.Key];
                    item.Value.Init(go);
                    item.Value.GetProperty( task );
                }
            }
        }

        public Dictionary<int, UIAniGameObject> AniGameObjectDic = new Dictionary<int, UIAniGameObject>();
    }

    public class UIAniGameObject 
    {
        [JsonIgnore]
        GameObject mGameObject;
        public void Init ( GameObject go ) 
        {
            mGameObject = go;
        }

        public void GetProperty ( AnimatorTask task ) 
        {
            Dictionary<string, UIAniComponent>.Enumerator it = AniComponentDic.GetEnumerator();
            while( it.MoveNext())
            {
                var item = it.Current;
                Type type = UTypeCache.GetType(item.Key);
                item.Value.Init( mGameObject.GetComponent( type ) );
                item.Value.GetProperty( task );
            }
        }

        public Dictionary<string, UIAniComponent> AniComponentDic = new Dictionary<string, UIAniComponent>();
    }

    public class UIAniComponent 
    {
        [JsonIgnore]
        public Component mComponent;

        public void Init ( Component com ) 
        {
            mComponent = com;
        }

        public void GetProperty ( AnimatorTask task )
        {
            if (mComponent != null)
            {
                Dictionary<string, UIAniProperty>.Enumerator it = AniPropertyDic.GetEnumerator();
                while(it.MoveNext())
                {
                    var item = it.Current;
                    PropertyInfo info = mComponent.GetType().GetProperty(item.Key);

                    item.Value.Init( mComponent, info, info.GetValue(mComponent, null) );
                    task.properties.Add( item.Value );
                }
            }
        }

        public Dictionary<string, UIAniProperty> AniPropertyDic = new Dictionary<string, UIAniProperty>();
    }

    public class UIAniProperty 
    {
        public int FloatChangeMode;
        public bool IsField = false;
        public List<UIClipData> clips = new List<UIClipData>();
        public Dictionary<string, UIAniProperty> ChildPropertyDic = new Dictionary<string, UIAniProperty>();

        [JsonIgnore]
        public Component mComponent;
        [JsonIgnore]
        UIAniProperty parentProperty;
        [JsonIgnore]
        MemberInfo m_Info;
        [JsonIgnore]
        object mValue;

        public void Init ( Component com, MemberInfo info , object value = null ) 
        {
            m_Info = info;
            mComponent = com;
            mValue = value;
            if (info is FieldInfo)
            {
                Type type = ( ( FieldInfo )info ).FieldType;
                for (int i = 0 ; i < clips.Count ; i++)
                {
                    clips[i].m_Type = type;
                }
            }
            Dictionary<string, UIAniProperty>.Enumerator it = ChildPropertyDic.GetEnumerator();
            while(it.MoveNext())
            {
                var item = it.Current;
                MemberInfo mInfo = value.GetType().GetMember(item.Key)[0];

                object m_Value = null;
                if (mInfo is PropertyInfo)
                {
                    m_Value = ( ( PropertyInfo )mInfo ).GetValue( value, null );
                }
                else
                {
                    m_Value = ( ( FieldInfo )mInfo ).GetValue( value );
                }

                item.Value.Init( com, mInfo, value );
                item.Value.parentProperty = this;
            }
        }

        float currentV;
        public void Updata ( float time ) 
        {
            time = time * 30f;
            for (int i = 0 ; i < clips.Count ; i++)
            {
                if (clips[i].m_Type == typeof( float ))
                {
                    FieldInfo field = ( FieldInfo )m_Info;
                    if (i == clips.Count - 1)
                    {
                        if (time >= clips[i].time)
                        {
                            field.SetValue( mValue, clips[i].floatValue );
                        }
                    }
                    else
                    {
                        if (time > clips[i].time && time <= clips[i + 1].time)
                        {
                            float result = 0;
                            if (FloatChangeMode == 0)
                            {
                                float t =( time - ( float )clips[i].time ) / ( ( float )clips[i + 1].time - ( float )clips[i].time );
                                result = Mathf.Lerp( clips[i].floatValue, clips[i + 1].floatValue, t );
                            }
                            else
                            {
                                float smoothTime = ( ( float )( clips[i + 1].time - time ) ) / 30f;
                                float speed = ( clips[i + 1].floatValue - clips[i].floatValue ) * ( clips[i + 1].floatValue - clips[i].floatValue ) / smoothTime;
                                float deltaTime = ( ( float )time ) / 30f;
                                result = Mathf.SmoothDamp( clips[i].floatValue, clips[i + 1].floatValue, ref currentV, smoothTime, speed, deltaTime );
                            }
                            field.SetValue( mValue, result );
                        }

                        if (i == 0 && time <= clips[i].time)
                        {
                            field.SetValue( mValue, clips[i].floatValue );
                        }
                    }

                    if (parentProperty.m_Info is PropertyInfo)
                    {
                        PropertyInfo property = ( PropertyInfo )parentProperty.m_Info;
                        property.SetValue( parentProperty.mComponent, mValue, null );
                        if (parentProperty.mComponent is Graphic)
                        {
                            ( ( Graphic )parentProperty.mComponent ).SetLayoutDirty();
                        }
                    }
                }
            }
        }
    }

    public class UIClipData
    {
        public float floatValue;

        public Type m_Type;

        public int time;
    }
}
