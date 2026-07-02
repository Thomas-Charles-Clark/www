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
            cmsSnippetDefinition snippetStore;

            var objSnippetCache = new SnippetCacheProvider();

            snippetStore = objSnippetCache.GetOrCreate(languageId, countryId, useDate, () => {

                var result = Task.Run(() => loadSnippetFromCmsDatabaseAsync(languageId, countryId, tradeAccountId, displayTradePrices, useDate)).Result;

                return result;

            });

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