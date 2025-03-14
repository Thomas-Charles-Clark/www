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
using wwwDesignersGuild.Models.Form;
using System.Threading;
using Braintree.Exceptions;

namespace wwwDesignersGuild.ViewModels
{
    public class FixedPageViewModel_Checkout : FixedPageViewModel<object>
    {
        public long CheckoutID { get; set; }
        public dgCheckoutType checkoutType { get; set; }
        public string amazonClientId { get; set; }
        public string amazonMerchantId { get; set; }
        public string amazonClientUrl { get; set; }
        public passCheckoutAddress checkoutAddressModel { get; set; }
        public passCheckoutPayment checkoutPaymentModel { get; set; }
        public passCheckoutTotals checkoutTotalModel { get; set; }
        public promotionCode promoCode { get; set; }
        public List<gtagProduct> products { get; set; }
        public string email { get; set; }

        public bool RedirectToBasket { get; set; }

        public NewMasterDefinition masterDefinition
        {
            get
            {
                return MasterDefinition;
            }
        }

        private FixedPageViewModel_Checkout(
            BaseViewModel objBase,
            long checkoutId)
            : base(objBase)
        {
            CheckoutID = checkoutId;
            RedirectToBasket = false;
        }

        public static async Task<FixedPageViewModel_Checkout> CreateAsync(
            BaseViewModel objBase,
            long checkoutId)
        {
            var result = new FixedPageViewModel_Checkout(
                objBase, 
                checkoutId);

            await result.Initialise();

            return result;
        }

