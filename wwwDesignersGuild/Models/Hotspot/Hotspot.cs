using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Cache.General;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.ImgSize;
using wwwDesignersGuild.ViewModels;

namespace wwwDesignersGuild.Models.Hotspot
{
    public class cmsHotspotDefinition
    {
        public IEnumerable<ImageHotspot> Hotspots { get; set; }

        public bool TimedOut { get; set; }
    }

    public class PassHotspotCacheFetch
    {
        [JsonIgnore]
        public long VisitorID { get; set; }

        [JsonIgnore]
        public int? MemberID { get; set; }

        public int ContentID { get; set; }
        public dgContentType ContentType { get; set; }

        public int? TradeAccountID { get; set; }
        public bool DisplayTradePrice { get; set; }
        public short CountryID { get; set; }
        public byte LanguageID { get; set; }

        [JsonIgnore]
        public DateTime ContentDate { get; set; }

        public string Date
        {
            get
            {
                return $"{ContentDate:ddMMyyyy}";
            }
            set
            {

            }
        }

        public IEnumerable<long> ImageIDs { get; set; }

        public bool CountryMatch
        {
            get
            {
                return Convert.ToBoolean(HttpContext.Current.Session["countryCheckMatched"]);
            }
        }

        public PassHotspotCacheFetch(
            int contentId,
            dgContentType contentType,
            long visitorId,
            int? memberId,
            int? tradeAccountId,
            bool displayTradePrice,
            short countryId,
            byte languageId,
            DateTime contentDate,
            IEnumerable<long> imageIds
            )
        {
            ContentID = contentId;
            ContentType = contentType;
            VisitorID = visitorId;
            MemberID = memberId;
            TradeAccountID = tradeAccountId;
            DisplayTradePrice = displayTradePrice;
            CountryID = countryId;
            LanguageID = languageId;
            ContentDate = contentDate;
            ImageIDs = imageIds.Distinct().OrderBy(i => i);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }


    public class cmsHotspot
    {
        public async Task<cmsHotspotDefinition> loadHotspotsFromCmsAsync(PassHotspotCacheFetch objFetch, BaseViewModel objBase)
        {
            // Is page in cache?
            var cache = new HotspotCacheProvider();
            var data = cache.Get(objFetch);

            if (data == null)
            {
                data = await loadHotspotsFromCms(objFetch, objBase);

                if (!data.TimedOut)
                {
                    cache.Set(objFetch, data);
                }
            }

            return data;
        }

        public void SetHotspot(
            PassHotspotCacheFetch objFetch,
            cmsHotspotDefinition objScrapbook)
        {
            var cache = new HotspotCacheProvider();
            cache.Set(objFetch, objScrapbook);
        }

        private async Task<cmsHotspotDefinition> loadHotspotsFromCms(PassHotspotCacheFetch objFetch, BaseViewModel objBase)
        {
            var results = await new databaseDataContext().FetchHotspots(objFetch);

            return new cmsHotspotDefinition()
            {
                Hotspots = JObject.FromObject(results)
                    .SelectToken("images")
                    .Children()
                    .Where(i => i.SelectToken("hotspots") != null)
                    .Select(i =>
                        new ImageHotspot()
                        {
                            ImageID = i.SelectToken("imageId").Value<long>(),
                            Hotspots = i.SelectToken("hotspots")
                                .Children()
                                .Select(h =>
                                    new Hotspot()
                                    {
                                        X = h.SelectToken("x").Value<decimal>(),
                                        Y = h.SelectToken("y").Value<decimal>(),
                                        Product = new OrderFormItemViewModel(
                                            JObject.FromObject(h),
                                            objBase.MasterDefinition,
                                            objBase,
                                            true,
                                            ImageSize.Two,
                                            false)
                                    }
                            )
                        }
                    ),
                TimedOut = JObject.FromObject(results)
                    .SelectToken("timedOut").Value<bool>()
            };
        }
    }

    public class Hotspot
    {
        public decimal X { get; set; }
        public string x
        {
            get
            {
                return $"calc({X:N4}% - 12.5px)";
            }
        }

        public decimal Y { get; set; }
        public string y
        {
            get
            {
                return $"calc({Y:N4}% - 12.5px)";
            }
        }

        public string OverlayPosition
        {
            get
            {
                var result = "top";

                if (X > 50)
                {
                    result += "-right";
                }
                else
                {
                    result += "-left";
                }

                return result;
            }
        }

        public OrderFormItemViewModel Product { get; set; }
    }
}