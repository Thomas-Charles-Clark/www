using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Trade
{
    public class PassTradeClearanceFetch
    {
        public List<int> productPageIds { get; set; }

        public PassTradeClearanceFetch()
        {
            productPageIds = new List<int>();
        }
    }
}