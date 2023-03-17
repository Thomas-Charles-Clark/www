using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Careers;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheCareersPageProvider
    {
        cmsCareersPageDefinition Get(byte languageId, short countryId, DateTime useDate);
        void Set(byte languageId, short countryId, DateTime useDate, cmsCareersPageDefinition data);
        bool IsSet(byte languageId, short countryId, DateTime useDate);
        void Invalidate(byte languageId, short countryId, DateTime useDate);
    }
}