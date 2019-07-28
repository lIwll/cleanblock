using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaRawImagePort : ULuaUIBasePort
    {
        ULuaRawImage _rawImage;
        ULuaRawImage rawImage
        {
            get
            {
                if (_rawImage == null)
                {
                    if (behbase != null)
                    {
                        _rawImage = (ULuaRawImage)behbase;
                    }
                }
                return _rawImage;
            }
        }

		public void CaptureScreen(int scale = 8, int blur = 1)
		{
            if (IsClose)
            {
                ULogger.Warn( "ULuaRawImagePort_CaptureScreen_对象被销毁仍调用接口" );
                return;
            }
			rawImage.CaptureScreen(scale, blur);
		}

		public void CreatePreviewActor(IActor actor, string cubemapPath = "", float offset_Y = 0f, float ar = 0.6764706f, float ag = 0.6764706f, float ab = 0.6764706f, float _Intensity = 0.429f, bool SceneLight = true, float dr = 0.82f, float dg = 0.77f, float db = 0.72f, float difIntensity = 1.0f) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaRawImagePort_CreatePreviewActor_对象被销毁仍调用接口" );
                return;
            }
            rawImage.CreatePreviewActor(actor, cubemapPath, offset_Y, ar, ag, ab, _Intensity, SceneLight, dr, dg, db, difIntensity);
        }

        public void SetCameraRotation(float angle)
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaRawImagePort_SetCameraRotation_对象被销毁仍调用接口" );
                return;
            }
            rawImage.SetCameraRotation(angle);
        }

        public void SetCameraAngle ( float angle ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaRawImagePort_SetCameraAngle_对象被销毁仍调用接口" );
                return;
            }
            rawImage.SetCameraAngle( angle );
        }

        public void SetRotateSpeed ( float RotateSpeed, float Angle_Z = 0f ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaRawImagePort_SetRotateSpeed_对象被销毁仍调用接口" );
                return;
            }
            rawImage.SetRotateSpeed( RotateSpeed, Angle_Z );
        }

        public void SetRendererMode ( int Mode ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaRawImagePort_SetRendererMode_对象被销毁仍调用接口" );
                return;
            }
            rawImage.SetRendererMode( Mode );
        }

        public void SetManualSpeed ( float Speed ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaRawImagePort_SetManualSpeed_对象被销毁仍调用接口" );
                return;
            }
            rawImage.SetManualSpeed( Speed );
        }
        
        public void PlayVideo ( string path, int width, int height, Action onFinished, float volume ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "ULuaRawImagePort_PlayVideo_对象被销毁仍调用接口" );
                return;
            }
            rawImage.PlayVideo( path, width, height, onFinished, volume );
        }

        public void Stop ( ) 
        {
            rawImage.StopVideo();
        }

        public override void OnClose() 
        {
            rawImage.OnClose();
        }

        public override int GetUIType()
        {
            return (int)UIType.RawImage;
        }

    }
}
