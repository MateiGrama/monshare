using System;

namespace monshare.Utils
{
    internal class CachedData<T>
    {
        public T Data { get; set; }
        public DateTime LastCached { get; set; }
    }
}