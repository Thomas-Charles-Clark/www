using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.ViewModels;

namespace wwwDesignersGuild.Models.ProductPage
{

    public class AttributeGroupViewModel : BaseViewModel
    {
        public AttributeGroup AttributeGroup { get; set; }
        public int Index { get; set; }
        public ProductPageGridPageSection Grid { get; set; }

        public AttributeGroupViewModel(
            ProductPageGridPageSection grid,
            AttributeGroup ag, 
            int idx, 
            BaseViewModel bvm) : base(bvm)
        {
            Grid = grid;
            AttributeGroup = ag;
            Index = idx;
        }
    }
}