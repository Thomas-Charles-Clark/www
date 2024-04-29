using Braintree;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Interface;

namespace wwwDesignersGuild.Models.Checkout
{
    public class checkoutDb : ICheckout
    {
        #region Get Delivery Options


        public async Task<checkoutAddressGenerateResult> checkoutAddressGenerate(Int64 checkoutId, short countryId, byte languageId, long visitorId, int? memberId, int? tradeAccountId)
        {
            var deliveryOptions = await Task.FromResult(checkoutAddressGenerateAsync(checkoutId, countryId, languageId, visitorId, memberId, tradeAccountId));
            return deliveryOptions;
        }


        private checkoutAddressGenerateResult checkoutAddressGenerateAsync(Int64 checkoutId, short countryId, byte languageId, long visitorId, int? memberId, int? tradeAccountId)
        {
            var dataContext = new databaseDataContext();
            IMultipleResults results = dataContext.CheckoutAddress_GenerateMultiple(checkoutId, countryId, languageId, visitorId, memberId, tradeAccountId);

            var result = new checkoutAddressGenerateResult
            {
                deliveryItems = results.GetResult<checkoutDeliveryItem>().ToList(),
                //result.deliveryCosts = results.GetResult<checkoutDeliveryCost>().ToList();
                createSnapshotBaskets = results.GetResult<checkoutCreateSnapshotBasket>().ToList(),
                selectedAddresses = results.GetResult<checkoutDeliveryAddress>().ToList(),
                //result.defaultAddress = results.GetResult<memberAddress>().ToList();
                customerServiceDetails = results.GetResult<checkoutSummaryCustomerServiceDetails>().ToList(),

                collectFromKingsRoad = results.GetResult<bool>().Single(),
                collectFromMarylebone = results.GetResult<bool>().Single(),
                collectFromWarehouse = results.GetResult<bool>().Single()
            };

            return result;
        }


        #endregion


        #region Get Checkout Types

        public async Task<Boolean> checkoutTypeIsSet(Int64 checkoutId)
        {
            Boolean result = await Task.FromResult<Boolean>(checkoutTypeIsSetAsync(checkoutId));
            return result;
        }


        private Boolean checkoutTypeIsSetAsync(Int64 checkoutId)
        {
            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.checkoutTypeIsSet(checkoutId);
        }

        public async Task<CheckoutTypeResponse> GetCheckoutTypes(short countryId, byte languageId, DateTime date, bool mobile, long visitorId, int? memberId, int? tradeAccountId)
        {
            var result = await Task.FromResult<CheckoutTypeResponse>(getCheckoutTypesAsync(countryId, languageId, date, mobile, visitorId, memberId, tradeAccountId));
            return result;
        }

        private CheckoutTypeResponse getCheckoutTypesAsync(short countryId, byte languageId, DateTime date, bool mobile, long visitorId, int? memberId, int? tradeAccountId)
        {
            var dataContext = new databaseDataContext();
            IMultipleResults results = dataContext.CheckoutType_Select(countryId, languageId, date, mobile, visitorId, memberId, tradeAccountId);
            return new CheckoutTypeResponse()
            {
                CheckoutTypes = results.GetResult<CheckoutType>().ToList(),
                CheckoutTypeNotice = results.GetResult<CheckoutTypeNotice>().Single()
            };
        }

        public async Task<dgCheckoutType> GetCheckoutType(Int64 checkoutId)
        {
            return await Task.FromResult<dgCheckoutType>(getCheckoutTypeAsync(checkoutId));
        }

        private dgCheckoutType getCheckoutTypeAsync(Int64 checkoutId)
        {
            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.getCheckoutType(checkoutId);
        }
        
        public async Task<checkoutProductSelectResult> GetCheckoutProducts(Int64 checkoutId)
        {
            return await Task.FromResult(getCheckoutProductsAsync(checkoutId));
        }
        
        private checkoutProductSelectResult getCheckoutProductsAsync(Int64 checkoutId)
        {
            var dataContext = new databaseDataContext();
            IMultipleResults results = dataContext.Checkout_Products_Select(checkoutId);
            var result = new checkoutProductSelectResult
            {
                products = results.GetResult<gtagProduct>().ToList(),
                promoCodes = results.GetResult<promotionCode>().ToList()
            };
            return result;
        }

        #endregion


        #region Get Merchant Details

