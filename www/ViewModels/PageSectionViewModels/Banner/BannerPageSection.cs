using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Banner;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class BannerGridPageSection
	{
		public BannerLocationGroup BannerGrid { get; set; }

		public BannerGridPageSection(
			BannerLocationGroup bannerGrid
			)
		{
			BannerGrid = bannerGrid;
		}
	}
}