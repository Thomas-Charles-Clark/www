using System;
using System.Data.Linq;
using System.Linq;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Resource;
using wwwDesignersGuild.ViewModels.Trade;
using wwwDesignersGuild.ViewModels;

namespace wwwDesignersGuild.Models.Trade
{
    public class cmsTradeAccountRequest
    {
        public async Task<TradeAccountRequestViewModel> loadTradeAccountRequestFormFromCms(BaseViewModel objBase)
        {

            //  Is page in cache?
            TradeAccountRequestViewModel store;
            TradeAccountRequestFormCacheProvider objTradeAccountRequestFormCache = new TradeAccountRequestFormCacheProvider();
            store = objTradeAccountRequestFormCache.Get(objBase.CountryID, objBase.LanguageID, objBase.Date);

            if (store == null)
            {
                store = await Task.FromResult<TradeAccountRequestViewModel>(loadTradeAccountRequestFormFromCmsDatabase(objBase));

                objTradeAccountRequestFormCache.Set(objBase.CountryID, objBase.LanguageID, objBase.Date, store);
            }

            return store;
        }

        private TradeAccountRequestViewModel loadTradeAccountRequestFormFromCmsDatabase(BaseViewModel objBase)
        {
            var dataContext = new databaseDataContext();
            
            IMultipleResults results = dataContext.TradeAccountRequestForm_Select(objBase.CountryID, objBase.LanguageID, objBase.Date);

            var tradeAccountRequestForm = new TradeAccountRequestViewModel
            {
                availableCountries = results.GetResult<Classes.Country>().ToList(),
                availableCounties = results.GetResult<CountyItem>().ToList(),
                availableTitles = results.GetResult<titleItem>().ToList(),
                companyTypes = results.GetResult<companyType>().ToList(),
                tradingForOptions = new System.Collections.Generic.List<byteSelectOption>(),
                productTypeGroups = results.GetResult<productTypeGroup_Brand_Country>().ToList(),
                channels = results.GetResult<channel>().ToList(),
                contactTypes = results.GetResult<leadContactType>().ToList(),
                availableLanguages = results.GetResult<MertexLanguage>().ToList()
            };

            var res = new cmsResource().loadResourceFromCms(
                objBase.LanguageID,
                objBase.CountryID,
                objBase.Date);

            var openingSoon = res.Result.resourceSelects.Where(p => p.Name == "TradingOpeningSoon").Select(p => p.Value).SingleOrDefault();
            var under1Year = res.Result.resourceSelects.Where(p => p.Name == "TradingUnder1Year").Select(p => p.Value).SingleOrDefault();
            var oneYear = res.Result.resourceSelects.Where(p => p.Name == "Trading1Year").Select(p => p.Value).SingleOrDefault();
            var twoYears = res.Result.resourceSelects.Where(p => p.Name == "Trading2Years").Select(p => p.Value).SingleOrDefault();
            var threeYears = res.Result.resourceSelects.Where(p => p.Name == "Trading3Years").Select(p => p.Value).SingleOrDefault();
            var fourYears = res.Result.resourceSelects.Where(p => p.Name == "Trading4Years").Select(p => p.Value).SingleOrDefault();
            var fiveYears = res.Result.resourceSelects.Where(p => p.Name == "Trading5Years").Select(p => p.Value).SingleOrDefault();
            var fivePlusYears = res.Result.resourceSelects.Where(p => p.Name == "Trading5YearsPlus").Select(p => p.Value).SingleOrDefault();

            tradeAccountRequestForm.tradingForOptions.Add(new byteSelectOption()
            {
                Name = openingSoon,
                Value = 0
            });
            tradeAccountRequestForm.tradingForOptions.Add(new byteSelectOption()
            {
                Name = under1Year,
                Value = 1
            });
            tradeAccountRequestForm.tradingForOptions.Add(new byteSelectOption()
            {
                Name = oneYear,
                Value = 2
            });
            tradeAccountRequestForm.tradingForOptions.Add(new byteSelectOption()
            {
                Name = twoYears,
                Value = 3
            });
            tradeAccountRequestForm.tradingForOptions.Add(new byteSelectOption()
            {
                Name = threeYears,
                Value = 4
            });
            tradeAccountRequestForm.tradingForOptions.Add(new byteSelectOption()
            {
                Name = fourYears,
                Value = 5
            });
            tradeAccountRequestForm.tradingForOptions.Add(new byteSelectOption()
            {
                Name = fiveYears,
                Value = 6
            });
            tradeAccountRequestForm.tradingForOptions.Add(new byteSelectOption()
            {
                Name = fivePlusYears,
                Value = 7
            });


            tradeAccountRequestForm.BaseViewModel = objBase;

            return tradeAccountRequestForm;
        }

        public async Task<bool> isImmediateOrderFlagSet(long leadId)
        {
            return await Task.FromResult<bool>(loadImmediateOrderFlagSetFromCmsDatabase(leadId));
        }

        private bool loadImmediateOrderFlagSetFromCmsDatabase(long leadId)
        {
            var context = new databaseDataContext();

            return context.isImmediateOrderFlagSet(leadId);
        }
    }
}