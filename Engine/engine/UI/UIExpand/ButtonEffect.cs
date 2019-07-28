using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UEngine.UI.UIEffect
{
    class ButtonEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Vector3 scale = Vector3.one;
        public float time = 0;
        public Transform target = null;

        bool isplaying = false;
        bool isplayback = false;
        Vector3 startSclae;
        void Start ( )
        {
            if (target != null)
            {
                startSclae = target.localScale;
            }
            else
            {
                startSclae = transform.localScale;
            }
        }

        float timeInterval;
        void Update ( )
        {
            if (isplaying)
            {
                if (timeInterval < time)
                {
                    timeInterval += Time.deltaTime;
                    float x = timeInterval / time;
                    if (target != null)
                    {
                        target.localScale = Vector3.Lerp( startSclae, scale, x );
                    }
                    else
                    {
                        transform.localScale = Vector3.Lerp( startSclae, scale, x );
                    }
                }
                else
                {
                    timeInterval = 0;
                    isplaying = false;
                }
            }
            if (isplayback && !isplaying)
            {
                if (target != null) 
                {
                    target.localScale = startSclae;
                }
                else
                {
                    transform.localScale = startSclae;
                }
                isplayback = false;
            }
        }

        public void OnPointerDown ( PointerEventData eventData )
        {
            if (!isplaying)
            {
                isplaying = true;
            }
        }

        public void OnPointerUp ( PointerEventData eventData )
        {
            if (!isplayback)
            {
                isplayback = true;
            }
        }
    }
}
