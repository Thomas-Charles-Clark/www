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
            var objCache = new ContactDetailsCacheProvider();

            DatabaseJSONResponse contactDetails = objCache.GetOrCreate(tradeAccountId, languageId, departmentId, () => {

                var result = Task.Run(() => loadTradeAccountContactDetailsFromDatabase(tradeAccountId, languageId, departmentId)).Result;

                return result;

            });

            return contactDetails;
        }

        private DatabaseJSONResponse loadTradeAccountContactDetailsFromDatabase(int tradeAccountId, byte languageId, string departmentId)
        {
            return new visitorDb().fetchTradeDepartmentContact(tradeAccountId, languageId, departmentId);
        }
    }
}