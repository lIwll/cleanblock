using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Newtonsoft.Json;
using System;

[Serializable]
public class UIListener : MonoBehaviour
{
    public float MoveUpoffsetX;
    public float MoveUpoffsetY;
    public float MoveUpSpeed;
    public float distance;
    public float StickyDis;

    public Transform uieffectImage;
    public Image maskImage;
    public Button but;


    Transform UICamera;
    RectTransform recttransform;
    public bool IsSelect = false;
    public Vector3 startPos;
    Vector3 target;
    //[NonSerialized]
    //Image maskImage;

    void Awake ( )
    {
        
    }

    void Start ( )
    {
        
    }

    void OnEnable ( ) 
    {
        if (UICamera == null)
        {
            UICamera = GameObject.Find( "UICamera" ).transform;
        }
        recttransform = GetComponent<RectTransform>();

        if (but)
            but.onClick.AddListener( UIAdjust );
    }

    public void MyStart ( ) 
    {
        startPos = transform.localPosition;
        Vector3 dir = UICamera.transform.position - transform.position;
        target = dir.normalized * distance + transform.localPosition;
        target = new Vector3( target.x + MoveUpoffsetX, target.y + MoveUpoffsetY, target.z );
    }

    public void UIAdjust ( )
    {
        if (!IsSelect)
        {
            transform.DOLocalMove( target, MoveUpSpeed );
            transform.SetAsLastSibling();
            if (uieffectImage != null)
            {
                uieffectImage.gameObject.SetActive( true );
            }
            maskImage.DOFade( 0, 1 );
            IsSelect = true;
            ListManager.Instance.AdjustChild( this );
        }
    }

    public void Init ( )
    {
        Vector3 dir = UICamera.transform.position - transform.position;
        Vector3 _target;
        _target = dir.normalized * distance + transform.localPosition;
        _target = new Vector3( _target.x + MoveUpoffsetX, _target.y + MoveUpoffsetY, _target.z );
        transform.DOLocalMove( _target, MoveUpSpeed );
        if (uieffectImage != null)
        {
            uieffectImage.gameObject.SetActive( true );
        }
        maskImage.DOFade( 0, 1 );
        IsSelect = true;
    }

    // Update is called once per frame
    void Update ( )
    {
        if (but)
            but.onClick.AddListener( UIAdjust );
    }

    public void StickySele ( Transform target, Tweener tw = null )
    {
        Vector3 _dir = new Vector3( 0, target.localPosition.y - startPos.y, 0 );
        Vector3 targetPos = _dir * StickyDis + startPos;
        if (tw == null)
        {
            transform.DOLocalMove( targetPos, .3f );
        }
        else
        {
            tw.ChangeEndValue( targetPos, .3f, false );
        }
    }

    public Tweener GuiWei ( )
    {
        Tweener tw = transform.DOLocalMove( startPos, MoveUpSpeed );
        if (uieffectImage != null)
        {
            uieffectImage.gameObject.SetActive( false );
        }
        maskImage.DOFade( 1, 1 );
        return tw;
    }
}