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
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Stockists;
using wwwDesignersGuild.Models.StockistsContract;


namespace wwwDesignersGuild.Controllers
{
    public class StockistsContractController : ApiController
    {


        #region Controller routines


        [HttpPost]
        public async Task<stockistsContractSearchSelect> GetStockistsContractData(webapiStockistContractRequest stockistContractRequest)
        {
            //  Has a future date been passed
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


            //  Get the language of the basket country
            String basketCountry = stockistContractRequest.basketCountry.Trim();
            cmsCountry objCountry = new cmsCountry();
            cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(useDate);

            if (dgFramework.doesCountryExistFromText(basketCountry, objCountryData)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
            }

            countryItem objCountryItem = dgFramework.getCountryFromText(basketCountry, objCountryData);

            

            
            //  Validate the selected stockist country id
            Stockist objStockistCountry = new Stockist();
            StockistCountrySelect objStockistCountryData = await objStockistCountry.getStockistCountries(useDate);

            Boolean stockistCountryFound = false;
            foreach (StockistCountry stockistCountry in objStockistCountryData.stockistCountries)
            {
                if (stockistCountry.CountryID == stockistContractRequest.selectedCountry)
                {
                    stockistCountryFound = true;
                    break;
                }
            }

            if (stockistCountryFound == true) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidStockistsContractCountry);
            }


            //  Get contract stockist details
            StockistsContract objStockistsContract = new StockistsContract();
            stockistsContractSearchSelect stockistsContractData = await objStockistsContract.stockistContractSearch(stockistContractRequest.selectedCountry,
                                                                                                                        useDate,
                                                                                                                        objCountryItem.LanguageID);


            return stockistsContractData;


        }


        #endregion


    }
}
