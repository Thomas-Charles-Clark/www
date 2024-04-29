using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;


namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheCollectionProductPageProvider
    {
        cmsCollectionProductPageDefinition Get(int collectionId, byte languageId, short countryId, DateTime useDate, byte? brandId, byte? productTypeId, string sort);
        void Set(int collectionId, byte languageId, short countryId, DateTime useDate, byte? brandId, byte? productTypeId, string sort, cmsCollectionProductPageDefinition data);
        bool IsSet(int collectionId, byte languageId, short countryId, DateTime useDate, byte? brandId, byte? productTypeId, string sort);
        void Invalidate(int collectionId, byte languageId, short countryId, DateTime useDate, byte? brandId, byte? productTypeId, string sort);
    }
}
