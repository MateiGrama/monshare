using monshare.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace monshare.Models
{
    public class Message

    {
        public int SenderId { get; internal set; }
        public string Text { get; internal set; }
        public DateTime DateTime { get; internal set; }
        public bool IsOwnMessage => SenderId == LocalStorage.GetUserId();
    }
}
