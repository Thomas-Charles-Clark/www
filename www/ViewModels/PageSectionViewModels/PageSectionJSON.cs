using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageSectionJSON : DatabaseProductPagesJSONResponse
	{
		public long PageSectionID { get; set; }
		public List<pageSectionAttribute> attributes { get; set; }
		public PageSectionProductPageTotals totals { get; set; }
	}
}