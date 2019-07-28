using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

namespace UEngine.UIAnimation
{
    public class UIShowTime : MonoBehaviour
    {
        public float ShowTime;
        public float DelayTime;
        float time;
        float delayTime = 0;

        Graphic graphic;

        void Start ( ) 
        {
            graphic = GetComponent<Graphic>();
            if (graphic)
                graphic.enabled = false;
        }

        void Update ( )
        {
            if (delayTime >= DelayTime)
            {
                if (graphic)
                    graphic.enabled = true;

                if (time >= ShowTime)
                {
                    delayTime = 0;
                    time = 0;
                    gameObject.SetActive( false );
                }
                else
                {
                    time += Time.deltaTime;
                }
            }
            else
            {
                delayTime += Time.deltaTime;
                if (graphic)
                    graphic.enabled = false;
            }
        }
    }
}
