using Kendo.Mvc.Extensions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.AlternateColourway;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Carousel;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.ImgSize;
using wwwDesignersGuild.Models.OrderForm;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.ProductReview;
using wwwDesignersGuild.Models.RelatedProductPage;
using wwwDesignersGuild.Models.Resource;
using wwwDesignersGuild.Models.ResourceFetch;
using wwwDesignersGuild.Models.TechInfo;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.ViewModels
{
    public class ProductPageViewModel : BaseViewModel
    {
        public bool AllProductsUnavailable { get; set; }

        public bool UseSizeButtons { get; set; }
        public bool UseOrderFormGrouping { get; set; }
        public bool UsePaintOrderForm { get; set; }

        public object TechInfo { get; set; }
        public object Prices { get; set; }

        [JsonIgnore]
        public SocialMediaShareViewModel SocialMediaViewModel { get; set; }

        public productPageCacheSelect Definition { get; set; }
        public cmsRealTimeStock Stock { get; set; }
        public CarouselViewModel Carousel { get; set; }
        public OrderFormItemViewModel MainProduct { get; set; }
        public List<OrderFormItemViewModel> OtherProducts { get; set; }
        public List<ProductPagePodViewModel> RelatedProductPages { get; set; }
        public List<ProductPageRelatedSearch> RelatedSearches
        {
            get 
            {
                return Definition.RelatedSearches;
            }
            set 
            { 
            
            } 
        }
        public IEnumerable<OrderFormItemViewModel> AllProducts
        {
            get
            {
                if (!AllProductsUnavailable)
                {
                    var allProducts = new List<OrderFormItemViewModel>
                    {
                        MainProduct
                    };

                    allProducts.AddRange(OtherProducts);

                    return allProducts;
                }
                else
                {
                    return new List<OrderFormItemViewModel>();
                }
            }
        }

        public IEnumerable<OrderFormGroupViewModel> OrderFormGroups 
        {
            get
            {
                var autoOpenFirstSize = AllProducts
                    .Where(p => p.PrimaryProduct)
                    .GroupBy(p => new { OrderFormGroupID = p.OrderFormGroupID == null ? (int?)-1 : p.OrderFormGroupID }).Count() == 1;

                return AllProducts
                    .GroupBy(p => new { p.OrderFormGroupID, p.OrderFormGroup, p.PrimaryProduct })
                    .Select(g => new OrderFormGroupViewModel(
                        orderFormGroupId: g.Key.OrderFormGroupID,
                        name: g.Key.OrderFormGroup,
                        useSizeButtons: UseSizeButtons,
                        autoOpenFirstSize: g.Key.PrimaryProduct && autoOpenFirstSize,
                        autoOpenPartNumber: AutoOpenPartNumber,
                        items: g,
                        objMaster: MasterDefinition));
            }
        }

        public List<AlternateColourway> AlternateColourways
        {
            get
            {
                if (JObject.FromObject(Definition.json)["alternateColourways"].Type != JTokenType.Null)
                    return JObject.FromObject(Definition.json)["alternateColourways"].Value<JArray>().ToObject<List<AlternateColourway>>();
                else
                    return new List<AlternateColourway>();
            }
        }

        public object ProductPagePrice
        {
            get
            {
                if (Prices != null)
                {
                    return JObject.FromObject(Prices).SelectToken("productPages").Children().Where(p => p.SelectToken("productPageId").Value<int>() == Definition.productPageId).Select(p => p.SelectToken("productPage")).Select(p => p.SelectToken("productPagePrice")).SingleOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }

        private int ReviewCount
        {
            get
            {
                if (JObject.FromObject(Definition.json).SelectToken("reviewCount").Type != JTokenType.Null)
                {
                    return JObject.FromObject(Definition.json).SelectToken("reviewCount").Value<int>();
                }

                return 0;
            }
        }

        public byte TemplateID
        {
            get
            {
                if (JObject.FromObject(Definition.json).SelectToken("templateId").Type != JTokenType.Null)
                {
                    return JObject.FromObject(Definition.json).SelectToken("templateId").Value<byte>();
                }

                return 0;
            }
        }

        private decimal? ReviewAverage
        {
            get
            {
                if (JObject.FromObject(Definition.json).SelectToken("reviewAverage").Type != JTokenType.Null)
                {
                    return JObject.FromObject(Definition.json).SelectToken("reviewAverage").Value<decimal>();
                }

                return null;
            }
        }

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
        }

        private int RemainingStars
        {
            get
            {
                return 5 - ReviewWholeStars - (ReviewHalfStar ? 1 : 0);
            }
        }

        public IHtmlString ReviewStarsDisplay()
        {
            if (ReviewCount > 0)
            {
                var sb = new StringBuilder();

                sb.AppendLine(@"<a href=""#reviews-panel"" class=""product-page-rating"" onclick=""bootstrap.Collapse.getOrCreateInstance(document.getElementById('reviews-panel')).show();"">");

                for (var i = 0; i < ReviewWholeStars; i++)
                {
                    sb.AppendLine(@"<i class=""bi bi-star-fill""></i>");
                }

                if (ReviewHalfStar)
                {
                    sb.AppendLine(@"<i class=""bi bi-star-half""></i>");
                }

                for (var i = 0; i < RemainingStars; i++)
                {
                    sb.AppendLine(@"<i class=""bi bi-star""></i>");
                }

                sb.AppendLine($"<span>({ReviewCount})</span>");

                sb.AppendLine("</a>");

                return new HtmlString($"{sb}");
            }
            else
            {
                return null;
            }
        }

        public IHtmlString Breadcrumbs()
        {
            var ti = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;

            if (!String.IsNullOrEmpty(Definition.breadcrumb))
            {
                return new HtmlString($@"<ul class=""breadcrumb"">{ti.ToTitleCase(Definition.breadcrumb.ToLowerInvariant())?.Replace("<Li><Span", @"<li class=""breadcrumb-item active""><span").Replace("<Li>", @"<li class=""breadcrumb-item"">")}</ul>");
            }
            else
            {
                return null;
            }
        }

        public List<ProductReview> Reviews 
        { 
            get
            {
                if (JObject.FromObject(Definition.json).SelectToken("reviews").Type != JTokenType.Null) 
                {
                    return JObject.FromObject(Definition.json)
                        .SelectToken("reviews")
                        .ToObject<List<ProductReview>>()
                        .OrderByDescending(p => p.DateTime)
                        .ToList();
                }
                else
                {
                    return new List<ProductReview>();
                }
            }
        }

        public List<TechInfoCollection> TechInfoCollections { get; set; }

        public List<TechInfoAftercare> TechInfoAftercares { get; set; }

        public List<TechInfoUsage> TechInfoUsages { get; set; }

        public List<TechInfoTest> TechInfoTests { get; set; }

        public string Width
        {
            get
            {
                return JObject.FromObject(TechInfo)?.SelectToken("width")?.Value<string>();
            }
        }

        public string Weight
        {
            get
            {
                return JObject.FromObject(TechInfo)?.SelectToken("weight")?.Value<string>();
            }
        }

        public string HorizontalPatternRepeat
        {
            get
            {
                return JObject.FromObject(TechInfo)?.SelectToken("horizontalPatternRepeat")?.Value<string>();
            }
        }

        public string VerticalPatternRepeat
        {
            get
            {
                return JObject.FromObject(TechInfo)?.SelectToken("verticalPatternRepeat")?.Value<string>();
            }
        }

        public string Martindale
        {
            get
            {
                return JObject.FromObject(TechInfo)?.SelectToken("martindale")?.Value<string>();
            }
        }

        public string Composition
        {
            get
            {
                return JObject.FromObject(TechInfo)?.SelectToken("composition")?.Value<string>();
            }
        }

        public string PatternMatch
        {
            get
            {
                return JObject.FromObject(TechInfo)?.SelectToken("patternMatch")?.Value<string>();
            }
        }

        public string Flamcode
        {
            get
            {
                return JObject.FromObject(TechInfo)?.SelectToken("flamcode")?.Value<string>();
            }
        }

        public string ContractFR
        {
            get
            {
                return JObject.FromObject(TechInfo)?.SelectToken("contractFR")?.Value<string>();
            }
        }

        public string DomesticFR
        {
            get
            {
                return JObject.FromObject(TechInfo)?.SelectToken("domesticFR")?.Value<string>();
            }
        }

        public string Type
        {
            get
            {
                return JObject.FromObject(TechInfo)?.SelectToken("type")?.Value<string>();
            }
        }

        public string AdditionalInfo
        {
            get
            {
                return JObject.FromObject(TechInfo)?.SelectToken("additionalInfo")?.Value<string>();
            }
        }

        public string RollLength
        {
            get
            {
                return JObject.FromObject(TechInfo)?.SelectToken("rollLength")?.Value<string>();
            }
        }

        public string PileHeight
        {
            get
            {
                return JObject.FromObject(TechInfo)?.SelectToken("pileHeight")?.Value<string>();
            }
        }

        public string WideWidthFabricDefaultPoleWidth
        {
            get
            {
                return JObject.FromObject(TechInfo)?.SelectToken("wideWidthFabricDefaultPoleWidth")?.Value<string>();
            }
        }

        public string MaxFoldLength
        {
            get
            {
                return JObject.FromObject(TechInfo)?.SelectToken("maxFoldLength")?.Value<string>();
            }
        }

        [JsonIgnore]
        public string TechInfoDisplay
        {
            get
            {
                var result = "";

                if (TechInfoCollections.Any())
                {
                    result += $@"<dt>{HtmlHelperExtra.Resource("Collections", LanguageID, CountryID, Date)}</dt>";

                    result += $@"<dd>";

                    foreach (var c in TechInfoCollections)
                    {
                        result += $@"<a class=""d-block mb-1"" href=""{dgFramework.buildUrl(dgContentType.collectionPage, 
                            c.CollectionID,
                            MasterDefinition.UrlBreakdown.countryDetails,
                            null,
                            null,
                            null,
                            c.URLSemantics,
                            new[] 
                            {
                                "countryOverride",
                                "dateOverride"
                            })}"">{c.Name}</a>";

                        if (!String.IsNullOrEmpty(c.BrochureURL))
                        {
                            result += $@"<a class=""d-block mb-1"" href=""{c.BrochureURL}"">{HtmlHelperExtra.Resource("ViewBrochure", LanguageID, CountryID, Date)}</a>";
                        }
                    }

                    result += "</dd>";
                }

                if (!String.IsNullOrEmpty(Composition))
                {
                    result += $@"<dt>{HtmlHelperExtra.Resource("Composition", LanguageID, CountryID, Date)}</dt><dd>{Composition}</dd>";
                }

                if (!String.IsNullOrEmpty(Flamcode))
                {
                    result += $@"<dt>{HtmlHelperExtra.Resource("FlamCode", LanguageID, CountryID, Date)}</dt><dd>{Flamcode}</dd>";
                }

                if (!String.IsNullOrEmpty(Width))
                {
                    if (Width != "0 cm")
                    {
                        result += $@"<dt>{HtmlHelperExtra.Resource("Width", LanguageID, CountryID, Date)}</dt><dd>{Width}</dd>";
                    }
                }

                if (!String.IsNullOrEmpty(Weight))
                {
                    result += $@"<dt>{HtmlHelperExtra.Resource("Weight", LanguageID, CountryID, Date)}</dt><dd>{Weight}</dd>";
                }

                if (!String.IsNullOrEmpty(PileHeight))
                {
                    result += $@"<dt>{HtmlHelperExtra.Resource("PileHeight", LanguageID, CountryID, Date)}</dt><dd>{PileHeight}</dd>";
                }

                if (!String.IsNullOrEmpty(Martindale))
                {
                    result += $@"<dt>{HtmlHelperExtra.Resource("Martindale", LanguageID, CountryID, Date)}</dt><dd>{Martindale}</dd>";
                }

                if (!String.IsNullOrEmpty(PatternMatch))
                {
                    result += $@"<dt>{HtmlHelperExtra.Resource("PatternMatch", LanguageID, CountryID, Date)}</dt><dd>{PatternMatch}</dd>";
                }

                if (!String.IsNullOrEmpty(RollLength))
                {
                    result += $@"<dt>{HtmlHelperExtra.Resource("RollLength", LanguageID, CountryID, Date)}</dt><dd>{RollLength}</dd>";
                }

                if (!String.IsNullOrEmpty(VerticalPatternRepeat))
                {
                    result += $@"<dt>{HtmlHelperExtra.Resource("VerticalPatternRepeat", LanguageID, CountryID, Date)}</dt><dd>{VerticalPatternRepeat}</dd>";
                }

                if (!String.IsNullOrEmpty(HorizontalPatternRepeat))
                {
                    result += $@"<dt>{HtmlHelperExtra.Resource("HorizontalPatternRepeat", LanguageID, CountryID, Date)}</dt><dd>{HorizontalPatternRepeat}</dd>";
                }

                if (!String.IsNullOrEmpty(MaxFoldLength))
                {
                    result += $@"<dt>{HtmlHelperExtra.Resource("MaxFoldLength", LanguageID, CountryID, Date)}</dt><dd>{MaxFoldLength}</dd>";
                }

                if (!String.IsNullOrEmpty(WideWidthFabricDefaultPoleWidth))
                {
                    result += $@"<dt>{HtmlHelperExtra.Resource("WideWidthFabricDefaultPoleWidth", LanguageID, CountryID, Date)}</dt><dd>{WideWidthFabricDefaultPoleWidth}</dd>";
                }

                if (TechInfoTests.Any())
                {
                    result += $@"<dt>{HtmlHelperExtra.Resource("TestsPassed", LanguageID, CountryID, Date)}</dt>";

                    result += $@"<dd>";

                    foreach (var t in TechInfoTests)
                    {
                        result += $@"<span class=""d-block"">{t.Test}</span>";
                    }

                    result += "</dd>";
                }

                if (TechInfoUsages.Any())
                {
                    result += $@"<dt>{HtmlHelperExtra.Resource("ProductUsage", LanguageID, CountryID, Date)}</dt>";

                    result += $@"<dd>";

                    foreach (var u in TechInfoUsages)
                    {
                        result += $@"<div class=""d-flex align-items-center"">";

                        result += $@"<img width=""32"" height=""32"" class=""usage-icon"" title=""{u.Usage}"" alt=""{u.Usage}"" src=""https://www.designersguild.com/images/icons/techinfo/usage/{u.UsageID}.gif"" />";

                        result += $@"<span class=""d-block"">{u.Usage}</span>";

                        result += $@"</div>";
                    }

                    result += "</dd>";
                }

                if (TechInfoAftercares.Any())
                {
                    result += $@"<dt>{HtmlHelperExtra.Resource("ProductAfterCare", LanguageID, CountryID, Date)}</dt>";

                    result += $@"<dd>";

                    foreach (var a in TechInfoAftercares)
                    {
                        result += $@"<div class=""d-flex align-items-center"">";

                        result += $@"<img width=""32"" height=""32"" class=""usage-icon"" title=""{a.Aftercare}"" alt=""{a.Aftercare}"" src=""https://www.designersguild.com/images/icons/techinfo/aftercare/{a.AftercareID}.gif"" />";

                        result += $@"<span class=""d-block"">{a.Aftercare}</span>";

                        result += $@"</div>";
                    }

                    result += "</dd>";
                }

                if (!String.IsNullOrEmpty(AdditionalInfo))
                {
                    result += $@"<dt>{HtmlHelperExtra.Resource("AdditionalInfo", LanguageID, CountryID, Date)}</dt><dd>{AdditionalInfo}</dd>";
                }

                return result;
            }
        }

        public string TechInfoDisplayBase64
        {
            get
            {
                if (!String.IsNullOrEmpty(TechInfoDisplay))
                {
                    return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(TechInfoDisplay));
                }
                else
                {
                    return null;
                }
            }
        }

        public bool IsShop { get; set; }

        public bool IsLifestyle
        {
            get
            {
                return HttpContext.Current.Request.QueryString.AllKeys.Contains("lifestyle");
            }
        }

        public string AutoOpenPartNumber
        {
            get
            {
                return HttpContext.Current.Request.QueryString["productCode"];
            }
        }

        protected ProductPageViewModel(
            NewMasterDefinition objMaster, 
            VisitorSelect objVisitor,
            productPageCacheSelect objCacheSelect,
            bool isShop,
            bool verifiedBot) : base(objMaster, objVisitor, verifiedBot)
        {
            UseSizeButtons = false;
            UseOrderFormGrouping = false;
            UsePaintOrderForm = false;
            Definition = objCacheSelect;
            IsShop = isShop;

            SocialMediaViewModel = new SocialMediaShareViewModel(directionClass: "dropdown", buttonOnly: false, objBase: this);
        }

        protected async Task Initialise(bool injectProductNameIntoProperties = false)
        {
#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            await base.Initialise(VerifiedBot);

            var imageIds = Definition.Images.Select(p => p.imageId).Distinct().ToList();

            if (Definition.productTypeHtmlSection != null && Definition.productTypeHtmlSection.ImageIDs.Any())
            {
                imageIds.AddRange(Definition.productTypeHtmlSection.ImageIDs);

                imageIds = imageIds.Distinct().ToList();
            }

            var objProductPage = new cmsProductPage();

            var objDb = new databaseDataContext();

            var stockTask = new cmsOrderForm().loadOrderFormFromCms
                (
                    Visitor.visitorDbId,
                    Visitor.memberId,
                    MasterDefinition.UrlBreakdown.ContentId,
                    MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
                    MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                    Visitor.memberTradeAccountID,
                    Visitor.memberDisplayTradePrice,
                    HttpContext.Current.Request.QueryString.AllKeys.Contains("lifestyle"),
					base.MasterDefinition.UrlBreakdown.CountryAwareness.Match,
                    MasterDefinition.UrlBreakdown.ContentDate
                );

            var techInfoTask = objProductPage.loadTechInfoFromCms(MasterDefinition.UrlBreakdown.ContentId,
                MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
                MasterDefinition.UrlBreakdown.ContentDate,
                Convert.ToInt16(System.Web.Configuration.WebConfigurationManager.AppSettings["keepCacheInMinutes"]));

            var fetchHotspots = true;

            // async magic juice

            if (fetchHotspots)
            {
                var hotspotsTask = base.InitialiseHotspots(imageIds, base.MasterDefinition.UrlBreakdown.CountryAwareness.Match);

                await Task.WhenAll(stockTask, techInfoTask, hotspotsTask);

                if (Definition.productTypeHtmlSection != null && Definition.productTypeHtmlSection.ImageIDs.Any())
                {
                    await hotspotsTask;

                    var hotspots = base.Hotspots.ToList();

                    var productTypeImageIds = Definition.productTypeHtmlSection.ImageIDs.ToList();

                    Definition.productTypeHtmlSectionImageHotspots = hotspots.Where(h => productTypeImageIds.Contains(h.ImageID)).ToList();
                }

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"ProductPageViewModel stock, tech & hotspot tasks complete {sw.Elapsed}");
#endif
            }
            else
            {
                await Task.WhenAll(stockTask, techInfoTask);

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"ProductPageViewModel stock & tech tasks complete {sw.Elapsed}");
#endif
            }

            var resourcesTask = new cmsResource().loadResourceFromCms(
                    LanguageID,
                    CountryID,
                    Date);

            await Task.WhenAll(stockTask, techInfoTask, resourcesTask);

            Stock = await stockTask;
            
            var techInfo = await techInfoTask;

            var resourceDefinition = await resourcesTask;

            // yum, resourcesTask

            TechInfo = JObject.FromObject(techInfo?.json);

            TechInfoCollections = JObject.FromObject(TechInfo)?.SelectToken("collections")?.ToObject<List<TechInfoCollection>>()
                    ?? new List<TechInfoCollection>();

            TechInfoAftercares = JObject.FromObject(TechInfo)?.SelectToken("aftercares")?.ToObject<List<TechInfoAftercare>>()
                    ?? new List<TechInfoAftercare>();

            TechInfoUsages = JObject.FromObject(TechInfo)?.SelectToken("usages")?.ToObject<List<TechInfoUsage>>()
                    ?? new List<TechInfoUsage>();

            TechInfoTests = JObject.FromObject(TechInfo)?.SelectToken("tests")?.ToObject<List<TechInfoTest>>()
                    ?? new List<TechInfoTest>();

            var json = Definition.json;

            Definition.PageJSON = JsonConvert.SerializeObject(json);

            var productPageIds = new List<int>
            {
                Definition.productPageId
            };

            var rpps = JObject.FromObject(Definition.json)
                                           .SelectToken("relatedProductPages")
                                           .ToObject<List<ProductPageFilterablePod>>();

            if (rpps != null)
            {
                RelatedProductPages = rpps.Select(rpp => new ProductPagePodViewModel(MasterDefinition, rpp, !IsShop, dgFramework.IsMobileDevice() ? ImageSize.Six : ImageSize.Two))
                                          .ToList();
            }
            else
            {
                RelatedProductPages = new List<ProductPagePodViewModel>();
            }

            productPageIds.AddRange(RecentlyViewed.Select(p => p.ProductPageID));

            if (RelatedProductPages != null)
                productPageIds.AddRange(RelatedProductPages.Select(p => p.ProductPageID));

            productPageIds = productPageIds.Distinct().ToList();

            var objPrices = objDb.ProductPage_Price_Select(
                new PassProductPageIDs ()
                {
                    productPageIds = productPageIds,
                    objMaster = MasterDefinition
				},
                Visitor);

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"ProductPageViewModel prices task complete {sw.Elapsed}");
            //System.Diagnostics.Debug.WriteLine($"ProductPageViewModel stock, tech & hotspot tasks complete {sw.Elapsed}");
