using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Web;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Checkout;
using wwwDesignersGuild.Models.CollectionPage;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Hotspot;
using wwwDesignersGuild.Models.ImgSize;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Redirects;
using wwwDesignersGuild.Models.Source;
using wwwDesignersGuild.Models.Trade;
using wwwDesignersGuild.Util;
using wwwDesignersGuild.ViewModels;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.Models.Classes
{
    public class titleItem
    {
        public byte TitleID { get; set; }
        public string Title { get; set; }
    }

    #region Country object


    public class cmsCountryDefinition
    {
        public List<countryRegion> countryRegions { get; set; }
        public List<countryItem> countryItems { get; set; }
        public List<countryTradePriceList> countryTradePriceLists { get; set; }
    }

    public class cmsCountryDropDownDefinition
    {
        public List<countryDropDownItem> countryDropDownItems { get; set; }
    }

    public class countryRegion
    {
        public byte RegionID { get; set; }
        public string Name { get; set; }
        public byte SortOrder { get; set; }
    }

    [JsonObject]
    [Serializable]
    public class countryItem
    {
        public byte CountryLanguageID { get; set; }
        public byte RegionID { get; set; }
        public short CountryID { get; set; }
        public string CountryCode { get; set; }
        public byte LanguageID { get; set; }
        public short SortOrder { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public string DotNetCulture { get; set; }
        public bool ShopEnabled { get; set; }
        public string LanguageCode { get; set; }
        public string LanguageDotNetCulture { get; set; }
        public string MertexCurrencyCode { get; set; }
        public string CurrencyCode { get; set; }
        public byte? PriceListID { get; set; }
        public bool AllowTradeOrders { get; set; }
        public bool AcceptAmazonPay { get; set; }
        public bool BuyOnlineRedirectAllowed { get; set; }
        public bool EcommerceEnabled { get; set; }
        public bool SamplesEnabled { get; set; }
        public string TitleTagSuffix { get; set; }
    }

    public class countryTradePriceList
    {
        public Byte PriceListID { get; set; }
        public Int16 TradePriceListID { get; set; }
    }

    public class countryDropDownItem
    {
        public Int16 CountryID { get; set; }
        public string Code { get; set; }
        public byte LanguageID { get; set; }
        public byte RegionID { get; set; }
        public string LanguageCode { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string URL { get; set; }
        public string URLKeywords { get; set; }
        public Int16 SortOrder { get; set; }
        public int PageID { get; set; }
    }

    #endregion


    #region Country (Price List) object


    public class cmsCountryPriceListDefinition
    {
        public List<Country> countries { get; set; }
        public List<CountyItem> counties { get; set; }
    }

    public class cmsLanguageDefinition
    {
        public List<Language> languages { get; set; }
    }

    #endregion


    #region Snippet object


    public class cmsSnippetDefinition
    {
        public List<Snippet> snippetRows { get; set; }
    }




    #endregion


    #region Menu object


    public class cmsMenuDefinition
    {
        public List<menuGroup> menuGroups { get; set; }
        public List<menuItem> menuItems { get; set; }
        public List<MobileMenuItem> MobileMenuItems { get; set; }
        public bool MenuLoadedFromCache { get; set; }
    }


    public class menuGroup
    {
        public byte MenuID { get; set; }
        public string Name { get; set; }
        public string MessageHTML { get; set; }
        public byte SortOrder { get; set; }
        public string URL { get; set; }
        public bool NewWindow { get; set; }
        public bool ExternalURL { get; set; }
        public string FontClass { get; set; }
        public bool NoFollow { get; set; }
        public string QueryStringParameters { get; set; }
        public Byte? CountryLanguageID { get; set; }
        public bool TrackExternalURL { get; set; }

        public bool HasMessageHTML 
        { 
            get
            {
                return !string.IsNullOrEmpty(MessageHTML);
            }
            set
            {

            }
        }

        public string FinalURL(string siteCountry)
        {
            var result = "";

            if (URL == null)
            {
                return "javascript:;";
            }

            if (!ExternalURL)
            {
                result = $@"/{siteCountry}/{(FontClass == "menu-spacer"
                            ? "javascript:;"
                            : $@"{URL?.TrimStart(Framework.ForwardSlash)}{QueryStringParameters}")}"
                .TrimEnd(Framework.QueryStringTrim);
            }
            else
            {
                result = $@"{(FontClass == "menu-spacer"
                            ? "javascript:;"
                            : $@"{URL}{QueryStringParameters}")}"
                .TrimEnd(Framework.QueryStringTrim);
            }

            return Framework.URLFixup(result);
        }
    }


    public class menuItem
    {
        public byte MenuID { get; set; }
        public byte Column { get; set; }
        public short MenuItemID { get; set; }
        public byte? BannerLocationID { get; set; }
        public string FontClass { get; set; }
        public string Name { get; set; }
        public byte SortOrder { get; set; }
        public string URL { get; set; }
        public bool NewWindow { get; set; }
        public bool Header { get; set; }
        public bool ExternalURL { get; set; }
        public bool NoFollow { get; set; }
        public string QueryStringParameters { get; set; }
        public Byte? CountryLanguageID { get; set; }
        public bool TrackExternalURL { get; set; }
        public string TextAlign { get; set; }
        public bool FirstInColumn { get; set; }

        public string FinalURL(string siteCountry)
        {
            var result = "";

            if (URL == null)
            {
                return "javascript:;";
            }

            if (!ExternalURL)
            {
                result = $@"/{siteCountry}/{(FontClass == "menu-spacer"
                            ? "javascript:;"
                            : $@"{URL?.TrimStart(Framework.ForwardSlash)}{QueryStringParameters}")}"
                .TrimEnd(Framework.QueryStringTrim);
            }
            else
            {
                result = $@"{(FontClass == "menu-spacer"
                            ? "javascript:;"
                            : $@"{URL}{QueryStringParameters}")}"
                .TrimEnd(Framework.QueryStringTrim);
            }

            return Framework.URLFixup(result);
        }
        
        public menuItem ()
        {
            FirstInColumn = false;
        }
    }

    public class MobileMenuItem
    {
        public menuItem MenuItem { get; set; }
        public bool IsMenuItemAlreadyBuilt { get; set; }

        public MobileMenuItem (menuItem objItem)
        {
            IsMenuItemAlreadyBuilt = false;
            MenuItem = objItem;
        }
    }

    public class menuBanner
    {
        public int BannerID { get; set; }
        public byte BannerLocationID { get; set; }
        public byte MenuID { get; set; }
        public string AltText { get; set; }
        public string TitleText { get; set; }
        public string OverlayText { get; set; }
        public byte Version { get; set; }
        public string URL { get; set; }
    }


    #endregion


    #region Transfer objects

    public class passServerSearch
    {
        public masterDefinition masterDefinition { get; set; }
        public int searchResultCount { get; set; }
        public string dateOverrideUrl { get; set; }
        public List<ProductType> productTypes { get; set; }
        public int selectedProductTypeID { get; set; }
        public filteredSearchResult filteredSearchResult { get; set; }
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public string sortIdentifier { get; set; }
    }

    public class passRenderProductPages
    {
        public List<pageSectionProductPageUrl> pageSectionProductPageUrlList { get; set; }
    }

    public class PassTradeAccountRequestForm
    {
        public masterDefinition masterDefinition { get; set; }
        public string heading { get; set; }
        public string subHeading { get; set; }
        public string description { get; set; }
    }

    public class channel
    {
        public byte ChannelID { get; set; }
        public string Name { get; set; }
    }

    public class leadContactType
    {
        public byte LeadContactTypeID { get; set; }
        public string Name { get; set; }
    }

    public class productTypeGroup_Brand_Country
    {
        public byte ProductTypeGroupID { get; set; }
        public string ProductTypeGroupName { get; set; }
        public byte ProductTypeGroupSortOrder { get; set; }
        public byte BrandID { get; set; }
        public string BrandName { get; set; }
        public byte BrandSortOrder { get; set; }
        public bool Available { get; set; }
        public short CountryID { get; set; }
    }

    public class companyType
    {
        public byte CompanyTypeID { get; set; }
        public string Name { get; set; }
    }

    public class byteSelectOption
    {
        public byte Value { get; set; }
        public string Name { get; set; }
    }

    public class passLogin
    {
        public masterDefinition masterDefinition { get; set; }
        public List<CheckoutType> checkoutTypes { get; set; }
        public string braintreeClientKey { get; set; }
        public string braintreeEnvironment { get; set; }
        public string currency { get; set; }
        public decimal provisionalTotal { get; set; }
        public string amazonLoginClientId { get; set; }
        public string amazonClientId { get; set; }
        public string amazonMerchantId { get; set; }
        public string amazonClientUrl { get; set; }
        public List<string> checkoutCountries { get; set; }
        public bool acceptVisa { get; set; }
        public bool acceptMasterCard { get; set; }
        public bool acceptAmex { get; set; }
        public bool acceptDiscover { get; set; }
        public bool acceptMaestro { get; set; }
        public bool acceptMaestroUK { get; set; }
        public bool onlyShowTrade { get; set; }
        public bool tradePermissionPlaceOrder { get; set; }
        public TradeAccountUsersModel tradeAccountModel { get; set; }
        public decimal? subTotal { get; set; }
    }

    public class checkoutProductSelectResult
    {
        public List<gtagProduct> products { get; set; }
        public List<promotionCode> promoCodes { get; set; }
    }

    public class gtagProduct
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class passMemberLogin
    {
        public masterDefinition masterDefinition { get; set; }
        public List<Country> countries { get; set; }
        public List<Language> languages { get; set; }
    }

    public class passPayPalResponse
    {
        public masterDefinition masterDefinition { get; set; }
        public bool checkoutValid { get; set; }
        public bool isExpired { get; set; }
    }
    public class passAmazonResponse
    {
        public masterDefinition masterDefinition { get; set; }
        public bool checkoutValid { get; set; }
        public bool isExpired { get; set; }
    }
    public class passMemberEdit
    {
        public masterDefinition masterDefinition { get; set; }
        public MemberDetail memberDetail { get; set; }
        public List<Country> countries { get; set; }
        public List<Language> languages { get; set; }
        public MarketingPermissionResult marketingPermissions { get; set; }
        public TradeAccountUsersModel tradeAccountModel { get; set; }
    }

    public class webapiSaveTradeUserPermissions
    {
        public int? MemberID { get; set; }
        public string Email { get; set; }
        public bool ManageAccount { get; set; }
        public bool PlaceOrders { get; set; }
        public bool ViewAccounts { get; set; }
        public bool ViewSalesStats { get; set; }
        public bool DownloadAssets { get; set; }
    }

    public class tradeMemberSignUpReturn
    {
        public string errorMessage { get; set; }
    }

    public class passTradeMemberEdit
    {
        public masterDefinition masterDefinition { get; set; }
        public MemberDetail memberDetail { get; set; }
        public tradeOfferDetail tradeOfferDetail { get; set; }
        public tradeAccountNotification tradeAccountMessage { get; set; }
        public string tradeAuthenticationGUID { get; set; }

        public string Heading { get; set; }
        public string SubHeading { get; set; }
    }

    public class passMemberOrderHistory
    {
        public masterDefinition masterDefinition { get; set; }
        public OrderHistory memberOrderHistory { get; set; }
        public MemberAddressSelect memberAddressSelects { get; set; }
        public List<Country> countryItems { get; set; }
        public List<CountyItem> countyItems { get; set; }
    }


    public class passLifeStylePopup
    {
        public int productPageId { get; set; }
        public string basketCountry { get; set; }
        public cmsLifestyleGalleryPopupDefinition cmsLifestyleGalleryPopupDefinition { get; set; }
        public countryItem countryItem { get; set; }
        public DateTime contentDate { get; set; }
    }



    public class passPromotionCheck
    {
        public string promoCodeDetails { get; set; }

        public object promoCodeDetailsJson
        {
            get
            {
                return JsonConvert.DeserializeObject(string.IsNullOrEmpty(promoCodeDetails) ? "[]" : promoCodeDetails);
            }
            set
            {

            }
        }

        public string validation { get; set; }

        public object validationJson
        {
            get
            {
                return JsonConvert.DeserializeObject(string.IsNullOrEmpty(validation) ? "[]" : validation);
            }
            set
            {

            }
        }

        public bool passed
        {
            set { }
            get
            {
                return string.IsNullOrEmpty(validation);
            }
        }
    }



    public class passSnippet
    {
        public byte SnippetLocationID { get; set; }
        public NewMasterDefinition masterDefinition { get; set; }
        public Snippet snippetContent { get; set; }
        public int? height { get; set; }
        public int? width { get; set; }
    }

    public class passCheckoutSinglePage
    {
        public NewMasterDefinition masterDefinition { get; set; }
        public dgCheckoutType checkoutType { get; set; }
        public string amazonClientId { get; set; }
        public string amazonMerchantId { get; set; }
        public string amazonClientUrl { get; set; }

        public passCheckoutAddress checkoutAddressModel { get; set; }
        public passCheckoutPayment checkoutPaymentModel { get; set; }
        public passCheckoutTotals checkoutTotalModel { get; set; }

        public promotionCode promoCode { get; set; }
        public List<gtagProduct> products { get; set; }

        public string email { get; set; }

        public passCheckoutSinglePage(
            NewMasterDefinition md,
            dgCheckoutType ct,
            MemberAddressSelect mas,
            List<Country> countries,
            List<CountyItem> counties,
            checkoutAddressGenerateResult cdo,
            string azcid,
            string azmid,
            string azcurl,
            tradeAccountNotification tam,
            passCheckoutPayment pcp,
            passCheckoutTotals ctm,
            promotionCode pc,
            List<gtagProduct> p,
            string e
            )
        {
            masterDefinition = md;

            checkoutType = ct;

            amazonClientId = azcid;

            amazonMerchantId = azmid;

            amazonClientUrl = azcurl;

            checkoutAddressModel = new passCheckoutAddress()
            {
                masterDefinition = md,
                checkoutMethod = ct,
                countryItems = countries,
                countyItems = counties,
                deliveryOptions = cdo,
                memberAddressSelect = mas,
                tradeAccountMessage = tam
            };

            checkoutPaymentModel = pcp;

            checkoutTotalModel = ctm;

            promoCode = pc;

            products = p;

            email = e;
        }
    }

    public class passCheckoutSaveAddressData
    {
        public long? CheckoutBasketID { get; set; }
        public byte DeliveryOptionID { get; set; }
        public long? MemberAddressID { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public short CountryID { get; set; }
        public string PhoneNumber { get; set; }
        public string UPSAPID { get; set; }
        public string ZipTax { get; set; }
    }


    public class passCheckoutGiftCard
    {
        public NewMasterDefinition masterDefinition { get; set; }
        public checkoutGiftCard checkoutGiftVouchers { get; set; }
        public bool displayGiftCards { get; set; }
    }


    public class passCheckoutGift
    {
        public NewMasterDefinition masterDefinition { get; set; }
        public checkoutGift checkoutGifts { get; set; }
    }


    public class passCheckoutTotals
    {
        public NewMasterDefinition masterDefinition { get; set; }
        public checkoutTotal checkoutSummarys { get; set; }
        public dgCheckoutType checkoutType { get; set; }
    }

    public class passCheckoutHelp
    {
        public NewMasterDefinition masterDefinition { get; set; }
        public string CustomerServicesPhoneNumber { get; set; }
        public string CustomerServicesEmail { get; set; }
    }

    public class passCheckoutId
    {
        public long checkoutId { get; set; }
    }

    public class passCheckoutConfirm
    {
        public long checkoutId { get; set; }
        public NewMasterDefinition masterDefinition { get; set; }
        public checkoutReceipt checkoutReceipt { get; set; }
    }


    public class passCheckoutPayment
    {
        public NewMasterDefinition masterDefinition { get; set; }
        public CheckoutPayment checkoutPayments { get; set; }
        public MarketingPermissionResult marketingPermissions { get; internal set; }
        public dgCheckoutType checkoutMethod { get; set; }
        public List<CheckoutBraintreeCreditCardDetails> savedCards { get; set; }
        public bool isMemberCheckout { get; set; }
        public string braintreeClient { get; set; }
    }


    public class passCheckoutDispatch
    {
        public NewMasterDefinition masterDefinition { get; set; }
        public checkoutDispatchOptions checkoutDispatchOptions { get; set; }
        public string TradeDeliveryURL { get; set; }
        public bool dispatchOptionsAvailable { get; set; }
        //public bool defaultOptionSelected { get; set; }
        public dgCheckoutType checkoutMethod { get; set; }
    }


    public class passPayPalExpress
    {
        public masterDefinition masterDefinition { get; set; }
        public CheckoutPayment checkoutPayments { get; set; }
        public string iFrameSource { get; set; }
    }


    public class passMemberAddress
    {
        public masterDefinition masterDefinition { get; set; }
        public List<CountyItem> countyItems { get; set; }
        public List<Country> countryItems { get; set; }
        public MemberAddressSelect memberAddressSelects { get; set; }
    }


    public class passCheckoutAddress
    {
        public tradeAccountNotification tradeAccountMessage { get; set; }
        public NewMasterDefinition masterDefinition { get; set; }
        public checkoutAddressGenerateResult deliveryOptions { get; set; }
        public List<Country> countryItems { get; set; }
        public List<CountyItem> countyItems { get; set; }
        public MemberAddressSelect memberAddressSelect { get; set; }
        public dgCheckoutType checkoutMethod { get; set; }
    }

    public class tradeAccountNotification
    {
        public bool success { get; set; }
        public string message { get; set; }
    }

    public class passMaster
    {
        public masterDefinition masterDefinition { get; set; }
        public object banners { get; set; }
    }

    public class promotionDetailReturn
    {
        public passPromotionCheck passPromotionCheck { get; set; }
    }

    public class passBasketDetail
    {
        public masterDefinition masterDefinition { get; set; }
        public BasketClasses.Basket basketGenerate { get; set; }
    }


    public class passSocialMediaShare
    {
        public string displayText { get; set; }
        public long? pinterestImageId { get; set; }
        public masterDefinition masterDefinition { get; set; }
    }

    public class passScrapbookPod
    {
        public string ProductType { get; set; }
        public string Heading { get; set; }
        public string ProductCode { get; set; }
        public long? ImageID { get; set; }
        public Int16 ImageWidth { get; set; }
        public bool New { get; set; }
        public string URL { get; set; }
        public decimal? PriceIncMin { get; set; }
        public decimal? PriceIncMax { get; set; }
        public decimal? SalePriceIncMin { get; set; }
        public decimal? SalePriceIncMax { get; set; }
        public string DiscountMessage { get; set; }
        public bool ShowRating { get; set; }
        public int? RatingAvg { get; set; }
        public int? RatingCount { get; set; }
        public bool IsLifestyleGallery { get; set; }
        public int? ProductPageID { get; set; }
        public bool Live { get; set; }
        public Byte AspectRatioID { get; set; }
        public masterDefinition masterDefinition { get; set; }
    }

    public class pod
    {
        public int ProductPageID { get; set; }
        public int? ProductTypeID { get; set; }
        public string URL { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public long? ImageID { get; set; }
        public Int16 ImageWidth { get; set; }
        public bool New { get; set; }
        public decimal? DisplayPriceMin { get; set; }
        public decimal? DisplayPriceMax { get; set; }
        public decimal? DisplaySalePriceMin { get; set; }
        public decimal? DisplaySalePriceMax { get; set; }
        public string DiscountMessage { get; set; }
        public bool ShowRating { get; set; }
        public int? RatingAvg { get; set; }
        public int? RatingCount { get; set; }
        public bool IsLifestyleGallery { get; set; }
        public bool IncludeBasketFunctionality { get; set; }
        public int? ProductID { get; set; }
        public int? StockStatusID { get; set; }
        public bool Sample { get; set; }
        public masterDefinition masterDefinition { get; set; }
        public int? FreeStock { get; set; }
        public string StockUnit { get; set; }
        public string StockStatus { get; set; }
    }

    //public class serverPod
    //{
    //    public int ProductPageID { get; set; }
    //    public byte ProductTypeID { get; set; }
    //    public string URLKeywords { get; set; }
    //    public string Heading { get; set; }
    //    public string SubHeading { get; set; }
    //    public bool New { get; set; }
    //    public decimal? DisplayPriceMin { get; set; }
    //    public decimal? DisplayPriceMax { get; set; }
    //    public decimal? DisplaySalePriceMin { get; set; }
    //    public decimal? DisplaySalePriceMax { get; set; }
    //    public string DiscountMessage { get; set; }
    //    public string Code { get; set; }
    //    public int ProductCount { get; set; }
    //    public long PageSectionID { get; set; }
    //    public decimal? RatingAvg { get; set; }
    //    public short? RatingCount { get; set; }
    //    public short SortOrder { get; set; }
    //    public int? ProductID { get; set; }
    //    public bool IsLifestyleGallery { get; set; }
    //    public bool ShowRating { get; set; }
    //    public int? ImageID { get; set; }
    //    public int? FreeStock { get; set; }
    //    public byte? StockStatusID { get; set; }
    //    public string StockStatus { get; set; }
    //    public string StockUnit { get; set; }
    //}

    public class passCountryPicker
    {
        public masterDefinition masterDefinition { get; set; }
        public cmsCountryDefinition cmsCountryDefinitions { get; set; }
    }

    public class passCountryDropDownPicker
    {
        public masterDefinition masterDefinition { get; set; }
        public cmsCountryDropDownDefinition cmsCountryDropDownDefinitions { get; set; }
    }

    public class passProductPageGrid
    {
        public masterDefinition masterDefinition { get; set; }
        public long PageSectionID { get; set; }
        public string Heading { get; set; }
        public bool HalfWidth { get; set; }
        public bool PreviewColourways { get; set; }
        public bool PreviewColourwaysCircularClip { get; set; }
        //public int pageSectionProductPagesTotalCount { get; set; }
        //public List<pageSectionProductPageStub> pageSectionAllProductPageStubs { get; set; }
        public cmsAttributeDefinition attributeDefinitions { get; set; }
        public List<PageRelated> pageRelateds { get; set; }
        //public List<searchResultRenderCore> usePageSectionProductPages { get; set; }
        //public List<searchResultRenderImage> usePageSectionProductPageImages { get; set; }
        public List<pageSectionAttribute> usePageSectionAttributes { get; set; }
        public cmsAttributeDefinition useAttributeDefinitions { get; set; }
        public List<PageRelated> usePageRelateds { get; set; }
        public bool IsShop { get; set; }
        public bool Paging { get; set; }
        public bool IsLifestyleGallery { get; set; }
        //public DatabaseJSONResponse pageSectionNoFilterProductPages { get; set; }
        public PageSectionJSON productPages { get; set; }
    }

    public class passScrapbook
    {
        public Byte? productTypeId { get; set; }
        public Byte? brandId { get; set; }
        public bool? isNew { get; set; }
        public Byte? top { get; set; }
        public masterDefinition masterDefinition { get; set; }
        public List<ScrapbookImage> scrapbookImages { get; set; }
        public string displayClass { get; set; }
    }

    public class cmsScrapbookImagesDefinition
    {
        public List<ScrapbookImage> Images { get; set; }
    }

    public class ScrapbookImage
    {
        public long ImageID { get; set; }
        public int? ProductPageID { get; set; }
        public string URLKeywords { get; set; }
        public string Heading { get; set; }
        public string ProductCode { get; set; }
        public string ProductType { get; set; }
        public bool New { get; set; }
        public bool Live { get; set; }
        public Byte AspectRatioID { get; set; }
        public byte? ProductTypeID { get; set; }
        public byte? BrandID { get; set; }
        public DateTime CreatedDT { get; set; }
        public DateTime ModifiedDT { get; set; }
        public int MaxWidth { get; set; }
        public decimal AspectRatio { get; set; }
        public short Height 
        { 
            get 
            {
                return (short)(MaxWidth * (1m / (AspectRatio == 0 ? 1 : AspectRatio)));
            }
            private set { }
        }
    }


    public class passViewedProductPage
    {
        public masterDefinition masterDefinition { get; set; }
        public int? ProductPageID { get; set; }
        public string displayClass { get; set; }
        public bool hideHeading { get; set; }
        public bool showCheckoutHeading { get; set; }
        public List<searchResultRenderCore> viewedProductCores { get; set; }
        public List<searchResultRenderImage> viewedProductImages { get; set; }
    }

    public class passHtml
    {
        public masterDefinition masterDefinition { get; set; }
        public long PageSectionID { get; set; }
        public string Heading { get; set; }
        public bool HalfWidth { get; set; }
        public List<PageSectionHTML> pageSectionHtmls { get; set; }
    }

    public class passVideo
    {
        public masterDefinition masterDefinition { get; set; }
        public long PageSectionID { get; set; }
        public string Heading { get; set; }
        public bool HalfWidth { get; set; }
        public List<PageSectionVideo> pageSectionVideos { get; set; }
    }

    public class passAnimation
    {
        public masterDefinition masterDefinition { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public string Description { get; set; }
        public long PageSectionID { get; set; }
        public bool IsHeader { get; set; }
        public bool HalfWidth { get; set; }
        public List<PageSectionAnimation> pageSectionAnimations { get; set; }
        public List<PageSectionAnimationSlide> pageSectionAnimationSlides { get; set; }
        public List<PageSectionAnimationSlideCallToAction> pageSectionAnimationSlideCallsToAction { get; set; }
        public string DiscountMessage { get; set; }
    }

    public class passSlideShow
    {
        public masterDefinition masterDefinition { get; set; }
        public string Heading { get; set; }
        public long PageSectionID { get; set; }
        public bool HalfWidth { get; set; }
        public bool IsLifestyleGallery { get; set; }
        public List<PageSectionSlideShow> pageSectionSlideShows { get; set; }
        public List<PageSectionSlideShowAspect> pageSectionSlideShowAspects { get; set; }
    }

    public class passBranding
    {
        public masterDefinition masterDefinition { get; set; }
        public cmsSearchProductTypeDropDownDefinition searchProductTypeDropDown { get; set; }
    }

    public class passPullDownMenu
    {
        public passMaster passMaster { get; set; }
        public cmsMenuDefinition menuDefinition { get; set; }
    }

    public class passImageUrlBuild
    {
        public string imageId { get; set; }
        public string imageWidth { get; set; }
    }


    public class passPageGrid
    {
        public masterDefinition masterDefinition { get; set; }
        public long PageSectionID { get; set; }
        public string Heading { get; set; }
        public bool HalfWidth { get; set; }
        public List<PageSectionPageSelect> pageSectionPageSelects { get; set; }
        public List<PageSectionPageSelectImage> pageSectionPageSelectImages { get; set; }
    }


    public class passCollectionGrid
    {
        public masterDefinition masterDefinition { get; set; }
        public long PageSectionID { get; set; }
        public string Heading { get; set; }
        public bool HalfWidth { get; set; }
        public List<PageSectionCollection> pageSectionCollections { get; set; }
        public List<PageSectionCollectionImage> pageSectionCollectionImages { get; set; }
    }

    public class passArticleGrid
    {
        public masterDefinition masterDefinition { get; set; }
        public long PageSectionID { get; set; }
        public string Heading { get; set; }
        public bool HalfWidth { get; set; }
        public List<PageSectionArticle> pageSectionArticles { get; set; }
        public List<PageSectionArticleImage> pageSectionArticleImages { get; set; }
        public List<articleTypeSelect> pageSectionArticleTypes { get; set; }
        public List<articleSubTypeSelect> pageSectionArticleSubTypes { get; set; }
    }

    public class passPageForm
    {
        public masterDefinition masterDefinition { get; set; }
        public string Heading { get; set; }
        public bool HalfWidth { get; set; }
        public List<PageSectionForm> pageSectionForms { get; set; }
        public List<PageSectionFormRetailAppointmentAgenda> pageSectionFormRetailAppointmentAgendas { get; set; }
        public List<PageSectionFormRetailAppointmentType> pageSectionFormRetailAppointmentTypes { get; set; }
        public List<Language> languages { get; set; }
        public short? defaultCountryForDropDown { get; set; }
        public List<PageFormCountry> pageFormCountrys { get; set; }
        public List<PageSectionStoreEvent> pageSectionStoreEvents { get; set; }
    }


    public class passPageBannerLocation
    {
        public masterDefinition masterDefinition { get; set; }
        public string Heading { get; set; }
        public bool HalfWidth { get; set; }
        public PageSectionBannerJSON pageSectionBannerJson { get; set; }
        public long PageSectionID { get; set; }
    }


    public class passLandingPage
    {
        public masterDefinition masterDefinition { get; set; }
        public cmsLandingPageDefinition cmsLandingPageDefinitionReadOnly { get; set; }

    }


    public class passCollectionPage
    {
        public masterDefinition masterDefinition { get; set; }
        public cmsCollectionPageDefinition cmsCollectionPageDefinitionReadOnly { get; set; }
    }


    public class passCollectionProductPage
    {
        public masterDefinition masterDefinition { get; set; }
        public cmsCollectionProductPageDefinition cmsCollectionProductPageDefinitionReadOnly { get; set; }
        public string selectedCollection { get; set; }
        public string selectedBrand { get; set; }
        public string selectedProductType { get; set; }
        public string selectedSort { get; set; }
    }


    public class passModularProductPageDefinition
    {
        public masterDefinition masterDefinition { get; set; }
        public cmsProductPageDefinition cmsProductPageDefinitionReadOnly { get; set; }
        public cmsRealTimeStock cmsRealTimeStockReadOnly { get; set; }
        public Basket passBasketGenerate { get; set; }
    }

    public class passProductPageDefinition
    {
        public masterDefinition masterDefinition { get; set; }
        public cmsProductPageDefinition cmsProductPageDefinitionReadOnly { get; set; }
        public cmsRealTimeStock cmsRealTimeStockReadOnly { get; set; }
        public BasketGenerateQuick objBasket { get; set; }
    }

    public class passShoppingPageDefinition
    {
        public masterDefinition masterDefinition { get; set; }
        public cmsProductPageDefinition cmsProductPageDefinitionReadOnly { get; set; }
        public cmsRealTimeStock cmsRealTimeStockReadOnly { get; set; }

    }

    public class passArticlePage
    {
        public masterDefinition masterDefinition { get; set; }
        public cmsArticlePageDefinition cmsArticlePageDefinitionReadOnly { get; set; }
    }

    public class passUnsubscribePage
    {
        public NewMasterDefinition MasterDefinition { get; set; }
        public cmsUnsubscribePageDefinition cmsUnsubscribePageDefinitionReadOnly { get; set; }
        public MemberDetail member { get; internal set; }
        public Guid? guid { get; internal set; }
        public string emailName { get; set; }
        public string state { get; set; }
    }

    public class passResubscribePage
    {
        public NewMasterDefinition MasterDefinition { get; set; }
        public PageLanguageSelect pageLanguageSelect { get; set; }
        public MemberDetail member { get; internal set; }
        public Guid? guid { get; internal set; }
        public string emailName { get; set; }
        public string state { get; set; }
    }


    #endregion


    #region Attribute Database and Cache objects


    public class cmsAttributeDefinition
    {
        public List<AttributeGroupGroup> attributeGroupGroups { get; set; }
        public List<AttributeGroup> attributeGroups { get; set; }
        public List<AttributeGroup_ProductType> attributeGroupProductTypes { get; set; }
        public List<AttributeDetail> attributeDetails { get; set; }
        public List<ProductType> productTypes { get; set; }
    }


    public class ProductPage
    {
        public int ProductPageID { get; set; }
    
        public ProductPage() { }

        public ProductPage(int productPageId)
        {
            ProductPageID = productPageId;
        }
    }


    #endregion


    #region Lifestyle Gallery and Cache objects


    public class cmsLifestyleGalleryPopupDefinition
    {
        public List<productPageHeader> productPageHeaders { get; set; }
        public List<searchResultRenderImage> productPageImages { get; set; }
        public List<productPageAlternateColourway> productPageAlternateColourways { get; set; }
        public List<lifestyleBuyOnline> lifestyleBuyOnlines { get; set; }
        public List<productPageProduct> productPagePrimarys { get; set; }
        public List<productPageRRP> productPageRRPs { get; set; }
    }


    public class lifestyleBuyOnline
    {
        public bool BuyOnline { get; set; }
    }


    #endregion


    #region Product Page Database and Cache objects

    public class productPageCacheSelect
    {
        public int productPageId { get; set; }
        public byte productTypeId { get; set; }
        public byte languageId { get; set; }
        public short countryId { get; set; }
        public bool noIndex { get; set; }
        public bool noFollow { get; set; }
        public byte templateId 
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectToken("templateId").Value<byte>();
            }
        }

        public string titleTag 
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectToken("titleTag").Value<string>();
            }
        }

        public string heading
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectToken("heading").Value<string>();
            }
        }

        public string aka
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectToken("akaHeading")?.Value<string>();
            }
        }

        public string subHeading
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectToken("subHeading").Value<string>();
            }
        }

        public string brand
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectToken("brand").Value<string>();
            }
        }

        public bool isNew
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectToken("isNew").Value<bool>();
            }
        }

        public string metaDescription
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectToken("metaDescription")?.Value<string>();
            }
        }

        public string delivery
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectToken("delivery")?.Value<string>();
            }
        }

        public string deliveryHTMLSection 
        { 
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectToken("deliveryHTMLSection")?.Value<string>();
            }
        }

        public string urlKeywords
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectToken("url").Value<string>();
            }
        }

        public bool isRugOrRunner
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectToken("isRugOrRunner").Value<bool>();
            }
        }

        public string urlSemantics
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectToken("urlSemantics").Value<string>();
            }
        }

        public string videoUrl
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectToken("videoUrl")?.Value<string>();
            }
        }
        
        public string videoThumbnail
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectToken("videoThumbnail")?.Value<string>();
            }
        }

        public HtmlPageSection productTypeHtmlSection
        {
            set
            {

            }
            get
            {
                var obj = JObject.FromObject(json).SelectToken("productTypeHtmlSection");

                if (obj.HasValues)
                {

                    return obj.ToObject<HtmlPageSection>();
                }
                else
                {
                    return null;
                }
            }
        }

        public IEnumerable<ImageHotspot> productTypeHtmlSectionImageHotspots { get; set; } = new List<ImageHotspot>();

        public List<GoogleHref> googleHrefs 
        { 
            set
            {

            }
            get 
            {
                return JObject.FromObject(json).SelectTokens("hreflangs").Children().Select(p => new GoogleHref() 
                { 
                    href = p["href"].Value<string>(),
                    hreflang = p["hreflang"].Value<string>()
                }).ToList();
            } 
        }

        public long? pinterestImageId
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectToken("images").Select(p => p["imageId"].Value<long>()).FirstOrDefault();
            }
        }

        public string breadcrumb
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectToken("breadcrumb").Value<string>();
            }
        }

        public List<productPageImage> Images
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectTokens("images").Children().Select(p => new productPageImage()
                {
                    imageId = JObject.FromObject(p).SelectToken("imageId").Value<long>(),
                    altText = JObject.FromObject(p).SelectToken("altText")?.Value<string>(),
                    titleText = JObject.FromObject(p).SelectToken("titleText")?.Value<string>(),
                    sortOrder = JObject.FromObject(p).SelectToken("sortOrder").Value<byte>(),
                    templateImageId = JObject.FromObject(p).SelectToken("templateImageId").Value<byte>(),
                    maxWidth = JObject.FromObject(p).SelectToken("maxWidth").Value<int?>() ?? 1024
                }).ToList();
            }
        }

        public List<ProductPageRelatedSearch> RelatedSearches
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectTokens("relatedSearches").Children().Select(p => new ProductPageRelatedSearch()
                {
                    target = JObject.FromObject(p).SelectToken("target").Value<string>(),
                    text = JObject.FromObject(p).SelectToken("text").Value<string>()
                }).ToList();
            }
        }

        public List<pageSectionBanner> Banners
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectTokens("banners").Children().Select(p => {
                    if (p.HasValues)
                    {
                        return p.ToObject<pageSectionBanner>();
                    }
                    else
                    {
                        return null;
                    }
                }).ToList();
            }
        }

        public string CustomCanonical
        {
            set
            {

            }
            get
            {
                return JObject.FromObject(json).SelectToken("customCanonical")?.Value<string>();
            }
        }

        public string PageJSON { get; set; }

        public object json 
        { 
            set
            {

            }
            get
            {
                return JsonConvert.DeserializeObject(PageJSON);
            }
        }
    }

    public class productPageImage
    {
        public long imageId { get; set; }
        public byte templateImageId { get; set; }
        public string altText { get; set; }
        public string titleText { get; set; }
        public byte sortOrder { get; set; }
        public int maxWidth { get; set; }

        public productPageImage()
        {

        }

        public productPageImage(
            long imageId,
            byte templateImageId,
            string altText,
            string titleText,
            byte sortOrder = 0
        )
        {
            this.imageId = imageId;
            this.templateImageId = templateImageId;
            this.altText = altText;
            this.titleText = titleText;
            this.maxWidth = 488;
            this.sortOrder = sortOrder;
        }
    }

    public class cmsProductPageDefinition
    {
        public productPageCacheSelect objCacheSelect { get; set; }

        public productPageUnavailableRedirect objUnavailabilityRedirect { get; set; }

        //public List<productPageHeader> productPageHeaders { get; set; }
        //public List<productPageReviewSummary> productPageReviewSummarys { get; set; }
        //public List<productPageReviewLastFive> productPageReviewLastFives { get; set; }
        //public List<productPageInformation> productPageInformations { get; set; }
        //public List<searchResultRenderImage> productPageImages { get; set; }
        //public List<productPageImageHotspot> productPageImageHotspots { get; set; }
        //public List<productPageAlternateColourway> productPageAlternateColourways { get; set; }
        //public List<searchResultRenderCore> productPageRelatedProductPages { get; set; }
        //public List<searchResultRenderImage> productPageRelatedProductPageImages { get; set; }
        //public List<productPageCollection> productPageCollections { get; set; }
        //public List<productPageTechInfoCore> productPageTechInfoCores { get; set; }
        //public List<productPageTechInfoAftercare> productPageTechInfoAftercares { get; set; }
        //public List<productPageTechInfoUsage> productPageTechInfoUsages { get; set; }
        //public List<productPageTechInfoTest> productPageTechInfoTests { get; set; }
        //public List<StockStatusSelect> productPageStockStatuss { get; set; }
        //public List<googleHref> productPageGoogleHrefs { get; set; }
        //public List<productPageBrochure> productPageBrochures { get; set; }
        //public List<productPageType> productPageTypes { get; set; }
        //public List<productPageRRP> productPageRRPs { get; set; }
        //public List<productPageGeneratedWARImage> productPageGeneratedWARImages { get; set; }
        //public List<productPageGeneratedWARImageHotspot> productPageGeneratedWARImageHotspots { get; set; }
        //public List<productPageModuleOption> productPageModuleOptions { get; set; }
        //public List<productPageModule> productPageModules { get; set; }
        //public List<productPageModuleOptionGroup> productPageModuleOptionGroups { get; set; }
    }

    public class productPageUnavailableRedirect 
    {
        public string redirectUrl { get; set; }
    }

    public class productPageHeader
    {
        public int ProductPageID { get; set; }
        public byte TemplateID { get; set; }
        public string URLKeywords { get; set; }
        public string TitleTag { get; set; }
        public string MetaDescription { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public string Description { get; set; }
        public string FurnitureDimensions { get; set; }
        public string FurnitureCutOut { get; set; }
        public bool FurnitureFabricOptions { get; set; }
        public bool FurnitureLeatherOptions { get; set; }
        public bool FurnitureLegFinishes { get; set; }
        public bool FurnitureLegStyles { get; set; }
        public bool FurnitureStitchingOptions { get; set; }
        public string BrochureURL { get; set; }
        public Byte ProductTypeID { get; set; }
        public string FurniturePDF { get; set; }
    }


    public class productPageReviewSummary
    {
        public int? ReviewCount { get; set; }
        public int? AverageRating { get; set; }
    }


    public class productPageReviewLastFive
    {
        public string Code { get; set; }
        public string BasketName { get; set; }
        public long ProductReviewID { get; set; }
        public byte? ReviewPartnerID { get; set; }
        public DateTime CreatedDT { get; set; }
        public byte? Rating { get; set; }
        public string Review { get; set; }
        public string Reply { get; set; }
        public string ReadMoreURL { get; set; }
    }


    public class productPageInformation
    {
        public string AttributeGroup { get; set; }
        public string Attribute { get; set; }
    }


    //public class productPageImage
    //{
    //    public int ProductPageID { get; set; }
    //    public long ImageID { get; set; }
    //    public byte? SortOrder { get; set; }
    //    public string AltText { get; set; }
    //    public string TitleText { get; set; }
    //    public byte TemplateImageID { get; set; }
    //}


    public class productPageImageHotspot
    {
        public long ImageID { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string HotspotHeading { get; set; }
        public long HotspotImageID { get; set; }
        public int ProductPageID { get; set; }
        public string URLKeywords { get; set; }
        public string Code { get; set; }
        public int? ProductID { get; set; }
        public bool BasketOrSample { get; set; }
    }

    public class productPageGeneratedWARImage
    {
        public long ImageID { get; set; }
        public string RoomName { get; set; }
        public string AltText { get; set; }
        public string TitleText { get; set; }
    }

    public class productPageGeneratedWARImageHotspot
    {
        public long ImageID { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string HotspotHeading { get; set; }
        public long HotspotImageID { get; set; }
        public int ProductPageID { get; set; }
        public string URLKeywords { get; set; }
        public string Code { get; set; }
        public int? ProductID { get; set; }
        public bool BasketOrSample { get; set; }
    }

    public class productPageHotspotLink
    {
        public long HotspotImageID { get; set; }
        public string HotspotProductLink { get; set; }
    }

    public class productPageImageHotspotURL
    {
        public long HotspotImageID { get; set; }
        public string HotspotImageURL { get; set; }
    }

    public class productPageHotspotProductID
    {
        public long HotspotImageID { get; set; }
        public int? HotspotProductID { get; set; }
        public bool BasketOrSample { get; set; }
    }

    public class productPageAlternateColourway
    {
        public int ProductPageID { get; set; }
        public int? ProductID { get; set; }
        public string Code { get; set; }
        public string URLKeywords { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public long ImageID { get; set; }
    }
    
    public class productPageCollection
    {
        public short collectionID { get; set; }
        public string Heading { get; set; }
        public string URLKeywords { get; set; }
        public string InteractiveBrochureURL { get; set; }
    }

    public class productPageTechInfoCore
    {
        public double? Width { get; set; }
        public double? Weight { get; set; }
        public double? HorizontalPatternRepeat { get; set; }
        public double? VerticalPatternRepeat { get; set; }
        public double? Martindale { get; set; }
        public short? CompositionID { get; set; }
        public string CompositionName { get; set; }
        public short? PatternMatchID { get; set; }
        public string PatternMatchName { get; set; }
        public short? FlamcodeID { get; set; }
        public string FlamcodeName { get; set; }
        public short? ContractFRID { get; set; }
        public string ContractFRName { get; set; }
        public short? DomesticFRID { get; set; }
        public string DomesticFRName { get; set; }
        public short? TypeID { get; set; }
        public string TypeName { get; set; }
        public string AdditionalInfo { get; set; }
        public string Status { get; set; }
    }

    public class productPageTechInfoAftercare
    {
        public short AftercareID { get; set; }
        public string AftercareName { get; set; }
    }

    public class productPageTechInfoUsage
    {
        public short UsageID { get; set; }
        public string UsageName { get; set; }
    }

    public class productPageTechInfoTest
    {
        public short TestID { get; set; }
        public string TestName { get; set; }
    }

    //public class productPageQualityCore
    //{
    //    public double? Width { get; set; }
    //    public double? Weight { get; set; }
    //    public double? HorizontalPatternRepeat { get; set; }
    //    public double? VerticalPatternRepeat { get; set; }
    //    public int? Martindale { get; set; }
    //    public string Composition { get; set; }
    //    public string Description { get; set; }
    //}


    //public class productPageQualityAftercare
    //{
    //    public byte Code { get; set; }
    //    public string Name { get; set; }
    //}


    //public class productPageQualityFinish
    //{
    //    public byte Code { get; set; }
    //    public string Name { get; set; }
    //}


    //public class productPageQualityUsage
    //{
    //    public byte Code { get; set; }
    //    public string Name { get; set; }
    //}


    //public class productPageQualityTechnical
    //{
    //    public string Name { get; set; }
    //}


    public class productPageStockStatus
    {
        public byte? StockStatusID { get; set; }
        public string StockStatus { get; set; }
    }

    public class collectionBreadcrumb
    {
        public int? PageID { get; set; }
        public int? ParentPageID { get; set; }
        public string URLKeywords { get; set; }
        public string Heading { get; set; }
    }

    public class productPageBrochure
    {
        public Int16 brochureID { get; set; }
        public string description { get; set; }
        public string pdfPath { get; set; }
    }

    public class searchProductType
    {
        public byte ProductTypeID { get; set; }
        public string Name { get; set; }
    }

    public class productPageType
    {
        public byte productTypeID { get; set; }
        public string Name { get; set; }
    }

    public class productPageRRP
    {
        public int ProductID { get; set; }
        public decimal? RRP { get; set; }

        public decimal? RollLength { get; set; }

        public decimal? RollWidth { get; set; }
    }

    public class productPageModuleOption
    {
        public int ProductPageID { get; set; }
        public string URLKeywords { get; set; }
        public int ModuleOptionID { get; set; }
        public string Name { get; set; }
        public long ModuleOptionImageID { get; set; }
        public long? ProductPageImageID { get; set; }
        public Int16 ModuleID { get; set; }
        public Int16? ModuleOptionGroupID { get; set; }
        public string Code { get; set; }
        public int? SampleProductID { get; set; }
        public string Size { get; set; }
    }

    public class productPageModule
    {
        public Int16 ModuleID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Int16 SortOrder { get; set; }
    }

    public class productPageModuleOptionGroup
    {
        public Int16 ModuleOptionGroupID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    #endregion


    #region Realtime Stock objects

    public class cmsRealTimeStock
    {
        public string OrderFormJSON { get; set; }

        public object orderForm 
        { 
            set
            {

            }
            get
            {
                return JsonConvert.DeserializeObject(string.IsNullOrEmpty(OrderFormJSON) ? "{}" : OrderFormJSON);
            }
        }
        //public List<productPagePrice> productPagePrices { get; set; }
        //public List<productPageProduct> productPagePrimarys { get; set; }
        //public List<productPageProduct> productPageSecondarys { get; set; }
        //public List<AlreadyInBasket> alreadyInBasket { get; set; }
        //public List<basketLineType> basketLineTypes { get; set; }
    }

    public class AlreadyInBasket
    {
        public int ProductID { get; set; }
        public decimal Quantity { get; set; }
        public bool SaveForLater { get; set; }
        public long SortOrder { get; set; }
        public string StockUnit { get; set; }
    }


    public class productPagePrice
    {
        public int ProductPageID { get; set; }
        public decimal? DisplayPriceMin { get; set; }
        public decimal? DisplayPriceMax { get; set; }
        public decimal? DisplaySalePriceMin { get; set; }
        public decimal? DisplaySalePriceMax { get; set; }
        public string DiscountMessage { get; set; }
    }

    public class productPageQuickLook
    {
        public int ProductID { get; set; }
        public int? ProductPageID { get; set; }
        public string Code { get; set; }
        public string OrderFormName { get; set; }
        public string Description { get; set; }
        public string Size { get; set; }
        public byte? SortOrder { get; set; }
        public decimal? DisplayPrice { get; set; }
        public decimal? DisplaySalePrice { get; set; }
        public decimal? Discount { get; set; }
        public byte? SalePriceTypeID { get; set; }
        public long? ImageID { get; set; }
        public byte productTypeID { get; set; }
        public string OrderFormGroup { get; set; }
        public bool PrimaryProduct { get; set; }
        public bool WhiteOnBlack { get; set; }
        public string DiscountMessage { get; set; }
    }

    public class productPageQuickLookText
    {
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public string Description { get; set; }
    }

    public class productPageQuickLookPrice
    {
        public decimal? DisplayPriceMin { get; set; }
        public decimal? DisplayPriceMax { get; set; }
        public decimal? DisplaySalePriceMin { get; set; }
        public decimal? DisplaySalePriceMax { get; set; }
    }

    public class productPageQuickLookBasketContent
    {
        public int ProductID { get; set; }
        public decimal Quantity { get; set; }
        public bool SaveForLater { get; set; }
        public long SortOrder { get; set; }
    }

    public class productPageBundleNotice
    {
        public List<bundledProduct> Products { get; set; }
    }

    public class bundledProduct
    {
        public int ProductID { get; set; }
        public long? ImageID { get; set; }
        public decimal Quantity { get; set; }
        public string Code { get; set; }
        public string OrderFormName { get; set; }
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
    }

    public class productPageProduct
    {
        public int ProductID { get; set; }
        public string Code { get; set; }
        public string OrderFormName { get; set; }
        public string OrderFormGroup { get; set; }
        public string Size { get; set; }
        public byte? SortOrder { get; set; }
        public decimal? DisplayPrice { get; set; }
        public decimal? DisplaySalePrice { get; set; }
        public string DiscountMessage { get; set; }
        public decimal? Discount { get; set; }
        public byte? SalePriceTypeID { get; set; }
        public long? ImageID { get; set; }
        public byte productTypeID { get; set; }
        public bool PrimaryProduct { get; set; }
        public bool Bundle { get; set; }
        public string Brand { get; set; }
        public DateTime? SalePriceAvailableUntil { get; set; }
        public string ItemAvailability { get; set; }
        public int? RatingCount { get; set; }
        public decimal? AverageRatingValue { get; set; }
        public byte? ExampleReviewRating { get; set; }
        public string ExampleReviewBody { get; set; }
        public DateTime? ExampleReviewDate { get; set; }
        public string ExampleReviewURL { get; set; }
        public bool AllowReserve { get; set; }
        public bool AllowBatchCutting { get; set; }
        public string StockMessage { get; set; }
        public object stockMessage
        {
            set
            {

            }
            get
            {
                if (!string.IsNullOrEmpty(StockMessage))
                    return JsonConvert.DeserializeObject(StockMessage);
                else
                    return null;
            }
        }
    }

    #endregion


    #region Collection Product Page Database and Cache objects


    public class cmsCollectionProductPageDefinition
    {
        public List<collectionAll> collectionAlls { get; set; }
        public List<collectionProduct> collectionProducts { get; set; }
        public List<collectionHeader> collectionHeaders { get; set; }
        public List<collectionProductType> collectionProductTypes { get; set; }
        public List<collectionBrand> collectionBrands { get; set; }
        public List<pageSectionBanner> pageSectionBanners { get; set; }

    }


    public class collectionAll
    {
        public short CollectionID { get; set; }
        public string Heading { get; set; }
        public string URLKeywords { get; set; }
    }


    public class collectionHeader
    {
        public short CollectionID { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public string URLKeywords { get; set; }
        public string TitleTag { get; set; }
        public string MetaDescription { get; set; }
    }


    public class collectionProduct
    {
        public int ProductPageID { get; set; }
        public string URLKeywords { get; set; }
        public string Heading { get; set; }
        public string Code { get; set; }
        public long ImageID { get; set; }
        public byte? SortOrder { get; set; }
    }


    public class collectionProductType
    {
        public byte ProductTypeID { get; set; }
        public string Name { get; set; }
        public byte? SortOrder { get; set; }
        public int useProductTypeID { get; set; }
        public int useSortOrder { get; set; }
    }


    public class collectionBrand
    {
        public byte BrandID { get; set; }
        public string Name { get; set; }
        public byte? SortOrder { get; set; }
        public int useBrandID { get; set; }
        public int useSortOrder { get; set; }
    }


    public class collectionSort
    {
        public int SortSequence { get; set; }
        public string SortOrderKey { get; set; }
        public string SortOrderDescription { get; set; }
    }


    #endregion


    #region Collection Database and Cache objects


    public class cmsCollectionPageDefinition
    {
        public collectionSelect CollectionDetails { get; set; }
        public List<collectionImageSelect> collectionImageSelects { get; set; }
        public List<collectionRelatedCollectionSelect> collectionRelatedCollectionSelects { get; set; }
        public List<collectionBrochureSelect> collectionBrochureSelects { get; set; }
        public List<collectionVideo> collectionVideos { get; set; }
        public List<GoogleHref> collectionGoogleHrefs { get; set; }
        public List<collectionBreadcrumb> collectionPageBreadcrumbs { get; set; }
        public PageSectionJSON productPageJson { get; set; }

        public collectionUnavailabilityRedirect objUnavailabilityRedirect { get; set; }

        private List<ProductPageFilterablePod> collectionProductPages 
        {
            get 
            {
                return JObject.FromObject(productPageJson.json)
                                         .SelectToken("productPages")
                                         .Children()
                                         .Select(p => JsonConvert.DeserializeObject<ProductPageFilterablePod>(p.SelectToken("productPage").ToString(), new JsonSerializerSettings() { ContractResolver = new JsonIgnoreAttributeIgnorerContractResolver() }))
                                         .ToList();
            } 
        }

        public List<ProductPagePodViewModel> GetProductPages(NewMasterDefinition model)
        {
            return collectionProductPages
                .OrderBy(cpp => cpp.MerchandisedSortOrder)
                .Select(p => new ProductPagePodViewModel(model, p, false, dgFramework.IsMobileDevice() ? ImageSize.Six : ImageSize.Two)).ToList();
        }
    }


    public class collectionSelect
    {
        public short CollectionID { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public string Description { get; set; }
        public string TitleTag { get; set; }
        public string MetaDescription { get; set; }
        public string URLKeywords { get; set; }
        public bool PreviewColourways { get; set; }
        public bool NoIndex { get; set; }
        public bool NoFollow { get; set; }
    }


    public class collectionImageSelect
    {
        public long ImageID { get; set; }
        public byte TemplateImageID { get; set; }
        public byte? SortOrder { get; set; }
        public decimal AspectRatio { get; set; }
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
        private string titleText;
        public string TitleText
        {
            get
            {
                if (titleText == null)
                {
                    return "";
                }
                return titleText;
            }
            set
            {
                titleText = value;
            }
        }

        public int? MaxWidth { get; set; }
    }


    public class collectionImageHotspotSelect
    {
        public long ImageID { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string HotspotHeading { get; set; }
        public long HotspotImageID { get; set; }
        public int ProductPageID { get; set; }
        public string URLKeywords { get; set; }
        public string Code { get; set; }
        public int? ProductID { get; set; }
        public bool BasketOrSample { get; set; }
    }


    public class collectionRelatedCollectionSelect
    {
        public short CollectionID { get; set; }
        public string URLKeywords { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public byte? SortOrder { get; set; }
        public long ImageID { get; set; }
        public bool New { get; set; }
    }


    public class collectionBrochureSelect
    {
        public Int16 BrochureID { get; set; }
        public string InteractiveBrochureURL { get; set; }
        public string CoverImagePath { get; set; }
        public string Heading { get; set; }
        public string Description { get; set; }
    }


    public class collectionVideo
    {
        public Int16 VideoID { get; set; }
        public string Name { get; set; }
        public string EmbedCode { get; set; }
    }



    #endregion


    #region Landing Page Database and Cache objects


    public class pageSectionProductPageStub
    {
        public long PageSectionID { get; set; }
        public int ProductPageID { get; set; }
        public byte ProductTypeID { get; set; }
        public int MerchandisedSortOrder { get; set; }
        public int SearchSortOrder { get; set; }
        public long AZSortOrder { get; set; }
        public byte ColourSort { get; set; }
        public long RatingSortOrder { get; set; }
        public long PriceSortOrder { get; set; }
    }

    public class cmsUnsubscribePageDefinition
    {
        public List<unsubscribeReason> unsubscribeReasons { get; set; }
    }

    public class ImageIDWrapper
    {
        public long ImageID { get; set; }
    }

    public class pageSectionProductPage
    {
        public long PageSectionID { get; set; }
        public short SortOrder { get; set; }
        public int ProductPageID { get; set; }
        public byte ProductTypeID { get; set; }
        public bool New { get; set; }
        public string URLKeywords { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public int? RatingAvg { get; set; }
        public int? RatingCount { get; set; }
        public bool ShowRating { get; set; }
        public bool IsLifestyleGallery { get; set; }
        public int? ProductID { get; set; }
        public byte? StockStatusID { get; set; }
        public bool Sample { get; set; }
    }

    public class pageSectionAttribute
    {
        public int ProductPageID { get; set; }
        public Int16 AttributeID { get; set; }
    }

    public class pageSectionPrice
    {
        public int ProductPageID { get; set; }
        public decimal? PriceIncMin { get; set; }
        public decimal? PriceIncMax { get; set; }
        public decimal? SalePriceIncMin { get; set; }
        public decimal? SalePriceIncMax { get; set; }
        public string DiscountMessage { get; set; }
    }


    public class pageSectionBanner
    {
        public int BannerID { get; set; }
        public string OverlayText { get; set; }
        public string FontClass { get; set; }
        public string AltText { get; set; }
        public string TitleText { get; set; }
        public string LinkURL { get; set; }
        public bool ExternalURL { get; set; }
        public bool NewWindow { get; set; }
        public byte? BannerLocationID { get; set; }
        public byte Version { get; set; }
        public long SortOrder { get; set; }
        public string ForegroundColour { get; set; }
        public string BackgroundColour { get; set; }
    }

    public class pageSectionArticleType
    {
        public byte ArticleTypeID { get; set; }
        public string Name { get; set; }
        public byte SortOrder { get; set; }
        public int PageID { get; set; }
        public string URLKeywords { get; set; }
        public int ArticleCount { get; set; }
    }
    #endregion

    #region Resource Database and Cache objects

    public class cmsResourceDefinition
    {
        public List<resourceSelect> resourceSelects { get; set; }
    }

    public class resourceSelect
    {
        public string Name { get; set; }

        [StringLength(1000)]
        public string Value { get; set; }
    }

    #endregion

    #region Braintree Error Response and Cache objects

    public class cmsBraintreeErrorResponseDefinition
    {
        public List<braintreeErrorResponseSelect> errorResponseSelects { get; set; }
    }

    public class braintreeErrorResponseSelect
    {
        public string Code { get; set; }

        public string Text { get; set; }
        
        public bool PermitRetry { get; set; }

        public bool AdviseCustomerToContactTheirBank { get; set; }

        public bool AdviseCustomerToAmendCVV { get; set; }
    }
    #endregion

    #region Article Page Database and Cache objects

    public class cmsArticleTypeDefinition
    {
        public List<articleTypeSelect> articleTypeSelects { get; set; }
    }

    public class cmsArticleSubTypeDefinition
    {
        public List<articleSubTypeSelect> articleSubTypeSelects { get; set; }
    }

    public class cmsArticlePageDefinition
    {
        public articleSelect ArticleDetails { get; set; }
        public List<Breadcrumb> Breadcrumbs { get; set; }
        public List<articleImageSelect> articleImageSelects { get; set; }
        public List<articleImageHotspotSelect> articleImageHotspotSelects { get; set; }
        public List<articleRelatedArticleSelect> articleRelatedArticleSelects { get; set; }
        public List<articleVideoSelect> articleVideoSelects { get; set; }
        public List<GoogleHref> articleGoogleHrefs { get; set; }
        public List<articleNextArticleSelect> articleNextArticleSelects { get; set; }
        public List<articlePreviousArticleSelect> articlePreviousArticleSelects { get; set; }
        public List<articleTypeSelect> articleTypeSelects { get; set; }
        public List<articleSubTypeSelect> articleSubTypeSelects { get; set; }
    }

    public class articleSelect
    {
        public byte ArticleTemplateID { get; set; }
        public byte ArticleTypeID { get; set; }
        public byte? ArticleSubTypeID { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public string TitleTag { get; set; }
        public string MetaDescription { get; set; }
        public string URLKeywords { get; set; }
        public string Contents { get; set; }
        public bool NoIndex { get; set; }
        public bool NoFollow { get; set; }
    }

    public class articleTypeSelect
    {
        public byte ArticleTypeID { get; set; }
        public string Name { get; set; }
        public byte SortOrder { get; set; }
        public string URL { get; set; }
        public int? ContentID { get; set; }
        public byte ContentTypeFlag { get; set; }
        //public int ArticleCount { get; set; }
    }

    public class articleSubTypeSelect
    {
        public byte ArticleSubTypeID { get; set; }
        public string Name { get; set; }
        public byte SortOrder { get; set; }
        public int? PageID { get; set; }
        public string URL { get; set; }
        public string URLKeywords { get; set; }
        public int ArticleCount { get; set; }
    }

    public class articleImageSelect
    {
        public long ImageID { get; set; }
        public byte TemplateImageID { get; set; }
        public byte SortOrder { get; set; }
        public string AltText { get; set; }
        public string TitleText { get; set; }
    }

    public class articleImageHotspotSelect
    {
        public long ImageID { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string HotspotHeading { get; set; }
        public long HotspotImageID { get; set; }
        public int ProductPageID { get; set; }
        public string URLKeywords { get; set; }
        public string Code { get; set; }
        public int? ProductID { get; set; }
        public bool BasketOrSample { get; set; }
    }

    public class articleRelatedArticleSelect
    {
        public int ArticleID { get; set; }
        public string URLKeywords { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public byte SortOrder { get; set; }
        public long ImageID { get; set; }
    }

    public class articleVideoSelect
    {
        public Int16 VideoID { get; set; }
        public string Name { get; set; }
        public DateTime RunningTime { get; set; }
        public string EmbedCode { get; set; }
        public decimal AspectRatio { get; set; }
    }

    public class articleNextArticleSelect
    {
        public int ArticleID { get; set; }
        public string Heading { get; set; }
        public string URLKeywords { get; set; }
    }

    public class articlePreviousArticleSelect
    {
        public int ArticleID { get; set; }
        public string Heading { get; set; }
        public string URLKeywords { get; set; }
    }
    #endregion

    #region Helper Objects

    public class countryAwarenessCheckReturn
    {
        public bool Match { get; set; }

        public string Culture { get; set; }

        public countryItem objCountryItem { get; set; }
    }

    public class pageSectionProductPageUrl
    {
        public int ProductPageID { get; set; }
        public string ImageUrl { get; set; }
        public List<string> ImageUrls { get; set; }
        public string LinkUrl { get; set; }
        public string PriceLine { get; set; }
        public string NewInd { get; set; }
        public string DiscountMessage { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public decimal? LowestPrice { get; set; }
        public decimal? HighestPrice { get; set; }
        public int? ProductID { get; set; }
        public byte? StockStatusID { get; set; }
        public bool Sample { get; set; }
        public string Code { get; set; }
        public decimal? FreeStock { get; set; }
        public string StockStatus { get; set; }
        public string StockUnit { get; set; }
        public decimal? RatingAvg { get; set; }
        public short? RatingCount { get; set; }
        public decimal? DisplayPriceMin { get; set; }
        public decimal? DisplayPriceMax { get; set; }
        public decimal? DisplaySalePriceMin { get; set; }
        public decimal? DisplaySalePriceMax { get; set; }
    }

    #endregion

    #region Mobile Detection Objects

    public class MobileDetectionDefinition
    {
        public bool IsTablet { get; set; }
        public bool IsMobile { get; set; }
        public bool IsAppleDevice { get; set; }
    }

    #endregion



    public class unsubscribeReason
    {
        public byte UnsubscribeReasonID { get; set; }
        public string Reason { get; set; }
    }
    
    #region Product Stock Info
    public class PassProductCode
    {
        public string code { get; set; }
    }

    public class ProductStockInfoReturn
    {
        public string Code { get; set; }
        public string TotalStock { get; set; }
        public List<Batch> Batches { get; set; }
        public List<PurchaseOrder> PurchaseOrders { get; set; }
        public string TotalPurchaseOrderLength { get; set; }
    }

    public class BatchPiece
    {
        public string Quantity { get; set; }
        public string Length { get; set; }
    }
    
    public class Batch
    {
        public string BatchNumber { get; set; }
        public string ItemCount { get; set; }
        public string Length { get; set; }

        public List<BatchPiece> Pieces { get; set; }
    }

    public class PurchaseOrder
    {
        public string PurchaseOrderNumber { get; set; }
        public string DueDate { get; set; }
        public string Length { get; set; }
    }
    #endregion

}