        public async Task<MerchantInfoSelectResult> getMerchantDetails(Int64 checkoutId)
        {
            var result = await Task.FromResult(getMerchantDetailsAsync(checkoutId));
            return result;
        }


        private MerchantInfoSelectResult getMerchantDetailsAsync(Int64 checkoutId)
        {
            var objDbCheckout = new databaseDataContext();

            var result = new MerchantInfoSelectResult();

            var results = objDbCheckout.Checkout_MerchantInfo_Select(checkoutId).ToList();
            
            var sprocresult = results.Single();

            result.checkoutParameters = sprocresult;
                
            return result;
        }
        #endregion        


        #region Save Checkout Type
        public async Task<Boolean> checkoutSaveType(Int64 checkoutId, DateTime date, dgCheckoutType type, int memberId, string email, int? siteMemberId, string loginStatus, int? tradeAccountId)
        {
            Boolean result = await Task.FromResult<Boolean>(checkoutSaveTypeAsync(checkoutId, date, type, memberId, email, siteMemberId, loginStatus, tradeAccountId));
            return result;
        }


        private Boolean checkoutSaveTypeAsync(Int64 checkoutId, DateTime date, dgCheckoutType type, int memberId, string email, int? siteMemberId, string loginStatus, int? tradeAccountId)
        {
            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.saveCheckoutType(checkoutId, date, type, memberId, email, siteMemberId, loginStatus, tradeAccountId);
        }
        #endregion


        #region Get Checkout Address


        public async Task<checkoutDeliveryAddress> checkoutAddressSelect(long checkoutId, byte languageId)
        {
            var address = await Task.FromResult(checkoutAddressSelectAsync(checkoutId, languageId));
            return address;
        }


        private checkoutDeliveryAddress checkoutAddressSelectAsync(long checkoutId, byte languageId)
        {
            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.SelectDeliveryAddress(checkoutId, languageId);
        }


        #endregion


        #region Save Checkout Delivery Address


        public async Task<Boolean> checkoutAddressSave(Int64 checkoutId, List<passCheckoutSaveAddressData> checkoutAddresses, Int64? UPSAPMemberAddressId, List<passCheckoutSaveAddressData> upsIDAddress, bool isClickCollect, bool resetDeliveryOptions = true)
        {
            Boolean deliveryOptions = await Task.FromResult<Boolean>(checkoutAddressSaveAsync(checkoutId, checkoutAddresses, UPSAPMemberAddressId, upsIDAddress, isClickCollect, resetDeliveryOptions));
            return deliveryOptions;
        }


        private Boolean checkoutAddressSaveAsync(Int64 checkoutId, List<passCheckoutSaveAddressData> checkoutAddresses, Int64? UPSAPMemberAddressId, List<passCheckoutSaveAddressData> upsIDAddress, bool isClickCollect, bool resetDeliveryOptions)
        {
            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.saveCheckoutAddress(checkoutId, checkoutAddresses, UPSAPMemberAddressId, upsIDAddress, isClickCollect, resetDeliveryOptions);
        }


        public async Task<Boolean> deleteCheckoutAddresses(Int64 checkoutId)
        {
            Boolean deliveryOptions = await Task.FromResult<Boolean>(deleteCheckoutAddressesAsync(checkoutId));
            return deliveryOptions;
        }


        private Boolean deleteCheckoutAddressesAsync(Int64 checkoutId)
        {
            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.deleteCheckoutAddresses(checkoutId);
        }




        #endregion


        #region Validate Basket Quantities

        public async Task<DatabaseJSONResponse> validateBasket(
            long visitorId, 
            int? memberId, 
            int? tradeAccountId,
            byte languageId,
            short countryId,
            DateTime useDate,
            bool displayTradePrice)
        {
            var result = await Task.FromResult<DatabaseJSONResponse>(validateBasketAsync(visitorId, memberId, tradeAccountId, languageId, countryId, useDate, displayTradePrice));
            return result;
        }

        private DatabaseJSONResponse validateBasketAsync(
            long visitorId,
            int? memberId,
            int? tradeAccountId,
            byte languageId,
            short countryId,
            DateTime useDate,
            bool displayTradePrice)
        {
            var db = new Database.checkoutDb();
            return db.validateBasket(visitorId, memberId, tradeAccountId, useDate, countryId, languageId, displayTradePrice);
        }

