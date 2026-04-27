using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.ViewModels;

namespace wwwDesignersGuild.Models.LandingPage
{
	public class ProductPageGrid_PagingDTO
	{
		public int PageID { get; set; }

		public long PageSectionID { get; set; }

		public byte LanguageID { get; set; }

		public short CountryID { get; set; }

		public DateTime UseDate { get; set; }

		public bool IsMember { get; set; }

		public bool IsTrade { get; set; }

		public int? MemberID { get; set; }

		public bool DisplayTradePrice { get; set; }
		
		public int? TradeAccountID { get; set; }

		public bool Debug { get; set; }

		public string DebugProductCode { get; set; }

		public bool CountryMatch { get; set; }

		// needs to be zero-based
		public int Page { get; set; }

		public int PageSize { get; set; }

		// we'll also need all of the potential query string parameters; filtering, sorting, etc.
		// hmmm.
		// ... there are others, I'm sure. What are they? Is it as simple as AttributeIDs and Sort? ProductTypeID!
		public IEnumerable<ProductPageGrid_AttributeDTO> Attributes { get; set; }

		public string Sort { get; set; }

		public byte? ProductTypeID { get; set; }

		public BaseViewModel BaseViewModel { get; set; }
	}
}