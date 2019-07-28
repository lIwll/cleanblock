using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UEngine;

using UnityEngine;
using UnityEngine.EventSystems;

namespace UEngine.UIExpand
{
    public class TextLink : MonoBehaviour, IPointerClickHandler
    {
        public CText text;

        public static void LoginLink(CText text)
        {
            if (!text.GetComponent<TextLink>())
            {
                TextLink link = text.gameObject.AddComponent<TextLink>();
                link.text = text;
            }
        }

        public void OnPointerClick ( PointerEventData eventData )
        {
            Vector2 lp;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                text.rectTransform, eventData.position, eventData.pressEventCamera, out lp );

            for (int i = 0; i < text.mHreInfoList.Count; ++ i)
            {
				var hrefInfo = text.mHreInfoList[i];

                var boxes = hrefInfo.boxes;
                if (boxes.Contains( lp ))
                {
                    text.OnHrefClick.Invoke( hrefInfo.tag );
                    return;
                }
            }
        }
    }
}
