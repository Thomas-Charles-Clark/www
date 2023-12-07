using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Security.Cryptography;
using System.Security;
using System.Text;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Interface;
using wwwDesignersGuild.Models.Trade;
using wwwDesignersGuild.Controllers;

namespace wwwDesignersGuild.Models.Visitor
{
    public class VisitorFactory : IVisitor
    {


        #region Insert


        public async Task<identificationDefinition> callVisitorInsert(String userAgent, String ipAddress, String culture, String cloudFlareCountryCode, String cloudFlareDevice)
        {
            identificationDefinition storeResults = await Task.FromResult<identificationDefinition>(callVisitorInsertAsync(userAgent, ipAddress, culture, cloudFlareCountryCode, cloudFlareDevice));
            return storeResults;
        }


        private identificationDefinition callVisitorInsertAsync(String userAgent, String ipAddress, String culture, String cloudFlareCountryCode, String cloudFlareDevice)
        {

            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callVisitorInsert(userAgent, ipAddress, culture, cloudFlareCountryCode, cloudFlareDevice);

        }


        #endregion


        #region Select


        public async Task<VisitorSelect> callVisitorSelect(string passGuid, string cfCountryHeader, string cfDevice)
        {
            VisitorSelect objVisitorSelect = await Task.FromResult<VisitorSelect>(callVisitorSelectAsync(passGuid, cfCountryHeader, cfDevice));
            return objVisitorSelect;
        }


        private VisitorSelect callVisitorSelectAsync(string passGuid, string cfCountryHeader, string cfDevice)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callVisitorSelect(passGuid, cfCountryHeader, cfDevice);
        }


        #endregion


        #region Select Culture


        public async Task<String> callVisitorSelectCulture(Int64 visitorDbId)
        {
            String storeResults = await Task.FromResult<String>(callVisitorSelectCultureAsync(visitorDbId));
            return storeResults;
        }


        private String callVisitorSelectCultureAsync(Int64 visitorDbId)
        {

            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callVisitorSelectCulture(visitorDbId);

        }


        #endregion


        #region Visitor Country Select (for Forms)


        public async Task<Int16?> callVisitorCountrySelect(Int64 visitorDbId)
        {
            Int16? storeResults = await Task.FromResult<Int16?>(callVisitorCountrySelectAsync(visitorDbId));
            return storeResults;
        }


        private Int16? callVisitorCountrySelectAsync(Int64 visitorDbId)
        {

            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callVisitorCountrySelect(visitorDbId);

        }


        #endregion


        #region Select Email Exists


        public async Task<int?> callEmailExistsSelect(string passEmail)
        {
            var storeResults = await Task.FromResult<int?>(callEmailExistsSelectAsync(passEmail));
            return storeResults;
        }


        private int? callEmailExistsSelectAsync(string passEmail)
        {

            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callEmailExistsSelect(passEmail);

        }


        public async Task<int?> callGuestCheckoutEmailExistsSelect(string passEmail)
        {
            var storeResults = await Task.FromResult<int?>(callGuestCheckoutEmailExistsSelectAsync(passEmail));
            return storeResults;
        }


        private int? callGuestCheckoutEmailExistsSelectAsync(string passEmail)
        {

            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callGuestCheckoutEmailExistsSelect(passEmail);

        }


        public async Task<int> logEmailExistsCheck(long visitorId, string passEmail)
        {
            var storeResults = await Task.FromResult<int>(logEmailExistsCheckAsync(visitorId, passEmail));
            return storeResults;
        }


        private int logEmailExistsCheckAsync(long visitorId, string passEmail)
        {

            visitorDb objDbVisitor = new visitorDb();
            objDbVisitor.logEmailExistsCheck(visitorId, passEmail);
            return 0;
        }


        public async Task<int> logMarketingPermissionsFailure(long visitorId, webapiLogMarketingPermissionsError objError)
        {
            var storeResults = await Task.FromResult<int>(logMarketingPermissionsFailureAsync(visitorId, objError));
            return storeResults;
        }


        private int logMarketingPermissionsFailureAsync(long visitorId, webapiLogMarketingPermissionsError objError)
        {

            visitorDb objDbVisitor = new visitorDb();
            objDbVisitor.logMarketingPermissionsError(visitorId, objError);
            return 0;
        }
        #endregion


        #region Select Trade Permissions

        public async Task<TradeAccountUser> callTradePermissionsCheck(string email)
        {
            var storeResults = await Task.FromResult<TradeAccountUser>(tradePermissionsCheckAsync(email));
            return storeResults;
        }

        public TradeAccountUser tradePermissionsCheckAsync(string email)
        {
            var objVisitorDb = new visitorDb();
            return objVisitorDb.callTradePermissionsCheck(email);
        }

        #endregion


        #region Select Member Exists

        public async Task<memberCheckResult> callMemberCheckSelect(string passEmail)
        {
            memberCheckResult storeResults = await Task.FromResult<memberCheckResult>(callMemberCheckSelectAsync(passEmail));
            return storeResults;
        }


        private memberCheckResult callMemberCheckSelectAsync(string passEmail)
        {

            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callMemberCheckSelect(passEmail);

        }

        #endregion

        #region Fetch MemberID from Email (regardless of registration state)

        public async Task<memberFetchResult> callMemberFetch(string passEmail)
        {
            memberFetchResult storeResults = await Task.FromResult(callMemberFetchAsync(passEmail));
            return storeResults;
        }


        private memberFetchResult callMemberFetchAsync(string passEmail)
        {

            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callMemberFetch(passEmail);

        }

        #endregion


        #region Country Awareness Prompt Check
        public async Task<countryAwarenessCheckReturn> countryAwarenessCheck(Int64 visitorDbId, short countryId, DateTime contentDate, short? countryIdOverride)
        {
            countryAwarenessCheckReturn checkResult = await Task.FromResult<countryAwarenessCheckReturn>(callVisitorCountryAwarenessCheckAsync(visitorDbId, countryId, contentDate, countryIdOverride));

            return checkResult;
        }

