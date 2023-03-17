using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.ViewModels
{
    public class FixedPageViewModel_CheckoutConfirm : BaseViewModel
    {
        public passCheckoutConfirm PassCheckoutConfirm { get; set; }

        public FixedPageViewModel_CheckoutConfirm(BaseViewModel objBase, passCheckoutConfirm passCheckoutConfirm) : base(objBase)
        {
            this.PassCheckoutConfirm = passCheckoutConfirm;
        }
    }
}