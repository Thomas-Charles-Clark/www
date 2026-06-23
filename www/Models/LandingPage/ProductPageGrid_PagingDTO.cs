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
		public long PageSectionID { get; set; }

		public byte LanguageID { get; set; }

		public short CountryID { get; set; }

		public int Page { get; set; }

		public int PageSize { get; set; }

		// we'll also need all of the potential query string parameters; filtering, sorting, etc.
		// hmmm.
		// ... there are others, I'm sure. What are they? Is it as simple as AttributeIDs and Sort? ProductTypeID!
		public IEnumerable<ProductPageGrid_AttributeDTO> Attributes { get; set; }

		public string Sort { get; set; }

		public byte? ProductTypeID { get; set; }

		public VisitorSelect Visitor { get; set; }

		public NewMasterDefinition MasterDefinition { get; set; }

		public BaseViewModel Base { get; set; }
	}
}