#endif

            Prices = objPrices.json;

            if (Prices != null)
            {
                var prices = JObject.FromObject(Prices)
                    .SelectToken("productPages")
                    .Children()
                    .Select(p => p.SelectToken("productPage"))
                    .Select(p => new
                    {
                        productPageId = p.SelectToken("productPageId").Value<int>(),
                        price = p.SelectToken("productPagePrice").ToObject<ProductPagePodPrice>()
                    });

                if (RelatedProductPages != null)
                {
                    RelatedProductPages.ForEach(rp =>
                    {
                        rp.ProductPage.ProductPagePrice = prices.Where(p => p.productPageId == rp.ProductPageID).Select(p => p.price).SingleOrDefault();
                        rp.ProductPage.ReviewCount = 0;
                    });
                }

                if (RecentlyViewed != null)
                {
                    RecentlyViewed.ForEach(rv =>
                    {
                        rv.ProductPage.ProductPagePrice = prices.Where(p => p.productPageId == rv.ProductPageID).Select(p => p.price).SingleOrDefault();
                        rv.ProductPage.ReviewCount = 0;
                    });
                }
            }

            Carousel = new CarouselViewModel(
                carouselId: $"#product-page-{MasterDefinition.UrlBreakdown.ContentId}-carousel",
                wrapperCssClass: null,
                aspectRatio: 1.0m,
                heading: Definition.heading,
                images: Definition.Images
                                  .Where(i => i.templateImageId == 2)
                                  .OrderBy(i => i.sortOrder)
                                  .Select(i => new CarouselImage(i.imageId, i.sortOrder, i.altText, ImageSize.Six, null, i.maxWidth)),
                this,
                true,
                null,
                true,
                Definition.videoUrl,
                Definition.videoThumbnail
            )
            {
                DisplayThumbnails = true
            };

            var abort = false;

            if (JObject.FromObject(Stock.orderForm)["orderFormMessage"] != null)
            {
                if (JObject.FromObject(Stock.orderForm).SelectToken("orderFormMessage").Value<string>() == "Sorry, all products on this page are presently unavailable.")
                {
                    abort = true;
                }
            }

            if (!abort)
            {
                MainProduct = new OrderFormItemViewModel(JObject.FromObject(Stock.orderForm).SelectToken("products")?.FirstOrDefault(), MasterDefinition, this, false, ImageSize.One, injectProductNameIntoProperties);

                OtherProducts = JObject.FromObject(Stock.orderForm)
                    .SelectToken("products")
                    .Skip(1)
                    .Select(p => new OrderFormItemViewModel(p, MasterDefinition, this, false, ImageSize.One, injectProductNameIntoProperties))
                    .ToList();
            }
            else
            {
                AllProductsUnavailable = true;
                OtherProducts = new List<OrderFormItemViewModel>();
            }

            CustomCanonical = Definition.CustomCanonical;

            var result = new List<ResourceResult>();

            var required = new List<string>() {
                "FormsErrorEmail",
                "FormsErrorEmailFormat",
                "ThankYou",
                "ThankYouEmailSent",
                "PleaseProvideWidth",
                "CalculatorMustBeBetween",
                "Width",
                "Height",
                "Length",
                "PleaseProvideHeight",
                "PleaseProvideWidth",
                "PleaseProvideLength",
                "CloseAndReturnToProductPage",
                "ViewBasket",
                "MajorError" };

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

            NoIndex = Definition.noIndex;

            NoFollow = Definition.noFollow;

#if DEBUG
            sw.Stop();

            System.Diagnostics.Debug.WriteLine($"ProductPageViewModel initialised in {sw.Elapsed}");
#endif
        }

        //public override string ToString()
        //{
        //    return JsonConvert.SerializeObject(this, Formatting.Indented);
        //}
    }
}