using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Banner
{
    public class cmsBannerDefinition
    {
        public string JSON { get; set; }
        public TemplateBannerResult result
        {
            set
            {

            }
            get
            {
                return JsonConvert.DeserializeObject<TemplateBannerResult>(JSON);
            }
        }
    }

    public class TemplateBannerResult
    {
        public List<bannerLocationGroup> bannerLocationGroups { get; set; }
    }

    public class bannerLocationGroup
    {
        public byte id { get; set; }
        public string name { get; set; }
        public bannerLocation bannerLocations { get; set; }
    }

    public class bannerLocation
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<templateBanner> banners { get; set; }

        public bannerLocation()
        {
            banners = new List<templateBanner>();
        }
    }

    public class templateBanner
    {
        public int bannerId { get; set; }
        public byte bannerLocationId { get; set; }
        public string name { get; set; }
        public short version { get; set; }
        public byte sortOrder { get; set; }
        public string overlayText { get; set; }
        public string altText { get; set; }
        public string titleText { get; set; }
        public short width { get; set; }
        public short height { get; set; }
        public string linkURL { get; set; }
        public bool newWindow { get; set; }
        public string foregroundColour { get; set; }
        public string backgroundColour { get; set; }
        public string fontClass { get; set; }
        public bannerRestriction restrictions { get; set; }
    }

    public class bannerRestriction
    {
        public List<int> menuIds { get; set; }
        public List<int> siteSectionIds { get; set; }
        public List<int> pageIds { get; set; }
    }

    public class passBanner
    {
        public masterDefinition masterDefinition { get; set; }
        public List<bannerLocation> banners { get; set; }
    }
}