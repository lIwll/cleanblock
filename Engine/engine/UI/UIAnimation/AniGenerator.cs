using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace UEngine.UIAnimation
{

    public class AniGenerator
    {
        public static UIAnimatorBase Do ( DOGetter<Color> getter, DOSetter<Color> setter, Color endValue, float duration ) 
        {
            UIColorAni colorAni = new UIColorAni();
            colorAni.getter = getter;
            colorAni.setter = setter;
            colorAni.endValue = endValue;
            colorAni.duration = duration;
            return null;
        }

        public static UIAnimatorBase Do ( DOGetter<Vector3> getter, DOSetter<Vector3> setter, Vector3 endValue, float duration )
        {

            return null;
        }

        public static UIAnimatorBase Do ( DOGetter<float> getter, DOSetter<float> setter, float endValue, float duration )
        {

            return null;
        }
    }

    public delegate void OnComplete();

    public enum AnimatorType 
    {
        Anibase,
        Color,
        Vector3,
    }

    public class UIAnimatorBase
    {
        OnComplete m_Complete;

        public float duration;

        protected AnimatorType mType = AnimatorType.Anibase;

        public virtual void Play ( ) 
        {
            UIAnimationManage.AddUIAniBase(this);
        }

        public void OnComplete ( OnComplete onCom ) 
        {
            m_Complete = onCom;
        }

        public void AddOnComplete ( OnComplete onCom ) 
        {
            m_Complete += onCom;
        }

        public void Relese ( ) 
        {
            UIAnimationManage.RemoveAniBase( this );
        }
    }

    public class UIColorAni : UIAnimatorBase
    {
        public UIColorAni ( ) 
        {
            mType = AnimatorType.Color;
        }

        public DOGetter<Color> getter;

        public DOSetter<Color> setter;

        public Color endValue;
    }

    public class UIVector3Ani : UIAnimatorBase
    {
        public UIVector3Ani ( )
        {
            mType = AnimatorType.Vector3;
        }

        public DOGetter<Vector3> getter;

        public DOSetter<Vector3> setter;

        public Vector3 endValue;
    }
}
