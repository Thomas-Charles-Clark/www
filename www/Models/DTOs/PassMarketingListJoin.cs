using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.DTOs
{
    public class PassMarketingListJoin
    {
        public string Email { get; set; }
        public short CountryID { get; set; }
        public byte LanguageID { get; set; }
        public byte PreferenceEmailMarketingFrequencyID { get; set; }
        public string URL { get; set; }
    }
}