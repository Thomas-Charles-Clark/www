using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;

namespace wwwDesignersGuild.Models.BraintreeErrorResponse
{
    public class cmsBraintreeErrorResponse
    {
        public async Task<cmsBraintreeErrorResponseDefinition> loadBraintreeResponseCodesFromCms(byte languageId, DateTime useDate)
        {
            // Is page in cache?
            var btStore = new cmsBraintreeErrorResponseDefinition();
            var btCache = new BraintreeErrorResponseCacheProvider();
            btStore.errorResponseSelects = btCache.Get(languageId, useDate);

            if (btStore.errorResponseSelects == null)
            {
                btStore = await Task.FromResult<cmsBraintreeErrorResponseDefinition>(loadBraintreeErrorResponsesFromCmsDatabase(languageId, useDate));
                btCache.Set(languageId, useDate, btStore.errorResponseSelects);
            }

            return btStore;
        }

        private cmsBraintreeErrorResponseDefinition loadBraintreeErrorResponsesFromCmsDatabase(byte languageId, DateTime useDate)
        {
            var dataContext = new databaseDataContext();

            IMultipleResults errorResults = dataContext.Braintree_ErrorResponse_Select(languageId, useDate);

            var responsesStore = new cmsBraintreeErrorResponseDefinition
            {
                errorResponseSelects = errorResults.GetResult<braintreeErrorResponseSelect>().ToList()
            };

            return responsesStore;
        }
    }
}