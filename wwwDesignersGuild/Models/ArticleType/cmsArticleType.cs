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
            // Is page in cache?
            cmsArticleTypeDefinition articleTypeStore;
            ArticleTypeCacheProvider objArticleTypeCache = new ArticleTypeCacheProvider();
            articleTypeStore = objArticleTypeCache.Get(languageId, countryId, useDate);

            if (articleTypeStore == null)
            {
                articleTypeStore = await Task.FromResult<cmsArticleTypeDefinition>(loadArticleTypeFromCmsDatabase(languageId, countryId, useDate));
                objArticleTypeCache.Set(languageId, countryId, useDate, articleTypeStore);
            }

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