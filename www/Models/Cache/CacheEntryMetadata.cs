using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace wwwDesignersGuild.Models.Cache
{
    public class CacheEntryMetadata
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public DateTimeOffset AbsoluteExpiration { get; set; }
        public TimeSpan SlidingExpiration { get; set; }
        public DateTimeOffset LastAccessed { get; set; }
    }
}