        new protected async Task Initialise()
        {
#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            var objCheckout = new Models.Checkout.checkoutDb();

            var objFormRoutines = new formRoutines();

            var visitorFactory = new VisitorFactory();

            var objCountry = new cmsCountry();

            var objMemberAddressSelect = new MemberAddressSelect();

            var tradeAccountNotification = new tradeAccountNotification();

            var checkoutType = await objCheckout.GetCheckoutType(this.CheckoutID);

            Int64 visitorDbId = -1;

            int? memberId = null;

            Int32? tradeAccountId = null;

            string tradeAccountNumber = null;

            var tasks = new List<Task>();

            if (HttpContext.Current.Session["memberTradeAccountNumber"] != null)
            {
                tradeAccountNumber = HttpContext.Current.Session["memberTradeAccountNumber"].ToString();
            }

            if (HttpContext.Current.Session["visitorIdFromDatabase"] != null)
            {
                visitorDbId = Convert.ToInt64(HttpContext.Current.Session["visitorIdFromDatabase"].ToString());
            }

            if (HttpContext.Current.Session["guestCheckoutMemberId"] != null)
            {
                memberId = null;
            }
            else
            {
                if (HttpContext.Current.Session["memberIdFromDatabase"] != null)
                {
                    memberId = Convert.ToInt32(HttpContext.Current.Session["memberIdFromDatabase"].ToString());
                }
            }

            if (HttpContext.Current.Session["guestCheckoutMemberId"] != null)
            {
                // memberDetails must be pulled with a memberId!
                // because of the need to set null to the pub.Checkout_Address_Generate earlier
                memberId = (int?)HttpContext.Current.Session["guestCheckoutMemberId"];
            }

            if (HttpContext.Current.Session["memberTradeAccountID"] != null)
            {
                tradeAccountId = Convert.ToInt32(HttpContext.Current.Session["memberTradeAccountID"].ToString());
            }

            if (checkoutType == dgCheckoutType.Trade)
            {
                var basketPreflightResult = await objCheckout.validateBasket(visitorDbId,
                    memberId,
                    tradeAccountId,
                    LanguageID,
                    CountryID,
                    Date,
                    HttpContext.Current.Session["memberDisplayTradePrice"] != null && Convert.ToBoolean(HttpContext.Current.Session["memberDisplayTradePrice"]));

                if (basketPreflightResult.JSON != "{}")
                {
                    RedirectToBasket = true;
                    return;
                }
            }

            //  Get the delivery options
            var deliveryOptionsTask = objCheckout.checkoutAddressGenerate(
                this.CheckoutID,
                CountryID,
                LanguageID,
                visitorDbId,
                memberId,
                tradeAccountId);

            var countryTask = objCountry.countrySelectList(CountryID, LanguageID);

            var addressTask = visitorFactory.getMemberAddresses(visitorDbId, null, LanguageID, CountryID, Date);

            var merchantDetailTask = objCheckout.getMerchantDetails(CheckoutID);

            var payGenerateTask = objCheckout.checkoutPayGenerate(CheckoutID, CountryID, LanguageID, "SectionController._checkoutSinglePage");

            var memberStoreTask = visitorFactory.memberDetails(visitorDbId, Date, memberId);

            var cardTokensTask = objCheckout.getPaymentTokens(CheckoutID);

            var giftVouchersTask = objCheckout.getGiftVouchersInBasket(CheckoutID);

            var checkoutProductsTask = objCheckout.GetCheckoutProducts(CheckoutID);

            tasks.Add(deliveryOptionsTask);

            tasks.Add(countryTask);

            tasks.Add(addressTask);

            tasks.Add(merchantDetailTask);

            tasks.Add(payGenerateTask);

            tasks.Add(memberStoreTask);

            tasks.Add(cardTokensTask);

            tasks.Add(giftVouchersTask);

            tasks.Add(checkoutProductsTask);

            await Task.WhenAll(tasks);



            var objDeliveryOptions = await deliveryOptionsTask;

            HttpContext.Current.Session["checkoutDisplayGiftTab"] = objDeliveryOptions.createSnapshotBaskets[0].CheckoutDisplayGiftScreen;
            HttpContext.Current.Session["checkoutDisplayGiftVoucherTab"] = objDeliveryOptions.createSnapshotBaskets[0].CheckoutDisplayGiftVoucherScreen;



            //  Get the country information (within the current price list)
            var objCountryData = await countryTask;



            if (checkoutType == dgCheckoutType.Trade || checkoutType == dgCheckoutType.Member)
            {
                objMemberAddressSelect = await addressTask;
            }
            else
            {
                objMemberAddressSelect.Addresses = new List<MemberAddress>();
                objMemberAddressSelect.TradeAddresses = new List<MemberTradeAddress>();
            }


            var merchantDetailSelect = await merchantDetailTask;

            var merchantDetails = merchantDetailSelect.checkoutParameters;



            var passCheckoutPay = new passCheckoutPayment();

            var objCheckoutPayments = await payGenerateTask;

            decimal grandTotal = 0.0M;

            if (objCheckoutPayments.paymentTotals.Count == 0) { }
            else
            {
                grandTotal = (decimal)objCheckoutPayments.paymentTotals[0].TotalBalance;
            }

            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(MasterDefinition.UrlBreakdown.countryDetails.DotNetCulture);

            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            passCheckoutPay.checkoutMethod = checkoutType;


            var memberStore = await memberStoreTask;

            // depends on memberStoreTask, can't be run in parallel
            var marketingPermissions = new MarketingPermissionResult();

            if (memberStore.MemberDetail != null)
            {
                marketingPermissions = await objFormRoutines.marketingPermissionCheck(new webapiMarketingPermissionCheck()
                {
                    VisitorID = visitorDbId,
                    MemberID = memberId,
                    AddressRequired = true,
                    CountryID = CountryID,
                    LanguageID = LanguageID,
                    Email = memberStore.MemberDetail.Email,
                    SourceID = 25,
                    MarketingCountryID = memberStore.MemberDetail.MarketingCountryID,
                    ThirdPartyID = null
                });
            }
            
            passCheckoutPay.masterDefinition = MasterDefinition;
            passCheckoutPay.checkoutPayments = objCheckoutPayments;
            passCheckoutPay.marketingPermissions = marketingPermissions;
            passCheckoutPay.isMemberCheckout = passCheckoutPay.checkoutMethod == dgCheckoutType.Member;

            var savedCards = new List<CheckoutBraintreeCreditCardDetails>();
            if (passCheckoutPay.checkoutMethod == dgCheckoutType.Guest || 
                passCheckoutPay.checkoutMethod == dgCheckoutType.Member || 
                passCheckoutPay.checkoutMethod == dgCheckoutType.Paypal ||
                passCheckoutPay.checkoutMethod == dgCheckoutType.Apple)
            {
                var gateway = new BraintreeGateway()
                {
                    Environment = Braintree.Environment.ParseEnvironment(objCheckoutPayments.checkoutDetails[0].Environment),
                    MerchantId = objCheckoutPayments.checkoutDetails[0].MerchantID,
                    PublicKey = objCheckoutPayments.checkoutDetails[0].PublicKey,
                    PrivateKey = objCheckoutPayments.checkoutDetails[0].PrivateKey
                };

                var tokenRequest =
                    new ClientTokenRequest()
                    {
                        MerchantAccountId = objCheckoutPayments.checkoutDetails[0].MerchantAccountID
                    };

                if (passCheckoutPay.checkoutMethod == dgCheckoutType.Member)
                {
                    string searchCustomerId = memberId.ToString();
                    string customerId;
                    try
                    {
                        Customer customer = gateway.Customer.Find(searchCustomerId);
                        customerId = customer.Id;
                    }
                    catch (Braintree.Exceptions.NotFoundException)
                    {
                        // if Customer.FindAsync fails to find a customer it throws an exception, we need to create a new Braintree customer to be able to save any card tokens against
                        CustomerRequest newCustomer = new CustomerRequest()
                        {
                            Id = searchCustomerId,
                            Email = memberStore.MemberDetail.Email,
                            FirstName = memberStore.MemberDetail.FullName
                        };
                        var customer = gateway.Customer.Create(newCustomer);
                        if (customer.IsSuccess())
                            customerId = customer.Target.Id;
                        else
                            customerId = null;
                    }
                    catch (Exception)
                    {
                        customerId = null;
                    }
                    tokenRequest.CustomerId = customerId;
                }

                passCheckoutPay.braintreeClient = gateway.ClientToken.Generate(
                    tokenRequest
                );

                // load any cards in the Braintree vault for this customer
                if (passCheckoutPay.checkoutMethod == dgCheckoutType.Member)
                {
                    try
                    {
                        Customer cust = gateway.Customer.Find(memberId.ToString());

                        foreach (CreditCard card in cust.CreditCards)
                        {
                            var newCard = new CheckoutBraintreeCreditCardDetails()
                            {
                                cardType = card.CardType.ToString(),
                                endDigits = card.LastFour,
                                expiry = card.ExpirationDate,
                                defaultCard = card.IsDefault ?? false,
                                vaultToken = card.Token
                            };

                            if (objCheckoutPayments.checkoutDetails[0].Mandate3DSecure)
                            {
                                newCard.token = card.Token;
                            }
                            else
                            {
                                var token = gateway.PaymentMethodNonce.Create(card.Token);

                                if (token.IsSuccess())
                                    newCard.token = token.Target.Nonce;
                                else
                                    continue;
                            }

                            /* the token that is stored in the vault cannot have 3d secure applied to it by default, to be able to do this
                                we need to use it to create a new payment nonce. */
                            /*var token = gateway.PaymentMethodNonce.Create(card.Token);

                            if (token.IsSuccess())
                                newCard.token = token.Target.Nonce;
                            else
                                continue;*/

                            bool expired = true;
                            if (!string.IsNullOrEmpty(newCard.expiry))
                            {
                                var dateparts = newCard.expiry.Split('/');
                                if (dateparts.Length == 2)
                                {
                                    var month = Convert.ToInt32(dateparts[0]);
                                    var year = Convert.ToInt32(dateparts[1]);

                                    expired = (year < DateTime.Now.Year || (year == DateTime.Now.Year && month < DateTime.Now.Month));
                                }
                            }
                            newCard.isExpired = expired;
                            if (newCard.isExpired)
                                newCard.defaultCard = false;

                            savedCards.Add(newCard);
                        }
                    }
                    catch (NotFoundException) { }
                    catch (Exception ex)
                    {
                        var message = string.Format("CheckoutID: {0} - {1} ", CheckoutID.ToString(), "Unable to load cards from Braintree vault - " + ex.Message);
                    }
                }

                // add any card tokens saved against this basket
                var cardTokens = await cardTokensTask;

                var cardsToAppend = cardTokens.Select(t => new CheckoutBraintreeCreditCardDetails()
                {
                    cardType = t.CardType,
                    endDigits = t.LastFourDigits,
                    expiry = null,
                    token = t.Token,
                    bin = t.Bin,
                    vaultToken = string.Empty,
                    defaultCard = false
                });

                savedCards.AddRange(cardsToAppend);

                // default to the first card if a default isn't set
                if (savedCards.Any() && !savedCards.Where(c => c.defaultCard == true).Any())
                {
                    var defaultCard = savedCards.Where(c => c.isExpired == false).FirstOrDefault();
                    if (defaultCard != null)
                        defaultCard.defaultCard = true;
                }
            }

            passCheckoutPay.savedCards = savedCards;


            var checkoutTotals = new passCheckoutTotals();

            checkoutTotals.checkoutSummarys = new checkoutTotal
            {
                checkoutTotals = passCheckoutPay.checkoutPayments.paymentTotals,
                checkoutGiftVouchers = await giftVouchersTask
            };

            checkoutTotals.checkoutType = checkoutType;

            checkoutTotals.masterDefinition = MasterDefinition;

            var productResults = await checkoutProductsTask;

            this.checkoutType = checkoutType;

            this.checkoutAddressModel = new passCheckoutAddress()
            {
                masterDefinition = MasterDefinition,
                checkoutMethod = checkoutType,
                countryItems = objCountryData.countries,
                countyItems = objCountryData.counties,
                deliveryOptions = objDeliveryOptions,
                memberAddressSelect = objMemberAddressSelect,
                tradeAccountMessage = new tradeAccountNotification()
            };

            this.amazonClientId = merchantDetails.AmazonClientId;
            this.amazonMerchantId = merchantDetails.AmazonMerchantId;
            this.amazonClientUrl = merchantDetails.AmazonPayAPIURL;
            this.checkoutPaymentModel = passCheckoutPay;
            this.checkoutTotalModel = checkoutTotals;
            this.promoCode = productResults.promoCodes.FirstOrDefault();
            this.products = productResults.products;
            this.email = email;

#if DEBUG
            sw.Stop();
            System.Diagnostics.Debug.WriteLine($"FixedPageViewModel_Checkout initialised in {sw.Elapsed}");
#endif
        }
    }
}