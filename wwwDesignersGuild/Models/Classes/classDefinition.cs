using System;
using System.Collections.Generic;
using wwwDesignersGuild.Models.Helper;
using AmazonPay;
using Newtonsoft.Json;
using System.Web.Mvc;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Stockists;

namespace wwwDesignersGuild.Models.Classes
{


    #region Global objects


    public class masterDefinition
    {
        public string TitleTag { get; set; }
        public string MetaDescription { get; set; }
        public int? BackInStockReminder { get; set; } // added to avoid fishing for and modifying a Session variable in view
        public decimal ItemsInBasket { get; set; }
        public urlBreakdown UrlBreakdown { get; set; }
        public List<googleHref> googleHrefs { get; set; }
    }

    public class urlBreakdown
    {
        public dgContentType ContentType { get; set; }
        public int ContentId { get; set; }
        public DateTime ContentDate { get; set; }
        public string SemanticString { get; set; }
        public countryItem countryDetails { get; set; }
        public bool contentTypeAndIdNotFound { get; set; }
        public countryAwarenessCheckReturn CountryAwareness { get; set; }
        public string GeoIPCulture { get; set; }
        public bool siteCountryNotFoundInUrl { get; set; }
    }


    #endregion


    #region Identification objects


    public class identificationDefinition
    {
        public Int64 visitorDbId { get; set; }
        public string userGuid { get; set; }
        public string Culture { get; set; }
    }

    public class memberCheckResult
    {
        public Int32 MemberID { get; set; }
        public String TradeAccountNumber { get; set; }
    }
    
    public class memberFetchResult
    {
        public int? MemberID { get; set; }
    }

    public class webapiMemberId
    {
        public int MemberID { get; set; }
    }

    public class webapiTradeMemberResendInvite
    {
        public int MemberID { get; set; }
        public short CountryID { get; set; }
        public byte LanguageID { get; set; }
        public bool ManageAccount { get; set; }
        public bool PlaceOrders { get; set; }
        public bool ViewAccounts { get; set; }
        public bool ViewSalesStats { get; set; }
        public bool Downloads { get; set; }
    }

    public class VisitorSelect
    {
        public Int64 visitorDbId { get; set; }
        public String FullName { get; set; }
        public String Email { get; set; }
        public String Culture { get; set; }
        public DateTime VisitorCreatedDT { get; set; }
        public Int32? memberId { get; set; }
        public short? tradePriceListId { get; set; }
        public String memberTradeAccountNumber { get; set; }
        public Int32? memberTradeAccountID { get; set; }
        public Boolean memberDisplayTradePrice { get; set; }
        public string memberTradeDataSourceJson { get; set; }
        public bool tradePermissionAccountAdmin { get; set; }
        public bool tradePermissionPlaceOrder { get; set; }
        public bool tradePermissionViewAccounts { get; set; }
        public bool tradePermissionDownloadPricelistsBrochures { get; set; }
        public bool tradePermissionDownloadProductFeeds { get; set; }

        public VisitorSelect()
        {
            visitorDbId = -1;
            memberId = null;
            FullName = null;
            Email = null;
            Culture = null;
            VisitorCreatedDT = DateTime.MinValue;
            tradePriceListId = null;
            memberTradeAccountNumber = null;
            memberTradeAccountID = null;
            memberTradeDataSourceJson = null;
            memberDisplayTradePrice = false;
            tradePermissionAccountAdmin = false;
            tradePermissionPlaceOrder = false;
            tradePermissionViewAccounts = false;
            tradePermissionDownloadPricelistsBrochures = false;
            tradePermissionDownloadProductFeeds = false;
        }
    }

    #endregion


    #region WebApi objects




    #region Dispatch


    public class webapiDispatchLineSave
    {
        public List<webapiDispatchLine> dispatchLines { get; set; }
    }


    public class webapiDispatchEstimate
    {
        public byte dispatchId { get; set; }
        public short deliveryBandId { get; set; }
        public String basketCountry { get; set; }
        public bool partShip { get; set; }
    }


    public class webapiDispatchLine
    {
        public byte dispatchId { get; set; }
        public short deliveryBandId { get; set; }
        public bool partShip { get; set; }
        public DateTime minDeliveryDate { get; set; }
        public DateTime maxDeliveryDate { get; set; }
        public bool? customerRequestsFullDespatch { get; set; }
    }


    #endregion


    #region Address


    public class webapiAddAddress
    {
        public string companyName { get; set; }
        public string addressName { get; set; }
        public string addressLineOne { get; set; }
        public string addressLineTwo { get; set; }
        public string addressTown { get; set; }
        public string addressCounty { get; set; }
        public string addressPostCode { get; set; }
        public string addressCountryCode { get; set; }
        public string addressPhone { get; set; }
        public string memberCountry { get; set; }
    }


