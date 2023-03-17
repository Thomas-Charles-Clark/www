using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;


namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheCollectionPageProvider
    {
        cmsCollectionPageDefinition Get(int collectionId, byte languageId, short countryId, DateTime useDate, bool isTrade);
        void Set(int collectionId, byte languageId, short countryId, DateTime useDate, bool isTrade, cmsCollectionPageDefinition data);
        bool IsSet(int collectionId, byte languageId, short countryId, DateTime useDate, bool isTrade);
        void Invalidate(int collectionId, byte languageId, short countryId, DateTime useDate, bool isTrade);
    }
}

