using System;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheProductTypeOrderRestrictionProvider
    {
        cmsProductTypeOrderRestrictionDefinition Get(Int32? memberTradeAccountId, Byte languageId, Int16 countryId, DateTime useDate);
        void Set(Int32? memberTradeAccountId, Byte languageId, Int16 countryId, DateTime useDate, cmsProductTypeOrderRestrictionDefinition data);
        bool IsSet(Int32? memberTradeAccountId, Byte languageId, Int16 countryId, DateTime useDate);
        void Invalidate(Int32? memberTradeAccountId, Byte languageId, Int16 countryId, DateTime useDate);
    }
}