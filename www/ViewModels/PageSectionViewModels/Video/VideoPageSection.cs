using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class VideoPageSection
	{
		public PageSectionVideo Video { get; set; }

		public VideoPageSection(
			PageSectionVideo video
			)
		{
			Video = video;
		}
	}
}