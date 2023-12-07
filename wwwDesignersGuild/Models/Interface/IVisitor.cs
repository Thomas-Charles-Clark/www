using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;

namespace wwwDesignersGuild.Models.Interface
{
    public interface IVisitor
    {
        Task<identificationDefinition> callVisitorInsert(String userAgent, String ipAddress, String culture, String cloudFlareCountryCode, String cloudFlareDevice);
        Task<VisitorSelect> callVisitorSelect(String passGuid, String cloudFlareCountryCode, String cloudFlareDevice);
        Task<String> callVisitorSelectCulture(Int64 visitorDbId);
        Task<Boolean> callVisitorUpdate(Int64 visitorDbId, String ipAddress, String urlCountry, String cloudFlareCountryCode, String cloudFlareDevice);
        Task<memberCheckResult> callMemberCheckSelect(String passEmail);
        Task<Int32?> saveRegistration(PassNewMember objRegistrant);
        Task<String> getMemberTradeAuthGUID(Int32 memberId);
        Task<String> callMemberTradeAuthGUIDRenew(Int16 countryId, Byte languageId, Int32 memberId);
        Task<tradeAuthGuidCheckResponse> callMemberTradeAuthCommence(Guid identifier);
        Task<Boolean> callMemberTradeAuthCancel(Int32 memberId);
        Task<Boolean> callMemberTradeAuthResendEmail(Int16 countryId, Byte languageId, Int32 memberId);
        Task<String> generatePassword(String password);
        Task<Int64> saveMemberAddress(Int64 visitorDbId, String fullName, String companyName, String addressLineOne, String addressLineTwo, String addressTown, String addressCounty, String addressPostCode, Int16 addressCountryId, String addressPhone);
        Task<MemberAddressSelect> getMemberAddresses(Int64 visitorDbId, Int64? memberAddressId, Byte languageId, Int16? countryId, DateTime contentDate);
        Task<Boolean> updateMemberAddress(Int64 memberAddressId, String fullName, String companyName, String addressLineOne, String addressLineTwo, String addressTown, String addressCounty, String addressPostCode, Int16 addressCountryId, String addressPhone);
        Task<Boolean> saveMemberThirdPartyPreference(Int32 memberId, Boolean preference);
        Task<Boolean> deleteMemberAddress(Int64 memberAddressId);
        Task<Boolean> updateMemberPassword(Int64 visitorDbId, String currentPasswordEnc, String newPasswordEnc);
        Task<Boolean> updateMemberEmail(Int64 visitorDbId, String newEmail);
        Task<Boolean> updateMemberDetail(Int64 visitorDbId, String fullName);
        Task<Boolean> passwordResetRequest(PassPasswordResetRequest objRequest);
        Task<Boolean> resetPassword(String token, String newPasswordEnc);
        Task<OrderHistory> getOrderHistory(Int64 visitorDbId, countryItem objCountryItem, String searchString, Int16? historyDays, Byte? OrderStatusId, String RequestDateOverride, int? transactionId);
        Task<memberReturnPaperworkGenerate> getReturnPaperwork(Int32 returnHeadId, Byte languageId, Int64 visitorDbId);
        Task<Int32> saveReturn(Int64 visitorDbId, Int32 orderHeadId, DataTable returnTableType);
        Task<List<ScrapbookImage>> getVisitorScrapbook(Int64 visitorDbId, Int32? memberId, Byte? productTypeId, Byte? brandId, Boolean? isNew, Int16 countryID, Byte languageID, DateTime contentDate);
        Task<Boolean> saveImageToScrapbook(Int64 visitorDbId, Int32? memberId, Int64 imageId, Int16 countryId, Int32? productPageId);
        Task<Boolean> removeImageFromScrapbook(Int64 visitorDbId, Int32? memberId, Int16 countryId, Int64 imageId);
        Task<Boolean> clearMoodboard(Int32 memberId, Int32 moodboardId);
        Task<Int32> createMoodboard(Int32 memberId, Int16 countryId);
        Task<Boolean> deleteMoodboard(Int32 memberId, Int32 moodboardId);
        Task<Boolean> addImageToMoodboard(Int32 moodboardId, Int64 imageId, Int32? productPageId);
        Task<Boolean> removeImageFromMoodboard(Int32 moodboardId, Int64 imageId);
        Task<Boolean> clearScrapbook(Int64 visitorDbId, Int32? memberId, Int16 countryId);
        Task<MoodboardContainer> searchMoodboards(Int32 memberId, String customerName, Byte? roomId, String reference, Int16 countryId, Byte languageId, DateTime? contentDate);
        Task<MoodboardContainer> getMemberMoodboards(Int32 memberId, Int16 countryID, Byte languageID, DateTime contentDate);
        Task<Boolean> saveMoodboard(Int32 moodboardId, Int16 height, String json, String customerName, Byte? roomId, String reference);
        Task<Boolean> saveMoodboardFromGridView(Int32 moodboardId, String customerName, Byte? roomId, String reference);
        Task<Moodboard> fetchMoodboard(Int32 moodboardId);
        Task<DatabaseJSONResponse> fetchMoodboardProducts(int memberId, int? tradeAccountId, bool displayTradePrice, PassMoodboardOrderingFetch objFetch);
        Task<Boolean> SendMoodboardEmail(MoodboardShareEmail objEmail);
        Task<memberReceiptGenerate> getReceiptData(Int64 visitorDbId, Int32 transactionId);
        Task<Boolean> auditPostCodeLookupSearch(Int64 visitorDbId, String countryCode, String postcode, String addressLine1);
        Task<countryAwarenessCheckReturn> countryAwarenessCheck(Int64 visitorDbId, Int16 countryId, DateTime contentDate, Int16? countryIdOverride);
        Task<MemberDetail> getMemberByGuid(Guid memberGuid);
        Task<MemberDetail> getMemberById(int memberId);
    }
}
