using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.ViewModels;


namespace wwwDesignersGuild.Models.Interface
{
    public interface ICmsLandingPageRepository
    {
        Task<cmsLandingPageDefinition> loadLandingPageFromCms(
            int? memberId,
            int pageId,
            byte languageId,
            short countryId,
            DateTime useDate,
            bool isMember,
            bool isTrade,
            bool countryMatch,
            bool displayTradePrice,
            int? tradeAccountId,
            int? productPageGridPage,
			int? productPageGridPageSize,
            HttpResponseBase responseBase = null
        );
    }
}
