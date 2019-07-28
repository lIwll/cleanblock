using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;


public class ListManager : MonoBehaviour
{
    UIListener[] childs;
    private static ListManager instance;
    public static ListManager Instance
    {
        get
        {
            return instance;
        }
    }

    [HideInInspector]
    [NonSerialized]
    private UIListener seletion_Target;

    void Awake ( )
    {
        
    }

    // Use this for initialization
    void Start ( )
    {
        instance = this;
        childs = new UIListener[transform.childCount];
        for (int i = 0 ; i < childs.Length ; i++)
        {
            childs[i] = transform.GetChild( i ).GetComponent<UIListener>();
        }
        seletion_Target = childs[childs.Length - 1];

        for (int i = 0 ; i < childs.Length ; i++)
        {
            childs[i].MyStart();
        }

        seletion_Target.Init();
        for (int i = 0 ; i < childs.Length ; i++)
        {
            if (childs[i] != seletion_Target)
            {
                childs[i].StickySele( seletion_Target.transform );
            }
        }
    }

    void OnEnable ( ) 
    {
        
    }

    // Update is called once per frame
    void Update ( )
    {

    }

    public void AdjustChild ( UIListener target )
    {
        seletion_Target.IsSelect = false;
        Tweener tw = seletion_Target.GuiWei();
        for (int i = 0 ; i < childs.Length ; i++)
        {
            if (childs[i] != seletion_Target && childs[i] != target)
            {
                childs[i].StickySele( target.transform );
            }
            else if (childs[i] == seletion_Target)
            {
                childs[i].StickySele( target.transform, tw );
            }
        }
        seletion_Target = target;
    }
}