        private countryAwarenessCheckReturn callVisitorCountryAwarenessCheckAsync(Int64 visitorDbId, short countryId, DateTime contentDate, short? countryIdOverride)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callVisitorCountryAwarenessCheck(visitorDbId, countryId, contentDate, countryIdOverride);
        }
        #endregion


        #region Update Culture


        public async Task<Boolean> callVisitorUpdate(Int64 visitorDbId, String ipAddress, String urlCountry, String cloudFlareCountryCode, String cloudFlareDevice)
        {
            Boolean storeResults = await Task.FromResult<Boolean>(callVisitorUpdateAsync(visitorDbId, ipAddress, urlCountry, cloudFlareCountryCode, cloudFlareDevice));
            return storeResults;
        }


        private Boolean callVisitorUpdateAsync(Int64 visitorDbId, String ipAddress, String urlCountry, String cloudFlareCountryCode, String cloudFlareDevice)
        {

            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callVisitorUpdate(visitorDbId, ipAddress, urlCountry, cloudFlareCountryCode, cloudFlareDevice);

        }

        #endregion
        
        #region Save Registration


        public async Task<Int32?> saveRegistration(PassNewMember objRegistrant)
        {
            objRegistrant.password = await generatePassword(objRegistrant.password);

            return await Task.FromResult<Int32?>(saveRegistrationAsync(objRegistrant));
        }

        private Int32? saveRegistrationAsync(PassNewMember objRegistrant)
        {
            var objDbVisitor = new visitorDb();

            return objDbVisitor.callMemberInsert(objRegistrant.languageId, objRegistrant.countryId, objRegistrant.name, objRegistrant.email, objRegistrant.password, 62);
        }

        public async Task<Int32?> saveGuestRegistration(byte languageId, short countryId, String fullName, String email)
        {
            Int32? storeResults = await Task.FromResult<Int32?>(saveGuestRegistrationAsync(languageId, countryId, fullName, email));
            return storeResults;
        }


        private Int32? saveGuestRegistrationAsync(byte languageId, short countryId, String fullName, String email)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callMemberUpsert(languageId, countryId, fullName, email, null, 61, countryId, languageId);
        }


        #endregion

        #region Vistor / Member Associate
        public async Task<bool> visitorMemberAssociate(Int64 visitorId, int memberId)
        {
            bool result = await Task.FromResult<bool>(visitorMemberAssociateAsync(visitorId, memberId));
            return result;
        }


        private bool visitorMemberAssociateAsync(Int64 visitorId, int memberId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callMemberAssociate(visitorId, memberId);
        }
        #endregion

        #region Trade Check

        public async Task<bool> tradePermissionCheck(int? memberId, short countryId)
        {
            bool result = await Task.FromResult(tradePermissionCheckAsync(memberId, countryId));
            return result;
        }

        private bool tradePermissionCheckAsync(int? memberId, short countryId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.PlaceOrderPermissionCheck(memberId, countryId);
        }

        #endregion

        #region Save Trade Registration
        
        public async Task<String> getMemberTradeAuthGUID(int memberId)
        {
            String result = await Task.FromResult<String>(getMemberTradeAuthGUIDAsync(memberId));
            return result;
        }

        public String getMemberTradeAuthGUIDAsync(int memberId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.getMemberTradeAuthGUID(memberId);
        }

        public async Task<String> callMemberTradeAuthGUIDRenew(short countryId, byte languageId, int memberId)
        {
            String result = await Task.FromResult<String>(memberTradeAuthGUIDRenewAsync(countryId, languageId, memberId));
            return result;
        }

        private String memberTradeAuthGUIDRenewAsync(short countryId, byte languageId, int memberId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callTradeMemberAuthGUIDRenew(countryId, languageId, memberId);
        }

        public async Task<tradeAuthGuidCheckResponse> callMemberTradeAuthCommence(Guid identifier)
        {
            tradeAuthGuidCheckResponse result = await Task.FromResult(memberTradeAuthCommenceAsync(identifier));
            return result;
        }

        public tradeAuthGuidCheckResponse memberTradeAuthCommenceAsync(Guid identifier)
        {
            var dataContext = new databaseDataContext();

            var result = dataContext.callTradeMemberAuthCommence(identifier).SingleOrDefault();

            return result;
        }
        
        public async Task<bool> callMemberTradeAuthCancel(int memberId)
        {
            Boolean result = await Task.FromResult<Boolean>(memberTradeAuthCancelAsync(memberId));
            return result;
        }

        public Boolean memberTradeAuthCancelAsync(int memberId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callTradeMemberAuthCancel(memberId);
        }

        public async Task<bool> callMemberTradeAuthResendEmail(short countryId, byte languageId, int memberId)
        {
            Boolean result = await Task.FromResult<Boolean>(memberTradeAuthResendEmailAsync(countryId, languageId, memberId));
            return result;
        }

        public Boolean memberTradeAuthResendEmailAsync(short countryId, byte languageId, int memberId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callTradeMemberAuthResendEmail(countryId, languageId, memberId);
        }

        #endregion

        #region Member Sign In


        public async Task<memberLogin> memberSignIn(long visitorDbId, string email, string newPassword)
        {
            var memberData = await Task.FromResult<memberLogin>(memberSignInAsync(visitorDbId, email, newPassword));

            return memberData;
        }


        private memberLogin memberSignInAsync(long visitorDbId, string email, string newPassword)
        {
            var dataContext = new databaseDataContext();

            var results = dataContext.MemberSignIn_GenerateMultiple(visitorDbId, email, newPassword);

            //  Convert from IEnumerable to a List so a collection query can be run multiple times
            var memberData = new memberLogin
            {
                memberLoginStatusData = results.GetResult<memberLoginStatus>().ToList<memberLoginStatus>()
            };

            return memberData;
        }


        #endregion


        #region Member Name


        public async Task<memberDetailStore> memberDetails(Int64 visitorDbId, DateTime useDate, int? memberId = null)
        {


            memberDetailStore memberData = await Task.FromResult<memberDetailStore>(memberDetailsAsync(visitorDbId, useDate, memberId));
            return memberData;

        }


        private memberDetailStore memberDetailsAsync(Int64 visitorDbId, DateTime useDate, int? memberId = null)
        {
            var dataContext = new databaseDataContext();

            var results = dataContext.MemberDetails_GenerateMultiple(visitorDbId, useDate, memberId);

            var memberData = new memberDetailStore
            {
                MemberDetail = results.GetResult<MemberDetail>().SingleOrDefault(),
                regions = results.GetResult<countryRegion>().ToList()
            };
            return memberData;
        }


        #endregion

        #region Trade Users


        public async Task<TradeAccountUsersModel> callTradeMemberSelect(int tradeAccountId, int memberId, short countryId, byte languageId)
        {
            var tradeData = await Task.FromResult(callTradeMemberSelectAsync(tradeAccountId, memberId, countryId, languageId));
            return tradeData;
        }


        private TradeAccountUsersModel callTradeMemberSelectAsync(int tradeAccountId, int memberId, short countryId, byte languageId)
        {
            var dataContext = new databaseDataContext();

            IMultipleResults results = dataContext.TradeMember_Select_GenerateMultiple(tradeAccountId, memberId, countryId, languageId);

            var tradeData = new TradeAccountUsersModel
            {
                AccountDetails = results.GetResult<TradeAccountDetail>().Single(),
                Administrators = results.GetResult<TradeAccountAdmin>().ToList(),
                Users = results.GetResult<TradeAccountUser>().ToList()
            };
            return tradeData;
        }

        internal countryCheckResult checkCountry(short countryId, byte languageId, bool isTrade, string userCountryCode)
        {
            countryCheckResult result = new countryCheckResult();

            var dataContext = new databaseDataContext();

            IMultipleResults results = dataContext.checkCountry(countryId, languageId, isTrade, userCountryCode);

            result.canSeeRRPs = results.GetResult<bool>().Single();
            result.countryItem = results.GetResult<countryItem>().SingleOrDefault();

            return result;
        }

        public async Task<bool> callTradeMemberDelete(int memberId, int editorMemberId)
        {
            var tradeData = await Task.FromResult(callTradeMemberDeleteAsync(memberId, editorMemberId));
            return tradeData;
        }


        private bool callTradeMemberDeleteAsync(int memberId, int editorMemberId)
        {
            var visitorDb = new visitorDb();
            return visitorDb.callTradeMemberDelete(memberId, editorMemberId);
        }

        
        public async Task<int> callTradeMemberResendInvite(MertexerV2.Models.Accounts.PassTradeUserManagementSave objUser, int editorMemberId, int tradeAccountId)
        {
            var tradeData = await Task.FromResult(callTradeMemberResendInviteAsync(objUser, editorMemberId, tradeAccountId));
            return tradeData;
        }


        private int callTradeMemberResendInviteAsync(MertexerV2.Models.Accounts.PassTradeUserManagementSave objUser, int editorMemberId, int tradeAccountId)
        {
            var visitorDb = new visitorDb();
            return visitorDb.callTradeMemberResendInvite(objUser, editorMemberId, tradeAccountId);
        }

        public async Task<int> callTradeAdminResendInvite(MertexerV2.Models.Accounts.PassTradeUserManagementSave objUser, int editorMemberId, int tradeAccountId)
        {
            var tradeData = await Task.FromResult(callTradeAdminInviteAsync(objUser, editorMemberId, tradeAccountId));
            return tradeData;
        }


        private int callTradeAdminInviteAsync(MertexerV2.Models.Accounts.PassTradeUserManagementSave objUser, int editorMemberId, int tradeAccountId)
        {
            var visitorDb = new visitorDb();
            return visitorDb.callTradeAdminResendInvite(objUser, editorMemberId, tradeAccountId);
        }

        #endregion

        #region tradeOfferDetails 
        public async Task<tradeOfferDetail> tradeOfferDetails(Int32 tradeAccountId)
        {
            tradeOfferDetail tradeOffer = await Task.FromResult<tradeOfferDetail>(tradeOfferDetailsAsync(tradeAccountId));

            return tradeOffer;
        }

        public tradeOfferDetail tradeOfferDetailsAsync(Int32 tradeAccountId)
        {
            tradeOfferDetail result = new databaseDataContext().TradeOffer_Select(tradeAccountId);

            return result;
        }

        #endregion


        #region Password Generation


        public async Task<String> generatePassword(string password)
        {
            String storeResults = await Task.FromResult<String>(generatePasswordAsync(password));
            return storeResults;
        }

        private String generatePasswordAsync(string password)
        {

            //  Rfc2898DeriveBytes is used, rather than MD5 or SHA, as this is "algorithmic slow" (harder to crack with a brute force attack)
            Rfc2898DeriveBytes hasher = new Rfc2898DeriveBytes(password, System.Text.Encoding.Default.GetBytes(getSalt()), 10000);
            return Convert.ToBase64String(hasher.GetBytes(25));

        }


        private String getSalt()
        {
            return "t!W6J39q1m3ULMK35pg2HsW_L8&fPJ(dpJn32(90B9loMblgd(-cM%5ZA0qwKzXc";
        }


        #endregion


        #region Member Sign Out


        public async Task<Boolean> memberSignOut(Int64 visitorDbId)
        {


            Boolean memberData = await Task.FromResult<Boolean>(memberSignOutAsync(visitorDbId));
            return memberData;

        }


        private Boolean memberSignOutAsync(Int64 visitorDbId)
        {
            _ = new databaseDataContext();

            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callSignOut(visitorDbId);

        }


        #endregion


        #region Member Address Insert


        public async Task<Int64> saveMemberAddress(Int64 visitorDbId, string fullName, string companyName, string addressLineOne, string addressLineTwo, string addressTown, string addressCounty, string addressPostCode, short addressCountryId, string addressPhone)
        {

            Int64 storeResults = await Task.FromResult<Int64>(saveMemberAddressAsync(visitorDbId, fullName, companyName, addressLineOne, addressLineTwo, addressTown, addressCounty, addressPostCode, addressCountryId, addressPhone));
            return storeResults;

        }


        private Int64 saveMemberAddressAsync(Int64 visitorDbId, string fullName, string companyName, string addressLineOne, string addressLineTwo, string addressTown, string addressCounty, string addressPostCode, short addressCountryId, string addressPhone)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callAddressInsert(visitorDbId, fullName, companyName, addressLineOne, addressLineTwo, addressTown, addressCounty, addressPostCode, addressCountryId, addressPhone);
        }


        #endregion


        #region Member Address Update


        public async Task<Boolean> updateMemberAddress(Int64 memberAddressId, string fullName, string companyName, string addressLineOne, string addressLineTwo, string addressTown, string addressCounty, string addressPostCode, short addressCountryId, string addressPhone)
        {

            Boolean storeResults = await Task.FromResult<Boolean>(updateMemberAddressAsync(memberAddressId, fullName, companyName, addressLineOne, addressLineTwo, addressTown, addressCounty, addressPostCode, addressCountryId, addressPhone));
            return storeResults;

        }


        private Boolean updateMemberAddressAsync(Int64 memberAddressId, string fullName, string companyName, string addressLineOne, string addressLineTwo, string addressTown, string addressCounty, string addressPostCode, short addressCountryId, string addressPhone)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callAddressUpdate(memberAddressId, fullName, companyName, addressLineOne, addressLineTwo, addressTown, addressCounty, addressPostCode, addressCountryId, addressPhone);
        }

        #endregion

        #region Member Address Default

        public async Task<Boolean> setDefaultMemberAddress(Int32 memberId, Int64 memberAddressId, short countryId)
        {
            Boolean storeResults = await Task.FromResult<Boolean>(setDefaultMemberAddressAsync(memberId, memberAddressId, countryId));
            return storeResults;
        }

        private Boolean setDefaultMemberAddressAsync(Int32 memberId, Int64 memberAddressId, short countryId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callMakeMemberAddressDefault(memberId, memberAddressId, countryId);
        }
        #endregion 

     
        #region Member Address Delete


        public async Task<Boolean> deleteMemberAddress(Int64 memberAddressId)
        {

            Boolean storeResults = await Task.FromResult<Boolean>(deleteMemberAddressAsync(memberAddressId));
            return storeResults;

        }


        private Boolean deleteMemberAddressAsync(Int64 memberAddressId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callAddressDelete(memberAddressId);
        }


        #endregion


        #region Member Third Party Preference Save


        public async Task<Boolean> saveMemberThirdPartyPreference(Int32 memberId, Boolean preference)
        {
            Boolean storeResults = await Task.FromResult<Boolean>(saveMemberThirdPartyPreferenceAsync(memberId, preference));
            return storeResults;
        }

        private Boolean saveMemberThirdPartyPreferenceAsync(Int32 memberId, Boolean preference)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callMemberThirdPartyPreferenceUpdate(memberId, preference);
        }

        #endregion


        #region Member Address Select


        public async Task<MemberAddressSelect> getMemberAddresses(Int64 visitorDbId, Int64? memberAddressId, byte languageId, short? countryId, DateTime contentDate)
        {
            MemberAddressSelect basketStore = await Task.FromResult<MemberAddressSelect>(getMemberAddressesAsync(visitorDbId, memberAddressId, languageId, countryId));

            var visitorFactory = new VisitorFactory();
            var memberStore = await visitorFactory.memberDetails(visitorDbId, contentDate);

            if (memberStore.MemberDetail != null)
            {
                if (memberStore.MemberDetail.MertexCustomerAccountNumber != null)
                {
                    try
                    {
                        var mertexAddresses = MertexAccountChecks.AddressBookFetch.FetchAddresses(memberStore.MemberDetail.MertexCustomerAccountNumber);

                        foreach (var ma in mertexAddresses)
                        {
                            basketStore.TradeAddresses.Add(new MemberTradeAddress()
                            {
                                FullName = (ma.ContactName == null || ma.ContactName == "") ? ma.DeliveryName : ma.ContactName,
                                CompanyName = ma.DeliveryName,
                                AddressLine1 = ma.AddressLine1,
                                AddressLine2 = ma.AddressLine2,
                                Town = ma.AddressLine3 == "" ? ma.AddressLine2 : ma.AddressLine3,
                                County = ma.AddressLine4 == "" ? ma.AddressLine3 == "" ? ma.AddressLine2 : ma.AddressLine3 : ma.AddressLine4,
                                PostCode = ma.PostCode,
                                CountryCode = ma.Country,
                                CountryID = null,
                                PhoneNumber = ma.PhoneNumber,
                                Usable = false
                            });
                        }
                    } catch {
                        basketStore.TradeAddressErrored = true;
                    }

                    foreach(var a in basketStore.Addresses)
                    {
                        basketStore.TradeAddresses.Add(new MemberTradeAddress() {
                            FullName = a.FullName,
                            CompanyName = a.CompanyName,
                            AddressLine1 = a.AddressLine1,
                            AddressLine2 = a.AddressLine2,
                            Town = a.Town,
                            County = a.County,
                            PostCode = a.PostCode,
                            CountryCode = a.CountryCode,
                            CountryID = a.CountryID,
                            PhoneNumber = a.PhoneNumber,
                            Usable = true
                        });
                    }
                }
            }
            
            Country.cmsCountry objCountry = new Country.cmsCountry();
            cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(contentDate);

            foreach(var t in basketStore.TradeAddresses)
            {
                if (dgFramework.doesCountryExistFromCode(t.CountryCode, objCountryData))
                {
                    countryItem objCountryItem = dgFramework.getCountryFromCode(t.CountryCode, objCountryData);

                    t.CountryID = objCountryItem.CountryID;
                }

                if ((t.FullName?.Length >= 2 && t.FullName?.Length <= 40) &&
                    (t.AddressLine1?.Length >= 2 && t.AddressLine2?.Length <= 60) &&
                    (t.Town?.Length >= 2 && t.Town?.Length <= 50) &&
                    (t.County?.Length >= 2 && t.County?.Length <= 40) &&
                    (t.PostCode?.Length >= 2 && t.PostCode?.Length <= 20) &&
                    //(t.PhoneNumber.Length >= 2 && t.PhoneNumber?.Length <= 20) &&
                    (t.CountryID != null))
                {
                    t.Usable = true;
                }
            }
            
            return basketStore;
        }

        public async Task<MemberAddress> getDefaultMemberAddress(int memberId, short countryId)
        {
            return await Task.FromResult<MemberAddress>(getDefaultMemberAddressAsync(memberId, countryId));
        }

        private MemberAddressSelect getMemberAddressesAsync(Int64 visitorDbId, Int64? memberAddressId, byte languageId, short? countryId)
        {


            var dataContext = new databaseDataContext();


            IMultipleResults results = dataContext.MemberAddress_GenerateMultiple(visitorDbId, memberAddressId, countryId, languageId);


            //  Convert from IEnumerable to a List so a query can be run multiple times
            MemberAddressSelect memberAddressStore = new MemberAddressSelect
            {
                Addresses = results.GetResult<MemberAddress>().ToList()
            };

            return memberAddressStore;
        }

        private MemberAddress getDefaultMemberAddressAsync(int memberId, short countryId)
        {
            var dataContext = new databaseDataContext();

            var results = dataContext.GetDefaultMemberAddress(memberId, countryId);

            return results.SingleOrDefault();
        }


        #endregion


        #region Member Update Password
        

        public async Task<Boolean> updateMemberPassword(Int64 visitorDbId, String currentPasswordEnc, String newPasswordEnc)
        {

            Boolean storeResults = await Task.FromResult<Boolean>(updateMemberPasswordAsync(visitorDbId, currentPasswordEnc, newPasswordEnc));
            return storeResults;

        }


        private Boolean updateMemberPasswordAsync(Int64 visitorDbId, String currentPasswordEnc, String newPasswordEnc)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callUpdateMemberPassword(visitorDbId, currentPasswordEnc, newPasswordEnc);
        }

        #endregion

        #region Member Update Country

        public async Task<Boolean> updateMemberCountry(Int32 memberId, Byte countryLanguageId)
        {

            Boolean storeResults = await Task.FromResult<Boolean>(updateMemberCountryAsync(memberId, countryLanguageId));
            return storeResults;

        }


        private Boolean updateMemberCountryAsync(Int32 memberId, Byte countryLanguageId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callUpdateMemberCountry(memberId, countryLanguageId);
        }

        #endregion


        #region Member Fetch Trade Contact Us

        public async Task<tradeContactUsResult> generateTradeContactUs(int tradeAccountId)
        {
            var storeResults = await Task.FromResult<tradeContactUsResult>(generateTradeContactUsAsync(tradeAccountId));
            return storeResults;
        }


        private tradeContactUsResult generateTradeContactUsAsync(int tradeAccountId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.generateTradeContactUs(tradeAccountId);
        }

        #endregion



        #region Member Update Trade Account Number

        public async Task<updateMemberTradeAccountNumberResult> callMemberTradeAccountUpdate(MertexerV2.Models.Accounts.PassTradeUserManagementSave objSave, int tradeAccountId, int editorMemberId)
        {
            var storeResults = await Task.FromResult<updateMemberTradeAccountNumberResult>(updateMemberTradeAccountNumberAsync(objSave, tradeAccountId, editorMemberId));
            return storeResults;
        }


        private updateMemberTradeAccountNumberResult updateMemberTradeAccountNumberAsync(MertexerV2.Models.Accounts.PassTradeUserManagementSave objSave, int tradeAccountId, int editorMemberId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callUpdateMemberTradeAccountNumber(objSave, tradeAccountId, editorMemberId);
        }
        
        #endregion


        #region Member Update Display Trade Prices
        public async Task<Boolean> updateMemberDisplayTradePrice(Int32 memberId, Boolean displayTradePrices)
        {
            Boolean storeResults = await Task.FromResult<Boolean>(updateMemberDisplayTradePriceAsync(memberId, displayTradePrices));
            return storeResults;
        }


        private Boolean updateMemberDisplayTradePriceAsync(Int32 memberId, Boolean displayTradePrices)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callUpdateMemberDisplayTradePrice(memberId, displayTradePrices);
        }
        #endregion

        public async Task<bool> tradeAccountInvoiceAddressRequestChange(loggedInMemberDetail loginCheck, MertexerV2.Models.Accounts.PassInvoiceAddressChangeRequest objRequest)
        {
            var db = new visitorDb();

            return await db.storeTradeInvoiceAddressChangeRequest(loginCheck, objRequest);
        }

        public quickSearchResult tradeQuickSearch(MertexerV2.Models.Accounts.PassQuickSearch objSearch, int? tradeAccountId, int? memberId, bool displayTradePrice, DateTime useDate)
        {
            var db = new visitorDb();

            return db.tradeQuickSearch(objSearch, tradeAccountId, displayTradePrice, memberId, useDate);
        }

        #region Member Update Name


        public async Task<Boolean> updateMemberName(long visitorDbId, string name)
        {

            Boolean storeResults = await Task.FromResult<Boolean>(updateMemberNameAsync(visitorDbId, name));
            return storeResults;

        }


        private Boolean updateMemberNameAsync(long visitorDbId, string name)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callUpdateMemberName(visitorDbId, name);
        }

        #endregion

        #region Member Update Email


        public async Task<Boolean> updateMemberEmail(long visitorDbId, string newEmail)
        {

            Boolean storeResults = await Task.FromResult<Boolean>(updateMemberEmailAsync(visitorDbId, newEmail));
            return storeResults;

        }


        private Boolean updateMemberEmailAsync(long visitorDbId, string newEmail)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callUpdateMemberEmail(visitorDbId, newEmail);
        }

        #endregion


        #region Member Update Detail


        public async Task<Boolean> updateMemberDetail(Int64 visitorDbId, String fullName)
        {

            Boolean storeResults = await Task.FromResult<Boolean>(updateMemberDetailAsync(visitorDbId, fullName));
            return storeResults;

        }


        private Boolean updateMemberDetailAsync(Int64 visitorDbId, String fullName)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.callUpdateMemberDetail(visitorDbId, fullName);
        }

        #endregion


        #region Password Reset Request


        public async Task<bool> passwordResetRequest(PassPasswordResetRequest objRequest)
        {

            var storeResults = await Task.FromResult<Boolean>(passwordResetRequestAsync(objRequest));
            return storeResults;

        }


        private bool passwordResetRequestAsync(PassPasswordResetRequest objRequest)
        {
            var objDbVisitor = new visitorDb();
            return objDbVisitor.passwordResetRequest(objRequest);
        }

        #endregion


        #region Password Reset


        public async Task<Boolean> resetPassword(String token, String newPasswordEnc)
        {

            Boolean storeResults = await Task.FromResult<Boolean>(resetPasswordAsync(token, newPasswordEnc));
            return storeResults;

        }


        private Boolean resetPasswordAsync(String token, String newPasswordEnc)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.passwordReset(token, newPasswordEnc);
        }

        #endregion


        #region Password Reset - Token Validation


        public async Task<Boolean> validateResetPasswordToken(String token)
        {

            Boolean storeResults = await Task.FromResult<Boolean>(validateResetPasswordTokenAsync(token));
            return storeResults;

        }


        private Boolean validateResetPasswordTokenAsync(String token)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.validateResetPasswordToken(token);
        }


        #endregion


        #region Order History


        public async Task<OrderHistory> getOrderHistory(Int64 visitorDbId, countryItem objCountryItem, String searchString, Int16? historyDays, byte? OrderStatusId, String RequestDateOverride, int? transactionId = null)
        {

            if (OrderStatusId == 99)
            {
                OrderStatusId = null;
            }
            if (historyDays == 999)
            {
                historyDays = null;
            }

            if (RequestDateOverride == null || RequestDateOverride == "")
                RequestDateOverride = DateTime.Now.ToString();
            DateTime.TryParse(RequestDateOverride, out DateTime useDate);

            if (useDate == DateTime.MinValue)
                useDate = DateTime.Now;

            if (searchString == HtmlHelperExtra.Resource("EnterSearch", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString())
            {
                searchString = null;
            }


            OrderHistory storeResults = await Task.FromResult<OrderHistory>(getOrderHistoryAsync(visitorDbId, objCountryItem, searchString, historyDays, OrderStatusId, RequestDateOverride, transactionId));
            return storeResults;

        }


        private OrderHistory getOrderHistoryAsync(Int64 visitorDbId, countryItem objCountryItem, String searchString, Int16? historyDays, byte? OrderStatusId, String RequestDateOverride, int? transactionId = null)
        {

            var dataContext = new databaseDataContext();
            _ = DateTime.Now;

            if (RequestDateOverride == null || RequestDateOverride == "")
                RequestDateOverride = DateTime.Now.ToString();
            DateTime.TryParse(RequestDateOverride, out DateTime useDate);

            if (useDate == DateTime.MinValue)
                useDate = DateTime.Now;

            IMultipleResults results = dataContext.OrderHistory_GenerateMultiple(visitorDbId, objCountryItem.CountryID, objCountryItem.LanguageID, searchString, historyDays, OrderStatusId, transactionId);


            //  Convert from IEnumerable to a List so a query can be run multiple times
            OrderHistory memberOrderHistory = new OrderHistory
            {
                OrderHeads = results.GetResult<OrderHistoryHeader>().ToList<OrderHistoryHeader>(),
                OrderLines = results.GetResult<OrderHistoryLine>().ToList<OrderHistoryLine>(),
                OrderLineTracking = results.GetResult<OrderHistoryLineTracking>().ToList<OrderHistoryLineTracking>(),
                OrderStatuses = results.GetResult<OrderHistoryOrderStatus>().ToList<OrderHistoryOrderStatus>(),
                ReturnReasons = results.GetResult<OrderHistoryReturnReason>().ToList<OrderHistoryReturnReason>(),
                PreviousReturns = results.GetResult<OrderHistoryPreviousReturn>().ToList<OrderHistoryPreviousReturn>()
            };


            //  Generate display properties for use on the client
            foreach (OrderHistoryHeader objHeader in memberOrderHistory.OrderHeads)
            {
                objHeader.DisplayOrderDate = objHeader.OrderDate.ToString("dd MMMM yyyy");
                objHeader.DisplayOrderTotal = String.Format("{0}{1}", 
                    HtmlHelperExtra.Resource("currencySymbol", objCountryItem.LanguageID, objCountryItem.CountryID, useDate),
                    objHeader.OrderTotal.ToString("###,##0.00"));

                String tempFrom = "";
                String tempTo = "";
                if (objHeader.MinDeliveryEstimate == null) { }
                else
                {
                    tempFrom = ((DateTime)objHeader.MinDeliveryEstimate).ToString("dddd dd MMMM yyyy");
                }
                if (objHeader.MaxDeliveryEstimate == null) { }
                else
                {
                    tempTo = ((DateTime)objHeader.MaxDeliveryEstimate).ToString("dddd dd MMMM yyyy");
                }

                if (tempFrom == tempTo)
                {
                    if (tempFrom == "")
                    {
                        objHeader.DisplayDelivery = HtmlHelperExtra.Resource("Unknown", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString();
                    }
                    else
                    {
                        objHeader.DisplayDelivery = tempFrom;
                    }
                }
                else
                {
                    objHeader.DisplayDelivery = String.Format("{0} {1} {2}", tempFrom, HtmlHelperExtra.Resource("To", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString(), tempTo);
                }

            }

            // Tracking display date
            foreach (OrderHistoryLineTracking objTracking in memberOrderHistory.OrderLineTracking)
            {
                objTracking.DisplayDispatchedDate = objTracking.DispatchedDate.ToString("dddd dd MMMM yyyy");
            }

            //  Add an all status
            memberOrderHistory.OrderStatusList = new List<DisplayOrderStatus>
            {
                new DisplayOrderStatus { OrderStatusID = 99, Name = HtmlHelperExtra.Resource("OrderHistoryAllStatus", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString(), Default = true }
            };
            foreach (OrderHistoryOrderStatus objStatus in memberOrderHistory.OrderStatuses)
            {
                memberOrderHistory.OrderStatusList.Add(new DisplayOrderStatus { OrderStatusID = objStatus.OrderStatusID, Name = objStatus.Name, Default = false});
            }


            //  Add the history status list (default to 6 months)
            memberOrderHistory.DateRangeList = new List<DisplayDateRange>
            {
                new DisplayDateRange { NumberOfDays = 30, Name = HtmlHelperExtra.Resource("OrderHistory30Days", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString(), Default = false },
                new DisplayDateRange { NumberOfDays = 182, Name = HtmlHelperExtra.Resource("OrderHistory6Months", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString(), Default = true },
                new DisplayDateRange { NumberOfDays = 365, Name = HtmlHelperExtra.Resource("OrderHistory1Year", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString(), Default = false },
                new DisplayDateRange { NumberOfDays = 999, Name = HtmlHelperExtra.Resource("OrderHistoryAll", objCountryItem.LanguageID, objCountryItem.CountryID, useDate).ToString(), Default = false }
            };


            //  Generate display data for previous returns
            foreach (OrderHistoryPreviousReturn previousReturn in memberOrderHistory.PreviousReturns)
            {
                previousReturn.DisplayCreatedDT = previousReturn.CreatedDT.ToString("dddd dd MMMM yyyy");
            }


            //  Generate display data for each line
            foreach (OrderHistoryLine objOrderLine in memberOrderHistory.OrderLines)
            {
                if (objOrderLine.ProductPageID == null)
                {
                    objOrderLine.DisplayURL = "";   //  Product could have been discontinued
                }
                else
                {
                    objOrderLine.DisplayURL = dgFramework.buildUrl(objCountryItem, "", dgContentType.productPage, objOrderLine.ProductPageID, null, null);
                }
            }


            return memberOrderHistory;

        }


        #endregion


        #region Save Return


        public async Task<Int32> saveReturn(Int64 visitorDbId, Int32 orderHeadId, DataTable returnTableType)
        {

            Int32 storeResults = await Task.FromResult<Int32>(saveReturnAsync(visitorDbId, orderHeadId, returnTableType));
            return storeResults;

        }


        private Int32 saveReturnAsync(Int64 visitorDbId, Int32 orderHeadId, DataTable returnTableType)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.saveReturn(visitorDbId, orderHeadId, returnTableType);
        }

        public async Task<Int32> saveUPSTrackingNumbers(int returnHeadId, DataTable upsLabelsTableType)
        {
            Int32 storeResults = await Task.FromResult<Int32>(saveUPSTrackingNumbersAsync(returnHeadId, upsLabelsTableType));
            return storeResults;
        }

        private Int32 saveUPSTrackingNumbersAsync(int returnHeadId, DataTable upsLabelsTableType)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.saveUPSTrackingNumbers(returnHeadId, upsLabelsTableType);
        }
        
        public async Task<Int32> saveReturnCollectionPRN(int returnHeadId, string prn)
        {
            Int32 results = await Task.FromResult<Int32>(saveReturnCollectionPRNAsync(returnHeadId, prn));
            return results;
        }

        private Int32 saveReturnCollectionPRNAsync(int returnHeadId, string prn)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.saveReturnCollectionPRN(returnHeadId, prn);
        }
        #endregion


        #region Scrapbook Add Image

        public async Task<bool> saveImageToScrapbook(Int64 visitorDbId, Int32? memberId, Int64 imageId, Int16 countryId, Int32? productPageId)
        {
            Boolean result = await Task.FromResult<Boolean>(saveImageToScrapbookAsync(visitorDbId, memberId, imageId, countryId, productPageId));
            return result;
        }


        private bool saveImageToScrapbookAsync(Int64 visitorDbId, Int32? memberId, Int64 imageId, Int16 countryId, Int32? productPageId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.saveImageToScrapbook(visitorDbId, memberId, imageId, countryId, productPageId);
        }

        #endregion


        #region Scrapbook Delete Image

        public async Task<bool> removeImageFromScrapbook(Int64 visitorDbId, Int32? memberId, Int16 countryId, Int64 imageId)
        {
            Boolean result = await Task.FromResult<Boolean>(removeImageFromScrapbookAsync(visitorDbId, memberId, countryId, imageId));
            return result;
        }


        private bool removeImageFromScrapbookAsync(Int64 visitorDbId, Int32? memberId, Int16 countryId, Int64 imageId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.removeImageFromScrapbook(visitorDbId, memberId, countryId, imageId);
        }

        #endregion

        #region Scrapbook Sort

        public async Task<bool> sortScrapbook(int memberId, PassScrapbook objScrapbook)
        {
            return await Task.FromResult<bool>(sortScrapbookAsync(memberId, objScrapbook));
        }

        private bool sortScrapbookAsync(int memberId, PassScrapbook objScrapbook)
        {
            var db = new visitorDb();
            return db.SortScrapbook(memberId, objScrapbook);
        }

        #endregion

        #region Scrapbook Clear

        public async Task<Boolean> clearScrapbook(Int64 visitorDbId, Int32? memberId, Int16 countryId)
        {
            Boolean result = await Task.FromResult<Boolean>(clearScrapbookAsync(visitorDbId, memberId, countryId));
            return result;
        }


        private bool clearScrapbookAsync(Int64 visitorDbId, Int32? memberId, Int16 countryId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.clearScrapbook(visitorDbId, memberId, countryId);
        }

        #endregion



        #region Scrapbook Get

        public async Task<List<ScrapbookImage>> getVisitorScrapbook(Int64 visitorDbId, Int32? memberId, Byte? productTypeId, Byte? brandId, Boolean? isNew, Int16 countryID, Byte languageID, DateTime contentDate)
        {
            List<ScrapbookImage> results = await Task.FromResult<List<ScrapbookImage>>(getVisitorScrapbookAsync(visitorDbId, memberId, productTypeId, brandId, isNew, countryID, languageID, contentDate));
            return results;
        }

        public List<ScrapbookImage> getVisitorScrapbookAsync(Int64 visitorDbId, Int32? memberId, Byte? productTypeId, Byte? brandId, Boolean? isNew, Int16 countryId, Byte languageId, DateTime contentDate)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.getVisitorScrapbook(visitorDbId, memberId, productTypeId, brandId, isNew, countryId, languageId, contentDate);
        }

        #endregion


        #region Moodboard Add Image

        public async Task<Boolean> addImageToMoodboard(Int32 moodboardId, Int64 imageId, Int32? productPageId)
        {
            Boolean result = await Task.FromResult<Boolean>(addImageToMoodboardAsync(moodboardId, imageId, productPageId));
            return result;
        }


        private Boolean addImageToMoodboardAsync(Int32 moodboardId, Int64 imageId, Int32? productPageId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.addImageToMoodboard(moodboardId, imageId, productPageId);
        }

        #endregion


        #region Moodboard Delete Image

        public async Task<bool> removeImageFromMoodboard(Int32 moodboardId, Int64 imageId)
        {
            Boolean result = await Task.FromResult<Boolean>(removeImageFromMoodboardAsync(moodboardId, imageId));
            return result;
        }


        private bool removeImageFromMoodboardAsync(Int32 moodboardId, Int64 imageId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.removeImageFromMoodboard(moodboardId, imageId);
        }

        #endregion
        
        #region Moodboard Clear

        public async Task<bool> clearMoodboard(Int32 memberId, Int32 moodboardId)
        {
            Boolean result = await Task.FromResult<Boolean>(clearMoodboardAsync(memberId, moodboardId));
            return result;
        }


        private bool clearMoodboardAsync(Int32 memberId, Int32 moodboardId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.clearMoodboard(memberId, moodboardId);
        }

        #endregion

        #region Moodboard Get

        public async Task<MoodboardContainer> getMemberMoodboards(Int32 memberId, Int16 countryID, Byte languageID, DateTime contentDate)
        {
            var results = await Task.FromResult<MoodboardContainer>(getMemberMoodboardsAsync(memberId, countryID, languageID, contentDate));
            return results;
        }

        public MoodboardContainer getMemberMoodboardsAsync(Int32 memberId, Int16 countryId, Byte languageId, DateTime contentDate)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.getMemberMoodboards(memberId, countryId, languageId, contentDate);
        }

        #endregion


        #region Moodboard Save

        public async Task<Boolean> saveMoodboard(Int32 moodboardId, Int16 height, String json, String customerName, Byte? roomId, String reference)
        {
            var results = await Task.FromResult<Boolean>(saveMoodboardAsync(moodboardId, height, json, customerName, roomId, reference));
            return results;
        }

        public Boolean saveMoodboardAsync(Int32 moodboardId, Int16 height, String json, String customerName, Byte? roomId, String reference)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.saveMoodboard(moodboardId, height, json, customerName, roomId, reference);
        }

        #endregion

        #region Moodboard Save (GridView row)

        public async Task<Boolean> saveMoodboardFromGridView(Int32 moodboardId, String customerName, Byte? roomId, String reference)
        {
            var results = await Task.FromResult<Boolean>(saveMoodboardFromGridViewAsync(moodboardId, customerName, roomId, reference));
            return results;
        }

        public Boolean saveMoodboardFromGridViewAsync(Int32 moodboardId, String customerName, Byte? roomId, String reference)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.saveMoodboardFromGridView(moodboardId, customerName, roomId, reference);
        }

        #endregion


        #region Moodboard Create


        public async Task<Int32> createMoodboard(Int32 memberId, Int16 countryId)
        {
            var result = await Task.FromResult<Int32>(createMoodboardAsync(memberId, countryId));
            return result;
        }

        public Int32 createMoodboardAsync(Int32 memberId, Int16 countryId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.createMoodboard(memberId, countryId);
        }


        #endregion


        #region Moodboard Delete

        public async Task<Boolean> deleteMoodboard(Int32 memberId, Int32 moodboardId)
        {
            var result = await Task.FromResult<Boolean>(deleteMoodboardAsync(memberId, moodboardId));
            return result;
        }

        public Boolean deleteMoodboardAsync(Int32 memberId, Int32 moodboardId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.deleteMoodboard(memberId, moodboardId);
        }

        #endregion


        #region Moodboard Search

        public async Task<MoodboardContainer> searchMoodboards(Int32 memberId, String customerName, Byte? roomId, String reference, Int16 countryId, Byte languageId, DateTime? contentDate)
        {
            var results = await Task.FromResult<MoodboardContainer>(searchMoodboardsAsync(memberId, customerName, roomId, reference, countryId, languageId, contentDate));
            return results;
        }

        public MoodboardContainer searchMoodboardsAsync(Int32 memberId, String customerName, Byte? roomId, String reference, Int16 countryId, Byte languageId, DateTime? contentDate)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.searchMemberMoodboards(memberId, customerName, roomId, reference, countryId, languageId, contentDate);
        }

