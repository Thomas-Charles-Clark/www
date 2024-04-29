using Braintree;

using DocumentFormat.OpenXml.Wordprocessing;

using HtmlAgilityPack;

using iTextSharp.text;

using MertexerV2.SQLFunctions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

using wwwDesignersGuild.Models;
using wwwDesignersGuild.Models.ArticleSubType;
using wwwDesignersGuild.Models.ArticleType;
using wwwDesignersGuild.Models.Aspect;
using wwwDesignersGuild.Models.Attribute;
using wwwDesignersGuild.Models.Banner;
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
using wwwDesignersGuild.Util;

namespace wwwDesignersGuild.ViewModels
{
    public class LandingPageViewModel : BaseViewModel
    {
        [JsonIgnore]
        public cmsLandingPageDefinition Definition { get; set; }

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

        public static async Task<LandingPageViewModel> CreateAsync(NewMasterDefinition objMaster, VisitorSelect objVisitor, bool isShop)
        {
            var result = new LandingPageViewModel(objMaster, objVisitor);

            await result.Initialise(isShop);

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

        protected async Task Initialise(bool isShop)
        {
#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            await base.Initialise();

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
                tradeAccountId: Visitor.memberTradeAccountID);

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

            MasterDefinition.TitleTag = page.TitleTag;

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
                    case (byte)PageSectionType.ProductPageGrid:
                        PageSections.Add(new PageSectionViewModel(
                            pageSection: pageSection,
                            baseViewModel: this,
                            sectionDefinition: (object)await ProductPageGridPageSection.CreateAsync(
                                originalUrlKeywords: Definition.PageDetails.URLKeywords,
                                productPage: Definition.productPageGridSections.Single(p => p.PageSectionID == pageSectionId),
                                pagingEnabled: pageSection.Paging || pageSection.Filter,
                                filterIndexingEnabled: Definition.PageDetails.EnableFilterIndexing,
                                isShop: Definition.PageDetails.IsShop,
                                isLifestyleGallery: pageSection.IsLifestyleGallery,
                                model: this
                            ),
                            isShop: Definition.PageDetails.IsShop));

                        var ppgps = PageSections.Last().Definition as ProductPageGridPageSection;

                        if (ppgps.Contents.Any())
                        {
                            allImagesSorted.Add(new Tuple<long, int, int>(ppgps.GetContents().Skip(0).Take(1).First().ImageID, 0, pageSection.SortOrder ?? 0));
                        }

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
                await base.InitialiseHotspots(imageIds);

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
            System.Diagnostics.Debug.WriteLine($"LandingPageViewModel initialised in {sw.Elapsed}");
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
                        "KendoPagerMorePages"
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
                    "PleaseTryAgain"
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

    public class HtmlPageSection
    {
        [JsonIgnore]
        public string Contents { get; set; }

        public long HtmlSectionID { get; set; }

        public IEnumerable<long> ImageIDs { get; set; }

        public List<ImageHotspot> Hotspots { get; set; }

        private HtmlPageSection(
            long htmlSectionId,
            string contents,
            IEnumerable<long> imageIds
            )
        {
            HtmlSectionID = htmlSectionId;
            Contents = contents;
            ImageIDs = imageIds;
            Hotspots = new List<ImageHotspot>();
        }

        public static async Task<HtmlPageSection> CreateAsync(
            long htmlSectionId,
            string contents,
            IEnumerable<long> imageIds)
        {
            var result = new HtmlPageSection(htmlSectionId, contents, imageIds);

            result.Hotspots = new List<ImageHotspot>();

            return result;
        }
    }

    public class SlideshowPageSection
    {
        /// <summary>
        /// Used just in case a carousel image is missing some alt text
        /// </summary>
        public string Heading { get; set; }
        public CarouselViewModel CarouselViewModel { get; set; }

        public SlideshowPageSection(
            long pageSectionId,
            bool halfWidth,
            string heading,
            List<pageSectionSlideShow> images,
            pageSectionSlideShowAspect aspect,
            BaseViewModel baseViewModel
            )
        {
            Heading = heading;
            CarouselViewModel = new CarouselViewModel
            (
                carouselId: $"#page-section-{pageSectionId}-carsouel",
                wrapperCssClass: null,
                aspectRatio: aspect.AspectRatio,
                heading: heading,
                images: images.Select(i => new CarouselImage(i.ImageID, i.SortOrder, i.AltText, ImageSize.Twelve, new CarouselImageOverlay(i), i.MaxWidth))
                              .OrderBy(i => i.SortOrder),
                baseViewModel: baseViewModel
            );
        }
    }

    public class CollectionGridPageSection
    {
        public List<pageSectionCollection> Collections { get; set; }

        public CollectionGridPageSection(
            List<pageSectionCollection> collections,
            List<pageSectionCollectionImage> images,
            BaseViewModel baseViewModel
            )
        {
            Collections = collections.OrderBy(p => p.SortOrder).ToList();

            Collections.ForEach(c =>
            {
                c.CarouselViewModel = new CarouselViewModel
                (
                    carouselId: $"#collection-{c.CollectionID}-carousel",
                    wrapperCssClass: "card-img-top",
                    aspectRatio: AspectRatio.LandscapeFeature,
                    heading: c.Heading,
                    images: images.Where(i => i.CollectionID == c.CollectionID)
                                  .Select(i => new CarouselImage(
                                      imageId: i.ImageID,
                                      sortOrder: i.SortOrder,
                                      altText: c.Heading, 
                                      imageSize: ImageSize.Nine, 
                                      overlay: null,
                                      maxWidth: null, 
                                      forceLazyLoading: true))
                                  .OrderBy(i => i.SortOrder),
                    baseViewModel: baseViewModel,
                    displayDashes: false
                );
            });
        }
    }

    public class ProductPageGridPageSection : PagerViewModel
    {
        public bool Searchable { get; set; }
        public bool IsLifestyleGallery { get; set; }
        public PageSectionJSON Json { get; set; }
        public bool FilterIndexingEnabled { get; set; }

        public Regex UrlParsingRegex = new Regex(@"(.*)\/([^\s\/]+)(\/)([a-z][\d]+)(\??)(.*)$");
        public string OriginalSemanticString { get; set; }
        public string OriginalTitle { get; set; }
        public string DeselectedAttributeNames { get; set; }
        public string OriginalURLKeywords { get; set; }
        public bool MustOverwritePageCanonical { get; set; }
        public string CurrentAttributeSignature { get; set; }
        public IEnumerable<FilterIndexAttributeCombination> AttributeCombinations { get; set; }

        // This for use by search 
        public ProductPageGridPageSection(
            string originalUrlKeywords,
            NewMasterDefinition objMaster,
            bool filterIndexingEnabled,
            bool ajax = false,
            string ajaxEndpoint = null) : base(-1,
                                               true,
                                               true,
                                               objMaster,
                                               ajax,
                                               ajaxEndpoint)
        {
            FilterIndexingEnabled = filterIndexingEnabled;
            Searchable = false;
            IsLifestyleGallery = false;
            OriginalSemanticString = null;
            OriginalTitle = null;

            DeselectedAttributeNames = null;

            OriginalURLKeywords = originalUrlKeywords;

            ProductTypes = new List<ProductType>();

            MustOverwritePageCanonical = false;
        }

        private ProductPageGridPageSection(
            string originalUrlKeywords,
            PageSectionJSON json,
            bool pagingEnabled,
            bool filterIndexingEnabled,
            bool isShop,
            bool isLifestyleGallery,
            NewMasterDefinition objMaster,
            bool ajax = false,
            string ajaxEndpoint = null) : base(json.PageSectionID,
                                                  pagingEnabled,
                                                  isShop,
                                                  objMaster,
                                                  ajax,
                                                  ajaxEndpoint)
        {
            OriginalURLKeywords = originalUrlKeywords;
            Json = json;
            FilterIndexingEnabled = filterIndexingEnabled;
            Searchable = false;
            IsLifestyleGallery = isLifestyleGallery;
            OriginalSemanticString = null;
            OriginalTitle = null;

            DeselectedAttributeNames = null;

            ProductTypes = new List<ProductType>();

            MustOverwritePageCanonical = false;
        }

        //public List<int> FilteredProductPageIDs { get; set; }

        public new List<ProductPageFilterablePod> GetContents()
        {
            if (FilterCount > 0 || ProductType != "")
            {
                if (ProductType == "")
                {
                    var r = Contents.Cast<ProductPageFilterablePod>()
                        .Where(ir => (!FilteredIDs.Any() || FilteredIDs.Contains(ir.ProductPageID)))
                        .ToList();

                    r.ForEach(c => c.IsLifestyle = IsLifestyleGallery);

                    return r;
                }
                else
                {
                    var matches = Contents.Cast<ProductPageFilterablePod>()
                        .Where(ir => ir.ProductType.FormValue == ProductType)
                        .ToList();

                    if (FilteredIDs.Any())
                    {
                        matches = matches.Where(ptm => FilteredIDs.Contains(ptm.ProductPageID)).ToList();
                    }

                    SortContents(ref matches);

                    matches.ForEach(c => c.IsLifestyle = IsLifestyleGallery);

                    return matches;
                }
            }
            else
            {
                var r = Contents.Cast<ProductPageFilterablePod>().ToList();

                r.ForEach(c => c.IsLifestyle = IsLifestyleGallery);

                return r;
            }
        }

        public IEnumerable<ProductPagePodViewModel> GetProductPages(NewMasterDefinition model, int? skip, int? take)
        {
            // Should already be sorted correctly, shouldn't it...?

            if (skip == null)
            {
                return GetContents().Select(p => new ProductPagePodViewModel(model, p, IsLifestyleGallery));
            }
            else
            {
                return GetContents().Skip(skip.Value).Take(take.Value).Select(p => new ProductPagePodViewModel(model, p, IsLifestyleGallery, dgFramework.IsMobileDevice() ? ImageSize.Six : ImageSize.Three));
            }
        }

        public ProductPagePodViewModel GetProductPage(int productPageId, NewMasterDefinition model)
        {
            var results = Contents.Cast<ProductPageFilterablePod>().ToList();

            results.ForEach(c => c.IsLifestyle = IsLifestyleGallery);

            return results.Where(p => p.ProductPageID == productPageId)
                          .Select(p => new ProductPagePodViewModel(model, p, IsShop, dgFramework.IsMobileDevice() ? ImageSize.Six : ImageSize.Three))
                          .Single();
        }

        public static async Task<ProductPageGridPageSection> CreateAsync(
            string originalUrlKeywords,
            PageSectionJSON productPage,
            bool pagingEnabled,
            bool filterIndexingEnabled,
            bool isShop,
            bool isLifestyleGallery,
            BaseViewModel model
            )
        {
            var result = new ProductPageGridPageSection(
                originalUrlKeywords,
                productPage,
                pagingEnabled,
                filterIndexingEnabled,
                isShop,
                isLifestyleGallery,
                model.MasterDefinition);

            var objDb = new databaseDataContext();

            var objAttribute = new cmsAttribute();
            
            var objAttributeLoad = objAttribute.loadAttributeFromCms(model.MasterDefinition.UrlBreakdown.countryDetails.LanguageID);
            
            // Filter index restrictions fn
            var objFilterIndexRestrictionLoad = new cmsFilterIndexRestriction().loadFilterIndexingRestrictionsFromCmsAsync(
                    new PassFilterIndexRestrictionCacheFetch() { Date = model.Date },
                    model
                );

            // Thanks https://stackoverflow.com/questions/32119507/multiple-awaits-vs-task-waitall-equivalent#answer-32119616

            await Task.WhenAll(objAttributeLoad, objFilterIndexRestrictionLoad); //(, ...);

            var attributeData = await objAttributeLoad;

            var filterIndexRestrictions = await objFilterIndexRestrictionLoad;

            var preResultSet = new List<ProductPageFilterablePodWrapper>();

            var jsonString = result.Json.JSON.Replace("{\"productPages\":", "").TrimEnd(new char[] { '}' });

            using (var stream = Framework.GenerateStreamFromString(jsonString, System.Text.Encoding.UTF8))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    using (JsonReader jsonReader = new JsonTextReader(streamReader))
                    {
                        //jsonReader.CloseInput = false;
                        jsonReader.SupportMultipleContent = true;

                        JsonSerializer serializer = new JsonSerializer();

                        serializer.ContractResolver = new JsonIgnoreAttributeIgnorerContractResolver();

                        while (jsonReader.Read())
                        {
                            if (jsonReader.TokenType == JsonToken.StartObject)
                            {
                                var obj = JObject.Load(jsonReader);
                                preResultSet.Add(obj.ToObject<ProductPageFilterablePodWrapper>(serializer));
                            }
                        }
                    }
                }
            }

            var completeResultSet = preResultSet.Select(p => p.ProductPage).ToList();

            //var completeResultSet = JObject.FromObject(result.Json.json)
            //                                .SelectToken("productPages")
            //                                .Children()
            //                                .Select(p => JsonConvert.DeserializeObject<ProductPageFilterablePod>(p.SelectToken("productPage").ToString(), 
            //                                                new JsonSerializerSettings() { ContractResolver = new JsonIgnoreAttributeIgnorerContractResolver() }))
            //                                .ToList();

            completeResultSet.ForEach(c => c.IsLifestyle = isLifestyleGallery);

            var productPageIds = completeResultSet.Select(p => p.ProductPageID).ToList();

            var attributeIds = result.Json.attributes
                                          .Where(p => productPageIds.Contains(p.ProductPageID))
                                          .GroupBy(p => p.AttributeID)
                                          .Select(p => p.Key).ToList();

            completeResultSet.ForEach(pp =>
            {
                var ids = result.Json.attributes.Where(p => p.ProductPageID == pp.ProductPageID)
                                                        .Select(p => p.AttributeID).ToList();

                pp.AttributeFormValues = attributeData.attributeDetails.Where(p => ids.Contains(p.AttributeID))
                                                                       .Select(p => p.FormValue)
                                                                       .ToList();

                pp.ProductType = attributeData.productTypes.Single(pt => pt.ProductTypeID == pp.ProductTypeID);
            });

            var gridProductTypeIds = completeResultSet.Select(p => p.ProductTypeID).Distinct();

            result.ProductTypes = attributeData.productTypes.Where(pt => gridProductTypeIds.Contains(pt.ProductTypeID)).ToList();

            result.ProductTypes.ForEach(pt =>
            {
                pt.ProductPageIDs = completeResultSet.Where(crs => crs.ProductTypeID == pt.ProductTypeID).Select(crs => crs.ProductPageID);
                pt.FilterIndexRestrictions = filterIndexRestrictions.Restrictions.Where(fir => fir.ProductTypeID == pt.ProductTypeID);
            });

            if (result.ProductTypes.Count > 1)
            {
                result.ProductTypes.Insert(0, new ProductType
                {
                    ProductTypeID = 0,
                    Name = HtmlHelperExtra.Resource("All", model.LanguageID, model.CountryID, model.Date).ToString(),
                    SortOrder = 0,
                    ProductPageIDs = completeResultSet.Select(p => p.ProductPageID)
                });
            }
            else
            {
                if (result.ProductTypes.Count() == 1)
                {
                    result.ProductType = result.ProductTypes[0].FormValue;

                    // Filter index restrictions fn
                    var objFilterIndexCombinations = await new cmsFilterIndexAttributeCombination().loadFilterIndexingAttributeCombinationsFromCmsAsync(
                            new PassFilterIndexAttributeCombinationCacheFetch(model.LanguageID, result.ProductTypes[0].ProductTypeID, model.Date),
                            model
                        );

                    result.AttributeCombinations = objFilterIndexCombinations.Combinations;
                }
            }

            var attributes = attributeData.attributeDetails
                                          .Where(p => attributeIds.Contains(p.AttributeID))
                                          .OrderBy(p => p.SortOrder)
                                          .ToList();

            attributes.ForEach(a =>
            {
                a.ProductPageIDs = result.Json
                                         .attributes
                                         .Where(p => p.AttributeID == a.AttributeID)
                                         .Select(p => p.ProductPageID)
                                         .ToList();
            });

            var attributeGroupIds = attributes.Select(p => p.AttributeGroupID).Distinct();

            result.AttributeGroupGroups = attributeData.attributeGroupGroups;

            result.AttributeGroups = attributeData.attributeGroups
                                               .Where(p => attributeGroupIds.Contains(p.AttributeGroupID))
                                               .ToList();

            result.AttributeGroups.ForEach(ag =>
            {
                ag.Attributes = attributes.Where(p => p.AttributeGroupID == ag.AttributeGroupID).ToList();

                ag.AttributeGroupGroup = attributeData.attributeGroupGroups.SingleOrDefault(agg => agg.AttributeGroupGroupID == ag.AttributeGroupGroupID);

                ag.ProductTypes = attributeData.attributeGroupProductTypes
                    .Where(agpt => agpt.AttributeGroupID == ag.AttributeGroupID)
                    .Select(agpt => new AttributeGroupProductType(attributeData.productTypes.Single(pt => pt.ProductTypeID == agpt.ProductTypeID), agpt.LexicographicalSortOrder))
                    .ToList();
            });

            if (!model.MasterDefinition.TradeAllowed) /* If you're not trade and trade-allowed */
            {
                // and there's a Clearance attribute group delivered
                result.AttributeGroups.RemoveAll(p => p.AttributeGroupID == 0);
            }

            result.AttributeGroups.ForEach(ag =>
            {
                var attributeNames = HttpContext.Current.Request.QueryString[$"{HttpUtility.UrlEncode(ag.Name.ToLowerInvariant())}"]?
                                      .Split(Framework.CommaDelimiter)
                                      .ToList();

                if (attributeNames != null)
                {
                    ag.Selection = ag.Attributes
                        .Where(p => attributeNames.Contains(p.FormValue))
                        .Select(p => p.FormValue)
                        .ToList();

                    ag.Selection = ag.Selection.Distinct().ToList();
                }
                else
                {
                    ag.Selection = new List<string>();
                }
            });

            var url = HttpContext.Current.Request.Url.ToString();
            
            if (!String.IsNullOrEmpty(originalUrlKeywords))
                url = url.Replace(originalUrlKeywords, "");

            var matches = result.UrlParsingRegex.Matches(url);

            var attributesList = new List<AttributeSorter>();

            if (matches.Count == 1)
            {
                if (matches[0].Groups.Count >= 3)
                {
                    // produce the power set of all words in the malleable url segment
                    // it's possible that some are attributes
                    var urlKeywordsAttributesPowerSet = EnumerableExtensions.PowerSet(matches[0].Groups[2].Value.Split(Framework.HyphenDelimiter).AsEnumerable<string>());

                    foreach (var potentialAttribute in urlKeywordsAttributesPowerSet)
                    {
                        var value = String.Join("-", potentialAttribute);

                        foreach (var ag in result.AttributeGroups.Where(g => g.InvolveInFilterIndexing(result.ProductType)))
                        {
                            var attrs = ag.Attributes.ToList();
                            var attr = attrs.SingleOrDefault(a => a.FormValue == HttpUtility.UrlEncode(value));

                            // Do this little dance
                            if (attr != null)
                            {
                                attributesList.Add(new AttributeSorter(attr.AttributeID, attr.AttributeGroupID, attr.FormValue, attr.Name, ag.ProductTypes.Single(pt => pt.FormValue == result.ProductType).LexicographicalSortOrder, ag.LogicalOperator));

                                ag.Selection.Add(HttpUtility.UrlEncode(value));

                                ag.Selection = ag.Selection.Distinct().ToList();
                            }
                        }
                    }
                }
            }

            attributesList = attributesList.OrderBy(al => al.LexicographicalSortOrder).ToList();

            var attributeSignature = String.Join(".", attributesList.Select(a => $"{a.AttributeID}"));

            result.CurrentAttributeSignature = attributeSignature;

            model.AttributeFilterText = String.Join(" ", attributesList.Select(al => al.AttributeName));

            model.AttributeSignatureText = result.AttributeCombinations?.SingleOrDefault(c => c.AttributeSignature == attributeSignature)?.FooterText;

            if (attributesList.Any())
            {
                result.MasterDefinition.GoogleHrefs = new databaseDataContext().Page_Hreflang_GenerateMultiple(model.ContentID, model.Date, JsonConvert.SerializeObject(attributesList.Select(al => new { id = al.AttributeID }).ToList())).ToList().ToList();
            }

            //var regex = new Regex(@"([^|]+)");

            //var titleMatches = regex.Matches(result.MasterDefinition.TitleTag);

            //if (titleMatches != null)
            //{
            //    if (titleMatches.Count > 1)
            //    {
            //        result.MasterDefinition.TitleTag = $"{model.AttributeFilterText} | {titleMatches[1].Value.Trim()}";
            //    }
            //}

            result.OriginalTitle = result.MasterDefinition.TitleTag;

            if (result.FilterIndexingEnabled)
            {
                if (!String.IsNullOrEmpty(model.AttributeFilterText))
                {
                    if (!String.IsNullOrEmpty(model.CategoryPageProductType?.NamePlural))
                    {
                        result.MasterDefinition.TitleTag = $"{model.AttributeFilterText} {model.CategoryPageProductType.NamePlural} | Designers Guild";
                    }
                    else
                    {
                        result.MasterDefinition.TitleTag = $"{model.AttributeFilterText} {result.MasterDefinition.TitleTag}";
                    }

                    result.MasterDefinition.MetaDescription = HtmlHelperExtra.ProcessAttributeFilterText(
                        filterText: result.MasterDefinition.MetaDescription ?? "",
                        attributeFilterText: model.AttributeFilterText,
                        productTypePluralPropercase: model.CategoryPageProductType?.NamePlural,
                        attributeSignatureText: model.AttributeSignatureText,
                        useHtml: false);

                    result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("<span class=\"filter-text\">", "");

                    result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("</span> ", "");

                    result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("<span class=\"no-filter-text\">", "");

                    result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("</span> ", "");

                    completeResultSet.ForEach(crs => 
                    {
                        if (!String.IsNullOrEmpty(crs.ShortHeading)) 
                        {
                            crs.Heading = $"{crs.ShortHeading} {model.AttributeFilterText} {crs.ProductType.Name}";
                        }
                    });
                }
                else
                {
                    result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("{ProductTypeStart}", "");
                    result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("{ProductTypeEnd}", "");
                    result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("{FilterStart}", "");
                    result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("{FilterEnd}", "");
                    result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("{NoFilterStart}", "");
                    result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("{NoFilterEnd}", "");

                    result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Trim();
                }
            }

            var overallSelection = result.AttributeGroups.Where(ag => ag.Selection.Any())
                .SelectMany(ag => ag.Selection).Distinct().ToList();

            result.InitialisePager();

            var initialProductPageIds = completeResultSet
                .Where(crs => result.ProductType == null || crs.ProductTypeFormValue == result.ProductType)
                .Select(p => p.ProductPageID).ToList();

            var attributeProductPageIds = new List<AttributeGroupProductPage>();

            var detectedAttributeGroups = new List<AttributeGroup>();

            foreach (var a in overallSelection)
            {
                var attributeGroup = result.AttributeGroups.SingleOrDefault(g => g.Selection.Contains(a));

                var attribute = attributeGroup.Attributes.SingleOrDefault(at => at.FormValue == a);

                if (attributeGroup != null &&
                    attribute != null)
                {
                    detectedAttributeGroups.Add(attributeGroup);

                    switch (attributeGroup.LogicalOperator)
                    {
                        case "AND":
                            attributeProductPageIds.Add(new AttributeGroupProductPage(attributeGroup.AttributeGroupID, attribute.ProductPageIDs));
                            break;
                        case "OR":
                            // if this attributeGroupId is already in the array we're putting together
                            var extant = attributeProductPageIds.SingleOrDefault(p => p.AttributeGroupID == attributeGroup.AttributeGroupID);

                            if (extant != null)
                            {
                                // concatenate
                                extant.ProductPageIDs.AddRange(attribute.ProductPageIDs);
                                extant.ProductPageIDs = extant.ProductPageIDs.Distinct().ToList();
                            }
                            else
                            {
                                // if not, add to the array as normal
                                attributeProductPageIds.Add(new AttributeGroupProductPage(attributeGroup.AttributeGroupID, attribute.ProductPageIDs));
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            if (detectedAttributeGroups.Any() && detectedAttributeGroups.Count == attributesList.Count)
            {
                if (result.AttributeCombinations.FirstOrDefault(r => r.AttributeSignature == attributeSignature) == null)
                {
                     result.MustOverwritePageCanonical = true;
                }
            }

            attributeProductPageIds.ForEach(appid =>
            {
                initialProductPageIds = initialProductPageIds.Intersect(appid.ProductPageIDs).ToList();
            });

            result.AttributeGroups.ForEach(ag =>
            {
                var productPageIdsToConsider = initialProductPageIds;

                attributeProductPageIds.ForEach(appid =>
                {
                    if (appid.AttributeGroupID != ag.AttributeGroupID)
                    {
                        productPageIdsToConsider = productPageIdsToConsider.Intersect(appid.ProductPageIDs).ToList();
                    }
                });

                var productPageIdsAffectingThisAttributeGroup = new List<int>();

                switch (ag.LogicalOperator)
                {
                    case "AND":
                        productPageIdsAffectingThisAttributeGroup = productPageIdsToConsider.Intersect(
                            attributeProductPageIds.SelectMany(appid => appid.ProductPageIDs)
                        ).ToList();
                        break;
                    case "OR":
                        productPageIdsAffectingThisAttributeGroup = productPageIdsToConsider.Intersect(
                            attributeProductPageIds.Where(appid => appid.AttributeGroupID != ag.AttributeGroupID)
                                                   .SelectMany(appid => appid.ProductPageIDs)
                        ).ToList();
                        break;
                }

                ag.Attributes.ForEach(a =>
                {
                    //if (a.FormValue == "orange")
                    //{
                    //    int f = 0;
                    //}

                    if (productPageIdsAffectingThisAttributeGroup.Any())
                    {
                        a.AvailableProductPageIDs = a.ProductPageIDs.Intersect(productPageIdsAffectingThisAttributeGroup).ToList();
                    }
                    else
                    {
                        a.AvailableProductPageIDs = a.ProductPageIDs;
                    }

                    if (a.AvailableProductPageIDs == null)
                    {
                        a.AvailableProductPageIDs = new List<int>();
                    }
                });
            });

            result.FilteredIDs = initialProductPageIds;

            if (result.FilteredIDs.Count == 0)
            {
                var problemAttributes = result.AttributeGroups.Where(ag => ag.Selection.Any()).SelectMany(ag =>
                {
                    return ag.Attributes.Where(a => ag.Selection.Contains(a.FormValue));
                }).ToList();

                result.DeselectedAttributeNames = String.Join(", ", problemAttributes.Select(pa => pa.Name));

                result.FilteredIDs = completeResultSet.Select(p => p.ProductPageID).ToList();

                model.AttributeSignatureText = null;
            }

            #region Perform Sorting

            result.SortContents(ref completeResultSet);

            #endregion

            result.Contents = completeResultSet.Cast<object>()
                                               .ToList();

            if (result.Contents.Count() < result.PageSize)
                result.TakeCount = result.Contents.Count();
            else
                result.TakeCount = result.PageSize;

            result.InitialisePager();

            return result;
        }

        public void SortContents(ref List<ProductPageFilterablePod> items)
        {
            switch (Sort)
            {
                case "bestSellers":
                    items = items
                        .OrderBy(p => p.SalesRank)
                        .ThenBy(p => p.MerchandisedSortOrder)
                        .ToList();
                    break;
                case "alphaAZ":
                    items = items
                        .OrderBy(p => p.AZSortOrder)
                        .ThenBy(p => p.MerchandisedSortOrder)
                        .ToList();
                    break;
                case "alphaZA":
                    items = items
                        .OrderByDescending(p => p.AZSortOrder)
                        .ThenBy(p => p.MerchandisedSortOrder)
                        .ToList();
                    break;
                case "new":
                    items = items
                        .OrderBy(p => p.NewSort)
                        .ThenBy(p => p.MerchandisedSortOrder)
                        .ToList();
                    break;
                case "priceHighToLow":
                    items = items
                        .OrderBy(p => p.PriceSortOrder)
                        .ThenBy(p => p.MerchandisedSortOrder)
                        .ToList();
                    break;
                case "priceLowToHigh":
                    items = items
                        .OrderByDescending(p => p.PriceSortOrder)
                        .ThenBy(p => p.MerchandisedSortOrder)
                        .ToList();
                    break;
                case "discountHighToLow":
                    items = items
                        .OrderByDescending(p => p.DiscountSortOrder)
                        .ThenBy(p => p.MerchandisedSortOrder)
                        .ToList();
                    break;
                case "discountLowToHigh":
                    items = items
                        .OrderBy(p => p.DiscountSortOrder)
                        .ThenBy(p => p.MerchandisedSortOrder)
                        .ToList();
                    break;
                case "stockHightoLow":
                    items = items
                        .OrderByDescending(p => p.StockSortOrder)
                        .ThenBy(p => p.MerchandisedSortOrder)
                        .ToList();
                    break;
                case "stockLowToHigh":
                    items = items
                        .OrderBy(p => p.StockSortOrder)
                        .ThenBy(p => p.MerchandisedSortOrder)
                        .ToList();
                    break;
                default:
                    items = items
                        .OrderBy(p => p.MerchandisedSortOrder)
                        .ToList();
                    break;
            }
        }

        public string BuildAttributeURL(AttributeGroup attributeGroup, AttributeDetail attribute, IEnumerable<FilterIndexRestriction> filterIndexRestrictions, string currentAttributeSignature, IEnumerable<FilterIndexAttributeCombination> attributeCombinations)
        {
            //if (attribute.FormValue == "blue")
            //{
            //    int f = 0;
            //}

            // if FilterIndexing is enabled on this grid's containing page
            if (FilterIndexingEnabled && attribute.EnableFilterIndexing)
            {
                // does this ProductType describe a LexicographicalSortOrder for this AttributeGroup?
                if (attributeGroup.InvolveInFilterIndexing(ProductType))
                {
                    // Yes, we can include this attribute in the URL
                    var originalUrl = HttpContext.Current.Request.Url.ToString();
                    var placeholder = $"{(char)254}";
                    var url = HttpContext.Current.Request.Url.ToString().Replace(OriginalURLKeywords, placeholder);

                    var matches = UrlParsingRegex.Matches(url);

                    if (matches.Count == 1)
                    {
                        if (matches[0].Groups.Count >= 3)
                        {
                            var split = new List<AttributeSorter>();

                            var urlKeywordsAttributesPowerSet = EnumerableExtensions.PowerSet(matches[0].Groups[2].Value.Split(Framework.HyphenDelimiter).AsEnumerable<string>());

                            foreach (var set in urlKeywordsAttributesPowerSet)
                            {
                                var potentialAttribute = String.Join("-", set);

                                //if (AttributeGroups.SelectMany(a => a.Attributes).Where(a => a.FormValue == potentialAttribute).Any())
                                //    split.Add(new AttributeSorter(potentialAttribute, null, null, null));

                                var attr = AttributeGroups.SelectMany(a => a.Attributes).SingleOrDefault(a => a.FormValue == potentialAttribute);
                                if (attr != null)
                                    split.Add(new AttributeSorter(attr.AttributeID, attr.AttributeGroupID, attr.FormValue, attr.Name, null, null));
                            }

                            if (!split.Select(s => s.Attribute).Contains(attribute.FormValue))
                            {
                                split.Add(new AttributeSorter(attribute.AttributeID, attribute.AttributeGroupID, attribute.FormValue, attribute.Name, null, null));
                            }

                            // split now contains e.g. new, cushions, blue, patterned
                            // we want to rebuild it in the correct sort order
                            // so we need to pull the attributeGroups having these attributes...

                            var attributeGroups = AttributeGroups.Where(ag => ag.InvolveInFilterIndexing(ProductType)).ToList();

                            var toRemove = new List<AttributeSorter>();
                            var toAdd = new List<AttributeSorter>();

                            foreach (var s in split)
                            {
                                var thisAttributeGroup = attributeGroups
                                    .Where(ag => ag.Attributes.SingleOrDefault(a => a.FormValue == s.Attribute) != null)
                                    .SingleOrDefault();

                                // find the lexSortOrder for this split
                                var lexSortOrder = attributeGroups
                                    .Where(ag => ag.Attributes.SingleOrDefault(a => a.FormValue == s.Attribute) != null)
                                    ?.Select(ag => ag.ProductTypes.Single(pt => pt.FormValue == ProductType).LexicographicalSortOrder)
                                    ?.SingleOrDefault();

                                if (lexSortOrder != null)
                                {
                                    s.LexicographicalSortOrder = lexSortOrder;
                                    s.LogicalOperator = thisAttributeGroup.LogicalOperator;

                                    // we want to remove other attributes already in split belonging to this attribute group, leaving just the incoming attribute representing this group
                                    if (attribute.AttributeGroupID == thisAttributeGroup.AttributeGroupID)
                                    {
                                        toRemove.Add(s);
                                        if (toAdd.SingleOrDefault(a =>
                                            a.Attribute == attribute.FormValue &&
                                            a.LexicographicalSortOrder == lexSortOrder &&
                                            a.LogicalOperator == thisAttributeGroup.LogicalOperator)
                                            == null)
                                        {
                                            toAdd.Add(new AttributeSorter(attribute.AttributeID, attribute.AttributeGroupID, attribute.FormValue, attribute.Name, lexSortOrder, thisAttributeGroup.LogicalOperator));
                                        }
                                    }
                                }
                            }

                            foreach (var s in toRemove)
                            {
                                split.Remove(s);
                            }

                            foreach (var s in toAdd)
                            {
                                split.Add(s);
                            }

                            split = split.OrderBy(s => s.LexicographicalSortOrder).ToList();

                            if (filterIndexRestrictions == null || 
                                filterIndexRestrictions.Count() == 0 || 
                                filterIndexRestrictions
                                    .Select(fir => fir.AttributeGroupSignature)
                                    .Contains(String.Join(".", split.Where(s => s.AttributeGroupID != 0).Select(s => s.AttributeGroupID))))
                            {
                                split.Add(new AttributeSorter(0, 0, OriginalSemanticString, null, null, null));

                                url = UrlParsingRegex.Replace(url, $"$1/{String.Join("-", split.Select(s => s.Attribute))}/$4");

                                // we don't want Google to be indexing the query string parameters
                                if (url.IndexOf("?") > -1)
                                {
                                    url = url.Substring(0, url.IndexOf("?"));
                                }

                                if (attributeCombinations.SingleOrDefault(ac =>
                                    ac.AttributeSignature == (
                                        String.IsNullOrEmpty(currentAttributeSignature)
                                            ? $"{attribute.AttributeID}"
                                            : String.Join(".", split.Where(s => s.AttributeGroupID != 0).Select(s => s.AttributeID)))) == null)
                                {
                                    return null;
                                }

                                return url;
                            }
                            else
                            {
                                return "#";
                            }
                        }
                    }
                }

                return "#";
            }

            return "#";
        }
    }

    public class PageGridPageSection
    {
        public List<pageSectionPageSelect> Pages { get; set; }

        public PageGridPageSection(
            List<pageSectionPageSelect> pages,
            List<pageSectionPageSelectImage> images,
            BaseViewModel baseViewModel
            )
        {
            Pages = pages;

            Pages.ForEach(p =>
            {
                p.CarouselViewModel = new CarouselViewModel
                (
                    carouselId: $"#page-{p.PageID}-carousel",
                    wrapperCssClass: "card-img-top",
                    aspectRatio: AspectRatio.LandscapeFeature,
                    heading: p.Heading,
                    images: images.Where(i => i.PageID == p.PageID)
                                    .Select(i => new CarouselImage(i.ImageID, i.SortOrder, p.Heading, ImageSize.Four))
                                    .OrderBy(i => i.SortOrder),
                    baseViewModel: baseViewModel
                );
            });
        }
    }

    public class ArticleGridPageSection : PagerViewModel
    {
        [JsonIgnore]
        public List<pageSectionArticle> Articles { get; set; }

        public byte GridArticleType { get; set; }

        public List<articleTypeSelect> ArticleTypes { get; set; }

        public List<articleSubTypeSelect> ArticleSubTypes { get; set; }

        private ArticleGridPageSection(
            long pageSectionId,
            bool pagingEnabled,
            bool isShop,
            List<pageSectionArticle> articles,
            List<pageSectionArticleImage> images,
            BaseViewModel baseViewModel) : base(pageSectionId,
                                                pagingEnabled,
                                                isShop,
                                                baseViewModel.MasterDefinition)
        {
            ArticleTypes = new List<articleTypeSelect>();

            ArticleSubTypes = new List<articleSubTypeSelect>();

            FilteredIDs = new List<int>();

            Articles = articles.OrderByDescending(art => art.CreatedDT)
                               .ThenBy(art => Regex.Replace(art.Heading, "'", ""))
                               .ToList();

            Articles.ForEach(art =>
            {
                art.Heading = art.Heading?.Trim();
                art.SubHeading = art.SubHeading?.Trim();

                art.CarouselViewModel = new CarouselViewModel
                (
                    carouselId: $"#article-{art.ArticleID}-carousel",
                    wrapperCssClass: "card-img-top",
                    aspectRatio: AspectRatio.LandscapeFeature,
                    art.Heading,
                    images: images.Where(i => i.ArticleID == art.ArticleID)
                                  .Select(i => new CarouselImage(i.ImageID, i.SortOrder, art.Heading, ImageSize.Four))
                                  .OrderBy(i => i.SortOrder),
                    baseViewModel: baseViewModel
                );
            });

            Contents = Articles.Cast<object>()
                               .ToList();
        }

        public static async Task<ArticleGridPageSection> CreateAsync(
            long pageSectionId,
            bool pagingEnabled,
            bool isShop,
            List<pageSectionArticle> articles,
            List<pageSectionArticleImage> images,
            BaseViewModel baseViewModel)
        {
            var result = new ArticleGridPageSection(pageSectionId,
                pagingEnabled,
                isShop,
                articles,
                images,
                baseViewModel);

            var articleType = new cmsArticleType();
            var articleSubType = new cmsArticleSubType();

            var objArticleTypes = await articleType.loadArticleTypeFromCms(
                baseViewModel.MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
                baseViewModel.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                baseViewModel.MasterDefinition.UrlBreakdown.ContentDate);

            result.ArticleTypes = objArticleTypes.articleTypeSelects
                                                 .OrderBy(at => at.SortOrder)
                                                 .ToList();

            // Aha. So, if there are articles of more than one major type in this page section, we do not want to fetch the
            // list of article sub-types to render the sub-type-menu.

            var articleTypes = articles.Select(p => p.ArticleTypeID).Distinct();

            var articleTypeCount = articleTypes.Count();

            // In theory there should never be 0 article types, so that's just a sanity check
            if (articleTypeCount == 0 || articleTypeCount > 1)
            {
                result.GridArticleType = 0;

                result.ArticleSubTypes = new List<articleSubTypeSelect>();
            }
            else
            {
                result.GridArticleType = articleTypes.First();

                var objArticleSubTypes = await articleSubType.loadArticleSubTypeFromCms(
                    articleTypes.First(),
                    baseViewModel.MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
                    baseViewModel.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                    baseViewModel.MasterDefinition.UrlBreakdown.ContentDate);

                result.ArticleSubTypes = objArticleSubTypes.articleSubTypeSelects
                                                           .OrderBy(ast => ast.SortOrder)
                                                           .ToList();
            }

            var articleCountResourceName = result.Articles.Count() == 1 ? "ArticleSingular" : "articleMenuHeading";

            result.DisplayCount = $@"{result.Articles.Count():N0} {(HtmlHelperExtra.Resource(articleCountResourceName, baseViewModel.MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
              baseViewModel.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
              baseViewModel.MasterDefinition.UrlBreakdown.ContentDate))}";

            result.FilteredIDs = result.Articles.Select(a => a.ArticleID).ToList();

            result.InitialisePager();

            return result;
        }

        public new List<pageSectionArticle> GetContents()
        {
            if (AttributeGroups.Any(p => p.InUse))
            {
                var results = Contents.Cast<pageSectionArticle>()
                                      .Where(p => FilteredIDs.Contains(p.ArticleID))
                                      .ToList();

                return results;
            }
            else
            {
                return Contents.Cast<pageSectionArticle>().ToList();
            }
        }

        public List<ArticlePodViewModel> GetArticles(NewMasterDefinition model)
        {
            switch (Sort)
            {
                case "alphaAZ":
                    return GetContents().Select(a => new ArticlePodViewModel(model, a))
                        .OrderBy(p => p.Article.Heading)
                        .ThenBy(p => p.Article.SubHeading)
                        .ToList();
                case "alphaZA":
                    return GetContents().Select(a => new ArticlePodViewModel(model, a))
                        .OrderByDescending(p => p.Article.Heading)
                        .ThenBy(p => p.Article.SubHeading)
                        .ToList();
                case "new":
                    return GetContents().Select(a => new ArticlePodViewModel(model, a))
                        .OrderBy(p => p.Article.CreatedDT)
                        .ToList();
                default:
                    return GetContents().Select(a => new ArticlePodViewModel(model, a))
                        .OrderByDescending(p => p.Article.CreatedDT)
                        .ToList();
            }
        }
    }

