using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using UEngine;
using UEngine.ULua;
using UEngine.UIExpand;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;

namespace UEngine.UI.UILuaBehaviour
{
    class ULuaRawImage : ULuaUIBase
    {
        RawImage _rawImage;

        RawImage rawImage 
        {
            get 
            {
                if (!_rawImage)
                {
                    _rawImage = GetComponent<RawImage>();
                }
                return _rawImage;
            }
        }

        RenderTexture renderTex;
        
        Texture2D captureImage = null;

        RenderTask mRenderTask;

        RendererMode mRendererMode = RendererMode.none;

        UIEventListener mEventListener;
        UIEventListener EventListener 
        {
            get 
            {
                if (!mEventListener)
                {
                    mEventListener = UIEventListener.Get(gameObject);
                }
                return mEventListener;
            }
        }

        IActor mActor;

        float currentTime = 0f;

        int tempX = 0;
        int tempY = 0;

        void Update() 
        {
            if (mRendererMode == RendererMode.Autogyration && mRenderTask != null && mRenderTask.RotateSpeed > 0f)
            {
                mRenderTask.Angle_Y = Mathf.Lerp(0, 360f, currentTime);
                currentTime += mRenderTask.RotateSpeed * Time.deltaTime;
                if (currentTime > 1)
                {
                    currentTime = 0;
                }
            }
        }
/*
		public void OnRenderObject()
		{
		}
*/
		public void CaptureScreen(int scale, int blur)
		{
			if (null == UCore.MainCamera)
                return;

            var camera = new GameObject("Capture Camera", typeof(Camera)) { hideFlags = HideFlags.HideAndDontSave }.GetComponent< Camera >();
            camera.CopyFrom(UCore.MainCamera);
			camera.enabled = true;

			var oriTex = camera.targetTexture;

			var captureTex = RenderTexture.GetTemporary(Screen.width / scale, Screen.height / scale, 16, RenderTextureFormat.Default);

			camera.targetTexture = captureTex;
			camera.Render();
			camera.targetTexture = oriTex;

            DestroyImmediate(camera.gameObject);

			rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, 1);
			rawImage.texture = BlurImage(captureTex, blur);

			if (null != captureTex)
				RenderTexture.ReleaseTemporary(captureTex);
			captureTex = null;
		}

		Texture BlurImage(RenderTexture captureTex, int blur)
		{
			RenderTexture rendText = RenderTexture.active;

			RenderTexture.active = captureTex;

			if (null != captureImage)
				GameObject.DestroyImmediate(captureImage);
			captureImage = new Texture2D(captureTex.width, captureTex.height, TextureFormat.RGB24, false);
			captureImage.ReadPixels(new Rect(0, 0, captureTex.width, captureTex.height), 0, 0);
			captureImage.Apply();

			RenderTexture.active = rendText;

			Color32[] pixels = captureImage.GetPixels32();

			pixels = BlurImage(pixels, captureTex.width, captureTex.height, blur);

			captureImage.SetPixels32(pixels);
			captureImage.Apply();

			return captureImage;
		}

		public void CreatePreviewActor(IActor actor, string cubemapPath, float offset_Y, float ar, float ag, float ab, float _Intensity, bool SceneLight, float dr, float dg, float db, float difIntensity) 
        {
            //RebuildActor( mRenderTask );

            float startTime = Time.realtimeSinceStartup;
            currentTime = 0;
            
            if (actor == null)
            {
                ULogger.Warn("传入的Actor对象为空");
                return;
            }
            else
            {
                mActor = actor;
            }

            actor.GetAddChlidEvent = UpdateActor;

            Cubemap cubeMap = null;
            if (!string.IsNullOrEmpty(cubemapPath))
            {
                panelmanager.SynGetRes( cubemapPath, ( IRes ) =>
                {
                    if (null != IRes)
                        cubeMap = IRes.Res as Cubemap;
                }, false );

            }

            float x = recttransform.rect.width;
            float y = recttransform.rect.height;

            float resultX = USystemConfig.Instance.Anti_AliasingCoefficient * x;
            float resultY = USystemConfig.Instance.Anti_AliasingCoefficient * y;

            if (resultX > Screen.width || resultY > Screen.height)
            {
                if (resultX > resultY)
                {
                    resultY = resultY / resultX * Screen.width;
                    resultX = Screen.width;
                }
                else
                {
                    resultX = resultX / resultY * Screen.height;
                    resultY = Screen.height;
                }
            }

            if (renderTex == null)
            {
			    renderTex = RenderTexture.GetTemporary((int)resultX, (int)resultY, 16, RenderTextureFormat.Default);
                rawImage.texture = renderTex;

                tempX = ( int )resultX;
                tempY = ( int )resultY;
            }
            else if (renderTex != null && tempX != ( int )resultX && tempY != ( int )resultY)
            {
                RenderTexture.ReleaseTemporary( renderTex );
                rawImage.texture = null;

                renderTex = RenderTexture.GetTemporary( ( int )resultX, ( int )resultY, 16, RenderTextureFormat.Default );
                rawImage.texture = renderTex;

                tempX = ( int )resultX;
                tempY = ( int )resultY;
            }

            rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, 1);