#endregion


        #region Moodboard Reload

        public async Task<MoodboardContainer> reloadMoodboards(Int32 memberId, Int16 countryId, Byte languageId, DateTime contentDate)
        {
            var results = await Task.FromResult<MoodboardContainer>(reloadMoodboardsAsync(memberId, countryId, languageId, contentDate));
            return results;
        }

        public MoodboardContainer reloadMoodboardsAsync(Int32 memberId, Int16 countryId, Byte languageId, DateTime contentDate)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.reloadMemberMoodboards(memberId, countryId, languageId, contentDate);
        }

        #endregion


        #region Moodboard Fetch Single

        public async Task<Moodboard> fetchMoodboard(Int32 moodboardId)
        {
            var result =  await Task.FromResult<Moodboard>(fetchMoodboardAsync(moodboardId));
            return result;
        }

        public Moodboard fetchMoodboardAsync(Int32 moodboardId)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.fetchMoodboard(moodboardId);
        }

        #endregion


        #region Moodboard Fetch Sample Ordering

        public async Task<DatabaseJSONResponse> fetchMoodboardProducts(int memberId, int? tradeAccountId, bool displayTradePrice, PassMoodboardOrderingFetch objFetch)
        {
            var result = await Task.FromResult<DatabaseJSONResponse>(fetchMoodboardSampleOrderingAsync(memberId, tradeAccountId, displayTradePrice, objFetch));

            return result;
        }


        public DatabaseJSONResponse fetchMoodboardSampleOrderingAsync(int memberId, int? tradeAccountId, bool displayTradePrice, PassMoodboardOrderingFetch objFetch)
        {
            visitorDb objDbVisitor = new visitorDb();
            return objDbVisitor.fetchMoodboardSampleOrdering(memberId, tradeAccountId, displayTradePrice, objFetch);
        }

        #endregion


        #region Moodboard Send Share Email

        public async Task<Boolean> SendMoodboardEmail(MoodboardShareEmail objEmail)
        {
            var result = await Task.FromResult<Boolean>(SendMoodboardEmailAsync(objEmail));
        
            return result;
        }

        public Boolean SendMoodboardEmailAsync(MoodboardShareEmail objEmail)
        {
            visitorDb objVisitorDb = new visitorDb();
            return objVisitorDb.SendMoodboardEmail(objEmail);
        }

        #endregion


        #region Generate the Return Paperwork


        public async Task<memberReturnPaperworkGenerate> getReturnPaperwork(Int32 returnHeadId, byte languageId, Int64 visitorDbId)
        {

            memberReturnPaperworkGenerate storeResults = await Task.FromResult<memberReturnPaperworkGenerate>(getReturnPaperworkAsync(returnHeadId, languageId, visitorDbId));
            return storeResults;

        }


        private memberReturnPaperworkGenerate getReturnPaperworkAsync(Int32 returnHeadId, byte languageId, Int64 visitorDbId)
        {

            var dataContext = new databaseDataContext();


            IMultipleResults results = dataContext.GenerateReturnPaperwork_GenerateMultiple(returnHeadId, languageId, visitorDbId);


            //  Convert from IEnumerable to a List so a query can be run multiple times
            memberReturnPaperworkGenerate memberReturnPaperwork = new memberReturnPaperworkGenerate
            {
                memberReturnHeaders = results.GetResult<memberReturnHeader>().ToList<memberReturnHeader>(),
                memberReturnItems = results.GetResult<memberReturnItem>().ToList<memberReturnItem>(),
                memberReturnDetails = results.GetResult<memberReturnDetail>().ToList<memberReturnDetail>()
            };


            return memberReturnPaperwork;

        }


        #endregion


        #region Receipt Generate


        public async Task<memberReceiptGenerate> getReceiptData(Int64 visitorDbId, Int32 transactionId)
        {

            memberReceiptGenerate storeResults = await Task.FromResult<memberReceiptGenerate>(getReceiptDataAsync(visitorDbId, transactionId));
            return storeResults;

        }


        private memberReceiptGenerate getReceiptDataAsync(Int64 visitorDbId, Int32 transactionId)
        {

            var dataContext = new databaseDataContext();


            IMultipleResults results = dataContext.GenerateReceipt_GenerateMultiple(transactionId, visitorDbId);


            //  Convert from IEnumerable to a List so a query can be run multiple times
            memberReceiptGenerate memberReceiptData = new memberReceiptGenerate
            {
                receiptSummarys = results.GetResult<receiptSummary>().ToList<receiptSummary>(),
                receiptHeaders = results.GetResult<receiptHeader>().ToList<receiptHeader>(),
                receiptLines = results.GetResult<receiptLine>().ToList<receiptLine>()
            };


            return memberReceiptData;

        }


        #endregion


        #region Postcode Lookup Search Audit


        public async Task<Boolean> auditPostCodeLookupSearch(Int64 visitorDbId, String countryCode, String postcode, String addressLine1)
        {


            Boolean postcodeAudit = await Task.FromResult<Boolean>(auditPostCodeLookupSearchAsync(visitorDbId, countryCode, postcode, addressLine1));
            return postcodeAudit;

        }


        private Boolean auditPostCodeLookupSearchAsync(Int64 visitorDbId, String countryCode, String postcode, String addressLine1)
        {
            _ = new databaseDataContext();

            visitorDb objDbVisitor = new visitorDb();
            objDbVisitor.callPostCodeSearchAudit(visitorDbId, countryCode, postcode, addressLine1);
            return true;

        }


        #endregion


        #region Back In Stock Reminder

        public async Task<Boolean> createBackInStockReminder(Int64 visitorDbId, Int32 productId, Int16 countryId, Byte languageId)
        {
            Boolean backInStockReminderCreated = await Task.FromResult<Boolean>(createBackInStockReminderAsync(visitorDbId, productId, countryId, languageId));
            return backInStockReminderCreated;
        }

        private Boolean createBackInStockReminderAsync(Int64 visitorDbId, Int32 productId, Int16 countryId, Byte languageId)
        {
            _ = new databaseDataContext();

            visitorDb objDbVisitor = new visitorDb();

            Boolean result = objDbVisitor.callBackInStockReminderInsert(visitorDbId, productId, countryId, languageId);

            return result;
        }

        public async Task<MemberDetail> getMemberByGuid(Guid memberGuid)
        {
            var result = await Task.FromResult<MemberDetail>(getMemberByGuidAsync(memberGuid));

            return result;
        }

        private MemberDetail getMemberByGuidAsync(Guid memberGuid)
        {
            var dataContext = new databaseDataContext();

            var result = dataContext.Member_SelectByGUID(memberGuid).SingleOrDefault();

            return result;
        }

        public async Task<MemberDetail> getMemberById(int memberId)
        {
            var result = await Task.FromResult<MemberDetail>(getMemberByIdAsync(memberId));

            return result;
        }

        private MemberDetail getMemberByIdAsync(int memberId)
        {
            var dataContext = new databaseDataContext();

            var result = dataContext.Member_SelectByID(memberId).SingleOrDefault();

            return result;
        }

        public async Task<MemberDetail> getMemberByEmail(string email)
        {
            var result = await Task.FromResult<MemberDetail>(getMemberByEmailAsync(email));

            return result;
        }

        private MemberDetail getMemberByEmailAsync(string email)
        {
            var dataContext = new databaseDataContext();

            var result = dataContext.Member_SelectByEmail(email).SingleOrDefault();

            return result;
        }
        #endregion

        #region Log PageView

        /// <summary>
        /// Only logs pageView if appSettings "logPageViews" == true.
        /// Please don't log everything ever: we don't really need or want to know about the redirects we serve. Log definite actual page views.
        /// </summary>
        /// <param name="pageView"></param>
        public void logPageView(logPageView pageView)
        {
            try
            {
                if (Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["logPageViews"]) == true)
                {
                    Task.Run(() => {
                        var visitorDb = new visitorDb();
                        visitorDb.logPageView(pageView);
                    });
                }
            }
            catch (Exception)
            {
                // Do NOT explode just because we failed to log a pageView
            }
        }

        #endregion
    }
}