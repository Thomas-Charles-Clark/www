using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Interface;

namespace wwwDesignersGuild.Models.Careers
{
    public class cmsCareersPage : ICmsCareersPageRepository
    {
        public async Task<cmsCareersPageDefinition> loadCareersPageFromCms(byte languageId, short countryId, DateTime useDate)
        {
            // Is page in cache?
            var objCareersPageCache = new CareersPageCacheProvider();
            var careersPageStore = objCareersPageCache.Get(languageId, countryId, useDate);

            if (careersPageStore == null)
            {
                careersPageStore = await Task.FromResult<cmsCareersPageDefinition>(loadCareersPageFromCmsDatabase(languageId, countryId, useDate));
                objCareersPageCache.Set(languageId, countryId, useDate, careersPageStore);
            }

            return careersPageStore;
        }

        private cmsCareersPageDefinition loadCareersPageFromCmsDatabase(byte languageId, short countryId, DateTime useDate)
        {
            var dataContext = new databaseDataContext();

            IMultipleResults results = dataContext.Careers_GenerateMultiple(languageId, countryId, useDate);

            cmsCareersPageDefinition careersStore = new cmsCareersPageDefinition
            {
                CareerListings = results.GetResult<CareerSelect>().ToList(),
                Blurb = results.GetResult<CareersBlurb>().Single()
            };

            return careersStore;
        }
    }
}