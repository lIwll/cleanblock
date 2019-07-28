using System;
using System.Collections;
using System.Collections.Generic;

using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

using UEngine.UI.UILuaBehaviour;
using UEngine.UIExpand;

namespace UEngine.UI
{
    public enum AniType
    {
        AniPosMoveBy = 0,
        AniPosMove,
        AniPercentPosMoveBy,     //2
        AniPercentPosMove,
        AniScale,                //4
        AniRotate,
        AniColor,                //6
        AniColorBy,
        AniAlpha,                //8
        AniAlphaBy,
        CombineAni,              //10
        SequenceAni,
        AniPercentPos3DObjMoveBy,//12
        AniPercentPos3DObjMove,
        CustomAni,               //14
        AniTransMove,
        AniOutline,              //16
        AniGradient,
        AniOutlineAlpha,
        AniTransStay,
        AniWorldPosMove,
        AniSizeChange,
        None,
    }
    
    [Serializable]
    public class UIAnimationBase
    {
        public string guid;
        public float startTime;
        public float endTime;
        public bool IsRestore;
        public Transform trans;
        public virtual int getType() { return 0; }
    }
    
    [Serializable]
    public class AniTransMove : UIAnimationBase
    {
        public Vector2 endPos;
        public override int getType()
        {
            return (int)AniType.AniTransMove;
        }
    }

    [Serializable]
    public class AniTransStay : UIAnimationBase
    {
        public Vector2 endPos;
        public override int getType()
        {
            return (int)AniType.AniTransStay;
        }
    }

    [Serializable]
    public class AniPosMoveBy : UIAnimationBase
    {
        public Vector2 endPos;
        public override int getType()
        {
            return (int)AniType.AniPosMoveBy;
        }

    }

    [Serializable]
    public class AniPosMove : UIAnimationBase
    {
        public Vector2 startPos;
        public Vector2 endPos;
        public override int getType()
        {
            return (int)AniType.AniPosMove;
        }

    }
    
    [Serializable]
    public class AniWorldPosMove : UIAnimationBase
    {
        public Vector2 startPos;
        public Vector2 endPos;
        public override int getType ( )
        {
            return ( int )AniType.AniWorldPosMove;
        }
    }

    [Serializable]
    public class AniPercentPosMoveBy : UIAnimationBase
    {
        public Vector2 endPos;
        public override int getType()
        {
            return (int)AniType.AniPercentPosMoveBy;
        }
    }
    
    [Serializable]
    public class AniPercentPos3DObjMoveBy : UIAnimationBase
    {
        public GameObject obj;
        public bool isMove;
        public Vector2 endPos;
        public override int getType()
        {
            return (int)AniType.AniPercentPos3DObjMoveBy;
        }
    }
    
    [Serializable]
    public class AniPercentPos3DObjMove : UIAnimationBase
    {
        public GameObject obj;
        public bool isMove;
        public Vector2 endPos;
        public Vector2 startPos;
        public override int getType()
        {
            return (int)AniType.AniPercentPos3DObjMove;
        }
    }
    
    [Serializable]
    public class AniPercentPosMove : UIAnimationBase
    {
        public Vector2 startPos;
        public Vector2 endPos;
        public override int getType()
        {
            return (int)AniType.AniPercentPosMove;
        }

    }
    
    [Serializable]
    public class AniScale : UIAnimationBase
    {
        public Vector2 startScale;
        public Vector2 endScale;
        public override int getType()
        {
            return (int)AniType.AniScale;
        }

    }
    
    [Serializable]
    public class AniRotate : UIAnimationBase
    {
        public Vector3 startRotate;
        public Vector3 endRotate;
        public override int getType()
        {
            return (int)AniType.AniRotate;
        }


    }
    
    [Serializable]
    public class AniColor : UIAnimationBase
    {
        public uint startColor;
        public uint endColor;
        public override int getType()
        {
            return (int)AniType.AniColor;
        }


    }
    
