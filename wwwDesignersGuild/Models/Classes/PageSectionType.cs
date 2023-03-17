using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Util;

namespace wwwDesignersGuild.Models.Classes
{
    public enum PageSectionType
    {
        [PageSectionClass("html-section")]
        [ViewPath("~/Views-New/LandingPages/PageSections/_HTML.cshtml")]
        HTML = 1,

        [PageSectionClass("carousel-section")]
        [ViewPath("~/Views-New/LandingPages/PageSections/_Carousel.cshtml")]
        Slideshow = 2,

        [PageSectionClass("form-section")]
        [ViewPath("~/Views-New/LandingPages/PageSections/_Form.cshtml")]
        Form = 3,

        [PageSectionClass("product-page-grid-section")]
        [ViewPath("~/Views-New/LandingPages/PageSections/_ProductPageGrid.cshtml")]
        ProductPageGrid = 4,

        [PageSectionClass("collection-grid-section")]
        [ViewPath("~/Views-New/LandingPages/PageSections/_CollectionGrid.cshtml")]
        CollectionGrid = 5,

        [PageSectionClass("page-grid-section")]
        [ViewPath("~/Views-New/LandingPages/PageSections/_PageGrid.cshtml")]
        PageGrid = 6,

        [PageSectionClass("brochure-section")]
        [ViewPath(null)]
        Brochure = 7,

        [PageSectionClass("banner-grid-section")]
        [ViewPath("~/Views-New/LandingPages/PageSections/_BannerGrid.cshtml")]
        BannerLocationGroup = 8,

        [PageSectionClass("article-grid-section")]
        [ViewPath("~/Views-New/LandingPages/PageSections/_ArticleGrid.cshtml")]
        ArticleGrid = 9,

        [PageSectionClass("video-section")]
        [ViewPath("~/Views-New/LandingPages/PageSections/_Video.cshtml")]
        Video = 10,

        [PageSectionClass("masonry-section")]
        [ViewPath(null)]
        Masonry = 11,

        [PageSectionClass("animation-section html-section")]
        [ViewPath("~/Views-New/LandingPages/PageSections/_Animation.cshtml")]
        Animation = 12,

        [PageSectionClass("bespoke-section")]
        [ViewPath(null)]
        BespokeContent = 13
    }
}