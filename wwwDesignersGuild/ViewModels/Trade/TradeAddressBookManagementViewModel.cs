using MertexerV2.Models.Accounts;
using MertexerV2SQL.Models.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Trade;

namespace wwwDesignersGuild.ViewModels.Trade
{
    public class TradeAddressBookManagementViewModel
    {
        public BaseViewModel BaseViewModel { get; set; }

        public tradeAccountEmailsModel Emails { get; set; }

        public TradeAccountUsersModel Users { get; set; }

        public MertexInvoiceAddress InvoiceAddress { get; set; }

        public string DefaultDeliveryReference { get; set; }

        public bool Permission { get; set; }

        public TradeAddressBookManagementViewModel(
            tradeAccountEmailsModel emails,
            TradeAccountUsersModel users,
            MertexInvoiceAddress invoiceAddress,
            string defaultDeliveryReference,
            bool permission,
            BaseViewModel objBase)
        {
            Emails = emails;
            Users = users;
            InvoiceAddress = invoiceAddress;
            DefaultDeliveryReference = defaultDeliveryReference;
            Permission = permission;
            BaseViewModel = objBase;
        }
    }
}