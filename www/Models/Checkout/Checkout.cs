using AmazonPay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Helper;

namespace wwwDesignersGuild.Models.Checkout
{
    public class CheckoutType
    {
        public byte CheckoutTypeId { get; set; }
        public string Name { get; set; }
        public byte SortOrder { get; set; }
    }

    public class webapiCheckoutSaveAddress
    {
        public byte? DeliveryOptionID { get; set; }
        public long? MemberAddressID { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string CountryCode { get; set; }
        public string BasketCountry { get; set; }
        public string PhoneNumber { get; set; }
        public string UpsAPid { get; set; }
        public string ZipTaxTown { get; set; }
        public string ZipTaxCounty { get; set; }
        public string ZipTaxState { get; set; }
        public long? UPSAPMemberAddressId { get; set; }
        public MemberAddress UPSAPAddress { get; set; }
        public bool IsTradeAddress { get; set; }
        public masterDefinition masterDefinition { get; set; }
        public string USCounty { get; set; }
        public bool isClickCollect { get; set; }
        public bool secondSave { get; set; }
    }

    public class webapiCheckoutSalesTaxZipCode
    {
        public Int64? MemberAddressID { get; set; }
        public string Postcode { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string USCounty { get; set; }
        public string Country { get; set; }
        public string BasketCountry { get; set; }
    }

    public class webapiGuestCheckoutSalesTaxZipCode
    {
        public string country { get; set; }
        public string postcode { get; set; }
    }


    public class checkoutDispatchOptions
    {
        public List<checkoutDeliveryAddress> checkoutDeliveryAddresses { get; set; }
        public List<checkoutDispatchGroupedBasket> checkoutDispatchGroupedBaskets { get; set; }
        public List<checkoutDispatchDeliveryOption> checkoutDispatchDeliveryOptions { get; set; }
        public List<checkoutDispatchDeliverySelection> checkoutDispatchDeliverySelections { get; set; }
        public List<checkoutDispatchStockStatusLine> checkoutDispatchStockStatusLines { get; set; }
        public List<StockStatusSelect> checkoutDispatchStockStatuses { get; set; }
        public List<checkoutDispatchPartShipPrice> checkoutDispatchPartShipPrices { get; set; }
        public checkoutDispatchNotice checkoutDispatchNotice { get; set; }
        public List<checkoutDispatchAppleDeliveryOption> checkoutDispatchAppleDeliveryOptions { get; set; }
    }


    public class checkoutDeliveryAddress
    {
        public byte CheckoutAddressID { get; set; }
        public long? MemberAddressID { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string PostCode { get; set; }
        public short CountryID { get; set; }
        public string CountryName { get; set; }
        public string PhoneNumber { get; set; }
        public string Code { get; set; }
        public bool ShouldPerformSalesTaxCheck { get; set; }
        public byte DeliveryOptionID { get; set; }
        public string UPSAPID { get; set; }
    }


    public class checkoutDispatchGroupedBasket
    {
        public long? CheckoutBasketID { get; set; }
        public int ProductID { get; set; }
        public int ProductPageID { get; set; }
        public long? ImageID { get; set; }
        public decimal Quantity { get; set; }
        public string BasketName { get; set; }
        public string Size { get; set; }
        public string Code { get; set; }
        public string PriceMessage { get; set; }
        public string DespatchMessage { get; set; }
        public string stockMessage { get; set; }
        public byte? CheckoutAddressID { get; set; }
        public byte? DispatchID { get; set; }
        public bool GiftWrappable { get; set; }
        public long? CheckoutGiftBoxID { get; set; }
        public bool IsGiftVoucher { get; set; }
        public string GiftVoucherMessage { get; set; }
        public byte BasketLineTypeID { get; set; }
        public string BasketLineType { get; set; }
        public bool PermitCutLengthBreakdown { get; set; }
        public bool PermitTreatment { get; set; }
        public bool PermitBatch { get; set; }
        public decimal? MinCutLength { get; set; }
        public decimal? MaxCutLength { get; set; }
        public string CutPieceBreakdown { get; set; }
        public string Batch { get; set; }
        public string Memo { get; set; }
        public string Units { get; set; }
        public string LineReference { get; set; }
        public string OtherTreatment { get; set; }
        public bool StainTreatment { get; set; }
        public bool DomesticUpholsteryTreatment { get; set; }
        public bool Crib5Treatment { get; set; }
        public bool ExpressTreatment { get; set; }

