using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageStockStatus
	{
		public int PageID { get; set; }
		public int ParentID { get; set; }
		public string URLKeywords { get; set; }
		public string Heading { get; set; }
	}
}