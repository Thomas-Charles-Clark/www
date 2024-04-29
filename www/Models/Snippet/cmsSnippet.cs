using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Interface;


namespace wwwDesignersGuild.Models.Snippet
{


    public class cmsSnippet : ICmsSnippetRepository
    {

        public async Task<cmsSnippetDefinition> loadSnippetFromCms(byte languageId, short countryId, int? tradeAccountId, bool displayTradePrices, DateTime useDate)
        {

            //  Is page in cache?
            cmsSnippetDefinition snippetStore;
            SnippetCacheProvider objSnippetCache = new SnippetCacheProvider();
            snippetStore = objSnippetCache.Get(languageId, countryId, useDate);

            if (snippetStore == null)
            {
                snippetStore = await Task.FromResult<cmsSnippetDefinition>(loadSnippetFromCmsDatabaseAsync(languageId, countryId, tradeAccountId, displayTradePrices, useDate));
                objSnippetCache.Set(languageId, countryId, useDate, snippetStore);
            }

            return snippetStore;

        }

        private cmsSnippetDefinition loadSnippetFromCmsDatabaseAsync(byte languageId, short countryId, int? tradeAccountId, bool displayTradePrices, DateTime useDate)
        {


            var dataContext = new databaseDataContext();


            IMultipleResults results = dataContext.Snippet_GenerateMultiple(languageId, countryId, tradeAccountId, displayTradePrices, useDate);


            //  Convert from IEnumerable to a List so a collection query can be run multiple times
            cmsSnippetDefinition snippetStore = new cmsSnippetDefinition
            {
                snippetRows = results.GetResult<Classes.Snippet>().ToList<Classes.Snippet>()
            };
            return snippetStore;


        }


    }
}