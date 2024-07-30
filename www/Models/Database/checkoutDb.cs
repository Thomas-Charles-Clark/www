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
using Microsoft.SqlServer.Server;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Helper;
using Braintree;
using AmazonPay;
using wwwDesignersGuild.Models.Checkout;
using Dapper;

namespace wwwDesignersGuild.Models.Database
{


    public class saveCheckoutAddressCollection : List<passCheckoutSaveAddressData>, IEnumerable<SqlDataRecord>
    {
        IEnumerator<SqlDataRecord> IEnumerable<SqlDataRecord>.GetEnumerator()
        {
            SqlDataRecord ret = new SqlDataRecord(
                new SqlMetaData("CheckoutAddressID", SqlDbType.TinyInt, true, false, SortOrder.Unspecified, -1),
                new SqlMetaData("CheckoutBasketID", SqlDbType.BigInt),
                new SqlMetaData("DeliveryOptionID", SqlDbType.TinyInt),
                new SqlMetaData("MemberAddressID", SqlDbType.BigInt),
                new SqlMetaData("FullName", SqlDbType.NVarChar, 50),
                new SqlMetaData("CompanyName", SqlDbType.NVarChar, 35),
                new SqlMetaData("AddressLine1", SqlDbType.NVarChar, 60),
                new SqlMetaData("AddressLine2", SqlDbType.NVarChar, 60),
                new SqlMetaData("Town", SqlDbType.NVarChar, 50),
                new SqlMetaData("County", SqlDbType.NVarChar, 40),
                new SqlMetaData("Postcode", SqlDbType.NVarChar, 20),
                new SqlMetaData("CountryID", SqlDbType.SmallInt),
                new SqlMetaData("PhoneNumber", SqlDbType.NVarChar, 20),
                new SqlMetaData("UPSAPID", SqlDbType.NVarChar, 10),
                new SqlMetaData("ZipTax", SqlDbType.Xml)
                );

            foreach (passCheckoutSaveAddressData data in this)
            {
                ret.SetValue(1, data.CheckoutBasketID);
                
                ret.SetByte(2, data.DeliveryOptionID);

                ret.SetValue(3, data.MemberAddressID);

                if (data.FullName == null)
                {
                    ret.SetString(4, "");
                }
                else
                {
                    ret.SetString(4, data.FullName);
                }

                if (data.CompanyName == null)
                {
                    ret.SetDBNull(5);
                }
                else
                {
                    ret.SetString(5, data.CompanyName);
                }

                ret.SetString(6, data.AddressLine1);

                if (data.AddressLine2 == null)
                {
                    ret.SetDBNull(7);
                }
                else
                {
                    ret.SetString(7, data.AddressLine2);
                }

                ret.SetString(8, data.Town);
                
                if (data.County == null)
                {
                    //ret.SetDBNull(9);
                    ret.SetString(9, ""); // Grrr. County is non-nullable in dbo.CheckoutAddressTableType, and it's a PITA to change.
                                          // Counties are not mandatory for delivery in the UK: haven't been for years.
                                          // Amazon does not mandate a county in a delivery address.
                }
                else
                {
                    ret.SetString(9, data.County);
                }

                if (data.Postcode == null)
                {
                    ret.SetDBNull(10);
                }
                else
                {
                    ret.SetString(10, data.Postcode);
                }

                ret.SetInt16(11, data.CountryID);
                
                if (data.PhoneNumber == null)
                {
                    ret.SetDBNull(12);
                }
                else
                {
                    ret.SetString(12, data.PhoneNumber);
                }

                if (data.UPSAPID == null)
                {
                    ret.SetDBNull(13);
                }
                else
                {
                    ret.SetValue(13, data.UPSAPID);
                }

                if (data.ZipTax == null)
                {
                    ret.SetDBNull(14);
                }
                else
                {
                    ret.SetValue(14, data.ZipTax);
                }

                yield return ret;
            }
        }
    }


    public class checkoutDb
    {
        public checkoutDeliveryAddress SelectDeliveryAddress(long checkoutId, byte languageId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                return conn.QuerySingle<checkoutDeliveryAddress>("pub.CheckoutAddress_Select", new { 
                    CheckoutID = checkoutId,
                    LanguageID = languageId
                }, null, 10, CommandType.StoredProcedure);
            }
        }

