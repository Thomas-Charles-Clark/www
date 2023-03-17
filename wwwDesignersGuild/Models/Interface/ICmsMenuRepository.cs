using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;


namespace wwwDesignersGuild.Models.Interface
{
    public interface ICmsMenuRepository
    {
        Task<cmsMenuDefinition> loadMenuFromCms(byte languageId, short countryId, DateTime useDate, bool isTrade);

    }
}
