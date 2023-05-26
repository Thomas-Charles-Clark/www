using Braintree;

using DevExpress.Web;

using iText.Layout.Renderer;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Banner;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.DotDigital;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Hotspot;
using wwwDesignersGuild.Models.ImgSize;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.RecentlyViewed;
using wwwDesignersGuild.Models.Resource;
using wwwDesignersGuild.Models.ResourceFetch;
using wwwDesignersGuild.Models.Scrapbook;
using wwwDesignersGuild.Util;

namespace wwwDesignersGuild.ViewModels
{
    public class BaseViewModel
    {
        public bool NoFollow { get; set; }
        public bool NoIndex { get; set; }
        public string CustomCanonical { get; set; }
        public string AttributeFilterText { get; set; }
        public string AttributeSignatureText { get; set; } // this comes from dbo.FilterIndexCombination_Language

        public bool UserInCorrectSite
        {
            get
            {
                return CorrectSitesForUser.Any() ? CorrectSitesForUser.First().CountryID == CountryID : true;
            }
        }

        public bool CountryCorrectorDismissed
        {
            get
            {
                return Convert.ToBoolean(HttpContext.Current.Session["countryCorrectorDismissed"]?.ToString() ?? "false");
            }
        }

        public List<countryItem> CorrectSitesForUser { get; set; }

        public List<ResourceResult> PrefetchedResources { get; set; }

        public List<Snippet> PrefetchedSnippets { get; set; }

        public IEnumerable<bannerLocation> HeaderBanners { get; set; }
        public bool HasHeaderBanners 
        { 
            get
            {
                return HeaderBanners.Where(bl => bl.banners?.Any() == true).Any();
            }
        }

        public IEnumerable<bannerLocation> FooterBanners { get; set; }
        public bool HasFooterBanners
        {
            get
            {
                return FooterBanners.Where(bl => bl.banners?.Any() == true).Any();
            }
        }

        public IEnumerable<bannerLocation> EcommercePopupBanners { get; set; }

        public bool HasEcommercePopupBanners
        {
            get
            {
                return EcommercePopupBanners.Where(bl => bl.banners?.Any() == true).Any();
            }
        }

        public IEnumerable<bannerLocation> Banners { get; set; } 

        public IEnumerable<ImageHotspot> Hotspots { get; set; }

        public List<ProductPagePodViewModel> RecentlyViewed { get; set; }

        public NewMasterDefinition MasterDefinition { get; set; }

        public VisitorSelect Visitor { get; set; }

        [JsonIgnore]
        public BasketGenerateQuick Basket { get; set; }

        public DotDigitalTrackingBehaviourProfile DotDigitalBehaviourProfile { get; set; }

        public cmsScrapbookImagesDefinition Scrapbook { get; set; }

        public bool TradeOrderingAllowed
        {
            get
            {
                return Boolean.Parse($"{HttpContext.Current.Session["tradeOrderingAllowed"]}" ?? "false");
            }
        }

        public string SearchTerm
        {
            get
            {
                return HttpContext.Current.Request["search-term"];
            }
        }

        public string ProductType
        {
            get
            {
                return HttpContext.Current.Request.QueryString["product-type"];
            }
        }

        public byte LanguageID
        {
            get
            {
                return MasterDefinition.UrlBreakdown.countryDetails.LanguageID;
            }
        }

        public short CountryID
        {
            get
            {
                return MasterDefinition.UrlBreakdown.countryDetails.CountryID;
            }
        }

        public DateTime Date
        {
            get
            {
                return MasterDefinition.UrlBreakdown.ContentDate;
            }
        }

        public dgContentType ContentType
        {
            get
            {
                return MasterDefinition.UrlBreakdown.ContentType;
            }
        }

        public int ContentID
        {
            get
            {
                return MasterDefinition.UrlBreakdown.ContentId;
            }
        }

        public byte? SiteSectionID
        {
            get
            {
                return MasterDefinition.SiteSectionID;
            }
        }

        public bool ShowRecentlyViewed { get; set; }

        public string Partial
        {
            get
            {
                return HttpContext.Current.Request.QueryString["partial"];
            }
        }

        public ProductType CategoryPageProductType { get; set; }

