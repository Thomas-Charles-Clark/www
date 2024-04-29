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

namespace wwwDesignersGuild.Models.Classes
{
    public class cmsLandingPageDefinition
    {
        public pageLanguageSelect PageDetails { get; set; }
        public List<pageSectionSelect> pageSectionSelects { get; set; }
        public List<PageSectionJSON> productPageGridSections { get; set; } // new!
        public List<pageSectionHtml> pageSectionHtmls { get; set; }
        public List<pageSectionSlideShowAspect> pageSectionSlideShowAspects { get; set; }
        public List<pageSectionSlideShow> pageSectionSlideShows { get; set; }
        public List<pageSectionForm> pageSectionForms { get; set; }
        public List<pageSectionFormRetailAppointmentAgenda> pageSectionFormRetailAppointmentAgendas { get; set; }
        public List<pageSectionFormRetailAppointmentType> pageSectionFormRetailAppointmentTypes { get; set; }
        public List<pageSectionBrochure> pageSectionBrochures { get; set; }
        public List<pageSectionProductPageTotalCount> pageSectionProductPageTotalCounts { get; set; }
        //public List<pageSectionProductPageStub> pageSectionAllProductPageStubs { get; set; }
        public List<pageSectionCollection> pageSectionCollections { get; set; }
        public List<pageSectionCollectionImage> pageSectionCollectionImages { get; set; }
        public List<pageSectionPageSelect> pageSectionPageSelects { get; set; }
        public List<pageSectionPageSelectImage> pageSectionPageSelectImages { get; set; }
        public pageSectionBannerJson pageSectionBanners { get; set; }
        public List<pageRelated> pageRelateds { get; set; }
        //public List<pageStockStatus> pageStockStatuss { get; set; }
        public List<Breadcrumb> Breadcrumbs { get; set; }
        public List<googleHref> pageGoogleHrefs { get; set; }
        public List<pageSectionArticle> pageSectionArticles { get; set; }
        public List<pageSectionArticleImage> pageSectionArticleImages { get; set; }
        public List<pageFormCountry> pageFormCountrys { get; set; }
        public List<pageSectionVideo> pageSectionVideos { get; set; }
        public List<pageSectionStoreEvent> pageSectionStoreEvents { get; set; }
        public List<pageSectionAnimation> pageSectionAnimations { get; set; }
        public List<pageSectionAnimationSlide> pageSectionAnimationSlides { get; set; }
        public List<pageSectionAnimationSlideCallToAction> pageSectionAnimationSlideCallsToAction { get; set; }
        public List<pageRatingAggregate> ratingAggregate { get; set; }
    }

    public class pageLanguageSelect
    {
        public byte SiteSectionID { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        [JsonIgnore]
        public string Description { get; set; }
        public string TitleTag { get; set; }
        public string MetaDescription { get; set; }
        public string URLKeywords { get; set; }
        public bool IsShop { get; set; }
        public string DiscountMessage { get; set; }
        public bool ShowRecentlyViewed { get; set; }
        public bool NoIndex { get; set; }
        public bool NoFollow { get; set; }
        public string CustomCanonical { get; set; }
        public bool EnableFilterIndexing { get; set; }
        public byte? ProductTypeID { get; set; }
    }

    public class pageSectionSelect
    {
        public long PageSectionID { get; set; }
        public byte? PageSectionTypeID { get; set; }
        public byte? SortOrder { get; set; }
        public bool? PlatformSwitch { get; set; }
        public string Heading { get; set; }
        public bool PreviewColourways { get; set; }
        public bool PreviewColourwaysCircularClip { get; set; }
        public bool IsHeader { get; set; }
        public bool? TradeSwitch { get; set; }
        public bool HalfWidth { get; set; }
        public bool Paging { get; set; }
        public bool Filter { get; set; }
        public bool IsLifestyleGallery { get; set; }
        public bool IsShop { get; set; }
        public byte ProductTypeID { get; set; }
        public bool ViewportWidth { get; set; }
    }

    public class pageSectionHtml
    {
        public long HTMLSectionID { get; set; }

        public long PageSectionID { get; set; }

        [JsonIgnore]
        public string Contents { get; set; }

        public string ImageList { get; set; }

        public IEnumerable<long> ImageIDs
        {
            get
            {
                if (!string.IsNullOrEmpty(ImageList))
                {
                    return JsonConvert.DeserializeObject<IEnumerable<ImageIDWrapper>>(ImageList).Select(i => i.ImageID);
                }
                else
                {
                    return Enumerable.Empty<long>();
                }
            }
            internal set
            {

            }
        }
    }

    public class PageSectionJSON : DatabaseProductPagesJSONResponse
    {
        public long PageSectionID { get; set; }
        public List<pageSectionAttribute> attributes { get; set; }
    }
    public class pageSectionSlideShowAspect
    {
        public long PageSectionID { get; set; }
        public long SlideShowID { get; set; }
        public decimal AspectRatio { get; set; }
    }

