using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Trade;
using wwwDesignersGuild.Models.Visitor;

using MertexerV2.Accounts;
using MertexerV2.Models.Accounts;

using MertexerV2SQL.Accounts;
using MertexerV2SQL.Models.Accounts;
using System.Net.Http.Headers;
using System.Xml.Linq;
using MertexerV2.Utilities;
using wwwDesignersGuild.Models.Trade.Accounting;
using wwwDesignersGuild.ViewModels.Trade;
using DevExpress.Web;

namespace wwwDesignersGuild.Controllers
{
    public class TradeAPIController : ApiController
    {
        [HttpPost]
        public async Task<IHttpActionResult> tradeLoginDetailsSaveFullname([FromBody]PassNameSave objSave)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            if (!String.IsNullOrEmpty(objSave.Name))
            {
                objSave.Name = objSave.Name.Trim();

                if ((objSave.Name.Length < 2) || (objSave.Name.Length > 40))
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.EmailOutsideRange);
                }

                var vf = new VisitorFactory();

                var success = await vf.updateMemberName(loginCheck.visitorId.Value, objSave.Name);

                if (!success)
                {
                    return Ok(dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed));
                }
            }
            else
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed));
            }

            return Ok(new { });
        }

        [HttpPost]
        public async Task<IHttpActionResult> tradeLoginDetailsSaveEmail([FromBody]PassEmailSave objSave)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            if (!String.IsNullOrEmpty(objSave.Email))
            {
                objSave.Email = objSave.Email.Trim();

                if ((objSave.Email.Length < 5) || (objSave.Email.Length > 256))
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.EmailOutsideRange);
                }

                var vf = new VisitorFactory();

                var success = await vf.updateMemberEmail(loginCheck.visitorId.Value, objSave.Email);

                if (!success)
                {
                    return Ok(dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed));
                }
            }
            else
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed));
            }

            return Ok(new { });
        }

        [HttpPost]
        public async Task<IHttpActionResult> tradeLoginDetailsSavePassword([FromBody]PassPasswordSave objSave)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            if (!String.IsNullOrEmpty(objSave.NewPassword))
            {
                objSave.CurrentPassword = objSave.CurrentPassword.Trim();
                objSave.NewPassword = objSave.NewPassword.Trim();

                if ((objSave.NewPassword.Length < 6) || (objSave.NewPassword.Length > 50))
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.PasswordOutsideRange);
                }

                var vf = new VisitorFactory();

                var currentPasswordEnc = await vf.generatePassword(objSave.CurrentPassword);
                var newPasswordEnc = await vf.generatePassword(objSave.NewPassword);

                var success = await vf.updateMemberPassword(loginCheck.visitorId.Value, currentPasswordEnc, newPasswordEnc);

                if (!success)
                {
                    return Ok(dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed));
                }
            }
            else
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed));
            }

            return Ok(new { });
        }

        [HttpPost]
        public async Task<IHttpActionResult> setTradePricingDisplay([FromBody]PassTradePricingSave objSave)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            var vf = new VisitorFactory();

            await vf.updateMemberDisplayTradePrice(loginCheck.memberId.Value, objSave.DisplayTradePrice);

            HttpContext.Current.Session["memberDisplayTradePrice"] = objSave.DisplayTradePrice;

            return Ok(new { });
        }

        [HttpPost]
        public async Task<IHttpActionResult> userManagementSave([FromBody]PassTradeUserManagementSave objSave)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            var vf = new VisitorFactory();

            await vf.callMemberTradeAccountUpdate(objSave, loginCheck.tradeAccountId.Value, loginCheck.memberId.Value);

            var tradeAccountModel = await vf.callTradeMemberSelect(
                    loginCheck.tradeAccountId.Value,
                    loginCheck.memberId.Value,
                    objSave.CountryID,
                    objSave.LanguageID);

            return Ok(new { tradeAccountModel });
        }

        [HttpPost]
        public async Task<IHttpActionResult> deleteTradeUser(PassTradeUserManagementSave objUser)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            var vf = new VisitorFactory();

            await vf.callTradeMemberDelete(objUser.MemberID.Value, loginCheck.memberId.Value);

            return Ok(new { });
        }

        [HttpPost]
        public async Task<IHttpActionResult> resendTradeAuthenticationInvite([FromBody]PassTradeUserManagementSave objUser)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            var vf = new VisitorFactory();

            int daysRemaining = await vf.callTradeMemberResendInvite(objUser, loginCheck.memberId.Value, loginCheck.tradeAccountId.Value);

            return Ok(new { daysRemaining });
        }

        [HttpPost]
        public async Task<IHttpActionResult> resendTradeAdminAuthenticationInvite([FromBody] PassTradeUserManagementSave objUser)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            var vf = new VisitorFactory();

            int daysRemaining = await vf.callTradeAdminResendInvite(objUser, loginCheck.memberId.Value, loginCheck.tradeAccountId.Value);

            return Ok(new { daysRemaining });
        }

        [HttpPost]
        public IHttpActionResult RefreshAccountBalance()
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            var result = AccountFunctions.AccountSummary(loginCheck.tradeDataSourceJson);

            return Ok(result);
        }

        [HttpPost]
        public IHttpActionResult AccountBalanceDrilldown([FromBody]Trade_Enquiry_InvoiceSearch_Request dto)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            dto.TradeAccountID = loginCheck.tradeAccountId.Value;

			var db = new databaseDataContext();

			var results = db.Trade_Enquiry_AccountBalance_Drilldown(dto);

			return Ok(results);
        }

        [HttpPost]
        public IHttpActionResult RefreshOutstandingInvoices([FromBody]Trade_Enquiry_InvoiceSearch_Request dto)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            dto.TradeAccountID = loginCheck.tradeAccountId.Value;

            return Ok(new databaseDataContext().Trade_Enquiry_InvoiceSearch(dto));
        }

        [HttpPost]
        public IHttpActionResult RefreshRecentPayments([FromBody]Trade_Enquiry_RecentPayments_Request dto)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

			dto.TradeAccountID = loginCheck.tradeAccountId.Value;

			return Ok(new databaseDataContext().Trade_Enquiry_RecentPayments(dto));
        }

        [HttpPost]
        public IHttpActionResult ProformaInvoiceSearch([FromBody]PassProformaInvoiceSearch objSearch)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            if (objSearch.searchTerm == null)
                objSearch.searchTerm = "";

            return Ok(AccountFunctions.ProformaInvoices(loginCheck.tradeDataSourceJson, objSearch));
        }

        [HttpPost]
        public IHttpActionResult InvoiceSearch([FromBody]Trade_Enquiry_InvoiceSearch_DetailedRequest dto)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            dto.TradeAccountID = loginCheck.tradeAccountId.Value;

			DateTime? fromDt = null;
			DateTime? toDt = null;

			switch (dto.DateRange)
			{
				case 12:
					fromDt = DateTime.Now.AddYears(-1);
					break;
				case 6:
					fromDt = DateTime.Now.AddMonths(-6);
					break;
				case 1:
					fromDt = DateTime.Now.AddDays(-DateTime.Now.Day).AddDays(1);// DateTime.Now.AddMonths(-1);
					break;
				case -1:
					fromDt = dto.DateRangeStart ?? DateTime.Now.AddMonths(-1);
					toDt = dto.DateRangeEnd ?? DateTime.Now;
					break;
			}

            dto.DateRangeStart = fromDt;
            dto.DateRangeEnd = toDt;

			var results = new databaseDataContext().Trade_Enquiry_InvoiceSearch(dto);

            return Ok(results);

            //return Ok(AccountFunctions.InvoiceSearch(loginCheck.tradeDataSourceJson, objSearch));
        }

        [HttpPost]
        public async Task<IHttpActionResult> UploadOrderFile()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var provider = new MultipartMemoryStreamProvider();

            await Request.Content.ReadAsMultipartAsync(provider);

            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            var visitorFactory = new VisitorFactory();

            var tradeAccountModel = await visitorFactory.callTradeMemberSelect(
                    loginCheck.tradeAccountId.Value,
                    loginCheck.memberId.Value,
                    Convert.ToInt16(HttpContext.Current.Request.Form.Get("countryId")),
                    Convert.ToByte(HttpContext.Current.Request.Form.Get("languageId")));

            var tradeAccountType = tradeAccountModel.AccountDetails.TradeAccountTypeID == 21 ? "DISTRIBUTOR" : "SORDI";

            var admin = tradeAccountModel.Administrators.Where(p => p.MemberID == loginCheck.memberId).SingleOrDefault();
            var user = tradeAccountModel.Users.Where(p => p.MemberID == loginCheck.memberId).SingleOrDefault();

            var email = admin?.Email ?? user?.Email;

            foreach (var file in provider.Contents)
            {
                var fn = file.Headers.ContentDisposition.FileName.Trim('\"');

                var fileName = System.IO.Path.GetFileNameWithoutExtension(fn);
                var buffer = await file.ReadAsByteArrayAsync();

                var tradeAccountNumber = tradeAccountModel.AccountDetails.Code;

                var contents = await file.ReadAsStringAsync();// File.ReadAllText(file);

                var now = DateTime.Now;

                using (var conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
                {
                    var comm = new System.Data.SqlClient.SqlCommand(@"INSERT INTO [portal].[TradeOrderImport_File]
                                                                                (ImportedByMemberID
                                                                                ,FileName
                                                                                ,TradeAccountNumber
                                                                                ,Email
                                                                                ,Contents
                                                                                ,UploadedDT
                                                                                ,CreatedDT)
                                                                    VALUES (
                                                                        @MemberID,
                                                                        @FileName,
                                                                        @TradeAccountNumber,
                                                                        @Email,
                                                                        @Contents,
                                                                        @UploadedDT,
                                                                        @CreatedDT
                                                                    )", conn);

                    if (!contents.Contains('\r'))
                    {
                        contents = System.Text.RegularExpressions.Regex.Replace(contents, "\n", "\r\n");
                    }

                    comm.Parameters.AddWithValue("@MemberID", loginCheck.memberId);
                    comm.Parameters.AddWithValue("@FileName", fileName);
                    comm.Parameters.AddWithValue("@TradeAccountNumber", tradeAccountNumber);
                    comm.Parameters.AddWithValue("@Email", email);
                    comm.Parameters.AddWithValue("@Contents", contents);
                    comm.Parameters.AddWithValue("@UploadedDT", now);
                    comm.Parameters.AddWithValue("@CreatedDT", now);

                    conn.Open();

                    var result = comm.ExecuteNonQuery();

                    conn.Close();
                }

                System.IO.File.WriteAllBytes($"\\\\{wwwDesignersGuild.Util.Framework.WebServerHostname}\\d$\\wwwroot\\portal\\uploads\\orderimport\\{loginCheck.tradeAccountNumber}_{DateTime.Now:dd-MM-yyyy}_{tradeAccountType}_{fileName}_{Guid.NewGuid()}.csv", buffer);
            }

            return Ok(new { });
        }

        [HttpPost]
        public async Task<IHttpActionResult> tradeContactDetailsSaveOrderConfirmation([FromBody]PassEmailSave objSave)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            if (!String.IsNullOrEmpty(objSave.Email))
            {
                objSave.Email = objSave.Email.Trim();

                if ((objSave.Email.Length < 5) || (objSave.Email.Length > 254))
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.EmailOutsideRange);
                }

                var success = AccountFunctionsUniSQL.UpdateOrderConfirmations(loginCheck.tradeDataSourceJson, objSave.Email);

                if (!success)
                {
                    return Ok(dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed));
                }
            }
            else
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed));
            }

            return Ok(new { });
        }

        [HttpPost]
        public async Task<IHttpActionResult> tradeContactDetailsSaveInvoicesStatements([FromBody]PassEmailSave objSave)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            if (!String.IsNullOrEmpty(objSave.Email))
            {
                objSave.Email = objSave.Email.Trim();

                if ((objSave.Email.Length < 5) || (objSave.Email.Length > 254))
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.EmailOutsideRange);
                }

                var success = AccountFunctionsUniSQL.UpdateInvoicesStatements(loginCheck.tradeDataSourceJson, objSave.Email);

                if (!success)
                {
                    return Ok(dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed));
                }
            }
            else
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed));
            }

            return Ok(new { });
        }

        [HttpPost]
        public async Task<IHttpActionResult> tradeContactDetailsSaveProductNewsEvents([FromBody]PassEmailSave objSave)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            if (!String.IsNullOrEmpty(objSave.Email))
            {
                objSave.Email = objSave.Email.Trim();

                if ((objSave.Email.Length < 5) || (objSave.Email.Length > 254))
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.EmailOutsideRange);
                }

                var success = AccountFunctionsUniSQL.UpdateProductNewsEvents(loginCheck.tradeDataSourceJson, objSave.Email);

                if (!success)
                {
                    return Ok(dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed));
                }
            }
            else
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed));
            }

            return Ok(new { });
        }

        [HttpPost]
        public async Task<IHttpActionResult> tradeContactDetailsSavePricelists([FromBody]PassEmailSave objSave)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            if (!String.IsNullOrEmpty(objSave.Email))
            {
                objSave.Email = objSave.Email.Trim();

                if ((objSave.Email.Length < 5) || (objSave.Email.Length > 254))
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.EmailOutsideRange);
                }

                var success = AccountFunctionsUniSQL.UpdatePricelists(loginCheck.tradeDataSourceJson, objSave.Email);

                if (!success)
                {
                    return Ok(dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed));
                }
            }
            else
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed));
            }

            return Ok(new { });
        }

        [HttpGet]
        public DataSourceResult GetCountries([ModelBinder(typeof(WebApiDataSourceRequestModelBinder))]DataSourceRequest request)
        {
            var db = new databaseDataContext();

            var results = db.GetCountries().Where(p => p.IsCountry == true && p.DeletedDT == null).OrderBy(p => p.Name).ToList();

            return results.AsQueryable().ToDataSourceResult(request);
        }

        [HttpPost]
        public IHttpActionResult saveDeliveryAddress(PassBasketAddressCleanse objAddress)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, false);

            if (loginCheck.loginStatus != dgLoginStatus.currentLogin ||
                loginCheck.tradeAccountId == null ||
                loginCheck.tradeDataSourceJson == null)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutTradeInvalidPermission));
            }

            var dataSource = JsonConvert.DeserializeObject<MertexerV2.Models.DataSource.TradeAccountDataSourceInfo>(loginCheck.tradeDataSourceJson)
                .EnquiryTypes
                .Where(p => p.EnquiryType == MertexerV2.Models.DataSource.EnquiryType.Address)
                .Single()
                .DataSources
                .Single();

            var tradeAccountNumber = dataSource.CustomerReference;

            objAddress.tradeAccountCode = tradeAccountNumber;

            var db = new databaseDataContext();

            if (String.IsNullOrWhiteSpace(objAddress.deliveryCompanyName) && !String.IsNullOrWhiteSpace(objAddress.deliveryName))
            {
                objAddress.deliveryCompanyName = objAddress.deliveryName;
            }

            if (!String.IsNullOrWhiteSpace(objAddress.deliveryCompanyName) && String.IsNullOrWhiteSpace(objAddress.deliveryName))
            {
                objAddress.deliveryName = objAddress.deliveryCompanyName;
            }

            objAddress.tradeAccountCode = objAddress.tradeAccountCode?.ToUpperInvariant();
            objAddress.endCustomerCode = objAddress.endCustomerCode?.ToUpperInvariant();
            objAddress.endCustomerName = objAddress.endCustomerName?.ToUpperInvariant();
            objAddress.deliveryCompanyName = objAddress.deliveryCompanyName?.ToUpperInvariant();
            objAddress.deliveryName = objAddress.deliveryName?.ToUpperInvariant();
            objAddress.deliveryAddressLine1 = objAddress.deliveryAddressLine1?.ToUpperInvariant();
            objAddress.deliveryAddressLine2 = objAddress.deliveryAddressLine2?.ToUpperInvariant();
            objAddress.deliveryTown = objAddress.deliveryTown?.ToUpperInvariant();
            objAddress.deliveryCounty = objAddress.deliveryCounty?.ToUpperInvariant();
            objAddress.deliveryPostcode = objAddress.deliveryPostcode?.ToUpperInvariant();
            objAddress.deliveryCountryCode = objAddress.deliveryCountryCode?.ToUpperInvariant();
            objAddress.email = objAddress.email?.ToUpperInvariant();
            objAddress.deliveryPhoneNumber = objAddress.deliveryPhoneNumber?.ToUpperInvariant();

            var address = new MertexerV2.Models.DeliveryAddress(
                    tradeAccountCode: objAddress.endCustomerCode ?? objAddress.tradeAccountCode, // we might be dealing with an end customer, if so we want to write to THEIR address book.
                    addressLine1: objAddress.deliveryAddressLine1,
                    addressLine2: objAddress.deliveryAddressLine2,
                    town: objAddress.deliveryTown,
                    county: objAddress.deliveryCounty,
                    postcode: objAddress.deliveryPostcode,
                    countryCode: objAddress.deliveryCountryCode,
                    contactName: objAddress.deliveryName,
                    companyName: objAddress.deliveryCompanyName,
                    phoneNumber: objAddress.deliveryPhoneNumber,
                    email: objAddress.email,
                    deliveryReference: objAddress.deliveryReference,
                    domesticOrTrade: null,
                    haulierCode: null,
                    carriageCode: null,
                    usStateCode: null,
                    usCountyCode: null,
                    usTaxExemptionCertificateReference: null);

            if (String.IsNullOrEmpty(objAddress.deliveryReference))
            {
                // does this address already exist in Mertex if it does say "This address already exists with Delivery referreererence: X", use that instead?

                if (!objAddress.skipDedupe)
                {
                    var extantDelref = AccountFunctionsUniSQL.AddressExists(loginCheck.tradeDataSourceJson, address);

                    if (!String.IsNullOrEmpty(extantDelref))
                    {
                        objAddress.deliveryReference = extantDelref;

                        return Ok(new { objAddress.deliveryReference });
                    }
                }

                objAddress.deliveryReference = MertexerV2.AddressBookFunctions.AddDeliveryAddress(dataSource.UniVerseAccount, address);

                var delRef = System.Text.RegularExpressions.Regex.Replace(objAddress.deliveryReference?.Replace($"{objAddress.endCustomerCode ?? objAddress.tradeAccountCode}-", "") ?? "", "[^0-9\\-]*", "");

                if (delRef.Length >= 3)
                {
                    delRef = System.Text.RegularExpressions.Regex.Replace(delRef.Split(new char[] { '-' })[0], "[^0-9]*", "");
                }

                if (String.IsNullOrWhiteSpace(delRef))
                {
                    delRef = "0";
                }

                var cleanseList = db.tradeAccountAddressCleanseList(objAddress.endCustomerCode ?? objAddress.tradeAccountCode);
                var cleanse = cleanseList.Where(p => p.DeliveryReference == objAddress.deliveryReference).SingleOrDefault();

                if (cleanse == null)
                {
                    cleanse = new TradeAccountAddressCleanse()
                    {
                        TradeAccountCode = objAddress.endCustomerCode ?? objAddress.tradeAccountCode,
                        DeliveryReference = objAddress.deliveryReference,
                        SourceAddressHash = objAddress.md5Hash,
                        SourceAddress = JsonConvert.SerializeObject(new
                        {
                            Type = objAddress.manual ? "Manual" : "Loqate",
                            DeliveryReferenceID = delRef,
                            DeliveryReference = objAddress.deliveryReference,
                            TradeAccountCode = objAddress.endCustomerCode ?? objAddress.tradeAccountCode,
                            Label = objAddress.label,
                            Name = objAddress.deliveryName,
                            Company = objAddress.deliveryCompanyName,
                            Line1 = objAddress.deliveryAddressLine1,
                            Line2 = objAddress.deliveryAddressLine2,
                            City = objAddress.deliveryTown,
                            Province = objAddress.deliveryCounty,
                            PostalCode = objAddress.deliveryPostcode,
                            Email = objAddress.email,
                            Phone = objAddress.deliveryPhoneNumber,
                            Country = objAddress.deliveryCountryCode,
                            MD5Hash = objAddress.md5Hash
                        }),
                        Status = null,
                        CreatedDT = DateTime.Now,
                        LookupDT = null,
                        DeletedDT = null
                    };

                    db.insertTradeAccountAddressClease(cleanse);
                }

                if (cleanse.SourceAddressHash != objAddress.md5Hash)
                {
                    cleanse.SourceAddressHash = objAddress.md5Hash;

                    db.updateTradeAccountAddressCleanseHash(cleanse);
                }
            }
            else
            {
                var savedDelRef = MertexerV2.AddressBookFunctions.UpdateDeliveryAddress(dataSource.UniVerseAccount, address);
            }

            return Ok(new { objAddress.deliveryReference });
        }

        [HttpPost]
        public IHttpActionResult SetDefaultDeliveryReference([FromBody]PassDeliveryReference objRef)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, false);

            if (loginCheck.loginStatus != dgLoginStatus.currentLogin ||
                loginCheck.tradeAccountId == null ||
                loginCheck.tradeDataSourceJson == null)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutTradeInvalidPermission));
            }

            AccountFunctionsUniSQL.SetDefaultDeliveryReference(loginCheck.tradeDataSourceJson, objRef.deliveryReference);

            return Ok(new { });
        }

        [HttpPost]
        public IHttpActionResult DeleteDeliveryReference([FromBody]PassDeliveryReference objRef)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, false);

            if (loginCheck.loginStatus != dgLoginStatus.currentLogin ||
                loginCheck.tradeAccountId == null ||
                loginCheck.tradeDataSourceJson == null)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutTradeInvalidPermission));
            }

            using (var conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                var comm = new System.Data.SqlClient.SqlCommand(@"UPDATE 
                                                                    portal.TradeAccountAddressCleanse
                                                                SET 
                                                                    DeletedDT = GETDATE() 
                                                                WHERE 
                                                                    TradeAccountCode = @TradeAccountNumber AND
                                                                    DeliveryReference = @DeliveryReference", conn);

                comm.Parameters.AddWithValue("@TradeAccountNumber", loginCheck.tradeAccountNumber);
                comm.Parameters.AddWithValue("@DeliveryReference", objRef.deliveryReference);

                conn.Open();

                var result = comm.ExecuteNonQuery();

                conn.Close();
            }

            return Ok(new { });
        }

        [HttpPost]
        public IHttpActionResult outstandingOrderSearch([FromBody]PassOutstandingOrdersSearch objSearch)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, false);

            if (loginCheck.loginStatus != dgLoginStatus.currentLogin ||
                loginCheck.tradeAccountId == null ||
                loginCheck.tradeDataSourceJson == null)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutTradeInvalidPermission));
            }

            var results = AccountFunctions.OutstandingOrders(loginCheck.tradeDataSourceJson, objSearch);

            return Ok(results.Results);
        }

        [HttpPost]
        public IHttpActionResult reserveOrderSearch([FromBody]PassOutstandingOrdersSearch objSearch)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, false);

            if (loginCheck.loginStatus != dgLoginStatus.currentLogin ||
                loginCheck.tradeAccountId == null ||
                loginCheck.tradeDataSourceJson == null)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutTradeInvalidPermission));
            }

            var results = AccountFunctions.ReserveOrders(loginCheck.tradeDataSourceJson, objSearch);

            return Ok(results.Results);
        }

        [HttpPost]
        public IHttpActionResult GetParcelCount([FromBody]PassParcelCount objParcel)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, false);

            if (loginCheck.loginStatus != dgLoginStatus.currentLogin ||
                loginCheck.tradeAccountId == null ||
                loginCheck.tradeDataSourceJson == null)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutTradeInvalidPermission));
            }

            var results = AccountFunctions.CheckParcelCount(loginCheck.tradeDataSourceJson, objParcel);

            return Ok(results);
        }

        [HttpPost]
        public IHttpActionResult GetOrderDeliveryInfo([FromBody]PassOrderNumber objOrderNumber)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, false);

            if (loginCheck.loginStatus != dgLoginStatus.currentLogin ||
                loginCheck.tradeAccountId == null ||
                loginCheck.tradeDataSourceJson == null)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutTradeInvalidPermission));
            }

            var results = AccountFunctions.OrderDeliveryInfo(loginCheck.tradeDataSourceJson, objOrderNumber);

            return Ok(results);
        }

        [HttpPost]
        public async Task<IHttpActionResult> saveInvoiceAddressChangeRequest([FromBody]PassInvoiceAddressChangeRequest objRequest)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, false);

            if (loginCheck.loginStatus != dgLoginStatus.currentLogin ||
                loginCheck.tradeAccountId == null ||
                loginCheck.tradeDataSourceJson == null)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutTradeInvalidPermission));
            }

            var vf = new VisitorFactory();

            await vf.tradeAccountInvoiceAddressRequestChange(loginCheck, objRequest);

            return Ok(new { });
        }

        [HttpPost]
        public async Task<IHttpActionResult> quickSearch([FromBody]PassQuickSearch objSearch)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, false);

            if (loginCheck.loginStatus != dgLoginStatus.currentLogin ||
                loginCheck.tradeAccountId == null ||
                loginCheck.tradeDataSourceJson == null)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutTradeInvalidPermission));
            }

            var vf = new VisitorFactory();

            var tradeAccountModel = await vf.callTradeMemberSelect(
                    loginCheck.tradeAccountId.Value,
                    loginCheck.memberId.Value,
                    objSearch.countryId,
                    objSearch.languageId);

            var user = tradeAccountModel.Users.Where(p => p.MemberID == loginCheck.memberId.Value).SingleOrDefault();
            var admin = tradeAccountModel.Administrators.Where(p => p.MemberID == loginCheck.memberId.Value).SingleOrDefault();

            DateTime useDate = DateTime.Now;
            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                var match = Request.Headers.Where(kv => kv.Key.ToLowerInvariant() == "dateoverride").Select(p => p.Value.FirstOrDefault()).FirstOrDefault();

                if (string.IsNullOrEmpty(match)) { }
                else
                {
                    if (match.Length == "ddMMyyyy".Length)
                    {
                        bool dateConvertSuccess = DateTime.TryParseExact(match,
                                                                        "ddMMyyyy",
                                                                        CultureInfo.InvariantCulture,
                                                                        DateTimeStyles.None,
                                                                        out useDate);
                    }
                }
            }

            var results = vf.tradeQuickSearch(objSearch, loginCheck.tradeAccountId, loginCheck.memberId, Convert.ToBoolean(HttpContext.Current.Session["memberDisplayTradePrice"]), useDate);

            return Ok(results.Results);
        }

        [HttpPost]
        public IHttpActionResult FetchProductPagePrices([FromBody]PassProductPageIDs objProductPages)
        {
            objProductPages.productPageIds = objProductPages.productPageIds.Distinct().ToList();

            Int64 visitorId = -1;
            if (HttpContext.Current.Session["visitorIdFromDatabase"] == null)
            {
            }
            else
            {
                visitorId = Convert.ToInt64(HttpContext.Current.Session["visitorIdFromDatabase"].ToString());
            }

            Int32? memberId = null;
            if (HttpContext.Current.Session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(HttpContext.Current.Session["memberIdFromDatabase"].ToString());
            }


            var db = new Models.Database.databaseDataContext();

            var result = db.ProductPage_Price_Select(
                objProductPages,
                visitorId,
                memberId,
                (int?)HttpContext.Current.Session["memberTradeAccountID"],
                HttpContext.Current.Session["memberDisplayTradePrice"] != null && Convert.ToBoolean(HttpContext.Current.Session["memberDisplayTradePrice"]));

            return Ok(result.json);
        }

        public IHttpActionResult FetchReserveDeliveryDetail([FromBody]PassReserveFetch objReserve)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, false);

            if (loginCheck.loginStatus != dgLoginStatus.currentLogin ||
                loginCheck.tradeAccountId == null ||
                loginCheck.tradeDataSourceJson == null)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutTradeInvalidPermission));
            }

            var tradeAccountNumber = JsonConvert.DeserializeObject<MertexerV2.Models.DataSource.TradeAccountDataSourceInfo>(loginCheck.tradeDataSourceJson)
                .EnquiryTypes
                .Where(p => p.EnquiryType == MertexerV2.Models.DataSource.EnquiryType.Address)
                .Select(p => p.DataSources.First().CustomerReference)
                .Single();

            objReserve.tradeAccountCode = tradeAccountNumber;

            var result = AccountFunctions.FetchReserveDetail(loginCheck.tradeDataSourceJson, objReserve);

            return Ok(result);
        }

        [HttpPost]
        public IHttpActionResult FetchReserveLines([FromBody]PassReserveFetch objReserve)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, false);

            if (loginCheck.loginStatus != dgLoginStatus.currentLogin ||
                loginCheck.tradeAccountId == null ||
                loginCheck.tradeDataSourceJson == null)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutTradeInvalidPermission));
            }

            var tradeAccountNumber = JsonConvert.DeserializeObject<MertexerV2.Models.DataSource.TradeAccountDataSourceInfo>(loginCheck.tradeDataSourceJson)
                .EnquiryTypes
                .Where(p => p.EnquiryType == MertexerV2.Models.DataSource.EnquiryType.Address)
                .Select(p => p.DataSources.First().CustomerReference)
                .Single();

            objReserve.tradeAccountCode = tradeAccountNumber;

            var results = AccountFunctions.FetchReserveLines(loginCheck.tradeDataSourceJson, objReserve);

            var db = new Models.Database.databaseDataContext();

            db.fetchImageIdsFromCodes(results);

            return Ok(results);
        }

        public IHttpActionResult ModifyReserveOrder([FromBody]PassReserveModify objReserveModify)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, false);

            if (loginCheck.loginStatus != dgLoginStatus.currentLogin ||
                loginCheck.tradeAccountId == null ||
                loginCheck.tradeDataSourceJson == null)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutTradeInvalidPermission));
            }

            if (String.IsNullOrEmpty(objReserveModify.deliveryReference))
            {
                objReserveModify.deliveryReference = ""; // if aiming for invoice address, OE -> PG.MODIFY_RESERVE_ORDER.WI reads:
                /*
                 CASE DELREF = "" AND DELADDR = ""
                      L.ORD.DELREF = "C"
                      L.ORD.DELADD = F("SLCUST",L.ORDER<1>)<1>
                */
            }

            var tradeAccountNumber = JsonConvert.DeserializeObject<MertexerV2.Models.DataSource.TradeAccountDataSourceInfo>(loginCheck.tradeDataSourceJson)
                .EnquiryTypes
                .Where(p => p.EnquiryType == MertexerV2.Models.DataSource.EnquiryType.CheckOrders)
                .Select(p => p.DataSources.First().CustomerReference)
                .Single();

            objReserveModify.tradeAccountCode = loginCheck.tradeAccountNumber;

            switch (objReserveModify.action)
            {
                case "delref":
                    return Ok(AccountFunctions.ModifyReserveDeliveryReference(loginCheck.tradeDataSourceJson, objReserveModify));
                case "release":
                    return Ok(AccountFunctions.ModifyReserveReleaseOrder(loginCheck.tradeDataSourceJson, objReserveModify));
            }

            return Ok(new { });
        }

        public IHttpActionResult UpdateReserveLine([FromBody]PassReserveLine objReserveLine)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, false);

            if (loginCheck.loginStatus != dgLoginStatus.currentLogin ||
                loginCheck.tradeAccountId == null ||
                loginCheck.tradeDataSourceJson == null)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutTradeInvalidPermission));
            }
            
            // riiiight.
            // so if... there's a requested quantity increase
            // or a requested date extension
            // we need to email customer services
            // via portal.Email

            // if there is a requested quantity increase, then originalLine *DO NOT CHANGE THE QUANTITY*

            if (objReserveLine.isLineQuantityIncrease || objReserveLine.isDateExtension)
            {
                var orderNo = "";
                var lineNo = "";

                var split = objReserveLine.lineKey.Split(new char[] { '*' });

                orderNo = split[0];
                lineNo = split[1];

                var body = $@"<p>Hello,</p><p>&nbsp;</p>
<p><strong>{loginCheck.tradeAccountNumber}</strong> has requested a change to their Reserve Order <strong>{orderNo}</strong></p>";

                if (objReserveLine.isLineQuantityIncrease)
                {
                    body += $@"<p>They would like the quantity on <strong>Line {lineNo}: {objReserveLine.code}</strong> increased to: <strong>{objReserveLine.requestedIncreasedQuantity}</strong></p>";
                }

                if (objReserveLine.isDateExtension)
                {
                    body += $@"<p>They would like the reservation on <strong>Line {lineNo}: {objReserveLine.code}</strong> extended until: <strong>{objReserveLine.requestedExtension?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}</strong></p>";
                }

                body += $"<p>&nbsp;</p><p>Please process this request as appropriate and inform the customer by emailing <a href='mailto:{HttpContext.Current.Session["memberEmail"]}'>{HttpContext.Current.Session["memberEmail"]}</a></p>";

                body += "<p>&nbsp;</p><p>Thank you</p>";

                Task.Run(() =>
                {
                    var db = new visitorDb();

                    var contactResult = db.fetchTradeDepartmentContact(loginCheck.tradeAccountId, objReserveLine.languageId, "CS");

                    var email = JObject.FromObject(contactResult.json).SelectToken("$.contactDetails[0].email").Value<string>();

                    string cc = null;

//#if DEBUG
//                    email = "tclark@designersguild.com";
//                    cc = "mchristie@designersguild.com";
//#endif

                    db.addEmailToQueue(email,
                        cc,
                        "itsupport@designersguild.com",
                        $"Reserve Order {orderNo} Change Request",
                        body);
                });
            }

            // This Modify call can be made regardless of isLineQuantityIncrease or isDateExtension: both of those avoid actually modifying the underlying data.
            // Indeed we must call this in all eventualities, as the user may have modified the line reference / memo.
            return Ok(AccountFunctions.ModifyReserveLine(loginCheck.tradeDataSourceJson, objReserveLine));
        }

        [HttpPost]
        public IHttpActionResult ResendBatchCutting([FromBody]PassReserveLine objReserveLine)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, false);

            if (loginCheck.loginStatus != dgLoginStatus.currentLogin ||
                loginCheck.tradeAccountId == null ||
                loginCheck.tradeDataSourceJson == null)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutTradeInvalidPermission));
            }

            Task.Run(() =>
            {
                var orderNo = "";
                var lineNo = "";

                var split = objReserveLine.lineKey.Split(new char[] { '*' });

                orderNo = split[0];
                lineNo = split[1];

                var body = $@"<p>Hello,</p><p>&nbsp;</p>
<p><strong>{loginCheck.tradeAccountNumber}</strong> requests the resending of a batch cutting from their Reserve Order <strong>{orderNo}</strong></p>";

                body += $@"<p>They would like a replacement cutting from <strong>Line {lineNo}: {objReserveLine.code}</strong> batch: <strong>{objReserveLine.batch}</strong></p>";

                body += "<p>&nbsp;</p><p>Thank you</p>";

                var db = new visitorDb();

                var contactResult = db.fetchTradeDepartmentContact(loginCheck.tradeAccountId, objReserveLine.languageId, "CS");

                var email = JObject.FromObject(contactResult.json).SelectToken("$.contactDetails[0].email").Value<string>();

                db.addEmailToQueue(email,
                    null,
                    "itsupport@designersguild.com",
                    $"Reserve Order {orderNo} Resend Batch Cutting Request",
                    body);
            });

            return Ok(new { });
        }

        [HttpPost]
        public HttpResponseMessage downloadClearanceSpreadsheet([FromBody]PassProductPageIDs objProductPageIds)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session);

            if (loginCheck == null)
            {
                return null;
            }

            if (loginCheck.tradeAccountId == null || loginCheck.tradeDataSourceJson == null)
            {
                return null;
            }

            var db = new Models.Database.visitorDb();

            var cl = db.getTradeAccountCountryLanguage(loginCheck.tradeAccountId.Value);

            var data = new Models.Database.promotionDb().DownloadClearanceList(loginCheck.tradeAccountId.Value, cl.CountryID, cl.LanguageID, objProductPageIds?.productPageIds, xlsx: true);

            var unique = Guid.NewGuid().ToString();

            var dirPath = System.Web.Hosting.HostingEnvironment.MapPath("~/ExcelSpreadsheets/");

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            var filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/ExcelSpreadsheets/" + unique + ".xlsx");

            var products = data.Document.Element("result").Element("Products").Elements("Product").ToList();

            var stocks = data.Document.Element("result").Element("Stocks")?.Elements("Stock").ToList() ?? new List<XElement>();

            Util.SpreadsheetHelper.WriteClearanceSpreadsheet(filePath, products, stocks);

            var f = System.IO.File.OpenRead(filePath);
            _ = $"DesignersGuild-Clearance-{DateTime.Now:dd-MMM-yyyy}.xlsx";

            var resultMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(f)
            };

            resultMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            return resultMessage;
        }

        [HttpPost]
        public HttpResponseMessage downloadClearancePdf([FromBody]PassProductPageIDs objProductPageIds)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session);

            if (loginCheck == null)
            {
                return null;
            }

            if (loginCheck.tradeAccountId == null || loginCheck.tradeDataSourceJson == null)
            {
                return null;
            }

            _ = new VisitorFactory();

            var db = new Models.Database.visitorDb();

            var cl = db.getTradeAccountCountryLanguage(loginCheck.tradeAccountId.Value);

            var data = new Models.Database.promotionDb().DownloadClearanceList(loginCheck.tradeAccountId.Value, cl.CountryID, cl.LanguageID, objProductPageIds?.productPageIds);

            Util.ClearancePDFWriter.buildPDF(System.Web.Hosting.HostingEnvironment.MapPath("~"), data, HttpContext.Current, new System.Web.Mvc.UrlHelper(HttpContext.Current.Request.RequestContext), HttpContext.Current.Response);

            return null;
        }

        [HttpPost]
        public IHttpActionResult changeReserveOrderCountry([FromBody]PassReserveOrderCountryChange objRequest)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, false);

            if (loginCheck.loginStatus != dgLoginStatus.currentLogin ||
                loginCheck.tradeAccountId == null ||
                loginCheck.tradeDataSourceJson == null)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutTradeInvalidPermission));
            }

            var body = $@"<p>Hello,</p><p>&nbsp;</p>
