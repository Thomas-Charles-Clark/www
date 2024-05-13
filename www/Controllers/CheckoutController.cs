using Braintree;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
using wwwDesignersGuild.Models.Checkout;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Visitor;
using wwwDesignersGuild.Util;
using AmazonPay;
using AmazonPay.CommonRequests;
using AmazonPay.Responses;
using AmazonPay.StandardPaymentRequests;
using wwwDesignersGuild.Models.BasketClasses;
using System.Web.Configuration;
using System.Web.WebPages;
using wwwDesignersGuild.Models.BraintreeErrorResponse;
using wwwDesignersGuild.Models.UPS;
using Newtonsoft.Json;

namespace wwwDesignersGuild.Controllers
{
    public class CheckoutController : ApiController
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        #region Return Data classes

        public class deliveryEstimateReturn
        {
            public string displayLine { get; set; }

            public string minDeliveryDate { get; set; }

            public string maxDeliveryDate { get; set; }
        }

        public class salesTaxAddress
        {
            public String town { get; set; }
            public String county { get; set; }
            public String state { get; set; }
            public String addressLine { get; set; }
        }

        public class salesTaxReturn
        {
            public Boolean isSalesTaxKnown { get; set; }
            public List<salesTaxAddress> salesTaxAddresses { get; set; }
        }

        #endregion Return Data classes

        #region Controller routines