        #endregion


        #region Validate Basket


        //public async Task<Boolean> validateCheckoutBasket(decimal totalPrice, int totalQuantity, DateTime snapshotCurrentDateTime, short countryId, byte languageId, Int64 visitorDbId, Int32? memberId, int? tradeAccountID)
        //{
        //    Boolean storePassResults = await Task.FromResult<Boolean>(validateCheckoutBasketAsync(totalPrice, totalQuantity, snapshotCurrentDateTime, countryId, languageId, visitorDbId, memberId, tradeAccountID));
        //    return storePassResults;
        //}


        //private Boolean validateCheckoutBasketAsync(decimal totalPrice, int totalQuantity, DateTime snapshotCurrentDateTime, short countryId, byte languageId, Int64 visitorDbId, Int32? memberId, int? tradeAccountID)
        //{
        //    checkoutDb objDbCheckout = new checkoutDb();
        //    Boolean storePassResults = objDbCheckout.validateCheckoutBasket(totalPrice, totalQuantity, snapshotCurrentDateTime, countryId, languageId, visitorDbId, memberId, tradeAccountID);
        //    return storePassResults;
        //}

        #endregion


        #region Create Basket


        public async Task<checkoutCreateSnapshotBasket> createCheckoutBasket(
            long visitorDbId,
            byte languageId,
            short countryId,
            DateTime date,
            string cfCountryHeader,
            int? tradeAccountId)
        {
            checkoutCreateSnapshotBasket storeResults = await Task.FromResult<checkoutCreateSnapshotBasket>(createCheckoutBasketAsync(
                visitorDbId, 
                languageId, 
                countryId,
                date,
                cfCountryHeader,
                tradeAccountId));
            return storeResults;
        }

        private checkoutCreateSnapshotBasket createCheckoutBasketAsync(
            long visitorDbId,
            byte languageId,
            short countryId,
            DateTime date,
            string cfCountryHeader,
            int? tradeAccountId)
        {
            var objDbCheckout = new Database.checkoutDb();
            checkoutCreateSnapshotBasket storeResults = objDbCheckout.createCheckoutBasket(
                visitorDbId, 
                languageId, 
                countryId, 
                date,
                cfCountryHeader,
                tradeAccountId);
            return storeResults;
        }


        #endregion        


        #region Log Checkout Action


        public async Task<Boolean> logCheckoutAction(Int64 checkoutId, int checkoutActionId, string details)
        {
            Boolean logSuccess = await Task.FromResult<Boolean>(logCheckoutActionAsync(checkoutId, checkoutActionId, details));
            return logSuccess;
        }


        private Boolean logCheckoutActionAsync(Int64 checkoutId, int checkoutActionId, string details)
        {
            var objDbCheckout = new Database.checkoutDb();
            Boolean storeResults = objDbCheckout.checkoutLogInsert(checkoutId, checkoutActionId, details);
            return storeResults;
        }


        #endregion


        #region Get Dispatch Options


        public async Task<checkoutDispatchOptions> getDispatchOptions(Int64 checkoutId)
        {
            checkoutDispatchOptions dispatchOptions = await Task.FromResult<checkoutDispatchOptions>(getDispatchOptionsAsync(checkoutId));
            return dispatchOptions;
        }


        private checkoutDispatchOptions getDispatchOptionsAsync(Int64 checkoutId)
        {
            var dataContext = new databaseDataContext();
            IMultipleResults results = dataContext.DispatchOptions_GenerateMultiple(checkoutId);


            var dispatchOptions = new checkoutDispatchOptions
            {
                checkoutDeliveryAddresses = results.GetResult<checkoutDeliveryAddress>().ToList(),
                checkoutDispatchGroupedBaskets = results.GetResult<checkoutDispatchGroupedBasket>().ToList(),
                checkoutDispatchDeliveryOptions = results.GetResult<checkoutDispatchDeliveryOption>().ToList(),
                checkoutDispatchDeliverySelections = results.GetResult<checkoutDispatchDeliverySelection>().ToList(),
                checkoutDispatchStockStatusLines = results.GetResult<checkoutDispatchStockStatusLine>().ToList(),
                checkoutDispatchPartShipPrices = results.GetResult<checkoutDispatchPartShipPrice>().ToList(),
                checkoutDispatchNotice = results.GetResult<checkoutDispatchNotice>().SingleOrDefault(),
                checkoutDispatchAppleDeliveryOptions = results.GetResult<checkoutDispatchAppleDeliveryOption>().ToList()
            };

            return dispatchOptions;
        }


