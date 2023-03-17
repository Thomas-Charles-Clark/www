using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICmsCollectionProductPageRepository
    {
        Task<cmsCollectionProductPageDefinition> loadCollectionProductPageFromCms(short collectionId, byte languageId, short countryId, DateTime useDate, byte? brandId, byte? productTypeId, string sort, Int16 CacheTimeMinutes);
    }
}
