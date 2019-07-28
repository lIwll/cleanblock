using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Reflection;
using UnityEngine.UI.Extensions;

namespace UEngine.UIExpand
{
    public class TDropdown : Dropdown
    {
        public Image arrow;
        
        [HideInInspector]
        public Sprite tempSprite;
        
        [HideInInspector]
        public Sprite morenSprite;
        
        [HideInInspector]
        public Color32 color;
        
        private GameObject my_DropDown;

        protected override void Start()
        {
            base.Start(); 
            //if (morenSprite != null)
            //{
            //    arrow.sprite = morenSprite;
            //}
        }
        
        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            //arrow.sprite = tempSprite;
            arrow.transform.localEulerAngles = new Vector3(0, 0, 180);
            FieldInfo PrfieldInfo = typeof(Dropdown).GetField("m_Dropdown", BindingFlags.NonPublic | BindingFlags.Instance);
            GameObject tempdrop = PrfieldInfo.GetValue(this) as GameObject;
            my_DropDown = tempdrop;
            if (my_DropDown != null && value >= 0 && value < options.Count)
            {
                TDropdownItem[] TDropdownItems = my_DropDown.transform.GetComponentsInChildren<TDropdownItem>();
                TDropdownItems[value].m_Text.color = color;
                UText utext = TDropdownItems[value].m_Text.GetComponent<UText>();
                if (utext)
                {
                    utext.outlineColor = color;
                }
            }
        }
        
        protected override void DestroyBlocker(GameObject blocker)
        {
            base.DestroyBlocker(blocker);
            //arrow.sprite = morenSprite;
            arrow.transform.localEulerAngles = new Vector3(0, 0, 0);
        }

        protected override GameObject CreateDropdownList(GameObject template)
        {
            return base.CreateDropdownList(template);
        }
    }
}
