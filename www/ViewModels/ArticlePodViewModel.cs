using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.ViewModels
{
    public class ArticlePodViewModel
    {
        public NewMasterDefinition MasterDefinition { get; set; }

        public pageSectionArticle Article { get; set; }

        // set by the ArticleGrid template as rendering occurs

        public ArticlePodViewModel(
            NewMasterDefinition objMaster,
            pageSectionArticle objArticle)
        {
            MasterDefinition = objMaster;
            Article = objArticle;
        }
    }
}