    [Serializable]
    public class AniColorBy : UIAnimationBase
    {

        public uint endColor;
        public override int getType()
        {
            return (int)AniType.AniColorBy;
        }

    }
    
    [Serializable]
    public class AniAlpha : UIAnimationBase
    {
        public float startAlpha;
        public float endAlpha;
        public override int getType()
        {
            return (int)AniType.AniAlpha;
        }
    }
    
    [Serializable]
    public class AniAlphaBy : UIAnimationBase
    {
        public float endAlpha;
        public override int getType()
        {
            return (int)AniType.AniAlphaBy;
        }
    }
    
    [Serializable]
    public class CombineAni : UIAnimationBase
    {
        public UIAnimationBase ani1;
        public UIAnimationBase ani2;
        public override int getType()
        {
            return (int)AniType.CombineAni;
        }

    }
    
    [Serializable]
    public class SequenceAni : UIAnimationBase
    {
        public UIAnimationBase ani1;
        public UIAnimationBase ani2;
        public float startTime1;
        public float startTime2;

        public float interval;
        public override int getType()
        {
            return (int)AniType.SequenceAni;
        }

    }
    
    [Serializable]
    public class CustomAni : UIAnimationBase
    {
        public UIAnimationBase[] anis;

        public override int getType()
        {
            return (int)AniType.SequenceAni;
        }
    }
    
    [Serializable]
    public class AniOutline : UIAnimationBase
    {
        public uint startColor;
        public uint endColor;
        public override int getType()
        {
            return (int)AniType.AniOutline;
        }
    }
    
    [Serializable]
    public class AniGradient : UIAnimationBase
    {
        public float endAlpha;
        public override int getType()
        {
            return (int)AniType.AniGradient;
        }
    }
    
    [Serializable]
    public class AniOutLineAlpha : UIAnimationBase
    {
        public float endAlpha;
        public override int getType()
        {
            return (int)AniType.AniOutlineAlpha;
        }
    }

    public class AniSizeChange : UIAnimationBase 
    {
        public float endWidth;
        public float endHeight;

        public override int getType ( )
        {
            return ( int )AniType.AniSizeChange;
        }
    }

    public static class TweenFactoryTemp
    {
        public delegate Tween GetTweens(UIAnimationBase ani, PanelManager bhv);
        public static GetTweens[] GetTweenDelegates ={   
           Get_AniPosMoveByTween,  //0
           Get_AniPosMoveTween,
           Get_AniPercentPosMoveByTween, //2
           Get_AniPercentPosMoveTween,
           Get_AniScaleTween,            //4
           Get_AniRotateTween,      
           Get_AniColorTween,            //6
           Get_AniColorByTween,
           Get_AniAlphaTween,            //8
           Get_AniAlphaByTween,
           Get_CombineAniTween,          //10
           Get_SequenceAniTween,
           Get_AniPercentPos3DObjMoveByTween,//12
           Get_AniPercentPos3DObjMoveTween,
           Get_CustomAniTween,               //14
           Get_AniTransMoveTween,
           Get_AniOutlineTween,              //16
           Get_AniGradientTween,
           Get_AniOutLineATween,
           Get_AniTransStayTween,
           Get_AniWorldPosMoveTween,
           Get_AniSizeByTween,
       };

        #region 方法组
        public static Tween Get_AniTransStayTween(UIAnimationBase ani, PanelManager bhv = null)
        {
            AniTransStay ani1 = (AniTransStay)ani;

            Transform trans = GetObj(ani1.guid, bhv).transform;

            Vector2 to = Vector2.zero;
            Tween tween = DOTween.To(() => to, r =>
            {
                r = ani1.endPos;
                trans.localPosition += r;
            }, ani1.endPos, ani1.endTime - ani1.startTime);
            tween.SetDelay(ani1.startTime);
            tween.SetUpdate(UpdateType.Late);
            bhv.tweenList.Add(tween);
            return tween;
        }

