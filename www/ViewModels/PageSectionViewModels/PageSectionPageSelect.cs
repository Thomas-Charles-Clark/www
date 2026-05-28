using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageSectionPageSelect
	{
		public long PageSectionID { get; set; }
		public int PageID { get; set; }
		public byte? SortOrder { get; set; }
		public int ProductPageID { get; set; }
		public byte ProductTypeID { get; set; }
		public string URLKeywords { get; set; }
		public string Heading { get; set; }
		public string SubHeading { get; set; }
		public bool New { get; set; }
		public string DiscountMessage { get; set; }

		public CarouselViewModel CarouselViewModel { get; set; }
	}
}