using System;
using System.Collections.Generic;
using System.Text;

namespace monshare.Utils
{
    class NullObject<T> where T : new()
    {
        public static T NullInstance = new T();
        public string message { get; internal set; }
    }
}
