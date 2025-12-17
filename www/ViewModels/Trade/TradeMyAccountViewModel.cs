using MertexerV2.Models.Accounts;
using MertexerV2SQL.Models.Accounts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Trade;
using wwwDesignersGuild.Models.Trade.Accounting;
using wwwDesignersGuild.ViewModels;

namespace wwwDesignersGuild.ViewModels.Trade
{
    public class PassTradeAccessRequest
    {
        public string tradeAccountName { get; set; }
        public string tradeAccountCode { get; set; }
        public string userName { get; set; }
        public string userEmail { get; set; }
        public string userPhone { get; set; }

        public string tradeAccountLabel
        {
            get
            {
                var hyphen = "";

                if (!String.IsNullOrEmpty(tradeAccountCode) && !String.IsNullOrEmpty(tradeAccountName))
                {
                    hyphen = " - ";
                }

                return $"{tradeAccountCode}{hyphen}{tradeAccountName}";
            }
            set
            {

            }
        }
    }

    public class webapiSaveTradeUserPermissions
    {
        public int? MemberID { get; set; }
        public string Email { get; set; }
        public bool ManageAccount { get; set; }
        public bool PlaceOrders { get; set; }
        public bool ViewAccounts { get; set; }
        public bool ViewSalesStats { get; set; }
        public bool DownloadAssets { get; set; }
    }
    
    public class TradeContactUsViewModel
    {
        public BaseViewModel BaseViewModel { get; set; }
        public tradeContactUsResult ContactDetails { get; set; }
    }

    public class TradeAccountsInvoicesViewModel
    {
        public BaseViewModel BaseViewModel { get; set; }
        public string heading { get; set; }
        public bool permission { get; set; }
        public TradeAccountUsersModel tradeAccountModel { get; set; }
        public Trade_Enquiry_AccountBalance_Result accountBalance { get; set; }
        public Trade_Enquiry_InvoiceSearch_Result outstandingInvoices { get; set; }
        public Trade_Enquiry_RecentPayments_Result recentPayments { get; set; }

        public TradeAccountsInvoicesViewModel (
            BaseViewModel bvm, 
            string h, 
            bool p,
            TradeAccountUsersModel tam,
			Trade_Enquiry_AccountBalance_Result asr,
			Trade_Enquiry_InvoiceSearch_Result lir,
			Trade_Enquiry_RecentPayments_Result rpr)
        {
            BaseViewModel = bvm;
            heading = h;
            permission = p;
            tradeAccountModel = tam;
            accountBalance = asr;
            outstandingInvoices = lir;
            recentPayments = rpr;
        }
    }

    public class TradeAccountsProformaInvoicesViewModel
    {
        public BaseViewModel BaseViewModel { get; set; }
        public string heading { get; set; }
        public bool permission { get; set; }
        public TradeAccountUsersModel tradeAccountModel { get; set; }
        public ListProformaInvoicesReturn proformaInvoices { get; set; }

        public TradeAccountsProformaInvoicesViewModel(
            BaseViewModel bvm,
            string h,
            bool p,
            TradeAccountUsersModel tam,
            ListProformaInvoicesReturn pro)
        {
            BaseViewModel = bvm;
            heading = h;
            permission = p;
            tradeAccountModel = tam;
            proformaInvoices = pro;
        }
    }

    public class TradeAccountOrderStatusModel
    {
        public BaseViewModel BaseViewModel { get; set; }
        public bool permission { get; set; }

        public TradeAccountUsersModel tradeAccountModel { get; set; }
        public OutstandingOrdersReturn outstandingOrders { get; set; }
        public OutstandingOrdersReturn backOrders { get; set; }
        public OutstandingOrdersReturn reserveOrders { get; set; }
        public OutstandingOrdersReturn despatchOrders { get; set; }
        public OutstandingOrdersReturn ordersInProgress { get; set; }
        public OutstandingOrdersReturn heldOrders { get; set; }

        public TradeAccountOrderStatusModel (
            BaseViewModel bvm,
            bool p,
            TradeAccountUsersModel tam,
            OutstandingOrdersReturn os,
            OutstandingOrdersReturn bo,
            OutstandingOrdersReturn ro,
            OutstandingOrdersReturn deo,
            OutstandingOrdersReturn ip,
            OutstandingOrdersReturn ho)
        {
            BaseViewModel = bvm;
            permission = p;
            tradeAccountModel = tam;
            outstandingOrders = os;
            backOrders = bo;
            reserveOrders = ro;
            despatchOrders = deo;
            ordersInProgress = ip;
            heldOrders = ho;
        }