        // This sucks but is temporarily necessary to get the mobile menu updated throughout
        public BaseViewModel(masterDefinition objMaster)
        {
            MasterDefinition = NewMasterDefinition.Create(objMaster.TitleTag,
                objMaster.MetaDescription,
                objMaster.BackInStockReminder,
                objMaster.ItemsInBasket,
                objMaster.UrlBreakdown,
                objMaster.googleHrefs,
                HttpContext.Current.Session["memberTradeAccountID"] != null);

            HeaderBanners = new List<bannerLocation>();
            FooterBanners = new List<bannerLocation>();
            EcommercePopupBanners = new List<bannerLocation>();
            Banners = new List<bannerLocation>();
            Hotspots = new List<ImageHotspot>();
            ShowRecentlyViewed = true;
            PrefetchedResources = new List<ResourceResult>();
            PrefetchedSnippets = new List<Snippet>();
            NoFollow = false;
            NoIndex = false;
            CustomCanonical = null;
            CategoryPageProductType = null;
        }

        protected BaseViewModel(NewMasterDefinition objMaster, VisitorSelect objVisitor)
        {
            MasterDefinition = objMaster;
            Visitor = objVisitor;
            RecentlyViewed = new List<ProductPagePodViewModel>();
            HeaderBanners = new List<bannerLocation>();
            FooterBanners = new List<bannerLocation>();
            EcommercePopupBanners = new List<bannerLocation>();
            Banners = new List<bannerLocation>();
            Hotspots = new List<ImageHotspot>();
            ShowRecentlyViewed = true;
            PrefetchedResources = new List<ResourceResult>();
            PrefetchedSnippets = new List<Snippet>();
            NoFollow = false;
            NoIndex = false;
            CustomCanonical = null;
            CategoryPageProductType = null;
        }

        protected BaseViewModel(BaseViewModel bvm)
        {
            MasterDefinition = bvm.MasterDefinition;
            Visitor = bvm.Visitor;
            RecentlyViewed = bvm.RecentlyViewed;
            HeaderBanners = bvm.HeaderBanners;
            FooterBanners = bvm.FooterBanners;
            EcommercePopupBanners = bvm.EcommercePopupBanners;
            Banners = bvm.Banners;
            Hotspots = bvm.Hotspots;
            ShowRecentlyViewed = bvm.ShowRecentlyViewed;
            PrefetchedResources = bvm.PrefetchedResources;
            PrefetchedSnippets = bvm.PrefetchedSnippets;
            NoFollow = bvm.NoFollow;
            NoIndex = bvm.NoIndex;
            CustomCanonical = bvm.CustomCanonical;
            CategoryPageProductType = bvm.CategoryPageProductType;
        }

        public BaseViewModel()
        {
            RecentlyViewed = new List<ProductPagePodViewModel>();

            HeaderBanners = new List<bannerLocation>();
            FooterBanners = new List<bannerLocation>();
            EcommercePopupBanners = new List<bannerLocation>();
            Banners = new List<bannerLocation>();
            Hotspots = new List<ImageHotspot>();
            ShowRecentlyViewed = true;
            PrefetchedResources = new List<ResourceResult>();
            PrefetchedSnippets = new List<Snippet>();
            NoFollow = false;
            NoIndex = false;
            CustomCanonical = null;
            CategoryPageProductType = null;
        }

