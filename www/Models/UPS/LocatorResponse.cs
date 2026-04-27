using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.UPS
{

	public class LocatorResponseRootObject
	{
		public LocatorResponse LocatorResponse { get; set; }
	}

	public class LocatorResponse
	{
		public Response Response { get; set; }
		public Geocode Geocode { get; set; }
		public SearchResults SearchResults { get; set; }
		public string AllowAllConfidenceLevels { get; set; }
	}

	public class Response
	{
		public ResponseTransactionReference TransactionReference { get; set; }
		public string ResponseStatusCode { get; set; }
		public string ResponseStatusDescription { get; set; }
		public Error Error { get; set; }
	}

	public class ResponseTransactionReference
	{
		public string CustomerContext { get; set; }
		public string XpciVersion { get; set; }
	}

	public class Error
	{
		public string ErrorSeverity { get; set; }
		public string ErrorCode { get; set; }
		public string ErrorDescription { get; set; }
		public string MinimumRetrySeconds { get; set; }
		public ErrorLocation[] ErrorLocation { get; set; }
		public string[] ErrorDigest { get; set; }
	}

	public class ErrorLocation
	{
		public string ErrorLocationElementName { get; set; }
		public string ErrorLocationAttributeName { get; set; }
	}

	public class Geocode
	{
		public string Latitude { get; set; }
		public string Longitude { get; set; }
	}

	public class SearchResults
	{
		public GeocodeCandidate[] GeocodeCandidate { get; set; }
		public string Disclaimer { get; set; }
		public DropLocation[] DropLocation { get; set; }
		public AvailableLocationAttribute[] AvailableLocationAttributes { get; set; }
		public string ActiveAvailableAccessPointIndicator { get; set; }
	}

	public class GeocodeCandidate
	{
		public ResponseAddressKeyFormat AddressKeyFormat { get; set; }
		public GeoCode1 Geocode { get; set; }
		public string LandmarkName { get; set; }
	}

	public class ResponseAddressKeyFormat
	{
		public string ConsigneeName { get; set; }
		public string AddressLine { get; set; }
		public string PoliticalDivision3 { get; set; }
		public string PoliticalDivision2 { get; set; }
		public string PoliticalDivision1 { get; set; }
		public string PostcodePrimaryLow { get; set; }
		public string PostcodeExtendedLow { get; set; }
		public string CountryCode { get; set; }
	}

	public class GeoCode1
	{
		public string Latitude { get; set; }
		public string Longitude { get; set; }
	}

	public class DropLocation
	{
		public string LocationID { get; set; }
		public string OriginOrDestination { get; set; }
		public IVR IVR { get; set; }
		public Geocode2 Geocode { get; set; }
		public ResponseAddressKeyFormat AddressKeyFormat { get; set; }
		public string[] PhoneNumber { get; set; }
		public string FaxNumber { get; set; }
		public string EMailAddress { get; set; }
		public LocationAttribute[] LocationAttribute { get; set; }
		public Distance Distance { get; set; }
		public SpecialInstruction[] SpecialInstructions { get; set; }
		public string[] LatestGroundDropOffTime { get; set; }
		public string[] LatestAirDropOffTime { get; set; }
		public string AdditionalChargeIndicator { get; set; }
		public string StandardHoursOfOperation { get; set; }
		public string NonStandardHoursOfOperation { get; set; }
		public string WillCallHoursOfOperation { get; set; }
		public string Number { get; set; }
		public string HomePageURL { get; set; }
		public string Comments { get; set; }
		public AdditionalComments AdditionalComments { get; set; }
		public string[] Disclaimer { get; set; }
		public string SLIC { get; set; }
		public string Timezone { get; set; }
		public string FacilityType { get; set; }
		public OperatingHours OperatingHours { get; set; }
		public LocalizedInstruction[] LocalizedInstruction { get; set; }
		public PromotionInformation[] PromotionInformation { get; set; }
		public Sortcode SortCode { get; set; }
		public ServiceOfferingList ServiceOfferingList { get; set; }
		public string DisplayPhoneNumberIndicator { get; set; }
		public AccessPointInformation AccessPointInformation { get; set; }
		public LocationImage LocationImage { get; set; }
		public string LocationNewIndicator { get; set; }
		public string PromotionalLinkURL { get; set; }
		public string FeaturedRank { get; set; }
		public string WillCallLocationIndicator { get; set; }
	}

	public class IVR
	{
		public string PhraseID { get; set; }
		public string TextToSpeechIndicator { get; set; }
	}

	public class Geocode2
	{
		public string Latitude { get; set; }
		public string Longitude { get; set; }
	}

	public class Distance
	{
		public string Value { get; set; }
		public UnitOfMeasurement UnitOfMeasurement { get; set; }
	}

	public class AdditionalComments
	{
		public CommentType[] CommentType { get; set; }
	}

	public class CommentType
	{
		public object Code { get; set; }
		public object Text { get; set; }
	}

	public class OperatingHours
	{
		public StandardHour[] StandardHours { get; set; }
	}

	public class StandardHour
	{
		public object HoursType { get; set; }
		public object[] DayOfWeek { get; set; }
	}

	public class Sortcode
	{
		public string HubSortCode { get; set; }
		public string FreightSortFacilityCode { get; set; }
	}

	public class ServiceOfferingList
	{
		public ServiceOffering[] ServiceOffering { get; set; }
	}

	public class ServiceOffering
	{
		public object Code { get; set; }
		public object Description { get; set; }
	}

	public class AccessPointInformation
	{
		public string PublicAccessPointID { get; set; }
		public string ImageURL { get; set; }
		public BusinessClassificationList BusinessClassificationList { get; set; }
		public AccessPointStatus AccessPointStatus { get; set; }
		public string FacilitySLIC { get; set; }
		public PrivateNetworkList PrivateNetworkList { get; set; }
		public Availability Availability { get; set; }
	}

	public class BusinessClassificationList
	{
		public object[] BusinessClassification { get; set; }
	}

	public class AccessPointStatus
	{
		public string Code { get; set; }
		public string Description { get; set; }
	}

	public class PrivateNetworkList
	{
		public object[] PrivateNetwork { get; set; }
	}

	public class Availability
	{
		public ShippingAvailability ShippingAvailability { get; set; }
		public DCRAvailability DCRAvailability { get; set; }
	}

	public class ShippingAvailability
	{
		public object AvailableIndicator { get; set; }
		public object UnavailableReason { get; set; }
	}

	public class DCRAvailability
	{
		public object AvailableIndicator { get; set; }
		public object UnavailableReason { get; set; }
	}

	public class LocationImage
	{
		public string SecureURL { get; set; }
		public string NonSecureURL { get; set; }
	}

	public class LocationAttribute
	{
		public OptionType OptionType { get; set; }
		public object[] OptionCode { get; set; }
	}

	public class SpecialInstruction
	{
		public object[] Segment { get; set; }
	}

	public class LocalizedInstruction
	{
		public string Locale { get; set; }
		public string Last50ftInstruction { get; set; }
	}

	public class PromotionInformation
	{
		public string Locale { get; set; }
		public string Promotion { get; set; }
	}

	public class AvailableLocationAttribute
	{
		public OptionType OptionType { get; set; }
		public OptionCode[] OptionCode { get; set; }
	}

	public class OptionCode
	{
		public string Code { get; set; }
		public string Description { get; set; }
		public string Name { get; set; }
		public string Category { get; set; }
		public TransportationPickupSchedule TransportationPickUpSchedule { get; set; }
	}

	public class TransportationPickupSchedule
	{
		public object[] PickUp { get; set; }
	}

}