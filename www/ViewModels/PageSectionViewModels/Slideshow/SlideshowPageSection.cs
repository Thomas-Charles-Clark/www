using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Carousel;
using wwwDesignersGuild.Models.ImgSize;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class SlideshowPageSection
	{
		/// <summary>
		/// Used just in case a carousel image is missing some alt text
		/// </summary>
		public string Heading { get; set; }
		public CarouselViewModel CarouselViewModel { get; set; }

		public SlideshowPageSection(
			long pageSectionId,
			bool halfWidth,
			string heading,
			List<PageSectionSlideShow> images,
			PageSectionSlideShowAspect aspect,
			BaseViewModel baseViewModel
			)
		{
			Heading = heading;
			CarouselViewModel = new CarouselViewModel
			(
				carouselId: $"#page-section-{pageSectionId}-carsouel",
				wrapperCssClass: null,
				aspectRatio: aspect.AspectRatio,
				heading: heading,
				images: images.Select(i => new CarouselImage(i.ImageID, i.SortOrder, i.AltText, ImageSize.Twelve, new CarouselImageOverlay(i), i.MaxWidth))
							  .OrderBy(i => i.SortOrder),
				baseViewModel: baseViewModel
			);
		}
	}
}