using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UEngine.UIAnimation
{
    public delegate T DOGetter<out T> ( );

    public delegate void DOSetter<in T> ( T pNewValue );
}
