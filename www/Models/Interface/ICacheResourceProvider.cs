using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheResourceProvider
    {
        List<resourceSelect> Get(byte languageId, short countryId, DateTime useDate);
        void Set(byte languageId, short countryId, DateTime useDate, List<resourceSelect> data);
        bool IsSet(byte languageId, short countryId, DateTime useDate);
        void Invalidate(byte languageId, short countryId, DateTime useDate);
    }
}