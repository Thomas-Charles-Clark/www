using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Globalization;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Promotion;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Util;

namespace wwwDesignersGuild.Controllers
{
    public class PromotionController : ApiController
    {
        #region Controller routines

        //  Add
        [HttpPost]
        public async Task<promotionDetailReturn> AddPromotionCode(webapiPromotionAdd objPromotionAdd)
        {
            var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

            if (objPromotionAdd == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }


            String promotionCode = objPromotionAdd.promotionCode.Trim();
            String basketCountry = objPromotionAdd.basketCountry.Trim();

            
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
            
            Int32? tradeAccountId = null;
            
            if (Convert.ToBoolean(session["memberTradeChecksPassed"]) == true)
            {
                if (session["memberTradeAccountID"] != null)
                {
                    tradeAccountId = Convert.ToInt32(session["memberTradeAccountID"]);
                }
            }

            bool displayTradePrice = false;

            if (Convert.ToBoolean(session["memberDisplayTradePrice"]) == true)
            {
                displayTradePrice = true;
            }

            promotionDetailReturn objReturn = new promotionDetailReturn();
            promotionRoutines objPromotion = new promotionRoutines();

            //  Is the promotion code valid?
            passPromotionCheck objPromotionValid = await objPromotion.callCheckPromotionCode(promotionCode, useDate, objCountryItem.CountryID, objCountryItem.LanguageID, visitorDbId, memberId, tradeAccountId, displayTradePrice);

            objReturn.passPromotionCheck = objPromotionValid;

            return objReturn;
        }


        //  Delete
        [HttpPost]
        public async Task<IHttpActionResult> DeletePromotionCode(webapiPromotionDelete objPromotionDelete)
        {
            var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

            if (objPromotionDelete == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }


            short promotionCodeId = objPromotionDelete.promotionCodeId;
            String basketCountry = objPromotionDelete.basketCountry.Trim();


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
            var Session = HttpContext.Current.Session;
            Int64 visitorDbId = -1;
            if (Session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
            }


            Int32? memberId = null;
            if (Session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(Session["memberIdFromDatabase"].ToString());
            }

            promotionRoutines objPromotion = new promotionRoutines();
            _ = await objPromotion.callDeletePromotionCode(visitorDbId, promotionCodeId, objCountryItem.CountryID);

            var objBasket = new basketRoutines();

            var results = await objBasket.loadBasketDetail(
                            Convert.ToInt64(Session["visitorIdFromDatabase"]),
                            (int?)Session["memberIdFromDatabase"],
                            objPromotionDelete.languageId,
                            objPromotionDelete.countryId,
                            useDate,
                            (int?)Session["memberTradeAccountID"],
                            Session["memberTradeAccountID"] != null,
                            Convert.ToBoolean(Session["memberDisplayTradePrice"] ?? "false")
                        );

            return Ok(results);
        }

        #endregion
    }
}
