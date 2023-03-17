using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Classes
{
    public class AttributeDetail
    {
        public byte AttributeGroupID { get; set; }
        public Int16 AttributeID { get; set; }
        public String Name { get; set; }
        public byte SortOrder { get; set; }
        public bool EnableFilterIndexing { get; set; }
        public List<int> ProductPageIDs { get; set; }
        public List<int> AvailableProductPageIDs { get; set; }

        public string FormValue
        {
            get
            {
                return HttpUtility.UrlEncode($"{Name?.ToLowerInvariant().Replace(" ", "-")}");
            }
            set
            {

            }
        }

        public AttributeDetail()
        {
            ProductPageIDs = new List<int>();
            AvailableProductPageIDs = new List<int>();
        }
    }
}