using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Cache.Resources;
using wwwDesignersGuild.Models.Database;

namespace wwwDesignersGuild.Models.Trade
{
    public class cmsTradeAccountContactDetails
    {
        public async Task<DatabaseJSONResponse> loadTradeAccountContactDetailsFromCms(int tradeAccountId, byte languageId, string departmentId)
        {
            _ = new DatabaseJSONResponse();
            var objCache = new ContactDetailsCacheProvider();
            DatabaseJSONResponse result = objCache.Get(tradeAccountId, languageId, departmentId);

            if (result == null)
            {
                result = await Task.FromResult<DatabaseJSONResponse>(loadTradeAccountContactDetailsFromDatabase(tradeAccountId, languageId, departmentId));
                objCache.Set(tradeAccountId, languageId, departmentId, result);
            }

            return result;
        }

        private DatabaseJSONResponse loadTradeAccountContactDetailsFromDatabase(int tradeAccountId, byte languageId, string departmentId)
        {
            return new visitorDb().fetchTradeDepartmentContact(tradeAccountId, languageId, departmentId);
        }
    }
}