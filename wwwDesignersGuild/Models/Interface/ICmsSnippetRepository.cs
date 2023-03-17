using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;


namespace wwwDesignersGuild.Models.Interface
{
    public interface ICmsSnippetRepository
    {
        Task<cmsSnippetDefinition> loadSnippetFromCms(byte languageId, short countryId, int? tradeAccountId, bool displayTradePrices, DateTime useDate);

    }
}