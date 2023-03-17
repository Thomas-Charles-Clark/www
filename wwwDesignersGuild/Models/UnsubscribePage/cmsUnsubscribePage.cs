using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Interface;

namespace wwwDesignersGuild.Models.UnsubscribePage
{
    public class cmsUnsubscribePage : ICmsUnsubscribePageRepository
    {
        public async Task<cmsUnsubscribePageDefinition> loadUnsubscribePageFromCms( byte languageId, short countryId, DateTime useDate, Boolean isMember, Boolean isTrade)
        {
            //  Is page in cache?
            cmsUnsubscribePageDefinition unsubscribePageStore;
            UnsubscribePageCacheProvider objUnsubscribePageCache = new UnsubscribePageCacheProvider();
            unsubscribePageStore = objUnsubscribePageCache.Get(languageId, countryId, useDate);

            if (unsubscribePageStore == null)
            {
                unsubscribePageStore = await Task.FromResult(loadUnsubscribePageFromCmsDatabase(languageId, countryId, useDate, isMember, isTrade));
                objUnsubscribePageCache.Set(languageId, countryId, useDate, unsubscribePageStore);
            }

            return unsubscribePageStore;

        }

        private cmsUnsubscribePageDefinition loadUnsubscribePageFromCmsDatabase(byte languageId, short countryId, DateTime useDate, Boolean isMember, Boolean isTrade)
        {
            using (var dataContext = new databaseDataContext())
            {
                var results = dataContext.Unsubscribe_GenerateMultiple(languageId, countryId, useDate, isMember, isTrade);

                cmsUnsubscribePageDefinition pageStore = new cmsUnsubscribePageDefinition
                {
                    unsubscribeReasons = results.GetResult<unsubscribeReason>().ToList()
                };

                return pageStore;
            }
        }
    }
}