using monshare.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace monshare.Models
{
    class Chat : NullObject<Chat>
    {
        public Group group { get; internal set; }
        public List<Message> messages { get ; internal set; }
    }
}