    public class BrochurePageSection
    {
        public pageSectionBrochure Brochure { get; set; }

        public BrochurePageSection(
            pageSectionBrochure brochure
            )
        {
            Brochure = brochure;
        }
    }

    public class FormPageSection
    {
        public pageSectionForm Form { get; set; }
        public List<pageSectionFormRetailAppointmentAgenda> RetailAppointmentAgendas { get; set; }
        public List<pageSectionFormRetailAppointmentType> RetailAppointmentTypes { get; set; }
        public List<StoreEventsByLocation> StoreEventsByLocation { get; set; }
        public List<pageFormCountry> Countries { get; set; }
        public List<Language> Languages { get; set; }
        public short DefaultCountryID { get; set; }

        public FormPageSection(
            pageSectionForm form,
            List<pageSectionFormRetailAppointmentAgenda> appointmentAgendas,
            List<pageSectionFormRetailAppointmentType> appointmentTypes,
            List<pageSectionStoreEvent> storeEvents,
            List<pageFormCountry> countries,
            short defaultCountryId,
            DateTime useDate
            )
        {
            Form = form;
            RetailAppointmentAgendas = appointmentAgendas;
            RetailAppointmentTypes = appointmentTypes;
            Countries = countries;
            DefaultCountryID = defaultCountryId;

            Languages = new cmsCountry().loadCompleteLanguageList(DefaultCountryID, useDate).Result.languages;

            StoreEventsByLocation = storeEvents.GroupBy(p => new
            {
                p.StoreEventLocationID,
                p.StoreEventLocation
            })
                .Select(p => new StoreEventsByLocation(
                    storeEventLocationId: p.Key.StoreEventLocationID,
                    storeEventLocation: p.Key.StoreEventLocation,
                    events: p.ToList()))
                .ToList();
        }
    }

