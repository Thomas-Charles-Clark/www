using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class ProductPageFilterablePodWrapper
	{
		public int ProductPageID { get; set; }
		public ProductPageFilterablePod ProductPage { get; set; }
		public bool PreviewColourways { get; set; }
	}
}