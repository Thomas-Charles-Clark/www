using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.ViewModels;

namespace wwwDesignersGuild.Models.SearchRender
{
    public class PassSearchRender
    {
        public BaseViewModel @base { get; set; }

        public IEnumerable<int> productPageIds { get; set; }
    }
}