        public static Tween Get_AniTransMoveTween(UIAnimationBase ani, PanelManager bhv = null)
        {
            AniTransMove ani1 = (AniTransMove)ani;

            Transform trans = GetObj(ani1.guid, bhv).transform;

            Vector2 to = Vector2.zero;
            Tween tween = DOTween.To(() => to, r =>
            {
                trans.localPosition += r;
            }, ani1.endPos, ani1.endTime - ani1.startTime);
            tween.SetDelay(ani1.startTime);
            tween.SetUpdate(UpdateType.Late);
            bhv.tweenList.Add(tween);
            return tween;
        }

        public static Tween Get_AniPosMoveByTween(UIAnimationBase ani, PanelManager bhv = null)
        {
            AniPosMoveBy ani1 = (AniPosMoveBy)ani;
            Transform trans = GetObj(ani1.guid, bhv).transform;

            Vector2 to = Vector2.zero;
            Tween tween = DOTween.To(() => to, r =>
            {
                Vector3 diff = r - to;
                to = r;
                trans.localPosition += diff;
            }, ani1.endPos, ani1.endTime - ani1.startTime);
            tween.SetDelay(ani1.startTime);
            tween.SetUpdate(UpdateType.Late);
            bhv.tweenList.Add(tween);
            return tween;
        }

        public static Tween Get_AniPosMoveTween(UIAnimationBase ani, PanelManager bhv)
        {
            AniPosMove ani1 = (AniPosMove)ani;
            Transform trans = GetObj(ani1.guid, bhv).transform;
            Tween tween = DOTween.To(
                () => ani1.startPos, r =>
                {
                    trans.localPosition = r;
                    //trans.position = r;
                    //trans.localPosition = new Vector2( trans.localPosition.x, trans.localPosition.y );
                }
                    , ani1.endPos, ani1.endTime - ani1.startTime);
            tween.SetDelay(ani1.startTime);
            tween.SetUpdate(UpdateType.Late);
            bhv.tweenList.Add(tween);
            return tween;
        }

        public static Tween Get_AniWorldPosMoveTween ( UIAnimationBase ani, PanelManager bhv )
        {
            AniWorldPosMove ani1 = ( AniWorldPosMove )ani;
            Transform trans = GetObj( ani1.guid, bhv ).transform;
            Tween tween = DOTween.To(
                ( ) => ani1.startPos, r =>
                {
                    trans.position = r;
                    trans.localPosition = new Vector2( trans.localPosition.x, trans.localPosition.y );
                }
                    , ani1.endPos, ani1.endTime - ani1.startTime );
            tween.SetDelay( ani1.startTime );
            tween.SetUpdate( UpdateType.Late );
            bhv.tweenList.Add( tween );
            return tween;
        }

        public static Tween Get_AniPercentPosMoveByTween(UIAnimationBase ani, PanelManager bhv)
        {
            AniPercentPosMoveBy ani1 = (AniPercentPosMoveBy)ani;
            Transform trans = GetObj(ani1.guid, bhv).transform;
            Vector2 tranPar = trans.parent.GetComponent<RectTransform>().sizeDelta;
            Vector2 to = new Vector2();
            Tween tween = DOTween.To(() => to, r =>
            {
                Vector3 diff = r - to;
                to = r;
                trans.localPosition += diff;
            }, new Vector2(ani1.endPos.x * tranPar.x, ani1.endPos.y * tranPar.y), ani1.endTime - ani1.startTime);
            tween.SetDelay(ani1.startTime);
            tween.SetUpdate(UpdateType.Late);
            bhv.tweenList.Add(tween);
            return tween;
        }

