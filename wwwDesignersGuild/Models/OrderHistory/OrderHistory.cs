using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Helper;

namespace wwwDesignersGuild.Models.Classes
{
    public class OrderHistory
    {
        public List<OrderHistoryHeader> OrderHeads { get; set; }
        public List<OrderHistoryLine> OrderLines { get; set; }
        public List<OrderHistoryLineTracking> OrderLineTracking { get; set; }
        public List<OrderHistoryReturnReason> ReturnReasons { get; set; }
        public List<OrderHistoryOrderStatus> OrderStatuses { get; set; }
        public List<OrderHistoryPreviousReturn> PreviousReturns { get; set; }
        public List<DisplayOrderStatus> OrderStatusList { get; set; }
        public List<DisplayDateRange> DateRangeList { get; set; }
    }

    public class OrderHistoryHeader
    {
        [JsonIgnore]
        public int TransactionID { get; set; }

        public string TransactionIdentifier
        {
            set
            {

            }
            get
            {
                return dgCrypto.Encrypt($"{TransactionID}");
            }
        }

        public int OrderHeadID { get; set; }
        public DateTime OrderDate { get; set; }
        public byte OrderStatusID { get; set; }
        public decimal OrderTotal { get; set; }
        public DateTime? MinDeliveryEstimate { get; set; }
        public DateTime? MaxDeliveryEstimate { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }
        public string DisplayOrderDate { get; set; }
        public string DisplayOrderTotal { get; set; }
        public string DisplayDelivery { get; set; }
        public bool AllowReturns { get; set; }
        public long MemberAddressID { get; set; }
        public string ReturnsPhoneNumber { get; set; }
    }

    public class OrderHistoryLine
    {
        public int OrderHeadID { get; set; }
        public long OrderLineID { get; set; }
        public short OrderLineNo { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal Quantity { get; set; }
        public decimal? ReturnedQuantity { get; set; }
        public byte OrderStatusID { get; set; }
        public long? ImageID { get; set; }
        public bool AllowReturns { get; set; }
        public bool ReturnWindowExpired { get; set; }
        public int? ProductPageID { get; set; }
        public string DisplayURL { get; set; }
    }

    public class OrderHistoryLineTracking
    {
        public int OrderHeadID { get; set; }
        public short OrderLineNo { get; set; }
        public string ParcelTrackingURL { get; set; }
        public DateTime DispatchedDate { get; set; }
        public string DisplayDispatchedDate { get; set; }

    }

    public class OrderHistoryReturnReason
    {
        public byte ReturnReasonID { get; set; }
        public string Name { get; set; }
    }

    public class OrderHistoryOrderStatus
    {
        public byte OrderStatusID { get; set; }
        public string Name { get; set; }
    }

    public class OrderHistoryPreviousReturn
    {
        public int ReturnHeadID { get; set; }

        public string ReturnIdentifier
        {
            set
            {

            }
            get
            {
                return dgCrypto.Encrypt($"{ReturnHeadID}");
            }
        }

        public int OrderHeadID { get; set; }
        public DateTime CreatedDT { get; set; }
        public string DisplayCreatedDT { get; set; }
    }

    public class DisplayOrderStatus
    {
        public byte OrderStatusID { get; set; }
        public string Name { get; set; }
        public bool Default { get; set; }
    }

    public class DisplayDateRange
    {
        public short NumberOfDays { get; set; }
        public string Name { get; set; }
        public bool Default { get; set; }
    }
}