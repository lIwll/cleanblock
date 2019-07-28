using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaSlider : ULuaUIBase
    {
        Slider _slider;
        Slider slider
        {
            get
            {
                if (!_slider)
                {
                    _slider = GetComponent<Slider>();
                }
                return _slider;
            }
        }

        Action<float> onValueChange;

        void Awake()
        {
            slider.onValueChanged.AddListener(OnClickLuaFunc);
        }

        public float GetValue() 
        {
            return slider.value;
        }

        public void SetMaxOrMinValue(float maxvalue = 1, float minvalue = 0) 
        {
            if (minvalue >= maxvalue)
            {
                ULogger.Warn("设置的最小值大于等于最大值");
            }

            slider.maxValue = maxvalue;
            slider.minValue = minvalue;

            ResetStatus();
        }

        public void SetValue(float value) 
        {
            if (value > slider.maxValue)
            {
                ULogger.Warn("设置的值大于最大值");

                return;
            }

            if (value < slider.minValue)
            {
                ULogger.Warn("设置的值小于最小值");

                return;
            }

            ResetStatus();
            slider.value = value; 
        }

        public void SetValueSmoothAni ( float value, float time, float MaxSpeed )
        {
            if (value > slider.maxValue)
            {
                ULogger.Warn( "设置的值大于最大值" );

                return;
            }

            if (value < slider.minValue)
            {
                ULogger.Warn( "设置的值小于最小值" );

                return;
            }

            currentVelocity = 0f;
            IsPlaySmooth = true;
            IsPlayLerp = false;
            IsPlayTransLerp = false;
            IsPlayTransSmooth = false;
            targetValue = value;
            totalTime = time;
            maxSpeed = MaxSpeed;
        }

        public void SetValueLerpAni ( float value, float time ) 
        {
            if (value > slider.maxValue)
            {
                ULogger.Warn( "设置的值大于最大值" );

                return;
            }

            if (value < slider.minValue)
            {
                ULogger.Warn( "设置的值小于最小值" );

                return;
            }

            IsPlaySmooth = false;
            IsPlayLerp = true;
            IsPlayTransLerp = false;
            IsPlayTransSmooth = false;
            startValue = slider.value;
            endValue = value;
            totalTime = time;
            currentTime = 0;
        }

        private void ResetStatus ( ) 
        {
            currentVelocity = 0f;
            currentVelocityTrans = 0f;
            IsPlaySmooth = false;
            IsPlayLerp = false;
            IsPlayTransLerp = false;
            IsPlayTransSmooth = false;
        }

        private float startValue;
        private float endValue;
        private float currentTime;
        
        private float targetValue;
        private float currentVelocity;
        private float currentVelocityTrans;
        private float maxSpeed;

        private float totalTime;

        private bool IsPlayLerp = false;
        private bool IsPlaySmooth = false;
        void Update ( ) 
        {
            if (IsPlaySmooth)
            {
                if (maxSpeed < 0)
                {
                    slider.value = Mathf.SmoothDamp( slider.value, targetValue, ref currentVelocity, totalTime );
                }
                else
                {
                    slider.value = Mathf.SmoothDamp( slider.value, targetValue, ref currentVelocity, totalTime, maxSpeed );
                }
                if (Mathf.Abs( targetValue - slider.value ) < 0.0001f)
                {
                    IsPlaySmooth = false;
                    slider.value = targetValue;
                }
            }

            if (IsPlayLerp)
            {
                if (totalTime >= currentTime)
                {
                    currentTime += Time.deltaTime;
                    slider.value = Mathf.Lerp( startValue, endValue, currentTime / totalTime );
                }
                else
                {
                    slider.value = endValue;
                    IsPlayLerp = false;
                }
            }

            if (IsPlayTransSmooth)
            {
                if (maxSpeed < 0)
                {
                    SetTransFill( Mathf.SmoothDamp( GetTransFill(), targetValue, ref currentVelocityTrans, totalTime ) );
                }
                else
                {
                    SetTransFill( Mathf.SmoothDamp( GetTransFill(), targetValue, ref currentVelocityTrans, totalTime, maxSpeed ) );
                }
                if (Mathf.Abs( targetValue - GetTransFill() ) < 0.0001f)
                {
                    IsPlayTransSmooth = false;
                    slider.value = targetValue;
                }
            }

            if (IsPlayTransLerp)
            {
                if (totalTime >= currentTime)
                {
                    currentTime += Time.deltaTime;
                    SetTransFill( Mathf.Lerp( startValue, endValue, currentTime / totalTime ) );
                }
                else
                {
                    slider.value = endValue;
                    IsPlayTransLerp = false;
                }
            }
        }

        private bool IsPlayTransSmooth = false;

        private bool IsPlayTransLerp = false;

        public void SetTransFillColor ( uint color ) 
        {
            if (TransFill)
                TransFill.color = UIManager.RGBA( color );
        }

        private string TempPath;
        public void SetTransFillSprite ( string ResPath, bool IsAsyn )
        {
            if (TransFill)
            {
                TempPath = ResPath;

                string path = ResPath;
                panelmanager.SynGetSprite( ResPath, ( s ) =>
                {
                    if (this == null || TransFill == null || TransFill.sprite == s || path == TempPath || s == null)
                    {
                        return;
                    }
                    if (s.border == Vector4.zero && TransFill.type != Image.Type.Filled)
                    {
                        TransFill.type = Image.Type.Simple;
                    }
                    else if (s.border != Vector4.zero && TransFill.type != Image.Type.Filled)
                    {
                        TransFill.type = Image.Type.Sliced;
                    }
                    TransFill.sprite = s;

                }, IsAsyn );
            }
        }

        public void SetValueSmoothWithTransition ( float value, float time, float MaxSpeed ) 
        {
            if (value > slider.maxValue)
            {
                ULogger.Warn( "设置的值大于最大值" );

                return;
            }

            if (value < slider.minValue)
            {
                ULogger.Warn( "设置的值小于最小值" );

                return;
            }

            if (value == slider.value)
            {
                return;
            }

            if (TransFill != null)
            {
                if (value > slider.value)
                {
                    SetTransFill( value );

                    SetValueSmoothAni( value, time, MaxSpeed );
                }
                else
                {
                    if (GetTransFill() < slider.value)
                    {
                        SetTransFill( slider.value );
                    }

                    slider.value = value;

                    SetTranFillSmoothAni( value, time, MaxSpeed );
                }
            }
        }

        public void SetValueLerpWithTransition ( float value, float time ) 
        {
            if (value > slider.maxValue)
            {
                ULogger.Warn( "设置的值大于最大值" );

                return;
            }

            if (value < slider.minValue)
            {
                ULogger.Warn( "设置的值小于最小值" );

                return;
            }

            if (value == slider.value)
            {
                return;
            }

            if (TransFill != null)
            {
                if (value > slider.value)
                {
                    SetTransFill( value );

                    SetValueLerpAni( value, time );
                }
                else
                {
                    if (GetTransFill() < slider.value)
                    {
                        SetTransFill( slider.value );
                    }

                    slider.value = value;

                    SetTranFillLerpAni( value, time );
                }
            }
        }

        private RectTransform mFillAera 
        {
            get 
            {
                if (slider.fillRect != null)
                {
                    return slider.fillRect.parent as RectTransform;
                }
                else
                {
                    return null;
                }
            }
        }

        private Image TransFill
        {
            get 
            {
                if (mAlphaFill == null)
                {
                    CreateFill();
                }
                return mAlphaFill;
            }
        }

        private Image mAlphaFill;
        private Image CreateFill ( ) 
        {
            if (mAlphaFill != null)
            {
                return mAlphaFill;
            }

            if (slider.fillRect != null)
            {
                GameObject go = new GameObject("AlphaSlider");
                go.transform.SetParent( mFillAera, false );
                RectTransform rect = go.AddComponent<RectTransform>();
                rect.SetSiblingIndex( 0 );
                mAlphaFill = go.AddComponent<Image>();
                rect.sizeDelta = mFillAera.rect.size;
                switch (slider.direction)
                {
                    case Slider.Direction.BottomToTop:
                        rect.anchorMin = new Vector2( 0.5f, 0 );
                        rect.anchorMax = new Vector2( 0.5f, 0 );
                        rect.pivot = new Vector2( 0.5f, 0 );
                        break;
                    case Slider.Direction.LeftToRight:
                        rect.anchorMin = new Vector2( 0, 0.5f );
                        rect.anchorMax = new Vector2( 0, 0.5f );
                        rect.pivot = new Vector2( 0, 0.5f );
                        break;
                    case Slider.Direction.RightToLeft:
                        rect.anchorMin = new Vector2( 1, 0.5f );
                        rect.anchorMax = new Vector2( 1, 0.5f );
                        rect.pivot = new Vector2( 1, 0.5f );
                        break;
                    case Slider.Direction.TopToBottom:
                        rect.anchorMin = new Vector2( 0.5f, 1 );
                        rect.anchorMax = new Vector2( 0.5f, 1 );
                        rect.pivot = new Vector2( 0.5f, 1 );
                        break;
                    default:
                        break;
                }

                SetTransFill(slider.value);

                return mAlphaFill;
            }
            else
            {
                return null;
            }
        }

        private float TransFillValue = -1f;
        private void SetTransFill ( float value ) 
        {
            TransFillValue = value;
            if (TransFill == null)
            {
                return;
            }

            if (slider.direction == Slider.Direction.LeftToRight || slider.direction == Slider.Direction.RightToLeft)
            {
                float widthValue = value / ( slider.maxValue - slider.minValue ) * mFillAera.rect.width;
                TransFill.rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, widthValue );
            }
            else
            {
                float heightValue = value / ( slider.maxValue - slider.minValue ) * mFillAera.rect.height;
                TransFill.rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, heightValue );
            }
        }

        private float GetTransFill ( ) 
        {
            if (TransFillValue == -1f)
            {
                TransFillValue = slider.value;
            }
            return TransFillValue;
        }

        private void SetTranFillSmoothAni ( float value, float time, float MaxSpeed ) 
        {
            IsPlayTransSmooth = true;
            IsPlayLerp = false;
            IsPlaySmooth = false;
            IsPlayTransLerp = false;

            currentVelocityTrans = 0f;
            targetValue = value;
            totalTime = time;
            maxSpeed = MaxSpeed;
        }

        private void SetTranFillLerpAni ( float value, float time )
        {
            IsPlayTransSmooth = false;
            IsPlayLerp = false;
            IsPlaySmooth = false;
            IsPlayTransLerp = true;

            startValue = GetTransFill();
            endValue = value;
            totalTime = time;
            currentTime = 0;
        }

        void OnClickLuaFunc(float value)
        {
            if (onValueChange != null)
            {
                onValueChange( value );
            }
        }

        public void AddValueChangeEvent ( Action<float> luafunc )
        {
            onValueChange = luafunc;
        }

        public void RemoveValueChangeEvent(int index)
        {
        }

        public void RemoveAllValueChangeEvent()
        {
        }

        public override void OnClose ( )
        {
            base.OnClose();
            IsPlaySmooth = false;
            IsPlayLerp = false;
            onValueChange = null;
        }
    }
}
