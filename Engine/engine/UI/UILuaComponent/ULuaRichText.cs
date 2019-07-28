using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using UnityEngine.UI;

using UEngine.UIExpand;
using UEngine.UI.ricktext;

namespace UEngine.UI.UILuaBehaviour
{
    public class ULuaRichText : ULuaUIBase
    {
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


        Action<int> onHref;
        void Awake()
        {
            if (ctext)
            {
                ctext.OnHrefClick.AddListener(OnClickLuaFunc);
            }
        }

        void OnClickLuaFunc ( int id ) 
        {
            if (onHref != null)
            {
                onHref( id );
            }
        }

        public void AddHrefClick(Action<int> luafunc)
        {
            onHref = luafunc;
        }

        public void RemoveHrefClick(int index)
        {
        }

        public void RemoveAllHrefClick()
        {
        }

        public override void OnClose ( )
        {
            base.OnClose();
            onHref = null;
        }
    }
}
