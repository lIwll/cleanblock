using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaJoystickPort : ULuaUIBasePort
    {
        public ULuaJoystick _joystick;
        public ULuaJoystick joystick
        {
            get
            {
                if (_joystick == null)
                {
                    if (behbase != null)
                    {
                        _joystick = (ULuaJoystick)behbase;
                    }
                }
                return _joystick;
            }
        }

        public Action<Vector3> OnMoveAction 
        {
            get 
            {
                return joystick.OnMoveAction;
            }
        }

        public void AddOnMoveEvent(Action<Vector3> luafunc)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaJoystickPort_AddOnMoveEvent_对象被销毁仍调用接口" );
                return;
            }
            joystick.AddOnMoveEvent(luafunc);
        }

        public virtual void RemoveOnMove(int index)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaJoystickPort_RemoveOnMove_对象被销毁仍调用接口" );
                return;
            }
            joystick.RemoveOnMove(index);
        }

        public virtual void RemoveAllOnMove()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaJoystickPort_RemoveAllOnMove_对象被销毁仍调用接口" );
                return;
            }
            joystick.RemoveAllOnMove();
        }

        public void AddOnStartEvent(Action luafunc)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaJoystickPort_AddOnStartEvent_对象被销毁仍调用接口" );
                return;
            }
            joystick.AddOnStartEvent(luafunc);
        }

        public virtual void RemoveOnStart(int index)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaJoystickPort_RemoveOnStart_对象被销毁仍调用接口" );
                return;
            }
            joystick.RemoveOnStart(index);
        }

        public virtual void RemoveAllOnStart()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaJoystickPort_RemoveAllOnStart_对象被销毁仍调用接口" );
                return;
            }
            joystick.RemoveAllOnStart();
        }

        public void AddOnStopEvent(Action luafunc)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaJoystickPort_AddOnStopEvent_对象被销毁仍调用接口" );
                return;
            }
            joystick.AddOnStopEvent(luafunc);
        }

        public void OnDragOut ( ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaJoystickPort_OnDragOut_对象被销毁仍调用接口" );
                return;
            }
            joystick.OnDragOut();
        }

        public virtual void RemoveOnStop(int index)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaJoystickPort_RemoveOnStop_对象被销毁仍调用接口" );
                return;
            }
            joystick.RemoveOnStop(index);
        }

        public virtual void RemoveAllOnStop()
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaJoystickPort_RemoveAllOnStop_对象被销毁仍调用接口" );
                return;
            }
            joystick.RemoveAllOnStop();
        }

        public override int GetUIType()
        {
            return (int)UIType.JoyStick;
        }
    }
}
