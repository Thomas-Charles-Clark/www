using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.ImgSize;

namespace wwwDesignersGuild.Models.Banner
{
    public class BannerLocationGroup
    {
        public List<BannerLocation> BannerLocations { get; set; }

        public string Name { get; set; }

        public string GridColumnClass 
        {
            get
            {
                return $"g-col-12 g-col-lg-{(12 / BannerLocations.Count())}";
            }
        }

        public ImageSize ImageSize
        {
            get
            {
                switch(BannerLocations.Count)
                {
                    case 1:
                        return ImageSize.Twelve;
                    case 2:
                        return ImageSize.Six;
                    case 3:
                        return ImageSize.Four;
                    case 4:
                        return ImageSize.Three;
                    default:
                        return ImageSize.Two;
                }
            }
        }

        public BannerLocationGroup()
        {
            BannerLocations = new List<BannerLocation>();
        }

        public int GetImageWidth()
        {
            switch (ImageSize)
            {
                case ImageSize.Twelve:
                    return 1024;
                case ImageSize.Six:
                    return 1024;
                case ImageSize.Four:
                    return 512;
                default:
                    return 342;
            }
        }

        public int GetImageHeight(decimal aspectRatio)
        {
            switch (ImageSize)
            {
                case ImageSize.Twelve:
                    return (int)(1024 / aspectRatio);
                case ImageSize.Six:
                    return (int)(1024 / aspectRatio);
                case ImageSize.Four:
                    return (int)(512 / aspectRatio);
                default:
                    return (int)(342 / aspectRatio);
            }
        }
    }

    public class BannerLocation
    {
        public byte ID { get; set; }
        public List<Banner> Banners { get; set; }
        public string Name { get; set; }
        public string CarouselID 
        {
            get
            {
                return $"banner-location-{ID}-carousel";
            } 
        }

        public BannerLocation()
        {
            Banners = new List<Banner>();
        }


    }

    public class Banner
    {
        public int BannerID { get; set; }
        public long? ImageID { get; set; }
        public string AltText { get; set; }
        public decimal AspectRatio { get; set; }
        public string BackgroundColour { get; set; }
        public string FontClass { get; set; }
        public string ForegroundColour { get; set; }
        public int Height { get; set; }
        public string LinkURL { get; set; }
        public string Name { get; set; }
        public bool NewWindow { get; set; }
        public string OverlayText { get; set; }
        public short SortOrder { get; set; }
        public string TitleText { get; set; }
        public int Version { get; set; }
        public int Width { get; set; }

        public string InlineStyle
        {
            get
            {
                return $"color: {(ForegroundColour ?? "inherit")}; background-color: {(BackgroundColour ?? "inherit")};";
            }
        }

        public string OverlayTextDisplay
        {
            get
            {
                var result = OverlayText;

                result = result?.Replace(">", "")?.Trim();

                return result;
            }
        }
    }
}