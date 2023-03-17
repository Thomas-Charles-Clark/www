using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheUnsubscribePageProvider
    {
        cmsUnsubscribePageDefinition Get(byte languageId, short countryId, DateTime useDate);
        void Set(byte languageId, short countryId, DateTime useDate, cmsUnsubscribePageDefinition data);
        bool IsSet(byte languageId, short countryId, DateTime useDate);
        void Invalidate(byte languageId, short countryId, DateTime useDate);
    }
}
