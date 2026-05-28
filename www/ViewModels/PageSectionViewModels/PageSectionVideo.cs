using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageSectionVideo
	{
		public long PageSectionID { get; set; }
		public short VideoID { get; set; }
		public string VideoEmbedCode { get; set; }
		public decimal AspectRatio { get; set; }
	}
}