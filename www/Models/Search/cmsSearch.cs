using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Classes;

using wwwDesignersGuild.Models.Database;

namespace wwwDesignersGuild.Models.Search
{
	public class cmsSearch
	{
		public async Task<serverSearchDatabase> loadSearchFromCms(DataTable productPageTable, byte languageId, short countryId, string searchTerm, int? tradeAccountId, DateTime useDate)
		{
			//  Is result already in cache?
			serverSearchDatabase searchResult = null;

			var cache = new SearchCacheProvider();

			searchResult = cache.GetOrCreate(languageId, countryId, searchTerm, tradeAccountId, useDate, () => {

				var result = Task.Run(() => loadSearchFromDatabase(productPageTable, languageId, countryId, searchTerm, tradeAccountId, useDate)).Result;

				return result;

			});

			return searchResult;
		}

		private serverSearchDatabase loadSearchFromDatabase(DataTable productPageTable, byte languageId, short countryId, string searchTerm, int? tradeAccountId, DateTime useDate)
		{
			var objDbSearchResults = new searchDb();
			return objDbSearchResults.generateServerSearchResults(productPageTable,
				languageId,
				countryId,
				useDate,
				String.IsNullOrEmpty(searchTerm),
				tradeAccountId,
				tradeAccountId.HasValue);
		}
	}
}