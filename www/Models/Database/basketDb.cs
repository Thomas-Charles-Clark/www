using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Reflection;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Classes;
using Newtonsoft.Json;
using wwwDesignersGuild.ViewModels;

namespace wwwDesignersGuild.Models.Database
{
    public class basketDb
    {
        public AddToBasketReturn callAddToBasket(Int64 visitorDbId, Int32? memberId, Byte priceListId, Int32? tradeAccountId, Int32 productId, decimal quantity, Int16 countryId, Byte languageId, DateTime useDate, int? productPageId = null, byte basketLineTypeId = 1)
        {
            var result = new AddToBasketReturn
            {
                totalDisplayQuantity = -1,
                quantity = -1
            };

            decimal? valueDecimal = 0.0M;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Basket_Add", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                cmd.Parameters.AddWithValue("@MemberID", memberId);
                cmd.Parameters.AddWithValue("@PriceListID", priceListId);
                cmd.Parameters.AddWithValue("@ProductID", productId);
                cmd.Parameters.AddWithValue("@Quantity", quantity);
                cmd.Parameters.AddWithValue("@CountryID", countryId);
                cmd.Parameters.AddWithValue("@LanguageID", languageId);
                cmd.Parameters.AddWithValue("@TradeAccountID", tradeAccountId);
                cmd.Parameters.AddWithValue("@Date", useDate);
                cmd.Parameters.AddWithValue("@ProductPageID", productPageId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@BasketLineTypeID", basketLineTypeId);

                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result.totalDisplayQuantity = reader.IsDBNull(0) ? null : (decimal?)reader[0];
                    result.quantity = reader.IsDBNull(1) ? null : (decimal?)reader[1];
                    result.basketName = reader.IsDBNull(2) ? null : reader.GetString(2);
                    result.code = reader.IsDBNull(3) ? null : reader.GetString(3);
                    result.currency = reader.IsDBNull(4) ? null : reader.GetString(4);
                    valueDecimal = reader.IsDBNull(5) ? null : (Decimal?)reader[5];
                    result.stockUnit = reader.IsDBNull(6) ? null : reader.GetString(6);
                    result.stockUnitId = reader.IsDBNull(7) ? (byte)0 : reader.GetByte(7);
                }
            }

            if (valueDecimal == null)
            {
                result.price = 0.0M;
            }
            else
            {
                result.price = valueDecimal;
            }

            return result;
        }
        
