using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Lifestyle;


namespace wwwDesignersGuild.Controllers
{
    public class LifeStyleController : Controller
    {

        //[RequireHttps]
        public async Task<ActionResult> Index(String basketCountry, String productPageId)
        {
            
            DateTime useDate = DateTime.Now;
            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                //  Should the "use date" be overriden?
                if (Request.QueryString["dateOverride"] == null) { }
                else
                {
                    String overrideDate = Request.QueryString["dateOverride"].ToString();
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


            //  Country valid?
            cmsCountry objCountry = new cmsCountry();
            cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(useDate);

            if (dgFramework.doesCountryExistFromText(basketCountry, objCountryData)) { }
            else
            {
                return View("~/Views-New/Shared/Error/_returnToHomeDirect.cshtml");
            }

            countryItem objCountryItem = dgFramework.getCountryFromText(basketCountry, objCountryData);


            //  Get data from cache or database
            if (Int32.TryParse(productPageId, out int useProductPageId) == true) { }
            else
            {
                return View("~/Views-New/Shared/Error/_returnToHomeDirect.cshtml");
            }

            cmsLifestyle objLifeStylePopup = new cmsLifestyle();
            cmsLifestyleGalleryPopupDefinition objLifestyleData = await objLifeStylePopup.loadLifestyleGalleryPopupFromCms(useProductPageId,
                                                                                                                            objCountryItem.LanguageID,
                                                                                                                            objCountryItem.CountryID,
                                                                                                                            Session["memberTradeAccountID"] != null,
                                                                                                                            useDate);


            if (objLifestyleData == null || objLifestyleData.productPageHeaders.Count == 0)
            {
                return View("~/Views-New/Shared/Error/_returnToHomeDirect.cshtml");
            }

            passLifeStylePopup objReturn = new passLifeStylePopup
            {
                basketCountry = basketCountry,
                productPageId = useProductPageId,
                cmsLifestyleGalleryPopupDefinition = objLifestyleData,
                countryItem = objCountryItem,
                contentDate = useDate
            };

            return View(dgFramework.GetPreferredDeviceView("~/Views/LifeStyle/_LifeStylePopup{0}.cshtml"), objReturn);
        }

    }
}