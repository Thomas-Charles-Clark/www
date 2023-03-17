using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Trade
{
    public class TradeAccountUsersModel
    {
        public TradeAccountDetail AccountDetails { get; set; }
        public List<TradeAccountAdmin> Administrators { get; set; }
        public List<TradeAccountUser> Users { get; set; }
        public bool UserIsAdmin { get; set; }
    }

    public class TradeAccountDetail
    {
        public int TradeAccountID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string Postcode { get; set; }
        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public string CustomerServicesPhoneNumber { get; set; }
        public string CustomerServicesEmail { get; set; }
        public string CreditControlPhoneNumber { get; set; }
        public string SalesRepresentativeName { get; set; }
        public string SalesRepresentativeEmail { get; set; }
        public short TradeAccountTypeID { get; set; }
        public bool Distributor { get; set; }
    }

    public class TradeAccountAdmin
    {
        public int MemberID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool TradePermissionAccountAdmin { get; set; }
        public bool TradePermissionPlaceOrder { get; set; }
        public bool TradePermissionRequestSample { get; set; }
        public bool TradePermissionViewAccounts { get; set; }
        public bool TradePermissionDownloadPricelistsBrochures { get; set; }
        public bool TradePermissionDownloadProductFeeds { get; set; }
        public bool ReadOnly { get; set; }
        public bool TradeAuthenticationComplete { get; set; }
        public int DaysRemainingToRegister { get; set; }
        public bool DisplayTradePrice { get; set; }
    }


    // I know, this looks stupid - it's so that I can concat the two IQueryables easier, 
    // and so that the auto mapping using IMultipleResults still works.
    public class TradeAccountUser : TradeAccountAdmin
    {
        public new int MemberID { get; set; }
        public new string FullName { get; set; }
        public new string Email { get; set; }
        public new bool TradePermissionAccountAdmin { get; set; }
        public new bool TradePermissionPlaceOrder { get; set; }
        public new bool TradePermissionRequestSample { get; set; }
        public new bool TradePermissionViewAccounts { get; set; }
        public new bool TradePermissionDownloadPricelistsBrochures { get; set; }
        public new bool TradePermissionDownloadProductFeeds { get; set; }
        public new bool ReadOnly { get; set; }
        public new bool TradeAuthenticationComplete { get; set; }
        public new int DaysRemainingToRegister { get; set; }
        public new bool DisplayTradePrice { get; set; }
    }
}