        public string ImageURL
        {
            get
            {
                if (ImageID == null)
                {
                    return "/image/156/-1";
                }
                else
                {
                    return $"/image/156/{ImageID}";
                }
            }
            set
            {

            }
        }

        public string FullBasketName
        {
            get
            {
                if (Size == null || Size == "" || Size == "-")
                {
                    return BasketName;
                }

                return string.Format("{0} - {1}", BasketName, Size);
            }
            set
            {

            }
        }

        public object stockMessageJson
        {
            get
            {
                return JsonConvert.DeserializeObject(string.IsNullOrEmpty(stockMessage) ? "{}" : stockMessage);
            }
            set
            {

            }
        }
    }


    public class checkoutDispatchDeliveryOption
    {
        public byte? DispatchID { get; set; }
        public short DeliveryBandID { get; set; }
        public string DispatchName { get; set; }
        public string DisplayName { get; set; }
        public byte CourierID { get; set; }
        public decimal? PriceInc { get; set; }
        public bool PerItem { get; set; }
        public long? SortOrder { get; set; }
        public bool DisplayPartShipOption { get; set; }
        public DateTime MinDeliveryEstimate { get; set; }
        public DateTime MaxDeliveryEstimate { get; set; }
        public DateTime? MinDeliveryEstimateWithPartShip { get; set; }

        public string MinDeliveryEstimateDisplay
        {
            get
            {
                return MinDeliveryEstimate.ToString("dddd dd MMMM yyyy");
            }
            set
            {

            }
        }

        public string MaxDeliveryEstimateDisplay
        {
            get
            {
                return MaxDeliveryEstimate.ToString("dddd dd MMMM yyyy");
            }
            set
            {

            }
        }

        public string MinDeliveryEstimateWithPartShipDisplay
        {
            get
            {
                return MinDeliveryEstimateWithPartShip?.ToString("dddd dd MMMM yyyy");
            }
            set
            {

            }
        }

        public string MinDeliveryDate
        {
            get
            {
                return MinDeliveryEstimate.ToString("yyyy MM dd");
            }
            set
            {

            }
        }

        public string MaxDeliveryDate
        {
            get
            {
                return MaxDeliveryEstimate.ToString("yyyy MM dd");
            }
            set
            {

            }
        }
    }


    public class checkoutDispatchAppleDeliveryOption
    {
        public byte? DispatchID { get; set; }
        public short DeliveryBandID { get; set; }
        public string DispatchName { get; set; }
        public string DisplayName { get; set; }
        public byte CourierID { get; set; }
        public decimal? PriceInc { get; set; }
        public bool PerItem { get; set; }
        public long? SortOrder { get; set; }
        public bool DisplayPartShipOption { get; set; }
        public DateTime MinDeliveryEstimate { get; set; }
        public DateTime MaxDeliveryEstimate { get; set; }
        public DateTime? MinDeliveryEstimateWithPartShip { get; set; }

        public string MinDeliveryEstimateDisplay
        {
            get
            {
                return MinDeliveryEstimate.ToString("dddd dd MMMM yyyy");
            }
            set
            {

            }
        }

        public string MaxDeliveryEstimateDisplay
        {
            get
            {
                return MaxDeliveryEstimate.ToString("dddd dd MMMM yyyy");
            }
            set
            {

            }
        }

