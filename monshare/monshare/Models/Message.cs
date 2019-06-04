using monshare.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace monshare.Models
{
    public class Message

    {
        public int senderId { get; internal set; }
        public string text { get; internal set; }
        public DateTime dateTime { get; internal set; }
        public bool isOwnMessage => senderId == LocalStorage.GetUserId();
    }
}
