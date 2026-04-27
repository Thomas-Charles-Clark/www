using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.UPS
{
	public class LocatorRequestRoot
	{
		public LocatorRequest LocatorRequest { get; set; }
	}

	public class LocatorRequest
	{
		public Request Request { get; set; }
		public OriginAddress OriginAddress { get; set; }
		public Translate Translate { get; set; }
		public UnitOfMeasurement UnitOfMeasurement { get; set; }
		public LocationSearchCriteria LocationSearchCriteria { get; set; }
		public SortCriteria SortCriteria { get; set; }
	}

	public class Request
	{
		public TransactionReference TransactionReference { get; set; }
		public string RequestAction { get; set; }
	}

	public class TransactionReference
	{
		public string CustomerContext { get; set; }
	}

	public class OriginAddress
	{
		public AddressKeyFormat AddressKeyFormat { get; set; }
		public string MaximumListSize { get; set; }
	}

	public class AddressKeyFormat
	{
		public string AddressLine { get; set; }
		public string PoliticalDivision2 { get; set; }
		public string PoliticalDivision1 { get; set; }
		public string PostcodePrimaryLow { get; set; }
		public string PostcodeExtendedLow { get; set; }
		public string CountryCode { get; set; }
	}

	public class Translate
	{
		public string LanguageCode { get; set; }
		public string Locale { get; set; }
	}

	public class UnitOfMeasurement
	{
		public string Code { get; set; }
	}

	public class LocationSearchCriteria
	{
		public SearchOption[] SearchOption { get; set; }
		public string MaximumListSize { get; set; }
		public string SearchRadius { get; set; }
		public ServiceSearch ServiceSearch { get; set; }
	}

	public class ServiceSearch
	{
		public string Time { get; set; }
		public ServiceCode ServiceCode { get; set; }
	}

	public class ServiceCode
	{
		public string Code { get; set; }
	}

	public class SearchOption
	{
		public OptionType OptionType { get; set; }
		public object OptionCode { get; set; }
		public Relation Relation { get; set; }
	}

	public class OptionType
	{
		public string Code { get; set; }
	}

	public class Relation
	{
		public string Code { get; set; }
	}

	public class SortCriteria
	{
		public string SortType { get; set; }
	}

}