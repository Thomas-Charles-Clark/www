using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class ProductPagePod
	{
		public int ProductPageID { get; set; }
		public string Heading { get; set; }
		public string AKAHeading { get; set; }
		public string SubHeading { get; set; }
		public bool New { get; set; }
		public byte ProductTypeID { get; set; }
		public string URLKeywords { get; set; }
		public long ImageID { get; set; }
		public short ImageWidth { get; set; }
		public short ImageHeight { get; set; }
		public decimal AspectRatio { get; set; }
		public int? MerchandisedSortOrder { get; set; }
		public int? SearchSortOrder { get; set; }
		public int? ColourSort { get; set; }
		public decimal StockSortOrder { get; set; }
		public long AZSortOrder { get; set; }
		public long SalesRank { get; set; }
		public int? ReviewCount { get; set; }
		public decimal? ReviewAverage { get; set; }
		public int? RatingSortOrder { get; set; }
		public long? PriceSortOrder { get; set; }
		public int? DiscountSortOrder { get; set; }
		public long? NewSort { get; set; }
		public int? DesignID { get; set; }
		public byte BrandID { get; set; }
		public bool AlternateColourwayIncludeSize { get; set; }
		public string SizeMetric { get; set; }
		public string SizeImperial { get; set; }
		public string Code { get; set; }
		public byte? SortOrder { get; set; }
		public string Design { get; set; }
		public ProductPagePodPrice ProductPagePrice { get; set; }
		public bool NoIndex { get; set; }
		public bool NoFollow { get; set; }
		public string Rel
		{
			get
			{
				var parts = new List<string>();

				if (NoIndex)
				{
					parts.Add("noindex");
				}

				if (NoFollow)
				{
					parts.Add("nofollow");
				}

				if (parts.Any())
				{
					return String.Join(", ", parts);
				}
				else
				{
					return null;
				}
			}
		}

		private int ReviewWholeStars
		{
			get
			{
				if (ReviewAverage.HasValue)
				{
					return (int)Math.Truncate(ReviewAverage.Value);
				}

				return 0;
			}
			set
			{

			}
		}

		private bool ReviewHalfStar
		{
			get
			{
				if (ReviewAverage.HasValue)
				{
					return ReviewAverage - Math.Truncate(ReviewAverage.Value) > 0;
				}

				return false;
			}
			set
			{

			}
		}

		private int RemainingStars
		{
			get
			{
				return 5 - ReviewWholeStars - (ReviewHalfStar ? 1 : 0);
			}
			set
			{

			}
		}

		public bool DisplayParagraph
		{
			get
			{
				return (!string.IsNullOrEmpty(ProductPagePrice?.CodeMessage)) ||
					   (!string.IsNullOrEmpty(ProductPagePrice?.DiscountMessage)) ||
					   (!string.IsNullOrEmpty(ProductPagePrice?.PriceMessage)) ||
					   (!string.IsNullOrEmpty(ProductPagePrice?.RRPMessage));
			}
		}

		public bool DisplayRating
		{
			get
			{
				return ReviewCount > 0;
			}
		}

		[JsonIgnore]
		public string ReviewStarsDisplay
		{
			get
			{
				if (ReviewCount > 0)
				{
					var sb = new StringBuilder();

					sb.Append(@"<div class=""product-page-rating"">");

					for (var i = 0; i < ReviewWholeStars; i++)
					{
						sb.Append(@"<i class=""bi bi-star-fill""></i>");
					}

					if (ReviewHalfStar)
					{
						sb.Append(@"<i class=""bi bi-star-half""></i>");
					}

					for (var i = 0; i < RemainingStars; i++)
					{
						sb.Append(@"<i class=""bi bi-star""></i>");
					}

					sb.Append($"<span>&nbsp;({ReviewCount})</span>");

					sb.Append("</div>");

					return sb.ToString();
				}
				else
				{
					return null;
				}
			}
		}

		public string ReviewStarsDisplayBase64
		{
			get
			{
				if (!string.IsNullOrEmpty(ReviewStarsDisplay))
				{
					return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(ReviewStarsDisplay));
				}
				else
				{
					return null;
				}
			}
		}

		public bool DisplayRRPs
		{
			get
			{
				return ProductTypeID == 15 || ProductTypeID == 18;
			}
		}
	}
}