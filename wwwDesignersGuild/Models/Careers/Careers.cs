using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Careers
{
    public class cmsCareersPageDefinition
    {
        public List<CareerSelect> CareerListings { get; set; }
        public CareersBlurb Blurb { get; set; }
    }

    public class CareerSelect
    {
        public short CareerListingID { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public DateTime? ApplyBeforeDT { get; set; }
        public string Contents { get; set; }
        public string ContactPostal { get; set; }
        public string ContactEmail { get; set; }
        public bool Salaried { get; set; }
        public short SortOrder { get; set; }
    }

    public class CareersBlurb
    {
        public string Blurb { get; set; }
    }
}