        public static Tween Get_AniPercentPosMoveTween(UIAnimationBase ani, PanelManager bhv)
        {
            AniPercentPosMove ani1 = (AniPercentPosMove)ani;
            Transform trans = GetObj(ani1.guid, bhv).transform;
            Vector2 tranPar = trans.parent.GetComponent<RectTransform>().sizeDelta;
            Tween tween = DOTween.To(() => ani1.startPos,
                r => trans.localPosition = r,
                new Vector2(ani1.endPos.x * tranPar.x, ani1.endPos.y * tranPar.y),
                ani1.endTime - ani1.startTime);
            tween.SetDelay(ani1.startTime);
            tween.SetUpdate(UpdateType.Late);
            bhv.tweenList.Add(tween);
            return tween;
        }

        public static Tween Get_AniRotateTween(UIAnimationBase ani, PanelManager bhv)
        {
            AniRotate ani1 = (AniRotate)ani;
            Transform trans = GetObj(ani1.guid, bhv).transform;
            Tween tween = DOTween.To(() => ani1.startRotate, r => trans.localEulerAngles = r, ani1.endRotate, ani1.endTime - ani1.startTime);
            tween.SetDelay(ani1.startTime);
            tween.SetUpdate(UpdateType.Late);
            bhv.tweenList.Add(tween);
            return tween;
        }

        public static Tween Get_AniScaleTween(UIAnimationBase ani, PanelManager bhv)
        {
            AniScale ani1 = (AniScale)ani;
            Transform trans = GetObj(ani1.guid, bhv).transform;
            Tween tween = DOTween.To(() => new Vector3(ani1.startScale.x, ani1.startScale.y, ani1.startScale.x), r => trans.localScale = r, new Vector3(ani1.endScale.x, ani1.endScale.y, ani1.endScale.x), ani1.endTime - ani1.startTime);
            tween.SetDelay(ani1.startTime);
            tween.SetUpdate(UpdateType.Late);
            bhv.tweenList.Add(tween);
            return tween;
        }

        public static Tween Get_AniColorByTween(UIAnimationBase ani, PanelManager bhv)
        {
            AniColorBy ani1 = (AniColorBy)ani;
            Transform trans = GetObj(ani1.guid, bhv).transform;
            Graphic gc = trans.GetComponent<Graphic>();
            Color32 to = gc.color;
            Tween tween = DOTween.To(() => to, r =>
            {
                Color32 diff = r - to;
                to = r;
                trans.GetComponent<Graphic>().color += diff;
            }, RGB(ani1.endColor), ani1.endTime - ani1.startTime);
            tween.SetDelay(ani1.startTime);
            tween.SetUpdate(UpdateType.Late);
            bhv.tweenList.Add(tween);
            return tween;
        }
        public static Tween Get_AniColorTween(UIAnimationBase ani, PanelManager bhv)
        {
            AniColor ani1 = (AniColor)ani;
            Transform trans = GetObj(ani1.guid, bhv).transform;
            Graphic gc = trans.GetComponent<Graphic>();
            Tween tween = DOTween.To(() => RGB(ani1.startColor), r => gc.color = r, RGB(ani1.endColor), ani1.endTime - ani1.startTime);
            tween.SetDelay(ani1.startTime);
            tween.SetUpdate(UpdateType.Late);
            bhv.tweenList.Add(tween);
            return tween;
        }
        public static Tween Get_AniAlphaTween(UIAnimationBase ani, PanelManager bhv)
        {
            AniAlpha ani1 = (AniAlpha)ani;
            Transform trans = GetObj(ani1.guid, bhv).transform;
            Graphic gc = trans.GetComponent<Graphic>();
            Tween tween = DOTween.To(() => new Color(gc.color.r, gc.color.g, gc.color.b, ani1.startAlpha), r => gc.color = r, new Color(gc.color.r, gc.color.g, gc.color.b, ani1.endAlpha), ani1.endTime - ani1.startTime);
            tween.SetDelay(ani1.startTime);
            tween.SetUpdate(UpdateType.Late);
            bhv.tweenList.Add(tween);
            return tween;
        }

