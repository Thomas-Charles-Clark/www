using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheLifestyleGalleryPopupProvider
    {
        cmsLifestyleGalleryPopupDefinition Get(int productPageId, byte languageId, short countryId, DateTime useDate);
        void Set(int productPageId, byte languageId, short countryId, DateTime useDate, cmsLifestyleGalleryPopupDefinition data);
        bool IsSet(int productPageId, byte languageId, short countryId, DateTime useDate);
        void Invalidate(int productPageId, byte languageId, short countryId, DateTime useDate);
    }
}
