using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Banner;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheBannerProvider
    {
        //  false = Desktop Only, true = Mobile Only 
        cmsBannerDefinition Get(short countryId, byte languageId, DateTime useDate, bool isTrade, bool platformSwitch);
        void Set(cmsBannerDefinition data, short countryId, byte languageId, DateTime useDate, bool isTrade, bool platformSwitch);
        bool IsSet(short countryId, byte languageId, DateTime useDate, bool isTrade, bool platformSwitch);
        void Invalidate(short countryId, byte languageId, DateTime useDate, bool isTrade, bool platformSwitch);
    }
}