        public static Tween Get_AniAlphaByTween(UIAnimationBase ani, PanelManager bhv)
        {
            AniAlphaBy ani1 = (AniAlphaBy)ani;
            Transform trans = GetObj(ani1.guid, bhv).transform;
            Graphic gc = trans.GetComponent<Graphic>();
            Color to = gc.color;
            Tween tween = DOTween.To(() => to, r =>
            {
                Color diff = r - to;
                to = r;
                trans.GetComponent<Graphic>().color += diff;
            }, new Color(to.r, to.g, to.b, ani1.endAlpha), ani1.endTime - ani1.startTime);
            tween.SetDelay(ani1.startTime);
            tween.SetUpdate(UpdateType.Late);
            bhv.tweenList.Add(tween);
            return tween;
        }

        public static Tween Get_CombineAniTween(UIAnimationBase ani, PanelManager bhv)
        {
            CombineAni ani1 = (CombineAni)ani;
            Sequence result = DOTween.Sequence();

            Tween tween1 = GetTween(ani1.ani1, bhv);
            Tween tween2 = GetTween(ani1.ani2, bhv);
            result.Insert(0, tween1);
            result.Insert(0, tween2);
            result.SetDelay(ani1.startTime);
            result.SetUpdate(UpdateType.Late);
            bhv.tweenList.Add(result);
            return result;
        }

        public static Tween Get_SequenceAniTween(UIAnimationBase ani, PanelManager bhv)
        {
            SequenceAni que = (SequenceAni)ani;
            Sequence result = DOTween.Sequence();

            Tween tween1 = GetTween(que.ani1, bhv);
            Tween tween2 = null;
            if (que.ani2 != null)
            {
                tween2 = GetTween(que.ani2, bhv);
            }
            result.Append(tween1);
            //result.AppendInterval(que.interval);
            if (tween2 != null)
            {
                result.Append(tween2);
            }
            result.SetUpdate(UpdateType.Late);
            bhv.tweenList.Add(result);
            return result;

        }
        public static Tween Get_AniGradientTween(UIAnimationBase ani, PanelManager bhv)
        {
            AniGradient ani1 = (AniGradient)ani;
            Sequence result = DOTween.Sequence();

            Transform trans = GetObj(ani1.guid, bhv).transform;
            UText gc = trans.GetComponent<UText>();
            if (gc)
            {
                float TstartA = gc.topColor.a;
                float BstartA = gc.bottomColor.a;

                Tween tween1 = DOTween.To(() => gc.topColor, r => gc.topColor = r, new Color(gc.topColor.r, gc.topColor.g, gc.topColor.b, ani1.endAlpha), ani1.endTime - ani1.startTime);
                tween1.SetDelay(ani1.startTime);
                tween1.SetUpdate(UpdateType.Late);

                Tween tween2 = DOTween.To(() => gc.bottomColor, r => gc.bottomColor = r, new Color(gc.bottomColor.r, gc.bottomColor.g, gc.bottomColor.b, ani1.endAlpha), ani1.endTime - ani1.startTime);
                tween2.SetDelay(ani1.startTime);
                tween2.SetUpdate(UpdateType.Late);

                result.Insert(0, tween1);
                result.Insert(0, tween2);
                result.SetUpdate(UpdateType.Late);
                result.AppendCallback(() =>
                {
                    gc.topColor = new Color(gc.topColor.r, gc.topColor.g, gc.topColor.b, TstartA);
                    gc.bottomColor = new Color(gc.bottomColor.r, gc.bottomColor.g, gc.bottomColor.b, BstartA);
                });
            }

            CText ctext = trans.GetComponent<CText>();
            if (ctext)
            {
                float TstartA = ctext.TopColor.a;
                float BstartA = ctext.BottomColor.a;

                Tween tween1 = DOTween.To( ( ) => ctext.TopColor, r => ctext.TopColor = r, new Color( ctext.TopColor.r, ctext.TopColor.g, ctext.TopColor.b, ani1.endAlpha ), ani1.endTime - ani1.startTime );
                tween1.SetDelay( ani1.startTime );
                tween1.SetUpdate( UpdateType.Late );

                Tween tween2 = DOTween.To( ( ) => ctext.BottomColor, r => ctext.BottomColor = r, new Color( ctext.BottomColor.r, ctext.BottomColor.g, ctext.BottomColor.b, ani1.endAlpha ), ani1.endTime - ani1.startTime );
                tween2.SetDelay( ani1.startTime );
                tween2.SetUpdate( UpdateType.Late );

                result.Insert( 0, tween1 );
                result.Insert( 0, tween2 );
                result.SetUpdate( UpdateType.Late );
                result.AppendCallback( ( ) =>
                {
                    ctext.TopColor = new Color( ctext.TopColor.r, ctext.TopColor.g, ctext.TopColor.b, TstartA );
                    ctext.BottomColor = new Color( ctext.BottomColor.r, ctext.BottomColor.g, ctext.BottomColor.b, BstartA );
                } );
            }

            bhv.tweenList.Add(result);
            return result;
        }

