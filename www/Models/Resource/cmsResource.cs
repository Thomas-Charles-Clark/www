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
using Microsoft.Ajax.Utilities;

namespace wwwDesignersGuild.Models.Resource
{
    public class cmsResource : ICmsResourceRepository
    {
        public async Task<cmsResourceDefinition> loadResourceFromCms(byte languageId, short countryId, DateTime useDate)
        {
            var objResourceCache = new ResourceCacheProvider();

			var resourceStore = objResourceCache.GetOrCreate(languageId, countryId, useDate, () => {

                var result = Task.Run(() => loadResourceFromCmsDatabase(languageId, countryId, useDate)).Result;

                return result;

            });

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