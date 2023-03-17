using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheProductPageProvider
    {
        cmsProductPageDefinition Get(int productPageId, byte languageId, short countryId, bool tradeSwitch, DateTime useDate);
        void Set(int productPageId, byte languageId, short countryId, bool tradeSwitch, DateTime useDate, cmsProductPageDefinition data);
        bool IsSet(int productPageId, byte languageId, short countryId, bool tradeSwitch, DateTime useDate);
        void Invalidate(int productPageId, byte languageId, short countryId, bool tradeSwitch, DateTime useDate);
    }
}
