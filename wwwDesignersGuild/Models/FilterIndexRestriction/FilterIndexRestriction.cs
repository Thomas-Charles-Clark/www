using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models
{
    public class FilterIndexRestriction
    {
        public byte ProductTypeID { get; set; }
        public string AttributeGroupSignature { get; set; }
        public string AttributeGroupDescription { get; set; }
        public bool Deleted { get; set; }
    }
}