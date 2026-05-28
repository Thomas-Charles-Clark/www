using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageSectionAnimationSlideCallToAction
	{
		public long PageSectionID { get; set; }
		public int SlideID { get; set; }
		public short AnimationID { get; set; }
		public byte SlideAreaID { get; set; }
		public string Text { get; set; }
		public string URL { get; set; }
		public bool NewWindow { get; set; }
		public short StyleID { get; set; }
		public byte CallToActionWidthModeID { get; set; }
		public decimal? AppearanceDelay { get; set; }
		public decimal? VisibilityDuration { get; set; }
		public byte SortOrder { get; set; }
		public string Margin { get; set; }
		public string Padding { get; set; }
		public string BackgroundColour { get; set; }
		public string Colour { get; set; }
		public string BorderColour { get; set; }
		public byte? BorderWidth { get; set; }
		public string BorderStyle { get; set; }
		public decimal? Opacity { get; set; }
	}
}