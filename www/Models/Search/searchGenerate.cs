using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Web.Mvc;
using dtSearch.Engine;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Interface;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Attribute;
using Nuclex.Support.Cloning;
using System.Globalization;
using wwwDesignersGuild.Models.ProductPage;
using System.IO;

namespace wwwDesignersGuild.Models.Search
{


    public class searchResultDefinition
    {
        public int productPageID { get; set; }
        public Int32 ScorePercent { get; set; }
    }


    public class searchGenerate : ISearchRepository
    {


        #region Server Search


        public async Task<serverSearchDatabase> buildSearchResultsDb(byte languageId, short countryId, string searchText, byte? productTypeId, DateTime useDate)
        {
            serverSearchDatabase storeResults = await buildSearchResultsAsyncDb(languageId, countryId, searchText, productTypeId, useDate);            
            return storeResults;
        }


        private async Task<serverSearchDatabase> buildSearchResultsAsyncDb(byte languageId, short countryId, string searchTerm, byte? productTypeId, DateTime useDate)
        {

#if DEBUG

            var sw = new System.Diagnostics.Stopwatch();

            sw.Start();

#endif
            // Ducks, align!

            var objSearchResults = new List<searchResultDefinition>();

            var productPageTable = new DataTable();
            
            productPageTable.Columns.Add("ProductPageID", typeof(int));
            productPageTable.Columns.Add("ScorePercentage", typeof(int));

            if (!String.IsNullOrEmpty(searchTerm))
            {
                var dtSearchIndexThesaurusPath = System.Web.Configuration.WebConfigurationManager.AppSettings["dtSearchIndexThesaurusPath"];


                if (!Directory.Exists(dtSearchIndexThesaurusPath))
                {
                    Directory.CreateDirectory(dtSearchIndexThesaurusPath);
                }

                //  Call dtSearch
                var dtSearchThesaurusPath = $"{dtSearchIndexThesaurusPath}\\{languageId}\\Thesaurus\\thesaur.xml";

                var dtSearchProductCodeIndexPath = $"{dtSearchIndexThesaurusPath}\\{languageId}\\ProductCodeIndex";

                var dtSearchMainIndexPath = $"{dtSearchIndexThesaurusPath}\\{languageId}\\MainIndex";

                Options options = new Options
                {
                    UserThesaurusFile = dtSearchThesaurusPath.Replace(@"\\", @"\")
                };
                //options.Save();


                //  SEARCH: Product code
                var searchJobProductCodeIndex = new SearchJob();
                searchJobProductCodeIndex.IndexesToSearch.Add(dtSearchProductCodeIndexPath.Replace(@"\\", @"\"));
                searchJobProductCodeIndex.Request = searchTerm;
                searchJobProductCodeIndex.SearchFlags = SearchFlags.dtsSearchPositionalScoring | SearchFlags.dtsSearchTypeAllWords;

                searchJobProductCodeIndex.Execute();

                var tempProductCodeSearchResults = new List<searchResultDefinition>();
                using (var results = searchJobProductCodeIndex.Results)
                {
                    results.Sort(SortFlags.dtsSortByRelevanceScore, "");

                    for (int i = 0; i < results.Count; ++i)
                    {
                        results.GetNthDoc(i);
                        tempProductCodeSearchResults.Add(new searchResultDefinition { productPageID = Convert.ToInt32(results.DocName), ScorePercent = results.CurrentItem.ScorePercent });
                    }
                }



                //  SEARCH: Main text
                //  Note:   SearchFlags.dtsSearchPhonic has been removed as it made the search results too general
                var searchJobMainIndex = new SearchJob();
                searchJobMainIndex.IndexesToSearch.Add(dtSearchMainIndexPath.Replace(@"\\", @"\"));
                searchJobMainIndex.Request = searchTerm;
                searchJobMainIndex.SearchFlags = SearchFlags.dtsSearchStemming |
                                        //SearchFlags.dtsSearchFuzzy |          // no longer fuzzy 
                                        SearchFlags.dtsSearchSynonyms |
                                        SearchFlags.dtsSearchUserSynonyms |
                                        SearchFlags.dtsSearchPositionalScoring |
                                        SearchFlags.dtsSearchTypeAllWords;

                //searchJobMainIndex.Fuzziness = 1;

                searchJobMainIndex.Execute();

                var tempMainSearchResults = new List<searchResultDefinition>();
                using (SearchResults results = searchJobMainIndex.Results)
                {
                    results.Sort(SortFlags.dtsSortByRelevanceScore, "");

                    for (int i = 0; i < results.Count; ++i)
                    {
                        results.GetNthDoc(i);
                        tempMainSearchResults.Add(new searchResultDefinition { productPageID = Convert.ToInt32(results.DocName), ScorePercent = results.CurrentItem.ScorePercent });
                    }
                }


                //  Merge the two search results (product code takes priority)
                foreach (var objProductResult in tempProductCodeSearchResults)
                {
                    objProductResult.ScorePercent = 100;    //  Product code matches always get 100% as they appear first in the search results
                    objSearchResults.Add(objProductResult);
                }
                foreach (var objMainResult in tempMainSearchResults)
                {
                    bool productPageFound = false;
                    foreach (var objProductResult in tempProductCodeSearchResults)
                    {
                        if (objProductResult.productPageID == objMainResult.productPageID)
                        {
                            productPageFound = true;
                            break;
                        }
                    }
                    if (productPageFound == true) { }
                    else
                    {
                        objSearchResults.Add(objMainResult);
                    }
                }
                foreach (var searchResult in objSearchResults)
                {
                    productPageTable.Rows.Add(searchResult.productPageID, searchResult.ScorePercent);
                    // replaced 0 with searchResult.ScorePercent: forever and a day the ServerSearchResults_Generate procedure wanted nothing to do with manipulating the relevance scoring
                    // now maybe it does, eh?
                }

            }
            else
            {

            }

            int? tradeAccountId = null;

            if (HttpContext.Current.Session["memberTradeAccountID"] != null)
            {
                tradeAccountId = Convert.ToInt32(HttpContext.Current.Session["memberTradeAccountID"].ToString());
            }

            Int32? memberId = null;
            if (HttpContext.Current.Session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(HttpContext.Current.Session["memberIdFromDatabase"].ToString());
            }

            var displayTradePrice = false;

            if (HttpContext.Current.Session["memberDisplayTradePrice"] == null) { }
            else
            {
                displayTradePrice = Convert.ToBoolean(HttpContext.Current.Session["memberDisplayTradePrice"]);
            }

#if DEBUG
            sw.Stop();

            System.Diagnostics.Debug.WriteLine($"dtSearch executed in {sw.Elapsed}");
            
            sw.Reset();

            sw.Start();
#endif
            var search = new cmsSearch();

            var objSearchResultReturn = await search.loadSearchFromCms(productPageTable, languageId, countryId, searchTerm, tradeAccountId, productTypeId, useDate);

            Parallel.ForEach(objSearchResultReturn.searchResultCores, tempResult =>
            {
                var tempSearch = objSearchResults.Where(p => p.productPageID == tempResult.ProductPageID).FirstOrDefault();

                if (tempSearch == null) { }
                else
                {
                    tempResult.ScorePercent = tempSearch.ScorePercent;
                }
            });

            objSearchResultReturn.searchResultCores = objSearchResultReturn.searchResultCores.OrderByDescending(p => p.ScorePercent).ThenByDescending(p => p.SearchSortOrder).ThenBy(p => p.ProductPageID).ToList();


#if DEBUG
            sw.Stop();

            System.Diagnostics.Debug.WriteLine($"ServerSearchResults_Generate executed in {sw.Elapsed}");
#endif

            return objSearchResultReturn;

        }
               
        
#endregion


#region Server Filtering


        public async Task<filteredSearchResult> serverFiltering(webapiSearchParameters passSearch, bool tradeChecksPassed)
        {
            //  Determine the date to obtain the data from the cms
            DateTime useDate = DateTime.Now;
            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                //  Should the "use date" be overriden?
                if (HttpContext.Current.Request.QueryString["dateOverride"] == null) { }
                else
                {
                    String overrideDate = HttpContext.Current.Request.QueryString["dateOverride"].ToString();
                    if (overrideDate.Length == "ddMMyyyy".Length)
                    {
                        bool dateConvertSuccess = DateTime.TryParseExact(overrideDate,
                                                                              "ddMMyyyy",
                                                                              CultureInfo.InvariantCulture,
                                                                              DateTimeStyles.None,
                                                                              out useDate);
                        if (dateConvertSuccess == true) { }
                        else
                        {
                            useDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                        }
                    }
                }
            }

            String basketCountry = passSearch.basketCountry.Trim();

            cmsCountry objCountry = new cmsCountry();
            cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(useDate);

            if (dgFramework.doesCountryExistFromText(basketCountry, objCountryData)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
            }

            countryItem objCountryItem = dgFramework.getCountryFromText(basketCountry, objCountryData);


            //var session = HttpContext.Current.Session;
            //  Remove check, as it slowed down the process as the session object has to be retrieved to test for null
            //if (HttpContext.Current.Session["serverSearchResult"] == null)
            //{
            //    dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            //}


            serverSearchDatabase objSearchResult = ExpressionTreeCloner.DeepFieldClone((serverSearchDatabase)HttpContext.Current.Session["serverSearchResult"]);

            //  To slow use ExpressionTreeCloner
            //serverSearchDatabase objSearchResult2 = ((serverSearchDatabase)HttpContext.Current.Session["serverSearchResult"]).DeepClone();


            cmsAttribute objAttribute = new cmsAttribute();
            cmsAttributeDefinition objAttributeDefinitions = await objAttribute.loadAttributeFromCms(objCountryItem.LanguageID);


            if (passSearch.productTypeId == 0) { }
            else
            {
                objSearchResult.searchResultCores = objSearchResult.searchResultCores.Where(p => p.ProductTypeID == passSearch.productTypeId).ToList();
            }


            List<short> uniqueAttribute = objSearchResult.searchResultAttributes.GroupBy(p => p.AttributeID).Select(grp => grp.Key).ToList<short>();
            List<AttributeDetail> attributeDetails = objAttributeDefinitions.attributeDetails.Where(p => uniqueAttribute.Contains(p.AttributeID)).ToList<AttributeDetail>();
            List<AttributeGroup> attributeGroups = objAttributeDefinitions.attributeGroups;


            List<AttributeGroup_ProductType> attributeGroupProductTypes = objAttributeDefinitions.attributeGroupProductTypes;
            List<byte> uniqueProductTypes = objSearchResult.searchResultCores.GroupBy(p => p.ProductTypeID).Select(grp => grp.Key).ToList<byte>();

            List<byte> commonAttributeGroups = new List<byte>();
            foreach (AttributeGroup objGroup in attributeGroups)
            {

                bool productTypeMatch = true;
                foreach (byte tempProductType in uniqueProductTypes)
                {
                    if (attributeGroupProductTypes.Where(p => p.AttributeGroupID == objGroup.AttributeGroupID && p.ProductTypeID == tempProductType).Count() == 0)
                    {
                        productTypeMatch = false;
                        break;
                    }
                }

                if (productTypeMatch == true)
                {
                    commonAttributeGroups.Add(objGroup.AttributeGroupID);
                }

            }
            
            //  Only include the attributes within the common groups
            attributeGroups = attributeGroups.Where(p => commonAttributeGroups.Contains(p.AttributeGroupID)).OrderBy(p => p.SortOrder).ToList<AttributeGroup>();

            attributeDetails = attributeDetails.Where(p => commonAttributeGroups.Contains(p.AttributeGroupID)).OrderBy(p => p.SortOrder).ToList<AttributeDetail>();

            uniqueAttribute = attributeDetails.GroupBy(p => p.AttributeID).Select(grp => grp.Key).ToList<short>();
            
            
            List<serverSearchResultAttribute> filteredSearchResultAttributes = objSearchResult.searchResultAttributes.AsParallel().Where(a => objSearchResult.searchResultCores.Select(p => p.ProductPageID).Contains(a.ProductPageID)).ToList();

            Int32 numberOfSelectedFilters = passSearch.filterIds.Count;
            
            //  Filter Groups
            List<FilterGroup> objFilterGroups = attributeGroups.AsParallel().Select(ag => new FilterGroup
            {
                attributeGroup = ag,
                productSet = null,
                filters = attributeDetails.Where(ad => ad.AttributeGroupID == ag.AttributeGroupID)
                                          .OrderBy(ad => ad.SortOrder)
                                          .Select(ad => new wwwDesignersGuild.Models.Classes.Filter
                                          {
                                              attributeDetail = ad,
                                              isChecked = passSearch.filterIds.Contains(ad.AttributeID),
                                              isEnabled = true,
                                              productIds = filteredSearchResultAttributes.Where(a => a.AttributeID == ad.AttributeID).Select(p => p.ProductPageID).ToList()
                                          }).ToList()
            }).ToList();

            //  Remove non-required data
            objFilterGroups.RemoveAll(fg => fg.attributeGroup.UseIcons && fg.filters.Where(f => f.isEnabled).Count() == 0);
            objFilterGroups.ForEach(fg => fg.filters.RemoveAll(f => f.productIds.Count == 0));


            //  Set counts (all products)
            objFilterGroups.ForEach(fg => fg.filters.ForEach(f => f.productsCount = f.productIds.Count));



            List<Int32> productIdsToDisplay = objSearchResult.searchResultCores.Select(p => p.ProductPageID).ToList();

            foreach (FilterGroup objFilterGroup in objFilterGroups)
            {
                objFilterGroup.productSet = objFilterGroup.filters.Where(f => f.isChecked).SelectMany(f => f.productIds).Distinct().ToList();

                if (objFilterGroup.productSet.Count == 0) { }
                else
                {
                    productIdsToDisplay = productIdsToDisplay.Intersect(objFilterGroup.productSet).ToList();
                }
            }
            
            if (numberOfSelectedFilters == 0) { }
            else
            {
                //  Generate Filter counts
                foreach (FilterGroup objFilterGroup in objFilterGroups)
                {
                    List<Int32> tempProductSet = objSearchResult.searchResultCores.Select(p => p.ProductPageID).ToList();

                    foreach (FilterGroup tempFilterGroup in objFilterGroups)
                    {
                        if (tempFilterGroup.attributeGroup.AttributeGroupID == objFilterGroup.attributeGroup.AttributeGroupID) { }
                        else if (tempFilterGroup.productSet.Count == 0) { }
                        else
                        {
                            tempProductSet = tempProductSet.Except(tempProductSet.Where(id => !tempFilterGroup.productSet.Contains(id)).ToList()).ToList();
                        }
                    }

                    foreach (wwwDesignersGuild.Models.Classes.Filter tempFilter in objFilterGroup.filters)
                    {
                        Int32 filterProductCount = tempFilter.productIds.Where(id => tempProductSet.Contains(id)).Count();

                        if (filterProductCount == 0)
                        {
                            tempFilter.isEnabled = false;
                        }
                        else
                        {
                            tempFilter.productsCount = filterProductCount;
                            tempFilter.isEnabled = true;
                        }
                    }
                }
            }

            objSearchResult.searchResultCores = objSearchResult.searchResultCores.Where(p => productIdsToDisplay.Contains(p.ProductPageID)).ToList();


            //  Remove disabled colours (not shown on screen)
            objFilterGroups.ForEach(fg => fg.filters.RemoveAll(f => fg.attributeGroup.UseIcons && f.isEnabled == false));


            filteredSearchResult objFilteredSearchResult = new filteredSearchResult
            {
                filterGroups = objFilterGroups.Where(fg => fg.filters.Count >= 1).OrderBy(p => p.attributeGroup.SortOrder).ToList()
            };

            var displayTradePrice = false;

            if (HttpContext.Current.Session["memberDisplayTradePrice"] == null) { }
            else
            {
                displayTradePrice = Convert.ToBoolean(HttpContext.Current.Session["memberDisplayTradePrice"]);
            }

            if (!tradeChecksPassed || !displayTradePrice) /* If you haven't passed the trade checks */
            {
                // and there's a Clearance attribute group delivered
                var clearanceGroup = objFilteredSearchResult.filterGroups.Where(p => p.attributeGroup.AttributeGroupID == 0).SingleOrDefault();

                if (clearanceGroup != null)
                {
                    // remove it
                    objFilteredSearchResult.filterGroups.Remove(clearanceGroup);
                }
            }

            //  Note:   Need to cast "serverSearchResultCore" to "serverSearchResultCoreToClient", as 
            //          the "[Serializable]" attribute on "serverSearchResultCore" causes the JSON 
            //          serialize to fail when passing the data to the client
            objFilteredSearchResult.products = objSearchResult.searchResultCores.Select(p => new serverSearchResultCoreToClient 
            { 
                ColourSort = p.ColourSort, 
                AtoZScore = p.AtoZScore, 
                SearchSortOrder = p.SearchSortOrder, 
                ProductPageID = p.ProductPageID, 
                ProductTypeID = p.ProductTypeID, 
                ScorePercent = p.ScorePercent,
                NewSort = p.NewSort
            }).ToList();


            return objFilteredSearchResult;
        }


#endregion


#region Render Server Search


        public async Task<DatabaseJSONResponse> getRenderForSearchResults(int? tradeAccountId, bool isTrade, bool displayTradePrice, DataTable searchResultData, DateTime useDate, countryItem objCountryItem, String dateOverride, String countryOverride)
        {
            var storeResults = await Task.FromResult(getRenderForSearchResultsAsync(tradeAccountId, isTrade, displayTradePrice, searchResultData, useDate, objCountryItem, dateOverride, countryOverride));
            
            return storeResults;
        }


        public DatabaseJSONResponse getRenderForSearchResultsAsync(int? tradeAccountId, bool isTrade, bool displayTradePrice, DataTable searchResultData, DateTime useDate, countryItem objCountryItem, String dateOverride, String countryOverride)
        {
            var objDbSearchResults = new searchDb();

            var result = objDbSearchResults.getSearchDataForRender(tradeAccountId, isTrade, displayTradePrice, searchResultData, objCountryItem.LanguageID, objCountryItem.CountryID, useDate);

            return result;

            //var pageSectionProductPageUrls = new List<pageSectionProductPageUrl>();
            
            //foreach (var pp in objRenderReturn.searchResultRenderCores)
            //{
            //    var objPageSectionProductPageImage = objRenderReturn.searchResultRenderImages.Where(i => i.ProductPageID == pp.ProductPageID).FirstOrDefault();

            //    string imageId = "";

            //    if (objPageSectionProductPageImage == null) { }
            //    else
            //    {
            //        imageId = objPageSectionProductPageImage.ImageID.ToString();
            //    }
                
            //    string priceLine = "";
            //    string newIndicator = "";
                
            //    if (pp.DisplayPriceMin != null)
            //    {
            //        priceLine = dgFramework.displayPriceRange(pp.DisplayPriceMin, pp.DisplayPriceMax, pp.DisplaySalePriceMin, pp.DisplaySalePriceMax, objCountryItem, useDate);
            //    }
                
            //    if (pp.New)
            //    {
            //        newIndicator = HtmlHelperExtra.Resource("PodNew", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString();
            //    }

            //    var Url = new UrlHelper(HttpContext.Current.Request.RequestContext);

            //    string imageUrl = "";
            //    if (imageId == null) { }
            //    else
            //    {
            //        imageUrl = Url.RouteUrl("ImageRoutine", new { width = dgFramework.getImageWidth(dgImageSize.productPage), imageid = imageId });
            //    }
                
            //    pageSectionProductPageUrls.Add(new pageSectionProductPageUrl
            //    {
            //        ProductPageID = pp.ProductPageID,
            //        ImageUrl = imageUrl,
            //        ImageUrls = objRenderReturn.searchResultRenderImages.Where(p => p.ProductPageID == pp.ProductPageID).Select(p => {
            //            return Url.RouteUrl("ImageRoutine", new { width = dgFramework.getImageWidth(dgImageSize.productPage), imageid = p.ImageID });
            //        }).ToList(),
            //        LinkUrl = dgFramework.buildUrl(objCountryItem,
            //                                        pp.URLKeywords, dgContentType.productPage,
            //                                        pp.ProductPageID,
            //                                        dateOverride,
            //                                        countryOverride),
            //        PriceLine = priceLine,
            //        NewInd = newIndicator,
            //        LowestPrice = pp.DisplaySalePriceMin ?? pp.DisplayPriceMin,
            //        HighestPrice = pp.DisplayPriceMax ?? pp.DisplaySalePriceMax,
            //        DiscountMessage = pp.DiscountMessage,
            //        Heading = pp.Heading,
            //        SubHeading = pp.SubHeading,
            //        ProductID = pp.ProductID,
            //        StockStatusID = pp.StockStatusID,
            //        Sample = pp.Sample,
            //        Code = pp.Code,
            //        FreeStock = pp.FreeStock,
            //        StockUnit = pp.StockUnit,
            //        RatingAvg = pp.RatingAvg,
            //        RatingCount = pp.RatingCount,
            //        DisplaySalePriceMin = pp.DisplaySalePriceMin,
            //        DisplaySalePriceMax = pp.DisplaySalePriceMax,
            //        DisplayPriceMin = pp.DisplayPriceMin,
            //        DisplayPriceMax = pp.DisplayPriceMax
            //    });
            //}

            //return pageSectionProductPageUrls;
        }


#endregion


#region Log Search Keywords


        public async Task<Boolean> logSearchKeywords(Int64 visitorDbId, String searchKeyword, byte languageId, short countryId)
        {
            Boolean storeResults = await Task.FromResult<Boolean>(logSearchKeywordsAsync(visitorDbId, searchKeyword, languageId, countryId));
            return storeResults;
        }


        private Boolean logSearchKeywordsAsync(Int64 visitorDbId, String searchKeyword, byte languageId, short countryId)
        {

            searchDb objDbSearch = new searchDb();
            return objDbSearch.callSaveSearchKeywords(visitorDbId, searchKeyword, languageId, countryId);

        }


#endregion


#region Predictive Search


        public async Task<predictiveSearchResult> getPredictiveSearch(String searchKeyword, byte languageId, short countryId)
        {
            predictiveSearchResult storeResults = await Task.FromResult<predictiveSearchResult>(getPredictiveSearchAsync(searchKeyword, languageId, countryId));
            return storeResults;
        }


        private predictiveSearchResult getPredictiveSearchAsync(String searchKeyword, byte languageId, short countryId)
        {

            var dataContext = new databaseDataContext();


            IMultipleResults results = dataContext.SearchPredictive_GenerateMultiple(searchKeyword, countryId, languageId);


            //  Convert from IEnumerable to a List so a collection query can be run multiple times
            predictiveSearchResult searchResults = new predictiveSearchResult
            {
                results = results.GetResult<predictiveSearch>().ToList()
            };

            return searchResults;


        }



        public async Task<predictiveSearchHints> getPredictiveSearchHints(String searchKeyword, byte languageId, short countryId, DateTime useDate, byte? productTypeId, int? tradeAccountId)
        {
            predictiveSearchHints storeResults = await Task.FromResult<predictiveSearchHints>(getPredictiveSearchHintsAsync(searchKeyword, languageId, countryId, useDate, productTypeId, tradeAccountId));
            return storeResults;
        }


        private predictiveSearchHints getPredictiveSearchHintsAsync(String searchKeyword, byte languageId, short countryId, DateTime useDate, byte? productTypeId, int? tradeAccountId)
        {
            var dataContext = new databaseDataContext();


            IMultipleResults results = dataContext.SearchHint_GenerateMultiple(searchKeyword, countryId, languageId, useDate, productTypeId, tradeAccountId, tradeAccountId.HasValue);


            predictiveSearchHints searchResults = new predictiveSearchHints
            {
                collections = results.GetResult<collectionSearch>().ToList(),
                designs = results.GetResult<designSearch>().ToList(),
                products = results.GetResult<productSearch>().ToList(),
                brands = results.GetResult<brandSearch>().ToList(),
                discontinueds = results.GetResult<discontinuedSearch>().ToList(),
                productTypes = results.GetResult<ProductTypeHint>().ToList(),
            };

            return searchResults;
        }



#endregion


#region Client Search


        public async Task<searchResult> buildSearchResults(byte languageId, short countryId, string searchText, DateTime useDate)
        {
            searchResult storeResults = await Task.FromResult<searchResult>(buildSearchResultsAsync(languageId, countryId, searchText, useDate));
            return storeResults;
        }

        private searchResult buildSearchResultsAsync(byte languageId, short countryId, string searchText, DateTime useDate)
        {

            //  Call dtSearch
            string dtSearchThesaurusPath = String.Format("{0}{1}\\Thesaurus\\thesaur.xml", 
                                                            System.Web.Configuration.WebConfigurationManager.AppSettings["dtSearchIndexThesaurusPath"],
                                                            languageId.ToString());

            string dtSearchProductCodeIndexPath = String.Format("{0}{1}\\ProductCodeIndex", 
                                                            System.Web.Configuration.WebConfigurationManager.AppSettings["dtSearchIndexThesaurusPath"],
                                                            languageId.ToString());

            string dtSearchMainIndexPath = String.Format("{0}{1}\\MainIndex", 
                                                            System.Web.Configuration.WebConfigurationManager.AppSettings["dtSearchIndexThesaurusPath"],
                                                            languageId.ToString());


            Options options = new Options
            {
                UserThesaurusFile = dtSearchThesaurusPath.Replace(@"\\", @"\")
            };
            options.Save();


            //  SEARCH: Product code
            SearchJob searchJobProductCodeIndex = new SearchJob();
            searchJobProductCodeIndex.IndexesToSearch.Add(dtSearchProductCodeIndexPath.Replace(@"\\", @"\"));
            searchJobProductCodeIndex.Request = searchText;
            searchJobProductCodeIndex.SearchFlags = SearchFlags.dtsSearchPositionalScoring |
                                                       SearchFlags.dtsSearchTypeAllWords;


            searchJobProductCodeIndex.Execute();
            

            List<searchResultDefinition> tempProductCodeSearchResults = new List<searchResultDefinition>();
            using (SearchResults results = searchJobProductCodeIndex.Results)
            {
                results.Sort(SortFlags.dtsSortByRelevanceScore, "");

                for (int i = 0; i < results.Count; ++i)
                {
                    results.GetNthDoc(i);
                    tempProductCodeSearchResults.Add(new searchResultDefinition { productPageID = Convert.ToInt32(results.DocName), ScorePercent = results.CurrentItem.ScorePercent });
                }
            }



            //  SEARCH: Main text
            //  Note:   SearchFlags.dtsSearchPhonic has been removed as it made the search results too general
            SearchJob searchJobMainIndex = new SearchJob();
            searchJobMainIndex.IndexesToSearch.Add(dtSearchMainIndexPath.Replace(@"\\", @"\"));
            searchJobMainIndex.Request = searchText;
            searchJobMainIndex.SearchFlags = SearchFlags.dtsSearchStemming |
                                    SearchFlags.dtsSearchFuzzy |
                                    SearchFlags.dtsSearchSynonyms |
                                    SearchFlags.dtsSearchUserSynonyms |
                                    SearchFlags.dtsSearchPositionalScoring |
                                    SearchFlags.dtsSearchTypeAllWords;
            searchJobMainIndex.Fuzziness = 1;
            
            searchJobMainIndex.Execute();
            
            List<searchResultDefinition> tempMainSearchResults = new List<searchResultDefinition>();
            using (SearchResults results = searchJobMainIndex.Results)
            {
                results.Sort(SortFlags.dtsSortByRelevanceScore, "");

                for (int i = 0; i < results.Count; ++i)
                {
                    results.GetNthDoc(i);
                    tempMainSearchResults.Add(new searchResultDefinition { productPageID = Convert.ToInt32(results.DocName), ScorePercent = results.CurrentItem.ScorePercent });
                }
            }


            //  Merge the two search results (product code takes priority)
            List<searchResultDefinition> objSearchResults = new List<searchResultDefinition>();
            foreach (searchResultDefinition objProductResult in tempProductCodeSearchResults)
            {
                objProductResult.ScorePercent = 100;    //  Product code matches always get 100% as they appear first in the search results
                objSearchResults.Add(objProductResult);
            }
            foreach (searchResultDefinition objMainResult in tempMainSearchResults)
            {
                bool productPageFound = false;
                foreach (searchResultDefinition objProductResult in tempProductCodeSearchResults)
                {
                    if (objProductResult.productPageID == objMainResult.productPageID)
                    {
                        productPageFound = true;
                        break;
                    }
                }
                if (productPageFound == true) { }
                else
                {
                    objSearchResults.Add(objMainResult);
                }

            }


            //  Call the "search" stored procedure to return the product page details (Limit the number of search results)
            DataTable productPageTable = new DataTable();
            productPageTable.Columns.Add("ProductPageID", typeof(int));
            productPageTable.Columns.Add("ScorePercentage", typeof(int));

            foreach (searchResultDefinition searchResult in objSearchResults)
            {
                productPageTable.Rows.Add( searchResult.productPageID, searchResult.ScorePercent );
            }
                                   
            searchDb objDbSearchResults = new searchDb();
            searchResult objSearchResultReturn = objDbSearchResults.generateSearchResults(productPageTable, languageId, countryId, useDate);

            //  Save the score to the main table
            Parallel.ForEach(objSearchResultReturn.searchResultCores, tempResult =>
            {
                searchResultDefinition tempSearch = objSearchResults.Where(p => p.productPageID == tempResult.ProductPageID).FirstOrDefault();

                if (tempSearch == null) { }
                else
                {
                    tempResult.ScorePercent = tempSearch.ScorePercent;
                }
            });

            return objSearchResultReturn;
        }


#endregion


    }
}