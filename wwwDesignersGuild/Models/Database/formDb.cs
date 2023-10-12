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
using wwwDesignersGuild.Models.Form;
using static DevExpress.Utils.Drawing.Helpers.NativeMethods;
using SimpleImpersonation;
using System.Security.Principal;

namespace wwwDesignersGuild.Models.Database
{
    public class formDb
    {

        // This is now defunct - Tom, 22/05/2018
        // see below PageSectionFormSave
        public Boolean saveForm(Int64 visitorDbId, short countryId, byte languageId,  String email, String fullName, String companyName,
                                    String addressLineOne, String addressLineTwo, String addressTown, String addressCounty, String addressPostCode,
                                    Int16? addressCountryId, String phoneNumber, Int16? competitionID, Int16? brochureId, String prefShop, String prefTime, DateTime? prefDate, Int64 pageSectionId, Boolean? thirdPartyPermission, Boolean? genericPermission,
                                    Int16? storeEventId, Int16? storeEventLocationId, DateTime? storeEventDate, Int32? storeEventDateID, Int64? storeEventDateTimeSlotID, bool marketingConsent)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("pub.Form_Save", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                    cmd.Parameters.AddWithValue("@CountryID", countryId);
                    cmd.Parameters.AddWithValue("@LanguageID", languageId);
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Password", null);
                    cmd.Parameters.AddWithValue("@CompanyName", companyName);
                    cmd.Parameters.AddWithValue("@AddressLine1", addressLineOne);
                    cmd.Parameters.AddWithValue("@AddressLine2", addressLineTwo);
                    cmd.Parameters.AddWithValue("@Town", addressTown);
                    cmd.Parameters.AddWithValue("@County", addressCounty);
                    cmd.Parameters.AddWithValue("@Postcode", addressPostCode);
                    cmd.Parameters.AddWithValue("@AddressCountryID", addressCountryId);
                    cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                    cmd.Parameters.AddWithValue("@CompetitionID", competitionID);
                    cmd.Parameters.AddWithValue("@BrochureID", brochureId);
                    cmd.Parameters.AddWithValue("@PreferredShop", prefShop);
                    cmd.Parameters.AddWithValue("@PreferredTime", prefTime);
                    cmd.Parameters.AddWithValue("@PreferredDate", prefDate);
                    cmd.Parameters.AddWithValue("@PageSectionID", pageSectionId);
                    cmd.Parameters.AddWithValue("@GenericPermission", genericPermission);
                    cmd.Parameters.AddWithValue("@ThirdPartyPermission", thirdPartyPermission);
                    cmd.Parameters.AddWithValue("@StoreEventID", storeEventId);
                    cmd.Parameters.AddWithValue("@StoreEventLocationID", storeEventLocationId);
                    cmd.Parameters.AddWithValue("@StoreEvent_Date", storeEventDate);
                    cmd.Parameters.AddWithValue("@StoreEvent_DateID", storeEventDateID);
                    cmd.Parameters.AddWithValue("@StoreEvent_Date_TimeSlotID", storeEventDateTimeSlotID);
                    cmd.Parameters.AddWithValue("@MarketingOptIn", marketingConsent);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return true;
        }