    public class webapiAddress
    {
        public long? memberAddressId { get; set; }
        public string companyName { get; set; }
        public string name { get; set; }
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string town { get; set; }
        public string county { get; set; }
        public string postcode { get; set; }
        public string countryCode { get; set; }
        public string phone { get; set; }
        public byte languageId { get; set; }
        public short countryId { get; set; }
    }


    public class webapiDeleteAddress
    {
        public long memberAddressId { get; set; }
        public string country { get; set; }
    }

    #endregion


    #region Promotion Codes


    public class webapiPromotionAdd
    {
        public String promotionCode { get; set; }
        public String basketCountry { get; set; }
    }


    public class webapiPromotionDelete
    {
        public short promotionCodeId { get; set; }
        public short countryId { get; set; }
        public byte languageId { get; set; }
        public string basketCountry { get; set; }
    }


    #endregion


    #region Member

    public class preflightCheckAddressResult
    {
        public Boolean Passed { get; set; }
        public String Email { get; set; }
        public String MertexCustomerAccountNumber { get; set; }
        public String CurrencyCode { get; set; }
    }

    public class webapiHandleError
    {
        public bool error 
        { 
            get
            {
                return true;
            } 
        }

        public string message { get; set; }
        public string additionalInfo { get; set; }
        public object model { get; set; }
    }

    public class webapiMarketingPermissionCheck
    {
        public long VisitorID { get; set; }
        public int? MemberID { get; set; }
        public string Email { get; set; }
        public bool AddressRequired { get; set; }
        public short CountryID { get; set; }
        public byte LanguageID { get; set; }
        public short? MarketingCountryID { get; set; }
        public byte SourceID { get; set; }
        public short? ThirdPartyID { get; set; }
    }

    public class webapiValidateEmail
    {
        public string emailAddress { get; set; }
    }

    public class webapiMoodboardSaveImage
    {
        public Int32 moodboardId { get; set; }
        public String imageData { get; set; }
    }

    public class webapiMoodboardReload
    {
        public Int16 countryId { get; set; }
        public Byte languageId { get; set; }
        public DateTime contentDate { get; set; }
    }

    public class webapiMoodboardSearch
    {
        public Int16 countryId { get; set; }
        public Byte languageId { get; set; }
        public DateTime contentDate { get; set; }
        public String customerName { get; set; }
        public Byte? roomId { get; set; }
        public String reference { get; set; }
    }

    public class webapiMoodboardCreate
    {
        public Int16 countryId { get; set; }
    }

    public class webapiMoodboardID
    {
        public Int32 moodboardId { get; set; }
    }

    public class webapiMoodboard
    {
        public Int32 moodboardId { get; set; }
        public String JSON { get; set; }
        public String customerName { get; set; }
        public Int16? height { get; set; }
        public Byte? roomId { get; set; }
        public String reference { get; set; }
    }

    public class webapiAddImageToMoodboard
    {
        public Int64 imageId { get; set; }
        public Int32 moodboardId { get; set; }
        public Int32? productPageId { get; set; }
    }

    public class webapiAddImageToScrapbook
    {
        public long imageId { get; set; }
        public int? productPageId { get; set; }
        public short countryId { get; set; }
    }

    public class webapiToggleScrapbook
    {
        public long imageId { get; set; }
        public int? productPageId { get; set; }
        public short countryId { get; set; }
        public byte languageId { get; set; }
    }

    public class webapiClearScrapbook
    {
        public Int16 countryId { get; set; }
    }

    public class webapiEmailExist
    {
        public string memberEmail { get; set; }
    }

    public class webapiLogMarketingPermissionsError
    {
        public string error { get; set; }
    }

    public class webapiNewTradeMember
    {
        public string memberTradeAccountNumber { get; set; }
        public string memberName { get; set; }
        public string memberEmail { get; set; }
        public string memberPassword { get; set; }
        public string basketCountry { get; set; }
        public string siteMTXCurrency { get; set; }
        public short countryId { get; set; }
    }

    public class webapiTradeRegistrationSubmit
    {
        public Guid guid { get; set; }
        public string email { get; set; }
        public string accountNumber { get; set; }
        public string postcode { get; set; }
        public string phone { get; set; }
        public string fullName { get; set; }
        public string password { get; set; }
        public bool displayTradePrice { get; set; }
        public masterDefinition masterDefinition { get; set; }
    }

    public class webapiTradeRegistrationCheckResult
    {
        public bool mayProceed { get; set; }
        public string message { get; set; }
    }