        public static Tween Get_AniOutLineATween(UIAnimationBase ani, PanelManager bhv)
        {
            AniOutLineAlpha ani1 = (AniOutLineAlpha)ani;
            Transform trans = GetObj(ani1.guid, bhv).transform;
            UText outline = trans.GetComponent<UText>();
            Sequence result = DOTween.Sequence();
            if (outline)
            {
                float startA = outline.outlineColor.a;
                Tween tween1 = DOTween.To(() => outline.outlineColor, r => outline.outlineColor = r, new Color(outline.outlineColor.r, outline.outlineColor.g, outline.outlineColor.b, ani1.endAlpha), ani1.endTime - ani1.startTime);

                tween1.SetDelay(ani1.startTime);
                result.Insert(0, tween1);
                result.AppendCallback(() =>
                {
                    if (ani1.IsRestore)
                    {
                        outline.outlineColor = new Color(outline.outlineColor.r, outline.outlineColor.g, outline.outlineColor.b, startA);
                    }
                });
            }
            result.SetUpdate(UpdateType.Late);
            bhv.tweenList.Add(result);
            return result;
        }
        public static Tween Get_AniPercentPos3DObjMoveByTween(UIAnimationBase ani, PanelManager bhv)
        {
            AniPercentPos3DObjMoveBy ani1 = (AniPercentPos3DObjMoveBy)ani;
            //xiangji
            GameObject camera = GameObject.FindGameObjectWithTag("UICamera");
            Vector2 Point = camera.GetComponent<Camera>().WorldToScreenPoint(ani1.obj.transform.position);
            //dangqian ui
            Transform trans = GetObj(ani1.guid, bhv).transform;

            Vector2 tranPar = trans.parent.GetComponent<RectTransform>().sizeDelta;
            if (ani1.isMove)
            {
                Tween tween = DOTween.To(() => Point,
                r => trans.localPosition = r,
                new Vector2(ani1.endPos.x * tranPar.x, ani1.endPos.y * tranPar.y),
                ani1.endTime - ani1.startTime);
                tween.SetDelay(ani1.startTime);
                tween.SetUpdate(UpdateType.Late);
                bhv.tweenList.Add(tween);
                return tween;
            }
            else
            {
                trans.localPosition = Point;
                Tween tween = DOTween.To(() => Point,
                r => trans.localPosition = r,
                new Vector2(ani1.endPos.x * tranPar.x, ani1.endPos.y * tranPar.y),
                ani1.endTime - ani1.startTime);
                tween.SetDelay(ani1.startTime);
                tween.SetUpdate(UpdateType.Late);
                bhv.tweenList.Add(tween);
                return tween;
            }
        }
        public static Tween Get_AniPercentPos3DObjMoveTween(UIAnimationBase ani, PanelManager bhv)
        {
            AniPercentPos3DObjMove ani1 = (AniPercentPos3DObjMove)ani;
            //xiangji
            GameObject camera = GameObject.FindGameObjectWithTag("UICamera");
            Vector2 Point = camera.GetComponent<Camera>().WorldToScreenPoint(ani1.obj.transform.position);
            //dangqian ui
            Transform trans = GetObj(ani1.guid, bhv).transform;

            Vector2 tranPar = trans.parent.GetComponent<RectTransform>().sizeDelta;
            if (ani1.isMove)
            {

                Tween tween = DOTween.To(() => Point, r => trans.localPosition = r, ani1.endPos, ani1.endTime - ani1.startTime);
                tween.SetDelay(ani1.startTime);
                tween.SetUpdate(UpdateType.Late);
                bhv.tweenList.Add(tween);
                return tween;
            }
            else
            {
                trans.localPosition = Point;
                Tween tween = DOTween.To(() => Point, r => trans.localPosition = r, Point + ani1.endPos - ani1.startPos, ani1.endTime - ani1.startTime);
                tween.SetDelay(ani1.startTime);
                tween.SetUpdate(UpdateType.Late);
                bhv.tweenList.Add(tween);
                return tween;
            }
        }

