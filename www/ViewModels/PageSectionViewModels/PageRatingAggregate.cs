using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageRatingAggregate
	{
		public string Type { get; set; }
		public decimal? RatingValue { get; set; }
		public decimal? BestRating { get; set; }
		public int RatingCount { get; set; }
	}
}