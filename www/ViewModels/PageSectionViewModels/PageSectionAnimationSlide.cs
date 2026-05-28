using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Hotspot;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageSectionAnimationSlide
	{
		public long PageSectionID { get; set; }
		public int SlideID { get; set; }
		public short AnimationID { get; set; }
		public byte SlideTypeID { get; set; }
		public bool DisplayCallsToAction { get; set; }
		public bool AutoHideCallsToAction { get; set; }
		public bool DisplayHotspots { get; set; }
		public bool AutoHideHotspots { get; set; }
		public long ImageID { get; set; }
		public string TitleText { get; set; }
		public string AltText { get; set; }
		public short Width { get; set; }
		public short Height { get; set; }
		public int MaxWidth { get; set; }
		public decimal AspectRatio { get; set; }
		public short? VideoID { get; set; }
		public string VideoEmbedCode { get; set; }
		public Byte SortOrder { get; set; }
		public string URL { get; set; }
		public bool NewWindow { get; set; }
		public IEnumerable<Hotspot> Hotspots { get; set; }

		public PageSectionAnimationSlide()
		{
			Hotspots = new List<Hotspot>();
		}
	}
}