    public class pageSectionSlideShow
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

    public class pageSectionForm
    {
        public short FormID { get; set; }
        public long PageSectionID { get; set; }
        public bool ConfirmEmail { get; set; }
        public bool AddressRequired { get; set; }
        public bool? PhoneNumberRequired { get; set; }
        public bool PasswordRequired { get; set; }
        public bool CaptchaRequired { get; set; }
        public string SubmitButtonText { get; set; }
        public string HeaderText { get; set; }
        public string FooterText { get; set; }
        public short? CompetitionID { get; set; }
        public string CompetitionTerms { get; set; }
        public short? BrochureID { get; set; }
        public string BrochureTitle { get; set; }
        public string BrochureCoverImagePath { get; set; }
        public string BrochurePDFPath { get; set; }
        public string InteractiveBrochureURL { get; set; }
        public short? StoreEventID { get; set; }
        public string StoreEventName { get; set; }
        public short? ThirdPartyID { get; set; }
        public byte SourceID { get; set; }
        public byte SourceGroupID { get; set; }
        public FormSource Source
        {
            get
            {
                return (FormSource)SourceID;
            }
            set
            {

            }
        }
        public string ThanksText { get; set; }
        public int? RedirectPageID { get; set; }

        public string PromotionCode { get; set; }
        public string PromotionDescription { get; set; }

        public string Heading { get; set; }
        public string SubHeading { get; set; }
    }

    public class pageSectionFormRetailAppointmentAgenda
    {
        public byte RetailAppointmentAgendaID { get; set; }
        public string Name { get; set; }
        public byte SortOrder { get; set; }
    }

    public class pageSectionFormRetailAppointmentType
    {
        public byte RetailAppointmentTypeID { get; set; }
        public string Name { get; set; }
        public byte SortOrder { get; set; }
    }

    public class pageSectionBrochure
    {
        public long PageSectionID { get; set; }
        public short BrochureID { get; set; }
        public string InteractiveBrochureURL { get; set; }
        public string CoverImagePath { get; set; }
        public string Heading { get; set; }
        public string Description { get; set; }
    }

    public class pageSectionProductPageTotalCount
    {
        public long PageSectionID { get; set; }
        public int TotalCount { get; set; }
    }

    public class pageSectionCollection
    {
        public long PageSectionID { get; set; }
        public short CollectionID { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public string URLKeywords { get; set; }
        public short? SortOrder { get; set; }
        public bool New { get; set; }
        public CarouselViewModel CarouselViewModel { get; internal set; }
    }

    public class pageSectionCollectionImage
    {
        public short CollectionID { get; set; }
        public long ImageID { get; set; }
        public byte? SortOrder { get; set; }
    }

    public class pageSectionPageSelect
    {
        public long PageSectionID { get; set; }
        public int PageID { get; set; }
        public byte? SortOrder { get; set; }
        public int ProductPageID { get; set; }
        public byte ProductTypeID { get; set; }
        public string URLKeywords { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public bool New { get; set; }
        public string DiscountMessage { get; set; }

        public CarouselViewModel CarouselViewModel { get; set; }
    }

    public class pageSectionPageSelectImage
    {
        public int PageID { get; set; }
        public long ImageID { get; set; }
        public byte? SortOrder { get; set; }
        public long PageSectionID { get; set; }
    }

    public class pageSectionBannerJson
    {
        public string BannerJSON { get; set; }
        public object json
        {
            set
            {

            }
            get
            {
                return JsonConvert.DeserializeObject(string.IsNullOrEmpty(BannerJSON) ? "{}" : BannerJSON);
            }
        }
    }

    public class pageRelated
    {
        public int PageID { get; set; }
        public string Heading { get; set; }
        public string URLKeywords { get; set; }
        public string FullUrl { get; set; }
        public string RelatedFilterFontColour { get; set; }
        public bool RelatedFilterBold { get; set; }
    }

    public class pageStockStatus
    {
        public int PageID { get; set; }
        public int ParentID { get; set; }
        public string URLKeywords { get; set; }
        public string Heading { get; set; }
    }

    public class googleHref
    {
        public string hreflang { get; set; }
        public string href { get; set; }
    }

    public class pageSectionArticle
    {
        public long PageSectionID { get; set; }
        public int ArticleID { get; set; }
        public byte ArticleTypeID { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public string URLKeywords { get; set; }
        public short SortOrder { get; set; }
        public DateTime CreatedDT { get; set; }
        public bool New
        {
            get
            {
                return CreatedDT > DateTime.Now.AddMonths(-2);
            }
            set
            {

            }
        }

        public CarouselViewModel CarouselViewModel { get; set; }
        public string URL { get; set; }
        public List<string> AttributeFormValues { get; set; }
    }

