using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICmsUnsubscribePageRepository
    {
        Task<cmsUnsubscribePageDefinition> loadUnsubscribePageFromCms(byte languageId, short countryId, DateTime useDate, Boolean isMember, Boolean isTrade);
    }
}