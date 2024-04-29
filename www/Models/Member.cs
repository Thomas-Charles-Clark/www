using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Classes
{
    public class MemberDetail
    {
        public int MemberID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public short? TradePriceListID { get; set; }
        public string MertexCustomerAccountNumber { get; set; }
        public string TradeDataSourceJson { get; set; }
        public int? TradeAccountID { get; set; }
        public bool DisplayTradePrice { get; set; }
        public short CountryID { get; set; }
        public byte LanguageID { get; set; }
        public bool TradeAuthenticationComplete { get; set; }
        public short MarketingCountryID { get; set; }
        public byte MarketingLanguageID { get; set; }
        public bool Unsubscribed { get; set; }
        public Guid GUID { get; set; }
    }
}