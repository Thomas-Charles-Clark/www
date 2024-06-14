using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.ViewModels
{
    public class OrderFormGroupViewModel
    {
        public int? OrderFormGroupID { get; set; }
        public string Name { get; set; }
        public bool UseSizeButtons { get; set; }
        public IEnumerable<OrderFormItemViewModel> Items { get; set; }
        public NewMasterDefinition MasterDefinition { get; set; }
        public int ParentProductPageProductTypeID { get; set; }

        public int SortOrder 
        {
            get 
            {
                return Items.Select(i => i.SortOrder).Min();
            } 
        }

        public string OrderFormCarouselID
        {
            get
            {
                return $"order-form-group-{OrderFormGroupID}-carousel";
            }
        }

        public bool AutoOpenFirstSize { get; set; }
        
        public string AutoOpenPartNumber { get; set; }

        public OrderFormGroupViewModel(
            int? orderFormGroupId, 
            string name,
            bool useSizeButtons,
            bool autoOpenFirstSize,
            string autoOpenPartNumber,
            IEnumerable<OrderFormItemViewModel> items,
            NewMasterDefinition objMaster)
        {
            OrderFormGroupID = orderFormGroupId;
            Name = name;
            UseSizeButtons = useSizeButtons;
            AutoOpenFirstSize = String.IsNullOrEmpty(autoOpenPartNumber) ? autoOpenFirstSize : false;
            AutoOpenPartNumber = autoOpenPartNumber;
            Items = items;
            MasterDefinition = objMaster;
        }
    }
}