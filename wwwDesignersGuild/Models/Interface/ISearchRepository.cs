using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;

namespace wwwDesignersGuild.Models.Interface
{  
    public interface ISearchRepository
    {
        Task<serverSearchDatabase> buildSearchResultsDb(byte languageId, short countryId, string searchText, DateTime useDate);
        Task<searchResult> buildSearchResults(byte languageId, short countryId, string searchText, DateTime useDate);
        Task<DatabaseJSONResponse> getRenderForSearchResults(int? tradeAccountId, bool isTrade, bool displayTradePrice, DataTable searchResultData, DateTime useDate, countryItem objCountryItem, String dateOverride, String countryOverride);
        Task<filteredSearchResult> serverFiltering(webapiSearchParameters passSearch, bool tradeChecksPassed);
        Task<Boolean> logSearchKeywords(Int64 visitorDbId, String searchKeyword, byte languageId, short countryId);
        Task<predictiveSearchResult> getPredictiveSearch(String searchKeyword, byte languageId, short countryId);
    }
}