        [System.Web.Http.HttpPost]
        public IHttpActionResult saveCutLengthBreakdown([FromBody]PassCutLengthBreakdown objSave)
        {
            var session = HttpContext.Current.Session;
            if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
            }
            else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired));
            }
            else
            {
                var checkoutDb = new Models.Database.checkoutDb();

                checkoutDb.saveCutLengthBreakdown(objSave);
            }

            return Ok(new { });
        }


        [System.Web.Http.HttpPost]
        public IHttpActionResult saveLineMemo([FromBody]PassLineMemo objSave)
        {
            var session = HttpContext.Current.Session;
            if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
            }
            else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired));
            }
            else
            {
                var checkoutDb = new Models.Database.checkoutDb();

                checkoutDb.saveLineMemo(objSave);
            }

            return Ok(new { });
        }

        [System.Web.Http.HttpPost]
        public IHttpActionResult saveLineTreatments([FromBody]PassLineTreatment objSave)
        {
            var session = HttpContext.Current.Session;
            if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
            }
            else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired));
            }
            else
            {
                var checkoutDb = new Models.Database.checkoutDb();

                checkoutDb.saveLineTreatments(objSave);
            }

            return Ok(new { });
        }

        [System.Web.Http.HttpPost]
        public async Task<bool> isValidDeliveryCountry(webapiCheckoutCountryCheck objCheckoutCountry)
        {
            var session = HttpContext.Current.Session;
            if (session["visitorIdFromDatabase"] == null)
            {
                Logger.Error("isValidDeliveryCountry - no visitor ID from session!");
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            checkoutDb objCheckout = new checkoutDb();

            cmsCountry objCountry = new cmsCountry();

            cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(DateTime.Now);

            if (dgFramework.doesCountryExistFromText(objCheckoutCountry.basketCountry, objCountryData)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
            }

            countryItem objCountryItem = dgFramework.getCountryFromText(objCheckoutCountry.basketCountry, objCountryData);
            // check that provided address is accepted by the system
            cmsCountryPriceListDefinition objDeliverableCountries = await objCountry.countrySelectList(objCountryItem.CountryID, objCountryItem.LanguageID);

            var result = objDeliverableCountries.countries.Any(c => c.Code == objCheckoutCountry.deliveryCountry);

            if (result == false)
            {
                // if you've selected an invalid delivery country, we want to:
                // delete the current CheckoutAddress record
                // this is because we want the checkoutDispatch partial to render "Please select a delivery address first".
                // only way to have it do that is to not have any records in the checkoutaddress table for this checkout.
                await objCheckout.deleteCheckoutAddresses(Convert.ToInt64(session["checkoutId"]));
            }

            return result;
        }

        [System.Web.Http.HttpPost]
        public async Task<bool> removeCheckoutAddress()
        {
            var session = HttpContext.Current.Session;

            var objCheckout = new checkoutDb();

            if (session["visitorIdFromDatabase"] == null)
            {
                Logger.Error("removeCheckoutAddress - no visitor ID from session!");
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            await objCheckout.deleteCheckoutAddresses(Convert.ToInt64(session["checkoutId"]));

            return true;
        }

        [System.Web.Http.HttpPost]
        public async Task<bool> setPayPalNonce(webapiCheckoutPayPalNonce objCheckoutNonce)
        {
            var session = HttpContext.Current.Session;
            if (session["visitorIdFromDatabase"] == null)
            {
                Logger.Error("setPayPalNonce - no visitor ID from session!");
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            Int64 checkoutId = Convert.ToInt64(session["checkoutId"].ToString());
            checkoutDb objCheckout = new checkoutDb();

            if (await objCheckout.GetCheckoutType(checkoutId) != dgCheckoutType.Paypal)
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);

            cmsCountry objCountry = new cmsCountry();
            _ = await objCountry.countryLanguageSelect(DateTime.Now);

            session["paypal_nonce_" + checkoutId.ToString()] = objCheckoutNonce.nonce;

            return true;
        }

        [System.Web.Http.HttpPost]
        public async Task<object> setCheckoutType(webapiCheckoutSetCheckoutType objCheckoutType)
        {
            int? memberId = null;

            var session = HttpContext.Current.Session;

            // clear the workaround.
            session["guestCheckoutMemberId"] = null;

            //  Has any future date been passed
            var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

            if (session["visitorIdFromDatabase"] == null)
            {
                Logger.Error("setCheckoutType - no visitor ID from session!");
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }
            else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
            }
            else
            {
                Int64 checkoutId = Convert.ToInt64(session["checkoutId"].ToString());
                Int64 visitorId = Convert.ToInt64(session["visitorIdFromDatabase"]);

                bool isValid = true;
                bool guestCheckout = true;

                switch(objCheckoutType.checkoutType)
                {
                    case dgCheckoutType.Member:
                        guestCheckout = false;
                        isValid = session["memberIdFromDatabase"] != null;
                        break;
                    case dgCheckoutType.Trade:
                        guestCheckout = false;
                        isValid = session["memberTradeAccountID"] != null;
                        break;
                    case dgCheckoutType.Paypal:
                        isValid = !(string.IsNullOrEmpty(objCheckoutType.token) || string.IsNullOrEmpty(objCheckoutType.guestEmail) || objCheckoutType.initialShippingAddress == null);
                        break;
                    case dgCheckoutType.Guest:
                        isValid = !String.IsNullOrEmpty(objCheckoutType.guestEmail);
                        break;
                    case dgCheckoutType.Amazon:
                        isValid = !(String.IsNullOrEmpty(objCheckoutType.guestEmail) || String.IsNullOrEmpty(objCheckoutType.token));
                        break;
                    case dgCheckoutType.Apple:
                        isValid = true;
                        break;
                }

                if (!isValid)
                    return dgFramework.raiseErrorToHandle(webapiDataError.InvalidLogin);

                checkoutDb objCheckout = new checkoutDb();

                cmsCountry objCountry = new cmsCountry();
                cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(useDate);

                if (dgFramework.doesCountryExistFromText(objCheckoutType.basketCountry, objCountryData)) { }
                else
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
                }

                countryItem objCountryItem = dgFramework.getCountryFromText(objCheckoutType.basketCountry, objCountryData);

                string email = null;
                switch (objCheckoutType.checkoutType)
                {
                    case dgCheckoutType.Guest:
                        email = objCheckoutType.guestEmail;
                        break;
                    case dgCheckoutType.Amazon:
                        email = objCheckoutType.guestEmail;
                        session["amazon_token"] = objCheckoutType.token;
                        break;
                    case dgCheckoutType.Paypal:
                        email = objCheckoutType.guestEmail;
                        session["paypal_nonce_" + checkoutId.ToString()] = objCheckoutType.token;

                        var allCountries = await objCountry.countrySelectList(null, (byte)1);

                        // check that provided address is accepted by the system
                        cmsCountryPriceListDefinition objDeliverableCountries = await objCountry.countrySelectList(objCountryItem.CountryID, objCountryItem.LanguageID);

                        var shippingCountry = allCountries.countries.Where(p => p.Code == objCheckoutType.initialShippingAddress.addressCountryCode).SingleOrDefault();

                        if (shippingCountry == null)
                        {
                            return dgFramework.raiseErrorToHandle(webapiDataError.invalidDeliveryAddress, JsonConvert.SerializeObject(objDeliverableCountries.countries));
                        }

                        if (!objDeliverableCountries.countries.Any(c => c.CountryID == shippingCountry.CountryID))
                        {
                            return dgFramework.raiseErrorToHandle(webapiDataError.invalidDeliveryAddress, JsonConvert.SerializeObject(objDeliverableCountries.countries));
                        }

                        var address = new passCheckoutSaveAddressData()
                        {
                            CheckoutBasketID = null,
                            DeliveryOptionID = 1,
                            MemberAddressID = null,
                            FullName = objCheckoutType.initialShippingAddress.addressName,
                            CompanyName = objCheckoutType.initialShippingAddress.companyName,
                            AddressLine1 = objCheckoutType.initialShippingAddress.addressLineOne,
                            AddressLine2 = objCheckoutType.initialShippingAddress.addressLineTwo,
                            Town = objCheckoutType.initialShippingAddress.addressTown,
                            County = objCheckoutType.initialShippingAddress.addressCounty,
                            Postcode = objCheckoutType.initialShippingAddress.addressPostCode,
                            CountryID = shippingCountry.CountryID,
                            PhoneNumber = objCheckoutType.initialShippingAddress.addressPhone,
                            UPSAPID = null,
                            ZipTax = null
                        };

                        await objCheckout.checkoutAddressSave(checkoutId, new List<passCheckoutSaveAddressData> { address }, null, null, false);
                        break;
                    case dgCheckoutType.Apple:
                        // We don't yet have the actual email address from Apple.
                        email = $"{Guid.NewGuid()}@apple-pay-temporary-email.com";
                        break;
                }

                /* 
                 * If we are a guest checkout we still need to associate the checkout with a member, check if the supplied email address 
                 * matches a member if so associate it (without logging in), if not create a new member record and do the same
                */

                if (objCheckoutType.checkoutType == dgCheckoutType.Member || objCheckoutType.checkoutType == dgCheckoutType.Trade)
                {
                    memberId = (int?)session["memberIdFromDatabase"];
                }

                if (guestCheckout)
                {
                    if (dgFramework.emailChecker(email)) { }
                    else
                    {
                        return dgFramework.raiseErrorToHandle(webapiDataError.EmailInvalidFormat);
                    }

                    VisitorFactory visitorFactory = new VisitorFactory();
                    var memberExists = await visitorFactory.callGuestCheckoutEmailExistsSelect(email.Trim());
                    if (memberExists != null)
                    {
                        memberId = memberExists;                        
                    }
                    else
                    {
                        //  Save the new member
                        memberId = await visitorFactory.saveGuestRegistration(objCountryItem.LanguageID, objCountryItem.CountryID, string.Empty, email);
                    }

                    if (memberId != null)
                    {
                        session["guestCheckoutMemberId"] = memberId;
                    }
                    else
                    {
                        dgFramework.raiseErrorNotHandled(webapiUnhandledError.MemberNotInserted);
                    }
                }

                // Email IS null at this point for Member checkout, it will need to be pulled.
                if ((objCheckoutType.checkoutType == dgCheckoutType.Member || objCheckoutType.checkoutType == dgCheckoutType.Trade) && email == null)
                {
                    var memberStore = await new VisitorFactory().memberDetails(visitorId, useDate, memberId);
                    email = memberStore.MemberDetail.Email;
                }

                // we want to be logging the memberId they were browsing the site with, if any
                // as well as their loginStatus.

                int? siteMemberId = null;

                if (session["memberIdFromDatabase"] != null)
                {
                    siteMemberId = Convert.ToInt32(session["memberIdFromDatabase"]);
                }

                int? tradeAccountId = null;

                if (session["memberTradeChecksPassed"] != null)
                {
                    if (Convert.ToBoolean(session["memberTradeChecksPassed"]) == true)
                    {
                        tradeAccountId = Convert.ToInt32(session["memberTradeAccountID"]);
                    }
                }

                session["checkoutEmail"] = email;

                var result = await objCheckout.checkoutSaveType(checkoutId,
                    useDate,
                    objCheckoutType.checkoutType,
                    memberId.Value, 
                    email, 
                    siteMemberId, 
                    session["loginStatus"]?.ToString(),
                    tradeAccountId);

                if (result == true)
                {
                    return new 
                    {
                        redirect = dgFramework.buildUrl(dgContentType.landingPage, (int)dgFixedPage.checkoutSinglePage, objCountryItem) 
                    };
                }
                else 
                {
                    return new 
                    { 
                        redirect = dgFramework.buildUrl(dgContentType.landingPage, (int)dgFixedPage.checkoutProblem, objCountryItem)
                    };
                }
            }

            return "";
        }

        [System.Web.Http.HttpPost]
        public async Task<object> getAmazonDeliveryAddress(webapiCheckoutGetAmazonShippingAddress objCheckoutAmazon)
        {
            var session = HttpContext.Current.Session;

            if (session["visitorIdFromDatabase"] == null)
            {
                Logger.Error("getAmazonDeliveryAddress - no visitor ID from session!");
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }
            else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
            }
            else if (session["amazon_token"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            Int64 checkoutId = (Int64)session["checkoutId"];

            string token = (string)session["amazon_token"];

            checkoutDb objCheckout = new checkoutDb();
            var merchantDetailSelect = await objCheckout.getMerchantDetails(checkoutId);
            var merchantDetails = merchantDetailSelect.checkoutParameters;

            AmazonPay.CommonRequests.Configuration config = new AmazonPay.CommonRequests.Configuration();
            config.WithAccessKey(merchantDetails.AmazonAccessKey)
                .WithMerchantId(merchantDetails.AmazonMerchantId)
                .WithSecretKey(merchantDetails.AmazonSecret)
                .WithRegion(merchantDetails.AmazonRegion);

            if (merchantDetails.AmazonLiveAPI)
            {
                // do nothing
            }
            else
            {
                config.WithSandbox(true);
            }

            GetOrderReferenceDetailsRequest requestParameters = new GetOrderReferenceDetailsRequest();
            requestParameters.WithAmazonOrderReferenceId(objCheckoutAmazon.orderReferenceId);
            requestParameters.WithAccessToken(token);

            Client client = new Client(config);

            try
            {
                OrderReferenceDetailsResponse response = client.GetOrderReferenceDetails(requestParameters);

                // check that the selected address is valid for our country
                cmsCountry objCountry = new cmsCountry();
                cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(DateTime.Now);
                countryItem objBasketCountryItem = dgFramework.getCountryFromText(objCheckoutAmazon.basketCountry, objCountryData);

                cmsCountryPriceListDefinition objDeliverableCountries = await objCountry.countrySelectList(objBasketCountryItem.CountryID, objBasketCountryItem.LanguageID);
                if (!objDeliverableCountries.countries.Any(c => c.Code == response.GetCountryCode()))
                {
                    return dgFramework.raiseErrorToHandle(webapiDataError.invalidDeliveryAddress, $"{response.GetAddressLine1()}, {response.GetPostalCode()}, {response.GetCountryCode()}");
                }

                if (response.GetSuccess())
                {
                    var objReturn = new MemberAddress()
                    {
                        AddressLine1 = response.GetAddressLine1(),
                        AddressLine2 = response.GetAddressLine2(),
                        FullName = response.GetBuyerShippingName(),
                        CountryCode = response.GetCountryCode(),
                        County = response.GetCounty(),
                        IsDefault = true,
                        PhoneNumber = response.GetPhone(),
                        PostCode = response.GetPostalCode(),
                        Town = response.GetCity()
                    };

                    return objReturn;
                }
                else
                    return null;
            }
            catch (Exception e)
            {
                Logger.Error(e, $"getAmazonDeliveryAddress errored", checkoutId);

                return null;
            }
        }

        public async Task<object> updateAmazonPrice(webapiCheckoutUpdateAmazonPrice objCheckoutAmazon)
        {
            var session = HttpContext.Current.Session;

            if (session["visitorIdFromDatabase"] == null)
            {
                Logger.Error("updateAmazonPrice - no visitor ID from session!");
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }
            else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
            }
            else if (session["amazon_token"] == null || objCheckoutAmazon.masterDefinition == null || string.IsNullOrEmpty(objCheckoutAmazon.orderReferenceId))
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            _ = (string)session["amazon_token"];
            Int64 checkoutId = Convert.ToInt64(session["checkoutId"].ToString());

            checkoutDb objCheckout = new checkoutDb();
            var merchantDetailSelect = await objCheckout.getMerchantDetails(checkoutId);
            var merchantDetails = merchantDetailSelect.checkoutParameters;

            AmazonPay.CommonRequests.Configuration config = new AmazonPay.CommonRequests.Configuration();
            config.WithAccessKey(merchantDetails.AmazonAccessKey)
                .WithMerchantId(merchantDetails.AmazonMerchantId)
                .WithSecretKey(merchantDetails.AmazonSecret)
                .WithRegion(merchantDetails.AmazonRegion);

            if (merchantDetails.AmazonLiveAPI)
            {
                // do nothing
            }
            else
            {
                config.WithSandbox(true);
            }

            countryItem country = objCheckoutAmazon.masterDefinition.UrlBreakdown.countryDetails;
            CheckoutPayment payment = await objCheckout.checkoutPayGenerate(checkoutId, country.CountryID, country.LanguageID, "CheckoutController.updateAmazonPrice");

            Enum currencyCode = (Regions.currencyCode)Enum.Parse(typeof(Regions.currencyCode), payment.checkoutDetails[0].Currency);

            SetOrderReferenceDetailsRequest requestParameters = new SetOrderReferenceDetailsRequest();
            requestParameters.WithAmazonOrderReferenceId(objCheckoutAmazon.orderReferenceId)
                .WithAmount(payment.paymentTotals[0].TotalBalance)
                .WithCurrencyCode(currencyCode)
                .WithSellerOrderId(payment.checkoutDetails[0].GUID.ToString())
                .WithStoreName("Designers Guild");

            Client client = new Client(config);

            // can I not just call this first thing in the amazon submit method...?
            OrderReferenceDetailsResponse response = client.SetOrderReferenceDetails(requestParameters);

            return response.GetSuccess();
        }

        [System.Web.Http.HttpPost]
        public async Task<object> logExitCheckoutProcess()
        {
            var session = HttpContext.Current.Session;
            if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }
            else
            {
                Int64 checkoutId = Convert.ToInt64(session["checkoutId"].ToString());

                session["checkoutId"] = null;
                session["checkoutExpiry"] = null;

                checkoutDb objCheckout = new checkoutDb();
                _ = await objCheckout.logCheckoutAction(checkoutId, (int)dgCheckoutActionId.logCheckoutExit, "");
            }

            return "";
        }

        [System.Web.Http.HttpPost]
        public async Task<bool> recreateCheckout(webapiRecreateCheckout objRecreateCheckout)
        {
            try
            {
                #region Date Override Check
                DateTime useDate = DateTime.Now;
                useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
                if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
                else
                {
                    var queryStrings = Request.GetQueryNameValuePairs();
                    var match = queryStrings.FirstOrDefault(kv => string.Compare(kv.Key, "dateOverride", true) == 0);
                    if (string.IsNullOrEmpty(match.Value)) { }
                    else
                    {
                        if (match.Value.Length == "ddMMyyyy".Length)
                        {
                            bool dateConvertSuccess = DateTime.TryParseExact(match.Value,
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
                #endregion

                var currentDateTime = useDate;
                var session = HttpContext.Current.Session;

                if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
                }

                Int64 visitorDbId;
                if (session["visitorIdFromDatabase"] == null)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
                }

                int? tradeAccountID = null;
                if (session["memberTradeAccountID"] == null)
                {
                }
                else
                {
                    tradeAccountID = (int?)session["memberTradeAccountID"];
                }

                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"]);

                //  Get the basket country details
                cmsCountry objCountry = new cmsCountry();
                cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(currentDateTime);

                if (dgFramework.doesCountryExistFromCode(objRecreateCheckout.country, objCountryData)) { }
                else
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
                }

                countryItem objCountryItem = dgFramework.getCountryFromCode(objRecreateCheckout.country, objCountryData);

                //  Is this checkout linked to a linkshare request
                DateTime? linkshareDate = null;
                String linkshareId = null;
                String cookieName = null;

                if (objCountryItem.CountryID == 81)
                {
                    cookieName = "_linkshare_uk";
                }

                if (objCountryItem.CountryID == 238)
                {
                    cookieName = "_linkshare_us";
                }

                if (cookieName == null) { }
                else
                {
                    HttpCookie cookie = HttpContext.Current.Request.Cookies[cookieName];
                    if (cookie != null)
                    {
                        linkshareId = cookie.Value;
                        linkshareDate = currentDateTime;
                    }
                }

                checkoutDb objCheckout = new checkoutDb();

                //  Create the snapshot basket
                checkoutCreateSnapshotBasket objSnapshot = await objCheckout.createCheckoutBasket(
                    visitorDbId,
                    objCountryItem.LanguageID,
                    objCountryItem.CountryID,
                    DateTime.Now,
                    (string)HttpContext.Current.Session["CF-IPCountry"],
                    tradeAccountID);
                if (objSnapshot == null)
                {
                    return false;
                }
                else
                {
                    //  If okay, generate basket, store in Session
                    session["checkoutId"] = objSnapshot.CheckoutID;

                    objSnapshot.CheckoutExpiryDT = objSnapshot.CheckoutExpiryDT.ToUniversalTime();
                    session["checkoutExpiry"] = objSnapshot.CheckoutExpiryDT;//.ToString("dd MMMM yyyy HH:mm");
                    session["checkoutDisplayGiftTab"] = objSnapshot.CheckoutDisplayGiftScreen;
                    session["checkoutDisplayGiftVoucherTab"] = objSnapshot.CheckoutDisplayGiftVoucherScreen;

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> selectCheckoutAddress(byte languageId)
        {
            var session = HttpContext.Current.Session;

            if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
            }
            else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired));
            }

            var checkoutId = Convert.ToInt64(session["checkoutId"].ToString());

            return Ok(new checkoutDb().checkoutAddressSelect(checkoutId, languageId));
        }

        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> saveCheckoutAddress(webapiCheckoutSaveAddress objDeliveryAddress)
        {
            var session = HttpContext.Current.Session;

            var objCheckout = new checkoutDb();

            var objCountry = new cmsCountry();

            var sb = new StringBuilder();
            
            long visitorDbId = -1;

            if (session["visitorIdFromDatabase"] == null)
            {
                Logger.Error("saveCheckoutAddress - no visitor ID from session!");
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter, "Session.visitorIdFromDatabase was NULL!");
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            if (objDeliveryAddress == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters, "objDeliveryAddress was NULL!");
            }

            if (objDeliveryAddress.masterDefinition == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters, "objDeliveryAddress.masterDefinition was NULL!");
            }

            if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket, "Session.checkoutId was NULL!");
            }
            else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired));
            }
            
            var useDate = DateTime.Now;

            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);

            if (Convert.ToBoolean(WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                //  Should the "use date" be overriden?
                if (HttpContext.Current.Request.QueryString["dateOverride"] == null)
                {
                    useDate = DateTime.Now;
                }
                else
                {
                    String overrideDate = HttpContext.Current.Request.QueryString["dateOverride"].ToString();
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

            var checkoutId = Convert.ToInt64(session["checkoutId"].ToString());

            var checkoutType = await objCheckout.GetCheckoutType(checkoutId);
            
            var objCountryData = await objCountry.countryLanguageSelect(DateTime.Now);

            if (dgFramework.doesCountryExistFromText(objDeliveryAddress.BasketCountry, objCountryData)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry, "objDeliveryAddress.BasketCountry was invalid!");
            }

            var objCountryItem = dgFramework.getCountryFromText(objDeliveryAddress.BasketCountry, objCountryData);

            if (objCountryItem == null)
            {
                objCountryItem = dgFramework.getCountryFromCode(objDeliveryAddress.BasketCountry, objCountryData);
            }

            if (objCountryItem == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry, "countryItem could not be identified from objDeliveryAddress.BasketCountry!");
            }

            var objDeliverableCountries = await objCountry.countrySelectList(objCountryItem.CountryID, objCountryItem.LanguageID);
            
            var deliveryCountry = objDeliverableCountries.countries.Where(c => c.Code == objDeliveryAddress.CountryCode?.ToUpperInvariant()).SingleOrDefault();

            if (deliveryCountry == null)
            {
                return Ok(new { state = "FAIL", message = "invalidCountry" });
            }

            var raiseInvalidParameters = false;

            var amazonOrPayPalOrUPS = (checkoutType == dgCheckoutType.Amazon) || (checkoutType == dgCheckoutType.Paypal) || (objDeliveryAddress.DeliveryOptionID == 3);
        
            if ((String.IsNullOrEmpty(objDeliveryAddress.FullName) && !amazonOrPayPalOrUPS) ||
                String.IsNullOrEmpty(objDeliveryAddress.AddressLine1) ||
                String.IsNullOrEmpty(objDeliveryAddress.Town) ||
                //(String.IsNullOrEmpty(objDeliveryAddress.County) && !amazonOrPayPalOrUPS) || /* Because Amazon doesn't mandate a County... */
                (String.IsNullOrEmpty(objDeliveryAddress.Postcode) && deliveryCountry.MandatePostcode)) /* Because PayPal doesn't guarantee me a phone number  */
            {
                raiseInvalidParameters = true;
            }


            if (
                ((objDeliveryAddress.FullName?.Length < 1 || objDeliveryAddress.FullName?.Length > 50) && !amazonOrPayPalOrUPS) ||
                (objDeliveryAddress.AddressLine1.Length < 1 || objDeliveryAddress.AddressLine1.Length > 60) ||
                (objDeliveryAddress.Town.Length < 1 || objDeliveryAddress.Town.Length > 50) ||
                //((objDeliveryAddress.County?.Length < 1 || objDeliveryAddress.County?.Length > 40) && !amazonOrPayPalOrUPS) || /* see above */
                ((objDeliveryAddress.Postcode?.Length < 1 || objDeliveryAddress.Postcode?.Length > 20) && deliveryCountry.MandatePostcode)) /* see above */
            {
                raiseInvalidParameters = true;
            }

            if (raiseInvalidParameters)
            {
                PropertyPrinter.PrintProperties(objDeliveryAddress, 5, sb);

                Logger.Error($"saveCheckoutAddress errored {checkoutId} {sb}");

                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters, $"saveCheckoutAddress errored {checkoutId} {sb}");
            }
            
            string zipTax = null;

            if (deliveryCountry.CountryID == 237 || deliveryCountry.CountryID == 238)
            {
                var isSalesTaxKnown = false;

                var salesTaxAddressesToSelectFrom = new List<salesTaxAddress>();

                var postCode = objDeliveryAddress.Postcode;

                if (postCode.Length > 5)
                {
                    postCode = postCode.Substring(0, 5);
                }

                var zipTaxApiUrl = String.Format("{0}?key={1}&postalcode={2}&format=XML",
                                                        WebConfigurationManager.AppSettings["zipTaxUrl"],
                                                        WebConfigurationManager.AppSettings["zipTaxKey"],
                                                        Uri.EscapeDataString(postCode));

                if (objDeliveryAddress.County != null && objDeliveryAddress.Town != null)
                {
                    // don't know why ZipTaxCounty isn't used... probably because Town is sufficiently specific
                    zipTaxApiUrl = String.Format("{0}?key={1}&postalcode={2}&state={3}&city={4}&format=XML",
                                                    WebConfigurationManager.AppSettings["zipTaxUrl"],
                                                    WebConfigurationManager.AppSettings["zipTaxKey"],
                                                    Uri.EscapeDataString(postCode),
                                                    Uri.EscapeDataString(objDeliveryAddress.County), // State != County
                                                    Uri.EscapeDataString(objDeliveryAddress.Town));

                    if (objDeliveryAddress.USCounty != null)
                    {
                        // don't know why ZipTaxCounty isn't used... probably because Town is sufficiently specific
                        zipTaxApiUrl = String.Format("{0}?key={1}&postalcode={2}&state={3}&city={4}&county={5}&format=XML",
                                                        WebConfigurationManager.AppSettings["zipTaxUrl"],
                                                        WebConfigurationManager.AppSettings["zipTaxKey"],
                                                        Uri.EscapeDataString(postCode),
                                                        Uri.EscapeDataString(objDeliveryAddress.County), // State != County
                                                        Uri.EscapeDataString(objDeliveryAddress.Town),
                                                        Uri.EscapeDataString(objDeliveryAddress.USCounty)); // USCounty == County
                    }
                }

                try
                {
                    XDocument xmlResponse = XDocument.Load(zipTaxApiUrl);

                    if (xmlResponse.Descendants("ziptax").Count() == 0)
                    {
                        throw new Exception("ZipTax expected response not returned");
                    }
                    else
                    {
                        var code = xmlResponse.Element("ziptax").Element("code").Value;
                        var responses = xmlResponse.Element("ziptax").Descendants("response");

                        if (code == "100")
                        {
                            if (responses.Count() == 0)
                            {
                                // We tried to perform a lookup for Sales Tax based on your address, but there was no response. Please check your ZIP code.
                                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.invalidZipCode));
                            }
                            else if (responses.Count() == 1)
                            {
                                isSalesTaxKnown = true;
                                zipTax = xmlResponse.Element("ziptax").ToString();
                            }
                            else
                            {
                                isSalesTaxKnown = false;
                            }

                            salesTaxAddressesToSelectFrom = new List<salesTaxAddress>();

                            foreach (var response in responses)
                            {
                                salesTaxAddress objSalesTaxAddress = new salesTaxAddress
                                {
                                    town = response.Element("geoCity").Value,
                                    county = response.Element("geoCounty").Value,
                                    state = response.Element("geoState").Value
                                };

                                objSalesTaxAddress.addressLine = String.Format("{0}, {1}, {2}", objSalesTaxAddress.town, objSalesTaxAddress.county, objSalesTaxAddress.state);

                                salesTaxAddressesToSelectFrom.Add(objSalesTaxAddress);
                            }
                        }
                        else if (code == "104")
                        {
                            //  104 (Invalid_postal_code) has been added for completeness
                            //  When an invalid zip code is passed the code 100 (Success) is returned with no Response elements instead
                            // We tried to perform a lookup for Sales Tax based on your address, but there was no response. Please check your ZIP code.
                            return Ok(dgFramework.raiseErrorToHandle(webapiDataError.invalidZipCode));
                        }
                        else
                        {
                            // We tried to perform a lookup for Sales Tax based on your address, but there was no response. Please check your ZIP code.
                            return Ok(dgFramework.raiseErrorToHandle(webapiDataError.zipTaxServiceFailed));
                        }
                    }
                }
                catch (Exception)
                {
                    return Ok(dgFramework.raiseErrorToHandle(webapiDataError.zipTaxServiceFailed));
                }

                if (isSalesTaxKnown)
                {
                    // great
                }
                else

                if (salesTaxAddressesToSelectFrom.Count == 0)
                {
                    return Ok(dgFramework.raiseErrorToHandle(webapiDataError.zipTaxServiceFailed));
                }
                else
                {
                    return Ok(new { isSalesTaxKnown = false, state = "selectSalesTaxAddress", salesTaxAddressList = salesTaxAddressesToSelectFrom });
                }
            }

            var address = new passCheckoutSaveAddressData()
            {
                CheckoutBasketID = null,
                DeliveryOptionID = objDeliveryAddress.DeliveryOptionID ?? 1,
                MemberAddressID = objDeliveryAddress.MemberAddressID,
                FullName = objDeliveryAddress.FullName,
                CompanyName = objDeliveryAddress.CompanyName,
                AddressLine1 = objDeliveryAddress.AddressLine1,
                AddressLine2 = objDeliveryAddress.AddressLine2,
                Town = objDeliveryAddress.Town,
                County = objDeliveryAddress.County,
                Postcode = objDeliveryAddress.Postcode,
                CountryID = deliveryCountry.CountryID,
                PhoneNumber = objDeliveryAddress.PhoneNumber,
                UPSAPID = objDeliveryAddress.UpsAPid,
                ZipTax = zipTax
            };

            passCheckoutSaveAddressData upsIdAddress = null;
            
            if (objDeliveryAddress.UPSAPAddress != null)
            {
                var identificationCountries = await objCountry.countrySelectList(null, objCountryItem.LanguageID);
            
                var idCountry = identificationCountries.countries.Where(c => c.Code == objDeliveryAddress.UPSAPAddress.CountryCode).SingleOrDefault();

                if (idCountry == null)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry, "idCountry could not be identified from objDeliveryAddress.BasketCountry!");
                }
                upsIdAddress = new passCheckoutSaveAddressData()
                {
                    CheckoutBasketID = null,
                    DeliveryOptionID = 3,
                    MemberAddressID = objDeliveryAddress.UPSAPMemberAddressId,
                    FullName = objDeliveryAddress.UPSAPAddress.FullName,
                    CompanyName = objDeliveryAddress.UPSAPAddress.CompanyName,
                    AddressLine1 = objDeliveryAddress.UPSAPAddress.AddressLine1,
                    AddressLine2 = objDeliveryAddress.UPSAPAddress.AddressLine2,
                    Town = objDeliveryAddress.UPSAPAddress.Town,
                    County = objDeliveryAddress.UPSAPAddress.County,
                    Postcode = objDeliveryAddress.UPSAPAddress.PostCode,
                    CountryID = idCountry.CountryID,
                    PhoneNumber = objDeliveryAddress.UPSAPAddress.PhoneNumber,
                    UPSAPID = objDeliveryAddress.UpsAPid,
                    ZipTax = zipTax
                };
            }

            List<passCheckoutSaveAddressData> upsIdAddressList = null;

            if (upsIdAddress != null)
            {
                upsIdAddressList = new List<passCheckoutSaveAddressData>() { upsIdAddress };
            }

            var success = await objCheckout.checkoutAddressSave(checkoutId, 
                new List<passCheckoutSaveAddressData> { address }, 
                objDeliveryAddress.UPSAPMemberAddressId,
                upsIdAddressList,
                objDeliveryAddress.isClickCollect,
                resetDeliveryOptions: objDeliveryAddress.secondSave == true ? false : true);

            if (success == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutAddressNotSelected, "checkoutAddressSave did not return successfully!");
            }

            return Ok(new { state = "OK" });
        }


        [System.Web.Http.HttpPost]
        public async Task<object> checkSalesTaxZipCode(webapiCheckoutSalesTaxZipCode objDeliveryAddress)
        {
            //  Determine the date to obtain the data from the cms
            DateTime useDate = DateTime.Now;
            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                //  Should the "use date" be overriden?
                if (HttpContext.Current.Request.QueryString["dateOverride"] == null)
                {
                    useDate = DateTime.Now;
                }
                else
                {
                    String overrideDate = HttpContext.Current.Request.QueryString["dateOverride"].ToString();
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

            if (objDeliveryAddress == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            var session = HttpContext.Current.Session;
            if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
            }
            else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
            }
            else if (dgFramework.isUserLoggedIn(session["loginStatus"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.SecurityFail);
            }

            _ = Convert.ToInt64(session["checkoutId"].ToString());


            //  Get the country details linked to the user
            cmsCountry objCountry = new cmsCountry();
            cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(useDate);

            if (dgFramework.doesCountryExistFromText(objDeliveryAddress.BasketCountry, objCountryData)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
            }
            countryItem objBasketCountryItem = dgFramework.getCountryFromText(objDeliveryAddress.BasketCountry, objCountryData);

            //  Get the visitor id
            Int64 visitorDbId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            salesTaxReturn objSalesTaxReturn = new salesTaxReturn();

            if (objDeliveryAddress.MemberAddressID == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            VisitorFactory visitorFactory = new VisitorFactory();
            MemberAddressSelect objMemberAddressSelect = await visitorFactory.getMemberAddresses(visitorDbId, objDeliveryAddress.MemberAddressID, objBasketCountryItem.LanguageID, objBasketCountryItem.CountryID, useDate);
            if (objMemberAddressSelect.Addresses.Count() == 0)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            //  Get the visitordb and member address id to ensure member address for this person            
            string postCode = objMemberAddressSelect.Addresses[0].PostCode;
            short countryID = objMemberAddressSelect.Addresses[0].CountryID;

            if (countryID == 237 || countryID == 238) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            if (postCode.Length > 5)
            {
                postCode = postCode.Substring(0, 5);
            }

            String zipTaxApiUrl = String.Format("{0}?key={1}&postalcode={2}&format=XML",
                                                    System.Web.Configuration.WebConfigurationManager.AppSettings["zipTaxUrl"],
                                                    System.Web.Configuration.WebConfigurationManager.AppSettings["zipTaxKey"],
                                                    Uri.EscapeDataString(postCode));

            if (objDeliveryAddress.County != null && objDeliveryAddress.Town != null)
            {
                // don't know why ZipTaxCounty isn't used... probably because Town is sufficiently specific
                zipTaxApiUrl = String.Format("{0}?key={1}&postalcode={2}&state={3}&city={4}&format=XML",
                                                WebConfigurationManager.AppSettings["zipTaxUrl"],
                                                WebConfigurationManager.AppSettings["zipTaxKey"],
                                                Uri.EscapeDataString(postCode),
                                                Uri.EscapeDataString(objDeliveryAddress.County), // State != County
                                                Uri.EscapeDataString(objDeliveryAddress.Town));

                if (objDeliveryAddress.USCounty != null)
                {
                    // don't know why ZipTaxCounty isn't used... probably because Town is sufficiently specific
                    zipTaxApiUrl = String.Format("{0}?key={1}&postalcode={2}&state={3}&city={4}&county={5}&format=XML",
                                                    WebConfigurationManager.AppSettings["zipTaxUrl"],
                                                    WebConfigurationManager.AppSettings["zipTaxKey"],
                                                    Uri.EscapeDataString(postCode),
                                                    Uri.EscapeDataString(objDeliveryAddress.County), // State != County
                                                    Uri.EscapeDataString(objDeliveryAddress.Town),
                                                    Uri.EscapeDataString(objDeliveryAddress.USCounty)); // USCounty == County
                }
            }
            try
            {
                XDocument xmlResponse = XDocument.Load(zipTaxApiUrl);

                if (xmlResponse.Descendants("ziptax").Count() == 0)
                {
                    throw new Exception("Expected response not returned");
                }
                else
                {
                    String code = xmlResponse.Element("ziptax").Element("code").Value;
                    var responses = xmlResponse.Element("ziptax").Descendants("response");

                    if (code == "100")
                    {
                        if (responses.Count() == 0)
                        {
                            return dgFramework.raiseErrorToHandle(webapiDataError.invalidZipCode);
                        }
                        else if (responses.Count() == 1)
                        {
                            objSalesTaxReturn.isSalesTaxKnown = true;
                        }
                        else
                        {
                            objSalesTaxReturn.isSalesTaxKnown = false;
                        }

                        objSalesTaxReturn.salesTaxAddresses = new List<salesTaxAddress>();

                        foreach (var response in responses)
                        {
                            salesTaxAddress objSalesTaxAddress = new salesTaxAddress
                            {
                                town = response.Element("geoCity").Value,
                                county = response.Element("geoCounty").Value,
                                state = response.Element("geoState").Value
                            };

                            objSalesTaxAddress.addressLine = String.Format("{0}, {1}, {2}", objSalesTaxAddress.town, objSalesTaxAddress.county, objSalesTaxAddress.state);

                            objSalesTaxReturn.salesTaxAddresses.Add(objSalesTaxAddress);
                        }
                    }
                    else if (code == "104")
                    {
                        //  104 (Invalid_postal_code) has been added for completeness
                        //  When an invalid zip code is passed the code 100 (Success) is returned with no Response elements instead
                        return dgFramework.raiseErrorToHandle(webapiDataError.invalidZipCode);
                    }
                    else
                    {
                        throw new Exception("Invalid parameters passed");
                    }
                }
            }
            catch (Exception)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.zipTaxServiceFailed);
            }

            return objSalesTaxReturn;
        }
        
        [System.Web.Http.HttpPost]
        public async Task<object> guestCheckSalesTaxZipCode(webapiGuestCheckoutSalesTaxZipCode objDeliveryAddress)
        {
            //  Determine the date to obtain the data from the cms
            DateTime useDate = DateTime.Now;
            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                //  Should the "use date" be overriden?
                if (HttpContext.Current.Request.QueryString["dateOverride"] == null)
                {
                    useDate = DateTime.Now;
                }
                else
                {
                    String overrideDate = HttpContext.Current.Request.QueryString["dateOverride"].ToString();
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

            if (objDeliveryAddress == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            var session = HttpContext.Current.Session;
            if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
            }
            else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
            }

            _ = Convert.ToInt64(session["checkoutId"].ToString());


            //  Get the country details linked to the user
            cmsCountry objCountry = new cmsCountry();
            cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(useDate);

            if (dgFramework.doesCountryExistFromText(objDeliveryAddress.country, objCountryData)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
            }

            _ = dgFramework.getCountryFromText(objDeliveryAddress.country, objCountryData);
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                //  Get the visitor id
                _ = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }
            
            if (objDeliveryAddress.country == "UM" || objDeliveryAddress.country == "US") { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            if (objDeliveryAddress.postcode.Length > 5)
            {
                objDeliveryAddress.postcode = objDeliveryAddress.postcode.Substring(0, 5);
            }

            String zipTaxApiUrl = String.Format("{0}?key={1}&postalcode={2}&format=XML",
                                                    System.Web.Configuration.WebConfigurationManager.AppSettings["zipTaxUrl"],
                                                    System.Web.Configuration.WebConfigurationManager.AppSettings["zipTaxKey"],
                                                    Uri.EscapeDataString(objDeliveryAddress.postcode));

            var objSalesTaxReturn = new salesTaxReturn();

            try
            {
                XDocument xmlResponse = XDocument.Load(zipTaxApiUrl);

                if (xmlResponse.Descendants("ziptax").Count() == 0)
                {
                    throw new Exception("Expected response not returned");
                }
                else
                {
                    String code = xmlResponse.Element("ziptax").Element("code").Value;
                    var responses = xmlResponse.Element("ziptax").Descendants("response");

                    if (code == "100")
                    {
                        if (responses.Count() == 0)
                        {
                            return dgFramework.raiseErrorToHandle(webapiDataError.invalidZipCode);
                        }
                        else if (responses.Count() == 1)
                        {
                            objSalesTaxReturn.isSalesTaxKnown = true;
                        }
                        else
                        {
                            objSalesTaxReturn.isSalesTaxKnown = false;
                        }

                        objSalesTaxReturn.salesTaxAddresses = new List<salesTaxAddress>();

                        foreach (var response in responses)
                        {
                            salesTaxAddress objSalesTaxAddress = new salesTaxAddress
                            {
                                town = response.Element("geoCity").Value,
                                county = response.Element("geoCounty").Value,
                                state = response.Element("geoState").Value
                            };

                            objSalesTaxAddress.addressLine = String.Format("{0}, {1}, {2}", objSalesTaxAddress.town, objSalesTaxAddress.county, objSalesTaxAddress.state);

                            objSalesTaxReturn.salesTaxAddresses.Add(objSalesTaxAddress);
                        }
                    }
                    else if (code == "104")
                    {
                        //  104 (Invalid_postal_code) has been added for completeness
                        //  When an invalid zip code is passed the code 100 (Success) is returned with no Response elements instead
                        return dgFramework.raiseErrorToHandle(webapiDataError.invalidZipCode);
                    }
                    else
                    {
                        throw new Exception("Invalid parameters passed");
                    }
                }
            }
            catch (Exception)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.zipTaxServiceFailed);
            }

            return objSalesTaxReturn;
        }

        [System.Web.Http.HttpPost]
        public async Task<object> saveDispatch(webapiDispatchLineSave objDispatchSave)
        {
            if (objDispatchSave.dispatchLines == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            var session = HttpContext.Current.Session;
            if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
            }
            else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
            }
            else
            {
                Int64 checkoutId = Convert.ToInt64(session["checkoutId"].ToString());

                //  Build dispatch storage
                DataTable dispatchTableType = new DataTable();
                dispatchTableType.Columns.Add("DispatchID", typeof(byte));
                dispatchTableType.Columns.Add("DeliveryBandID", typeof(short));
                dispatchTableType.Columns.Add("PartShip", typeof(short));
                dispatchTableType.Columns.Add("MinDeliveryDate", typeof(DateTime));
                dispatchTableType.Columns.Add("MaxDelivery", typeof(DateTime));
                dispatchTableType.Columns.Add("CustomerRequestsFullDespatch", typeof(bool));

                foreach (webapiDispatchLine objDispatchLine in objDispatchSave.dispatchLines)
                {
                    dispatchTableType.Rows.Add(objDispatchLine.dispatchId, 
                        objDispatchLine.deliveryBandId, 
                        objDispatchLine.partShip, 
                        objDispatchLine.minDeliveryDate,
                        objDispatchLine.maxDeliveryDate, 
                        objDispatchLine.customerRequestsFullDespatch ?? false);
                }

                checkoutDb objCheckout = new checkoutDb();
                _ = await objCheckout.saveDispatchOptions(checkoutId, dispatchTableType);
            }
            return "";
        }


        [System.Web.Http.HttpPost]
        public async Task<object> saveGiftBoxing(PassGiftBoxSave objGiftSave)
        {
            if (objGiftSave == null)
            {
                objGiftSave = new PassGiftBoxSave()
                {
                    giftBoxes = new List<giftBox>()
                };
            }

            if (objGiftSave.giftBoxes == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            var session = HttpContext.Current.Session;
            if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
            }
            else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
            }
            else
            {
                Int64 checkoutId = Convert.ToInt64(session["checkoutId"].ToString());

                //  Build gift box table type
                DataTable giftBoxTableType = new DataTable();
                giftBoxTableType.Columns.Add("CheckoutGiftBoxID", typeof(byte));
                giftBoxTableType.Columns.Add("GiftMessage", typeof(string));

                foreach (giftBox objGiftBoxLine in objGiftSave.giftBoxes)
                {
                    giftBoxTableType.Rows.Add(objGiftBoxLine.giftBoxId, objGiftBoxLine.giftBoxMessage);
                }

                //  Build basket gift table type
                DataTable giftBasketTableType = new DataTable();
                giftBasketTableType.Columns.Add("CheckoutBasketID", typeof(Int64));
                giftBasketTableType.Columns.Add("BasketLineTypeID", typeof(byte));
                giftBasketTableType.Columns.Add("CheckoutGiftBoxID", typeof(byte));
                giftBasketTableType.Columns.Add("Quantity", typeof(decimal));

                foreach (giftProduct objGiftBasketLine in objGiftSave.giftBoxes.SelectMany(p => p.products))
                {
                    giftBasketTableType.Rows.Add(objGiftBasketLine.checkoutBasketId, 
                        objGiftBasketLine.basketLineTypeId,
                        objGiftBasketLine.giftBoxId,
                        objGiftBasketLine.quantity);
                }

                checkoutDb objCheckout = new checkoutDb();
                Boolean success = await objCheckout.saveGiftOptions(checkoutId, giftBoxTableType, giftBasketTableType);
            }
            return "";
        }


        [System.Web.Http.HttpPost]
        public async Task<object> clearGift()
        {
            var session = HttpContext.Current.Session;
            if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
            }
            else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
            }
            else
            {
                Int64 checkoutId = Convert.ToInt64(session["checkoutId"].ToString());

                //  Build gift box table type
                DataTable giftBoxTableType = new DataTable();
                giftBoxTableType.Columns.Add("CheckoutGiftBoxID", typeof(byte));
                giftBoxTableType.Columns.Add("GiftMessage", typeof(string));


                //  Build basket gift table type
                DataTable giftBasketTableType = new DataTable();
                giftBasketTableType.Columns.Add("CheckoutBasketID", typeof(Int64));
                giftBasketTableType.Columns.Add("BasketLineTypeID", typeof(byte));
                giftBasketTableType.Columns.Add("CheckoutGiftBoxID", typeof(byte));
                giftBasketTableType.Columns.Add("Quantity", typeof(decimal));


                checkoutDb objCheckout = new checkoutDb();
                _ = await objCheckout.saveGiftOptions(checkoutId, giftBoxTableType, giftBasketTableType);
            }
            return "";
        }


        [System.Web.Http.HttpPost]
        public async Task<object> saveBillingAddressSelection(webapiBillingAddressSelect objBillingAddressSelection)
        {
            //  Determine the date to obtain the data from the cms
            DateTime useDate = DateTime.Now;
            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                //  Should the "use date" be overriden?
                if (HttpContext.Current.Request.QueryString["dateOverride"] == null)
                {
                    useDate = DateTime.Now;
                }
                else
                {
                    String overrideDate = HttpContext.Current.Request.QueryString["dateOverride"].ToString();
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

            if (objBillingAddressSelection == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            var session = HttpContext.Current.Session;
            if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
            }
            else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
            }
            else
            {
                _ = Convert.ToInt64(session["checkoutId"].ToString());


                //  Get the visitor id
                Int64 visitorDbId = -1;
                if (session["visitorIdFromDatabase"] == null)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
                }
                else
                {
                    visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
                }


                //  Get the country
                String basketCountry = objBillingAddressSelection.basketCountry.Trim();
                cmsCountry objCountry = new cmsCountry();
                cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(useDate);

                if (dgFramework.doesCountryExistFromText(basketCountry, objCountryData)) { }
                else
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
                }
                countryItem objCountryItem = dgFramework.getCountryFromText(basketCountry, objCountryData);


                if (objBillingAddressSelection.memberAddressId == null) { }
                else
                {
                    //  Does the address belong to the visitor?
                    VisitorFactory visitorFactory = new VisitorFactory();
                    MemberAddressSelect tempMemberAddressSelect = await visitorFactory.getMemberAddresses(visitorDbId, objBillingAddressSelection.memberAddressId, objCountryItem.LanguageID, null, useDate);
                    if (tempMemberAddressSelect.Addresses.Count() == 0)
                    {
                        dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressNotInserted);
                    }
                }

                //  Save the selected Billing Address (null will remove current selection)
                _ = new checkoutDb();
            }

            return new { memberAddressId = objBillingAddressSelection.memberAddressId };
        }


        [System.Web.Http.HttpPost]
        public async Task<object> getGiftVouchersInBasket()
        {
            giftVoucherInBasket objGiftVouchersInBasket = new giftVoucherInBasket();
            var session = HttpContext.Current.Session;
            if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
            }
            else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
            }
            else
            {
                Int64 checkoutId = Convert.ToInt64(session["checkoutId"].ToString());

                checkoutDb objCheckout = new checkoutDb();
                objGiftVouchersInBasket = await objCheckout.getGiftVouchersInBasket(checkoutId);


                foreach (giftVoucherDetail objRow in objGiftVouchersInBasket.giftVoucherDetails)
                {
                    if (objRow.ExpiryDT == null) { }
                    else
                    {
                        objRow.displayExpiryDate = objRow.ExpiryDT.Value.ToString("dd/MM/yyyy");
                    }
                }

            }

            return objGiftVouchersInBasket;
        }


        [System.Web.Http.HttpPost]
        public async Task<object> saveGiftCardsInBasket(PassGiftCardSave objGiftCardSave)
        {
            if (objGiftCardSave.giftCards == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            var session = HttpContext.Current.Session;
            if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
            }
            else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
            }
            else
            {
                Int64 checkoutId = Convert.ToInt64(session["checkoutId"].ToString());

                //  Build gift voucher table type
                var giftVoucherTableType = new DataTable();
                giftVoucherTableType.Columns.Add("CheckoutBasketID", typeof(Int64));
                giftVoucherTableType.Columns.Add("GiftVoucherMessage", typeof(string));

                foreach (var objRow in objGiftCardSave.giftCards)
                {
                    giftVoucherTableType.Rows.Add(objRow.CheckoutBasketID, objRow.Message);
                }


                var objCheckout = new checkoutDb();
                _ = await objCheckout.saveGiftVoucherOptions(checkoutId, giftVoucherTableType);
            }
            return "";




        }


        [System.Web.Http.HttpPost]
        public async Task<object> insertGiftVoucher(webapiGiftVoucherInsert objInsertGiftVoucher)
        {
            //  Determine the date to obtain the data from the cms
            DateTime useDate = DateTime.Now;
            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                //  Should the "use date" be overriden?
                if (HttpContext.Current.Request.QueryString["dateOverride"] == null)
                {
                    useDate = DateTime.Now;
                }
                else
                {
                    String overrideDate = HttpContext.Current.Request.QueryString["dateOverride"].ToString();
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

            if (objInsertGiftVoucher == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            giftVoucherDetail objReturn = new giftVoucherDetail();
            var session = HttpContext.Current.Session;
            if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
            }
            else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
            }
            else
            {
                Int64 checkoutId = Convert.ToInt64(session["checkoutId"].ToString());


                //  Get the country details linked to the user
                String tempCountry = objInsertGiftVoucher.Country.Trim();
                cmsCountry objCountry = new cmsCountry();
                cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(useDate);

                if (dgFramework.doesCountryExistFromText(tempCountry, objCountryData)) { }
                else
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
                }
                countryItem objCountryItem = dgFramework.getCountryFromText(tempCountry, objCountryData);


                objInsertGiftVoucher.Code = objInsertGiftVoucher.Code.Trim();
                if (objInsertGiftVoucher.Code == "")
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
                }


                checkoutDb objCheckout = new checkoutDb();
                giftVoucherInBasket objGiftVouchersInBasket = await objCheckout.getGiftVouchersInBasket(checkoutId);


                CheckoutPayment objCheckoutPayments;
                
                //  Validate code
                String validVoucherCodeMessage = await objCheckout.validateGiftVoucherCode(checkoutId, objInsertGiftVoucher.Code, objCountryItem.LanguageID);
                if (validVoucherCodeMessage == "") { }
                else
                {
                    objReturn.isValid = false;
                    objReturn.failureMessage = validVoucherCodeMessage;

                    //  Get the order balance
                    objCheckoutPayments = await objCheckout.checkoutPayGenerate(checkoutId, objCountryItem.CountryID, objCountryItem.LanguageID, "CheckoutController.insertGiftVoucher (invalid)");
                    objReturn.BalanceTotal = objCheckoutPayments.paymentTotals[0].TotalBalance;

                    return objReturn;
                }


                //  Insert code
                Boolean success = await objCheckout.insertGiftVoucherCode(checkoutId, objInsertGiftVoucher.Code);
                if (success == true) { }
                else
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
                }


                //  Get all codes, but return only the entered code
                objGiftVouchersInBasket = await objCheckout.getGiftVouchersInBasket(checkoutId);
                if (objGiftVouchersInBasket.giftVoucherDetails.Where(p => p.Code.ToLower() == objInsertGiftVoucher.Code.ToLower()).FirstOrDefault() == null)
                {
                    return dgFramework.raiseErrorToHandle(webapiDataError.invalidGiftVoucherEntered);
                }

                objReturn = objGiftVouchersInBasket.giftVoucherDetails.Where(p => p.Code.ToLower() == objInsertGiftVoucher.Code.ToLower()).First();

                if (objReturn.ExpiryDT == null) { }
                else
                {
                    objReturn.displayExpiryDate = objReturn.ExpiryDT.Value.ToString("dd/MM/yyyy");
                }


                //  Get the order balance                
                objCheckoutPayments = await objCheckout.checkoutPayGenerate(checkoutId, objCountryItem.CountryID, objCountryItem.LanguageID, "CheckoutController.insertGiftVoucher (valid)");
                objReturn.BalanceTotal = objCheckoutPayments.paymentTotals[0].TotalBalance;


            }

            objReturn.isValid = true;

            return objReturn;
        }


        [System.Web.Http.HttpPost]
        public async Task<object> deleteGiftVoucher(webapiGiftVoucherDelete objDeleteGiftVoucher)
        {
            //  Determine the date to obtain the data from the cms
            DateTime useDate = DateTime.Now;
            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                //  Should the "use date" be overriden?
                if (HttpContext.Current.Request.QueryString["dateOverride"] == null)
                {
                    useDate = DateTime.Now;
                }
                else
                {
                    String overrideDate = HttpContext.Current.Request.QueryString["dateOverride"].ToString();
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

            if (objDeleteGiftVoucher == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            giftVoucherDelete objReturn = new giftVoucherDelete();
            var session = HttpContext.Current.Session;
            if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
            }
            else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
            }
            else
            {
                Int64 checkoutId = Convert.ToInt64(session["checkoutId"].ToString());
                
                //  Get the country details linked to the user
                String tempCountry = objDeleteGiftVoucher.Country.Trim();
                cmsCountry objCountry = new cmsCountry();
                cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(useDate);

                if (dgFramework.doesCountryExistFromText(tempCountry, objCountryData)) { }
                else
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
                }
                countryItem objCountryItem = dgFramework.getCountryFromText(tempCountry, objCountryData);


                //  Delete code
                checkoutDb objCheckout = new checkoutDb();
                Boolean success = await objCheckout.deleteGiftVoucherCode(checkoutId, objDeleteGiftVoucher.GiftVoucherId);
                if (success == true) { }
                else
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
                }


                //  Get the order balance
                CheckoutPayment objCheckoutPayments = await objCheckout.checkoutPayGenerate(checkoutId, objCountryItem.CountryID, objCountryItem.LanguageID, "CheckoutController.deleteGiftVoucher");
                objReturn.BalanceTotal = objCheckoutPayments.paymentTotals[0].TotalBalance;


            }

            return objReturn;
        }

        [System.Web.Http.HttpPost]
        public async Task<object> checkoutBraintreeGenerate(webapiCheckoutBraintree objCheckoutBraintree)
        {
            try
            {
                //  Determine the date to obtain the data from the cms
                DateTime useDate = DateTime.Now;
                useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
                if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
                else
                {
                    //  Should the "use date" be overriden?
                    if (HttpContext.Current.Request.QueryString["dateOverride"] == null)
                    {
                        useDate = DateTime.Now;
                    }
                    else
                    {
                        String overrideDate = HttpContext.Current.Request.QueryString["dateOverride"].ToString();
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

                //  Get the country
                String basketCountry = objCheckoutBraintree.basketCountry.Trim();
                cmsCountry objCountry = new cmsCountry();
                cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(useDate);

                var session = HttpContext.Current.Session;

                if (dgFramework.doesCountryExistFromText(basketCountry, objCountryData)) { }
                else
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
                }
                countryItem objCountryItem = dgFramework.getCountryFromText(basketCountry, objCountryData);

                if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
                }
                else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
                {
                    return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
                }
                else
                {
                    Int64 checkoutId = Convert.ToInt64(HttpContext.Current.Session["checkoutId"].ToString());
                    
                    checkoutDb objCheckout = new checkoutDb();
                    var objCheckoutPayments = await objCheckout.checkoutPayGenerate(checkoutId, objCountryItem.CountryID, objCountryItem.LanguageID, "CheckoutController.checkoutBraintreeGenerate");

                    var gateway = new BraintreeGateway()
                    {
                        Environment = Braintree.Environment.ParseEnvironment(objCheckoutPayments.checkoutDetails[0].Environment),
                        MerchantId = objCheckoutPayments.checkoutDetails[0].MerchantID,
                        PublicKey = objCheckoutPayments.checkoutDetails[0].PublicKey,
                        PrivateKey = objCheckoutPayments.checkoutDetails[0].PrivateKey
                    };

                    CheckoutBraintreeParameters objReturn = new CheckoutBraintreeParameters
                    {
                        acceptDiscover = objCheckoutPayments.checkoutDetails[0].AcceptDiscover,
                        acceptMaestro = objCheckoutPayments.checkoutDetails[0].AcceptMaestro,
                        acceptMaestroUK = objCheckoutPayments.checkoutDetails[0].AcceptMaestroUK,
                        acceptMasterCard = objCheckoutPayments.checkoutDetails[0].AcceptMasterCard,
                        acceptVisa = objCheckoutPayments.checkoutDetails[0].AcceptVisa,
                        acceptAmex = objCheckoutPayments.checkoutDetails[0].AcceptAmex,
                        countryItem = objCountryItem,
                        threeDSecure = objCheckoutPayments.checkoutDetails[0].Mandate3DSecure,
                        currencyCode = objCheckoutPayments.checkoutDetails[0].Currency,
                        avs = objCheckoutPayments.checkoutDetails[0].AVS,
                        paypal = objCheckoutPayments.checkoutDetails[0].PayPal,
                        amount = objCheckoutPayments.paymentTotals[0].TotalBalance,
                        clientToken = await gateway.ClientToken.GenerateAsync(
                        new ClientTokenRequest()
                        {
                            MerchantAccountId = objCheckoutPayments.checkoutDetails[0].MerchantAccountID
                        }
                    ),

                        isMember = await objCheckout.GetCheckoutType(checkoutId) == dgCheckoutType.Member
                    };

                    String viewName = "~/Views-New/LandingPages/FixedPages/Checkout/PaymentSections/_CheckoutBraintree.cshtml"; //dgFramework.IsMobileDevice() ? "_checkoutBraintree.mobile" : "_checkoutBraintree";

                    var partial = RenderViewToString("Checkout", viewName, objReturn);

                    return Request.CreateResponse(HttpStatusCode.OK, partial);
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, $"checkoutBraintreeGenerate - CheckoutID: {HttpContext.Current.Session["checkoutId"]}, MemberID: {HttpContext.Current.Session["memberIdFromDatabase"]}, VisitorID: {HttpContext.Current.Session["visitorIdFromDatabase"]}");
                throw ex;
            }
        }

        [System.Web.Http.HttpPost]
        public async Task<bool> checkoutAttachPaymentToken(webApiCheckoutPaymentToken objCheckoutPaymentToken)
        {
            try
            {
                var session = HttpContext.Current.Session;
                if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
                }
                else if (objCheckoutPaymentToken.lastFourDigits == null || objCheckoutPaymentToken.lastFourDigits.Length != 4)
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
                else if (String.IsNullOrEmpty(objCheckoutPaymentToken.token) || String.IsNullOrEmpty(objCheckoutPaymentToken.cardType))
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
                else
                {
                    Int64 checkoutId = Convert.ToInt64(HttpContext.Current.Session["checkoutId"].ToString());

                    checkoutDb objCheckout = new checkoutDb();

                    return await objCheckout.addPaymentToken(checkoutId, objCheckoutPaymentToken.token, objCheckoutPaymentToken.bin, objCheckoutPaymentToken.lastFourDigits, objCheckoutPaymentToken.cardType);
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "checkoutAttachPaymentToken");
                return false;
            }
        }

        [System.Web.Http.HttpPost]
        public async Task<object> checkoutBraintreeSubmit(webapiCheckoutBraintreeSubmit objCheckoutSubmission)
        {
            var Session = HttpContext.Current.Session;
            checkoutDb objCheckout = new checkoutDb();

            try
            {
                checkoutBraintreeResult objReturn = new checkoutBraintreeResult() {
                    result = "FAIL",
                    message = null,
                    redirect = "basket"
                };

                /* Die early to test the braintree response error lookups */
                //{
                //    objReturn.redirect = "pay";

                //    var authCode = "2004";

                //    var languageId = objCheckoutSubmission.masterDefinition.UrlBreakdown.countryDetails.LanguageID;
                //    var countryId = objCheckoutSubmission.masterDefinition.UrlBreakdown.countryDetails.CountryID;
                //    var contentDate = objCheckoutSubmission.masterDefinition.UrlBreakdown.ContentDate;

                //    var braintreeErrorResponseLookup = new cmsBraintreeErrorResponse();
                //    var braintreeErrorResponses = await braintreeErrorResponseLookup.loadBraintreeResponseCodesFromCms(languageId, contentDate,
                //        Convert.ToInt16(System.Web.Configuration.WebConfigurationManager.AppSettings["keepCacheInMinutes"]));

                //    var lookupResult = braintreeErrorResponses.errorResponseSelects.Where(p => p.Code == Convert.ToInt16(authCode)).SingleOrDefault();

                //    var customerMessage = $"{HtmlHelperExtra.Resource("BraintreeErrorResponseGeneric", languageId, countryId, contentDate)}";

                //    if (lookupResult == null)
                //    {
                //        customerMessage += $"<br/><br/>{HtmlHelperExtra.Resource("BraintreeErrorResponseRetry", languageId, countryId, contentDate)}";
                //        customerMessage += $"<br/><br/>{HtmlHelperExtra.Resource("BraintreeErrorResponseContactCS1", languageId, countryId, contentDate)}";
                //        customerMessage += $"<br/><br/>{HtmlHelperExtra.Resource("BraintreeErrorResponseContactCS2", languageId, countryId, contentDate)}";
                //    }
                //    else
                //    {
                //        if (lookupResult.AdviseCustomerToAmendCVV)
                //        {
                //            customerMessage += $"<br/><br/>{HtmlHelperExtra.Resource("BraintreeErrorResponseCVV", languageId, countryId, contentDate)}";
                //        }
                //        else
                //        {
                //            if (lookupResult.PermitRetry)
                //            {
                //                customerMessage += $"<br/><br/>{HtmlHelperExtra.Resource("BraintreeErrorResponseRetry", languageId, countryId, contentDate)}";
                //            }

                //            if (lookupResult.AdviseCustomerToContactTheirBank)
                //            {
                //                customerMessage += $"<br/><br/>{HtmlHelperExtra.Resource("BraintreeErrorResponseContactBank", languageId, countryId, contentDate)}";
                //            }
                //            else
                //            {
                //                customerMessage += $"<br/><br/>{HtmlHelperExtra.Resource("BraintreeErrorResponseContactCS1", languageId, countryId, contentDate)}";
                //                customerMessage += $"<br/><br/>{HtmlHelperExtra.Resource("BraintreeErrorResponseContactCS2", languageId, countryId, contentDate)}";
                //            }
                //        }
                //    }

                //    objReturn.message = customerMessage;

                //    return objReturn;
                //}
                /* End of early death */

                
                if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == false)
                {
                    return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
                }
                else if (dgFramework.isCheckoutSessionValid(Session["checkoutExpiry"]) == false)
                {
                    return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
                }

                Int64 checkoutId = Convert.ToInt64(Session["checkoutId"].ToString());

                dgCheckoutType checkoutType = await objCheckout.GetCheckoutType(checkoutId);

                if (objCheckoutSubmission == null)
                {
                    objReturn.message = "objCheckoutSubmission was null!";
                    Logger.Fatal($"objCheckoutSubmission was null! - CheckoutID: {HttpContext.Current.Session["checkoutId"]}, MemberID: {HttpContext.Current.Session["memberIdFromDatabase"]}, VisitorID: {HttpContext.Current.Session["visitorIdFromDatabase"]}");
                    return objReturn;
                }
                if ((checkoutType == dgCheckoutType.Guest || checkoutType == dgCheckoutType.Member) && objCheckoutSubmission.payment_method_nonce == null)
                {
                    objReturn.message = "Payment Method was null!";
                    Logger.Fatal($"Payment method was null! - CheckoutID: {HttpContext.Current.Session["checkoutId"]}, MemberID: {HttpContext.Current.Session["memberIdFromDatabase"]}, VisitorID: {HttpContext.Current.Session["visitorIdFromDatabase"]}");
                    return objReturn;
                }
                else if (checkoutType == dgCheckoutType.Paypal)
                {
                    if (Session["paypal_nonce_" + checkoutId.ToString()] == null)
                    {
                        objReturn.message = "Paypal selected but no nonce present";
                        Logger.Fatal($"Paypal selected but no nonce present! - CheckoutID: {HttpContext.Current.Session["checkoutId"]}, MemberID: {HttpContext.Current.Session["memberIdFromDatabase"]}, VisitorID: {HttpContext.Current.Session["visitorIdFromDatabase"]}");
                        return objReturn;
                    }
                    else
                    {
                        objCheckoutSubmission.payment_method_nonce = (string)Session["paypal_nonce_" + checkoutId.ToString()];
                    }
                }
                else if (checkoutType == dgCheckoutType.Apple) 
                { 
                    // must remember to save email
                }

                if (objCheckoutSubmission.masterDefinition == null)
                {
                    objReturn.message = "masterDefinition was null!";
                    Logger.Fatal($"masterDefinition was null! - CheckoutID: {HttpContext.Current.Session["checkoutId"]}, MemberID: {HttpContext.Current.Session["memberIdFromDatabase"]}, VisitorID: {HttpContext.Current.Session["visitorIdFromDatabase"]}");
                    return objReturn;
                }
                if (objCheckoutSubmission.basketCountry == null)
                {
                    objReturn.message = "basketCountry was null!";
                    Logger.Fatal($"basketCountry was null! - CheckoutID: {HttpContext.Current.Session["checkoutId"]}, MemberID: {HttpContext.Current.Session["memberIdFromDatabase"]}, VisitorID: {HttpContext.Current.Session["visitorIdFromDatabase"]}");
                    return objReturn;
                }

                masterDefinition objMaster = objCheckoutSubmission.masterDefinition;
                paymentType paymentType = paymentType.unknown;
                StringWriter str = new StringWriter();
                String threeDSecureLog = "Not Secured";
                Guid paymentToken = Guid.NewGuid();

                // Validate that the order has been completed by gift vouchers only
                Byte paidInFull = await objCheckout.checkoutPaidInFullCheck(paymentToken, checkoutId);

                if (paidInFull == 1)
                {
                    paymentType = paymentType.giftVoucherFull;
                }
                else if (paidInFull == 2)
                {
                    paymentType = paymentType.tradeOrder;
                }
                else if (paidInFull == 3)
                {
                    paymentType = paymentType.zeroValueRetailOrder;
                }
                else
                {
                    BraintreeGateway gateway = null;
                    TransactionRequest request = null;
                    DateTime useDate = DateTime.Now;
                    String basketCountry = objCheckoutSubmission.basketCountry;
                    cmsCountry objCountry = new cmsCountry();
                    cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(useDate);

                    var returnedCheckoutGUID = "";

                    if (dgFramework.doesCountryExistFromText(basketCountry, objCountryData)) { }
                    else
                    {
                        dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
                    }
                    countryItem objCountryItem = dgFramework.getCountryFromText(basketCountry, objCountryData);

                    if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == false)
                    {
                        dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
                    }
                    else if (dgFramework.isCheckoutSessionValid(Session["checkoutExpiry"]) == false)
                    {
                        return objReturn;
                    }
                    else
                    {
                        var objCheckoutPayments = await objCheckout.checkoutPayGenerate(checkoutId, objCountryItem.CountryID, objCountryItem.LanguageID, "CheckoutController.checkoutBraintreeSubmit");

                        gateway = new BraintreeGateway
                        {
                            Environment = Braintree.Environment.ParseEnvironment(objCheckoutPayments.checkoutDetails[0].Environment),
                            MerchantId = objCheckoutPayments.checkoutDetails[0].MerchantID,
                            PublicKey = objCheckoutPayments.checkoutDetails[0].PublicKey,
                            PrivateKey = objCheckoutPayments.checkoutDetails[0].PrivateKey
                        };

                        PaymentMethodNonce nonce = gateway.PaymentMethodNonce.Find(objCheckoutSubmission.payment_method_nonce);

                        if (nonce == null)
                        {
                            objReturn.message = "nonce from gateway was null!";
                            Logger.Fatal($"Nonce from gateway was null! - CheckoutID: {HttpContext.Current.Session["checkoutId"]}, MemberID: {HttpContext.Current.Session["memberIdFromDatabase"]}, VisitorID: {HttpContext.Current.Session["visitorIdFromDatabase"]}");
                            return objReturn;
                        }
                        if (nonce.Type == null)
                        {
                            objReturn.message = "nonce.Type was null!";
                            Logger.Fatal($"Nonce.Type was null! - CheckoutID: {HttpContext.Current.Session["checkoutId"]}, MemberID: {HttpContext.Current.Session["memberIdFromDatabase"]}, VisitorID: {HttpContext.Current.Session["visitorIdFromDatabase"]}");
                            return objReturn;
                        }

                        var nonceLiabilityShifted = false;

                        ThreeDSecureInfo info = null;

                        if (nonce.Type.ToLower() == "creditcard")
                        {
                            info = nonce.ThreeDSecureInfo;

                            if (info == null)
                            {
                                objReturn.message = "nonce.ThreeDSecureInfo was null - tokenized data was not passed through client.verify3DS!";
                                Logger.Fatal($"nonce.ThreeDSecureInfo was null - tokenized data was not passed through client.verify3DS! - CheckoutID: {HttpContext.Current.Session["checkoutId"]}, MemberID: {HttpContext.Current.Session["memberIdFromDatabase"]}, VisitorID: {HttpContext.Current.Session["visitorIdFromDatabase"]}");
                                return objReturn;
                            }

                            threeDSecureLog = "\r\n\r\nCheckoutID: " + checkoutId.ToString();
                            threeDSecureLog += "\r\nPriceList: " + objCheckoutPayments.checkoutDetails[0].PriceList;
                            threeDSecureLog += "\r\n3D Secure Info...";
                            threeDSecureLog += "\r\nEnrolled: " + info.Enrolled;
                            threeDSecureLog += "\r\nStatus: " + info.Status;
                            threeDSecureLog += "\r\nLiabilityShifted: " + info.LiabilityShifted;
                            threeDSecureLog += "\r\nLiabilityShiftPossible: " + info.LiabilityShiftPossible;

                            if (info.LiabilityShifted.HasValue)
                            {
                                if (info.LiabilityShifted.Value == true)
                                {
                                    nonceLiabilityShifted = true;
                                }
                            }

                            if (info.LiabilityShiftPossible.HasValue)
                            {
                                if (info.LiabilityShiftPossible.Value == true)
                                {
                                    if (nonceLiabilityShifted == false)
                                    {
                                        objReturn.message = "3dsecuremessage";
                                        objReturn.redirect = "pay";
                                        return objReturn;
                                    }
                                }
                            }

                            if (info.Status == "authenticate_rejected" ||
                                info.Status == "authenticate_unable_to_authenticate")
                            {
                                objReturn.message = "3dsecuremessage";
                                objReturn.redirect = "pay";
                                return objReturn;
                            }
                        }

                        if (nonceLiabilityShifted == true || nonce.Type.ToLower() == "paypalaccount" || objCheckoutPayments.checkoutDetails[0].Mandate3DSecure == false)
                        {
                            string memberId = null;

                            if (Session["memberIdFromDatabase"] != null)
                                memberId = Session["memberIdFromDatabase"].ToString();

                            request = new TransactionRequest
                            {
                                MerchantAccountId = objCheckoutPayments.checkoutDetails[0].MerchantAccountID,
                                OrderId = checkoutId.ToString(),

                                Customer = new CustomerRequest()
                                {
                                    Email = objCheckoutPayments.checkoutDetails[0].Email
                                },
                                Amount = Math.Round(objCheckoutPayments.paymentTotals[0].TotalBalance, (int)2),
                                CustomFields = new Dictionary<String, String>() {
                                    { "checkoutguid", objCheckoutPayments.checkoutDetails[0].GUID.ToString() },
                                    { "orderid", checkoutId.ToString() },
                                    { "pricelist", objCheckoutPayments.checkoutDetails[0].PriceList }
                                },
                                Options = new TransactionOptionsRequest()
                                {
                                    ThreeDSecure = new TransactionOptionsThreeDSecureRequest()
                                    {
                                        Required = objCheckoutPayments.checkoutDetails[0].Mandate3DSecure
                                    },

                                    SubmitForSettlement = true
                                },
                                DeviceData = objCheckoutSubmission.device_data,
                                ShippingAddress = new AddressRequest()
                                {
                                    FirstName = objCheckoutPayments.deliveryAddresses[0].FullName,
                                    LastName = "",
                                    StreetAddress = objCheckoutPayments.deliveryAddresses[0].AddressLine1,
                                    Locality = objCheckoutPayments.deliveryAddresses[0].AddressLine2,
                                    ExtendedAddress = objCheckoutPayments.deliveryAddresses[0].Town,
                                    Region = objCheckoutPayments.deliveryAddresses[0].County,
                                    PostalCode = dgFramework.SimpleTruncate(Regex.Replace(objCheckoutPayments.deliveryAddresses[0].PostCode ?? "", @"[^A-Za-z\d\s-]", ""), 9),
                                    CountryCodeAlpha2 = objCheckoutPayments.deliveryAddresses[0].Code,
                                }
                            };

                            request.PaymentMethodNonce = objCheckoutSubmission.payment_method_nonce;

                            Result<Transaction> result = gateway.Transaction.Sale(request);

                            String logString = PrintResult(result);
                            Logger.Trace("braintreeResult: \r\n{0}", logString);
                            Logger.Trace("3D Secure: \r\n{0}", threeDSecureLog);

                            if (result.IsSuccess())
                            {
                                paymentType = paymentType.payWithBraintree;

                                Transaction transaction = null;
                                string nullIdMessage = string.Empty;

                                if (result.Transaction != null)
                                {
                                    if (result.Transaction.CustomFields != null)
                                    {
                                        if (result.Transaction.CustomFields["checkoutguid"] != null)
                                        {
                                            returnedCheckoutGUID = result.Transaction.CustomFields["checkoutguid"].ToString();
                                        }
                                    }

                                    transaction = result.Transaction;
                                    nullIdMessage = "result.Transaction.Id was NULL";
                                }
                                else if (result.Target != null)
                                {
                                    if (result.Target.CustomFields != null)
                                    {
                                        if (result.Target.CustomFields["checkoutguid"] != null)
                                        {
                                            returnedCheckoutGUID = result.Target.CustomFields["checkoutguid"].ToString();
                                        }
                                    }
                                    transaction = result.Target;
                                    nullIdMessage = "result.Target.Id was NULL";
                                }

                                // this is being performed as a prelude to checkoutPaySave so that if checkoutPaySave does fail, we do at least
                                // know that we've taken payment and can prevent duplicate payment attempts following successful payment.

                                try
                                {
                                    if (checkoutType != dgCheckoutType.Apple)
                                    {
                                        var paymentTakenStored = await objCheckout.checkoutPaymentTaken(
                                            checkoutId,
                                            transaction.Id == null ? nullIdMessage : result.Target.Id,
                                            paymentType.payWithBraintree.ToString(),
                                            XmlResult(result),
                                            objCheckoutSubmission.marketingPermissions);
                                    }
                                    else
                                    {
                                        var paymentTakenStored = await objCheckout.checkoutPaymentTaken(
                                            checkoutId,
                                            transaction.Id == null ? nullIdMessage : result.Target.Id,
                                            paymentType.applePay.ToString(),
                                            XmlResult(result),
                                            objCheckoutSubmission.marketingPermissions,
                                            null,
                                            objCheckoutSubmission.email, objCheckoutSubmission.phone);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    var sb = new StringBuilder();

                                    foreach (var i in objCheckoutSubmission.marketingPermissions)
                                    {
                                        PropertyPrinter.PrintProperties(i, 2, sb);
                                    }

                                    var marketingPermissionsString = sb.ToString();

                                    if (String.IsNullOrEmpty(marketingPermissionsString))
                                    {
                                        marketingPermissionsString = "[NONE PROVIDED]";
                                    }

                                    var checkoutLogger = LogManager.GetLogger("CheckoutLogger");

                                    var detailsString = $@"FAILED to execute pub.Checkout_Payment_Taken 

                                        MemberID: { HttpContext.Current.Session["memberIdFromDatabase"]}, 
                                        VisitorID: { HttpContext.Current.Session["visitorIdFromDatabase"]}
                                    
                                        @CheckoutID = {checkoutId}, 
                                        @PaymentRef = {(transaction.Id == null ? nullIdMessage : result.Target.Id)}, 
                                        @PaymentType = {paymentType.payWithBraintree}, 
                                        @PaymentDetails = [Available in Log text file], 
                                        @MarketingPermissions = {marketingPermissionsString}";

                                    checkoutLogger.Fatal(ex, detailsString);

                                    //  Get exception details
                                    var exceptionType = ex.GetType().ToString();
                                    var exceptionMessage = ex.Message;
                                    var exceptionSource = ex.Source;
                                    var stackTrace = "";
                                    var innerExceptionType = ex.GetType().ToString();
                                    var innerExceptionMessage = ex.Message;
                                    var innerExceptionSource = ex.Source;
                                    var innerExceptionStackTrace = "";

                                    if (ex.StackTrace != null)
                                    {
                                        stackTrace = ex.StackTrace;
                                    }

                                    //  Get inner exception details
                                    if (ex.InnerException == null) { }
                                    else
                                    {
                                        innerExceptionType = ex.InnerException.GetType().ToString();
                                        innerExceptionMessage = ex.InnerException.Message;
                                        innerExceptionSource = ex.InnerException.Source;

                                        if (ex.InnerException.StackTrace != null)
                                        {
                                            innerExceptionStackTrace = ex.InnerException.StackTrace;
                                        }
                                    }

                                    var objError = new Models.Database.errorDB();

                                    objError.saveToErrorLog(Convert.ToInt64(Session["visitorDbId"]), exceptionType, exceptionMessage, exceptionSource, stackTrace,
                                                            innerExceptionType, innerExceptionMessage, innerExceptionSource, innerExceptionStackTrace,
                                                            detailsString, dgFramework.getUserAgent(), dgFramework.getIPAddress());

                                    await objCheckout.logCheckoutAction(checkoutId, 251, detailsString);
                                }
                                /*
                                 
                                    if (!paymenttakenstored) { send email to IT support with the words URGENT and PAYMENT PROBLEM somewhere in there - include CheckoutID, response from Braintree (id, XML details?) }
                                                                    
                                    log to checkoutLogs generic error 

                                 */

                                // the idea is that if the following call to checkoutPaySave fails OR TIMEOUTs, we already know that a payment has been made... but transaction records not
                                // generated successfully. In that instance, we want to drop the user into checkoutConfirmSpartan.cshtml; this will at least be something rather than nothing.

                                // in order to achieve this, we need to:

                                // modify checkoutPaySave to accept a checkoutId instead of a GUID; we don't have the GUID in session and we do have the checkoutId

                                // this effectively decouples from the heavy lifting process, pub.Checkout_Pay_Save_Redux


                                //if (!string.IsNullOrEmpty(returnedCheckoutGUID))
                                //{
                                //    Guid paymentGuid = new Guid(returnedCheckoutGUID);
                                //    Int16? countryId = null;
                                //    if (transaction.BillingAddress.CountryCodeAlpha2 != null)
                                //    {
                                //        countryItem country = dgFramework.getCountryFromCode(transaction.BillingAddress.CountryCodeAlpha2, objCountryData);
                                //        countryId = country?.CountryID;
                                //    }
                                //    await objCheckout.saveSelectedBillingAddressFromBraintree(paymentGuid, transaction.BillingAddress, countryId);
                                //    await objCheckout.checkoutPaySave(paymentGuid, transaction.Id == null ? nullIdMessage : result.Target.Id, paymentType.payWithBraintree.ToString(), XmlResult(result), objCheckoutSubmission.marketingPermissions);
                                //}


                                // Task.Run() to fire this off without waiting for it to happen

#pragma warning disable 4014
                                Task.Run(() => {
                                    if (objCheckoutSubmission.make_default && !string.IsNullOrEmpty(transaction.CreditCard?.Token))
                                    {
                                        PaymentMethodRequest paymentMethodRequest = new PaymentMethodRequest()
                                        {
                                            Options = new PaymentMethodOptionsRequest()
                                            {
                                                MakeDefault = true,
                                                VerifyCard = false
                                            }
                                        };

                                        try
                                        {
                                            var updResult = gateway.PaymentMethod.Update(transaction.CreditCard.Token, paymentMethodRequest);
                                        }
                                        catch { }
                                    }
                                });
#pragma warning restore 4014

                                // we are *done* here.
                                // return to the user, inform her that her payment has been taken, change her loading notification to indicate that we are generating her order.
                                // this will appear proactive and good; she will be happy and wait for the... 30 seconds that we're going to leave her waiting maximum 
                                // while we fire a subsequent AJAX call to checkoutPaySave[Redux]
                                objReturn.result = "OK";
                                objReturn.message = "braintreePaymentOK, paymentTaken, call orderGenerate";
                                objReturn.redirect = "orderGen";
                                return objReturn;
                            }
                            else
                            {
                                string xmlResult = XmlResult(result);

                                string
                                    summaryMessage = "",
                                    gatewayRejectionReason = "",
                                    threeDSecureStatus = "",
                                    threeDSecureEnrolled = "",
                                    processorAuthCode = "",
                                    processorResponseCode = "",
                                    processorResponseText = "",
                                    additionalProcessorResponse = "",
                                    avsPostalCodeResponseCode = "";

                                bool
                                    liabilityShifted = false,
                                    liabilityShiftPossible = false;

                                summaryMessage = result.Message ?? "";

                                if (result.Target != null)
                                {
                                    if (result.Target.ThreeDSecureInfo != null)
                                    {
                                        threeDSecureStatus = result.Target.ThreeDSecureInfo.Status;
                                        threeDSecureEnrolled = result.Target.ThreeDSecureInfo.Enrolled;
                                        liabilityShifted = result.Target.ThreeDSecureInfo.LiabilityShifted ?? false;
                                        liabilityShiftPossible = result.Target.ThreeDSecureInfo.LiabilityShiftPossible ?? false;
                                    }
                                    else
                                    {
                                        threeDSecureStatus = "ThreeDSecureInfo Response Object was NULL";
                                        threeDSecureEnrolled = null;
                                        liabilityShifted = false;
                                        liabilityShiftPossible = false;
                                    }
                                    gatewayRejectionReason = result.Target.GatewayRejectionReason.ToString();

                                    processorAuthCode = result.Target.ProcessorAuthorizationCode;
                                    processorResponseCode = result.Target.ProcessorResponseCode;
                                    processorResponseText = result.Target.ProcessorResponseText;
                                    additionalProcessorResponse = result.Target.AdditionalProcessorResponse;
                                    avsPostalCodeResponseCode = result.Target.AvsPostalCodeResponseCode;
                                }
                                else
                                {
                                    if (result.Errors != null)
                                    {

                                        String errorList = "\r\nresult.Errors.DeepAll()";
                                        foreach (ValidationError error in result.Errors.DeepAll())
                                        {
                                            errorList += "\r\nCode: " + error.Code.ToString();

                                            if (error.Message == null)
                                            {
                                                errorList += "\r\nMessage was null.";
                                            }
                                            else
                                            {
                                                errorList += "\r\nMessage: " + error.Message;
                                            }

                                            if (error.Attribute == null)
                                            {
                                                errorList += "\r\nAttribute was null.";
                                            }
                                            else
                                            {
                                                errorList += "\r\nAttribute: " + error.Attribute;
                                            }
                                        }

                                        Logger.Trace("{0}", errorList);
                                    }

                                    if (result.Transaction != null)
                                    {
                                        if (result.Transaction.ThreeDSecureInfo != null)
                                        {
                                            threeDSecureStatus = result.Transaction.ThreeDSecureInfo.Status;
                                            threeDSecureEnrolled = result.Transaction.ThreeDSecureInfo.Enrolled.Substring(0, 1);
                                            liabilityShifted = result.Transaction.ThreeDSecureInfo.LiabilityShifted ?? false;
                                            liabilityShiftPossible = result.Transaction.ThreeDSecureInfo.LiabilityShiftPossible ?? false;
                                        }
                                        else
                                        {
                                            threeDSecureStatus = "ThreeDSecureInfo Response Object was NULL";
                                            threeDSecureEnrolled = "?";
                                            liabilityShifted = false;
                                            liabilityShiftPossible = false;
                                        }

                                        gatewayRejectionReason = result.Transaction.GatewayRejectionReason.ToString();
                                        processorAuthCode = result.Transaction.ProcessorAuthorizationCode;
                                        processorResponseCode = result.Transaction.ProcessorResponseCode;
                                        processorResponseText = result.Transaction.ProcessorResponseText;
                                        additionalProcessorResponse = result.Transaction.AdditionalProcessorResponse;
                                        avsPostalCodeResponseCode = result.Transaction.AvsPostalCodeResponseCode;
                                    }
                                    if (result.Target != null)
                                    {
                                        if (result.Target.ThreeDSecureInfo != null)
                                        {
                                            threeDSecureStatus = result.Target.ThreeDSecureInfo.Status;
                                            threeDSecureEnrolled = result.Target.ThreeDSecureInfo.Enrolled.Substring(0, 1);
                                            liabilityShifted = result.Target.ThreeDSecureInfo.LiabilityShifted ?? false;
                                            liabilityShiftPossible = result.Target.ThreeDSecureInfo.LiabilityShiftPossible ?? false;
                                        }
                                        else
                                        {
                                            threeDSecureStatus = "ThreeDSecureInfo Response Object was NULL";
                                            threeDSecureEnrolled = "?";
                                            liabilityShifted = false;
                                            liabilityShiftPossible = false;
                                        }

                                        gatewayRejectionReason = result.Target.GatewayRejectionReason.ToString();
                                        processorAuthCode = result.Target.ProcessorAuthorizationCode;
                                        processorResponseCode = result.Target.ProcessorResponseCode;
                                        processorResponseText = result.Target.ProcessorResponseText;
                                        additionalProcessorResponse = result.Target.AdditionalProcessorResponse;
                                        avsPostalCodeResponseCode = result.Target.AvsPostalCodeResponseCode;
                                    }
                                }

                                await objCheckout.checkoutAttemptSave(checkoutId,
                                    summaryMessage ?? "",
                                    gatewayRejectionReason ?? "",
                                    threeDSecureStatus ?? "",
                                    threeDSecureEnrolled ?? "",
                                    liabilityShifted,
                                    liabilityShiftPossible,
                                    processorAuthCode ?? "",
                                    processorResponseCode ?? "",
                                    processorResponseText ?? "",
                                    additionalProcessorResponse ?? "",
                                    avsPostalCodeResponseCode ?? "",
                                    xmlResult ?? "");

                                objReturn.message = result.Message;
                                objReturn.redirect = "pay";

                                var languageId = objCheckoutSubmission.masterDefinition.UrlBreakdown.countryDetails.LanguageID;
                                var countryId = objCheckoutSubmission.masterDefinition.UrlBreakdown.countryDetails.CountryID;
                                var contentDate = objCheckoutSubmission.masterDefinition.UrlBreakdown.ContentDate;

                                var braintreeErrorResponseLookup = new cmsBraintreeErrorResponse();
                                var braintreeErrorResponses = await braintreeErrorResponseLookup.loadBraintreeResponseCodesFromCms(languageId, contentDate);

                                var lookupResult = braintreeErrorResponses.errorResponseSelects.Where(p => p.Code == processorAuthCode).SingleOrDefault();

                                var customerMessage = $"{HtmlHelperExtra.Resource("BraintreeErrorResponseGeneric", languageId, countryId, contentDate)}";

                                if (lookupResult == null)
                                {
                                    customerMessage += $"<br/><br/>{HtmlHelperExtra.Resource("BraintreeErrorResponseRetry", languageId, countryId, contentDate)}";
                                    customerMessage += $"<br/><br/>{HtmlHelperExtra.Resource("BraintreeErrorResponseContactCS1", languageId, countryId, contentDate)}";
                                    customerMessage += $"<br/><br/>{HtmlHelperExtra.Resource("BraintreeErrorResponseContactCS2", languageId, countryId, contentDate)}";
                                }
                                else
                                {
                                    if (lookupResult.AdviseCustomerToAmendCVV)
                                    {
                                        customerMessage += $"<br/><br/>{HtmlHelperExtra.Resource("BraintreeErrorResponseCVV", languageId, countryId, contentDate)}";
                                    }
                                    else
                                    {
                                        if (lookupResult.PermitRetry)
                                        {
                                            customerMessage += $"<br/><br/>{HtmlHelperExtra.Resource("BraintreeErrorResponseRetry", languageId, countryId, contentDate)}";
                                        }

                                        if (lookupResult.AdviseCustomerToContactTheirBank)
                                        {
                                            customerMessage += $"<br/><br/>{HtmlHelperExtra.Resource("BraintreeErrorResponseContactBank", languageId, countryId, contentDate)}";
                                        }
                                        else
                                        {
                                            customerMessage += $"<br/><br/>{HtmlHelperExtra.Resource("BraintreeErrorResponseContactCS1", languageId, countryId, contentDate)}";
                                            customerMessage += $"<br/><br/>{HtmlHelperExtra.Resource("BraintreeErrorResponseContactCS2", languageId, countryId, contentDate)}";
                                        }
                                    }
                                }

                                objReturn.message = customerMessage;

                                return objReturn;
                            }
                        }
                        else
                        {
                            var status = "";
                            var enrolled = "";
                            var liabilityShift = false;
                            var liabilityShiftPossible = false;

                            if (nonce == null)
                            {

                            }
                            else
                            {
                                if (info != null)
                                {
                                    if (info.Status != null)
                                        status = info.Status;
                                    else
                                        status = "NULL";

                                    if (info.Enrolled != null)
                                        enrolled = info.Enrolled.Substring(0, 1);
                                    else
                                        enrolled = "NULL";

                                    liabilityShift = info.LiabilityShifted.HasValue && info.LiabilityShifted.Value;
                                    liabilityShiftPossible = info.LiabilityShiftPossible.HasValue && info.LiabilityShiftPossible.Value;
                                }
                                else
                                {
                                    status = "NULL";
                                    enrolled = "NULL";
                                    liabilityShift = false;
                                    liabilityShiftPossible = false;
                                }
                            }

                            await objCheckout.checkoutAttemptSave(
                                    checkoutId,
                                    "Non-US, Non-PayPal, No Liability Shift",
                                    "Not Submitted to Gateway",
                                    status,
                                    enrolled,
                                    liabilityShift,
                                    liabilityShiftPossible,
                                    null,
                                    null,
                                    null,
                                    null,
                                    null,
                                    null);

                            objReturn.redirect = "pay";
                            objReturn.message = "3dsecuremessage";
                            return objReturn;
                        }
                    }

                    if (paymentType == paymentType.unknown)
                    {
                        objReturn.message = "Payment type not detected!";
                        Logger.Fatal($"Payment type not detected! - CheckoutID: {HttpContext.Current.Session["checkoutId"]}, MemberID: {HttpContext.Current.Session["memberIdFromDatabase"]}, VisitorID: {HttpContext.Current.Session["visitorIdFromDatabase"]}");
                        return objReturn;
                    }
                    else if (paymentType == paymentType.giftVoucherFull ||
                        paymentType == paymentType.zeroValueRetailOrder ||
                        paymentType == paymentType.tradeOrder)
                    {
                        // this is being performed as a prelude to checkoutPaySave so that if checkoutPaySave does fail, we do at least
                        // know that we've taken payment and can prevent duplicate payment attempts following successful payment.
                        var paymentTakenStored = await objCheckout.checkoutPaymentTaken(checkoutId, paymentType.ToString(), paymentType.payWithBraintree.ToString(), "<root></root>", objCheckoutSubmission.marketingPermissions);

                        //Boolean success = await objCheckout.checkoutPaySave(new Guid(returnedCheckoutGUID.ToUpper()), paymentType.ToString(), paymentType.ToString(), "<root></root>", objCheckoutSubmission.marketingPermissions);
                    }
                    else
                    {
                        // already called Save!
                    }

                    //  Check that the receipt can continue
                    if (await objCheckout.preflightCheckReceipt(new Guid(returnedCheckoutGUID.ToUpper()), objMaster.UrlBreakdown.countryDetails.CountryID) == false)
                    {
                        objReturn.message = "Checkout preflight checks failed.";
                        Logger.Fatal($"Checkout preflight checks failed! - CheckoutID: {HttpContext.Current.Session["checkoutId"]}, MemberID: {HttpContext.Current.Session["memberIdFromDatabase"]}, VisitorID: {HttpContext.Current.Session["visitorIdFromDatabase"]}");
                        return objReturn;
                    }

                    // are these lines ever hit now?
                    objReturn.result = "OK";
                    objReturn.redirect = "confirm";
                    objReturn.message = "braintreePaymentOK";

                    return objReturn;
                }

                // Are these lines ever hit now?
                objReturn.result = "OK";
                objReturn.redirect = "confirm";
                objReturn.message = "giftvoucherPaymentOK";

                return objReturn;
            }
            catch (Braintree.Exceptions.NotFoundException bex)
            {
                var checkoutLogger = LogManager.GetLogger("CheckoutLogger");

                checkoutLogger.Error(bex, $"checkoutBraintreeSubmit - CheckoutID: {Session["checkoutId"]}");

                await objCheckout.logCheckoutAction(Convert.ToInt64(Session["checkoutId"]), 251, bex.ToString());

                var languageId = objCheckoutSubmission.masterDefinition.UrlBreakdown.countryDetails.LanguageID;
                var countryId = objCheckoutSubmission.masterDefinition.UrlBreakdown.countryDetails.CountryID;
                var contentDate = objCheckoutSubmission.masterDefinition.UrlBreakdown.ContentDate;

                var customerMessage = $"{HtmlHelperExtra.Resource("BraintreeErrorResponseGeneric", languageId, countryId, contentDate)}";
                
                customerMessage += $"<br/><br/>{HtmlHelperExtra.Resource("BraintreeErrorResponseRetry", languageId, countryId, contentDate)}";
                customerMessage += $"<br/><br/>{HtmlHelperExtra.Resource("BraintreeErrorResponseContactCS1", languageId, countryId, contentDate)}";
                customerMessage += $"<br/><br/>{HtmlHelperExtra.Resource("BraintreeErrorResponseContactCS2", languageId, countryId, contentDate)}";
                
                var objReturn = new checkoutBraintreeResult()
                {
                    result = "FAIL",
                    message = customerMessage,
                    redirect = "pay"
                };

                return objReturn;
            }
            catch (Exception ex)
            {
                var checkoutLogger = LogManager.GetLogger("CheckoutLogger");
                
                checkoutLogger.Fatal(ex, $"checkoutBraintreeSubmit - CheckoutID: { HttpContext.Current.Session["checkoutId"]}, MemberID: { HttpContext.Current.Session["memberIdFromDatabase"]}, VisitorID: { HttpContext.Current.Session["visitorIdFromDatabase"]}");

                await objCheckout.logCheckoutAction(Convert.ToInt64(Session["checkoutId"]), 251, ex.ToString());

                throw ex;
            }
        }

        // do not save marketing permissions here; these should be saved in the paymentTaken call...?
        [System.Web.Http.HttpPost]
        public async Task<object> checkoutOrderGenerate()
        {
            var Session = HttpContext.Current.Session;

            var objCheckout = new checkoutDb();

            try
            {
                if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == false)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
                }
                // probably don't check expiry - adds little value and you might well just be on the cusp of the expiry
                else if (dgFramework.isCheckoutSessionValid(Session["checkoutExpiry"]) == false)
                {
                    return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
                }
                // We want as much security as possible around this call, obviously...
                // Should that exist in the database, or here, or both? Database, I say.
                // We can't just let people use Postman etc. to fire a call off to checkoutOrderGenerate - which they could well do...
                // as this would result in the order being generated.
                // they must have:
                // a valid checkoutId
                // a PaymentSavedDT... it would actually be desirable not to have to perform a million different calls to fetch data out of the db, 
                // when we're about to call the DB and give it a perfectly good opportunity to perform those checks itself.

                await objCheckout.checkoutPaySaveRedux(Convert.ToInt64(Session["checkoutId"]));

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception exc)
            {
                var checkoutLogger = LogManager.GetLogger("CheckoutLogger");

                if (HttpContext.Current.Session["memberTradeAccountID"] == null)
                    checkoutLogger.Fatal(exc, $"FAILED executing pub.Checkout_Complete @CheckoutID = {Session["checkoutId"]}, MemberID: { HttpContext.Current.Session["memberIdFromDatabase"]}, VisitorID: { HttpContext.Current.Session["visitorIdFromDatabase"]}");

                //  Get exception details
                var exceptionType = exc.GetType().ToString();
                var exceptionMessage = exc.Message;
                var exceptionSource = exc.Source;
                var stackTrace = "";
                var innerExceptionType = exc.GetType().ToString();
                var innerExceptionMessage = exc.Message;
                var innerExceptionSource = exc.Source;
                var innerExceptionStackTrace = "";

                if (exc.StackTrace != null)
                {
                    stackTrace = exc.StackTrace;
                }

                //  Get inner exception details
                if (exc.InnerException == null) { }
                else
                {
                    innerExceptionType = exc.InnerException.GetType().ToString();
                    innerExceptionMessage = exc.InnerException.Message;
                    innerExceptionSource = exc.InnerException.Source;

                    if (exc.InnerException.StackTrace != null)
                    {
                        innerExceptionStackTrace = exc.InnerException.StackTrace;
                    }
                }

                var objError = new Models.Database.errorDB();

                long visitorId = -1;
                if (Session["visitorIdFromDatabase"] != null)
                {
                    visitorId = Convert.ToInt64(Session["visitorIdFromDatabase"]);
                }

                var detailsString = $"FAILED executing pub.Checkout_Complete @CheckoutID = {Session["checkoutId"]}";

                // Trade orders have a manual throw in Checkout_Complete which will always be called
                // we don't want to be getting emailed about them all the time
                if (HttpContext.Current.Session["memberTradeAccountID"] == null)
                {
                    objError.saveToErrorLog(visitorId, exceptionType, exceptionMessage, exceptionSource, stackTrace,
                                            innerExceptionType, innerExceptionMessage, innerExceptionSource, innerExceptionStackTrace,
                                            detailsString, dgFramework.getUserAgent(), dgFramework.getIPAddress());
                }
                
                await objCheckout.logCheckoutAction(Convert.ToInt64(Session["checkoutId"]), 251, detailsString);

                // log to checkout logs! use checkoutError actionId, state that checkoutPaySaveRedux has failed and user has been redirect to the Spartan confirmation page

                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        [System.Web.Http.HttpPost]
        public async Task<object> checkoutAmazonSubmit(webapiCheckoutAmazonSubmit objCheckoutSubmission)
        {
            checkoutDb objCheckout = new checkoutDb();

            var Session = HttpContext.Current.Session;

            try
            {
                checkoutAmazonPayResult objReturn = new checkoutAmazonPayResult()
                {
                    result = "FAIL",
                    message = null,
                    redirect = "pay"
                };
                
                if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == false)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
                }
                else if (dgFramework.isCheckoutSessionValid(Session["checkoutExpiry"]) == false)
                {
                    return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
                }
                else if (objCheckoutSubmission == null || String.IsNullOrEmpty(objCheckoutSubmission.orderReferenceId) || Session["amazon_token"] == null)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
                }

                Int64 checkoutId = Convert.ToInt64(Session["checkoutId"].ToString());
                string amazonToken = Session["amazon_token"].ToString();

                if (objCheckoutSubmission.masterDefinition == null)
                {
                    objReturn.message = "masterDefinition was null!";
                    Logger.Fatal($"masterDefinition was null! @CheckoutID = {Session["checkoutId"]}, MemberID: { HttpContext.Current.Session["memberIdFromDatabase"]}, VisitorID: { HttpContext.Current.Session["visitorIdFromDatabase"]}");
                    return objReturn;
                }

                dgCheckoutType checkoutType = await objCheckout.GetCheckoutType(checkoutId);

                if (checkoutType != dgCheckoutType.Amazon)
                {
                    objReturn.message = "Attempting to complete using Amazon on non Amazon checkout";
                    String s = $"CheckoutID: {checkoutId} - {objReturn.message}";
                    Logger.Fatal($"Attempting to complete using Amazon on non Amazon checkout! @CheckoutID = {Session["checkoutId"]}, MemberID: { HttpContext.Current.Session["memberIdFromDatabase"]}, VisitorID: { HttpContext.Current.Session["visitorIdFromDatabase"]}");
                    return objReturn;
                }

                var merchantDetailSelect = await objCheckout.getMerchantDetails(checkoutId);
                var merchantDetails = merchantDetailSelect.checkoutParameters;

                AmazonPay.CommonRequests.Configuration config = new AmazonPay.CommonRequests.Configuration();
                config.WithAccessKey(merchantDetails.AmazonAccessKey)
                    .WithMerchantId(merchantDetails.AmazonMerchantId)
                    .WithSecretKey(merchantDetails.AmazonSecret)
                    .WithRegion(merchantDetails.AmazonRegion);

                if (merchantDetails.AmazonLiveAPI)
                {
                    // do nothing
                }
                else
                {
                    config.WithSandbox(true);
                }

                Client client = new Client(config);

                var objCheckoutPayments = await objCheckout.checkoutPayGenerate(checkoutId,
                    objCheckoutSubmission.masterDefinition.UrlBreakdown.countryDetails.CountryID,
                    objCheckoutSubmission.masterDefinition.UrlBreakdown.countryDetails.LanguageID,
                   "CheckoutController.checkoutAmazonSubmit");

                Enum currencyCode = (Regions.currencyCode)Enum.Parse(typeof(Regions.currencyCode), objCheckoutPayments.checkoutDetails[0].Currency);

                //SetOrderReferenceDetailsRequest setRequestParameters = new SetOrderReferenceDetailsRequest();

                //setRequestParameters.WithAmazonOrderReferenceId(objCheckoutSubmission.orderReferenceId)
                //    .WithAmount(objCheckoutPayments.paymentTotals[0].TotalBalance)
                //    .WithCurrencyCode(currencyCode)
                //    .WithSellerOrderId(objCheckoutPayments.checkoutDetails[0].GUID.ToString())
                //    .WithStoreName("Designers Guild");

                //OrderReferenceDetailsResponse setResponse = client.SetOrderReferenceDetails(setRequestParameters);

                //if (setResponse.GetSuccess())
                //{
                    ConfirmOrderReferenceRequest confirmRequestParameters = new ConfirmOrderReferenceRequest();

                    confirmRequestParameters.WithAmazonOrderReferenceId(objCheckoutSubmission.orderReferenceId);

                    ConfirmOrderReferenceResponse confirmResponse = client.ConfirmOrderReference(confirmRequestParameters);

                    if (confirmResponse.GetSuccess())
                    {
                        // get the order details
                        GetOrderReferenceDetailsRequest orderRequest = new GetOrderReferenceDetailsRequest();
                        orderRequest.WithAccessToken(amazonToken)
                            .WithAmazonOrderReferenceId(objCheckoutSubmission.orderReferenceId);

                        OrderReferenceDetailsResponse orderResponse = client.GetOrderReferenceDetails(orderRequest);

                        if (orderResponse.GetSuccess())
                        {
                            BillingAddressDetails billingDetails = orderResponse.GetBillingAddressDetails();

                            //cmsCountry objCountry = new cmsCountry();
                            //cmsCountryDefinition objCountryData = await objCountry.loadCountryFromCms(DateTime.Now);
                            //countryItem country = dgFramework.getCountryFromCode(billingDetails.GetCountryCode(), objCountryData);

                            // Authorise the payment
                            int authorisationAttempt = Session["AmazonAttempt"] == null ? 1 : (int)Session["AmazonAttempt"] + 1;
                            Session["AmazonAttempt"] = authorisationAttempt;
                            string authorisationId = $"{checkoutId}-{authorisationAttempt}";

                            AuthorizeRequest authRequest = new AuthorizeRequest();
                            authRequest.WithAmazonOrderReferenceId(objCheckoutSubmission.orderReferenceId)
                                .WithAuthorizationReferenceId(authorisationId)
                                .WithAmount(objCheckoutPayments.paymentTotals[0].TotalBalance)
                                .WithCurrencyCode(currencyCode)
                                .WithTransactionTimeout(0)
                                .WithCaptureNow(true)
                                .WithSellerAuthorizationNote("Designers Guild Online Purchase");

                            AuthorizeResponse authResponse = client.Authorize(authRequest);
                            if (authResponse.GetSuccess())
                            {
                                // Authorization should be Closed and we should have the ReasonCode "MaxCapturesProcessed" (1 capture for synchronous mode)
                                if (authResponse.GetAuthorizationState() == "Closed" &&
                                    authResponse.GetReasonCode() == "MaxCapturesProcessed")
                                {
                                    // if we've captured what we're after, great.
                                    if (authResponse.GetCapturedAmount() == objCheckoutPayments.paymentTotals[0].TotalBalance)
                                    {
                                        Guid paymentId = Guid.Parse(orderResponse.GetSellerOrderId());
                                        // update billing address
                                        //var countryId = country?.CountryID;

                                        //try
                                        //{
                                        //    await objCheckout.saveSelectedBillingAddressFromLines(paymentId,
                                        //        billingDetails.GetName(),
                                        //        billingDetails.GetAddressLine1(),
                                        //        billingDetails.GetAddressLine2(),
                                        //        billingDetails.GetCity(),
                                        //        billingDetails.GetCounty(),
                                        //        billingDetails.GetPostalCode(),
                                        //        countryId,
                                        //        billingDetails.GetPhone());
                                        //}
                                        //catch (Exception ex)
                                        //{
                                        //    Logger.Trace(ex, $"CheckoutID: {checkoutId} - Amazon Pay - failed to save billing address");
                                        //}

                                        try
                                        {
                                            // complete the booking
                                            var paymentTaken = await objCheckout.checkoutPaymentTaken(checkoutId, orderResponse.GetAmazonOrderReferenceId(), paymentType.amazonPay.ToString(), "<root></root>", objCheckoutSubmission.marketingPermissions);
                                        }
                                        catch (Exception ex)
                                        {
                                            var sb = new StringBuilder();

                                            foreach (var i in objCheckoutSubmission.marketingPermissions)
                                            {
                                                PropertyPrinter.PrintProperties(i, 2, sb);
                                            }

                                            var marketingPermissionsString = sb.ToString();

                                            if (String.IsNullOrEmpty(marketingPermissionsString))
                                            {
                                                marketingPermissionsString = "[NONE PROVIDED]";
                                            }

                                            var checkoutLogger = LogManager.GetLogger("CheckoutLogger");

                                            var detailsString = $@"FAILED to execute pub.Checkout_Payment_Taken 

                                            MemberID: { HttpContext.Current.Session["memberIdFromDatabase"]}, 
                                            VisitorID: { HttpContext.Current.Session["visitorIdFromDatabase"]}

                                            @CheckoutID = {checkoutId}, 
                                            @PaymentRef = {orderResponse.GetAmazonOrderReferenceId()}, 
                                            @PaymentType = {paymentType.amazonPay}, 
                                            @PaymentDetails = <root></root>, 
                                            @MarketingPermissions = {marketingPermissionsString}";

                                            checkoutLogger.Fatal(ex, detailsString);

                                            //  Get exception details
                                            var exceptionType = ex.GetType().ToString();
                                            var exceptionMessage = ex.Message;
                                            var exceptionSource = ex.Source;
                                            var stackTrace = "";
                                            var innerExceptionType = ex.GetType().ToString();
                                            var innerExceptionMessage = ex.Message;
                                            var innerExceptionSource = ex.Source;
                                            var innerExceptionStackTrace = "";

                                            if (ex.StackTrace != null)
                                            {
                                                stackTrace = ex.StackTrace;
                                            }

                                            //  Get inner exception details
                                            if (ex.InnerException == null) { }
                                            else
                                            {
                                                innerExceptionType = ex.InnerException.GetType().ToString();
                                                innerExceptionMessage = ex.InnerException.Message;
                                                innerExceptionSource = ex.InnerException.Source;

                                                if (ex.InnerException.StackTrace != null)
                                                {
                                                    innerExceptionStackTrace = ex.InnerException.StackTrace;
                                                }
                                            }

                                            var objError = new Models.Database.errorDB();

                                            objError.saveToErrorLog(Convert.ToInt64(Session["visitorDbId"]), exceptionType, exceptionMessage, exceptionSource, stackTrace,
                                                                    innerExceptionType, innerExceptionMessage, innerExceptionSource, innerExceptionStackTrace,
                                                                    detailsString, dgFramework.getUserAgent(), dgFramework.getIPAddress());

                                            await objCheckout.logCheckoutAction(checkoutId, 251, detailsString);
                                        }


                                        objReturn.result = "OK";
                                        objReturn.message = "amazonPaymentOK, paymentTaken, call orderGenerate";
                                        objReturn.redirect = "orderGen";

                                        return objReturn;
                                    }
                                    else // !(authResponse.GetCapturedAmount() == objCheckoutPayments.paymentTotals[0].TotalBalance)
                                    {
                                        objReturn.message = $"Failed Amazon Pay Capture {objCheckoutSubmission.orderReferenceId} - {authResponse.GetJson()}";
                                        objReturn.errorCode = authResponse.GetReasonCode();
                                        objReturn.redirect = "amazonLogout";
                                        Logger.Trace($"CheckoutID: {checkoutId} - {objReturn.message}");

                                        return objReturn;
                                    }
                                }
                                else // !(authResponse.GetAuthorizationState() == "Closed" && authResponse.GetReasonCode() == "MaxCapturesProcessed")
                                {
                                    // as per workflow 
                                    // https://m.media-amazon.com/images/G/01/EPSDocumentation/AmazonPay/Integration/diagram_OrderConfirm-and-OptimalAuth-with-IPNhandler.png?ld=seukapa_1044740793_50715452309

                                    if (authResponse.GetReasonCode() == "InvalidPaymentMethod")
                                    {
                                        // MUST INFORM THE USER
                                        objReturn.message = $"Failed Amazon Pay Authorisation {objCheckoutSubmission.orderReferenceId} - {authResponse.GetJson()}";
                                        objReturn.errorCode = authResponse.GetReasonCode();
                                        objReturn.redirect = "pay";
                                        return objReturn;
                                    }

                                    // for any other response code than invalidPaymentMethod, inform the user that they must be logged out of Amazon

                                    // if the OrderReferenceObject State is Open
                                    if (orderResponse.GetOrderReferenceState() == "Open")
                                    {
                                        // Cancel it
                                        client.CancelOrderReference(new CancelOrderReferenceRequest().WithAmazonOrderReferenceId(objCheckoutSubmission.orderReferenceId).WithCancelationReason(authResponse.GetErrorCode()));
                                    }

                                    Logger.Trace($"CheckoutID: {checkoutId} - {objReturn.message}");
                                    objReturn.message = $"Failed Amazon Pay Authorisation {objCheckoutSubmission.orderReferenceId} - {authResponse.GetJson()}";
                                    objReturn.errorCode = authResponse.GetReasonCode();
                                    objReturn.redirect = "amazonLogout";

                                    return objReturn;
                                }
                            }
                            else // authResponse.GetSuccess() == false
                            {
                                objReturn.message = $"Failed Amazon Pay Authorisation {objCheckoutSubmission.orderReferenceId} - {authResponse.GetJson()}";
                                objReturn.errorCode = authResponse.GetReasonCode();
                                objReturn.redirect = "amazonLogout";
                                Logger.Trace($"CheckoutID: {checkoutId} - {objReturn.message}");
                                return objReturn;
                            }
                        }
                        else
                        {
                            objReturn.message = $"Unable to load Amazon Pay Order {objCheckoutSubmission.orderReferenceId} - {orderResponse.GetJson()}";
                            objReturn.errorCode = confirmResponse.GetErrorCode();
                            objReturn.redirect = "amazonLogout";
                            Logger.Fatal($"CheckoutID: {checkoutId} - {objReturn.message} - MemberID: { HttpContext.Current.Session["memberIdFromDatabase"]}, VisitorID: { HttpContext.Current.Session["visitorIdFromDatabase"]}");
                            return objReturn;
                        }
                    }
                    else
                    {
                        objReturn.message = $"Unable to confirm Amazon Pay Order {objCheckoutSubmission.orderReferenceId} - {confirmResponse.GetJson()}";
                        objReturn.errorCode = confirmResponse.GetErrorCode();
                        if (objReturn.errorCode != "ConstraintsExist")
                            objReturn.errorCode = "generic";

                        objReturn.redirect = "amazonLogout";

                        Logger.Trace($"CheckoutID: {checkoutId} - {objReturn.message}");
                        return objReturn;
                    }
                //}
                //else
                //{
                //    objReturn.message = $"Unable to set order reference details - Amazon Pay Order {objCheckoutSubmission.orderReferenceId} - {setResponse.GetJson()}";
                //    objReturn.errorCode = setResponse.GetErrorCode();

                //    if (objReturn.errorCode != "ConstraintsExist")
                //        objReturn.errorCode = "generic";

                //    objReturn.redirect = "amazonLogout";

                //    Logger.Trace($"CheckoutID: {checkoutId} - {objReturn.message}");

                //    return objReturn;
                //}
            }
            catch (Exception ex)
            {
                var checkoutLogger = LogManager.GetLogger("CheckoutLogger");

                checkoutLogger.Fatal(ex, $"checkoutAmazonSubmit - CheckoutID: {HttpContext.Current.Session["checkoutId"]}, MemberID: {HttpContext.Current.Session["memberIdFromDatabase"]}, VisitorID: {HttpContext.Current.Session["visitorIdFromDatabase"]}");

                await objCheckout.logCheckoutAction(Convert.ToInt64(Session["checkoutId"]), 251, ex.ToString());

                throw ex;
            }
        }

        [System.Web.Http.HttpPost]
        public async Task<object> asyncSaveBraintreeLoaded(webapiBraintreeLoaded objLoaded)
        {
            var Session = HttpContext.Current.Session;
            Int64 checkoutId = Convert.ToInt64(Session["checkoutId"].ToString());

            checkoutDb objCheckout = new checkoutDb();
            bool success = await objCheckout.logCheckoutAction(checkoutId, (int)dgCheckoutActionId.logBraintreeLoaded, string.Format("{0} - {1}ms", objLoaded.version, objLoaded.time.ToString()));
            return success;
        }

        [System.Web.Http.HttpPost]
        public async Task<object> asyncSaveBraintreeLog(webapiBraintreeLate objLog)
        {
            var Session = HttpContext.Current.Session;
            Int64 checkoutId = Convert.ToInt64(Session["checkoutId"].ToString());

            checkoutDb objCheckout = new checkoutDb();
            Boolean success = false;

            switch (objLog.type)
            {
                case "late":
                    success = await objCheckout.logCheckoutAction(checkoutId, (int)dgCheckoutActionId.logBraintreeLate, objLog.time.ToString() + "ms");
                    break;

                case "timeout":
                    success = await objCheckout.logCheckoutAction(checkoutId, (int)dgCheckoutActionId.logBraintreeTimeout, objLog.time.ToString() + "ms");
                    break;

                case "ready":
                    success = await objCheckout.logCheckoutAction(checkoutId, (int)dgCheckoutActionId.logBraintreeReady, objLog.detail);
                    break;

                case "cardType":
                    success = await objCheckout.logCheckoutAction(checkoutId, (int)dgCheckoutActionId.logBraintreeCardType, objLog.detail);
                    break;

                case "payPalSelected":
                    success = await objCheckout.logCheckoutAction(checkoutId, (int)dgCheckoutActionId.logBraintreePayPalSelected, objLog.detail);
                    break;

                case "payPalCancelled":
                    success = await objCheckout.logCheckoutAction(checkoutId, (int)dgCheckoutActionId.logBraintreePayPalCancelled, objLog.detail);
                    break;

                case "buyNowClicked":
                    success = await objCheckout.logCheckoutAction(checkoutId, (int)dgCheckoutActionId.logBraintreeBuyNowClick, objLog.detail);
                    break;

                case "failure":
                    success = await objCheckout.logCheckoutAction(checkoutId, (int)dgCheckoutActionId.logBraintreeScriptNotLoaded, "Uh oh...");
                    break;

                default: break;
            }

            return success;
        }

        public async Task<object> asyncSaveCheckoutLog(webapiCheckoutLog objLog)
        {
            var Session = HttpContext.Current.Session;

            if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
            }
            else if (dgFramework.isCheckoutSessionValid(Session["checkoutExpiry"]) == false)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
            }
            else if (objLog == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }
            
            Int64 checkoutId = Convert.ToInt64(Session["checkoutId"].ToString());

            var validActions = new dgCheckoutActionId[] 
            {
                dgCheckoutActionId.logInvalidAddress,
                dgCheckoutActionId.logValidationError,
                dgCheckoutActionId.logBraintreeBuyNowClick,
                dgCheckoutActionId.logBraintreeBuyClientSideStep,
                dgCheckoutActionId.logAppleBegin,
                dgCheckoutActionId.logAppleOnValidateMerchant,
                dgCheckoutActionId.logAppleOnValidateMerchantError,
                dgCheckoutActionId.logAppleOnValidateMerchantSuccess,
                dgCheckoutActionId.logAppleOnShippingContactSelected,
                dgCheckoutActionId.logAppleCompleteShippingContactSelection,
                dgCheckoutActionId.logAppleOnShippingMethodSelected,
                dgCheckoutActionId.logAppleCompleteShippingMethodSelection,
                dgCheckoutActionId.logAppleOnPaymentAuthorized,
                dgCheckoutActionId.logApplePaymentTokenizationError,
                dgCheckoutActionId.logApplePaymentTokenizationSuccess,
                dgCheckoutActionId.logAppleCompletePayment,
                dgCheckoutActionId.logAppleOnCancel,
                dgCheckoutActionId.logAppleAddressError
            };

            if (validActions.Contains((dgCheckoutActionId)objLog.action))
            {
                
                checkoutDb objCheckout = new checkoutDb();
                Boolean success = await objCheckout.logCheckoutAction(checkoutId, (int)objLog.action, objLog.detail);

                return success;
            }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);

                return false;
            }
        }

        [System.Web.Http.HttpPost]
        public async Task<object> asyncSaveBraintreeError(webapiBraintreeError objError)
        {
            var Session = HttpContext.Current.Session;
            //  Get the receipt data
            Int64 visitorDbId = -1;
            if (Session["visitorIdFromDatabase"] == null)
            {
            }
            else
            {
                visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
            }

            if (HttpContext.Current.Request == null) { }
            else
            {
                if (HttpContext.Current.Request.Url == null) { }
                else
                {
                    _ = HttpContext.Current.Request.Url.ToString();
                }
            }

            var objCheckout = new checkoutDb();

            Int64 checkoutId = Convert.ToInt64(Session["checkoutId"].ToString());

            return await objCheckout.checkoutAttemptSave(checkoutId,
                "Braintree onError function hit",//summaryMessage ?? "",
                "Not Submitted",//gatewayRejectionReason ?? "",
                "",//threeDSecureStatus ?? "",
                "",//threeDSecureEnrolled ?? "",
                false,//liabilityShifted,
                false,//liabilityShiftPossible,
                "",//processorAuthCode ?? "",
                "VisitorID: " + visitorDbId,//processorResponseCode ?? "",
                objError.type,//processorResponseText ?? "",
                objError.message,//additionalProcessorResponse ?? "",
                "",//avsPostalCodeResponseCode ?? "",
                ""//xmlResult ?? "");
            );
        }

        [System.Web.Http.HttpPost]
        public async Task<object> asyncSaveContactReferral(webapiContactReferral objContactReferral)
        {
            checkoutDb objCheckout = new checkoutDb();

            bool success = await objCheckout.saveContactReferral(objContactReferral.transactionId);
            return success;
        }

        private string PrintResult(Result<Transaction> result)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(String.Format("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"));

            PropertyPrinter.PrintProperties(result, 0, sb);

            return sb.ToString();
        }

        private string XmlResult(Result<Transaction> result)
        {
            StringBuilder sb = new StringBuilder();

            XmlWriter xw = XmlWriter.Create(sb);

            xw.WriteStartDocument();

            xw.WriteStartElement("Transaction");

            PropertyPrinter.PrintProperties(result, 5, xw);

            xw.WriteEndDocument();

            xw.Close();

            return sb.ToString();
        }

        private string formatAddressLine(String address, String line)
        {
            if (line == null)
            {
                return address;
            }
            else if (line.Trim() == "")
            {
                return address;
            }
            else
            {
                if (address == "")
                {
                    address = String.Format("{0}", line.Trim());
                }
                else
                {
                    address = String.Format("{0}, {1}", address, line.Trim());
                }
                return address;
            }
        }


        [System.Web.Http.HttpPost]
        public async Task<object> getURLforPaidInFull(webapiCheckoutUrlForPaidInFull objPaidInFull)
        {
            //Determine the date to obtain the data from the cms
            DateTime useDate = DateTime.Now;
            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                //  Should the "use date" be overriden?
                if (HttpContext.Current.Request.QueryString["dateOverride"] == null)
                {
                    useDate = DateTime.Now;
                }
                else
                {
                    String overrideDate = HttpContext.Current.Request.QueryString["dateOverride"].ToString();
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

            var session = HttpContext.Current.Session;

            var checkoutConfirmURL = "";

            checkoutDb objCheckout = new checkoutDb();

            if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
            }
            else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
            }
            //no longer a valid check; we're allowing guest checkouts
            //else if (dgFramework.isUserLoggedIn(session["loginStatus"]) == false)
            //{
            //    dgFramework.raiseErrorNotHandled(webapiUnhandledError.SecurityFail);
            //}
            else
            {
                //  Get the country
                String basketCountry = objPaidInFull.basketCountry.Trim();
                cmsCountry objCountry = new cmsCountry();
                cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(useDate);

                if (dgFramework.doesCountryExistFromText(basketCountry, objCountryData)) { }
                else
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
                }
                countryItem objCountryItem = dgFramework.getCountryFromText(basketCountry, objCountryData);
                
                Int64 checkoutId = Convert.ToInt64(session["checkoutId"].ToString());
                
                //  Get the token
                CheckoutPayment objCheckoutPayments = await objCheckout.checkoutPayGenerate(checkoutId, objCountryItem.CountryID, objCountryItem.LanguageID, "CheckoutController.getURLForPaidInFull");
                if (objCheckoutPayments.checkoutDetails.Count() == 0)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
                }

                //  Validate that the order has been completed by gift vouchers only
                Byte paidInFull = await objCheckout.checkoutPaidInFullCheck(objCheckoutPayments.checkoutDetails[0].GUID, checkoutId);

                paymentType paymentType = paymentType.unknown;

                if (paidInFull == 1) 
                {
                    paymentType = paymentType.giftVoucherFull;
                }
                else if (paidInFull == 2)
                {
                    paymentType = paymentType.tradeOrder;
                }
                else if (paidInFull == 3)
                {
                    paymentType = paymentType.zeroValueRetailOrder;
                }
                else
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
                }

                var success = await objCheckout.checkoutPaymentTaken(checkoutId, paymentType.ToString(), paymentType.ToString(), "<root></root>", objPaidInFull.marketingPermissions, objPaidInFull.orderReference);
                
                //var paySave = await objCheckout.checkoutPaySaveRedux(checkoutId);
                
                if (success)
                {
                    if (!String.IsNullOrEmpty(objPaidInFull.orderReference))
                    {
                        session["tradeOrderReference"] = objPaidInFull.orderReference;
                    }

                    return new
                    {
                        result = "OK",
                        redirect = "orderGen"
                    };
                }
                else
                {
                    return new
                    {
                        result = "FAIL",
                        redirect = "basket"
                    };
                }
                //Boolean success = await objCheckout.checkoutPaySave(
                //    new Guid(objCheckoutPayments.checkoutMemberDetails[0].GUID.ToString().ToUpper()),
                //    paymentType.ToString(),
                //    paymentType.ToString(),
                //    "<root></root>",
                //    objGiftVoucherFull.marketingPermissions);

                // this no longer holds... we need to redirect to the orderGen!

                //orderGenURL = "/api/checkout/checkoutOrderGenerate";
                
            }

            return checkoutConfirmURL;

        }

        [System.Web.Http.HttpPost]
        public async Task<object> checkoutRemoveCard(webapiCheckoutRemoveCard objRemoveCard)
        {
            try
            {
                var session = HttpContext.Current.Session;

                if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
                }
                else if (dgFramework.isCheckoutSessionValid(session["checkoutExpiry"]) == false)
                {
                    return dgFramework.raiseErrorToHandle(webapiDataError.checkoutBasketExpired);
                }
                else if (session["memberIdFromDatabase"] == null)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
                }
                else
                {
                    Int64 checkoutId = Convert.ToInt64(HttpContext.Current.Session["checkoutId"].ToString());

                    checkoutDb objCheckout = new checkoutDb();

                    if (!string.IsNullOrEmpty(objRemoveCard.vaultToken))
                    {
                        // vaulted card
                        var memberId = session["memberIdFromDatabase"].ToString();

                        //  Get the country
                        String basketCountry = objRemoveCard.basketCountry.Trim();
                        cmsCountry objCountry = new cmsCountry();
                        cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(DateTime.Now);

                        if (dgFramework.doesCountryExistFromText(basketCountry, objCountryData)) { }
                        else
                        {
                            dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
                        }
                        countryItem objCountryItem = dgFramework.getCountryFromText(basketCountry, objCountryData);

                        var objCheckoutPayments = await objCheckout.checkoutPayGenerate(checkoutId, objCountryItem.CountryID, objCountryItem.LanguageID, "CheckoutController.checkoutRemoveCard");

                        var gateway = new BraintreeGateway()
                        {
                            Environment = Braintree.Environment.ParseEnvironment(objCheckoutPayments.checkoutDetails[0].Environment),
                            MerchantId = objCheckoutPayments.checkoutDetails[0].MerchantID,
                            PublicKey = objCheckoutPayments.checkoutDetails[0].PublicKey,
                            PrivateKey = objCheckoutPayments.checkoutDetails[0].PrivateKey
                        };

                        // rather than delete the card outright, first check that it belongs to the logged in customer
                        try
                        {
                            var paymentMethod = await gateway.PaymentMethod.FindAsync(objRemoveCard.vaultToken);

                            if (paymentMethod?.CustomerId == memberId)
                            {
                                await gateway.PaymentMethod.DeleteAsync(objRemoveCard.vaultToken);
                            }
                        }
                        catch
                        {
                        }   
                    }
                    else if (!string.IsNullOrEmpty(objRemoveCard.cardToken))
                    {
                        // temporary card
                        await objCheckout.deletePaymentToken(checkoutId, objRemoveCard.cardToken);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, $"checkoutRemoveCard - CheckoutID: {HttpContext.Current.Session["checkoutId"]}, MemberID: {HttpContext.Current.Session["memberIdFromDatabase"]}, VisitorID: {HttpContext.Current.Session["visitorIdFromDatabase"]}");
                throw ex;
            }
        }

        [System.Web.Http.HttpPost]
        public async Task<object> checkoutCheckCVV(webapiCheckoutValidateCVV objValidate)
        {
            try
            {
                var session = HttpContext.Current.Session;

                if (dgFramework.doesUserHaveACheckoutId(session["checkoutId"]) == false)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.checkoutNoBasket);
                }
                else if (session["memberIdFromDatabase"] == null)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
                }
                else if (string.IsNullOrEmpty(objValidate.token) || string.IsNullOrEmpty(objValidate.nonce))
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
                }
                else
                {
                    Int64 checkoutId = Convert.ToInt64(HttpContext.Current.Session["checkoutId"].ToString());

                    checkoutDb objCheckout = new checkoutDb();

                    var memberId = session["memberIdFromDatabase"].ToString();

                    String basketCountry = objValidate.country.Trim();
                    cmsCountry objCountry = new cmsCountry();
                    cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(DateTime.Now);

                    if (dgFramework.doesCountryExistFromText(basketCountry, objCountryData)) { }
                    else
                    {
                        dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
                    }
                    countryItem objCountryItem = dgFramework.getCountryFromText(basketCountry, objCountryData);

                    var objCheckoutPayments = await objCheckout.checkoutPayGenerate(checkoutId, objCountryItem.CountryID, objCountryItem.LanguageID, "CheckoutController.checkoutCheckCVV");

                    var gateway = new BraintreeGateway()
                    {
                        Environment = Braintree.Environment.ParseEnvironment(objCheckoutPayments.checkoutDetails[0].Environment),
                        MerchantId = objCheckoutPayments.checkoutDetails[0].MerchantID,
                        PublicKey = objCheckoutPayments.checkoutDetails[0].PublicKey,
                        PrivateKey = objCheckoutPayments.checkoutDetails[0].PrivateKey
                    };
                    
                    try
                    {
                        var paymentMethod = await gateway.PaymentMethod.FindAsync(objValidate.token);
                        var card = (CreditCard)paymentMethod;
                        PaymentMethodRequest req = new PaymentMethodRequest()
                        {
                            PaymentMethodNonce = objValidate.nonce,
                            Token = objValidate.token,
                            BillingAddress = new PaymentMethodAddressRequest()
                            {
                                PostalCode = card.BillingAddress.PostalCode,
                                CountryCodeAlpha2 = card.BillingAddress.CountryCodeAlpha2
                            },
                            Options = new PaymentMethodOptionsRequest()
                            {
                                VerifyCard = true
                            }
                        };

                        var res = await gateway.PaymentMethod.UpdateAsync(objValidate.token, req);
                        if (res.IsSuccess())
                        {
                            // the supplied nonce matches the vaulted card, return a new payment token to the client     
                            var token = gateway.PaymentMethodNonce.Create(res.Target.Token);

                            if (token.IsSuccess())
                                return new { nonce = token.Target.Nonce, bin = objValidate.bin };
                            else
                                return false;
                        }
                        else
                        {
                            Logger.Info("checkoutCheckCVV verification failed - " + res.Message);
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "checkoutCheckCVV - Verifying");
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, $"checkoutCheckCVV - CheckoutID: {HttpContext.Current.Session["checkoutId"]} MemberID: {HttpContext.Current.Session["memberIdFromDatabase"]}, VisitorID: {HttpContext.Current.Session["visitorIdFromDatabase"]}");
                return false;
            }
        }

        [System.Web.Http.HttpPost]
        public IHttpActionResult receiptViewed([FromBody]passCheckoutId objCheckoutId)
        {
            var objCheckout = new checkoutDb();

            objCheckout.receiptViewed(objCheckoutId.checkoutId);

            return Ok(new { });
        }

        #endregion Controller routines

        public static string RenderViewToString(string controllerName, string viewName, object viewData)
        {
            using (var writer = new StringWriter())
            {
                var routeData = new RouteData();
                routeData.Values.Add("controller", controllerName);
                var fakeRequest = new HttpRequest(null, "http://google.com", null);
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

        public IHttpActionResult UPSLocatorSearch([FromBody]PassUPSLocatorSearch objSearch)
        {
            return Ok(UPSAccessPointLocator.Search(objSearch));
        }
    }

    public class FakeController : ControllerBase { protected override void ExecuteCore() { } }
}