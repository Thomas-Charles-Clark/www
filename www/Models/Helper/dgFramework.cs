using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using wwwDesignersGuild.Models.Visitor;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.LandingPage;
using System.Text.RegularExpressions;
using System.Web.SessionState;
using System.Web.Configuration;
using wwwDesignersGuild.Models.Cache;
using Newtonsoft.Json;
using System.Web.Helpers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using wwwDesignersGuild.Util;
using wwwDesignersGuild.Models.Checkout;
using System.Net.Http;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using wwwDesignersGuild.Controllers;
using Microsoft.Owin.Security.Provider;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.Models.Helper
{
    public enum dgFixedPageSection
    {
        [LoginRequired(true, true)]
        [Action("TradeLoginDetails", "Trade")]
        tradeLoginDetails = 11885,

        [LoginRequired(true, true)]
        [Action("TradePricingDisplay", "Trade")]
        tradePricingDetails = 11888,

        [LoginRequired(true, true)]
        [Action("TradeUserManagement", "Trade")]
        tradeUsers = 11891,

        [LoginRequired(true, true)]
        [Action("TradeAccountContactDetails", "Trade")]
        tradeContactDetails = 11894,

        [LoginRequired(true, true)]
        [Action("TradeDownloads", "Trade")]
        tradePriceListBrochures = 11897,

        [LoginRequired(true, true)]
        [Action("TradeContactUs", "Trade")]
        tradeContactUs = 11900,

        [LoginRequired(true, true)]
        [Action("TradeAccountInvoices", "Trade")]
        tradeBalanceInvoicesPayments = 11903,

        [LoginRequired(true, true)]
        [Action("TradeAccountProformaInvoices", "Trade")]
        tradeProformaInvoices = 11906,

        [LoginRequired(true, true)]
        [Action("TradeOrderImport", "Trade")]
        tradeImportOrders = 11909,

        [LoginRequired(true, true)]
        [Action("TradeAccountOrderStatus", "Trade")]
        tradeOrderStatus = 11929,

        [LoginRequired(true, true)]
        [Action("TradeAccountClearancePricingCheck", "Trade", false)]
        tradeClearancePricingCheck = 12087,

        //[LoginRequired(true, true)]
        //[Action("TradeClearanceJavascript", "Trade", false)]
        //tradeClearanceJavascript = 12527,

        [LoginRequired(true, true)]
        [Action("TradeQuickOrdering", "Trade")]
        tradeQuickOrdering = 12104,

        [LoginRequired(true, true)]
        [Action("TradeAccountReserveOrders", "Trade")]
        tradeReserveOrders = 12232,

        [LoginRequired(true)]
        [Action("TradeAccountAddressBook", "Trade")]
        tradeAddressBook = 12562,

        [Action("TradeRequestAccess", "Trade")]
        tradeRequestAccess = 13970,

        [Action("Resubscribe", "NewSection")]
        resubscribe = 14699
    }

    public enum slideArea
    { topLeft = 1,
        topCenter = 2,
        topRight = 3,
        middleLeft = 4,
        middleCenter = 5,
        middleRight = 6,
        bottomLeft = 7,
        bottomCenter = 8,
        bottomRight = 9
    }

    public enum slideType
    {
        image = 1,
        video = 2
    }

    public enum callToActionWidthMode
    {
        fitToText = 1,
        fitToNeighbours = 2
    }

    public enum snippetDefinitions
    {
        modularFurnitureDelivery = 90,
        modularFurnitureReturns = 95,
        upsIdentificationProof = 240,
        furnitureFabric = 100,
        furnitureLeather = 105,
        furnitureLegFinishes = 110,
        furnitureLegStyles = 111,
        furnitureStich = 115,
        memberLegalTerms = 120,
        memberLegalPrivacy = 125,
        memberLegalCookie = 130,
        addressInformation = 135,
        shopDelivery = 140,
        shopReturn = 145,
        moodboardHelp = 154,
        checkoutGiftVoucherTerms = 200,
        upsAccessPointAbout = 240
    }

    public enum dgLoginStatus
    {
        notLoggedIn,
        previousLogin,
        currentLogin
    }

    public enum dgPageSections
    {
        html = 1,
        slideshow = 2,
        form = 3,
        productPageGrid = 4,
        collectionGrid = 5,
        pageGrid = 6,
        bannerGrid = 8,
        articleGrid = 9,
        video = 10,
        masonry = 11,
        animation = 12,
        bespokeContent = 13
    }

    public enum dgArticleTemplates
    {
        singleImage = 1,
        slideShow = 2,
        video = 3,
        slideShowLeft = 4
    }

    public enum dgCheckoutActionId
    {
        tradeAccountExistsCheck = 29,
        tradeAccountSettingsCheck = 30,
        logInvalidAddress = 49,
        logBraintreeLate = 123,
        logBraintreeTimeout = 124,
        logBraintreeLoaded = 128,
        logBraintreeReady = 137,
        logBraintreeScriptNotLoaded = 131,
        logBraintreeCardType = 132,
        logBraintreePayPalSelected = 133,
        logBraintreePayPalCancelled = 134,
        logBraintreeBuyNowClick = 136,
        logBraintreeBuyClientSideStep = 138,
        logValidationError = 139,
        logAppleBegin = 170,
        logAppleOnValidateMerchant = 171,
        logAppleOnValidateMerchantError = 172,
        logAppleOnValidateMerchantSuccess = 173,
        logAppleOnShippingContactSelected = 174,
        logAppleCompleteShippingContactSelection = 175,
        logAppleOnShippingMethodSelected = 176,
        logAppleCompleteShippingMethodSelection = 177,
        logAppleOnPaymentAuthorized = 178,
        logApplePaymentTokenizationError = 179,
        logApplePaymentTokenizationSuccess = 180,
        logAppleCompletePayment = 181,
        logAppleOnCancel = 182,
        logAppleAddressError = 183,
        logSessionTimeout = 254,
        logCheckoutExit = 255,
    }

    public enum dgFixedPage
    {
        [LoginRequired(false)]
        globalSiteSelector = -1,

        [LoginRequired(false)]
        home = 0,

        [LoginRequired(false)]
        [Action("ChangeCountry")]
        [PageSectionViewModel("ChangeCountry")]
        countryPicker = 50,

        [LoginRequired(false)]
        newsletter = 100,

        [LoginRequired(false)]
        memberRegistration = 101,

        [LoginRequired(false)]
        [Action("Login")]
        [PageSectionViewModel("Login")]
        memberLogin = 102,

        [LoginRequired(true, true)]
        tradeInformation = 103,

        [LoginRequired(false, false)]
        [Action("TradeRegistration", "Trade")]
        [PageSectionViewModel("TradeRegistration")]
        tradeRegistration = 104,

        [LoginRequired(true, true)]
        tradeLoginDetails = 106,

        [LoginRequired(true, true)]
        tradePriceDisplayPreferences = 107,

        [LoginRequired(true, true)]
        tradeUserManagement = 108,

        [LoginRequired(true, true)]
        tradeAccountContactDetails = 109,

        [LoginRequired(true, true)]
        tradeDownloads = 110,

        [LoginRequired(true, true)]
        tradeContactUs = 111,

        [LoginRequired(true, true)]
        tradeAccountInvoices = 112,

        [LoginRequired(true, true)]
        tradeProformaInvoices = 113,

        [LoginRequired(true, true)]
        tradeImportOrders = 114,

        [LoginRequired(true, true)]
        tradeOrderStatus = 115,

        [LoginRequired(true, true)]
        tradeQuickOrdering = 116,

        [LoginRequired(true, true)]
        tradeReserveOrders = 117,

        [LoginRequired(true, true)]
        tradeAddressBook = 118,

        [LoginRequired(true)] 
        memberMenu = 150,

        [LoginRequired(true)]
        [Action("MemberAddresses")]
        [PageSectionViewModel("MemberAddressManagement")]
        memberAddress = 151,

        [LoginRequired(true)]
        [Action("MemberLoginDetails")]
        [PageSectionViewModel("MemberLoginDetails")]
        memberEdit = 152,

        [LoginRequired(true)]
        [Action("MemberOrderHistory")]
        [PageSectionViewModel("MemberOrderHistory")]
        memberOrderHistory = 153,

        [LoginRequired(true)]
        [Action("MemberMoodboards")]
        [PageSectionViewModel("MemberMoodboard")]
        moodboard = 154,

        [LoginRequired(false)]
        [Action("Unsubscribe")]
        [PageSectionViewModel("MemberUnsubscribe")]
        unsubscribe = 160,

        [LoginRequired(false)]
        [PageSectionViewModel("MemberResubscribe")]
        resubscribe = 161,

        [LoginRequired(false)]
        [Action("ReturnsPaperwork", "NewSection")]
        [PageSectionViewModel("ReturnsPaperwork")]
        returnsPaperwork = 164,

        [LoginRequired(false)]
        [Action("MemberResetPassword", "NewSection")]
        [PageSectionViewModel("MemberPasswordReset")]
        memberPasswordReset = 181,

        [LoginRequired(false)]
        [Action("Search")]
        [PageSectionViewModel("SearchResults")]
        serverSearch = 76,

        [LoginRequired(false)]
        [Action("Basket")]
        [PageSectionViewModel("Basket")]
        maintainBasket = 201,

        [LoginRequired(false)]
        [Action("CheckoutLogin")]
        [PageSectionViewModel("CheckoutLogin")]
        checkoutLogin = 211,

        [LoginRequired(false)]
        [Action("Checkout", "NewSection")]
        [PageSectionViewModel("CheckoutSinglePage")]
        checkoutSinglePage = 213,

        [LoginRequired(false)]
        checkoutAddress = 221,

        [LoginRequired(false)]
        checkoutDispatch = 231,

        [LoginRequired(false)]
        checkoutGiftVoucher = 236,

        [LoginRequired(false)]
        checkoutGift = 241,

        [LoginRequired(false)]
        checkoutTotals = 251,

        [LoginRequired(false)]
        checkoutPay = 261,

        [LoginRequired(false)]
        [Action("_checkoutConfirm", "Section")]
        [PageSectionViewModel("CheckoutConfirm")]
        checkoutConfirmation = 271,

        [LoginRequired(false)]
        [Action("_checkoutConfirmSpartan", "Section")]
        [PageSectionViewModel("CheckoutConfirm")]
        checkoutConfirmationSpartan = 281,

        [LoginRequired(false)]
        [Action("CheckoutExpired")]
        [PageSectionViewModel("CheckoutExpired")]
        checkoutSessionExpired = 300,

        [LoginRequired(false)]
        [Action("CheckoutProblem")]
        [PageSectionViewModel("CheckoutProblem")]
        checkoutProblem = 301,

        [LoginRequired(false)]
        [Action("DotDigitalDataCleansePositive")]
        [PageSectionViewModel("DotDigitalDataCleanse")]
        dotDigitalDataCleansePositive = 311,

        [LoginRequired(false)]
        [Action("DotDigitalDataCleanseNegative")]
        [PageSectionViewModel("DotDigitalDataCleanse")]
        dotDigitalDataCleanseNegative = 312,

        [LoginRequired(false)]
        [Action("Stockists")]
        [PageSectionViewModel("Stockists")]
        stockists = 500,

        [LoginRequired(false)]
        [Action("TradeAccountRequest", "Trade")]
        [PageSectionViewModel("TradeAccountRequest")]
        tradeAccountRequest = 540,

        [LoginRequired(false)]
        tradeRequestAccess = 550,

        [LoginRequired(false)]
        dgWorld = 600,

        [LoginRequired(false)]
        interiorDesignService = 800,

        [LoginRequired(false)]
        newFabricCollections = 1003,

        [LoginRequired(false)]
        [Action("Careers")]
        [PageSectionViewModel("Careers")]
        careers = 1005,

        [LoginRequired(false)]
        cookie = 1011,

        [LoginRequired(false)]
        contractStockistSearch = 1038,

        [LoginRequired(false)]
        termsAndConditions = 1013,

        [LoginRequired(false)]
        privacyPolicy = 1010,

        [LoginRequired(true, true)]
        tradeClearance = 1605,

        [LoginRequired(true, true)]
        tradeHomePage = 1743
    }

    public enum dgLandingPagesNewTemplate
    {
        cushions = 1046,
        newFabricCollections = 1003,
        outdoorCollectionShop = 1679,
        dgWorld = 600
    }

    public enum dgContentType
    {
        landingPage,
        productPage,
        collectionPage,
        articlePage,
        invalid,
        checkoutPart
    }

    public enum dgImageSize
    {
        colourSwatch,
        marker,
        productPage,
        collectionSlideShow,
        largeProduct,
        furnitureProduct,
        zoomProduct,
        largeSlideshow,
        bannerMedium,
        bannerLarge,
        bannerLargeMobile,
        articleSlideShow
    }

    public enum webapiDataError
    {
        EmailDuplicate,
        EmailInvalidFormat,
        EmailConfirmInvalidFormat,
        EmailConfirmFailed,
        InvalidLogin,
        validateBasketFailed,
        checkoutBasketExpired,
        invalidZipCode,
        zipTaxServiceFailed,
        MemberUpdateFailed,
        PasswordResetRequestEmailNotFound,
        PasswordResetFailed,
        InvalidCaptcha,
        duplicateGiftVoucherEntered,
        invalidGiftVoucherEntered,
        accountNotFoundInMertex,
        mertexCommunicationError,
        accountNotConfiguredCorrectly,
        invalidDeliveryAddress,
        TradePermissionsExist,
        checkoutTradeInvalidPermission
    }


    public enum webapiUnhandledError
    {
        InvalidParameters,
        SecurityFail,
        MissingSessionParameter,
        NameOutsideRange,
        TradeAccountNumberOutsideRange,
        CompanyNameOutsideRange,
        EmailOutsideRange,
        PasswordOutsideRange,
        InvalidQuantity,
        InvalidPrice,
        InvalidQuantityRange,
        InvalidCountry,
        InvalidCountryBasket,
        InvalidStockistsContractCountry,
        MemberNotInserted,
        AddressLineOneOutsideRange,
        AddressLineTwoOutsideRange,
        AddressTownOutsideRange,
        AddressCountyOutsideRange,
        AddressPostCodeOutsideRange,
        AddressCountryOutsideRange,
        AddressPhoneOutsideRange,
        AddressNotInserted,
        checkoutAddressNotSelected,
        checkoutNoBasket,
        interiorDesignPreferredShopOutsideRange,
        interiorDesignNotesOutsideRange,
        checkoutTradeInvalidPermission
    }

    public enum dgCheckoutType
    {
        Member = 0,
        Guest = 1,
        Trade = 2,
        Amazon = 3,
        Paypal = 4,
        Apple = 5
    }

    public class CheckoutTypeResponse 
    { 
        public IEnumerable<CheckoutType> CheckoutTypes { get; set; }
        public CheckoutTypeNotice CheckoutTypeNotice { get; set; }
    }

    public class CheckoutTypeNotice
    {
        public string AlertMessage { get; set; }
    }

    //mucho appreciado: https://stackoverflow.com/questions/27764692/validating-recaptcha-2-no-captcha-recaptcha-in-asp-nets-server-side#answer-27767239
    public class ReCaptchaVerifier
    {
        public static string Validate(string EncodedResponse)
        {
            var client = new System.Net.WebClient();

            string PrivateKey = "6LcweykUAAAAACQPHUhkR2Vzr53VIgWeCzkDYFP4";

            var GoogleReply = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", PrivateKey, EncodedResponse));

            var captchaResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ReCaptchaVerifier>(GoogleReply);

            return captchaResponse.Success;
        }

        public static string Validate2022(string EncodedResponse)
        {
            var client = new System.Net.WebClient();

            var PrivateKey = System.Configuration.ConfigurationManager.AppSettings["recaptchaV2Secret"];

            var GoogleReply = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", PrivateKey, EncodedResponse));

            var captchaResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ReCaptchaVerifier>(GoogleReply);

            return captchaResponse.Success;
        }

        [JsonProperty("success")]
        public string Success
        {
            get { return m_Success; }
            set { m_Success = value; }
        }

        private string m_Success;
        [JsonProperty("error-codes")]
        public List<string> ErrorCodes
        {
            get { return m_ErrorCodes; }
            set { m_ErrorCodes = value; }
        }

        private List<string> m_ErrorCodes;
    }

    public static class dgFramework
    {
        public static String FiftyOneDegreesLicenseKey { get { return WebConfigurationManager.AppSettings["fiftyOneDegreesLicenseKey"]; } }

        public static Int16 CacheInMinutes { get { return Convert.ToInt16(WebConfigurationManager.AppSettings["keepCacheInMinutes"]); } }

        #region WebApi Error Routines

        //  This routine raises error that are handled in the JavaScript (e.g. validation failures)
        public static webapiHandleError raiseErrorToHandle(webapiDataError errorMessage)
        {
            var objError = new webapiHandleError
            {
                message = errorMessage.ToString()
            };
            return objError;
        }

        public static webapiHandleError raiseErrorToHandle(webapiDataError errorMessage, string additionalInfo = null, object model = null)
        {
            var objError = new webapiHandleError
            {
                message = errorMessage.ToString(),
                additionalInfo = additionalInfo,
                model = model
            };
            return objError;
        }

        // This is sort of meant to be used to catch violations of JS validation.
        // We know that you were instructed to provide a Name value of between 2 and 50 characters in length.
        // We know that JavaScript should have enforced that constraint.
        // If we raise webapiUnhandledError.NameOutsideRange, the client will just catch the non-OK response
        // as an error and display the errorModal
        // This routine raises error that are NOT handled in the JavaScript (e.g. bad data submitted)
        public static void raiseErrorNotHandled(webapiUnhandledError errorMessage, string additionalInfo = null)
        {
            HttpResponseMessage resp = null;

            if (additionalInfo != null)
            {
                resp = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.ExpectationFailed)
                {
                    Content = new StringContent($"{errorMessage}\t\t\t{additionalInfo}".Replace(System.Environment.NewLine, "")),
                    ReasonPhrase = $"{errorMessage}\t\t\t{additionalInfo}".Replace(System.Environment.NewLine, "")
                };
            }
            else
            {
                resp = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.ExpectationFailed)
                {
                    Content = new StringContent($"{errorMessage}".Replace(System.Environment.NewLine, "")),
                    ReasonPhrase = $"{errorMessage}".Replace(System.Environment.NewLine, "")
                };
            }
            throw new System.Web.Http.HttpResponseException(resp);
        }
        #endregion


        #region Enum Conversion Routines


        public static String getSnippetId(byte snippetId)
        {
            return String.Format("snippet-id-{0}", snippetId);
        }


        public static string getUrlKeywordsForFixedPage(int? contentId, countryItem objCountryItem)
        {
            String urlText = "";
            if (contentId == null)
            {
                return urlText;
            }

            switch (contentId)
            {
                case ((int)dgFixedPage.checkoutLogin):
                    urlText = HtmlHelperExtra.Resource("urlCheckoutWelcome", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.checkoutSinglePage):
                    urlText = HtmlHelperExtra.Resource("urlCheckoutSinglePage", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.countryPicker):
                    urlText = HtmlHelperExtra.Resource("urlCountryPickerKeyword", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.home):
                    // We removed all keywords from sites' home pages.
                    // urlText = HtmlHelperExtra.Resource("urlHomeKeyword", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.maintainBasket):
                    urlText = HtmlHelperExtra.Resource("urlBasketMaintenance", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.memberLogin):
                    urlText = HtmlHelperExtra.Resource("urlLoginKeyword", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.tradeInformation):
                    urlText = HtmlHelperExtra.Resource("urlTradeInformation", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.tradeRegistration):
                    urlText = HtmlHelperExtra.Resource("urlTradeRegistration", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.memberRegistration):
                    urlText = HtmlHelperExtra.Resource("urlRegistrationKeyword", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.memberMenu):
                    urlText = HtmlHelperExtra.Resource("urlMemberMenu", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.memberAddress):
                    urlText = HtmlHelperExtra.Resource("urlMemberAddress", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.memberEdit):
                    urlText = HtmlHelperExtra.Resource("urlMemberEdit", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.memberOrderHistory):
                    urlText = HtmlHelperExtra.Resource("urlMemberOrderHistory", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.moodboard):
                    urlText = HtmlHelperExtra.Resource("urlMoodboard", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.checkoutSessionExpired):
                    urlText = HtmlHelperExtra.Resource("urlCheckoutExpired", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.serverSearch):
                    urlText = HtmlHelperExtra.Resource("urlSearchKeyword", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.checkoutConfirmation):
                    urlText = HtmlHelperExtra.Resource("urlCheckoutConfirm", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.cookie):
                    urlText = HtmlHelperExtra.Resource("urlCookiePolicy", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.stockists):
                    urlText = HtmlHelperExtra.Resource("urlStockistKeyword", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.termsAndConditions):
                    urlText = HtmlHelperExtra.Resource("urlTermsKeyword", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;
                case ((int)dgFixedPage.privacyPolicy):
                    urlText = HtmlHelperExtra.Resource("urlPrivacyKeyword", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                    break;

            }

            return urlText;
        }

        public static string getAriaLabelForFixedPage(int? contentId, countryItem objCountryItem)
        {
            var ti = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;

            return ti.ToTitleCase(Regex.Replace(getUrlKeywordsForFixedPage(contentId, objCountryItem), "-", " "));
        }

        public static dgContentType getContentTypeFromString(String contentType)
        {
            switch (contentType.ToLower())
            {
                case "l":
                    {
                        return dgContentType.landingPage;
                    }
                case "p":
                    {
                        return dgContentType.productPage;
                    }
                case "c":
                    {
                        return dgContentType.collectionPage;
                    }
                case "a":
                    {
                        return dgContentType.articlePage;
                    }
                case "h":
                    {
                        return dgContentType.checkoutPart;
                    }
                default:
                    {
                        return dgContentType.invalid;
                    }
            }
        }


        public static String getStringFromContentType(dgContentType contentType)
        {
            switch (contentType)
            {

                case dgContentType.landingPage:
                    {
                        return "l";
                    }
                case dgContentType.productPage:
                    {
                        return "p";
                    }
                case dgContentType.collectionPage:
                    {
                        return "c";
                    }
                case dgContentType.articlePage:
                    {
                        return "a";
                    }
                case dgContentType.checkoutPart:
                    {
                        return "h";
                    }

            }
            return "l";
        }


        public static String buildLifestyleGalleryPopupUrl(String basketCountry, int contentId, String dateOverride, String countryOverride)
        {
            string url = string.Format("/lifestyle/{0}/{1}", basketCountry, contentId.ToString());
            if (dateOverride == null) { }
            else
            {
                url = String.Format("{0}?dateOverride={1}", url, dateOverride);

            }

            if (countryOverride == null) { }
            else
            {
                if (dateOverride == null)
                {
                    url = String.Format("{0}?countryOverride={1}", url, countryOverride);
                }
                else
                {
                    url = String.Format("{0}&countryOverride={1}", url, countryOverride);
                }
            }

            return url;
        }

        public static string buildUrl(
            dgContentType contentType,
            int contentId,
            countryItem objCountryItem,
            int? redirectContentId = null,
            dgContentType? redirectContentType = null,
            IEnumerable<KeyValuePair<string, string>> additionalParameters = null,
            string urlKeywords = null,
            //IEnumerable<string> keysForRemoval = null,
            IEnumerable<string> keysToRetain = null
            )
        {
            var nvc = new System.Collections.Specialized.NameValueCollection(HttpContext.Current.Request.QueryString);

            if (keysToRetain == null)
            {
                nvc.Clear();
            }
            else
            {
                if (keysToRetain.Any())
                {
                    for (var i = nvc.AllKeys.Length - 1; i >= 0; i--)
                    {
                        if (!keysToRetain.Contains(nvc.AllKeys[i]))
                            nvc.Remove(nvc.AllKeys[i]);
                    }
                }
                else
                {
                    for (var i = 0; i < nvc.AllKeys.Length; i++)
                    {
                        nvc.Remove(nvc.AllKeys[i]);
                    }
                }
            }

            if (additionalParameters != null) {
                additionalParameters.ToList().ForEach(kvp =>
                {
                    nvc.Add(kvp.Key, kvp.Value);
                });
            }

            if (redirectContentId != null &&
                redirectContentType != null)
            {
                nvc.Add("redirect", $"{redirectContentId}");
                nvc.Add("contentType", getStringFromContentType(redirectContentType.Value));
            }

            var qs = Framework.ConstructQueryString(nvc);

            if (!String.IsNullOrEmpty(qs))
                qs = $"?{qs}";

            if (contentType == dgContentType.landingPage)
            {
                if (String.IsNullOrEmpty(urlKeywords))
                {
                    if (String.IsNullOrEmpty(getUrlKeywordsForFixedPage((int?)contentId, objCountryItem)))
                    {
                        return Framework.URLFixup($"/{objCountryItem.URL}/{getStringFromContentType(contentType)}{(int?)contentId}{qs}");
                    }
                    else
                    {
                        return Framework.URLFixup($"/{objCountryItem.URL}/{getUrlKeywordsForFixedPage((int?)contentId, objCountryItem)}/{getStringFromContentType(contentType)}{(int?)contentId}{qs}");
                    }
                }
                else
                {
                    return Framework.URLFixup($"/{objCountryItem.URL}/{urlKeywords}/{getStringFromContentType(contentType)}{(int?)contentId}{qs}");
                }
            }
            else
            {
                if (String.IsNullOrEmpty(urlKeywords))
                    return Framework.URLFixup($"/{objCountryItem.URL}/{getStringFromContentType(contentType)}{(int?)contentId}{qs}");
                else
                    return Framework.URLFixup($"/{objCountryItem.URL}/{urlKeywords}/{getStringFromContentType(contentType)}{(int?)contentId}{qs}");
            } 
        }

        public static String buildUrl(countryItem objCountryItem, String semanticString, dgContentType contentType, int? contentId)
        {
            return Framework.URLFixup($"/{objCountryItem.URL}/{semanticString}/{dgFramework.getStringFromContentType(contentType)}{(int?)contentId}");
        }

        public static String buildUrl(countryItem objCountryItem, String semanticString, dgContentType contentType, int? contentId, String dateOverride, String countryOverride, String extraParameters = null)
        {
            //  Build url
            if (semanticString == null)
            {
                semanticString = getUrlKeywordsForFixedPage(contentId, objCountryItem);
            }
            if (semanticString.StartsWith("/"))
            {
                semanticString = semanticString.Substring(1);
            }
            string url;
            if (contentId == null)
            {
                if (String.IsNullOrEmpty(semanticString))
                {
                    url = String.Format("/{0}/{1}", objCountryItem.URL, getStringFromContentType(contentType));
                }
                else
                {
                    url = String.Format("/{0}/{1}/{2}", objCountryItem.URL, semanticString, getStringFromContentType(contentType));
                }
            }
            else
            {
                if (String.IsNullOrEmpty(semanticString))
                {
                    url = String.Format("/{0}/{1}{2}", objCountryItem.URL, getStringFromContentType(contentType), contentId.ToString());
                }
                else
                {
                    url = String.Format("/{0}/{1}/{2}{3}", objCountryItem.URL, semanticString, getStringFromContentType(contentType), contentId.ToString());
                }
            }

            if (extraParameters == null) { }
            else
            {
                if (url.Contains("?"))
                {
                    url = String.Format("{0}&{1}", url, extraParameters);
                }
                else
                {
                    url = String.Format("{0}?{1}", url, extraParameters);
                }
            }

            return Framework.URLFixup(url);
        }


        public static String buildUrl(countryItem objCountryItem, String fullUrl, String dateOverride, String countryOverride)
        {

            //  Build url
            if (fullUrl == null)
            {
                fullUrl = "";
            }
            if (fullUrl.ToUpper().StartsWith("HTTP://"))
            {
                return fullUrl;
            }
            if (fullUrl.StartsWith("/"))
            {
                fullUrl = fullUrl.Substring(1);
            }
            String url = String.Format("/{0}/{1}", objCountryItem.URL, fullUrl);

            if (!Framework.ProductionVersion)
            {
                if (dateOverride == null) { }
                else
                {
                    if (url.Contains("?"))
                    {
                        url = String.Format("{0}&dateOverride={1}", url, dateOverride);
                    }
                    else
                    {
                        url = String.Format("{0}?dateOverride={1}", url, dateOverride);
                    }
                }

                if (countryOverride == null) { }
                else
                {
                    if (url.Contains("?"))
                    {
                        url = String.Format("{0}&countryOverride={1}", url, countryOverride);
                    }
                    else
                    {
                        url = String.Format("{0}?countryOverride={1}", url, countryOverride);
                    }
                }
            }

            return Framework.URLFixup(url);
        }

        public static string buildLoginRedirectToCurrentUrl(
            countryItem objCountryItem,
            dgContentType? redirectContentType,
            int? redirectContentId)
        {
            var nvc = new System.Collections.Specialized.NameValueCollection(HttpContext.Current.Request.QueryString);

            if (redirectContentId != null &&
                redirectContentType != null)
            {
                nvc.Add("redirect", $"{redirectContentId}");
                nvc.Add("contentType", getStringFromContentType(redirectContentType.Value));
            }

            var qs = Framework.ConstructQueryString(nvc);

            if (!String.IsNullOrEmpty(qs))
                qs = $"?{qs}";

            return $"/{objCountryItem.URL}/{getUrlKeywordsForFixedPage((int?)dgFixedPage.memberLogin, objCountryItem)}/l102{qs}";
        }

        public static string buildRegisterRedirectToCurrentUrl(
            countryItem objCountryItem,
            dgContentType? redirectContentType,
            int? redirectContentId)
        {
            var nvc = new System.Collections.Specialized.NameValueCollection(HttpContext.Current.Request.QueryString);

            if (redirectContentId != null &&
                redirectContentType != null)
            {
                nvc.Add("redirect", $"{redirectContentId}");
                nvc.Add("contentType", getStringFromContentType(redirectContentType.Value));
            }

            nvc.Add("r", "1");

            var qs = Framework.ConstructQueryString(nvc);

            if (!String.IsNullOrEmpty(qs))
                qs = $"?{qs}";

            return $"/{objCountryItem.URL}/{getUrlKeywordsForFixedPage((int?)dgFixedPage.memberLogin, objCountryItem)}/l102{qs}";
        }

        public static String getImageWidth(dgImageSize imageSize)
        {

            if (imageSize == dgImageSize.colourSwatch)
            {
                return "32";
            }
            else if (imageSize == dgImageSize.marker)
            {
                return "156";
            }
            else if (imageSize == dgImageSize.bannerLargeMobile)
            {
                return "256";
            }
            else if (imageSize == dgImageSize.bannerMedium)
            {
                return "342";
            }
            else if (imageSize == dgImageSize.productPage)
            {
                //return "488";
                return "322";
            }
            else if (imageSize == dgImageSize.collectionSlideShow)
            {
                return "488";
            }
            else if (imageSize == dgImageSize.articleSlideShow)
            {
                return "488";
            }
            else if (imageSize == dgImageSize.bannerLarge)
            {
                return "512";
            }
            else if (imageSize == dgImageSize.largeProduct)
            {
                return "654";
            }
            else if (imageSize == dgImageSize.furnitureProduct)
            {
                return "986";
            }
            else if (imageSize == dgImageSize.zoomProduct)
            {
                return "1024";
            }
            else if (imageSize == dgImageSize.largeSlideshow)
            {
                return "1024";
            }
            else
            {
                return "-";
            }
        }


        #endregion


        #region Standard Conversion Routines

        public static string SimpleTruncate(string source, int length)
        {
            if (source == null)
                return null;

            if (source.Length > length)
            {
                source = source.Substring(0, length);
            }

            return source;
        }

        public static byte? convertStringToByte(String passText)
        {
            if (passText == null)
            {
                return null;
            }

            //  Note -1 is returned as null (required)

            if (Byte.TryParse(passText, out byte tempByte))
            {
                return tempByte;
            }
            else
            {
                return null;
            }
        }


        public static string convertPriceToDisplay(Decimal? passPrice)
        {
            String returnValue = "";
            if (passPrice == null)
            {
                return returnValue;
            }

            Boolean shouldConvertTo2DP = false;
            if ((passPrice % 1) > 0)
            {
                shouldConvertTo2DP = true;
            }

            returnValue = shouldConvertTo2DP ? convertPriceToDisplayWithDP(passPrice) : convertPriceToDisplayWithoutDP(passPrice);

            return returnValue;
        }


        public static string convertPriceToDisplayWithoutDP(Decimal? passPrice)
        {
            if (passPrice == null)
            {
                return "";
            }
            Decimal.TryParse(passPrice.ToString(), out decimal tempPrice);
            return ((Int64)Math.Truncate(tempPrice)).ToString();

        }


        public static string convertPriceToDisplayWithDP(Decimal? passPrice)
        {
            if (passPrice == null)
            {
                return "0.00";
            }
            Decimal.TryParse(passPrice.ToString(), out decimal tempPrice);
            return tempPrice.ToString("#,##0.00");
        }


        public static double convertCentimetersToInches(double centimeters)
        {
            return centimeters / 2.54;
        }


        public static double convertGsmToLbs(double gsm)
        {
            return gsm / 33.906;
        }


        #endregion


        #region Display Routines


        public static string displayGsmOrLbs(String passGsm, masterDefinition objMaster)
        {
            if (objMaster.UrlBreakdown.countryDetails.URL.ToUpper() == "US")
            {
                if (double.TryParse(passGsm, out double valueGsm) == true)
                {
                    double lbs = convertGsmToLbs(valueGsm);
                    if (lbs.ToString("0.000").EndsWith(".000"))
                    {
                        return String.Format("{0}{1}", lbs.ToString("0"), HtmlHelperExtra.Resource("gsm", objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate).ToString());
                    }
                    else
                    {
                        return String.Format("{0}{1}", lbs.ToString("0.00"), HtmlHelperExtra.Resource("gsm", objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate).ToString());
                        //ounces per square yard
                    }
                }
                else
                {
                    //  If the conversion fails, always display the gsm value
                    return String.Format("{0}gsm", passGsm);
                }
            }
            else
            {
                return String.Format("{0}{1}", passGsm, HtmlHelperExtra.Resource("gsm", objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate).ToString());
            }
        }


        public static string displayCentimetersOrInches(String passCentimeter, masterDefinition objMaster)
        {
            if (objMaster.UrlBreakdown.countryDetails.URL.ToUpper() == "US")
            {
                if (double.TryParse(passCentimeter, out double valueCentimeter) == true)
                {
                    double inches = convertCentimetersToInches(valueCentimeter);
                    if (inches.ToString("0.000").EndsWith(".000"))
                    {
                        return String.Format("{0}{1}", inches.ToString("0"), HtmlHelperExtra.Resource("cm", objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate).ToString());
                    }
                    else
                    {
                        //  Convert decimal to fraction for output
                        return generateInchDisplay(inches, objMaster);
                    }
                }
                else
                {
                    //  If the conversion fails, always display the cm value
                    return String.Format("{0}cm", passCentimeter);
                }
            }
            else
            {
                return String.Format("{0}{1}", passCentimeter, HtmlHelperExtra.Resource("cm", objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate).ToString());
            }

        }


        private static String generateInchDisplay(double inches, masterDefinition objMaster)
        {
            int displayInches = (int)inches;

            double storeRemainder = inches - displayInches;

            string fraction;
            if (storeRemainder >= 0.875)
            {
                displayInches++;
                fraction = "";
            }
            else if (storeRemainder >= 0.625)
            {
                fraction = displayFraction(3, 4);
            }
            else if (storeRemainder >= 0.375)
            {
                fraction = displayFraction(1, 2);
            }
            else if (storeRemainder >= 0.125)
            {
                fraction = displayFraction(1, 4);
            }
            else
            {
                fraction = "";
            }


            if (fraction == "")
            {
                return String.Format("{0}{1}", displayInches.ToString(), HtmlHelperExtra.Resource("cm", objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate).ToString());
            }
            else
            {
                return String.Format("{0}{1} {2}", displayInches.ToString(), fraction, HtmlHelperExtra.Resource("cm", objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate).ToString());
            }

        }

        private static string displayFraction(int minor, int major)
        {
            //return String.Format("<sup style=\"vertical-align:top;letter-spacing: 0px;font-size:90%;\">{0}</sup>/<sub style=\"vertical-align:top;letter-spacing: 0px;font-size:90%;\">{1}</sub>", minor, major);
            return String.Format("<span style=\"letter-spacing: 0px;font-size:80%;\">{0}/{1}</span>", minor, major);
        }

        public static string displayProductsAlreadyInBasket(short passQuantity, string stockUnit, countryItem objCountryItem, DateTime useDate)
        {
            if (passQuantity == 1)
            {
                return $"{passQuantity} {stockUnit} {HtmlHelperExtra.Resource("ShoppingProductInBasket", objCountryItem.LanguageID, objCountryItem.CountryID, useDate)}";
            }
            else
            {
                return $"{passQuantity} {stockUnit} {HtmlHelperExtra.Resource("ShoppingProductsInBasket", objCountryItem.LanguageID, objCountryItem.CountryID, useDate)}";
            }
        }


        public static string displayPriceRangeShopping(Decimal? passMinPrice, Decimal? passMaxPrice,
                                                        Decimal? passMinSalePrice, Decimal? passMaxSalePrice, masterDefinition objMaster)
        {
            Boolean onSale = false;
            Boolean salePriceSame = false;
            Boolean priceSame = false;
            Boolean shouldConvertTo2DP = false;



            if (passMinPrice == null)
            {
                passMinPrice = passMaxPrice;
            }

            if (passMaxPrice == null)
            {
                passMaxPrice = passMinPrice;
            }

            if (passMinPrice == null)
            {
                passMinPrice = 0;
                passMaxPrice = 0;
            }

            if (passMinPrice == passMaxPrice)
            {
                priceSame = true;
            }

            if ((passMinSalePrice == null) && (passMaxSalePrice == null)) { }
            else
            {
                onSale = true;
                if (passMinSalePrice == passMaxSalePrice)
                {
                    salePriceSame = true;
                }
            }

            if (onSale)
            {
                if ((passMinPrice % 1) > 0 || (passMaxPrice % 1) > 0 || (passMinSalePrice % 1) > 0 || (passMaxSalePrice % 1) > 0)
                {
                    shouldConvertTo2DP = true;
                }
            }
            else
            {
                if ((passMinPrice % 1) > 0 || (passMaxPrice % 1) > 0)
                {
                    shouldConvertTo2DP = true;
                }
            }

            String minPrice = shouldConvertTo2DP ? convertPriceToDisplayWithDP(passMinPrice) : convertPriceToDisplayWithoutDP(passMinPrice);
            String maxPrice = shouldConvertTo2DP ? convertPriceToDisplayWithDP(passMaxPrice) : convertPriceToDisplayWithoutDP(passMaxPrice);
            string linePrice;
            if (priceSame == true)
            {
                if (onSale == true)
                {
                    linePrice = String.Format("{0} <del class=\"no-strike\">{1}{2}</del>", HtmlHelperExtra.Resource("was", objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate).ToString(), HtmlHelperExtra.Resource("currencySymbol", objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate).ToString(), minPrice);
                }
                else
                {
                    linePrice = String.Format("{0}{1}", HtmlHelperExtra.Resource("currencySymbol", objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate).ToString(), minPrice);
                }
            }
            else
            {
                if (onSale == true)
                {
                    linePrice = String.Format("{0} <del class=\"no-strike\">{1}{2}</del> - <del class=\"no-strike\">{1}{3}</del><br />", HtmlHelperExtra.Resource("was", objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate).ToString(), HtmlHelperExtra.Resource("currencySymbol", objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate).ToString(), minPrice, maxPrice);
                }
                else
                {
                    linePrice = String.Format("{0}{1} - {0}{2}", HtmlHelperExtra.Resource("currencySymbol", objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate).ToString(), minPrice, maxPrice);
                }
            }


            String lineSalePrice = "";
            if (onSale == true)
            {
                if ((passMinSalePrice % 1) == 0 && (passMaxSalePrice % 1) == 0) { }
                else
                {
                    shouldConvertTo2DP = true;
                }

                string saleMinPrice = shouldConvertTo2DP ? convertPriceToDisplayWithDP(passMinSalePrice) : convertPriceToDisplayWithoutDP(passMinSalePrice);
                string saleMaxPrice = shouldConvertTo2DP ? convertPriceToDisplayWithDP(passMaxSalePrice) : convertPriceToDisplayWithoutDP(passMaxSalePrice);
                if (salePriceSame == true)
                {
                    lineSalePrice = String.Format("&nbsp;<span class=\"discount\">{0} {1}{2}</span>", HtmlHelperExtra.Resource("now", objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate).ToString(), HtmlHelperExtra.Resource("currencySymbol", objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate).ToString(), saleMinPrice);
                }
                else
                {
                    lineSalePrice = String.Format("&nbsp;<span class=\"discount\">{0} {1}{2} - {1}{3}</span>", HtmlHelperExtra.Resource("now", objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate).ToString(), HtmlHelperExtra.Resource("currencySymbol", objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate).ToString(), saleMinPrice, saleMaxPrice);
                }

            }

            if (IsMobileDevice())
            {
                //  Price line is for mobile device - needs to be on one line
                if (linePrice.Contains("<br />"))
                {
                    linePrice = linePrice.Replace("<br />", "");
                }
            }

            return String.Format("{0}{1}", linePrice, lineSalePrice);
        }


        public static string displayPriceRange(Decimal? passMinPrice, Decimal? passMaxPrice,
                                                Decimal? passMinSalePrice, Decimal? passMaxSalePrice, countryItem objCountryItem, DateTime useDate)
        {
            Boolean onSale = false;
            Boolean salePriceSame = false;
            Boolean priceSame = false;
            
            if (passMinPrice == null)
            {
                passMinPrice = passMaxPrice;
            }
            if (passMaxPrice == null)
            {
                passMaxPrice = passMinPrice;
            }
            if (passMinPrice == null)
            {
                passMinPrice = 0;
                passMaxPrice = 0;
            }
            if (passMinPrice == passMaxPrice)
            {
                priceSame = true;
            }
            if ((passMinSalePrice == null) && (passMaxSalePrice == null)) { }
            else
            {
                onSale = true;
                if (passMinSalePrice == passMaxSalePrice)
                {
                    salePriceSame = true;
                }
            }


            String minPrice = (passMinPrice % 1) > 0 ? convertPriceToDisplayWithDP(passMinPrice) : convertPriceToDisplayWithoutDP(passMinPrice);
            String maxPrice = (passMaxPrice % 1) > 0 ? convertPriceToDisplayWithDP(passMaxPrice) : convertPriceToDisplayWithoutDP(passMaxPrice);
            String linePrice = "<span class=\"desc pod\">";

            if (priceSame == true)
            {
                if (onSale == true)
                {
                    linePrice = String.Format("{0}{1} <del class=\"no-strike\">{2}{3}</del>",
                        linePrice,
                        HtmlHelperExtra.Resource("was", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString(),
                        HtmlHelperExtra.Resource("currencySymbol", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString(),
                        minPrice
                    );
                }
                else
                {
                    linePrice = String.Format("{0}{1}{2}",
                        linePrice,
                        HtmlHelperExtra.Resource("currencySymbol", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString(),
                        minPrice
                    );
                }
            }
            else
            {
                if (onSale == true)
                {
                    linePrice = String.Format("{0}{1} <del class=\"no-strike\">{2}{3}</del> - <del class=\"no-strike\">{2}{4}</del>",
                        linePrice,
                        HtmlHelperExtra.Resource("was", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString(),
                        HtmlHelperExtra.Resource("currencySymbol", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString(),
                        minPrice,
                        maxPrice
                    );
                }
                else
                {
                    linePrice = String.Format("{0}{1}{2} - {1}{3}",
                        linePrice,
                        HtmlHelperExtra.Resource("currencySymbol", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString(),
                        minPrice,
                        maxPrice
                    );
                }
            }
            linePrice = String.Format("{0}</span>", linePrice);


            String lineSalePrice = "";
            if (onSale == true)
            {
                string saleMinPrice = (passMinSalePrice % 1) > 0 ? convertPriceToDisplayWithDP(passMinSalePrice) : convertPriceToDisplayWithoutDP(passMinSalePrice);
                string saleMaxPrice = (passMaxSalePrice % 1) > 0 ? convertPriceToDisplayWithDP(passMaxSalePrice) : convertPriceToDisplayWithoutDP(passMaxSalePrice);
                if (salePriceSame == true)
                {
                    lineSalePrice = String.Format("<span class=\"desc pod-now\">{0} {1}{2}</span>",
                        HtmlHelperExtra.Resource("now", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString(),
                        HtmlHelperExtra.Resource("currencySymbol", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString(),
                        saleMinPrice);
                }
                else
                {
                    lineSalePrice = String.Format("<span class=\"desc pod-now\">{0} {1}{2} - {1}{3}</span>",
                        HtmlHelperExtra.Resource("now", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString(),
                        HtmlHelperExtra.Resource("currencySymbol", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString(),
                        saleMinPrice,
                        saleMaxPrice);
                }

            }

            return String.Format("{0}{1}", linePrice, lineSalePrice);
        }


        public static string convertDescriptionForDisplay(String passDisplay)
        {
            if (passDisplay == null)
            {
                passDisplay = "&nbsp;";
            }
            else if (passDisplay == "")
            {
                passDisplay = "&nbsp;";
            }
            else
            {
                passDisplay = passDisplay.Replace(Environment.NewLine, "<br />");
                passDisplay = passDisplay.Replace("\r", "<br />");
                passDisplay = passDisplay.Replace("\n", "<br />");

            }
            return string.Format("<p>{0}</p>", passDisplay);

        }


        public static string displayBasketSummaryText(decimal? totalItemsInBasket, countryItem objCountryItem, DateTime useDate)
        {
            //@(HtmlHelperExtra.Resource("currencySymbol", objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate).ToString())
            //@(Model.BasketValue.ToString("0.00"))
            if (totalItemsInBasket == 1)
            {
                return String.Format("({0} {1})", totalItemsInBasket.HasValue ? totalItemsInBasket.Value.ToString("0.##") : "0", HtmlHelperExtra.Resource("item", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString()
                );
            }
            else
            {
                return String.Format("({0} {1})", totalItemsInBasket.HasValue ? totalItemsInBasket.Value.ToString("0.##") : "0", HtmlHelperExtra.Resource("items", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString());
            }
        }

        /// <summary>
        /// Converts an integer to its ordinal representation
        /// thanks, AaronHS - http://stackoverflow.com/questions/9601593/how-can-i-format-07-03-2012-to-march-7th-2012-in-c-sharp
        /// </summary>
        public static String convertIntToOrdinal(this Int32 number)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException("number");

            var work = number.ToString("n0");

            var modOf100 = number % 100;

            if (modOf100 == 11 || modOf100 == 12 || modOf100 == 13)
                return work + "th";

            switch (number % 10)
            {
                case 1:
                    work += "st"; break;
                case 2:
                    work += "nd"; break;
                case 3:
                    work += "rd"; break;
                default:
                    work += "th"; break;
            }

            return work;
        }

        public static String convertIntToSuperscriptOrdinal(this Int32 number)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException("number");

            var work = number.ToString("n0");

            var modOf100 = number % 100;

            if (modOf100 == 11 || modOf100 == 12 || modOf100 == 13)
                return work + "<sup>th</sup>";

            switch (number % 10)
            {
                case 1:
                    work += "<sup>st</sup>"; break;
                case 2:
                    work += "<sup>nd</sup>"; break;
                case 3:
                    work += "<sup>rd</sup>"; break;
                default:
                    work += "<sup>th</sup>"; break;
            }

            return work;
        }

        #endregion


        #region User Identification Cookie Routines


        public static string getIdentificationFromCookie()
        {

            string identification = "";
            HttpCookie cookie = HttpContext.Current.Request.Cookies["_dgVisitIdent"];
            if (cookie != null)
            {
                identification = cookie.Value;
            }
            return identification;
        }


        public static void saveIdentificationToCookie(string identification)
        {
            HttpCookie cookie = new HttpCookie("_dgVisitIdent")
            {
                Value = identification,
                Expires = DateTime.Now.AddYears(1),
                SameSite = SameSiteMode.None
            };
            HttpContext.Current.Response.Cookies.Add(cookie);
        }


        public static async Task<VisitorSelect> identifySession(VisitorSelect objVisitorSelect, VisitorFactory objVisitorFactory, string guidIdentification, string cfCountryHeader, string cfDevice)
        {
            if (guidIdentification == "") { }
            else
            {
                objVisitorSelect = await objVisitorFactory.callVisitorSelect(guidIdentification, cfCountryHeader, cfDevice);
            }

            return objVisitorSelect;
        }

        public static async Task<identificationDefinition> generateNewVisitorId(VisitorFactory visitorFactory, String userAgent, String ipAddress, String culture, String cloudFlareCountryCode, string cfDevice)
        {
            identificationDefinition objUser = await visitorFactory.callVisitorInsert(userAgent, ipAddress, culture, cloudFlareCountryCode, cfDevice);

            return objUser;
        }

        #endregion


        #region Country Routines

        public static countryItem getCountryItemFromCountryLanguageID(byte? countryLanguageID, DateTime useDate)
        {
            cmsCountry objCountry = new cmsCountry();
            cmsCountryDefinition objCountryData = objCountry.countryLanguageSelect(useDate).Result;

            return objCountryData.countryItems.Where(p => p.CountryLanguageID == countryLanguageID.Value).Single();
        }

        public static String getDefaultCountry()
        {
            return "en-gb";
        }

        public static bool doesCountryExistFromText(string text, cmsCountryDefinition objCountryData)
        {
            if (objCountryData.countryItems.Where(p => p.URL.ToUpper() == text.ToUpper()).Count() == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool doesCountryExistFromCode(string code, cmsCountryDefinition objCountryData)
        {
            if (objCountryData.countryItems.Where(p => p.CountryCode.ToUpper() == code.ToUpper()).Count() == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static countryItem getCountryFromText(string text,
                                                                cmsCountryDefinition objCountryData)
        {
            return objCountryData.countryItems.Where(p => p.URL.ToUpper() == text.ToUpper()).First();
        }


        public static countryItem getCountryFromCode(string code,
                                                                cmsCountryDefinition objCountryData)
        {
            return objCountryData.countryItems.Where(p => p.CountryCode.ToUpper() == code.ToUpper()).First();
        }

        public static async Task<countryAwarenessCheckReturn> isCountryAwarenessPromptRequired(Int64 visitorDbId, short countryId, DateTime contentDate, short? countryIdOverride)
        {
            wwwDesignersGuild.Models.Visitor.VisitorFactory objVisitor = new wwwDesignersGuild.Models.Visitor.VisitorFactory();

            return await objVisitor.countryAwarenessCheck(visitorDbId, countryId, contentDate, countryIdOverride);
        }

        #endregion


        #region Security Routines
        
        public static Boolean isUserLoggedIn(object sessionLoginStatus)
        {
            Boolean _loggedIn = false;

            if (sessionLoginStatus.ToString() == dgLoginStatus.currentLogin.ToString())
            {
                _loggedIn = true;
            }

            return _loggedIn;
        }


        public static Boolean doesUserHaveACheckoutId(object sessionCheckoutId)
        {
            Boolean _checkoutIdPresent = false;
            if (sessionCheckoutId == null) { }
            else
            {
                _checkoutIdPresent = true;
            }

            return _checkoutIdPresent;
        }


        public static Boolean isCheckoutSessionValid(object sessionCheckoutExpiry)
        {
            //return false;

            Boolean _checkoutSessionValid = false;

            if (sessionCheckoutExpiry == null) { }
            else if (DateTime.Compare(DateTime.UtcNow, Convert.ToDateTime(sessionCheckoutExpiry.ToString())) > 0) { }
            else
            {
                _checkoutSessionValid = true;
            }

            return _checkoutSessionValid;
        }


        #endregion


        #region Validation Routines


        public static Boolean emailChecker(string passEmail)
        {
            if (new EmailAddressAttribute().IsValid(passEmail))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        #endregion


        #region Other Routines

        public static string CallToActionStyleString(PageSectionAnimationSlideCallToAction call)
        {
            var result = "";

            if (call.Margin != null)
                result += $"margin: {call.Margin};";
            if (call.Padding != null)
                result += $"padding: {call.Padding};";
            if (call.BackgroundColour != null)
                result += $"background-color: {call.BackgroundColour};";
            if (call.Colour != null)
                result += $"color: {call.Colour};";
            if (call.BorderStyle != null)
                result += $"border: {call.BorderWidth}px {call.BorderStyle} {call.BorderColour};";
            if (call.Opacity != null)
                result += $"opacity: {call.Opacity};";

            return result;
        }

        public static string getIPAddress()
        {
            return HttpContext.Current.Request.Headers["CF-Connecting-IP"] ?? HttpContext.Current.Request.UserHostAddress;
        }


        public static string getUserAgent()
        {
            String tempUserAgent = "Unavailable";
            if (HttpContext.Current.Request == null) { }
            else
            {
                if (HttpContext.Current.Request.UserAgent == null) { }
                else
                {
                    tempUserAgent = HttpContext.Current.Request.UserAgent;
                }
            }

            return tempUserAgent;
        }


        public static Boolean isPagePartOfCheckOutProcess(dgContentType tempContentType, int tempContentId)
        {
            if (tempContentType != dgContentType.landingPage && 
                tempContentType != dgContentType.checkoutPart)
            {
                return false;
            }

            if (tempContentType == dgContentType.checkoutPart)
            {
                if (tempContentId == (int)dgFixedPage.checkoutAddress ||
                    tempContentId == (int)dgFixedPage.checkoutDispatch ||
                    tempContentId == (int)dgFixedPage.checkoutGift ||
                    tempContentId == (int)dgFixedPage.checkoutGiftVoucher ||
                    tempContentId == (int)dgFixedPage.checkoutTotals ||
                    tempContentId == (int)dgFixedPage.checkoutPay ||
                    tempContentId == (int)dgFixedPage.checkoutConfirmation ||
                    tempContentId == (int)dgFixedPage.checkoutConfirmationSpartan)
                {
                    return true;
                }
            }

            if (tempContentType == dgContentType.landingPage)
            {
                if (
                    tempContentId == (int)dgFixedPage.checkoutLogin ||
                    tempContentId == (int)dgFixedPage.checkoutSessionExpired ||
                    tempContentId == (int)dgFixedPage.checkoutProblem ||
                    tempContentId == (int)dgFixedPage.checkoutSinglePage ||
                    tempContentId == (int)dgFixedPage.checkoutConfirmation ||
                    tempContentId == (int)dgFixedPage.checkoutConfirmationSpartan)
                {
                    return true;
                }
            }

            if (tempContentId == (int)dgFixedPage.checkoutLogin)
            {
                return true;
            }
            if (tempContentId == (int)dgFixedPage.checkoutSessionExpired)
            {
                return true;
            }
            if (tempContentId == (int)dgFixedPage.checkoutSinglePage)
            {
                return true;
            }

            return false;
        }

        #endregion



        #region Mobile Detection Routines

        [Obsolete("Do not use. Remove.")]
        public static String GetPreferredDeviceView(String viewPath)
        {
            return String.Format(viewPath, "");

            ////  Need to ensure 'viewPath' is a string that contains '{0}'
            //String viewExtension = IsMobileDevice() ? ".mobile" : "";

            //return String.Format(viewPath, viewExtension);
        }

        public static Boolean IsMobileDevice()
        {
            MobileDetectionDefinition mobileDetectionResult = getDeviceData();
            bool isMobile = !mobileDetectionResult.IsTablet && mobileDetectionResult.IsMobile;

            //if (mobileDetectionResult.ScreenInchesDiagonal > 0)
            //{
            //    isMobile = (mobileDetectionResult.ScreenInchesDiagonal < 8);
            //}

            return isMobile;
        }

        public static Boolean IsTabletDevice()
        {
            MobileDetectionDefinition mobileDetectionResult = getDeviceData();
            bool isTablet = mobileDetectionResult.IsTablet;
            return isTablet;
        }
         
        public static Boolean IsAppleDevice()
        {
            MobileDetectionDefinition mobileDetectionResult = getDeviceData();
            bool isApple = mobileDetectionResult.IsAppleDevice;
            return isApple;
        }

        private static MobileDetectionDefinition getDeviceData()
        {
            MobileDetectionCacheProvider objMobileDetectionCache = new MobileDetectionCacheProvider();

            // Check in cache before performing request to 51 degrees
            string cloudflareDeviceTypeHeader = (HttpContext.Current.Request.Headers["CF-Device-Type-Override"] ??
                                                HttpContext.Current.Request.Headers["CF-Device-Type"] ?? 
                                                "mobile")?.ToLowerInvariant();


            var mobileDetectionResult = objMobileDetectionCache.GetOrCreate(cloudflareDeviceTypeHeader, () => { 

                var result = new MobileDetectionDefinition()
				{
					IsTablet = false,
					IsMobile = false,
					IsAppleDevice = false
				};

				switch (cloudflareDeviceTypeHeader?.ToLowerInvariant())
				{
					case "desktop":
						// nothing to do
						break;
					case "tablet":
					case "mobile":
						result.IsMobile = true;
						break;
				}

                return result;

			});

            return mobileDetectionResult;
        }

        internal static bool TryParseSandboxNonce(string payment_method_nonce, out Guid? paymentToken)
        {
            var guid = Guid.NewGuid();

            // might look like switch statement, ends ups as a constant hash jump table
            switch (payment_method_nonce)
            {
                case "fake-valid-nonce":
                case "fake-valid-no-billing-address-nonce":
                case "fake-valid-visa-nonce":
                case "fake-valid-amex-nonce":
                case "fake-valid-mastercard-nonce":
                case "fake-valid-discover-nonce":
                case "fake-valid-jcb-nonce":
                case "fake-valid-maestro-nonce":
                case "fake-valid-dinersclub-nonce":
                case "fake-valid-prepaid-nonce":
                case "fake-valid-commercial-nonce":
                case "fake-valid-durbin-regulated-nonce":
                case "fake-valid-healthcare-nonce":
                case "fake-valid-debit-nonce":
                case "fake-valid-payroll-nonce":
                case "fake-valid-no-indicators-nonce":
                case "fake-valid-unknown-indicators-nonce":
                case "fake-valid-country-of-issuance-usa-nonce":
                case "fake-valid-country-of-issuance-cad-nonce":
                case "fake-valid-issuing-bank-network-only-nonce":
                case "fake-android-pay-nonce":
                case "fake-android-pay-visa-nonce":
                case "fake-android-pay-mastercard-nonce":
                case "fake-android-pay-amex-nonce":
                case "fake-android-pay-discover-nonce":
                case "fake-apple-pay-amex-nonce":
                case "fake-apple-pay-visa-nonce":
                case "fake-apple-pay-mastercard-nonce":
                case "fake-apple-pay-discover-nonce":
                case "fake-paypal-one-time-nonce":
                case "fake-paypal-future-nonce":
                case "fake-paypal-billing-agreement-nonce":
                case "fake-visa-checkout-amex-nonce":
                case "fake-visa-checkout-discover-nonce":
                case "fake-visa-checkout-mastercard-nonce":
                case "fake-visa-checkout-visa-nonce":
                case "fake-masterpass-amex-nonce":
                case "fake-masterpass-discover-nonce":
                case "fake-masterpass-mastercard-nonce":
                case "fake-masterpass-visa-nonce":
                case "fake-venmo-account-nonce":
                case "fake-processor-declined-visa-nonce":
                case "fake-processor-declined-mastercard-nonce":
                case "fake-processor-declined-amex-nonce":
                case "fake-processor-declined-discover-nonce":
                case "fake-processor-declined-dinersclub-nonce":
                case "fake-processor-failure-jcb-nonce":
                case "fake-luhn-invalid-nonce":
                case "fake-consumed-nonce":
                case "fake-gateway-rejected-fraud-nonce":
                case "fake-three-digit-cvv-only-nonce":
                case "fake-three-digit-cvv-only-n-response-nonce":
                case "fake-three-digit-cvv-only-u-response-nonce":
                case "fake-three-digit-cvv-only-s-response-nonce":
                case "fake-four-digit-cvv-only-nonce":
                case "fake-four-digit-cvv-only-n-response-nonce":
                case "fake-four-digit-cvv-only-u-response-nonce":
                case "fake-four-digit-cvv-only-s-response-nonce":
                    paymentToken = guid;
                    return true;
                default:
                    paymentToken = null;
                    return false;
            }
        }

        //private static MobileDetectionDefinition performFiftyOneDegreesRequest()
        //{
        //    MobileDetectionCacheProvider objMobileDetectionCache = new MobileDetectionCacheProvider();
        //    MobileDetectionDefinition mobileDetectionResult = new MobileDetectionDefinition();
        //    String requestUrl = String.Format("https://cloud.51degrees.com/api/v1/{0}/match?user-agent={1}", FiftyOneDegreesLicenseKey, HttpUtility.UrlEncode(getUserAgent()));

        //    using (var webClient = new System.Net.WebClient())
        //    {
        //        using (var stream = webClient.OpenRead(requestUrl))
        //        {
        //            if (stream != null)
        //            {
        //                try
        //                {
        //                    stream.ReadTimeout = 1000 * 30; // 30 seconds

        //                    using (var reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8, false))
        //                    {
        //                        string json;

        //                        while ((json = reader.ReadLine()) != null)
        //                        {
        //                            if (json != String.Empty)
        //                            {
        //                                Double screenInchesDiagonal = 0;

        //                                dynamic match = JObject.Parse(json);

        //                                Double.TryParse(match.Values.ScreenInchesDiagonal[0].ToString(), out screenInchesDiagonal);

        //                                mobileDetectionResult.IsTablet = match.Values.IsTablet[0];
        //                                mobileDetectionResult.IsMobile = match.Values.IsMobile[0];
        //                                mobileDetectionResult.IsAppleDevice = match.Values.HardwareVendor[0] == "Apple";
        //                                mobileDetectionResult.ScreenInchesDiagonal = screenInchesDiagonal;

        //                                // Successful request made, store result in cache
        //                                objMobileDetectionCache.Set(getUserAgent(), mobileDetectionResult, CacheInMinutes);
        //                            }
        //                        }
        //                    }
        //                }
        //                catch (Exception)
        //                {
        //                    //  Error with 51 degrees request (default to desktop view)
        //                }
        //            }
        //        }
        //    }

        //    return mobileDetectionResult;
        //}

        #endregion

        #region Bot Detection

        private static RegexOptions RegexOptions = RegexOptions.Multiline | RegexOptions.CultureInvariant;

        private static System.Text.RegularExpressions.Regex BotRegex = new System.Text.RegularExpressions.Regex(@"(Googlebot)|(AdsBot-Google)|(AdsBot-Google-Mobile)|(http:\/\/www\.google\.com\/bot\.html)|(APIs-Google)|(GoogleOther)|(Google-InspectionTool)|(Storebot-Google)|(Mediapartners-Google)|(bingbot)|(MicrosoftPreview)", RegexOptions);

        public static bool IsBot
        {
            get 
            {
                return BotRegex.IsMatch(HttpContext.Current?.Request?.UserAgent ?? "");
            }
        }

        #endregion

        public static DateTime DetermineContentDate (string dateOverride)
        {
            var useDate = DateTime.Now;

            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);

            if (Convert.ToBoolean(WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                //  Should the "use date" be overriden?
                if (dateOverride == null) { }
                else
                {
                    String overrideDate = dateOverride;
                    if (overrideDate.Length == "ddMMyyyy".Length)
                    {
                        bool dateConvertSuccess = DateTime.TryParseExact(overrideDate,
                                                                              "ddMMyyyy",
                                                                              CultureInfo.InvariantCulture,
                                                                              DateTimeStyles.None,
                                                                              out useDate);
                        if (dateConvertSuccess == true) { }
                        else
                        {
                            useDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                        }
                    }
                }
            }

            return useDate;
        }

        public static loggedInMemberDetail checkMemberLogin (HttpSessionStateBase session)
        {
            var result = new loggedInMemberDetail();
            
            if (dgFramework.isUserLoggedIn(session["loginStatus"]) == false)
            {
                return null;
            }

            if (session["visitorIdFromDatabase"] == null)
            {
                return null;
            }
            else
            {
                result.visitorId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            if (session["memberIdFromDatabase"] == null)
            {
                return null;
            }
            else
            {
                result.memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
            }

            if (session["memberTradeAccountID"] != null)
            {
                result.tradeAccountId = Convert.ToInt32(session["memberTradeAccountID"]);
            }

            if (session["memberTradeAccountNumber"] != null)
            {
                result.tradeAccountNumber = session["memberTradeAccountNumber"].ToString();
            }

            if (session["memberTradeDataSourceJson"] != null)
            {
                result.tradeDataSourceJson = session["memberTradeDataSourceJson"].ToString();
            }

            if (session["memberEmail"] != null)
            {
                result.email = session["memberEmail"].ToString();
            }

            if (session["memberName"] != null)
            {
                result.fullName = session["memberName"].ToString();
            }

            if (Enum.TryParse(session["loginStatus"].ToString(), out dgLoginStatus status))
            {
                result.loginStatus = status;
            }

            return result;
        }

        public static loggedInMemberDetail ensureMemberLoginWebApi(HttpSessionState session, bool mandateTrade = false)
        {
            var result = new loggedInMemberDetail();
            
            if (dgFramework.isUserLoggedIn(session["loginStatus"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.SecurityFail);
            }

            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                result.visitorId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            if (session["memberIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                result.memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
            }
            
            if (mandateTrade)
            {
                if (session["memberTradeAccountID"] == null ||
                    session["memberTradeAccountNumber"] == null)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.SecurityFail);
                }
            }

            if (session["memberTradeAccountID"] != null)
            {
                result.tradeAccountId = Convert.ToInt32(session["memberTradeAccountID"]);
            }

            if (session["memberTradeAccountNumber"] != null)
            {
                result.tradeAccountNumber = session["memberTradeAccountNumber"].ToString();
            }

            if (session["memberTradeDataSourceJson"] != null)
            {
                result.tradeDataSourceJson = session["memberTradeDataSourceJson"].ToString();
            }

            if (session["memberEmail"] != null)
            {
                result.email = session["memberEmail"].ToString();
            }

            if (session["memberName"] != null)
            {
                result.fullName = session["memberName"].ToString();
            }

            if (Enum.TryParse(session["loginStatus"].ToString(), out dgLoginStatus status))
            {
                result.loginStatus = status;
            }

            return result;
        }


        public static readonly string FoundationEmailCSS = @"
.wrapper {
  width: 100%;
}

#outlook a {
  padding: 0;
}

body {
  width: 100% !important;
  min-width: 100%;
  -webkit-text-size-adjust: 100%;
  -ms-text-size-adjust: 100%;
  margin: 0;
  Margin: 0;
  padding: 0;
  -moz-box-sizing: border-box;
  -webkit-box-sizing: border-box;
  box-sizing: border-box;
}

.ExternalClass {
  width: 100%;
}
.ExternalClass,
.ExternalClass p,
.ExternalClass span,
.ExternalClass font,
.ExternalClass td,
.ExternalClass div {
  line-height: 100%;
}

#backgroundTable {
  margin: 0;
  Margin: 0;
  padding: 0;
  width: 100% !important;
  line-height: 100% !important;
}

img {
  outline: none;
  text-decoration: none;
  -ms-interpolation-mode: bicubic;
  width: auto;
  max-width: 100%;
  clear: both;
  display: block;
}

center {
  width: 100%;
  min-width: 640px;
}

a img {
  border: none;
}

table {
  border-spacing: 0;
  border-collapse: collapse;
}

td {
  word-wrap: break-word;
  -webkit-hyphens: auto;
  -moz-hyphens: auto;
  hyphens: auto;
  border-collapse: collapse !important;
}

table, tr, td {
  padding: 0;
  vertical-align: top;
  text-align: left;
}

@media only screen {
  html {
    min-height: 100%;
    background: #f3f3f3;
  }
}
table.body {
  background: #f3f3f3;
  height: 100%;
  width: 100%;
}
table.container {
  background: #fefefe;
  width: 640px;
  margin: 0 auto;
  Margin: 0 auto;
  text-align: inherit;
}
table.row {
  padding: 0;
  width: 100%;
  position: relative;
}
table.spacer {
  width: 100%;
}
table.spacer td {
  mso-line-height-rule: exactly;
}

table.container table.row {
  display: table;
}

td.columns,
td.column,
th.columns,
th.column {
  margin: 0 auto;
  Margin: 0 auto;
  padding-left: 16px;
  padding-bottom: 16px;
}
td.columns .column,
td.columns .columns,
td.column .column,
td.column .columns,
th.columns .column,
th.columns .columns,
th.column .column,
th.column .columns {
  padding-left: 0 !important;
  padding-right: 0 !important;
}
td.columns .column center,
td.columns .columns center,
td.column .column center,
td.column .columns center,
th.columns .column center,
th.columns .columns center,
th.column .column center,
th.column .columns center {
  min-width: none !important;
}

td.columns.last,
td.column.last,
th.columns.last,
th.column.last {
  padding-right: 16px;
}

td.columns table:not(.button),
td.column table:not(.button),
th.columns table:not(.button),
th.column table:not(.button) {
  width: 100%;
}

td.large-1,
th.large-1 {
  width: 32.3333333333px;
  padding-left: 8px;
  padding-right: 8px;
}

td.large-1.first,
th.large-1.first {
  padding-left: 16px;
}

td.large-1.last,
th.large-1.last {
  padding-right: 16px;
}

.collapse > tbody > tr > td.large-1,
.collapse > tbody > tr > th.large-1 {
  padding-right: 0;
  padding-left: 0;
  width: 48.3333333333px;
}
.collapse td.large-1.first,
.collapse th.large-1.first,
.collapse td.large-1.last,
.collapse th.large-1.last {
  width: 56.3333333333px;
}

td.large-1 center,
th.large-1 center {
  min-width: 0.3333333333px;
}

.body .columns td.large-1,
.body .column td.large-1,
.body .columns th.large-1,
.body .column th.large-1 {
  width: 8.333333%;
}

td.large-2,
th.large-2 {
  width: 80.6666666667px;
  padding-left: 8px;
  padding-right: 8px;
}

td.large-2.first,
th.large-2.first {
  padding-left: 16px;
}

td.large-2.last,
th.large-2.last {
  padding-right: 16px;
}

.collapse > tbody > tr > td.large-2,
.collapse > tbody > tr > th.large-2 {
  padding-right: 0;
  padding-left: 0;
  width: 96.6666666667px;
}
.collapse td.large-2.first,
.collapse th.large-2.first,
.collapse td.large-2.last,
.collapse th.large-2.last {
  width: 104.6666666667px;
}

td.large-2 center,
th.large-2 center {
  min-width: 48.6666666667px;
}

.body .columns td.large-2,
.body .column td.large-2,
.body .columns th.large-2,
.body .column th.large-2 {
  width: 16.666666%;
}

td.large-3,
th.large-3 {
  width: 129px;
  padding-left: 8px;
  padding-right: 8px;
}

td.large-3.first,
th.large-3.first {
  padding-left: 16px;
}

td.large-3.last,
th.large-3.last {
  padding-right: 16px;
}

.collapse > tbody > tr > td.large-3,
.collapse > tbody > tr > th.large-3 {
  padding-right: 0;
  padding-left: 0;
  width: 145px;
}
.collapse td.large-3.first,
.collapse th.large-3.first,
.collapse td.large-3.last,
.collapse th.large-3.last {
  width: 153px;
}

td.large-3 center,
th.large-3 center {
  min-width: 97px;
}

.body .columns td.large-3,
.body .column td.large-3,
.body .columns th.large-3,
.body .column th.large-3 {
  width: 25%;
}

td.large-4,
th.large-4 {
  width: 177.3333333333px;
  padding-left: 8px;
  padding-right: 8px;
}

td.large-4.first,
th.large-4.first {
  padding-left: 16px;
}

td.large-4.last,
th.large-4.last {
  padding-right: 16px;
}

.collapse > tbody > tr > td.large-4,
.collapse > tbody > tr > th.large-4 {
  padding-right: 0;
  padding-left: 0;
  width: 193.3333333333px;
}
.collapse td.large-4.first,
.collapse th.large-4.first,
.collapse td.large-4.last,
.collapse th.large-4.last {
  width: 201.3333333333px;
}

td.large-4 center,
th.large-4 center {
  min-width: 145.3333333333px;
}

.body .columns td.large-4,
.body .column td.large-4,
.body .columns th.large-4,
.body .column th.large-4 {
  width: 33.333333%;
}

td.large-5,
th.large-5 {
  width: 225.6666666667px;
  padding-left: 8px;
  padding-right: 8px;
}

td.large-5.first,
th.large-5.first {
  padding-left: 16px;
}

td.large-5.last,
th.large-5.last {
  padding-right: 16px;
}

.collapse > tbody > tr > td.large-5,
.collapse > tbody > tr > th.large-5 {
  padding-right: 0;
  padding-left: 0;
  width: 241.6666666667px;
}
.collapse td.large-5.first,
.collapse th.large-5.first,
.collapse td.large-5.last,
.collapse th.large-5.last {
  width: 249.6666666667px;
}

td.large-5 center,
th.large-5 center {
  min-width: 193.6666666667px;
}

.body .columns td.large-5,
.body .column td.large-5,
.body .columns th.large-5,
.body .column th.large-5 {
  width: 41.666666%;
}

td.large-6,
th.large-6 {
  width: 274px;
  padding-left: 8px;
  padding-right: 8px;
}

td.large-6.first,
th.large-6.first {
  padding-left: 16px;
}

td.large-6.last,
th.large-6.last {
  padding-right: 16px;
}

.collapse > tbody > tr > td.large-6,
.collapse > tbody > tr > th.large-6 {
  padding-right: 0;
  padding-left: 0;
  width: 290px;
}
.collapse td.large-6.first,
.collapse th.large-6.first,
.collapse td.large-6.last,
.collapse th.large-6.last {
  width: 298px;
}

td.large-6 center,
th.large-6 center {
  min-width: 242px;
}

.body .columns td.large-6,
.body .column td.large-6,
.body .columns th.large-6,
.body .column th.large-6 {
  width: 50%;
}

td.large-7,
th.large-7 {
  width: 322.3333333333px;
  padding-left: 8px;
  padding-right: 8px;
}

td.large-7.first,
th.large-7.first {
  padding-left: 16px;
}

td.large-7.last,
th.large-7.last {
  padding-right: 16px;
}

.collapse > tbody > tr > td.large-7,
.collapse > tbody > tr > th.large-7 {
  padding-right: 0;
  padding-left: 0;
  width: 338.3333333333px;
}
.collapse td.large-7.first,
.collapse th.large-7.first,
.collapse td.large-7.last,
.collapse th.large-7.last {
  width: 346.3333333333px;
}

td.large-7 center,
th.large-7 center {
  min-width: 290.3333333333px;
}

.body .columns td.large-7,
.body .column td.large-7,
.body .columns th.large-7,
.body .column th.large-7 {
  width: 58.333333%;
}

td.large-8,
th.large-8 {
  width: 370.6666666667px;
  padding-left: 8px;
  padding-right: 8px;
}

td.large-8.first,
th.large-8.first {
  padding-left: 16px;
}

td.large-8.last,
th.large-8.last {
  padding-right: 16px;
}

.collapse > tbody > tr > td.large-8,
.collapse > tbody > tr > th.large-8 {
  padding-right: 0;
  padding-left: 0;
  width: 386.6666666667px;
}
.collapse td.large-8.first,
.collapse th.large-8.first,
.collapse td.large-8.last,
.collapse th.large-8.last {
  width: 394.6666666667px;
}

td.large-8 center,
th.large-8 center {
  min-width: 338.6666666667px;
}

.body .columns td.large-8,
.body .column td.large-8,
.body .columns th.large-8,
.body .column th.large-8 {
  width: 66.666666%;
}

td.large-9,
th.large-9 {
  width: 419px;
  padding-left: 8px;
  padding-right: 8px;
}

td.large-9.first,
th.large-9.first {
  padding-left: 16px;
}

td.large-9.last,
th.large-9.last {
  padding-right: 16px;
}

.collapse > tbody > tr > td.large-9,
.collapse > tbody > tr > th.large-9 {
  padding-right: 0;
  padding-left: 0;
  width: 435px;
}
.collapse td.large-9.first,
.collapse th.large-9.first,
.collapse td.large-9.last,
.collapse th.large-9.last {
  width: 443px;
}

td.large-9 center,
th.large-9 center {
  min-width: 387px;
}

.body .columns td.large-9,
.body .column td.large-9,
.body .columns th.large-9,
.body .column th.large-9 {
  width: 75%;
}

td.large-10,
th.large-10 {
  width: 467.3333333333px;
  padding-left: 8px;
  padding-right: 8px;
}

td.large-10.first,
th.large-10.first {
  padding-left: 16px;
}

td.large-10.last,
th.large-10.last {
  padding-right: 16px;
}

.collapse > tbody > tr > td.large-10,
.collapse > tbody > tr > th.large-10 {
  padding-right: 0;
  padding-left: 0;
  width: 483.3333333333px;
}
.collapse td.large-10.first,
.collapse th.large-10.first,
.collapse td.large-10.last,
.collapse th.large-10.last {
  width: 491.3333333333px;
}

td.large-10 center,
th.large-10 center {
  min-width: 435.3333333333px;
}

.body .columns td.large-10,
.body .column td.large-10,
.body .columns th.large-10,
.body .column th.large-10 {
  width: 83.333333%;
}

td.large-11,
th.large-11 {
  width: 515.6666666667px;
  padding-left: 8px;
  padding-right: 8px;
}

td.large-11.first,
th.large-11.first {
  padding-left: 16px;
}

td.large-11.last,
th.large-11.last {
  padding-right: 16px;
}

.collapse > tbody > tr > td.large-11,
.collapse > tbody > tr > th.large-11 {
  padding-right: 0;
  padding-left: 0;
  width: 531.6666666667px;
}
.collapse td.large-11.first,
.collapse th.large-11.first,
.collapse td.large-11.last,
.collapse th.large-11.last {
  width: 539.6666666667px;
}

td.large-11 center,
th.large-11 center {
  min-width: 483.6666666667px;
}

.body .columns td.large-11,
.body .column td.large-11,
.body .columns th.large-11,
.body .column th.large-11 {
  width: 91.666666%;
}

td.large-12,
th.large-12 {
  width: 564px;
  padding-left: 8px;
  padding-right: 8px;
}

td.large-12.first,
th.large-12.first {
  padding-left: 16px;
}

td.large-12.last,
th.large-12.last {
  padding-right: 16px;
}

.collapse > tbody > tr > td.large-12,
.collapse > tbody > tr > th.large-12 {
  padding-right: 0;
  padding-left: 0;
  width: 640px;
}
.collapse td.large-12.first,
.collapse th.large-12.first,
.collapse td.large-12.last,
.collapse th.large-12.last {
  width: 648px;
}

td.large-12 center,
th.large-12 center {
  min-width: 532px;
}

.body .columns td.large-12,
.body .column td.large-12,
.body .columns th.large-12,
.body .column th.large-12 {
  width: 100%;
}

td.large-offset-1,
td.large-offset-1.first,
td.large-offset-1.last,
th.large-offset-1,
th.large-offset-1.first,
th.large-offset-1.last {
  padding-left: 64.3333333333px;
}

td.large-offset-2,
td.large-offset-2.first,
td.large-offset-2.last,
th.large-offset-2,
th.large-offset-2.first,
th.large-offset-2.last {
  padding-left: 112.6666666667px;
}

td.large-offset-3,
td.large-offset-3.first,
td.large-offset-3.last,
th.large-offset-3,
th.large-offset-3.first,
th.large-offset-3.last {
  padding-left: 161px;
}

td.large-offset-4,
td.large-offset-4.first,
td.large-offset-4.last,
th.large-offset-4,
th.large-offset-4.first,
th.large-offset-4.last {
  padding-left: 209.3333333333px;
}

td.large-offset-5,
td.large-offset-5.first,
td.large-offset-5.last,
th.large-offset-5,
th.large-offset-5.first,
th.large-offset-5.last {
  padding-left: 257.6666666667px;
}

td.large-offset-6,
td.large-offset-6.first,
td.large-offset-6.last,
th.large-offset-6,
th.large-offset-6.first,
th.large-offset-6.last {
  padding-left: 306px;
}

td.large-offset-7,
td.large-offset-7.first,
td.large-offset-7.last,
th.large-offset-7,
th.large-offset-7.first,
th.large-offset-7.last {
  padding-left: 354.3333333333px;
}

td.large-offset-8,
td.large-offset-8.first,
td.large-offset-8.last,
th.large-offset-8,
th.large-offset-8.first,
th.large-offset-8.last {
  padding-left: 402.6666666667px;
}

td.large-offset-9,
td.large-offset-9.first,
td.large-offset-9.last,
th.large-offset-9,
th.large-offset-9.first,
th.large-offset-9.last {
  padding-left: 451px;
}

td.large-offset-10,
td.large-offset-10.first,
td.large-offset-10.last,
th.large-offset-10,
th.large-offset-10.first,
th.large-offset-10.last {
  padding-left: 499.3333333333px;
}

td.large-offset-11,
td.large-offset-11.first,
td.large-offset-11.last,
th.large-offset-11,
th.large-offset-11.first,
th.large-offset-11.last {
  padding-left: 547.6666666667px;
}

td.expander,
th.expander {
  visibility: hidden;
  width: 0;
  padding: 0 !important;
}

table.container.radius {
  border-radius: 0;
  border-collapse: separate;
}

.block-grid {
  width: 100%;
  max-width: 640px;
}
.block-grid td {
  display: inline-block;
  padding: 8px;
}

.up-2 td {
  width: 274px !important;
}

.up-3 td {
  width: 177px !important;
}

.up-4 td {
  width: 129px !important;
}

.up-5 td {
  width: 100px !important;
}

.up-6 td {
  width: 80px !important;
}

.up-7 td {
  width: 66px !important;
}

.up-8 td {
  width: 56px !important;
}

table.text-center,
th.text-center,
td.text-center,
h1.text-center,
h2.text-center,
h3.text-center,
h4.text-center,
h5.text-center,
h6.text-center,
p.text-center,
span.text-center {
  text-align: center;
}
table.text-left,
th.text-left,
td.text-left,
h1.text-left,
h2.text-left,
h3.text-left,
h4.text-left,
h5.text-left,
h6.text-left,
p.text-left,
span.text-left {
  text-align: left;
}
table.text-right,
th.text-right,
td.text-right,
h1.text-right,
h2.text-right,
h3.text-right,
h4.text-right,
h5.text-right,
h6.text-right,
p.text-right,
span.text-right {
  text-align: right;
}

span.text-center {
  display: block;
  width: 100%;
  text-align: center;
}

@media only screen and (max-width: 596px) {

    html, 
    body {
        background-color: white !important;
        min-width: unset !important;
    }

    body {
        height:100% !important;
        margin:0 !important;
        padding:0 !important;
        width:100vw !important;
    }

  .small-float-center {
    margin: 0 auto !important;
    float: none !important;
    text-align: center !important;
  }

  .small-text-center {
    text-align: center !important;
  }

  .small-text-left {
    text-align: left !important;
  }

  .small-text-right {
    text-align: right !important;
  }
}
img.float-left {
  float: left;
  text-align: left;
}

img.float-right {
  float: right;
  text-align: right;
}

img.float-center,
img.text-center {
  margin: 0 auto;
  Margin: 0 auto;
  float: none;
  text-align: center;
}

table.float-center,
td.float-center,
th.float-center {
  margin: 0 auto;
  Margin: 0 auto;
  float: none;
  text-align: center;
}

.hide-for-large {
  display: none !important;
  mso-hide: all;
  overflow: hidden;
  max-height: 0;
  font-size: 0;
  width: 0;
  line-height: 0;
}
@media only screen and (max-width: 596px) {
  .hide-for-large {
    display: block !important;
    width: auto !important;
    overflow: visible !important;
    max-height: none !important;
    font-size: inherit !important;
    line-height: inherit !important;
  }
}

table.body table.container .hide-for-large * {
  mso-hide: all;
}

@media only screen and (max-width: 596px) {
  table.body table.container .hide-for-large,
  table.body table.container .row.hide-for-large {
    display: table !important;
    width: 100% !important;
  }
}

@media only screen and (max-width: 596px) {
  table.body table.container .callout-inner.hide-for-large {
    display: table-cell !important;
    width: 100% !important;
  }
}

@media only screen and (max-width: 596px) {
  table.body table.container .show-for-large {
    display: none !important;
    width: 0;
    mso-hide: all;
    overflow: hidden;
  }
}

body,
table.body,
h1,
h2,
h3,
h4,
h5,
h6,
p,
td,
th,
a {
  color: #0a0a0a;
  font-family: Lucida, Arial, sans-serif;
  font-weight: normal;
  padding: 0;
  margin: 0;
  Margin: 0;
  text-align: left;
  line-height: 1.3;
}

h1,
h2,
h3,
h4,
h5,
h6 {
  color: inherit;
  word-wrap: normal;
  font-family: Lucida, Arial, sans-serif;
  font-weight: normal;
  margin-bottom: 10px;
  Margin-bottom: 10px;
}

h1 {
  font-size: 34px;
}

h2 {
  font-size: 30px;
}

h3 {
  font-size: 28px;
}

h4 {
  font-size: 24px;
}

h5 {
  font-size: 20px;
}

h6 {
  font-size: 18px;
}

body,
table.body,
p,
td,
th {
  font-size: 16px;
  line-height: 1.3;
}

p {
  margin-bottom: 10px;
  Margin-bottom: 10px;
}
p.lead {
  font-size: 20px;
  line-height: 1.6;
}
p.subheader {
  margin-top: 4px;
  margin-bottom: 8px;
  Margin-top: 4px;
  Margin-bottom: 8px;
  font-weight: normal;
  line-height: 1.4;
  color: #8a8a8a;
}

small {
  font-size: 80%;
  color: #cacaca;
}

a {
  color: #2199e8;
  text-decoration: none;
}
a:hover {
  color: #147dc2;
}
a:active {
  color: #147dc2;
}
a:visited {
  color: #2199e8;
}

h1 a,
h1 a:visited,
h2 a,
h2 a:visited,
h3 a,
h3 a:visited,
h4 a,
h4 a:visited,
h5 a,
h5 a:visited,
h6 a,
h6 a:visited {
  color: #2199e8;
}

pre {
  background: #f3f3f3;
  margin: 30px 0;
  Margin: 30px 0;
}
pre code {
  color: #cacaca;
}
pre code span.callout {
  color: #8a8a8a;
  font-weight: bold;
}
pre code span.callout-strong {
  color: #ff6908;
  font-weight: bold;
}

table.hr {
  width: 100%;
}
table.hr th {
  height: 0;
  max-width: 640px;
  border-top: 0;
  border-right: 0;
  border-bottom: 1px solid #0a0a0a;
  border-left: 0;
  margin: 20px auto;
  Margin: 20px auto;
  clear: both;
}

.stat {
  font-size: 40px;
  line-height: 1;
}
p + .stat {
  margin-top: -16px;
  Margin-top: -16px;
}

span.preheader {
  display: none !important;
  visibility: hidden;
  mso-hide: all !important;
  font-size: 1px;
  color: #f3f3f3;
  line-height: 1px;
  max-height: 0px;
  max-width: 0px;
  opacity: 0;
  overflow: hidden;
}

table.button {
  width: auto;
  margin: 0 0 16px 0;
  Margin: 0 0 16px 0;
}
table.button table td {
  text-align: left;
  color: #fefefe;
  background: #2199e8;
  border: 2px solid #2199e8;
}
table.button table td a {
  font-family: Lucida, Arial, sans-serif;
  font-size: 16px;
  font-weight: bold;
  color: #fefefe;
  text-decoration: none;
  display: inline-block;
  padding: 8px 16px 8px 16px;
  border: 0 solid #2199e8;
  border-radius: 3px;
}
table.button.radius table td {
  border-radius: 3px;
  border: none;
}
table.button.rounded table td {
  border-radius: 500px;
  border: none;
}

table.button:hover table tr td a,
table.button:active table tr td a,
table.button table tr td a:visited,
table.button.tiny:hover table tr td a,
table.button.tiny:active table tr td a,
table.button.tiny table tr td a:visited,
table.button.small:hover table tr td a,
table.button.small:active table tr td a,
table.button.small table tr td a:visited,
table.button.large:hover table tr td a,
table.button.large:active table tr td a,
table.button.large table tr td a:visited {
  color: #fefefe;
}

table.button.tiny table td,
table.button.tiny table a {
  padding: 4px 8px 4px 8px;
}
table.button.tiny table a {
  font-size: 10px;
  font-weight: normal;
}

table.button.small table td,
table.button.small table a {
  padding: 5px 10px 5px 10px;
  font-size: 16px;
}

table.button.large table a {
  padding: 10px 20px 10px 20px;
  font-size: 20px;
}

table.button.expand,
table.button.expanded {
  width: 100% !important;
}
table.button.expand table,
table.button.expanded table {
  width: 100%;
}
table.button.expand table a,
table.button.expanded table a {
  text-align: center;
  width: 100%;
  padding-left: 0;
  padding-right: 0;
}
table.button.expand center,
table.button.expanded center {
  min-width: 0;
}

table.button:hover table td,
table.button:visited table td,
table.button:active table td {
  background: #147dc2;
  color: #fefefe;
}

table.button:hover table a,
table.button:visited table a,
table.button:active table a {
  border: 0 solid #147dc2;
}

table.button.secondary table td {
  background: #777777;
  color: #fefefe;
  border: 0px solid #777777;
}
table.button.secondary table a {
  color: #fefefe;
  border: 0 solid #777777;
}

table.button.secondary:hover table td {
  background: #919191;
  color: #fefefe;
}
table.button.secondary:hover table a {
  border: 0 solid #919191;
}

table.button.secondary:hover table td a {
  color: #fefefe;
}

table.button.secondary:active table td a {
  color: #fefefe;
}

table.button.secondary table td a:visited {
  color: #fefefe;
}

table.button.success table td {
  background: #3adb76;
  border: 0px solid #3adb76;
}
table.button.success table a {
  border: 0 solid #3adb76;
}

table.button.success:hover table td {
  background: #23bf5d;
}
table.button.success:hover table a {
  border: 0 solid #23bf5d;
}

table.button.alert table td {
  background: #ec5840;
  border: 0px solid #ec5840;
}
table.button.alert table a {
  border: 0 solid #ec5840;
}

table.button.alert:hover table td {
  background: #e23317;
}
table.button.alert:hover table a {
  border: 0 solid #e23317;
}

table.button.warning table td {
  background: #ffae00;
  border: 0px solid #ffae00;
}
table.button.warning table a {
  border: 0px solid #ffae00;
}

table.button.warning:hover table td {
  background: #cc8b00;
}
table.button.warning:hover table a {
  border: 0px solid #cc8b00;
}

table.callout {
  margin-bottom: 16px;
  Margin-bottom: 16px;
}

th.callout-inner {
  width: 100%;
  border: 1px solid #cbcbcb;
  padding: 10px;
  background: #fefefe;
}
th.callout-inner.primary {
  background: #def0fc;
  border: 1px solid #444444;
  color: #0a0a0a;
}
th.callout-inner.secondary {
  background: #ebebeb;
  border: 1px solid #444444;
  color: #0a0a0a;
}
th.callout-inner.success {
  background: #e1faea;
  border: 1px solid #1b9448;
  color: #fefefe;
}
th.callout-inner.warning {
  background: #fff3d9;
  border: 1px solid #996800;
  color: #fefefe;
}
th.callout-inner.alert {
  background: #fce6e2;
  border: 1px solid #b42912;
  color: #fefefe;
}

.thumbnail {
  border: solid 4px #fefefe;
  box-shadow: 0 0 0 1px rgba(10, 10, 10, 0.2);
  display: inline-block;
  line-height: 0;
  max-width: 100%;
  transition: box-shadow 200ms ease-out;
  border-radius: 3px;
  margin-bottom: 16px;
}
.thumbnail:hover, .thumbnail:focus {
  box-shadow: 0 0 6px 1px rgba(33, 153, 232, 0.5);
}

body.outlook p {
  display: inline !important;
}

@media only screen and (max-width: 596px) {
  table.body img {
    width: auto;
    height: auto;
  }

  table.body center {
    min-width: 0 !important;
  }

  table.body .container {
    width: 95% !important;
  }

  table.body .columns,
  table.body .column {
    height: auto !important;
    -moz-box-sizing: border-box;
    -webkit-box-sizing: border-box;
    box-sizing: border-box;
    padding-left: 16px !important;
    padding-right: 16px !important;
  }
  table.body .columns .column,
  table.body .columns .columns,
  table.body .column .column,
  table.body .column .columns {
    padding-left: 0 !important;
    padding-right: 0 !important;
  }

  table.body .collapse .columns, table.body .collapse .column {
    padding-left: 0 !important;
    padding-right: 0 !important;
  }

  td.small-1,
  th.small-1 {
    display: inline-block !important;
    width: 8.333333% !important;
  }

  td.small-2,
  th.small-2 {
    display: inline-block !important;
    width: 16.666666% !important;
  }

  td.small-3,
  th.small-3 {
    display: inline-block !important;
    width: 25% !important;
  }

  td.small-4,
  th.small-4 {
    display: inline-block !important;
    width: 33.333333% !important;
  }

  td.small-5,
  th.small-5 {
    display: inline-block !important;
    width: 41.666666% !important;
  }

  td.small-6,
  th.small-6 {
    display: inline-block !important;
    width: 50% !important;
  }

  td.small-7,
  th.small-7 {
    display: inline-block !important;
    width: 58.333333% !important;
  }

  td.small-8,
  th.small-8 {
    display: inline-block !important;
    width: 66.666666% !important;
  }

  td.small-9,
  th.small-9 {
    display: inline-block !important;
    width: 75% !important;
  }

  td.small-10,
  th.small-10 {
    display: inline-block !important;
    width: 83.333333% !important;
  }

  td.small-11,
  th.small-11 {
    display: inline-block !important;
    width: 91.666666% !important;
  }

  td.small-12,
  th.small-12 {
    display: inline-block !important;
    width: 100% !important;
  }

  .columns td.small-12,
  .column td.small-12,
  .columns th.small-12,
  .column th.small-12 {
    display: block !important;
    width: 100% !important;
  }

  table.body td.small-offset-1,
  table.body th.small-offset-1 {
    margin-left: 8.333333% !important;
    Margin-left: 8.333333% !important;
  }

  table.body td.small-offset-2,
  table.body th.small-offset-2 {
    margin-left: 16.666666% !important;
    Margin-left: 16.666666% !important;
  }

  table.body td.small-offset-3,
  table.body th.small-offset-3 {
    margin-left: 25% !important;
    Margin-left: 25% !important;
  }

  table.body td.small-offset-4,
  table.body th.small-offset-4 {
    margin-left: 33.333333% !important;
    Margin-left: 33.333333% !important;
  }

  table.body td.small-offset-5,
  table.body th.small-offset-5 {
    margin-left: 41.666666% !important;
    Margin-left: 41.666666% !important;
  }

  table.body td.small-offset-6,
  table.body th.small-offset-6 {
    margin-left: 50% !important;
    Margin-left: 50% !important;
  }

  table.body td.small-offset-7,
  table.body th.small-offset-7 {
    margin-left: 58.333333% !important;
    Margin-left: 58.333333% !important;
  }

  table.body td.small-offset-8,
  table.body th.small-offset-8 {
    margin-left: 66.666666% !important;
    Margin-left: 66.666666% !important;
  }

  table.body td.small-offset-9,
  table.body th.small-offset-9 {
    margin-left: 75% !important;
    Margin-left: 75% !important;
  }

  table.body td.small-offset-10,
  table.body th.small-offset-10 {
    margin-left: 83.333333% !important;
    Margin-left: 83.333333% !important;
  }

  table.body td.small-offset-11,
  table.body th.small-offset-11 {
    margin-left: 91.666666% !important;
    Margin-left: 91.666666% !important;
  }

  table.body table.columns td.expander,
  table.body table.columns th.expander {
    display: none !important;
  }

  table.body .right-text-pad,
  table.body .text-pad-right {
    padding-left: 10px !important;
  }

  table.body .left-text-pad,
  table.body .text-pad-left {
    padding-right: 10px !important;
  }

  table.menu {
    width: 100% !important;
  }
  table.menu td,
  table.menu th {
    width: auto !important;
    display: inline-block !important;
  }
  table.menu.vertical td,
  table.menu.vertical th, table.menu.small-vertical td,
  table.menu.small-vertical th {
    display: block !important;
  }

  table.menu[align=""center""] {
    width: auto !important;
  }

    table.button.small-expand,
  table.button.small-expanded {
    width: 100% !important;
  }
table.button.small-expand table,
table.button.small-expanded table
{
    width: 100%;
}
table.button.small-expand table a,
  table.button.small-expanded table a {
    text-align: center !important;
    width: 100% !important;
    padding-left: 0 !important;
    padding-right: 0 !important;
  }
  table.button.small-expand center,
  table.button.small-expanded center
{
    min-width: 0;
}
}
.footer h5
{
    text-align: center;
    margin-bottom: 10px;
    font-size: 12px;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 1px;
}
.footer h5 a {
  color: #333 !important;
  font-weight: bold;
}

.footer p
{
    text-align: center;
    margin-bottom: 0px;
    font-size: 11px;
    color: #999;
}
.footer p a {
  color: #999;
  font-weight: bold;
}
.product-name {
    font-weight: bold;
}
.red-text {
    color: #e60000;
}
  .dl-horizontal dt {
    float: left;
    width: 160px;
    clear: left;
    text-align: right;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
  .dl-horizontal dd {
    margin-left: 180px;
  }
    .padding-left-5 {
        padding-left: 5px;
    }
	p {
	margin-top: 4px;
	margin-bottom: 0px;
	}
	hr {
	margin-bottom: 2px;
	}
";

        public static string RenderViewToString(string controllerName, string viewName, object viewData)
        {
            using (var writer = new StringWriter())
            {
                var routeData = new RouteData();
                routeData.Values.Add("controller", controllerName);
                var fakeRequest = new HttpRequest(null, "https://www.google.com", null);
                var fakeContext = new HttpContext(fakeRequest, new HttpResponse(null));
                var fakeControllerContext = new ControllerContext(new HttpContextWrapper(fakeContext), routeData, new FakeController());

                // setting IDisplayMode in order to override to force desktop render... orrrr... just remove the .mobile variant of the file?

                var razorViewEngine = new RazorViewEngine();
                var razorViewResult = razorViewEngine.FindView(fakeControllerContext, viewName, "", false);

                var viewContext = new ViewContext(fakeControllerContext, razorViewResult.View, new ViewDataDictionary(viewData), new TempDataDictionary(), writer);
                razorViewResult.View.Render(viewContext, writer);
                return writer.ToString();
            }
        }
    }

    public class loggedInMemberDetail
    {
        public long? visitorId { get; set; }
        public int? memberId { get; set; }
        public int? tradeAccountId { get; set; }
        public dgLoginStatus loginStatus { get; set; }
        public string tradeAccountNumber { get; set; }
        public string tradeDataSourceJson { get; set; }
        public string email { get; set; }
        public string fullName { get; set; }
    }
}