        protected async Task Initialise()
        {
#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            var objBasket = new basketRoutines();

            var basketTask = objBasket.loadBasketDetailQuick
                (
                    Visitor.visitorDbId,
                    Visitor.memberId,
                    MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
                    MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                    MasterDefinition.UrlBreakdown.ContentDate,
                    Visitor.memberTradeAccountID,
                    Visitor.memberTradeAccountID.HasValue,
                    Visitor.memberDisplayTradePrice
                );

            var dd = new cmsDotDigital();

            var dotDigitalBehaviourProfileTask = dd.loadDotDigitalBehaviourProfile(
                MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
                MasterDefinition.UrlBreakdown.ContentDate);

            var scrapbook = new cmsScrapbook();

            var scrapbookTask = scrapbook.loadScrapbookFromCmsAsync(
                Visitor.visitorDbId,
                Visitor.memberId,
                MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
                MasterDefinition.UrlBreakdown.ContentDate);

            var objDb = new databaseDataContext();

            var recentlyViewedTask = objDb.FetchRecentlyViewed(
                MasterDefinition.UrlBreakdown.ContentType == Models.Helper.dgContentType.productPage
                ? MasterDefinition.UrlBreakdown.ContentId
                : (int?)null,
                Visitor.visitorDbId,
                MasterDefinition);

            var bannersTask = this.InitialiseBanners();

            var resourcesTask = new cmsResource().loadResourceFromCms(
                    LanguageID,
                    CountryID,
                    Date);

            await Task.WhenAll(
                basketTask,
                dotDigitalBehaviourProfileTask,
                scrapbookTask,
                recentlyViewedTask,
                bannersTask,
                resourcesTask);

            Basket = await basketTask;

            DotDigitalBehaviourProfile = await dotDigitalBehaviourProfileTask;

            Scrapbook = await scrapbookTask;

            var recentlyViewed = await recentlyViewedTask;

            await bannersTask;

            var resourceDefinition = await resourcesTask;

            var requiredResources = new List<string>() {
                "Quantity",
                "QuantityRequired",
                "pleaseGreaterThanEqualTo",
                "pleaseLessThanEqualTo"
            };

            foreach (var r in requiredResources)
            {
                PrefetchedResources.Add(new ResourceResult()
                {
                    Name = r,
                    Value = resourceDefinition.resourceSelects
                                            .Where(p => p.Name == r)
                                            .Select(p => p.Value)
                                            .SingleOrDefault()
                });
            }

            MasterDefinition.ItemsInBasket = Basket.basketDetails.Select(p => p.Quantity).Sum();

            RecentlyViewed = recentlyViewed.json == null
                ? new List<ProductPagePodViewModel>()
                : JObject.FromObject(recentlyViewed.json).SelectToken("productPages").ToObject<IEnumerable<ProductPageFilterablePod>>()
                .Select(rv => new ProductPagePodViewModel(MasterDefinition, rv, false, dgFramework.IsMobileDevice() ? ImageSize.Six : ImageSize.Two)) // it might be wrong to force recently viewed lists to behave as shop
                .ToList();

            RecentlyViewed.ForEach(rv => 
            {
                rv.ProductPage.ReviewCount = 0;
            });

            await SiteCultureCheck();

#if DEBUG
            sw.Stop();

            System.Diagnostics.Debug.WriteLine($"BaseViewModel initialised in {sw.Elapsed}");
#endif
        }

        public async Task FetchRecentlyViewedPrices()
        {
            if (RecentlyViewed.Any())
            {
                var objDb = new databaseDataContext();

                var objPrices = await objDb.ProductPage_Price_SelectAsync(RecentlyViewed.Select(rv => rv.ProductPage.ProductPageID), Visitor, MasterDefinition);

                if (objPrices.json != null)
                {
                    var prices = JObject.FromObject(objPrices.json)
                        .SelectToken("productPages")
                        .Children()
                        .Select(p => p.SelectToken("productPage"))
                        .Select(p => new
                        {
                            productPageId = p.SelectToken("productPageId").Value<int>(),
                            price = p.SelectToken("productPagePrice").ToObject<productPagePodPrice>()
                        });

                    RecentlyViewed.ForEach(rv =>
                    {
                        rv.ProductPage.ProductPagePrice = prices.Where(p => p.productPageId == rv.ProductPage.ProductPageID).Select(p => p.price).SingleOrDefault();
                    });
                }
            }
        }

        public bool IsImageInScrapbook(long imageId)
        {
            return Scrapbook.Images.Where(p => p.ImageID == imageId).Any();
        }

        public async Task InitialiseHotspots(IEnumerable<long> imageIds)
        {
            var db = new databaseDataContext();

            Hotspots = new List<ImageHotspot>();

            if (Framework.HotspotsEnabled)
            {
                var objHotspotHouse = new cmsHotspot();

                var objFetch = new PassHotspotCacheFetch(
                    ContentID,
                    ContentType,
                    Visitor.visitorDbId,
                    Visitor.memberId,
                    Visitor.memberTradeAccountID,
                    Visitor.memberDisplayTradePrice,
                    CountryID,
                    LanguageID,
                    Date,
                    imageIds.Distinct()
                );

                var result = await objHotspotHouse.loadHotspotsFromCmsAsync(objFetch, this);

                Hotspots = result.Hotspots;
            }
        }