        public int saveLineTreatments(PassLineTreatment objSave)
        {
            var result = -1;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(@"UPDATE ecm.Checkout_Basket
                                                        SET OtherTreatmentRequested = @OtherTreatment,
                                                            StainTreatmentRequested = @StainTreatment,
                                                            DomesticUpholsteryTreatmentRequested = @DomesticUpholsteryTreatment,
                                                            Crib5TreatmentRequested = @Crib5Treatment,
                                                            ExpressTreatmentRequested = @ExpressTreatment
                                                    WHERE 
                                                        CheckoutBasketID = @CheckoutBasketID
                                                    ", conn)
                {
                    CommandType = CommandType.Text
                };
                cmd.Parameters.AddWithValue("@CheckoutBasketID", objSave.CheckoutBasketID);
                cmd.Parameters.AddWithValue("@OtherTreatment", objSave.OtherTreatment ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@StainTreatment", objSave.StainTreatment);
                cmd.Parameters.AddWithValue("@DomesticUpholsteryTreatment", objSave.DomesticUpholsteryTreatment);
                cmd.Parameters.AddWithValue("@Crib5Treatment", objSave.Crib5Treatment);
                cmd.Parameters.AddWithValue("@ExpressTreatment", objSave.ExpressTreatment);

                conn.Open();

                result = cmd.ExecuteNonQuery();

                conn.Close();
            }

            return result;
        }

        public int saveLineMemo(PassLineMemo objSave)
        {
            var result = -1;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(@"UPDATE ecm.Checkout_Basket
                                                        SET Memo = @LineMemo,
                                                            LineReference = @LineReference
                                                    WHERE 
                                                        CheckoutBasketID = @CheckoutBasketID
                                                    ", conn)
                {
                    CommandType = CommandType.Text
                };
                cmd.Parameters.AddWithValue("@CheckoutBasketID", objSave.CheckoutBasketID);
                cmd.Parameters.AddWithValue("@LineMemo", String.IsNullOrEmpty(objSave.Memo) ? (object)DBNull.Value : objSave.Memo);
                cmd.Parameters.AddWithValue("@LineReference", String.IsNullOrEmpty(objSave.Reference) ? (object)DBNull.Value : objSave.Reference);

                conn.Open();

                result = cmd.ExecuteNonQuery();

                conn.Close();
            }

            return result;
        }

        public int saveCutLengthBreakdown(PassCutLengthBreakdown objSave)
        {
            var result = -1;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand(@"UPDATE ecm.Checkout_Basket
                                                        SET CutPieceBreakdown = @CutPieceBreakdown,
                                                            Batch = @Batch
                                                    WHERE 
                                                        CheckoutBasketID = @CheckoutBasketID
                                                    ", conn)
                {
                    CommandType = CommandType.Text
                };
                cmd.Parameters.AddWithValue("@CheckoutBasketID", objSave.CheckoutBasketID);
                cmd.Parameters.AddWithValue("@Batch", String.IsNullOrEmpty(objSave.Batch) ? (object)DBNull.Value : objSave.Batch);
                cmd.Parameters.AddWithValue("@CutPieceBreakdown", objSave.Breakdown ?? (object)DBNull.Value);

                conn.Open();

                result = cmd.ExecuteNonQuery();

                conn.Close();
            }

            return result;
        }


        #region Preflight Checks


        public Boolean preflightCheckWelcome(Int64 checkoutId, short countryId)
        {
            Boolean storeResults = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_Welcome_PreflightCheck", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                cmd.Parameters.AddWithValue("@CountryID", countryId);

                SqlParameter retval = cmd.Parameters.Add("@Passed", SqlDbType.Bit);
                retval.Direction = ParameterDirection.ReturnValue;

                conn.Open();


                cmd.ExecuteNonQuery();

                String returnValue = retval.Value.ToString();

                if (returnValue == "1")
                {
                    storeResults = true;
                }

            }

            return storeResults;
        }


        public preflightCheckAddressResult preflightCheckAddress(Int64 checkoutId, short countryId)
        {
            preflightCheckAddressResult storeResults = new preflightCheckAddressResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_Address_PreflightCheck", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                cmd.Parameters.AddWithValue("@CountryID", countryId);

                SqlParameter retval = cmd.Parameters.Add("@Passed", SqlDbType.Bit);
                retval.Direction = ParameterDirection.ReturnValue;

                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while(reader.Read())
                {
                    storeResults.Passed = reader.GetBoolean(0);
                    storeResults.Email = reader.IsDBNull(1) ? null : reader.GetString(1);
                    storeResults.MertexCustomerAccountNumber = reader.IsDBNull(2) ? null : reader.GetString(2);
                    storeResults.CurrencyCode = reader.IsDBNull(3) ? null : reader.GetString(3);
                }

                conn.Close();
            }

