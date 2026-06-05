using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.SearchRender
{
    public class PassSearchRender
    {
        public NewMasterDefinition masterDefinition { get; set; }

        public IEnumerable<int> productPageIds { get; set; }
    }
}