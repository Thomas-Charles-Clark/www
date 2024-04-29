using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Xml;
using wwwDesignersGuild.Models.UPS;
using wwwDesignersGuild.Models.Database;

namespace wwwDesignersGuild.Util
{
    public class UPSAccessPointLocator
    {
        private static readonly string upsLocatorEndpoint = "https://onlinetools.ups.com/ups.app/xml/Locator";

        public static UPSAccessPointResponse Search(PassUPSLocatorSearch objSearch)
        {
            var credentials = new databaseDataContext().FetchUPSAPICredential();

            var result = new UPSAccessPointResponse();

            using (var client = new WebClient())
            {
                client.Headers.Clear();
                client.Headers.Add("Content-Type", "application/json");

                var accessRequest = new AccessRequest()
                {
                    AccessLicenseNumber = "4D61FFBADC79A42A",
                    UserId = credentials.UserID,
                    Password = credentials.Password
                };

                var locatorRequest = new LocatorRequest()
                {
                    Request = new Request()
                    {
                        RequestAction = "Locator",
                        RequestOption = "64"
                    },
                    UnitOfMeasurement = new UnitOfMeasurementType()
                    {
                        Code = "MI"
                    },
                    OriginAddress = new OriginAddressType()
                    {
                        Geocode = null,
                        AddressKeyFormat = new AddressKeyFormatType()
                        {
                            CountryCode = objSearch.countryCode,
                            PostcodePrimaryLow = objSearch.searchTerm.ToLowerInvariant()
                        },
                        MaximumListSize = "10" // if UPS cannot sufficiently determine your origin location with the given parameters, they will return me a list of N suggestions. This is N.
                    },
                    Translate = new TranslateType()
                    {
                        Locale = objSearch.culture.Replace("-", "_")
                    },
                    LocationSearchCriteria = new LocationSearchCriteriaType()
                    {
                        SearchOption = new SearchOptionType[]
                        {
                            new SearchOptionType()
                            {
                                OptionCode = new OptionCodeType[]
                                {
                                    new OptionCodeType()
                                    {
                                        Code = "06"
                                    }
                                }
                            }
                        },
                        MaximumListSize = "50",
                        SearchRadius = objSearch.radius,
                        AccessPointSearch = new AccessPointSearchType()
                        {
                            //AccountNumber = "5WR515", // Hmmmm. Might not want to be providing this: "The account number to use for UPS Access Point Search in the country or territory. Used to locate a private network for the account. Once this parameter is present any access point address or geocode search is ignored. It cannot be combined with PublicAccessPointID search parameter"
                        }
                    }
                };

                var accessRequestXmlSerializer = new System.Xml.Serialization.XmlSerializer(accessRequest.GetType());
                var locatorRequestXmlSerializer  = new System.Xml.Serialization.XmlSerializer(locatorRequest.GetType());

                var accessRequestStringBuilder = new StringBuilder();
                var locatorRequestStringBuilder = new StringBuilder();

                var accessWriter = new ExtendedStringWriter(accessRequestStringBuilder, Encoding.UTF8);
                var requestWriter = new ExtendedStringWriter(locatorRequestStringBuilder, Encoding.UTF8);

                accessRequestXmlSerializer.Serialize(accessWriter, accessRequest);
                locatorRequestXmlSerializer.Serialize(requestWriter, locatorRequest);

                var request = $"{accessWriter}" +
                    $"{requestWriter}";

                var response = client.UploadString(upsLocatorEndpoint, request);

                Console.WriteLine(response);

                var tr = new StringReader(response);

                var reader = XmlReader.Create(tr);

                var responseSerializer = new System.Xml.Serialization.XmlSerializer(typeof(UPSAccessPointResponse));

               result = (UPSAccessPointResponse)responseSerializer.Deserialize(reader);
            }

            return result;
        }
    }

    // cheers: https://blog.jsinh.in/use-utf-8-encoding-for-stringwriter-in-c/
    public class ExtendedStringWriter : StringWriter
    {
        private readonly Encoding stringWriterEncoding;
        public ExtendedStringWriter(StringBuilder builder, Encoding desiredEncoding)
            : base(builder)
        {
            this.stringWriterEncoding = desiredEncoding;
        }

        public override Encoding Encoding
        {
            get
            {
                return this.stringWriterEncoding;
            }
        }
    }
}