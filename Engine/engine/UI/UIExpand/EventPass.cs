﻿using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using UEngine.UI.UILuaBehaviour;

namespace UEngine.UIExpand
{
    public class EventPass : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public ULuaUIBase uiBase;

        //监听按下
        public void OnPointerDown ( PointerEventData eventData )
        {
            PassEvent( eventData, ExecuteEvents.pointerDownHandler );
        }

        //监听抬起
        public void OnPointerUp ( PointerEventData eventData )
        {
            PassEvent( eventData, ExecuteEvents.pointerUpHandler );
        }

        //监听点击
        public void OnPointerClick ( PointerEventData eventData )
        {
            PassEvent( eventData, ExecuteEvents.submitHandler );
            //PassEvent( eventData, ExecuteEvents.pointerClickHandler );
        }

        //把事件透下去
        public void PassEvent<T> ( PointerEventData data, ExecuteEvents.EventFunction<T> function )
            where T : IEventSystemHandler
        {
            if (uiBase)
               ExecuteEvents.Execute( uiBase.gameObject, data, function );
        }

        public void OnPointerEnter ( PointerEventData eventData )
        {
            PassEvent( eventData, ExecuteEvents.pointerEnterHandler );
        }

        public void OnPointerExit ( PointerEventData eventData )
        {
            PassEvent( eventData, ExecuteEvents.pointerExitHandler );
        }

        public void OnBeginDrag ( PointerEventData eventData )
        {
            PassEvent( eventData, ExecuteEvents.beginDragHandler );
        }

        public void OnEndDrag ( PointerEventData eventData ) 
        {
            PassEvent( eventData, ExecuteEvents.endDragHandler );
        }

        public void OnDrag ( PointerEventData eventData ) 
        {
            PassEvent( eventData, ExecuteEvents.dragHandler );
        }
    }
}
