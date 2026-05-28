using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageSectionSelect
	{
		public long PageSectionID { get; set; }
		public byte? PageSectionTypeID { get; set; }
		public byte? SortOrder { get; set; }
		public bool? PlatformSwitch { get; set; }
		public string Heading { get; set; }
		public bool PreviewColourways { get; set; }
		public bool PreviewColourwaysCircularClip { get; set; }
		public bool IsHeader { get; set; }
		public bool? TradeSwitch { get; set; }
		public bool HalfWidth { get; set; }
		public bool Paging { get; set; }
		public bool Filter { get; set; }
		public bool IsLifestyleGallery { get; set; }
		public bool IsShop { get; set; }
		public byte ProductTypeID { get; set; }
		public bool ViewportWidth { get; set; }
	}
}