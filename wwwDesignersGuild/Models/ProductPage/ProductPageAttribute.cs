using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Classes
{
    public class ProductPageAttribute
    {
        public int ProductPageID { get; set; }
        public short AttributeID { get; set; }
        public byte AttributeGroupID { get; set; }
        public string Attribute { get; set; }
        public string AttributeGroup { get; set; }
    }
}