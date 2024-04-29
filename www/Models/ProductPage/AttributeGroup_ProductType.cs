using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Classes
{
    public class AttributeGroup_ProductType
    {
        public byte AttributeGroupID { get; set; }
        public byte ProductTypeID { get; set; }
        public byte? LexicographicalSortOrder { get; set; }
    }
}