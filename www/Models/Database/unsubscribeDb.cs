using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Reflection;
using System.Data.Linq.Mapping;
using wwwDesignersGuild.Models.Classes;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace wwwDesignersGuild.Models.Database
{
    public partial class databaseDataContext
    {

        [Function(Name = "pub.UnsubscribeReason_SelectList")]
        [ResultTypeAttribute(typeof(unsubscribeReason))]
        public IMultipleResults Unsubscribe_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsMember", DbType = "Bit")] Boolean isMember,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsTrade", DbType = "Bit")] Boolean isTrade)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), languageID, countryID, date, isMember, isTrade);
            return (IMultipleResults)result.ReturnValue;
        }

        public bool UnsubscribeMember(MemberUnsubscribe objUnsub)
        {
            var result = true;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();
                comm.CommandType = CommandType.StoredProcedure;

                comm.CommandText = "pub.Member_Unsubscribe";

                comm.Parameters.AddWithValue("@GUID", objUnsub.guid ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@Email", objUnsub.email);
                comm.Parameters.AddWithValue("@UnsubscribeReasonID", objUnsub.reasonId ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@SourceEmailName", objUnsub.emailName ?? (object)DBNull.Value);
                
                conn.Open();

                var reader = comm.ExecuteReader();

                while (reader.Read())
                {

                }

                conn.Close();
            }

            return result;
        }

        public bool BeginMemberResubscribe(Guid guid)
        {
            var result = true;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();
                comm.CommandType = System.Data.CommandType.StoredProcedure;

                comm.CommandText = "pub.Member_Resubscribe_Begin";

                comm.Parameters.AddWithValue("@GUID", guid);

                conn.Open();

                var reader = comm.ExecuteReader();

                while (reader.Read())
                {

                }

                conn.Close();
            }

            return result;
        }

        public bool ResubscribeMember(Guid guid)
        {
            var result = true;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();
                comm.CommandType = System.Data.CommandType.StoredProcedure;

                comm.CommandText = "pub.Member_Resubscribe";

                comm.Parameters.AddWithValue("@GUID", guid);

                conn.Open();

                var reader = comm.ExecuteReader();

                while (reader.Read())
                {

                }

                conn.Close();
            }

            return result;
        }

        public MarketingPermissionResult MarketingPermissionCheck(webapiMarketingPermissionCheck objPermissionCheck)
        {
            var result = new MarketingPermissionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();
                comm.CommandType = System.Data.CommandType.StoredProcedure;

                comm.CommandText = "pub.Member_MarketingPermission_Check";

                comm.Parameters.AddWithValue("@VisitorID", objPermissionCheck.VisitorID);
                comm.Parameters.AddWithValue("@MemberID", objPermissionCheck.MemberID ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@Email", objPermissionCheck.Email);
                comm.Parameters.AddWithValue("@AddressRequired", objPermissionCheck.AddressRequired);
                comm.Parameters.AddWithValue("@CountryID", objPermissionCheck.CountryID);
                comm.Parameters.AddWithValue("@LanguageID", objPermissionCheck.LanguageID);
                comm.Parameters.AddWithValue("@MarketingCountryID", objPermissionCheck.MarketingCountryID ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@Date", DateTime.Now);
                comm.Parameters.AddWithValue("@SourceID", objPermissionCheck.SourceID);
                comm.Parameters.AddWithValue("@ThirdPartyID", objPermissionCheck.ThirdPartyID ?? (object)DBNull.Value);

                conn.Open();

                var reader = comm.ExecuteReader();

                while (reader.Read())
                {
                    result.Permissions.Add(new MarketingPermission()
                    {
                        MarketingPermissionID = reader.GetByte(0),
                        Name = reader.GetString(1),
                        Default = reader.GetBoolean(2),
                        Hidden = reader.GetBoolean(3),
                        Label = reader.GetString(4),
                        CurrentStatus = reader.GetBoolean(5),
                        Checked = reader.GetBoolean(5)
                    });
                }

                reader.NextResult();

                while(reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        result.EmailMarketingPermitted = reader.GetBoolean(0);
                        result.UnsubscribedFromEmailMarketing = reader.GetBoolean(1);
                    }
                }

                conn.Close();
            }

            return result;
        }


        // might be an appropriate place for our other functionality: calling unsubscribe_save should immediately unsubscribe from all sub accounts...


        //public tradeOfferDetail TradeOffer_Select(int tradeAccountId)
        //{
        //    tradeOfferDetail result = new tradeOfferDetail();

        //    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
        //    {
        //        SqlCommand comm = conn.CreateCommand();
        //        comm.CommandType = System.Data.CommandType.StoredProcedure;

        //        comm.CommandText = "pub.TradeOffer_Select";

        //        comm.Parameters.AddWithValue("@TradeAccountID", tradeAccountId);

        //        conn.Open();

        //        var reader = comm.ExecuteReader();

        //        while(reader.Read())
        //        {
        //            result.TradeCurrencyCode = reader.IsDBNull(0) ? null : reader.GetString(0);
        //            result.AllowTradePrice = reader.IsDBNull(1) ? false : reader.GetBoolean(1);
        //            result.FreeSamples = reader.IsDBNull(2) ? false : reader.GetBoolean(2);
        //            result.MarginBoosterPercentage = reader.IsDBNull(3) ? (byte)0 : reader.GetByte(3);
        //            result.RetailOnlyDiscountPercentage = reader.IsDBNull(4) ? (byte)0 : reader.GetByte(4);
        //        }

        //        conn.Close();
        //    }

        //    return result;
        //}
    }
}