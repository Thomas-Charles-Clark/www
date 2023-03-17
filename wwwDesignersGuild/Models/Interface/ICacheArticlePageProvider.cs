using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheArticlePageProvider
    {
        cmsArticlePageDefinition Get(int articleId, byte languageId, short countryId, DateTime useDate);
        void Set(int articleId, byte languageId, short countryId, DateTime useDate, cmsArticlePageDefinition data);
        bool IsSet(int articleId, byte languageId, short countryId, DateTime useDate);
        void Invalidate(int articleId, byte languageId, short countryId, DateTime useDate);
    }
}