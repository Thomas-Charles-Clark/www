using MertexerV2.Utilities;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.ProductReview;
using wwwDesignersGuild.ViewModels;

namespace wwwDesignersGuild.Helpers
{
    public static class JsonLDHelper
    {
        /// <summary>
        /// JSON-LD writer.
        /// It will need to be told what type of object it's receiving,
        /// then cast the incoming object to a type... and if it fails fail quietly!
        /// 
        /// </summary>
        /// <param name="html"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static JsonLD JsonLD(this HtmlHelper html, string type, object objData, string description)
        {
            return new JsonLD(type, objData, description);
        }

        public static JsonLD JsonLD(this HtmlHelper html, string type, object objData, string description, string currencyCode)
        {
            return new JsonLD(type, objData, description, currencyCode);
        }
    }

    public class JsonLD : IHtmlString
    {
        private readonly string type;

        private readonly object objModel;

        private readonly string description;

        private readonly string currencyCode;

        private readonly Regex rrpRegex = new Regex(@"([^\d?*.\d?*])(\(.*\))?(inc\.)?");

        public JsonLD(string text, object objModel, string description)
        {
            this.type = text;
            this.objModel = objModel;
            this.description = description?.Replace(System.Environment.NewLine, "");
        }
        public JsonLD(string text, object objModel, string description, string currency)
        {
            this.type = text;
            this.objModel = objModel;
            this.description = description?.Replace(System.Environment.NewLine, "").Replace("\n", "");
            this.currencyCode = currency;
        }

        public override string ToString()
        {
            return ToHtmlString();
        }

        public string ToHtmlString()
        {
            var result = "<script type='application/ld+json'>";
            try
            {
                switch (this.type)
                {
                    #region Base
                    case "Base":
                        //Provide: WebPage, Organisation, WebSite
                        result += @"
[{
    ""@context"": ""http://schema.org"",
    ""@type"": ""Organization"",
    ""name"": ""Designers Guild"",
    ""url"": ""https://www.designersguild.com"",
    ""logo"": {
        ""@type"": ""ImageObject"",
        ""url"": ""https://www.designersguild.com/Content/images/logo/designers_guild_logo.png"",
        ""width"": ""350"",
        ""height"": ""25""
    },
    ""sameAs"": [""//www.instagram.com/designersguild"", ""//www.facebook.com/designersguildgroup"", ""//www.youtube.com/user/DesignersGuildGroup""]
}, {
    ""@context"": ""http://schema.org"",
    ""@type"": ""WebSite"",
    ""name"": ""Designers Guild"",
    ""url"": ""https://www.designersguild.com""
}]
";
                        break;
                    #endregion

                    #region Primary Products
                    case "PrimaryProducts":

                        var model = objModel as ProductPageViewModel;

                        var primaries = model.AllProducts;

                        if (!model.AllProductsUnavailable && primaries != null)
                        {
                            var noPrices = false;

                            var noReviews = false;

                            var build = @"[";

                            foreach (var p in primaries)
                            {
                                var productString = $"";

                                productString += @"{";
                                productString += $@"
    ""@context"": ""http://www.schema.org"",
    ""@type"": ""Product"",
    ""name"": ""{(p.BasketName ?? "").Replace("\"", @"\""")}"",
    ""image"": ""https://www.designersguild.com/image/1024/{p.Image.imageId}{Util.Framework.WebpOrJpgExtension}"",
    ""description"": ""{description?.Replace("\"", @"\""") ?? p.BasketName}"",
    ""brand"": {{ 
        ""@type"": ""Brand"", 
        ""name"": ""{p.Brand}"" 
    }},
    ""sku"": ""{p.ProductCode}""";

                                if (p.Product.ContainsKey("gtin13"))
                                {
                                    productString += $@",
    ""gtin13"": ""{p.Product["gtin13"].Value<string>()}""";
                                }

                                if (JObject.FromObject(model.Definition.json).SelectToken("reviews").Type != JTokenType.Null)
                                {
                                    var reviews = JObject.FromObject(model.Definition.json)
                                        .SelectToken("reviews")
                                        .ToObject<List<ProductReview>>()
                                        .OrderByDescending(r => r.DateTime)
                                        .ToList();
                                    
                                    if (reviews.Any())
                                    {
                                        productString += $@",
    ""review"": [";

                                        foreach (var r in reviews)
                                        {
                                            productString += $@"{{
            ""@type"": ""Review"",
            ""reviewRating"": {{
                ""@type"": ""Rating"",
                ""ratingValue"": ""{r.Rating}""
            }},
            ""author"": {{
                ""@type"": ""Organization"",
                ""name"": ""Feefo""
            }},
            ""reviewBody"": ""{HtmlHelperExtra.EscapeForJS(r.Review)}""
        }},";
                                        }

                                        productString = productString.TrimEnd(new char[] { ',' });

                                        productString += $@"],
    ""aggregateRating"": {{
        ""@type"": ""AggregateRating"",
        ""worstRating"": ""1"",
        ""bestRating"": ""5"",
        ""ratingValue"": ""{reviews.Select(q => q.Rating).Sum() / reviews.Count}"",
        ""ratingCount"": ""{reviews.Count}""
    }}";

                                    }
                                    else
                                    {
                                        noReviews = true;
                                    }
                                }
                                else
                                {
                                    noReviews = true;
                                }

                                if (p.Product.ContainsKey("displayPrice") && p.Product["displayPrice"].Type != JTokenType.Null) // availableToBuy
                                {
                                    if (p.Product.ContainsKey("displaySalePrice") && p.Product["displaySalePrice"].Type != JTokenType.Null)
                                    {
                                        productString += $@",
    ""offers"": {{
        ""@type"": ""Offer"",
        ""price"": ""{p.Product["displaySalePrice"].Value<decimal>():F}"",
        ""priceCurrency"": ""{currencyCode}"",
        ""availability"": ""{p.Product["itemAvailability"].Value<string>()}"",
        ""url"": ""https://www.designersguild.com{p.Product["url"].Value<string>()}""
    }}";
                                    }
                                    else
                                    {
                                        productString += $@",
    ""offers"": {{
        ""@type"": ""Offer"",
        ""price"": ""{p.Product["displayPrice"]?.Value<decimal>():F}"",
        ""priceCurrency"": ""{currencyCode}"",
        ""availability"": ""{p.Product["itemAvailability"]?.Value<string>()}"",
        ""url"": ""https://www.designersguild.com{p.Product["url"]?.Value<string>()}""
    }}";
                                    }
                                }
                                else if (p.Product.ContainsKey("rRPMessage") && p.Product["rRPMessage"].Type != JTokenType.Null)
                                {
                                    productString += $@",
    ""offers"": {{
        ""@type"": ""Offer"",
        ""price"": ""{rrpRegex.Replace(p.Product["rRPMessage"].Value<string>(), "")}"",
        ""priceCurrency"": ""{currencyCode}"",
        ""availability"": ""InStoreOnly"",
        ""url"": ""{p.Product["url"].Value<string>()}""
    }}";
                                }
                                else
                                {
                                    noPrices = true;
                                }

                                productString += @"
}, ";

                                if (noPrices && noReviews)
                                {
                                    // no prices, no reviews: Google will complain "Either 'offers', 'reviews', or 'aggregateRating' should be specified
                                }
                                else
                                {
                                    build += productString;
                                }

                                if (p == primaries.Last())
                                    build = build.TrimEnd(new char[] { ',', ' ' });

                            }

                            build = build + @"]";

                            result = result + build;
                        }
                        break;

                    #endregion

                    case "Category":

                        var aggregateData = this.objModel as pageRatingAggregate;

                        result += $@"{{
    ""@context"": ""http://www.schema.org"",
    ""@type"": ""Product"",
    ""category"": ""{aggregateData.Type}"",
    ""aggregateRating"": {{
        ""@type"": ""AggregateRating"",
        ""ratingValue"": ""{aggregateData.RatingValue}"",
        ""bestRating"": ""{aggregateData.BestRating}"",
        ""ratingCount"": ""{aggregateData.RatingCount}""
    }}
}}";

                        break;
                }

                result += "</script>";

                result += System.Environment.NewLine;

                result += Unfurling();
            }
            catch (Exception)
            {

            }

            return result;
        }
    
