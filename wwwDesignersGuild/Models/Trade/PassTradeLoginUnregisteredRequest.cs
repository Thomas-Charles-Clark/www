using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Trade
{
    public class PassTradeLoginUnregisteredRequest
    {
        public string TradeAccountCode { get; set; }
        public string Email { get; set; }
    }
}