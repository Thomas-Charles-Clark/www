using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.ViewModels
{
    public class ArticlePodViewModel
    {
        public NewMasterDefinition MasterDefinition { get; set; }

        public PageSectionArticle Article { get; set; }

        // set by the ArticleGrid template as rendering occurs

        public ArticlePodViewModel(
            NewMasterDefinition objMaster,
            PageSectionArticle objArticle)
        {
            MasterDefinition = objMaster;
            Article = objArticle;
        }
    }
}