    public class webapiCountryIDLanguageID
    {
        public Int16 countryId { get; set; }
        public Byte languageId { get; set; }
    }

    public class webapiTradeAuthEmailClick
    {
        public Int16 countryId { get; set; }
        public String identifier { get; set; }
    }

    public enum webapiSignInAccountType
    {
        Either = 0,
        Member = 1,
        Trade = 2
    }

    public class webapiMemberThirdPartyPreference
    {
        //public String preferenceType { get; set; } // ... necessary?
        public Boolean permission { get; set; }
    }

    public class webapiCountryUpdateRequest
    {
        public Byte countryLanguageId { get; set; }
    }

    public class webapiCountry
    {
        public short countryId { get; set; }
    }

    public class webapiDefaultAddress
    {
        public long memberAddressId { get; set; }
        public string country { get; set; }
    }

    public class passEmailCaptureLoad
    {
        public short countryId { get; set; }
        public byte languageId { get; set; }
        public DateTime contentDate { get; set; }
    }

    public class webapiMyAccountSave
    {
        public short CountryID { get; set; }
        public byte LanguageID { get; set; }
        public string IPAddress { get; set; }
        public int MemberID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public short MarketingCountryID { get; set; }
        public byte MarketingLanguageID { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public List<MarketingPermission> MarketingPermissions { get; set; }
    }

    public class webapiPasswordUpdateRequest
    {
        public String currentPassword { get; set; }
        public String newPassword { get; set; }
    }

    public class webapiTradeAccountNumberUpdateRequest
    {
        public String siteMTXCurrency { get; set; }
        public String tradeAccountNumber { get; set; }
        public String emailAddress { get; set; }
        public Int16 countryId { get; set; }
        public Byte languageId { get; set; }
    }

    public class webapiDisplayTradePriceUpdateRequest
    {
        public Boolean displayTradePrice { get; set; }
    }

    public class webapiEmailUpdateRequest
    {
        public String newEmail { get; set; }
        public String password { get; set; }
    }


    public class webapiMemberUpdateRequest
    {
        public String FullName { get; set; }
    }

    public class webapiMemberResetPassword
    {
        public String token { get; set; }
        public String newPassword { get; set; }
    }


    public class webapiMemberOrderHistory
    {
        public string countryCode { get; set; }
        public string searchString { get; set; }
        public Int16? historyDays { get; set; }
        public byte? orderStatusId { get; set; }
    }


    public class webapiMemberReturnPaperwork
    {
        public String basketCountry { get; set; }
        public Int32? returnHeadId { get; set; }
    }

    public class webapiMemberReturnCollectionRequest
    {
        public String pickupDate { get; set; }
        public String readyTime { get; set; }
        public String closeTime { get; set; }

        public Int64 memberAddressId { get; set; }
        public Byte languageId { get; set; }
        public short countryId { get; set; }
    }

    public class webapiMemberSaveReturn
    {
        public Int32? orderHeadId { get; set; }
        public List<memberSaveReturnDetail> memberSaveReturnDetails { get; set; }
    }


    public class memberSaveReturnDetail
    {
        public Int64 orderLineId { get; set; }
        public decimal returnedQuantity { get; set; }
        public byte returnReasonId { get; set; }
    }


    public class webapiMemberReceiptRequest
    {
        public Int32? transactionId { get; set; }
    }


    public class webapiAddressLookupSearch
    {
        public string id { get; set; }
        public string text { get; set; }
        public string container { get; set; }
        public string origin { get; set; }
        public string countries { get; set; }
        public string limit { get; set; }
        public string language { get; set; }
    }


    public class webapiAddressLookupRefine
    {
        public String refinement { get; set; }
        public String moniker { get; set; }
    }


    public class webapiAddressLookupSearchResult
    {
        public String text { get; set; }
        public String moniker { get; set; }
        public bool isMultipleAddresses { get; set; }
    }


    public class webapiAddressLookupDetail
    {
        public String moniker { get; set; }
    }


    public class webapiAddressLookupLine
    {
        public String label { get; set; }
        public String data { get; set; }
    }

    public class webapiSub2EmailFromPopup
    {
        public String email { get; set; }

        public Int16 countryId { get; set; }

        public Byte languageId { get; set; }

        public Byte sourceId { get; set; }
    }

    #endregion


    #region Form


