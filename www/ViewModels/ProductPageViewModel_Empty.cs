using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels
{
    public class ProductPageViewModel_Empty
    {
        public EmptyDefinition Definition { get; set; }

        public ProductPageViewModel_Empty()
        {
            Definition = new EmptyDefinition();
        }
    }

    public class EmptyDefinition
    {
        public object json { get; set; }

        public EmptyDefinition()
        {
            json = new object();
        }
    }
}