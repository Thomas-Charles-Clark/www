using Braintree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Careers;
using wwwDesignersGuild.Models.Checkout;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Search;
using wwwDesignersGuild.Models.Stockists;
using wwwDesignersGuild.Models.UnsubscribePage;
using wwwDesignersGuild.Models.Visitor;
using wwwDesignersGuild.Util;
using wwwDesignersGuild.ViewModels;

using wwwDesignersGuild.Models.Returns;

namespace wwwDesignersGuild.Controllers
{
    public class NewSectionController : Controller
    {
        public async Task<ActionResult> Login(BaseViewModel objBase)
        {
            if (Session["loginStatus"]?.ToString() == dgLoginStatus.currentLogin.ToString())
            {
                Server.TransferRequest("/l0");
            }

            var viewModel = await FixedPageViewModel_Login.CreateAsync(objBase);

            return PartialView("~/Views-New/LandingPages/FixedPages/_Login.cshtml", viewModel);
        }

        public async Task<ActionResult> MemberResetPassword(BaseViewModel objBase)
        {
            var token = Request.QueryString["token"]?.ToString();

            if (String.IsNullOrEmpty(token) || 
                token.Length != 56 || 
                !Guid.TryParse(token.Substring(0, 36), out Guid g))
            {
                return PartialView("~/Views-New/Shared/Redirects/_RedirectToHome.cshtml", objBase);
            }

            var visitorFactory = new VisitorFactory();

            var success = await visitorFactory.validateResetPasswordToken(token);

            var viewModel = await FixedPageViewModel_MemberPasswordReset.CreateAsync(
                new PasswordResetModel(success, token),
                objBase);

            return PartialView("~/Views-New/LandingPages/FixedPages/Member/_MemberPasswordReset.cshtml", viewModel);
        }

        public async Task<ActionResult> MemberAddresses(BaseViewModel objBase)
        {
            var objDefinition = await new VisitorFactory().getMemberAddresses(objBase.Visitor.visitorDbId,
                null,
                objBase.LanguageID,
                objBase.CountryID,
                objBase.Date);

            var viewModel = await FixedPageViewModel_MemberAddress.CreateAsync(
                objDefinition,
                objBase);

            return PartialView("~/Views-New/LandingPages/FixedPages/Member/_MemberAddressManagement.cshtml", viewModel);
        }

        public async Task<ActionResult> MemberLoginDetails(BaseViewModel objBase)
        {
            var objDefinition = await new VisitorFactory().memberDetails(objBase.Visitor.visitorDbId,
                objBase.Date,
                objBase.Visitor.memberId);

            var viewModel = await FixedPageViewModel_MemberLoginDetails.CreateAsync(
                objDefinition.MemberDetail,
                objBase);

            return PartialView("~/Views-New/LandingPages/FixedPages/Member/_MemberLoginDetails.cshtml", viewModel);
        }

        public async Task<ActionResult> MemberOrderHistory(BaseViewModel objBase)
        {
            var objDefinition = await new VisitorFactory().getOrderHistory(objBase.Visitor.visitorDbId,
                objBase.MasterDefinition.UrlBreakdown.countryDetails,
                null,
                365,
                null,
                null);

            var viewModel = await FixedPageViewModel_MemberOrderHistory.CreateAsync(
                objDefinition,
                objBase);

            return PartialView("~/Views-New/LandingPages/FixedPages/Member/_MemberOrderHistory.cshtml", viewModel);
        }

        public async Task<ActionResult> MemberMoodboards(BaseViewModel objBase)
        {
            var viewModel = await FixedPageViewModel_MemberMoodboard.CreateAsync(objBase);

            return PartialView("~/Views-New/LandingPages/FixedPages/Member/_MemberMoodboards.cshtml", viewModel);
        }

        public async Task<ActionResult> Basket(BaseViewModel objBase)
        {
            var objBasket = new basketRoutines();

#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            var objBasketDetails = await objBasket.loadBasketDetail(
                Convert.ToInt64(Session["visitorIdFromDatabase"]),
                (int?)Session["memberIdFromDatabase"],
                objBase.LanguageID,
                objBase.CountryID,
                objBase.Date,
                (int?)Session["memberTradeAccountID"],
                Session["memberTradeAccountID"] != null,
                Convert.ToBoolean(Session["memberDisplayTradePrice"] ?? "false")
            );

#if DEBUG
            sw.Stop();
            System.Diagnostics.Debug.WriteLine($"Basket generate called in {sw.Elapsed}");
#endif

            var viewModel = new FixedPageViewModel_Basket(objBasketDetails, objBase);

