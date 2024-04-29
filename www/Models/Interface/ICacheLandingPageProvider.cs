using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheLandingPageProvider
    {
        cmsLandingPageDefinition Get(int pageId, byte languageId, short countryId, DateTime useDate, bool isTrade);
        void Set(int pageId, byte languageId, short countryId, DateTime useDate, bool isTrade, cmsLandingPageDefinition data);
        bool IsSet(int pageId, byte languageId, short countryId, DateTime useDate, bool isTrade);
        void Invalidate(int pageId, byte languageId, short countryId, DateTime useDate, bool isTrade);
    }
}
