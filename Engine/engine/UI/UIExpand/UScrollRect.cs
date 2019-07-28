using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

namespace UEngine.UIExpand
{
    public class UScrollRect : ScrollRect
    {
        public bool isPlay = false;


        public Action<float> onDrag;

        private ScrollRect ScrollRectParent;

        public bool IsPenetrate;

        public Action<float,bool> UpdateContent;

        protected override void Awake ( )
        {
            if (!ScrollRectParent && IsPenetrate && transform.parent)
                ScrollRectParent = transform.parent.GetComponentInParent<ScrollRect>();

            base.Awake();
        }

        protected override void OnEnable ( )
        {
            if (!ScrollRectParent && IsPenetrate && transform.parent)
                ScrollRectParent = transform.parent.GetComponentInParent<ScrollRect>();
            base.OnEnable();
        }

        public override void OnInitializePotentialDrag(UnityEngine.EventSystems.PointerEventData eventData)
        {
            isPlay = false;
            base.OnInitializePotentialDrag(eventData);
        }

        public override void OnBeginDrag ( UnityEngine.EventSystems.PointerEventData eventData )
        {
            if (IsPenetrate && ScrollRectParent)
                ScrollRectParent.OnBeginDrag( eventData );

            base.OnBeginDrag( eventData );
        }

        public override void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (IsPenetrate && ScrollRectParent)
                ScrollRectParent.OnDrag( eventData );

            if (UpdateContent != null)
                UpdateContent( content.anchoredPosition.y ,false);

            ChangeNormalColor( Color.white );
            if (vertical && !horizontal)
            {
                if (onDrag != null)
                {
                    onDrag( eventData.delta.y );
                }
            }
            else if (!vertical && horizontal)
            {
                if (onDrag != null)
                {
                    onDrag( eventData.delta.x );
                }
            }
            base.OnDrag(eventData);
        }

        public override void OnEndDrag ( UnityEngine.EventSystems.PointerEventData eventData )
        {
            if (IsPenetrate && ScrollRectParent)
                ScrollRectParent.OnEndDrag( eventData );

            base.OnEndDrag( eventData );
        }

        protected override void LateUpdate ( )
        {
            base.LateUpdate();
            if (velocity == Vector2.zero)
            {
                ChangeNormalColor( new Color(1, 1, 1, 0) );
            }
        }

        protected override void SetContentAnchoredPosition ( Vector2 position )
        {
            base.SetContentAnchoredPosition( position );
            if (UpdateContent != null)
                UpdateContent( position.y , false);
        }

        public override void StopMovement ( )
        {
            base.StopMovement();
        }

        public void ChangeNormalColor ( Color normalColor ) 
        {
            if (verticalScrollbar)
            {
                ColorBlock tempBlock = verticalScrollbar.colors;
                tempBlock.normalColor = normalColor;
                verticalScrollbar.colors = tempBlock;
            }
            if (horizontalScrollbar)
            {
                ColorBlock tempBlock = horizontalScrollbar.colors;
                tempBlock.normalColor = normalColor;
                horizontalScrollbar.colors = tempBlock;
            }
        }
    }
}