    public class webapiFormSave
    {
        public Int32 ContentId { get; set; }
        public Int64 PageSectionID { get; set; }
        public String basketCountry { get; set; }
        public String email { get; set; }
        public String confirmEmail { get; set; }
        public String fullName { get; set; }
        public String companyName { get; set; }
        public String addressLine1 { get; set; }
        public String addressLine2 { get; set; }
        public String addressTown { get; set; }
        public String addressCounty { get; set; }
        public String addressPostCode { get; set; }
        public String addressCountry { get; set; }
        public String phoneNumber { get; set; }

        public Boolean? genericPermission { get; set; }
        public Boolean? thirdPartyPermission { get; set; }

        public String prefShop { get; set; }
        public String prefTime { get; set; }
        public String prefDateStr { get; set; }
        public DateTime? prefDate { get; set; }

        public Int16? storeEventID { get; set; }
        public Int16? storeEventLocationID { get; set; }
        public DateTime? storeEvent_Date { get; set; }
        public Int32? storeEvent_DateID { get; set; }
        public Int64? storeEvent_Date_TimeSlotID { get; set; }
        public String gRecaptcha { get; set; }
        public bool marketingConsent { get; set; }
    }

    public class webapiPageSectionFormSave
    {
        public short FormID { get; set; }

        public string Email { get; set; }
        public string EmailConfirmation { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public short? MarketingCountryID { get; set; }
        public byte? MarketingLanguageID { get; set; }
        public string PhoneNumber { get; set; }
        public short CountryID { get; set; }
        public byte LanguageID { get; set; }
        public bool ConfirmEmail { get; set; }
        public bool AddressRequired { get; set; }
        public bool PasswordRequired { get; set; }
        public bool CaptchaRequired { get; set; }
        public string gRecaptcha { get; set; }
        public byte SourceID { get; set; }
        public string SubmitButtonText { get; set; }
        public int ContentID { get; set; }
        public string HeaderText { get; set; }
        public string FooterText { get; set; }
        public long? PageSectionID { get; set; }
        public short? ThirdPartyID { get; set; }
        public short? CompetitionID { get; set; }
        public short? BrochureID { get; set; }
        public string PreferredShop { get; set; }
        public string PreferredTime { get; set; }
        public DateTime? PreferredDate { get; set; }
        public int? TransactionID { get; set; }
        public short? StoreEventID { get; set; }
        public short? StoreEventLocationID { get; set; }
        public DateTime? StoreEvent_Date { get; set; }
        public int? StoreEvent_DateID { get; set; }
        public long? StoreEvent_Date_TimeSlotID { get; set; }
        public List<MarketingPermission> MarketingPermissions { get; set; }
        public byte? AppointmentAgendaID { get; set; }
        public byte? AppointmentTypeID { get; set; }
        public string AppointmentComments { get; set; }
    }

    public class pageSectionFormSave
    {
        public webapiPageSectionFormSave Form { get; set; }
        public long VisitorID { get; set; }
        public int? MemberID { get; set; }
    }

    public class webapiLeadId
    {
        public long leadId { get; set; }
    }

    public class webapiTradeAccountRequestFormSave
    {
        public long? leadId { get; set; }
        public string gRecaptcha { get; set; }

        public NewMasterDefinition objMaster { get; set; }

        public string companyName { get; set; }
        public byte channelId { get; set; }
        public string companyRegistration { get; set; }
        public string companyVATNumber { get; set; }
        public short? vatCountryId { get; set; }
        public string vatNumberDisplay { get; set; }
        public string companyEORINumber { get; set; }
        public byte? companyTypeId { get; set; }
        public string taxId { get; set; }
        public string autoCorrespondenceLanguageCode { get; set; }
        public string website { get; set; }
        public byte? tradingSinceId { get; set; }
        public byte contactTypeId { get; set; }
        public byte? titleId { get; set; }
        public string fullName { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string orderConfirmationsEmail { get; set; }
        public string invoicesStatementsEmail { get; set; }
        public string pricelistsProductsNewsEventsEmail { get; set; }
        public string deliveryNotificationsEmail { get; set; }
        public string notes { get; set; }
        public bool patternBookInterest { get; set; }
        public bool distributorConsent { get; set; }
        public bool sellOnline { get; set; }
        public string businessTypeOther { get; set; }
        public string positionOther { get; set; }

        public bool deliverySameAsInvoice { get; set; }
        public bool visitSameAsInvoice { get; set; }
        public leadAddress invoiceAddress { get; set; }
        public leadAddress deliveryAddress { get; set; }
        public leadAddress visitAddress { get; set; }
        public List<productTypeGroup_Brand_Country> leadInterests { get; set; }
        public byte[] salesTaxExemptionCertificateUpload { get; set; }
    }

    public class leadAddress
    {
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string town { get; set; }
        public string county { get; set; }
        public string postcode { get; set; }
        public short? countryId { get; set; }
    }

