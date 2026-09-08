using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.DTOs;
using wwwDesignersGuild.Models.Form;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.LandingPage;
using wwwDesignersGuild.Models.Visitor;
using wwwDesignersGuild.Util;

using wwwDesignersGuild.Models.Database;
using static Antlr.Runtime.Tree.TreeWizard;

namespace wwwDesignersGuild.Controllers
{
    public class FormController : ApiController
    {
         
        [HttpPost]
        public async Task<object> SaveTradeAccountRequestForm()//webapiTradeAccountRequestFormSave objFormDetails)
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var root = HttpContext.Current.Server.MapPath("~/Uploads");

            var provider = new MultipartFormDataStreamProvider(root);

            webapiTradeAccountRequestFormSave objFormDetails = null;

            var result = await Request.Content.ReadAsMultipartAsync(provider);

            List<byte> bytes = null;

            string fileExtension = null;

            try
            {
                if (provider.FileData.Any())
                {
                    bytes = File.ReadAllBytes(result.FileData[0].LocalFileName).ToList();

                    if (bytes.Count > 2097152)
                    {
                        return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "File too large");
                    }

                    switch (provider.FileData[0].Headers.ContentType.MediaType)
                    {
                        case "application/pdf":
                            fileExtension = "pdf";
                            break;
                        case "image/jpg":
                        case "image/jpeg":
                        case "image/jfif":
                            fileExtension = "jpg";
                            break;
                    }

                    try
                    {
                        File.Delete(result.FileData[0].LocalFileName);
                    }
                    catch (Exception)
                    {

                    }
                }

