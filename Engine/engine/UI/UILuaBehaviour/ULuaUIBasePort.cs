using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaUIBasePort : ULuaBehaviourBasePort
    {
        ULuaUIBase _uibase;
        ULuaUIBase uibase 
        {
            get 
            {
                if (_uibase == null)
                {
                    if (behbase != null)
                    {
                        _uibase = (ULuaUIBase)behbase;
                    }
                }
                return _uibase;
            }
        }

        public void SetBtenable ( bool isEnable ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetBtenable_对象被销毁仍调用接口" );
                return;
            }
            uibase.SetBtenable( isEnable );
        }

        public void SetBtenableImage ( bool isEnable, string path = "ui/tongyong/an_hui.png", bool IsAsyn = UIManager.IsAsyn ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetBtenableImage_对象被销毁仍调用接口" );
                return;
            }
            uibase.SetBtenableImage( isEnable, path, IsAsyn );
        }

        public bool GetBtenable() 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_GetBtenable_对象被销毁仍调用接口" );
                return false;
            }
            return uibase.GetBtenable();
        }

        public virtual void SetImage ( string ResPath, bool IsAsyn = true )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetImage_对象被销毁仍调用接口" );
                return;
            }
            if (uibase)
            {
                if (!IsAsyn)
                {
                    uibase.SetImage( ResPath, false );
                }
                else
                {
                    uibase.SetImage( ResPath, USystemConfig.Instance.UILoadResIsAsyn );
                }
            }
            else
            {
                ULogger.Error("uibase的对象为空");
            }
        }

        public void SetNetImage ( string URL ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetNetImage_对象被销毁仍调用接口" );
                return;
            }
            uibase.SetNetImage( URL );
        }

        public void SetTextSize(int size) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetTextSize_对象被销毁仍调用接口" );
                return;
            }
            uibase.SetTextSize(size);
        }

        public void SetFontStyle ( int style ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetFontStyle_对象被销毁仍调用接口" );
                return;
            }
            uibase.SetFontStyle( style );
        }

        public virtual void SetText(string value) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetText_对象被销毁仍调用接口" );
                return;
            }
            if (uibase)
            {
                uibase.SetText(value);
            }
            else
            {
                ULogger.Error("uibase的对象为空");
            }
        }

        public void PlayTextAni ( float TimeInterval, Action AniCallBack = null )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_PlayTextAni_对象被销毁仍调用接口" );
                return;
            }
            if (uibase)
            {
                uibase.PlayTextAni( TimeInterval, AniCallBack );
            }
            else
            {
                ULogger.Error( "uibase的对象为空" );
            }
        }

        public void StopTextAni ( bool IsComplete = true )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_StopTextAni_对象被销毁仍调用接口" );
                return;
            }
            if (uibase)
            {
                uibase.StopTextAni( IsComplete );
            }
            else
            {
                ULogger.Error( "uibase的对象为空" );
            }
        }

        public virtual string GetText() 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_GetText_对象被销毁仍调用接口" );
                return "";
            }
            if (uibase)
            {
                return uibase.GetText();
            }
            else
            {
                ULogger.Error("uibase的对象为空");
                return "";
            }
        }

        public void SetFillAmount(float percent)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetFillAmount_对象被销毁仍调用接口" );
                return;
            }
            uibase.SetFillAmount(percent);
        }

        public void SetTransColor ( uint color )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetTransFillColor_对象被销毁仍调用接口" );
                return;
            }
            uibase.SetTransColor( color );
        }

        public void SetTransSprite ( string ResPath, bool IsAsyn )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetTransFillSprite_对象被销毁仍调用接口" );
                return;
            }
            uibase.SetTransSprite( ResPath, IsAsyn );
        }

        public void SetFillSmoothAni ( float value, float time, float MaxSpeed ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetFillSmoothAni_对象被销毁仍调用接口" );
                return;
            }
            uibase.SetFillSmoothAni( value, time, MaxSpeed );
        }

        public void SetFillSmoothWithTransition ( float value, float time, float MaxSpeed ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetFillSmoothWithTransition_对象被销毁仍调用接口" );
                return;
            }
            uibase.SetFillSmoothWithTransition( value, time, MaxSpeed );
        }

        public virtual void SetColor(uint color) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetColor_对象被销毁仍调用接口" );
                return;
            }
            if (uibase)
            {
                uibase.SetColor(color);
            }
            else
            {
                ULogger.Error("uibase的对象为空");
            }
        }

        public void SetGradientColor(uint topcolor, uint bottomcolor)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetGradientColor_对象被销毁仍调用接口" );
                return;
            }
            uibase.SetGradientColor(topcolor, bottomcolor);
        }

        public void SetOutLineSize ( int size ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetOutLineSize_对象被销毁仍调用接口" );
                return;
            }
            uibase.SetOutLineSize( size );
        }

        public void SetOutLineColor(uint OutLineColor)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetOutLineColor_对象被销毁仍调用接口" );
                return;
            }
            uibase.SetOutLineColor(OutLineColor);
        }

        public void SetRaycastTarget ( bool enable ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetRaycastTarget_对象被销毁仍调用接口" );
                return;
            }
            uibase.SetRaycastTarget(enable);
        }

        public virtual void AddClick ( Action luafunc )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_AddClick_对象被销毁仍调用接口" );
                return;
            }
            if (uibase)
            {
                uibase.AddClick(luafunc);
            }
            else
            {
                ULogger.Error("uibase的对象为空");
            }
        }

        public virtual void RemoveClick(int index)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_RemoveClick_对象被销毁仍调用接口" );
                return;
            }
            if (uibase)
            {
                uibase.RemoveClick(index);
            }
            else
            {
                ULogger.Error("uibase的对象为空");
            }
        }

        public virtual void RemoveAllClick()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_RemoveAllClick_对象被销毁仍调用接口" );
                return;
            }
            if (uibase)
            {
                uibase.RemoveAllClick();
            }
            else
            {
                ULogger.Error("uibase的对象为空");
            }
        }

        public virtual void AddUpClick(Action luafunc)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_AddUpClick_对象被销毁仍调用接口" );
                return;
            }
            if (uibase)
            {
                uibase.AddUpClick(luafunc);
            }
            else
            {
                ULogger.Error("uibase的对象为空");
            }
        }

        public virtual void RemoveUpClick(int index)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_RemoveUpClick_对象被销毁仍调用接口" );
                return;
            }
            if (uibase)
            {
                uibase.RemoveUpClick(index);
            }
            else
            {
                ULogger.Error("uibase的对象为空");
            }
        }

        public virtual void RemoveAllUpClick()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_RemoveAllUpClick_对象被销毁仍调用接口" );
                return;
            }
            if (uibase)
            {
                uibase.RemoveAllUpClick();
            }
            else
            {
                ULogger.Error("uibase的对象为空");
            }
        }

        public virtual void AddDownClick ( Action luafunc )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_AddDownClick_对象被销毁仍调用接口" );
                return;
            }
            if (uibase)
            {
                uibase.AddDownClick(luafunc);
            }
            else
            {
                ULogger.Error("uibase的对象为空");
            }
        }

        public virtual void RemoveDownClick(int index)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_RemoveDownClick_对象被销毁仍调用接口" );
                return;
            }
            if (uibase)
            {
                uibase.RemoveDownClick(index);
            }
            else
            {
                ULogger.Error("uibase的对象为空");
            }
        }

        public virtual void RemoveAllDownClick()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_RemoveAllDownClick_对象被销毁仍调用接口" );
                return;
            }
            if (uibase)
            {
                uibase.RemoveAllDownClick();
            }
            else
            {
                ULogger.Error("uibase的对象为空");
            }
        }

        public void AddDragEvent ( Action<Vector2> luafunc )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_AddDragEvent_对象被销毁仍调用接口" );
                return;
            }
            uibase.AddDragEvent(luafunc);
        }

        public virtual void RemoveDragEvent(int index)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_RemoveDragEvent_对象被销毁仍调用接口" );
                return;
            }
            uibase.RemoveDragEvent(index);
        }

        public virtual void RemoveAllDragEvent()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_RemoveAllDragEvent_对象被销毁仍调用接口" );
                return;
            }
            uibase.RemoveAllDragEvent();
        }

        public virtual void AddDragOutEvent ( Action luafunc )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_AddDragOutEvent_对象被销毁仍调用接口" );
                return;
            }
            uibase.AddDragOutEvent(luafunc);
        }

        public virtual void RemoveDragOutEvent(int index)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_RemoveDragOutEvent_对象被销毁仍调用接口" );
                return;
            }
            uibase.RemoveDragOutEvent(index);
        }

        public virtual void RemoveAllDragOutEvent()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_RemoveAllDragOutEvent_对象被销毁仍调用接口" );
                return;
            }
            uibase.RemoveAllDDragOutEvent();
        }

        public virtual void AddDragEnterEvent(Action luafunc, ULuaUIBasePort basePort = null )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_AddDragEnterEvent_对象被销毁仍调用接口" );
                return;
            }

            if (basePort == null || !basePort.uibase)
                uibase.AddDragEnterEvent( luafunc );
            else
                uibase.AddDragEnterEvent( luafunc, basePort.uibase );
        }

        public virtual void RemoveDragEnterEvent(int index)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_RemoveDragEnterEvent_对象被销毁仍调用接口" );
                return;
            }
            uibase.RemoveDragEnterEvent(index);
        }

        public virtual void RemoveAllDragEnterEvent()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_RemoveAllDragEnterEvent_对象被销毁仍调用接口" );
                return;
            }
            uibase.RemoveAllDDragEnterEvent();
        }

        public void SetParentDrag ( bool IsDrag ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetParentDrag_s对象被销毁仍调用接口" );
                return;
            }
            uibase.SetParentDrag( IsDrag );
        }

        public override int GetUIType()
        {
            return (int)UIType.Image;
        }

        public void SetGray(bool IsGray) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetGray_对象被销毁仍调用接口" );
                return;
            }
            uibase.SetGray(IsGray);
        }

        public void SetGrayChild(bool IsGray, bool InCludeSelf = true) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetGrayChild_对象被销毁仍调用接口" );
                return;
            }
            uibase.SetGrayChild(IsGray, InCludeSelf);
        }

        public void SetGuideTarget ( ULuaUIBasePort uiBase ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetGuideTarget_对象被销毁仍调用接口" );
                return;
            }
            if (uiBase != null)
            {
                uibase.SetGuideTarget(uiBase.uibase);
            }
        }

        public void SeteventAlphaThreshold ( bool isThreshold )
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SeteventAlphaThreshold_对象被销毁仍调用接口" );
                return;
            }
            uibase.SeteventAlphaThreshold( isThreshold );
        }

        public void SetEventPassBefore ( bool IsBefore ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaUIBasePort_SetEventPassBefore_对象被销毁仍调用接口" );
                return;
            }
            uibase.SetEventPassBefore( IsBefore );
        }
    }
}
