using Braintree;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.DTOs
{
	// Whererver this is used in API endpoints, useDate must be filled from frameworkj.,
	public class ProductPage_Price_SelectDTO
	{
		public IEnumerable<int> productPageIds { get; set; }
		public VisitorSelect objVisitor { get; set; }
		public NewMasterDefinition objMaster { get; set; }
		public bool isLifestyle { get; set; } = false;
		public bool countryCheckMatched { get; set; }
		public bool displayTradePrice { get; set; }
		public short countryId { get; set; }
		public byte languageId { get; set; }
		public DateTime useDate { get; set; }
		public int? pageId { get; set; }
	}
}