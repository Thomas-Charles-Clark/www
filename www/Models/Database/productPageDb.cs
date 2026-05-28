using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Reflection;
using System.Data.Linq.Mapping;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Classes;
using System.Data.SqlClient;
using System.Configuration;
using Newtonsoft.Json;
using wwwDesignersGuild.Controllers;
using System.Data;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.Models.Database
{
    public partial class databaseDataContext
    {
        [Function(Name = "cache.ProductPage_Select")]
        [ResultTypeAttribute(typeof(productPageCacheSelect))]
        [ResultTypeAttribute(typeof(productPageUnavailableRedirect))]
        public IMultipleResults ProductPage_Select([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "ProductPageID", DbType = "Int")] int productPageID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] byte languageID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] short countryID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] DateTime date,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsTrade", DbType = "Bit")] bool isTrade,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "PlatformSwitch", DbType = "Bit")] bool? platformSwitch)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), productPageID, languageID, countryID, date, isTrade, platformSwitch);
            return (IMultipleResults)result.ReturnValue;
        }

        [Function(Name = "pub.ProductPage_Generate")]
        [ResultTypeAttribute(typeof(productPageHeader))]
        [ResultTypeAttribute(typeof(productPageReviewSummary))]
        [ResultTypeAttribute(typeof(productPageReviewLastFive))]
        [ResultTypeAttribute(typeof(productPageInformation))]
        [ResultTypeAttribute(typeof(searchResultRenderImage))]
        [ResultTypeAttribute(typeof(productPageImageHotspot))]
        [ResultTypeAttribute(typeof(productPageAlternateColourway))]
        [ResultTypeAttribute(typeof(searchResultRenderCore))]
        [ResultTypeAttribute(typeof(searchResultRenderImage))]
        [ResultTypeAttribute(typeof(productPageCollection))]
        [ResultTypeAttribute(typeof(productPageTechInfoCore))]
        [ResultTypeAttribute(typeof(productPageTechInfoAftercare))]
        [ResultTypeAttribute(typeof(productPageTechInfoUsage))]
        [ResultTypeAttribute(typeof(productPageTechInfoTest))]
        [ResultTypeAttribute(typeof(GoogleHref))]
        [ResultTypeAttribute(typeof(productPageBrochure))]
        [ResultTypeAttribute(typeof(productPageRRP))]
        public IMultipleResults ProductPage_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "ProductPageID", DbType = "Int")] System.Nullable<int> productPageID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsMember", DbType = "Bit")] bool isMember,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsTrade", DbType = "Bit")] bool isTrade,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAccountID", DbType = "Int")] int? tradeAccountId,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "DisplayTradePrice", DbType = "Bit")] bool displayTradePrice)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), productPageID, languageID, countryID, date, isMember, isTrade, tradeAccountId, displayTradePrice);
            return (IMultipleResults)result.ReturnValue;

        }

        [Function(Name = "cache.ProductPage_OrderForm_Select")]
        public ISingleResult<cmsRealTimeStock> ProductPage_OrderForm_Select([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] Int64 visitorId,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "Int")] System.Nullable<Int32> memberId,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "ProductPageID", DbType = "Int")] System.Nullable<int> productPageID,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAccountID", DbType = "Int")] System.Nullable<int> memberTradeAccountID,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "DisplayTradePrice", DbType = "Bit")] bool displayTradePrice,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsTrade", DbType = "Bit")] bool isTrade,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsLifestyle", DbType = "Bit")] bool isLifestyle,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryMatch", DbType = "Bit")] bool countryMatch)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), visitorId, memberId, productPageID, languageID, countryID, date, memberTradeAccountID, displayTradePrice, isTrade, isLifestyle, countryMatch);
            return (ISingleResult<cmsRealTimeStock>)result.ReturnValue;

        }

        [Function(Name = "pub.ProductPage_OrderForm_Generate")]
        [ResultTypeAttribute(typeof(productPagePrice))]
        [ResultTypeAttribute(typeof(productPageProduct))]
        [ResultTypeAttribute(typeof(BasketSummary))]
        [ResultTypeAttribute(typeof(BasketLineType))]
        public IMultipleResults ProductPage_RealtimeStock_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] Int64 visitorId,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "Int")] System.Nullable<Int32> memberId,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "ProductPageID", DbType = "Int")] System.Nullable<int> productPageID,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAccountID", DbType = "Int")] System.Nullable<int> memberTradeAccountID,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "DisplayTradePrice", DbType = "Bit")] bool displayTradePrice,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsTrade", DbType = "Bit")] bool isTrade)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), visitorId, memberId, productPageID, languageID, countryID, date, memberTradeAccountID, displayTradePrice, isTrade);
            return (IMultipleResults)result.ReturnValue;

        }


        [Function(Name = "pub.ProductPage_QuickLook")]
        [ResultTypeAttribute(typeof(productPageQuickLook))]
        [ResultTypeAttribute(typeof(productPageQuickLookText))]
        public IMultipleResults ProductPage_QuickLook_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] Int64 visitorId,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "Int")] System.Nullable<Int32> memberId,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "ProductPageID", DbType = "Int")] System.Nullable<int> productPageID,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAccountID", DbType = "Int")] System.Nullable<int> memberTradeAccountID)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), visitorId, memberId, productPageID, countryID, languageID, date, memberTradeAccountID);
            return (IMultipleResults)result.ReturnValue;

        }

        [Function(Name = "pub.ProductPage_ProductBundle_Select")]
        [ResultTypeAttribute(typeof(bundledProduct))]
        public ISingleResult<bundledProduct> ProductPage_ProductBundle_GenerateSingle([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "ProductID", DbType = "Int")] Int32 productId,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "PriceListID", DbType = "TinyInt")] System.Nullable<byte> priceListId,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryId,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageId,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), productId, priceListId, countryId, languageId, date);
            return (ISingleResult<bundledProduct>)result.ReturnValue;
        }
        public tradeOfferDetail TradeOffer_Select(int tradeAccountId)
        {
            tradeOfferDetail result = new tradeOfferDetail();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();
                comm.CommandType = System.Data.CommandType.StoredProcedure;

                comm.CommandText = "pub.TradeOffer_Select";

                comm.Parameters.AddWithValue("@TradeAccountID", tradeAccountId);

                conn.Open();

                var reader = comm.ExecuteReader();

                while (reader.Read())
                {
                    result.TradeCurrencyCode = reader.IsDBNull(0) ? null : reader.GetString(0);
                    result.AllowTradePrice = !reader.IsDBNull(1) && reader.GetBoolean(1);
                    result.FreeSamples = !reader.IsDBNull(2) && reader.GetBoolean(2);
                    result.MarginBoosterPercentage = reader.IsDBNull(3) ? (byte)0 : reader.GetByte(3);
                    result.RetailOnlyDiscountPercentage = reader.IsDBNull(4) ? (byte)0 : reader.GetByte(4);
                }

                conn.Close();
            }

            return result;
        }

        public PageSectionJSON FetchProductPageGrid(
            long pageSectionId,
            int? memberId,
            byte languageId,
            short countryId,
            int? tradeAccountId,
            bool countryMatch,
            bool displayTradePrice,
            DateTime useDate,
            int? productPageGridPageId = null,
            int? productPageGridPagSize = null)
        {
            var result = new PageSectionJSON
            {
                attributes = new List<pageSectionAttribute>(),

                PageSectionID = pageSectionId
            };

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();

                comm.CommandText = @"cache.ProductPageGrid_Select";
                comm.CommandType = System.Data.CommandType.StoredProcedure;

                comm.Parameters.AddWithValue("@PageSectionID", pageSectionId);
                comm.Parameters.AddWithValue("@LanguageID", languageId);
                comm.Parameters.AddWithValue("@CountryID", countryId);
                comm.Parameters.AddWithValue("@Date", useDate);
                comm.Parameters.AddWithValue("@IsMember", memberId.HasValue);
                comm.Parameters.AddWithValue("@IsTrade", tradeAccountId.HasValue);
                comm.Parameters.AddWithValue("@MemberID", memberId ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@DisplayTradePrice", displayTradePrice);
                comm.Parameters.AddWithValue("@CountryMatch", countryMatch);
                comm.Parameters.AddWithValue("@TradeAccountID", tradeAccountId ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@PageNumber", productPageGridPageId ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@PageSize", productPageGridPagSize ?? (object)DBNull.Value);

                conn.Open();

                var reader = comm.ExecuteReader();

                while (reader.Read())
                {
                    result.JSON = reader.IsDBNull(0) ? "" : reader.GetString(0);
                }

                reader.NextResult();

                while (reader.Read())
                {
                    result.attributes.Add(new pageSectionAttribute()
                    {
                        ProductPageID = reader.GetInt32(0),
                        AttributeID = reader.GetInt16(1)
                    });
                }

                result.totals = new PageSectionProductPageTotals();

				reader.NextResult();

				while (reader.Read())
				{
                    result.totals.ProductPageIDs.Add(reader.GetInt32(0));
				}

				conn.Close();
            }

            return result;
        }

        public PageSectionJSON FetchCollectionProductPageGrid(short collectionId, int? memberId, byte languageId, short countryId, int? tradeAccountId, bool displayTradePrice, DateTime useDate)
        {
            var result = new PageSectionJSON
            {
                attributes = new List<pageSectionAttribute>(),

                PageSectionID = 0
            };

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();

                comm.CommandText = @"cache.Collection_ProductPageGrid_Select";
                comm.CommandType = System.Data.CommandType.StoredProcedure;

                comm.Parameters.AddWithValue("@CollectionID", collectionId);
                comm.Parameters.AddWithValue("@LanguageID", languageId);
                comm.Parameters.AddWithValue("@CountryID", countryId);
                comm.Parameters.AddWithValue("@Date", useDate);
                comm.Parameters.AddWithValue("@IsMember", memberId.HasValue);
                comm.Parameters.AddWithValue("@IsTrade", tradeAccountId.HasValue);
                comm.Parameters.AddWithValue("@MemberID", memberId ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@DisplayTradePrice", displayTradePrice);
                comm.Parameters.AddWithValue("@CountryMatch", Convert.ToBoolean(HttpContext.Current.Session["countryCheckMatched"]));
                comm.Parameters.AddWithValue("@TradeAccountID", tradeAccountId ?? (object)DBNull.Value);

                conn.Open();

                var reader = comm.ExecuteReader();

                while (reader.Read())
                {
                    result.JSON = reader.IsDBNull(0) ? "" : reader.GetString(0);
                }

                conn.Close();
            }

            return result;
        }

        public async Task<DatabaseJSONResponse> ProductPage_Price_SelectAsync(IEnumerable<int> productPageIds, VisitorSelect objVisitor, NewMasterDefinition objMaster, bool isLifestyle = false)
        {
            var result = new DatabaseJSONResponse();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();

                comm.CommandText = @"cache.ProductPage_Price_Select";
                comm.CommandType = CommandType.StoredProcedure;

                var dt = new DataTable();

                dt.Columns.Add("ProductPageID");

                foreach (var pp in productPageIds)
                {
                    dt.Rows.Add(pp);
                }

                comm.Parameters.AddWithValue("@ProductPageIDs", dt);
                comm.Parameters.AddWithValue("@CountryID", objMaster.UrlBreakdown.countryDetails.CountryID);
                comm.Parameters.AddWithValue("@LanguageID", objMaster.UrlBreakdown.countryDetails.LanguageID);
                comm.Parameters.AddWithValue("@Date", objMaster.UrlBreakdown.ContentDate);
                comm.Parameters.AddWithValue("@VisitorID", objVisitor.visitorDbId);
                comm.Parameters.AddWithValue("@MemberID", objVisitor.memberId ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@TradeAccountID", objVisitor.memberTradeAccountID ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@IsTrade", objVisitor.memberTradeAccountID.HasValue);
                comm.Parameters.AddWithValue("@CountryMatch", Convert.ToBoolean(HttpContext.Current.Session["countryCheckMatched"]));
                comm.Parameters.AddWithValue("@DisplayTradePrice", objVisitor.memberDisplayTradePrice);
                comm.Parameters.AddWithValue("@IsLifestyle", isLifestyle);

                conn.Open();

                var reader = await comm.ExecuteReaderAsync();

                while (reader.Read())
                {
                    result.JSON = reader.IsDBNull(0) ? "" : reader.GetString(0);
                }

                conn.Close();
            }

            return result;
        }

        public DatabaseJSONResponse ProductPage_Price_Select(IEnumerable<int> productPageIds, VisitorSelect objVisitor, NewMasterDefinition objMaster)
        {
            var result = new DatabaseJSONResponse();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();

                comm.CommandText = @"cache.ProductPage_Price_Select";
                comm.CommandType = CommandType.StoredProcedure;

                var dt = new DataTable();

                dt.Columns.Add("ProductPageID");

                foreach (var pp in productPageIds)
                {
                    dt.Rows.Add(pp);
                }

                comm.Parameters.AddWithValue("@ProductPageIDs", dt);
                comm.Parameters.AddWithValue("@CountryID", objMaster.UrlBreakdown.countryDetails.CountryID);
                comm.Parameters.AddWithValue("@LanguageID", objMaster.UrlBreakdown.countryDetails.LanguageID);
                comm.Parameters.AddWithValue("@Date", objMaster.UrlBreakdown.ContentDate);
                comm.Parameters.AddWithValue("@VisitorID", objVisitor.visitorDbId);
                comm.Parameters.AddWithValue("@MemberID", objVisitor.memberId ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@TradeAccountID", objVisitor.memberTradeAccountID ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@IsTrade", objVisitor.memberTradeAccountID.HasValue);
                comm.Parameters.AddWithValue("@CountryMatch", Convert.ToBoolean(HttpContext.Current.Session["countryCheckMatched"]));
                comm.Parameters.AddWithValue("@DisplayTradePrice", objVisitor.memberDisplayTradePrice);
                comm.Parameters.AddWithValue("@PageID", objMaster.UrlBreakdown.ContentType == Helper.dgContentType.landingPage ? objMaster.UrlBreakdown.ContentId : (object)DBNull.Value);

                conn.Open();

                var reader = comm.ExecuteReader();

                while (reader.Read())
                {
                    result.JSON = reader.IsDBNull(0) ? "" : reader.GetString(0);
                }

                conn.Close();
            }

            return result;
        }

        public DatabaseJSONResponse ProductPage_Price_Select(IEnumerable<int> productPageIds, long? visitorId, int? memberId, int? tradeAccountId, bool displayTradePrice, int? pageId, short countryId, byte languageId, DateTime useDate)
        {
            var result = new DatabaseJSONResponse();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();

                comm.CommandText = @"cache.ProductPage_Price_Select";
                comm.CommandType = CommandType.StoredProcedure;

                var dt = new DataTable();

                dt.Columns.Add("ProductPageID");

                foreach (var pp in productPageIds)
                {
                    dt.Rows.Add(pp);
                }

                comm.Parameters.AddWithValue("@ProductPageIDs", dt);
                comm.Parameters.AddWithValue("@CountryID", countryId);
                comm.Parameters.AddWithValue("@LanguageID", languageId);
                comm.Parameters.AddWithValue("@Date", useDate);
                comm.Parameters.AddWithValue("@VisitorID", visitorId);
                comm.Parameters.AddWithValue("@MemberID", memberId ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@TradeAccountID", tradeAccountId ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@IsTrade", tradeAccountId.HasValue);
                comm.Parameters.AddWithValue("@CountryMatch", Convert.ToBoolean(HttpContext.Current.Session["countryCheckMatched"]));
                comm.Parameters.AddWithValue("@DisplayTradePrice", displayTradePrice);
                comm.Parameters.AddWithValue("@PageID", pageId ?? (object)DBNull.Value);

                conn.Open();

                var reader = comm.ExecuteReader();

                while (reader.Read())
                {
                    result.JSON = reader.IsDBNull(0) ? "" : reader.GetString(0);
                }

                conn.Close();
            }

            return result;
        }


        public DatabaseJSONResponse ProductPage_Price_Select(PassProductPageIDs objProductPages, long? visitorId, int? memberId, int? tradeAccountId, bool displayTradePrice)
        {
            var result = new DatabaseJSONResponse();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();

                comm.CommandText = @"cache.ProductPage_Price_Select";
                comm.CommandType = System.Data.CommandType.StoredProcedure;

                var dt = new DataTable();

                dt.Columns.Add("ProductPageID");

                foreach (var pp in objProductPages.productPageIds)
                {
                    dt.Rows.Add(pp);
                }

                comm.Parameters.AddWithValue("@ProductPageIDs", dt);
                comm.Parameters.AddWithValue("@CountryID", objProductPages.objMaster.UrlBreakdown.countryDetails.CountryID);
                comm.Parameters.AddWithValue("@LanguageID", objProductPages.objMaster.UrlBreakdown.countryDetails.LanguageID);
                comm.Parameters.AddWithValue("@Date", objProductPages.objMaster.UrlBreakdown.ContentDate);
                comm.Parameters.AddWithValue("@VisitorID", visitorId ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@MemberID", memberId ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@TradeAccountID", tradeAccountId ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@IsTrade", tradeAccountId.HasValue);
                comm.Parameters.AddWithValue("@CountryMatch", Convert.ToBoolean(HttpContext.Current.Session["countryCheckMatched"]));
                comm.Parameters.AddWithValue("@DisplayTradePrice", displayTradePrice);

                conn.Open();

                var reader = comm.ExecuteReader();

                while (reader.Read())
                {
                    result.JSON = reader.IsDBNull(0) ? "" : reader.GetString(0);
                }

                conn.Close();
            }

            return result;
        }

        public DatabaseJSONResponse ProductPage_TechInfo_Select(int productPageId, short countryId, byte languageId, DateTime contentDate)
        {
            var result = new DatabaseJSONResponse();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();

                comm.CommandText = @"cache.ProductPage_TechInfo_Select";
                comm.CommandType = System.Data.CommandType.StoredProcedure;

                var dt = new DataTable();


                comm.Parameters.AddWithValue("@ProductPageID", productPageId);
                comm.Parameters.AddWithValue("@CountryID", countryId);
                comm.Parameters.AddWithValue("@LanguageID", languageId);
                comm.Parameters.AddWithValue("@Date", contentDate);

                conn.Open();

                var reader = comm.ExecuteReader();

                while (reader.Read())
                {
                    result.JSON = reader.GetString(0);
                }

                conn.Close();
            }

            return result;
        }

        public async Task<DatabaseJSONResponse> FetchRecentlyViewed(int? productPageId, long visitorId, NewMasterDefinition objMaster)
        {
            var result = new DatabaseJSONResponse();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();

                comm.CommandText = @"cache.Visitor_History_Select";
                comm.CommandType = System.Data.CommandType.StoredProcedure;

                var dt = new DataTable();

                comm.Parameters.AddWithValue("@VisitorID", visitorId);
                comm.Parameters.AddWithValue("@ProductPageID", productPageId ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@CountryID", objMaster.UrlBreakdown.countryDetails.CountryID);
                comm.Parameters.AddWithValue("@LanguageID", objMaster.UrlBreakdown.countryDetails.LanguageID);
                comm.Parameters.AddWithValue("@Date", objMaster.UrlBreakdown.ContentDate);
                comm.Parameters.AddWithValue("@IsTrade", objMaster.TradeMember);

                conn.Open();

                var sb = new StringBuilder();

                var reader = await comm.ExecuteReaderAsync();

                while (reader.Read())
                {
                    sb.Append(reader.GetString(0));
                }

                result.JSON = sb.ToString();

                conn.Close();
            }

            return result;
        }

        public DatabaseJSONResponse FetchRecentlyViewed(PassProductPageID objProductPage, long visitorId, bool isTrade)
        {
            var result = new DatabaseJSONResponse();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();

                comm.CommandText = @"cache.Visitor_History_Select";
                comm.CommandType = System.Data.CommandType.StoredProcedure;

                var dt = new DataTable();

                comm.Parameters.AddWithValue("@VisitorID", visitorId);
                comm.Parameters.AddWithValue("@ProductPageID", objProductPage.productPageId);
                comm.Parameters.AddWithValue("@CountryID", objProductPage.objMaster.UrlBreakdown.countryDetails.CountryID);
                comm.Parameters.AddWithValue("@LanguageID", objProductPage.objMaster.UrlBreakdown.countryDetails.LanguageID);
                comm.Parameters.AddWithValue("@Date", objProductPage.objMaster.UrlBreakdown.ContentDate);
                comm.Parameters.AddWithValue("@IsTrade", isTrade);

                conn.Open();

                var sb = new StringBuilder();

                var reader = comm.ExecuteReader();

                while (reader.Read())
                {
                    sb.Append(reader.GetString(0));
                }

                result.JSON = sb.ToString();

                conn.Close();
            }

            return result;
        }

        public DatabaseJSONResponse FetchModularOptions(int productPageId, short countryId, byte languageId, DateTime contentDate)
        {
            var result = new DatabaseJSONResponse();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();

                comm.CommandText = @"cache.ProductPage_ModuleOption_Select";
                comm.CommandType = System.Data.CommandType.StoredProcedure;

                var dt = new DataTable();

                comm.Parameters.AddWithValue("@ProductPageID", productPageId);
                comm.Parameters.AddWithValue("@CountryID", countryId);
                comm.Parameters.AddWithValue("@LanguageID", languageId);
                comm.Parameters.AddWithValue("@Date", contentDate);

                conn.Open();

                var sb = new StringBuilder();

                var reader = comm.ExecuteReader();

                while (reader.Read())
                {
                    sb.Append(reader.GetString(0));
                }

                result.JSON = sb.ToString();

                conn.Close();
            }

            return result;
        }

        public byte? FetchSiteSection(int pageId)
        {
            var result = (byte?)null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();

                comm.CommandText = @"pub.SiteSection_Select";
                comm.CommandType = CommandType.StoredProcedure;

                comm.Parameters.AddWithValue("@PageID", pageId);

                conn.Open();

                var reader = comm.ExecuteReader();

                while (reader.Read())
                {
                    result = reader.GetByte(0);
                }

                conn.Close();
            }

            return result;
        }
    }

    public class DatabaseJSONResponse
    {
        public string JSON { get; set; }
        public object json
        {
            set
            {

            }
            get
            {
                return JsonConvert.DeserializeObject(JSON ?? "{}");
            }
        }
    }

    public class DatabaseJSONArrayResponse
    {
        public string JSON { get; set; }
        public object json
        {
            set
            {

            }
            get
            {
                return JsonConvert.DeserializeObject(JSON ?? "[]");
            }
        }
    }

    public class DatabaseProductPagesJSONResponse : DatabaseJSONResponse
    {
        new public object json
        {
            set
            {

            }
            get
            {
                return JsonConvert.DeserializeObject(String.IsNullOrEmpty(JSON) ? "{ productPages: [] }" : JSON);
            }
        }
    }
}