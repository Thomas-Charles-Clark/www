using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.LandingPage
{
	public class ProductPageGrid_AttributeDTO
	{
		public int AttributeID { get; set; }

		public short AttributeGroupID { get; set; }

		public string AttributeOperator { get; set; } // AND / OR
	}
}