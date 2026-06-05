using DocumentFormat.OpenXml.Spreadsheet;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;

using wwwDesignersGuild.PCAFind;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.Util
{
    public static partial class Framework
    {
        public static readonly string InkyEndpoint = "https://vm-web2:8091/inkyUpdated/inkyUpdated.s";
        public static readonly string WebServerHostname = "vm-web2";

        public readonly static char[] SemicolonDelimiter = new char[] { ';' };
        public readonly static char[] CommaDelimiter = new char[] { ',' };
        public readonly static char[] HyphenDelimiter = new char[] { '-' };
        public readonly static char[] QuestionMark = new char[] { '?' };
        public readonly static char[] Ampersand = new char[] { '&' };
        public readonly static char[] ForwardSlash = new char[] { '/' };
        public readonly static char[] QueryStringTrim = new char[] { '?', '&' };

        public static DateTime GetUseDate(HttpRequestWrapper request)
        {
            // Has an future date been passed
            string storeDateOverride = null;
            var useDate = DateTime.Now;
            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                var key = request.Headers.AllKeys.Where(k => k.ToLowerInvariant() == "dateoverride").SingleOrDefault();

                if (key != null)
                {
                    var match = request.Headers[key];

                    if (!String.IsNullOrEmpty(match))
                    {
                        if (match.Length == "ddMMyyyy".Length)
                        {
                            bool dateConvertSuccess = DateTime.TryParseExact(match,
                                "ddMMyyyy",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out useDate);

                            if (dateConvertSuccess == true)
                            {
                                storeDateOverride = match;
                            }
                            else
                            {
                                useDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                            }
                        }
                    }
                }
            }
            return useDate;
        }

        public static DateTime GetUseDate(HttpRequestBase request)
        {
            // Has an future date been passed
            string storeDateOverride = null;
            var useDate = DateTime.Now;
            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                var key = request.Headers.AllKeys.Where(k => k.ToLowerInvariant() == "dateoverride").SingleOrDefault();

                if (key != null)
                {
                    var match = request.Headers[key];

                    if (!String.IsNullOrEmpty(match))
                    {
                        if (match.Length == "ddMMyyyy".Length)
                        {
                            bool dateConvertSuccess = DateTime.TryParseExact(match,
                                "ddMMyyyy",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out useDate);

                            if (dateConvertSuccess == true)
                            {
                                storeDateOverride = match;
                            }
                            else
                            {
                                useDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                            }
                        }
                    }
                }
            }
            return useDate;
        }

        public static string GetCountryOverride(HttpRequestWrapper request)
        {
            string result = null;

            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                var key = request.Headers.AllKeys.Where(k => k.ToLowerInvariant() == "countryoverride").SingleOrDefault();

                if (key != null)
                {
                    var match = request.Headers[key];

                    if (!String.IsNullOrEmpty(match))
                    {
                        result = match;
                    }
                }
            }
            return result;
        }

        public static string GetUseCountry(HttpRequestBase request)
        {
            string result = null;

            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                var key = request.Headers.AllKeys.Where(k => k.ToLowerInvariant() == "countryoverride").SingleOrDefault();

                if (key != null)
                {
                    var match = request.Headers[key];

                    if (!String.IsNullOrEmpty(match))
                    {
                        result = match;
                    }
                }
            }
            return result;
        }

        public static string GetDaySuffix(int day)
        {
            switch (day)
            {
                case 1:
                case 21:
                case 31:
                    return "st";
                case 2:
                case 22:
                    return "nd";
                case 3:
                case 23:
                    return "rd";
                default:
                    return "th";
            }
        }

        public static bool ServeWebP
        {
            get
            {
                return HttpContext.Current?.Request?.AcceptTypes?.Contains("image/webp") == true;
            }
        }

        public static string WebPFallback
        {
            get
            {
                return ServeWebP ? null : "-nonwebp";
            }
        }

        public static string WebpOrJpgExtension
        {
            get
            {
                return ServeWebP ? ".webp" : ".jpg"; 
            }
        }

        public static string WebpOrPngExtension
        {
            get
            {
                return ServeWebP ? ".webp" : ".png";
            }
        }

        public static bool HotspotsEnabled
        {
            get
            {
                return Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["hotspotsEnabled"]);
            }
        }

        /// <summary>
        /// Constructs a QueryString (string).^
        /// Consider this method to be the opposite of "System.Web.HttpUtility.ParseQueryString"
        /// </summary>
        public static string ConstructQueryString(System.Collections.Specialized.NameValueCollection parameters)
        {
            List<string> items = new List<string>();

            foreach (string name in parameters)
                items.Add(string.Concat(name, "=", System.Web.HttpUtility.UrlEncode(parameters[name])));

            return string.Join("&", items.ToArray());
        }

        public static string URLFixup(string url)
        {
            var keysToAdd = new List<string>();

            if (!Framework.ProductionVersion)
            {
                if (HttpContext.Current.Request.QueryString.AllKeys.Contains("dateOverride") && !url.Contains("dateOverride"))
                {
                    keysToAdd.Add("dateOverride");
                }

                if (HttpContext.Current.Request.QueryString.AllKeys.Contains("countryOverride") && !url.Contains("countryOverride"))
                {
                    keysToAdd.Add("countryOverride");
                }
            }
            else
            {
                url = System.Text.RegularExpressions.Regex.Replace(url, "dateOverride=.*\\??", "");
                url = System.Text.RegularExpressions.Regex.Replace(url, "countryOverride=.*\\??", "");
                url = url.TrimEnd(new char[] { '?' });
            }

            if (HttpContext.Current.Request.QueryString?.AllKeys.Contains("lifestyle") == true && !url.Contains("lifestyle"))
            {
                keysToAdd.Add("lifestyle");
            }

            return Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(url, keysToAdd.ToDictionary(k => k, k => HttpContext.Current.Request.QueryString[k]));
        }

        public static Stream GenerateStreamFromString(string input, System.Text.Encoding encoding)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, encoding);
            writer.Write(input);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static bool ProductionVersion 
        { 
            get
            {
                return Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]);
            }
        }

		public static IEnumerable<ProductPageFilterablePodWrapper> StreamSegmentOfJsonArray(string json, string targetArrayProperty, int skip, int take, string sort)
		{
			var allItems = new List<ProductPageFilterablePodWrapper>();

			// 1. Open the file stream sequentially
			using (var sr = new StringReader(json))
			using (var reader = new JsonTextReader(sr))
			{
				// 2. Scan forward until we find the target property name
				while (reader.Read())
				{
					if (reader.TokenType == JsonToken.PropertyName && (string)reader.Value == targetArrayProperty)
					{
						// Move the reader past the property name to the StartArray token '['
						reader.Read();
						break;
					}
				}

				// 3. Ensure we are actually looking at an array
				if (reader.TokenType != JsonToken.StartArray)
				{
					throw new JsonException("reader.TokenType != JsonToken.StartArray");
				}

				var serializer = new JsonSerializer();

				// 4. Stream ALL items into memory for sorting
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					var item = serializer.Deserialize<ProductPageFilterablePodWrapper>(reader);
					if (item != null)
					{
						allItems.Add(item);
					}
				}
			}

			IEnumerable<ProductPageFilterablePodWrapper> sortedResults;

			switch (sort)
			{
				case "bestSellers":
					sortedResults = allItems.OrderBy(p => p.ProductPage.SalesRank);
					break;
                case "colour":
                    sortedResults = allItems.OrderBy(p => p.ProductPage.ColourSort);
                    break;
				case "alphaAZ":
					sortedResults = allItems.OrderBy(p => p.ProductPage.AZSortOrder);
					break;
				case "alphaZA":
					sortedResults = allItems.OrderByDescending(p => p.ProductPage.AZSortOrder);
					break;
				case "new":
					sortedResults = allItems.OrderBy(p => p.ProductPage.NewSort);
					break;
				case "priceHighToLow":
					sortedResults = allItems.OrderByDescending(p => p.ProductPage.PriceSortOrder);
					break;
				case "priceLowToHigh":
					sortedResults = allItems.OrderBy(p => p.ProductPage.PriceSortOrder);
					break;
				case "discountHighToLow":
					sortedResults = allItems.OrderByDescending(p => p.ProductPage.DiscountSortOrder);
					break;
				case "discountLowToHigh":
					sortedResults = allItems.OrderBy(p => p.ProductPage.DiscountSortOrder);
					break;
				case "stockHighToLow":
                    sortedResults = allItems.OrderByDescending(p => p.ProductPage.StockSortOrder);
                    break;
				case "stockLowToHigh":
					sortedResults = allItems.OrderBy(p => p.ProductPage.StockSortOrder);
					break;
				case "default":
				default:
					sortedResults = allItems.OrderBy(p => p.ProductPage.MerchandisedSortOrder);
					break;
			}

			// 6. Paginate after sorting
			return sortedResults.Skip(skip).Take(take);
		}

		// Thanks again, Google
		public static IEnumerable<ProductPageFilterablePodWrapper> StreamingJsonReduction(string enormousJsonString, IEnumerable<int> productPageIds, int skip, int take, string sort)
		{
			// Convert to a HashSet for O(1) lookups instead of O(N) scans
			var targetIds = new HashSet<int>(productPageIds);
			var results = new List<ProductPageFilterablePodWrapper>();
			var serializer = new JsonSerializer();

			using (var stringReader = new StringReader(enormousJsonString))
			using (var jsonReader = new JsonTextReader(stringReader))
			{
				// 1. Advance the reader until we find the "productPages" property
				while (jsonReader.Read())
				{
					if (jsonReader.TokenType == JsonToken.PropertyName && "productPages".Equals(jsonReader.Value?.ToString(), StringComparison.OrdinalIgnoreCase))
					{
						break;
					}
				}

				// 2. Advance to the start of the array inside that property
				while (jsonReader.Read())
				{
					if (jsonReader.TokenType == JsonToken.StartArray)
					{
						break;
					}
				}

				// 3. Stream through the objects inside the array safely
				while (jsonReader.Read())
				{
					// Break out if we hit the end of the productPages array
					if (jsonReader.TokenType == JsonToken.EndArray)
					{
						break;
					}

					if (jsonReader.TokenType == JsonToken.StartObject)
					{
						// Only loads one array item into memory at a time
						var obj = JObject.Load(jsonReader);
						var idValue = obj["productPageId"]?.Value<int>();

						if (idValue.HasValue && targetIds.Contains(idValue.Value))
						{
							var pod = obj.ToObject<ProductPageFilterablePodWrapper>(serializer);
							if (pod != null)
							{
								results.Add(pod);
							}
						}
					}
				}
			}

			IEnumerable<ProductPageFilterablePodWrapper> sortedResults;

			switch (sort)
			{
				case "bestSellers":
					sortedResults = results.OrderBy(p => p.ProductPage.SalesRank);
					break;
				case "colour":
					sortedResults = results.OrderBy(p => p.ProductPage.ColourSort);
					break;
				case "alphaAZ":
					sortedResults = results.OrderBy(p => p.ProductPage.AZSortOrder);
					break;
				case "alphaZA":
					sortedResults = results.OrderByDescending(p => p.ProductPage.AZSortOrder);
					break;
				case "new":
					sortedResults = results.OrderBy(p => p.ProductPage.NewSort_New);
					break;
				case "priceHighToLow":
					sortedResults = results.OrderByDescending(p => p.ProductPage.PriceSortOrder);
					break;
				case "priceLowToHigh":
					sortedResults = results.OrderBy(p => p.ProductPage.PriceSortOrder);
					break;
				case "discountHighToLow":
					sortedResults = results.OrderByDescending(p => p.ProductPage.DiscountSortOrder);
					break;
				case "discountLowToHigh":
					sortedResults = results.OrderBy(p => p.ProductPage.DiscountSortOrder);
					break;
				case "stockHighToLow":
					sortedResults = results.OrderByDescending(p => p.ProductPage.StockSortOrder);
					break;
				case "stockLowToHigh":
					sortedResults = results.OrderBy(p => p.ProductPage.StockSortOrder);
					break;
				case "default":
				default:
					sortedResults = results.OrderBy(p => p.ProductPage.MerchandisedSortOrder);
					break;
			}

			// 6. Paginate after sorting
			return sortedResults.Skip(skip).Take(take);
		}

		public static IEnumerable<int> IntersectAll(IEnumerable<IEnumerable<int>> source)
		{
            using (var enumerator = source.GetEnumerator())
            {
                // If the outer collection is empty, return empty
                if (!enumerator.MoveNext())
                    return Enumerable.Empty<int>();

                // Load the first collection into a HashSet
                var intersectResult = new HashSet<int>(enumerator.Current);

                // Intersect with remaining collections in-place
                while (enumerator.MoveNext() && intersectResult.Count > 0)
                {
                    intersectResult.IntersectWith(enumerator.Current);
                }
                return intersectResult;
            }
		}
	}

    public static class StringExtension
    {
        public static string TrimReduceTruncate(this string str, int length)
        {
            if (str == null)
            {
                return "";
            }

            var result = ConvertWhitespacesToSingleSpaces(str).Trim();

            if (result.Length > length)
            {
                return result.Substring(0, length);
            }
            else
            {
                return result;
            }
        }

        public static string ConvertWhitespacesToSingleSpaces(this string value)
        {
            return Regex.Replace(value, @"\s+", " ");
        }
    }
}