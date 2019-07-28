using UEngine.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaBehaviourBasePort
    {
        public ULuaBehaviourBase behbase;

        public bool IsClose = false;

        public void SetVisible(bool value)
        {
            if (behbase != null)
            {
                behbase.SetVisible(value);
            }
        }

        public bool GetVisible() 
        {
            return behbase.GetVisible();
        }

        public bool GetActiveScene() 
        {
            return behbase.GetActiveScene();
        }

        public void SetUITargetObj(IActor actor, string bonename, Vector3 offset)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_SetUITargetObj_对象被销毁仍调用接口" );
                return;
            }
            behbase.SetUITargetObj( actor, bonename, offset );
        }

        public Vector2 GetSize() 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_GetSize_对象被销毁仍调用接口" );
                return Vector2.zero;
            }
            return behbase.GetSize();
        }

        public Vector2 GetWorldPosition ( ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_GetWorldPosition_对象被销毁仍调用接口" );
                return Vector2.zero;
            }
            return behbase.GetWorldPosition();
        }

        public void SetWorldPosition ( Vector2 pos ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_SetWorldPosition_对象被销毁仍调用接口" );
                return;
            }
            behbase.SetWorldPosition( pos );
        }

        public Vector2 GetPosition() 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_GetPosition_对象被销毁仍调用接口" );
                return Vector2.zero;
            }
            return behbase.GetPosition();
        }

        public void SetPosition(Vector2 pos)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_SetPosition_对象被销毁仍调用接口" );
                return;
            }
            behbase.SetPosition(pos);
        }

        public void SetScenePosition ( Vector2 pos ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_SetScenePosition_对象被销毁仍调用接口" );
                return;
            }
            behbase.SetScenePosition( pos );
        }

        public Vector2 GetScenePosition()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_GetScenePosition_对象被销毁仍调用接口" );
                return Vector2.zero;
            }
            return behbase.GetScenePosition();
        }

        public void SetRotation ( Vector3 angles )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_SetRotation_对象被销毁仍调用接口" );
                return;
            }
            behbase.SetRotation( angles );
        }

        public Vector3 GetRotation ( )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_GetRotation_对象被销毁仍调用接口" );
                return Vector3.zero;
            }
            return behbase.GetRotation();
        }

        public void SetLocalRotation ( Vector3 angles )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_SetLocalRotation_对象被销毁仍调用接口" );
                return;
            }
            behbase.SetLocalRotation( angles );
        }

        public Vector3 GetLocalRotation ( )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_GetLocalRotation_对象被销毁仍调用接口" );
                return Vector3.zero;
            }
            return behbase.GetLocalRotation();
        }

        public Vector2 GetAnchoredPosition() 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_GetAnchoredPosition_对象被销毁仍调用接口" );
                return Vector2.zero;
            }
            return behbase.GetAnchoredPosition();
        }

        public void SetAnchoredPosition(Vector2 pos)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_SetAnchoredPosition_对象被销毁仍调用接口" );
                return;
            }
            behbase.SetAnchoredPosition(pos);
        }

        public Vector2 GetLocalPosition()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_GetLocalPosition_对象被销毁仍调用接口" );
                return Vector2.zero;
            }
            return behbase.GetLocalPosition();
        }

        public void SetLocalPosition(Vector2 position)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_SetLocalPosition_对象被销毁仍调用接口" );
                return;
            }
            behbase.SetLocalPosition(position);
        }

        public void SetSize(float width, float height) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_SetSize_对象被销毁仍调用接口" );
                return;
            }
            behbase.SetSize(width, height);
        }

        public virtual PanelManagerPort AddItem ( string path, bool isCache = true, bool active = true ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_AddItem_对象被销毁仍调用接口" );
                return new PanelManagerPort();
            }
            return behbase.AddItem( path, isCache, active );
        }

        public virtual PanelManagerPort[] AddItem ( string path, int num, bool isCache = true, bool active = true )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_AddItem_对象被销毁仍调用接口" );
                return new PanelManagerPort[0];
            }
            return behbase.AddItem( path, num, isCache, active );
        }

        public virtual void RemoveAllChild() 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_RemoveAllChild_对象被销毁仍调用接口" );
                return;
            }
            behbase.RemoveAllChild();
        }

        public virtual void OnClose() 
        {
            behbase.OnClose();
        }

        public virtual void OnHide ( ) 
        {
            behbase.OnHide();
        }

        public void AddCloseEvent(Action func)
        {
            behbase.AddCloseEvent(func);
        }

        public void RemoveCloseEvent(int index)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_RemoveCloseEvent_对象被销毁仍调用接口" );
                return;
            }
            behbase.RemoveCloseEvent(index);
        }

        public void RemoveAllCloseEvent()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_RemoveAllCloseEvent_对象被销毁仍调用接口" );
                return;
            }
            behbase.RemoveAllCloseEvent();
        }

        public void AddHideEvent ( Action func ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_AddHideEvent_对象被销毁仍调用接口" );
                return;
            }
            behbase.AddHideEvent( func );
        }

        public void RemoveAllHideEvent ( ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_RemoveAllHideEvent_对象被销毁仍调用接口" );
                return;
            }
            behbase.RemoveAllHideEvent();
        }

        public Vector2 TransformLocalToWold(Vector2 LocalPosition)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_TransformLocalToWold_对象被销毁仍调用接口" );
                return Vector2.zero;
            }
            return behbase.TransformLocalToWold(LocalPosition);
        }

        public Vector2 TransformWorldToLocal(Vector2 WorldPosition)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_TransformWorldToLocal_对象被销毁仍调用接口" );
                return Vector2.zero;
            }
            return behbase.TransformWorldToLocal(WorldPosition);
        }

        public Vector2 TransformAnchorsToWorld(Vector2 AnchorPosition)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_TransformAnchorsToWorld_对象被销毁仍调用接口" );
                return Vector2.zero;
            }
            return behbase.TransformAnchorsToWorld(AnchorPosition);
        }

        public Vector2 TransformWorldToAnchors(Vector2 WorldPosition)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_TransformWorldToAnchors_对象被销毁仍调用接口" );
                return Vector2.zero;
            }
            return behbase.TransformWorldToAnchors(WorldPosition);
        }
        
        public virtual int GetUIType()
        {
            return (int)UIType.BaseTransform;
        }

        public void SetSiblingIndex ( int index ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_SetSiblingIndex_对象被销毁仍调用接口" );
                return;
            }
            behbase.SetSiblingIndex( index );
        }

        public int GetSiblingIndex ()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_SetSiblingIndex_对象被销毁仍调用接口" );
                return 0;
            }
            return behbase.GetSiblingIndex();
        }

        public int GetParentChildCount ()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_SetSiblingIndex_对象被销毁仍调用接口" );
                return 0;
            }
            return behbase.GetParentChildCount();
        }

        public void PlayAlphaAnimation ( float Duration, uint delayTime, float TargetValue = 0f )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_PlayAlphaAnimation_对象被销毁仍调用接口" );
                return;
            }
            behbase.PlayAlphaAnimation( Duration, delayTime, TargetValue );
        }

        public void PlayAnimator ( string AniName, Action callBack = null, float time = -1f )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaBehaviourBasePort_PlayAnimator_对象被销毁仍调用接口" );
                return;
            }
            behbase.PlayAnimator( AniName, callBack, time );
        }
    }
}
