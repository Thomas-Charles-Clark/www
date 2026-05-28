using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.Models.RecentlyViewed
{
    public class RecentlyViewedProductPage
    {
        public decimal AspectRatio { get; set; }
        public string Heading { get; set; }
        public short Height { get; set; }
        public long ImageID { get; set; }
        public bool New { get; set; }
        public int ProductPageID { get; set; }
        public decimal? ReviewAverage { get; set; }
        public int? ReviewCount { get; set; }
        public string SubHeading { get; set; }
        public string URL { get; set; }
        public string URLSemantics { get; set; }
        public short Width { get; set; }
        public ProductPagePodPrice ProductPagePrice { get; set; }
    }
}