        public async Task InitialiseBanners()
        {
            var banners = new cmsBanner();

            var bannerStore = await banners.loadBannersFromCms(
                MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
                MasterDefinition.UrlBreakdown.ContentDate,
                Visitor.memberTradeAccountID.HasValue,
                dgFramework.IsMobileDevice());

            var unrestrictedBySiteSection = bannerStore
                .result
                .bannerLocationGroups
                .Where(p => p.bannerLocations.banners != null)
                .Select(p => p.bannerLocations)
                .Where(p => p.banners != null && p.banners.All(q => q.restrictions.siteSectionIds == null))
                .ToList();

            var restrictedToSiteSections = bannerStore
                .result
                .bannerLocationGroups
                .Where(p => p.bannerLocations.banners != null)
                .Select(p => p.bannerLocations)
                .Where(p => p.banners != null && p.banners.Any(q => q.restrictions.siteSectionIds != null))
                .ToList();

            if (SiteSectionID != null)
            {
                restrictedToSiteSections.ForEach(c =>
                {
                    c.banners = c.banners == null ? new List<templateBanner>() : c.banners.Where(p => p.restrictions.siteSectionIds == null || p.restrictions.siteSectionIds.Contains(SiteSectionID.Value)).ToList();
                });
            }

            var bannersForThisSiteSection = unrestrictedBySiteSection.Concat(restrictedToSiteSections).ToList();

            var unrestrictedByPage = bannersForThisSiteSection.Where(p => p.banners != null && p.banners.All(q => q.restrictions.pageIds == null)).ToList();

            var restrictedByPage = bannersForThisSiteSection.Where(p => p.banners != null && p.banners.Any(q => q.restrictions.pageIds != null)).ToList();

            restrictedByPage.ForEach(c =>
            {
                c.banners = c.banners == null ? new List<templateBanner>() : c.banners.Where(p => p.restrictions.pageIds == null || p.restrictions.pageIds.Contains(ContentID)).ToList();
            });

            var availableBanners = unrestrictedByPage.Concat(restrictedByPage).ToList();

            var headerBanners = bannerStore.result.bannerLocationGroups.Where(p => p.name == "Header").Select(p => p.bannerLocations).ToList();

            var headerBannersUnrestrictedBySiteSection = headerBanners.Where(p => p.banners != null && p.banners.Count != 0 && p.banners.All(q => q.restrictions.siteSectionIds == null)).ToList();

            var headerBannersRestrictedToSiteSections = headerBanners.Where(p => p.banners != null && p.banners.Count != 0 && p.banners.Any(q => q.restrictions.siteSectionIds != null)).ToList();

            if (SiteSectionID != null)
            {
                headerBannersRestrictedToSiteSections.ForEach(c =>
                {
                    c.banners = c.banners == null ? new List<templateBanner>() : c.banners.Where(p => p.restrictions.siteSectionIds == null || p.restrictions.siteSectionIds.Contains(SiteSectionID.Value)).ToList();
                });
            }

            var availableHeaderBanners = headerBannersUnrestrictedBySiteSection.Concat(headerBannersRestrictedToSiteSections);

            var headerBannersUnrestrictedByPage = availableHeaderBanners.Where(p => p.banners != null && p.banners.Count != 0 && p.banners.All(q => q.restrictions.pageIds == null)).ToList();

            var headerBannersRestrictedByPage = availableHeaderBanners.Where(p => p.banners != null && p.banners.Count != 0 && p.banners.Any(q => q.restrictions.pageIds != null)).ToList();

            headerBannersRestrictedByPage.ForEach(c =>
            {
                c.banners = c.banners == null ? new List<templateBanner>() : c.banners.Where(p => p.restrictions.pageIds == null || p.restrictions.pageIds.Contains(MasterDefinition.UrlBreakdown.ContentId)).ToList();
            });


            var footerBanners = bannerStore.result.bannerLocationGroups.Where(p => p.name == "Footer").Select(p => p.bannerLocations).ToList();

            var footerBannersUnrestrictedBySiteSection = footerBanners.Where(p => p.banners != null && p.banners.All(q => q.restrictions.siteSectionIds == null)).ToList();

            var footerBannersRestrictedToSiteSections = footerBanners.Where(p => p.banners != null && p.banners.Any(q => q.restrictions.siteSectionIds != null)).ToList();

            if (SiteSectionID != null)
            {
                footerBannersRestrictedToSiteSections.ForEach(c =>
                {
                    c.banners = c.banners == null ? new List<templateBanner>() : c.banners.Where(p => p.restrictions.siteSectionIds == null || p.restrictions.siteSectionIds.Contains(SiteSectionID.Value)).ToList();
                });
            }

            var availableFooterBanners = footerBannersUnrestrictedBySiteSection.Concat(footerBannersRestrictedToSiteSections);

            var footerBannersUnrestrictedByPage = availableFooterBanners.Where(p => p.banners != null && p.banners.All(q => q.restrictions.pageIds == null)).ToList();

            var footerBannersRestrictedByPage = availableFooterBanners.Where(p => p.banners != null && p.banners.Any(q => q.restrictions.pageIds != null)).ToList();

            footerBannersRestrictedByPage.ForEach(c =>
            {
                c.banners = c.banners == null ? new List<templateBanner>() : c.banners.Where(p => p.restrictions.pageIds == null || p.restrictions.pageIds.Contains(ContentID)).ToList();
            });


            var ecommerceBanners = bannerStore.result.bannerLocationGroups.Where(p => p.name == "Pop-up Email Capture").Select(p => p.bannerLocations).ToList();

            var ecommerceBannersUnrestrictedBySiteSection = ecommerceBanners.Where(p => p.banners != null && p.banners.Count != 0 && p.banners.All(q => q.restrictions.siteSectionIds == null)).ToList();

            var ecommerceBannersRestrictedToSiteSections = ecommerceBanners.Where(p => p.banners != null && p.banners.Count != 0 && p.banners.Any(q => q.restrictions.siteSectionIds != null)).ToList();

            if (SiteSectionID != null)
            {
                ecommerceBannersRestrictedToSiteSections.ForEach(c =>
                {
                    c.banners = c.banners == null ? new List<templateBanner>() : c.banners.Where(p => p.restrictions.siteSectionIds == null || p.restrictions.siteSectionIds.Contains(SiteSectionID.Value)).ToList();
                });
            }

            var availableEcommerceBanners = ecommerceBannersUnrestrictedBySiteSection.Concat(ecommerceBannersRestrictedToSiteSections);

            var ecommerceBannersUnrestrictedByPage = availableEcommerceBanners.Where(p => p.banners != null && p.banners.Count != 0 && p.banners.All(q => q.restrictions.pageIds == null)).ToList();

            var ecommerceBannersRestrictedByPage = availableEcommerceBanners.Where(p => p.banners != null && p.banners.Count != 0 && p.banners.Any(q => q.restrictions.pageIds != null)).ToList();

            ecommerceBannersRestrictedByPage.ForEach(c =>
            {
                c.banners = c.banners == null ? new List<templateBanner>() : c.banners.Where(p => p.restrictions.pageIds == null || p.restrictions.pageIds.Contains(MasterDefinition.UrlBreakdown.ContentId)).ToList();
            });

            EcommercePopupBanners = ecommerceBannersUnrestrictedByPage.Concat(ecommerceBannersRestrictedByPage).OrderBy(bl => bl.id).ToList();

            HeaderBanners = headerBannersUnrestrictedByPage.Concat(headerBannersRestrictedByPage).OrderBy(bl => bl.id).ToList();

            FooterBanners = footerBannersUnrestrictedByPage.Concat(footerBannersRestrictedByPage).OrderBy(bl => bl.id).ToList();

            Banners = availableBanners;
        }

