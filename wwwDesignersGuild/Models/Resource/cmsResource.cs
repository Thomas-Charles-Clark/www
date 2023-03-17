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

namespace wwwDesignersGuild.Models.Resource
{
    public class cmsResource : ICmsResourceRepository
    {
        public async Task<cmsResourceDefinition> loadResourceFromCms(byte languageId, short countryId, DateTime useDate)
        {
            // Is page in cache?
            cmsResourceDefinition resourceStore = new cmsResourceDefinition();
            ResourceCacheProvider objResourceCache = new ResourceCacheProvider();
            resourceStore.resourceSelects = objResourceCache.Get(languageId, countryId, useDate);

            if (resourceStore.resourceSelects == null)
            {
                resourceStore = await Task.FromResult<cmsResourceDefinition>(loadResourceFromCmsDatabase(languageId, countryId, useDate));
                objResourceCache.Set(languageId, countryId, useDate, resourceStore.resourceSelects);
            }

            return resourceStore;
        }

        private cmsResourceDefinition loadResourceFromCmsDatabase(byte languageId, short countryId, DateTime useDate)
        {
            var dataContext = new databaseDataContext();

            IMultipleResults resourceResults = dataContext.Resource_Select(languageId, countryId, useDate);

            cmsResourceDefinition resourceStore = new cmsResourceDefinition
            {
                resourceSelects = resourceResults.GetResult<resourceSelect>().ToList<resourceSelect>()
            };

            return resourceStore;
        }
    }
}