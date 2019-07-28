using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UEngine;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaJoystick : ULuaUIBase
    {
        UJoystick _joystick;
        UJoystick joystick 
        {
            get
            {
                if (!_joystick)
                {
                    _joystick = GetComponent<UJoystick>();
                }
                return _joystick;
            }
        }

        public Action<Vector3> OnMoveAction 
        {
            get 
            {
                return joystick.onMove;
            }
        }

        Action<Vector3> onMove;
        Action onStart;
        Action onStop;
        void OnMove(Vector3 pos) 
        {
            if (onMove != null)
            {
                onMove(pos);
            }
        }

        void OnStart() 
        {
            if (onStart != null)
            {
                onStart();
            }
        }

        void OnStop()
        {
            if (onStop != null)
            {
                onStop();
            }
        }

        public void AddOnMoveEvent(Action<Vector3> luafunc) 
        {
            if (joystick.onMove == null)
            {
                joystick.onMove = OnMove;
            }
            onMove = luafunc;
        }

        public virtual void RemoveOnMove(int index)
        {
        }

        public virtual void RemoveAllOnMove()
        {
        }

        public void AddOnStartEvent(Action luafunc)
        {
            if (joystick.onStart == null)
            {
                joystick.onStart = OnStart;
            }
            onStart = luafunc;
        }

        public virtual void RemoveOnStart(int index)
        {
        }

        public virtual void RemoveAllOnStart()
        {
        }

        public void AddOnStopEvent(Action luafunc)
        {
            if (joystick.onStop == null)
            {
                joystick.onStop = OnStop;
            }
            onStop = luafunc;
        }

        public void OnDragOut ( ) 
        {
            if (joystick)
            {
                joystick.OnDragOut();
            }
        }

        public virtual void RemoveOnStop(int index)
        {
        }

        public virtual void RemoveAllOnStop()
        {
        }

        public override void OnClose ( )
        {
            base.OnClose();
            onMove = null;
            onStart = null;
            onStop = null;
        }
    }
}
