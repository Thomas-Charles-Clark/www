using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Aspect;
using wwwDesignersGuild.Models.Carousel;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.ImgSize;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class CollectionGridPageSection
	{
		public List<PageSectionCollection> Collections { get; set; }

		public CollectionGridPageSection(
			List<PageSectionCollection> collections,
			List<PageSectionCollectionImage> images,
			BaseViewModel baseViewModel
			)
		{
			Collections = collections.OrderBy(p => p.SortOrder).ToList();

			Collections.ForEach(c =>
			{
				c.CarouselViewModel = new CarouselViewModel
				(
					carouselId: $"#collection-{c.CollectionID}-carousel",
					wrapperCssClass: "card-img-top",
					aspectRatio: AspectRatio.LandscapeFeature,
					heading: c.Heading,
					images: images.Where(i => i.CollectionID == c.CollectionID)
								  .Select(i => new CarouselImage(
									  imageId: i.ImageID,
									  sortOrder: i.SortOrder,
									  altText: c.Heading,
									  imageSize: ImageSize.Nine,
									  overlay: null,
									  maxWidth: null,
									  forceLazyLoading: true))
								  .OrderBy(i => i.SortOrder),
					baseViewModel: baseViewModel,
					displayDashes: false
				);
			});
		}
	}
}