        public Boolean callUpdateToBasket(Int64 visitorDbId, Int32? memberId, Int32 productId, byte basketLineTypeId, short countryId, decimal quantity, Boolean savedForLaterFlag)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Basket_Update", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                cmd.Parameters.AddWithValue("@MemberID", memberId);
                cmd.Parameters.AddWithValue("@BasketLineTypeID", basketLineTypeId);
                cmd.Parameters.AddWithValue("@ProductID", productId);
                cmd.Parameters.AddWithValue("@Quantity", quantity);
                cmd.Parameters.AddWithValue("@CountryID", countryId);
                cmd.Parameters.AddWithValue("@SaveForLater", savedForLaterFlag);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return true;

        }
        
        public Boolean callSaveForLaterBasket(Int64 visitorDbId, Int32? memberId, Int32 productId, byte basketLineTypeId, short countryId, Boolean saveForLater)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Basket_SaveForLater_Toggle", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                cmd.Parameters.AddWithValue("@MemberID", memberId);
                cmd.Parameters.AddWithValue("@ProductID", productId);
                cmd.Parameters.AddWithValue("@BasketLineTypeID", basketLineTypeId);
                cmd.Parameters.AddWithValue("@CountryID", countryId);
                cmd.Parameters.AddWithValue("@SaveForLater", saveForLater);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return true;
        }
        
        public TrackingJsonViewModel callGetBasketBronto(long visitorDbId, int? memberId, int? pageId, long? checkoutId, short countryId, byte languageId, Int32? tradeAccountId, DateTime date)
        {
            databaseDataContext db = new databaseDataContext();

            var result = new TrackingJsonViewModel();

            var ret = db.Bronto_Basket_Fetch(visitorDbId, memberId, pageId, checkoutId, countryId, languageId, tradeAccountId, date);

            result.Header = ret.GetResult<brontoJSONBasketHeader>().Single();
            result.Lines = ret.GetResult<brontoJSONBasketLine>().ToList();

            result.DotDigitalBehaviourProfile = db.DotDigital_TrackingBehaviourProfileID_Fetch(countryId, languageId).Single();

            return result;
        }

        public DotDigitalJSON callGetBasketDotDigital(Int64 visitorDbId, Int32? memberId, Int16 countryId, Byte languageId, bool checkoutConfirmation)
        {
            databaseDataContext db = new databaseDataContext();

            return db.DotDigital_Basket_Fetch(visitorDbId, memberId, countryId, languageId, checkoutConfirmation).Single();
        }

        public bool callMemberBasketClone(int sourceMemberId, int destinationMemberId, short countryId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("adm.Member_Basket_Clone", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@SourceMemberID", sourceMemberId);
                cmd.Parameters.AddWithValue("@DestinationMemberID", destinationMemberId);
                cmd.Parameters.AddWithValue("@CountryID", countryId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return true;
        }
    }


    public partial class databaseDataContext
    {
        [Function(Name = "pub.Basket_TotalQuantity_Select")]
        [ResultTypeAttribute(typeof(BasketTotalQuantity))]
        public ISingleResult<BasketTotalQuantity> Basket_TotalQuantity_Select([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorDbId,
                                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "Int")] System.Nullable<Int32> memberId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), visitorDbId, memberId, countryID);
            return (ISingleResult<BasketTotalQuantity>)result.ReturnValue;
        }

        [Function(Name = "pub.Basket_Summary")]
        [ResultTypeAttribute(typeof(BasketTotal))]
        [ResultTypeAttribute(typeof(BasketDetail))]
        public IMultipleResults BasketSummary_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorDbId,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "Int")] System.Nullable<Int32> memberId,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAccountID", DbType = "Int")] System.Nullable<Int32> tradeAccountId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "DisplayTradePrice", DbType = "Bit")] bool displayTradePrice,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsTrade", DbType = "Bit")] bool isTrade)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), visitorDbId, memberId, tradeAccountId, countryID, languageID, date, displayTradePrice, isTrade);
            return (IMultipleResults)result.ReturnValue;

        }


        [Function(Name = "pub.Message_Select")]
        [ResultTypeAttribute(typeof(promotionalMessage))]
        public IMultipleResults PromotionalMessages_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsTrade", DbType = "Bit")] bool isTrade,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorDbId,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), isTrade, visitorDbId, countryID, languageID, date);
            return (IMultipleResults)result.ReturnValue;

        }


        [Function(Name = "pub.Basket_LineDetails")]
        [ResultTypeAttribute(typeof(BasketDetail))]
        public IMultipleResults BasketLineDetail_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorDbId,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "Int")] System.Nullable<Int32> memberId,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "ProductID", DbType = "Int")] System.Nullable<Int32> productId,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), visitorDbId, memberId, productId, countryID, languageID, date);
            return (IMultipleResults)result.ReturnValue;

        }

        [Function(Name = "pub.Basket_Generate")]
        [ResultTypeAttribute(typeof(BasketTotal))]
        [ResultTypeAttribute(typeof(BasketDetail))]
        [ResultTypeAttribute(typeof(BasketDeliveryOption))]
        [ResultTypeAttribute(typeof(basketRecommendation))]
        [ResultTypeAttribute(typeof(BasketPromotionalMessage))]
        [ResultTypeAttribute(typeof(BasketErrorMessage))]
        [ResultType(typeof(PromotionCodeDetails))]
        public IMultipleResults BasketDetail_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorDbId,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "Int")] System.Nullable<Int32> memberId,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAccountID", DbType = "Int")] System.Nullable<int> memberTradeAccountID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsTrade", DbType = "Bit")] bool isTrade,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "DisplayTradePrice", DbType = "Bit")] bool displayTradePrice)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), visitorDbId, memberId, countryID, languageID, date, memberTradeAccountID, isTrade, displayTradePrice);
            return (IMultipleResults)result.ReturnValue;

        }


        [Function(Name = "pub.QuickOrder_Basket_Generate")]
        [ResultTypeAttribute(typeof(BasketTotal))]
        [ResultTypeAttribute(typeof(BasketDetail))]
        public IMultipleResults BasketDetail_GenerateQuickMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorDbId,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "Int")] System.Nullable<Int32> memberId,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAccountID", DbType = "Int")] System.Nullable<int> memberTradeAccountID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsTrade", DbType = "Bit")] bool isTrade,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "DisplayTradePrice", DbType = "Bit")] bool displayTradePrice)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), visitorDbId, memberId, countryID, languageID, date, memberTradeAccountID, isTrade, displayTradePrice);
            return (IMultipleResults)result.ReturnValue;

        }


        [Function(Name = "pub.Product_Stock_Select")]
        [ResultTypeAttribute(typeof(BasketProductTotal))]
        [ResultTypeAttribute(typeof(StockStatusSelect))]
        public IMultipleResults ProductStockSelect_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorDbId, 
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "Int")] System.Nullable<Int32> memberId,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "ProductID", DbType = "Int")] System.Nullable<Int32> productId,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "SaveForLater", DbType = "Bit")] System.Nullable<Boolean> saveForLater)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), visitorDbId, memberId, productId, countryID, saveForLater);
            return (IMultipleResults)result.ReturnValue;

        }


        [Function(Name = "pub.Bronto_Basket_Fetch")]
        [ResultTypeAttribute(typeof(brontoJSONBasketHeader))]
        [ResultTypeAttribute(typeof(brontoJSONBasketLine))]
        public IMultipleResults Bronto_Basket_Fetch([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorDbId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "Int")] System.Nullable<Int32> memberId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "PageID", DbType = "Int")] System.Nullable<Int32> pageId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CheckoutID", DbType = "BigInt")] System.Nullable<Int64> checkoutId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] Int16 countryId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] Byte languageId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAccountID", DbType = "Int")] System.Nullable<Int32> tradeAccountId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), visitorDbId, memberId, pageId, checkoutId, countryId, languageId, tradeAccountId, date);
            return (IMultipleResults)result.ReturnValue;

        }


        [Function(Name = "pub.DotDigital_TrackingBehaviourProfile_Select")]
        [ResultTypeAttribute(typeof(DotDigitalTrackingBehaviourProfile))]
        public ISingleResult<DotDigitalTrackingBehaviourProfile> DotDigital_TrackingBehaviourProfileID_Fetch( [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] Int16 countryId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] Byte languageId)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), countryId, languageId);
            return (ISingleResult<DotDigitalTrackingBehaviourProfile>)result.ReturnValue;

        }


        [Function(Name = "pub.DotDigital_Basket_Select")]
        [ResultTypeAttribute(typeof(DotDigitalJSON))]
        public ISingleResult<DotDigitalJSON> DotDigital_Basket_Fetch([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorDbId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "Int")] System.Nullable<Int32> memberId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] Int16 countryId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] Byte languageId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CheckoutConfirmation", DbType = "BigInt")] bool checkoutConfirmation)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), visitorDbId, memberId, countryId, languageId, checkoutConfirmation);

            return (ISingleResult<DotDigitalJSON>)result.ReturnValue;
        }
    }
}