    public class pageSectionArticleImage
    {
        public int ArticleID { get; set; }
        public long ImageID { get; set; }
        public byte? SortOrder { get; set; }
    }

    public class pageFormCountry
    {
        public long PageSectionID { get; set; }
        public short CountryID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class pageSectionVideo
    {
        public long PageSectionID { get; set; }
        public short VideoID { get; set; }
        public string VideoEmbedCode { get; set; }
        public decimal AspectRatio { get; set; }
    }

    public class pageSectionStoreEvent
    {
        public long PageSectionID { get; set; }
        public short StoreEventID { get; set; }
        public short StoreEventLocationID { get; set; }
        public string StoreEventLocation { get; set; }
        public int? StoreEvent_DateID { get; set; }
        public DateTime? StoreEventDate { get; set; }
        public long? StoreEvent_Date_TimeSlotID { get; set; }
        public string StoreEventTimeSlot { get; set; }
    }


    public class pageSectionAnimation
    {
        public long PageSectionID { get; set; }
        public short AnimationID { get; set; }
        public decimal AspectRatio { get; set; }
    }

    public class pageSectionAnimationSlide
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
        public IEnumerable<Hotspot.Hotspot> Hotspots { get; set; }

        public pageSectionAnimationSlide()
        {
            Hotspots = new List<Hotspot.Hotspot>();
        }
    }

    public class pageSectionAnimationSlideCallToAction
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

    public class pageRatingAggregate
    {
        public string Type { get; set; }
        public decimal? RatingValue { get; set; }
        public decimal? BestRating { get; set; }
        public int RatingCount { get; set; }
    }


    public class productPagePod
    {
        public int ProductPageID { get; set; }
        public string Heading { get; set; }
        public string AKAHeading { get; set; }
        public string SubHeading { get; set; }
        public bool New { get; set; }
        public byte ProductTypeID { get; set; }
        public string URLKeywords { get; set; }
        public long ImageID { get; set; }
        public short ImageWidth { get; set; }
        public short ImageHeight { get; set; }
        public decimal AspectRatio { get; set; }
        public int? MerchandisedSortOrder { get; set; }
        public int? SearchSortOrder { get; set; }
        public int? ColourSort { get; set; }
        public decimal StockSortOrder { get; set; }
        public long AZSortOrder { get; set; }
        public long SalesRank { get; set; }
        public int? ReviewCount { get; set; }
        public decimal? ReviewAverage { get; set; }
        public int? RatingSortOrder { get; set; }
        public long? PriceSortOrder { get; set; }
        public int? DiscountSortOrder { get; set; }
        public long? NewSort { get; set; }
        public int? DesignID { get; set; }
        public byte BrandID { get; set; }
        public bool AlternateColourwayIncludeSize { get; set; }
        public string SizeMetric { get; set; }
        public string SizeImperial { get; set; }
        public string Code { get; set; }
        public byte? SortOrder { get; set; }
        public string Design { get; set; }
        public productPagePodPrice ProductPagePrice { get; set; }

        private int ReviewWholeStars
        {
            get
            {
                if (ReviewAverage.HasValue)
                {
                    return (int)Math.Truncate(ReviewAverage.Value);
                }

                return 0;
            }
            set
            {

            }
        }

        private bool ReviewHalfStar
        {
            get
            {
                if (ReviewAverage.HasValue)
                {
                    return ReviewAverage - Math.Truncate(ReviewAverage.Value) > 0;
                }

                return false;
            }
            set
            {

            }
        }

        private int RemainingStars
        {
            get
            {
                return 5 - ReviewWholeStars - (ReviewHalfStar ? 1 : 0);
            }
            set
            {

            }
        }

        public bool DisplayParagraph
        {
            get
            {
                return (!string.IsNullOrEmpty(ProductPagePrice?.CodeMessage)) ||
                       (!string.IsNullOrEmpty(ProductPagePrice?.DiscountMessage)) ||
                       (!string.IsNullOrEmpty(ProductPagePrice?.PriceMessage)) ||
                       (!string.IsNullOrEmpty(ProductPagePrice?.RRPMessage));
            }
        }

        public bool DisplayRating
        {
            get
            {
                return ReviewCount > 0;
            }
        }

        [JsonIgnore]
        public string ReviewStarsDisplay
        {
            get
            {
                if (ReviewCount > 0)
                {
                    var sb = new StringBuilder();

                    sb.Append(@"<div class=""product-page-rating"">");

                    for (var i = 0; i < ReviewWholeStars; i++)
                    {
                        sb.Append(@"<i class=""bi bi-star-fill""></i>");
                    }

                    if (ReviewHalfStar)
                    {
                        sb.Append(@"<i class=""bi bi-star-half""></i>");
                    }

                    for (var i = 0; i < RemainingStars; i++)
                    {
                        sb.Append(@"<i class=""bi bi-star""></i>");
                    }

                    sb.Append($"<span>&nbsp;({ReviewCount})</span>");

                    sb.Append("</div>");

                    return sb.ToString();
                }
                else
                {
                    return null;
                }
            }
        }