        public string Unfurling()
        {
            var sb = new StringBuilder();

            try
            {
                switch (this.type)
                {
                    case "LandingPage":
                        var model = objModel as LandingPageViewModel;
                        sb.AppendLine($@"<meta property=""og:url"" content=""{HttpContext.Current.Request.Url}"" />");
                        sb.AppendLine($@"<meta property=""og:type"" content=""website"" />");
                        sb.AppendLine($@"<meta property=""og:title"" content=""{model.MasterDefinition.TitleTag}"" />");
                        sb.AppendLine($@"<meta property=""og:description"" content=""{description}"" />");
                        if (model.FirstImageID.HasValue)
                        {
                            sb.AppendLine($@"<meta property=""og:image"" content=""https://www.designersguild.com/image/1024/{model.FirstImageID}{Util.Framework.WebpOrJpgExtension}"" />");
                        }
                        break;
                    case "PrimaryProducts":
                        var p = (objModel as ProductPageViewModel).MainProduct;

                        if (p != null)
                        {
                            // OpenGraph
                            sb.AppendLine($@"<meta property=""og:url"" content=""{p.Product["url"].Value<string>()}"" />");
                            sb.AppendLine($@"<meta property=""og:type"" content=""website"" />");
                            sb.AppendLine($@"<meta property=""og:title"" content=""{p.BasketName}"" />");
                            sb.AppendLine($@"<meta property=""og:description"" content=""{description}"" />");
                            sb.AppendLine($@"<meta property=""og:image"" content=""https://www.designersguild.com/image/1024/{p.Image.imageId}{Util.Framework.WebpOrJpgExtension}"" />");

                            sb.AppendLine($@"<meta property=""twitter:card"" content=""summary"" />");
                            sb.AppendLine($@"<meta property=""twitter:site"" content=""@DesignersGuild"" />");
                            sb.AppendLine($@"<meta property=""twitter:title"" content=""{p.BasketName}"" />");
                            sb.AppendLine($@"<meta property=""twitter:description"" content=""{description}"" />");
                            sb.AppendLine($@"<meta property=""twitter:image"" content=""https://www.designersguild.com/image/1024/{p.Image.imageId}{Util.Framework.WebpOrJpgExtension}"" />");
                            sb.AppendLine($@"<meta property=""twitter:image:alt"" content=""{p.BasketName}"" />");
                        }

                        break;
                    default:
                        break;
                }
            }
            catch(Exception)
            {

            }

            return sb.ToString();
        }
    }
}