        public TradeAccountOrderStatusModel(
            BaseViewModel bvm,
            bool p,
            TradeAccountUsersModel tam)
        {
            BaseViewModel = bvm;
            permission = p;
            tradeAccountModel = tam;
            outstandingOrders = new OutstandingOrdersReturn();
            backOrders = new OutstandingOrdersReturn();
            reserveOrders = new OutstandingOrdersReturn();
            despatchOrders = new OutstandingOrdersReturn();
            ordersInProgress = new OutstandingOrdersReturn();
            heldOrders = new OutstandingOrdersReturn();
        }
    }

    public class TradeOrderImportViewModel
    {
        public TradeAccountUsersModel TradeAccountModel { get; set; }
        public BaseViewModel BaseViewModel { get; set; }
        public string heading { get; set; }
        public bool permission { get; set; }
        public bool distributor { get; set; }

        public TradeOrderImportViewModel(TradeAccountUsersModel tam, BaseViewModel bvm, string h, bool po, bool d)
        {
            TradeAccountModel = tam;
            BaseViewModel = bvm;
            heading = h;
            permission = po;
            distributor = d;

        }
    }

    public class TradeAccountContactDetailsViewModel
    {
        public TradeAccountUsersModel tradeAccountModel { get; set; }
        public BaseViewModel BaseDefinition { get; set; }
        public string heading { get; set; }
        public bool placeOrderPermission { get; set; }
        public bool distributor { get; set; }
        public tradeAccountEmailsModel emails { get; set; }
        public bool tradePermissionAccountAdmin { get; set; }
        public MertexInvoiceAddress invoiceAddress { get; set; }
        public string defaultDeliveryReference { get; set; }

        public TradeAccountContactDetailsViewModel(MertexInvoiceAddress iv, tradeAccountEmailsModel e, TradeAccountUsersModel taum, BaseViewModel bd, string h, bool p, string ddr)
        {
            invoiceAddress = iv;
            emails = e;
            tradeAccountModel = taum;
            BaseDefinition = bd;
            heading = h;
            tradePermissionAccountAdmin = p;
            defaultDeliveryReference = ddr;
        }
    }

    public class TradeAccountAddressCleanse
    {
        public long TradeAccountAddressCleanseID { get; set; }
        public string TradeAccountCode { get; set; }
        public string DeliveryReference { get; set; }
        public string SourceAddressHash { get; set; }
        public string SourceAddress { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDT { get; set; }
        public DateTime? LookupDT { get; set; }
        public DateTime? DeletedDT { get; set; }
    }

    public class TradeCountry
    {
        public short CountryID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string LocalName { get; set; }
        public byte? RegionID { get; set; }
        public byte? ContinentID { get; set; }
        public byte? PriceListID { get; set; }
        public bool IsCountry { get; set; } 
        public short SortOrder { get; set; }
        public DateTime? DeletedDT { get; set; }
    }

    public class TradeAccountReserveOrdersModel
    {
        public BaseViewModel BaseViewModel { get; set; }
        public string Heading { get; set; }
        public bool Permission { get; set; }
        public TradeAccountUsersModel TradeAccountModel { get; set; }
        public OutstandingOrdersReturn ReserveOrders { get; set; }
        public MertexInvoiceAddress InvoiceAddress { get; set; }
        public string DefaultDeliveryReference { get; set; }
        public MemberAddress DefaultMemberAddress { get; set; }

        public TradeAccountReserveOrdersModel(
            BaseViewModel bvm,
            string h,
            bool p,
            MertexInvoiceAddress mia,
            string ddr,
            MemberAddress dma,
            TradeAccountUsersModel tam)
        {
            BaseViewModel = bvm;
            Heading = h;
            Permission = p;
            TradeAccountModel = tam;
            InvoiceAddress = mia;
            DefaultDeliveryReference = ddr;
            DefaultMemberAddress = dma;
            ReserveOrders = new OutstandingOrdersReturn();
        }
    }
}