<p><strong>{loginCheck.tradeAccountNumber}</strong> has requested a change to their Reserve Order <strong>{objRequest.orderNumber}</strong></p>";

            body += $"<p><strong>This is a request for a change of address, which includes a change from the original destination country.</strong></p>";

            body += $"<p>They would like the destination address for this order updated to:</p>";

            body += $@"<p>
{objRequest.desiredDestination.deliveryName}<br/>
{objRequest.desiredDestination.deliveryAddressLine1}<br/>
{objRequest.desiredDestination.deliveryAddressLine2}<br/>
{objRequest.desiredDestination.deliveryTown}<br/>
{objRequest.desiredDestination.deliveryCounty}<br/>
{objRequest.desiredDestination.deliveryPostcode}<br/>
{objRequest.desiredDestination.deliveryCountryCode}</p>";

            body += $"<p><strong>This change may have serious tax and duty implications. Please consider this request carefully.</strong></p>";

            body += $"<p>&nbsp;</p><p>Once a decision has been made, please inform the customer by emailing <a href='mailto:{HttpContext.Current.Session["memberEmail"]}'>{HttpContext.Current.Session["memberEmail"]}</a></p>";

            body += "<p>&nbsp;</p><p>Thank you</p>";

            Task.Run(() =>
            {
                var db = new visitorDb();

                var contactResult = db.fetchTradeDepartmentContact(loginCheck.tradeAccountId, objRequest.languageId, "CS");

                var email = JObject.FromObject(contactResult.json).SelectToken("$.contactDetails[0].email").Value<string>();

#if DEBUG
                email = "tclark@designersguild.com";
#endif

                var emailId = db.addEmailToQueue(
                    email,
                    null,
                    "itsupport@designersguild.com",
                    $"Reserve Order {objRequest.orderNumber} Address Change Request",
                    body);
            });

            return Ok(new { });
        }

        public IHttpActionResult RequestTradeAccess([FromBody]PassTradeAccessRequest objRequest)
        {
            var db = new visitorDb();

            Int64 visitorId = -1;
            if (HttpContext.Current.Session["visitorIdFromDatabase"] == null)
            {
            }
            else
            {
                visitorId = Convert.ToInt64(HttpContext.Current.Session["visitorIdFromDatabase"].ToString());
            }
            var result = db.callTradeAccountRequestAccess(visitorId, objRequest);

            return Ok(result?.json);

            //var contactResult = db.fetchTradeAccountSalesRepresentative(objRequest);
            
            //            var body = $@"<p>Hello {JObject.FromObject(contactResult.json).SelectToken("$.contactDetails[0].contactName").Value<string>()},</p><p>&nbsp;</p>
            //<p><strong>{objRequest.tradeAccountLabel}</strong> has requested a login for our new & improved B2B Trade Site.</p>";

            //            body += $@"<p>The person requesting a B2B login on {DateTime.Now:ddd dd MMMM yyyy} was:</p>";

            //            body += $@"<p>&nbsp;</p>";

            //            body += $@"<p>{objRequest.userName}</p>";

            //            body += $@"<p><a href=""mailto:{objRequest.userEmail}"">{objRequest.userEmail}</a></p>";

            //            if (!String.IsNullOrEmpty(objRequest.userPhone))
            //            {
            //                body += $@"<p><a href=""mailto:{objRequest.userPhone}"">{objRequest.userPhone}</a></p>";
            //            }

            //            body += $@"<p>&nbsp;</p>";

            //            body += $"<p><strong>THIS PERSON MAY NOT BE THE ACCOUNT OWNER. WE CAN ONLY SEND A B2B TRADE REGISTRATION LINK TO THE MAIN EMAIL ADDRESS WE HOLD ON {objRequest.tradeAccountLabel}</strong></p>";

            //            body += $@"<p>Please use the <a href=""https://portal.designersguild.com/customers/invite"">""Send Trade Admin Invites"" Portal App</a> to view the registered email address we hold and to see if this account has been registered previously.</p>";

            //            body += @"<p>Once registered, the account holder is responsible for the creation and management of their own users.</p>";

            //            body += @"<p>If the email address above is not the registered account email address, please reply to them and ask that they get the owner of the account to create them as a user.</p>";

            //            body += "<p>&nbsp;</p><p>Thank you</p>";

            //            var email = JObject.FromObject(contactResult.json).SelectToken("$.contactDetails[0].email").Value<string>();

            //            string cc = null;

            //#if DEBUG
            //            email = "tclark@designersguild.com";
            //            cc = "mchristie@designersguild.com";
            //#endif

            //            var emailId = db.addEmailToQueue(
            //                email,
            //                cc,
            //                "itsupport@designersguild.com",
            //                $"Trade Admin Invite Requested - {objRequest.tradeAccountLabel}",
            //                body);

            //            return Ok(new { emailId });
        }

        [HttpPost]
        [Route("api/tradeapi/downloadClearanceSpreadsheet")]
        public IHttpActionResult downloadClearanceSpreadsheet([FromBody]PassTradeClearanceFetch objFetch)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            if (loginCheck == null)
            {
                return Redirect("/l102?redirect=0");
            }

            if (loginCheck.tradeAccountId == null || loginCheck.tradeDataSourceJson == null)
            {
                return Redirect("/l102?redirect=0");
            }

            var db = new visitorDb();

            var cl = db.getTradeAccountCountryLanguage(loginCheck.tradeAccountId.Value);

            System.Xml.Linq.XDocument data = null;

            if (objFetch.productPageIds.Any())
                data = new promotionDb().DownloadClearanceList(loginCheck.tradeAccountId.Value, cl.CountryID, cl.LanguageID, objFetch.productPageIds, xlsx: true);
            else
                data = new promotionDb().DownloadClearanceList(loginCheck.tradeAccountId.Value, cl.CountryID, cl.LanguageID, null, xlsx: true);

            var unique = Guid.NewGuid().ToString();

            var dirPath = System.Web.Hosting.HostingEnvironment.MapPath("~/ExcelSpreadsheets/");

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            var filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/ExcelSpreadsheets/" + unique + ".xlsx");

            var products = data.Document.Element("result").Element("Products").Elements("Product").ToList();

            var stocks = data.Document.Element("result").Element("Stocks")?.Elements("Stock").ToList() ?? new List<XElement>();
            
            Util.SpreadsheetHelper.WriteClearanceSpreadsheet(filePath, products, stocks);

            var bytes = File.ReadAllBytes(filePath);

            var result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(bytes);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
            result.Content.Headers.ContentDisposition.FileName = $"DesignersGuild-Clearance-{DateTime.Now:dd-MMM-yyyy}.xlsx";
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            return new System.Web.Http.Results.ResponseMessageResult(result);
        }

        [HttpPost]
        [Route("api/tradeapi/downloadClearancePDF")]
        public IHttpActionResult downloadClearancePDF([FromBody]PassTradeClearanceFetch objFetch)
        {
            var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, true);

            if (loginCheck == null)
            {
                return Redirect("/l102?redirect=0");
            }

            if (loginCheck.tradeAccountId == null || loginCheck.tradeDataSourceJson == null)
            {
                return Redirect("/l102?redirect=0");
            }

            var db = new visitorDb();

            var cl = db.getTradeAccountCountryLanguage(loginCheck.tradeAccountId.Value);

            System.Xml.Linq.XDocument data = null;

            if (objFetch.productPageIds.Any())
                data = new promotionDb().DownloadClearanceList(loginCheck.tradeAccountId.Value, cl.CountryID, cl.LanguageID, objFetch.productPageIds);
            else
                data = new promotionDb().DownloadClearanceList(loginCheck.tradeAccountId.Value, cl.CountryID, cl.LanguageID);
            
            var bytes = Util.ClearancePDFWriter.buildPDF(HttpContext.Current.Request.MapPath("~"), data, HttpContext.Current.Request.Url);

            var result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(bytes);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
            result.Content.Headers.ContentDisposition.FileName = $"DesignersGuild-Clearance-{DateTime.Now:dd-MMM-yyyy}.pdf";
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            return new System.Web.Http.Results.ResponseMessageResult(result);
        }

        [HttpPost]
        public async Task<IHttpActionResult> TradeLoginUnregistered([FromBody]PassTradeLoginUnregisteredRequest obj)
        {
            Int64 visitorId = -1;

            if (HttpContext.Current.Session["visitorIdFromDatabase"] != null) 
            { 
                visitorId = Convert.ToInt64(HttpContext.Current.Session["visitorIdFromDatabase"].ToString());
            }

            _ = await new databaseDataContext().TradeLogin_Unregistered(obj, visitorId);

            return Ok(new { });
        }
    }
}