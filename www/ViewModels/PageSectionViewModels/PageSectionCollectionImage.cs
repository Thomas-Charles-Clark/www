using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageSectionCollectionImage
	{
		public short CollectionID { get; set; }
		public long ImageID { get; set; }
		public byte? SortOrder { get; set; }
	}
}