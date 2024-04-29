using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheMenuProvider
    {
        cmsMenuDefinition Get(byte languageId, short countryId, DateTime useDate, bool isTrade);
        void Set(byte languageId, short countryId, DateTime useDate, cmsMenuDefinition data, bool isTrade);
        bool IsSet(byte languageId, short countryId, DateTime useDate, bool isTrade);
        void Invalidate(byte languageId, short countryId, DateTime useDate, bool isTrade);
    }
}
