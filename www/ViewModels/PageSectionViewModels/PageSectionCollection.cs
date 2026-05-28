using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageSectionCollection
	{
		public long PageSectionID { get; set; }
		public short CollectionID { get; set; }
		public string Heading { get; set; }
		public string SubHeading { get; set; }
		public string URLKeywords { get; set; }
		public short? SortOrder { get; set; }
		public bool New { get; set; }
		public CarouselViewModel CarouselViewModel { get; internal set; }
	}
}