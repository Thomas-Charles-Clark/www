using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Cache.General;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;

namespace wwwDesignersGuild.Models.DotDigital
{
    public class cmsDotDigital
    {
        public async Task<DotDigitalTrackingBehaviourProfile> loadDotDigitalBehaviourProfile(short countryId, byte languageId, DateTime useDate)
        {
            // Is page in cache?
            var cache = new DotDigitalTrackingBehaviourCacheProvider();

            var data = cache.GetOrCreate(countryId, languageId, useDate, () => {

                var result = loadDotDigitalBehaviourProfileFromCms(countryId, languageId);

                return result;

			});

            return data;
        }

        private DotDigitalTrackingBehaviourProfile loadDotDigitalBehaviourProfileFromCms(short countryId, byte languageId)
        {
            return new databaseDataContext().DotDigital_TrackingBehaviourProfileID_Fetch(countryId, languageId).Single();
        }
    }
}