    public class webapiStoreEventDateTimeHelper
    {
        public Int32 StoreEvent_DateID { get; set; }
        public DateTime Date { get; set; }
    }

    #endregion


    #region Search


    public class webapiSearchParameters
    {
        public string basketCountry { get; set; }
        public Int32 productTypeId { get; set; }
        public List<Int32> filterIds { get; set; }
        public masterDefinition objMaster { get; set; }
    }

    public class webapiRenderProductPages
    {
        public DateTime? dateOverride { get; set; }
        public short? countryIdOverride { get; set; }
        public string basketCountry { get; set; }
        public List<Int32> productPageIds { get; set; }
    }


    public class webapiPredictiveSearch
    {
        public string productType { get; set; }
        public short countryId { get; set; }
        public byte languageId { get; set; }
        public string siteCountry { get; set; }
        public string searchTerm { get; set; }
    }

    public class webapiQuickLook
    {
        public Int16 CountryID { get; set; }
        public Byte LanguageID { get; set; }
        public Int32 ProductPageID { get; set; }

        public String Date { get; set; }
    }

    public class quickLookReturn
    {
        public List<productPageQuickLookPrice> prices { get; set; }
        public List<productPageQuickLook> products { get; set; }
        public List<productPageQuickLookBasketContent> basketContents { get; set; }
        public List<productPageQuickLookText> texts { get; set; }
    }

    #endregion


    #endregion


    #region Stockist Contracts object


    public class stockistsContractSearchSelect
    {
        public List<stockistsContractDetail> stockistsContractDetails { get; set; }
    }


    public class stockistsContractDetail
    {
        public String Name { get; set; }
        public String Address { get; set; }
        public String City { get; set; }
        public String County { get; set; }
        public String Postcode { get; set; }
        public String Telephone { get; set; }
        public String Fax { get; set; }
        public String Email { get; set; }
        public String Website { get; set; }
        public String Facebook { get; set; }
        public String Twitter { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public String Image { get; set; }
    }


    public class webapiStockistContractRequest
    {
        public String basketCountry { get; set; }
        public short selectedCountry { get; set; }
    }


    #endregion


    #region Member object

    #region Member Login


    public class memberLogin
    {
        public List<memberLoginStatus> memberLoginStatusData { get; set; }
    }

    public class tradeAuthGuidCheckResponse
    {
        public bool Result { get; set; }
        public string FormType { get; set; }
        public string Email { get; set; }
        public string CustomerServicesPhoneNumber { get; set; }
        public string SalesRepresentativeName { get; set; }
        public string SalesRepresentativeEmail { get; set; }
    }

    public class tradeAuthCompleteResponse
    {
        public bool Result { get; set; }
        public int? TradeAccountID { get; set; }
        public int? MemberID { get; set; }
    }

    public class passTradeRegistration
    {
        public masterDefinition masterDefinition { get; set; }
        public Guid guid { get; set; }
        public tradeAuthGuidCheckResponse data { get; set; }
    }

    public class memberLoginStatus
    {
        public Int32 LoginSuccessful { get; set; }
        public Int32 MemberID { get; set; }
        public String MertexCustomerAccountNumber { get; set; }
        public Int32? TradeAccountID { get; set; }
        public Boolean DisplayTradePrice { get; set; }
    }


    #endregion


    #region Member Details


    public class memberDetailStore
    {
        public MemberDetail MemberDetail { get; set; }
        public List<countryRegion> regions { get; set; }
    }

    public struct updateMemberTradeAccountNumberResult
    {
        public bool success { get; set; }
        public int? tradeAccountId { get; set; }
        public int? memberId { get; set; }
    }

    public class tradeContactUsResult
    {
        public List<tradeContactUsDetail> Details { get; set; }
    }

    public class tradeContactUsDetail
    {
        public byte SortOrder { get; set; }
        public string Department { get; set; }
        public long? ImageID { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public string OpeningHours { get; set; }
        public string Notes { get; set; }
    }

    public class tradeOfferDetail
    {
        public string TradeCurrencyCode { get; set; }
        public Boolean AllowTradePrice { get; set; }
        public Boolean FreeSamples { get; set; }
        public Byte MarginBoosterPercentage { get; set; }
        public Byte RetailOnlyDiscountPercentage { get; set; }
    }

    #endregion


    #region Trade Address

    public class MemberTradeAddress
    {
        public String FullName { get; set; }
        public String CompanyName { get; set; }
        public String AddressLine1 { get; set; }
        public String AddressLine2 { get; set; }
        public String Town { get; set; }
        public String County { get; set; }
        public String PostCode { get; set; }
        public String PhoneNumber { get; set; }
        public short? CountryID { get; set; }
        public String CountryCode { get; set; }
        public Boolean Usable { get; set; }
    }

