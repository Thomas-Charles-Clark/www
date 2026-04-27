using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using wwwDesignersGuild.Models.UPS;
using wwwDesignersGuild.Models.Database;
using System.Runtime.Remoting.Messaging;
using SearchOption = wwwDesignersGuild.Models.UPS.SearchOption;

namespace wwwDesignersGuild.Util
{
	public class UPSAccessPointLocator
	{
		private static readonly string upsLocatorEndpoint = "https://onlinetools.ups.com/api/locations/v3/search/availabilities/64";
		const string upsOAuthEndpoint = "https://onlinetools.ups.com/security/v1/oauth/token";

		public static LocatorResponseRootObject Search(PassUPSLocatorSearch objSearch)
		{
			var credentials = new databaseDataContext().FetchUPSAPICredential();

			var result = new UPSAccessPointResponse();

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("x-merchant-id", credentials.AccountNumber);

				var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{credentials.ClientID}:{credentials.ClientSecret}"));

				client.DefaultRequestHeaders.Add("Authorization", "Basic " + base64String);

				var oauthRequest = client.PostAsync(upsOAuthEndpoint, new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("grant_type", "client_credentials")
				})).Result;

				var oauthResponse = oauthRequest.Content.ReadAsStringAsync().Result;

				var oauthTokenResponse = JsonConvert.DeserializeObject<OAuthResponse>(oauthResponse);


				using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, upsLocatorEndpoint
					))
				{
					httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", oauthTokenResponse.AccessToken);

					httpRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

					var locatorRequest = new LocatorRequestRoot()
					{
						LocatorRequest = new LocatorRequest()
						{
							Request = new Request()
							{
								RequestAction = "Locator"
							},
							UnitOfMeasurement = new UnitOfMeasurement()
							{
								Code = "MI"
							},
							OriginAddress = new OriginAddress()
							{
								AddressKeyFormat = new AddressKeyFormat()
								{
									CountryCode = objSearch.countryCode,
									PostcodePrimaryLow = objSearch.searchTerm.ToLowerInvariant()
								},
								MaximumListSize = "10" // if UPS cannot sufficiently determine your origin location with the given parameters, they will return me a list of N suggestions. This is N.
							},
							Translate = new Translate()
							{
								Locale = objSearch.culture.Replace("-", "_")
							},
							LocationSearchCriteria = new LocationSearchCriteria()
							{
								SearchOption = new SearchOption[]
								{
									new SearchOption()
									{
										OptionType = new OptionType()
										{
											Code = "06"
										}
									}
								},
								MaximumListSize = "50",
								SearchRadius = objSearch.radius
							}
						}
					};

					var c = JsonConvert.SerializeObject(locatorRequest, Formatting.Indented);

					httpRequest.Content = new StringContent(c, Encoding.UTF8, "application/json");

					var response = client.SendAsync(httpRequest).Result;

					if (response.IsSuccessStatusCode)
					{
						var responseString = response.Content.ReadAsStringAsync().Result;

						return JsonConvert.DeserializeObject<LocatorResponseRootObject>(responseString);
					}
					else
					{
						throw new Exception(response.Content.ReadAsStringAsync().Result);
					}
				}
			}
		}

		//using (var client = new WebClient())
		//         {
		//             client.Headers.Clear();
		//             client.Headers.Add("Content-Type", "application/json");

		//             var accessRequest = new AccessRequest()
		//             {
		//                 AccessLicenseNumber = "4D61FFBADC79A42A",
		//                 UserId = credentials.UserID,
		//                 Password = credentials.Password
		//             };

		//             var locatorRequest = new LocatorRequest()
		//             {
		//                 Request = new Request()
		//                 {
		//                     RequestAction = "Locator",
		//                     RequestOption = "64"
		//                 },
		//                 UnitOfMeasurement = new UnitOfMeasurementType()
		//                 {
		//                     Code = "MI"
		//                 },
		//                 OriginAddress = new OriginAddressType()
		//                 {
		//                     Geocode = null,
		//                     AddressKeyFormat = new AddressKeyFormatType()
		//                     {
		//                         CountryCode = objSearch.countryCode,
		//                         PostcodePrimaryLow = objSearch.searchTerm.ToLowerInvariant()
		//                     },
		//                     MaximumListSize = "10" // if UPS cannot sufficiently determine your origin location with the given parameters, they will return me a list of N suggestions. This is N.
		//                 },
		//                 Translate = new TranslateType()
		//                 {
		//                     Locale = objSearch.culture.Replace("-", "_")
		//                 },
		//                 LocationSearchCriteria = new LocationSearchCriteriaType()
		//                 {
		//                     SearchOption = new SearchOptionType[]
		//                     {
		//                         new SearchOptionType()
		//                         {
		//                             OptionCode = new OptionCodeType[]
		//                             {
		//                                 new OptionCodeType()
		//                                 {
		//                                     Code = "06"
		//                                 }
		//                             }
		//                         }
		//                     },
		//                     MaximumListSize = "50",
		//                     SearchRadius = objSearch.radius,
		//                     AccessPointSearch = new AccessPointSearchType()
		//                     {
		//                         //AccountNumber = "5WR515", // Hmmmm. Might not want to be providing this: "The account number to use for UPS Access Point Search in the country or territory. Used to locate a private network for the account. Once this parameter is present any access point address or geocode search is ignored. It cannot be combined with PublicAccessPointID search parameter"
		//                     }
		//                 }
		//             };

		//             var accessRequestXmlSerializer = new System.Xml.Serialization.XmlSerializer(accessRequest.GetType());
		//             var locatorRequestXmlSerializer  = new System.Xml.Serialization.XmlSerializer(locatorRequest.GetType());

		//             var accessRequestStringBuilder = new StringBuilder();
		//             var locatorRequestStringBuilder = new StringBuilder();

		//             var accessWriter = new ExtendedStringWriter(accessRequestStringBuilder, Encoding.UTF8);
		//             var requestWriter = new ExtendedStringWriter(locatorRequestStringBuilder, Encoding.UTF8);

		//             accessRequestXmlSerializer.Serialize(accessWriter, accessRequest);
		//             locatorRequestXmlSerializer.Serialize(requestWriter, locatorRequest);

		//             var request = $"{accessWriter}" +
		//                 $"{requestWriter}";

		//             var response = client.UploadString(upsLocatorEndpoint, request);

		//             Console.WriteLine(response);

		//             var tr = new StringReader(response);

		//             var reader = XmlReader.Create(tr);

		//             var responseSerializer = new System.Xml.Serialization.XmlSerializer(typeof(UPSAccessPointResponse));

		//            result = (UPSAccessPointResponse)responseSerializer.Deserialize(reader);
		//         }

		//         return result;
		//     }
		//}

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
}