using HtmlAgilityPack;

using MertexerV2.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using StackExchange.Profiling;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

using U2.Data.ClientTypes;

using wwwDesignersGuild.Models;
using wwwDesignersGuild.Models.ArticleSubType;
using wwwDesignersGuild.Models.ArticleType;
using wwwDesignersGuild.Models.Aspect;
using wwwDesignersGuild.Models.Attribute;
using wwwDesignersGuild.Models.Banner;
using wwwDesignersGuild.Models.Cache.ProductPages;
using wwwDesignersGuild.Models.Carousel;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Hotspot;
using wwwDesignersGuild.Models.ImgSize;
using wwwDesignersGuild.Models.LandingPage;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Resource;
using wwwDesignersGuild.Models.ResourceFetch;
using wwwDesignersGuild.Models.Search;
using wwwDesignersGuild.Util;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.ViewModels
{
	public class LandingPageViewModel : BaseViewModel
    {
        [JsonIgnore]
        public Models.Classes.cmsLandingPageDefinition Definition { get; set; }

        [JsonIgnore]
        public List<PageSectionViewModel> PageSections { get; set; }

        [JsonIgnore]
        public SocialMediaShareViewModel SocialMediaViewModel { get; set; }

        [JsonIgnore]
        public long? FirstImageID { get; set; }

        public bool PagingOverride { get; set; }

        public bool IncludesForm
        {
            get
            {
                return PageSections.Where(p => p.PageSectionType == PageSectionType.Form).Any();
            }
        }

        public bool RequiresKendoDatePicker
        {
            get
            {
                if (IncludesForm)
                {
                    var formSourceIds = PageSections.Where(p => p.PageSectionType == PageSectionType.Form)
                                              .Select(p => (int)((FormPageSection)p.Definition).Form.SourceID)
                                              .ToList();

                    // int > byte, in more ways than one
                    var sourceIdsRequiringKendo = new List<int>() { 71, 72 };

                    return formSourceIds.Intersect(sourceIdsRequiringKendo).Any();
                }
                else
                {
                    return PageSections.Where(p =>
                            p.PageSection.PageSectionID == (long)dgFixedPageSection.tradeAddressBook ||
                            p.PageSection.PageSectionID == (long)dgFixedPageSection.tradeRequestAccess
                        ).Any();
                }
            }
        }

        public bool RequiresMDC
        {
            get
            {
                return false; //...
            }
        }

        public bool RequiresFileDownloader
        {
            get
            {
                return PageSections.Where(p =>
                        p.PageSection.PageSectionID == (long)dgFixedPageSection.tradePriceListBrochures
                    ).Any();
            }
        }

        public IHtmlString Breadcrumbs()
        {
            var ti = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;

            //{ti.ToTitleCase(Definition.pageBreadcrumbs[0].ToLowerInvariant())?.Replace("<Li><Span", @"<li class=""breadcrumb-item active""><span").Replace("<Li>", @"<li class=""breadcrumb-item"">")}

            var breadcrumbs = String.Join("", Definition.Breadcrumbs.Select(b =>
            {
                var heading = ti.ToTitleCase(b.Heading.ToLowerInvariant());

                var url = $"/{MasterDefinition.UrlBreakdown.countryDetails.URL}/{b.URLKeywords}/{b.ContentType}{b.ContentID}";

                if (b != Definition.Breadcrumbs.Last())
                    return $@"<li class=""breadcrumb-item""><a href=""{url}"">{heading}</a></li>";
                else
                    return $@"<li class=""breadcrumb-item active""><span>{heading}</span></li>";
            }));

            return new HtmlString($@"<ul class=""breadcrumb"">{breadcrumbs}</ul>");
        }

        public static async Task<LandingPageViewModel> CreateAsync(NewMasterDefinition objMaster, VisitorSelect objVisitor, bool isShop, bool verifiedBot, HttpResponseBase responseBase)
        {
		    var result = new LandingPageViewModel(objMaster, objVisitor);

            await result.Initialise(isShop, responseBase);

		    return result;
        }

        public bool IsShop
        {
            get
            {
                return Definition.PageDetails.IsShop;
            }
        }

        public string OriginalSemanticString { get; set; }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //
        //
        //
        //
        //
        //
        //
        //
        //
        //
        //
        //
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



        private LandingPageViewModel(NewMasterDefinition objMaster, VisitorSelect objVisitor) : base(objMaster, objVisitor)
        {
            PagingOverride = false;
            PageSections = new List<PageSectionViewModel>();
            SocialMediaViewModel = new SocialMediaShareViewModel(directionClass: "dropdown", buttonOnly: false, objBase: this);
            CategoryPageProductType = null;
        }

        protected async Task Initialise(bool isShop, HttpResponseBase responseBase)
        {
#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            await base.Initialise();

			var pager = new PagerViewModel(-1, true, true, null, false, null);

            var queryString = HttpContext.Current.Request.QueryString;

			pager.InitialisePager(queryString);
            
			int? tradeAccountId = null;

			if (HttpContext.Current.Session["memberTradeAccountID"] != null)
			{
				tradeAccountId = Convert.ToInt32(HttpContext.Current.Session["memberTradeAccountID"].ToString());
			}

            var rawUrl = HttpContext.Current.Request.RawUrl;

			Definition = await new cmsLandingPage().loadLandingPageFromCms(
                memberId: Visitor.memberId,
                pageId: MasterDefinition.UrlBreakdown.ContentId,
                languageId: MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
                countryId: MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                useDate: MasterDefinition.UrlBreakdown.ContentDate,
                isMember: Visitor.memberId.HasValue,
                isTrade: MasterDefinition.TradeMember,
                countryMatch: Convert.ToBoolean(HttpContext.Current.Session["countryCheckMatched"]),
                displayTradePrice: Visitor.memberDisplayTradePrice,
                tradeAccountId: tradeAccountId,
                productPageGridPage: pager.Page,
                productPageGridPageSize: pager.PageSize);
                tradeAccountId: Visitor.memberTradeAccountID,
                responseBase: responseBase);

            if (!isShop)
            {
                Definition.PageDetails.IsShop = isShop;
            }

            // Need... to... throw a PageNotFound exception, catch that and redirect to GHP.
            if (Definition.PageDetails == null)
            {
                throw new PageNotFoundException(MasterDefinition.UrlBreakdown.countryDetails.URL);
            }

            ShowRecentlyViewed = Definition.PageDetails.ShowRecentlyViewed;

            var page = Definition.PageDetails;

            MasterDefinition.TitleTag = $"{page.TitleTag} {MasterDefinition.UrlBreakdown.countryDetails.TitleTagSuffix}";

            MasterDefinition.MetaDescription = page.MetaDescription;

            MasterDefinition.GoogleHrefs = Definition.pageGoogleHrefs;

            if (Definition.PageDetails.ProductTypeID != null)
            {
                var objAttribute = new cmsAttribute();

                var attributeData = await objAttribute.loadAttributeFromCms(LanguageID);

                CategoryPageProductType = attributeData.productTypes.FirstOrDefault(pt => pt.ProductTypeID == Definition.PageDetails.ProductTypeID);
            }

            var objDb = new databaseDataContext();

            var allImagesSorted = new List<Tuple<long, int, int>>();

            foreach (var pageSection in Definition.pageSectionSelects.OrderBy(p => p.SortOrder))
            {
                var pageSectionId = pageSection.PageSectionID;

                switch (pageSection.PageSectionTypeID)
                {
                    case (byte)PageSectionType.ProductPageGrid:
                        // 26/05/2026
                        // This ProductPageGrid PageSection must now, server-side, 
                        // determine all applicable attribute groups and attributes so as to be able to render them out.
                        // It would make sense if it is already doing so... let me see what happens when I prevent client-side
                        // viewModel binding happening at all.

#if DEBUG
                        var ppSw = new System.Diagnostics.Stopwatch();
                        System.Diagnostics.Debug.WriteLine($"Initialising productPageGridSection: {pageSection.PageSectionID}");
                        System.Diagnostics.Debug.WriteLine("");
                        ppSw.Start();
#endif

                        // createAsync internally checks a memoryCache object for an extant ProductPageGridPageSection for this key
                        var productPageGridPageSection = await ProductPageGridPageSection.CreateAsync(
                            originalUrlKeywords: Definition.PageDetails.URLKeywords,
                            productPage: Definition.productPageGridSections.Single(p => p.PageSectionID == pageSectionId),
                            pagingEnabled: pageSection.Paging || pageSection.Filter,
                            filterIndexingEnabled: Definition.PageDetails.EnableFilterIndexing,
                            isShop: Definition.PageDetails.IsShop,
                            isLifestyleGallery: pageSection.IsLifestyleGallery,
                            tradeAccountId,
                            queryString,
                            model: this
                        );

                        PageSections.Add(new PageSectionViewModel(
                            pageSection: pageSection,
                            baseViewModel: this,
                            sectionDefinition: (object)productPageGridPageSection,
                            isShop: Definition.PageDetails.IsShop));

                        var ppgps = PageSections.Last().Definition as ProductPageGridPageSection;

                        if (productPageGridPageSection.Contents.Any())
                        {
                            allImagesSorted.Add(new Tuple<long, int, int>(ppgps.GetContents().Skip(0).Take(1).First().ImageID, 0, pageSection.SortOrder ?? 0));
                        }

#if DEBUG
                        ppSw.Stop();
                        System.Diagnostics.Debug.WriteLine($"Initialised productPageGridSection: {pageSection.PageSectionID} in {ppSw.Elapsed}");
#endif

                        break;
                    case (byte)PageSectionType.HTML:
                        var htmlSection = Definition.pageSectionHtmls.Single(p => p.PageSectionID == pageSectionId);

                        allImagesSorted.AddRange(htmlSection.ImageIDs.Select((imageId, index) => new Tuple<long, int, int>(imageId, index, pageSection.SortOrder ?? 0)));

                        PageSections.Add(new PageSectionViewModel(
                            pageSection: pageSection,
                            baseViewModel: this,
                            sectionDefinition: (object)await HtmlPageSection.CreateAsync(
                                htmlSectionId: htmlSection.HTMLSectionID,
                                contents: htmlSection.Contents,
                                imageIds: htmlSection.ImageIDs
                            ),
                            isShop: Definition.PageDetails.IsShop));

                        var doc = new HtmlDocument();

                        doc.LoadHtml(htmlSection.Contents);

                        var imgQuery = doc.DocumentNode.Descendants("img");

                        if (imgQuery.Any())
                        {
                            if (Int64.TryParse(imgQuery.First().Attributes["imageid"]?.Value, out long imageId))
                            {
                                allImagesSorted.Add(new Tuple<long, int, int>(imageId, 0, pageSection.SortOrder ?? 0));
                            }
                        }

                        break;
                    case (byte)PageSectionType.Slideshow:
                        var slideshowImages = Definition.pageSectionSlideShows
                                                  .Where(p => p.PageSectionID == pageSectionId)
                                                  .ToList();

                        allImagesSorted.AddRange(slideshowImages.Select((i, index) => new Tuple<long, int, int>(i.ImageID, index, pageSection.SortOrder ?? 0)));

                        PageSections.Add(new PageSectionViewModel(
                            pageSection: pageSection,
                            baseViewModel: this,
                            sectionDefinition: (object)new SlideshowPageSection(
                                pageSection.PageSectionID,
                                halfWidth: pageSection.HalfWidth,
                                heading: page.Heading,
                                images: slideshowImages,
                                aspect: Definition.pageSectionSlideShowAspects
                                                  .Single(p => p.PageSectionID == pageSectionId),
                                baseViewModel: this
                            ),
                            isShop: Definition.PageDetails.IsShop));
                        break;
                    case (byte)PageSectionType.Form:
                        PageSections.Add(new PageSectionViewModel(
                            pageSection: pageSection,
                            baseViewModel: this,
                            sectionDefinition: (object)new FormPageSection(
                                form: Definition.pageSectionForms.Single(p => p.PageSectionID == pageSectionId),
                                appointmentAgendas: Definition.pageSectionFormRetailAppointmentAgendas,
                                appointmentTypes: Definition.pageSectionFormRetailAppointmentTypes,
                                storeEvents: Definition.pageSectionStoreEvents,
                                countries: Definition.pageFormCountrys,
                                defaultCountryId: MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                                useDate: MasterDefinition.UrlBreakdown.ContentDate
                            ),
                            isShop: Definition.PageDetails.IsShop));
                        break;
                    case (byte)PageSectionType.CollectionGrid:
                        var collections = Definition.pageSectionCollections.Where(p => p.PageSectionID == pageSectionId).ToList();
                        var collectionIds = collections.Select(p => p.CollectionID).ToArray();
                        PageSections.Add(new PageSectionViewModel(
                            pageSection: pageSection,
                            baseViewModel: this,
                            sectionDefinition: (object)new CollectionGridPageSection(
                                collections: collections,
                                images: Definition.pageSectionCollectionImages.Where(p => collectionIds.Contains(p.CollectionID)).ToList(),
                                baseViewModel: this),
                            isShop: Definition.PageDetails.IsShop));

                        var cgps = PageSections.Last().Definition as CollectionGridPageSection;

                        if (cgps.Collections.Any() && cgps.Collections.First().CarouselViewModel.Images.Any())
                        {
                            allImagesSorted.Add(new Tuple<long, int, int>(cgps.Collections.First().CarouselViewModel.Images.First().ImageID, 0, pageSection.SortOrder ?? 0));
                        }

                        break;
                    case (byte)PageSectionType.PageGrid:
                        var pages = Definition.pageSectionPageSelects.Where(p => p.PageSectionID == pageSectionId).ToList();
                        var pageIds = pages.Select(p => p.PageID).ToArray();
                        PageSections.Add(new PageSectionViewModel(
                            pageSection: pageSection,
                            baseViewModel: this,
                            sectionDefinition: (object)new PageGridPageSection(
                                pages: pages,
                                images: Definition.pageSectionPageSelectImages
                                                  .Where(p => pageIds.Contains(p.PageID)).ToList(),
                                baseViewModel: this),
                            isShop: Definition.PageDetails.IsShop));

                        var pgps = PageSections.Last().Definition as PageGridPageSection;

                        if (pgps.Pages.Any() && pgps.Pages.First().CarouselViewModel.Images.Any())
                        {
                            allImagesSorted.Add(new Tuple<long, int, int>(pgps.Pages.First().CarouselViewModel.Images.First().ImageID, 0, pageSection.SortOrder ?? 0));
                        }
                        break;
                    case (byte)PageSectionType.Brochure:
                        PageSections.Add(new PageSectionViewModel(
                            pageSection: pageSection,
                            baseViewModel: this,
                            sectionDefinition: (object)new BrochurePageSection(
                                brochure: Definition.pageSectionBrochures.Single(p => p.PageSectionID == pageSectionId)
                            ),
                            isShop: Definition.PageDetails.IsShop));
                        break;
                    case (byte)PageSectionType.BannerLocationGroup:
                        var bannerLocationGroup = JObject.FromObject(Definition.pageSectionBanners.json)
                                                         .SelectToken("pageSection")
                                                         .Children()
                                                         .Where(p => p["id"].Value<long>() == pageSectionId)
                                                         .Select(p => p.SelectToken("bannerLocationGroups"))
                                                         .Single()
                                                         .ToObject<BannerLocationGroup>();
                        if (bannerLocationGroup.BannerLocations.All(bl => bl.Banners != null && bl.Banners.Any()))
                        {
                            PageSections.Add(new PageSectionViewModel(
                                pageSection: pageSection,
                                baseViewModel: this,
                                sectionDefinition: (object)new BannerGridPageSection(
                                    bannerGrid: bannerLocationGroup
                                ),
                                isShop: Definition.PageDetails.IsShop));
                        }
                        break;
                    case (byte)PageSectionType.ArticleGrid:
                        var articles = Definition.pageSectionArticles.Where(p => p.PageSectionID == pageSectionId).ToList();
                        var articleIds = articles.Select(p => p.ArticleID).ToArray();
                        pageSection.Paging = true;
                        PageSections.Add(new PageSectionViewModel(
                            pageSection: pageSection,
                            baseViewModel: this,
                            sectionDefinition: (object)await ArticleGridPageSection.CreateAsync(
                                pageSectionId: pageSection.PageSectionID,
                                pagingEnabled: pageSection.Paging,
                                isShop: Definition.PageDetails.IsShop,
                                articles: articles,
                                images: Definition.pageSectionArticleImages
                                                  .Where(p => articleIds.Contains(p.ArticleID))
                                                  .ToList(),
                                baseViewModel: this
                            ),
                            isShop: Definition.PageDetails.IsShop,
                            pagingOverride: pageSection.Paging));

                        var agps = PageSections.Last().Definition as ArticleGridPageSection;

                        if (agps.Articles.Any() && agps.Articles.First().CarouselViewModel.Images.Any())
                        {
                            allImagesSorted.Add(new Tuple<long, int, int>(agps.Articles.First().CarouselViewModel.Images.First().ImageID, 0, pageSection.SortOrder ?? 0));
                        }
                        break;
                    case (byte)PageSectionType.Video:
                        PageSections.Add(new PageSectionViewModel(
                            pageSection: pageSection,
                            baseViewModel: this,
                            sectionDefinition: (object)new VideoPageSection(
                                video: Definition.pageSectionVideos.Single(p => p.PageSectionID == pageSectionId)
                            ),
                            isShop: Definition.PageDetails.IsShop));
                        break;
                    case (byte)PageSectionType.Masonry:
                        throw new Exception($"Masonry (PageSectionTypeID 11) is defunct, sorry.");
                    case (byte)PageSectionType.Animation:
                        var animation = Definition.pageSectionAnimations.Where(p => p.PageSectionID == pageSectionId).Single();

                        var animationImages = Definition.pageSectionAnimationSlides.Where(p => p.AnimationID == animation.AnimationID).ToList();

                        allImagesSorted.AddRange(animationImages.Select((i, index) => new Tuple<long, int, int>(i.ImageID, index, pageSection.SortOrder ?? 0)));

                        PageSections.Add(new PageSectionViewModel(
                            pageSection: pageSection,
                            baseViewModel: this,
                            sectionDefinition: (object)new AnimationPageSection(
                                pageHeading: page.Heading,
                                pageSubHeading: page.SubHeading,
                                pageDescription: page.Description,
                                pageDiscountMessage: page.DiscountMessage,
                                isHeader: pageSection.IsHeader,
                                animation: animation,
                                images: animationImages,
                                callsToAction: Definition.pageSectionAnimationSlideCallsToAction.Where(p => p.AnimationID == animation.AnimationID).ToList()
                            ),
                            isShop: Definition.PageDetails.IsShop));
                        break;
                    case (byte)PageSectionType.BespokeContent:
                        PageSections.Add(new PageSectionViewModel(
                            pageSection: pageSection,
                            baseViewModel: this,
                            sectionDefinition: (object)new BespokePageSection(),
                            isShop: Definition.PageDetails.IsShop));
                        break;
                    default:
                        throw new Exception($"Unknown PageSectionTypeID: {pageSection.PageSectionTypeID}");
                }
            }

            if (allImagesSorted.Any())
            {
                FirstImageID = allImagesSorted.OrderBy(p => p.Item3).ThenBy(p => p.Item2).First().Item1;
            }

            var imageIds = new List<long>();

            PageSections.ForEach(ps =>
            {
                if (ps.Definition is SlideshowPageSection c)
                {
                    imageIds.AddRange(c.CarouselViewModel.Images.Select(i => i.ImageID));
                    c.CarouselViewModel.Images.ForEach(i =>
                    {
                        i.Hotspots = new List<Hotspot>();
                    });
                }

                if (ps.Definition is AnimationPageSection aps)
                {
                    imageIds.AddRange(aps.Images.Select(i => i.ImageID));
                    aps.Images.ForEach(i =>
                    {
                        i.Hotspots = new List<Models.Hotspot.Hotspot>();
                    });
                }

                if (ps.Definition is HtmlPageSection hps)
                {
                    imageIds.AddRange(hps.ImageIDs);
                    hps.Hotspots = new List<ImageHotspot>();
                }
            });

            if (imageIds.Any())
            {
                await base.InitialiseHotspots(imageIds, this.MasterDefinition.UrlBreakdown.CountryAwareness.Match);

                PageSections.ForEach(ps =>
                {
                    if (ps.Definition is SlideshowPageSection c)
                    {
                        foreach (var i in c.CarouselViewModel.Images)
                        {
                            if (Hotspots.SingleOrDefault(h => h.ImageID == i.ImageID)?.Hotspots != null)
                                i.Hotspots = Hotspots.SingleOrDefault(h => h.ImageID == i.ImageID)?.Hotspots;
                        }
                    }

                    if (ps.Definition is AnimationPageSection aps)
                    {
                        foreach (var i in aps.Images)
                        {
                            if (Hotspots.SingleOrDefault(h => h.ImageID == i.ImageID)?.Hotspots != null)
                                i.Hotspots = Hotspots.SingleOrDefault(h => h.ImageID == i.ImageID)?.Hotspots;
                        }
                    }

                    if (ps.Definition is HtmlPageSection hps)
                    {
                        foreach (var i in hps.ImageIDs)
                        {
                            if (Hotspots.SingleOrDefault(h => h.ImageID == i)?.Hotspots != null)
                                hps.Hotspots.AddRange(Hotspots.Where(h => h.ImageID == i));
                        }
                    }
                });
            }

            if (PageSections.Where(ps => ps.Definition is ProductPageGridPageSection ppgps)
                            .Any(ps => (ps.Definition as ProductPageGridPageSection).MustOverwritePageCanonical) &&
                            String.IsNullOrEmpty(Definition.PageDetails.CustomCanonical))
            {
                CustomCanonical = $"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Authority}{dgFramework.buildUrl(MasterDefinition.UrlBreakdown.countryDetails, Definition.PageDetails.URLKeywords, ContentType, ContentID)}";
            }
            else
            {
                CustomCanonical = Definition.PageDetails.CustomCanonical;
            }

            NoFollow = Definition.PageDetails.NoFollow;

            NoIndex = Definition.PageDetails.NoIndex;

            PrefetchResources();

#if DEBUG
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("");
            System.Diagnostics.Debug.WriteLine($"LandingPageViewModel initialised in {sw.Elapsed}");
			System.Diagnostics.Debug.WriteLine("");
#endif
		}

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public void PrefetchResources()
        {
            var resourceDefinition = new cmsResource().loadResourceFromCms(
                    LanguageID,
                    CountryID,
                    Date).Result;

            var result = new List<ResourceResult>();

            var required = new List<string>() {
                "ShowImageProducts",
                "HideImageProducts",
                "Sorry",
                "ThereWereNoResultsForThatCombination",
                "WeHaveDeselectedTheFollowing",
                "AttributeFilterTextResultFoundFor",
                "AttributeFilterTextResultsFoundFor",
                "MajorError" };

            switch (ContentID)
            {
                case (int)dgFixedPage.serverSearch:
                    required.AddRange(new string[] {
                            "ShowImageProducts",
                            "HideImageProducts",
                            "Sorry",
                            "ThereWereNoResultsForThatCombination",
                            "WeHaveDeselectedTheFollowing"
                        });
                    break;
                case (int)dgFixedPage.memberLogin:
                    required.AddRange(new string[] {
                    "FormsErrorEmailFormat",
                    "nameValidation",
                    "emailValidation",
                    "PleaseProvideYourPassword",
                    "ManageAccountErrorPassword",
                    "ManageAccountPasswordStrength0",
                    "ManageAccountPasswordStrength1",
                    "ManageAccountPasswordStrength2",
                    "ManageAccountPasswordStrength3",
                    "ManageAccountPasswordStrength4",
                    "FormsErrorEmailCompare",
                    "PleaseProvideTradeAccountNumber",
                    "TradeLoginPleaseCheckYourEmail",
                    "TradeLoginIfProblemsPersist",
                    "RegistrationDuplicateText",
                    "LoginInvalid",
                    "PaypalInvalidAddress",
                    "PayWithCard",
                    "CheckoutTradeCheckout"
                    });
                    break;
                case (int)dgFixedPage.memberAddress:
                    required.AddRange(new string[] {
                        "AreYouSure",
                        "AddressSearch",
                        "SearchYourAddressBook",
                        "MoodboardSave",
                        "ThisWillMakeDeliveryReference",
                        "TheDefaultReplacing",
                        "TheDefaultOnYourAccount",
                        "ThisWillDeleteDeliveryReference",
                        "PleaseProvideCompanyName",
                        "FormsCompanyName",
                        "PleaseProvideName",
                        "PleaseProvideAddressLine1",
                        "FormsAddressLineTwo",
                        "PleaseProvideTown",
                        "PleaseProvidePostcode",
                        "PleaseSelectCountry",
                        "PleaseSelectCountryBefore",
                        "FormsCompanyName",
                        "FormsFullName",
                        "FormsAddressLineOne",
                        "FormsAddressLineTwo",
                        "FormsTown",
                        "FormsCounty",
                        "FormsPostCode",
                        "FormsPhoneNumber",
                        "email",
                        "SearchForAddress",
                        "FormsCountry",
                        "ThisWillDeleteThisDeliveryAddress",
                        "ThisWillChangeTheDefaultDelivery",
                    });
                    break;
                case (int)dgFixedPage.memberEdit:
                    required.AddRange(new string[] {
                         "ManageAccountDetailsEdit",
                        "Save",
                        "MoodboardSave",
                        "EditNegative",
                        "ManageAccountErrorName",
                        "ManageAccountErrorEmail",
                        "ManageAccountErrorEmailCompare",
                        "ManageAccountErrorPassword",
                        "ManageTradeAccount_Edit",
                        "ManageAccountErrorCountry",
                        "ManageAccountDuplicateEmail",
                        "RegistrationDuplicateText",
                        "ManageAccountErrorPasswordCompare",
                        "ManageAccountErrorPasswordInvalid",
                        "ManageAccountPasswordStrength0",
                        "ManageAccountPasswordStrength1",
                        "ManageAccountPasswordStrength2",
                        "ManageAccountPasswordStrength3",
                        "ManageAccountPasswordStrength4",
                        "ManageAccountPasswordNotStrongEnough",
                        "ManageAccountErrorEmailFormat",
                        "ManageAccountChangedName",
                        "ManageAccountChangedEmail",
                        "ManageAccountChangedPassword",
                        "ManageAccountChangedCountry",
                        "FormsCountryRequired",
                        "FormsLanguageRequired",
                        "ManageAccountSaved",
                        "ManageAccountSaveFailed",
                        "ManageTradeAccount_Edit",
                        "AreYouSure",
                        "Cancel",
                    });
                    break;
                case (int)dgFixedPage.returnsPaperwork:
                case (int)dgFixedPage.memberOrderHistory:
                    required.AddRange(new string[] { "AreYouSure",
                        "AddressSearch",
                        "SearchYourAddressBook",
                        "MoodboardSave",
                        "PleaseProvideCompanyName",
                        "FormsCompanyName",
                        "PleaseProvideName",
                        "PleaseProvideAddressLine1",
                        "FormsAddressLineTwo",
                        "PleaseProvideTown",
                        "PleaseProvidePostcode",
                        "PleaseSelectCountry",
                        "PleaseSelectCountryBefore",
                        "FormsCompanyName",
                        "FormsFullName",
                        "FormsAddressLineOne",
                        "FormsAddressLineTwo",
                        "FormsTown",
                        "FormsCounty",
                        "FormsPostCode",
                        "FormsPhoneNumber",
                        "Parcel",
                        "Parcels",
                        "orderHistoryReturnQuantityError",
                        "NotApplicable",
                        "orderHistoryPreviousReturns",
                        "pleaseGreaterThanEqualTo",
                        "pleaseLessThanEqualTo",
                        "orderHistoryReturnReasonError",
                        "Sorry",
                        "orderHistoryNotReturningAnything",
                        "Info",
                        "orderHistoryReturnGenerated",
                        "pleaseGreaterThanEqualTo",
                        "pleaseLessThanEqualTo"
                    });
                    break;
                case (int)dgFixedPage.maintainBasket:
                    required.AddRange(new string[] {
                        "AreYouSure",
                        "ConfirmBasketItemDelete",
                        "QuantityRequired",
                        "ErrorQuantity",
                        "PromotionCode",
                        "PromotionCodeAppliedSuccessfully",
                        "BasketProductCode",
                        "RemoveAll"
                    });
                    break;
                case (int)dgFixedPage.checkoutLogin:
                    required.AddRange(new string[] {
                        "FormsErrorEmailFormat",
                        "nameValidation",
                        "emailValidation",
                        "PleaseProvideYourPassword",
                        "ManageAccountErrorPassword",
                        "ManageAccountPasswordStrength0",
                        "ManageAccountPasswordStrength1",
                        "ManageAccountPasswordStrength2",
                        "ManageAccountPasswordStrength3",
                        "ManageAccountPasswordStrength4",
                        "FormsErrorEmailCompare",
                        "RegistrationDuplicateText",
                        "LoginInvalid",
                        "PaypalInvalidAddress",
                        "PayWithCard",
                        "CheckoutTradeCheckout",
                        "Sorry",
                        "CheckoutExpired",
                        "CheckoutLoginInvalidDeliveryAddress",
                        "CheckoutLoginInvalidDeliveryAddress2",
                        "CheckoutLoginIncorrectSiteCulture",
                        "FormsErrorEmailFormat",
                        "nameValidation",
                        "emailValidation",
                        "PleaseProvideYourPassword",
                        "ManageAccountErrorPassword",
                        "ManageAccountPasswordStrength0",
                        "ManageAccountPasswordStrength1",
                        "ManageAccountPasswordStrength2",
                        "ManageAccountPasswordStrength3",
                        "ManageAccountPasswordStrength4",
                        "FormsErrorEmailCompare",
                        "RegistrationDuplicateText",
                        "LoginInvalid",
                        "PaypalInvalidAddress",
                        "PayWithCard",
                        "CheckoutTradeCheckout",
                        "CheckoutPaymentSuccessGeneratingOrder",
                        "ApplePayFailedToLoad",
                        "ApplePayFailureNotCharged",
                        "CheckoutAddressZipCodeInvalid",
                        "CheckoutAddressZipCodeProblem",
                        "ZipTaxCouldNotDetermine",
                        "CannotDeliverToThatCountry"
                    });
                    break;
                case (int)dgFixedPage.checkoutSinglePage:
                    required.AddRange(new string[] {
                        "ThankYou",
                        "CustomerServiceApplyTreatments1",
                        "TreatmentChargesDocument",
                        "TreatmentChargesDocumentURL",
                        "CustomerServiceApplyTreatments2",
                        "AvailableBatches",
                        "AreYouSure",
                        "ManageAddressBookConfirmDelete",
                        "CheckoutGiftUnusedGiftBoxMessage",
                        "CheckoutGiftNoChargeMessage",
                        "CheckoutPayGiftRemove",
                        "Quantity",
                        "CheckoutGiftWouldYouLike",
                        "CheckoutGiftPerBox",
                        "CheckoutGiftGiftMessage",
                        "addressErrorName",
                        "addressErrorCompanyName",
                        "addressErrorLineOne",
                        "addressErrorLineTwo",
                        "addressErrorTown",
                        "addressErrorCounty",
                        "addressErrorPostCode",
                        "addressErrorPhone",
                        "phoneValidation",
                        "addressErrorPhoneRequired",
                        "mobileNumberInvalid",
                        "FindAddressErrorPostcodeRequired",
                        "FindAddressErrorSearchAddressRequired",
                        "FindAddressSelectAnAddress",
                        "FindAddressSearchForAddress",
                        "CheckoutAddressErrorSelectSalesTaxAddress",
                        "CheckoutAddressSelectIdentificationAddress",
                        "CheckoutAddressSelectSalesTaxAddress",
                        "CheckoutAddressHomeDeliverTo",
                        "CheckoutAddressCollectDeliver",
                        "CheckoutAddressUpsDeliver",
                        "DispatchDelivery",
                        "DispatchOf",
                        "MajorError",
                        "currencySymbol",
                        "FindAddressErrorSelectAddress",
                        "CheckoutPaySelectBillingAddress",
                        "VoucherCodeInvalid",
                        "VoucherCodeDuplicate",
                        "VoucherCodeLengthMessage",
                        "CheckoutBraintree3DSecureMessage",
                        "CheckoutBraintreePaymentDeclined",
                        "CheckoutBraintreePaymentDeclined",
                        "CheckoutCardEnding",
                        "CheckoutCardExpires",
                        "Characters",
                        "CheckoutGiftVoucherLines",
                        "CheckoutGiftGiftBox",
                        "CheckoutGiftGiftMessage",
                        "CheckoutGiftCharsLeft",
                        "CheckoutGiftLinesLeft",
                        "CheckoutGiftCharLimitExceeded",
                        "CheckoutGiftVoucherCharLimitExceeded",
                        "CheckoutGiftGiftBoxes",
                        "CheckoutGiftTotalCost",
                        "CheckoutCardExpired",
                        "CheckoutErrorNoAddress",
                        "CheckoutErrorNoIdentificationAddress",
                        "CheckoutErrorInvalidAddress",
                        "CheckoutErrorNoPaymentMethod",
                        "CheckoutErrorMissingDispatchMethod",
                        "PaypalPleaseProvideAddress",
                        "BraintreeAcceptedCards",
                        "FormsPostCode",
                        "CheckoutBraintreeCardNumber",
                        "CheckoutBraintreeCVV",
                        "CheckoutBraintreeExpiry",
                        "CheckoutBraintreeMM",
                        "CheckoutBraintreeYYYY",
                        "CheckoutPaymentSuccessGeneratingOrder",
                        "CheckoutTradeSuccessGeneratingOrder",
                        "VoucherCodeLengthMessage",
                        "PaypalInvalidAddress",
                        "AmazonPayWalletWidgetError",
                        "AmazonPayAddressWidgetError",
                        "AmazonPayConstraintsExist",
                        "AmazonPayInvalidPaymentMethod",
                        "AmazonPayRejected",
                        "AmazonPayProcessingFailure",
                        "AmazonPayGenericError",
                        "UseThisDeliveryAddress",
                        "UseThisIdentificationAddress",
                        "CheckoutAddressSelectIdentificationAddress",
                        "CheckoutErrorCVV",
                        "Tel",
                        "MoodboardSave",
                        "ManageAccountDetailsEdit",
                        "CutPieceBreakdown",
                        "AreYouSure",
                        "UnsavedChangesWillBeLost",
                        "AreYouSureRemoveCutLengthBreakdown",
                        "OrderReferenceRequired",
                        "TradeCheckoutSpecialInstructions",
                        "TradeCheckoutLineReference",
                        "POBoxDetected",
                        "FormsPostCode",
                        "LiveChatNow",
                        "ThisWillRemoveCardFromVault",
                        "StockDetailNotAvailable",
                        "Message",
                        "ApplePayDeviceNotSupported",
                        "ApplePayDeviceNotSupported",
                        "ApplePayDeviceCannotPay",
                        "ApplePayFailureNotCharged"
                    });
                    break;
                case (int)dgFixedPage.stockists:
                    required.AddRange(new string[] {
                        "PleaseSelectCountryBefore",
                        "SearchResult",
                        "SearchResults",
                        "SearchNoResults",
                        "PleaseWait",
                        "PleaseProvideSearchTerm",
                        "Sorry",
                        "GoogleMapsCouldNotDetermine",
                        "BrowserCouldNotDetermine",
                        "LoadingStockistDetail",
                        "Miles",
                        "Mile"
                    });
                    break;
                case (int)dgFixedPage.tradeAccountRequest:
                    required.AddRange(new string[] {
                        "FormsCompanyName",
                        "PleaseSelect",
                        "companyNameValidation",
                        "nameValidation",
                        "phoneValidation",
                        "emailValidation",
                        "emailValidationFormat",
                        "addressLine1Validation",
                        "addressTownValidation",
                        "addressPostcodeValidation",
                        "FormsErrorCaptchaCompare",
                        "PleaseProvideCompanyType",
                        "PleaseProvideTradingFor",
                        "PleaseProvideTitle",
                        "PleaseSpecifyAtLeastOne",
                        "PleaseProvideBusinessType",
                        "PleaseProvideYourPosition",
                        "PleaseProvideCompanyRegistration",
                        "PleaseProvideVATNumber",
                        "PleaseProvideYourEORI",
                        "BusinessTypeInteriorDesigner",
                        "BusinessTypeRetailStore",
                        "BusinessTypeOther",
                        "PleaseProvideNotes",
                        "YouMustConsent",
                        "WebsiteRequired",
                        "PleaseSpecify",
                        "Title",
                        "CompanyType",
                        "BusinessType",
                        "HowLongTradingFor",
                        "AreYouSure",
                        "AddressSearch",
                        "SearchYourAddressBook",
                        "MoodboardSave",
                        "ThisWillMakeDeliveryReference",
                        "TheDefaultReplacing",
                        "TheDefaultOnYourAccount",
                        "ThisWillDeleteDeliveryReference",
                        "CompanyRegistrationNumber",
                        "PleaseProvideCompanyName",
                        "FormsCompanyName",
                        "PleaseProvideName",
                        "PleaseProvideAddressLine1",
                        "FormsAddressLineTwo",
                        "PleaseProvideTown",
                        "PleaseProvidePostcode",
                        "PleaseSelectCountry",
                        "PleaseSelectCountryBefore",
                        "FormsCompanyName",
                        "AddAddressBookFullName",
                        "FormsAddressLineOne",
                        "FormsAddressLineTwo",
                        "FormsTown",
                        "FormsCounty",
                        "FormsPostCode",
                        "FormsPhoneNumber",
                        "email",
                        "SearchForAddress",
                        "FormsCountry",
                        "AreYouVATRegistered",
                        "VATNumber",
                        "VATCountry",
                        "VATInvoiceFormat",
                        "DoYouHaveEORI",
                        "EORINumber",
                        "SellOnline",
                        "Website",
                        "TradeAccountYourPosition",
                        "FormsFullName",
                        "PrimaryEmailAddress",
                        "SameAsPrimary",
                        "OrderConfirmations",
                        "InvoicesStatements",
                        "ProductsNewsEvents",
                        "Pricelists",
                        "DeliveryNotifications",
                        "DeliverySameAsInvoiceAddress",
                        "Notes",
                        "WhyWorkWithDG",
                        "TARFIUnderstand",
                        "TaxID",
                        "PleaseProvideTaxID",
                        "WhichLanguageForAutomatedCorrespondence",
                        "PleaseSelectLanguage",
                        "USSalesTaxExemptionCertificationRequired",
                        "USSalesTaxExemptionCertification",
                        "PleaseProvideExemptionCertificate",
                        "MajorError"
                    });
                    break;
                case (int)dgFixedPage.tradeAddressBook:
                    required.AddRange(new string[] {
                        "AreYouSure",
                        "AddressSearch",
                        "SearchYourAddressBook",
                        "MoodboardSave",
                        "ThisWillMakeDeliveryReference",
                        "TheDefaultReplacing",
                        "TheDefaultOnYourAccount",
                        "ThisWillDeleteDeliveryReference",
                        "PleaseProvideCompanyName",
                        "FormsCompanyName",
                        "PleaseProvideName",
                        "PleaseProvideAddressLine1",
                        "FormsAddressLineTwo",
                        "PleaseProvideTown",
                        "PleaseProvidePostcode",
                        "PleaseSelectCountry",
                        "PleaseSelectCountryBefore",
                        "FormsCompanyName",
                        "FormsFullName",
                        "FormsAddressLineOne",
                        "FormsAddressLineTwo",
                        "FormsTown",
                        "FormsCounty",
                        "FormsPostCode",
                        "FormsPhoneNumber",
                        "email",
                        "SearchForAddress",
                        "ThisWillChangeTheDefaultDelivery",
                        "FormsCountry"
                    });
                    break;
                case (int)dgFixedPage.tradeAccountContactDetails:
                    required.AddRange(new string[] {
                        "ManageAccountTradeEmailOrderConfirmations",
                        "InvoicesStatements",
                        "ManageAccountTradeEmailProductNews",
                        "ManageAccountTradeEmailPricelists",
                        "ManageAccountDetailsEdit",
                        "Save",
                        "EditNegative",
                        "ManageAccountChangedEmail",
                        "ManageAccountErrorEmail",
                        "FormsErrorEmailFormat",
                        "ManageTradeAccountErrorOrderConfirmations",
                        "ManageAccountSaveFailed"
                    });
                    break;
                case (int)dgFixedPage.tradeAccountInvoices:
                    required.AddRange(new string[] {
                        "Last12Months",
                        "Last6Months",
                        "ThisMonth",
                        "Custom",
                        "ShipmentDate",
                        "InvoiceDate",
                        "OrderDate",
                        "StatusDate",
                        "DespatchDate",
                        "ExpiryDate",
                        "DueDate",
                        "FromMustBeDate",
                        "PleaseProvideFromDate",
                        "PleaseProvideToDate",
                        "PleaseProvideFromToOrBoth",
                        "ToMustBeDate",
                        "FromMustPreceedTo",
                        "ToMustFollowFrom",
                        "OrderNo.",
                        "YourOrderReference",
                        "CustOrderRef.",
                        "Orig.Value",
                        "Value",
                        "Status",
                        "Total",
                        "Totals",
                        "Disputed",
                        "OutstandingAmount",
                        "Outstanding",
                        "OnAccount",
                        "Released",
                        "NoRecordsToDisplay",
                        "Quantity",
                        "Product",
                        "Batch",
                        "Description",
                        "YourReference",
                        "Tracking",
                        "InvoiceNo.",
                        "LineNo.",
                        "BatchNo.",
                        "Despatch",
                        "Track",
                        "PaymentReference",
                        "PaymentDate",
                        "OriginalValue",
                        "KendoPagerDisplay",
                        "KendoPagerEmpty",
                        "KendoPagerPage",
                        "KendoPagerOf",
                        "KendoPagerItemsPerPage",
                        "KendoPagerFirstPage",
                        "KendoPagerLastPage",
                        "KendoPagerNextPage",
                        "KendoPagerPreviousPage",
                        "KendoPagerRefresh",
                        "KendoPagerMorePages",
                    });
                    break;
                case (int)dgFixedPage.tradeOrderStatus:
                    required.AddRange(new string[] {
                        "Last12Months",
                        "Last6Months",
                        "ThisMonth",
                        "Custom",
                        "ShipmentDate",
                        "InvoiceDate",
                        "OrderDate",
                        "StatusDate",
                        "DespatchDate",
                        "ExpiryDate",
                        "DueDate",
                        "FromMustBeDate",
                        "PleaseProvideFromDate",
                        "PleaseProvideToDate",
                        "PleaseProvideFromToOrBoth",
                        "ToMustBeDate",
                        "FromMustPreceedTo",
                        "ToMustFollowFrom",
                        "OrderNo.",
                        "YourOrderReference",
                        "CustOrderRef.",
                        "Orig.Value",
                        "Value",
                        "Status",
                        "Total",
                        "Totals",
                        "Disputed",
                        "OutstandingAmount",
                        "Outstanding",
                        "OnAccount",
                        "Released",
                        "NoRecordsToDisplay",
                        "Quantity",
                        "Product",
                        "Batch",
                        "Description",
                        "YourReference",
                        "Tracking",
                        "InvoiceNo.",
                        "LineNo.",
                        "BatchNo.",
                        "Despatch",
                        "Track",
                        "PaymentReference",
                        "PaymentDate",
                        "OriginalValue",
                        "KendoPagerDisplay",
                        "KendoPagerEmpty",
                        "KendoPagerPage",
                        "KendoPagerOf",
                        "KendoPagerItemsPerPage",
                        "KendoPagerFirstPage",
                        "KendoPagerLastPage",
                        "KendoPagerNextPage",
                        "KendoPagerPreviousPage",
                        "KendoPagerRefresh",
                        "KendoPagerMorePages",
                        "IncludeSamples"
                    });
                    break;
                case (int)dgFixedPage.tradeProformaInvoices:
                    required.AddRange(new string[] {
                        "Last12Months",
                        "Last6Months",
                        "ThisMonth",
                        "Custom",
                        "ShipmentDate",
                        "InvoiceDate",
                        "DueDate",
                        "FromMustBeDate",
                        "ToMustBeDate",
                        "PleaseProvideFromDate",
                        "PleaseProvideToDate",
                        "FromMustPreceedTo",
                        "ToMustFollowFrom",
                        "OrderNo.",
                        "YourOrderReference",
                        "Value",
                        "Status",
                        "Total",
                        "Outstanding",
                        "Released",
                        "NoRecordsToDisplay",
                        "KendoPagerDisplay",
                        "KendoPagerEmpty",
                        "KendoPagerPage",
                        "KendoPagerOf",
                        "KendoPagerItemsPerPage",
                        "KendoPagerFirstPage",
                        "KendoPagerLastPage",
                        "KendoPagerNextPage",
                        "KendoPagerPreviousPage",
                        "KendoPagerRefresh",
                        "KendoPagerMorePages",
                    });
                    break;
                case (int)dgFixedPage.tradeReserveOrders:
                    required.AddRange(new string[] {
                        "AreYouSure",
                        "AreYouSureChangeDelivery",
                        "AreYouSureReleaseAllLines",
                        "AreYouSureCancelThisLine",
                        "AreYouSureReleaseThisLine",
                        "AddressSearch",
                        "SearchYourAddressBook",
                        "MoodboardSave",
                        "ThisWillMakeDeliveryReference",
                        "TheDefaultReplacing",
                        "TheDefaultOnYourAccount",
                        "ThisWillDeleteDeliveryReference",
                        "PleaseProvideCompanyName",
                        "FormsCompanyName",
                        "PleaseProvideName",
                        "PleaseProvideAddressLine1",
                        "FormsAddressLineTwo",
                        "PleaseProvideTown",
                        "PleaseProvidePostcode",
                        "PleaseSelectCountry",
                        "PleaseSelectCountryBefore",
                        "FormsCompanyName",
                        "FormsFullName",
                        "FormsAddressLineOne",
                        "FormsAddressLineTwo",
                        "FormsTown",
                        "FormsCounty",
                        "FormsPostCode",
                        "FormsPhoneNumber",
                        "email",
                        "SearchForAddress",
                        "FormsCountry",
                        "CheckoutChangeDeliveryAddress",
                        "Release",
                        "Modify",
                        "ReleaseThisOrder",
                        "OrderReleased",
                        "ThankYouOrderReleased",
                        "OrderLineReleased",
                        "ReserveLineCancelledTitle",
                        "ReserveLineCancelled",
                        "ThankYouOrderLineReleased",
                        "Quantity",
                        "LineReference",
                        "QuantityRequired",
                        "LineReferenceRequired",
                        "SaveBeforeRelease",
                        "ReleaseThisLine",
                        "LineMemo",
                        "Cancel",
                        "Last12Months",
                        "Last6Months",
                        "ThisMonth",
                        "Custom",
                        "ShipmentDate",
                        "InvoiceDate",
                        "OrderDate",
                        "StatusDate",
                        "Date",
                        "DespatchDate",
                        "ExpiryDate",
                        "DueDate",
                        "FromMustBeDate",
                        "PleaseProvideFromDate",
                        "PleaseProvideToDate",
                        "PleaseProvideFromToOrBoth",
                        "ToMustBeDate",
                        "FromMustPreceedTo",
                        "ToMustFollowFrom",
                        "OrderNo.",
                        "YourOrderReference",
                        "CustOrderRef.",
                        "Orig.Value",
                        "Value",
                        "Status",
                        "Total",
                        "Totals",
                        "Disputed",
                        "OutstandingAmount",
                        "Outstanding",
                        "OnAccount",
                        "Released",
                        "NoRecordsToDisplay",
                        "Product",
                        "Batch",
                        "Description",
                        "YourReference",
                        "DeliveryReference",
                        "Tracking",
                        "InvoiceNo.",
                        "LineNo.",
                        "BatchNo.",
                        "Despatch",
                        "ReservedUntil",
                        "Track",
                        "Memo",
                        "PaymentReference",
                        "PaymentDate",
                        "OriginalValue",
                        "RequestSubmitted",
                        "BatchRequestSubmitted",
                        "Actions",
                        "YouMayNotChangeCountryManually1",
                        "YouMayNotChangeCountryManually2",
                        "ThankYou",
                        "AddressChangeRequestSubmitted",
                        "ReserveDateError",
                        "ReserveQuantityChangeRequested",
                        "ReserveQuantityPleaseWaitForCustomerServices",
                        "ReserveOrderAddressChangedText",
                    });
                    break;
                case (int)dgFixedPage.tradeLoginDetails:
                    required.AddRange(new string[] {
                        "ManageAccountDetailsEdit",
                        "Save",
                        "MoodboardSave",
                        "EditNegative",
                        "ManageAccountErrorName",
                        "ManageAccountErrorEmail",
                        "ManageAccountErrorEmailCompare",
                        "ManageAccountErrorPassword",
                        "ManageTradeAccount_Edit",
                        "ManageAccountErrorCountry",
                        "ManageAccountDuplicateEmail",
                        "RegistrationDuplicateText",
                        "ManageAccountErrorPasswordCompare",
                        "ManageAccountErrorPasswordInvalid",
                        "ManageAccountPasswordStrength0",
                        "ManageAccountPasswordStrength1",
                        "ManageAccountPasswordStrength2",
                        "ManageAccountPasswordStrength3",
                        "ManageAccountPasswordStrength4",
                        "ManageAccountPasswordNotStrongEnough",
                        "ManageAccountErrorEmailFormat",
                        "ManageAccountChangedName",
                        "ManageAccountChangedEmail",
                        "ManageAccountChangedPassword",
                        "ManageAccountChangedCountry",
                        "FormsCountryRequired",
                        "FormsLanguageRequired",
                        "ManageAccountSaved",
                        "ManageAccountSaveFailed",
                        "AreYouSure",
                        "Cancel",
                    });
                    break;
                case (int)dgFixedPage.tradeRegistration:
                    required.AddRange(new string[] {
                        "ManageAccountErrorEmailFormat",
                        "PasswordResetRequestEmailNotFound",
                        "RegistrationErrorName",
                        "RegistrationErrorEmail",
                        "RegistrationErrorEmailFormat",
                        "RegistrationErrorEmailCompare",
                        "RegistrationErrorPassword",
                        "RegistrationErrorPasswordCompare",
                        "ManageAccountPasswordNotStrongEnough",
                        "ManageAccountErrorPassword",
                        "ManageAccountErrorPasswordInvalid",
                        "ManageAccountPasswordStrength0",
                        "ManageAccountPasswordStrength1",
                        "ManageAccountPasswordStrength2",
                        "ManageAccountPasswordStrength3",
                        "ManageAccountPasswordStrength4",
                        "TradeRegistrationErrorPostcode",
                        "TradeRegistrationErrorPostcodeNoMatch",
                        "FormsErrorPhone",
                        "TradeRegistrationErrorPhoneNoMatch",
                        "TradeRegistrationNoMatch",
                        "FormsErrorEmail",
                        "ManageAccountErrorTradeAccountNumber",
                        "TradeRegistrationCheckUnknownEmail",
                        "TradeRegistrationCheckSignIn",
                        "TradeRegistrationCheckMustAssociate",
                        "TradeRegistrationCheckSignInThenMustAssociate",
                        "TradeRegistrationCheckError",
                        "TradeRegistrationCompleteMessage",
                        "ManageAccountErrorTradeAccountNumberUnverified",
                        "ManageAccountErrorTradeAccountNumberConfiguration",
                        "ErrorTradeAccountNumberNotFound",
                        "ErrorTradeAccountNumberTransient",
                        "ManageAccountErrorTradeAccountNumberTransient",
                        "TradeAccountNumberVerified",
                        "ManageAccountTradeAccountNumber",
                        "ManageAccountTradeAccountNumberVerified",
                        "LoginInvalid",
                        "ManageAccountErrorTradeAccountNumberTransient",
                        "TradeLoginHeadingText",
                        "RegistrationDuplicateHeading",
                        "RegistrationDuplicateText",
                        "PasswordReset",
                        "PasswordResetRequestEmailSent",
                        "RegistrationTradeAccountEmailAddressNotFoundHeading",
                        "RegistrationTradeAccountEmailAddressNotFoundText",
                        "LoginInvalid",
                        "RegistrationEmailLabel",
                        "TradeAccountInvoiceAddressPostcode",
                        "TradeAccountBusinessPhoneNumber",
                        "TradeRegistrationNameLabel",
                        "RegistrationPasswordLabel",
                        "PleaseProvideYourPassword",
                    });
                    break;
                case (int)dgFixedPage.tradeUserManagement:
                    required.AddRange(new string[] {
                        "ManageAccountDetailsEdit",
                        "EditNegative",
                        "ManageAccountErrorName",
                        "ManageTradeAccountErrorEmail",
                        "ManageAccountErrorEmailCompare",
                        "ManageAccountErrorPassword",
                        "ManageAccountErrorCountry",
                        "ManageAccountDuplicateEmail",
                        "RegistrationDuplicateText",
                        "ManageAccountErrorPasswordCompare",
                        "ManageAccountErrorPasswordInvalid",
                        "ManageAccountPasswordStrength0",
                        "ManageAccountPasswordStrength1",
                        "ManageAccountPasswordStrength2",
                        "ManageAccountPasswordStrength3",
                        "ManageAccountPasswordStrength4",
                        "ManageAccountPasswordNotStrongEnough",
                        "ManageAccountErrorEmailFormat",
                        "ManageAccountChangedName",
                        "ManageAccountChangedEmail",
                        "ManageAccountChangedPassword",
                        "ManageAccountChangedCountry",
                        "FormsCountryRequired",
                        "FormsLanguageRequired",
                        "ManageAccountSaved",
                        "ManageAccountSaveFailed",
                        "AreYouSure",
                        "ManageTradeAccount_AddAdmin",
                        "ManageTradeAccount_AddUser",
                        "ManageTradeAccount_SelectPermissions",
                        "email",
                    });
                    break;
                case (int)dgFixedPage.tradeQuickOrdering:
                    required.AddRange(new string[] {
                        "AreYouSure",
                        "ConfirmBasketItemDelete",
                        "EnterSearch",
                        "AddToBasket",
                        "Quantity",
                        "item",
                        "Price",
                        "IndividualPrice",
                        "SetPrice",
                        "currencySymbol",
                        "Free",
                        "was",
                        "now",
                        "tradeQuickSearchPlaceholder",
                        "BasketProductCode",
                        "pleaseLessThanEqualTo",
                        "pleaseGreaterThanEqualTo",
                        "tradeQuickBasketErrors",
                        "ShoppingProductInBasket",
                        "ShoppingProductsInBasket",
                        "Meter",
                        "Roll",
                        "Unit",
                        "DetailedStockInfo",
                        "AvailableStock",
                        "BatchNo",
                        "Quantity",
                        "Length",
                        "Total",
                        "BasketProductCode",
                        "NoPieceInfo",
                        "NoBatches",
                        "IncomingStock",
                        "DueDate",
                        "NoPurchaseOrders",
                        "Product",
                        "Status",
                        "StockDetailNotAvailable",
                        "EnterSearch",
                        "ReserveNotAvailable",
                        "RemoveAll"
                    });
                    break;
                case (int)dgFixedPage.moodboard:
                    required.AddRange(new string[] {
                        "Sorry",
                        "ThankYou",
                        "Moodboard",
                        "MoodboardShareText",
                        "MoodboardThisWillTweet",
                        "MoodboardThisWillTweetImageOnly",
                        "MustHaveActiveMoodboard",
                        "AreYouSure",
                        "ThisWillDeleteMoodboard",
                        "MoodboardCreated",
                        "MoodboardCustomerName",
                        "MoodboardRoomSelector",
                        "MoodboardYourReference",
                        "MoodboardViewEdit",
                        "MoodboardResetAreYouSure",
                        "MoodboardClickToType",
                        "ScrapbookClearAllAreYouSure",
                        "KendoPagerDisplay",
                        "KendoPagerEmpty",
                        "KendoPagerPage",
                        "KendoPagerOf",
                        "KendoPagerItemsPerPage",
                        "KendoPagerFirstPage",
                        "KendoPagerLastPage",
                        "KendoPagerNextPage",
                        "KendoPagerPreviousPage",
                        "KendoPagerRefresh",
                        "KendoPagerMorePages",
                        "FormsErrorEmailFormat",
                        "SendEmailLabel",
                        "RecipientEmailRequired",
                        "ToProperCase",
                        "Subject",
                        "ReplyTo",
                        "ReplyToEmailRequired",
                        "MoodboardShareEmailRecipientLabel",
                        "EmailMessageRequired",
                        "EmailMessage",
                        "MoodboardIncludeProductData",
                        "MoodboardShareNameLabel",
                        "MoodboardShareRecipientNameLabel",
                        "MoodboardShareSendEmailSuccess",
                        "SomeoneSharedAMoodboardWithYou"
                    });
                    break;
                case (int)dgFixedPage.tradeClearance:
                    required.AddRange(new string[] { 
                        "AreYouSure",
                        "ChangingFiltersClearsSelection"
                    });
                    break;
                default:
                    break;
            }

            if (IncludesForm)
            {
                required.AddRange(new string[] {
                    "PleaseSelect",
                    "PleaseCorrectAll",
                    "AddressSearchPlaceholder",
                    "FormsErrorSelectLocation",
                    "FormsErrorSelectDate",
                    "FormsErrorSelectTimeSlot",
                    "FormsErrorSelectAppointmentAgenda",
                    "FormsErrorSelectAppointmentType",
                    "FormsErrorName",
                    "NoMarketingPermissions",
                    "FormsErrorEmail",
                    "FormsErrorEmailFormat",
                    "FormsErrorEmailCompare",
                    "FormsErrorCompanyName",
                    "FormsErrorLineOne",
                    "FormsErrorLineTwo",
                    "FormsErrorTown",
                    "FormsErrorCounty",
                    "FormsErrorPostCode",
                    "FormsCountryRequired",
                    "FormsLanguageRequired",
                    "phoneValidation",
                    "FormsErrorPhone",
                    "FormsErrorCaptchaCompare",
                    "MoodboardSave",
                    "PleaseProvideCompanyName",
                    "FormsCompanyName",
                    "PleaseProvideName",
                    "PleaseProvideAddressLine1",
                    "FormsAddressLineTwo",
                    "PleaseProvideTown",
                    "PleaseProvidePostcode",
                    "PleaseSelectCountry",
                    "PleaseSelectCountryBefore",
                    "FormsCompanyName",
                    "FormsFullName",
                    "FormsAddressLineOne",
                    "FormsAddressLineTwo",
                    "FormsTown",
                    "FormsCounty",
                    "FormsPostCode",
                    "FormsPhoneNumber",
                    "FormsEmail",
                    "FormsConfirmEmail",
                    "email",
                    "SearchForAddress",
                    "FormsCountry",
                    "Sorry",
                    "PleaseTryAgain",
                    "FormsNotes", // actually reads "Preferred Date"
                    "FormsPreferredTime"
                });
            }

            foreach (var r in required)
            {
                result.Add(new ResourceResult()
                {
                    Name = r,
                    Value = resourceDefinition.resourceSelects
                                            .Where(p => p.Name == r)
                                            .Select(p => p.Value)
                                            .SingleOrDefault()
                });
            }

            PrefetchedResources.AddRange(result);
        }
    }
}