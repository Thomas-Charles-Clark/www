using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICmsProductTypeOrderRestrictionRepository
    {
        Task<cmsProductTypeOrderRestrictionDefinition> loadProductTypeOrderRestrictionFromCms(Int32? memberTradeAccountId, byte languageId, short countryId, DateTime useDate);
    }
}