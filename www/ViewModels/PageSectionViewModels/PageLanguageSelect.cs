using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageLanguageSelect
	{
		public byte SiteSectionID { get; set; }

		public string Heading { get; set; }

		public string SubHeading { get; set; }

		[JsonIgnore]
		public string Description { get; set; }

		public string TitleTag { get; set; }

		public string MetaDescription { get; set; }

		public string URLKeywords { get; set; }

		public bool IsShop { get; set; }

		public string DiscountMessage { get; set; }

		public bool ShowRecentlyViewed { get; set; }

		public bool NoIndex { get; set; }

		public bool NoFollow { get; set; }

		public string CustomCanonical { get; set; }

		public bool EnableFilterIndexing { get; set; }

		public byte? ProductTypeID { get; set; }
	}
}