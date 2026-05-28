using iText.Layout.Renderer;

using Kendo.Mvc.UI;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Web;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Source;
using wwwDesignersGuild.ViewModels;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.Models.Classes
{
    public class cmsLandingPageDefinition
    {
        public PageLanguageSelect PageDetails { get; set; }
        public List<PageSectionSelect> pageSectionSelects { get; set; }
        public List<PageSectionJSON> productPageGridSections { get; set; } // new!
        public List<PageSectionHTML> pageSectionHtmls { get; set; }
        public List<PageSectionSlideShowAspect> pageSectionSlideShowAspects { get; set; }
        public List<PageSectionSlideShow> pageSectionSlideShows { get; set; }
        public List<PageSectionForm> pageSectionForms { get; set; }
        public List<PageSectionFormRetailAppointmentAgenda> pageSectionFormRetailAppointmentAgendas { get; set; }
        public List<PageSectionFormRetailAppointmentType> pageSectionFormRetailAppointmentTypes { get; set; }
        public List<PageSectionBrochure> pageSectionBrochures { get; set; }
        public List<PageSectionProductPageTotals> pageSectionProductPageTotalCounts { get; set; }
        public List<PageSectionCollection> pageSectionCollections { get; set; }
        public List<PageSectionCollectionImage> pageSectionCollectionImages { get; set; }
        public List<PageSectionPageSelect> pageSectionPageSelects { get; set; }
        public List<PageSectionPageSelectImage> pageSectionPageSelectImages { get; set; }
        public PageSectionBannerJSON pageSectionBanners { get; set; }
        public List<PageRelated> pageRelateds { get; set; }
        public List<Breadcrumb> Breadcrumbs { get; set; }
        public List<GoogleHref> pageGoogleHrefs { get; set; }
        public List<PageSectionArticle> pageSectionArticles { get; set; }
        public List<PageSectionArticleImage> pageSectionArticleImages { get; set; }
        public List<PageFormCountry> pageFormCountrys { get; set; }
        public List<PageSectionVideo> pageSectionVideos { get; set; }
        public List<PageSectionStoreEvent> pageSectionStoreEvents { get; set; }
        public List<PageSectionAnimation> pageSectionAnimations { get; set; }
        public List<PageSectionAnimationSlide> pageSectionAnimationSlides { get; set; }
        public List<PageSectionAnimationSlideCallToAction> pageSectionAnimationSlideCallsToAction { get; set; }
        public List<PageRatingAggregate> ratingAggregate { get; set; }
    }
}