        public string MinDeliveryEstimateWithPartShipDisplay
        {
            get
            {
                return MinDeliveryEstimateWithPartShip?.ToString("dddd dd MMMM yyyy");
            }
            set
            {

            }
        }

        public string MinDeliveryDate
        {
            get
            {
                return MinDeliveryEstimate.ToString("yyyy MM dd");
            }
            set
            {

            }
        }

        public string MaxDeliveryDate
        {
            get
            {
                return MaxDeliveryEstimate.ToString("yyyy MM dd");
            }
            set
            {

            }
        }
    }

    public class checkoutDispatchDeliverySelection
    {
        public byte? DispatchID { get; set; }
        public short? DeliveryBandID { get; set; }
        public bool? PartShip { get; set; }
        public bool OfferFullDespatch { get; set; }
        public bool CustomerRequestsFullDespatch { get; set; }
    }


    public class checkoutDispatchStockStatusLine
    {
        public int ProductID { get; set; }
        public decimal? StockQty { get; set; }
        public byte StockStatusID { get; set; }
    }


    public class checkoutDispatchStockStatus
    {
        public byte? StockStatusID { get; set; }
        public string StockStatus { get; set; }
    }


    public class checkoutDispatchPartShipPrice
    {
        public decimal? PartShipPriceInc { get; set; }
    }


    public class checkoutDispatchNotice
    {
        public string Notification { get; set; }
        public string TradeDeliveryURL { get; set; }
    }

    public class CheckoutPayment
    {
        public List<checkoutPaymentTotal> paymentTotals { get; set; }
        public List<checkoutDetails> checkoutDetails { get; set; }
        public List<checkoutDeliveryAddress> deliveryAddresses { get; set; }
        public List<MarketingPermission> marketingPermissions { get; set; }
    }


    public class checkoutPaymentTotal
    {
        public decimal? TotalItems { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal? TotalPandP { get; set; }
        public decimal? TotalTax { get; set; }
        public decimal? TotalGiftBox { get; set; }
        public decimal? TotalPayable { get; set; }
        public decimal? TotalPromotionDiscount { get; set; }
        public decimal? TotalVATExemption { get; set; }
        public decimal? TotalUSSalesTax { get; set; }
        public decimal TotalBalance { get; set; }
        public string TradeDeliveryURL { get; set; }
    }

    public class checkoutDetails
    {
        public string Email { get; set; }
        public Guid GUID { get; set; }
        public string TradeAccountNumber { get; set; }
        public string PriceList { get; set; }
        public bool PayPal { get; set; }
        public bool AVS { get; set; }
        public bool Mandate3DSecure { get; set; }
        public string Currency { get; set; }
        public string MerchantID { get; set; }
        public string MerchantAccountID { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public bool AcceptDiscover { get; set; }
        public bool AcceptMaestro { get; set; }
        public bool AcceptMaestroUK { get; set; }
        public bool AcceptMasterCard { get; set; }
        public bool AcceptVisa { get; set; }
        public bool AcceptAmex { get; set; }
        public string Environment { get; set; }
        public string AmazonClientId { get; set; }
        public string AmazonSecret { get; set; }
        public string AmazonMerchantId { get; set; }
        public Regions.supportedRegions? AmazonRegion { get; set; }
        public string AmazonPayAPIURL { get; set; }
        public string AmazonAccessKey { get; set; }
        public bool AmazonLiveAPI { get; set; }

    }

    public class checkoutPayPalCurrency
    {
        public string PayPalCurrency { get; set; }
        public string PayPalAPIUsername { get; set; }
        public string PayPalAPIPassword { get; set; }
        public string PayPalAPISignature { get; set; }
        public string PayPalAPIURL { get; set; }
        public string PayPaliFrameURL { get; set; }
    }

    public class MerchantInfoSelectResult
    {
        public CheckoutParameters checkoutParameters { get; set; }
    }

