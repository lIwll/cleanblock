using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using UEngine.Data;
using DG.Tweening;

namespace UEngine.UI.UILuaBehaviour
{
    [Serializable]
    public class PanelManagerPort
    {
        public PanelManager panelmanager;

        public ULuaBehaviourBasePort GetBehbyGuid(string id)
        {
            if (panelmanager != null)
            {
                return panelmanager.GetBehbyGuid(id);
            }
            else
            {
                ULogger.Warn( "panelmanager为空_GetBehbyGuid_" + id );
                return new ULuaBehaviourBasePort();
            }
        }

        public void SetVisible ( bool value ) 
        {
            if (panelmanager != null)
            {
                panelmanager.SetVisible( value );
            }
        }

        public bool GetVisible ( ) 
        {
            if (panelmanager == null) 
            {
                return false;
            }
            return panelmanager.GetVisible();
        }

        public void ClosePanel() 
        {
            if (panelmanager != null)
            {
                panelmanager.ClosePanel();
            }
            else
            {
                ULogger.Error("panelmanager为空_ClosePanel");
            }
        }

        public void SetSortfrequency(int interval) 
        {
            if (panelmanager != null)
            {
                panelmanager.SetSortfrequency(interval);
            }
            else
            {
                ULogger.Error( "panelmanager为空_SetSortfrequency" );
            }
        }

        public void PlayAni(UIAnimationBase anim, Action luafunc = null)
        {
            if (panelmanager)
            {
                panelmanager.PlayAni(anim, luafunc);
            }
            else
            {
                ULogger.Error( "panelmanager为空_PlayAni" );
            }
        }

        public void PlayAni(UIAnimationBase anim, Action luafunc, bool isRepeat)
        {
            if (panelmanager)
            {
                panelmanager.PlayAni(anim, luafunc, isRepeat);
            }
            else
            {
                ULogger.Error( "panelmanager为空_PlayAni2" );
            }
        }

        public int AddAnimationEvent(Action< int > function) 
        {
            return panelmanager.AddAnimationEvent(function);
        }

        public void RemoveAnimationEvent ( int index ) 
        {
            panelmanager.RemoveAnimationEvent(index);
        }

        public void RemoveAllAnimationEvent ( ) 
        {
            panelmanager.RemoveAllAnimationEvent();
        }

        public void PlayAnimation ( string AniName, int Times = 1, Action function = null, Action<int> AniEvents = null, bool isResum = false )
        {
            panelmanager.PlayAnimation( AniName, Times, function, AniEvents, isResum );
        }

        public void StopAnimation ( string AniName, bool IsEnd = true ) 
        {
            panelmanager.StopAnimation( AniName, IsEnd );
        }

        public void PlayAnimator ( string AniName, Action callBack = null, float time = -1f ) 
        {
            panelmanager.PlayAnimator( AniName, callBack, time );
        }

        public void AddAnimatorEvent ( Action<int> aniEvent ) 
        {
            panelmanager.AddAnimatorEvent( aniEvent );
        }

        public ULuaBehaviourBasePort GetSelfBeh()
        {
            if (panelmanager)
            {
                return panelmanager.GetSelfBeh();
            }
            else
            {
                ULogger.Error( "panelmanager为空_GetSelfBeh" );
                return new ULuaBehaviourBasePort();
            }
        }

        public int AddCloseEvent(Action func)
        {
            return panelmanager.AddCloseEvent(func);
        }

        public void RemoveCloseEvent(int index)
        {
            panelmanager.RemoveCloseEvent(index);
        }

        public void RemoveAllCloseEvent()
        {
            panelmanager.RemoveAllCloseEvent();
        }

        public void PlayAlphaAnimation ( float duration, uint delayTime = 100, float targetValue = 0f ) 
        {
            panelmanager.PlayAlphaAnimation( duration, delayTime, targetValue );
        }

        public void ChangeGraphicsAlpha ( float TargetValue ) 
        {
            panelmanager.ChangeGraphicsAlpha( TargetValue );
        }
    }
}
