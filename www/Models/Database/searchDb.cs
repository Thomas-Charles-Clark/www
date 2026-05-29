using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Reflection;
using wwwDesignersGuild.Models.Classes;
using System.Text;
using wwwDesignersGuild.Models.ProductPage;

namespace wwwDesignersGuild.Models.Database
{
    public class searchDb
    {

        //  Please note:
        //  This function cannot use "Linq to Sql" as it requires a "User Defined Table Type" (passed via a "data table")
        //  This function converts the resultsets to classes manually


        #region Server Side Search

        public serverSearchDatabase generateServerSearchResults(DataTable searchResultData,
            byte? languageId,
            short? countryId,
            DateTime? date,
            bool emptySearch,
            int? tradeAccountId,
            bool isTrade,
            byte? productTypeId)
        {
            serverSearchDatabase storeSearchResults = new serverSearchDatabase();

            Task.Run(async () =>
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
                {
                    SqlCommand cmd = new SqlCommand("pub.ServerSearchResults_Generate", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    SqlParameter storeSqlParam = cmd.Parameters.AddWithValue("@SearchResults", searchResultData);
                    storeSqlParam.SqlDbType = SqlDbType.Structured;
                    storeSqlParam.TypeName = "SearchResultsTableType";

                    cmd.Parameters.AddWithValue("@LanguageID", languageId);
                    cmd.Parameters.AddWithValue("@CountryID", countryId);
                    cmd.Parameters.AddWithValue("@Date", date);
                    cmd.Parameters.AddWithValue("@EmptySearch", emptySearch); // @EmptySearch = 1 if it's an empty search!
                    cmd.Parameters.AddWithValue("@IsTrade", isTrade);
                    cmd.Parameters.AddWithValue("@TradeAccountID", tradeAccountId);
                    cmd.Parameters.AddWithValue("@ProductTypeID", productTypeId);

					//conn.Open();
					await conn.OpenAsync();
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    storeSearchResults.searchResultCores = new List<serverSearchResultCore>();
                    storeSearchResults.searchResultAttributes = new List<serverSearchResultAttribute>();

                    while (await reader.ReadAsync())
                    {
                        storeSearchResults.searchResultCores.Add(new serverSearchResultCore
                        {
                            ProductPageID = reader.GetInt32(0),
                            ProductTypeID = reader.GetByte(1),
                            SearchSortOrder = reader.GetInt32(2),
                            AtoZScore = reader.GetInt64(3),
                            ColourSort = reader.GetByte(4),
                            NewSort = reader.GetInt64(5),
                            PriceSort = reader.GetInt64(6)
                        });
                    }


                    reader.NextResult();


                    while (await reader.ReadAsync())
                    {
                        storeSearchResults.searchResultAttributes.Add(new serverSearchResultAttribute
                        {
                            ProductPageID = reader.GetInt32(0),
                            AttributeID = reader.GetInt16(1)
                        });
                    }

                }

            }).Wait();


            return storeSearchResults;
        }


        public DatabaseJSONResponse getSearchDataForRender(int? tradeAccountId, bool isTrade, bool displayTradePrice, DataTable searchResultData, byte languageId, short countryId, DateTime? date)
        {
            var result = new DatabaseJSONResponse();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.SearchResults_Render", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter storeSqlParam = cmd.Parameters.AddWithValue("@SearchResults", searchResultData);
                storeSqlParam.SqlDbType = SqlDbType.Structured;
                storeSqlParam.TypeName = "SearchResultsRenderTableType";

                cmd.Parameters.AddWithValue("@TradeAccountID", tradeAccountId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsTrade", isTrade);
                cmd.Parameters.AddWithValue("@DisplayTradePrice", displayTradePrice);
                cmd.Parameters.AddWithValue("@LanguageID", languageId);
                cmd.Parameters.AddWithValue("@CountryID", countryId);
                cmd.Parameters.AddWithValue("@Date", date);

                conn.Open();

                var reader = cmd.ExecuteReader();

                var sb = new StringBuilder();

                while (reader.Read())
                {
                    sb.Append(reader.GetString(0));
                }

                reader.Close();

                result.JSON = sb.ToString();
            }

            return result;
        }
        #endregion


        #region Client Side Search


