using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageSectionSlideShow
	{
		public long PageSectionID { get; set; }
		public long SlideShowID { get; set; }
		public byte? SortOrder { get; set; }
		public long ImageID { get; set; }
		private string altText;
		public string AltText
		{
			get
			{
				if (altText == null)
				{
					return "";
				}
				return altText;
			}
			set
			{
				altText = value;
			}
		}
		public short Width { get; set; }
		public short Height { get; set; }
		public int MaxWidth { get; set; }
		public decimal AspectRatio { get; set; }
		public bool DisplayOverlay { get; set; }
		public string URL { get; set; }
		public bool NewWindow { get; set; }
		public string OverlayHeading { get; set; }
		public string OverlayText { get; set; }
		public decimal? OverlayTop { get; set; }
		public decimal? OverlayLeft { get; set; }
		public decimal? OverlayWidth { get; set; }
		public decimal? OverlayHeight { get; set; }
		public string OverlayBackgroundColour { get; set; }
		public decimal? OverlayBackgroundOpacity { get; set; }
		public int AutoHideOverlay { get; set; }
		public string OverlayFontColour { get; set; }
		public int OverlayFade { get; set; }
	}
}