    #endregion

    #region Order History




    public class itemImageURL
    {
        public Int64? ImageID { get; set; }
        public String ImageURL { get; set; }
    }

    #endregion


    #region Return Paperwork Generate

    public class UPSShippingResult
    {
        public System.Drawing.Image Image { get; set; }
        public String TrackingNumber { get; set; }
    }

    public class PickupRequestResult
    {
        public String Code { get; set; }
        public String PRN { get; set; }
    }

    public class memberReturnPaperworkGenerate
    {
        public List<memberReturnHeader> memberReturnHeaders { get; set; }
        public List<memberReturnItem> memberReturnItems { get; set; }
        public List<memberReturnDetail> memberReturnDetails { get; set; }
    }


    public class memberReturnHeader
    {
        public String ReturnsAddress { get; set; }
        public Int32? OrderHeadID { get; set; }
    }


    public class memberReturnItem
    {
        public String Code { get; set; }
        public String Name { get; set; }
        public decimal ReturnedQuantity { get; set; }
        public String ReturnReason { get; set; }
    }


    public class memberReturnDetail
    {
        public String Email { get; set; }
        public String FullName { get; set; }
        public String PhoneNumber { get; set; }
    }


    #endregion


    #region Receipt Generate


    public class memberReceiptGenerate
    {
        public List<receiptSummary> receiptSummarys { get; set; }
        public List<receiptHeader> receiptHeaders { get; set; }
        public List<receiptLine> receiptLines { get; set; }
    }


    public class receiptSummary
    {
        public Int32 TransactionID { get; set; }
        public Decimal? TotalItems { get; set; }
        public Decimal? TotalGiftBox { get; set; }
        public Decimal? TotalPandP { get; set; }
        public Decimal? TotalPromotionDiscount { get; set; }
        public Decimal? TotalVATExemption { get; set; }
        public Decimal? TotalUSSalesTax { get; set; }
        public Decimal? TotalPayable { get; set; }
    }


    public class receiptHeader
    {
        public Int32 TransactionID { get; set; }
        public DateTime CreatedDT { get; set; }
        public Int32? OrderHeadID { get; set; }
        public String FullName { get; set; }
        public String CompanyName { get; set; }
        public String AddressLine1 { get; set; }
        public String AddressLine2 { get; set; }
        public String Town { get; set; }
        public String County { get; set; }
        public String PostCode { get; set; }
        public String Country { get; set; }
    }


    public class receiptLine
    {
        public Int32 TransactionID { get; set; }
        public Int32? OrderHeadID { get; set; }
        public String Code { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public Decimal? FinalPriceInc { get; set; }
        public Decimal? SubTotal { get; set; }
    }


    #endregion


    #endregion


    #region Basket objects


    #region Promotional message


    public class storePromotionalMessage
    {
        public List<promotionalMessage> promotionalMessages { get; set; }
    }


    public class promotionalMessage
    {
        public string Message { get; set; }
    }

    public class promotionCode
    {
        public string PromoCode { get; set; }
    }


    public class promotionCodeSelect
    {
        public List<promotionCodeDetail> promotionCodeDetails { get; set; }
        public List<promotionCodeMoney> promotionCodeMoneys { get; set; }
    }


    public class promotionCodeDetail
    {
        public short promotionCodeId { get; set; }
        public String Code { get; set; }
        public String Description { get; set; }
        public DateTime OnDate { get; set; }
        public DateTime? OffDate { get; set; }
        public Decimal? Discount { get; set; }
        public Boolean DiscountType { get; set; }
        public int? FreeGiftProductID { get; set; }
    }


    public class promotionCodeMoney
    {
        public Decimal? TotalDiscount { get; set; }
    }


    #endregion


    #region Add to Basket Return

    #endregion

    #region Bronto JSON Basket Return


    public class DotDigitalTrackingBehaviourProfile
    {
        public string WebBehaviourTrackingProfileID { get; set; }
    }

    public class brontoJSONBasketHeader
    {
        public string CartPhase { get; set; }
        public string Currency { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? GrandTotal { get; set; }
        public long? OrderID { get; set; }
        public string Email { get; set; }
        public string CartURL { get; set; }
    }

    public class brontoJSONBasketLine
    {
        public string SKU { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public Decimal? UnitPrice { get; set; }
        public Decimal? SalePrice { get; set; }
        public decimal Quantity { get; set; }
        public Decimal? TotalPrice { get; set; }
        public string ImageURL { get; set; }
        public string ProductURL { get; set; }
    }
    #endregion

