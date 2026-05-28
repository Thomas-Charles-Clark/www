using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageSectionBrochure
	{
		public long PageSectionID { get; set; }
		public short BrochureID { get; set; }
		public string InteractiveBrochureURL { get; set; }
		public string CoverImagePath { get; set; }
		public string Heading { get; set; }
		public string Description { get; set; }
	}
}