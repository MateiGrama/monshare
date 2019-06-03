using System;
using System.Collections.Generic;
using System.Json;
using System.Text;

namespace monshare.Models
{
    public class Group
    {
        public int groupId { get; internal set; }
        public string title { get; internal set; }
        public string description { get; internal set; }
        public DateTime creationDateTime { get; internal set; }
        public DateTime endDateTime { get; internal set; }
        public int minMembers { get; internal set; }
        public int membersNumber { get; internal set; }
        public int ownerId { get; internal set; }
    }
}
