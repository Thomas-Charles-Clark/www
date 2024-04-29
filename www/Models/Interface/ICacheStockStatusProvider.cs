using System;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheStockStatusProvider
    {
        cmsStockStatusDefinition Get(byte languageId, short countryId, DateTime useDate);
        void Set(byte languageId, short countryId, DateTime useDate, cmsStockStatusDefinition data);
        bool IsSet(byte languageId, short countryId, DateTime useDate);
        void Invalidate(byte languageId, short countryId, DateTime useDate);
    }
}