                // Show all the key-value pairs.
                foreach (var key in provider.FormData.AllKeys)
                {
                    if (key == "obj")
                    {
                        objFormDetails = JsonConvert.DeserializeObject<webapiTradeAccountRequestFormSave>(provider.FormData.GetValues(key)[0]);

                        var objFormRoutines = new formRoutines();

                        var visitorId = -1L;

                        if (HttpContext.Current.Session["visitorIdFromDatabase"] == null)
                        {
                            dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
                        }
                        else
                        {
                            visitorId = Convert.ToInt64(HttpContext.Current.Session["visitorIdFromDatabase"].ToString());
                        }

                        if (objFormDetails.gRecaptcha == null)
                        {
                            var error = dgFramework.raiseErrorToHandle(webapiDataError.InvalidCaptcha);
                            return error;
                        }
                        else
                        {
                            //  Validate captcha
                            if (String.IsNullOrEmpty(objFormDetails.gRecaptcha))
                            {
                                return dgFramework.raiseErrorToHandle(webapiDataError.InvalidCaptcha);
                            }
                            else
                            {
                                if (ReCaptchaVerifier.Validate(objFormDetails.gRecaptcha) == "true")
                                {

                                }
                                else
                                {
                                    return dgFramework.raiseErrorToHandle(webapiDataError.InvalidCaptcha);
                                }
                            }
                        }

                        
                        var saveResult = await objFormRoutines.saveTradeAccountRequestForm(visitorId, objFormDetails, bytes?.ToArray(), fileExtension);

                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.ExpectationFailed);
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [HttpPost]
        public async Task<object> SaveTradeAccountImmediateOrderFlag(webapiLeadId objLeadId)
        {
            var result = await new formRoutines().saveLeadImmediateOrderFlag(objLeadId.leadId);

            return result;
        }

        [HttpPost]
        public async Task<object> UnsubscribeMember(MemberUnsubscribe objUnsub)
        {
            formRoutines objFormRoutines = new formRoutines();
            if (HttpContext.Current.Session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                _ = Convert.ToInt64(HttpContext.Current.Session["visitorIdFromDatabase"].ToString());
            }
            
            var result = await objFormRoutines.unsubscribeMember(objUnsub);

            return result;
        }

        [HttpPost]
        public async Task<object> BeginMemberResubscribe(MemberResubscribe objResub)
        {
            var objFormRoutines = new formRoutines();

            var member = await new VisitorFactory().getMemberByEmail(objResub.email);
            
            var result = await objFormRoutines.beginMemberResubscribe(member.GUID);

            return result;
        }

        [HttpPost]
        public async Task<IHttpActionResult> CheckMarketingPermission([FromBody]webapiMarketingPermissionCheck objCheck)
        {
            long visitorDbId = -1;

            if (HttpContext.Current.Session["visitorIdFromDatabase"] != null)
            {
                visitorDbId = Convert.ToInt64(HttpContext.Current.Session["visitorIdFromDatabase"].ToString());
            }

            int? memberId = null;

            if (HttpContext.Current.Session["memberIdFromDatabase"] != null)
            {
                memberId = Convert.ToInt32(HttpContext.Current.Session["memberIdFromDatabase"].ToString());
            }

            objCheck.VisitorID = visitorDbId;
            objCheck.MemberID = memberId;

            formRoutines objFormRoutines = new formRoutines();
            
            var result = await objFormRoutines.marketingPermissionCheck(objCheck);

            return Ok(result);
        }
        
        [HttpPost]
        public async Task<object> LogMarketingPermissionsError(webapiLogMarketingPermissionsError objError)
        {
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
            _ = await visitorFactory.logMarketingPermissionsFailure(visitorDbId, objError);

            return true;
        }

        [HttpPost]
        public async Task<object> SavePageSectionForm(webapiPageSectionFormSave objForm)
        {
            //  Has a date override been passed
            var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

            // Get the basket country details
            var objCountry = new cmsCountry();
            var objCountryData = await objCountry.countryLanguageSelect(useDate);
            
            var objCountryItem = objCountryData.countryItems.Where(p => p.CountryID == objForm.CountryID).FirstOrDefault();

            if (objCountryItem == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
            }

            //  Get the database user id from the session
            var session = HttpContext.Current.Session;
            long visitorId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            string tradeAccountNumber = null;
            if (session["memberTradeAccountNumber"] == null) { }
            else
            {
                tradeAccountNumber = session["memberTradeAccountNumber"].ToString();
            }

            int? memberId = null;
            if (session["memberIdFromDatabase"] == null) { }
            else
            {
                memberId = Convert.ToInt32(session["memberIdFromDatabase"]);
            }

            // fudge to get registration to save from login & checkoutLogin pages
            if (objForm.SourceID == 10 || objForm.SourceID == 75 || objForm.FormID == 151 || objForm.FormID == 152)
            {
                var earlySave = new pageSectionFormSave()
                {
                    VisitorID = visitorId,
                    MemberID = null,
                    Form = objForm
                };

                if (objForm.SourceID != 10)
                {
                    //  Validate captcha - mandatory form registration 
                    if (String.IsNullOrEmpty(objForm.gRecaptcha))
                    {
                        return dgFramework.raiseErrorToHandle(webapiDataError.InvalidCaptcha);
                    }
                    else
                    {
                        if (ReCaptchaVerifier.Validate(objForm.gRecaptcha) == "true")
                        {

                        }
                        else
                        {
                            return dgFramework.raiseErrorToHandle(webapiDataError.InvalidCaptcha);
                        }
                    }
                }

                if (objForm.PasswordRequired && objForm.Password != null)
                {
                    var visitorFactory = new VisitorFactory();
                    objForm.Password = await visitorFactory.generatePassword(objForm.Password);
                }

                formRoutines objEarlyFormRoutines = new formRoutines();

                var earlyResult = await objEarlyFormRoutines.pageSectionFormSave(earlySave);
                
                if (earlyResult == true && objForm.SourceID == 10)
                {
                    return earlyResult;
                }

                if (earlyResult == true)
                {
                    // we've successfully registered; sign in:

                    var visitorFactory = new VisitorFactory();

                    memberLogin memberData = await visitorFactory.memberSignIn(visitorId, objForm.Email, objForm.Password);

                    if (memberData.memberLoginStatusData.Count == 0)
                    {
                        return dgFramework.raiseErrorToHandle(webapiDataError.InvalidLogin);
                    }
                    if (memberData.memberLoginStatusData[0].LoginSuccessful == 0)
                    {
                        return dgFramework.raiseErrorToHandle(webapiDataError.InvalidLogin);
                    }

                    session["memberIdFromDatabase"] = memberData.memberLoginStatusData[0].MemberID;
                    session["memberName"] = objForm.FullName;
                    session["memberEmail"] = objForm.Email;
                    session["loginStatus"] = dgLoginStatus.currentLogin.ToString();

                    session["memberTradeAccountID"] = null;
                    session["memberTradeAccountNumber"] = null;
                    session["memberTradeChecksPassed"] = false;
                    session["memberDisplayTradePrice"] = false;

                    if (session["backInStockReminder"] == null) { }
                    else
                    {
                        if (session["visitorIdFromDatabase"] == null) { }
                        else
                        {
                            await visitorFactory.createBackInStockReminder(visitorId, Convert.ToInt32(session["backInStockReminder"]), objCountryItem.CountryID, objCountryItem.LanguageID);
                        }
                    }
                }

                return earlyResult;
            }

            //  Get the form details for validation purposes
            var objLandingPage = new cmsLandingPage();
			Models.Classes.cmsLandingPageDefinition objPageData = await objLandingPage.loadLandingPageFromCms(memberId,
                                                                                                objForm.ContentID,
                                                                                                objCountryItem.LanguageID,
                                                                                                objCountryItem.CountryID,
                                                                                                useDate,
                                                                                                session["memberIdFromDatabase"] != null,
                                                                                                session["memberTradeAccountID"] != null,
																								Convert.ToBoolean(HttpContext.Current.Session["countryCheckMatched"]),
                                                                                                session["memberDisplayTradePrice"] != null && Convert.ToBoolean(session["memberDisplayTradePrice"]),
                                                                                                session["memberTradeAccountID"] == null ? (int?)null : Convert.ToInt32(session["memberTradeAccountID"]),
                                                                                                null,
                                                                                                null,
                                                                                                null);

            if (objPageData.pageSectionForms.Where(p => p.PageSectionID == objForm.PageSectionID).FirstOrDefault() == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            var objFormDefinition = objPageData.pageSectionForms.Where(p => p.PageSectionID == objForm.PageSectionID).First();


            //  Validate email block
            objForm.FullName = objForm.FullName.Trim();
            objForm.Email = objForm.Email.Trim();

            if ((objForm.FullName.Length >= 2) && (objForm.FullName.Length <= 40)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.NameOutsideRange);
            }

            if ((objForm.Email.Length >= 5) && (objForm.Email.Length <= 256)) { }
            else
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.EmailOutsideRange);
            }

