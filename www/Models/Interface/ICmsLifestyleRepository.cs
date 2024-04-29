using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;


namespace wwwDesignersGuild.Models.Interface
{
    public interface ICmsLifestyleGalleryPopupRepository
    {
        Task<cmsLifestyleGalleryPopupDefinition> loadLifestyleGalleryPopupFromCms(int productPageId, byte languageId, short countryId, bool isTrade, DateTime useDate);
    }
}
