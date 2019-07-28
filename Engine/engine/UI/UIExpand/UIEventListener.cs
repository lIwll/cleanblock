using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using UEngine.UI.UILuaBehaviour;

namespace UEngine.UIExpand
{
    public class UIEventListener : MonoBehaviour,IPointerClickHandler,
                                                 IPointerDownHandler,
                                                 IPointerEnterHandler,
                                                 IPointerExitHandler,
                                                 IPointerUpHandler,
                                                 IBeginDragHandler,
                                                 IEndDragHandler,
                                                 IDragHandler
    {
        public ULuaUIBase uiBase;

        public Action onClick;
        public Action onDown;
        public Action onEnter;
        public Action onExit;
        public Action onUp;
        public Action onSelect;
        public Action onUpdateSelect;

        public Action onDragIn;
        public Action<Vector2> onDrag;
        public Action onDragOut;

        private ScrollRect ScrollRectParent;
        public ULuaUIBase DragUIBase;

        public bool ParentScrollDrag = true;

        public bool EventPassBeforeClick = true;

        void Awake() 
        {
            ScrollRectParent = GetComponentInParent<ScrollRect>();
        }

        public void ClearEvent ( ) 
        {
            onClick = null;
            onDown = null;
            onEnter = null;
            onExit = null;
            onUp = null;
            onSelect = null;
            onUpdateSelect = null;
            onDragIn = null;
            onDrag = null;
            onDragOut = null;
        }

        static public UIEventListener Get(GameObject go)
        {
            if (go == null)
            {
                ULogger.Warn("添加的对象gameobject为空");

                return null;
            }
            else
            {
                UIEventListener listener = go.GetComponent<UIEventListener>();
                if (listener == null)
                    listener = go.AddComponent<UIEventListener>();

                return listener;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (EventPassBeforeClick)
            {
                PassEvent( eventData, ExecuteEvents.beginDragHandler, uiBase );
            }

            if (ScrollRectParent != null && ParentScrollDrag)
                ScrollRectParent.OnBeginDrag(eventData);

            if (onDragIn != null)
                onDragIn();

            if (!EventPassBeforeClick)
            {
                PassEvent( eventData, ExecuteEvents.beginDragHandler, uiBase );
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (EventPassBeforeClick)
                PassEvent( eventData, ExecuteEvents.dragHandler, uiBase );

            if (ScrollRectParent != null && ParentScrollDrag)
                ScrollRectParent.OnDrag(eventData);

            if (onDrag != null)
                onDrag(eventData.delta);

            if (DragUIBase != null)
            {
                Vector2 UIRootSize = UIManager.UIRootRectTransform.sizeDelta;
                Vector2 eventPos = UIManager.ScreenToViewPoint( eventData.position );
                DragUIBase.SetPosition( new Vector2( eventPos.x - UIRootSize.x / 2, eventPos.y - UIRootSize.y / 2 ) );
            }

            if (!EventPassBeforeClick)
                PassEvent( eventData, ExecuteEvents.dragHandler, uiBase );
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (EventPassBeforeClick)
                PassEvent( eventData, ExecuteEvents.endDragHandler, uiBase );

            if (ScrollRectParent != null && ParentScrollDrag)
                ScrollRectParent.OnEndDrag(eventData);

            if (onDragOut != null)
                onDragOut();

            if (!EventPassBeforeClick)
                PassEvent( eventData, ExecuteEvents.endDragHandler, uiBase );
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (EventPassBeforeClick)
                PassEvent( eventData, ExecuteEvents.pointerClickHandler, uiBase );

            if (onClick != null)
                onClick();

            if (!EventPassBeforeClick)
                PassEvent( eventData, ExecuteEvents.pointerClickHandler, uiBase );
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (EventPassBeforeClick)
                PassEvent( eventData, ExecuteEvents.pointerDownHandler, uiBase );

            if (onDown != null)
                onDown();

            if (!EventPassBeforeClick)
                PassEvent( eventData, ExecuteEvents.pointerDownHandler, uiBase );
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (onEnter != null)
                onEnter();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (onExit != null)
                onExit();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (EventPassBeforeClick)
                PassEvent( eventData, ExecuteEvents.pointerUpHandler, uiBase );

            if (onUp != null)
                onUp();

            if (!EventPassBeforeClick)
                PassEvent( eventData, ExecuteEvents.pointerUpHandler, uiBase );
        }

        public static void PassEvent<T> ( PointerEventData data, ExecuteEvents.EventFunction<T> function, ULuaUIBase uiBase)
            where T : IEventSystemHandler
        {
            if (uiBase != null && uiBase.gameObject != null)
                ExecuteEvents.Execute( uiBase.gameObject, data, function );
        }
    }
}