    public class StoreEventsByLocation
    {
        public List<pageSectionStoreEvent> StoreEvents { get; set; }
        public string StoreEventLocation { get; set; }
        public short StoreEventLocationID { get; set; }

        public StoreEventsByLocation(short storeEventLocationId,
            string storeEventLocation,
            List<pageSectionStoreEvent> events)
        {
            StoreEventLocationID = storeEventLocationId;
            StoreEventLocation = storeEventLocation;
            StoreEvents = events;
        }
    }

    public class BannerGridPageSection
    {
        public BannerLocationGroup BannerGrid { get; set; }

        public BannerGridPageSection(
            BannerLocationGroup bannerGrid
            )
        {
            BannerGrid = bannerGrid;
        }
    }

    public class VideoPageSection
    {
        public pageSectionVideo Video { get; set; }

        public VideoPageSection(
            pageSectionVideo video
            )
        {
            Video = video;
        }
    }

    public class AnimationPageSection
    {
        public string PageHeading { get; set; }
        public string PageSubHeading { get; set; }
        public string PageDescription { get; set; }
        public string PageDiscountMessage { get; set; }
        public ImageSize ImageSize { get; set; }
        public pageSectionAnimation Animation { get; set; }
        public List<pageSectionAnimationSlide> Images { get; set; }
        public List<pageSectionAnimationSlideCallToAction> CallsToAction { get; set; }

