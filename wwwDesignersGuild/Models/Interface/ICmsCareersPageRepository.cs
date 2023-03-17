using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Careers;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICmsCareersPageRepository
    {
        Task<cmsCareersPageDefinition> loadCareersPageFromCms(byte languageId, short countryId, DateTime useDate);
    }
}