    #region DotDigital JSON Basket Return

    public class DotDigitalJSON 
    {
        public string JSON { get; set; }
    }

    #endregion

    public class cmsStockStatusDefinition
    {
        public List<StockStatusSelect> StockStatuses { get; set; }
    }

    public class stockStatus
    {
        public byte BasketLineTypeID { get; set; }
        public int ProductID { get; set; }
        public Boolean SaveForLater { get; set; }
        public short? StockQty { get; set; }
        public byte StockStatusID { get; set; }
    }

    public class basketRecommendation
    {
        public int ProductPageID { get; set; }
        public string URLKeywords { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public Int64? ImageID { get; set; }
        public Boolean New { get; set; }
        public Decimal? PriceIncMin { get; set; }
        public Decimal? PriceIncMax { get; set; }
        public Decimal? SalePriceIncMin { get; set; }
        public Decimal? SalePriceIncMax { get; set; }
        public string DiscountMessage { get; set; }
    }


    public class StockStatusSelect
    {
        public byte? StockStatusID { get; set; }
        public String StockStatus { get; set; }
    }

    public class cmsProductTypeOrderRestrictionDefinition
    {
        public List<productTypeOrderRestrictionSelect> ProductTypeOrderRestrictions { get; set; }
    }
    public class productTypeOrderRestrictionSelect
    {
        public byte productTypeID { get; set; }
        public String Name { get; set; }
    }


    #endregion


    #region Server Search Results object

    //  TO SLOW - using ExpressionTreeCloner.cs
    //public static class ExtensionMethods
    //{
    //    // Deep clone (class must be marked as Serializable)
    //    public static T DeepClone<T>(this T a)
    //    {
    //        using (MemoryStream stream = new MemoryStream())
    //        {
    //            BinaryFormatter formatter = new BinaryFormatter();
    //            formatter.Serialize(stream, a);
    //            stream.Position = 0;
    //            return (T)formatter.Deserialize(stream);
    //        }
    //    }
    //}


    [Serializable]
    public class serverSearchDatabase
    {
        public List<serverSearchResultCore> searchResultCores { get; set; }
        public List<serverSearchResultAttribute> searchResultAttributes { get; set; }
    }

    [Serializable]
    public class serverSearchResultCore
    {
        public int ProductPageID { get; set; }
        public byte ProductTypeID { get; set; }
        public int ScorePercent { get; set; }
        public int SearchSortOrder { get; set; }
        public long AtoZScore { get; set; }
        public byte ColourSort { get; set; }
        public long NewSort { get; set; }
        public long PriceSort { get; set; }
        public ProductType ProductType { get; internal set; }
    }

    [Serializable]
    public class serverSearchResultAttribute
    {
        public int ProductPageID { get; set; }
        public short AttributeID { get; set; }
    }

    public class serverSearchResultCoreToClient
    {
        public int ProductPageID { get; set; }
        public byte ProductTypeID { get; set; }
        public Int32 ScorePercent { get; set; }
        public Int32 SearchSortOrder { get; set; }
        public long AtoZScore { get; set; }
        public byte ColourSort { get; set; }
        public Int64 NewSort { get; set; }
    }

    public class searchResultForRender
    {
        public List<searchResultRenderCore> searchResultRenderCores { get; set; }
        public List<searchResultRenderImage> searchResultRenderImages { get; set; }

        public searchResultForRender()
        {
            searchResultRenderCores = new List<searchResultRenderCore>();
            searchResultRenderImages = new List<searchResultRenderImage>();
        }
    }

    public class searchResultRenderCore
    {
        public int ProductPageID { get; set; }
        public byte ProductTypeID { get; set; }
        public string URLKeywords { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public bool New { get; set; }
        public short SortOrder { get; set; }
        public bool IsLifestyleGallery { get; set; }
        public decimal? DisplayPriceMin { get; set; }
        public decimal? DisplayPriceMax { get; set; }
        public decimal? DisplaySalePriceMin { get; set; }
        public decimal? DisplaySalePriceMax { get; set; }
        public string DiscountMessage { get; set; }
        public bool ShowRating { get; set; }
        public decimal? RatingAvg { get; set; }
        public short? RatingCount { get; set; }
        public string Code { get; set; }
        public int ProductCount { get; set; }
        public int? ProductID { get; set; }
        public bool Sample { get; set; }
        public decimal? FreeStock { get; set; }
        public byte? StockStatusID { get; set; }
        public string StockUnit { get; set; }
        public string StockStatus { get; set; } // fulfilled from cache
    }

