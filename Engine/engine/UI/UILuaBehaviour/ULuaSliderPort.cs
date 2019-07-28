using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaSliderPort : ULuaUIBasePort
    {
        ULuaSlider _slider;
        ULuaSlider slider
        {
            get
            {
                if (_slider == null)
                {
                    if (behbase != null)
                    {
                        _slider = (ULuaSlider)behbase;
                    }
                }
                return _slider;
            }
        }

        public float GetValue() 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaSliderPort_GetValue_对象被销毁仍调用接口" );
                return 0f;
            }
            return slider.GetValue();
        }

        public void SetMaxOrMinValue(float maxvalue = 1, float minvalue = 0) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaSliderPort_SetMaxOrMinValue_对象被销毁仍调用接口" );
                return;
            }
            slider.SetMaxOrMinValue(maxvalue, minvalue);
        }

        public void SetValue(float value) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaSliderPort_SetValue_对象被销毁仍调用接口" );
                return;
            }
            slider.SetValue( value );
        }

        public void SetTransFillColor ( uint color ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaSliderPort_SetTransFillColor_对象被销毁仍调用接口" );
                return;
            }
            slider.SetTransFillColor( color );
        }

        public void SetTransFillSprite ( string ResPath, bool IsAsyn ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaSliderPort_SetTransFillSprite_对象被销毁仍调用接口" );
                return;
            }
            slider.SetTransFillSprite( ResPath, IsAsyn );
        }

        public void SetValueSmoothAni ( float value, float time, float MaxSpeed ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaSliderPort_SetValueSmoothAni_对象被销毁仍调用接口" );
                return;
            }
            slider.SetValueSmoothAni( value, time, MaxSpeed );
        }

        public void SetValueLerpAni ( float value, float time ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaSliderPort_SetValueLerpAni_对象被销毁仍调用接口" );
                return;
            }
            slider.SetValueLerpAni( value, time );
        }

        public void SetValueSmoothWithTransition ( float value, float time, float MaxSpeed ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaSliderPort_SetValueSmoothWithTransition_对象被销毁仍调用接口" );
                return;
            }
            slider.SetValueSmoothWithTransition( value, time, MaxSpeed );
        }

        public void SetValueLerpWithTransition ( float value, float time ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaSliderPort_SetValueLerpWithTransition_对象被销毁仍调用接口" );
                return;
            }
            slider.SetValueLerpWithTransition( value, time );
        }

        public void AddValueChangeEvent(Action<float> luafunc)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaSliderPort_AddValueChangeEvent_对象被销毁仍调用接口" );
                return;
            }
            slider.AddValueChangeEvent(luafunc);
        }

        public void RemoveValueChangeEvent(int index)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaSliderPort_RemoveValueChangeEvent_对象被销毁仍调用接口" );
                return;
            }
            slider.RemoveValueChangeEvent(index);
        }

        public void RemoveAllValueChangeEvent()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaSliderPort_RemoveAllValueChangeEvent_对象被销毁仍调用接口" );
                return;
            }
            slider.RemoveAllValueChangeEvent();
        }

        public override int GetUIType()
        {
            return (int)UIType.Slider;
        }
    }
}