            return PartialView("~/Views-New/LandingPages/FixedPages/_Basket.cshtml", viewModel);
        }

        public async Task<ActionResult> Careers(BaseViewModel objBase)
        {
            var objCareers = new cmsCareersPage();

            var objCareersPageData = await objCareers.loadCareersPageFromCms(
                objBase.LanguageID,
                objBase.CountryID,
                objBase.Date);

            var viewModel = new FixedPageViewModel<cmsCareersPageDefinition>(objCareersPageData, objBase);

            return PartialView("~/Views-New/LandingPages/FixedPages/_Careers.cshtml", viewModel);
        }

        // Why was I having problems declaring this as async again?
        // The others above seem to work perfectly happily being async...
        public ActionResult Search(BaseViewModel objBase)
        {
            var searchTerm = Request.QueryString["search-term"];

            var objSearchGenerate = new searchGenerate();

#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            var objSearchResult = objSearchGenerate.buildSearchResultsDb
                (objBase.MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
                 objBase.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                 searchTerm,
                 objBase.MasterDefinition.ProductTypeDropDown.ProductTypes.SingleOrDefault(p => p.FormValue == objBase.ProductType)?.ProductTypeID,
                 objBase.MasterDefinition.UrlBreakdown.ContentDate).Result;

#if DEBUG
            sw.Stop();
            System.Diagnostics.Debug.WriteLine($"Search for \"{searchTerm}\" executed in {sw.Elapsed}");
#endif

            var viewModel = new FixedPageViewModel_Search(objSearchResult, objBase);
           
            return PartialView("~/Views-New/LandingPages/FixedPages/_Search.cshtml", viewModel);
        }

        public async Task<ActionResult> CheckoutLogin(BaseViewModel objBase)
        {
            var requestWrapper = new HttpRequestWrapper(System.Web.HttpContext.Current.Request);

            var useDate = Framework.GetUseDate(requestWrapper);

            // call basket_validate
            // if valid, call the beneath to construct a checkout if there isn't one in session already...

            var objCheckout = new checkoutDb();

            if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == true)
            {
                return PartialView($"~/Views-New/Shared/Error/_returnToBasket.cshtml", objBase.MasterDefinition);
            }

            var basketPreflightResult = await objCheckout.validateBasket(
                objBase.Visitor.visitorDbId,
                objBase.Visitor.memberId,
                objBase.Visitor.memberTradeAccountID,
                objBase.LanguageID,
                objBase.CountryID,
                objBase.Date,
                Session["memberDisplayTradePrice"] != null && Convert.ToBoolean(Session["memberDisplayTradePrice"]));

            var subTotal = 0.01M;

            if (basketPreflightResult.JSON == "{}")
            {
                var objSnapshot = await objCheckout.createCheckoutBasket(
                    objBase.Visitor.visitorDbId,
                    objBase.LanguageID,
                    objBase.CountryID,
                    DateTime.Now,
                    (string)Session["CF-IPCountry"],
                    objBase.Visitor.memberTradeAccountID);

                if (objSnapshot == null)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
                }

                Session["checkoutId"] = objSnapshot.CheckoutID;

                objSnapshot.CheckoutExpiryDT = objSnapshot.CheckoutExpiryDT.ToUniversalTime();

                Session["checkoutExpiry"] = objSnapshot.CheckoutExpiryDT;

