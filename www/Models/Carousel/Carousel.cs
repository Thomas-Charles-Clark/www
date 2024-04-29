using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Hotspot;
using wwwDesignersGuild.Models.ImgSize;

namespace wwwDesignersGuild.Models.Carousel
{
    public class CarouselImage
    {
        public long ImageID { get; set; }
        public byte? SortOrder { get; set; }
        public string AltText { get; set; }

        [JsonIgnore]
        public IEnumerable<Hotspot.Hotspot> Hotspots { get; set; }

        public CarouselImageOverlay Overlay { get; set; }

        public int? MaxWidth { get; set; }

        public ImageSize ImageSize { get; set; }

        public bool ForceLazyLoading { get; set; }

        public CarouselImage (
            long imageId,
            byte? sortOrder,
            string altText,
            ImageSize imageSize,
            CarouselImageOverlay overlay = null,
            int? maxWidth = null,
            bool forceLazyLoading = false)
        {
            ImageID = imageId;
            SortOrder = sortOrder;
            AltText = altText;
            Overlay = overlay;
            MaxWidth = maxWidth;
            ImageSize = imageSize;
            ForceLazyLoading = forceLazyLoading;

            Hotspots = new List<Hotspot.Hotspot>();
        } 
    }

    public class CarouselImageOverlay
    {
        public bool Visible { get; set; }
        public bool AutoHide { get; set; }
        public string Background { get; set; }
        public decimal Opacity { get; set; }
        public string FontColour { get; set; }
        public string Heading { get; set; }
        public decimal Height { get; set; }
        public decimal Left { get; set; }
        public string Text { get; set; }
        public decimal Top { get; set; }
        public decimal Width { get; set; }

        public string HeightPercent
        {
            get
            {
                return $"{Height:N2}%";
            }
        }

        public string LeftPercent
        {
            get
            {
                return $"{Left:N2}%";
            }
        }

        public string TopPercent
        {
            get
            {
                return $"{Top:N2}%";
            }
        }

        public string WidthPercent
        {
            get
            {
                return $"{Width:N2}%";
            }
        }

        public CarouselImageOverlay(pageSectionSlideShow i)
        {
            Visible = i.DisplayOverlay;
            AutoHide = Convert.ToBoolean(i.AutoHideOverlay);

            i.OverlayBackgroundColour = i.OverlayBackgroundColour ?? "255,255,255";

            Background = $"linear-gradient(to right, rgba({i.OverlayBackgroundColour},{i.OverlayBackgroundOpacity}) 0%, rgba({i.OverlayBackgroundColour},{i.OverlayBackgroundOpacity}) 72%, rgba({i.OverlayBackgroundColour},0) 98%, rgba({i.OverlayBackgroundColour},0) 100%)";
            Opacity = i.OverlayBackgroundOpacity ?? 1.0m;
            FontColour = $"rgb({i.OverlayFontColour?.Trim() ?? "80,84,93"})";
            Heading = i.OverlayHeading?.Replace(">", "").Trim();
            Height = i.OverlayHeight ?? 3.0m;
            Left = i.OverlayLeft ?? 0.0m;
            Text = i.OverlayText;
            Top = i.OverlayTop ?? 0.0m;
            Width = i.OverlayWidth ?? 35.0m;
        }

        public CarouselImageOverlay(pageSectionAnimationSlide i)
        {
            Visible = false;
            AutoHide = true;

            Background = null;
            Opacity = 0.0m;
            FontColour = null;
            Heading = null;
            Height = 0.0m;
            Left = 0.0m;
            Text = null;
            Top = 0.0m;
            Width = 0.0m;
        }
    }
}