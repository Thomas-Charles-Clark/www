using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Attribute;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Checkout;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Search;
using wwwDesignersGuild.Models.Trade;
using wwwDesignersGuild.Util;
using Braintree;
using wwwDesignersGuild.Models.Visitor;

namespace wwwDesignersGuild.ViewModels
{
    public class FixedPageViewModel_CheckoutLogin : FixedPageViewModel_Login
    {
        public long CheckoutID { get; set; }

        public decimal SubTotal { get; set; }

        public List<checkoutType> CheckoutTypes { get; set; }
        public string BraintreeClientKey { get; set; }
        public string BraintreeEnvironment { get; set; }
        public string Currency { get; set; }
        public decimal ProvisionalTotal { get; set; }
        public string AmazonLoginClientID { get; set; }
        public string AmazonClientID { get; set; }
        public string AmazonMerchantID { get; set; }
        public string AmazonClientURL { get; set; }
        public List<string> CheckoutCountries { get; set; }
        public bool AcceptVisa { get; set; }
        public bool AcceptMasterCard { get; set; }
        public bool AcceptAmex { get; set; }
        public bool AcceptDiscover { get; set; }
        public bool AcceptMaestro { get; set; }
        public bool AcceptMaestroUK { get; set; }
        public bool OnlyShowTrade { get; set; }
        public bool TradePermissionPlaceOrder { get; set; }
        public TradeAccountUsersModel TradeAccountModel { get; set; }

        public bool PermitPayPal
        {
            get
            {
                return SubTotal > 0 && CheckoutTypes.Any(p => p.CheckoutTypeId == (int)dgCheckoutType.Paypal);
            }
        }

        public bool PermitAmazon
        {
            get
            {
                return SubTotal > 0 && CheckoutTypes.Any(p => p.CheckoutTypeId == (int)dgCheckoutType.Amazon);
            }
        }

        public bool PermitApple
        {
            get
            {
                return SubTotal > 0 && CheckoutTypes.Any(p => p.CheckoutTypeId == (int)dgCheckoutType.Apple);
            }
        }

        public bool PermitGuest
        {
            get
            {
                return CheckoutTypes.Any(p => p.CheckoutTypeId == (int)dgCheckoutType.Guest);
            }
        }

        public bool PermitMember
        {
            get
            {
                return CheckoutTypes.Any(p => p.CheckoutTypeId == (int)dgCheckoutType.Member);
            }
        }

        public bool PermitTrade
        {
            get
            {
                return CheckoutTypes.Any(p => p.CheckoutTypeId == (int)dgCheckoutType.Trade);
            }
        }

        private FixedPageViewModel_CheckoutLogin(
            BaseViewModel objBase,
            long checkoutId,
            decimal subTotal)
            : base(objBase)
        {
            CheckoutID = checkoutId;
            SubTotal = subTotal;
            BeginRegistration = false;
        }

        public static async Task<FixedPageViewModel_CheckoutLogin> CreateAsync(BaseViewModel objBase, long checkoutId, decimal subTotal)
        {
            var result = new FixedPageViewModel_CheckoutLogin(objBase, checkoutId, subTotal);

            await result.Initialise();

            return result;
        }

