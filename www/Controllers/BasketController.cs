using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Threading;
using System.Globalization;
using System.Web;
using System.Data;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Checkout;
using System.Web.Http.Results;
using wwwDesignersGuild.Models.Visitor;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Hotspot;
using wwwDesignersGuild.Util;

namespace wwwDesignersGuild.Controllers
{
    public class BasketController : ApiController
    {
        #region Controller routines


        [HttpPost]
        public async Task<object> AddToBasket(PassBasketAdd objBasketAdd)
        {
            var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));
         
            #region Sanity Checks

            objBasketAdd.basketCountry = objBasketAdd.basketCountry.Trim();

            if (objBasketAdd == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters, "objBasketAdd was null");
            }

            //  Note:   This condition has now replaced by the dynamic check
            //if ((quantity >= 1) && (quantity <= 99)) { }
            //if (objBasketAdd.quantity >= 0.5) { }
            //else
            //{
            //    dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidQuantityRange);
            //}

            cmsCountry objCountry = new cmsCountry();
            cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(useDate);

            if (dgFramework.doesCountryExistFromText(objBasketAdd.basketCountry, objCountryData)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry, "country did not exist from text");
            }

            countryItem objCountryItem = dgFramework.getCountryFromText(objBasketAdd.basketCountry, objCountryData);


            //  Does the country allow basket functionality
            if (objCountryItem.ShopEnabled == true) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountryBasket, $"shopping not permitted in {objBasketAdd.basketCountry}");
            }
            #endregion

            //  Get the database user id from the session
            var session = HttpContext.Current.Session;
            Int64 visitorDbId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter, $"visitorIdFromDatabase was null!");
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            Int32? memberId = null;
            if (session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
            }

            //  Set the resource file language
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(objCountryItem.DotNetCulture);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            //  Validation Start
            basketRoutines objBasket = new basketRoutines();

            AddToBasketReturn objReturn = null;

            try
            {
                objReturn = await objBasket.callAddToBasket(
                   visitorDbId,
                   memberId,
                   objBasketAdd.priceListId,
                   (int?)HttpContext.Current.Session["memberTradeAccountID"],
                   objBasketAdd.productId,
                   objBasketAdd.quantity,
                   objCountryItem.CountryID,
                   objCountryItem.LanguageID,
                   useDate,
                   objBasketAdd.productPageId,
                   objBasketAdd.basketLineTypeId ?? 1);

                var itemString = HtmlHelperExtra.Resource(objReturn.totalDisplayQuantity == 1 ? "item" : "items", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString();
                
                var currentlyInYourBasketString = HtmlHelperExtra.Resource(objReturn.quantity == 1 ? "ShoppingProductInBasket" : "ShoppingProductsInBasket", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString();

                var trimmedUnit = objReturn.stockUnit.Trim();

                if (trimmedUnit == "roll(s)" ||
                    trimmedUnit == "rouleaux" ||
                    trimmedUnit == "rollen" ||
                    trimmedUnit == "rotoli" ||
                    trimmedUnit == "рулонов")
                {
                    objReturn.stockUnit = HtmlHelperExtra.Resource(objReturn.quantity == 1 ? "RollLowerCase" : "RollsLowerCase", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString();
                }

                objReturn.quantityDisplay = $"({objReturn.totalDisplayQuantity?.ToString("0.##") + " " + itemString})";

                if (objReturn.stockUnitId == 0)
                {
                    objReturn.stockUnit = null;// HtmlHelperExtra.Resource("Are", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString();
                }

                var spacer = " ";

                if (objReturn.stockUnit == "m" ||
                    objReturn.stockUnit == "m²")
                {
                    spacer = "";
                }

                objReturn.basketLineText = $"{objReturn.quantity:0.##}{spacer}{objReturn.stockUnit} {currentlyInYourBasketString}";

            }
            catch (Exception e)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters, $"{e}");
            }

            return objReturn;
        }


        [HttpPost]
        public async Task<IHttpActionResult> UpdateBasket(PassBasketUpdate objBasketUpdate)
        {
            if (objBasketUpdate == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            //  Has any future date been passed
            var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

            var basketCountry = objBasketUpdate.basketCountry.Trim();

            var objCountry = new cmsCountry();
            var objCountryData = await objCountry.countryLanguageSelect(useDate);

            if (dgFramework.doesCountryExistFromText(basketCountry, objCountryData)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
            }

            countryItem objCountryItem = dgFramework.getCountryFromText(basketCountry, objCountryData);


            //  Does the country allow basket functionality
            if (objCountryItem.ShopEnabled == true) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountryBasket);
            }


            //  Get the database user id from the session
            var session = HttpContext.Current.Session;
            Int64 visitorDbId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            Int32? memberId = null;
            if (session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
            }

            basketRoutines objBasket = new basketRoutines();

            //  Update the product in the basket
            if (objBasketUpdate.productId.HasValue &&
                objBasketUpdate.basketLineTypeId.HasValue &&
                objBasketUpdate.quantity.HasValue &&
                objBasketUpdate.savedForLaterFlag.HasValue)
            {
                _ = await objBasket.callUpdateToBasket(visitorDbId,
                    memberId,
                    objBasketUpdate.productId.Value,
                    objBasketUpdate.basketLineTypeId.Value,
                    objCountryItem.CountryID,
                    objBasketUpdate.quantity.Value,
                    objBasketUpdate.savedForLaterFlag.Value);
            }

            var basketValidationDetails = await objBasket.loadBasketDetail(
                visitorDbId,
                memberId,
                objCountryItem.LanguageID,
                objCountryItem.CountryID,
                useDate,
                (int?)HttpContext.Current.Session["memberTradeAccountID"],
                HttpContext.Current.Session["memberTradeAccountID"] != null,
                HttpContext.Current.Session["memberDisplayTradePrice"] != null && Convert.ToBoolean(HttpContext.Current.Session["memberDisplayTradePrice"])
                );

            //basketValidationDetails.SummaryMessage = dgFramework.displayBasketSummaryText(basketValidationDetails.Totals[0].TotalQuantity,
            //                                                            objBasketUpdate.objMaster.UrlBreakdown.countryDetails,
            //                                                            objBasketUpdate.objMaster.UrlBreakdown.ContentDate);

            //basketValidationDetails.SummaryTotal = basketValidationDetails.Totals[0].TotalQuantity;

            return Ok(basketValidationDetails);
        }

        [HttpPost]
        public async Task<decimal?> GetNumBasketItems(PassBasketSummary objBasketSummary)
        {
            if (objBasketSummary == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }


            //  Has an future date been passed
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


            String basketCountry = objBasketSummary.basketCountry.Trim();


            cmsCountry objCountry = new cmsCountry();
            cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(useDate);

            if (dgFramework.doesCountryExistFromText(basketCountry, objCountryData)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
            }

            countryItem objCountryItem = dgFramework.getCountryFromText(basketCountry, objCountryData);


            //  Does the country allow basket functionality
            if (objCountryItem.ShopEnabled == true) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountryBasket);
            }


            //  Get the database user id from the session
            var session = HttpContext.Current.Session;
            Int64 visitorDbId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            Int32? memberId = null;
            if (session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
            }

            //  Get the basket summary
            basketRoutines objBasket = new basketRoutines();
            BasketSummary basketSummaryData = await objBasket.loadLatestBasket(visitorDbId, memberId, (Int32?)session["memberTradeAccountID"],
                HttpContext.Current.Session["memberDisplayTradePrice"] != null && Convert.ToBoolean(HttpContext.Current.Session["memberDisplayTradePrice"]),
                objCountryItem.LanguageID, objCountryItem.CountryID, useDate);

            //  Get total quantity
            BasketTotal basketTotalQuantities = basketSummaryData.basketTotals.FirstOrDefault();
            decimal? numBasketItems = basketTotalQuantities == null ? 0 : basketTotalQuantities.TotalQuantity;

            return numBasketItems;
        }


        [HttpPost]
        public async Task<basketSummaryReturn> GetBasketSummary(PassBasketSummary objBasketSummary)
        {


            if (objBasketSummary == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }


            //  Has an future date been passed
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


            String basketCountry = objBasketSummary.basketCountry.Trim();


            cmsCountry objCountry = new cmsCountry();
            cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(useDate);

            if (dgFramework.doesCountryExistFromText(basketCountry, objCountryData)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
            }

            countryItem objCountryItem = dgFramework.getCountryFromText(basketCountry, objCountryData);


            //  Does the country allow basket functionality
            if (objCountryItem.ShopEnabled == true) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountryBasket);
            }


            //  Get the database user id from the session
            var session = HttpContext.Current.Session;
            Int64 visitorDbId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            Int32? memberId = null;
            if (session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
            }

            var objBasket = new basketRoutines();

            return await buildSummaryReturnPacket(visitorDbId, memberId, objCountryItem, useDate, objCountryItem.DotNetCulture);
        }


        [HttpPost]
        public async Task<basketUpdateReturn> SaveForLaterToggle(PassSaveForLater objBasketSave)
        {


            if (objBasketSave == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }


            //  Has an future date been passed
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


            Int32 productId = objBasketSave.productCode;
            byte basketLineTypeId = objBasketSave.basketLineTypeId;
            String basketCountry = objBasketSave.basketCountry.Trim();
            Boolean saveForLater = objBasketSave.savedForLaterFlag;  //  1 = save for later, 0 = move to basket


            cmsCountry objCountry = new cmsCountry();
            cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(useDate);

            if (dgFramework.doesCountryExistFromText(basketCountry, objCountryData)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
            }

            countryItem objCountryItem = dgFramework.getCountryFromText(basketCountry, objCountryData);


            //  Does the country allow basket functionality
            if (objCountryItem.ShopEnabled == true) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountryBasket);
            }


            //  Get the database user id from the session
            var session = HttpContext.Current.Session;
            Int64 visitorDbId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            Int32? memberId = null;
            if (session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
            }

            //  Toggle the save for later
            basketRoutines objBasket = new basketRoutines();
            Boolean tempSuccess = await objBasket.callSaveForLaterBasket(visitorDbId, memberId, productId, basketLineTypeId, objCountryItem.CountryID, saveForLater);

            var basketValidationDetails = await objBasket.loadBasketDetail(
                visitorDbId,
                memberId,
                objCountryItem.LanguageID,
                objCountryItem.CountryID,
                useDate,
                (int?)HttpContext.Current.Session["memberTradeAccountID"],
                HttpContext.Current.Session["memberTradeAccountID"] != null,
                HttpContext.Current.Session["memberDisplayTradePrice"] != null && Convert.ToBoolean(HttpContext.Current.Session["memberDisplayTradePrice"])
                );

            return buildUpdateReturnPacket(visitorDbId, memberId, objCountryItem, useDate, objCountryItem.DotNetCulture, productId, basketLineTypeId, basketValidationDetails);
        }

        [HttpPost]
        public async Task<object> DotDigitalJSON(masterDefinition objMaster)
        {
            var objBasket = new basketRoutines();

            Int32? memberId = null;
            if (HttpContext.Current.Session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = (Int32?)HttpContext.Current.Session["memberIdFromDatabase"];
            }

            Int64 visitorDbId = Convert.ToInt64(HttpContext.Current.Session["visitorIdFromDatabase"].ToString());

            bool checkoutConfirmation = false;

            if (objMaster.UrlBreakdown.ContentType == dgContentType.landingPage &&
                (objMaster.UrlBreakdown.ContentId == (int)dgFixedPage.checkoutConfirmation ||
                 objMaster.UrlBreakdown.ContentId == (int)dgFixedPage.checkoutConfirmationSpartan))
            {
                checkoutConfirmation = true;
            }

            var r = await objBasket.callGetDotDigitalJSON(
                visitorDbId,
                memberId,
                objMaster.UrlBreakdown.countryDetails.CountryID,
                objMaster.UrlBreakdown.countryDetails.LanguageID,
                checkoutConfirmation);

            return Ok(r.JSON);
        }


        [HttpPost]
        public async Task<object> FetchDotDigitalJSON(urlBreakdown objBreakdown)
        {
            var objBasket = new basketRoutines();

            Int32? memberId = null;
            if (HttpContext.Current.Session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = (Int32?)HttpContext.Current.Session["memberIdFromDatabase"];
            }

            Int64 visitorDbId = Convert.ToInt64(HttpContext.Current.Session["visitorIdFromDatabase"].ToString());

            bool checkoutConfirmation = false;

            if (objBreakdown.ContentType == dgContentType.landingPage &&
                (objBreakdown.ContentId == (int)dgFixedPage.checkoutConfirmation ||
                 objBreakdown.ContentId == (int)dgFixedPage.checkoutConfirmationSpartan))
            {
                checkoutConfirmation = true;
            }

            var r = await objBasket.callGetDotDigitalJSON(
                visitorDbId,
                memberId,
                objBreakdown.countryDetails.CountryID,
                objBreakdown.countryDetails.LanguageID,
                checkoutConfirmation);

            return Ok(r.JSON);
        }

        [HttpPost]
        public async Task<object> BrontoJSON(countryItem objInput)
        {
            var objBasket = new basketRoutines();

            Int32? memberId = null;
            if (HttpContext.Current.Session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = (Int32?)HttpContext.Current.Session["memberIdFromDatabase"];
            }

            Int64 visitorDbId = Convert.ToInt64(HttpContext.Current.Session["visitorIdFromDatabase"].ToString());

            var objPreReturn = await objBasket.callGetBrontoJSON(
                visitorDbId,
                memberId,
                null, // PageID - for identifying checkout stages. This POST function will not be called from within checkout.
                null, // CheckoutID "
                objInput.CountryID,
                objInput.LanguageID,
                (Int32?)HttpContext.Current.Session["memberTradeAccountID"],
                DateTime.Now
            );

            var objReturn = new brontoJsonPostReturn()
            {
                cartUrl = objPreReturn.Header.CartURL,
                grandTotal = objPreReturn.Header.GrandTotal,
                currency = objPreReturn.Header.Currency,
                subtotal = objPreReturn.Header.SubTotal,
                discountAmount = objPreReturn.Header.DiscountAmount,
                taxAmount = objPreReturn.Header.TaxAmount,
                email = objPreReturn.Header.Email,
                lineItems = objPreReturn.Lines.Select(p => new brontoJsonPostReturnBasketLine
                {
                    sku = p.SKU,
                    name = p.Name,
                    description = p.Description,
                    category = p.Category,
                    unitPrice = p.UnitPrice,
                    salePrice = p.SalePrice ?? p.UnitPrice,
                    quantity = p.Quantity,
                    totalPrice = p.TotalPrice,
                    imageUrl = p.ImageURL,
                    productUrl = p.ProductURL
                }).ToList()
            };

            return objReturn;
        }
        #endregion 


        #region Helper Routines

        public async Task<basketSummaryReturn> buildSummaryReturnPacket(Int64 visitorDbId, Int32? memberId, countryItem objCountry, DateTime useDate, String DotNetCulture)
        {

            //  Get the basket summary
            basketRoutines objBasket = new basketRoutines();
            BasketSummary basketSummaryData = null;

            try
            {
                basketSummaryData = await objBasket.loadLatestBasket(visitorDbId, memberId, (Int32?)HttpContext.Current.Session["memberTradeAccountID"],
                    HttpContext.Current.Session["memberDisplayTradePrice"] != null && Convert.ToBoolean(HttpContext.Current.Session["memberDisplayTradePrice"]),
                    objCountry.LanguageID, objCountry.CountryID, useDate);
            }
            catch (Exception e)
            {
                throw e;
            }

            //  Set the image url
            foreach (BasketDetail objBasketLine in basketSummaryData.basketDetails)
            {
                objBasketLine.ImageUrl = Url.Route("ImageRoutine", new { width = dgFramework.getImageWidth(dgImageSize.marker), imageId = objBasketLine.ImageID.ToString() });
                
                String dateOverride = null;
                var queryStrings = Request.GetQueryNameValuePairs();
                var match = queryStrings.FirstOrDefault(kv => string.Compare(kv.Key, "dateOverride", true) == 0);
                if (string.IsNullOrEmpty(match.Value)) { }
                else
                {
                    dateOverride = match.Value;
                }
                String countryOverride = null;
                match = queryStrings.FirstOrDefault(kv => string.Compare(kv.Key, "countryOverride", true) == 0);
                if (string.IsNullOrEmpty(match.Value)) { }
                else
                {
                    countryOverride = match.Value;
                }
            }


            //  Get the stock status for each product code
            //basketProductStockSelect storeProductStockSelect = null;
            //if (productCode == -1) { }
            //else
            //{
            //    Boolean saveForLater = false;
            //    storeProductStockSelect = await objBasket.getProductStockStatus(visitorDbId, memberId, productCode, objCountry.CountryID, saveForLater);
            //}

            ////Get the promotional message
            ////storePromotionalMessage storePromotionalMessage = await objBasket.getPromotionalMessage(visitorDbId, objCountry.LanguageID, objCountry.CountryID, useDate);
            String promotionalDisplayMessage = "";
            if (!String.IsNullOrEmpty(basketSummaryData.basketTotals[0].PromotionalMessage))
            {
                promotionalDisplayMessage = basketSummaryData.basketTotals[0].PromotionalMessage;
            }

            //  Set the resource file language
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(DotNetCulture);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;


            basketSummaryReturn objReturn = new basketSummaryReturn
            {
                basketSummaryData = basketSummaryData,
                //objReturn.basketStockSelect = storeProductStockSelect;
                promotionalMessage = promotionalDisplayMessage,
                itemsCountMessage = String.Format(
                HtmlHelperExtra.Resource("BasketSummaryItemMessage", objCountry.LanguageID, objCountry.CountryID, useDate).ToString(),
                basketSummaryData.basketTotals[0].TotalQuantity.HasValue ? basketSummaryData.basketTotals[0].TotalQuantity.Value.ToString("0.##") : "0"
            ),
                quantityDisplay = dgFramework.displayBasketSummaryText(basketSummaryData.basketTotals[0].TotalQuantity, objCountry, useDate)
            };

            return objReturn;
        }


        public basketUpdateReturn buildUpdateReturnPacket(Int64 visitorDbId, Int32? memberId, countryItem objCountry, DateTime useDate, String DotNetCulture, Int32 productID, byte basketLineTypeId, Basket objBasketGenerate)
        {

            //  Get the basket summary
            basketRoutines objBasket = new basketRoutines();

            //  Set the image url
            foreach (BasketDetail objBasketLine in objBasketGenerate.BasketItems)
            {
                objBasketLine.ImageUrl = Url.Route("ImageRoutine", new { width = dgFramework.getImageWidth(dgImageSize.marker), imageId = objBasketLine.ImageID.ToString() });
                
                String dateOverride = null;
                var queryStrings = Request.GetQueryNameValuePairs();
                var match = queryStrings.FirstOrDefault(kv => string.Compare(kv.Key, "dateOverride", true) == 0);
                if (string.IsNullOrEmpty(match.Value)) { }
                else
                {
                    dateOverride = match.Value;
                }
                String countryOverride = null;
                match = queryStrings.FirstOrDefault(kv => string.Compare(kv.Key, "countryOverride", true) == 0);
                if (string.IsNullOrEmpty(match.Value)) { }
                else
                {
                    countryOverride = match.Value;
                }
            }

            String promotionalDisplayMessage = "";
            if (objBasketGenerate.PromotionalMessages.Count == 0) { }
            else
            {
                promotionalDisplayMessage = objBasketGenerate.PromotionalMessages[0].Message;//storePromotionalMessage.promotionalMessages[0].Message;
            }

            //  Set the resource file language
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(DotNetCulture);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            basketUpdateReturn objReturn = new basketUpdateReturn
            {
                basketTotals = objBasketGenerate.Totals,
                lineDetail = objBasketGenerate.BasketItems.Where(p => p.BasketLineTypeID == basketLineTypeId && p.ProductID == productID).SingleOrDefault(),
                promotionalMessage = promotionalDisplayMessage,
                itemsCountMessage = String.Format(
                HtmlHelperExtra.Resource("BasketSummaryItemMessage", objCountry.LanguageID, objCountry.CountryID, useDate).ToString(),
                objBasketGenerate.Totals[0].TotalQuantity.HasValue ? objBasketGenerate.Totals[0].TotalQuantity.Value.ToString("0.##") : "0"
            ),
                quantityDisplay = dgFramework.displayBasketSummaryText(objBasketGenerate.Totals[0].TotalQuantity, objCountry, useDate)
            };

            return objReturn;
        }


        [HttpPost]
        public async Task<IHttpActionResult> refreshBasket([FromBody]NewMasterDefinition objMaster)
        {
            //  Validation Start
            var br = new basketRoutines();

            Int64 visitorDbId = -1;
            if (HttpContext.Current.Session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(HttpContext.Current.Session["visitorIdFromDatabase"].ToString());
            }

            Int32? memberId = null;
            if (HttpContext.Current.Session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = (Int32?)HttpContext.Current.Session["memberIdFromDatabase"];
            }

            //  Get current quantity in basket for product code via "loadLatestBasket"
            var objBasket = await br.loadBasketDetailQuick(
                visitorDbId,
                memberId,
                objMaster.UrlBreakdown.countryDetails.LanguageID,
                objMaster.UrlBreakdown.countryDetails.CountryID,
                objMaster.UrlBreakdown.ContentDate,
                (int?)HttpContext.Current.Session["memberTradeAccountID"],
                ((int?)HttpContext.Current.Session["memberTradeAccountID"]).HasValue,
                HttpContext.Current.Session["memberDisplayTradePrice"] != null && Convert.ToBoolean(HttpContext.Current.Session["memberDisplayTradePrice"]));

            return Ok(new
            {
                basketItems = objBasket.basketDetails.OrderBy(p => p.SortOrder),
                summaryTotal = objBasket.basketTotals[0].TotalQuantity,
                summaryMessage = dgFramework.displayBasketSummaryText(objBasket.basketTotals[0].TotalQuantity,
                                                                        objMaster.UrlBreakdown.countryDetails,
                                                                        objMaster.UrlBreakdown.ContentDate),
                promotionalMessage = objBasket.basketTotals[0].PromotionalMessage
            });
        }
        #endregion
    }
}