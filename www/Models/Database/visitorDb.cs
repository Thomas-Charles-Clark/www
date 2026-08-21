using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Reflection;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Trade;
using wwwDesignersGuild.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Dapper;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Hotspot;
using wwwDesignersGuild.Util;
using SimpleImpersonation;
using Braintree;
using System.Runtime.CompilerServices;
using wwwDesignersGuild.ViewModels.Trade;

namespace wwwDesignersGuild.Models.Database
{
    public class CountryIDLanguageID
    {
        public short CountryID { get; set; }
        public byte LanguageID { get; set; }
    }

    public class visitorDb
    {
        public DatabaseJSONResponse callTradeAccountRequestAccess(long visitorId, PassTradeAccessRequest objRequest)
        {
            var result = new DatabaseJSONResponse();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("[pub].[TradeAccount_RequestAccess]", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var parameters = new SqlParameter[] 
                {
                    new SqlParameter("@VisitorID", visitorId),
                    new SqlParameter("@TradeAccountName", objRequest.tradeAccountName ?? (object)DBNull.Value),
                    new SqlParameter("@TradeAccountCode", objRequest.tradeAccountCode ?? (object)DBNull.Value),
                    new SqlParameter("@TradeAccountLabel", objRequest.tradeAccountLabel ?? (object)DBNull.Value),
                    new SqlParameter("@UserName", objRequest.userName ?? (object)DBNull.Value),
                    new SqlParameter("@UserEmail", objRequest.userEmail ?? (object)DBNull.Value),
                    new SqlParameter("@UserPhone", objRequest.userPhone ?? (object)DBNull.Value)
                };

                cmd.Parameters.AddRange(parameters);

                conn.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result.JSON = reader.GetString(0);
                }

                conn.Close();
            }

            return result;
        }

        public DatabaseJSONResponse fetchTradeAccountSalesRepresentative(PassTradeAccessRequest objRequest)
        {
            var result = new DatabaseJSONResponse();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("[pub].[TradeAccount_SalesRepresentative_Search]", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@TradeAccountCode", objRequest.tradeAccountCode ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@TradeAccountName", objRequest.tradeAccountName ?? (object)DBNull.Value);

                conn.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result.JSON = reader.GetString(0);
                }