        new protected async Task Initialise()
        {
#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            await base.Initialise();

            var objCheckout = new Models.Checkout.checkoutDb();

            var objCountry = new cmsCountry();

            // HttpRequestWrapper - for pulling country / date overrides
            var objRequestWrapper = new HttpRequestWrapper(HttpContext.Current.Request);

            // Need to know whether or not we're going to limit the display to Trade only.
            // This must be done if:
            // They're a soft or hard signed in trade member
            // We permit trade ordering on the given site
            var onlyShowTrade = false;

            if (BaseViewModel.MasterDefinition.LoginStatus == dgLoginStatus.currentLogin ||
                BaseViewModel.MasterDefinition.LoginStatus == dgLoginStatus.previousLogin)
            {
                // great, we're either soft or hard signed in.
                // are we a trade user?
                if (BaseViewModel.MasterDefinition.TradeAllowed)
                {
                    // Yes. We must only be shown the Trade checkout option.
                    onlyShowTrade = true;
                }
            }

            var checkoutTypesTask = objCheckout.GetCheckoutTypes(
                BaseViewModel.CountryID,
                BaseViewModel.LanguageID,
                BaseViewModel.Date,
                dgFramework.IsMobileDevice());

            var merchantDetailsTask = objCheckout.getMerchantDetails(CheckoutID);

            var availableCountriesTask = objCountry.countrySelectList(BaseViewModel.CountryID, BaseViewModel.LanguageID);

            await Task.WhenAll(checkoutTypesTask, merchantDetailsTask, availableCountriesTask);

            var merchantDetailsSelect = await merchantDetailsTask;

            var availableCountries = await availableCountriesTask;

            CheckoutTypes = await checkoutTypesTask;

            CheckoutCountries = availableCountries.countries.Select(c => c.Name).ToList();

            var checkoutParams = merchantDetailsSelect.checkoutParameters;

            Currency = checkoutParams.Currency;

            if (CheckoutTypes.Any(c => c.CheckoutTypeId == Convert.ToByte(dgCheckoutType.Paypal)))
            {
                BraintreeEnvironment = checkoutParams.Environment;
                ProvisionalTotal = 0.01M;

                var gateway = new BraintreeGateway()
                {
                    Environment = Braintree.Environment.ParseEnvironment(checkoutParams.Environment),
                    MerchantId = checkoutParams.MerchantID,
                    PublicKey = checkoutParams.PublicKey,
                    PrivateKey = checkoutParams.PrivateKey
                };

                BraintreeClientKey = gateway.ClientToken.Generate(
                    new ClientTokenRequest()
                    {
                        MerchantAccountId = checkoutParams.MerchantAccountID
                    }
                );
            }

            AmazonClientID = checkoutParams.AmazonClientId;
            AmazonMerchantID = checkoutParams.AmazonMerchantId;
            AmazonClientURL = checkoutParams.AmazonPayAPIURL;
            AcceptAmex = checkoutParams.AcceptAmex;
            AcceptDiscover = checkoutParams.AcceptDiscover;
            AcceptMaestro = checkoutParams.AcceptMaestro;
            AcceptMaestroUK = checkoutParams.AcceptMaestroUK;
            AcceptMasterCard = checkoutParams.AcceptMasterCard;
            AcceptVisa = checkoutParams.AcceptVisa;
            OnlyShowTrade = onlyShowTrade;
            TradePermissionPlaceOrder = false;

            if (onlyShowTrade)
            {
                var vf = new VisitorFactory();

                var tradeAccountModel = await vf.callTradeMemberSelect(
                        BaseViewModel.Visitor.memberTradeAccountID.Value,
                        BaseViewModel.Visitor.memberId.Value,
                        BaseViewModel.CountryID,
                        BaseViewModel.LanguageID);

                if (BaseViewModel.MasterDefinition.LoginStatus != dgLoginStatus.previousLogin)
                {
                    TradeAccountModel = tradeAccountModel;

                    var user = tradeAccountModel.Users.Where(p => p.MemberID == BaseViewModel.Visitor.memberId).SingleOrDefault();
                    var admin = tradeAccountModel.Administrators.Where(p => p.MemberID == BaseViewModel.Visitor.memberId).SingleOrDefault();

                    TradePermissionPlaceOrder = user?.TradePermissionPlaceOrder ?? admin?.TradePermissionPlaceOrder ?? false;
                }
            }

            Redirect = dgFramework.buildUrl(dgContentType.landingPage, (int)dgFixedPage.checkoutSinglePage, MasterDefinition.UrlBreakdown.countryDetails);

#if DEBUG
            sw.Stop();
            System.Diagnostics.Debug.WriteLine($"FixedPageViewModel_Login initialised in {sw.Elapsed}");
#endif
        }
    }
}