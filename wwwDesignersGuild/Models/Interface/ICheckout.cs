using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Checkout;
using wwwDesignersGuild.Models.Classes;


namespace wwwDesignersGuild.Models.Interface
{
    public interface ICheckout
    {
        Task<checkoutAddressGenerateResult> checkoutAddressGenerate(Int64 checkoutId, short countryId, byte languageId, long visitorId, int? memberId, int? tradeAccountId);
        Task<Boolean> checkoutAddressSave(Int64 checkoutId, List<passCheckoutSaveAddressData> checkoutAddresses, Int64? UPSAPMemberAddressId, List<passCheckoutSaveAddressData> upsIDAddresses, bool isClickCollect);
        //Task<Boolean> validateCheckoutBasket(decimal totalPrice, int totalQuantity, DateTime snapshotCurrentDateTime, short countryId, byte languageId, Int64 visitorDbId, Int32? memberId, int? tradeAccountID);
        Task<checkoutCreateSnapshotBasket> createCheckoutBasket(long visitorDbId, byte languageId, short countryId, DateTime date, string cfCountryHeader, int? tradeAccountId);
        Task<Boolean> logCheckoutAction(Int64 checkoutId, int checkoutActionId, string details);
        Task<checkoutDispatchOptions> getDispatchOptions(Int64 checkoutId);
        Task<checkoutDeliveryEstimate> getCheckoutDeliveryDateEstimate(Int64 checkoutId, short? deliveryBandId, byte dispatchId, Boolean partShip);
        Task<Boolean> saveDispatchOptions(Int64 checkoutId, DataTable dispatchTableType);
        Task<CheckoutPayment> checkoutPayGenerate(Int64 checkoutId, short countryId, byte languageId, string origin);
        Task<Boolean> saveGiftVoucherOptions(Int64 checkoutId, DataTable giftVoucherTableType);
        Task<checkoutGiftCard> getGiftVoucherData(Int64 checkoutId);
        Task<checkoutGift> getGiftData(Int64 checkoutId, short countryId, byte languageId);
        Task<Boolean> saveGiftOptions(Int64 checkoutId, DataTable giftBoxTableType, DataTable giftBasketTableType);
        Task<Boolean> checkoutPaymentTaken(long checkoutId, string transactionId, string paymentType, string paymentDetails, List<MarketingPermission> marketingPermissions, string orderReference = null);
        Task<Boolean> checkoutPaySaveRedux(long checkoutId);
        Task<Boolean> checkoutPaySave(Guid paymentToken, string transactionId, string paymentType, string paymentDetails, List<MarketingPermission> marketingPermissions);
        Task<Boolean> checkoutAttemptSave(Int64 checkoutId,
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
            String resultDetails);
        Task<Boolean> preflightCheckWelcome(Int64 checkoutId, short countryId);
        Task<preflightCheckAddressResult> preflightCheckAddress(Int64 checkoutId, short countryId);
        //Task<Boolean> preflightCheckDispatch(Int64 checkoutId, short countryId);
        //Task<Boolean> preflightCheckGiftVoucher(Int64 checkoutId, short countryId);
        //Task<Boolean> preflightCheckGiftBox(Int64 checkoutId, short countryId);
        //Task<Boolean> preflightCheckSummary(Int64 checkoutId, short countryId);
        Task<Boolean> preflightCheckPay(Int64 checkoutId, short countryId);
        Task<Boolean> preflightCheckReceipt(Guid PaymentToken, short countryId);
        Task<String> validateGiftVoucherCode(Int64 checkoutId, string voucherCode, byte languageId);
        Task<Boolean> insertGiftVoucherCode(Int64 checkoutId, string voucherCode);
        Task<Boolean> deleteGiftVoucherCode(Int64 checkoutId, Int32 giftVoucherId);
        Task<Byte> checkoutPaidInFullCheck(Guid paymentToken, Int64 checkoutId);
    }
}
