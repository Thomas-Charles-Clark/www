using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class BrochurePageSection
	{
		public PageSectionBrochure Brochure { get; set; }

		public BrochurePageSection(
			PageSectionBrochure brochure
			)
		{
			Brochure = brochure;
		}
	}
}