            GameObject go = actor.GetGameObj();

            BoxCollider collider = go.GetComponent<BoxCollider>();

            if (collider && offset_Y <= 0f)
            {
                offset_Y = collider.center.y * go.transform.localScale.y ;
            }
            int index = 0;
            if (mRenderTask == null && go)
            {
                actor.SetLayer("UI_Preview");
				mRenderTask = new RenderTask(renderTex, actor, 0, offset_Y, cubeMap, new Color(ar, ag, ab), _Intensity, SceneLight, new Color(dr, dg, db), difIntensity);
                UIManager.mRenderTasks.Add( mRenderTask );
                index = UIManager.mRenderTasks.Count - 1;
            }
            else
            {
                if (UIManager.mRenderTasks.Contains(mRenderTask))
                {
                    actor.SetLayer("UI_Preview");
                    for (int i = 0 ; i < UIManager.mRenderTasks.Count ; i++)
                    {
                        if (mRenderTask == UIManager.mRenderTasks[i])
                        {
                            index = i;
                            break;
                        }
                    }
                    mRenderTask = new RenderTask( renderTex, actor, 0, offset_Y, cubeMap, new Color( ar, ag, ab ), _Intensity, SceneLight, new Color( dr, dg, db ), difIntensity );
                    UIManager.mRenderTasks[index] = mRenderTask;
                    actor.SetPos( go.transform.position + UIManager.GetPreviewPosition(index)/*new Vector3( index * 100 + 10000, index * 100 + 10000, 0 )*/ );
                    mRenderTask.index = index;
                    return;
                }
                else
                {
                    actor.SetLayer( "UI_Preview" );
                    mRenderTask = new RenderTask( renderTex, actor, 0, offset_Y, cubeMap, new Color( ar, ag, ab ), _Intensity, SceneLight, new Color( dr, dg, db ), difIntensity );
                }
                UIManager.mRenderTasks.Add(mRenderTask);
                index = UIManager.mRenderTasks.Count - 1;
            }
            actor.SetPos( go.transform.position + UIManager.GetPreviewPosition(index)/*new Vector3( index * 100 + 10000, index * 100 + 10000, 0 )*/ );
            mRenderTask.index = index;
            //Debug.Log( "Actor TempBuffer Create Time : " + ( Time.realtimeSinceStartup - startTime ) );
        }

        public void SetRotateSpeed ( float RotateSpeed, float Angle_Z ) 
        {
            if (mRenderTask != null)
            {
                mRenderTask.RotateSpeed = RotateSpeed;
            }
            transform.localEulerAngles = new Vector3( transform.localEulerAngles.x, transform.localEulerAngles.y, Angle_Z );
        }

        public void SetRendererMode ( int Mode ) 
        {
            mRendererMode = ( RendererMode )Mode;
            if (mRendererMode == RendererMode.ManualMode)
            {
                EventListener.onDrag = RotateCamera;
            }
            else if (mRendererMode == RendererMode.none)
            {
                currentTime = 0;
                if (mRenderTask != null)
                {
                    mRenderTask.Angle_Y = 0;
                }
            }
        }

        private void RotateCamera ( Vector2 delta ) 
        {
            if (mRenderTask != null && mRendererMode == RendererMode.ManualMode)
            {
                mRenderTask.Angle_Y += delta.x * ManualSpeed;
            }
        }

        float ManualSpeed = 0f;
        public void SetManualSpeed ( float Speed ) 
        {
            if (mRendererMode == RendererMode.ManualMode)
            {
                ManualSpeed = Speed;
            }
        }

        public void SetCameraRotation(float angle)
        {
            if (mRenderTask != null)
            {
                mRenderTask.Angle = angle;
            }
        }

        public void SetCameraAngle ( float angle )
        {
            if (mRenderTask != null)
            {
                mRenderTask.Angle_Y = angle;
            }
        }

        public override void OnClose()
        {
            if (mRenderTask != null && UIManager.mRenderTasks.Contains(mRenderTask))
            {
                //RebuildActor( mRenderTask );
                UIManager.mRenderTasks.Remove(mRenderTask);
            }
            if (renderTex)
            {
                UIManager.mCameraPreview.targetTexture = null;
				RenderTexture.ReleaseTemporary(renderTex);
                //renderTex.Release();
				renderTex = null;
            }

			if (null != captureImage)
				GameObject.DestroyImmediate(captureImage);
			captureImage = null;

            rawImage.color = new Color( rawImage.color.r, rawImage.color.g, rawImage.color.b, 0 );

            base.OnClose();

            EventListener.ClearEvent();
        }

        public void UpdateActor() 
        {
            if (mActor == null)
            {
                ULogger.Error("传入的Actor对象为空");
                return;
            }

            mActor.SetLayer("UI_Preview");
        }

        public void RebuildActor ( RenderTask mRender ) 
        {
            if (mRender != null)
            {
                //for (int i = 0 ; i < mRender.rendererList.Count ; i++)
                //{
                //    if (mRender.rendererList[i] != null)
                //    {
                //        mRender.rendererList[i].enabled = true;
                //    }
                //}
            }
        }

        enum RendererMode 
        {
            none,
            Autogyration,
            ManualMode,
        }

		Color32[] BlurImage(Color32[] image, int width, int height, int radius)
		{
			Color32[] newImage = new Color32[width * height];

			for (int y = 0; y < height; y ++)
			{
				for (int x = 0; x < width; x ++)
				{
					int r = 0, g = 0, b = 0;
					// blur pixel
					for (int r1 = -radius; r1 <= radius; r1 ++)
					{
						for (int r2 = -radius; r2 <= radius; r2 ++)
						{
							int y1 = Mathf.Clamp(y + r1, 0, height - 1);
							int x1 = Mathf.Clamp(x + r2, 0, width - 1);

							Color32 col = image[y1 * width + x1];

							r += col.r;
							g += col.g;
							b += col.b;
						}
					}

					var div = (radius * 2 + 1) * (radius * 2 + 1);
					r /= div;
					g /= div;
					b /= div;

					newImage[y * width + x] = new Color32((byte)r, (byte)g, (byte)b, 255);
				}
			}

			return newImage;
		}

        IEnumerator mPlayVideo = null;
        public void PlayVideo ( string path, int width, int height, Action onFinished, float volume )
        {
            Application.runInBackground = true;

            if (null != mPlayVideo)
                StopCoroutine( mPlayVideo );
            mPlayVideo = _PlayVideo( path, width, height, onFinished, volume );

            StartCoroutine( mPlayVideo );
        }

        void VideoError ( VideoPlayer source, string message )
        {
            ULogger.Error( "play video error: {0}", message );
        }

        bool mPrepared = false;
        void VideoPrepared ( VideoPlayer source )
        {
            mPrepared = true;
        }

        bool mPlayed = false;
        void VideoPlayed ( VideoPlayer source )
        {
            mPlayed = true;
        }
        
        VideoPlayer videoPlayer;
        bool IsStoped = false;
        public void StopVideo ( ) 
        {
            IsStoped = true;
            mPrepared = true;
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
            }
        }

        IEnumerator _PlayVideo ( string path, int width, int height, System.Action onFinished = null, float volume = 1f)
        {
            mPrepared = false;
            mPlayed = false;

            RenderTexture targetTexture = RenderTexture.GetTemporary( width, height, 0, RenderTextureFormat.Default );

            videoPlayer = GetComponent<VideoPlayer>();
            if (null != videoPlayer)
                GameObject.DestroyImmediate( videoPlayer );

            videoPlayer = gameObject.AddComponent<VideoPlayer>();
            videoPlayer.targetTexture = targetTexture;
            videoPlayer.playOnAwake = false;
            videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
            videoPlayer.errorReceived += VideoError;
            videoPlayer.prepareCompleted += VideoPrepared;
            videoPlayer.loopPointReached += VideoPlayed;

            AudioSource audioSource = GetComponent<AudioSource>();
            if (null != audioSource)
                GameObject.DestroyImmediate( audioSource );

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = volume;

            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = USystemConfig.ResourceFolder + path.PathNormalize();

            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.controlledAudioTrackCount = 1;

            videoPlayer.EnableAudioTrack( 0, true );
            videoPlayer.SetTargetAudioSource( 0, audioSource );

            videoPlayer.Prepare();

            while (!mPrepared)
                yield return new WaitForEndOfFrame();

            if (!IsStoped)
            {
                rawImage.texture = targetTexture;
                rawImage.color = Color.white;

                videoPlayer.Play();
                try
                {
                    audioSource.Play();

                }
                catch (System.Exception e)
                {
                    ULogger.Warn( string.Format( "Exception thrown whilst getting audio clip {0} ", e.Message ) );
                }
            }

            while (!mPlayed)
                yield return null;

            videoPlayer.errorReceived -= VideoError;
            videoPlayer.prepareCompleted -= VideoPrepared;
            videoPlayer.loopPointReached -= VideoPlayed;

            if (null != targetTexture)
                RenderTexture.ReleaseTemporary( targetTexture );

            if (null != onFinished)
                onFinished();
        }
    }
}
