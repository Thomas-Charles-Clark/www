using DevExpress.Web.Mvc;
using iText.Html2pdf;
using MertexerV2.Accounts;
using MertexerV2.Models.Accounts;
using MertexerV2SQL.Accounts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Search;
using wwwDesignersGuild.Models.Trade;
using wwwDesignersGuild.Models.Visitor;
using wwwDesignersGuild.Util;
using wwwDesignersGuild.ViewModels;
using wwwDesignersGuild.ViewModels.Trade;

namespace wwwDesignersGuild.Controllers
{
    public class TradeController : Controller
    {
        public async Task<ActionResult> TradeRegistration(BaseViewModel objBase)
        {
            var Model = await TradeRegistrationViewModel.CreateAsync(objBase);

            return PartialView("~/Views-New/LandingPages/Trade/TradeRegistration.cshtml", Model);
        }

        public async Task<ActionResult> TradeAccountRequest(BaseViewModel objBase)
        {
            var objTradeAccountRequest = new cmsTradeAccountRequest();

            var Model = await objTradeAccountRequest.loadTradeAccountRequestFormFromCms(objBase);

            Model.immediateOrderRequested = false;

            if (Int64.TryParse(Request.QueryString["id"], out _))
            {
                Model.leadId = Convert.ToInt64(Request.QueryString["id"]);

                Model.immediateOrderRequested = await objTradeAccountRequest.isImmediateOrderFlagSet(Model.leadId.Value);
            }

            return PartialView("~/Views-New/LandingPages/Trade/TradeAccountRequest.cshtml", Model);
        }

        public ActionResult TradeRequestAccess(BaseViewModel objBase)
        {
            var Model = new TradeRequestAccessViewModel() 
            { 
                BaseViewModel = objBase
            };

            return PartialView("~/Views-New/LandingPages/Trade/TradeRequestAccess.cshtml", Model);
        }

        public async Task<ActionResult> TradeLoginDetails(BaseViewModel objBase)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            var useDate = Framework.GetUseDate(HttpContext.Request);

            var visitorFactory = new VisitorFactory();

            var memberStore = await visitorFactory.memberDetails(loginCheck.visitorId.Value, useDate);

            var Model = new TradeLoginDetailsViewModel()
            {
                BaseViewModel = objBase,
                MemberDetail = memberStore.MemberDetail
            };

            return PartialView("~/Views-New/LandingPages/Trade/TradeLoginDetails.cshtml", Model);
        }

        public async Task<ActionResult> TradePricingDisplay(BaseViewModel objBase, string heading)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            var useDate = Framework.GetUseDate(HttpContext.Request);

            var visitorFactory = new VisitorFactory();

            var memberStore = await visitorFactory.memberDetails(loginCheck.visitorId.Value, useDate);

            var Model = new TradePriceDisplayViewModel()
            {
                BaseViewModel = objBase,
                MemberDetail = memberStore.MemberDetail
            };

