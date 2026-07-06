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

namespace wwwDesignersGuild.Models.ArticleType
{
    public class cmsArticleType : ICmsArticleTypeRepository
    {
        public async Task<cmsArticleTypeDefinition> loadArticleTypeFromCms(byte languageId, short countryId, DateTime useDate)
        {
            cmsArticleTypeDefinition articleTypeStore;

            ArticleTypeCacheProvider objArticleTypeCache = new ArticleTypeCacheProvider();

            articleTypeStore = objArticleTypeCache.GetOrCreate(languageId, countryId, useDate, () => {

                var result = Task.Run(() => loadArticleTypeFromCmsDatabase(languageId, countryId, useDate)).Result;

                return result;

            });

            return articleTypeStore;
        }

        private cmsArticleTypeDefinition loadArticleTypeFromCmsDatabase(byte languageId, short countryId, DateTime useDate)
        {
            var dataContext = new databaseDataContext();

            IMultipleResults typeResults = dataContext.ArticleType_Select(languageId, countryId, useDate);

            cmsArticleTypeDefinition articleTypeStore = new cmsArticleTypeDefinition
            {
                articleTypeSelects = typeResults.GetResult<articleTypeSelect>().ToList<articleTypeSelect>()
            };

            return articleTypeStore;
        }
    }
}