using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Aspect;
using wwwDesignersGuild.Models.Carousel;
using wwwDesignersGuild.Models.ImgSize;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageGridPageSection
	{
		public List<PageSectionPageSelect> Pages { get; set; }

		public PageGridPageSection(
			List<PageSectionPageSelect> pages,
			List<PageSectionPageSelectImage> images,
			BaseViewModel baseViewModel
			)
		{
			Pages = pages;

			Pages.ForEach(p =>
			{
				p.CarouselViewModel = new CarouselViewModel
				(
					carouselId: $"#page-{p.PageID}-carousel",
					wrapperCssClass: "card-img-top",
					aspectRatio: AspectRatio.LandscapeFeature,
					heading: p.Heading,
					images: images.Where(i => i.PageID == p.PageID)
									.Select(i => new CarouselImage(i.ImageID, i.SortOrder, p.Heading, ImageSize.Four))
									.OrderBy(i => i.SortOrder),
					baseViewModel: baseViewModel
				);
			});
		}
	}

}