            return PartialView("~/Views-New/LandingPages/Trade/TradePriceDisplay.cshtml", Model);
        }

        public async Task<ActionResult> TradeUserManagement(BaseViewModel objBase, string heading)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            var useDate = Framework.GetUseDate(HttpContext.Request);

            var visitorFactory = new VisitorFactory();

            var memberStore = await visitorFactory.memberDetails(loginCheck.visitorId.Value, useDate);

            var tradeAccountModel = await visitorFactory.callTradeMemberSelect(
                    loginCheck.tradeAccountId.Value,
                    loginCheck.memberId.Value,
                    objBase.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                    objBase.MasterDefinition.UrlBreakdown.countryDetails.LanguageID);

            var Model = new TradeUserManagementViewModel()
            {
                BaseViewModel = objBase,
                MemberDetail = memberStore.MemberDetail,
                TradeAccountModel = tradeAccountModel,
                Permission = tradeAccountModel.Administrators.Select(q => q.MemberID).Contains(loginCheck.memberId.Value)
            };

            return PartialView("~/Views-New/LandingPages/Trade/TradeUserManagement.cshtml", Model);
        }
        
        public async Task<ActionResult> TradeContactUs(BaseViewModel objBase)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            var useDate = Framework.GetUseDate(HttpContext.Request);

            var visitorFactory = new VisitorFactory();

            var contactDetails = await visitorFactory.generateTradeContactUs(loginCheck.tradeAccountId.Value);

            var Model = new TradeContactUsViewModel()
            {
                BaseViewModel = objBase,
                ContactDetails = contactDetails
            };

            return PartialView("~/Views-New/LandingPages/Trade/TradeContactUs.cshtml", Model);
        }

        public async Task<ActionResult> TradeAccountInvoices(BaseViewModel objBase, string heading)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            var vf = new VisitorFactory();

            var tradeAccountModel = await vf.callTradeMemberSelect(
                    loginCheck.tradeAccountId.Value,
                    loginCheck.memberId.Value,
                    objBase.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                    objBase.MasterDefinition.UrlBreakdown.countryDetails.LanguageID);

            var user = tradeAccountModel.Users.Where(p => p.MemberID == loginCheck.memberId).SingleOrDefault();
            var admin = tradeAccountModel.Administrators.Where(p => p.MemberID == loginCheck.memberId).SingleOrDefault();

            var permission = admin?.TradePermissionViewAccounts ?? user.TradePermissionViewAccounts;

            AccountSummaryReturn accountSummary = null;
            ListInvoicesReturn outstandingInvoices = null;
            AccountSummaryRecentPaymentsReturn recentPayments = null;

            var retryCount = 0;

            while (accountSummary == null && retryCount < 3)
            {
                try
                {
                    accountSummary = AccountFunctions.AccountSummary(loginCheck.tradeDataSourceJson);
                }
                catch (Exception)
                {
                    retryCount++;
                }
            }

            retryCount = 0;

            while (outstandingInvoices == null && retryCount < 3)
            {
                try
                {
                    outstandingInvoices = AccountFunctions.OutstandingInvoices(loginCheck.tradeDataSourceJson);
                }
                catch (Exception)
                {
                    retryCount++;
                }
            }

            retryCount = 0;

            while (recentPayments == null && retryCount < 3)
            {
                try
                {
                    recentPayments = AccountFunctions.RecentPayments(loginCheck.tradeDataSourceJson);
                }
                catch (Exception)
                {
                    retryCount++;
                }
            }

            var Model = new TradeAccountsInvoicesViewModel(
                objBase, 
                heading,
                permission,
                tradeAccountModel,
                accountSummary,
                outstandingInvoices,
                recentPayments);

            return PartialView("~/Views-New/LandingPages/Trade/TradeAccountInvoices.cshtml", Model);
        }
        
        public async Task<ActionResult> TradeAccountProformaInvoices(BaseViewModel objBase, string heading)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            var vf = new VisitorFactory();

            var tradeAccountModel = await vf.callTradeMemberSelect(
                    loginCheck.tradeAccountId.Value,
                    loginCheck.memberId.Value,
                    objBase.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                    objBase.MasterDefinition.UrlBreakdown.countryDetails.LanguageID);

            var user = tradeAccountModel.Users.Where(p => p.MemberID == loginCheck.memberId).SingleOrDefault();
            var admin = tradeAccountModel.Administrators.Where(p => p.MemberID == loginCheck.memberId).SingleOrDefault();

            var permission = admin?.TradePermissionViewAccounts ?? user.TradePermissionViewAccounts;

            var Model = new TradeAccountsProformaInvoicesViewModel(
                objBase,
                heading,
                permission,
                tradeAccountModel,
                AccountFunctions.ProformaInvoices(loginCheck.tradeDataSourceJson, new PassProformaInvoiceSearch() { 
                    dateRange = 12
                }));

            return PartialView("~/Views-New/LandingPages/Trade/TradeAccountProformaInvoices.cshtml", Model);
        }

        public async Task<ActionResult> TradeAccountOrderStatus(BaseViewModel objBase)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            var vf = new VisitorFactory();

            var tradeAccountModel = await vf.callTradeMemberSelect(
                    loginCheck.tradeAccountId.Value,
                    loginCheck.memberId.Value,
                    objBase.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                    objBase.MasterDefinition.UrlBreakdown.countryDetails.LanguageID);

            var user = tradeAccountModel.Users.Where(p => p.MemberID == loginCheck.memberId.Value).SingleOrDefault();
            var admin = tradeAccountModel.Administrators.Where(p => p.MemberID == loginCheck.memberId.Value).SingleOrDefault();

            var tradePermissionPlaceOrder = user?.TradePermissionPlaceOrder ?? admin?.TradePermissionPlaceOrder ?? false;

            var Model = new TradeAccountOrderStatusModel(
                objBase,
                tradePermissionPlaceOrder,
                tradeAccountModel);

            return PartialView("~/Views-New/LandingPages/Trade/TradeAccountOrderStatus.cshtml", Model);
        }

        public async Task<ActionResult> TradeDownloads(BaseViewModel objBase)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            var vf = new VisitorFactory();

            var tradeAccountModel = await vf.callTradeMemberSelect(
                    loginCheck.tradeAccountId.Value,
                    loginCheck.memberId.Value,
                    objBase.CountryID,
                    objBase.LanguageID);

            var user = tradeAccountModel.Users.Where(p => p.MemberID == loginCheck.memberId).SingleOrDefault();
            var admin = tradeAccountModel.Administrators.Where(p => p.MemberID == loginCheck.memberId).SingleOrDefault();

            var permission = admin?.TradePermissionDownloadPricelistsBrochures ?? user.TradePermissionDownloadPricelistsBrochures;

            var tradePriceListCode = new databaseDataContext().getTradeAccountPriceListCode(loginCheck.tradeAccountId.Value);

            var viewModel = await TradeDownloadsViewModel.CreateAsync(
                permission,
                $"\\\\{Framework.WebServerHostname}\\d$\\wwwroot\\Trade\\Downloads\\MERTEX\\{tradePriceListCode}\\",
                tradeAccountModel,
                objBase);

            return PartialView("~/Views-New/LandingPages/Trade/TradeDownloads.cshtml", viewModel);
        }

        [ValidateInput(false)]
        public async Task<ActionResult> TradeDownloadFileManagerDataBindingPartial(NewMasterDefinition objMaster)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            var vf = new VisitorFactory();

            var tradeAccountModel = await vf.callTradeMemberSelect(
                    loginCheck.tradeAccountId.Value,
                    loginCheck.memberId.Value,
                    objMaster.UrlBreakdown.countryDetails.CountryID,
                    objMaster.UrlBreakdown.countryDetails.LanguageID);

            var user = tradeAccountModel.Users.Where(p => p.MemberID == loginCheck.memberId).SingleOrDefault();
            var admin = tradeAccountModel.Administrators.Where(p => p.MemberID == loginCheck.memberId).SingleOrDefault();

            var permission = admin?.TradePermissionDownloadPricelistsBrochures ?? user.TradePermissionDownloadPricelistsBrochures;

            var tradePriceListCode = new databaseDataContext().getTradeAccountPriceListCode(loginCheck.tradeAccountId.Value);

            var viewModel = await TradeDownloadsViewModel.CreateAsync(
                permission,
                $"\\\\{Framework.WebServerHostname}\\d$\\wwwroot\\Trade\\Downloads\\MERTEX\\{tradePriceListCode}\\",
                tradeAccountModel,
                objMaster);

            var tradeAccountId = Convert.ToInt32(Session["memberTradeAccountID"]);

            return PartialView("~/Views-New/LandingPages/Trade/TradeDownloadFileManagerPartial.cshtml", viewModel);
        }

        [ValidateInput(false)]
        public FileStreamResult TradeDownloadFileManagerDownloadFile(short countryId, byte languageId)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            var vf = new VisitorFactory();
             
            var tradeAccountModel = vf.callTradeMemberSelect(
                    loginCheck.tradeAccountId.Value,
                    loginCheck.memberId.Value,
                    countryId,
                    languageId).Result;

            var user = tradeAccountModel.Users.Where(p => p.MemberID == loginCheck.memberId).SingleOrDefault();
            var admin = tradeAccountModel.Administrators.Where(p => p.MemberID == loginCheck.memberId).SingleOrDefault();

            var permission = admin?.TradePermissionDownloadPricelistsBrochures ?? user.TradePermissionDownloadPricelistsBrochures;

            var tradePriceListCode = new databaseDataContext().getTradeAccountPriceListCode(loginCheck.tradeAccountId.Value);

            var tradeAccountId = Convert.ToInt32(Session["memberTradeAccountID"]);

            var settings = new TradeDownloadFileManagerSettings($"\\\\{Framework.WebServerHostname}\\d$\\wwwroot\\Trade\\Downloads\\MERTEX\\{tradePriceListCode}\\");

            return FileManagerExtension.DownloadFiles(TradeDownloadFileManagerSettings.CreateFileManagerDownloadSettings(), settings.RootFolder);

            //return PartialView("~/Views/Trade/_tradeDownloadFileManagerPartial.cshtml", new TradeDownloadFileSystemProvider(Server.MapPath("~/TradeDownloads/")));
        }

        public ActionResult ViewDocument(string docref, short c, byte l)
        {
            ViewBag.Title = $"{dgCrypto.Decrypt(docref)} | Designers Guild";

            docref = HttpUtility.UrlEncode(docref);

            ViewBag.Contents = $"/downloads/trade-document-fetch?docref={docref}&c={c}&l={l}";
            
            ViewBag.EmailLink = $"/downloads/trade-document-email?docref={docref}&c={c}&l={l}";

            ViewBag.Email = HtmlHelperExtra.Resource("email", l, c, DateTime.Now).ToString();
            
            ViewBag.UserEmail = Session["memberEmail"].ToString();

            ViewBag.Print = HtmlHelperExtra.Resource("print", l, c, DateTime.Now).ToString();

            ViewBag.PleaseWait = HtmlHelperExtra.Resource("PleaseWait", l, c, DateTime.Now).ToString();

            ViewBag.EmailSentTo = HtmlHelperExtra.Resource("ThisDocumentEmailedTo", l, c, DateTime.Now);

            return View("~/Views-New/LandingPages/Trade/TradeDocumentWrapper.cshtml");
        }

        public ActionResult FetchDocument(string docref)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            if (loginCheck == null)
            {
                return Content("<h3>Not Authorised. Please sign in again.</h3>");
            }

            if (loginCheck.tradeAccountId == null || loginCheck.tradeDataSourceJson == null)
            {
                return Content("<h3>Not Authorised. Please sign in again.</h3>");
            }

            var docrefdecrypted = dgCrypto.Decrypt(docref);

            if (String.IsNullOrEmpty(docrefdecrypted))
            {
                return Content("<h3>Error decrypting document reference.</h3>");
            }

            return Content("<br/>" + AccountFunctions.ViewDocument(loginCheck.tradeDataSourceJson, docrefdecrypted)
                .Replace("<TITLE>HTML document</TITLE>", $"<title>{docrefdecrypted}</title>")
                .Replace("<img", "<img style='width: 284px'")
                .Replace("../../../images/BRANDS/DESIGNERSGUILD.GIF", "/content/images/logo/designers_guild_logo.png"));
        }

        public ActionResult EmailDocument(string docref)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            if (loginCheck == null)
            {
                return Content("<h3>Not Authorised. Please sign in again.</h3>");
            }

            if (loginCheck.tradeAccountId == null || loginCheck.tradeDataSourceJson == null)
            {
                return Content("<h3>Not Authorised. Please sign in again.</h3>");
            }

            var email = Session["memberEmail"].ToString();

            Task.Run(() =>
            {
                var docrefdecrypted = dgCrypto.Decrypt(docref);

                var body = AccountFunctions.ViewDocument(loginCheck.tradeDataSourceJson, docrefdecrypted)
                    .Replace("<TITLE>HTML document</TITLE>", $"<title>{docrefdecrypted}</title>")
                    .Replace("<img", "<img style='width: 284px'")
                    .Replace("../../../images/BRANDS/DESIGNERSGUILD.GIF", "https://www.designersguild.com/content/images/logo/designers_guild_logo.png");

                body = System.Text.RegularExpressions.Regex.Replace(body, "(\\d)pt", "$1px");

                var cssRegex = new System.Text.RegularExpressions.Regex($@"(<STYLE TYPE=""text\/css"">)(.*)(<\/STYLE>)");

                var matches = cssRegex.Matches(body);

                var fileNameWithExtension = $"{docrefdecrypted}.pdf";

                //var css = PdfGenerator.ParseStyleSheet(matches[0].Groups[2].Value, false);

                //var css = PdfGenerator.ParseStyleSheet(matches[0].Value, true);
                //var pdfDocument = PdfGenerator.GeneratePdf(body, PageSize.A4, 20, css);

                var fullUNCFilePath = $"\\\\vm-sql2\\d$\\EmailAttachments\\{fileNameWithExtension}";

                //pdfDocument.Save(fullUNCFilePath);

                using (var fs = new FileStream(fullUNCFilePath, FileMode.Create))
                {
                    HtmlConverter.ConvertToPdf(body, fs);
                }

#if RELEASE
                var db = new visitorDb();

                db.addEmailToQueue(
                    destination: email,
                    cc: null,
                    bcc: "itsupport@designersguild.com",
                    subject: $"Document {docrefdecrypted} - Designers Guild",
                    body: body,
                    emailTemplateId: 1,
                    emailSourceId: 14,
                    emailTypeId: 2,
                    attachments: fullUNCFilePath);
#endif

            });

            return Content("");
        }

        public ActionResult ViewProformaDocument(string docref, short c, byte l)
        {
            ViewBag.Title = $"{dgCrypto.Decrypt(docref)} | Designers Guild";

            docref = HttpUtility.UrlEncode(docref);

            ViewBag.Contents = $"/downloads/trade-proforma-fetch?docref={docref}&c={c}&l={l}";

            ViewBag.EmailLink = $"/downloads/trade-proforma-email?docref={docref}&c={c}&l={l}";

            ViewBag.Email = Session["memberEmail"].ToString();

            ViewBag.Print = HtmlHelperExtra.Resource("print", l, c, DateTime.Now).ToString();

            ViewBag.PleaseWait = HtmlHelperExtra.Resource("PleaseWait", l, c, DateTime.Now).ToString();

            ViewBag.EmailSentTo = HtmlHelperExtra.Resource("ThisDocumentEmailedTo", l, c, DateTime.Now);

            return View("~/Views-New/LandingPages/Trade/TradeDocumentWrapper.cshtml");
        }

        public ActionResult FetchProformaDocument(string docref)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            if (loginCheck == null)
            {
                return Content("<h3>Not Authorised. Please sign in again.</h3>");
            }

            if (loginCheck.tradeAccountId == null || loginCheck.tradeDataSourceJson == null)
            {
                return Content("<h3>Not Authorised. Please sign in again.</h3>");
            }

            var docrefdecrypted = dgCrypto.Decrypt(docref);

            return Content("<br/>" + AccountFunctions.ViewProformaInvoice(loginCheck.tradeDataSourceJson, docrefdecrypted)
                .Replace("<TITLE>HTML document</TITLE>", $"<title>{docrefdecrypted}</title>")
                .Replace("<img", "<img style='width: 284px'")
                .Replace("../../../images/BRANDS/DESIGNERSGUILD.GIF", "/content/images/logo/designers_guild_logo.png"));
        }

        public ActionResult EmailProformaDocument(string docref)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            if (loginCheck == null)
            {
                return Content("<h3>Not Authorised. Please sign in again.</h3>");
            }

            if (loginCheck.tradeAccountId == null || loginCheck.tradeDataSourceJson == null)
            {
                return Content("<h3>Not Authorised. Please sign in again.</h3>");
            }

            var email = Session["memberEmail"].ToString();

            Task.Run(() =>
            {
                var docrefdecrypted = dgCrypto.Decrypt(docref);

                var body = AccountFunctions.ViewDocument(loginCheck.tradeDataSourceJson, docrefdecrypted)
                    .Replace("<TITLE>HTML document</TITLE>", $"<title>{docrefdecrypted}</title>")
                    .Replace("<img", "<img style='width: 284px'")
                    .Replace("../../../images/BRANDS/DESIGNERSGUILD.GIF", "https://www.designersguild.com/content/images/logo/designers_guild_logo.png");

                var cssRegex = new System.Text.RegularExpressions.Regex($@"(<style type=""text/css"">)(.*)(</style>)");

                var matches = cssRegex.Matches(body);

                var fileNameWithExtension = $"{docrefdecrypted}.pdf";
                //var css = PdfGenerator.ParseStyleSheet(matches[1].Value, false);
                //var pdfDocument = PdfGenerator.GeneratePdf(body, PageSize.A4, 20, css);
                var fullUNCFilePath = $"\\\\vm-sql2\\d$\\EmailAttachments\\{fileNameWithExtension}";

                //pdfDocument.Save(fullUNCFilePath);

                using (var fs = new FileStream(fullUNCFilePath, FileMode.Create))
                {
                    HtmlConverter.ConvertToPdf(body, fs);
                }

#if RELEASE
                var db = new visitorDb();

                db.addEmailToQueue(
                    destination: email,
                    cc: null,
                    bcc: "itsupport@designersguild.com",
                    subject: $"Document {docrefdecrypted} - Designers Guild",
                    body: body,
                    emailTemplateId: 1,
                    emailSourceId: 14,
                    emailTypeId: 2,
                    attachments: fullUNCFilePath);
#endif
            });

            return Content("");
        }


        public async Task<ActionResult> TradeOrderImport(BaseViewModel objBase, string heading)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            var useDate = Framework.GetUseDate(HttpContext.Request);

            var visitorFactory = new VisitorFactory();
            
            var tradeAccountModel = await visitorFactory.callTradeMemberSelect(
                    loginCheck.tradeAccountId.Value,
                    loginCheck.memberId.Value,
                    objBase.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                    objBase.MasterDefinition.UrlBreakdown.countryDetails.LanguageID);
            
            var user = tradeAccountModel.Users.Where(p => p.MemberID == loginCheck.memberId.Value).SingleOrDefault();
            var admin = tradeAccountModel.Administrators.Where(p => p.MemberID == loginCheck.memberId.Value).SingleOrDefault();

            var tradePermissionPlaceOrder = user?.TradePermissionPlaceOrder ?? admin?.TradePermissionPlaceOrder ?? false;

            // Still need to figure out how to identify whether or not the trade account is a distributor...
            var Model = new TradeOrderImportViewModel(tradeAccountModel, objBase, heading, tradePermissionPlaceOrder, false);
            
            return PartialView("~/Views-New/LandingPages/Trade/TradeOrderImport.cshtml", Model);
        }

        public async Task<ActionResult> TradeAccountAddressBook(BaseViewModel objBase, string heading)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            var useDate = Framework.GetUseDate(HttpContext.Request);

            var visitorFactory = new VisitorFactory();

            var tradeAccountModel = await visitorFactory.callTradeMemberSelect(
                    loginCheck.tradeAccountId.Value,
                    loginCheck.memberId.Value,
                    objBase.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                    objBase.MasterDefinition.UrlBreakdown.countryDetails.LanguageID);

            var emailsModel = AccountFunctionsUniSQL.TradeEmails(loginCheck.tradeDataSourceJson);

            var user = tradeAccountModel.Users.Where(p => p.MemberID == loginCheck.memberId.Value).SingleOrDefault();
            var admin = tradeAccountModel.Administrators.Where(p => p.MemberID == loginCheck.memberId.Value).SingleOrDefault();

            var tradePermissionAccountAdmin = user?.TradePermissionAccountAdmin ?? admin?.TradePermissionAccountAdmin ?? false;

            var invoiceAddress = AccountFunctionsUniSQL.InvoiceAddress(loginCheck.tradeDataSourceJson);

            invoiceAddress.Email = null;

            var objCountry = new cmsCountry();
            var objCountryData = await objCountry.countryLanguageSelect(useDate);

            var invoiceCountry = objCountryData.countryItems.Where(p => p.CountryCode == invoiceAddress.Country).SingleOrDefault();

            if (invoiceCountry != null)
            {
                invoiceAddress.CountryID = invoiceCountry.CountryID;
            }

            var objDefinition = await new VisitorFactory().getMemberAddresses(objBase.Visitor.visitorDbId,
                null,
                objBase.LanguageID,
                objBase.CountryID,
                objBase.Date);

            var viewModel = await TradeAddressBookManagementViewModel.CreateAsync(
                objDefinition,
                tradeAccountModel,
                invoiceAddress,
                tradePermissionAccountAdmin,
                objBase);

            return PartialView("~/Views-New/LandingPages/Trade/TradeAccountAddressBook.cshtml", viewModel);
        }

        public async Task<ActionResult> TradeAccountContactDetails(BaseViewModel objBase, string heading)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            var useDate = Framework.GetUseDate(HttpContext.Request);

            var visitorFactory = new VisitorFactory();

            var tradeAccountModel = await visitorFactory.callTradeMemberSelect(
                    loginCheck.tradeAccountId.Value,
                    loginCheck.memberId.Value,
                    objBase.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                    objBase.MasterDefinition.UrlBreakdown.countryDetails.LanguageID);

            var emailsModel = AccountFunctionsUniSQL.TradeEmails(loginCheck.tradeDataSourceJson);

            var user = tradeAccountModel.Users.Where(p => p.MemberID == loginCheck.memberId.Value).SingleOrDefault();
            var admin = tradeAccountModel.Administrators.Where(p => p.MemberID == loginCheck.memberId.Value).SingleOrDefault();

            var tradePermissionAccountAdmin = user?.TradePermissionAccountAdmin ?? admin?.TradePermissionAccountAdmin ?? false;

            var invoiceAddress = AccountFunctionsUniSQL.InvoiceAddress(loginCheck.tradeDataSourceJson);

            var objCountry = new cmsCountry();
            var objCountryData = await objCountry.countryLanguageSelect(useDate);

            var invoiceCountry = objCountryData.countryItems.Where(p => p.CountryCode == invoiceAddress.Country).SingleOrDefault();

            if (invoiceCountry  != null)
            {
                invoiceAddress.CountryID = invoiceCountry.CountryID;
            }

            var Model = new TradeAccountContactDetailsViewModel(
                invoiceAddress, 
                emailsModel, 
                tradeAccountModel,
                objBase, 
                heading, 
                tradePermissionAccountAdmin,
                AccountFunctionsUniSQL.GetDefaultDeliveryReference(loginCheck.tradeDataSourceJson));

            return PartialView("~/Views-New/LandingPages/Trade/TradeAccountContactDetails.cshtml", Model);
        }

        public ActionResult TradeAccountClearancePricingCheck(NewMasterDefinition objMaster)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            return PartialView("~/Views-New/LandingPages/Trade/TradeClearancePricingCheck.cshtml", new tradeClearancePricingCheckViewModel() 
            {
                MasterDefinition = objMaster
            });
        }

        public ActionResult TradeClearanceJavascript(NewMasterDefinition objMaster)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            return PartialView("~/Views-New/LandingPages/Trade/TradeClearanceJavascript.cshtml", objMaster);
        }

        public async Task<ActionResult> TradeQuickOrdering(BaseViewModel objBase)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session); // as of 20/06/2022, this should be a defunct check.
            // The user is forced to be logged in before these leaf actions are called.

            var visitorFactory = new VisitorFactory();

            var tradeAccountModel = await visitorFactory.callTradeMemberSelect(
                    loginCheck.tradeAccountId.Value,
                    loginCheck.memberId.Value,
                    objBase.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                    objBase.MasterDefinition.UrlBreakdown.countryDetails.LanguageID);

            var user = tradeAccountModel.Users.Where(p => p.MemberID == loginCheck.memberId.Value).SingleOrDefault();

            var admin = tradeAccountModel.Administrators.Where(p => p.MemberID == loginCheck.memberId.Value).SingleOrDefault();

            var orderPermission = user?.TradePermissionPlaceOrder ?? admin?.TradePermissionPlaceOrder ?? false;

            var samplePermission = user?.TradePermissionRequestSample ?? admin?.TradePermissionRequestSample ?? false;

            var permission = orderPermission || samplePermission;

            var viewModel = await TradeQuickOrderingViewModel.CreateAsync(permission, tradeAccountModel, objBase);

            return PartialView("~/Views-New/LandingPages/Trade/TradeQuickOrdering.cshtml", viewModel);
        }

        public async Task<ActionResult> TradeAccountReserveOrders(BaseViewModel objBase, string heading)
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

            var vf = new VisitorFactory();

            var tradeAccountModel = await vf.callTradeMemberSelect(
                    loginCheck.tradeAccountId.Value,
                    loginCheck.memberId.Value,
                    objBase.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                    objBase.MasterDefinition.UrlBreakdown.countryDetails.LanguageID);

            var user = tradeAccountModel.Users.Where(p => p.MemberID == loginCheck.memberId.Value).SingleOrDefault();
            var admin = tradeAccountModel.Administrators.Where(p => p.MemberID == loginCheck.memberId.Value).SingleOrDefault();

            var tradePermissionPlaceOrder = user?.TradePermissionPlaceOrder ?? admin?.TradePermissionPlaceOrder ?? false;

            var Model = new TradeAccountReserveOrdersModel(
                objBase,
                heading,
                tradePermissionPlaceOrder,
                AccountFunctionsUniSQL.InvoiceAddress(loginCheck.tradeDataSourceJson),
                AccountFunctionsUniSQL.GetDefaultDeliveryReference(loginCheck.tradeDataSourceJson),
                tradeAccountModel);

            return PartialView("~/Views-New/LandingPages/Trade/TradeAccountReserveOrders.cshtml", Model);
        }

        public ActionResult downloadClearanceSpreadsheet()
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

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

            var data = new promotionDb().DownloadClearanceList(loginCheck.tradeAccountId.Value, cl.CountryID, cl.LanguageID, productPageIds: null, xlsx: true);

            var unique = Guid.NewGuid().ToString();

            var dirPath = Server.MapPath("~/ExcelSpreadsheets/");

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            var filePath = Server.MapPath("~/ExcelSpreadsheets/" + unique + ".xlsx");

            var products = data.Document.Element("result").Element("Products").Elements("Product").ToList();

            var stocks = data.Document.Element("result").Element("Stocks")?.Elements("Stock").ToList() ?? new List<XElement>();

            Util.SpreadsheetHelper.WriteClearanceSpreadsheet(filePath, products, stocks);

            var f = System.IO.File.OpenRead(filePath);

            var n = $"DesignersGuild-Clearance-{DateTime.Now:dd-MMM-yyyy}.xlsx";

            FileStreamResult result = new FileStreamResult(f, MimeMapping.GetMimeMapping(n)) { FileDownloadName = n };
            return result;
        }

        public ActionResult downloadClearancePDF()
        {
            var loginCheck = dgFramework.checkMemberLogin(Session);

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

            var data = new promotionDb().DownloadClearanceList(loginCheck.tradeAccountId.Value, cl.CountryID, cl.LanguageID);

            var bytes = ClearancePDFWriter.buildPDF(Request.MapPath("~"), data, Request.Url);

            Response.ContentType = "application/pdf";
            Response.Buffer = false;
            Response.Clear();
            Response.OutputStream.Write(bytes, 0, bytes.Length);
            Response.OutputStream.Flush();
            Response.End();

            return null;
        }

        public ActionResult TradeDownloadExampleCsv ()
        {
            var f = System.IO.File.OpenRead(Server.MapPath("~/TradeDownloads/import-example.csv"));

            var n = $"import-example.csv";

            return  new FileStreamResult(f, MimeMapping.GetMimeMapping(n)) { FileDownloadName = n };
        }

        public ActionResult TradeDownloadStandardImportFormat()
        {
            var f = System.IO.File.OpenRead(Server.MapPath("~/TradeDownloads/standard-file-specification.pdf"));

            var n = $"standard-file-specification.pdf";

            return new FileStreamResult(f, MimeMapping.GetMimeMapping(n)) { FileDownloadName = n };
        }

        public ActionResult TradeDownloadDistributorImportFormat()
        {
            var f = System.IO.File.OpenRead(Server.MapPath("~/TradeDownloads/distributor-file-specification.pdf"));

            var n = $"distributor-file-specification.pdf";

            return new FileStreamResult(f, MimeMapping.GetMimeMapping(n)) { FileDownloadName = n };
        }
    }

    public class tradeClearancePricingCheckViewModel
    {
        public NewMasterDefinition MasterDefinition { get; set; }
    }
}