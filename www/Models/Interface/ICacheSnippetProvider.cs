using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheSnippetProvider
    {
        cmsSnippetDefinition Get(byte languageId, short countryId, DateTime useDate);
        void Set(byte languageId, short countryId, DateTime useDate, cmsSnippetDefinition data);
        bool IsSet(byte languageId, short countryId, DateTime useDate);
        void Invalidate(byte languageId, short countryId, DateTime useDate);
    }
}
