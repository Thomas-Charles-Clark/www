using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.ImgSize;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class AnimationPageSection
	{
		public string PageHeading { get; set; }
		public string PageSubHeading { get; set; }
		public string PageDescription { get; set; }
		public string PageDiscountMessage { get; set; }
		public ImageSize ImageSize { get; set; }
		public PageSectionAnimation Animation { get; set; }
		public List<PageSectionAnimationSlide> Images { get; set; }
		public List<PageSectionAnimationSlideCallToAction> CallsToAction { get; set; }

		public AnimationPageSection(
			string pageHeading,
			string pageSubHeading,
			string pageDescription,
			string pageDiscountMessage,
			bool isHeader,
			PageSectionAnimation animation,
			List<PageSectionAnimationSlide> images,
			List<PageSectionAnimationSlideCallToAction> callsToAction
			)
		{
			PageHeading = pageHeading;
			PageSubHeading = pageSubHeading;
			PageDescription = pageDescription;
			PageDiscountMessage = pageDiscountMessage;
			Animation = animation;
			Images = images;
			ImageSize = isHeader ? ImageSize.Nine : ImageSize.Twelve;
			CallsToAction = callsToAction;

			CallsToAction.ForEach(c =>
			{
				c.Text = c.Text.Replace(">", "").Trim();
			});
		}
	}
}