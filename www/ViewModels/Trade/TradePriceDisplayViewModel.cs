using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.ViewModels.Trade
{
    public class TradePriceDisplayViewModel
    {
        public BaseViewModel BaseViewModel { get; set; }
        public MemberDetail MemberDetail { get; set; }
        public bool DisplayTradePrice { get; set; }
    }
}