using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Classes
{
    public class Country
    {
        public short CountryID { get; set; }
        public string Name { get; set; }
        public bool AddressLookup { get; set; }
        public string Code { get; set; }
        public string Alpha3Code { get; set; }
        public bool MandatePostcode { get; set; }
        public short ProxyCountryID { get; set; }
    }

    public class PassCountryID
    {
        public short countryId { get; set; }
    }
}