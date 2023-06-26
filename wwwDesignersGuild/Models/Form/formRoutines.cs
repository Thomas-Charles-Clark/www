using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Threading.Tasks;
using System.Data;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Interface;


namespace wwwDesignersGuild.Models.Form
{


    public class formRoutines : IForm
    {
        public async Task<long?> saveTradeAccountRequestForm(long visitorId, webapiTradeAccountRequestFormSave objFormDetails, byte[] usSalesTaxExemptionCertificate = null, string usSalesTaxExemptionCertificateFileType = null)
        {
            long? result = await Task.FromResult<long?>(saveTradeAccountRequestFormAsync(visitorId, objFormDetails, usSalesTaxExemptionCertificate, usSalesTaxExemptionCertificateFileType));

            return result;
        }

        private long? saveTradeAccountRequestFormAsync(long visitorId, webapiTradeAccountRequestFormSave objFormDetails, byte[] usSalesTaxExemptionCertificate = null, string usSalesTaxExemptionCertificateFileType = null)
        {
            formDb objDbForm = new formDb();

            return objDbForm.SaveTradeAccountRequestForm(visitorId, objFormDetails, usSalesTaxExemptionCertificate, usSalesTaxExemptionCertificateFileType);
        }

        public async Task<bool> saveLeadImmediateOrderFlag(long leadId)
        {
            bool result = await Task.FromResult<bool>(saveLeadImmediateOrderFlagAsync(leadId));

            return result;
        }

        private bool saveLeadImmediateOrderFlagAsync(long leadId)
        {
            formDb objDbForm = new formDb();

            return objDbForm.SaveImmediateOrderFlag(leadId);
        }

        internal async Task<bool> unsubscribeMember(MemberUnsubscribe objUnsub)
        {
            var db = new databaseDataContext();

            var result = await Task.FromResult<bool>(db.UnsubscribeMember(objUnsub));

            return result;
        }

        internal async Task<bool> beginMemberResubscribe(Guid guid)
        {
            var db = new databaseDataContext();

            var result = await Task.FromResult<bool>(db.BeginMemberResubscribe(guid));

            return result;
        }

        internal async Task<bool> resubscribeMember(Guid guid)
        {
            var db = new databaseDataContext();

            var result = await Task.FromResult<bool>(db.ResubscribeMember(guid));

            return result;
        }

        internal async Task<MarketingPermissionResult> marketingPermissionCheck(webapiMarketingPermissionCheck objCheck)
        {
            var db = new databaseDataContext();

            var result = await Task.FromResult<MarketingPermissionResult>(db.MarketingPermissionCheck(objCheck));

            return result;
        }

        internal async Task<bool> pageSectionFormSave(pageSectionFormSave objSave)
        {
            var db = new databaseDataContext();

            var result = await Task.FromResult<bool>(db.PageSectionFormSave(objSave));

            return result;
        }

        internal async Task<bool> pageSectionFormSave(PassFormSave objSave, long visitorId, int? memberId)
        {
            var db = new databaseDataContext();

            var result = await Task.FromResult<bool>(db.PageSectionFormSave(objSave, visitorId, memberId));

            return result;
        }

        internal async Task<bool> saveMyAccount(webapiMyAccountSave objSave)
        {
            var db = new databaseDataContext();

            var result = await Task.FromResult<bool>(db.MyAccountSave(objSave));

            return result;
        }
    }
}