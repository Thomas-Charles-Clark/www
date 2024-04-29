using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheSearchProductTypeDropDownProvider
    {
        cmsSearchProductTypeDropDownDefinition Get(short countryId, byte languageId, DateTime contentDate);
        void Set(cmsSearchProductTypeDropDownDefinition data, short countryId, byte languageId);
        bool IsSet(short countryId, byte languageId);
        void Invalidate(short countryId, byte languageId);
    }
}