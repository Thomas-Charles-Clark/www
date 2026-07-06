using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Classes
{
    public class PassProductPageID
    {
        public masterDefinition objMaster { get; set; }
        public int productPageId { get; set; }
    }

    public class PassProductPageIDs
    {
        public NewMasterDefinition objMaster { get; set; }
        public List<int> productPageIds { get; set; }
    }


    public class logPageView
    {
        public char Site { get; set; }
        public string URL { get; set; }
        public short? CountryID { get; set; }
        public byte? LanguageID { get; set; }
        public DateTime? Date { get; set; }
        public long? VisitorID { get; set; }
        public int? MemberID { get; set; }
        public int? TradeAccountID { get; set; }
        public int? PageID { get; set; }
        public short? CollectionID { get; set; }
        public int? ProductPageID { get; set; }
        public int? ArticleID { get; set; }
        public string UserAgent { get; set; }
        public string CFIPCountry { get; set; }
        public string CloudFlareDevice { get; set; }
        public string SessionID { get; set; }
        public string GoogleSource { get; set; }
        public string GoogleMedium { get; set; }
        public string GoogleCampaign { get; set; }
        public string GoogleTerm { get; set; }
        public string GoogleContent { get; set; }
    }

    public class intResult
    {
        public int Result { get; set; }
    }

    public class countryCheckResult
    {
        public bool canSeeRRPs { get; set; }
        public countryItem countryItem { get; set; }
    }
}