using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Classes
{
    public class AttributeSorter
    {
        public short AttributeID { get; set; }
        public byte AttributeGroupID { get; set; }
        public string Attribute { get; set; }
        public string AttributeName { get; set; }
        public byte? LexicographicalSortOrder { get; set; }
        public string LogicalOperator { get; set; }

        public AttributeSorter(short attributeId, byte attributeGroupId, string attribute, string attributeName, byte? lexSortOrder, string logicalOperator)
        {
            AttributeID = attributeId;
            AttributeGroupID = attributeGroupId;
            Attribute = attribute;
            AttributeName = attributeName;
            LexicographicalSortOrder = lexSortOrder;
            LogicalOperator = logicalOperator;
        }
    }
}