                subTotal = objSnapshot.SubTotal ?? 0.01M;
            } 
            else
            {
                return PartialView("~/Views-New/LandingPages/FixedPages/Checkout/_ReturnToBasket.cshtml", objBase);
            }

            if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == false)
            {
                return PartialView("~/Views-New/LandingPages/FixedPages/Checkout/_ReturnToBasket.cshtml", objBase);
            }
            
            if (dgFramework.isCheckoutSessionValid(Session["checkoutExpiry"]) == false)
            {
                return PartialView("~/Views-New/LandingPages/FixedPages/Checkout/_Expired.cshtml", objBase);
            }

            long checkoutId = (long)Session["checkoutId"];

            if (await objCheckout.preflightCheckWelcome(checkoutId, objBase.CountryID) == false)
            {
                return PartialView("~/Views-New/LandingPages/FixedPages/Checkout/_ReturnToBasket.cshtml", objBase);
            }
            else if (await objCheckout.checkoutTypeIsSet(checkoutId))
            {
                // If a checkout type is set on this checkout, we're in a problem state. Checkout type should not yet be set.
                return PartialView("~/Views-New/LandingPages/FixedPages/Checkout/_ReturnToBasket.cshtml", objBase);
            }

            var viewModel = await FixedPageViewModel_CheckoutLogin.CreateAsync(objBase, checkoutId, subTotal);

            return PartialView("~/Views-New/LandingPages/FixedPages/Checkout/_Login.cshtml", viewModel);
        }

        public ActionResult CheckoutExpired(BaseViewModel objBase)
        {
            return PartialView("~/Views-New/LandingPages/FixedPages/Checkout/_Expired.cshtml", objBase);
        } 

        public async Task<ActionResult> ChangeCountry(BaseViewModel objBase)
        {
            var objCountry = new cmsCountry();

            var objCountryData = await objCountry.countryLanguageSelect(objBase.Date);

            var viewModel = new FixedPageViewModel<cmsCountryDefinition>(objCountryData, objBase);

            return PartialView("~/Views-New/LandingPages/FixedPages/_ChangeCountry.cshtml", viewModel);
        }

        public async Task<ActionResult> Stockists(BaseViewModel objBase)
        {
            var objStockists = new Stockist();

            var objStockistsData = await objStockists.getStockistCountries(objBase.Date);

            var viewModel = await FixedPageViewModel_Stockists.CreateAsync(objStockistsData, objBase);

            return PartialView("~/Views-New/LandingPages/FixedPages/_Stockists.cshtml", viewModel);
        }

        public async Task<ActionResult> Resubscribe(BaseViewModel objBase)
        {
            var viewModel = await FixedPageViewModel_MemberResubscribe.CreateAsync(Request.QueryString["resub"], objBase);

            return PartialView("~/Views-New/LandingPages/FixedPages/Member/_MemberResubscribe.cshtml", viewModel);
        }

        public async Task<ActionResult> Unsubscribe(BaseViewModel objBase)
        {
            //  Get unsubscribe page
            var objUnsubscribe = new cmsUnsubscribePage();

            var objReasonData = await objUnsubscribe.loadUnsubscribePageFromCms(
                objBase.LanguageID,
                objBase.CountryID,
                objBase.Date,
                true,
                false);

            var viewModel = await FixedPageViewModel_MemberUnsubscribe.CreateAsync(objReasonData,
                Request.QueryString["unsub"],
                objBase);

            return PartialView("~/Views-New/LandingPages/FixedPages/Member/_MemberUnsubscribe.cshtml", viewModel);
        }

        public async Task<ActionResult> Checkout(BaseViewModel objBase)
        {
            long? checkoutId = null;

            if (Int64.TryParse(Session["checkoutId"]?.ToString(), out long l))
            {
                checkoutId = l;
            }

            if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == false ||
                checkoutId == null)
            {
                return PartialView("~/Views-New/LandingPages/FixedPages/Checkout/_ReturnToBasket.cshtml", objBase);
            }
            else if (dgFramework.isCheckoutSessionValid(Session["checkoutExpiry"]) == false)
            {
                return PartialView("~/Views-New/LandingPages/FixedPages/Checkout/_Expired.cshtml", objBase);
            }
            else
            {
                var objCheckout = new checkoutDb();

                // Run the all-important address preflight check. This should be renamed.
                var preflightCheckAddressResults = await objCheckout.preflightCheckAddress(Convert.ToInt64(Session["checkoutId"].ToString()), objBase.MasterDefinition.UrlBreakdown.countryDetails.CountryID);

                if (!preflightCheckAddressResults.Passed)
                {
                    return PartialView("~/Views-New/LandingPages/FixedPages/Checkout/_ReturnToBasket.cshtml", objBase);
                }

                var viewModel = await FixedPageViewModel_Checkout.CreateAsync(objBase, checkoutId.Value);

                if (viewModel.RedirectToBasket)
                {
                    return PartialView("~/Views-New/LandingPages/FixedPages/Checkout/_ReturnToBasket.cshtml", objBase);
                }

                return PartialView("~/Views-New/LandingPages/FixedPages/Checkout/_CheckoutSinglePage.cshtml", viewModel);
            }
        }

        public async Task<ActionResult> ReturnsPaperwork(BaseViewModel objBase, [FromUri]EncryptedOrderIdentifier orderIdentifier) 
        {
            var viewModel = await FixedPageViewModel_ReturnsPaperwork.CreateAsync(orderIdentifier.OrderID, objBase);

            return PartialView("~/Views-New/LandingPages/FixedPages/Member/_ReturnsPaperwork.cshtml", viewModel);
        }
        
        public async Task<ActionResult> CheckoutProblem(BaseViewModel objBase)
        {
            return PartialView("~/Views-New/LandingPages/FixedPages/Checkout/_ReturnToBasket.cshtml", objBase);
        }
    }
}