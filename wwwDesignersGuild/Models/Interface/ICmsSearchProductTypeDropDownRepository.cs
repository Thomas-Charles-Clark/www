using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;


namespace wwwDesignersGuild.Models.Interface
{
    public interface ICmsSearchProductTypeDropDownRepository
    {
        Task<cmsSearchProductTypeDropDownDefinition> loadSearchProductTypeDropDownFromCms(Int16 countryId, Byte languageId, DateTime contentDate);

    }
}