        public long? SaveTradeAccountRequestForm(long visitorId, webapiTradeAccountRequestFormSave objFormDetails, byte[] usSalesTaxExemptionCertificate = null, string usSalesTaxExemptionCertificateFileType = null)
        {
            var credentials = new UserCredentials("DESIGNERSGUILD\\SQLAgentFileAccess", "34949C02-4677-4711-999C-8E038E26B10F");

            long? result = null;

            var token = credentials.LogonUser(LogonType.Interactive);

            WindowsIdentity.RunImpersonated(token, () =>
            {
                using (SqlConnection conn = new SqlConnection("Data Source=vm-sql1;Initial Catalog=Flock;Integrated Security=SSPI"))
                {
                    DataTable leadInterestsTableType = new DataTable();

                    leadInterestsTableType.Columns.Add("BrandID", typeof(byte));
                    leadInterestsTableType.Columns.Add("ProductTypeGroupID", typeof(byte));

                    if (objFormDetails.leadInterests != null)
                    {
                        foreach (var i in objFormDetails.leadInterests)
                        {
                            leadInterestsTableType.Rows.Add(i.BrandID, i.ProductTypeGroupID);
                        }
                    }

                    SqlCommand cmd = new SqlCommand("pub.TradeAccountRequestForm_Save", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    DateTime? tradingSinceDT = null;
                    DateTime adjust = DateTime.Now;
                    if (objFormDetails.tradingSinceId.HasValue)
                    {
                        switch (objFormDetails.tradingSinceId.Value)
                        {
                            case 0:
                                adjust = DateTime.Now.AddMonths(3);
                                tradingSinceDT = new DateTime(adjust.Year, adjust.Month, 1);
                                break;
                            case 1:
                                adjust = DateTime.Now.AddMonths(-6);
                                tradingSinceDT = new DateTime(adjust.Year, adjust.Month, 1);
                                break;
                            case 2:
                                adjust = DateTime.Now.AddYears(-1);
                                tradingSinceDT = new DateTime(adjust.Year, adjust.Month, 1);
                                break;
                            case 3:
                                adjust = DateTime.Now.AddYears(-2);
                                tradingSinceDT = new DateTime(adjust.Year, adjust.Month, 1);
                                break;
                            case 4:
                                adjust = DateTime.Now.AddYears(-3);
                                tradingSinceDT = new DateTime(adjust.Year, adjust.Month, 1);
                                break;
                            case 5:
                                adjust = DateTime.Now.AddYears(-4);
                                tradingSinceDT = new DateTime(adjust.Year, adjust.Month, 1);
                                break;
                            case 6:
                                adjust = DateTime.Now.AddYears(-5);
                                tradingSinceDT = new DateTime(adjust.Year, adjust.Month, 1);
                                break;
                        }
                    }

                    cmd.Parameters.AddWithValue("@LeadID", objFormDetails.leadId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@VisitorID", visitorId);
                    cmd.Parameters.AddWithValue("@SiteLanguageID", objFormDetails.objMaster.UrlBreakdown.countryDetails.LanguageID);
                    cmd.Parameters.AddWithValue("@SiteCountryID", objFormDetails.objMaster.UrlBreakdown.countryDetails.CountryID);
                    cmd.Parameters.AddWithValue("@CompanyName", objFormDetails.companyName);
                    cmd.Parameters.AddWithValue("@ChannelID", objFormDetails.channelId);
                    cmd.Parameters.AddWithValue("@CompanyNumber", objFormDetails.companyRegistration ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@VATNumber", objFormDetails.companyVATNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@VATNumberDisplay", objFormDetails.vatNumberDisplay ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@VATCountryID", objFormDetails.vatCountryId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TaxID", objFormDetails.taxId ?? (object)DBNull.Value);

                    if (usSalesTaxExemptionCertificate != null &&
                        usSalesTaxExemptionCertificate?.Length > 0)
                    {
                        var salesTaxExemptionCertificateParam = new SqlParameter("@USSalesTaxExemptionCertificate", SqlDbType.VarBinary, usSalesTaxExemptionCertificate?.Length ?? 0);
                        salesTaxExemptionCertificateParam.Value = usSalesTaxExemptionCertificate;

                        cmd.Parameters.Add(salesTaxExemptionCertificateParam);
                        cmd.Parameters.AddWithValue("@USSalesTaxExemptionCertificateFileType", usSalesTaxExemptionCertificateFileType);
                    }

                    cmd.Parameters.AddWithValue("@MertexLanguageCode", objFormDetails.autoCorrespondenceLanguageCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EORINumber", objFormDetails.companyEORINumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CompanyTypeID", objFormDetails.companyTypeId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TradingSince", tradingSinceDT ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CountryID", objFormDetails.invoiceAddress.countryId);
                    cmd.Parameters.AddWithValue("@Website", objFormDetails.website ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeadContactTypeID", objFormDetails.contactTypeId);
                    cmd.Parameters.AddWithValue("@TitleID", objFormDetails.titleId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FullName", objFormDetails.fullName);
                    cmd.Parameters.AddWithValue("@Phone", objFormDetails.phone);
                    cmd.Parameters.AddWithValue("@Email", objFormDetails.email);
                    cmd.Parameters.AddWithValue("@OrderConfirmationsEmail", objFormDetails.orderConfirmationsEmail ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@InvoiceStatementsEmail", objFormDetails.invoicesStatementsEmail ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PriceListsProductsNewsEventsEmail", objFormDetails.pricelistsProductsNewsEventsEmail ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DeliveryNotificationsEmail", objFormDetails.deliveryNotificationsEmail ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Notes", objFormDetails.notes ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@WillBuyPatternBooks", objFormDetails.patternBookInterest);
                    cmd.Parameters.AddWithValue("@DistributorDataSharingConsent", objFormDetails.distributorConsent);
                    cmd.Parameters.AddWithValue("@SellOnline", objFormDetails.sellOnline);
                    cmd.Parameters.AddWithValue("@OtherChannel", objFormDetails.businessTypeOther ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OtherLeadContactType", objFormDetails.positionOther ?? (object)DBNull.Value);

                    DataTable leadAddressTableType = new DataTable();

                    leadAddressTableType.Columns.Add("LeadAddressID", typeof(long));
                    leadAddressTableType.Columns.Add("Label", typeof(string));
                    leadAddressTableType.Columns.Add("AddressLine1", typeof(string));
                    leadAddressTableType.Columns.Add("AddressLine2", typeof(string));
                    leadAddressTableType.Columns.Add("Town", typeof(string));
                    leadAddressTableType.Columns.Add("County", typeof(string));
                    leadAddressTableType.Columns.Add("Postcode", typeof(string));
                    leadAddressTableType.Columns.Add("CountryID", typeof(short));
                    leadAddressTableType.Columns.Add("DefaultInvoiceAddress", typeof(bool));
                    leadAddressTableType.Columns.Add("DefaultDeliveryAddress", typeof(bool));
                    leadAddressTableType.Columns.Add("DefaultVisitAddress", typeof(bool));

                    leadAddressTableType.Rows.Add(
                        (object)DBNull.Value,
                        (object)DBNull.Value,
                        objFormDetails.invoiceAddress.addressLine1,
                        objFormDetails.invoiceAddress.addressLine2 ?? (object)DBNull.Value,
                        objFormDetails.invoiceAddress.town,
                        objFormDetails.invoiceAddress.county,
                        objFormDetails.invoiceAddress.postcode,
                        objFormDetails.invoiceAddress.countryId,
                        true,
                        objFormDetails.deliverySameAsInvoice,
                        objFormDetails.visitSameAsInvoice
                    );

                    if (!objFormDetails.deliverySameAsInvoice)
                    {
                        leadAddressTableType.Rows.Add(
                            (object)DBNull.Value,
                            (object)DBNull.Value,
                            objFormDetails.deliveryAddress.addressLine1,
                            objFormDetails.deliveryAddress.addressLine2 ?? (object)DBNull.Value,
                            objFormDetails.deliveryAddress.town,
                            objFormDetails.deliveryAddress.county,
                            objFormDetails.deliveryAddress.postcode,
                            objFormDetails.deliveryAddress.countryId,
                            false,
                            true,
                            false
                        );
                    }

                    if (!objFormDetails.visitSameAsInvoice)
                    {
                        leadAddressTableType.Rows.Add(
                            (object)DBNull.Value,
                            (object)DBNull.Value,
                            objFormDetails.visitAddress.addressLine1,
                            objFormDetails.visitAddress.addressLine2 ?? (object)DBNull.Value,
                            objFormDetails.visitAddress.town,
                            objFormDetails.visitAddress.county,
                            objFormDetails.visitAddress.postcode,
                            objFormDetails.visitAddress.countryId,
                            false,
                            false,
                            true
                        );
                    }

                    SqlParameter leadAddresses = cmd.Parameters.AddWithValue("@LeadAddresses", leadAddressTableType);
                    leadAddresses.SqlDbType = SqlDbType.Structured;
                    leadAddresses.TypeName = "portal.LeadAddressTableType";

                    SqlParameter leadInterests = cmd.Parameters.AddWithValue("@LeadInterests", leadInterestsTableType);
                    leadInterests.SqlDbType = SqlDbType.Structured;
                    leadInterests.TypeName = "BrandIDProductTypeGroupIDTableType";

#if DEBUG
                    System.Diagnostics.Debug.WriteLine(Util.SQLCommandDumper.GetCommandText(cmd));
#endif

                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        result = reader.IsDBNull(0) ? null : (long?)reader.GetInt64(0);
                    }

                    reader.Close();

                    conn.Close();
                }
            });

            return result;
        }


        public bool SaveImmediateOrderFlag(long leadId)
        {
            bool result = true;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("UPDATE portal.Lead SET ImmediateOrderRequested = 1 WHERE LeadID = @LeadID", conn)
                {
                    CommandType = CommandType.Text
                };

                cmd.Parameters.AddWithValue("@LeadID", leadId);

                conn.Open();

                cmd.ExecuteNonQuery();

                conn.Close();
            }

            return result;
        }
    }

    public partial class databaseDataContext
    {
        [Function(Name = "pub.TradeAccountRequestForm_Select")]
        [ResultType(typeof(Classes.Country))]
        [ResultType(typeof(CountyItem))]
        [ResultType(typeof(titleItem))]
        [ResultType(typeof(companyType))]
        [ResultType(typeof(byteSelectOption))]
        [ResultType(typeof(productTypeGroup_Brand_Country))]
        [ResultType(typeof(channel))]
        [ResultType(typeof(leadContactType))]
        public IMultipleResults TradeAccountRequestForm_Select([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                   [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                   [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), countryID, languageID, date);
            return (IMultipleResults)result.ReturnValue;
        }
        
        [Function(Name = "pub.EmailCapturePopup_Form_Select")] 
        [ResultType(typeof(cmsEmailCapturePopupForm))]
        public IMultipleResults EmailCapturePopup_Form_Select([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                   [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                   [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), countryID, languageID, date);
            return (IMultipleResults)result.ReturnValue;
        }
        
        public bool isImmediateOrderFlagSet(long leadId)
        {
            bool result = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("SELECT ImmediateOrderRequested FROM portal.Lead WHERE LeadID = @LeadID", conn)
                {
                    CommandType = CommandType.Text
                };

                cmd.Parameters.AddWithValue("@LeadID", leadId);
                
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result = reader.GetBoolean(0);
                }

                reader.Close();

                conn.Close();
            }

            return result;
        }

        // This is where it's at with the saving of the forms
        // Tom, 22/05/2018
        public bool PageSectionFormSave(pageSectionFormSave objSave)
        {
            var result = true;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();
                comm.CommandType = System.Data.CommandType.StoredProcedure;

                comm.CommandText = "pub.Form_Save";

                comm.Parameters.AddWithValue("@MemberID", objSave.MemberID ?? (object)DBNull.Value); // INT = NULL
                comm.Parameters.AddWithValue("@VisitorID", objSave.VisitorID); // BIGINT,
                comm.Parameters.AddWithValue("@Email", objSave.Form.Email);// NVARCHAR(256),
                comm.Parameters.AddWithValue("@FullName", objSave.Form.FullName ?? (object)DBNull.Value); // NVARCHAR(50) = NULL,
                comm.Parameters.AddWithValue("@Password", objSave.Form.Password ?? (object)DBNull.Value); // NVARCHAR(64) = NULL,
                comm.Parameters.AddWithValue("@CompanyName", objSave.Form.CompanyName ?? (object)DBNull.Value); // NVARCHAR(35) = NULL, --Optional - MemberAdrres_Insert
                comm.Parameters.AddWithValue("@AddressLine1", objSave.Form.AddressLine1 ?? (object)DBNull.Value); // NVARCHAR(60) = NULL, --Mandatory - MemberAdrres_Insert
                comm.Parameters.AddWithValue("@AddressLine2", objSave.Form.AddressLine2 ?? (object)DBNull.Value); // NVARCHAR(60) = NULL, --Optional - MemberAdrres_Insert
                comm.Parameters.AddWithValue("@Town", objSave.Form.Town ?? (object)DBNull.Value); // NVARCHAR(50) = NULL, --Mandatory - MemberAdrres_Insert
                comm.Parameters.AddWithValue("@County", objSave.Form.County ?? (object)DBNull.Value); // NVARCHAR(40) = NULL, --Optional - MemberAdrres_Insert
                comm.Parameters.AddWithValue("@Postcode", objSave.Form.Postcode ?? (object)DBNull.Value); // NVARCHAR(20) = NULL, --Mandatory - MemberAdrres_Insert
                comm.Parameters.AddWithValue("@MarketingCountryID", objSave.Form.MarketingCountryID ?? (object)DBNull.Value); // SMALLINT = NULL, 
                comm.Parameters.AddWithValue("@MarketingLanguageID", objSave.Form.MarketingLanguageID ?? (object)DBNull.Value); // TINYINT = NULL, 
                comm.Parameters.AddWithValue("@PhoneNumber", objSave.Form.PhoneNumber ?? (object)DBNull.Value); // NVARCHAR(20) = NULL, --Optional - MemberAdrres_Insert
                comm.Parameters.AddWithValue("@FormID", objSave.Form.FormID); // SMALLINT,
                comm.Parameters.AddWithValue("@CountryID", objSave.Form.CountryID); // SMALLINT,
                comm.Parameters.AddWithValue("@LanguageID", objSave.Form.LanguageID); // TINYINT,
                comm.Parameters.AddWithValue("@ConfirmEmail", objSave.Form.ConfirmEmail); // BIT,
                comm.Parameters.AddWithValue("@AddressRequired", objSave.Form.AddressRequired); // BIT,
                comm.Parameters.AddWithValue("@PasswordRequired", objSave.Form.PasswordRequired); // BIT,
                comm.Parameters.AddWithValue("@CaptchaRequired", objSave.Form.CaptchaRequired); // BIT,
                comm.Parameters.AddWithValue("@SourceID", objSave.Form.SourceID); // TINYINT,
                comm.Parameters.AddWithValue("@SubmitButtonText", objSave.Form.SubmitButtonText); // NVARCHAR(1000),
                comm.Parameters.AddWithValue("@PageID", objSave.Form.ContentID); // INT,
                comm.Parameters.AddWithValue("@HeaderText", objSave.Form.HeaderText ?? (object)DBNull.Value); // NVARCHAR(1000) = NULL,
                comm.Parameters.AddWithValue("@FooterText", objSave.Form.FooterText ?? (object)DBNull.Value); // NVARCHAR(1000) = NULL,
                comm.Parameters.AddWithValue("@PageSectionID", objSave.Form.PageSectionID ?? (object)DBNull.Value); //  BIGINT = NULL,
                comm.Parameters.AddWithValue("@ThirdPartyID", objSave.Form.ThirdPartyID ?? (object)DBNull.Value); //  SMALLINT = NULL,
                comm.Parameters.AddWithValue("@CompetitionID", objSave.Form.CompetitionID ?? (object)DBNull.Value); //  SMALLINT = NULL,
                comm.Parameters.AddWithValue("@BrochureID", objSave.Form.BrochureID ?? (object)DBNull.Value); //  SMALLINT = NULL,
                comm.Parameters.AddWithValue("@PreferredShop", objSave.Form.PreferredShop ?? (object)DBNull.Value); //  NVARCHAR(100) = NULL,
                comm.Parameters.AddWithValue("@PreferredTime", objSave.Form.PreferredTime ?? (object)DBNull.Value); //  NVARCHAR(100) = NULL,
                comm.Parameters.AddWithValue("@PreferredDate", objSave.Form.PreferredDate ?? (object)DBNull.Value); //  DATE = NULL,
                comm.Parameters.AddWithValue("@TransactionID", objSave.Form.TransactionID ?? (object)DBNull.Value); //  INT = NULL,
                comm.Parameters.AddWithValue("@StoreEventID", objSave.Form.StoreEventID ?? (object)DBNull.Value); //  SMALLINT = NULL,
                comm.Parameters.AddWithValue("@StoreEventLocationID", objSave.Form.StoreEventLocationID ?? (object)DBNull.Value); //  SMALLINT = NULL,
                comm.Parameters.AddWithValue("@StoreEvent_Date", objSave.Form.StoreEvent_Date ?? (object)DBNull.Value); //  DATETIME2(7) = NULL,
                comm.Parameters.AddWithValue("@StoreEvent_DateID", objSave.Form.StoreEvent_DateID ?? (object)DBNull.Value); //  INT = NULL,
                comm.Parameters.AddWithValue("@StoreEvent_Date_TimeSlotID", objSave.Form.StoreEvent_Date_TimeSlotID ?? (object)DBNull.Value); //  BIGINT = NULL,
                comm.Parameters.AddWithValue("@RetailAppointmentAgendaID", objSave.Form.AppointmentAgendaID ?? (object)DBNull.Value); //  TINYINT = NULL,
                comm.Parameters.AddWithValue("@RetailAppointmentTypeID", objSave.Form.AppointmentTypeID ?? (object)DBNull.Value); //  TINYINT = NULL,
                comm.Parameters.AddWithValue("@RetailAppointmentComments", objSave.Form.AppointmentComments ?? (object)DBNull.Value); //  TINYINT = NULL,


                var marketingPermission = comm.CreateParameter();

                marketingPermission.ParameterName = "@MarketingPermission";
                marketingPermission.SqlDbType = System.Data.SqlDbType.Structured;
                marketingPermission.TypeName = "dbo.MarketingPermissionTableType";

                DataTable mp = new DataTable();

                mp.Columns.Add("MarketingPermissionID", typeof(byte));
                mp.Columns.Add("Value", typeof(bool));
                mp.Columns.Add("Label", typeof(string));
                mp.Columns.Add("Default", typeof(bool));
                mp.Columns.Add("Hidden", typeof(bool));

                if (objSave.Form.MarketingPermissions != null)
                {
                    objSave.Form.MarketingPermissions.ForEach(c => {
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

                comm.Parameters.Add(marketingPermission); //  MarketingPermissionTableType READONLY

                conn.Open();

                var reader = comm.ExecuteReader();

                while (reader.Read())
                {

                }

                conn.Close();
            }

            return result;
        }

        // This is where it's at with the saving of the forms
        // Tom, 22/05/2018
        public bool PageSectionFormSave(PassFormSave objSave, long visitorId, int? memberId)
        {
            var result = true;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();
                comm.CommandType = System.Data.CommandType.StoredProcedure;

                comm.CommandText = "pub.Form_Save";

                comm.Parameters.AddWithValue("@MemberID", memberId ?? (object)DBNull.Value); // INT = NULL
                comm.Parameters.AddWithValue("@VisitorID", visitorId); // BIGINT,
                comm.Parameters.AddWithValue("@Email", objSave.Email);// NVARCHAR(256),
                comm.Parameters.AddWithValue("@FullName", objSave.FullName ?? (object)DBNull.Value); // NVARCHAR(50) = NULL,
                comm.Parameters.AddWithValue("@Password", objSave.Password ?? (object)DBNull.Value); // NVARCHAR(64) = NULL,
                comm.Parameters.AddWithValue("@CompanyName", objSave.CompanyName ?? (object)DBNull.Value); // NVARCHAR(35) = NULL, --Optional - MemberAdrres_Insert
                comm.Parameters.AddWithValue("@AddressLine1", objSave.AddressLine1 ?? (object)DBNull.Value); // NVARCHAR(60) = NULL, --Mandatory - MemberAdrres_Insert
                comm.Parameters.AddWithValue("@AddressLine2", objSave.AddressLine2 ?? (object)DBNull.Value); // NVARCHAR(60) = NULL, --Optional - MemberAdrres_Insert
                comm.Parameters.AddWithValue("@Town", objSave.Town ?? (object)DBNull.Value); // NVARCHAR(50) = NULL, --Mandatory - MemberAdrres_Insert
                comm.Parameters.AddWithValue("@County", objSave.County ?? (object)DBNull.Value); // NVARCHAR(40) = NULL, --Optional - MemberAdrres_Insert
                comm.Parameters.AddWithValue("@Postcode", objSave.Postcode ?? (object)DBNull.Value); // NVARCHAR(20) = NULL, --Mandatory - MemberAdrres_Insert
                comm.Parameters.AddWithValue("@MarketingCountryID", objSave.MarketingCountryID ?? (object)DBNull.Value); // SMALLINT = NULL, 
                comm.Parameters.AddWithValue("@MarketingLanguageID", objSave.MarketingLanguageID ?? (object)DBNull.Value); // TINYINT = NULL, 
                comm.Parameters.AddWithValue("@PhoneNumber", objSave.PhoneNumber ?? (object)DBNull.Value); // NVARCHAR(20) = NULL, --Optional - MemberAdrres_Insert
                comm.Parameters.AddWithValue("@FormID", objSave.FormID); // SMALLINT,
                comm.Parameters.AddWithValue("@CountryID", objSave.CountryID); // SMALLINT,
                comm.Parameters.AddWithValue("@LanguageID", objSave.LanguageID); // TINYINT,
                comm.Parameters.AddWithValue("@ConfirmEmail", objSave.ConfirmEmail); // BIT,
                comm.Parameters.AddWithValue("@AddressRequired", objSave.AddressRequired); // BIT,
                comm.Parameters.AddWithValue("@PasswordRequired", objSave.PasswordRequired); // BIT,
                comm.Parameters.AddWithValue("@CaptchaRequired", objSave.CaptchaRequired); // BIT,
                comm.Parameters.AddWithValue("@SourceID", objSave.SourceID); // TINYINT,
                comm.Parameters.AddWithValue("@SubmitButtonText", objSave.SubmitButtonText); // NVARCHAR(1000),
                comm.Parameters.AddWithValue("@PageID", objSave.ContentID); // INT,
                comm.Parameters.AddWithValue("@HeaderText", objSave.HeaderText ?? (object)DBNull.Value); // NVARCHAR(1000) = NULL,
                comm.Parameters.AddWithValue("@FooterText", objSave.FooterText ?? (object)DBNull.Value); // NVARCHAR(1000) = NULL,
                comm.Parameters.AddWithValue("@PageSectionID", objSave.PageSectionID ?? (object)DBNull.Value); //  BIGINT = NULL,
                comm.Parameters.AddWithValue("@ThirdPartyID", objSave.ThirdPartyID ?? (object)DBNull.Value); //  SMALLINT = NULL,
                comm.Parameters.AddWithValue("@CompetitionID", objSave.CompetitionID ?? (object)DBNull.Value); //  SMALLINT = NULL,
                comm.Parameters.AddWithValue("@BrochureID", objSave.BrochureID ?? (object)DBNull.Value); //  SMALLINT = NULL,
                comm.Parameters.AddWithValue("@PreferredShop", objSave.AppointmentLocation ?? (object)DBNull.Value); //  NVARCHAR(100) = NULL,
                comm.Parameters.AddWithValue("@PreferredTime", objSave.AppointmentTime ?? (object)DBNull.Value); //  NVARCHAR(100) = NULL,
                comm.Parameters.AddWithValue("@PreferredDate", objSave.AppointmentDate ?? (object)DBNull.Value); //  DATE = NULL,
                comm.Parameters.AddWithValue("@TransactionID", objSave.TransactionID ?? (object)DBNull.Value); //  INT = NULL,
                comm.Parameters.AddWithValue("@StoreEventID", objSave.StoreEventID ?? (object)DBNull.Value); //  SMALLINT = NULL,
                comm.Parameters.AddWithValue("@StoreEventLocationID", objSave.StoreEventLocationID ?? (object)DBNull.Value); //  SMALLINT = NULL,
                comm.Parameters.AddWithValue("@StoreEvent_Date", objSave.StoreEventDate ?? (object)DBNull.Value); //  DATETIME2(7) = NULL,
                comm.Parameters.AddWithValue("@StoreEvent_DateID", objSave.StoreEventDateID ?? (object)DBNull.Value); //  INT = NULL,
                comm.Parameters.AddWithValue("@StoreEvent_Date_TimeSlotID", objSave.StoreEventTimeSlot ?? (object)DBNull.Value); //  BIGINT = NULL,
                comm.Parameters.AddWithValue("@RetailAppointmentAgendaID", objSave.AppointmentAgendaID ?? (object)DBNull.Value); //  TINYINT = NULL,
                comm.Parameters.AddWithValue("@RetailAppointmentTypeID", objSave.AppointmentTypeID ?? (object)DBNull.Value); //  TINYINT = NULL,
                comm.Parameters.AddWithValue("@RetailAppointmentComments", objSave.AppointmentComments ?? (object)DBNull.Value); //  TINYINT = NULL,


                var marketingPermission = comm.CreateParameter();

                marketingPermission.ParameterName = "@MarketingPermission";
                marketingPermission.SqlDbType = System.Data.SqlDbType.Structured;
                marketingPermission.TypeName = "dbo.MarketingPermissionTableType";

                DataTable mp = new DataTable();

                mp.Columns.Add("MarketingPermissionID", typeof(byte));
                mp.Columns.Add("Value", typeof(bool));
                mp.Columns.Add("Label", typeof(string));
                mp.Columns.Add("Default", typeof(bool));
                mp.Columns.Add("Hidden", typeof(bool));

                if (objSave.MarketingPermissions != null)
                {
                    objSave.MarketingPermissions.ForEach(c => {
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

                comm.Parameters.Add(marketingPermission); //  MarketingPermissionTableType READONLY

                conn.Open();

                var reader = comm.ExecuteReader();

                while (reader.Read())
                {
                    
                }

                conn.Close();
            }

            return result;
        }

        public bool MyAccountSave(webapiMyAccountSave objSave)
        {
            var result = true;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();
                comm.CommandType = System.Data.CommandType.StoredProcedure;

                comm.CommandText = "pub.Member_MyAccount_Save";

                comm.Parameters.AddWithValue("@CountryID", objSave.CountryID);
                comm.Parameters.AddWithValue("@LanguageID", objSave.LanguageID);
                comm.Parameters.AddWithValue("@IPAddress", objSave.IPAddress ?? "localhost");
                comm.Parameters.AddWithValue("@MemberID", objSave.MemberID);
                comm.Parameters.AddWithValue("@FullName", objSave.FullName);
                comm.Parameters.AddWithValue("@Email", objSave.Email);
                comm.Parameters.AddWithValue("@MarketingCountryID", objSave.MarketingCountryID);
                comm.Parameters.AddWithValue("@MarketingLanguageID", objSave.MarketingLanguageID);
                
                var marketingPermission = comm.CreateParameter();

                marketingPermission.ParameterName = "@MarketingPermission";
                marketingPermission.SqlDbType = System.Data.SqlDbType.Structured;
                marketingPermission.TypeName = "dbo.MarketingPermissionTableType";

                DataTable mp = new DataTable();

                mp.Columns.Add("MarketingPermissionID", typeof(byte));
                mp.Columns.Add("Value", typeof(bool));
                mp.Columns.Add("Label", typeof(string));
                mp.Columns.Add("Default", typeof(bool));
                mp.Columns.Add("Hidden", typeof(bool));

                if (objSave.MarketingPermissions != null)
                {
                    objSave.MarketingPermissions.ForEach(c => {
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

                comm.Parameters.Add(marketingPermission); //  MarketingPermissionTableType READONLY

                conn.Open();

                var reader = comm.ExecuteReader();

                while (reader.Read())
                {

                }

                conn.Close();
            }

            return result;
        }
    }
}