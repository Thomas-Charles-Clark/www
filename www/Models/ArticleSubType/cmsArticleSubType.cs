using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Interface;

namespace wwwDesignersGuild.Models.ArticleSubType
{
    public class cmsArticleSubType : ICmsArticleSubTypeRepository
    {
        public async Task<cmsArticleSubTypeDefinition> loadArticleSubTypeFromCms(byte articleTypeId, byte languageId, short countryId, DateTime useDate)
        {
            // Is page in cache?
            cmsArticleSubTypeDefinition articleSubTypeStore;

            ArticleSubTypeCacheProvider objArticleSubTypeCache = new ArticleSubTypeCacheProvider();

            articleSubTypeStore = objArticleSubTypeCache.GetOrCreate(articleTypeId, languageId, countryId, useDate, () => {

                var result = Task.Run(() => loadArticleSubTypeFromCmsDatabase(articleTypeId, languageId, countryId, useDate)).Result;

                return result;

            });

            return articleSubTypeStore;
        }

        private cmsArticleSubTypeDefinition loadArticleSubTypeFromCmsDatabase(byte articleTypeID, byte languageId, short countryId, DateTime useDate)
        {
            var dataContext = new databaseDataContext();

            IMultipleResults subTypeResults = dataContext.ArticleSubType_Select(articleTypeID, languageId, countryId, useDate);

            cmsArticleSubTypeDefinition articleSubTypeStore = new cmsArticleSubTypeDefinition
            {
                articleSubTypeSelects = subTypeResults.GetResult<articleSubTypeSelect>().ToList<articleSubTypeSelect>()
            };

            return articleSubTypeStore;
        }
    }
}