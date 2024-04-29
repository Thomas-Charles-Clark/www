using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Classes
{
    public class AttributeGroupProductPage
    {
        public byte AttributeGroupID { get; set; }

        public List<int> ProductPageIDs { get; set; }

        public AttributeGroupProductPage(byte attributeGroupId, IEnumerable<int> productPageIds)
        {
            AttributeGroupID = attributeGroupId;
            ProductPageIDs = productPageIds.ToList();
        }
    }
}