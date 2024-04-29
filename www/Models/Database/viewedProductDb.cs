using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Reflection;
using System.Data.Linq.Mapping;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using wwwDesignersGuild.Models.Classes;


namespace wwwDesignersGuild.Models.Database
{


    public partial class databaseDataContext
    {
        [Function(Name = "pub.Visitor_History_Select")]
        [ResultType(typeof(pageSectionProductPageStub))]
        [ResultTypeAttribute(typeof(searchResultRenderCore))]
        [ResultTypeAttribute(typeof(searchResultRenderImage))]
        public IMultipleResults ProductPageHistory_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorDbId,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "ProductPageID", DbType = "Int")] System.Nullable<int> ProductPageID,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAccountID", DbType = "Int")] System.Nullable<int> tradeAccountID,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsMember", DbType = "Int")] Boolean isMember,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsTrade", DbType = "Bit")] bool isTrade,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "DisplayTradePrice", DbType = "Bit")] bool displayTradePrice
                                                        )
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), visitorDbId, countryID, languageID, ProductPageID, date, tradeAccountID, isMember, isTrade, displayTradePrice);
            return (IMultipleResults)result.ReturnValue;
        }
    }


    public class viewedProductDb
    {


        public Boolean productPageHistoryMerge(Int64 visitorDbId, int? productPageId, short countryId)
        {


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Visitor_History_Add", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                cmd.Parameters.AddWithValue("@ProductPageID", productPageId);
                cmd.Parameters.AddWithValue("@CountryID", countryId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return true;


        }
        
        
    }
}