using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UEngine.UI.UILuaBehaviour;

using UnityEngine;
using UnityEngine.UI;

namespace UEngine.UI
{
    class UIExpandRect : MonoBehaviour
    {
        RectTransform uirootRecttransform;
        Image image;
        Sprite sprite;

        void Start ( ) 
        {
            image = GetComponent<Image>();
        }

        void Update ( )
        {
            if (image.sprite != null && sprite != image.sprite && image.sprite != UIDeserializer.alphaSpr)
            {
                uirootRecttransform = UIManager.UIRootRectTransform;

                float ScreenWidth = uirootRecttransform.rect.width;
                float ScreenHeight = uirootRecttransform.rect.height;

                float width = 1280f;
                float height = 720f;
                if (image != null && image.sprite != null)
                {
                    width = image.sprite.textureRect.width;
                    height = image.sprite.textureRect.height;
                    image.rectTransform.anchorMin = new Vector2( 0.5f, 0.5f );
                    image.rectTransform.anchorMax = new Vector2( 0.5f, 0.5f );
                    image.rectTransform.sizeDelta = new Vector2( width, height );
                }

                float scale;
                if (width * ScreenHeight >= height * ScreenWidth)
                {
                    scale = ScreenHeight / height;
                }
                else
                {
                    scale = ScreenWidth / width;
                }

                transform.localScale = new Vector3( scale, scale, 1 );

                sprite = image.sprite;
            }

        }

        public void UpdateRect ( ) 
        {
            image = GetComponent<Image>();
            if (image.sprite != null && sprite != image.sprite && image.sprite != UIDeserializer.alphaSpr)
            {
                uirootRecttransform = UIManager.UIRootRectTransform;

                float ScreenWidth = uirootRecttransform.rect.width;
                float ScreenHeight = uirootRecttransform.rect.height;

                float width = 1280f;
                float height = 720f;
                if (image != null && image.sprite != null)
                {
                    width = image.sprite.textureRect.width;
                    height = image.sprite.textureRect.height;
                    image.rectTransform.anchorMin = new Vector2( 0.5f, 0.5f );
                    image.rectTransform.anchorMax = new Vector2( 0.5f, 0.5f );
                    image.rectTransform.sizeDelta = new Vector2( width, height );
                }

                float scale;
                if (width * ScreenHeight >= height * ScreenWidth)
                {
                    scale = ScreenHeight / height;
                }
                else
                {
                    scale = ScreenWidth / width;
                }

                transform.localScale = new Vector3( scale, scale, 1 );

                sprite = image.sprite;
            }
        }
    }
}