    public class CheckoutParameters
    {
        public string PriceList { get; set; }
        public bool PayPal { get; set; }
        public bool AVS { get; set; }
        public bool Mandate3DSecure { get; set; }
        public string Currency { get; set; }
        public string MerchantID { get; set; }
        public string MerchantAccountID { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public bool AcceptDiscover { get; set; }
        public bool AcceptMaestro { get; set; }
        public bool AcceptMaestroUK { get; set; }
        public bool AcceptMasterCard { get; set; }
        public bool AcceptVisa { get; set; }
        public bool AcceptAmex { get; set; }
        public string Environment { get; set; }
        public string AmazonClientId { get; set; }
        public string AmazonSecret { get; set; }
        public string AmazonMerchantId { get; set; }
        public Regions.supportedRegions? AmazonRegion { get; set; }
        public string AmazonPayAPIURL { get; set; }
        public string AmazonAccessKey { get; set; }
        public bool AmazonLiveAPI { get; set; }
    }

    public class CheckoutBraintreeParameters
    {
        public countryItem countryItem { get; set; }
        public bool paypal { get; set; }
        public bool threeDSecure { get; set; }
        public bool avs { get; set; }
        public string clientToken { get; set; }
        public string currencyCode { get; set; }
        public decimal? amount { get; set; }
        public checkoutDeliveryAddress deliveryAddress { get; set; }

        public bool acceptDiscover { get; set; }
        public bool acceptMaestro { get; set; }
        public bool acceptMaestroUK { get; set; }
        public bool acceptMasterCard { get; set; }
        public bool acceptVisa { get; set; }
        public bool acceptAmex { get; set; }

        public Boolean isMember { get; set; }
    }

    public class CheckoutBraintreeCreditCardDetails
    {
        public string cardType { get; set; }
        public string endDigits { get; set; }
        public string expiry { get; set; }
        public string token { get; set; }
        public string bin { get; set; }
        public string vaultToken { get; set; }
        public bool defaultCard { get; set; }
        public bool isExpired { get; set; }
    }

    public class PassBraintreeNonce
    {
        public string payment_method_nonce { get; set; }
    }

    public class checkoutPayPalCreditCard
    {
        public string iFrameSource { get; set; }
    }


    public class checkoutPayPal
    {
        public string payWithPayPalLink { get; set; }
    }


    public class checkoutBillingAddress
    {
        public long? MemberAddressID { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string PostCode { get; set; }
        public short CountryID { get; set; }
        public string CountryName { get; set; }
        public string PhoneNumber { get; set; }
        public string Code { get; set; }
    }

    public class checkoutPaymentToken
    {
        public long CheckoutPaymentTokenId { get; set; }
        public string Token { get; set; }
        public string Bin { get; set; }
        public string LastFourDigits { get; set; }
        public string CardType { get; set; }
    }

    public class checkoutBraintreeResult
    {
        public checkoutBraintreeResult()
        {
        }

        public string result { get; set; }
        public string redirect { get; set; }
        public string message { get; set; }
    }

    public class checkoutAmazonPayResult
    {
        public checkoutAmazonPayResult()
        {
        }

        public string result { get; set; }
        public string redirect { get; set; }
        public string message { get; set; }
        public string errorCode { get; set; }
    }
    public class checkoutReceipt
    {
        public bool ReceiptViewed { get; set; }
        public List<checkoutReceiptOrder> checkoutReceiptOrders { get; set; }
        public List<checkoutReceiptTotalExVat> checkoutReceiptTotalExVats { get; set; }
        public List<checkoutReceiptTransactionDetail> checkoutReceiptTransactionDetails { get; set; }
        public List<checkoutReceiptOrderLine> checkoutReceiptOrderLines { get; set; }
        public List<brontoJSONBasketLine> checkoutReceiptBrontoLines { get; set; }
        public List<checkoutReceiptOrderReference> checkoutReceiptOrderReferences { get; set; }
        public checkoutReceiptAddendum checkoutAddendum { get; set; }
    }

