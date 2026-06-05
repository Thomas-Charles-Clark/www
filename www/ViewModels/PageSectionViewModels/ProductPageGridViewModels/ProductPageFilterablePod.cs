using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.ProductPage;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{

	public class ProductPageFilterablePod : ProductPagePod
	{
		public int ScorePercent { get; set; }
		public string URL { get; set; }
		public string URLSemantics { get; set; }
		public List<string> AttributeFormValues { get; set; }
		public ProductType ProductType { get; set; }
		public bool IsLifestyle { get; set; }
		public string ShortHeading { get; set; }

		public string ProductTypeFormValue
		{
			get
			{
				return ProductType?.FormValue;
			}
			set
			{
			}
		}

		public ProductPageFilterablePod()
		{

		}

		public ProductPageFilterablePod(serverSearchResultCore p, ProductType pt)
		{
			ProductPageID = p.ProductPageID;
			ProductTypeID = p.ProductTypeID;
			ScorePercent = p.ScorePercent;
			SearchSortOrder = p.SearchSortOrder;

			AZSortOrder = p.AtoZScore;
			ColourSort = p.ColourSort;
			NewSort = p.NewSort;
			PriceSortOrder = p.PriceSort;
			ProductType = pt;

			SearchSortOrder_New = p.SearchSortOrder_New;
			AtoZScore_New = p.AtoZScore_New;
			ColourSort_New = p.ColourSort_New;
			NewSort_New = p.NewSort_New;
			PriceSort_New = p.PriceSort_New;
		}

		internal void ApplyRender(ProductPageFilterablePod patch)
		{
			Heading = patch.Heading ?? Heading;
			SubHeading = patch.SubHeading ?? SubHeading;
			AKAHeading = patch.AKAHeading ?? AKAHeading;
			URLKeywords = patch.URLSemantics ?? patch.URLKeywords ?? URLKeywords;
			URL = patch.URL ?? URL;
			ImageID = patch.ImageID;
			ImageWidth = patch.ImageWidth;
			ImageHeight = patch.ImageHeight;
			AspectRatio = patch.AspectRatio;
			ReviewAverage = patch.ReviewAverage ?? patch.ReviewAverage;
			ReviewCount = patch.ReviewCount ?? ReviewCount;
			New = patch.New;
			ProductTypeID = patch.ProductTypeID;
			ProductPagePrice = patch.ProductPagePrice;

			SearchSortOrder_New = patch.SearchSortOrder_New;
			AtoZScore_New = patch.AtoZScore_New;
			ColourSort_New = patch.ColourSort_New;
			NewSort_New = patch.NewSort_New;
			PriceSort_New = patch.PriceSort_New;
		}

		internal void ApplySorting(serverSearchResultCore c)
		{
			SearchSortOrder = c.SearchSortOrder;
			AZSortOrder = c.AtoZScore;
			ColourSort = c.ColourSort;
			NewSort = c.NewSort;
			PriceSortOrder = c.PriceSort;

			SearchSortOrder_New = c.SearchSortOrder_New;
			AtoZScore_New = c.AtoZScore_New;
			ColourSort_New = c.ColourSort_New;
			NewSort_New = c.NewSort_New;
			PriceSort_New = c.PriceSort_New;
		}
	}
}