            if (dgFramework.emailChecker(objForm.Email)) { }
            else
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.EmailInvalidFormat);
            }

            if (objForm.ConfirmEmail)
            {
                objForm.EmailConfirmation = objForm.EmailConfirmation.Trim();

                if (dgFramework.emailChecker(objForm.EmailConfirmation)) { }
                else
                {
                    return dgFramework.raiseErrorToHandle(webapiDataError.EmailConfirmInvalidFormat);
                }
                
                if (objForm.Email == objForm.EmailConfirmation) { }
                else
                {
                    return dgFramework.raiseErrorToHandle(webapiDataError.EmailConfirmFailed);
                }
            }

            // Validate Interior Design Block

            if (objForm.PreferredShop != null)
            {

                objForm.PreferredShop = objForm.PreferredShop.Trim();
                objForm.PreferredTime = objForm.PreferredTime.Trim();

                if ((objForm.PreferredShop.Length >= 2) && (objForm.PreferredShop.Length <= 40)) { }
                else
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.interiorDesignPreferredShopOutsideRange);
                }

                // no, the preferred date should be passable directly into MVC with the correct offset for GMT
                // objForm.PreferredDate = DateTime.Parse(objForm.prefDateStr);
            }

            //  Validate address block
            if (objFormDefinition.AddressRequired == true)
            {
                if (objForm.CompanyName != null)
                {
                    objForm.CompanyName = objForm.CompanyName.Trim();
                    if (objForm.CompanyName.Length <= 35) { }
                    else
                    {
                        dgFramework.raiseErrorNotHandled(webapiUnhandledError.CompanyNameOutsideRange);
                    }
                }

                if (objForm.AddressLine1 != null)
                {
                    objForm.AddressLine1 = objForm.AddressLine1.Trim();
                    if ((objForm.AddressLine1.Length >= 2) && (objForm.AddressLine1.Length <= 60)) { }
                    else
                    {
                        dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressLineOneOutsideRange);
                    }
                }
                else
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressLineOneOutsideRange);
                }

                if (objForm.AddressLine2 != null)
                {
                    objForm.AddressLine2 = objForm.AddressLine2.Trim();
                    if (objForm.AddressLine2.Length <= 60) { }
                    else
                    {
                        dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressLineTwoOutsideRange);
                    }
                }

                if (objForm.Town != null)
                {
                    objForm.Town = objForm.Town.Trim();
                    if ((objForm.Town.Length >= 2) && (objForm.Town.Length <= 50)) { }
                    else
                    {
                        dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressTownOutsideRange);
                    }
                }
                else
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressTownOutsideRange);
                }

                if (objForm.Postcode != null)
                {
                    objForm.Postcode = objForm.Postcode.Trim();
                    if ((objForm.Postcode.Length >= 2) && (objForm.Postcode.Length <= 20)) { }
                    else
                    {
                        dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressPostCodeOutsideRange);
                    }
                }
                else
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressPostCodeOutsideRange);
                }

                if (objForm.County != null)
                {
                    objForm.County = objForm.County.Trim();
                    if ((objForm.County.Length >= 2) && (objForm.County.Length <= 40)) { }
                    else
                    {
                        dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressCountyOutsideRange);
                    }
                }
                else
                {
                    dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressCountyOutsideRange);
                }

                if (objForm.PhoneNumber != null)
                {
                    objForm.PhoneNumber = objForm.PhoneNumber.Trim();
                    if (objForm.PhoneNumber.Length <= 20) { }
                    else
                    {
                        dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressPhoneOutsideRange);
                    }
                }
            }
            else
            {
                objForm.CompanyName = null;
                objForm.AddressLine1 = null;
                objForm.AddressLine2 = null;
                objForm.Town = null;
                objForm.Postcode = null;
                objForm.County = null;

                if (objForm.PreferredShop != null || objForm.StoreEventID != null) { }
                else
                {
                    objForm.PhoneNumber = null;
                }
            }
            
            if (objForm.MarketingCountryID == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.AddressCountryOutsideRange);
            }
            
            //  Validate captcha
            if (objFormDefinition.CaptchaRequired == true)
            {
                if (String.IsNullOrEmpty(objForm.gRecaptcha))
                {
                    return dgFramework.raiseErrorToHandle(webapiDataError.InvalidCaptcha);
                }
                else
                {
                    if (ReCaptchaVerifier.Validate(objForm.gRecaptcha) == "true")
                    {

                    }
                    else
                    {
                        return dgFramework.raiseErrorToHandle(webapiDataError.InvalidCaptcha);
                    }
                }
            }

            if (objForm.PasswordRequired && objForm.Password != null)
            {
                var visitorFactory = new VisitorFactory();
                objForm.Password = await visitorFactory.generatePassword(objForm.Password);
            }

            var save = new pageSectionFormSave()
            {
                VisitorID = visitorId,
                MemberID = memberId,
                Form = objForm
            };

            var objFormRoutines = new formRoutines();
            
            var result = await objFormRoutines.pageSectionFormSave(save);

            return result;
        }

        [HttpPost]
        public async Task<object> LoadECPop(webapiCountryIDLanguageID objLoad)
        {
            var formDb = new cmsEmailCapturePopup();

            var emailCaptureLoad = new passEmailCaptureLoad()
            {
                countryId = objLoad.countryId,
                languageId = objLoad.languageId,
                contentDate = DateTime.Now
            };

            var result = await formDb.loadEmailCapturePopupFormFromCms(emailCaptureLoad);

            return result;
        }


        [HttpPost]
        public async Task<object> SavePageSectionFormNew([FromBody]PassFormSave objSave)
        {
            //  Has a date override been passed
            var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

            // Get the basket country details

            cmsCountry objCountry = new cmsCountry();
            cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(useDate);

            countryItem objCountryItem = objCountryData.countryItems.Where(p => p.CountryID == objSave.CountryID).FirstOrDefault();

            if (objCountryItem == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "InvalidCountry"));
            }

            //  Get the database user id from the session
            var session = HttpContext.Current.Session;
            Int64 visitorId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "VisitorIDFailure"));
            }
            else
            {
                visitorId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            string tradeAccountNumber = null;
            if (session["memberTradeAccountNumber"] == null) { }
            else
            {
                tradeAccountNumber = session["memberTradeAccountNumber"].ToString();
            }

            Int32? memberId = null;
            if (session["memberIdFromDatabase"] == null) { }
            else
            {
                memberId = Convert.ToInt32(session["memberIdFromDatabase"]);
            }

            // fudge to get registration to save from login & checkoutLogin pages
            if (objSave.SourceID == 10 || objSave.FormID == 151 || objSave.FormID == 152)
            {
                if (objSave.SourceID != 10)
                {
                    //  Validate captcha - mandatory form registration 
                    if (String.IsNullOrEmpty(objSave.gRecaptcha))
                    {
                        return dgFramework.raiseErrorToHandle(webapiDataError.InvalidCaptcha);
                    }
                    else
                    {
                        if (ReCaptchaVerifier.Validate(objSave.gRecaptcha) == "true")
                        {

                        }
                        else
                        {
                            return dgFramework.raiseErrorToHandle(webapiDataError.InvalidCaptcha);
                        }
                    }
                }

                if (objSave.PasswordRequired && objSave.Password != null)
                {
                    var visitorFactory = new VisitorFactory();
                    objSave.Password = await visitorFactory.generatePassword(objSave.Password);
                }

                formRoutines objEarlyFormRoutines = new formRoutines();

                var earlyResult = await objEarlyFormRoutines.pageSectionFormSave(objSave, visitorId, memberId);

                if (earlyResult == true && objSave.SourceID == 10)
                {
                    return earlyResult;
                }

                if (earlyResult == true)
                {
                    // we've successfully registered; sign in:

                    var visitorFactory = new VisitorFactory();

                    var memberData = await visitorFactory.memberSignIn(visitorId, objSave.Email, objSave.Password);

                    if (memberData.memberLoginStatusData.Count == 0)
                    {
                        return dgFramework.raiseErrorToHandle(webapiDataError.InvalidLogin);
                    }

                    if (memberData.memberLoginStatusData[0].LoginSuccessful == 0)
                    {
                        return dgFramework.raiseErrorToHandle(webapiDataError.InvalidLogin);
                    }

                    session["memberIdFromDatabase"] = memberData.memberLoginStatusData[0].MemberID;
                    session["memberName"] = objSave.FullName;
                    session["memberEmail"] = objSave.Email;
                    session["loginStatus"] = dgLoginStatus.currentLogin.ToString();

                    session["memberTradeAccountID"] = null;
                    session["memberTradeAccountNumber"] = null;
                    session["memberTradeChecksPassed"] = false;
                    session["memberDisplayTradePrice"] = false;

                    if (session["backInStockReminder"] == null) { }
                    else
                    {
                        if (session["visitorIdFromDatabase"] == null) { }
                        else
                        {
                            await visitorFactory.createBackInStockReminder(visitorId, Convert.ToInt32(session["backInStockReminder"]), objCountryItem.CountryID, objCountryItem.LanguageID);
                        }
                    }
                }

                return earlyResult;
            }

            //  Get the form details for validation purposes
            var objLandingPage = new cmsLandingPage();
			Models.Classes.cmsLandingPageDefinition objPageData = await objLandingPage.loadLandingPageFromCms(memberId,
                                                                                                objSave.ContentID,
                                                                                                objCountryItem.LanguageID,
                                                                                                objCountryItem.CountryID,
                                                                                                useDate,
                                                                                                session["memberIdFromDatabase"] != null,
                                                                                                session["memberTradeAccountID"] != null,
																								Convert.ToBoolean(HttpContext.Current.Session["countryCheckMatched"]),
                                                                                                session["memberDisplayTradePrice"] != null && Convert.ToBoolean(session["memberDisplayTradePrice"]),
                                                                                                session["memberTradeAccountID"] == null ? (int?)null : Convert.ToInt32(session["memberTradeAccountID"]),
                                                                                                null,
                                                                                                null,
                                                                                                null);

            if (objPageData.pageSectionForms.Where(p => p.PageSectionID == objSave.PageSectionID).FirstOrDefault() == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "PageSectionIDFailure"));
            }

            var objFormDefinition = objPageData.pageSectionForms.Where(p => p.PageSectionID == objSave.PageSectionID).First();


            //  Validate email block
            objSave.FullName = objSave.FullName.Trim();
            objSave.Email = objSave.Email.Trim();

            if ((objSave.FullName.Length >= 2) && (objSave.FullName.Length <= 40)) { }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "FullNameFailure"));
            }

            if ((objSave.Email.Length >= 5) && (objSave.Email.Length <= 256)) { }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "EmailFailure"));
            }

            if (dgFramework.emailChecker(objSave.Email)) { }
            else
            {
                return dgFramework.raiseErrorToHandle(webapiDataError.EmailInvalidFormat);
            }

            if (objFormDefinition.ConfirmEmail)
            {
                objSave.EmailConfirmation = objSave.EmailConfirmation.Trim();

                if (dgFramework.emailChecker(objSave.EmailConfirmation)) { }
                else
                {
                    return dgFramework.raiseErrorToHandle(webapiDataError.EmailConfirmInvalidFormat);
                }

                if (objSave.Email == objSave.EmailConfirmation) { }
                else
                {
                    return dgFramework.raiseErrorToHandle(webapiDataError.EmailConfirmFailed);
                }
            }

            // Validate Interior Design Block

            if (objSave.AppointmentLocation != null)
            {

                objSave.AppointmentLocation = objSave.AppointmentLocation.Trim();
                objSave.AppointmentTime = objSave.AppointmentTime.Trim();

                if ((objSave.AppointmentLocation.Length >= 2) && (objSave.AppointmentLocation.Length <= 40)) { }
                else
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "InteriorDesignShopOutsideRange"));
                }

                // no, the preferred date should be passable directly into MVC with the correct offset for GMT
                // objForm.PreferredDate = DateTime.Parse(objForm.prefDateStr);
            }

            //  Validate address block
            if (objFormDefinition.AddressRequired == true)
            {
                if (objSave.CompanyName != null)
                {
                    objSave.CompanyName = objSave.CompanyName.Trim();
                    if (objSave.CompanyName.Length <= 35) { }
                    else
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "CompanyNameOutsideRange"));
                    }
                }

                if (objSave.AddressLine1 != null)
                {
                    objSave.AddressLine1 = objSave.AddressLine1.Trim();
                    if ((objSave.AddressLine1.Length >= 2) && (objSave.AddressLine1.Length <= 60)) { }
                    else
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "AddressLineOneOutOfRange"));
                    }
                }
                else
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "AddressLineOneOutOfRange"));
                }

                if (objSave.AddressLine2 != null)
                {
                    objSave.AddressLine2 = objSave.AddressLine2.Trim();
                    if (objSave.AddressLine2.Length <= 60) { }
                    else
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "AddressLineTwoOutOfRange"));
                    }
                }

                if (objSave.Town != null)
                {
                    objSave.Town = objSave.Town.Trim();
                    if ((objSave.Town.Length >= 2) && (objSave.Town.Length <= 50)) { }
                    else
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "AddressTownOutOfRange"));
                    }
                }
                else
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "AddressTownOutOfRange"));
                }

                if (objSave.Postcode != null)
                {
                    objSave.Postcode = objSave.Postcode.Trim();
                    if ((objSave.Postcode.Length >= 2) && (objSave.Postcode.Length <= 20)) { }
                    else
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "AddressPostcodeOutOfRange"));
                    }
                }
                else
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "AddressPostcodeOutOfRange"));
                }

                if (objSave.County != null)
                {
                    objSave.County = objSave.County.Trim();
                    if ((objSave.County.Length >= 2) && (objSave.County.Length <= 40)) { }
                    else
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "AddressCountyOutOfRange"));
                    }
                }

                if (objSave.PhoneNumber != null)
                {
                    objSave.PhoneNumber = objSave.PhoneNumber.Trim();
                    if (objSave.PhoneNumber.Length <= 20) { }
                    else
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "AddressPhoneOutOfRange"));
                    }
                }
            }
            else
            {
                objSave.CompanyName = null;
                objSave.AddressLine1 = null;
                objSave.AddressLine2 = null;
                objSave.Town = null;
                objSave.Postcode = null;
                objSave.County = null;

                if (objSave.AppointmentLocation != null || objSave.StoreEventID != null) { }
                else
                {
                    objSave.PhoneNumber = null;
                }
            }

            if (objSave.MarketingCountryID == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "AddressCountryOutOfRange"));
            }

            //  Validate captcha
            if (objFormDefinition.CaptchaRequired == true)
            {
                if (String.IsNullOrEmpty(objSave.gRecaptcha))
                {
                    return dgFramework.raiseErrorToHandle(webapiDataError.InvalidCaptcha);
                }
                else
                {
                    if (ReCaptchaVerifier.Validate(objSave.gRecaptcha) == "true")
                    {

                    }
                    else
                    {
                        return dgFramework.raiseErrorToHandle(webapiDataError.InvalidCaptcha);
                    }
                }
            }

            if (objSave.PasswordRequired && objSave.Password != null)
            {
                var visitorFactory = new VisitorFactory();
                objSave.Password = await visitorFactory.generatePassword(objSave.Password);
            }

            formRoutines objFormRoutines = new formRoutines();

            var result = await objFormRoutines.pageSectionFormSave(objSave, visitorId, memberId);

            return result;
        }

        [HttpPost]
        public async Task<IHttpActionResult> JoinMarketingListPopup([FromBody] PassMarketingListJoin dto)
        {
            var db = new databaseDataContext();

            dto.PreferenceEmailMarketingFrequencyID = 1; // ALL THE EMAILS!

            _ = db.JoinMarketingList(dto, source: "Popup");

            return Ok(new { });
        }

        [HttpPost]
        public async Task<IHttpActionResult> RejoinMarketingListReceipt([FromBody] PassGUID dto)
        {
            var db = new databaseDataContext();

            _ = db.RejoinMarketingList(dto, source: "Receipt");
            
            return Ok(new { });
        }

        [HttpPost]
        public async Task<IHttpActionResult> RejoinMarketingListUnsubscribe([FromBody] PassGUID dto)
        {
            var db = new databaseDataContext();

            var vf = new VisitorFactory();

            if (Guid.TryParse(dto.GUID, out Guid g))
            {
                var fetchResult = await vf.getMemberByGuid(g);

                dto.Email = fetchResult.Email;

                _ = db.RejoinMarketingList(dto, source: "Unsubscribe");
            }

            return Ok(new { });
        }

        [HttpPost]
        public async Task<IHttpActionResult> RejoinMarketingListCleanse([FromBody] PassGUID dto)
        {
            var db = new databaseDataContext();

            var vf = new VisitorFactory();

            if (Guid.TryParse(dto.GUID, out Guid g))
            {
                var fetchResult = await vf.getMemberByGuid(g);

                dto.Email = fetchResult.Email;

                _ = db.RejoinMarketingList(dto, source: "Cleanse");
            }

            return Ok(new { });
        }

        [HttpPost]
        public async Task<IHttpActionResult> LeaveMarketingListCleanse([FromBody] PassGUID dto)
        {
            var db = new databaseDataContext();

            var vf = new VisitorFactory();

            if (Guid.TryParse(dto.GUID, out Guid g))
            {
                var fetchResult = await vf.getMemberByGuid(g);

                dto.Email = fetchResult.Email;

                _ = db.LeaveMarketingList(dto, source: "Cleanse");
            }

            return Ok(new { });
        }

        [HttpPost]
        public async Task<IHttpActionResult> LeaveMarketingListUnsubscribe([FromBody] PassGUID dto)
        {
            var db = new databaseDataContext();

            var vf = new VisitorFactory();

            if (Guid.TryParse(dto.GUID, out Guid g))
            {
                var fetchResult = await vf.getMemberByGuid(g);

                dto.Email = fetchResult.Email;

                _ = db.LeaveMarketingList(dto, source: "Unsubscribe");
            }

            return Ok(new { });
        }
    }
}
