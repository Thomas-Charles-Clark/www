using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Banner;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICmsBannerRepository
    {
        Task<cmsBannerDefinition> loadBannersFromCms(short countryId, byte languageId, DateTime useDate, bool isTrade, bool platformSwitch);
    }
}