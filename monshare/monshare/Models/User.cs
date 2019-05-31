using System;
using System.Collections.Generic;
using System.Json;
using System.Text;
using monshare.Utils;

namespace monshare.Models
{
    class User : NullObject<User>
    {
        public string  firstName{ get; internal set; }
        public string  lastName{ get; internal set; }
        public string  userId{ get; internal set; }
        public string  email{ get; internal set; }
    }
}