        #endregion


        #region Save Dispatch Options


        public async Task<Boolean> saveDispatchOptions(Int64 checkoutId, DataTable dispatchTableType)
        {
            Boolean dispatchOptions = await Task.FromResult<Boolean>(saveDispatchOptionsAsync(checkoutId, dispatchTableType));
            return dispatchOptions;
        }


        private Boolean saveDispatchOptionsAsync(Int64 checkoutId, DataTable dispatchTableType)
        {

            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.saveCheckoutDispatch(checkoutId, dispatchTableType);

        }


        #endregion


        #region Get Checkout Delivery Date Estimate


        public async Task<checkoutDeliveryEstimate> getCheckoutDeliveryDateEstimate(Int64 checkoutId, short? deliveryBandId, byte dispathId, Boolean partShip)
        {
            var estDeliveryDate = await Task.FromResult(getCheckoutDeliveryDateEstimateAsync(checkoutId, deliveryBandId, dispathId, partShip));
            return estDeliveryDate;
        }


        private checkoutDeliveryEstimate getCheckoutDeliveryDateEstimateAsync(Int64 checkoutId, short? deliveryBandId, byte dispathId, Boolean partShip)
        {
            var dataContext = new databaseDataContext();

            var result = dataContext.checkoutEstimateDeliveryDates(checkoutId, deliveryBandId, dispathId, partShip);
            
            return result;
        }


        #endregion


        #region Get Checkout Payment Totals

        public async Task<CheckoutPayment> checkoutPayGenerate(Int64 checkoutId, short countryId, byte languageId, string origin)
        {
            CheckoutPayment objPaymentTotal = await Task.FromResult(getPaymentTotalAsync(checkoutId, countryId, languageId, origin));
            return objPaymentTotal;
        }

        private CheckoutPayment getPaymentTotalAsync(Int64 checkoutId, short countryId, byte languageId, string origin)
        {
            var dataContext = new databaseDataContext();

            IMultipleResults results = dataContext.getPaymentTotals_GenerateMultiple(checkoutId, countryId, languageId, origin);

            CheckoutPayment objPaymentTotal = new CheckoutPayment
            {
                paymentTotals = results.GetResult<checkoutPaymentTotal>().ToList(),
                checkoutDetails = results.GetResult<checkoutDetails>().ToList(),
                deliveryAddresses = results.GetResult<checkoutDeliveryAddress>().ToList(),
                marketingPermissions = results.GetResult<MarketingPermission>().ToList()
            };

            return objPaymentTotal;
        }

        #endregion


        #region Gift Voucher Redeem


        public async Task<giftVoucherInBasket> getGiftVouchersInBasket(Int64 checkoutId)
        {
            giftVoucherInBasket objGiftVoucherList = await Task.FromResult<giftVoucherInBasket>(getGiftVouchersInBasketAsync(checkoutId));
            return objGiftVoucherList;
        }


        private giftVoucherInBasket getGiftVouchersInBasketAsync(Int64 checkoutId)
        {
            var dataContext = new databaseDataContext();
            IMultipleResults results = dataContext.getGiftVoucherDetails_GenerateMultiple(checkoutId);


            //  Convert from IEnumerable to a List so a query can be run multiple times
            giftVoucherInBasket objGiftVoucherList = new giftVoucherInBasket
            {
                giftVoucherDetails = results.GetResult<giftVoucherDetail>().ToList<giftVoucherDetail>()
            };
            return objGiftVoucherList;
        }


        #endregion


        #region Gift Voucher Code Validate

        public async Task<String> validateGiftVoucherCode(Int64 checkoutId, string voucherCode, byte languageId)
        {
            String storePassResults = await Task.FromResult<String>(validateGiftVoucherCodeAsync(checkoutId, voucherCode, languageId));
            return storePassResults;
        }


        private String validateGiftVoucherCodeAsync(Int64 checkoutId, string voucherCode, byte languageId)
        {
            var objDbCheckout = new Database.checkoutDb();
            String storePassResults = objDbCheckout.validateGiftVoucherCode(checkoutId, voucherCode, languageId);
            return storePassResults;
        }