        public async Task SiteCultureCheck()
        {
            // localhost don't get any CF headers 
            var ipCountry = HttpContext.Current.Request.Headers["CF-IPCountry-Override"] ?? HttpContext.Current.Request.Headers["CF-IPCountry"] ?? "GB";

            var objCountry = new cmsCountry();

            var objCountryData = await objCountry.countryLanguageSelect(Date);

            var objAllCountries = await objCountry.countrySelectList(null, null);

            // can we identify a target culture based on the incoming CF-IPCountry header value
            if (dgFramework.doesCountryExistFromCode(ipCountry, objCountryData))
            {
                CorrectSitesForUser = objCountryData.countryItems.Where(c => c.CountryCode == ipCountry && c.LanguageID == LanguageID).ToList();

                if (CorrectSitesForUser.Count() == 0)
                {
                    CorrectSitesForUser = objCountryData.countryItems.Where(c => c.CountryCode == ipCountry).ToList();
                }
            }
            else
            {
                // we should at least be able to identify a country from the complete list by code
                var countryFromHeader = objAllCountries.countries.SingleOrDefault(c => c.Code == ipCountry);

                var proxyCountry = objAllCountries.countries.SingleOrDefault(c => c.CountryID == countryFromHeader?.ProxyCountryID);

                CorrectSitesForUser = objCountryData.countryItems.Where(c => c.CountryID == proxyCountry?.CountryID && c.LanguageID == LanguageID).ToList();

                if (CorrectSitesForUser.Count() == 0)
                {
                    CorrectSitesForUser = objCountryData.countryItems.Where(c => c.CountryID == proxyCountry?.CountryID).ToList();
                }
            }
        }
    }
}