    public class searchResultRenderImage
    {
        public int ProductPageID { get; set; }
        public long ImageID { get; set; }
        public byte SortOrder { get; set; }
        public byte TemplateImageID { get; set; }
        public string AltText { get; set; }
        public string TitleText { get; set; }
    }
    
    public class filteredSearchResult
    {
        public List<FilterGroup> filterGroups { get; set; }
        public List<serverSearchResultCoreToClient> products { get; set; }
    }

    public class FilterGroup
    {
        public AttributeGroup attributeGroup { get; set; }
        public List<Filter> filters { get; set; }
        public List<Int32> productSet { get; set; }
    }

    public class Filter
    {
        public AttributeDetail attributeDetail { get; set; }
        public Boolean isChecked { get; set; }
        public Boolean isEnabled { get; set; }
        public List<Int32> productIds { get; set; }
        public Int32 productsCount { get; set; }
    }

    #endregion


    #region predictive Search


    public class passPredictiveSearch
    {
        public predictiveSearchHints predictiveSearchHints { get; set; }
        public predictiveSearchResult predictiveSearchResults { get; set; }
    }

    public class predictiveSearchResult
    {
        public List<predictiveSearch> results { get; set; }
    }

    public class predictiveSearchHints
    {
        public List<collectionSearch> collections { get; set; }
        public List<designSearch> designs { get; set; }
        public List<productSearch> products { get; set; }
        public List<brandSearch> brands { get; set; }
        public List<discontinuedSearch> discontinueds { get; set; }
        public List<ProductTypeHint> productTypes { get; set; }
    }

    public class collectionSearch
    {
        public short CollectionID { get; set; }
        public string URLKeywords { get; set; }
        public string Heading { get; set; }
        public string ProductType { get; set; }
    }

    public class designSearch
    {
        public int ProductPageID { get; set; }
        public string Heading { get; set; }
        public string ProductType { get; set; }
        public string URLKeywords { get; set; }
    }

    public class productSearch
    {
        public int ProductPageID { get; set; }
        public string URL { get; set; }
        public string Heading { get; set; }
        public string Code { get; set; }
        public long ImageID { get; set; }
        public bool New { get; set; }
    }

    public class brandSearch
    {
        public int PageID { get; set; }
        public string URLKeywords { get; set; }
        public string Heading { get; set; }
    }

    public class discontinuedSearch
    {
        public string DiscontinuedMessage { get; set; }
    }

    public class predictiveSearch
    {
        public string Keyword { get; set; }
    }


    #endregion

    #region Search Product Type Drop Down object
    public class cmsSearchProductTypeDropDownDefinition
    {
        public List<ProductType> ProductTypes { get; set; }
    }

    #endregion

    #region Search Results object


    public class searchResult
    {
        public masterDefinition masterDefinition { get; set; }
        public string searchText { get; set; }
        public List<searchResultCore> searchResultCores { get; set; }
        public List<searchResultImage> searchResultImages { get; set; }
        public List<searchResultPrice> searchResultPrices { get; set; }
        public List<searchResultAttribute> searchResultAttributes { get; set; }
        public cmsAttributeDefinition attributeDefinitions { get; set; }
        public cmsAttributeDefinition useAttributeDefinitions { get; set; }
        public List<searchResultAttribute> useSearchResultAttributes { get; set; }
    }


    public class searchResultCore
    {
        public int ProductPageID { get; set; }
        public byte ProductTypeID { get; set; }
        public Boolean New { get; set; }
        public string URLKeywords { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public Int32 ScorePercent { get; set; }
    }


    public class searchResultImage
    {
        public int ProductPageID { get; set; }
        public Int64 ImageID { get; set; }
        public byte SortOrder { get; set; }
    }


    public class searchResultPrice
    {
        public int ProductPageID { get; set; }
        public Decimal? PriceIncMin { get; set; }
        public Decimal? PriceIncMax { get; set; }
        public Decimal? SalePriceIncMin { get; set; }
        public Decimal? SalePriceIncMax { get; set; }
        public string DiscountMessage { get; set; }
    }


    public class searchResultAttribute
    {
        public int ProductPageID { get; set; }
        public Int16 AttributeID { get; set; }
    }

    #endregion

    public class ClearanceProductDownloadRecord
    {
        public string ProductType { get; set; }
        public string Brand { get; set; }
        public string Collection { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public string ImageURL { get; set; }
        public decimal ListPrice { get; set; }
        public decimal ClearancePrice { get; set; }
        public string ClearanceDiscountPercentage { get; set; }
        public short Stock { get; set; }
        public DateTime StockLastChecked { get; set; }
    }
}