        public searchResult generateSearchResults(DataTable searchResultData, byte? languageId, short? countryId, DateTime? date)
        {
            searchResult storeSearchResults = new searchResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.SearchResults_Generate", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter storeSqlParam = cmd.Parameters.AddWithValue("@SearchResults", searchResultData);
                storeSqlParam.SqlDbType = SqlDbType.Structured;
                storeSqlParam.TypeName = "SearchResultsTableType";

                cmd.Parameters.AddWithValue("@LanguageID", languageId);
                cmd.Parameters.AddWithValue("@CountryID", countryId);
                cmd.Parameters.AddWithValue("@Date", date);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                storeSearchResults.searchResultCores = new List<searchResultCore>();
                storeSearchResults.searchResultImages = new List<searchResultImage>();
                storeSearchResults.searchResultPrices = new List<searchResultPrice>();
                storeSearchResults.searchResultAttributes = new List<searchResultAttribute>();

                while (reader.Read())
                {
                    byte tempProductTypeID = 0;
                    if (reader[1] == DBNull.Value) { }
                    else
                    {
                        tempProductTypeID = (byte)reader[1];
                    }
                    storeSearchResults.searchResultCores.Add(new searchResultCore
                    {
                        ProductPageID = (int)reader[0],
                        ProductTypeID = tempProductTypeID,
                        New = (Boolean)reader[2],
                        URLKeywords = reader[3].ToString(),
                        Heading = reader[4].ToString(),
                        SubHeading = reader[5].ToString()
                    });
                }


                reader.NextResult();

                while (reader.Read())
                {
                    storeSearchResults.searchResultImages.Add(new searchResultImage
                    {
                        ProductPageID = (int)reader[0],
                        ImageID = (Int64)reader[1],
                        SortOrder = (byte)reader[2]
                    });
                }

                reader.NextResult();

                while (reader.Read())
                {
                    decimal.TryParse(reader[1].ToString(), out decimal _priceIncMin);
                    decimal.TryParse(reader[2].ToString(), out decimal _pPriceIncMax);
                    decimal.TryParse(reader[3].ToString(), out decimal _salePriceIncMin);
                    decimal.TryParse(reader[4].ToString(), out decimal _salePriceIncMax);

                    decimal? _priceIncMinUse = null;
                    decimal? _pPriceIncMaxUse = null;
                    decimal? _salePriceIncMinUse = null;
                    decimal? _salePriceIncMaxUse = null;

                    if (_priceIncMin == 0) { }
                    else
                    {
                        _priceIncMinUse = _priceIncMin;
                    }
                    if (_pPriceIncMax == 0) { }
                    else
                    {
                        _pPriceIncMaxUse = _pPriceIncMax;
                    }
                    if (_salePriceIncMin == 0) { }
                    else
                    {
                        _salePriceIncMinUse = _salePriceIncMin;
                    }
                    if (_salePriceIncMax == 0) { }
                    else
                    {
                        _salePriceIncMaxUse = _salePriceIncMax;
                    }

                    storeSearchResults.searchResultPrices.Add(new searchResultPrice
                    {
                        ProductPageID = (int)reader[0],
                        PriceIncMin = _priceIncMinUse,
                        PriceIncMax = _pPriceIncMaxUse,
                        SalePriceIncMin = _salePriceIncMinUse,
                        SalePriceIncMax = _salePriceIncMaxUse,
                        DiscountMessage = reader[5].ToString()
                    });
                }

                reader.NextResult();

                while (reader.Read())
                {
                    storeSearchResults.searchResultAttributes.Add(new searchResultAttribute
                    {
                        ProductPageID = (int)reader[0],
                        AttributeID = (Int16)reader[1]
                    });
                }


            }


            return storeSearchResults;

        }


        #endregion


        #region Save Search Keywords


        public Boolean callSaveSearchKeywords(Int64 visitorDbId, String searchKeyword, byte languageId, short countryId)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.SearchLog_Insert", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };


                cmd.Parameters.AddWithValue("@SearchTerm", searchKeyword);
                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                cmd.Parameters.AddWithValue("@CountryID", countryId);
                cmd.Parameters.AddWithValue("@LanguageID", languageId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return true;
        }


        #endregion


    }


    public partial class databaseDataContext
    {
        [Function(Name = "pub.Search_SelectProductTypes")]
        [ResultTypeAttribute(typeof(ProductType))]
        public IMultipleResults Search_ProductType_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<DateTime> date)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), countryID, languageID, date);
            return (IMultipleResults)result.ReturnValue;

        }

        [Function(Name = "pub.Search_Hint")]
        [ResultTypeAttribute(typeof(collectionSearch))]
        [ResultTypeAttribute(typeof(designSearch))]
        [ResultTypeAttribute(typeof(productSearch))]
        [ResultTypeAttribute(typeof(brandSearch))]
        [ResultTypeAttribute(typeof(discontinuedSearch))]
        public IMultipleResults SearchHint_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Keyword", DbType = "NVarChar(20)")] string SearchKeyword,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<DateTime> date,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "ProductTypeID", DbType = "TinyInt")] System.Nullable<byte> productTypeId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAccountID", DbType = "Int")] System.Nullable<int> tradeAccountId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsTrade", DbType = "Bit")] bool isTrade)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), SearchKeyword, countryID, languageID, date, productTypeId, tradeAccountId, isTrade);
            return (IMultipleResults)result.ReturnValue;
        }

        [Function(Name = "pub.Keyword_Search")]
        [ResultTypeAttribute(typeof(predictiveSearch))]
        public IMultipleResults SearchPredictive_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Keyword", DbType = "NVarChar(20)")] string SearchKeyword,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), SearchKeyword, countryID, languageID);
            return (IMultipleResults)result.ReturnValue;

        }

        internal async Task<IEnumerable<FilterIndexAttributeCombination>> FetchFilterIndexAttributeCombinationsAsync(byte languageId, byte? productTypeId, string siteUrl)
        {
            var db = new databaseDataContext();

            return await db.Connection.QueryAsync<FilterIndexAttributeCombination>(@"pub.FilterIndexCombination_Select", new { 
                ProductTypeID = productTypeId,
                LanguageID = languageId,
                Culture = siteUrl
            }, null, 30, CommandType.StoredProcedure);
        } 

        internal IEnumerable<FilterIndexAttributeCombination> FetchFilterIndexAttributeCombinations(byte languageId, string siteUrl)
        {
            var db = new databaseDataContext();

            return db.Connection.Query<FilterIndexAttributeCombination>(@"pub.FilterIndexCombination_Select", new { 
                LanguageID = languageId,
                Culture = siteUrl
            }, null, true, 30, CommandType.StoredProcedure);
        }
    }
}