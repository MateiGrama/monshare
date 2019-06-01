using System;
using System.Collections.Generic;
using System.Text;

namespace monshare.Utils
{
    class NullObject<T>
    {
        public static T NullInstance { get; }
        public string message { get; internal set; }
    }
}
