using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageSectionArticleImage
	{
		public int ArticleID { get; set; }
		public long ImageID { get; set; }
		public byte? SortOrder { get; set; }
	}
}