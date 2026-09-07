using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace wwwDesignersGuild.Models.Cache
{
    public class CacheStateSummary
    {
        public string Key { get; set; }

        public string CacheName { get; set; }

        public int ObjectCount { get; set; }

        public string[] KeyBreakdown { get; set; }

        public decimal TotalMebibytes { get; set; }
    }
}