using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Classes
{
    public class MemberAddressSelect
    {
        public List<MemberAddress> Addresses { get; set; }
        public List<MemberTradeAddress> TradeAddresses { get; set; }
        public bool TradeAddressErrored { get; set; }

        public MemberAddressSelect() 
        {
            Addresses = new List<MemberAddress>();
            TradeAddresses = new List<MemberTradeAddress>();
            TradeAddressErrored = false;
        }
    }

    public class MemberAddress
    {
        public long MemberAddressID { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string PostCode { get; set; }
        public string PhoneNumber { get; set; }
        public short CountryID { get; set; }
        public string CountryCode { get; set; }
        public bool IsDefault { get; set; }
    }
}