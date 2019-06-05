using System;
using System.Collections.Generic;
using System.Text;

namespace monshare.Models
{
    class Location
    {
        public string Address { get; internal set; }
        public double Lat { get; internal set; }
        public double Long { get; internal set; }
        public string Coords => Lat + "," + Long;
    }
}
