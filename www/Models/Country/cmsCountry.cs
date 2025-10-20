using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Interface;


namespace wwwDesignersGuild.Models.Country
{


    public class cmsCountry : ICmsCountryRepository
    {


        #region Load All Countries


        public async Task<cmsCountryDefinition> countryLanguageSelect(DateTime contentDate)
        {

            //  Is page in cache?
            cmsCountryDefinition countryStore;
            CountryCacheProvider objCountryCache = new CountryCacheProvider();
            countryStore = objCountryCache.Get();

            if (countryStore == null)
            {
                countryStore = await Task.FromResult(countryLanguageSelectAsync(contentDate));
                objCountryCache.Set(countryStore);
            }

            return countryStore;

        }

        private cmsCountryDefinition countryLanguageSelectAsync(DateTime contentDate)
        {


            var dataContext = new databaseDataContext();


            IMultipleResults results = dataContext.Country_Language_Select(contentDate);


            //  Convert from IEnumerable to a List so a collection query can be run multiple times
            cmsCountryDefinition countryStore = new cmsCountryDefinition
            {
                countryRegions = results.GetResult<countryRegion>().ToList(),
                countryItems = results.GetResult<countryItem>().ToList(),
                countryTradePriceLists = results.GetResult<countryTradePriceList>().ToList()
            };


            return countryStore;


        }


        #endregion

        #region Load Countries for DropDown


        public async Task<cmsCountryDropDownDefinition> countryDropDownSelectList(DateTime contentDate, byte languageId)
        {

            //  Is page in cache?
            cmsCountryDropDownDefinition countryDropDownStore;
            CountryDropDownCacheProvider objCountryDropDownCache = new CountryDropDownCacheProvider();
            countryDropDownStore = objCountryDropDownCache.Get();

            if (countryDropDownStore == null)
            {
                countryDropDownStore = await Task.FromResult<cmsCountryDropDownDefinition>(countryDropDownSelectListAsync(contentDate, languageId));
                objCountryDropDownCache.Set(countryDropDownStore);
            }

            return countryDropDownStore;

        }

        private cmsCountryDropDownDefinition countryDropDownSelectListAsync(DateTime contentDate, byte languageId)
        {


            var dataContext = new databaseDataContext();


            IMultipleResults results = dataContext.CountryDropDown_SelectList(contentDate, languageId);


            //  Convert from IEnumerable to a List so a collection query can be run multiple times
            cmsCountryDropDownDefinition countryDropDownStore = new cmsCountryDropDownDefinition
            {
                countryDropDownItems = results.GetResult<countryDropDownItem>().ToList<countryDropDownItem>()
            };

            return countryDropDownStore;
        }

        #endregion

        #region Load Countries from Price List


        public async Task<cmsCountryPriceListDefinition> countrySelectList(short? countryID, byte? languageId)
        {
            cmsCountryPriceListDefinition countryStore = await Task.FromResult<cmsCountryPriceListDefinition>(countrySelectListAsync(countryID, languageId));
            return countryStore;
        }
        
        private cmsCountryPriceListDefinition countrySelectListAsync(short? countryID, byte? languageId)
        {
            var dataContext = new databaseDataContext();
            
            IMultipleResults results = dataContext.Country_SelectList(countryID, languageId);

            //  Convert from IEnumerable to a List so a collection query can be run multiple times
            cmsCountryPriceListDefinition countryStore = new cmsCountryPriceListDefinition
            {
                countries = results.GetResult<Classes.Country>().ToList(),
                counties = results.GetResult<CountyItem>().ToList()
            };

            return countryStore;
        }

        public async Task<cmsLanguageDefinition> loadCompleteLanguageList(short? countryId, DateTime date)
        {
            var countryStore = await Task.FromResult<cmsLanguageDefinition>(loadCompleteLanguageListAsync(countryId,date));
            return countryStore;
        }

        private cmsLanguageDefinition loadCompleteLanguageListAsync(short? countryId, DateTime date)
        {
            var dataContext = new databaseDataContext();

            IMultipleResults results = dataContext.Language_SelectCompleteList(countryId, date);

            var languageStore = new cmsLanguageDefinition
            {
                languages = results.GetResult<Language>().ToList()
            };

            return languageStore;
        }


        #endregion


    }


}