        public static Tween Get_AniSizeByTween(UIAnimationBase ani, PanelManager bhv)
        {
            AniSizeChange ani1 = (AniSizeChange)ani;
            Transform trans = GetObj(ani1.guid, bhv).transform;
            RectTransform rt = trans as RectTransform;
            Vector2 sizeD = rt.sizeDelta;
            Tween tween = DOTween.To(() => sizeD, r =>
            {
                Vector2 diff = r - sizeD;
                sizeD = r;
                rt.sizeDelta += diff;
            }, new Vector2(ani1.endWidth, ani1.endHeight), ani1.endTime - ani1.startTime);
            tween.SetDelay(ani1.startTime);
            tween.SetUpdate(UpdateType.Late);
            bhv.tweenList.Add(tween);
            return tween;
        }

        public static Tween Get_CustomAniTween(UIAnimationBase ani, PanelManager bhv)
        {
            return null;
        }
        public static Tween Get_AniOutlineTween(UIAnimationBase ani, PanelManager bhv)
        {
            AniOutline ani1 = (AniOutline)ani;
            Transform trans = GetObj(ani1.guid, bhv).transform;
            Outline gc = trans.GetComponent<Outline>();
            float startA = gc.effectColor.a;
            Sequence result = DOTween.Sequence();
            Tween tween = DOTween.To(() => RGB((uint)ani1.startColor), r => gc.effectColor = r, RGB((uint)ani1.endColor), ani1.endTime - ani1.startTime);
            result.SetDelay(ani1.startTime);
            result.SetUpdate(UpdateType.Late);
            result.Insert(0, tween);
            result.AppendCallback(() => 
            {
                if (ani1.IsRestore)
                {
                    gc.effectColor = new Color(gc.effectColor.r, gc.effectColor.g, gc.effectColor.b, startA);
                }
            });
            bhv.tweenList.Add(result);
            return result;
        }
        #endregion
        #region 辅助方法
        private static GameObject GetObj(string guid, PanelManager bhv)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return bhv.gameObject;
            }
            return bhv.GetBehbyGuid(guid).behbase.gameObject;
        }
        public static Tween GetTween(UIAnimationBase ani, PanelManager bhv)
        {
            return GetTweenDelegates[ani.getType()](ani, bhv);
        }

        public static Color RGB(uint color)
        {
            uint a = 0xFF & color;
            uint b = 0xFF00 & color;
            b >>= 8;
            uint g = 0xFF0000 & color;
            g >>= 16;
            uint r = 0xFF000000 & color;
            r >>= 24;
            return new Color(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
        }

        #endregion
    }
}