            return storeResults;
        }

        public Boolean preflightCheckGiftVoucher(Int64 checkoutId, short countryId)
        {
            Boolean storeResults = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_GiftVoucher_Sale_PreflightCheck", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                cmd.Parameters.AddWithValue("@CountryID", countryId);

                SqlParameter retval = cmd.Parameters.Add("@Passed", SqlDbType.Bit);
                retval.Direction = ParameterDirection.ReturnValue;

                conn.Open();


                cmd.ExecuteNonQuery();

                String returnValue = retval.Value.ToString();

                if (returnValue == "1")
                {
                    storeResults = true;
                }

            }

            return storeResults;
        }


        public Boolean preflightCheckGiftBox(Int64 checkoutId, short countryId)
        {
            Boolean storeResults = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_GiftBox_PreflightCheck", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                cmd.Parameters.AddWithValue("@CountryID", countryId);

                SqlParameter retval = cmd.Parameters.Add("@Passed", SqlDbType.Bit);
                retval.Direction = ParameterDirection.ReturnValue;

                conn.Open();


                cmd.ExecuteNonQuery();

                String returnValue = retval.Value.ToString();

                if (returnValue == "1")
                {
                    storeResults = true;
                }

            }

            return storeResults;
        }


        public Boolean preflightCheckSummary(Int64 checkoutId, short countryId)
        {
            Boolean storeResults = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_Summary_PreflightCheck", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                cmd.Parameters.AddWithValue("@CountryID", countryId);

                SqlParameter retval = cmd.Parameters.Add("@Passed", SqlDbType.Bit);
                retval.Direction = ParameterDirection.ReturnValue;

                conn.Open();


                cmd.ExecuteNonQuery();

                String returnValue = retval.Value.ToString();

                if (returnValue == "1")
                {
                    storeResults = true;
                }

            }

            return storeResults;
        }


        public Boolean preflightCheckPay(Int64 checkoutId, short countryId)
        {
            Boolean storeResults = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_Pay_PreflightCheck", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                cmd.Parameters.AddWithValue("@CountryID", countryId);

                SqlParameter retval = cmd.Parameters.Add("@Passed", SqlDbType.Bit);
                retval.Direction = ParameterDirection.ReturnValue;

                conn.Open();


                cmd.ExecuteNonQuery();

                String returnValue = retval.Value.ToString();

                if (returnValue == "1")
                {
                    storeResults = true;
                }

            }

            return storeResults;
        }


        public Boolean preflightCheckReceipt(Guid paymentToken, short countryId)
        {
            Boolean storeResults = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_Receipt_PreflightCheck", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@GUID", paymentToken);
                cmd.Parameters.AddWithValue("@CountryID", countryId);

                SqlParameter retval = cmd.Parameters.Add("@Passed", SqlDbType.Bit);
                retval.Direction = ParameterDirection.ReturnValue;

                conn.Open();


                cmd.ExecuteNonQuery();


                String returnValue = retval.Value.ToString();

                if (returnValue == "1")
                {
                    storeResults = true;
                }

            }

            return storeResults;
        }

        public DatabaseJSONResponse validateBasket(long visitorId, int? memberId, int? tradeAccountId, DateTime useDate, short countryId, byte languageId, bool displayTradePrice)
        {
            var result = new DatabaseJSONResponse();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();

                comm.CommandText = @"pub.Basket_Validate";
                comm.CommandType = System.Data.CommandType.StoredProcedure;

                var dt = new DataTable();

                comm.Parameters.AddWithValue("@VisitorID", visitorId);
                comm.Parameters.AddWithValue("@MemberID", memberId ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@IsTrade", tradeAccountId.HasValue);
                comm.Parameters.AddWithValue("@TradeAccountID", tradeAccountId ?? (object)DBNull.Value);
                comm.Parameters.AddWithValue("@Date", useDate);
                comm.Parameters.AddWithValue("@CountryID", countryId);
                comm.Parameters.AddWithValue("@LanguageID", languageId);
                comm.Parameters.AddWithValue("@DisplayTradePrice", displayTradePrice);

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

        #endregion

        public CheckoutParameters getBraintreeCredentials(Int64 checkoutId)
        {
            try
            {
                CheckoutParameters result = new CheckoutParameters();

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
                {
                    SqlCommand cmd = new SqlCommand("pub.Checkout_MerchantInfo_Select", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@CheckoutId", checkoutId);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        result.AcceptAmex = (bool)reader["AcceptAmex"];
                        result.AcceptDiscover = (bool)reader["AcceptDiscover"];
                        result.AcceptMaestro = (bool)reader["AcceptMaestro"];
                        result.AcceptMaestroUK = (bool)reader["AcceptMaestroUK"];
                        result.AcceptMasterCard = (bool)reader["AcceptMasterCard"];
                        result.AcceptVisa = (bool)reader["AcceptVisa"];
                        result.AVS = (bool)reader["AVS"];
                        result.Currency = (string)reader["Currency"];
                        result.Environment = (string)reader["Environment"];
                        result.Mandate3DSecure = (bool)reader["Mandate3DSecure"];
                        result.MerchantAccountID = (string)reader["MerchantAccountID"];
                        result.MerchantID = (string)reader["MerchantID"];
                        result.PayPal = (bool)reader["PayPal"];
                        result.PriceList = (string)reader["PriceList"];
                        result.PrivateKey = (string)reader["PrivateKey"];
                        result.PublicKey = (string)reader["PublicKey"];
                        if (reader["AmazonClientId"] != DBNull.Value)
                            result.AmazonClientId = (string)reader["AmazonClientId"];
                        if (reader["AmazonSecret"] != DBNull.Value)
                            result.AmazonSecret = (string)reader["AmazonSecret"];
                        if (reader["AmazonMerchantId"] != DBNull.Value)
                            result.AmazonMerchantId = (string)reader["AmazonMerchantId"];
                        if (reader["AmazonRegion"] != DBNull.Value)
                        {
                            var region = (byte)reader["AmazonRegion"];
                            result.AmazonRegion = (Regions.supportedRegions)region;
                        }
                        if (reader["AmazonPayAPIURL"] != DBNull.Value)
                            result.AmazonPayAPIURL = (string)reader["AmazonPayAPIURL"];
                        if (reader["AmazonAccessKey"] != DBNull.Value)
                            result.AmazonAccessKey = (string)reader["AmazonAccessKey"];
                        if (reader["AmazonLiveAPI"] != DBNull.Value)
                            result.AmazonLiveAPI = (bool)reader["AmazonLiveAPI"];
                        
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Boolean checkoutContactReferralSave(Int32 TransactionID)
        {
            Boolean result = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_ContactReferral_Save", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@TransactionID", TransactionID);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result = (Boolean)reader[0];
                }

            }

            return result;
        }

        public checkoutCreateSnapshotBasket createCheckoutBasket(
            long visitorDbId,
            byte languageId,
            short countryId,
            DateTime snapshotCurrentDateTime,
            string cfCountryHeader,
            int? tradeAccountId)
        {
            try
            {
                var storeResults = new checkoutCreateSnapshotBasket();

                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
                {
                    var cmd = new SqlCommand("pub.Checkout_Initialise", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                    cmd.Parameters.AddWithValue("@LanguageID", languageId);
                    cmd.Parameters.AddWithValue("@CountryID", countryId);
                    cmd.Parameters.AddWithValue("@StartDT", snapshotCurrentDateTime);
                    cmd.Parameters.AddWithValue("@Mobile", dgFramework.IsMobileDevice());
                    cmd.Parameters.AddWithValue("@CloudFlareCountryCode", cfCountryHeader);
                    cmd.Parameters.AddWithValue("@TradeAccountID", tradeAccountId ?? (object)DBNull.Value);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        storeResults.CheckoutID = (Int64)reader[0];
                        storeResults.CheckoutExpiryDT = (DateTime)reader[1];
                        storeResults.CheckoutDisplayGiftScreen = false;
                        storeResults.CheckoutDisplayGiftVoucherScreen = false;
                        if (reader[2].ToString() == "1")
                        {
                            storeResults.CheckoutDisplayGiftScreen = true;
                        }
                        if (reader[3].ToString().ToLower() == "true")
                        {
                            storeResults.CheckoutDisplayGiftVoucherScreen = true;
                        }

                        storeResults.SubTotal = reader.IsDBNull(4) ? null : (decimal?)reader.GetDecimal(4);
                    }
                }

                return storeResults;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


        //public Boolean validateCheckoutBasket(
        //    decimal totalPrice, 
        //    int totalQuantity, 
        //    DateTime snapshotCurrentDateTime, 
        //    short countryId, 
        //    byte languageId, 
        //    long visitorDbId, 
        //    int? memberId, 
        //    int? tradeAccountID)
        //{
        //    Boolean storePassResults = false;

        //    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
        //    {
        //        SqlCommand cmd = new SqlCommand("pub.Basket_Check", conn);
        //        cmd.CommandType = CommandType.StoredProcedure;
                
        //        cmd.Parameters.AddWithValue("@TotalPrice", totalPrice);
        //        cmd.Parameters.AddWithValue("@TotalQuantity", totalQuantity);
        //        cmd.Parameters.AddWithValue("@Date", snapshotCurrentDateTime);
        //        cmd.Parameters.AddWithValue("@CountryID", countryId);
        //        cmd.Parameters.AddWithValue("@LanguageID", languageId);
        //        cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
        //        cmd.Parameters.AddWithValue("@MemberID", memberId);
        //        cmd.Parameters.AddWithValue("@TradeAccountID", (object)tradeAccountID ?? DBNull.Value);
                

        //        conn.Open();
        //        SqlDataReader reader = cmd.ExecuteReader();

        //        while (reader.Read())
        //        {
        //            storePassResults = (Boolean)reader[0];
        //        }

        //    }

        //    return storePassResults;
        //}


        public Boolean checkoutLogInsert(Int64 checkoutId, int checkoutActionId, string details)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.CheckoutLog_Insert", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                cmd.Parameters.AddWithValue("@CheckoutActionID", checkoutActionId);
                cmd.Parameters.AddWithValue("@Details", details);


                conn.Open();
                cmd.ExecuteNonQuery();

            }

            return true;
        }

        public Boolean saveCheckoutType(Int64 checkoutId, DateTime date, dgCheckoutType type, int memberId, string email, int? siteMemberId, string loginStatus, int? tradeAccountId)
        {            
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_Type_Save", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);

                cmd.Parameters.AddWithValue("@Date", date);
                cmd.Parameters.AddWithValue("@CheckoutTypeID", type);
                cmd.Parameters.AddWithValue("@MemberID", memberId);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@SiteMemberID", siteMemberId);
                cmd.Parameters.AddWithValue("@LoginStatus", loginStatus);
                cmd.Parameters.AddWithValue("@TradeAccountID", tradeAccountId);

                SqlParameter updated = cmd.Parameters.Add("@Updated", SqlDbType.TinyInt);

                updated.Direction = ParameterDirection.Output;

                conn.Open();
                cmd.ExecuteNonQuery();

                return Convert.ToByte(updated.Value) > 0;
            }
        }

        public Boolean checkoutTypeIsSet(Int64 checkoutId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_GetMaxStage", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);                
                conn.Open();

                bool result = Convert.ToInt32(cmd.ExecuteScalar()) > 1;
                return result; 
            }
        }

        public dgCheckoutType getCheckoutType(Int64 checkoutId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_CheckoutType_Select", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                conn.Open();

                return (dgCheckoutType)Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public Boolean saveCheckoutAddress(Int64 checkoutId, List<passCheckoutSaveAddressData> CheckoutAddressTableType, Int64? UPSAPMemberAddressId, List<passCheckoutSaveAddressData> UPSIDAddressTableType, bool isClickCollect, bool resetDeliveryOptions)
        {
            saveCheckoutAddressCollection addressTable = new saveCheckoutAddressCollection();
            addressTable.Add(CheckoutAddressTableType[0]);

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_Address_Save", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                saveCheckoutAddressCollection upsIDAddressTable = new saveCheckoutAddressCollection();
                if (UPSIDAddressTableType != null)
                {
                    if (UPSIDAddressTableType.Count == 1)
                    {
                        upsIDAddressTable.Add(UPSIDAddressTableType[0]);
                    }
                    else
                    {
                        upsIDAddressTable = null;
                    }
                }
                else
                {
                    upsIDAddressTable = null;
                }

                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);

                var storeSqlParam = cmd.Parameters.AddWithValue("@CheckoutAddress", addressTable);
                storeSqlParam.SqlDbType = SqlDbType.Structured;
                storeSqlParam.TypeName = "CheckoutAddressTableType";

                var upsAPIDAddressParam = cmd.Parameters.AddWithValue("@UPSAPIDAddress", upsIDAddressTable);
                upsAPIDAddressParam.SqlDbType = SqlDbType.Structured;
                upsAPIDAddressParam.TypeName = "CheckoutAddressTableType";

                cmd.Parameters.AddWithValue("@UPSAPMemberAddressID", UPSAPMemberAddressId);
                cmd.Parameters.AddWithValue("@IsClickCollect", isClickCollect);
                cmd.Parameters.AddWithValue("@ResetDeliveryOptions", resetDeliveryOptions);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        public Boolean deleteCheckoutAddresses(Int64 checkoutId)
        {   
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_Address_Delete", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }


        public Boolean saveCheckoutDispatch(Int64 checkoutId, DataTable dispatchTableType)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_Dispatch_Save", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };


                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);

                SqlParameter storeSqlParam = cmd.Parameters.AddWithValue("@DeliveryBand", dispatchTableType);
                storeSqlParam.SqlDbType = SqlDbType.Structured;
                storeSqlParam.TypeName = "CheckoutDeliveryBandTableType";


                conn.Open();
                cmd.ExecuteNonQuery();


            }
            return true;
        }


        public Boolean saveCheckoutGiftVoucher(Int64 checkoutId, DataTable giftVoucherTableType)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_GiftVoucher_Sale_Save", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };


                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);

                SqlParameter storeSqlParamGift = cmd.Parameters.AddWithValue("@GiftVouchers", giftVoucherTableType);
                storeSqlParamGift.SqlDbType = SqlDbType.Structured;
                storeSqlParamGift.TypeName = "CheckoutGiftVoucherTableType";


                conn.Open();
                cmd.ExecuteNonQuery();

            }
            return true;
        }


        public Boolean saveCheckoutGift(Int64 checkoutId, DataTable giftBoxTableType, DataTable giftBasketTableType)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_GiftBox_Save", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };


                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);

                SqlParameter storeSqlParamGift = cmd.Parameters.AddWithValue("@CheckoutGiftBox", giftBoxTableType);
                storeSqlParamGift.SqlDbType = SqlDbType.Structured;
                storeSqlParamGift.TypeName = "CheckoutGiftBoxTableType";


                SqlParameter storeSqlParamBasket = cmd.Parameters.AddWithValue("@CheckoutBasketGiftBoxItems", giftBasketTableType);
                storeSqlParamBasket.SqlDbType = SqlDbType.Structured;
                storeSqlParamBasket.TypeName = "CheckoutBasketGiftBoxQuantityTableType2";

                conn.Open();
                cmd.ExecuteNonQuery();

            }
            return true;
        }


        public Boolean saveSelectedBillingAddress(Int64 checkoutId, Int64? memberAddressId)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_Pay_BillingAddress_Update", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };


                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                cmd.Parameters.AddWithValue("@MemberAddressID", memberAddressId);

                conn.Open();
                cmd.ExecuteNonQuery();

            }
            return true;
        }

        public Boolean saveSelectedBillingAddressFromBrainTree(Guid paymentToken, Address braintreeAddress, Int16? countryID)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_Pay_BillingAddressFromBrainTree_Update", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };


                cmd.Parameters.AddWithValue("@PaymentToken", paymentToken);
                cmd.Parameters.AddWithValue("@FullName", $"{braintreeAddress.FirstName} {braintreeAddress.LastName}");
                cmd.Parameters.AddWithValue("@CompanyName", braintreeAddress.Company);
                cmd.Parameters.AddWithValue("@AddressLine1", braintreeAddress.StreetAddress);
                cmd.Parameters.AddWithValue("@AddressLine2", braintreeAddress.ExtendedAddress);
                cmd.Parameters.AddWithValue("@Town", braintreeAddress.Locality);
                cmd.Parameters.AddWithValue("@County", braintreeAddress.Region);
                cmd.Parameters.AddWithValue("@Postcode", braintreeAddress.PostalCode);
                cmd.Parameters.AddWithValue("@Country", countryID);
                //cmd.Parameters.AddWithValue("@PhoneNumber", braintreeAddress.);

                conn.Open();
                cmd.ExecuteNonQuery();

            }
            return true;
        }

        public Boolean saveSelectedBillingAddressFromLines(Guid paymentToken, string fullName, string addressLineOne, string addressLineTwo, string town, string county, string postcode, Int16? countryID, string phoneNumber)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_Pay_BillingAddressFromBrainTree_Update", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };


                cmd.Parameters.AddWithValue("@PaymentToken", paymentToken);
                cmd.Parameters.AddWithValue("@FullName", fullName);
                cmd.Parameters.AddWithValue("@AddressLine1", addressLineOne);
                cmd.Parameters.AddWithValue("@AddressLine2", addressLineTwo);
                cmd.Parameters.AddWithValue("@Town", town);
                cmd.Parameters.AddWithValue("@County", county);
                cmd.Parameters.AddWithValue("@Postcode", postcode);
                cmd.Parameters.AddWithValue("@Country", countryID);
                cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                conn.Open();
                cmd.ExecuteNonQuery();

            }
            return true;
        }

        public Boolean checkoutPaymentTaken(
            long checkoutId, 
            string paymentRef, 
            string paymentType, 
            string paymentDetails, 
            List<MarketingPermission> marketingPermissions,
            string orderReference = null,
            string email = null,
            string phone = null)
        {
            var result = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_Payment_Taken", conn)
                {
                    CommandTimeout = 10, // 10 seconds is more than enough to write Payment details & marketing permissions

                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                cmd.Parameters.AddWithValue("@PaymentRef", paymentRef);
                cmd.Parameters.AddWithValue("@PaymentType", paymentType);
                cmd.Parameters.AddWithValue("@PaymentDetails", paymentDetails);
                cmd.Parameters.AddWithValue("@OrderReference", orderReference);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Phone", phone);

                var marketingPermission = cmd.CreateParameter();

                marketingPermission.ParameterName = "@MarketingPermission";
                marketingPermission.SqlDbType = SqlDbType.Structured;
                marketingPermission.TypeName = "dbo.MarketingPermissionTableType";

                DataTable mp = new DataTable();

                mp.Columns.Add("MarketingPermissionID", typeof(byte));
                mp.Columns.Add("Value", typeof(bool));
                mp.Columns.Add("Label", typeof(string));
                mp.Columns.Add("Default", typeof(bool));
                mp.Columns.Add("Hidden", typeof(bool));

                if (marketingPermissions != null)
                {
                    marketingPermissions.ForEach(c => {
                        mp.Rows.Add(
                            c.MarketingPermissionID,
                            c.Checked,
                            c.Label,
                            c.Default,
                            c.Hidden
                        );
                    });
                }

                marketingPermission.Value = mp;

                cmd.Parameters.Add(marketingPermission);

                conn.Open();

                result = Convert.ToBoolean(cmd.ExecuteNonQuery());
            }

            return result;
        }

        // DEFUNCT 
        // Replaced by checkoutComplete
        public Boolean checkoutSavePay(Guid paymentToken, String transactionId, String paymentType, String paymentDetails, List<MarketingPermission> marketingPermissions)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_Pay_Save", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@GUID", paymentToken);
                cmd.Parameters.AddWithValue("@PaymentRef", transactionId);
                cmd.Parameters.AddWithValue("@PaymentType", paymentType);
                cmd.Parameters.AddWithValue("@PaymentDetails", paymentDetails);

                var marketingPermission = cmd.CreateParameter();

                marketingPermission.ParameterName = "@MarketingPermission";
                marketingPermission.SqlDbType = System.Data.SqlDbType.Structured;
                marketingPermission.TypeName = "dbo.MarketingPermissionTableType";

                DataTable mp = new DataTable();

                mp.Columns.Add("MarketingPermissionID", typeof(byte));
                mp.Columns.Add("Value", typeof(bool));
                mp.Columns.Add("Label", typeof(string));
                mp.Columns.Add("Default", typeof(bool));
                mp.Columns.Add("Hidden", typeof(bool));

                if (marketingPermissions != null)
                {
                    marketingPermissions.ForEach(c => {
                        mp.Rows.Add(
                            c.MarketingPermissionID,
                            c.Checked,
                            c.Label,
                            c.Default,
                            c.Hidden
                        );
                    });
                }

                marketingPermission.Value = mp;

                cmd.Parameters.Add(marketingPermission); //  MarketingPermissionTableType READONLY
                
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        
        public Boolean checkoutComplete(long checkoutId)
        {
            var result = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_Complete", conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 60 // 01/07/2024 -- 10 seconds isn't long enough! 
                                        // = 10 // in seconds (30 is the default, if this is going to succeed it should do so in well under 10 seconds)
                };
                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                
                conn.Open();

                result = Convert.ToBoolean(cmd.ExecuteNonQuery());
            }

            return result;
        }

        public Boolean checkoutAttemptSave(
            Int64 checkoutId, 
            String summaryMessage,
            String gatewayRejectionReason,
            String threeDSecureStatus,
            String threeDSecureEnrolled,
            Boolean liabilityShifted,
            Boolean liabilityShiftPossible,
            String processorAuthCode,
            String processorResponseCode,
            String processorResponseText,
            String additionalProcessorResponse,
            String avsPostalCodeResponseCode,
            String resultDetails)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_Attempt_Save", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                cmd.Parameters.AddWithValue("@SummaryMessage", dgFramework.SimpleTruncate(summaryMessage, 200));
                cmd.Parameters.AddWithValue("@GatewayRejectionReason", gatewayRejectionReason);
                cmd.Parameters.AddWithValue("@ThreeDSecureStatus", threeDSecureStatus);
                cmd.Parameters.AddWithValue("@ThreeDSecureEnrolled", dgFramework.SimpleTruncate(threeDSecureEnrolled, 1));
                cmd.Parameters.AddWithValue("@LiabilityShifted", liabilityShifted);
                cmd.Parameters.AddWithValue("@LiabilityShiftPossible", liabilityShiftPossible);
                cmd.Parameters.AddWithValue("@ProcessorAuthCode", dgFramework.SimpleTruncate(processorAuthCode, 50));
                cmd.Parameters.AddWithValue("@ProcessorResponseCode", dgFramework.SimpleTruncate(processorResponseCode, 200));
                cmd.Parameters.AddWithValue("@ProcessorResponseText", processorResponseText);
                cmd.Parameters.AddWithValue("@AdditionalProcessorResponse", additionalProcessorResponse);
                cmd.Parameters.AddWithValue("@AVSPostalCodeResponseCode", dgFramework.SimpleTruncate(avsPostalCodeResponseCode, 1));
                cmd.Parameters.AddWithValue("@ResultDetails", resultDetails);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        public Byte checkoutPaidInFullCheck(Guid paymentToken, Int64 checkoutId)
        {
            Byte result = 0;


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {

                SqlCommand cmd = new SqlCommand("pub.Checkout_Pay_PSPSkipCheck", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@GUID", paymentToken);
                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);


                SqlParameter retval = cmd.Parameters.Add("@Passed", SqlDbType.TinyInt);
                retval.Direction = ParameterDirection.Output;

                conn.Open();


                cmd.ExecuteNonQuery();

                result = Convert.ToByte(retval.Value);

            }

            return result;
        }
        
        public String validateGiftVoucherCode(Int64 checkoutId, string voucherCode, byte languageId)
        {
            String storeResults = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {

                SqlCommand cmd = new SqlCommand("pub.Checkout_GiftVoucher_Redeem_Check", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Code", voucherCode);
                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                cmd.Parameters.AddWithValue("@LanguageID", languageId);

                SqlParameter retval = cmd.Parameters.Add("@Passed", SqlDbType.Bit);
                //retval.Direction = ParameterDirection.InputOutput;
                retval.Direction = ParameterDirection.Output;
                //retval.Value = 0;


                SqlParameter retvalMsg = cmd.Parameters.Add("@FailedMessage", SqlDbType.NVarChar, 100);
                //retval.Direction = ParameterDirection.InputOutput;
                retvalMsg.Direction = ParameterDirection.Output;
                //retvalMsg.Value = "";

                conn.Open();
                cmd.ExecuteNonQuery();
                
                
                String returnValue = retval.Value.ToString();
                String failedMessage = retvalMsg.Value.ToString();

                if (returnValue.ToLower() == "true")
                {
                    storeResults = "";
                }
                else
                {
                    storeResults = failedMessage;
                }

            }

            return storeResults;
        }


        public Boolean saveGiftVoucherCode(Int64 checkoutId, String voucherCode)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_GiftVoucher_Redeem_Insert", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };


                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                cmd.Parameters.AddWithValue("@Code", voucherCode);


                conn.Open();
                cmd.ExecuteNonQuery();


            }
            return true;
        }


        public Boolean deleteGiftVoucherCode(Int64 checkoutId, Int32 giftVoucherId)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_GiftVoucher_Redeem_Delete", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };


                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                cmd.Parameters.AddWithValue("@GiftVoucherID", giftVoucherId);


                conn.Open();
                cmd.ExecuteNonQuery();


            }
            return true;
        }

        public Boolean addPaymentToken(Int64 checkoutId, string token, string bin, string lastFourDigits, string type)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_PaymentToken_Insert", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                cmd.Parameters.AddWithValue("@Token", token);
                cmd.Parameters.AddWithValue("@Bin", bin);
                cmd.Parameters.AddWithValue("@LastFourDigits", lastFourDigits);
                cmd.Parameters.AddWithValue("@CardType", type);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return true;
        }
        public Boolean deletePaymentToken(Int64 checkoutId, string token)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_PaymentToken_Remove", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                cmd.Parameters.AddWithValue("@Token", token);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return true;
        }
    }


    public partial class databaseDataContext
    {

        [Function(Name = "pub.CheckoutType_Select")]
        [ResultTypeAttribute(typeof(CheckoutType))]
        public IMultipleResults CheckoutType_Select([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<DateTime> date,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Mobile", DbType = "Bit")] System.Nullable<bool> mobile,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<long> visitorId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "Int")] System.Nullable<int> memberId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAccountID", DbType = "Int")] System.Nullable<int> tradeAccountId)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), countryID, languageID, date, mobile, visitorId, memberId, tradeAccountId);
            return (IMultipleResults)result.ReturnValue;
        }        

        [Function(Name = "pub.Checkout_GiftVoucher_Sale_Generate")]
        [ResultTypeAttribute(typeof(checkoutGiftVoucherBasket))]
        public IMultipleResults getGiftVoucherData_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CheckoutID", DbType = "BigInt")] System.Nullable<Int64> checkoutId)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), checkoutId);
            return (IMultipleResults)result.ReturnValue;
        }


        [Function(Name = "pub.Checkout_GiftBox_Generate")]
        [ResultTypeAttribute(typeof(checkoutGiftGroupedBasket))]
        [ResultTypeAttribute(typeof(checkoutGiftBox))]
        [ResultTypeAttribute(typeof(checkoutGiftBox))]
        [ResultTypeAttribute(typeof(checkoutGiftBoxPrice))]
        public IMultipleResults getGiftData_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CheckoutID", DbType = "BigInt")] System.Nullable<Int64> checkoutId,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), checkoutId, countryID, languageID);
            return (IMultipleResults)result.ReturnValue;
        }


        //[Function(Name = "pub.Checkout_Summary_Generate")]
        //[ResultTypeAttribute(typeof(checkoutSummaryTotal))]
        //[ResultTypeAttribute(typeof(checkoutSummaryDeliveryAddress))]
        //[ResultTypeAttribute(typeof(checkoutSummaryGroupedBasket))]
        //[ResultTypeAttribute(typeof(checkoutSummaryGiftBox))]
        //[ResultTypeAttribute(typeof(checkoutSummaryDeliveryOption))]
        //[ResultTypeAttribute(typeof(checkoutSummaryDeliverySelection))]
        //[ResultTypeAttribute(typeof(checkoutSummaryStockStatusLine))]
        //[ResultTypeAttribute(typeof(checkoutSummaryStockStatus))]
        //[ResultTypeAttribute(typeof(checkoutSummaryPartShipPrice))]
        //[ResultTypeAttribute(typeof(checkoutSummaryCustomerServiceDetails))]
        //public IMultipleResults getSummaryData_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CheckoutID", DbType = "BigInt")] System.Nullable<Int64> checkoutId,
        //                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
        //                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID)
        //{

        //    IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), checkoutId, countryID, languageID);
        //    return (IMultipleResults)result.ReturnValue;

        //}


        [Function(Name = "pub.Checkout_Pay_Generate")]
        [ResultType(typeof(checkoutPaymentTotal))]
        [ResultType(typeof(checkoutDetails))]
        [ResultType(typeof(CheckoutParameters))]
        [ResultType(typeof(MarketingPermission))]
        public IMultipleResults getPaymentTotals_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CheckoutID", DbType = "BigInt")] System.Nullable<Int64> checkoutId,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Origin", DbType = "NVarChar(256)")] string origin)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), checkoutId, countryID, languageID, origin);
            return (IMultipleResults)result.ReturnValue;
        }


        //[Function(Name = "pub.Checkout_DeliveryDate_Estimate")]
        //[ResultTypeAttribute(typeof(checkoutMinimum))]
        //[ResultTypeAttribute(typeof(checkoutMaximum))]
        //public IMultipleResults EstimatedDeliveryDate_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CheckoutID", DbType = "BigInt")] System.Nullable<Int64> checkoutId,
        //                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "DeliveryBandID", DbType = "SmallInt")] System.Nullable<short> DeliveryBandId,
        //                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "DispatchID", DbType = "TinyInt")] System.Nullable<byte> DispatchId,
        //                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "PartShip", DbType = "Bit")] System.Nullable<Boolean> PartShip)
        //{

        //    IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), checkoutId, DeliveryBandId, DispatchId, PartShip);
        //    return (IMultipleResults)result.ReturnValue;

        //}


        public checkoutDeliveryEstimate checkoutEstimateDeliveryDates(long checkoutId, short? deliveryBandId, byte dispatchId, bool partShip)
        {
            var result = new checkoutDeliveryEstimate();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_DeliveryDate_Estimate", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                cmd.Parameters.AddWithValue("@DeliveryBandID", deliveryBandId);
                cmd.Parameters.AddWithValue("@DispatchID", dispatchId);
                cmd.Parameters.AddWithValue("@PartShip", partShip);

                SqlParameter minReturn = cmd.Parameters.Add("@MinDeliveryEstimate", SqlDbType.Date);
                minReturn.Direction = ParameterDirection.Output;

                SqlParameter maxReturn = cmd.Parameters.Add("@MaxDeliveryEstimate", SqlDbType.Date);
                maxReturn.Direction = ParameterDirection.Output;

                conn.Open();
                
                cmd.ExecuteNonQuery();

                result.minDeliveryEstimate = Convert.ToDateTime(minReturn.Value);

                result.maxDeliveryEstimate = Convert.ToDateTime(maxReturn.Value);

                conn.Close();
            }

            return result;
        }



        [Function(Name = "pub.Checkout_Dispatch_Generate")]
        [ResultTypeAttribute(typeof(checkoutDeliveryAddress))]
        [ResultTypeAttribute(typeof(checkoutDispatchGroupedBasket))]
        [ResultTypeAttribute(typeof(checkoutDispatchDeliveryOption))]
        [ResultTypeAttribute(typeof(checkoutDispatchDeliverySelection))]
        [ResultTypeAttribute(typeof(checkoutDispatchStockStatusLine))]
        //[ResultTypeAttribute(typeof(checkoutDispatchStockStatus))]
        [ResultTypeAttribute(typeof(checkoutDispatchPartShipPrice))]
        [ResultTypeAttribute(typeof(checkoutDispatchNotice))]
        [ResultTypeAttribute(typeof(checkoutDispatchAppleDeliveryOption))]
        public IMultipleResults DispatchOptions_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CheckoutID", DbType = "BigInt")] System.Nullable<Int64> checkoutId)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), checkoutId);
            return (IMultipleResults)result.ReturnValue;

        }


        [Function(Name = "pub.Checkout_Address_Generate")]
        [ResultTypeAttribute(typeof(checkoutDeliveryItem))]
        [ResultTypeAttribute(typeof(checkoutDeliveryCost))]
        [ResultTypeAttribute(typeof(checkoutCreateSnapshotBasket))]
        public IMultipleResults CheckoutAddress_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CheckoutID", DbType = "BigInt")] System.Nullable<Int64> checkoutId,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorId,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "Int")] System.Nullable<Int32> memberId,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAccountID", DbType = "Int")] System.Nullable<Int32> tradeAccountID)
        {
             
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), checkoutId, countryID, languageID, visitorId, memberId, tradeAccountID);
            return (IMultipleResults)result.ReturnValue;

        }


        [Function(Name = "pub.Checkout_Receipt_Generate")]
        [ResultTypeAttribute(typeof(bool))]
        [ResultTypeAttribute(typeof(checkoutReceiptOrder))]
        [ResultTypeAttribute(typeof(checkoutReceiptDetail))]
        [ResultTypeAttribute(typeof(checkoutReceiptRecommendation))]
        [ResultTypeAttribute(typeof(checkoutReceiptTotalExVat))]
        [ResultTypeAttribute(typeof(checkoutReceiptTransactionDetail))]
        [ResultTypeAttribute(typeof(checkoutReceiptOrderLine))]
        [ResultTypeAttribute(typeof(brontoJSONBasketLine))]
        public IMultipleResults GenerateReceipt_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "GUID", DbType = "UNIQUEIDENTIFIER")] System.Nullable<Guid> Guid,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorId,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAccountID", DbType = "Int")] System.Nullable<int> memberTradeAccountID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsTrade", DbType = "Bit")] bool isTrade,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "DisplayTradePrice", DbType = "Bit")] bool displayTradePrice)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), Guid, visitorId, countryID, languageID, date, memberTradeAccountID, isTrade, displayTradePrice);
            return (IMultipleResults)result.ReturnValue;

        }


        [Function(Name = "pub.Checkout_GiftVoucher_Redeem_Select")]
        [ResultTypeAttribute(typeof(giftVoucherDetail))]
        public IMultipleResults getGiftVoucherDetails_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CheckoutID", DbType = "BigInt")] System.Nullable<Int64> checkoutId)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), checkoutId);
            return (IMultipleResults)result.ReturnValue;

        }

        [Function(Name = "pub.Checkout_MerchantInfo_Select")]
        public ISingleResult<CheckoutParameters> Checkout_MerchantInfo_Select([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CheckoutID", DbType = "BigInt")] System.Nullable<Int64> checkoutId)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), checkoutId);
            return ((ISingleResult<CheckoutParameters>)result.ReturnValue);
        }

        [Function(Name = "pub.Checkout_PaymentToken_Select")]
        [ResultTypeAttribute(typeof(List<checkoutPaymentToken>))]
        public IMultipleResults Checkout_PaymentToken_Select([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CheckoutID", DbType = "BigInt")] System.Nullable<Int64> checkoutId)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), checkoutId);
            return (IMultipleResults)result.ReturnValue;
        }   

        

        [Function(Name = "pub.Checkout_Google_Product_Select")]
        [ResultTypeAttribute(typeof(List<gtagProduct>))]
        [ResultTypeAttribute(typeof(List<promotionCode>))]
        public IMultipleResults Checkout_Products_Select([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CheckoutID", DbType = "BigInt")] System.Nullable<Int64> checkoutId)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), checkoutId);
            return (IMultipleResults)result.ReturnValue;
        }   

        public int checkout_ReceiptViewed(long checkoutId)
        {
            int result = -1;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("UPDATE ecm.Checkout SET ReceiptViewed = 1 WHERE CheckoutID = @CheckoutID", conn)
                {
                    CommandType = CommandType.Text
                };
                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);

                conn.Open();

                result = cmd.ExecuteNonQuery();

                conn.Close();
            }

            return result;
        }

        /*
         

        public checkoutProductSelectResult getCheckoutProducts(Int64 checkoutId)
        {
            var results = new List<gtagProduct>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.Checkout_CheckoutProduct_Select", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                conn.Open();
                
                SqlDataReader reader = cmd.ExecuteReader();

                while(reader.Read())
                {
                    results.Add(new gtagProduct()
                    {
                        Code = reader.GetString(0),
                        Name = reader.GetString(1),
                        Brand = reader.GetString(2),
                        Quantity = reader.GetInt32(3),
                        Price = reader.GetDecimal(4)
                    });
                }

                conn.Close();
            }

            return results;
        }
         */
    }
}