        #endregion


        #region Gift Voucher Insert

        public async Task<Boolean> insertGiftVoucherCode(Int64 checkoutId, string voucherCode)
        {
            Boolean deliveryOptions = await Task.FromResult<Boolean>(insertGiftVoucherCodeAsync(checkoutId, voucherCode));
            return deliveryOptions;
        }


        private Boolean insertGiftVoucherCodeAsync(Int64 checkoutId, string voucherCode)
        {
            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.saveGiftVoucherCode(checkoutId, voucherCode);
        }


        #endregion


        #region Gift Voucher Delete


        public async Task<Boolean> deleteGiftVoucherCode(Int64 checkoutId, Int32 giftVoucherId)
        {
            Boolean deliveryOptions = await Task.FromResult<Boolean>(deleteGiftVoucherCodeAsync(checkoutId, giftVoucherId));
            return deliveryOptions;
        }


        private Boolean deleteGiftVoucherCodeAsync(Int64 checkoutId, Int32 giftVoucherId)
        {
            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.deleteGiftVoucherCode(checkoutId, giftVoucherId);
        }


        #endregion
        

        #region Get Checkout Gift Voucher


        public async Task<checkoutGiftCard> getGiftVoucherData(Int64 checkoutId)
        {
            checkoutGiftCard objGiftVoucherData = await Task.FromResult<checkoutGiftCard>(getGiftVoucherDataAsync(checkoutId));
            return objGiftVoucherData;
        }


        private checkoutGiftCard getGiftVoucherDataAsync(Int64 checkoutId)
        {
            var dataContext = new databaseDataContext();
            IMultipleResults results = dataContext.getGiftVoucherData_GenerateMultiple(checkoutId);


            //  Convert from IEnumerable to a List so a query can be run multiple times
            checkoutGiftCard objGiftVoucherData = new checkoutGiftCard
            {
                checkoutGiftVoucherBaskets = results.GetResult<checkoutGiftVoucherBasket>().ToList<checkoutGiftVoucherBasket>()
            };
            return objGiftVoucherData;
        }


        #endregion


        #region Get Checkout Gift


        public async Task<checkoutGift> getGiftData(Int64 checkoutId, short countryId, byte languageId)
        {
            checkoutGift objGiftData = await Task.FromResult<checkoutGift>(getGiftDataAsync(checkoutId, countryId, languageId));
            return objGiftData;
        }


        private checkoutGift getGiftDataAsync(Int64 checkoutId, short countryId, byte languageId)
        {
            var dataContext = new databaseDataContext();
            IMultipleResults results = dataContext.getGiftData_GenerateMultiple(checkoutId, countryId, languageId);


            //  Convert from IEnumerable to a List so a query can be run multiple times
            checkoutGift objGiftData = new checkoutGift
            {
                checkoutGiftGroupedBaskets = results.GetResult<checkoutGiftGroupedBasket>().ToList<checkoutGiftGroupedBasket>(),
                checkoutGiftBoxs = results.GetResult<checkoutGiftBox>().ToList<checkoutGiftBox>(),
                checkoutGiftBoxItems = results.GetResult<checkoutGiftBoxItem>().ToList(),
                checkoutGiftBoxPrices = results.GetResult<checkoutGiftBoxPrice>().ToList<checkoutGiftBoxPrice>()
            };
            return objGiftData;
        }


        #endregion


        #region Save Voucher Options


        public async Task<Boolean> saveGiftVoucherOptions(Int64 checkoutId, DataTable giftVoucherTableType)
        {
            Boolean giftVoucherOptions = await Task.FromResult<Boolean>(saveGiftVoucherOptionsAsync(checkoutId, giftVoucherTableType));
            return giftVoucherOptions;
        }


        private Boolean saveGiftVoucherOptionsAsync(Int64 checkoutId, DataTable giftVoucherTableType)
        {

            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.saveCheckoutGiftVoucher(checkoutId, giftVoucherTableType);

        }


        #endregion


        #region Save Gift Options


        public async Task<Boolean> saveGiftOptions(Int64 checkoutId, DataTable giftBoxTableType, DataTable giftBasketTableType)
        {
            Boolean giftOptions = await Task.FromResult<Boolean>(saveGiftOptionsAsync(checkoutId, giftBoxTableType, giftBasketTableType));
            return giftOptions;
        }


