using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

using Tweetinvi.Core.Extensions;

using wwwDesignersGuild.Models.BackInStockReminder;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Form;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Scrapbook;
using wwwDesignersGuild.Models.UPS;
using wwwDesignersGuild.Models.Visitor;
using wwwDesignersGuild.Util;

namespace wwwDesignersGuild.Controllers
{
    public class MemberController : ApiController
    {
        #region Return Data classes

        public class booleanResult
        {
            public Boolean success { get; set; }
        }

        public class backInStockReminderReturn
        {
            public Boolean mustLogin { get; set; }
            public Boolean success { get; set; }
        }

        public class validateEmailReturn
        {
            public Boolean validEmailFormat { get; set; }
            public Boolean emailExists { get; set; }
        }


        public class newAddressReturn
        {
            public MemberAddress memberAddressDetail { get; set; }
        }


        public class editAddressReturn
        {
            public MemberAddress memberAddressDetail { get; set; }
        }


        public class memberFullName
        {
            public String fullName { get; set; }
        }


        public class returnDetails
        {
            public string ReturnIdentifier { get; set; }
        }

        #endregion

        #region Controller routines

        [HttpPost]
        public async Task<IHttpActionResult> toggleScrapbook(webapiToggleScrapbook objToggle)
        {
            var useDate = DateTime.Now;
            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                //  Should the "use date" be overriden?
                if (HttpContext.Current.Request.QueryString["dateOverride"] == null) { }
                else
                {
                    String overrideDate = HttpContext.Current.Request.QueryString["dateOverride"].ToString();
                    if (overrideDate.Length == "ddMMyyyy".Length)
                    {
                        bool dateConvertSuccess = DateTime.TryParseExact(overrideDate,
                                                                              "ddMMyyyy",
                                                                              CultureInfo.InvariantCulture,
                                                                              DateTimeStyles.None,
                                                                              out useDate);
                        if (dateConvertSuccess == true) { }
                        else
                        {
                            useDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                        }
                    }
                }
            }

            var session = HttpContext.Current.Session;

            Int64 visitorDbId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }


            Int32? memberId = null;
            if (session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
            }

            var visitorFactory = new VisitorFactory();

            var cmsScrapbook = new cmsScrapbook();

            var scrapbook = await cmsScrapbook.loadScrapbookFromCmsAsync(
                    visitorDbId,
                    memberId,
                    objToggle.countryId,
                    objToggle.languageId,
                    useDate
                );

            var addition = !scrapbook.Images.Where(p => p.ImageID == objToggle.imageId).Any();

            if (addition)
            {
                _ = await visitorFactory.saveImageToScrapbook(visitorDbId, memberId, objToggle.imageId, objToggle.countryId, objToggle.productPageId);

                scrapbook.Images.Append(new ScrapbookImage() 
                { 
                    ImageID = objToggle.imageId
                });
            }
            else
            {
                _ = await visitorFactory.removeImageFromScrapbook(visitorDbId, memberId,
                    objToggle.countryId, objToggle.imageId);

                var list = scrapbook.Images.ToList();
                list.RemoveAll(p => p.ImageID == objToggle.imageId);
                scrapbook.Images = list.AsEnumerable();
            }

            cmsScrapbook.setScrapbook(visitorDbId, memberId, objToggle.countryId, objToggle.languageId, useDate, scrapbook);

