using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Trade;

namespace wwwDesignersGuild.ViewModels.Trade
{
    public class TradeUserManagementViewModel
    {
        public BaseViewModel BaseViewModel { get; set; }
        public MemberDetail MemberDetail { get; set; }
        public TradeAccountUsersModel TradeAccountModel { get; set; }
        public bool Permission { get; set; }
    }
}