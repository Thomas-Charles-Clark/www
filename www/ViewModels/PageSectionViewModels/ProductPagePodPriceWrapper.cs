using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{

	public class ProductPagePodPriceWrapper
	{
		public int ProductPageID { get; set; }
		public ProductPagePodPrice Price { get; set; }

		public ProductPagePodPriceWrapper(int id, ProductPagePodPrice objPrice)
		{
			ProductPageID = id;
			Price = objPrice;
		}
	}
}