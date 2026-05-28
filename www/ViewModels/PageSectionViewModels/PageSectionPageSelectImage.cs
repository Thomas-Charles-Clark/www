using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageSectionPageSelectImage
	{
		public int PageID { get; set; }
		public long ImageID { get; set; }
		public byte? SortOrder { get; set; }
		public long PageSectionID { get; set; }
	}
}