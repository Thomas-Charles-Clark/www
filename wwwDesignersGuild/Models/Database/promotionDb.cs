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
using wwwDesignersGuild.Models.Classes;
using System.Xml.Linq;
using System.Xml;
using Newtonsoft.Json.Linq;
using wwwDesignersGuild.Controllers;
using System.Data.SqlTypes;

namespace wwwDesignersGuild.Models.Database
{


    public class promotionDb
    {


        public passPromotionCheck callCheckPromotionCode(String promotionCode, DateTime useDate, short countryId, byte languageId, Int64 visitorDbId, Int32? memberId, Int32? tradeAccountId, bool displayTradePrice)
        {
            var objReturn = new passPromotionCheck();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.PromotionCode_Validate", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                cmd.Parameters.AddWithValue("@MemberID", memberId);
                cmd.Parameters.AddWithValue("@IsTrade", tradeAccountId.HasValue);
                cmd.Parameters.AddWithValue("@DisplayTradePrice", displayTradePrice);
                cmd.Parameters.AddWithValue("@TradeAccountID", (object)tradeAccountId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Date", useDate);
                cmd.Parameters.AddWithValue("@CountryID", countryId);
                cmd.Parameters.AddWithValue("@LanguageID", languageId);
                cmd.Parameters.AddWithValue("@Code", promotionCode);

                SqlParameter promotionCodeDetailsJson = cmd.Parameters.Add("@PromotionCodeDetailsJSON", SqlDbType.NVarChar, -1);
                promotionCodeDetailsJson.Direction = ParameterDirection.Output;

                SqlParameter validationJson = cmd.Parameters.Add("@ValidationJSON", SqlDbType.NVarChar, -1);
                validationJson.Direction = ParameterDirection.Output;
                
                conn.Open();
                cmd.ExecuteNonQuery();

                objReturn.passed = false;

                objReturn.promoCodeDetails = Convert.ToString(promotionCodeDetailsJson.Value);

                objReturn.validation = Convert.ToString(validationJson.Value);
            }

            return objReturn;
        }


        public Boolean callAddPromotionCode(Int64 visitorDbId, int? memberId, Int16? promotionCodeId, short countryId, string promoCode)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.PromotionCode_Add", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                cmd.Parameters.AddWithValue("@MemberID", memberId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PromotionCodeID", promotionCodeId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Code", promoCode ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CountryID", countryId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return true;

        }


        public Boolean callDeletePromotionCode(Int64 visitorDbId, Int16 promotionCodeId, short countryId)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.PromotionCode_Delete", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                cmd.Parameters.AddWithValue("@PromotionCodeID", promotionCodeId);
                cmd.Parameters.AddWithValue("@CountryID", countryId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return true;

        }
        

        public XDocument DownloadClearanceList(int tradeAccountId, short countryId, byte languageId, IEnumerable<int> productPageIds = null, bool xlsx = false)
        {
            XDocument result = new XDocument();
            
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ConnectionString))
                {
                    SqlCommand comm = new SqlCommand("pub.TradeClearance_SelectList", conn)
                    {
                        CommandType = System.Data.CommandType.StoredProcedure
                    };

                    comm.Parameters.AddWithValue("@TradeAccountID", tradeAccountId);
                    comm.Parameters.AddWithValue("@CountryID", countryId);
                    comm.Parameters.AddWithValue("@LanguageID", languageId);
                    comm.Parameters.AddWithValue("@DestinationFileType", xlsx ? "xls" : (object)DBNull.Value);

                    if (productPageIds != null)
                    {
                        var dt = new DataTable();

                        dt.Columns.Add("ProductPageID", typeof(int));

                        foreach(var i in productPageIds)
                        {
                            dt.Rows.Add(i);
                        }

                        var p = comm.Parameters.AddWithValue("@ProductPageIDs", dt);
                        p.SqlDbType = SqlDbType.Structured;
                        p.TypeName = "[dbo].[ProductPageIDsTableType]";
                    }

                    conn.Open();

                    XmlReader reader = comm.ExecuteXmlReader();

                    while (reader.Read())
                    {
                        result = XDocument.Parse(reader.ReadOuterXml());
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
    }
}