        private Boolean saveGiftOptionsAsync(Int64 checkoutId, DataTable giftBoxTableType, DataTable giftBasketTableType)
        {

            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.saveCheckoutGift(checkoutId, giftBoxTableType, giftBasketTableType);

        }


        #endregion
        

        #region Get Checkout Receipt


        public async Task<checkoutReceipt> getPaymentReceipt(Guid paymentToken, Int64 visitorDbId, short countryId, byte languageId, DateTime useDate, int? tradeAccountId, bool isTrade, bool displayTradePrice)
        {
            checkoutReceipt objPaymentReceipt = await Task.FromResult<checkoutReceipt>(getPaymentReceiptAsync(paymentToken, visitorDbId, countryId, languageId, useDate, tradeAccountId, isTrade, displayTradePrice));

            var stockStatus = new cmsStockStatus();
            _ = await stockStatus.loadStockStatusesFromCms(languageId, countryId, useDate);

            return objPaymentReceipt;
        }


        private checkoutReceipt getPaymentReceiptAsync(Guid paymentToken, Int64 visitorDbId, short countryId, byte languageId, DateTime useDate, int? tradeAccountId, bool isTrade, bool displayTradePrice)
        {
            var dataContext = new databaseDataContext();
            IMultipleResults results = dataContext.GenerateReceipt_GenerateMultiple(paymentToken, visitorDbId, countryId, languageId, useDate, tradeAccountId, isTrade, displayTradePrice);

            var objPaymentReceipt = new checkoutReceipt
            {
                ReceiptViewed = results.GetResult<bool>().Single(),
                checkoutReceiptOrders = results.GetResult<checkoutReceiptOrder>().ToList(),
                checkoutReceiptTotalExVats = results.GetResult<checkoutReceiptTotalExVat>().ToList(),
                checkoutReceiptTransactionDetails = results.GetResult<checkoutReceiptTransactionDetail>().ToList(),
                checkoutReceiptOrderLines = results.GetResult<checkoutReceiptOrderLine>().ToList(),
                checkoutReceiptBrontoLines = results.GetResult<brontoJSONBasketLine>().ToList(),
                checkoutReceiptOrderReferences = results.GetResult<checkoutReceiptOrderReference>().ToList()
            };

            return objPaymentReceipt;
        }

        public int receiptViewed(long checkoutId)
        {
            var db = new databaseDataContext();

            return db.checkout_ReceiptViewed(checkoutId);
        }
        
        #endregion


        #region Pre-flight Checks


        public async Task<Boolean> preflightCheckWelcome(Int64 checkoutId, short countryId)
        {
            Boolean preflightCheck = await Task.FromResult<Boolean>(preflightCheckWelcomeAsync(checkoutId, countryId));
            return preflightCheck;
        }


        private Boolean preflightCheckWelcomeAsync(Int64 checkoutId, short countryId)
        {

            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.preflightCheckWelcome(checkoutId, countryId);

        }


        public async Task<preflightCheckAddressResult> preflightCheckAddress(Int64 checkoutId, short countryId)
        {
            preflightCheckAddressResult preflightCheck = await Task.FromResult<preflightCheckAddressResult>(preflightCheckAddressAsync(checkoutId, countryId));
            return preflightCheck;
        }


        private preflightCheckAddressResult preflightCheckAddressAsync(Int64 checkoutId, short countryId)
        {

            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.preflightCheckAddress(checkoutId, countryId);

        }


        //public async Task<Boolean> preflightCheckDispatch(Int64 checkoutId, short countryId)
        //{
        //    Boolean preflightCheck = await Task.FromResult<Boolean>(preflightCheckDispatchAsync(checkoutId, countryId));
        //    return preflightCheck;
        //}


        //private Boolean preflightCheckDispatchAsync(Int64 checkoutId, short countryId)
        //{

        //    checkoutDb objDbCheckout = new checkoutDb();
        //    return objDbCheckout.preflightCheckDispatch(checkoutId, countryId);

        //}


        
        //public async Task<Boolean> preflightCheckGiftVoucher(Int64 checkoutId, short countryId)
        //{
        //    Boolean preflightCheck = await Task.FromResult<Boolean>(preflightCheckGiftVoucherAsync(checkoutId, countryId));
        //    return preflightCheck;
        //}


