using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;


namespace wwwDesignersGuild.Models.Interface
{
    public interface ICmsCollectionPageRepository
    {
        Task<cmsCollectionPageDefinition> loadCollectionPageFromCms(short collectionId, byte languageId, short countryId, int? memberId, int? tradeAccountId, bool displayTradePrice, bool countryMatch, DateTime useDate, bool isTrade);
    }
}