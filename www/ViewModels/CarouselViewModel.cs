using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Carousel;
using wwwDesignersGuild.Models.ImgSize;

namespace wwwDesignersGuild.ViewModels
{
    public class CarouselViewModel
    {
        [JsonIgnore]
        public BaseViewModel Base { get; set; }
        public string CarouselID { get; set; }
        public string WrapperCSSClass { get; set; }
        public decimal AspectRatio { get; set; }
        public string Heading { get; set; }
        public List<CarouselImage> Images { get; set; }
        public bool DisplayThumbnails { get; set; }
        public bool DisplayDashes { get; set; }
        public int? Interval { get; set; }
        public bool PermitLargeImageModal { get; set; }
        public string VideoURL { get; set; }
        public string VideoThumbnail { get; set; }

        [JsonIgnore]
        public SocialMediaShareViewModel SocialMediaViewModel { get; set; }

        public CarouselViewModel
        (
            string carouselId,
            string wrapperCssClass,
            decimal aspectRatio,
            string heading,
            IEnumerable<CarouselImage> images,
            BaseViewModel baseViewModel,
            bool displayDashes = true,
            int? interval = null,
            bool permitLargeImageModal = true,
            string videoUrl = null,
            string videoThumbnail = null
        )
        {
            var imageList = images.ToList();

            imageList.ForEach(i =>
            {
                if (baseViewModel?.Hotspots.Where(h => h.ImageID == i.ImageID && h.Hotspots != null).Any() == true)
                { 
                    i.Hotspots = baseViewModel?.Hotspots.Where(h => h.ImageID == i.ImageID).SelectMany(h => h.Hotspots);
                }
            });

            CarouselID = carouselId;
            WrapperCSSClass = wrapperCssClass;
            AspectRatio = aspectRatio;
            Heading = heading;
            Images = imageList;
            Base = baseViewModel;
            DisplayThumbnails = false;
            DisplayDashes = displayDashes;

            SocialMediaViewModel = new SocialMediaShareViewModel(directionClass: "dropup", buttonOnly: true, objBase: Base);

            Interval = interval;

            PermitLargeImageModal = permitLargeImageModal;

            VideoURL = videoUrl;

            VideoThumbnail = videoThumbnail;
        }
    }
}