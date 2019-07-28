using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using UEngine.UIExpand;

namespace UEngine.UI
{
    [ExecuteInEditMode]
    public class ULayoutGroup : VerticalLayoutGroup
    {
        protected override void OnRectTransformDimensionsChange ( )
        {
            base.OnRectTransformDimensionsChange();
            SetDirty();
        }
    }

    [ExecuteInEditMode]
    public class UGridLayouGroup : GridLayoutGroup
    {

        protected override void OnRectTransformDimensionsChange ( )
        {
            base.OnRectTransformDimensionsChange();
            SetDirty();
        }
    }

    [ExecuteInEditMode]
    public class UHLayoutGroup : HorizontalLayoutGroup
    {
        protected override void OnRectTransformDimensionsChange ( )
        {
            base.OnRectTransformDimensionsChange();
            SetDirty();
        }
    }
}
