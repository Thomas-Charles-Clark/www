using iText.IO.Font;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.ProductPage;

namespace wwwDesignersGuild.Models.Classes
{
    public class AttributeGroup
    {
        public byte AttributeGroupID { get; set; }
        public byte? AttributeGroupGroupID { get; set; }
        public string Name { get; set; }
        public Boolean UseIcons { get; set; }
        public byte SortOrder { get; set; }
        public byte LexicographicalSortOrder { get; set; }
        public bool MultipleSelect { get; set; }
        public string LogicalOperator { get; set; }
        public List<AttributeDetail> Attributes { get; set; }
        public List<string> Selection { get; set; }

        public AttributeGroup()
        {
            Selection = new List<string>();
        }

        public bool InUse
        {
            get
            {
                return Selection?.Any() == true;
            }
            set
            {

            }
        }

        public string FormName
        {
            get
            {
                return HttpUtility.UrlEncode(Name?.ToLowerInvariant());
            }
            set
            {

            }
        }

        public List<AttributeGroupProductType> ProductTypes { get; set; }

        public AttributeGroupGroup AttributeGroupGroup { get; set; }

        public bool InvolveInFilterIndexing(string productType)
        {
            return ProductTypes.Any(pt => pt.FormValue == productType && pt.LexicographicalSortOrder != null);
        }
    }
}