using System;
using System.Collections.Generic;
using System.Json;
using System.Text;
using monshare.Utils;

namespace monshare.Models
{
    class User : NullObject<User>
    {
        public string  FirstName{ get; internal set; }
        public string  LastName{ get; internal set; }
        public int  UserId{ get; internal set; }
        public string  Email{ get; internal set; }
    }
}
