using UEngine.Data;
using UEngine.UIAnimation;

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaBehaviourBase : MonoBehaviour
    {
        public PanelManager panelmanager;

        IActor target;

        Vector3 PianyiDian = Vector3.zero;

        RectTransform _recttransform;
        Canvas _canvas;
        public RectTransform recttransform 
        {
            get 
            {
                if (!_recttransform)
                {
                    _recttransform = transform as RectTransform;
                }
                return _recttransform;
            }
        }
        private Canvas canvas
        {
            get
            {
                if (!_canvas)
                {
                    _canvas = GetComponentInParent<Canvas>();
                }
                return _canvas;
            }
        }

        private Animator _animator;
        public Animator mAnimator
        {
            get
            {
                if (!_animator)
                {
                    _animator = GetComponent<Animator>();
                }
                return _animator;
            }
        }
        
        private Dictionary<string, AnimationClip> mAnimatorDic = new Dictionary<string, AnimationClip>();

        Vector3 UIOffset = Vector3.zero;

        bool IsGetTarget = false;

        protected Action onClose;
        protected Action onHide;

        public virtual void ILateUpdate() 
        {
            if (IsGetTarget && target != null && isActiveAndEnabled)
            {
                recttransform.localPosition = Trans3DObjPos();
            }
        }

        public Vector2 GetWorldPosition ( ) 
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate( transform.parent as RectTransform );
            return transform.position;
        }

        public void SetWorldPosition ( Vector2 pos ) 
        {
            transform.position = new Vector3( pos.x, pos.y, recttransform.position.z );
        }

        public Vector2 GetPosition() 
        {
            return recttransform.localPosition;
        }

        public void SetPosition(Vector2 pos)
        {
            recttransform.localPosition = pos;
            ULuaBehaviourBase[] behes = transform.GetComponentsInChildren<ULuaBehaviourBase>();
            for (int i = 0; i < behes.Length; i++)
            {
                behes[i].IsChanged = true;
            }
        }

        public void SetScenePosition ( Vector2 pos ) 
        {
            recttransform.localPosition = pos / canvas.scaleFactor;
        }

        public Vector2 GetScenePosition()
        {
            return recttransform.localPosition * canvas.scaleFactor;
        }

        public void SetRotation ( Vector3 angles ) 
        {
            recttransform.eulerAngles = angles;
        }

        public Vector3 GetRotation ( ) 
        {
            return recttransform.eulerAngles;
        }

        public void SetLocalRotation ( Vector3 angles ) 
        {
            recttransform.localEulerAngles = angles;
        }

        public Vector3 GetLocalRotation ( )
        {
            return recttransform.localEulerAngles;
        }

        public Vector2 GetAnchoredPosition() 
        {
            return recttransform.anchoredPosition; 
        }

        public void SetAnchoredPosition(Vector2 pos) 
        {
            recttransform.anchoredPosition = pos;
            ULuaBehaviourBase[] behes = transform.GetComponentsInChildren<ULuaBehaviourBase>();
            for (int i = 0; i < behes.Length; i++)
            {
                behes[i].IsChanged = true;
            }
        }

        Vector3[] parentCorners;
        Vector3[] ParentCorners 
        {
            get 
            {
                if (parentCorners == null)
                {
                    parentCorners = new Vector3[4];
                }
                return parentCorners;
            }
        }

        public bool IsChanged = true;

        public Vector2 GetLocalPosition()
        {
            if (IsChanged)
            {
                RectTransform parentRect = transform.parent as RectTransform;
                parentRect.GetWorldCorners(ParentCorners);
                IsChanged = false;
            }

            Vector2 offsetV2 = transform.position - ParentCorners[0];
            return new Vector2(offsetV2.x / UIManager.UIRootRate, offsetV2.y / UIManager.UIRootRate);
        }

        public void SetLocalPosition(Vector2 position)
        {
            float x = position.x * UIManager.UIRootRate;
            float y = position.y * UIManager.UIRootRate;

            if (IsChanged)
            {
                RectTransform parentRect = transform.parent as RectTransform;
                parentRect.GetWorldCorners(ParentCorners);
                IsChanged = false;
            }

            transform.position = new Vector3( ParentCorners[0].x, ParentCorners[0].y, transform.position.z ) + new Vector3( x, y, 0 );
            ULuaBehaviourBase[] behes = transform.GetComponentsInChildren<ULuaBehaviourBase>();
            for (int i = 0; i < behes.Length; i++)
            {
                behes[i].IsChanged = true;
            }
        }

        public void SetVisible(bool value)
        {
            gameObject.SetActive(value);
        }

        public bool GetVisible() 
        {
            return gameObject.activeSelf;
        }

        public bool GetActiveScene() 
        {
            return gameObject.activeInHierarchy;
        }

        public void SetUITargetObj(IActor actor, string bonename, Vector3 offset)
        {
            if (actor == null)
            {
                if (IsGetTarget)
                {
                    UIAnimationManage.RemoveUIBase( this );
                }
                IsGetTarget = false;
                ULogger.Warn("SetUITarget设置的UActor对象为空");
                return;
            }
            else
            {
                target = actor;
                IsGetTarget = true;
            }
            if (!string.IsNullOrEmpty(bonename))
            {
                PianyiDian = actor.GetPos(bonename);
            }
            if (offset != null)
            {
                UIOffset = offset;
            }
            if (target != null && IsGetTarget)
            {
                recttransform.localPosition = Trans3DObjPos();
                UIAnimationManage.AddUIBase(this);
            }
        }

		static Vector2 msOutOfScreenPos = new Vector2(9999, 9999);
        public Vector2 Trans3DObjPos()
        {
			if (null == Camera.main)
				return msOutOfScreenPos;

            if (PianyiDian == Vector3.zero && UIOffset == Vector3.zero)
            {
                return V3toV2(target.GetPos());
            }
            else if (PianyiDian != Vector3.zero && UIOffset == Vector3.zero)
            {
                return V3toV2(PianyiDian);
            }
            else if (PianyiDian == Vector3.zero && UIOffset != Vector3.zero)
            {
                return V3toV2(target.GetPos() + UIOffset);
            }
			return msOutOfScreenPos;
        }

        private Vector2 V3toV2(Vector3 vector3) 
        {
            float width = UIManager.UIRootRectTransform.rect.width;
            float height = UIManager.UIRootRectTransform.rect.height;
            Vector3 v2 = Camera.main.WorldToViewportPoint(vector3);
            if (v2.z > 0)
            {
                v2 = new Vector2(v2.x * width - width / 2, v2.y * height - height / 2);
            }
            else
            {
				v2 = msOutOfScreenPos;
            }
            return v2;
        }
        
        public Vector2 GetSize() 
        {
            return new Vector2(recttransform.rect.width, recttransform.rect.height);
        }

        public void SetSize(float width, float height) 
        {
            recttransform.sizeDelta = new Vector2(width, height);
        }

        public virtual PanelManagerPort AddItem(string path, bool isCache , bool active ) 
        {
            path = "Data/bytes/" + path + ".byte";
            
            PanelManagerPort managerport = UILoader.LoadUIData( path, active, transform, isCache );
            panelmanager.ChildPanel_List.Add(managerport.panelmanager);
            managerport.panelmanager.ParentPanel = panelmanager;

            managerport.panelmanager.SetSubCanvas_ParticleSortLayerID( panelmanager.SortLayerID );

            return managerport; 
        }

        public virtual PanelManagerPort[] AddItem(string path, int num, bool isCache , bool active ) 
        {
            if (num <= 0)
            {
                return new PanelManagerPort[0];
            }

            path = "Data/bytes/" + path + ".byte";
            PanelManagerPort[] managerports = new PanelManagerPort[num];
            for (int i = 0; i < num; i++)
            {
                managerports[i] = UILoader.LoadUIData( path, active, transform, isCache ); 
                panelmanager.ChildPanel_List.Add( managerports[i].panelmanager );
                managerports[i].panelmanager.ParentPanel = panelmanager;

                managerports[i].panelmanager.SetSubCanvas_ParticleSortLayerID( panelmanager.SortLayerID );
            }
            return managerports;
        }

        public virtual void RemoveAllChild() 
        {
            if (transform == null)
            {
                ULogger.Warn( "控件上transform为空" );
                return;
            }

            int index = transform.childCount;
            for (int i = index - 1; i >= 0; i--)
            {
                PanelManager manager = transform.GetChild(i).GetComponent<PanelManager>();
                if (manager != null)
                {
                    
                    manager.ClosePanel();
                }
            }
        }

        public virtual void OnClose() 
        {
            AnimatorCallBack = null;

            if (onClose != null)
                onClose();

            if (IsGetTarget)
            {
                UIAnimationManage.RemoveUIBase(this);
            }

            onHide = null;
            onClose = null;
        }

        public virtual void OnHide ( ) 
        {
            if (onHide != null)
            {
                onHide();
            }
        }

        public void AddCloseEvent(Action func)
        {
            onClose = func;
        }

        public void RemoveCloseEvent(int index) 
        {
        }

        public void RemoveAllCloseEvent() 
        {
            onClose = null;
        }

        public void AddHideEvent ( Action func ) 
        {
            onHide = func;
        }

        public void RemoveAllHideEvent ( ) 
        {
            onHide = null;
        }

        public Vector2 TransformLocalToWold(Vector2 LocalPosition) 
        {
            if (IsChanged)
            {
                RectTransform parentRect = transform.parent as RectTransform;
                parentRect.GetWorldCorners(ParentCorners);
                IsChanged = false;
            }

            Vector3 worldPosition = new Vector3(parentCorners[0].x + LocalPosition.x * UIManager.UIRootRate, parentCorners[0].y + LocalPosition.y * UIManager.UIRootRate, parentCorners[0].z);

            float X = (worldPosition.x - UIManager.CenterPoint.x) / UIManager.UIRootRate;
            float Y = (worldPosition.y - UIManager.CenterPoint.y) / UIManager.UIRootRate;

            return new Vector2(X, Y);
        }

        public Vector2 TransformWorldToLocal(Vector2 WorldPosition) 
        {
            if (IsChanged)
            {
                RectTransform parentRect = transform.parent as RectTransform;
                parentRect.GetWorldCorners(ParentCorners);
                IsChanged = false;
            }
            float x = WorldPosition.x / UIManager.UIRootRectTransform.sizeDelta.x;
            float y = WorldPosition.y / UIManager.UIRootRectTransform.sizeDelta.y;

            Vector3 worldPos = new Vector3(UIManager.CenterPoint.x + x * UIManager.UIRootWidth, UIManager.CenterPoint.y + y * UIManager.UIRootHeight, UIManager.CenterPoint.z);
            Vector2 offsetV2 = worldPos - ParentCorners[0];
            
            return new Vector2(offsetV2.x / UIManager.UIRootRate, offsetV2.y / UIManager.UIRootRate);
        }

        public Vector2 TransformAnchorsToWorld(Vector2 AnchorPosition) 
        {
            float x = AnchorPosition.x - recttransform.anchoredPosition.x;
            float y = AnchorPosition.y - recttransform.anchoredPosition.y;

            Vector3 worldPos = new Vector3(transform.position.x + x * UIManager.UIRootRate, transform.position.y + y * UIManager.UIRootRate, transform.position.z);

            float X = (worldPos.x - UIManager.CenterPoint.x) / UIManager.UIRootRate;
            float Y = (worldPos.y - UIManager.CenterPoint.y) / UIManager.UIRootRate;

            return new Vector2(X, Y);
        }

        public Vector2 TransformWorldToAnchors(Vector2 WorldPosition) 
        {
            float x = WorldPosition.x / UIManager.UIRootRectTransform.sizeDelta.x;
            float y = WorldPosition.y / UIManager.UIRootRectTransform.sizeDelta.y;

            Vector3 worldPos = new Vector3(UIManager.CenterPoint.x + x * UIManager.UIRootWidth, UIManager.CenterPoint.y + y * UIManager.UIRootHeight, UIManager.CenterPoint.z);

            float offsetX = (worldPos.x - transform.parent.position.x) / UIManager.UIRootRate;
            float offsetY = (worldPos.y - transform.parent.position.y) / UIManager.UIRootRate;

            return new Vector2(offsetX, offsetY);
        }

        public void SetSiblingIndex ( int index ) 
        {
            if (recttransform.GetSiblingIndex() == index || ( index == -1 && recttransform.GetSiblingIndex() == recttransform.parent.childCount - 1 )) 
            {
                return;
            }

            recttransform.SetSiblingIndex(index);
            if (panelmanager != null && panelmanager.IsMainPanel)
            {
                UIManager.instance.SortCanvasSortLayerID( panelmanager.LayerID, panelmanager.SortLayerID, index );
            }
        }

        public int GetSiblingIndex ( ) 
        {
            return recttransform.GetSiblingIndex();
        }

        public int GetParentChildCount ( ) 
        {
            return recttransform.parent.childCount;
        }

        Graphic[] graphics;
        public void PlayAlphaAnimation ( float Duration, uint delayTime, float TargetValue = 0f )
        {
            graphics = null;
            UTimer.AddTimer( delayTime, 0, ( ) =>
            {
                graphics = GetComponentsInChildren<Graphic>();
                if (graphics != null && graphics.Length > 0)
                {
                    for (int i = 0 ; i < graphics.Length ; i++)
                    {
                        graphics[i].CrossFadeAlpha( TargetValue, Duration, true );
                    }
                }
            } );
        }

        private void AnimatorInit ( )
        {
            var controller = mAnimator.runtimeAnimatorController;
            if (controller != null)
            {
                AnimationClip[] clips = controller.animationClips;

                for (int i = 0 ; i < clips.Length ; i++)
                {
                    if (clips[i] && !mAnimatorDic.ContainsKey( clips[i].name ))
                    {
                        mAnimatorDic.Add( clips[i].name, clips[i] );
                    }
                }
            }
        }

        public void PlayAnimator ( string AniName, Action callBack = null, float time = -1f )
        {
            if (mAnimator)
            {
                TimeAdd = 0f;
                if (mAnimatorDic.Count == 0)
                {
                    AnimatorInit();
                }

                int NameHash = Animator.StringToHash( AniName );

                if (mAnimatorDic.ContainsKey( AniName ))
                {
                    AnimatorTime = mAnimatorDic[AniName].length;
                }
                if (time >= 0)
                {
                    mAnimator.Play( NameHash, 0, time );
                    AnimatorTime = 0;
                }
                else
                {
                    mAnimator.Play( NameHash, 0, 0 );
                }
                AnimatorCallBack = callBack;
            }
        }

        Action AnimatorCallBack;
        float AnimatorTime = 0f;
        float TimeAdd = 0f;
        void Update ( )
        {
            if (AnimatorCallBack != null)
            {
                if (TimeAdd >= AnimatorTime)
                {
                    if (AnimatorCallBack != null)
                    {
                        AnimatorCallBack();
                        AnimatorCallBack = null;
                    }
                    TimeAdd = 0f;
                }
                else
                {
                    TimeAdd += Time.deltaTime;
                }
            }
        }

        void OnDisable ( ) 
        {
            OnHide();
        }
    }
}
