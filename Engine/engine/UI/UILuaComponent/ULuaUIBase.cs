using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

using UEngine.UIExpand;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaUIBase : ULuaBehaviourBase
    {
        Image _image;
        Image image 
        {
            get 
            {
                if (!_image)
                {
                    {
                        _image = GetComponent<Image>();
                    }
                }
                return _image;
            }
        }

        CText _ctext;
        CText ctext 
        {
            get
            {
                if (!_ctext)
                {
                    _ctext = GetComponent<CText>();
                }
                return _ctext;
            }
        }
        
        Text _text;
        Text text 
        {
            get
            {
                if (!_text)
                {
                    _text = GetComponent<Text>();
                }
                return _text;
            }
        }

        UText _utext;
        UText utext 
        {
            get 
            {
                if (!_utext)
                {
                    _utext = GetComponent<UText>();
                }
                return _utext;
            }
        }

        UIEventListener _listener;
        UIEventListener listener 
        {
            get 
            {
                if (!_listener)
                {
                    _listener = UIEventListener.Get(gameObject);
                }
                return _listener;
            }
        }

        EventPass _eventPass;
        EventPass eventPass 
        {
            get 
            {
                if (!_eventPass)
                {
                    _eventPass = GetComponent<EventPass>();
                    if (!_eventPass)
                    {
                        _eventPass = gameObject.AddComponent<EventPass>();
                    }
                }
                return _eventPass;
            }
        }

        private Sprite mTempSprite;
        List<Texture> NetTextureList = new List<Texture>();

        private UIExpandRect ExpandRect;

        void Awake ( ) 
        {
            ExpandRect = GetComponent<UIExpandRect>();
        }

        public void SetBtenable(bool isEnable) 
        {
            var sele = GetComponent<Selectable>();
            if (sele)
            {
                sele.interactable = isEnable;
            }
        }

        public void SetBtenableImage ( bool isEnable, string path, bool IsAsyn ) 
        {
            var sele = GetComponent<Selectable>();
            if (sele)
            {
                sele.interactable = isEnable;
                if (image)
                {
                    if (!isEnable)
                    {
                        if (!mTempSprite)
                        {
                            mTempSprite = image.sprite;
                        }

                        panelmanager.SynGetSprite( path, ( sprite ) =>
                        {
                            image.sprite = sprite;
                        }, IsAsyn );
                    }
                    else
                    {
                        if (mTempSprite && mTempSprite != image.sprite)
                        {
                            image.sprite = mTempSprite;
                        }
                    }
                }
            }
        }

        public bool GetBtenable() 
        {
            var sele = GetComponent<Selectable>();
            if (sele)
            {
                return sele.interactable;
            }
            else
            {
                ULogger.Error("面板: " + panelmanager.name + " 里的 " + gameObject.name + " 上没有Select组件");
                return false;
            }
        }

        public void SetRaycastTarget(bool isRaycast)
        {
            var gc = GetComponent<Graphic>();
            if (gc)
            {
                gc.raycastTarget = isRaycast;
            }
        }

        private string mImagePath = "";
        public virtual void SetImage(string ResPath, bool IsAsyn)
        {
            if (image) 
            {
				mImagePath = ResPath;

                string path = ResPath;
                panelmanager.SynGetSprite(ResPath, (s) => 
                {
					if (this == null || image == null || image.sprite == s || path != mImagePath || s == null)
                        return;

                    if (s.border == Vector4.zero && image.type != Image.Type.Filled)
                        image.type = Image.Type.Simple;
                    else if (s.border != Vector4.zero && image.type != Image.Type.Filled)
                        image.type = Image.Type.Sliced;
                    image.sprite = s;

                    if (ExpandRect != null)
                        ExpandRect.UpdateRect();
                }, IsAsyn);
            }
        }

        public void SetNetImage ( string URLpath ) 
        {
            if (image)
            {
                UIManager.instance.SynGetNetImage( URLpath, (texture) => 
                {
                    if (texture != null)
                    {
                        Sprite sprite = Sprite.Create( texture, new Rect( Vector2.zero, new Vector2( texture.width, texture.height ) ), new Vector2( 0.5f, 0.5f ) );
                        image.sprite = sprite;

                        NetTextureList.Add( texture );
                    }
                } ); 
            }
        }

        public virtual void SetText(string value) 
        {
            if (text)
                text.text = value;

            if (ctext) 
            {
                ctext.Init();
            }
        }

        public void PlayTextAni ( float TimeInterval, Action AniCallBack ) 
        {
            if (ctext)
            {
                ctext.PlayTextAni( TimeInterval, AniCallBack );
            }
        }

        public void StopTextAni ( bool IsComplete = true ) 
        {
            if (ctext)
            {
                ctext.StopTextAni( IsComplete );
            }
        }

        public void SetTextSize(int size) 
        {
            if (text)
            {
                text.fontSize = size;
            }
            if (ctext)
            {
                ctext.FontSize = size;
            }
        }

        public void SetFillAmount(float percent) 
        {
            if (image != null && image.type == Image.Type.Filled)
            {
                image.fillAmount = percent;
                ResetStatus();
            }
            else
            {
                ULogger.Error(transform.name + ": imge组件为空或者类型不为filled");
            }
        }

        private float targetValue;
        private float currentVelocity;
        private float currentVelocityTrans;
        private float maxSpeed;

        private float totalTime;
        private bool IsPlaySmooth = false;

        public void SetFillSmoothAni ( float value, float time, float MaxSpeed ) 
        {
            if (image == null || image.type != Image.Type.Filled)
            {
                ULogger.Error( transform.name + ": imge组件为空或者类型不为filled" );
                return;
            }

            currentVelocity = 0f;
            IsPlaySmooth = true;
            IsPlayTransSmooth = false;
            targetValue = value;
            totalTime = time;
            maxSpeed = MaxSpeed;
        }

        private void ResetStatus ( ) 
        {
            currentVelocity = 0f;
            currentVelocityTrans = 0f;
            IsPlaySmooth = false;
            IsPlayTransSmooth = false;
        }

        void Update ( ) 
        {
            if (IsPlaySmooth)
            {
                if (maxSpeed < 0)
                {
                    image.fillAmount = Mathf.SmoothDamp( image.fillAmount, targetValue, ref currentVelocity, totalTime );
                }
                else
                {
                    image.fillAmount = Mathf.SmoothDamp( image.fillAmount, targetValue, ref currentVelocity, totalTime, maxSpeed );
                }
                if (Mathf.Abs( targetValue - image.fillAmount ) < 0.0001f)
                {
                    IsPlaySmooth = false;
                    image.fillAmount = targetValue;
                }
            }

            if (IsPlayTransSmooth)
            {
                if (maxSpeed < 0)
                {
                    TransFill.fillAmount = Mathf.SmoothDamp( TransFill.fillAmount, targetValue, ref currentVelocityTrans, totalTime );
                }
                else
                {
                    TransFill.fillAmount = Mathf.SmoothDamp( TransFill.fillAmount, targetValue, ref currentVelocityTrans, totalTime, maxSpeed );
                }
                if (Mathf.Abs( targetValue - TransFill.fillAmount ) < 0.0001f)
                {
                    IsPlayTransSmooth = false;
                    TransFill.fillAmount = targetValue;
                }
            }
        }

        private bool IsPlayTransSmooth = false;

        private RectTransform mFillAera
        {
            get
            {
                if (transform != null)
                {
                    return transform.parent as RectTransform;
                }
                else
                {
                    return null;
                }
            }
        }

        private Image mAlphaFill;
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

        private void CreateFill ( ) 
        {
            if (mFillAera != null)
            {
                GameObject go = new GameObject( "AlphaSlider" );
                go.transform.SetParent( mFillAera, false );
                RectTransform rect = go.AddComponent<RectTransform>();
                rect.SetSiblingIndex( 0 );
                mAlphaFill = go.AddComponent<Image>();
                rect.sizeDelta = mFillAera.rect.size;
                rect.anchorMin = recttransform.anchorMin;
                rect.anchorMax = recttransform.anchorMax;
                rect.pivot = recttransform.pivot;
            }
        }

        public void SetTransColor ( uint color )
        {
            if (TransFill)
                TransFill.color = UIManager.RGBA( color );
        }

        private string TempPath;
        public void SetTransSprite ( string ResPath, bool IsAsyn )
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

        public void SetFillSmoothWithTransition ( float value, float time, float MaxSpeed ) 
        {
            if (image == null || image.type != Image.Type.Filled)
            {
                ULogger.Error( transform.name + ": imge组件为空或者类型不为filled" );
                return;
            }

            if (TransFill != null)
            {
                if (value > image.fillAmount)
                {
                    TransFill.fillAmount = value;

                    SetFillSmoothAni( value, time, MaxSpeed );
                }
                else
                {
                    if (TransFill.fillAmount < image.fillAmount)
                    {
                        TransFill.fillAmount = image.fillAmount;
                    }

                    image.fillAmount = value;

                    SetTranFillSmoothAni( value, time, MaxSpeed );
                }
            }
        }

        private void SetTranFillSmoothAni ( float value, float time, float MaxSpeed )
        {
            IsPlayTransSmooth = true;
            IsPlaySmooth = false;

            currentVelocityTrans = 0f;
            targetValue = value;
            totalTime = time;
            maxSpeed = MaxSpeed;
        }

        public virtual string GetText() 
        {
            if (text)
                return text.text;
            else
            {
                ULogger.Error("目标控件上不包含Text组件,获取text为空");
                return string.Empty;
            }
        }

        public void SetFontStyle ( int style ) 
        {
            if (text)
            {
                text.fontStyle = ( FontStyle )style;
            }

            if (ctext)
            {
                ctext.FontStyle = ( FontStyle )style;
            }
        }

        public virtual void SetColor(uint color) 
        {
            if (image)
                image.color = UIManager.RGBA(color);
            
            if (text) 
            {
                text.color = UIManager.RGBA(color);
            }
        }

        public void SetGradientColor(uint topcolor, uint bottomcolor) 
        {
            if (ctext)
            {
                ctext.TopColor = UIManager.RGBA( topcolor );
                ctext.BottomColor = UIManager.RGBA( bottomcolor );
            }

            if (utext)
            {
                utext.topColor = UIManager.RGBA( topcolor );
                utext.BottomColor = UIManager.RGBA( bottomcolor );
            }
        }

        public void SetOutLineSize ( int size ) 
        {
            if (ctext)
            {
                ctext.OutLineSize = size;
                ctext.Init();
            }
        }

        public void SetOutLineColor(uint OutLineColor)
        {
            if (ctext) 
            {
                ctext.OutLineColor = UIManager.RGBA( OutLineColor );
                ctext.Init();
            }
        }

        public virtual void AddClick(Action luafunc)
        {
            listener.onClick = luafunc;
        }

        public virtual void RemoveClick(int index)
        {
            listener.onClick = null;
        }

        public virtual void RemoveAllClick()
        {
        }

        public virtual void AddUpClick(Action luafunc)
        {
            listener.onUp = luafunc;
        }

        public virtual void RemoveUpClick(int index)
        {
        }

        public virtual void RemoveAllUpClick()
        {
            listener.onUp = null;
        }

        public virtual void AddDownClick(Action luafunc)
        {
            listener.onDown = luafunc;
        }

        public virtual void RemoveDownClick(int index)
        {
        }

        public virtual void RemoveAllDownClick()
        {
            listener.onDown = null;
        }

        public void AddDragEvent(Action<Vector2> luafunc)
        {
            listener.onDrag = luafunc;
        }

        public virtual void RemoveDragEvent(int index)
        {
        }

        public virtual void RemoveAllDragEvent()
        {
            listener.onDrag = null;
        }

        public virtual void AddDragOutEvent(Action luafunc)
        {
            listener.onDragOut = luafunc;
        }

        public virtual void RemoveDragOutEvent(int index)
        {
        }

        public virtual void RemoveAllDDragOutEvent()
        {
            listener.onDragOut = null;
        }

        public virtual void AddDragEnterEvent(Action luafunc, ULuaUIBase uiBase = null )
        {
            listener.onDragIn = luafunc;
            listener.DragUIBase = uiBase;
        }

        public virtual void RemoveDragEnterEvent(int index)
        {
        }

        public virtual void RemoveAllDDragEnterEvent ( )
        {
            listener.onDragIn = null;
            listener.DragUIBase = null;
        }

        public void SetParentDrag ( bool IsDrag ) 
        {
            listener.ParentScrollDrag = IsDrag;
        }

        public void SetGray(bool IsGray)
        {
            if (image)
            {
                if (null == UIManager.UiGrayMaterial)
                    UIManager.UiGrayMaterial = UResourceManager.SynLoadResource< Material >("material/ui/uigray.mat");

                if (IsGray)
                    image.material = UIManager.UiGrayMaterial.Res as Material;

                if (image.material && !IsGray && image.material == UIManager.UiGrayMaterial.Res)
                    image.material = Canvas.GetDefaultCanvasMaterial();
            }
        }

        List<Image> mChildGrayList = new List<Image>();

        public void SetGrayChild(bool IsGray, bool InCludeSelf = true) 
        {
            if (null == UIManager.UiGrayMaterial)
                UIManager.UiGrayMaterial = UResourceManager.SynLoadResource< Material >("material/ui/uigray.mat");

            if (IsGray)
            {
                UCoreUtil.TraverseChild(gameObject, (GameObject go) => 
                {
                    if (go == gameObject)
                    {
                        if (InCludeSelf && image)
                        {
                            image.material = UIManager.UiGrayMaterial.Res as Material;
                            mChildGrayList.Add(image);
                        }
                    } else
                    {
                        Image _image = go.GetComponent<Image>();
                        if (_image)
                        {
                            _image.material = UIManager.UiGrayMaterial.Res as Material;
                            mChildGrayList.Add(_image);
                        }
                    }

                    return go.transform.childCount > 0 ? true : false;
                });
            } else
            {
                for (int i = 0; i < mChildGrayList.Count; i++)
                    mChildGrayList[i].material = null;

                mChildGrayList.Clear();
            }
        }

        public void SetGuideTarget ( ULuaUIBase uiBase ) 
        {
            if (listener)
            {
                listener.uiBase = uiBase;
            }
        }

        public override void OnClose ( )
        {
            base.OnClose();
            for (int i = 0 ; i < NetTextureList.Count ; i++)
            {
                if (NetTextureList[i] != null)
                {
                    Destroy( NetTextureList[i] );
                    NetTextureList[i] = null;
                }
            }

            if (flowMaterial != null)
            {
                Destroy( flowMaterial );
                flowMaterial = null;
            }

            if (listener)
            {
                listener.ClearEvent();
            }
        }

        public void SeteventAlphaThreshold ( bool isThreshold ) 
        {
            if (image)
            {
                if (isThreshold)
                {
                    image.alphaHitTestMinimumThreshold = 0.1f;
                }
                else
                {
                    image.alphaHitTestMinimumThreshold = 0f;
                }
            }
        }

        private Material flowMaterial;
        public void SetFlow ( bool IsFlow, uint FlowColor, float FlowAngle, float FlowWidth, float LoopTime, float TimeInterval, bool IsAsyn = false ) 
        {
            if (IsFlow && image)
            {
                if (flowMaterial == null)
                {
                    panelmanager.SynGetRes( "material/ui/flowlight.mat", ( res ) =>
                    {
                        flowMaterial = Instantiate( res.Res ) as Material;
                        if (flowMaterial.HasProperty( "Flash Color" ))
                            flowMaterial.SetColor( "Flash Color", UIManager.RGBA( FlowColor ) );
                        if (flowMaterial.HasProperty( "Flash Angle" ))
                            flowMaterial.SetFloat( "Flash Angle", FlowAngle );
                        if (flowMaterial.HasProperty( "Flash Width" ))
                            flowMaterial.SetFloat( "Flash Width", FlowWidth );
                        if (flowMaterial.HasProperty( "Loop Time" ))
                            flowMaterial.SetFloat( "Loop Time", LoopTime );
                        if (flowMaterial.HasProperty( "Time Interval" ))
                            flowMaterial.SetFloat( "TimeInterval", FlowAngle );

                        image.material = flowMaterial;
                    }, IsAsyn );
                }
                else
                {
                    if (flowMaterial.HasProperty( "Flash Color" ))
                        flowMaterial.SetColor( "Flash Color", UIManager.RGBA( FlowColor ) );
                    if (flowMaterial.HasProperty( "Flash Angle" ))
                        flowMaterial.SetFloat( "Flash Angle", FlowAngle );
                    if (flowMaterial.HasProperty( "Flash Width" ))
                        flowMaterial.SetFloat( "Flash Width", FlowWidth );
                    if (flowMaterial.HasProperty( "Loop Time" ))
                        flowMaterial.SetFloat( "Loop Time", LoopTime );
                    if (flowMaterial.HasProperty( "Time Interval" ))
                        flowMaterial.SetFloat( "TimeInterval", FlowAngle );

                    image.material = flowMaterial;
                }
            }
            else
            {
                if (image)
                {
                    image.material = Canvas.GetDefaultCanvasMaterial();
                }
            }
        }

        public void SetEventPassBefore ( bool IsBefore ) 
        {
            if (listener)
            {
                listener.EventPassBeforeClick = IsBefore;
            }
        }
    }
}