            return Ok(new { addition });
        }

        [HttpPost]
        public async Task<object> asyncRemoveImageFromScrapbook(webapiAddImageToScrapbook imageToRemove)
        {
            var session = HttpContext.Current.Session;

            Int64 visitorDbId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            Int32? memberId = null;
            if (session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
            }

            VisitorFactory visitorFactory = new VisitorFactory();

            Boolean success = await visitorFactory.removeImageFromScrapbook(visitorDbId, memberId, imageToRemove.countryId, imageToRemove.imageId);

            return success;
        }


        [HttpPost]
        public validateEmailReturn validateEmail(webapiValidateEmail objValidate)
        {

            if (objValidate == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }


            String memberEmail = objValidate.emailAddress.Trim();


            validateEmailReturn objReturn = new validateEmailReturn
            {
                validEmailFormat = dgFramework.emailChecker(memberEmail),
                emailExists = false      //  Switch off validation at this point as too obtrusive for user
            };

            return objReturn;
        }


        [HttpPost]
        public async Task<IHttpActionResult> signUpNewMember(PassNewMember objRegistrant)
        {
            var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

            if (objRegistrant == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            if (ReCaptchaVerifier.Validate2022(objRegistrant.gRecaptcha) == "true")
            {
                objRegistrant.name = objRegistrant.name.Trim();
                objRegistrant.email = objRegistrant.email.Trim();
                objRegistrant.password = objRegistrant.password.Trim();

                //  Validate passed data
                if ((objRegistrant.name.Length < 2) || (objRegistrant.name.Length > 40))
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.NameOutsideRange);
                }

                if ((objRegistrant.email.Length < 5) || (objRegistrant.email.Length > 100))
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.EmailOutsideRange);
                }

                if ((objRegistrant.password.Length < 6) || (objRegistrant.password.Length >= 50))
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.PasswordOutsideRange);
                }

                if (!dgFramework.emailChecker(objRegistrant.email))
                {
                    return Ok(dgFramework.raiseErrorToHandle(webapiDataError.EmailInvalidFormat));
                }

                //  Does the email address already exist
                var visitorFactory = new VisitorFactory();

                var memberExists = await visitorFactory.callMemberCheckSelect(objRegistrant.email);

                if (memberExists != null)
                {
                    return Ok(dgFramework.raiseErrorToHandle(webapiDataError.EmailDuplicate));
                }

                var session = HttpContext.Current.Session;

                //  Save the new member
                var memberId = await visitorFactory.saveRegistration(objRegistrant);

                if (memberId != null)
                {
                    session["memberIdFromDatabase"] = memberId;
                }
                else
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.MemberNotInserted);
                }

                var visitorDbId = -1L;

                if (session["visitorIdFromDatabase"] == null)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
                }
                else
                {
                    visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
                }

                //  Automatical Login
                var memberData = await visitorFactory.memberSignIn(visitorDbId, objRegistrant.email, objRegistrant.password);

                if (memberData.memberLoginStatusData.Count == 0)
                {
                    return Ok(dgFramework.raiseErrorToHandle(webapiDataError.InvalidLogin));
                }
                if (memberData.memberLoginStatusData[0].LoginSuccessful == 0)
                {
                    return Ok(dgFramework.raiseErrorToHandle(webapiDataError.InvalidLogin));
                }

                session["memberIdFromDatabase"] = memberData.memberLoginStatusData[0].MemberID;
                session["memberName"] = objRegistrant.name;
                session["memberEmail"] = objRegistrant.email;
                session["loginStatus"] = dgLoginStatus.currentLogin.ToString();

                session["memberTradeAccountID"] = null;
                session["memberTradeAccountNumber"] = null;
                session["memberTradeDataSourceJson"] = null;
                session["memberTradeChecksPassed"] = false;
                session["memberDisplayTradePrice"] = false;

                if (session["backInStockReminder"] != null)
                {
                    await visitorFactory.createBackInStockReminder(visitorDbId, Convert.ToInt32(session["backInStockReminder"]), objRegistrant.countryId, objRegistrant.languageId);
                }

                return Ok(true);
            }
            else
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.InvalidCaptcha));
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> signIn(PassSignIn objSignIn)
        {
            if (objSignIn == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            if (objSignIn.email == null || objSignIn.password == null)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.InvalidLogin));
            }

            String memberEmail = objSignIn.email.Trim();
            String memberPassword = objSignIn.password.Trim();


            if ((memberEmail == "") || (memberPassword == ""))
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.InvalidLogin));
            }


            //  Get the session identifier
            var session = HttpContext.Current.Session;
            Int64 visitorDbId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            //  Determine the date to obtain the data from the cms
            var requestWrapper = new HttpRequestWrapper(HttpContext.Current.Request);

            var useDate = Framework.GetUseDate(requestWrapper);

            var visitorFactory = new VisitorFactory();

            var newPasswordHash = await visitorFactory.generatePassword(memberPassword);

            var memberData = await visitorFactory.memberSignIn(visitorDbId, memberEmail, newPasswordHash);

            if (memberData.memberLoginStatusData.Count == 0)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.InvalidLogin));
            }
            if (memberData.memberLoginStatusData[0].LoginSuccessful == 0)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.InvalidLogin));
            }

            var objDetailStore = await visitorFactory.memberDetails(visitorDbId, useDate);

            if (objDetailStore.MemberDetail == null)
            {
                session["memberName"] = "";
                session["memberEmail"] = "";
                session["memberIdFromDatabase"] = null;
                session["memberTradeAccountNumber"] = null;
                session["memberTradeDataSourceJson"] = null;
                session["memberTradeAccountID"] = null;
                session["memberDisplayTradePrice"] = false;
            }
            else
            {
                session["memberName"] = objDetailStore.MemberDetail.FullName;
                session["memberEmail"] = objDetailStore.MemberDetail.Email;
                session["memberIdFromDatabase"] = objDetailStore.MemberDetail.MemberID;
                session["memberTradeAccountNumber"] = objDetailStore.MemberDetail.MertexCustomerAccountNumber;
                session["memberTradeDataSourceJson"] = objDetailStore.MemberDetail.TradeDataSourceJson;
                session["memberTradeAccountID"] = objDetailStore.MemberDetail.TradeAccountID;
                session["memberDisplayTradePrice"] = objDetailStore.MemberDetail.DisplayTradePrice;

                var objCountry = new cmsCountry();
                var objCountryData = await objCountry.countryLanguageSelect(DateTime.Now);
                var thisCountry = objCountryData.countryItems.Where(q => q.CountryID == objSignIn.countryId).FirstOrDefault();
                
                var thisCountryTradePriceLists = objCountryData.countryTradePriceLists
                    .Where(p => objCountryData.countryItems.Where(q => q.CountryID == objSignIn.countryId)
                                                           .Select(q => q.PriceListID).Contains(p.PriceListID)
                    ).ToList();

                session["memberTradeChecksPassed"] = false;

                if (objSignIn.accountType == webapiSignInAccountType.Trade || objSignIn.accountType == webapiSignInAccountType.Either)
                {
                    if (thisCountryTradePriceLists.Count > 0)
                    {
                        if (objDetailStore.MemberDetail.TradeAuthenticationComplete)
                        {
                            var objVisitorFactory = new VisitorFactory();
                            
                            if (objDetailStore.MemberDetail.TradePriceListID != null)
                            {
                                if (thisCountryTradePriceLists.Select(p => p.TradePriceListID).ToList().Contains(objDetailStore.MemberDetail.TradePriceListID.Value))
                                {
                                    //var placeOrderPermission = await objVisitorFactory.tradePermissionCheck(objDetailStore.memberDetails[0].MemberID, objSignIn.countryId);

                                    if (objDetailStore.MemberDetail.TradeAccountID.HasValue)
                                    {
                                        session["memberTradeChecksPassed"] = true;

                                        var tradeSignInCookie = new HttpCookie("dg-trade-signin", thisCountry.URL);

                                        tradeSignInCookie.Expires = DateTime.Now.AddMonths(1);

                                        HttpContext.Current.Response.Cookies.Add(tradeSignInCookie);
                                    }

                                    session["loginStatus"] = dgLoginStatus.currentLogin.ToString();

                                    //isValidTradeAccount = placeOrderPermission;

                                    //var tradeAccountModel = await objVisitorFactory.callTradeMemberSelect(objDetailStore.memberDetails[0].TradeAccountID.Value, 
                                    //    objDetailStore.memberDetails[0].MemberID, 
                                    //    objDetailStore.memberDetails[0].CountryID,
                                    //    objDetailStore.memberDetails[0].LanguageID);

                                    //if (!isValidTradeAccount)
                                    //{
                                    //    return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutTradeInvalidPermission, null, tradeAccountModel));
                                    //}

                                    //return Ok(new { redirect = "l1743" });
                                    return Ok(new { redirect = "l0" });
                                }
                            }
                        }
                    }
                }
            }

            //if (objSignIn.accountType == webapiSignInAccountType.Trade && !isValidTradeAccount)
            //{
            //    return Ok(dgFramework.raiseErrorToHandle(webapiDataError.InvalidLogin));
            //}

            session["loginStatus"] = dgLoginStatus.currentLogin.ToString();

            //if (session["backInStockReminder"] == null) { }
            //else
            //{
            //    if (session["visitorIdFromDatabase"] == null) { }
            //    else
            //    {
            //        await visitorFactory.createBackInStockReminder(visitorDbId, Convert.ToInt32(session["backInStockReminder"]), objSignIn.countryId, objSignIn.languageId);
            //    }
            //}
            
            return Ok(true);
        }

        [HttpPost]
        public async Task<object> signout()
        {


            //  Get the session identifier
            var session = HttpContext.Current.Session;
            Int64 visitorDbId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            VisitorFactory visitorFactory = new VisitorFactory();
            _ = await visitorFactory.memberSignOut(visitorDbId);

            session["memberIdFromDatabase"] = null;
            session["loginStatus"] = dgLoginStatus.notLoggedIn.ToString();
            session["memberName"] = null;
            session["memberEmail"] = null;
            session["memberTradeAccountNumber"] = null;
            session["memberTradeAccountID"] = null;
            session["memberTradeChecksPassed"] = false;
            session["memberDisplayTradePrice"] = false;

            return "";

        }

        [HttpPost]
        public async Task<object> doesEmailExist(webapiEmailExist objExist)
        {

            if (objExist == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }


            String memberEmail = objExist.memberEmail.Trim();

            if (String.IsNullOrWhiteSpace(memberEmail))
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.InvalidLogin);
            }

            if (dgFramework.emailChecker(memberEmail)) { }
            else
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.EmailInvalidFormat);
            }

            //  Does the email address already exist
            VisitorFactory visitorFactory = new VisitorFactory();
            var memberExists = await visitorFactory.callMemberCheckSelect(memberEmail);
            if (memberExists != null)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.EmailDuplicate);
            }

            return "";
        }


        [HttpPost]
        public async Task<newAddressReturn> addAddress(webapiAddress objAddress)
        {
            //  Determine the date to obtain the data from the cms
            var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

            if (objAddress == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }


            var session = HttpContext.Current.Session;
            if (dgFramework.isUserLoggedIn(session["loginStatus"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.SecurityFail);
            }

            string companyName = objAddress.companyName?.Trim();
            string addressName = objAddress.name?.Trim();
            string addressLineOne = objAddress.addressLine1?.Trim();
            string addressLineTwo = objAddress.addressLine2?.Trim();
            string addressTown = objAddress.town?.Trim();
            string addressCounty = objAddress.county?.Trim();
            string addressPostCode = objAddress.postcode?.Trim();
            string addressPhone = objAddress.phone?.Trim();


            //  Validate passed data
            if ((addressName?.Length >= 2) && (addressName?.Length <= 50)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.NameOutsideRange);
            }
                        
            if (companyName?.Length <= 35) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.CompanyNameOutsideRange);
            }

            if ((addressLineOne?.Length >= 2) && (addressLineOne?.Length <= 60)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressLineOneOutsideRange);
            }

            if (addressLineTwo?.Length <= 60) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressLineTwoOutsideRange);
            }

            if ((addressTown?.Length >= 2) && (addressTown?.Length <= 50)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressTownOutsideRange);
            }

            if ((addressCounty?.Length >= 2) && (addressCounty?.Length <= 40)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressCountyOutsideRange);
            }

            if ((addressPostCode?.Length >= 2) && (addressPostCode?.Length <= 20)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressPostCodeOutsideRange);
            }
            
            if (addressPhone?.Length <= 20) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressPhoneOutsideRange);
            }


            Int64 visitorDbId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }


            //  Save the new member address
            VisitorFactory visitorFactory = new VisitorFactory();
            Int64 memberAddressId = await visitorFactory.saveMemberAddress(visitorDbId, addressName, companyName, addressLineOne, addressLineTwo, addressTown, addressCounty, addressPostCode, objAddress.countryId, addressPhone);
            if (memberAddressId == -1) 
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressNotInserted);
            }


            //  Get the new address details (so the client can reuse its infrastructure)
            MemberAddressSelect objMemberAddressSelect = await visitorFactory.getMemberAddresses(visitorDbId, memberAddressId, objAddress.languageId, null, useDate);
            if (objMemberAddressSelect.Addresses.Count() == 0)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressNotInserted);
            }


            newAddressReturn objReturn = new newAddressReturn
            {
                memberAddressDetail = objMemberAddressSelect.Addresses[0]
            };
            return objReturn;

        }


        [HttpPost]
        public async Task<editAddressReturn> editAddress(webapiAddress objAddress)
        {
            //  Determine the date to obtain the data from the cms
            var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

            if (objAddress == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            var session = HttpContext.Current.Session;
            if (dgFramework.isUserLoggedIn(session["loginStatus"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.SecurityFail);
            }


            var memberAddressId = objAddress.memberAddressId;
            string companyName = objAddress.companyName?.Trim();
            string addressName = objAddress.name?.Trim();
            string addressLineOne = objAddress.addressLine1?.Trim();
            string addressLineTwo = objAddress.addressLine2?.Trim();
            string addressTown = objAddress.town?.Trim();
            string addressCounty = objAddress.county?.Trim();
            string addressPostCode = objAddress.postcode?.Trim();
            string addressPhone = objAddress.phone?.Trim();


            //  Validate passed data
            if (addressName != null && (addressName?.Length < 2 || addressName?.Length > 50))
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.NameOutsideRange);
            }

            if (companyName != null && (companyName?.Length > 35))
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.CompanyNameOutsideRange);
            }

            if (addressLineOne != null && (addressLineOne?.Length < 2 || addressLineOne?.Length > 60))
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressLineOneOutsideRange);
            }

            if (addressLineTwo != null && (addressLineTwo?.Length > 60))
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressLineTwoOutsideRange);
            }

            if (addressTown != null && (addressTown?.Length < 2 || addressTown?.Length > 50))
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressTownOutsideRange);
            }

            if (addressCounty != null && (addressCounty?.Length < 2 || addressCounty?.Length > 40))
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressCountyOutsideRange);
            }

            if (addressPostCode != null && (addressPostCode?.Length < 2 || addressPostCode?.Length > 20))
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressPostCodeOutsideRange);
            }

            if (addressPhone != null && (addressPhone?.Length > 20))
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressPhoneOutsideRange);
            }

            long visitorDbId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            //  Does the member address exist (and belong to the visitor)?
            var visitorFactory = new VisitorFactory();
           
            bool success = false;

            if (memberAddressId.HasValue)
            {
                //  Save the updated member address
                success = await visitorFactory.updateMemberAddress(memberAddressId.Value, addressName, companyName, addressLineOne, addressLineTwo, addressTown, addressCounty, addressPostCode, objAddress.countryId, addressPhone);
                if (success == true) { }
                else
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressNotInserted);
                }
            }
            else
            {
                memberAddressId = await visitorFactory.saveMemberAddress(visitorDbId, addressName, companyName, addressLineOne, addressLineTwo, addressTown, addressCounty, addressPostCode, objAddress.countryId, addressPhone);
                if (memberAddressId != null) 
                {
                    success = true;
                }
                else
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressNotInserted);
                }
            }

            var objMemberAddressSelect = await visitorFactory.getMemberAddresses(visitorDbId, memberAddressId, objAddress.languageId, null, useDate);

            if (objMemberAddressSelect.Addresses.Count() == 0)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressNotInserted);
            }

            var objReturn = new editAddressReturn
            {
                memberAddressDetail = objMemberAddressSelect.Addresses[0]
            };

            return objReturn;
        }


        [HttpPost]
        public async Task<IHttpActionResult> deleteAddress(webapiDeleteAddress objAddress)
        {
            var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

            if (objAddress == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            var session = HttpContext.Current.Session;
            if (dgFramework.isUserLoggedIn(session["loginStatus"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.SecurityFail);
            }

            var memberCountry = objAddress.country?.Trim();
            
            var objCountryDetail = new countryItem();

            //  Validate passed data
            var objCountry = new cmsCountry();

            var objCountryData = await objCountry.countryLanguageSelect(useDate);

            // Why did you not create an enum and use enum.isdefined or something?
            if (dgFramework.doesCountryExistFromCode(memberCountry, objCountryData) == true)
            {
                objCountryDetail = dgFramework.getCountryFromCode(memberCountry, objCountryData);
            }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressCountryOutsideRange);
            }

            
            Int64 visitorDbId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }


            //  Does the member address exist (and belong to the visitor)?
            var visitorFactory = new VisitorFactory();
            
            var tempMemberAddressSelect = await visitorFactory.getMemberAddresses(visitorDbId, objAddress.memberAddressId, objCountryDetail.LanguageID, null, useDate);

            if (tempMemberAddressSelect.Addresses.Count() == 0)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressNotInserted);
            }


            //  Delete the member address
            var success = await visitorFactory.deleteMemberAddress(objAddress.memberAddressId);
            
            if (!success)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressNotInserted);
            }

            return Ok(new { });
        }

        [HttpPost]
        public async Task<object> asyncSaveMemberThirdPartyPreference(webapiMemberThirdPartyPreference objPref)
        {
            var objReturn = new booleanResult
            {
                success = false
            };

            var session = HttpContext.Current.Session;
            if (dgFramework.isUserLoggedIn(session["loginStatus"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.SecurityFail);
            }
            
            Int32? memberId = null;
            if (HttpContext.Current.Session["memberIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                memberId = Convert.ToInt32(HttpContext.Current.Session["memberIdFromDatabase"].ToString());
            }
            
            VisitorFactory visitorFactory = new VisitorFactory();
            objReturn.success = await visitorFactory.saveMemberThirdPartyPreference(memberId.Value, objPref.permission);

            return objReturn;
        }

        [HttpPost]
        public memberFullName getCurrentMemberFullName()
        {
            var session = HttpContext.Current.Session;
            if (dgFramework.isUserLoggedIn(session["loginStatus"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.SecurityFail);
            }

            memberFullName objReturn = new memberFullName
            {
                fullName = (session["memberName"] ?? "").ToString()
            };

            return objReturn;
        }

        [HttpPost]
        public async Task<object> myAccountSave(webapiMyAccountSave objSave)
        {
            var vf = new VisitorFactory();
            var formDb = new formRoutines();
            
            if (objSave == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            // Must be logged in, have a visitorId and a memberId in Session...
            var session = HttpContext.Current.Session;

            if (dgFramework.isUserLoggedIn(session["loginStatus"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.SecurityFail);
            }

            long visitorDbId = -1;
            
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }
            
            int? memberId = null;
            
            if (HttpContext.Current.Session["memberIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                memberId = Convert.ToInt32(HttpContext.Current.Session["memberIdFromDatabase"].ToString());
            }
            
            if (!String.IsNullOrEmpty(objSave.NewPassword))
            {
                objSave.CurrentPassword = objSave.CurrentPassword.Trim();
                objSave.NewPassword = objSave.NewPassword.Trim();

                if ((objSave.NewPassword.Length >= 6) && (objSave.NewPassword.Length <= 20)) { }
                else
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.PasswordOutsideRange);
                }
                
                var currentPasswordEnc = await vf.generatePassword(objSave.CurrentPassword);
                var newPasswordEnc = await vf.generatePassword(objSave.NewPassword);
                
                var success = await vf.updateMemberPassword(visitorDbId, currentPasswordEnc, newPasswordEnc);

                if (success == true) { }
                else
                {
                    return dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed);
                }
            }

            objSave.MemberID = memberId.Value;

            objSave.IPAddress = HttpContext.Current.Request.Headers["CF-Connecting-IP"];

            _ = await formDb.saveMyAccount(objSave);

            if (objSave.FullName != session["memberName"].ToString())
            {
                session["memberName"] = objSave.FullName;
            }

            if (objSave.Email != session["memberEmail"].ToString())
            {
                session["memberEmail"] = objSave.Email;
            }

            return new { };
        }

        [HttpPost]
        public async Task<Object> memberPasswordUpdate(webapiPasswordUpdateRequest objPassword)
        {


            if (objPassword == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }


            var session = HttpContext.Current.Session;
            if (dgFramework.isUserLoggedIn(session["loginStatus"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.SecurityFail);
            }

            objPassword.currentPassword = objPassword.currentPassword.Trim();
            objPassword.newPassword = objPassword.newPassword.Trim();

            if ((objPassword.newPassword.Length >= 6) && (objPassword.newPassword.Length <= 20)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.PasswordOutsideRange);
            }


            VisitorFactory visitorFactory = new VisitorFactory();
            String currentPasswordEnc = await visitorFactory.generatePassword(objPassword.currentPassword);
            String newPasswordEnc = await visitorFactory.generatePassword(objPassword.newPassword);


            Int64 visitorDbId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }


            Boolean success = await visitorFactory.updateMemberPassword(visitorDbId, currentPasswordEnc, newPasswordEnc);
            if (success == true) { }
            else
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed);
            }


            return success;
        }



        [HttpPost]
        public async Task<Object> memberCountryUpdate(webapiCountryUpdateRequest objCountryLanguage)
        {
            var session = HttpContext.Current.Session;
            if (dgFramework.isUserLoggedIn(session["loginStatus"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.SecurityFail);
            }

            VisitorFactory visitorFactory = new VisitorFactory();
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                _ = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            Int32 memberId = -1;
            if (session["memberIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
            }


            Boolean success = await visitorFactory.updateMemberCountry(memberId, objCountryLanguage.countryLanguageId);
            if (success == true) { }
            else
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed);
            }


            return success;
        }
        
        [HttpPost]
        public async Task<object> memberTradeAccountCompleteAuthentication(webapiTradeRegistrationSubmit objSubmission)
        {
            if (objSubmission == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            var session = HttpContext.Current.Session;

            bool success;
            try
            {
                VisitorFactory visitorFactory = new VisitorFactory();

                var visitorDb = new databaseDataContext();

                Int64 visitorDbId = -1;
                if (session["visitorIdFromDatabase"] == null)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
                }
                else
                {
                    visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
                }

                var passwordHash = await visitorFactory.generatePassword(objSubmission.password);

                var updateResult = visitorDb.callTradeMemberAuthComplete(
                    objSubmission.guid,
                    objSubmission.postcode,
                    objSubmission.phone,
                    objSubmission.accountNumber,
                    objSubmission.fullName,
                    passwordHash,
                    objSubmission.displayTradePrice
                ).ToList().Single();

                if (updateResult.Result == true)
                {

                    //  Automatical Login
                    var memberData = await visitorFactory.memberSignIn(visitorDbId, objSubmission.email, passwordHash);

                    if (memberData.memberLoginStatusData.Count == 0)
                    {
                        return dgFramework.raiseErrorToHandle(webapiDataError.InvalidLogin);
                    }
                    if (memberData.memberLoginStatusData[0].LoginSuccessful == 0)
                    {
                        return dgFramework.raiseErrorToHandle(webapiDataError.InvalidLogin);
                    }

                    session["memberIdFromDatabase"] = memberData.memberLoginStatusData[0].MemberID;
                    session["memberName"] = objSubmission.fullName;
                    session["memberEmail"] = objSubmission.email;
                    session["loginStatus"] = dgLoginStatus.currentLogin.ToString();

                    session["memberTradeAccountNumber"] = memberData.memberLoginStatusData[0].MertexCustomerAccountNumber;
                    session["memberTradeChecksPassed"] = true;
                    session["memberTradeAccountID"] = updateResult.TradeAccountID;
                    session["memberDisplayTradePrice"] = objSubmission.displayTradePrice;

                    success = true;

                    var tradeSignInCookie = new HttpCookie("dg-trade-signin", objSubmission.masterDefinition.UrlBreakdown.countryDetails.URL);

                    tradeSignInCookie.Expires = DateTime.Now.AddMonths(1);

                    HttpContext.Current.Response.Cookies.Add(tradeSignInCookie);
                }
                else
                {
                    return dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed);
                }
            }
            catch (MertexAccountChecks.MertexAccountException)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.accountNotFoundInMertex);//Json(new Object() { handleErrorMessage = "accountNotFoundInMertex" });
            }
            catch (MertexAccountChecks.MertexCommunicationException)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.mertexCommunicationError);//Json(new Object() { handleErrorMessage = "mertexCommunicationError" });
            }
            catch (Exception)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed);
            }

            if (success)
            {
                var result = GenerateTradeMenu(objSubmission.masterDefinition);
                return result;
            }

            return success;
        }

        protected static string GenerateTradeMenu(masterDefinition objMaster)
        {
            var languageId = objMaster.UrlBreakdown.countryDetails.LanguageID;
            var countryId = objMaster.UrlBreakdown.countryDetails.CountryID;
            var contentDate = objMaster.UrlBreakdown.ContentDate;

            string result = $@"
            <a class=""btn navbar-btn"" aria-haspopup=""true"" href=""#"">{HtmlHelperExtra.Resource("Trade", languageId, countryId, contentDate)}</a>
            <ul class=""dropdown"" aria-label=""submenu"">
                <li>
                    {HtmlHelperExtra.Resource("TradeMenuPlaceOrders", languageId, countryId, contentDate)}
                </li>
                <li>
                    {HtmlHelperExtra.Resource("TradeMenuRequestSamples", languageId, countryId, contentDate)}
                </li>
                <li>
                    {HtmlHelperExtra.Resource("TradeMenuPricing", languageId, countryId, contentDate)}
                </li>
                <li>
                    <a href=""{dgFramework.buildUrl(objMaster.UrlBreakdown.countryDetails, null, dgContentType.landingPage, (int)dgFixedPage.memberEdit, objMaster.UrlBreakdown.ContentDate.ToString(), null)}"">
                        {HtmlHelperExtra.Resource("TradeMenuManage", languageId, countryId, contentDate)}
                    </a>
                </li>
                <li>
                    <a href=""{dgFramework.buildUrl(objMaster.UrlBreakdown.countryDetails, null, dgContentType.landingPage, (int)dgFixedPage.tradeClearance, objMaster.UrlBreakdown.ContentDate.ToString(), null)}"">
                        {HtmlHelperExtra.Resource("ClearanceOffers", languageId, countryId, contentDate)}
                    </a>
                </li>
                <li>
                    <a onclick=""downloadClearanceSpreadsheet();"">
                        {HtmlHelperExtra.Resource("TradeDownloadClearanceSpreadsheet", languageId, countryId, contentDate)}
                    </a>
                </li>
                <li>
                    <a onclick=""downloadClearancePdf();"">
                        {HtmlHelperExtra.Resource("TradeDownloadClearancePDF", languageId, countryId, contentDate)}
                    </a>
                </li>
                <li>
                    <a href=""{dgFramework.buildUrl(objMaster.UrlBreakdown.countryDetails, null, dgContentType.landingPage, (int)dgFixedPage.moodboard, objMaster.UrlBreakdown.ContentDate.ToString(), null)}"">
                        {HtmlHelperExtra.Resource("TradeMenuMoodBoards", languageId, countryId, contentDate)}
                    </a>
                </li>
                <li>
                    <a href=""{dgFramework.buildUrl(objMaster.UrlBreakdown.countryDetails, null, dgContentType.landingPage, 1179, objMaster.UrlBreakdown.ContentDate.ToString(), null)}"">
                        {HtmlHelperExtra.Resource("TradeMenuViewBrochures", languageId, countryId, contentDate)}
                    </a>
                </li>
                <li>
                    <a href=""{dgFramework.buildUrl(objMaster.UrlBreakdown.countryDetails, null, dgContentType.landingPage, 1180, objMaster.UrlBreakdown.ContentDate.ToString(), null)}"">
                        {HtmlHelperExtra.Resource("TradeMenuViewFilms", languageId, countryId, contentDate)}
                    </a>
                </li>
                <li>
                    <a href=""{dgFramework.buildUrl(objMaster.UrlBreakdown.countryDetails, null, dgContentType.landingPage, (int)dgFixedPage.serverSearch, objMaster.UrlBreakdown.ContentDate.ToString(), null, "productType=&sort=new")}"">
                        {HtmlHelperExtra.Resource("TradeMenuAdvancedProductSearch", languageId, countryId, contentDate)}
                    </a>
                </li>
                <li class=""dropdown-divider""></li>
                <li>
                    <span class=""trade-menu-header"">
                        {HtmlHelperExtra.Resource("TradeMenuProductFeeds", languageId, countryId, contentDate)}
                        <a href=""https://downloads.designersguild.com"" target=""_blank"" rel=""nofollow"">
                            {HtmlHelperExtra.Resource("TradeMenuVisitDownloads", languageId, countryId, contentDate)}
                        </a>
                    </span>
                </li>
                <li>
                    {HtmlHelperExtra.Resource("TradeMenuDownloadPrintReady", languageId, countryId, contentDate)}
                </li>
                <li>
                    {HtmlHelperExtra.Resource("TradeMenuDownloadFeeds", languageId, countryId, contentDate)}
                </li>
                <li>
                    {HtmlHelperExtra.Resource("TradeMenuDownloadExcel", languageId, countryId, contentDate)}
                </li>
                <li class=""dropdown-divider""></li>
                <li>
                    <span class=""trade-menu-header"">
                        {HtmlHelperExtra.Resource("TradeMenuB2BFeatures", languageId, countryId, contentDate)}
                        <a href=""https://trade.designersguild.com"" target=""_blank"" rel=""nofollow"">
                            {HtmlHelperExtra.Resource("TradeMenuTradeWebsite", languageId, countryId, contentDate)}
                        </a>
                    </span>
                </li>
                <li>
                    {HtmlHelperExtra.Resource("TradeMenuCheckStock", languageId, countryId, contentDate)}
                </li>
                <li>
                    {HtmlHelperExtra.Resource("TradeMenuPlaceOrders", languageId, countryId, contentDate)}
                </li>
                <li>
                    {HtmlHelperExtra.Resource("TradeMenuViewOrderStatus", languageId, countryId, contentDate)}
                </li>
                <li>
                    {HtmlHelperExtra.Resource("TradeMenuRequestSamples", languageId, countryId, contentDate)}
                </li>
                <li>
                    {HtmlHelperExtra.Resource("TradeMenuReserveStock", languageId, countryId, contentDate)}
                </li>
                <li>
                    {HtmlHelperExtra.Resource("TradeMenuCopyInvoices", languageId, countryId, contentDate)}
                </li>
                <li>
                    {HtmlHelperExtra.Resource("TradeMenuAccountBalance", languageId, countryId, contentDate)}
                </li>
                <li>
                    {HtmlHelperExtra.Resource("TradeMenuRecentPayments", languageId, countryId, contentDate)}
                </li>
                <li>
                    {HtmlHelperExtra.Resource("TradeMenuProformaInvoices", languageId, countryId, contentDate)}
                </li>
                <li>
                    {HtmlHelperExtra.Resource("TradeMenuSalesPerformance", languageId, countryId, contentDate)}
                </li>
                <li>
                    {HtmlHelperExtra.Resource("TradeMenuPricelists", languageId, countryId, contentDate)}
                </li>
            </ul>";
            return result;
        }

        [HttpPost]
        public async Task<object> memberDisplayTradePriceUpdate(webapiDisplayTradePriceUpdateRequest objDisplayTradePrice)
        {
            bool success;
            try
            {
                if (objDisplayTradePrice == null)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
                }
                
                var session = HttpContext.Current.Session;

                if (session["loginStatus"].ToString() == dgLoginStatus.notLoggedIn.ToString())
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.SecurityFail);
                }

                Boolean currentSetting = (Boolean)session["memberDisplayTradePrice"];




                VisitorFactory visitorFactory = new VisitorFactory();
                
                Int32 memberId = -1;
                if (session["memberIdFromDatabase"] == null)
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
                }
                else
                {
                    memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
                }

                success = await visitorFactory.updateMemberDisplayTradePrice(memberId, objDisplayTradePrice.displayTradePrice);
                if (success == true) { }
                else
                {
                    return dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed);
                }


                session["memberDisplayTradePrice"] = objDisplayTradePrice.displayTradePrice;

            }
            catch (Exception)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.MemberUpdateFailed);
            }

            return success;
        }
        
        [HttpPost]
        public async Task<Object> memberUpdate(webapiMemberUpdateRequest objMember)
        {


            if (objMember == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }


            var session = HttpContext.Current.Session;
            if (dgFramework.isUserLoggedIn(session["loginStatus"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.SecurityFail);
            }

            objMember.FullName = objMember.FullName.Trim();

            if ((objMember.FullName.Length >= 2) && (objMember.FullName.Length <= 40)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.NameOutsideRange);
            }

            
            Int64 visitorDbId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }


            VisitorFactory visitorFactory = new VisitorFactory();
            Boolean success = await visitorFactory.updateMemberDetail(visitorDbId, objMember.FullName);


            session["memberName"] = objMember.FullName;


            return success;
        }


        [HttpPost]
        public async Task<IHttpActionResult> requestPasswordReset(PassPasswordResetRequest objRequest)
        {
            var requestWrapper = new HttpRequestWrapper(HttpContext.Current.Request);

            var useDate = Framework.GetUseDate(requestWrapper);

            if (objRequest == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            if ((objRequest.email.Length < 5) || (objRequest.email.Length > 256))
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.EmailOutsideRange);
            }

            if (!dgFramework.emailChecker(objRequest.email))
            {
                return Content(HttpStatusCode.BadRequest, dgFramework.raiseErrorToHandle(webapiDataError.EmailInvalidFormat));
            }

            var visitorFactory = new VisitorFactory();

            var success = await visitorFactory.passwordResetRequest(objRequest);

            if (success)
            {
                return Ok(success);
            }
            else
            {
                return Content(HttpStatusCode.BadRequest, dgFramework.raiseErrorToHandle(webapiDataError.PasswordResetRequestEmailNotFound));
            }
        }


        [HttpPost]
        public async Task<IHttpActionResult> passwordReset([FromBody]webapiMemberResetPassword objPassword)
        {

            if (objPassword == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }


            objPassword.token = objPassword.token.Trim();
            objPassword.newPassword = objPassword.newPassword.Trim();

            if ((objPassword.newPassword.Length >= 6) && (objPassword.newPassword.Length <= 20)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.PasswordOutsideRange);
            }


            VisitorFactory visitorFactory = new VisitorFactory();
            String newPasswordEnc = await visitorFactory.generatePassword(objPassword.newPassword);

                        
            if (objPassword.token.Length == 56) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }


            var partToken = objPassword.token.Substring(0, 36);

            //  Validate token is a GUID
            _ = new Guid();
            var tokenValid = Guid.TryParse(partToken, out _);
            if (tokenValid == true) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }


            var success = await visitorFactory.resetPassword(objPassword.token, newPasswordEnc);

            return Ok(new { success });
        }


        [HttpPost]
        public async Task<IHttpActionResult> orderHistoryLoad(webapiMemberOrderHistory objOrderHistory)
        {
            var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

            if (objOrderHistory == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }


            //  Must be logged in
            var session = HttpContext.Current.Session;
            if (dgFramework.isUserLoggedIn(session["loginStatus"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.SecurityFail);
            }

            //  Get the basket country details
            var objCountry = new cmsCountry();
            var objCountryData = await objCountry.countryLanguageSelect(useDate);

            if (dgFramework.doesCountryExistFromCode(objOrderHistory.countryCode, objCountryData)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
            }

            var objCountryItem = dgFramework.getCountryFromCode(objOrderHistory.countryCode, objCountryData);


            //  Get the visitor id
            Int64 visitorDbId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }


            //  Set the resource file language
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(objCountryItem.DotNetCulture);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            var visitorFactory = new VisitorFactory();
            var orderHistoryData = await visitorFactory.getOrderHistory(
                visitorDbId, 
                objCountryItem,                      
                objOrderHistory.searchString,                        
                objOrderHistory.historyDays,                                   
                objOrderHistory.orderStatusId,                                   
                useDate.ToString("ddMMyyyy"));

            return Ok(orderHistoryData);
        }

        [HttpPost]
        public async Task<IHttpActionResult> saveReturn(webapiMemberSaveReturn objSaveReturn)
        {

            if (objSaveReturn == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }


            if (objSaveReturn.orderHeadId == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            var session = HttpContext.Current.Session;

            //  Must be logged in
            //
            // Tom 04/12/2023 - not anymore you mustn't! 
            // they wanted me to add pregenerated returns paperwork to the order confirmation email
            // no, seriously, that's what they said
            // it wasn't too difficult to convince them that this was a bad idea, and so here we are compromising
            // you can now click a "Return" link in your order confirmation email to be taken to L164 to be able to generate a return without being signed in, or even a member.
            //
            // Super.
            // 
            //var session = HttpContext.Current.Session;
            //if (dgFramework.isUserLoggedIn(session["loginStatus"]) == false)
            //{
            //    dgFramework.raiseErrorNotHandled(webapiUnhandledError.SecurityFail);
            //}


            //  Get the visitor id
            Int64 visitorDbId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            //  Build the SQL data table type
            DataTable returnTableType = new DataTable();
            returnTableType.Columns.Add("OrderLineID", typeof(Int64));
            returnTableType.Columns.Add("ReturnedQuantity", typeof(Int16));
            returnTableType.Columns.Add("ReturnReasonID", typeof(Byte));


            foreach (memberSaveReturnDetail objReturnLine in objSaveReturn.memberSaveReturnDetails)
            {
                returnTableType.Rows.Add(objReturnLine.orderLineId, objReturnLine.returnedQuantity, objReturnLine.returnReasonId);
            }


            //  Save the return
            Int32 returnHeadId = -1;

            try
            {
                VisitorFactory visitorFactory = new VisitorFactory();
                returnHeadId = await visitorFactory.saveReturn(visitorDbId, (Int32)objSaveReturn.orderHeadId, returnTableType);

                var objReturn = new returnDetails
                {
                    ReturnIdentifier = dgCrypto.Encrypt($"{returnHeadId}")
                };

                return Ok(objReturn);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpPost]
        public object addressLookupSearch(webapiAddressLookupSearch objSearch)
        {
            try
            {
                var key = "";

                switch(objSearch.countries)
                {
                    case "AU":
                        key = ConfigurationManager.AppSettings["PCA-API-Website-AU"];
                        break;
                    default:
                        key = ConfigurationManager.AppSettings["PCA-API-Website-UK"];
                        break;
                }

                if (objSearch.id == null)
                {
                    var client = new PCAFind.PostcodeAnywhere_SoapClient();

                    var results = client.Capture_Interactive_Find_v1_00(
                        Key: key,
                        Text: objSearch.text,
                        Container: objSearch.container,
                        Origin: objSearch.origin,
                        Countries: objSearch.countries,
                        Limit: 12,
                        Language: "en-GB"
                    );

                    return results;
                }
                else
                {
                    var client = new PCARetrieve.PostcodeAnywhere_SoapClient();

                    var results = client.Capture_Interactive_Retrieve_v1_00(
                        Key: key,
                        Id: objSearch.id,
                        Field1Format: null,
                        Field2Format: null,
                        Field3Format: null,
                        Field4Format: null,
                        Field5Format: null,
                        Field6Format: null,
                        Field7Format: null,
                        Field8Format: null,
                        Field9Format: null,
                        Field10Format: null,
                        Field11Format: null,
                        Field12Format: null,
                        Field13Format: null,
                        Field14Format: null,
                        Field15Format: null,
                        Field16Format: null,
                        Field17Format: null,
                        Field18Format: null,
                        Field19Format: null,
                        Field20Format: null
                    );

                    return results;
                }
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        [HttpPost]
        public async Task<Object> createBackInStockReminder(PassBackInStockReminderRequest objReminder)
        {
            var response = new backInStockReminderReturn();

            try
            {
                var session = HttpContext.Current.Session;

                session["backInStockReminder"] = objReminder.productId;
            
                if (dgFramework.isUserLoggedIn(session["loginStatus"]) == false)
                {
                    response.mustLogin = true;
                    response.success = false;
                }
                else
                {
                    var visitorFactory = new VisitorFactory();
                    long visitorDbId = -1;

                    if (session["visitorIdFromDatabase"] == null)
                    {
                        dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);

                        response.success = false;
                    }
                    else
                    {
                        visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());

                        response.success = await visitorFactory.createBackInStockReminder(visitorDbId, objReminder.productId, objReminder.countryId, objReminder.languageId);

                        session["backInStockReminder"] = null;
                    }

                    response.mustLogin = false;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return response;
        }
        
        [HttpPost]
        public async Task<IHttpActionResult> extantEmailCheck(PassEmail objEmail)
        { 
            //  Does the email address already exist
            VisitorFactory visitorFactory = new VisitorFactory();
            var emailExists = await visitorFactory.callEmailExistsSelect(objEmail.email);
            if (emailExists != null)
            {
                return Ok(dgFramework.raiseErrorToHandle(webapiDataError.EmailDuplicate));
            }
            else
            {
                return Ok(true);
            }
        }

        [HttpPost]
        public async Task<object> extantTradePermissionsCheck(webapiEmailExist objEmail)
        {
            VisitorFactory visitorFactory = new VisitorFactory();
            var permissionsExist = await visitorFactory.callTradePermissionsCheck(objEmail.memberEmail);
            if (permissionsExist != null)
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.TradePermissionsExist);
            }
            else
            {
                var result = await visitorFactory.callMemberFetch(objEmail.memberEmail);

                return result;
            }
        }

        [HttpPost]
        public async Task<object> logEmailCheck(webapiEmailExist objEmail)
        {
            string email = null;
            if (objEmail != null)
            {
                email = objEmail.memberEmail;
            }

            Int64 visitorDbId = -1;
            if (HttpContext.Current.Session["visitorIdFromDatabase"] == null)
            {
            }
            else
            {
                visitorDbId = Convert.ToInt64(HttpContext.Current.Session["visitorIdFromDatabase"].ToString());
            }

            //  Does the email address already exist
            VisitorFactory visitorFactory = new VisitorFactory();
            _ = await visitorFactory.logEmailExistsCheck(visitorDbId, email);

            return true;
        }

        [HttpPost]
        public async Task<object> LoadLanguagesForCountry(webapiCountry objCountry)
        {
            // Determine the date to obtain the data from the cms
            DateTime useDate = DateTime.Now;
            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                // Should the "use date" be overriden?
                if (HttpContext.Current.Request.QueryString["dateOverride"] == null) { }
                else
                {
                    String overrideDate = HttpContext.Current.Request.QueryString["dateOverride"].ToString();
                    if (overrideDate.Length == "ddMMyyyy".Length)
                    {
                        bool dateConvertSuccess = DateTime.TryParseExact(overrideDate,
                                                                              "ddMMyyyy",
                                                                              CultureInfo.InvariantCulture,
                                                                              DateTimeStyles.None,
                                                                              out useDate);
                        if (dateConvertSuccess == true) { }
                        else
                        {
                            useDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                        }
                    }
                }
            }

            var cmsCountry = new cmsCountry();
            var languageSelectResult = await cmsCountry.loadCompleteLanguageList(objCountry.countryId, useDate);

            return languageSelectResult.languages;
        }

#endregion

        [HttpPost]
        public async Task<IHttpActionResult> makeAddressDefault(webapiDefaultAddress objAddress)
        {
            var session = HttpContext.Current.Session;

            if (objAddress == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            if (dgFramework.isUserLoggedIn(session["loginStatus"]) == false)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.SecurityFail);
            }

            Int64 visitorId = 0;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.SecurityFail);
            }
            else
            {
                visitorId = Convert.ToInt64(session["visitorIdFromDatabase"]);
            }

            Int32 memberId = 0;
            if (session["memberIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.SecurityFail);
            }
            else
            {
                memberId = Convert.ToInt32(session["memberIdFromDatabase"]);
            }

            var objCountry = new cmsCountry();
            var objCountryData = await objCountry.countryLanguageSelect(DateTime.Now);

            if (dgFramework.doesCountryExistFromCode(objAddress.country, objCountryData)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
            }

            var objCountryItem = dgFramework.getCountryFromCode(objAddress.country, objCountryData);

            // check that supplied address belongs to this member
            var objVisitorFactory = new VisitorFactory();
            var objMemberAddressSelect = await objVisitorFactory.getMemberAddresses(visitorId, null, objCountryItem.LanguageID, null, DateTime.Now);

            if (objMemberAddressSelect.Addresses.Any(a => a.MemberAddressID == objAddress.memberAddressId))
            {
                await objVisitorFactory.setDefaultMemberAddress(memberId, objAddress.memberAddressId, objCountryItem.CountryID);
            }

            return Ok(new { });
        }
    }
}
