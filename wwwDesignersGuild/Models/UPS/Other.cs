using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace wwwDesignersGuild.Models.UPS
{
    public class UPSAPICredential
    {
        public string UserID { get; set; }
        public string Password { get; set; }
    }

    public class PassUPSLocatorSearch
    {
        public string radius { get; set; }
        public string searchTerm { get; set; }
        public string countryCode { get; set; }
        public string culture { get; set; }
    }

    [Serializable, XmlRoot("LocatorResponse")]
    public class UPSAccessPointResponse : LocatorResponse
    {

    }

    public class UPSLocatorRequestWrapper
    {
        public AccessRequest AccessRequest { get; set; }
        public LocatorRequest LocatorRequest { get; set; }
    }
 }