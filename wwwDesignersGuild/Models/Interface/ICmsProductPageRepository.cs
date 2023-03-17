using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;


namespace wwwDesignersGuild.Models.Interface
{
    public interface ICmsProductPageRepository
    {
        Task<cmsProductPageDefinition> loadProductPageFromCms(int productPageId, byte languageId, short countryId, DateTime useDate, bool isTrade);

    }
}
