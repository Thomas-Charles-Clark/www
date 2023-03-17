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
using NLog;

namespace wwwDesignersGuild.Models.ArticlePage
{
    public class cmsArticlePage : ICmsArticlePageRepository
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public async Task<cmsArticlePageDefinition> loadArticlePageFromCms(int articleId, byte languageId, short countryId, DateTime useDate)
        {
            // Is page in cache?
            cmsArticlePageDefinition articlePageStore;
            ArticlePageCacheProvider objArticlePageCache = new ArticlePageCacheProvider();
            articlePageStore = objArticlePageCache.Get(articleId, languageId, countryId, useDate);

            if (articlePageStore == null)
            {
                if (Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["extraRequestTracing"]))
                {
                    //Logger.Trace($"NOT CACHED: ArticleID: {articleId}, FROM {ip}, LanguageID: {languageId}, CountryID: {countryId}, useDate: {useDate}");
                }

                articlePageStore = await Task.FromResult<cmsArticlePageDefinition>(loadArticlePageFromCmsDatabase(articleId, languageId, countryId, useDate));
                objArticlePageCache.Set(articleId, languageId, countryId, useDate, articlePageStore);
            }
            else
            {
                if (Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["extraRequestTracing"]))
                {
                    //Logger.Trace($"FOUND IN CACHE: ArticleID: {articleId}, FROM {ip}, LanguageID: {languageId}, CountryID: {countryId}, useDate: {useDate}");
                }
            }

            return articlePageStore;
        }

        private cmsArticlePageDefinition loadArticlePageFromCmsDatabase(int articleId, byte languageId, short countryId, DateTime useDate)
        {
            var dataContext = new databaseDataContext();

            IMultipleResults results = dataContext.Article_GenerateMultiple(articleId, languageId, countryId, useDate);

            cmsArticlePageDefinition articleStore = new cmsArticlePageDefinition
            {
                ArticleDetails = results.GetResult<articleSelect>().SingleOrDefault(),
                articleImageSelects = results.GetResult<articleImageSelect>().ToList<articleImageSelect>(),
                articleImageHotspotSelects = results.GetResult<articleImageHotspotSelect>().ToList<articleImageHotspotSelect>(),
                articleRelatedArticleSelects = results.GetResult<articleRelatedArticleSelect>().ToList<articleRelatedArticleSelect>(),
                articleVideoSelects = results.GetResult<articleVideoSelect>().ToList<articleVideoSelect>(),
                articleGoogleHrefs = results.GetResult<googleHref>().ToList<googleHref>(),
                articleNextArticleSelects = results.GetResult<articleNextArticleSelect>().ToList<articleNextArticleSelect>(),
                articlePreviousArticleSelects = results.GetResult<articlePreviousArticleSelect>().ToList<articlePreviousArticleSelect>(),
                articleTypeSelects = results.GetResult<articleTypeSelect>().ToList<articleTypeSelect>(),
                articleSubTypeSelects = results.GetResult<articleSubTypeSelect>().ToList<articleSubTypeSelect>(),
                Breadcrumbs = results.GetResult<Breadcrumb>().ToList()
            };

            return articleStore;
        }
    }
}