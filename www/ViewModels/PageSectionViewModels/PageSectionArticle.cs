using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageSectionArticle
	{
		public long PageSectionID { get; set; }
		public int ArticleID { get; set; }
		public byte ArticleTypeID { get; set; }
		public string Heading { get; set; }
		public string SubHeading { get; set; }
		public string URLKeywords { get; set; }
		public short SortOrder { get; set; }
		public DateTime CreatedDT { get; set; }
		public bool New
		{
			get
			{
				return CreatedDT > DateTime.Now.AddMonths(-2);
			}
			set
			{

			}
		}

		public CarouselViewModel CarouselViewModel { get; set; }
		public string URL { get; set; }
		public List<string> AttributeFormValues { get; set; }
	}
}