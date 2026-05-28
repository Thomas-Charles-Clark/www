using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;


namespace wwwDesignersGuild.Models.Interface
{
    public interface ICmsLandingPageRepository
    {
        Task<cmsLandingPageDefinition> loadLandingPageFromCms(int? memberId, int pageId, byte languageId, short countryId, DateTime useDate, bool isMember, bool isTrade, bool countryMatch, bool displayTradePrice, int? tradeAccountId, int? productPageGridPageId = null, int? productPageGridPageSize = null);
    }
}