    public class checkoutReceiptOrderReference
    {
        public string OrderReference
        {
            get; set;
        }
    }

    public class checkoutReceiptOrder
    {
        public int OrderNumber { get; set; }
        public decimal? TotalQuantity { get; set; }
        public DateTime? MinDeliveryEstimate { get; set; }
        public DateTime? MaxDeliveryEstimate { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string PostCode { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CountryCode { get; set; }
        public string displayAddress { get; set; }
        public string displayEstimatedDelivery { get; set; }
    }



    public class checkoutReceiptDetail
    {
        public int ProductID { get; set; }
        public decimal Quantity { get; set; }
        public byte? SortOrder { get; set; }
        public string BasketName { get; set; }
        public int ProductPageID { get; set; }
        public string URLKeywords { get; set; }
        public string Code { get; set; }
        public long? ImageID { get; set; }
        public string Size { get; set; }
        public decimal? PriceInc { get; set; }
        public decimal? PriceEx { get; set; }
        public decimal? SalePriceInc { get; set; }
        public decimal? SalePriceEx { get; set; }
        public decimal? Discount { get; set; }
        public byte? SalePriceTypeID { get; set; }
        public decimal? TotalStock { get; set; }
        public bool GiftWrappable { get; set; }
        public bool SaveForLater { get; set; }
        public string ImageUrl { get; set; }
        public string DisplayLine { get; set; }
        public string ProductPageUrl { get; set; }
    }


    public class checkoutReceiptRecommendation
    {
        public int ProductPageID { get; set; }
        public string URLKeywords { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public long? ImageID { get; set; }
        public bool New { get; set; }
        public decimal? PriceIncMin { get; set; }
        public decimal? PriceIncMax { get; set; }
        public decimal? SalePriceIncMin { get; set; }
        public decimal? SalePriceIncMax { get; set; }
        public string DiscountMessage { get; set; }
    }

    public class checkoutReceiptTotalExVat
    {
        public decimal? TotalExVAT { get; set; }
        public decimal? ProductTotal { get; set; }
        public decimal? DiscountTotal { get; set; }
        public decimal? ShippingTotal { get; set; }
        public decimal? GrandTotal { get; set; }
    }

    public class checkoutReceiptTransactionDetail
    {
        public int TransactionID { get; set; }
        public decimal? Revenue { get; set; }
        public decimal? Shipping { get; set; }
        public decimal? Tax { get; set; }
        public string CurrencyCode { get; set; }
        public string Affiliation { get; set; }
        public string PromotionCode { get; set; }
        public decimal? DiscountTotal { get; set; }
        public string CustomerStatus { get; set; }
        public long CheckoutID { get; set; }
    }

    public class checkoutReceiptOrderLine
    {
        public int TransactionID { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public string Category { get; set; }
        public decimal PriceInc { get; set; }
        public decimal Quantity { get; set; }
        public int ProductID { get; set; }
        public decimal PriceEx { get; set; }
    }

    public class checkoutTotal
    {
        public List<checkoutPaymentTotal> checkoutTotals { get; set; }
        public giftVoucherInBasket checkoutGiftVouchers { get; set; }
    }

    public class checkoutSummaryDeliveryAddress
    {
        public byte CheckoutAddressID { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string PostCode { get; set; }
        public short CountryID { get; set; }
        public string CountryName { get; set; }
        public string PhoneNumber { get; set; }
    }


    public class checkoutSummaryGiftBox
    {
        public byte CheckoutGiftBoxID { get; set; }
        public string GiftMessage { get; set; }
    }


    public class checkoutSummaryDeliveryOption
    {
        public byte? DispatchID { get; set; }
        public short DeliveryBandID { get; set; }
        public string DisplayName { get; set; }
        public byte CourierID { get; set; }
        public decimal? PriceInc { get; set; }
        public bool PerItem { get; set; }
        public long? SortOrder { get; set; }
        public bool DisplayPartShipOption { get; set; }
    }


    public class checkoutSummaryDeliverySelection
    {
        public byte DispatchID { get; set; }
        public short? DeliveryBandID { get; set; }
        public bool partShip { get; set; }
        public DateTime? minDeliveryDate { get; set; }
        public DateTime? maxDeliveryDate { get; set; }
    }


    public class checkoutSummaryStockStatusLine
    {
        public int ProductID { get; set; }
        public short? StockQty { get; set; }
        public byte StockStatusID { get; set; }
    }

    public class checkoutSummaryStockStatus
    {
        public byte? StockStatusID { get; set; }
        public string StockStatus { get; set; }
    }


    public class checkoutSummaryPartShipPrice
    {
        public decimal? PartShipPriceInc { get; set; }
    }

    public class checkoutSummaryCustomerServiceDetails
    {
        public string CustomerServicePhone { get; set; }
        public string CustomerServiceEmail { get; set; }
        public string TradeCustomerServicePhone { get; set; }
        public string TradeCustomerServiceEmail { get; set; }

    }

    public class checkoutGiftCard
    {
        public List<checkoutGiftVoucherBasket> checkoutGiftVoucherBaskets { get; set; }
    }

    public class checkoutGiftVoucherBasket
    {
        public long? CheckoutBasketID { get; set; }
        public int ProductID { get; set; }
        public int ProductPageID { get; set; }
        public long? ImageID { get; set; }
        public decimal Quantity { get; set; }
        public string BasketName { get; set; }
        public string Size { get; set; }
        public string Code { get; set; }
        public decimal? PriceInc { get; set; }
        public decimal? SalePriceInc { get; set; }
        public byte CheckoutAddressID { get; set; }
        public byte DispatchID { get; set; }
        public bool GiftWrappable { get; set; }
        public long? CheckoutGiftBoxID { get; set; }
        public string GiftVoucherMessage { get; set; }
        public string imageUrl { get; set; }
    }

    public class checkoutGift
    {
        public List<checkoutGiftGroupedBasket> checkoutGiftGroupedBaskets { get; set; }
        public List<checkoutGiftBox> checkoutGiftBoxs { get; set; }
        public List<checkoutGiftBoxItem> checkoutGiftBoxItems { get; set; }
        public List<checkoutGiftBoxPrice> checkoutGiftBoxPrices { get; set; }
    }


    public class checkoutGiftGroupedBasket
    {
        public long? CheckoutBasketID { get; set; }
        public byte BasketLineTypeID { get; set; }
        public int ProductID { get; set; }
        public int ProductPageID { get; set; }
        public long? ImageID { get; set; }
        public decimal Quantity { get; set; }
        public string BasketName { get; set; }
        public string Size { get; set; }
        public string Code { get; set; }
        public decimal? PriceInc { get; set; }
        public decimal? SalePriceInc { get; set; }
        public byte CheckoutAddressID { get; set; }
        public byte DispatchID { get; set; }
        public bool GiftWrappable { get; set; }
        public long? CheckoutGiftBoxID { get; set; }
    }


    public class checkoutGiftBox
    {
        public byte CheckoutGiftBoxID { get; set; }
        public string GiftMessage { get; set; }
    }

    public class checkoutGiftBoxItem
    {
        public byte CheckoutGiftBoxID { get; set; }
        public long CheckoutBasketID { get; set; }
        public decimal Quantity { get; set; }
    }

    public class checkoutGiftBoxPrice
    {
        public decimal? GiftBoxPriceInc { get; set; }
    }

    public class giftVoucherInBasket
    {
        public List<giftVoucherDetail> giftVoucherDetails { get; set; }
    }


    public class giftVoucherDetail
    {
        public short GiftVoucherID { get; set; }
        public string Code { get; set; }
        public DateTime? ExpiryDT { get; set; }
        public string displayExpiryDate { get; set; }
        public decimal ValueInc { get; set; }
        public decimal BalanceTotal { get; set; }
        public bool isValid { get; set; }
        public string failureMessage { get; set; }
    }


    public class giftVoucherDelete
    {
        public decimal BalanceTotal { get; set; }
    }

    //public class checkoutEstimateDeliveryDate
    //{
    //    public List<checkoutMinimum> minDelEst { get; set; }
    //    public List<checkoutMaximum> maxDelEst { get; set; }
    //}


    //public class checkoutMinimum
    //{
    //    public DateTime? minDeliveryEstimate { get; set; }
    //}


    //public class checkoutMaximum
    //{
    //    public DateTime? maxDeliveryEstimate { get; set; }
    //}

    public class checkoutDeliveryEstimate
    {
        public DateTime? minDeliveryEstimate { get; set; }
        public DateTime? maxDeliveryEstimate { get; set; }
    }

    public class validateBasket
    {
        public List<validateBasketSku> validateBasketSkus { get; set; }
        public List<validateBasketOrderMax> validateBasketOrderMaxs { get; set; }
    }


    public class validateBasketSku
    {
        public int productID { get; set; }
        public decimal quantity { get; set; }
        public string basketName { get; set; }

    }

    public class validateBasketOrderMax
    {
        public byte productTypeId { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal MaxTotalOrderQuantity { get; set; }
        public string Name { get; set; }
    }

    public class workGroupDelivery
    {
        public byte CheckoutAddressID { get; set; }
        public byte DispatchID { get; set; }
        public short? DeliveryBandID { get; set; }
    }


    public class checkoutAddressGenerateResult
    {
        public List<checkoutDeliveryItem> deliveryItems { get; set; }
        //public List<checkoutDeliveryCost> deliveryCosts { get; set; }
        public List<checkoutCreateSnapshotBasket> createSnapshotBaskets { get; set; }
        public List<checkoutDeliveryAddress> selectedAddresses { get; set; }
        //public List<memberAddress> defaultAddress { get; set; }
        public List<checkoutSummaryCustomerServiceDetails> customerServiceDetails { get; set; }
        public bool collectFromKingsRoad { get; set; }
        public bool collectFromMarylebone { get; set; }
        public bool collectFromWarehouse { get; set; }
    }


    public class checkoutDeliveryItem
    {
        public byte DeliveryOptionID { get; set; }
    }


    public class checkoutDeliveryCost
    {
        public byte DeliveryOptionID { get; set; }
        public decimal? PriceInc { get; set; }
    }


    public class checkoutCreateSnapshotBasket
    {
        public long CheckoutID { get; set; }
        public DateTime CheckoutExpiryDT { get; set; }
        public bool CheckoutDisplayGiftScreen { get; set; }
        public bool CheckoutDisplayGiftVoucherScreen { get; set; }
        public decimal? SubTotal { get; set; }
    }

    public class PassCutLengthBreakdown
    {
        public long CheckoutBasketID { get; set; }
        public string Breakdown { get; set; }
        public string Batch { get; set; }
    }

    public class PassLineMemo
    {
        public long CheckoutBasketID { get; set; }
        public string Reference { get; set; }
        public string Memo { get; set; }
    }

    public class PassLineTreatment
    {
        public long CheckoutBasketID { get; set; }
        public string OtherTreatment { get; set; }
        public bool StainTreatment { get; set; }
        public bool DomesticUpholsteryTreatment { get; set; }
        public bool Crib5Treatment { get; set; }
        public bool ExpressTreatment { get; set; }
    }

    public class webapiBillingAddressSelect
    {
        public long? memberAddressId { get; set; }
        public string basketCountry { get; set; }
    }

    public class webapiCheckoutCountryCheck
    {
        public string basketCountry { get; set; }
        public string deliveryCountry { get; set; }
    }
    public class webapiBraintreeError
    {
        public string type { get; set; }
        public string message { get; set; }
    }



    public class webapiCheckoutPayPalNonce
    {
        public string nonce { get; set; }
    }
    public class webapiCheckoutSetCheckoutType
    {
        public dgCheckoutType checkoutType { get; set; }
        public string guestEmail { get; set; }
        public string token { get; set; }
        public string basketCountry { get; set; }

        public webapiAddAddress initialShippingAddress { get; set; }
    }

    public class webapiCheckoutGetAmazonShippingAddress
    {
        public string orderReferenceId { get; set; }
        public string basketCountry { get; set; }
    }
    public class webapiCheckoutUpdateAmazonPrice
    {
        public masterDefinition masterDefinition { get; set; }
        public string orderReferenceId { get; set; }
    }
    public class webapiCheckoutBraintreeSubmit
    {
        public string device_data { get; set; }
        public string basketCountry { get; set; }
        public string payment_method_nonce { get; set; }
        public bool make_default { get; set; }
        public string email { get; set; }
        public string phone { get; set; }

        public masterDefinition masterDefinition { get; set; }
        public List<MarketingPermission> marketingPermissions { get; set; }
    }

    public class webapiPassOrderGen
    {
        public List<MarketingPermission> marketingPermissions { get; set; }
    }

    public class webapiCheckoutAmazonSubmit
    {
        public string orderReferenceId { get; set; }
        public masterDefinition masterDefinition { get; set; }
        public List<MarketingPermission> marketingPermissions { get; set; }
    }

    public class webapiBraintreeLoaded
    {
        public string version { get; set; }
        public int time { get; set; }
    }

    public class webapiCheckoutLog
    {
        public int action { get; set; }
        public string detail { get; set; }
    }
    public class webapiBraintreeLate
    {
        public string type { get; set; }
        public int time { get; set; }
        public string detail { get; set; }
    }


    public class webapiCheckoutBraintree
    {
        public string basketCountry { get; set; }
    }
    public class webApiCheckoutPaymentToken
    {
        public string token { get; set; }
        public string bin { get; set; }
        public string lastFourDigits { get; set; }
        public string cardType { get; set; }
    }

    public class webapiCheckoutCreditCard
    {
        public string basketCountry { get; set; }
    }

    public class webapiCheckoutPayPal
    {
        public string basketCountry { get; set; }
    }


    public class webapiCheckoutUrlForPaidInFull
    {
        public string orderReference { get; set; }
        public string basketCountry { get; set; }
        public List<MarketingPermission> marketingPermissions { get; set; }
    }

    public class webapiCheckoutRemoveCard
    {
        public string basketCountry { get; set; }
        public string vaultToken { get; set; }
        public string cardToken { get; set; }
    }

    public class webapiCheckoutValidateCVV
    {
        public string token { get; set; }
        public string nonce { get; set; }
        public string bin { get; set; }
        public string country { get; set; }
    }
    public class webapiRecreateCheckout
    {
        public string country { get; set; }
    }
    public class webapiContactReferral
    {
        public int transactionId { get; set; }
    }

    public class PassGiftBoxSave
    {
        public List<giftBox> giftBoxes { get; set; }
    }


    public class giftBox
    {
        public byte giftBoxId { get; set; }
        public string giftBoxMessage { get; set; }
        public List<giftProduct> products { get; set; }
    }

    public class giftProduct
    {
        public long checkoutBasketId { get; set; }
        public byte giftBoxId { get; set; }
        public byte basketLineTypeId { get; set; }
        public decimal quantity { get; set; }
    }

    public class webapiGiftVoucherInsert
    {
        public string Code { get; set; }
        public string Country { get; set; }
    }

    public class webapiGiftVoucherDelete
    {
        public int GiftVoucherId { get; set; }
        public string Country { get; set; }
    }

    public class PassGiftCardSave
    {
        public List<giftCardSaveDetail> giftCards { get; set; }
    }

    public class giftCardSaveDetail
    {
        public long CheckoutBasketID { get; set; }
        public string Message { get; set; }
    }

}