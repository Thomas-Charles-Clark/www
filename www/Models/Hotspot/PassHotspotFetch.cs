using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Hotspot
{
    public class PassHotspotFetch
    {
        public IEnumerable<long> imageIds { get; set; }
        public long visitorId { get; set; }
        public int? memberId { get; set; }
        public int? tradeAccountId { get; set; }
        public bool displayTradePrice { get; set; }
        public NewMasterDefinition objMaster { get; set; }
    }
}