        public string ReviewStarsDisplayBase64
        {
            get
            {
                if (!string.IsNullOrEmpty(ReviewStarsDisplay))
                {
                    return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(ReviewStarsDisplay));
                }
                else
                {
                    return null;
                }
            }
        }

        public bool DisplayRRPs
        {
            get
            {
                return ProductTypeID == 15 || ProductTypeID == 18;
            }
        }
    }

    public class ProductPageFilterablePodWrapper
    {
        public int ProductPageID { get; set; }
        public ProductPageFilterablePod ProductPage { get; set; }
        public bool PreviewColourways { get; set; }
    }

    public class ProductPageFilterablePod : productPagePod
    {
        public int ScorePercent { get; set; }
        public string URL { get; set; }
        public string URLSemantics { get; set; }
        public List<string> AttributeFormValues { get; set; }
        public ProductType ProductType { get; set; }
        public bool IsLifestyle { get; set; }
        public string ShortHeading { get; set; }

        public string ProductTypeFormValue
        {
            get
            {
                return ProductType?.FormValue;
            }
            set
            {
            }
        }

        public ProductPageFilterablePod()
        {
        }

        public ProductPageFilterablePod(serverSearchResultCore p, ProductType pt)
        {
            ProductPageID = p.ProductPageID;
            ProductTypeID = p.ProductTypeID;
            ScorePercent = p.ScorePercent;
            SearchSortOrder = p.SearchSortOrder;
            MerchandisedSortOrder = p.SearchSortOrder;
            AZSortOrder = p.AtoZScore;
            ColourSort = p.ColourSort;
            NewSort = p.NewSort;
            PriceSortOrder = p.PriceSort;
            ProductType = pt;
        }

        internal void ApplyRender(ProductPageFilterablePod patch)
        {
            Heading = patch.Heading ?? Heading;
            SubHeading = patch.SubHeading ?? SubHeading;
            AKAHeading = patch.AKAHeading ?? AKAHeading;
            URLKeywords = patch.URLSemantics ?? patch.URLKeywords ?? URLKeywords;
            URL = patch.URL ?? URL;
            ImageID = patch.ImageID;
            ImageWidth = patch.ImageWidth;
            ImageHeight = patch.ImageHeight;
            AspectRatio = patch.AspectRatio;
            ReviewAverage = patch.ReviewAverage ?? patch.ReviewAverage;
            ReviewCount = patch.ReviewCount ?? ReviewCount;
            New = patch.New;
            ProductTypeID = patch.ProductTypeID;
            ProductPagePrice = patch.ProductPagePrice;
        }
    }

    public class productPagePodPriceWrapper
    {
        public int ProductPageID { get; set; }
        public productPagePodPrice Price { get; set; }

        public productPagePodPriceWrapper(int id, productPagePodPrice objPrice)
        {
            ProductPageID = id;
            Price = objPrice;
        }
    }

    public class productPagePodPrice
    {
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public decimal? SalePriceMin { get; set; }
        public decimal? SalePriceMax { get; set; }
        public decimal? DiscountPercentageMin { get; set; }
        public decimal? DiscountPercentageMax { get; set; }
        public decimal? ClearanceDiscountPercentageMin { get; set; }
        public decimal? ClearanceDiscountPercentageMax { get; set; }

        [JsonIgnore]
        public string PriceMessage { get; set; }

        [JsonIgnore]
        public string RRPMessage { get; set; }

        [JsonIgnore]
        public string DiscountMessage { get; set; }

        [JsonIgnore]
        public string CodeMessage { get; set; }

        [NotMapped]
        public string PriceMessageBase64
        {
            get
            {
                if (!string.IsNullOrEmpty(PriceMessage))
                {
                    return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(PriceMessage));
                }
                else
                {
                    return null;
                }
            }
        }

        [NotMapped]
        public string DiscountMessageBase64
        {
            get
            {
                if (!string.IsNullOrEmpty(DiscountMessage))
                {
                    return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(DiscountMessage));
                }
                else
                {
                    return null;
                }
            }
        }

        [NotMapped]
        public string RRPMessageBase64
        {
            get
            {
                if (!string.IsNullOrEmpty(RRPMessage))
                {
                    return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(RRPMessage));
                }
                else
                {
                    return null;
                }
            }
        }

        [NotMapped]
        public string CodeMessageBase64
        {
            get
            {
                if (!string.IsNullOrEmpty(CodeMessage))
                {
                    return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(CodeMessage));
                }
                else
                {
                    return null;
                }
            }
        }
    }
}