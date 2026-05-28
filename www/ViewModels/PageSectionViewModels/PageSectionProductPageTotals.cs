using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageSectionProductPageTotals
	{
		public List<int> ProductPageIDs { get; set; }

		public PageSectionProductPageTotals ()
		{
			ProductPageIDs = new List<int>();
		}

		public int Total 
		{ 
			get
			{
				return ProductPageIDs.Count();
			} 
		}
	}
}