        public AnimationPageSection(
            string pageHeading,
            string pageSubHeading,
            string pageDescription,
            string pageDiscountMessage,
            bool isHeader,
            pageSectionAnimation animation,
            List<pageSectionAnimationSlide> images,
            List<pageSectionAnimationSlideCallToAction> callsToAction
            )
        {
            PageHeading = pageHeading;
            PageSubHeading = pageSubHeading;
            PageDescription = pageDescription;
            PageDiscountMessage = pageDiscountMessage;
            Animation = animation;
            Images = images;
            ImageSize = isHeader ? ImageSize.Nine : ImageSize.Twelve;
            CallsToAction = callsToAction;

            CallsToAction.ForEach(c =>
            {
                c.Text = c.Text.Replace(">", "").Trim();
            });
        }
    }

    public class BespokePageSection
    {
        public BespokePageSection()
        {

        }
    }

    public class PageSectionViewModel
    {
        [JsonIgnore]
        public BaseViewModel Base { get; set; }
        public pageSectionSelect PageSection { get; set; }
        public object Definition { get; set; }
        public bool IsShop { get; set; }

        public PageSectionType PageSectionType
        {
            get
            {
                return (PageSectionType)PageSection.PageSectionTypeID;
            }
        }

        public dgFixedPageSection? BespokeContentPageSection
        {
            get
            {
                if (Enum.TryParse<dgFixedPageSection>($"{PageSection.PageSectionID}", out dgFixedPageSection dgfps))
                    return dgfps;
                else
                    return null;
            }
        }

        public PageSectionViewModel(
            pageSectionSelect pageSection,
            BaseViewModel baseViewModel,
            object sectionDefinition,
            bool isShop,
            bool? pagingOverride = null)
        {
            PageSection = pageSection;
            Base = baseViewModel;
            Definition = sectionDefinition;
            IsShop = isShop;
            if (pagingOverride != null)
            {
                PageSection.Paging = pagingOverride.Value;
            }
        }
    }
}