                conn.Close();
            }

            return result;
        }

        public DatabaseJSONResponse fetchTradeDepartmentContact(int? tradeAccountId, byte languageId, string departmentInitials, int? memberId = null)
        {
            var result = new DatabaseJSONResponse();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("[pub].[DesignersGuildContactDetails_Select]", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@TradeAccountID", tradeAccountId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LanguageID", languageId);
                cmd.Parameters.AddWithValue("@MemberID", memberId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DepartmentInitials", departmentInitials);

                conn.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result.JSON = reader.GetString(0);
                }

                conn.Close();
            }

            return result;
        }

        /// <summary>
        /// Sends an email via the portal.Email queue
        /// </summary>
        /// <param name="destination">Destination</param>
        /// <param name="cc">CC</param>
        /// <param name="bcc">BCC</param>
        /// <param name="subject">Subject</param>
        /// <param name="body">Body</param>
        /// <param name="emailTemplateId">EmailTemplateID</param>
        /// <param name="emailSourceId">EmailSourceID</param>
        /// <param name="emailTypeId">EmailTemplateID</param>
        /// <returns></returns>
        internal long addEmailToQueue(string destination,
            string cc,
            string bcc,
            string subject,
            string body,
            short emailTemplateId = 1,
            short emailSourceId = 14,
            short emailTypeId = 2,
            string attachments = null)
        {
            long result = -1;

            var credentials = new UserCredentials("DESIGNERSGUILD\\SQLAgentFileAccess", "34949C02-4677-4711-999C-8E038E26B10F");

            var safetyToken = credentials.LogonUser(LogonType.Interactive);

            System.Security.Principal.WindowsIdentity.RunImpersonated(safetyToken, () =>
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockIntegratedConnectionString"].ToString()))
                {
                    SqlCommand cmd = new SqlCommand("[portal].[Email_Insert]", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@To", destination);
                    cmd.Parameters.AddWithValue("@CC", cc ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BCC", bcc ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Subject", subject);
                    cmd.Parameters.AddWithValue("@Body", body);
                    cmd.Parameters.AddWithValue("@EmailTemplateID", emailTemplateId);
                    cmd.Parameters.AddWithValue("@EmailSourceID", emailSourceId);
                    cmd.Parameters.AddWithValue("@EmailTypeID", emailTypeId);
                    cmd.Parameters.AddWithValue("@FileAttachments", attachments ?? (object)DBNull.Value);
                    var p = cmd.Parameters.AddWithValue("@EmailID", DBNull.Value);

                    p.Direction = ParameterDirection.Output;
                    p.SqlDbType = SqlDbType.BigInt;

                    conn.Open();

                    cmd.ExecuteNonQuery();

                    result = Convert.ToInt64(p.Value);

                    SqlCommand sendCmd = new SqlCommand("[portal].[Email_Send]", conn);

                    sendCmd.Parameters.AddWithValue("@SingleEmailID", result);

                    var sendResult = sendCmd.ExecuteNonQuery();

                    conn.Close();
                }
            });

            return result;
        }

        internal CountryIDLanguageID getTradeAccountCountryLanguage(int tradeAccountId)
        {
            var result = new CountryIDLanguageID();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT 
                        TA.CountryID,
                        ISNULL(TA.LanguageID, CONVERT(TINYINT, 1)) AS LanguageID
                    FROM 
                        dbo.TradeAccount AS TA
                    WHERE
                        TA.TradeAccountID = @TradeAccountID", conn)
                {
                    CommandType = CommandType.Text
                };

                cmd.Parameters.AddWithValue("@TradeAccountID", tradeAccountId);

                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result.CountryID = reader.GetInt16(0);
                    result.LanguageID = reader.GetByte(1);
                }
            }

            return result;
        }

        internal String callTradeMemberAuthGUIDRenew(Int16 countryId, Byte languageId, Int32 memberId)
        {
            string result = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_TradeAuthGUID_Renew", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CountryID", countryId);
                cmd.Parameters.AddWithValue("@LanguageID", languageId);
                cmd.Parameters.AddWithValue("@MemberID", memberId);

                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result = reader.IsDBNull(0) ? null : reader.GetString(0);
                }

                reader.Close();

                conn.Close();
            }

            return result;
        }

        internal countryAwarenessCheckReturn callVisitorCountryAwarenessCheck(Int64 visitorDbId, short countryId, DateTime contentDate, short? countryIdOverride)
        {
            countryAwarenessCheckReturn result = new countryAwarenessCheckReturn();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Visitor_CountryCheck", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                cmd.Parameters.AddWithValue("@CountryID", countryId);
                cmd.Parameters.AddWithValue("@Date", contentDate);
                cmd.Parameters.AddWithValue("@CountryIDOverride", (object)countryIdOverride ?? DBNull.Value);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result.Match = (Boolean)reader[0];
                    result.Culture = reader[1] == DBNull.Value ? null : reader[1].ToString();

                    result.objCountryItem = null;

                    if (result.Match == false && result.Culture != null)
                    {
                        result.objCountryItem = new countryItem()
                        {
                            URL = reader[2].ToString()
                        };
                    }
                }
            }

            return result;
        }

        internal identificationDefinition callVisitorInsert(String userAgent, String ipAddress, String culture, String cloudFlareCountryCode, String cloudFlareDevice)
        {
            identificationDefinition objIdentification = new identificationDefinition();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Visitor_Insert", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserAgent", userAgent);
                cmd.Parameters.AddWithValue("@IP", ipAddress);
                cmd.Parameters.AddWithValue("@Culture", culture);
                cmd.Parameters.AddWithValue("@CloudFlareCountryCode", (object)cloudFlareCountryCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CloudFlareDevice", (object)cloudFlareDevice ?? DBNull.Value);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();


                while (reader.Read())
                {
                    objIdentification.visitorDbId = Convert.ToInt64(reader[0]);
                    objIdentification.userGuid = reader[1].ToString();
                }
            }

            return objIdentification;
        }


        internal VisitorSelect callVisitorSelect(string passGuid, string cfCountryHeader, string cfDevice)
        {
            VisitorSelect objVisitorSelect = new VisitorSelect();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Visitor_Select", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@GUID", passGuid);
                cmd.Parameters.AddWithValue("@CloudFlareCountryCode", (object)cfCountryHeader ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CloudFlareDevice", (object)cfDevice ?? DBNull.Value);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    // important that visitorDbId is set to -1 on null from database; this is checked for elsewhere
                    objVisitorSelect.visitorDbId = reader.IsDBNull(0) ? -1 : reader.GetInt64(0);

                    objVisitorSelect.FullName = reader.IsDBNull(1) ? null : reader.GetString(1);
                    objVisitorSelect.Email = reader.IsDBNull(2) ? null : reader.GetString(2);
                    objVisitorSelect.Culture = reader.IsDBNull(3) ? null : reader.GetString(3);
                    objVisitorSelect.VisitorCreatedDT = reader.IsDBNull(4) ? DateTime.Now : reader.GetDateTime(4);
                    objVisitorSelect.memberId = reader.IsDBNull(5) ? null : (int?)reader.GetInt32(5);
                    objVisitorSelect.memberTradeAccountNumber = reader.IsDBNull(6) ? null : reader.GetString(6);
                    objVisitorSelect.memberTradeAccountID = reader.IsDBNull(7) ? null : (int?)reader.GetInt32(7);
                    objVisitorSelect.memberDisplayTradePrice = !reader.IsDBNull(8) && reader.GetBoolean(8);
                    objVisitorSelect.tradePermissionAccountAdmin = !reader.IsDBNull(9) && reader.GetBoolean(9);
                    objVisitorSelect.tradePermissionPlaceOrder = !reader.IsDBNull(10) && reader.GetBoolean(10);
                    objVisitorSelect.tradePermissionViewAccounts = !reader.IsDBNull(11) && reader.GetBoolean(11);
                    objVisitorSelect.tradePermissionDownloadPricelistsBrochures = !reader.IsDBNull(12) && reader.GetBoolean(12);
                    objVisitorSelect.tradePermissionDownloadProductFeeds = !reader.IsDBNull(13) && reader.GetBoolean(13);
                    objVisitorSelect.tradePriceListId = reader.IsDBNull(14) ? (short?)null : reader.GetInt16(14);
                    objVisitorSelect.memberTradeDataSourceJson = reader.IsDBNull(15) ? (string)null : reader.GetString(15);

                    //if (reader.IsDBNull(0)) { }
                    //else
                    //{
                    //    objVisitorSelect.visitorDbId = reader.GetInt64(0);
                    //} 
                    //if (reader.IsDBNull(1)) { }
                    //else
                    //{
                    //    objVisitorSelect.FullName = reader.GetString(1);
                    //}
                    //if (reader.IsDBNull(2)) { }
                    //else
                    //{
                    //    objVisitorSelect.Email = reader.GetString(2);
                    //}
                    //if (reader.IsDBNull(3)) { }
                    //else
                    //{
                    //    objVisitorSelect.Culture = reader.GetString(3);
                    //}
                    //if (reader.IsDBNull(4)) { }
                    //else
                    //{
                    //    objVisitorSelect.VisitorCreatedDT = reader.GetDateTime(4);
                    //}
                    //if (reader.IsDBNull(5)) { }
                    //else
                    //{
                    //    objVisitorSelect.memberId = reader.GetInt32(5);
                    //}
                    //if (reader.IsDBNull(6)) { }
                    //else
                    //{
                    //    objVisitorSelect.memberTradeAccountNumber = reader.GetString(6);
                    //}
                    //if (reader.IsDBNull(7)) { }
                    //else
                    //{
                    //    objVisitorSelect.memberTradeAccountID = reader.GetInt32(7);
                    //}
                    //if (reader.IsDBNull(8)) { }
                    //else
                    //{
                    //    objVisitorSelect.memberDisplayTradePrice = reader.GetBoolean(8);
                    //}
                }
            }

            return objVisitorSelect;
        }

        internal string getMemberTradeAuthGUID(int memberId)
        {
            String result = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_TradeAuthGUID_Select", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@MemberID", memberId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();


                while (reader.Read())
                {
                    result = reader.IsDBNull(0) ? null : reader.GetGuid(0).ToString();
                }
            }

            return result;
        }

        internal bool callTradeMemberAuthResendEmail(short countryId, byte languageId, int memberId)
        {
            Boolean result = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_TradeAuth_Email_Resend", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CountryID", countryId);
                cmd.Parameters.AddWithValue("@LanguageID", languageId);
                cmd.Parameters.AddWithValue("@MemberID", memberId);

                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result = !reader.IsDBNull(0) && reader.GetBoolean(0);
                }

                reader.Close();

                conn.Close();
            }

            return result;
        }

        internal bool callTradeMemberAuthCancel(int memberId)
        {
            Boolean result = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_TradeAuth_Cancel", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@MemberID", memberId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();


                while (reader.Read())
                {
                    result = !reader.IsDBNull(0) && reader.GetBoolean(0);
                }
            }

            return result;
        }

        internal String callVisitorSelectCulture(Int64 visitorDbId)
        {

            String culture = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Visitor_Culture_Select", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();


                while (reader.Read())
                {
                    culture = reader[0].ToString();
                }

            }
            return culture;

        }


        internal Int16? callVisitorCountrySelect(Int64 visitorDbId)
        {

            Int16? dbCountry = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Visitor_Country_Select", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();


                while (reader.Read())
                {
                    if (reader.IsDBNull(0)) { }
                    else
                    {
                        dbCountry = (Int16)reader[0];
                    }
                }

            }
            return dbCountry;

        }

        internal Boolean callVisitorUpdate(Int64 visitorDbId, String ipAddress, String urlCountry, String cloudFlareCountryCode, String cloudFlareDevice)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Visitor_Update", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                cmd.Parameters.AddWithValue("@Culture", urlCountry);
                cmd.Parameters.AddWithValue("@IP", ipAddress);
                cmd.Parameters.AddWithValue("@CloudFlareCountryCode", (object)cloudFlareCountryCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CloudFlareDevice", (object)cloudFlareDevice ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return true;

        }

        internal int? callEmailExistsSelect(string passEmailAddress)
        {
            int? memberId = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_EmailExistsCheck", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Email", passEmailAddress);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    memberId = reader.GetInt32(0);
                }

                conn.Close();
            }

            return memberId;
        }

        internal int? callGuestCheckoutEmailExistsSelect(string passEmailAddress)
        {
            int? memberId = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_GuestCheckout_EmailExistsCheck", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Email", passEmailAddress);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    memberId = reader.GetInt32(0);
                }

                conn.Close();
            }

            return memberId;
        }

        internal TradeAccountUser callTradePermissionsCheck(string email)
        {
            TradeAccountUser result = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                var cmd = new SqlCommand("pub.TradeMember_Permissions_Select", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Email", email);

                conn.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result = new TradeAccountUser
                    {
                        MemberID = reader.GetInt32(0),
                        FullName = reader.GetString(1),
                        Email = reader.GetString(2),
                        TradePermissionAccountAdmin = reader.GetBoolean(3),
                        TradePermissionPlaceOrder = reader.GetBoolean(4)
                    };
                    result.TradePermissionPlaceOrder = reader.GetBoolean(5);
                    result.TradePermissionViewAccounts = reader.GetBoolean(6);
                    result.TradePermissionDownloadPricelistsBrochures = reader.GetBoolean(7);
                    result.TradePermissionDownloadProductFeeds = reader.GetBoolean(8);
                    result.ReadOnly = reader.GetBoolean(8);
                }

                conn.Close();
            }

            return result;
        }

        internal void logEmailExistsCheck(long visitorId, string passEmailAddress)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_EmailExistsLog", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorId);
                cmd.Parameters.AddWithValue("@Email", passEmailAddress ?? (object)DBNull.Value);

                conn.Open();

                cmd.ExecuteNonQuery();

                conn.Close();
            }
        }

        internal void logMarketingPermissionsError(long visitorId, webapiLogMarketingPermissionsError objError)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.MarketingPermissionsErrorLog_Insert", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorId);
                cmd.Parameters.AddWithValue("ErrorMessage", objError.error);

                conn.Open();

                cmd.ExecuteNonQuery();

                conn.Close();
            }
        }

        internal memberCheckResult callMemberCheckSelect(string passEmailAddress)
        {
            memberCheckResult memberExists = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_Check", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Email", passEmailAddress);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    memberExists = new memberCheckResult()
                    {
                        MemberID = reader.GetInt32(0),
                        TradeAccountNumber = reader.IsDBNull(1) ? null : reader.GetString(1)
                    };
                }
            }

            return memberExists;
        }

        internal memberFetchResult callMemberFetch(string passEmailAddress)
        {
            var result = new memberFetchResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_Fetch", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Email", passEmailAddress);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result.MemberID = reader.GetInt32(0);
                    result.GUID = reader.GetString(1);
                }
            }

            return result;
        }

        public Int32? callMemberInsert(byte languageId, short countryId, String fullName, String email, String encPassword, byte sourceId)
        {
            Int32? memberId = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_Insert", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CountryID", countryId);
                cmd.Parameters.AddWithValue("@LanguageID", languageId);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@FullName", fullName);
                cmd.Parameters.AddWithValue("@Password", encPassword);
                cmd.Parameters.AddWithValue("@SourceID", sourceId);
                cmd.Parameters.AddWithValue("@MarketingCountryID", countryId);
                cmd.Parameters.AddWithValue("@MarketingLanguageID", languageId);

                SqlParameter memberIdParam = cmd.Parameters.Add("@MemberID", SqlDbType.Int);
                memberIdParam.Direction = ParameterDirection.ReturnValue;

                conn.Open();
                cmd.ExecuteNonQuery();
                memberId = (Int32?)cmd.Parameters["@MemberID"].Value;
            }
            return memberId;
        }

        public Int32? callMemberUpsert(byte languageId, short countryId, String fullName, String email, String encPassword, short sourceId, short marketingCountryId, byte marketingLanguageId)
        {
            Int32? memberId = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_Member_Insert", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CountryID", countryId);
                cmd.Parameters.AddWithValue("@LanguageID", languageId);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@FullName", fullName);
                cmd.Parameters.AddWithValue("@Password", encPassword);
                cmd.Parameters.AddWithValue("@SourceId", sourceId);
                cmd.Parameters.AddWithValue("@MarketingCountryID", marketingCountryId);
                cmd.Parameters.AddWithValue("@MarketingLanguageID", marketingLanguageId);

                SqlParameter memberIdParam = cmd.Parameters.Add("@MemberID", SqlDbType.Int);
                memberIdParam.Direction = ParameterDirection.ReturnValue;

                conn.Open();
                cmd.ExecuteNonQuery();
                memberId = (Int32?)cmd.Parameters["@MemberID"].Value;
            }
            return memberId;
        }
        public Boolean callMemberAssociate(Int64 visitorId, int memberId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Visitor_Member_Associate", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorId);
                cmd.Parameters.AddWithValue("@MemberID", memberId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        public Boolean callMemberBasketClone(int sourceMemberId, int destinationMemberId, short countryId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_Basket_Clone", conn)
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

        public Int64 callAddressInsert(Int64 visitorDbId, string fullName, string companyName, string addressLineOne, string addressLineTwo, string addressTown,
                                        string addressCounty, string addressPostCode, short addressCountryId, string addressPhone)
        {
            Int64 newAddressID = -1;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.MemberAddress_Insert", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                cmd.Parameters.AddWithValue("@FullName", fullName);
                cmd.Parameters.AddWithValue("@CompanyName", companyName);
                cmd.Parameters.AddWithValue("@AddressLine1", addressLineOne);
                cmd.Parameters.AddWithValue("@AddressLine2", addressLineTwo);
                cmd.Parameters.AddWithValue("@Town", addressTown);
                cmd.Parameters.AddWithValue("@County", addressCounty);
                cmd.Parameters.AddWithValue("@PostCode", addressPostCode);
                cmd.Parameters.AddWithValue("@CountryID", addressCountryId);
                cmd.Parameters.AddWithValue("@PhoneNumber", addressPhone);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();


                while (reader.Read())
                {
                    newAddressID = (Int64)reader[0];
                }


            }
            return newAddressID;
        }

        internal Boolean callAddressUpdate(Int64 memberAddressId, string fullName, string companyName, string addressLineOne, string addressLineTwo, string addressTown,
                                        string addressCounty, string addressPostCode, short addressCountryId, string addressPhone)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.MemberAddress_Update", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@MemberAddressID", memberAddressId);
                cmd.Parameters.AddWithValue("@FullName", fullName);
                cmd.Parameters.AddWithValue("@CompanyName", companyName);
                cmd.Parameters.AddWithValue("@AddressLine1", addressLineOne);
                cmd.Parameters.AddWithValue("@AddressLine2", addressLineTwo);
                cmd.Parameters.AddWithValue("@Town", addressTown);
                cmd.Parameters.AddWithValue("@County", addressCounty);
                cmd.Parameters.AddWithValue("@PostCode", addressPostCode);
                cmd.Parameters.AddWithValue("@CountryID", addressCountryId);
                cmd.Parameters.AddWithValue("@PhoneNumber", addressPhone);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return true;
        }

        public Boolean callMakeMemberAddressDefault(Int32 memberId, Int64 memberAddressId, short countryId)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.MemberAddressDefault_Upsert", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@MemberID", memberId);
                cmd.Parameters.AddWithValue("@CountryID", countryId);
                cmd.Parameters.AddWithValue("@MemberAddressID", memberAddressId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return true;
        }


        internal Boolean callAddressDelete(Int64 memberAddressId)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.MemberAddress_Delete", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@MemberAddressID", memberAddressId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return true;
        }

        internal Boolean callMemberThirdPartyPreferenceUpdate(Int32 memberId, Boolean preference)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_ThirdPartyPreference_Update", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@MemberID", memberId);
                cmd.Parameters.AddWithValue("@Preference", preference);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return true;
        }

        internal Boolean callSignOut(Int64 visitorDbId)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_Logout", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return true;

        }


        internal updateMemberTradeAccountNumberResult callUpdateMemberTradeAccountNumber(MertexerV2.Models.Accounts.PassTradeUserManagementSave objSave, int tradeAccountId, int editorMemberId)
        {
            Boolean success = false;

            Int32? tradeAccountIdResult = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("mbr.TradeMember_Upsert", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@TradeAccountID", tradeAccountId);
                cmd.Parameters.AddWithValue("@MemberID", (object)objSave.MemberID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", objSave.Email);
                cmd.Parameters.AddWithValue("@TradePermissionAccountAdmin", objSave.ManageAccount);
                cmd.Parameters.AddWithValue("@TradePermissionPlaceOrder", objSave.PlaceOrders);
                cmd.Parameters.AddWithValue("@TradePermissionRequestSample", objSave.PlaceSamples);
                cmd.Parameters.AddWithValue("@TradePermissionViewAccounts", objSave.ViewAccounts);
                cmd.Parameters.AddWithValue("@TradePermissionDownloadPricelistsBrochures", objSave.DownloadPricelistsBrochures);
                cmd.Parameters.AddWithValue("@TradePermissionDownloadProductFeeds", objSave.DownloadProductFeeds);
                cmd.Parameters.AddWithValue("@CountryID", objSave.CountryID);
                cmd.Parameters.AddWithValue("@LanguageID", objSave.LanguageID);
                cmd.Parameters.AddWithValue("@EditorMemberID", editorMemberId);

                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tradeAccountIdResult = reader.IsDBNull(0) ? null : (Int32?)Convert.ToInt32(reader.GetInt32(0));
                }

                if (tradeAccountIdResult.HasValue)
                {
                    success = true;
                }
            }

            return new updateMemberTradeAccountNumberResult() { success = success, tradeAccountId = tradeAccountId };
        }

        internal tradeContactUsResult generateTradeContactUs(int tradeAccountId)
        {
            tradeContactUsResult result = new tradeContactUsResult
            {
                Details = new List<tradeContactUsDetail>()
            };

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.MyAccount_DesignersGuildContactDetails_Select", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@TradeAccountID", tradeAccountId);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result.Details.Add(new tradeContactUsDetail()
                    {
                        SortOrder = reader.GetByte(0),
                        Department = reader.GetString(1),
                        ImageID = reader.IsDBNull(2) ? null : (long?)reader.GetInt64(2),
                        Name = reader.IsDBNull(3) ? null : (string)reader.GetString(3),
                        PhoneNumber = reader.IsDBNull(4) ? null : (string)reader.GetString(4),
                        MobileNumber = reader.IsDBNull(5) ? null : (string)reader.GetString(5),
                        Email = reader.IsDBNull(6) ? null : (string)reader.GetString(6),
                        OpeningHours = reader.IsDBNull(7) ? null : (string)reader.GetString(7),
                        Notes = reader.IsDBNull(8) ? null : (string)reader.GetString(8),
                        DepartmentID = reader.IsDBNull(9) ? (byte?)null : (byte)reader.GetByte(9)
                    });
                }
            }

            return result;
        }

        internal async Task<IEnumerable<tradeContactUsDetail>> generateTradeContactUsAsync(int tradeAccountId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                var result = await conn.QueryAsync<tradeContactUsDetail>("pub.MyAccount_DesignersGuildContactDetails_Select", new { TradeAccountID = tradeAccountId }, null, null, CommandType.StoredProcedure);

                return result;
            }
        }

        internal bool callTradeMemberDelete(int memberId, int editorMemberId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("mbr.TradeMember_Delete", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@MemberID", memberId);
                cmd.Parameters.AddWithValue("@EditorMemberID", editorMemberId);

                conn.Open();
                _ = cmd.ExecuteNonQuery();

                conn.Close();
            }

            return true;
        }

        internal int callTradeAdminResendInvite(MertexerV2.Models.Accounts.PassTradeUserManagementSave objUser, int editorMemberId, int tradeAccountId)
        {
            var result = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.TradeMember_Authentication_SendAdminInvite", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CountryID", objUser.CountryID);
                cmd.Parameters.AddWithValue("@LanguageID", objUser.LanguageID);
                cmd.Parameters.AddWithValue("@MemberID", objUser.MemberID);
                cmd.Parameters.AddWithValue("@EditorMemberID", editorMemberId);
                cmd.Parameters.AddWithValue("@TradeAccountID", tradeAccountId);

                cmd.Parameters.AddWithValue("@SendInviteEmail", true);

                conn.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result = reader.GetInt32(0);
                }

                conn.Close();
            }

            return result;
        }

        internal int callTradeMemberResendInvite(MertexerV2.Models.Accounts.PassTradeUserManagementSave objUser, int editorMemberId, int tradeAccountId)
        {
            var result = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.TradeMember_Authentication_SendUserInvite", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CountryID", objUser.CountryID);
                cmd.Parameters.AddWithValue("@LanguageID", objUser.LanguageID);
                cmd.Parameters.AddWithValue("@MemberID", objUser.MemberID);
                cmd.Parameters.AddWithValue("@EditorMemberID", editorMemberId);
                cmd.Parameters.AddWithValue("@TradeAccountID", tradeAccountId);

                cmd.Parameters.AddWithValue("@ManageAccount", objUser.ManageAccount);
                cmd.Parameters.AddWithValue("@PlaceOrders", objUser.PlaceOrders);
                cmd.Parameters.AddWithValue("@RequestSamples", objUser.PlaceSamples);
                cmd.Parameters.AddWithValue("@ViewAccounts", objUser.ViewAccounts);
                cmd.Parameters.AddWithValue("@DownloadPricelistsBrochures", objUser.DownloadPricelistsBrochures);
                cmd.Parameters.AddWithValue("@DownloadProductFeeds", objUser.DownloadProductFeeds);
                cmd.Parameters.AddWithValue("@SendInviteEmail", true);

                conn.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result = reader.GetInt32(0);
                }

                conn.Close();
            }

            return result;
        }


        internal bool PlaceOrderPermissionCheck(int? memberId, short countryId)
        {
            var result = false;

            if (memberId == null)
            {
                return result;
            }

            try
            {
                // need to match trade account currency to site currency.
                // simples.

                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
                {
                    var comm = new SqlCommand(@"
                        SELECT	M.TradePermissionPlaceOrder
                        FROM	mbr.Member AS M
		                        INNER JOIN dbo.TradeAccount AS TA ON TA.TradeAccountID = M.TradeAccountID
		                        INNER JOIN dbo.TradePriceList AS TPL ON TPL.TradePriceListID = TA.TradePriceListID
		                        INNER JOIN dbo.PriceList AS PL ON PL.PriceListID = TPL.PriceListID AND
										                          PL.CurrencyID = TA.CurrencyID -- Ensures Trade Account Currency Matches Site Currency
		                        INNER JOIN dbo.Country AS C ON C.PriceListID = PL.PriceListID 
                        WHERE	C.CountryID = @CountryID AND
		                        M.MemberID = @MemberID AND 
		                        M.Deleted = 0 AND 
		                        M.TradePermissionPlaceOrder = 1 AND -- Alowed to Place Orders 
		                        M.TradeAuthenticationComplete = 1 AND
		                        TA.[Status] = 'O' -- Trade Account is Open",
                    con);

                    comm.Parameters.AddWithValue("@MemberID", memberId);
                    comm.Parameters.AddWithValue("@CountryID", countryId);

                    con.Open();

                    var reader = comm.ExecuteReader();

                    while (reader.Read())
                    {
                        result = !reader.IsDBNull(0);
                    }

                    reader.Close();

                    con.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }


        internal Boolean callUpdateMemberDisplayTradePrice(Int32 memberId, Boolean displayTradePrice)
        {
            Boolean success = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.TradeMember_DisplayTradePrice_Update", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@MemberID", memberId);
                cmd.Parameters.AddWithValue("@DisplayTradePrice", displayTradePrice);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    success = (Boolean)reader[0];
                }
            }
            return success;
        }

        internal bool callUpdateMemberPassword(Int64 visitorDbId, string currentPasswordEnc, string newPasswordEnc)
        {
            Boolean success = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_Password_Update", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };


                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                cmd.Parameters.AddWithValue("@CurrentPassword", currentPasswordEnc);
                cmd.Parameters.AddWithValue("@NewPassword", newPasswordEnc);


                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    success = (Boolean)reader[0];
                }


            }
            return success;

        }

        internal bool callUpdateMemberCountry(Int32 memberId, Byte countryLanguageId)
        {
            Boolean success = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_Country_Language_Update", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@MemberID", memberId);
                cmd.Parameters.AddWithValue("@CountryLanguageID", countryLanguageId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    success = (Boolean)reader[0];
                }
            }

            return success;
        }

        internal bool callUpdateMemberName(long visitorDbId, string name)
        {
            Boolean success = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_Name_Update", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                cmd.Parameters.AddWithValue("@Name", name);

                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    success = (Boolean)reader[0];
                }
            }
            return success;

        }

        internal bool callUpdateMemberEmail(long visitorDbId, string newEmail)
        {
            Boolean success = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_Email_Update", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                cmd.Parameters.AddWithValue("@NewEmail", newEmail);

                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    success = (Boolean)reader[0];
                }
            }
            return success;

        }


        internal bool callUpdateMemberDetail(Int64 visitorDbId, String fullName)
        {
            Boolean success = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_Update", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };


                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                cmd.Parameters.AddWithValue("@FullName", fullName);


                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    success = (Boolean)reader[0];
                }


            }
            return success;

        }

        internal bool passwordResetRequest(PassPasswordResetRequest objRequest)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                var cmd = new SqlCommand("pub.Member_Password_Reset_Request", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Email", objRequest.email);
                cmd.Parameters.AddWithValue("@CountryID", objRequest.countryId);
                cmd.Parameters.AddWithValue("@LanguageID", objRequest.languageId);

                conn.Open();

                cmd.ExecuteNonQuery();
            }

            return true;
        }


        internal bool passwordReset(String token, String newPasswordEnc)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_Password_Reset", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };


                cmd.Parameters.AddWithValue("@Token", token);
                cmd.Parameters.AddWithValue("@Password", newPasswordEnc);


                conn.Open();
                cmd.ExecuteNonQuery();


            }
            return true;

        }


        internal bool validateResetPasswordToken(String token)
        {
            Boolean success = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_Password_Reset_ValidateToken", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };


                cmd.Parameters.AddWithValue("@Token", token);

                SqlParameter retval = cmd.Parameters.Add("@Passed", SqlDbType.Bit);
                retval.Direction = ParameterDirection.Output;

                conn.Open();


                cmd.ExecuteNonQuery();

                String returnValue = retval.Value.ToString();

                if (returnValue == "True")
                {
                    success = true;
                }

            }
            return success;
        }


        public Int32 saveReturn(Int64 visitorDbId, Int32 orderHeadId, DataTable returnTableType)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Return_Save", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };


                cmd.Parameters.AddWithValue("@OrderHeadID", orderHeadId);

                SqlParameter storeSqlParam = cmd.Parameters.AddWithValue("@Returns", returnTableType);
                storeSqlParam.SqlDbType = SqlDbType.Structured;
                storeSqlParam.TypeName = "ReturnTableType";

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                SqlParameter retval = cmd.Parameters.Add("@ReturnHeadID", SqlDbType.Int);
                retval.Direction = ParameterDirection.Output;

                conn.Open();

                cmd.ExecuteNonQuery();

                return Convert.ToInt32(retval.Value.ToString());
            }

        }


        internal Boolean callPostCodeSearchAudit(Int64 visitorDbId, String countryCode, String postcode, String addressLine1)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.AddressLookup_Insert", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                cmd.Parameters.AddWithValue("@CountryCode", countryCode);
                cmd.Parameters.AddWithValue("@Postcode", postcode);
                cmd.Parameters.AddWithValue("@AddressLine1", addressLine1);

                conn.Open();
                cmd.ExecuteNonQuery();

            }
            return true;
        }


        internal Boolean callBackInStockReminderInsert(Int64 visitorDbId, Int32 productId, Int16 countryId, Byte languageId)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.MemberStockReminder_Insert", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                cmd.Parameters.AddWithValue("@ProductID", productId);
                cmd.Parameters.AddWithValue("@CountryID", countryId);
                cmd.Parameters.AddWithValue("@LanguageID", languageId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        internal Int32 createMoodboard(Int32 memberId, Int16 countryId)
        {
            var db = new databaseDataContext();

            var result = db.MemberMoodboard_Create(memberId, countryId);

            var moodboardId = result.GetResult<Int32>().Single();

            return moodboardId;
        }

        internal Boolean deleteMoodboard(Int32 memberId, Int32 moodboardId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Moodboard_Delete", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@MemberID", memberId);
                cmd.Parameters.AddWithValue("@MoodboardID", moodboardId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        internal Moodboard fetchMoodboard(Int32 moodboardId)
        {
            var db = new databaseDataContext();

            var result = db.MemberMoodboard_FetchSingle(moodboardId);

            return result.GetResult<Moodboard>().Single();
        }

        internal DatabaseJSONResponse fetchMoodboardSampleOrdering(int memberId, int? tradeAccountId, bool displayTradePrice, PassMoodboardOrderingFetch objFetch, bool countryCheckMatched)
        {
            var result = new DatabaseJSONResponse();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("[pub].[Moodboard_SelectForAddToBasket]", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

                cmd.Parameters.AddWithValue("@MoodboardID", objFetch.MoodboardID);
                cmd.Parameters.AddWithValue("@Date", useDate);
                cmd.Parameters.AddWithValue("@CountryID", objFetch.CountryID);
                cmd.Parameters.AddWithValue("@LanguageID", objFetch.LanguageID);
                cmd.Parameters.AddWithValue("@MemberID", memberId);
                cmd.Parameters.AddWithValue("@IsTrade", tradeAccountId.HasValue);
                cmd.Parameters.AddWithValue("@TradeAccountID", tradeAccountId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DisplayTradePrice", displayTradePrice);
                cmd.Parameters.AddWithValue("@CountryMatch", countryCheckMatched);

                conn.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result.JSON = reader.GetString(0);
                }

                conn.Close();
            }

            return result;
        }

        internal bool SendMoodboardEmail(MoodboardShareEmail objEmail)
        {
            var db = new databaseDataContext();
            var result = false;
            var str = new List<String>();

            try
            {
                var results = db.MemberMoodboard_SendShareEmail(
                    objEmail.MoodboardID,
                    objEmail.DetailedProductInfo,
                    objEmail.UserName,
                    objEmail.UserEmail,
                    objEmail.RecipientName,
                    objEmail.RecipientEmail,
                    objEmail.Body,
                    objEmail.ImageURL,
                    objEmail.CountryID,
                    objEmail.LanguageID);

                str = results.GetResult<String>().ToList();

            }
            catch (Exception ex)
            {
                str.Add(ex.ToString());
            }
            if (str.Count > 0)
            {
                if (str[0].ToLower().Contains("success"))
                {
                    result = true;
                }
            }

            return result;
        }

        internal MoodboardContainer getMemberMoodboards(Int32 memberId, Int16 countryId, Byte languageId, DateTime contentDate)
        {
            var db = new databaseDataContext();

            var results = db.MemberMoodboard_GenerateMultiple(memberId, countryId, languageId, contentDate);

            MoodboardContainer objPassMoodboard = new MoodboardContainer
            {
                Heading = results.GetResult<String>().Single(),
                SubHeading = results.GetResult<String>().Single(),
                ProductTypes = results.GetResult<ProductType>().ToList(),
                Brands = results.GetResult<MoodboardBrand>().ToList(),
                Moodboards = results.GetResult<Moodboard>().ToList(),
                MoodboardImages = results.GetResult<MoodboardImage>().ToList(),
                Scrapbook = results.GetResult<ScrapbookImage>().ToList(),
                Rooms = results.GetResult<Room>().ToList()
            };

            return objPassMoodboard;
        }

        internal MoodboardContainer reloadMemberMoodboards(Int32 memberId, Int16 countryId, Byte languageId, DateTime contentDate)
        {

            var db = new databaseDataContext();

            var results = db.MemberMoodboard_Reload(memberId, countryId, languageId, contentDate);

            MoodboardContainer objPassMoodboard = new MoodboardContainer
            {
                Moodboards = results.GetResult<Moodboard>().ToList()
            };

            return objPassMoodboard;
        }

        internal MoodboardContainer searchMemberMoodboards(Int32 memberId, String customerName, Byte? roomId, String reference, Int16 countryId, Byte languageId, DateTime? contentDate)
        {
            var db = new databaseDataContext();

            var results = db.MemberMoodboard_Search(memberId, customerName, roomId, reference, countryId, languageId, contentDate);

            MoodboardContainer objPassMoodboard = new MoodboardContainer
            {
                Moodboards = results.GetResult<Moodboard>().ToList(),
                Rooms = results.GetResult<Room>().ToList()
            };

            return objPassMoodboard;
        }

        internal Boolean saveMoodboard(Int32 moodboardId, Int16? height, String json, String customerName, Byte? roomId, String reference)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Moodboard_Save", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@MoodboardID", moodboardId);
                cmd.Parameters.AddWithValue("@JSON", json);
                cmd.Parameters.AddWithValue("@CustomerName", customerName);
                cmd.Parameters.AddWithValue("@Height", height);
                cmd.Parameters.AddWithValue("@RoomID", roomId);
                cmd.Parameters.AddWithValue("@Reference", reference);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }


        internal Boolean saveMoodboardFromGridView(Int32 moodboardId, String customerName, Byte? roomId, String reference)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Moodboard_Save_FromGridView", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@MoodboardID", moodboardId);
                cmd.Parameters.AddWithValue("@CustomerName", customerName);
                cmd.Parameters.AddWithValue("@RoomID", roomId);
                cmd.Parameters.AddWithValue("@Reference", reference);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        internal Boolean addImageToMoodboard(Int32 moodboardId, Int64 imageId, Int32? productPageId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Moodboard_Image_Insert", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@MoodboardID", moodboardId);
                cmd.Parameters.AddWithValue("@ImageID", imageId);
                if (productPageId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@ProductPageID", productPageId);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@ProductPageID", DBNull.Value);
                }
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        internal Boolean removeImageFromMoodboard(Int32 moodboardId, Int64 imageId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Moodboard_Image_Delete", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@MoodboardID", moodboardId);
                cmd.Parameters.AddWithValue("@ImageID", imageId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        internal Boolean clearMoodboard(Int32 memberId, Int32 moodboardId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Moodboard_Reset", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@MemberID", memberId);
                cmd.Parameters.AddWithValue("@MoodboardID", moodboardId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        internal List<ScrapbookImage> getVisitorScrapbook(Int64 visitorDbId, Int32? memberId, Byte? productTypeId, Byte? brandId, Boolean? isNew, Int16 countryId, Byte languageId, DateTime contentDate)
        {
            var db = new databaseDataContext();

            var results = db.VisitorScrapbook_GenerateMultiple(visitorDbId, memberId, productTypeId, brandId, isNew, countryId, languageId, contentDate);

            return results.GetResult<ScrapbookImage>().ToList();
        }

        internal async Task<IEnumerable<ScrapbookImage>> fetchCompleteScrapbookAsync(
            long visitorDbId,
            int? memberId,
            short countryId,
            byte languageId,
            DateTime contentDate)
        {
            var db = new databaseDataContext();

            var result = await db.Connection.QueryAsync<ScrapbookImage>("[pub].[Scrapbook_Select]", new
            {
                VisitorID = visitorDbId,
                MemberID = memberId,
                CountryID = countryId,
                LanguageID = languageId,
                Date = contentDate
            }, null, 30, CommandType.StoredProcedure);

            return result;
        }

        internal Boolean saveImageToScrapbook(Int64 visitorDbId, Int64? memberId, Int64 imageId, Int16 countryId, Int32? productPageId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Scrapbook_Insert", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                if (memberId != null)
                    cmd.Parameters.AddWithValue("@MemberID", memberId);
                cmd.Parameters.AddWithValue("@CountryID", countryId);
                cmd.Parameters.AddWithValue("@ImageID", imageId);

                if (productPageId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@ProductPageID", productPageId);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@ProductPageID", DBNull.Value);
                }

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        internal Boolean removeImageFromScrapbook(Int64 visitorDbId, Int64? memberId, Int16 countryId, Int64 imageId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Scrapbook_Delete", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                if (memberId != null)
                    cmd.Parameters.AddWithValue("@MemberID", memberId);
                cmd.Parameters.AddWithValue("@CountryID", countryId);
                cmd.Parameters.AddWithValue("@ImageID", imageId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        internal bool SortScrapbook(int memberId, PassScrapbook objScrapbook)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Member_Scrapbook_Sort", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var scrapbookTable = new DataTable();

                scrapbookTable.Columns.Add("ImageID", typeof(long));
                scrapbookTable.Columns.Add("SortOrder", typeof(int));

                foreach(var i in objScrapbook.Images.OrderBy(img => img.SortOrder))
                {
                    scrapbookTable.Rows.Add(i.ImageID, i.SortOrder);
                }

                cmd.Parameters.AddWithValue("@MemberID", memberId);
                cmd.Parameters.AddWithValue("@CountryID", objScrapbook.CountryID);
                cmd.Parameters.AddWithValue("@LanguageID", objScrapbook.LanguageID);

                var scrapbookImagesParam = cmd.Parameters.AddWithValue("@ScrapbookImages", scrapbookTable);
                scrapbookImagesParam.SqlDbType = SqlDbType.Structured;
                scrapbookImagesParam.TypeName = "ScrapbookImageTableType";

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                return true;
            }
        }

        internal Boolean clearScrapbook(Int64 visitorDbId, Int64? memberId, Int16 countryId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Scrapbook_Delete_All", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                cmd.Parameters.AddWithValue("@MemberID", memberId);
                cmd.Parameters.AddWithValue("@CountryID", countryId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        internal int saveUPSTrackingNumbers(int returnHeadId, DataTable upsLabelsTableType)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Return_Save_TrackingNumbers", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@ReturnHeadID", returnHeadId);

                SqlParameter storeSqlParam = cmd.Parameters.AddWithValue("@TrackingNumbers", upsLabelsTableType);
                storeSqlParam.SqlDbType = SqlDbType.Structured;
                storeSqlParam.TypeName = "UPSTrackingNumberTableType";

                SqlParameter retval = cmd.Parameters.Add("@Result", SqlDbType.Bit);
                retval.Direction = ParameterDirection.Output;

                conn.Open();

                cmd.ExecuteNonQuery();

                return Convert.ToInt32(retval.Value);
            }
        }

        internal int saveReturnCollectionPRN(int returnHeadId, string prn)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Return_Save_CollectionPRN", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@ReturnHeadID", returnHeadId);
                cmd.Parameters.AddWithValue("@PRN", prn);

                SqlParameter retval = cmd.Parameters.Add("@Result", SqlDbType.Bit);
                retval.Direction = ParameterDirection.Output;

                conn.Open();

                cmd.ExecuteNonQuery();

                return Convert.ToInt32(retval.Value);
            }
        }

        /// <summary>
        /// Will only wait 3 seconds; if this call takes longer than a tenth of that, something is amiss.
        /// </summary>
        /// <param name="objView"></param>
        internal void logPageView(logPageView objView)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("log.PageView_Insert", conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 3
                };

                cmd.Parameters.AddWithValue("@Site", objView.Site);
                cmd.Parameters.AddWithValue("@URL", objView.URL?.TrimReduceTruncate(500));
                cmd.Parameters.AddWithValue("@CountryID", objView.CountryID);
                cmd.Parameters.AddWithValue("@LanguageID", objView.LanguageID);
                cmd.Parameters.AddWithValue("@Date", objView.Date);
                cmd.Parameters.AddWithValue("@VisitorID", objView.VisitorID);
                cmd.Parameters.AddWithValue("@MemberID", objView.MemberID);
                cmd.Parameters.AddWithValue("@TradeAccountID", objView.TradeAccountID);
                cmd.Parameters.AddWithValue("@PageID", objView.PageID);
                cmd.Parameters.AddWithValue("@CollectionID", objView.CollectionID);
                cmd.Parameters.AddWithValue("@ProductPageID", objView.ProductPageID);
                cmd.Parameters.AddWithValue("@ArticleID", objView.ArticleID);
                cmd.Parameters.AddWithValue("@UserAgent", objView.UserAgent);
                cmd.Parameters.AddWithValue("@IPString", objView.CFIPCountry ?? "GB");
                cmd.Parameters.AddWithValue("@CloudFlareDevice", objView.CloudFlareDevice ?? "");
                cmd.Parameters.AddWithValue("@SessionID", objView.SessionID);
                cmd.Parameters.AddWithValue("@GoogleSource", objView.GoogleSource?.TrimReduceTruncate(100));
                cmd.Parameters.AddWithValue("@GoogleMedium", objView.GoogleMedium?.TrimReduceTruncate(100));
                cmd.Parameters.AddWithValue("@GoogleCampaign", objView.GoogleCampaign?.TrimReduceTruncate(100));
                cmd.Parameters.AddWithValue("@GoogleTerm", objView.GoogleTerm?.TrimReduceTruncate(100));
                cmd.Parameters.AddWithValue("@GoogleContent", objView.GoogleContent?.TrimReduceTruncate(100));

                conn.Open();

                cmd.ExecuteNonQuery();

                conn.Close();
            }
        }

        internal async Task<bool> storeTradeInvoiceAddressChangeRequest(loggedInMemberDetail loginCheck, MertexerV2.Models.Accounts.PassInvoiceAddressChangeRequest objRequest)
        {
            var detailFetcher = new cmsTradeAccountContactDetails();

            var contactResult = await detailFetcher.loadTradeAccountContactDetailsFromCms(loginCheck.tradeAccountId.Value, objRequest.LanguageID, "CC");

            var email = JObject.FromObject(contactResult.json).SelectToken("$.contactDetails[0].email").Value<string>();

            var tradeAccountPrimaryEmail = JObject.FromObject(contactResult.json).SelectToken("$.contactDetails[0].tradeAccountPrimaryEmail").Value<string>();

            string cc = null;

#if DEBUG
            email = "tclark@designersguild.com";
            cc = "mchristie@designersguild.com";
#endif

            var current = JsonConvert.DeserializeObject<MertexerV2.Models.Accounts.PassBasketAddress>(objRequest.CurrentJSON);
            var requested = JsonConvert.DeserializeObject<MertexerV2.Models.Accounts.PassBasketAddress>(objRequest.RequestedJSON);

            var inkyMarkup = $@"
                <style type=""text/css"">
                    {dgFramework.FoundationEmailCSS}
                </style>
                <meta name=""x-apple-disable-message-reformatting""/>
                <container>
	                <row>
		                <columns>
			                <img width=""640"" height=""54"" style=""margin: 15px 0 0 0"" alt='Designers Guild' src='https://www.designersguild.com/_assets/email_images/logos/header-logo.jpg' title='Designers Guild' />
		                </columns>
	                </row>
	                <row>
		                <columns>
			                <p class=""text-left"" style=""margin-top: 0"">
				                Hello,
			                </p>
		                </columns>
	                </row>
	                <row>
		                <columns>
			                <p>
				                <strong>{loginCheck.tradeAccountNumber}</strong> have requested a change of Invoice Address. Please apply this change as soon as you can.
			                </p>
                        </columns>
                    </row>
                    <row>
                        <columns>
                            <p>
                                Once you have made this change, please inform them by emailing {loginCheck.fullName} using <a href=""mailto:{loginCheck.email}"">{loginCheck.email}</a>.
                            </p>
		                </columns>
	                </row>
                    <row>
                        <columns>
                            <p>
                                The primary email address we have on file for {loginCheck.tradeAccountNumber} is <a href=""mailto:{tradeAccountPrimaryEmail}"">{tradeAccountPrimaryEmail}</a>.
                            </p>
		                </columns>
	                </row>
	                <spacer size=""10""></spacer>
	                <row>
		                <columns small=""12"" large=""12"">
			                <p>
				                <strong>
					                Original Address
				                </strong>
			                </p>
		                </columns>
                    </row>
                    <row>
                        <columns small=""3"" large=""4"">
                            <p class=""text-right"">
                                <strong>
                                    Address Line 1<br/>
                                    Address Line 2<br/>
                                    Town<br/>
                                    County<br/>
                                    Postcode<br/>
                                    Country Code
                                </strong>
                            </p>
                        </columns>
		                <columns small=""8"" large=""8"">
			                <p>
				                {(String.IsNullOrEmpty(current.deliveryAddressLine1) ? "<br/>" : current.deliveryAddressLine1 + "<br/>")}
				                {(String.IsNullOrEmpty(current.deliveryAddressLine2) ? "<br/>" : current.deliveryAddressLine2 + "<br/>")}
				                {(String.IsNullOrEmpty(current.deliveryTown) ? "<br/>" : current.deliveryTown + "<br/>")}
				                {(String.IsNullOrEmpty(current.deliveryCounty) ? "<br/>" : current.deliveryCounty + "<br/>")}
				                {(String.IsNullOrEmpty(current.deliveryPostcode) ? "<br/>" : current.deliveryPostcode + "<br/>")}
				                {(String.IsNullOrEmpty(current.deliveryCountryCode) ? "<br/>" : current.deliveryCountryCode + "<br/>")}
			                </p>
		                </columns>
	                </row>
	                <spacer size=""20""></spacer>
	                <row>
		                <columns small=""12"" large=""12"">
			                <p>
				                <strong>
					                New Address
				                </strong>
			                </p>
		                </columns>
                    </row>
                    <row>
                        <columns small=""3"" large=""4"">
                            <p class=""text-right"">
                                <strong>
                                    Address Line 1<br/>
                                    Address Line 2<br/>
                                    Town<br/>
                                    County<br/>
                                    Postcode<br/>
                                    Country Code
                                </strong>
                            </p>
                        </columns>
		                <columns small=""8"" large=""8"">
			                <p>
				                {(String.IsNullOrEmpty(requested.deliveryAddressLine1) ? "<br/>" : requested.deliveryAddressLine1 + "<br/>")}
				                {(String.IsNullOrEmpty(requested.deliveryAddressLine2) ? "<br/>" : requested.deliveryAddressLine2 + "<br/>")}
				                {(String.IsNullOrEmpty(requested.deliveryTown) ? "<br/>" : requested.deliveryTown + "<br/>")}
				                {(String.IsNullOrEmpty(requested.deliveryCounty) ? "<br/>" : requested.deliveryCounty + "<br/>")}
				                {(String.IsNullOrEmpty(requested.deliveryPostcode) ? "<br/>" : requested.deliveryPostcode + "<br/>")}
				                {(String.IsNullOrEmpty(requested.deliveryCountryCode) ? "<br/>" : requested.deliveryCountryCode + "<br/>")}
			                </p>
			                </p>
		                </columns>
	                </row>
<spacer size=""40""></spacer>
	                <row>
		                <columns small=""12"">
			                <p class=""footer text-left small smaller"">
				                Thanks,<br/>
                                Designers Guild Portal
			                </p>
		                </columns>
	                </row>
                </container>";

            using (var client = new HttpClient())
            {
                var content = new StringContent(inkyMarkup);

                var r = await client.PostAsync(Framework.InkyEndpoint, content);

                inkyMarkup = await r.Content.ReadAsStringAsync();
            }

            var emailId = this.addEmailToQueue(email,
                cc,
                "itsupport@designersguild.com",
                $"{loginCheck.tradeAccountNumber} Invoice Address Change Request",
                inkyMarkup,
                4);

            bool result = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(@"INSERT INTO [portal].[TradeInvoiceAddressChangeRequest] (
                    TradeAccountID,
                    RequestedByMemberID,
                    CurrentInvoiceAddressJSON,
                    CurrentInvoiceAddressDisplayBlock,
                    RequestedInvoiceAddressJSON,
                    RequestedInvoiceAddressDisplayBlock,
                    NotificationEmailID)
                VALUES (
                    @TradeAccountID,
                    @MemberID,
                    @CurrentJSON,
                    @CurrentDisplayBlock,
                    @RequestedJSON,
                    @RequestedDisplayBlock,
                    @NotificationEmailID
                )", conn)
                {
                    CommandType = CommandType.Text
                };

                cmd.Parameters.AddWithValue("@TradeAccountID", loginCheck.tradeAccountId);
                cmd.Parameters.AddWithValue("@MemberID", loginCheck.memberId);
                cmd.Parameters.AddWithValue("@CurrentJSON", objRequest.CurrentJSON);
                cmd.Parameters.AddWithValue("@CurrentDisplayBlock", objRequest.CurrentDisplayBlock);
                cmd.Parameters.AddWithValue("@RequestedJSON", objRequest.RequestedJSON);
                cmd.Parameters.AddWithValue("@RequestedDisplayBlock", objRequest.RequestedDisplayBlock);
                cmd.Parameters.AddWithValue("@NotificationEmailID", emailId);

                conn.Open();

                result = cmd.ExecuteNonQuery() == 1;

                conn.Close();
            }

            return result;
        }

        public quickSearchResult tradeQuickSearch(MertexerV2.Models.Accounts.PassQuickSearch objSearch, int? tradeAccountId, bool displayTradePrice, int? memberId, DateTime useDate, bool countryCheckMatched) 
        {
            var result = new quickSearchResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(@"pub.QuickOrder_Search", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@SearchTerm", objSearch.searchTerm);
                cmd.Parameters.AddWithValue("@CountryID", objSearch.countryId);
                cmd.Parameters.AddWithValue("@LanguageID", objSearch.languageId);
                cmd.Parameters.AddWithValue("@ProductTypeID", objSearch.productTypeId);
                cmd.Parameters.AddWithValue("@Date", useDate);
                cmd.Parameters.AddWithValue("@IsTrade", tradeAccountId.HasValue);
                cmd.Parameters.AddWithValue("@TradeAccountID", tradeAccountId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DisplayTradePrice", displayTradePrice);
                cmd.Parameters.AddWithValue("@CountryMatch", countryCheckMatched);
                cmd.Parameters.AddWithValue("@MemberID", memberId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Debug", DBNull.Value);

                conn.Open();

                var reader = cmd.ExecuteReader();

                while(reader.Read())
                {
                    result.ResultJSON = reader.IsDBNull(0) ? "" : reader.GetString(0);
                }

                reader.Close();

                conn.Close();
            }

            return result;
        }

        public CoverageResponse requirementsCalculator(PassCoverageParameters objFetch)
        {
            var result = new CoverageResponse();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(@"pub.RequirementsCalculator", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                long visitorId = -1;

                if (Int64.TryParse(HttpContext.Current.Session["visitorIdFromDatabase"]?.ToString(), out long vid))
                {
                    visitorId = vid;
                }

                int? tradeAccountId = null;

                if (Boolean.TryParse(HttpContext.Current.Session["memberTradeChecksPassed"]?.ToString(), out bool passed))
                {
                    if (passed)
                    {
                        if (Int32.TryParse(HttpContext.Current.Session["memberTradeAccountId"]?.ToString(), out int tid))
                        {
                            tradeAccountId = tid;
                        }
                    }
                }

                var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

                cmd.Parameters.AddWithValue("@VisitorID", visitorId);
                cmd.Parameters.AddWithValue("@CountryID", objFetch.CountryID);
                cmd.Parameters.AddWithValue("@LanguageID", objFetch.LanguageID);
                cmd.Parameters.AddWithValue("@Unit", objFetch.Unit == "\"" ? "in" : objFetch.Unit);
                cmd.Parameters.AddWithValue("@Dimensions", objFetch.Dimensions);
                cmd.Parameters.AddWithValue("@IncludeWastage", objFetch.IncludeWastage);
                cmd.Parameters.AddWithValue("@ProductPageID", objFetch.ProductPageID);
                cmd.Parameters.AddWithValue("@TradeAccountID", tradeAccountId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsTrade", tradeAccountId.HasValue);
                cmd.Parameters.AddWithValue("@Coats", objFetch.Coats);
                cmd.Parameters.AddWithValue("@PaintFinishAttributeID", objFetch.PaintFinishAttributeID);
                cmd.Parameters.AddWithValue("@Date", useDate);
                cmd.Parameters.AddWithValue("@EmailAddress", objFetch.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Debug", false);

                conn.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result.RequirementsExplanation = reader.IsDBNull(0) ? null : reader.GetString(0);
                    result.Errored = reader.IsDBNull(1) ? true : reader.GetBoolean(1);
                }

                reader.NextResult();

                while (reader.Read())
                {
                    result.ShoppingList.Add(
                        new CoverageRequirement(
                            productId: reader.GetInt32(0),
                            quantity: reader.GetDecimal(1),
                            basketLineTypeId: reader.GetByte(2)
                        )
                    );
                }

                reader.NextResult();

                while(reader.Read())
                {
                    result.EmailID = reader.IsDBNull(0) ? (long?)null : reader.GetInt64(0);
                }

                reader.Close();

                conn.Close();
            }

            return result;
        }
    }

    public class quickSearchResult
    {
        public string ResultJSON { get; set; }

        public object Results
        {
            set
            {

            }
            get
            {
                return JsonConvert.DeserializeObject(String.IsNullOrEmpty(ResultJSON) ? "{}" : ResultJSON);
            }
        }
    }

    //public class quickSearchRecord
    //{
    //    public bool AllowReserve { get; set; }
    //    public bool AllowReserveWithBatchSample { get; set; }
    //    public bool AllowSample { get; set; }
    //    public bool AllowLargeSample { get; set; }
    //    public long? ImageID { get; set; }
    //    public string Name { get; set; }
    //    public int ProductID { get; set; }
    //    public string Code { get; set; }
    //    public string StockMessage { get; set; }
    //    public object stockMessage
    //    {
    //        get
    //        {
    //            return JsonConvert.DeserializeObject(StockMessage);
    //        }
    //    }

    //    public decimal? DisplayPrice { get; set; }
    //    public decimal? DisplaySalePrice { get; set; }
    //    public int? CuttingSampleID { get; set; }
    //    public string CuttingStockMessage { get; set; }
    //    public object cuttingStockMessage
    //    {
    //        set
    //        {

    //        }
    //        get
    //        {
    //            if (!String.IsNullOrEmpty(CuttingStockMessage))
    //                return JsonConvert.DeserializeObject(CuttingStockMessage);
    //            else
    //                return null;
    //        }
    //    }
    //    public int? ReturnableSampleID { get; set; }

    //    public string ReturnableStockMessage { get; set; }
    //    public object returnableStockMessage
    //    {
    //        set
    //        {

    //        }
    //        get
    //        {
    //            if (!String.IsNullOrEmpty(ReturnableStockMessage))
    //                return JsonConvert.DeserializeObject(ReturnableStockMessage);
    //            else
    //                return null;
    //        }
    //    }
    //}

    public partial class databaseDataContext
    {
        [Function(Name = "sf.Member_MoodboardShare_Email_T_Send")]
        [ResultTypeAttribute(typeof(String))]
        internal IMultipleResults MemberMoodboard_SendShareEmail(
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MoodboardID", DbType = "Int")] System.Nullable<Int32> moodboardId,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IncludeProductDetails", DbType = "Bit")] Boolean includeProductDetails,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "@UserName", DbType = "NVarChar(100)")] String userName,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "@UserEmail", DbType = "NVarChar(256)")] String userEmail,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "@RecipientName", DbType = "NVarChar(100)")] String recipientName,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "@RecipientEmail", DbType = "NVarChar(256)")] String recipientEmail,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "@UserMessage", DbType = "NVarChar(250)")] String userMessage,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "@ImageURL", DbType = "NVarChar(500)")] String imageURL,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryId,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageId
            )
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), moodboardId, includeProductDetails, userName, userEmail, recipientName, recipientEmail, userMessage, imageURL, countryId, languageId);
            return (IMultipleResults)result.ReturnValue;
        }

        [Function(Name = "pub.Moodboard_Select_Single")]
        [ResultTypeAttribute(typeof(Moodboard))]
        internal IMultipleResults MemberMoodboard_FetchSingle([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MoodboardID", DbType = "BigInt")] System.Nullable<Int32> moodboardId)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), moodboardId);
            return (IMultipleResults)result.ReturnValue;
        }

        [Function(Name = "pub.Moodboard_Search")]
        [ResultTypeAttribute(typeof(Moodboard))]
        [ResultTypeAttribute(typeof(MoodboardImage))]
        internal IMultipleResults MemberMoodboard_Search([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "BigInt")] System.Nullable<Int32> memberId,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CustomerName", DbType = "NVarChar(70)")] String customerName,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "RoomID", DbType = "TinyInt")] System.Nullable<byte> roomId,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Reference", DbType = "NVarChar(70)")] String reference,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<DateTime> contentDate)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), memberId, customerName, roomId, reference, countryID, languageID, contentDate);
            return (IMultipleResults)result.ReturnValue;
        }

        [Function(Name = "pub.Moodboard_Select")]
        [ResultTypeAttribute(typeof(Moodboard))]
        [ResultTypeAttribute(typeof(MoodboardImage))]
        internal IMultipleResults MemberMoodboard_Reload([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "BigInt")] System.Nullable<Int32> memberId,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<DateTime> contentDate)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), memberId, countryID, languageID, contentDate);
            return (IMultipleResults)result.ReturnValue;
        }

        [Function(Name = "pub.Moodboard_Create")]
        [ResultTypeAttribute(typeof(Boolean))]
        internal IMultipleResults MemberMoodboard_Create([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "BigInt")] System.Nullable<Int32> memberId,
                                                       [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), memberId, countryID);
            return (IMultipleResults)result.ReturnValue;
        }

        [Function(Name = "pub.Moodboard_Generate")]
        [ResultTypeAttribute(typeof(ProductType))]
        [ResultTypeAttribute(typeof(MoodboardBrand))]
        [ResultTypeAttribute(typeof(Moodboard))]
        [ResultTypeAttribute(typeof(ScrapbookImage))]
        [ResultTypeAttribute(typeof(Room))]
        internal IMultipleResults MemberMoodboard_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "BigInt")] System.Nullable<Int32> memberId,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<Int16> countryID,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<Byte> languageID,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<DateTime> contentDate)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), memberId, countryID, languageID, contentDate);
            return (IMultipleResults)result.ReturnValue;
        }

        [Function(Name = "pub.Scrapbook_Select")]
        [ResultTypeAttribute(typeof(ScrapbookImage))]
        internal IMultipleResults VisitorScrapbook_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorDbId,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "BigInt")] System.Nullable<Int32> memberId,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "ProductTypeID", DbType = "TinyInt")] System.Nullable<Byte> productTypeId,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "BrandID", DbType = "TinyInt")] System.Nullable<Byte> brandId,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "New", DbType = "Bit")] System.Nullable<Boolean> isNew,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<Int16> countryId,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<Byte> languageId,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<DateTime> contentDate)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), visitorDbId, memberId, productTypeId, brandId, isNew, countryId, languageId, contentDate);
            return (IMultipleResults)result.ReturnValue;
        }

        [Function(Name = "pub.Member_Login")]
        [ResultTypeAttribute(typeof(memberLoginStatus))]
        internal IMultipleResults MemberSignIn_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorDbId,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Email", DbType = "NVarChar(256)")] string email,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Password", DbType = "NVarChar(64)")] string newPassword)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), visitorDbId, email, newPassword);
            return (IMultipleResults)result.ReturnValue;

        }


        [Function(Name = "pub.Member_SoftSignIn_Select")]
        [ResultTypeAttribute(typeof(MemberDetail))]
        [ResultTypeAttribute(typeof(countryRegion))]
        [ResultTypeAttribute(typeof(countryItem))]
        internal IMultipleResults MemberDetails_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorDbId,
                                                               [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<DateTime> contentDate,
                                                               [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "Int")] System.Nullable<Int32> memberId)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), visitorDbId, contentDate, memberId);
            return (IMultipleResults)result.ReturnValue;

        }

        internal string getTradeAccountPriceListCode(int tradeAccountId)
        {
            string result = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT 
                        TPL.Code 
                    FROM 
                        dbo.TradeAccount AS TA
                            INNER JOIN dbo.TradePriceList AS TPL ON TPL.TradePriceListID = TA.TradePriceListID
                    WHERE
                        TA.TradeAccountID = @TradeAccountID", conn)
                {
                    CommandType = CommandType.Text
                };

                cmd.Parameters.AddWithValue("@TradeAccountID", tradeAccountId);

                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result = reader.GetString(0);
                }
            }

            return result;
        }

        internal void fetchImageIdsFromCodes(MertexerV2.Models.Accounts.OrderDeliveryInfoReturn objInput)
        {
            List<imageIdFetchResult> results = new List<imageIdFetchResult>();
 
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT
                        PC.ProductCode,
                        P.ProductID,
	                    ISNULL(ImageID, 117989) 
                    FROM 
	                    @ProductCodes AS PC
	                LEFT OUTER JOIN dbo.Product AS P ON P.Code = PC.ProductCode
	                LEFT OUTER JOIN dbo.Product_Image AS I ON I.ProductID = P.ProductID AND
											                  I.PerspectiveID = 1", conn)
                {
                    CommandType = CommandType.Text
                };

                var pcs = new DataTable();

                pcs.Columns.Add("ProductCode");

                foreach(var i in objInput.OrderLines)
                {
                    pcs.Rows.Add(i.Product);
                }

                var param = cmd.Parameters.AddWithValue("@ProductCodes", pcs);

                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.ProductCodeTableType";

                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    results.Add(new imageIdFetchResult()
                    {
                        ProductCode = reader.GetString(0),
                        ProductID = reader.IsDBNull(1) ? null : (int?)reader.GetInt32(1),
                        ImageID = reader.IsDBNull(2) ? null : (long?)reader.GetInt64(2)
                    });
                }
            }

            objInput.OrderLines.ForEach(c =>
            {
                var v = results.Where(p => p.ProductCode == c.Product).FirstOrDefault();

                if (v != null)
                {
                    c.ImageID = v.ImageID;
                    c.ProductID = v.ProductID;
                }
            });
        }

        internal IEnumerable<ViewModels.Trade.TradeCountry> GetCountries()
        {
            var results = new List<ViewModels.Trade.TradeCountry>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT 
                        CountryID,
                        Code,
                        Name,
                        LocalName,
                        RegionID,
                        ContinentID,
                        PriceListID,
                        IsCountry,
                        SortOrder,
                        DeletedDT
                    FROM 
                        dbo.Country", conn)
                {
                    CommandType = CommandType.Text
                };

                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    results.Add(new ViewModels.Trade.TradeCountry()
                    {
                        CountryID = reader.GetInt16(0),
                        Code = reader.GetString(1),
                        Name = reader.GetString(2),
                        LocalName = reader.GetString(3),
                        RegionID = reader.IsDBNull(4) ? (byte?)null : reader.GetByte(4),
                        ContinentID = reader.IsDBNull(5) ? (byte?)null : reader.GetByte(5),
                        PriceListID = reader.IsDBNull(6) ? (byte?)null : reader.GetByte(6),
                        IsCountry = reader.GetBoolean(7),
                        SortOrder = reader.GetInt16(8),
                        DeletedDT = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9)
                    });
                }
            }

            return results;
        }

        internal List<TradeAccountAddressCleanse> tradeAccountAddressCleanseList(string tradeAccountNumber)
        {
            var results = new List<TradeAccountAddressCleanse>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT 
                        TradeAccountAddressCleanseID,
                        TradeAccountCode,
                        DeliveryReference,
                        SourceAddressHash,
                        SourceAddress,
                        Status,
                        CreatedDT,
                        LookupDT,
                        DeletedDT
                    FROM 
                        portal.TradeAccountAddressCleanse AS TAAC
                    WHERE
                        TAAC.TradeAccountCode = @TradeAccountNumber", conn)
                {
                    CommandType = CommandType.Text
                };

                cmd.Parameters.AddWithValue("@TradeAccountNumber", tradeAccountNumber);

                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    results.Add(new TradeAccountAddressCleanse()
                    {
                        TradeAccountAddressCleanseID = reader.GetInt64(0),
                        TradeAccountCode = reader.GetString(1),
                        DeliveryReference = reader.GetString(2),
                        SourceAddressHash = reader.GetString(3),
                        SourceAddress = reader.GetString(4),
                        Status = reader.IsDBNull(5) ? null : reader.GetString(5),
                        CreatedDT = reader.GetDateTime(6),
                        LookupDT = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                        DeletedDT = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8)
                    });
                }
            }

            return results;
        }

        internal int insertTradeAccountAddressClease(TradeAccountAddressCleanse a)
        {
            int result = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(@"INSERT INTO
                    portal.TradeAccountAddressCleanse
                    (
                        TradeAccountCode,
                        DeliveryReference,
                        SourceAddressHash,
                        SourceAddress,
                        Status,
                        CreatedDT
                    )
                    VALUES
                    (
                        @TradeAccountNumber,
                        @DeliveryReference,
                        @SourceAddressHash,
                        @SourceAddress,
                        @Status,
                        @CreatedDT
                    )", conn)
                {
                    CommandType = CommandType.Text
                };

                cmd.Parameters.AddWithValue("@TradeAccountNumber", a.TradeAccountCode);
                cmd.Parameters.AddWithValue("@DeliveryReference", a.DeliveryReference);
                cmd.Parameters.AddWithValue("@SourceAddressHash", a.SourceAddressHash);
                cmd.Parameters.AddWithValue("@SourceAddress", a.SourceAddress);
                cmd.Parameters.AddWithValue("@Status", a.Status ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedDT", DateTime.Now);

                conn.Open();

                result = cmd.ExecuteNonQuery();

            }

            return result;
        }

        internal int updateTradeAccountAddressCleanseHash(TradeAccountAddressCleanse a)
        {
            int result = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(@"UPDATE
                        portal.TradeAccountAddressCleanse
                    SET
                        SourceAddressHash = @Hash
                    WHERE 
                        TradeAccountAddressCleanseID = @TradeAccountAddressCleanseID", conn)
                {
                    CommandType = CommandType.Text
                };

                cmd.Parameters.AddWithValue("@TradeAccountAddressCleanseID", a.TradeAccountAddressCleanseID);

                conn.Open();

                result = cmd.ExecuteNonQuery();

            }

            return result;
        }

        [Function(Name = "mbr.TradeMember_Select")]
        [ResultTypeAttribute(typeof(TradeAccountDetail))]
        [ResultTypeAttribute(typeof(TradeAccountAdmin))]
        [ResultTypeAttribute(typeof(TradeAccountUser))]
        internal IMultipleResults TradeMember_Select_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAccountID", DbType = "Int")] int tradeAccountId,
                                                               [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "Int")] int memberId,
                                                               [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] short countryId,
                                                               [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] byte languageId)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), tradeAccountId, memberId, countryId, languageId);
            return (IMultipleResults)result.ReturnValue;
        }

        [Function(Name = "pub.MemberAddress_Select")]
        [ResultTypeAttribute(typeof(MemberAddress))]
        internal IMultipleResults MemberAddress_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorDbId,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberAddressID", DbType = "BigInt")] System.Nullable<Int64> memberAddressId,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), visitorDbId, memberAddressId, countryID, languageID);
            return (IMultipleResults)result.ReturnValue;

        }

        [Function(Name = "pub.MemberAddressDefault_Select")]
        [ResultType(typeof(MemberAddress))]
        internal ISingleResult<MemberAddress> GetDefaultMemberAddress ([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "Int")] int memberId,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] short countryId)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), memberId, countryId);
            return (ISingleResult<MemberAddress>)result.ReturnValue;
        }



        [Function(Name = "pub.Order_History_Generate")]
        [ResultTypeAttribute(typeof(OrderHistoryHeader))]
        [ResultTypeAttribute(typeof(OrderHistoryLine))]
        [ResultTypeAttribute(typeof(OrderHistoryLineTracking))]
        [ResultTypeAttribute(typeof(OrderHistoryOrderStatus))]
        [ResultTypeAttribute(typeof(OrderHistoryReturnReason))]
        [ResultTypeAttribute(typeof(OrderHistoryPreviousReturn))]
        internal IMultipleResults OrderHistory_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorDbId,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "SearchString", DbType = "NVarChar(20)")] string searchString,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "HistoryDays", DbType = "SmallInt")] System.Nullable<short> historyDays,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "OrderStatusID", DbType = "TinyInt")] System.Nullable<byte> orderStatusId,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TransactionID", DbType = "Int")] System.Nullable<int> transactionId = null)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), visitorDbId, countryID, languageID, searchString, historyDays, orderStatusId, transactionId);
            return (IMultipleResults)result.ReturnValue;

        }


        [Function(Name = "pub.Return_Paperwork_Generate")]
        [ResultTypeAttribute(typeof(memberReturnHeader))]
        [ResultTypeAttribute(typeof(memberReturnItem))]
        [ResultTypeAttribute(typeof(memberReturnDetail))]
        internal IMultipleResults GenerateReturnPaperwork_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "ReturnHeadID", DbType = "Int")] System.Nullable<Int32> returnHeadID,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorDbId)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), returnHeadID, languageID, visitorDbId);
            return (IMultipleResults)result.ReturnValue;

        }


        [Function(Name = "pub.Transaction_Receipt_Generate")]
        [ResultTypeAttribute(typeof(receiptSummary))]
        [ResultTypeAttribute(typeof(receiptHeader))]
        [ResultTypeAttribute(typeof(receiptLine))]
        internal IMultipleResults GenerateReceipt_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TransactionID", DbType = "Int")] System.Nullable<Int32> transactionId,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorDbId)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), transactionId, visitorDbId);
            return (IMultipleResults)result.ReturnValue;

        }
        
        [Function(Name = "pub.Member_Select")]
        [ResultTypeAttribute(typeof(MemberDetail))]
        internal ISingleResult<MemberDetail> Member_SelectByGUID([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "GUID", DbType = "UniqueIdentifier")] Guid memberGuid)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), memberGuid);
            return (ISingleResult<MemberDetail>)result.ReturnValue;
        }

        [Function(Name = "pub.Member_Select")]
        [ResultTypeAttribute(typeof(MemberDetail))]
        internal ISingleResult<MemberDetail> Member_SelectByID([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "Int")] int memberId)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), memberId);
            return (ISingleResult<MemberDetail>)result.ReturnValue;
        }

        [Function(Name = "pub.Member_Select")]
        [ResultTypeAttribute(typeof(MemberDetail))]
        internal ISingleResult<MemberDetail> Member_SelectByEmail([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Email", DbType = "NVarChar(256)")] string email)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), email);
            return (ISingleResult<MemberDetail>)result.ReturnValue;
        }

        [Function(Name = "pub.TradeMember_Authentication_Commence")]
        [ResultTypeAttribute(typeof(tradeAuthGuidCheckResponse))]
        internal ISingleResult<tradeAuthGuidCheckResponse> callTradeMemberAuthCommence([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAuthenticationGUID", DbType = "UniqueIdentifier")] Guid authGuid)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), authGuid);
            return (ISingleResult<tradeAuthGuidCheckResponse>)result.ReturnValue;
        }

        [Function(Name = "pub.TradeMember_Authentication_Complete")]
        [ResultTypeAttribute(typeof(tradeAuthCompleteResponse))]
        internal ISingleResult<tradeAuthCompleteResponse> callTradeMemberAuthComplete(
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAuthenticationGUID", DbType = "UniqueIdentifier")] Guid authGuid,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Postcode", DbType = "NVarChar(20)")] string postcode,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "PhoneNumber", DbType = "NVarChar(20)")] string phoneNumber,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "AccountCode", DbType = "NVarChar(50)")] string accountCode,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "FullName", DbType = "NVarChar(50)")] string fullName,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Password", DbType = "NVarChar(64)")] string password,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "DisplayTradePrice", DbType = "Bit")] bool displayTradePrice)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), authGuid, postcode, phoneNumber, accountCode, fullName, password, displayTradePrice);
            return (ISingleResult<tradeAuthCompleteResponse>)result.ReturnValue;
        }

        internal async Task<object> FetchHotspots(PassHotspotCacheFetch objFetch)
        {
            object result = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
                {
                    SqlCommand cmd = new SqlCommand(@"cache.Hotspot_Select", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    var dt = new DataTable();

                    dt.Columns.Add("ImageID", typeof(long));

                    objFetch.ImageIDs = objFetch.ImageIDs.Distinct();

                    foreach (var i in objFetch.ImageIDs)
                    {
                        dt.Rows.Add(i);
                    }

                    switch (objFetch.ContentType)
                    {
                        case dgContentType.landingPage:
                            cmd.Parameters.AddWithValue("@PageID", objFetch.ContentID);
                            break;
                        case dgContentType.productPage:
                            cmd.Parameters.AddWithValue("@ProductPageID", objFetch.ContentID);
                            break;
                        case dgContentType.collectionPage:
                            cmd.Parameters.AddWithValue("@CollectionID", (short)objFetch.ContentID);
                            break;
                        case dgContentType.articlePage:
                            cmd.Parameters.AddWithValue("@ArticleID", objFetch.ContentID);
                            break;
                    }

                    cmd.Parameters.AddWithValue("@ImageIDs", dt);
                    cmd.Parameters.AddWithValue("@CountryID", objFetch.CountryID);
                    cmd.Parameters.AddWithValue("@LanguageID", objFetch.LanguageID);
                    cmd.Parameters.AddWithValue("@Date", objFetch.ContentDate);
                    cmd.Parameters.AddWithValue("@VisitorID", objFetch.VisitorID);
                    cmd.Parameters.AddWithValue("@MemberID", objFetch.MemberID);
                    cmd.Parameters.AddWithValue("@TradeAccountID", objFetch.TradeAccountID);
                    cmd.Parameters.AddWithValue("@IsTrade", objFetch.TradeAccountID.HasValue);
                    cmd.Parameters.AddWithValue("@DisplayTradePrice", objFetch.DisplayTradePrice);
                    cmd.Parameters.AddWithValue("@CountryMatch", objFetch.CountryMatch);

                    cmd.CommandTimeout = 3;

                    conn.Open();

                    var reader = await cmd.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        result = reader.IsDBNull(0) ? JsonConvert.DeserializeObject("{ images: [], timedOut: false }") : JsonConvert.DeserializeObject(reader.GetString(0));
                    }
                }
            }
            catch (SqlException sqe) when (sqe.Number == -2)
            {
                result = JsonConvert.DeserializeObject("{ images: [], timedOut: true }");
            }

            return result;
        }

        public async Task<int> TradeLogin_Unregistered(PassTradeLoginUnregisteredRequest obj, long visitorId)
        {
            _ = await this.Connection.QueryAsync("pub.TradeMember_Authentication_UnregisteredLoginSubmit", new { 
                obj.TradeAccountCode,
                FullName = obj.FullName,
                EmailAddress = obj.Email,
                VisitorID = visitorId
            }, null, null, CommandType.StoredProcedure);

            return 0;
        }

        internal bool DotDigitalDataCleanse_RecordResponse(string emailAddress, short countryId, byte languageId, bool response)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                var result = conn.QuerySingle<bool>("pub.DotDigital_DataCleanse_RecordResponse", new { 
                    emailAddress, 
                    response,
                    countryId,
                    languageId
                }, null, null, CommandType.StoredProcedure);

                return result;
            }
        }
    }

    public class imageIdFetchResult
    {
        public int? ProductID { get; set; }
        public string ProductCode { get; set; }
        public long? ImageID { get; set; }
    }
}