        //private Boolean preflightCheckGiftVoucherAsync(Int64 checkoutId, short countryId)
        //{

        //    checkoutDb objDbCheckout = new checkoutDb();
        //    return objDbCheckout.preflightCheckGiftVoucher(checkoutId, countryId);

        //}


        //public async Task<Boolean> preflightCheckGiftBox(Int64 checkoutId, short countryId)
        //{
        //    Boolean preflightCheck = await Task.FromResult<Boolean>(preflightCheckGiftBoxAsync(checkoutId, countryId));
        //    return preflightCheck;
        //}


        //private Boolean preflightCheckGiftBoxAsync(Int64 checkoutId, short countryId)
        //{

        //    checkoutDb objDbCheckout = new checkoutDb();
        //    return objDbCheckout.preflightCheckGiftBox(checkoutId, countryId);

        //}


        //public async Task<Boolean> preflightCheckSummary(Int64 checkoutId, short countryId)
        //{
        //    Boolean preflightCheck = await Task.FromResult<Boolean>(preflightCheckSummaryAsync(checkoutId, countryId));
        //    return preflightCheck;
        //}


        //private Boolean preflightCheckSummaryAsync(Int64 checkoutId, short countryId)
        //{

        //    checkoutDb objDbCheckout = new checkoutDb();
        //    return objDbCheckout.preflightCheckSummary(checkoutId, countryId);

        //}


        public async Task<Boolean> preflightCheckPay(Int64 checkoutId, short countryId)
        {
            Boolean preflightCheck = await Task.FromResult<Boolean>(preflightCheckPayAsync(checkoutId, countryId));
            return preflightCheck;
        }


        private Boolean preflightCheckPayAsync(Int64 checkoutId, short countryId)
        {

            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.preflightCheckPay(checkoutId, countryId);

        }


        public async Task<Boolean> preflightCheckReceipt(Guid PaymentToken, short countryId)
        {
            Boolean preflightCheck = await Task.FromResult<Boolean>(preflightCheckReceiptAsync(PaymentToken, countryId));
            return preflightCheck;
        }


        private Boolean preflightCheckReceiptAsync(Guid PaymentToken, short countryId)
        {

            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.preflightCheckReceipt(PaymentToken, countryId);

        }

        #endregion



        #region Checkout Zero-Value Order Check (Paid in Full with Gift Voucher / Zero Value Trade Order)

        public async Task<Byte> checkoutPaidInFullCheck(Guid paymentToken, Int64 checkoutId)
        {
            Byte canProceed = await Task.FromResult<Byte>(checkoutPaidInFullCheckAsync(paymentToken, checkoutId));
            return canProceed;
        }


        private Byte checkoutPaidInFullCheckAsync(Guid paymentToken, Int64 checkoutId)
        {
            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.checkoutPaidInFullCheck(paymentToken, checkoutId);
        }

        #endregion

        #region Checkout Payment Taken
        
        public async Task<Boolean> checkoutPaymentTaken(long checkoutId, string paymentRef, string paymentType, string paymentDetails, List<MarketingPermission> marketingPermissions, string orderReference = null, string email = null, string phone = null)
        {
            Boolean deliveryOptions = await Task.FromResult<Boolean>(checkoutPaymentTakenAsync(checkoutId, paymentRef, paymentType, paymentDetails, marketingPermissions, orderReference, email, phone));
            return deliveryOptions;
        }


        private Boolean checkoutPaymentTakenAsync(long checkoutId, string paymentRef, string paymentType, string paymentDetails, List<MarketingPermission> marketingPermissions, string orderReference, string email, string phone = null)
        {
            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.checkoutPaymentTaken(checkoutId, paymentRef, paymentType, paymentDetails, marketingPermissions, orderReference, email, phone);
        }

        #endregion



        #region Checkout Create Order

        // DEFUNCT
        public async Task<Boolean> checkoutPaySave(Guid paymentToken, String transactionId, String paymentType, String paymentDetails, List<MarketingPermission> marketingPermissions)
        {
            Boolean deliveryOptions = await Task.FromResult<Boolean>(checkoutPaySaveAsync(paymentToken, transactionId, paymentType, paymentDetails, marketingPermissions));
            return deliveryOptions;
        }

