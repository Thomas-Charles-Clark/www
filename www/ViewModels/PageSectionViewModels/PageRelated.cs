using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageRelated
	{
		public int PageID { get; set; }
		public string Heading { get; set; }
		public string URLKeywords { get; set; }
		public string FullUrl { get; set; }
		public string RelatedFilterFontColour { get; set; }
		public bool RelatedFilterBold { get; set; }
	}
}