using System;
using System.Collections.Generic;
using System.Text;

namespace monshare.Models
{
    class Place
    {
        public string Id { get; internal set; }
        public string Name { get; internal set; }
        public Location Location { get; internal set; }

        public static Place DummyPlace = new Place() { Id = "", Name = "", Location = null };

    }
}