        // DEFUNCT
        private Boolean checkoutPaySaveAsync(Guid paymentToken, String transactionId, String paymentType, String paymentDetails, List<MarketingPermission> marketingPermissions)
        {
            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.checkoutSavePay(paymentToken, transactionId, paymentType, paymentDetails, marketingPermissions);
        }


        #endregion


        #region Checkout Create Order Redux


        public async Task<Boolean> checkoutPaySaveRedux(long checkoutId)
        {
            Boolean deliveryOptions = await Task.FromResult<Boolean>(checkoutPaySaveReduxAsync(checkoutId));
            return deliveryOptions;
        }


        private Boolean checkoutPaySaveReduxAsync(long checkoutId)
        {
            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.checkoutComplete(checkoutId);
        }


        #endregion

        #region Checkout Record Failed Attempt
        public async Task<Boolean> checkoutAttemptSave(
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
            Boolean saved = await Task.FromResult<Boolean>(checkoutAttemptSaveAsync(checkoutId, 
                                                                                    summaryMessage,
                                                                                    gatewayRejectionReason,
                                                                                    threeDSecureStatus,
                                                                                    threeDSecureEnrolled,
                                                                                    liabilityShifted,
                                                                                    liabilityShiftPossible,
                                                                                    processorAuthCode,
                                                                                    processorResponseCode,
                                                                                    processorResponseText,
                                                                                    additionalProcessorResponse,
                                                                                    avsPostalCodeResponseCode,
                                                                                    resultDetails));
            return saved;
        }

        public Boolean checkoutAttemptSaveAsync(
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
            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.checkoutAttemptSave(checkoutId, 
                                                     summaryMessage,
                                                     gatewayRejectionReason,
                                                     threeDSecureStatus,
                                                     threeDSecureEnrolled,
                                                     liabilityShifted,
                                                     liabilityShiftPossible,
                                                     processorAuthCode,
                                                     processorResponseCode,
                                                     processorResponseText,
                                                     additionalProcessorResponse,
                                                     avsPostalCodeResponseCode,
                                                     resultDetails);
        }

        #endregion

        #region Save Contact Referral flag
        public async Task<Boolean> saveContactReferral(Int32 TransactionID)
        {
            Boolean saved = await Task.FromResult<Boolean>(saveContactReferralAsync(TransactionID));
            return saved;
        }

        public Boolean saveContactReferralAsync(Int32 TransactionID)
        {
            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.checkoutContactReferralSave(TransactionID);
        }
        #endregion
        
        #region Checkout Payment Tokens

        public async Task<Boolean> addPaymentToken(Int64 checkoutId, string token, string bin, string lastFourDigits, string type)
        {
            Boolean result = await Task.FromResult<Boolean>(addPaymentTokenAsync(checkoutId, token, bin, lastFourDigits, type));
            return result;
        }


        private Boolean addPaymentTokenAsync(Int64 checkoutId, string token, string bin, string lastFourDigits, string type)
        {
            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.addPaymentToken(checkoutId, token, bin, lastFourDigits, type);
        }

        public async Task<List<checkoutPaymentToken>> getPaymentTokens(Int64 checkoutId)
        {
            List<checkoutPaymentToken> result = await Task.FromResult<List<checkoutPaymentToken>>(getPaymentTokensAsync(checkoutId));
            return result;
        }


        private List<checkoutPaymentToken> getPaymentTokensAsync(Int64 checkoutId)
        {
            var dataContext = new databaseDataContext();
            IMultipleResults results = dataContext.Checkout_PaymentToken_Select(checkoutId);
            return results.GetResult<checkoutPaymentToken>().ToList<checkoutPaymentToken>();
        }

        public async Task<Boolean> deletePaymentToken(Int64 checkoutId, string token)
        {
            Boolean result = await Task.FromResult<Boolean>(deletePaymentTokenAsync(checkoutId, token));
            return result;
        }


        private Boolean deletePaymentTokenAsync(Int64 checkoutId, string token)
        {
            var objDbCheckout = new Database.checkoutDb();
            return objDbCheckout.deletePaymentToken(checkoutId, token);
        }

        #endregion

        //public async Task<String> generateIframePayPalPaymentsAdvanced()
        //{
        //    return "";
        //}

        #region Helper routines


        private String truncate(String passString, Int16 maxLength)
        {
            if (passString.Length > maxLength)
            {
                passString = passString.Substring(0, maxLength);
            }

            return passString;
        }


        #endregion

    }
}