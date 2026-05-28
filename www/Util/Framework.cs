using Newtonsoft.Json;

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

		public static IEnumerable<ProductPageFilterablePod> StreamSegmentOfJsonArray(string json, string targetArrayProperty, int skip, int take)
		{
			int currentIndex = 0;
			int itemsDeserialized = 0;

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

				// 4. Stream through the array items one by one
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					// Check if the current item falls within our target segment bounds
					if (currentIndex >= skip && itemsDeserialized < take)
					{
						// Deserialize only this specific single object into memory
						var item = serializer.Deserialize<ProductPageFilterablePod>(reader);

                        yield return item;

						itemsDeserialized++;
					}
					else
					{
						// Skip the tokens of this object entirely without deserializing it
						reader.Skip();
					}

					currentIndex++;

					// Optimization: Break early if we have collected enough items
					if (itemsDeserialized >= take)
					{
						break;
					}
				}
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