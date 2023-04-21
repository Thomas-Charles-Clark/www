var LOG_ACTION_PAY_NOW_CLICK = 136,
    LOG_ACTION_PAY_NOW_STEP = 138,
    LOG_ACTION_PAY_NOW_VALIDATION_ERROR = 139

const stockInfo = (function (parent) {
    var self = {};

    self.parent = parent;

    self.resources = self.parent.resources;

    self.busy = ko.observable(false);

    self.product = ko.observable();
    self.totalStock = ko.observable();
    self.totalPurchaseOrderStock = ko.observable();
    self.unit = ko.observable();
    self.stockStatus = ko.observable();
    self.inStockData = ko.observable();
    self.dueInData = ko.observable();
    self.supplierData = ko.observable();
    self.supplierStockAvailabilityMessage = ko.observable();

    self.totalInStock = ko.pureComputed(function () {
        if (self.inStockData()) {
            return Number(Math.round((Enumerable.From(self.inStockData()).SelectMany(function (x) {
                return x.Pieces
            }).Select(function (x) {
                return +x.Length
            }).Sum()) * 10) / 10).toFixed(2) + self.unit()
        } else {
            return 0
        }
    }, self);

    self.totalDueIn = ko.pureComputed(function () {
        if (self.dueInData()) {
            return Number(Math.round((Enumerable.From(self.dueInData()).Select(function (x) {
                return +x.Length
            }).Sum()) * 10) / 10).toFixed(2) + self.unit()
        } else {
            return 0
        }
    }, self);

    self.totalSupplier = ko.pureComputed(function () {
        if (self.supplierData()) {
            return Number(Math.round((Enumerable.From(self.supplierData()).SelectMany(function (x) {
                return x.Pieces
            }).Select(function (x) {
                return +x.Length
            }).Sum()) * 10) / 10).toFixed(2) + self.unit()
        } else {
            return 0
        }
    }, self);

    self.supplierHasStock = ko.pureComputed(function () {
        return self.totalSupplier() != '0.00m' && self.totalSupplier() != 0
    }, self);

    self.sufficientSinglePieceAvailable = ko.pureComputed(function () {
        return Enumerable.From(self.inStockData()).SelectMany(function (x) {
            return x.Pieces
        }).Where(function (x) {
            return +x.Length <= +self.Quantity
        }).SingleOrDefault();
    }, self);

    self.showStockInfo = function (product, tab) {

        tab = typeof tab === 'undefined' ? 0 : tab;

        self.busy(true);

        self.product(product);

        if (product.hasOwnProperty('stockMessage')) {
            self.unit(product.stockMessage.unit);
            self.stockStatus(product.statusMessage);
        } else {
            self.unit('');
            self.stockStatus('');
        }

        //$('#stock-modal').off('show.bs.modal');

        //$('#stock-modal').on('show.bs.modal', function () {
        //$('#stock-tab-menu').tabs({ active: +tab });
        //});

        $("#stock-modal").modal('show');

        self.busy(true);

        if (self.parent.hasOwnProperty('deliveryAddressFactory')) {
            if (self.product().PermitCutLengthBreakdown || self.product().PermitBatch) {
                self.parent.deliveryAddressFactory.actingCutLength(self.product());
            } else {
                self.parent.deliveryAddressFactory.actingCutLength(undefined);
            }
        }

        self.supplierStockAvailabilityMessage(undefined);

        $.ajax({
            url: '/api/stockist/GetPieceLevelStockInfo',
            type: 'POST',
            dataType: 'json',
            data: {
                productId: ko.unwrap(self.product().productId || self.product().ProductID),
                countryId: getCountryId(),
                languageId: getLanguageId()
            },
            success: function (data) {

                //console.log('getpiecelevelstockinfo returned', data);

                if (data != null) {

                    self.totalStock(data.TotalStock);

                    ko.utils.arrayForEach(data.Batches, function (b) {
                        b.totalStock = Number(Math.round(Enumerable.From(b.Pieces).Select(function (p) {
                            return Math.round((Number(p.Length) * Number(p.Quantity)) * 10) / 10;
                        }).Sum() * 10) / 10).toFixed(2);
                    })

                    self.inStockData(data.Batches);

                    self.supplierData(data.SupplierStock);

                    self.dueInData(data.PurchaseOrders);

                    self.totalPurchaseOrderStock(data.TotalPurchaseOrderLength);

                    self.supplierStockAvailabilityMessage(data.SupplierAvailabilityMessage);

                    if (tab == 2 && !self.supplierHasStock()) {
                        let el = document.querySelector('#stock-tab-menu li:first-child button');
                        let bsTab = bootstrap.Tab.getOrCreateInstance(el);
                        bsTab.show();
                        //$('#stock-tab-menu').tabs({ active: 0 });
                    } else {
                        let el = document.querySelector('#stock-tab-menu li:nth-child(' + (tab + 1) + ') button');
                        let bsTab = bootstrap.Tab.getOrCreateInstance(el);
                        bsTab.show();
                        //$('#stock-tab-menu').tabs({ active: +tab });
                    }

                    //$('#stock-tab-menu').tabs("refresh");
                    //$('#stock-modal').modal('handleUpdate');
                }

                self.busy(false);

                if (self.parent.hasOwnProperty('deliveryAddressFactory')) {
                    if (self.product().hasOwnProperty('cutLengthBreakdown')) {
                        self.product().cutLengthBreakdown.selectedBatch(undefined);
                        self.product().cutLengthBreakdown.availableBatches(Enumerable.From(self.inStockData()).Select(function (x) {
                            return {
                                batch: x.BatchNumber,
                                text: x.BatchNumber
                            }
                        }).ToArray());
                    }
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                self.busy(false);
                $('#stock-container').append('<p>' + self.resources.StockDetailNotAvailable + '</p>')
            }
        })
    }

    return self;
});

const checkoutSinglePageViewModel = ((resources) => {
    //console.log('checkoutSinglePage instantiated');
    let self = {
        resources: resources
    };

    // #region properties

    // Descriptor comment for each property to follow pattern:
    // What (is it)
    // Where (is it used)
    // Why (does it exist)

    // Url to path towards h221 ("full" can include UPS AP query string values)
    // Provided to ajax method pulling checkoutAddress partial
    // Because the ajax request needs to know what to get, and potentially what UPS AP values to provide
    self.addressUrl = getUrlCheckoutAddressFull();

    // A numerical representation of the checkoutType.
    // Throughout
    // Because we need to know what to display and what behaviour to exhibit based on the checkoutType
    /*
        Member = 0,
        Guest = 1,
        Trade = 2,
        Amazon = 3,
        Paypal = 4
    */
    self.checkoutType = getCheckoutType();

    // #endregion


    // #region observables

    // orderReference
    self.tradeValidateNow = ko.observable(false);

    self.orderReference = ko.observable().extend({
        required: {
            params: true,
            onlyIf: function () {
                return (self.tradeValidateNow()) && (self.checkoutType == 2)
            },
            message: self.resources.OrderReferenceRequired
        },
        errorPulse: {}
    });

    self.tradeErrors = new ko.validation.group([
        self.orderReference
    ]);

    // The balance (as a number...) 
    // Mostly _checkoutPay, tiny bit in voucher payment modal?
    // Because we need something to pay
    self.balanceNumber = ko.observable(getBalanceNumber());

    // Prevent display of the Place Order button
    // hopefully near the Place Order button
    // because sometimes we don't want to display the Place Order button
    self.showPlaceOrder = ko.observable(false);

    // Hold selected delivery address (should always be of type checkoutDeliveryAddress)
    // Throughout
    // Because we need to know what address to deliver to, and this can change and must update the view when it does.
    self.deliveryAddress = ko.observable();

    // Hold selected payment method ({ name: 'name', nonce: 'token' })
    // When changing payment object and upon submission
    // Because we need a payment
    self.paymentMethod = ko.observable();

    // Maintain delivered / selected marketing permissions
    // Beneath 'Buy Now' button
    // Because we need to carefully track marketing permissions provided and replied
    var mps = [];
    getMarketingPermissions().Permissions.forEach(function (mp) {
        mps.push(new marketingPermission(mp));
    });
    self.marketingPermissions = ko.observableArray(mps);


    // general busy indicator
    self.busy = ko.observable(false);

    // is the delivery section busy?
    self.dispatchBusy = ko.observable(true);

    // Container to hold whether or not the "summary" (payment amount & buy now button) is 'busy'
    self.summaryBusy = ko.observable(false);

    // #endregion


    // #region subscriptions

    self.balanceNumber.subscribe(function (newValue) {
        //console.log('checkoutSinglePage balanceNumber subscribe', newValue);
    });

    self.deliveryAddress.subscribe(function (newValue) {
        //console.log('checkoutSinglePage deliveryAddress subscribe', newValue);
    });

    self.paymentMethod.subscribe(function (newValue) {
        //console.log('checkoutSinglePage paymentMethod subscribe', newValue);
    });

    // #endregion 


    // #region computeds

    self.isMemberCheckout = ko.computed(function () {
        if (self.checkoutType == 0 || self.checkoutType == 2)
            return true;
        else
            return false;
    }, self);

    // #endregion


    // #region components

    // Delivery address functionality wrapper
    // Throughout
    // Because we need a delivery address and a neat way to encapsulate all the related functionality
    self.deliveryAddressFactory = deliveryAddressFactory(self);

    // Payment method functionality wrapper
    // Throughout
    // Because we need a payment method and a neat way to encapsulate all the related functionality
    self.paymentMethodFactory = paymentMethodFactory(self);

    // Gift voucher functionality wrapper
    // Throughout
    // Because we need to permit the use of gift vouchers and a neat way to encapsulate all the related functionality
    self.giftVoucherFactory = giftVoucherFactory(self);


    // Gift card factory
    // Despatch
    // Because we sell gift cards
    self.giftCardFactory = giftCardFactory(self);


    // Gift box functionality wrapper
    // Throughout
    // Because... people like gift boxes?
    self.giftBoxFactory = giftBoxFactory(self);
    // #endregion

    // Stock Info Model
    self.stockInfoModel = stockInfo(self);

    // #region errors and validation

    self.errors = ko.validation.group([
        self.deliveryAddressFactory.addressConfirmed,
        self.deliveryAddressFactory.identificationAddressConfirmed,
        //self.paymentMethodFactory.paymentMethodSelected,
        self.paymentMethodFactory.amazonHappy,
        self.deliveryAddressFactory.dispatchOptions
    ], {
        //deep: true,
        live: true
    });

    self.closeError = function () {
        $("#payment-processing-error").modal('hide');
        $("#payment-processing-failure").modal('hide');
        $("#card-vaulting-failure").modal('hide');
    }

    // #endregion            



    // #region functions

    // init: this can be called directly or from some part of the DOM (or a KO virtual element) whenever we like.

    self.init = function () {
        $("#payment-processing-error").on('show.bs.modal', () => {
            $("#checkout-complete-waiting").modal('hide');
        });
        $("#payment-processing-error").on('hide.bs.modal', () => {
            $("#checkout-complete-waiting").modal('hide');
        });
        $("#payment-processing-error-no-details").on('hide.bs.modal', () => {
            $("#checkout-complete-waiting").modal('hide');
        });

        //console.log('checkoutSinglePage init: reset first');
        self.reset();
        //console.log('checkoutSinglePage init: initialising');

        self.deliveryAddressFactory.init();

        self.paymentMethodFactory.init();
    }


    self.loadBasketSummary = function () {
        //console.log('singlePageCheckout loadBasketSummary called');
        self.summaryBusy(true);
        $.ajax({
            url: getUrlCheckoutSummary(),
            type: "GET",
            dataType: "html",
            success: function (data) {
                //console.log('singlePageCheckout loadBasketSummary returned', data);
                var basketSummary = $('#checkout-total-wrapper');
                ko.cleanNode(basketSummary[0]);
                basketSummary.html(data);
                ko.applyBindings(viewModel, basketSummary[0]);
                self.summaryBusy(false);
                self.busy(false);
            },
            error: function (xhr, s, a) {
                //console.log('singlePageCheckout loadBasketSummary failed', xhr, s, a);
                self.summaryBusy(false);
            }
        });
    };

    self.loadDeliveryMethods = function (andSummary) {
        //console.log('singlePageCheckout loadDeliveryMethods called');
        self.dispatchBusy(true);

        $.ajax({
            url: getUrlCheckoutDeliveryOptions(),
            type: "GET",
            dataType: "html",
            success: function (data) {
                console.log('singlePageCheckout loadDeliveryMethods returned');

                var deliveryOptions = $('#despatch-items-wrapper');

                ko.cleanNode(deliveryOptions[0]);

                deliveryOptions.html(data);

                ko.applyBindings(viewModel, deliveryOptions[0]);

                if (andSummary) {
                    self.loadBasketSummary();
                }

                self.deliveryAddressFactory.dispatchOptionsAvailable(getDispatchAvailable());
                self.deliveryAddressFactory.dispatchOptionsFromServer(getDispatchOptions());

                window.setTimeout(() => {
                    self.dispatchBusy(false);
                }, 0);
            },
            error: function (xhr, s, a) {
                //console.log('singlePageCheckout loadDeliveryMethods failed', xhr, s, a);
                self.dispatchBusy(false);
            }
        });

    };


    self.loadGiftWrapOptions = function () {
        //console.log('loadGiftWrapOptions');
    }

    self.displayGiftWrapOptions = function () {
        $('#gift-wrap-modal').modal('show');
    }

    self.displayGiftCardOptions = function () {
        $('#gift-card-modal').modal('show');
    }

    self.hideGiftCardOptions = function () {
        $('#gift-card-modal').modal('hide');
    }

    self.giftWrapOptionsChanged = function () {
        self.loadBasketSummary();
    };

    self.vouchersUpdated = function () {
        //console.log('vouchersUpdated');
        self.loadBasketSummary();
    }

    self.paymentMethodSelected = function () {
        //console.log('paymentMethodSelected')
        self.showPlaceOrder(true);
    }

    self.complete = function () {
        self.checkoutLog(LOG_ACTION_PAY_NOW_CLICK, 'address fields: ' + JSON.stringify(ko.unwrap(self.deliveryAddressFactory.currentAddress().logLines)));
        if (self.errors().length) {
            var errorStr = self.errors().join(';');
            self.checkoutLog(LOG_ACTION_PAY_NOW_VALIDATION_ERROR, errorStr);
            self.paymentMethodFactory.showCheckoutErrors(self.errors());
        } else {
            self.paymentMethodFactory.complete();
        }
    }

    self.checkoutLog = function (type, detail) {
        $.ajax({
            url: '/api/checkout/asyncSaveCheckoutLog',
            type: 'POST',
            async: false,
            cache: false,
            data: {
                action: type,
                detail: detail || "no detail provided"
            },
            success: function (data) {
                if (data.hasOwnProperty('error') && typeof jumpToSessionExpired === 'function') {
                    if (data.message === "checkoutBasketExpired") {
                        jumpToSessionExpired();
                        return;
                    }
                }
            },
            error: function (xhr, s, a) {
                //console.log("braintree error logging failed");
            }
        });
    }

    self.reset = function () {
        //console.log('checkoutSinglePage reset');

        // initially this was only called for Guest checkout. Does it do any harm to call it regardless?
        // Where does .checkoutBtnWrapper exist, and where does its .loadingWrapper exist?
        $('.checkoutBtnWrapper').find(".loadingWrapper").hide();

        self.addressUrl = getUrlCheckoutAddressFull();

        self.deliveryAddressFactory.reset();

        self.paymentMethodFactory.reset();

        self.giftVoucherFactory.reset();
    }
    // #endregion

    return self;
});

const deliveryAddressFactory = ((parent) => {
    let self = {
        parent: parent,
        resources: parent.resources
    };

    // #region early functions

    self.createCheckoutDeliveryAddressFromMemberAddress = function (addr) {
        var result = checkoutDeliveryAddress(
            self,
            addr.CountryCode,
            addr.IsUPSIdAddress || false,
            addr.DeliveryOptionID,
            addr.FullName,
            addr.PhoneNumber,
            addr.MemberAddressID,
            addr.CompanyName,
            addr.AddressLine1,
            addr.AddressLine2,
            addr.Town,
            addr.County,
            addr.PostCode,
            addr.UPSAPID,
            null,
            addr.IsTradeAddress,
            addr.IsDefault);

        if (addr.UPSAPID) {
            self.isUPSDelivery(true);
        }

        return result;
    }

    self.createCheckoutDeliveryAddressFromServerAddress = function (addr) {
        var result = checkoutDeliveryAddress(
            self,
            addr.Code,
            addr.UPSAPID || false,
            addr.DeliveryOptionID,
            addr.FullName,
            addr.PhoneNumber,
            addr.MemberAddressID,
            addr.CompanyName,
            addr.AddressLine1,
            addr.AddressLine2,
            addr.Town,
            addr.County,
            addr.PostCode,
            addr.UPSAPID,
            null,
            addr.IsTradeAddress,
            addr.IsDefault);

        if (addr.UPSAPID) {
            self.isUPSDelivery(true);
        }

        return result;
    }

    self.createCheckoutDeliveryAddressFromPayPalAddress = function (addr) {
        var result = checkoutDeliveryAddress(
            self,
            addr.countryCode,
            false,
            1,
            addr.recipientName,
            null,
            null,
            null,
            addr.line1,
            addr.line2,
            addr.city,
            addr.state,
            addr.postalCode,
            null,
            null,
            false,
            false);

        return result;
    }

    self.createCheckoutDeliveryAddressFromAmazonAddress = function (addr) {
        if (addr.County == null && addr.Town == "London") {
            addr.County = "London"
        }
        var result = checkoutDeliveryAddress(
            self,
            addr.CountryCode,
            false,
            1,
            addr.FullName,
            addr.PhoneNumber,
            null,
            addr.CompanyName,
            addr.AddressLine1,
            addr.AddressLine2,
            addr.Town,
            addr.County,
            addr.PostCode,
            null,
            null,
            false,
            false);

        return result;
    }

    self.createCheckoutDeliveryAddressFromUPSAccessPoint = function (addr) {
        if (addr.County == null && addr.Town == "London") {
            addr.County = "London"
        }
        var result = checkoutDeliveryAddress(
            self,
            addr.addressKeyFormatField.countryCodeField,
            false,
            3,
            null,
            null,
            null,
            addr.addressKeyFormatField.consigneeNameField,
            addr.addressKeyFormatField.addressLineField,
            addr.addressKeyFormatField.addressLine2Field,
            addr.addressKeyFormatField.politicalDivision1Field ||
            addr.addressKeyFormatField.politicalDivision2Field,
            addr.addressKeyFormatField.politicalDivision3Field,
            addr.addressKeyFormatField.postcodePrimaryLowField + (addr.addressKeyFormatField.postcodeExtendedLowField ? ' ' + postcodeExtendedLowField : ''),
            addr.accessPointInformationField.publicAccessPointIDField,
            null,
            false,
            false);

        if (addr.accessPointInformationField.publicAccessPointIDField) {
            self.isUPSDelivery(true);
        }

        return result;
    }

    // #endregion

    // #region properties

    self.actingCutLength = ko.observable();

    self.actingLineMemo = ko.observable();

    self.actingLineTreatment = ko.observable();

    // #endregion


    // #region observables

    // this... must be set from server information when appropriate
    self.isUPSDelivery = ko.observable(false);

    // UPS Access Point selected identification address
    self.selectedIdentificationAddress = ko.observable();

    self.identificationAddressConfirmed = ko.observable(false).extend({
        equal: {
            params: true,
            onlyIf: function () {
                return self.isUPSDelivery()
            },
            message: self.resources.CheckoutErrorNoIdentificationAddress
        }
    });

    // whether or not the chosen address has been successfully confirmed by the server.
    // there is an addressConfirmed observable on checkoutDeliveryAddress: this is more to govern clicking and client side validation,
    // confirmation only TRULY occurs when the saveCheckoutAddress server method has replied with OK.
    self.addressConfirmed = ko.observable(false).extend({
        equal: {
            params: true,
            message: self.resources.CheckoutErrorNoAddress
        }
    });

    // container to hold whether or not this component is 'busy'
    // shows / hides blocker _addressMemberCompontent:14
    self.busy = ko.observable(false);

    // this should come from... checkoutType != trade && checkoutDeliveryItems.Where(DeliveryOptionID == 2).length > 0
    self.showCollectionOptions = getShowCollectionOptions();

    var addresses = getAllAddresses();
    var checkoutAddresses = [];

    for (var i = 0; i < addresses.length; i++) {
        var addressObj = self.createCheckoutDeliveryAddressFromMemberAddress(addresses[i]);
        checkoutAddresses.push(addressObj);
    }

    self.addressItems = ko.observableArray(checkoutAddresses);

    self.tradeAddressItems = ko.observableArray([]);

    self.currentAddress = ko.observable(checkoutDeliveryAddress(self, getMasterDefinition().UrlBreakdown.countryDetails.CountryCode, false, 1));

    self.upsIdAddress = ko.observable(checkoutDeliveryAddress(self, getMasterDefinition().UrlBreakdown.countryDetails.CountryCode, true));

    // Temporary container for holding checkoutAddress from server which needs sales tax verification selection performed
    self.holdingAddress = ko.observable();

    self.defaultMemberAddress = ko.observable(getDefaultMemberAddress()); // undefined; no default address

    self.checkoutAddressFromServer = ko.observable(getCurrentCheckoutAddress());

    self.salesTaxAddresses = ko.observableArray([]);

    self.salesTaxAddressFromList = ko.observable();

    // Whether or not any dispatch options have been returned
    self.dispatchOptionsAvailable = ko.observable(false);

    // The dispatch options delivered by the checkoutdispatchgenerate partial
    // ostensibly... each delivery address has a collection of dispatch options
    // the checkoutGroupedBasket links products to checkoutDeliveryAddress
    // via CheckoutAddressID
    // each product in checkoutGroupedBasket carries a DispatchID
    // we want to:
    // foreach checkoutDeliveryAddress
    // foreach DispatchID belonging to this delivery address
    // we loop through checkoutGroupedBasket items belonging to this address and to this dispatchId
    // render the products
    // we also fetch the dispatchDeliverySelection for this DispatchID (there should always be one..)
    // we enumerate the checkoutDispatchDeliveryOptions for this DispatchID
    // render them, marking as selected that which is selected by the dispatchDeliverySelection
    // display the estimated delivery dates for the selected!
    // now, the estimated delivery dates exist on the checkoutDispatchDeliveryOptions.
    // in the absence of moment.js, we should format as desired on the server before delivering...

    //var result = [];

    //for (var i = 0; i < fromServer.checkoutDeliveryAddresses.length; i++) {
    //    var addr = fromServer.checkoutDeliveryAddresses[i];
    //    var products = Enumerable.From(fromServer).Where(function (x) {
    //        return x.CheckoutAddressID === addr.CheckoutAddressID
    //    }).ToArray();
    //    //console.log('checkoutDispatch', dis)

    //    var dispatches = Enumerable.From(products).GroupBy('$.DispatchID',
    //        '', 'key,e=>e').Select().ToArray();
    //    //console.log('checkoutDispatch', dispatches);
    //}

    self.dispatchOptionsFromServer = ko.observable();

    self.dispatchOptions = ko.observableArray([]).extend({
        minLength: {
            params: 1,
            message: self.resources.CheckoutErrorMissingDispatchMethod
        }
    });

    // #endregion


    // #region subscriptions

    self.dispatchOptionsFromServer.subscribe(function (newValue) {

        //console.log('deliveryAddressFactory dispatchOptionsFromServer subscribe', newValue);

        if (newValue) {
            for (var a = 0; a < newValue.checkoutDeliveryAddresses.length; a++) {
                var addr = newValue.checkoutDeliveryAddresses[a];

                //console.log('dispatchOptionsFromServer address', addr);

                var products = Enumerable.From(newValue.checkoutDispatchGroupedBaskets).Where(function (x) {
                    return x.CheckoutAddressID === addr.CheckoutAddressID
                }).ToArray();
                //console.log('dispatchOptionsFromServer products', products)

                var dispatches = Enumerable.From(products).GroupBy('$.DispatchID',
                    '', 'key, e => { dispatchId: e.Key(), products: e.source }').Select().ToArray();

                // call .first on the products to get the control over whether or not to display the 
                // "How would you like your order despatched?"
                // - As soon as possible (Default)
                // - As few deliveries as possible

                for (var i = 0; i < dispatches.length; i++) {
                    for (var j = 0; j < dispatches[i].products.length; j++) {

                        if (dispatches[i].products[j].PermitCutLengthBreakdown || dispatches[i].products[j].PermitBatch)
                            dispatches[i].products[j].cutLengthBreakdown = cutLengthBreakdown({ record: dispatches[i].products[j], parent: self });

                        dispatches[i].products[j].lineMemo = lineMemo({ record: dispatches[i].products[j], parent: self });
                        dispatches[i].products[j].lineTreatment = lineTreatment({ record: dispatches[i].products[j], parent: self });
                        dispatches[i].products[j].giftCard = giftCard(dispatches[i].products[j], self);

                        var q = dispatches[i].products[j];

                        dispatches[i].products[j].name = dispatches[i].products[j].BasketName;
                        dispatches[i].products[j].productId = dispatches[i].products[j].ProductID;
                        dispatches[i].products[j].code = dispatches[i].products[j].Code;

                        dispatches[i].products[j].stockMessage = dispatches[i].products[j].stockMessageJson;

                        dispatches[i].products[j].stockMessagesDisplay = ko.pureComputed(function () {
                            var quantity = this.Quantity;
                            return Enumerable.From(this.stockMessage.basketQuantityThresholds).Where(function (bqt) {
                                return quantity >= bqt.minimum && quantity <= bqt.maximum
                            }).Select(function (bqt) {
                                return bqt.stockMessages
                            }).SingleOrDefault();
                        }, q);

                        dispatches[i].products[j].giftBoxed = ko.pureComputed(function () {
                            return (self.parent.giftBoxFactory.giftBoxedProducts().map(p => p.ProductID).indexOf(this.ProductID) > -1);
                        }, q);

                        var stockStatusLines = Enumerable.From(newValue.checkoutDispatchStockStatusLines).Where(function (x) {
                            return x.ProductID === dispatches[i].products[j].ProductID;
                        }).ToArray();

                        dispatches[i].products[j].StockStatuses = [];

                        for (var k = 0; k < stockStatusLines.length; k++) {

                            var stockStatusLine = Enumerable.From(newValue.checkoutDispatchStockStatuses).Where(function (x) {
                                return x.StockStatusID === stockStatusLines[k].StockStatusID;
                            }).Single();

                            var ssLine = $.extend({}, stockStatusLine);

                            var cssClass = 'shop-avail';
                            switch (ssLine.StockStatusID) {
                                case 1:
                                    cssClass += ' in';
                                    break;
                                case 0:
                                case 255:
                                    cssClass += ' out';
                                    break;
                                default:
                                    cssClass += ' soon';
                                    break;
                            }
                            ssLine.CssClass = cssClass;

                            var objStockStatus = Enumerable.From(stockStatusLines).Where(function (x) {
                                return x.StockStatusID == ssLine.StockStatusID;
                            }).Single();

                            if (objStockStatus.StockQty != null) {
                                ssLine.StockStatus = objStockStatus.StockQty + ' ' + ssLine.StockStatus;
                            }
                            dispatches[i].products[j].StockStatuses.push(ssLine);
                        };
                    }

                    var deliveryOptions = Enumerable.From(newValue.checkoutDispatchDeliveryOptions).Where(function (x) {
                        return x.DispatchID == dispatches[i].dispatchId;
                    }).ToArray();
                    dispatches[i].dispatchName = deliveryOptions[0].DispatchName;
                    dispatches[i].deliveryOptions = deliveryOptions;
                    var selected = Enumerable.From(newValue.checkoutDispatchDeliverySelections).Where(function (x) {
                        return x.DispatchID == dispatches[i].dispatchId;
                    }).Single();
                    dispatches[i].selected = ko.observable(Enumerable.From(dispatches[i].deliveryOptions).Where(function (x) {
                        return x.DispatchID == dispatches[i].dispatchId && x.DeliveryBandID == selected.DeliveryBandID;
                    }).Single());
                    dispatches[i].offerFullDispatch = selected.OfferFullDespatch;
                    dispatches[i].fullDispatchSelection = ko.observable(selected.CustomerRequestsFullDespatch);
                    dispatches[i].singleDeliverySelected = ko.observable(!selected.PartShip);

                }

                //console.log('dispatchOptionsFromServer dispatches', dispatches);

                self.dispatchOptions(dispatches);

                if (self.parent.paymentMethodFactory.amazonWalletWidget) {
                    self.parent.paymentMethodFactory.updateAmazonValue();
                }
            }
        } else {
            self.dispatchOptions([]);
        }
    });

    self.currentAddress.subscribe(function (newValue) {
        //console.log('deliveryAddressFactory currentAddress subscribe', newValue);
        if (newValue) {
            if (newValue.UPSAPID) {
                self.isUPSDelivery(true);
            }
        }
        var node = $('#checkout-delivery-address-form')[0];
        if (node) {
            ko.cleanNode(node);
            ko.applyBindings(self, node);
        }
    });

    self.addressConfirmed.subscribe(function (newValue) {
        //console.log('deliveryAddressFactory addressConfirmed', newValue);
        if (newValue) {
            self.parent.loadDeliveryMethods(true);
        } else {
            self.removeDeliveryAddress();
        }
    });

    self.identificationAddressConfirmed.subscribe(function (newValue) {
        //console.log('deliveryAddressFactory identificationAddressConfirmed', newValue);
        if (newValue) {
            self.parent.loadDeliveryMethods(true);
        } else {
            // nothing: definitely do not want to remove delivery address here
        }
    });

    self.defaultMemberAddress.subscribe(function (newValue) {
        //console.log('deliveryAddressFactory defaultMemberAddress subscribe', newValue);
    });

    self.checkoutAddressFromServer.subscribe(function (newValue) {
        //console.log('deliveryAddressFactory checkoutAddressFromServer subscribe', newValue);
    });

    // #endregion


    // #region computeds



    // #endregion


    // #region components



    // #endregion


    // #region functions

    self.showCutLengthBreakdown = function (product) {
        //console.log('showCutLengthBreakdown called', product);
        self.actingCutLength(product.cutLengthBreakdown);
        $('#cut-length-modal').modal('show');
    }

    self.showMemoModal = function (product) {
        //console.log('showMemoModal called', product);
        self.actingLineMemo(product.lineMemo);
        $('#line-memo-modal').modal('show');
    }

    self.showTreatmentsModal = function (product) {
        //console.log('showTreatmentsModal called', product);
        self.actingLineTreatment(product.lineTreatment);
        $('#line-treatments-modal').modal('show');
    }

    self.selectFromAddressBook = function (checkoutAddress) {
        //console.log('deliveryAddressFactory selectFromAddressBook', checkoutAddress);
        if (self.addressConfirmed() && self.isUPSDelivery()) {
            checkoutAddress.isUPSIdAddress = true;
            checkoutAddress.useThisAddressText = self.resources.UseThisIdentificationAddress;
            self.upsIdAddress(checkoutAddress);
            self.upsIdAddress().confirm();
            $("#address-book-modal").modal('hide');
        } else {
            self.currentAddress(checkoutAddress);
            self.currentAddress().confirm();
            $("#address-book-modal").modal('hide');
        }
    }

    self.deleteAddress = function (checkoutAddress) {
        window.showConfirm(self.resources.AreYouSure, self.resources.ManageAddressBookConfirmDelete,
            function () {
                //console.log('deliveryAddressFactory deleteAddress called', checkoutAddress);
                $.ajax({
                    url: "/api/member/deleteAddress",
                    type: "Post",
                    cache: false,
                    async: true,
                    data: JSON.stringify({
                        memberAddressId: checkoutAddress.memberAddressId(),
                        country: getSiteCountry()
                    }),
                    contentType: 'application/json; charset=utf-8',
                    success: function (data) {
                        //console.log('deliveryAddressFactory deleteAddress returned');
                        if (data.message === "checkoutBasketExpired") {
                            jumpToSessionExpired();
                            return;
                        }
                        var item = Enumerable.From(self.addressItems()).Where(function (x) {
                            return x.memberAddressId() == checkoutAddress.memberAddressId()
                        }).SingleOrDefault();
                        self.addressItems.remove(item);
                    },
                    error: function (xhr, textStatus, errorThrown) {
                        //console.log('dg-memberaddaddress: ' + textStatus);
                        alert(self.resources.MajorError);
                        setTimeout(function () {
                            $('#buttonBlockAddress-edit-delete').hide();
                        }, 500);
                    }
                });
            }, null);
    }

    self.saveDeliveryAddress = function (checkoutAddress) {
        self.busy(true);

        var upsAddress = null;
        var upsAPMemberAddressId = null;

        if (self.selectedIdentificationAddress()) {
            var idAddress = self.selectedIdentificationAddress();
            upsAddress = {
                DeliveryOptionID: idAddress.deliveryOptionId(),
                MemberAddressID: idAddress.memberAddressId,
                FullName: idAddress.fullName(),
                CompanyName: idAddress.companyName(),
                AddressLine1: idAddress.addressLine1(),
                AddressLine2: idAddress.addressLine2(),
                Town: idAddress.town(),
                County: idAddress.countyListOrText() ? idAddress.countyFromList() : idAddress.county(),
                Postcode: idAddress.postcode(),
                CountryCode: idAddress.country().Code,
                BasketCountry: getSiteCountry(),
                PhoneNumber: idAddress.phone(),
                UPSAPID: idAddress.UPSAPID,
                UPSAPMemberAddressId: upsAPMemberAddressId,
                IsTradeAddress: idAddress.isTradeAddress,
                masterDefinition: getMasterDefinition()
            };

            upsAPMemberAddressId = self.selectedIdentificationAddress().memberAddressId();
        }

        var addressData = {
            DeliveryOptionID: checkoutAddress.deliveryOptionId(),
            MemberAddressID: checkoutAddress.memberAddressId,
            FullName: checkoutAddress.fullName(),
            CompanyName: checkoutAddress.companyName(),
            AddressLine1: checkoutAddress.addressLine1(),
            AddressLine2: checkoutAddress.addressLine2(),
            Town: checkoutAddress.town(),
            County: checkoutAddress.countyListOrText() ? checkoutAddress.countyFromList() : checkoutAddress.county(),
            Postcode: checkoutAddress.postcode(),
            CountryCode: checkoutAddress.country().Code,
            BasketCountry: getSiteCountry(),
            PhoneNumber: checkoutAddress.phone(),
            UPSAPID: checkoutAddress.UPSAPID,
            UPSAPMemberAddressId: upsAPMemberAddressId,
            IsTradeAddress: checkoutAddress.isTradeAddress,
            IsClickCollect: checkoutAddress.isClickCollect || false,
            masterDefinition: getMasterDefinition()
        };

        if (addressData.FullName)
            addressData.FullName = addressData.FullName.substring(0, 50);

        if (addressData.CompanyName)
            addressData.CompanyName = addressData.CompanyName.substring(0, 35);

        if (addressData.AddressLine1)
            addressData.AddressLine1 = addressData.AddressLine1.substring(0, 60);

        if (addressData.AddressLine2)
            addressData.AddressLine2 = addressData.AddressLine2.substring(0, 60);

        if (addressData.Town)
            addressData.Town = addressData.Town.substring(0, 50)

        if (addressData.County)
            addressData.County = addressData.County.substring(0, 40)

        if (addressData.Postcode)
            addressData.Postcode = addressData.Postcode.substring(0, 20);

        if (upsAddress) {
            addressData.UPSAPAddress = upsAddress;
        }

        if (checkoutAddress.hasOwnProperty('USCounty')) {
            addressData.USCounty = checkoutAddress.USCounty;
        }

        addressData.CountryID = checkoutAddress.country()?.CountryID;

        //console.log('deliveryAddressFactory saveDeliveryAddress calling to server with', addressData);

        self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, checkoutAddress.displayLines());

        $.ajax({
            url: "/api/checkout/saveCheckoutAddress",
            type: "Post",
            cache: false,
            async: true,
            dataType: 'json',
            data: addressData,
            success: function (data) {
                self.busy(false);

                //console.log('deliveryAddressFactory saveDeliveryAddress returned', data);

                if (data.message === "checkoutBasketExpired") {
                    jumpToSessionExpired();
                    return;
                } else if (data.message === "invalidZipCode") {
                    $('#invalid-zip-modal').modal('show');
                    return;
                } else if (data.message === "zipTaxServiceFailed") {
                    $('#unavailable-zip-modal').modal('show');
                    return;
                } else {
                    if (data.hasOwnProperty('state')) {
                        if (data.state === 'OK') {
                            $("#payment-address-error").modal('hide');

                            // this is necessary because for example with PayPal, addressConfirmed is not set to false before it is oversaved.
                            // setting addressconfirmed false before saving is an option, but it would result in an unnecessary call to remove addresses
                            // in some instances.
                            if (self.addressConfirmed()) {
                                self.addressConfirmed.valueHasMutated();
                            } else {
                                self.addressConfirmed(true);

                                if (getCheckoutType() === 0 && self.addressItems().length === 0) {
                                    self.saveMemberAddress(addressData);
                                }

                            }

                            if (upsAddress) {
                                self.identificationAddressConfirmed(true);
                            }

                            checkoutAddress.phoneChanged(false);

                        } else if (data.state === 'selectSalesTaxAddress') {
                            self.holdingAddress(checkoutAddress);
                            self.salesTaxAddresses(data.salesTaxAddressList);
                            $("#sales-tax-confirm-modal").modal('show');
                        }
                    }
                }
                //singlepagecheckout.invalidaddressselected(false);
                //viewmodel.selectedaddress(checkoutaddress);
                //newaddressselected(checkoutaddress);
                //$('#buttonblockaddress-edit').hide();
                window.scrollTo(0, document.querySelector('#checkout-pay-region').offsetTop);
            },
            error: function (xhr, textStatus, errorThrown) {
                self.addressConfirmed(false);
                self.busy(false);
                //console.log('deliveryAddressFactory saveDeliveryAddress error: ', xhr, textStatus, errorThrown);
                alert(self.resources.MajorError);
                //    setTimeout(function () {
                //        $('#buttonBlockAddress-edit').hide();
                //    }, 500);
            }
        });
    }

    self.saveMemberAddress = function (checkoutAddress) {
        $.ajax({
            url: "/api/member/addAddress",
            type: "Post",
            cache: false,
            async: true,
            data: JSON.stringify({
                name: checkoutAddress.FullName || '',
                companyName: checkoutAddress.CompanyName || '',
                addressLine1: checkoutAddress.AddressLine1 || '',
                addressLine2: checkoutAddress.AddressLine2 || '',
                town: checkoutAddress.Town || '',
                county: checkoutAddress.County || '',
                postCode: checkoutAddress.Postcode || '',
                countryCode: checkoutAddress.CountryCode || '',
                phone: checkoutAddress.PhoneNumber || '',
                countryId: checkoutAddress.CountryID
            }),
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                var addressObj = self.createCheckoutDeliveryAddressFromMemberAddress(checkoutAddress);
                self.addressItems.push(addressObj);
            }
        });
    }

    self.saveIdentificationAddress = function (checkoutAddress) {
        self.selectedIdentificationAddress(checkoutAddress);
        self.saveDeliveryAddress(self.currentAddress());
    }

    self.removeDeliveryAddress = function () {

        //console.log('deliveryAddressFactory removeDeliveryAddress called');

        $.ajax({
            url: "/api/checkout/removeCheckoutAddress",
            type: "Post",
            cache: false,
            async: false,
            data: {},
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                //console.log('deliveryAddressFactory removeDeliveryAddress returned', data);
                self.parent.loadDeliveryMethods(true);
            },
            error: function (xhr, textStatus, errorThrown) {
                //console.log('deliveryAddressFactory removeDeliveryAddress error: ', xhr, textStatus, errorThrown);
                alert(self.resources.MajorError);
            }
        });
    }

    //self.getPayPalAddress = function (token) {
    //    //console.log('deliveryAddressFactory getPayPalAddress called', token);
    //    $.ajax({
    //        url: "https://api.sandbox.paypal.com/v1/oauth2/token/userinfo?schema=openid",
    //        type: "Get",
    //        cache: false,
    //        async: true,
    //        data: null,
    //        contentType: 'application/json;',
    //        beforeSend: function (xhr) {
    //            xhr.setRequestHeader("Authorization", "Bearer " + token);
    //        },
    //        success: function (data) {
    //            //console.log('deliveryAddressFactory getPayPalAddress returned', data);
    //            if (data && data.address) {
    //                var checkoutAddress = new CheckoutAddress(
    //                    5,
    //                    data.name,
    //                    data.phone_number,
    //                    0,
    //                    null,
    //                    data.address.street1,
    //                    data.address.street2, 
    //                    data.address.city,
    //                    data.address.state,
    //                    data.address.postal_code,
    //                    data.address.country,
    //                    null,
    //                    null,
    //                    false
    //                );

    //                asyncSelectCheckoutAddress(checkoutAddress);
    //                viewModel.selectedAddress(checkoutAddress);
    //            }
    //        },
    //        error: function (xhr) {
    //        }
    //    });
    //}

    self.checkSalesTaxZipCode = function (checkoutAddress, async, guest) {
        async = typeof async === 'undefined' ? true : async;
        guest = typeof guest === 'undefined' ? true : guest;

        var url = '/api/checkout/checkSalesTaxZipCode';

        var data = {
            MemberAddressID: ko.unwrap(checkoutAddress.memberAddressId),
            Town: ko.unwrap(checkoutAddress.town),
            County: ko.unwrap(checkoutAddress.county),
            Postcode: ko.unwrap(checkoutAddress.postcode),
            Country: ko.unwrap(checkoutAddress.country).Code,
            BasketCountry: getSiteCountry()
        };

        if (guest) {
            url = '/api/checkout/guestCheckSalesTaxZipCode';
            data = {
                country: ko.unwrap(checkoutAddress.country).Code,
                postcode: ko.unwrap(checkoutAddress.postcode)
            }
        }

        //console.log('deliveryAddressFactory checkSalesTaxZipCode called', data, async, guest, checkoutAddress)

        $.ajax({
            url: url,
            type: "Post",
            cache: false,
            async: async,
            data: JSON.stringify(data),
            contentType: 'application/json; charset=utf-8',
            success: function (data) {

                //console.log('deliveryAddressFactory checkSalesTaxZipCode returned', data);

                if (data.message === "checkoutBasketExpired") {
                    jumpToSessionExpired();
                    return;
                } else if (data.message === "invalidZipCode") {
                    $("#invalid-zip-modal").modal('show');
                    return;
                } else if (data.message === "zipTaxServiceFailed") {
                    $("#unavailable-zip-modal").modal('show');
                    return;
                }

                if (data.isSalesTaxKnown) {
                    var record = data.salesTaxAddresses[0];
                    checkoutAddress.town(record.town);
                    checkoutAddress.county(record.state);
                    //checkoutAddress.countyFromList(record.state);
                    self.currentAddress(checkoutAddress);
                    self.currentAddress().confirm();
                }
                else {
                    // hold the address temporarily, wait for confirmation, manipulate held address then set in the confirm fn
                    self.holdingAddress(checkoutAddress);
                    self.salesTaxAddresses(data.salesTaxAddresses);
                    $('#sales-tax-confirm-modal').modal('show');
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                //console.log('deliveryAddressFactory checkSalesTaxZipCode error:', xhr, textStatus, errorThrown);
                alert(self.resources.MajorError);
            }
        });
    }

    self.confirmSalesTaxAddress = function () {
        var checkoutAddress = self.holdingAddress();
        var record = self.salesTaxAddressFromList();
        if (checkoutAddress) {
            if (record) {
                checkoutAddress.countyListOrText(false);
                checkoutAddress.town(record.town);
                checkoutAddress.county(record.state);
                checkoutAddress.USCounty = record.county;
                //checkoutAddress.countyFromList(record.state);
                self.currentAddress(checkoutAddress);
                self.currentAddress().confirm();
                $('#sales-tax-confirm-modal').modal('hide');
            }
        }
    }

    self.makeDefaultAddress = function (address) {

        ko.utils.arrayForEach(self.addressItems(), function (addr) {
            addr.isDefault(false);
        });

        address.isDefault(true);

        //console.log('deliveryAddressFactory makeDefaultAddress called', address);

        // right, well this is unhelpful - it should be able to take in a full address (in case it hasn't been
        // saved before) and use that...
        // or that can just be handled from thingie. Main address save.

        $.ajax({
            url: "/api/member/makeAddressDefault",
            type: "Post",
            cache: false,
            async: true,
            data: JSON.stringify({
                memberAddressId: address.memberAddressId(),
                country: getCountryCode() // setting default address for this site culture
            }),
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                //console.log('makeDefaultAddress returned', data);
            },
            error: function () {
                alert(self.resources.MajorError);
            }
        });
    }

    self.loadAddressOptions = function () {
        //console.log('deliveryAddressFactory.loadAddressOptions');
    }

    self.upsApMap = null;
    self.upsApGeocoder = null;

    self.upsMoogleGapReady = function () {
        self.upsApGeocoder = new google.maps.Geocoder();
        let l = { lat: 55.378051, lng: -3.435973 };
        self.upsApGeocoder.geocode({ 'address': getMasterDefinition().UrlBreakdown.countryDetails.Name }, function (results, status) {
            if (status == google.maps.GeocoderStatus.OK) {
                //console.log('success', results);
                l = results[0].geometry.location
                self.upsApMap = new google.maps.Map(document.getElementById('ups-ap-map'), {
                    zoom: 4,
                    center: l
                });
            } else {
                alert("Could not find location: " + location);
                self.upsApMap = new google.maps.Map(document.getElementById('ups-ap-map'), {
                    zoom: 4,
                    center: l
                });
            }
        });
        //console.log('map ready');
    }

    self.upsLocatorNoResults = ko.observable(false);

    self.clearUpsLocatorResults = function () {
        ko.utils.arrayForEach(self.upsLocatorResults(), function (lr) {
            if (lr.marker) {
                lr.marker.setMap(null);
            }
            if (lr.infoWindow) {
                lr.infoWindow.setMap(null);
            }
        });

        self.upsLocatorResults([]);
    }

    self.closeApInfoWindows = function (onComplete) {
        ko.utils.arrayForEach(self.upsLocatorResults(), function (lr) {
            if (lr.infoWindow) {
                lr.infoWindow.close();
            }
        });
        if (typeof onComplete === 'function')
            onComplete();
    }

    self.selectUpsAp = function (ap) {
        self.selectedUpsAp(ap);
        $('#ups-collapse').collapse('hide');
    }

    self.cancelSelectedUpsAp = function () {
        self.selectedUpsAp(undefined);
        $('#ups-collapse').collapse('show');
    }

    self.confirmSelectedUpsAp = function () {
        self.deliverToUPSAccessPoint();
    }

    self.upsLocatorSearch = function () {

        self.upsLocatorNoResults(false);

        self.validateUPSSearchNow(true);

        if (self.upsSearchErrors().length) {
            self.upsSearchErrors.filter(function (x) {
                x.isValid.notifySubscribers(x.isValid());
            })
        } else {
            self.upsBusy(true);

            if (self.upsApGeocoder) {
                self.upsApGeocoder.geocode({ 'address': self.upsSearchTerm() + ',' + getMasterDefinition().UrlBreakdown.countryDetails.Name.replace(/\s/, '+') }, function (results, status) {
                    if (status == google.maps.GeocoderStatus.OK) {
                        self.upsApMap.setCenter(results[0].geometry.location);
                        self.upsApMap.setZoom(14);

                        var data = {
                            radius: self.upsApRange(),
                            searchTerm: self.upsSearchTerm(),
                            countryCode: getCountryCode(),
                            culture: getMasterDefinition().UrlBreakdown.countryDetails.LanguageDotNetCulture
                        }

                        self.clearUpsLocatorResults();

                        //console.log('calling upsLocatorSearch with', data);

                        $.ajax({
                            url: '/api/checkout/upsLocatorSearch',
                            type: 'POST',
                            dataType: 'json',
                            data: data,
                            success: function (data) {
                                //console.log('upsLocatorSearch returned', data);
                                self.upsBusy(false);
                                var results = [];
                                if (data.hasOwnProperty('searchResultsField')) {
                                    if (data.searchResultsField.hasOwnProperty('itemsField')) {
                                        ko.utils.arrayForEach(data.searchResultsField.itemsField, function (sr) {
                                            results.push(upsApMarker({
                                                upsLocatorResult: sr,
                                                map: self.upsApMap,
                                                parent: self
                                            }));
                                        });
                                    }
                                }
                                self.upsLocatorResults(results);
                            },
                            error: function (xhr, s, a) {
                                alert(getDGECResource('majorError'));
                            }
                        });

                    } else {
                        self.upsLocatorNoResults(true);
                        self.upsBusy(false);
                    }
                }).catch(function () {
                    self.upsLocatorNoResults(true);
                    self.upsBusy(false);
                });
            }
        }
    }

    self.validateUPSSearchNow = ko.observable(false);

    self.selectedUpsAp = ko.observable();

    self.upsApRange = ko.observable(15);

    self.upsSearchTerm = ko.observable('').extend({
        required: {
            params: true,
            onlyIf: function () {
                return self.validateUPSSearchNow()
            },
            message: 'UPS Search Requires Postcode'
        }
    });

    self.upsSearchParams = {
        label: self.resources.FormsPostCode,
        observable: self.upsSearchTerm,
        useNativeValidation: false,
        decorateInputElement: true,
        insertValidationMessages: false,
        showClear: false,
        permitRevert: false,
        charsRemainingOptions: {
            maxLength: 20
        },
        enterKey: self.upsLocatorSearch
    };

    self.upsSearchErrors = new ko.validation.group([
        self.upsSearchTerm
    ]);

    self.upsBusy = ko.observable(false);

    self.upsLocatorResults = ko.observableArray([]);

    self.showClickCollect = function () {
        $('#clickCollectPopup').fadeIn('slow');
    }

    self.closeClickAndCollectPopup = function () {
        $('#clickCollectPopup').hide();
    }

    self.closeSelectCheckoutAddress = function () {
        //asyncProcessPayPalAddress({ country: 'fake', fullName: null, phone: null }, null);
        //$('#payment-address-error-text').html(self.resources.PaypalPleaseProvideAddress);
        //$("#payment-address-error").show();
        //self.isUPSDelivery(false);
        //self.checkoutAddress(noCheckoutAddressSelected());
        //if (getCheckoutType() == 4) {
        //    self.selectedAddress(null);
        //    singlePageCheckout.addressSelected(false);
        //} else {
        //    self.selectedAddress(self.savedSelectedAddress);
        //}
    }

    self.unconfirmAddress = function () {
        // whether or not this is a ups delivery, when unconfirming delivery address we always want to reset to square one! 
        // simple

        self.currentAddress().isClickCollect = false;

        if (self.currentAddress().deliveryOptionId() != 1) {
            self.currentAddress(checkoutDeliveryAddress(self, getMasterDefinition().UrlBreakdown.countryDetails.CountryCode, false, 1));
        } else {
            self.currentAddress().confirmed(false);
        }

        window.setTimeout(function () {
            self.currentAddress().postcode.valueHasMutated();
        }, 0);

        self.selectedIdentificationAddress(undefined);
        self.isUPSDelivery(false);
        self.currentAddress().UPSAPID = null;
        self.addressConfirmed(false);
        self.identificationAddressConfirmed(false);


        //if (self.isUPSDelivery()) {
        //    self.selectedIdentificationAddress(undefined);
        //    self.currentAddress().UPSAPID = null;
        //    self.isUPSDelivery(false); // ?! it's still very much a UPS delivery!
        //    self.addressConfirmed(false);
        //    self.identificationAddressConfirmed(false);
        //} else {
        //    self.addressConfirmed(false);
        //}
    }

    self.unconfirmUPSIdentificationAddress = function () {
        self.identificationAddressConfirmed(false);
    }

    self.updateFromPayPal = function (payPalResponse) {

        //console.log('updateFromPayPal called', payPalResponse);

        var details = payPalResponse.details;
        var addr = details.shippingAddress;

        var checkoutAddress = self.createCheckoutDeliveryAddressFromPayPalAddress(addr);

        self.currentAddress(checkoutAddress);

        if (checkoutAddress.invalid == true) {
            self.addressConfirmed(false);
            self.invalidDeliveryCountry();
        } else {
            self.currentAddress().confirm();
        }

        //self.processPayPalAddress(checkoutAddress, payPalResponse.nonce);
    }

    self.invalidDeliveryCountry = function () {

        var message = self.resources.PaypalInvalidAddress;

        var countries = getAvailableCountries();
        var countryStr = '';

        $.each(countries, function (i, country) {
            if (countries.length > 1 && i == countries.length - 1)
                countryStr += ' & '
            else if (i > 0) {
                countryStr += ', '
            }
            countryStr += country.Name;
        });

        var error = message.replace('{Countries}', countryStr);

        $('#payment-address-error-text').html(error);
        $("#payment-address-error").modal('show');
    }

    // here we're actively clearing out the paypal nonce... why... because if the nonce includes shipping data
    // and we've changed the shipping data or some how failed to arrive at a valid shipping address... then...
    // bad.
    self.setPayPalNonce = function (nonce) {
        //console.log('deliveryAddressFactory setPayPalNonce called', nonce);
        $.ajax({
            url: '/api/checkout/setPayPalNonce',
            type: 'POST',
            async: false,
            cache: false,
            data: { nonce: nonce },
            success: function (data) {
                //console.log('deliveryAddressFactory setPayPalNonce returned', data);
                if (!data) {

                }
            },
            error: function (xhr, s, a) {

            }
        });
    }

    self.init = function () {
        //console.log('deliveryAddressFactory init');
        if (self.parent.checkoutType != 3) { // Amazon requires special treatment
            if (self.checkoutAddressFromServer()) {
                //console.log('deliveryAddressFactory init mark A: checkoutAddressFromServer was not null', self.checkoutAddressFromServer());
                // we have an address from the server, what do we have to do with it?
                // first off: do we TRUST this address?
                // yes, because how else would it be saved as the checkoutAddress for this checkout. Implicitly trust.
                // it might NOT be saved as the checkoutAddress for this checkout if it's actually a default from a member's address book
                // go figure: we need to catch this case and... well, save it...?
                // or: have the database do the checkoutAddress specification for us, so that if there IS a default member address to be had,
                // it's already saved as the checkout address
                var addr = self.checkoutAddressFromServer();

                var checkoutAddr = self.createCheckoutDeliveryAddressFromServerAddress(addr);

                if (addr.ShouldPerformSalesTaxCheck) {
                    //console.log('deliveryAddressFactory init mark A i: checkoutAddressFromServer performing sales tax check');
                    // need to perform the sales tax check, saving a result to the checkout address or prompting the user for selection...
                    self.checkSalesTaxZipCode(checkoutAddr, false, checkoutAddr.memberAddressId() == null);
                } else {
                    //console.log('deliveryAddressFactory init mark A ii: checkoutAddressFromServer no sales tax check needed');

                    self.currentAddress(checkoutAddr);

                    self.addressConfirmed(true);
                }
            } else {
                // we don't have an address from the server; do we have knowledge of a default?
                //console.log('deliveryAddressFactory init mark B: checkoutAddressFromServer was null');

                if (self.defaultMemberAddress()) {
                    // we have a default member address
                    //console.log('deliveryAddressFactory init mark B.i: defaultMemberAddress was not null');
                    var addr = self.defaultMemberAddress();

                    self.currentAddress(self.createCheckoutDeliveryAddressFromMemberAddress(addr));

                    // address is not yet confirmed: it is simply the default and must go through the same validation process
                    // (in saving) as a newly entered address.
                } else {
                    // we don't have a default address
                    //console.log('deliveryAddressFactory init mark B.ii: defaultMemberAddress was null');

                    // at this point, if we've come from PayPal, something has gone seriously wrong!
                    if (self.parent.checkoutType == 4) {
                        //console.log('deliveryAddressFactory init mark B.iii: paypal checkout with no address');

                        $('#payment-address-error-text').html(self.resources.PaypalPleaseProvideAddress);
                        $("#payment-address-error").modal('show');
                        self.setPayPalNonce('undefined');
                    }
                }
            }
        } else {
            // Amazon does its own dance elsewhere
            //console.log('deliveryAddressFactory init mark C: amazonCheckout');
        }

        $('.loadingWrapper').not('#checkout-complete-waiting .loadingWrapper').hide();
    }

    self.collectFromStore = function (store) {
        var checkoutAddress;

        switch (store) {
            case "kings":
                checkoutAddress = checkoutDeliveryAddress(
                    self,
                    "GB",
                    false,
                    2,
                    null,
                    null,
                    null,
                    "Designers Guild Homestore",
                    "267-271 Kings Road",
                    "",
                    "Chelsea",
                    "London",
                    "SW3 5EN",
                    null,
                    null,
                    false,
                    false);
                break;

            case "marylebone":
                checkoutAddress = checkoutDeliveryAddress(
                    self,
                    "GB",
                    false,
                    2,
                    null,
                    null,
                    null,
                    "Designers Guild Homestore",
                    "76 Marylebone High Street",
                    "",
                    "Marylebone",
                    "London",
                    "W1U 5JU",
                    null,
                    null,
                    false,
                    false);
                break;


            case "distribution":
                checkoutAddress = checkoutDeliveryAddress(
                    self,
                    "GB",
                    false,
                    2,
                    null,
                    null,
                    null,
                    "Designers Guild Distribution Centre",
                    "Unit 10 Matrix Park",
                    "900 Coronation Road",
                    "Park Royal",
                    "London",
                    "NW10 7PH",
                    null,
                    null,
                    false,
                    false);
                break;
        }

        if (checkoutAddress) {
            checkoutAddress.isClickCollect = true;
            $('#collection-modal').modal('hide');
            self.currentAddress(checkoutAddress);
            self.currentAddress().confirm();
        }
    }



    self.getNewEstimate = function (dispatchLine) {

        self.parent.summaryBusy(true);

        //console.log('deliveryAddressFactory getNewEstimate called', dispatchLine);
        var dispatchid = $(dispatchLine).attr("data-dispatchid");
        var deliverybandid = $(dispatchLine).attr("data-deliverybandid");
        var isPartShip = $(dispatchLine).attr("data-partship").toLowerCase() === 'true';
        var $partShipWrapper = $("#part-ship-wrapper-" + dispatchid);

        if (isPartShip) {
            $partShipWrapper.show();
        } else {
            $partShipWrapper.hide();
        }

        var deliverInPartShip = $("input[name=part-ship-" + dispatchid + "]:checked").attr('data-shouldpartship').toLowerCase() === 'true';

        // this should absolutely no longer be necessary: all the delivery options and their dispatch date estimates are
        // present from the initial load of the partial... 
        // however, at the moment they're not being rendered unto to our viewModel.

        // hooowever... we still do need to call saveDispatch.

        // so the crux of this method is to change the displayed dispatch date estimate above this delivery
        // and then call self.saveDispatch();

        // also, we are not showing the partShip option!

        // okay? go!

        self.saveDispatch();

        //$.ajax({
        //    url: "/api/checkout/getDispatchEstimate",
        //    type: "Post",
        //    cache: false,
        //    async: true,
        //    data: JSON.stringify({
        //        dispatchId: dispatchid,
        //        deliveryBandId: deliverybandid,
        //        basketCountry: getSiteCountry(),
        //        partShip: deliverInPartShip
        //    }),
        //    contentType: 'application/json; charset=utf-8',
        //    success: function (data) {

        //        //console.log('deliveryAddressFactory getNewEstimate returned', data);
        //        if (data.message === "checkoutBasketExpired") {
        //            jumpToSessionExpired();
        //            return;
        //        }

        //        var $estimate = $("#estimate-" + dispatchid);
        //        $estimate.text(data.displayLine);
        //        $estimate.attr('data-mindeliverydate', data.minDeliveryDate);
        //        $estimate.attr('data-maxdeliverydate', data.maxDeliveryDate);

        //        self.saveDispatch();
        //    },
        //    error: function (xhr, textStatus, errorThrown) {
        //        self.summaryBusy(false);
        //        //console.log('dg-checkoutdispatch-estimate: ' + textStatus);
        //        alert('@HtmlHelperExtra.EscapeForJS(HtmlHelperExtra.Resource("MajorError", languageId, countryId, contentDate))');
        //    }
        //});
    }

    self.getNewEstimateFromPartShip = function (partShipOption) {
        var dispatchid = $(partShipOption).attr("data-dispatchid");
        var dispatchLines = $(".deliveryradio[data-dispatchid='" + dispatchid + "'][data-dispatchid='" + dispatchid + "']:checked");

        self.getNewEstimate(dispatchLines[0]);
    }

    self.saveDispatch = function () {

        self.parent.dispatchBusy(true);

        var $selectedDispatchOptions = $(".delivery-option:checked").not('.consolidation');
        var dispatchLines = [];

        $.each($selectedDispatchOptions, function (index, value) {

            var data = ko.dataFor(value);
            var context = ko.contextFor(value);

            var singleDeliverySelected = null;
            var fullDespatchSelection = false;

            if (context.$data.hasOwnProperty('singleDeliverySelected')) {
                singleDeliverySelected = !context.$data.singleDeliverySelected();
                fullDespatchSelection = context.$data.fullDispatchSelection();
            }
            else {
                singleDeliverySelected = !context.$parent.singleDeliverySelected();
                fullDespatchSelection = context.$parent.fullDispatchSelection();
            }

            dispatchLines.push(dispatchLine({
                dispatchId: data.DispatchID,
                deliveryBandId: data.DeliveryBandID,
                shouldPartShip: singleDeliverySelected,
                minDeliveryDate: data.MinDeliveryDate,
                maxDeliveryDate: data.MaxDeliveryDate,
                customerRequestsFullDespatch: fullDespatchSelection
            }));
        });

        console.trace(dispatchLines);

        $.ajax({
            url: "/api/checkout/saveDispatch",
            type: "Post",
            cache: false,
            async: true,
            data: JSON.stringify({
                dispatchLines: dispatchLines
            }),
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                if (data.message === "checkoutBasketExpired") {
                    jumpToSessionExpired();
                    return;
                }
                self.deliveryMethodUpdated();
                self.parent.dispatchBusy(false);
            },
            error: function (xhr, textStatus, errorThrown) {
                self.parent.itemsBusy(false);
                self.parent.summaryBusy(false);
                alert(self.resources.MajorError);
            }
        });

        return true;
    }

    self.deliveryMethodUpdated = function () {
        self.parent.loadBasketSummary();
        if (self.parent.paymentMethodFactory.amazonWalletWidget) {
            self.parent.paymentMethodFactory.updateAmazonValue();
        }
    }

    self.deliverToUPSAccessPoint = function () {

        var addr = self.selectedUpsAp();

        var checkoutAddress = self.createCheckoutDeliveryAddressFromUPSAccessPoint(addr);

        self.currentAddress(checkoutAddress);
        self.currentAddress().confirm();

        self.isUPSDelivery(true);

        $("#collection-modal").modal('hide');

        if (typeof (window.history.pushState) === 'function') {
            var url = [location.protocol, '//', location.host, location.pathname].join('');
            var stateObj = { path: url };
            history.pushState(stateObj, '', url);
        } else {
            window.location.href = url;
        }
    }


    self.reset = function () {
        //console.log('deliveryAddressFactory reset');
        // do we actually need to do much here..?

        //self.defaultMemberAddress(getDefaultMemberAddress()); // this could be wrong: the default might have changed...?
        //self.checkoutAddressFromServer(getCurrentCheckoutAddress());
        //self.addressItems([]);
        //self.tradeAddressItems([]);
    }

    // #endregion


    // #region errors and validation



    // #endregion

    return self;
});

const lineMemo = ((r) => {
    var self = {
        resources: r.parent.resources
    };// r.record;

    //console.log('creating lineMemo for', self);
    self.CheckoutBasketID = r.record.CheckoutBasketID;
    self.BasketName = r.record.BasketName;
    self.Code = r.record.Code;
    self.Memo = r.record.Memo;
    self.Reference = r.record.LineReference;
    self.memo = ko.observable(r.record.Memo);
    self.reference = ko.observable(r.record.LineReference);

    self.memoParams = {
        label: self.resources.TradeCheckoutSpecialInstructions,
        observable: self.memo,
        useNativeValidation: false,
        decorateInputElement: true,
        insertValidationMessages: false,
        showClear: false,
        permitRevert: false,
        rows: 3,
        maxNewLines: 3,
        charsRemainingOptions: {
            maxLength: 500
        }
    };

    self.referenceParams = {
        label: self.resources.TradeCheckoutLineReference,
        observable: self.reference,
        useNativeValidation: false,
        decorateInputElement: true,
        insertValidationMessages: false,
        showClear: false,
        permitRevert: false,
        charsRemainingOptions: {
            maxLength: 50
        }
    };

    self.cancelMemoModal = function () {
        self.memo(undefined);
        $('#line-memo-modal').modal('hide');
    }

    self.saveLineMemo = function () {
        var data = {
            checkoutBasketId: self.CheckoutBasketID,
            reference: self.reference(),
            memo: self.memo()
        };
        //console.log('calling saveLineMemo with', data);
        $.ajax({
            url: '/api/checkout/saveLineMemo',
            type: 'POST',
            dataType: 'json',
            data: data,
            success: function (data) {
                if ((self.memo() == '' || self.memo() == null) && (self.reference() == '' || self.reference() == null)) {
                    self.savedMemo(false);
                } else {
                    self.savedMemo(true);
                }
                $('#line-memo-modal').modal('hide');
            },
            error: function (xhr, s, a) {
                alert(self.resources.MajorError);
            }
        });
    }

    self.removeLineMemo = function () {
        self.memo(undefined);
        self.reference(undefined);
        self.saveLineMemo();
    }

    self.savedMemo = ko.observable((self.Memo || self.Reference) ? true : false)

    return self;
});


const lineTreatment = ((r) => {
    let self = {
        resources: r.parent.resources
    };

    self.CheckoutBasketID = r.record.CheckoutBasketID;

    self.BasketName = r.record.BasketName;
    self.Code = r.record.Code;
    self.OtherTreatment = r.record.OtherTreatment;
    self.StainTreatment = r.record.StainTreatment;
    self.Crib5Treatment = r.record.Crib5Treatment;
    self.DomesticUpholsteryTreatment = r.record.DomesticUpholsteryTreatment;
    self.ExpressTreatment = r.record.ExpressTreatment;

    self.otherTreatment = ko.observable(r.record.OtherTreatment);
    self.otherTreatmentParams = {};
    self.otherTreatmentVisible = ko.observable(self.otherTreatment() ? true : false);

    self.stainTreatment = ko.observable(r.record.StainTreatment);
    self.domesticUpholsteryTreatment = ko.observable(r.record.DomesticUpholsteryTreatment);
    self.crib5Treatment = ko.observable(r.record.Crib5Treatment);
    self.expressTreatment = ko.observable(r.record.ExpressTreatment);

    self.cancelTreatmentModal = function () {
        self.otherTreatment(undefined);
        self.otherTreatmentVisible(false);
        self.stainTreatment(false);
        self.domesticUpholsteryTreatment(false);
        self.crib5Treatment(false);
        self.expressTreatment(false);
        $('#line-treatments-modal').modal('hide');
    }

    self.saveLineTreatments = function () {
        var data = {
            checkoutBasketId: self.CheckoutBasketID,
            otherTreatment: self.otherTreatment(),
            stainTreatment: self.stainTreatment(),
            domesticUpholsteryTreatment: self.domesticUpholsteryTreatment(),
            crib5Treatment: self.crib5Treatment(),
            expressTreatment: self.expressTreatment()
        };
        //console.log('calling saveLineTreatments with', data);
        $.ajax({
            url: '/api/checkout/saveLineTreatments',
            type: 'POST',
            dataType: 'json',
            data: data,
            success: function (data) {
                if (self.otherTreatmentVisible() == false && self.stainTreatment() == false && self.domesticUpholsteryTreatment() == false && self.crib5Treatment() == false) {
                    self.savedTreatments(false);
                } else {
                    self.savedTreatments(true);

                    var body = self.resources.CustomerServiceApplyTreatments1 +
                        ' <a href="' + self.resources.TreatmentChargesDocumentURL + '" target="_blank">' +
                        self.resources.TreatmentChargesDocument +
                        '</a> ' +
                        self.resources.CustomerServiceApplyTreatments2;

                    window.showInfo(self.resources.ThankYou, body);
                }
                $('#line-treatments-modal').modal('hide');
            },
            error: function (xhr, s, a) {
                alert(self.resources.MajorError);
            }
        });
    }

    self.removeLineTreatments = function () {
        self.otherTreatment(undefined);
        self.otherTreatmentVisible(false);
        self.stainTreatment(false);
        self.domesticUpholsteryTreatment(false);
        self.crib5Treatment(false);
        self.expressTreatment(false);
        self.saveLineTreatments();
    }

    self.savedTreatments = ko.observable((self.OtherTreatment || self.StainTreatment || self.DomesticUpholsteryTreatment || self.Crib5Treatment) ? true : false)

    return self;
});


const cutLengthBreakdown = ((r) => {

    let self = Object.assign({
        resources: r.parent.resources
    }, r.record);

    //console.log('created cutLengthBreakdown for', self);

    self.unsaved = ko.observable(false);

    self.parent = r.parent;

    self.cutMaxValue = self.MaxCutLength;
    self.cutMinValue = self.MinCutLength;

    if (self.cutMaxValue == 0)
        self.cutMaxValue = 10;

    self.loadedCuts = [];

    if (self.CutPieceBreakdown) {
        self.loadedCuts = JSON.parse(self.CutPieceBreakdown);
    }

    self.readOnlyCut = ko.observable(self.Quantity);

    self.readOnlyCut.subscribe(function (newValue) {
        //console.log('readOnlyCut subscribe hit', newValue);
        if (newValue < 0)
            self.readOnlyCut(0);
    });

    self.remainingToCut = ko.pureComputed(function () {
        return +self.Quantity - Enumerable.From(self.cuts()).Select(function (c) {
            if (c.confirmed())
                return +c.cutLength()
            else return 0;
        }).Sum();
    }, self);

    self.selectedBatch = ko.observable();

    self.selectedBatch.subscribe(function (newValue) {
        //console.log('selectedBatch', newValue);
        if (newValue == null) {
            //console.log('selectedBatch is going undefined');
        }
    });

    self.cuts = ko.observableArray([]).extend({
        validation: {
            validator: function (arr, otherVal) {
                if (arr.length) {
                    return Enumerable.From(arr).Select(function (le) {
                        return +le.cutLength()
                    }).Sum() == +self.Quantity;
                } else {
                    return true;
                }
            },
            params: false,
            message: 'Incomplete'
        }
    });

    self.cuts.subscribe(function (newValues) {
        self.unsaved(true);
    }, self, 'arrayChange');

    self.addCut = function (val, confirmed) {
        confirmed = typeof confirmed === 'undefined' ? false : confirmed;
        var newCut = {
            cutLength: ko.observable(val).extend({
                numberFormatted: {
                    digits: 1,
                    currency: {
                        FormattingCulture: 'en-GB'
                    },
                    minValue: 0,
                    maxValue: self.cutMaxValue
                }
            }),
            confirmed: ko.observable(confirmed),
            cutNumber: self.cuts().length + 1
        }
        newCut.cutLengthParams = {
            label: 'Cut',
            observable: newCut.cutLength,
            useNativeValidation: false,
            decorateInputElement: true,
            insertValidationMessages: false,
            showClear: true,
            clearFunction: () => {
                self.removeCut(newCut);
            }
        };

        newCut.cutLength.beforeChangeValue = 0;

        newCut.cutLength.subscribe(function (newValue) {
            newCut.confirmed(false);
            if (newValue) {
                newValue = +newValue;

                var maxLength = self.MaxCutLength;
                if (maxLength == 0)
                    maxLength = 10
                if (newValue > maxLength) {
                    newCut.cutLength(maxLength);
                    newCut.cutLength.beforeChangeValue = maxLength;
                    return;
                }

                if (newValue < self.MinCutLength) {
                    newCut.cutLength(self.MinCutLength);
                    newCut.cutLength.beforeChangeValue = self.MinCutLength;
                    return;
                }

                if (newValue > self.remainingToCut()) {
                    newCut.cutLength(self.remainingToCut());
                    return;
                }

                if (newValue <= self.readOnlyCut()) {
                    newCut.confirmed(true);
                } else {
                    newCut.cutLength(self.readOnlyCut())
                }
            } else {
                self.readOnlyCut(self.readOnlyCut() + +newCut.cutLength.beforeChangeValue);
            }
        });

        newCut.cutLength.subscribe(function (oldValue) {
            newCut.cutLength.beforeChangeValue = oldValue || 0;
        }, newCut, "beforeChange");

        //console.log('adding cut', newCut);

        self.cuts.push(newCut);

        window.setTimeout(function () {
            $('#stock-modal input.mdc-text-field__input').select();
        }, 0);
    };

    self.removeCut = function (cut) {
        self.cuts.remove(cut);
    };

    self.savedCutLengthBreakdown = ko.observable(false);

    if (self.CutPieceBreakdown) {
        self.savedCutLengthBreakdown(true);
    }

    if (self.Batch) {
        self.selectedBatch(self.Batch);
    }

    self.saveCutLengthBreakdown = function () {
        //console.log('saveCutLengthBreakdown called', self.toJson());
        $.ajax({
            url: '/api/checkout/saveCutLengthBreakdown',
            type: 'POST',
            dataType: 'json',
            data: {
                checkoutBasketId: self.CheckoutBasketID,
                breakdown: self.toJson(),
                batch: self.selectedBatch()
            },
            success: function (data) {
                if (data.hasOwnProperty('message') && typeof jumpToSessionExpired === 'function') {
                    if (data.message === "checkoutBasketExpired") {
                        jumpToSessionExpired();
                        return;
                    }
                } else {
                    $('#stock-modal').modal('hide');
                    self.Batch = self.selectedBatch();
                    self.CutLengthBreakdown = self.toJson();
                    if (self.cuts().length) {
                        self.savedCutLengthBreakdown(true);
                    }
                    self.parent.actingCutLength(undefined);
                }
            }
        });
    }

    self.toJson = function () {
        if (self.cuts().length) {
            return JSON.stringify(Enumerable.From(self.cuts()).Where(function (c) {
                return c.cutLength() != 0
            }).Select(function (c) {
                return { cut: ko.unwrap(c.cutLength) }
            }).ToArray());
        } else {
            return null;
        }
    }

    window.setTimeout(function () {
        for (var i = 0; i < self.loadedCuts.length; i++) {
            self.addCut(self.loadedCuts[i].cut, true);
        }
    }, 0);

    self.removeCutLengthBreakdown = function () {

        window.showConfirm(self.resources.AreYouSure, self.resources.AreYouSureRemoveCutLengthBreakdown, function () {
            $.ajax({
                url: '/api/checkout/saveCutLengthBreakdown',
                type: 'POST',
                dataType: 'json',
                data: {
                    productId: self.ProductID,
                    basketLineTypeId: self.BasketLineTypeID,
                    breakdown: null,
                    batch: null
                },
                success: function (data) {
                    if (data.hasOwnProperty('message') && typeof jumpToSessionExpired === 'function') {
                        if (data.message === "checkoutBasketExpired") {
                            jumpToSessionExpired();
                            return;
                        }
                    } else {
                        //$('#stock-modal').modal('hide');
                        self.cuts([]);
                        self.selectedBatchDataItem(undefined);
                        self.selectedBatch(undefined);
                        self.batchDropDownList.dropDownListSettings.value('');
                        //self.selectedBatchDataItem.mdcSelect.label_.float(false);
                        //self.selectedBatchDataItem.mdcSelect.layout();
                        self.Batch = undefined;
                        self.CutLengthBreakdown = undefined;
                        self.savedCutLengthBreakdown(false);
                    }
                }
            });
        }, null);
    }

    self.cancelCutModal = function () {
        if (self.unsaved()) {
            window.showConfirm(self.resources.AreYouSure, self.resources.UnsavedChangesWillBeLost, function () {
                $('#cut-length-modal').modal('hide');
                self.cuts([]);
                self.parent.actingCutLength(undefined);
            }, null);
        } else {
            $('#cut-length-modal').modal('hide');
            self.parent.actingCutLength(undefined);
        }
    }

    self.unsaved(false);

    self.availableBatches = ko.observableArray([]);

    self.selectedBatchDataItem = ko.observable();

    self.availableBatches.subscribe(function (newValues) {
        window.setTimeout(function () {
            if (Enumerable.From(newValues).Where(function (x) {
                return x.text == self.Batch
            }).Any()) {
                self.selectedBatch(self.Batch);
            } else {
                //self.selectedBatch(undefined);
                if (self.selectedBatchDataItem.hasOwnProperty('mdcSelect')) {
                    //self.selectedBatchDataItem.mdcSelect.label_.float(false);
                    //self.selectedBatchDataItem.mdcSelect.layout();
                }
            }
        }, 0);
    }, self);

    self.batchDropDownList = {
        observable: self.selectedBatchDataItem,
        showClear: true,
        width: 'auto',
        label: self.resources.AvailableBatches,
        valuePrimitive: false,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        controlClass: 'form-control',
        dropDownListSettings: {
            value: self.selectedBatch,
            dataItem: self.selectedBatchDataItem,
            dataValueField: 'batch',
            dataTextField: 'text',
            widget: ko.observable(),
            open: function (e) {
                $(e.sender._widgetOptions.dataItem.mdcSelect.root_).toggleClass('mdc-select--focused');
            },
            close: function (e) {
                $(e.sender._widgetOptions.dataItem.mdcSelect.root_).toggleClass('mdc-select--focused');
            },
            dataBound: function (e) {
                self.selectedBatchDataItem.mdcSelect.layout();
            },
            data: self.availableBatches,
            valuePrimitive: false,
            autoWidth: false,
            autoBind: true,
            optionLabel: ' ',
            popup: {
                appendTo: $('.modal-body', $('#stock-modal'))
            }
        },
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            self.dropDownListSettings.widget().value('');
            self.dropDownListSettings.widget()._widgetOptions.dataItem.mdcSelect.getDefaultFoundation().adapter_.closeOutline();
            self.dropDownListSettings.widget()._widgetOptions.dataItem.mdcSelect.label_.float(false);
        }
    };

    return self;
});

const dispatchLine = ({
    dispatchId,
    deliveryBandId,
    shouldPartShip,
    minDeliveryDate,
    maxDeliveryDate,
    customerRequestsFullDespatch
} = {}) => {
    let self = {};

    self.dispatchId = dispatchId;
    self.deliveryBandId = deliveryBandId;
    self.partShip = shouldPartShip;
    self.minDeliveryDate = minDeliveryDate;
    self.maxDeliveryDate = maxDeliveryDate;
    self.customerRequestsFullDespatch = customerRequestsFullDespatch;

    return self;
};

const checkoutDeliveryAddress = (
    parent,
    countryCode,
    isUPSIdAddress,
    deliveryOptionId,
    fullName,
    phone,
    memberAddressId,
    companyName,
    addressLine1,
    addressLine2,
    town,
    county,
    postcode,
    upsAPId,
    upsAPMemberAddressId,
    isTradeAddress,
    isDefault) => {
    let self = {
        parent: parent,
        resources: parent.resources
    };

    //console.log('checkoutDeliveryAddress instantiated');

    self.init = function () {
        //console.log('checkoutDeliveryAddress init');
    }

    // #region properties

    self.labelClass = 'classy';
    self.controlClass = 'inputField';
    self.UPSAPID = upsAPId;
    self.upsAPMemberAddressId = upsAPMemberAddressId;
    self.isTradeAddress = isTradeAddress;
    self.isUPSIdAddress = isUPSIdAddress;
    self.useThisAddressText = !self.isUPSIdAddress ? self.resources.UseThisDeliveryAddress : self.resources.UseThisIdentificationAddress;

    // #endregion


    // #region observables

    self.isDefault = ko.observable(isDefault);


    self.availableCountries = ko.observable(getAvailableCountries());

    var country = undefined;

    if (countryCode) {
        var countryFromList = Enumerable.From(self.availableCountries()).Where(function (x) {
            return x.Code == countryCode
        }).SingleOrDefault();

        if (countryFromList) {
            country = countryFromList;
        }
    }

    if (country == null) {
        self.invalid = true;
        country = {};
    }

    self.availableCounties = ko.observable(getAvailableCounties(country.Code));

    self.PCASearchId = ko.observable();

    self.PCASearchTerm = ko.observable();

    self.PCAContainer = ko.observable();

    self.PCACountry = ko.observable(country);

    self.validateNow = ko.observable(false);

    // true == list, false == text
    // initially getAvailableCounties() == 0!
    // only when country is force-selected by the list existing is this ever changed
    // it's just random good luck that United Kingdom happens to appear first in the list...
    // we really need to provide the site default to the country parameter, and use that to pick up the 
    // correct country to select in the list

    // initially we want this to be set depending on not just the availability of counties, but on the presence
    // of our chosen county in this list...
    var extantCounty = Enumerable.From(self.availableCounties()).Where(function (x) {
        return (x.County == county) || (x.CountyName == county);
    }).SingleOrDefault();

    self.countyListOrText = ko.observable(((county == null) && (self.availableCounties().length > 0)) || (extantCounty != null));

    self.deliveryOptionId = ko.observable(deliveryOptionId);

    self.memberAddressId = ko.observable(memberAddressId);

    self.fullName = ko.observable(fullName).extend({
        maxLength: {
            params: 50,
            onlyIf: function () {
                return (self.validateNow() && (self.deliveryOptionId() != 3));
            },
            message: self.resources.addressErrorName
        },
        minLength: {
            params: 1,
            onlyIf: function () {
                return (self.validateNow() && (self.deliveryOptionId() != 3));
            },
            message: self.resources.addressErrorName
        },
        required: {
            params: true,
            onlyIf: function () {
                return (self.validateNow() && (self.deliveryOptionId() != 3));
            },
            message: self.resources.addressErrorName
        },
        errorPulse: {}
    });

    self.companyName = ko.observable(companyName).extend({
        maxLength: {
            params: 35,
            onlyIf: function () {
                return self.validateNow();
            },
            message: self.resources.addressErrorCompanyName
        },
        errorPulse: {}
    });

    self.addressLine1 = ko.observable(addressLine1).extend({
        minLength: {
            params: 1,
            onlyIf: function () {
                return self.validateNow();
            },
            message: self.resources.addressErrorLineOne
        },
        maxLength: {
            params: 60,
            onlyIf: function () {
                return self.validateNow();
            },
            message: self.resources.addressErrorName
        },
        required: {
            params: true,
            onlyIf: function () {
                return self.validateNow();
            },
            message: self.resources.addressErrorLineOne
        },
        errorPulse: {}
    });

    self.addressLine2 = ko.observable(addressLine2).extend({
        maxLength: {
            params: 60,
            onlyIf: function () {
                return self.validateNow();
            },
            message: self.resources.addressErrorLineTwo
        },
        errorPulse: {}
    });

    self.town = ko.observable(town).extend({
        maxLength: {
            params: 50,
            onlyIf: function () {
                return self.validateNow();
            },
            message: self.resources.addressErrorTown
        },
        required: {
            params: true,
            onlyIf: function () {
                return self.validateNow();
            },
            message: self.resources.addressErrorTown
        },
        errorPulse: {}
    });

    // which of these two can be required? what is the interplay between them?
    // when there are counties in a list available, then the list display is preferred - unless a selection is made via 
    // PCA, which overrides the list existence and fills this in...
    self.county = ko.observable(county);

    // so... countyFromList can only be described as being required if it's supposed to be visible.
    // when countyListOrText == true, we will display list
    // when countyListOrText == false, we will display input text
    self.countyFromList = ko.observable(county).extend({
        required: {
            params: true,
            onlyIf: function () {
                return (self.validateNow()) &&
                    ((self.town() !== 'London' && self.town() !== 'london') &&
                        (self.countyListOrText() == true) &&
                        (self.parent.parent.checkoutType != 3 && self.parent.parent.checkoutType != 4 && self.deliveryOptionId() != 3));
            },
            message: self.resources.addressErrorCounty
        },
        errorPulse: {}
    });

    self.county.extend({
        maxLength: 40,
        required: {
            params: true,
            onlyIf: function () {
                return (self.validateNow()) &&
                    ((self.town() !== 'London' && self.town() !== 'london') &&
                        (self.countyListOrText() == false) &&
                        (self.parent.parent.checkoutType != 3 && self.parent.parent.checkoutType != 4 && self.deliveryOptionId() != 3));
            },
            message: self.resources.addressErrorCounty
        },
        errorPulse: {}
    });

    self.country = ko.observable(country);

    self.postcode = ko.observable(postcode).extend({
        maxLength: 20,
        required: {
            params: true,
            onlyIf: function () {
                if (self.country()) {
                    return (self.validateNow()) && (self.country().MandatePostcode);
                } else {
                    return self.validateNow();
                }
            },
            message: self.resources.addressErrorPostCode
        },
        errorPulse: {}
    });

    self.phone = ko.observable(phone).extend({
        minLength: {
            params: 1,
            onlyIf: function () {
                return
                (self.validateNow()) &&
                    (self.parent.parent.checkoutType != 3 && self.parent.parent.checkoutType != 4 && self.deliveryOptionId() != 3);
            },
            message: self.resources.addressErrorPhone
        },
        maxLength: {
            params: 60,
            onlyIf: function () {
                return
                (self.validateNow()) &&
                    (self.parent.parent.checkoutType != 3 && self.parent.parent.checkoutType != 4 && self.deliveryOptionId() != 3);
            },
            message: self.resources.addressErrorPhone
        },
        required: {
            params: true,
            onlyIf: function () {
                if (typeof forcePhoneRequired !== 'undefined') {
                    return self.validateNow();
                } else {
                    return (self.validateNow()) &&
                        (self.parent.parent.checkoutType != 2 &&
                            self.deliveryOptionId() != 3);
                }
            },
            message: self.resources.phoneValidation
        },
        errorPulse: {}
    });

    // This is used for both Amazon and PayPal to help gather phone numbers
    // It is used in the unsaved changes notice.
    self.phoneChanged = ko.observable(false);

    self.phoneChanged.subscribe(function (newValue) {
        //console.log('phoneChanged', newValue);
        if (newValue) {
            if (!self.confirmed()) { // might already be false - guest, member no addresses, amazon, paypal...
                self.confirmed.valueHasMutated(); // if so, we still want to fire the subscription
            } else {
                self.confirmed(false);
            }
        }
    });

    self.phoneSave = function () {
        self.validateNow(true);
        window.setTimeout(function () {
            if (!$(self.phone.DOMElement).intlTelInput('isValidNumber')) {
                self.parent.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'deliveryAddressFactory phoneSave: ' + $(self.phone.DOMElement).intlTelInput('getNumber') + ' (Error: ' +
                    $(self.phone.DOMElement).intlTelInput('getValidationError') + ')');
            } else {
                self.parent.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'deliveryAddressFactory phoneSave: ' + $(self.phone.DOMElement).intlTelInput('getNumber') + ' (Valid)');
            }
            if (self.phone.isValid()) {
                if (self.confirmed()) { // might already be true
                    self.confirmed.valueHasMutated(); // if so, we still want to fire the subscription
                } else {
                    self.confirmed(true);
                }
            } else {
                self.phone.isValid.notifySubscribers(false);
            }
        }, 0);
    }

    if (typeof additionalValidation != 'undefined') {
        additionalValidation.apply(self, []);
    }

    // Whether or not this particular address configuration has been validated
    // Set to false when address components are changed, because revalidation is necessary at that point
    self.confirmed = ko.observable(false);

    self.confirmed.subscribe(function (newValue) {
        //console.log('confirmed', newValue);
    })

    // #endregion


    // #region subscriptions

    self.countyListOrText.subscribe(function (newValue) {
        //console.log('checkoutDeliveryAddress countyListOrText subscribe hit', newValue);
        // true == county list, false == textInput
        if (newValue) {
            //self.countyFromList(undefined);
        } else {
            self.county(undefined);
        }
    });

    self.availableCounties.subscribe(function (newValues) {
        //console.log('checkoutDeliveryAddress availableCounties subscribe hit', newValues);
        if (newValues.length) {
            self.county(undefined);
            self.countyListOrText(true);
        } else {
            //self.countyFromList(undefined);
            self.countyListOrText(false);
        }
    });

    self.county.subscribe(function (newValue) {
        //console.log('checkoutDeliveryAddress county subscribe hit', newValue);
    });

    self.deliveryOptionId.subscribe(function (newValue) {
        //console.log('checkoutDeliveryAddress deliveryOptionId subscribe hit', newValue);
    });

    self.confirmed.subscribe(function (newValue) {
        //console.log('checkoutDeliveryAddress confirmed subscribe hit', newValue);

        if (newValue) {

            if ($(self.phone.DOMElement).intlTelInput('getInstance').length) {
                self.phone($(self.phone.DOMElement).intlTelInput("getNumber"));
            }

            if (self.isUPSIdAddress == false) {
                self.parent.saveDeliveryAddress(self);
            } else {
                self.parent.saveIdentificationAddress(self);
            }
        } else {
            self.validateNow(false);
        }

        //if (newValue) { 020 8 679 2600
        //    self.parent.saveDeliveryAddress(self);
        //} else {
        //    self.parent.removeDeliveryAddress();
        //}
    });

    self.country.subscribe(function (newValue) {
        //console.log('checkoutDeliveryAddress country subscribe hit', newValue);
        if (newValue) {
            var counties = getAvailableCounties(newValue.Code);
            self.availableCounties(counties);
        } else {
            self.availableCounties([]);
        }
        self.PCACountry(newValue);
    });

    self.fullName.subscribe(function (newValue) {
        self.confirmed(false);
    });

    self.companyName.subscribe(function (newValue) {
        //console.log('checkoutDeliveryAddress companyName subscribe hit', newValue);
        self.confirmed(false);
    });

    self.addressLine1.subscribe(function (newValue) {
        //console.log('checkoutDeliveryAddress addressLine1 subscribe hit', newValue);
        self.confirmed(false);
    });

    self.addressLine2.subscribe(function (newValue) {
        //console.log('checkoutDeliveryAddress addressLine2 subscribe hit', newValue);
        self.confirmed(false);
    });

    self.town.subscribe(function (newValue) {
        //console.log('checkoutDeliveryAddress town subscribe hit', newValue);
        self.confirmed(false);
    });

    self.county.subscribe(function (newValue) {
        //console.log('checkoutDeliveryAddress county subscribe hit', newValue);
        self.confirmed(false);
    });

    //self.countyFromList.subscribe(function (newValue) {
    //    self.confirmed(false);
    //});

    self.postcode.subscribe(function (newValue) {
        //console.log('checkoutDeliveryAddress postcode subscribe hit', newValue);

        let disableCountry = false;

        if (newValue) {
            if (newValue.toString().trim().toLowerCase().indexOf('je') == 0) {
                var j = Enumerable.From(self.availableCountries()).Where('$.CountryID == 115').SingleOrDefault();
                if (j) {
                    self.country.skipSubscription = true;
                    self.country(j);
                    disableCountry = true;
                }
            }
        }
        if (disableCountry) {
            $('select[name="delivery-country"]').attr('disabled', true);
        } else {
            $('select[name="delivery-country"]').removeAttr('disabled');
        }


        self.confirmed(false);
    });

    self.country.subscribe(function (newValue) {
        if (self.country.skipSubscription) {
            self.country.skipSubscription = false;
            self.confirmed(false);
        } else {
            //console.log('checkoutDeliveryAddress country subscribe hit', newValue);
            self.confirmed(false);
            self.addressLine1(undefined);
            self.addressLine2(undefined);
            self.town(undefined);
            self.county(undefined);
            //self.countyFromList(undefined);
            self.postcode(undefined);
        }
        self.errors.showAllMessages(false);
    });

    //self.phone.subscribe(function (newValue) {
    //    if (self.confirmed()) {
    //        self.confirmed(false);
    //    }
    //});

    // #endregion


    // #region computeds

    self.displayLines = ko.computed(function () {
        var result = [];
        if (ko.unwrap(self.fullName))
            result.push(ko.unwrap(self.fullName));
        if (ko.unwrap(self.companyName))
            result.push(ko.unwrap(self.companyName));
        if (ko.unwrap(self.addressLine1))
            result.push(ko.unwrap(self.addressLine1));
        if (ko.unwrap(self.addressLine2))
            result.push(ko.unwrap(self.addressLine2));
        if (ko.unwrap(self.town))
            result.push(ko.unwrap(self.town));
        if (ko.unwrap(self.county))
            result.push(ko.unwrap(self.county));
        else if (ko.unwrap(self.countyFromList))
            result.push(ko.unwrap(self.countyFromList));
        if (ko.unwrap(self.postcode))
            result.push(ko.unwrap(self.postcode));
        if (ko.unwrap(self.country))
            result.push(ko.unwrap(self.country).Name);
        if (ko.unwrap(self.phone) && !self.phoneChanged()) {
            result.push('<span style="display: block; margin-top: -1em;"></span>');
            result.push(self.resources.Tel + ': ' + ko.unwrap(self.phone));
        }
        return result.join('<br/>');
    });

    self.logLines = ko.computed(function () {
        var result = [];
        if (ko.unwrap(self.fullName))
            result.push(ko.unwrap(self.fullName));
        if (ko.unwrap(self.companyName))
            result.push(ko.unwrap(self.companyName));
        if (ko.unwrap(self.addressLine1))
            result.push(ko.unwrap(self.addressLine1));
        if (ko.unwrap(self.addressLine2))
            result.push(ko.unwrap(self.addressLine2));
        if (ko.unwrap(self.town))
            result.push(ko.unwrap(self.town));
        if (ko.unwrap(self.county))
            result.push(ko.unwrap(self.county));
        //else if (ko.unwrap(self.countyFromList))
        //    result.push(ko.unwrap(self.countyFromList));
        if (ko.unwrap(self.postcode))
            result.push(ko.unwrap(self.postcode));
        if (ko.unwrap(self.country))
            result.push(ko.unwrap(self.country).Name);
        //if (ko.unwrap(self.phone)) {
        //    result.push('');
        //    result.push(ko.unwrap(self.phone));
        //}
        return result.join(', ');
    });

    // #endregion


    // #region components


    // #endregion


    // #region functions

    self.addressChanged = function () {
        self.confirmed(false);
    }

    self.initPCAAutocomplete = function () {
        $(self.PCASearchTerm.DOMElement).autocomplete({
            appendTo: 'body',
            position: { my: "right top", at: "right bottom", of: $('.ui-front'), collision: "none" },
            source: function (request, response) {
                $.ajax({
                    url: "/api/member/addressLookupSearch",
                    type: "POST",
                    dataType: "json",
                    data: {
                        id: self.PCASearchId(),
                        text: self.PCASearchTerm(),
                        container: self.PCAContainer(),
                        countries: self.PCACountry().Code,
                        origin: self.PCACountry().Code
                    },
                    success: function (data) {
                        $(self.PCASearchTerm.DOMElement).removeClass('ui-autocomplete-loading');
                        if (self.PCASearchId() != null) {
                            if (data.length == 1) {

                                // COUNTRY FIRST because it clears other address fields on change
                                // it is noted that Welsh addresses might not return Country detail
                                if (data[0].CountryIso2) {
                                    var c = Enumerable.From(self.availableCountries()).Where(function (x) {
                                        return x.Code == data[0].CountryIso2
                                    }).SingleOrDefault();

                                    if (c) {
                                        self.PCACountry(c);
                                        self.country(c);
                                    } else {
                                        // COUNTRY ERROR MUST HANDLE!
                                    }
                                }

                                var county = data[0].Province || data[0].ProvinceName || data[0].ProvinceCode || data[0].AdminAreaName || data[0].AdminAreaCode;

                                if (data[0].City === "London" && self.PCACountry() == "GB")
                                    county = "London";

                                self.deliveryOptionId(1);
                                self.companyName(data[0].Company);
                                self.addressLine1(data[0].Line1);
                                self.addressLine2(data[0].Line2);
                                self.town(data[0].City);

                                //console.log('PCASearch select', data[0]);

                                //var countyFromList = Enumerable.From(self.availableCounties()).Where(function (x) {
                                //    if (self.PCACountry() == "US") {
                                //        if (x.County == county)
                                //            return x;
                                //    } else {
                                //        if (x.CountyName == county)
                                //            return x;
                                //    }
                                //}).SingleOrDefault();

                                //if (countyFromList) {
                                //    self.countyListOrText(true);
                                //    self.countyFromList(county);
                                //    self.county(undefined);
                                //}
                                //else {
                                self.countyListOrText(false);
                                //self.countyFromList(undefined);
                                self.county(county);
                                //}
                                self.postcode(data[0].PostalCode);

                                $(self.PCASearchTerm.DOMElement).autocomplete("close");

                                self.PCASearchId(null);
                                self.PCAContainer(null);
                                $(self.PCASearchTerm.DOMElement).val(undefined);

                                $(self.fullName.DOMElement).focus();

                                return false;
                            } else {

                                response($.map(data, function (item) {
                                    var county = item.Province || item.ProvinceName || item.ProvinceCode || item.AdminAreaName || item.AdminAreaCode;

                                    if (item.City === "London" && self.PCACountry().Code == "GB")
                                        county = "London";

                                    return {
                                        customCatch: true,
                                        label: item.Label,
                                        value: item.Id,
                                        companyName: item.Company,
                                        addressLine1: item.Line1,
                                        addressLine2: item.Line2,
                                        town: item.City,
                                        county: county,
                                        postcode: item.PostalCode,
                                        country: {
                                            name: item.Country || item.CountryName,
                                            countryIso2: item.CountryIso2
                                        }
                                    }
                                }));
                                return false;
                            }
                        }

                        self.PCASearchId(null);
                        self.PCAContainer(null);

                        response($.map(data, function (item, index) {
                            if (item.hasOwnProperty('Type')) {
                                if ((item.Type === "Address") || (item.Type === "Residential")) {
                                    return {
                                        id: item.Id,
                                        label: item.Text + ", " + item.Description,
                                        value: item.Id,
                                        type: item.Type,
                                        text: item.Text,
                                        index: index
                                        // highlight: item.Highlight
                                    };
                                } else {
                                    return {
                                        id: item.Id,
                                        label: item.Description,
                                        value: item.Id,
                                        type: item.Type,
                                        text: item.Text,
                                        index: index
                                        // highlight: item.Highlight
                                    };
                                }
                            } else {
                                return {
                                    label: item.Label,
                                    value: item.Id,
                                    index: index
                                }
                            }
                        }));
                    },
                    error: function (xhr, textStatus, errorThrown) {
                        $(self.PCASearchTerm.DOMElement).removeClass('ui-autocomplete-loading');
                    }
                });
            },
            minLength: 1,
            select: function (event, ui) {

                event.stopPropagation();

                if (ui.item.hasOwnProperty('type')) {
                    switch (ui.item.type) {
                        case 'Address':
                        case 'Residential':
                            jQuery.extend(event.originalEvent, { keepOpen: true });
                            self.PCASearchId(ui.item.id);
                            self.PCAContainer(null);
                            break;
                        default:
                            jQuery.extend(event.originalEvent, { keepOpen: true });
                            //modify value as required
                            self.PCASearchId(null);
                            self.PCAContainer(ui.item.id);
                            return false;
                    }
                } else if (ui.item.hasOwnProperty('customCatch')) {

                    //console.log('PCASearch selected', ui.item)

                    // COUNTRY FIRST because it clears other address fields on change
                    if (ui.item.hasOwnProperty('country')) {
                        var c = Enumerable.From(self.availableCountries()).Where(function (x) {
                            return x.Code == ui.item.country.countryIso2
                        }).SingleOrDefault();

                        if (c) {
                            self.PCACountry(c);
                            self.country(c);
                        } else {
                            // COUNTRY ERROR MUST HANDLE!
                        }
                    }

                    self.deliveryOptionId(1);
                    self.companyName(ui.item.companyName);
                    self.addressLine1(ui.item.addressLine1);
                    self.addressLine2(ui.item.addressLine2);
                    self.town(ui.item.town);
                    self.postcode(ui.item.postcode);

                    var countyFromList = Enumerable.From(self.availableCounties()).Where(function (x) {
                        if (self.PCACountry() == "US") {
                            if (x.County == ui.item.county)
                                return x;
                        } else {
                            if (x.CountyName == ui.item.county)
                                return x;
                        }
                    }).SingleOrDefault();

                    if (countyFromList) {
                        self.county(undefined);
                        self.countyListOrText(true);
                        self.countyFromList(countyFromList);
                    }
                    else {
                        self.countyFromList(undefined);
                        self.countyListOrText(false);
                        self.county(ui.item.county);
                    }


                    $(self.PCASearchTerm.DOMElement).autocomplete("close");
                    $(self.PCASearchTerm.DOMElement).val(undefined);
                    self.PCASearchId(undefined);
                    self.PCAContainer(undefined);

                    $(self.fullName.DOMElement).focus();

                }

                return false;
            },
            focus: function (event, ui) {
                event.preventDefault();
                return true;
            },
            autoFocus: true,
            open: function (event, ui) {
                $(self.PCASearchTerm.DOMElement).addClass('ui-autocomplete-open');
            },
            close: function (event, ui) {
                $(self.PCASearchTerm.DOMElement).removeClass('ui-autocomplete-open');
            }
        });

        $(self.PCASearchTerm.DOMElement).data("ui-autocomplete")._resizeMenu = function () {
            this.menu.element.css('width', 'auto');
        }

        $(self.PCASearchTerm.DOMElement).data("ui-autocomplete")._renderItem = function (ul, item) {
            return $('<li class="ui-menu-item">')
                .attr("data-value", item.value)
                .append('<div class="ui-menu-item-wrapper">' + item.label + '</div>')
                .appendTo(ul);
        }

        $(self.PCASearchTerm.DOMElement).data("ui-autocomplete")._renderMenu = function (ul, items) {
            var that = this;
            $.each(items, function (index, item) {
                that._renderItemData(ul, item);
            });
            //$(ul).addClass("list-group");
            $(ul).addClass("address-autocomplete");
        };
    };

    self.initLateExtenders = function (data) {
        //console.log('checkoutDeliveryAddress initLateExtenders', data);
        if (self.PCASearchTerm.DOMElement) {
            //console.log('address.PCASearchTerm had DOMElement: initialising PCA');
            self.initPCAAutocomplete();
        } else {
            //console.log('address.PCASearchTerm had no DOMElement: cannot initialise PCA');
        }

        $('input[name="delivery-phone"]').intlTelInput({
            //utilsScript: '/Scripts/int-tel/utils.min.js',
            dropdownContainer: document.body,
            initialCountry: self.country().Code,
            preferredCountries: []
        });

        //$('input[name="delivery-phone"]').on("countrychange", function () {
        //    self.phoneChanged(true);
        //    // do something with iti.getSelectedCountryData()
        //});

        // validateNow(true) must be called from the paypal route - it appears to be being called from the Amazon direction, but... but not PayPal.
        // No validation present on PayPal.

        self.phone.extend({
            validation: {
                validator: function (val, someOtherVal) {
                    // getNumberType
                    //intlTelInputUtils.numberType.MOBILE
                    //intlTelInputUtils.numberType.FIXED_LINE_OR_MOBILE
                    if (self.validateNow()) {
                        if (val) {
                            if (!$(self.phone.DOMElement).intlTelInput("isValidNumber")) {
                                return false;
                            }
                            var type = $(self.phone.DOMElement).intlTelInput("getNumberType");
                            if (type != intlTelInputUtils.numberType.MOBILE &&
                                type != intlTelInputUtils.numberType.FIXED_LINE_OR_MOBILE) {
                                return false;
                            }
                        }
                    }
                    return true;
                },
                message: self.resources.mobileNumberInvalid
            }
        });

        self.phone.subscribe(function (newValue) {
            if (self.parent.parent.checkoutType == 3 || self.parent.parent.checkoutType == 4) {
                self.phoneChanged(true);
            }
        });

        return true;
    }

    self.confirm = function () {
        //console.log('checkoutDeliveryAddress confirm');

        self.confirmed(false);
        self.parent.parent.busy(false);

        window.setTimeout(function () {


            if (getCheckoutType() == 3) {
                //console.log('checkoutDeliveryAddress confirm: skipping address validation (AMAZON) valid address');
                self.parent.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'checkoutDeliveryAddress confirm: skipping address validation (AMAZON) valid address');
                self.confirmed(true);
            } else {
                self.validateNow(true);

                self.errors.showAllMessages(false);

                var poboxErrors = false;
                if (typeof self.poboxErrors !== 'undefined') {
                    if (self.poboxErrors().length) {
                        poboxErrors = true;
                        self.poboxErrors.showAllMessages(true);
                    }
                }

                if (self.errors().length || poboxErrors) {
                    //console.log('checkoutDeliveryAddress confirm: invalid address')
                    self.errors.showAllMessages(true);

                    self.errors.filter(x => {
                        x.isValid.notifySubscribers(x.isValid());
                    })

                    self.parent.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'current checkoutAddress failed validation - ' + self.errors().join(', '));
                } else {
                    //console.log('checkoutDeliveryAddress confirm: valid address')
                    self.confirmed(true);
                }
            }
        }, 0);
    }

    // #endregion


    // #region errors and validation

    self.errors = ko.validation.group([
        self.fullName,
        self.companyName,
        self.addressLine1,
        self.addressLine2,
        self.town,
        self.county,
        self.countyFromList,
        self.postcode,
        self.country,
        self.phone
    ]);

    // #endregion

    self.reset = function () {
        //console.log('checkoutDeliveryAddress reset');

        self.deliveryOptionId(undefined);

        self.memberAddressId(undefined);

        self.fullName(undefined);
        self.companyName(undefined);
        self.addressLine1(undefined);
        self.addressLine2(undefined);
        self.town(undefined);
        self.county(undefined);
        self.countyFromList(undefined);
        self.postcode(undefined);
        self.country(undefined);
        self.phone(undefined);

        self.salesTaxAddressFromList(undefined);
        self.salesTaxAddresses([]);

        self.validateNow(false);
    }

    return self;
};

const paymentMethodFactory = ((parent) => {
    let self = {
        parent: parent,
        resources: parent.resources
    };
    self.parent = parent;

    // #region properties

    // Descriptor comment for each property to follow pattern:
    // What (is it)
    // Where (is it used)
    // Why (does it exist)

    // Primary Braintree HostedFields instance
    // Payment Partial
    // Because we need to hold reference to BT and teardown / recreate as necessary
    self.braintreeInstance = null;

    // Braintree HostedFields Instance - Complete card entry
    // Payment Partial
    // Because we need to hold reference to BT and teardown / recreate as necessary
    self.braintreeFullInstance = null;

    // Braintree HostedFields Instance - CVV validation specific
    // Payment Partial
    // Because for vaulted cards we only want to verify CVV, and need to hold a reference to the pertinent BT instance
    self.braintreeCVVInstance = null;

    // Site country
    // Throughout this model
    // Because we need to pass it up to numerous AJAX calls
    self.siteCountry = getSiteCountry();

    // The last card type logged
    // On change of card type in braintree instance
    // Because we log card types to database
    self.loggedCardType = null;

    // Amazon wallet widget
    self.amazonWalletWidget = null;

    // Amazon address book widget
    self.amazonAddressBookWidget = null;


    // #endregion











    // #region observables

    // Container to hold whether or not a payment method has been selected (and possibly what it is, too)
    // Unknown 
    // Unknown
    self.paymentMethodSelected = ko.observable().extend({
        equal: {
            params: true,
            message: self.resources.CheckoutErrorNoPaymentMethod
        }
    });

    // Boolean to indicate whether or not a purchase is allowed
    // Unknown
    // Unknown
    self.purchaseAllowed = ko.observable(false);

    // Something indicating whether or not the model should validate?
    // Unknown
    // Unknown
    self.shouldValidate = ko.observable(true);

    // Something indicating whether or not payment... can, or should? be overridden?! why the...
    // Unknown
    // Unknown... This is *weird*. Why does it exist?
    self.overridePayment = ko.observable();

    // Boolean to hold whether or not we want to save provided card details to vault
    // Payment Partial
    // Because we offer the ability to vault cards or not as user desires
    self.saveCardDetails = ko.observable(self.parent.isMemberCheckout() ? true : false);

    // Boolean to hold whether or not we want to save provided card detail as default
    // Payment Partial
    // Because we offer the ability to specify default card
    self.makeCardDefault = ko.observable(true);

    // An array holding all saved cards
    // Payment partial
    // Because we need to know about the cards a member has saved
    self.cards = ko.observableArray(getSavedCards());

    // Container holding selected card
    // Payment partial (Vaulted cards... and non-vaulted card perhaps?)
    // Because we need to know which card is selected
    self.selectedCard = ko.observable();

    // Container for whether or not a valid CVV has been entered for the selected vaulted card
    // Payment partial (vaulted cards)
    // Because we need to know if the CVV entered is valid for the selected vaulted card?
    self.validVaultCVVEntered = ko.observable(true);

    // Container for billing fullName; might differ from delivery name and we do want to capture it even if BT doesn't
    // Payment partial (full card input)
    // Because we want to provide something more than just card details and postal code...? 
    self.billingFullName = ko.observable();

    // Amazon order ref
    self.amazonOrderReferenceId = ko.observable();

    // 
    self.amazonPaymentSelectHit = ko.observable(false);

    // whether or not Amazon is happy
    self.amazonHappy = ko.observable(false).extend({
        equal: {
            params: true,
            onlyIf: function () {
                return self.parent.checkoutType === 3;
            },
            message: self.resources.CheckoutErrorNoPaymentMethod
        }
    });

    self.amazonHappy.subscribe(function (newValue) {
        //console.log('paymentMethodFactory amazonHappy subscribe', newValue);
    });

    // Whether or not we've had a successful response from setOrderReferenceDetails
    self.amazonValueUpdated = ko.observable(false);

    self.amazonValueUpdated.subscribe(function (newValue) {
        //console.log('paymentMethodFactory amazonValueUpdated subscribe', newValue);
    });

    // Whether or not the Add Card modal should display its blocker
    self.addCardBusy = ko.observable(false);

    // #endregion








    // #region subscriptions

    // this is necessary because in the case where you load the page with a default address selected
    // and a payment method selected
    // and the delivery options preselected for you 
    // no change occurs such as to necessitate the call to updateAmazonValue (which in turn calls SetOrderReferenceDetails)
    //self.amazonOrderReferenceId.subscribe(function (newValue) {
    //    //console.log('paymentMethodFactory amazonOrderReferenceId subscribe', newValue);
    //    if (newValue) {
    //        self.updateAmazonValue();
    //    }
    //});

    // Hook to make sure "default card" is selected when "save card" is selected, and vice-versa 
    // Payment partial
    // Because when a user opts to save a new card, it might be useful to also default it to being their default.
    self.saveCardDetails.subscribe(function (newValue) {
        //console.log('paymentMethodFactory saveCardDetails subscribe', newValue);
        var defaultCardLabel = $('#lblMakeCardDefault');
        if (newValue) {
            self.makeCardDefault(true);
            defaultCardLabel.show();
        }
        else {
            self.makeCardDefault(false);
            defaultCardLabel.hide();
        }
    });

    // Watch
    // Here
    // Debugging
    self.makeCardDefault.subscribe(function (newValue) {
        //console.log('paymentMethodFactory makeCardDefault subscribe', newValue);
    });

    // Watch
    // Here
    // Debugging
    self.paymentMethodSelected.subscribe(function (newValue) {
        //console.log('paymentMethodFactory paymentMethodSelected subscribe', newValue);
    });

    // Watch
    // Here
    // Debugging
    self.purchaseAllowed.subscribe(function (newValue) {
        //console.log('paymentMethodFactory purchaseAllowed subscribe', newValue);
    });

    // Hook to watch selectedCard change
    // Here
    // Debugging
    self.selectedCard.subscribe(function (newValue) {
        //console.log('paymentMethodFactory selectedCard subscribe', newValue);
        self.paymentMethodSelected(newValue);
        //localStorage.setItem('selectedCard', newValue);
        if (self.parent.checkoutType == 0) {
            if (newValue !== null && newValue !== undefined) {
                var card = self.getSelectedCardDetails()
                if (card.vaultToken() !== '') {
                    self.validVaultCVVEntered(false);
                    self.loadBraintreeCVVValidation();
                }
                else {
                    self.validVaultCVVEntered(true);
                }
            }
        }
    }
    );

    // #endregion

    // #region errors and validation



    // #endregion

    // #region computeds

    self.toPay = function () {
        var toPay = 0;

        if (self.overridePayment())
            toPay = self.overridePayment();
        else
            toPay = self.parent.balanceNumber(); // safe; this value must match that which is sent by the server for authorisation and capture

        return toPay;
    }

    // #endregion

    // #region components


    // #endregion

    // #region functions

    self.loadPaymentMethods = function () {
        //console.log('paymentMethodFactory loadPaymentMethods');
    }

    self.loadBraintreeCVVValidation = function () {

        //console.log('loadBraintreeCVVValidation called');

        if (self.braintreeCVVInstance) {
            self.braintreeCVVInstance.teardown();
        }

        $('#cvv-validation-wrapper').hide();
        $("#cvv-validation-loading-wrapper").show();

        braintree.hostedFields.create({
            client: self.braintreeInstance,
            styles: {
                'input': {
                    'font-family': 'Verdana',
                    'font-weight': 'normal',
                    'color': '#666',
                    'transition': 'color 100ms linear',
                    '-webkit-transition': 'color 100ms linear'
                },
                '.invalid': {
                    'color': 'red'
                },
                '.number': {
                },
                '.cvv': {
                }
            },
            fields: {
                cvv: {
                    selector: '#cvv-validation',
                    placeholder: self.resources.CheckoutBraintreeCVV
                }
            }
        },
            function (hostedFieldsErr, hostedFieldsInstance) {
                if (hostedFieldsErr) {
                    self.parent.checkoutLog(LOG_ACTION_PAY_NOW_VALIDATION_ERROR, 'CVV validation error - ' + hostedFieldsErr);
                    $('#payment-processing-failure').modal('show');
                    return;
                }

                self.braintreeCVVInstance = hostedFieldsInstance;

                $('#cvv-validation-loading-wrapper').hide();
                $('#cvv-validation-wrapper').show();
            });
    }

    self.init = function () {
        //console.log('paymentMethodFactory init');
        //if (window.amazon) {
        //    self.initialiseAmazonPaymentPartial();
        //}
    }

    self.reset = function () {
        //console.log('paymentMethodFactory reset');

        self.paymentMethodSelected(undefined);
        self.purchaseAllowed(false);
    }

    self.showPaymentProcessingError = function () {
        self.hideCompletingMessage();
        window.scrollTo(0, document.querySelector('#checkout-pay-region').offsetTop);
        window.setTimeout(() => {
            $("#payment-processing-error").modal('show');
        }, 300);
    }

    self.showPaymentProcessingErrorNoDetails = function () {
        self.hideCompletingMessage();
        window.scrollTo(0, document.querySelector('#checkout-pay-region').offsetTop);
        window.setTimeout(() => {
            $("#payment-processing-error-no-details").modal('show');
        }, 300);
    }

    self.hidePaymentProcessingError = function () {
        $("#payment-processing-error").modal('hide');
    }

    // #endregion

    self.complete = function () {

        //console.log('paymentMethodFactory complete');

        try {
            switch (self.parent.checkoutType) {
                case 0:
                case 1:

                    var selectedCard = self.getSelectedCardDetails();

                    if (selectedCard) {

                        self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory complete Mark i: selectedCard = ' + JSON.stringify(selectedCard));

                        //console.log('paymentMethodFactory complete Mark i: selectedCard', selectedCard);

                        if (self.parent.balanceNumber()) {
                            if (selectedCard.vaultToken() !== '') {
                                self.validateCVV();
                            }
                            else {
                                self.showCompletingMessage();

                                if (true) // ? threeDSecureRequired should... always be true, should it not?
                                {
                                    self.completePayment(selectedCard.token, selectedCard.bin);
                                }
                                else {
                                    self.submitBraintreeCheckout('card');
                                }
                            }
                        } else {
                            self.showCompletingMessage();
                            self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory complete Mark iii: balanceNumber was false, calling asyncPlaceOrderNow');
                            self.asyncPlaceOrderNow();
                        }
                    } else {
                        //console.log('paymentMethodFactory complete Mark ii: selectedCard was null');

                        self.showCompletingMessage();

                        if (self.parent.balanceNumber()) {

                            self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory complete Mark ii: selectedCard was null, calling braintreeFullInstance.tokenize');

                            $('#braintree-pay-button').attr('disabled', true);
                            //event.preventDefault();

                            var saveCardDetails = false;

                            if ($('#chkSaveCardForFuture').length) {
                                saveCardDetails = $('#chkSaveCardForFuture').prop('checked');
                            }

                            self.braintreeFullInstance.tokenize(
                                {
                                    vault: saveCardDetails
                                },
                                function (tokenizeErr, payload) {
                                    if (tokenizeErr) {
                                        var errorString = 'paymentMethodFactory braintreeFullInstance.tokenize error = ' + tokenizeErr.message;
                                        if (tokenizeErr.hasOwnProperty('details')) {
                                            if (tokenizeErr.details) {
                                                if (tokenizeErr.details.hasOwnProperty('invalidFieldKeys'))
                                                    errorString += ' keys: ' + tokenizeErr.details.invalidFieldKeys.reduce(function (acc, curr) {
                                                        return acc + ', ' + curr;
                                                    });
                                            }
                                        }
                                        self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, errorString);
                                        $('#braintree-pay-button').attr('disabled', false);
                                        if (tokenizeErr.code === "HOSTED_FIELDS_FIELDS_INVALID") {
                                            $.each(tokenizeErr.details.invalidFields, function (i, field) {
                                                $(field).addClass('braintree-hosted-fields-invalid')
                                            })
                                            self.showPaymentProcessingError();
                                        } else if (tokenizeErr.code === "HOSTED_FIELDS_FIELDS_EMPTY") {
                                            $("#braintree-checkout-credit-card-wrapper div.braintree-input-wrapper").addClass('braintree-hosted-fields-invalid');
                                            self.showPaymentProcessingErrorNoDetails();
                                        }
                                        else {
                                            $("#card-vaulting-failure").modal('show');
                                        }

                                        $("#pay-button-blocker").hide();
                                        self.notifyError(tokenizeErr);
                                        return;
                                    }

                                    self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory braintreeFullInstance.tokenize succeeded');

                                    var saveCardToDB = !saveCardDetails;

                                    self.addCard(payload.details, payload.nonce, payload.details.bin, saveCardToDB);

                                    self.completePayment(payload.nonce, payload.details.bin);
                                }
                            );
                        } else {
                            self.showCompletingMessage();

                            self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory complete Mark iii: balanceNumber was false, calling asyncPlaceOrderNow');

                            self.asyncPlaceOrderNow();
                        }
                    }

                    break;
                case 2:
                    self.parent.tradeErrors.showAllMessages(false);
                    self.parent.tradeValidateNow(true);
                    if (self.parent.tradeErrors().length) {
                        self.parent.tradeErrors.showAllMessages(true);
                        self.parent.tradeErrors.filter(function (x) {
                            x.isValid.notifySubscribers(x.isValid());
                        })
                        self.showCheckoutErrors(self.parent.tradeErrors());
                    }
                    else {
                        self.showCompletingMessage();
                        self.asyncPlaceOrderNow();
                    }
                    break;
                case 3:
                    self.parent.deliveryAddressFactory.currentAddress().validateNow(true);

                    if (self.parent.deliveryAddressFactory.currentAddress().phone.isValid() &&
                        self.parent.deliveryAddressFactory.currentAddress().phoneChanged() == false) {
                        if (self.parent.balanceNumber()) {
                            self.showCompletingMessage();
                            self.submitAmazonCheckout();
                        } else {
                            self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory complete Mark iii: balanceNumber was false, calling asyncPlaceOrderNow');
                            self.asyncPlaceOrderNow();
                        }
                    } else {
                        if (self.parent.deliveryAddressFactory.currentAddress().phone.isValid()) {
                            // you can get here by not having a mobile number, providing one and then not saving the number.
                            // in which case you're left with no red visual prompt to action, just the "You have unsaved changes" dialog.
                            document.querySelector('.phone-unsaved-changes').classList.add('alert');
                            document.querySelector('.phone-unsaved-changes').classList.add('alert-danger');
                            document.querySelector('.phone-unsaved-changes').classList.remove('error-pulse');
                            window.setTimeout(() => {
                                document.querySelector('.phone-unsaved-changes').classList.add('error-pulse');
                            }, 0);
                        }
                        self.parent.deliveryAddressFactory.currentAddress().errors.showAllMessages(true);
                        self.parent.deliveryAddressFactory.currentAddress().phone.isValid.notifySubscribers(false);
                        if (window.isMobileSite()) {
                            window.scrollTo(0, 0);
                        }
                    }

                    break;
                case 4:
                    self.parent.deliveryAddressFactory.currentAddress().validateNow(true);

                    if (self.parent.deliveryAddressFactory.currentAddress().phone.isValid() &&
                        self.parent.deliveryAddressFactory.currentAddress().phoneChanged() == false) {
                        if (self.parent.balanceNumber()) {
                            self.showCompletingMessage();
                            self.submitBraintreeCheckout('paypal');
                        } else {
                            self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory complete Mark iii: balanceNumber was false, calling asyncPlaceOrderNow');
                            self.asyncPlaceOrderNow();
                        }
                    } else {
                        if (self.parent.deliveryAddressFactory.currentAddress().phone.isValid()) {
                            // you can get here by not having a mobile number, providing one and then not saving the number.
                            // in which case you're left with no red visual prompt to action, just the "You have unsaved changes" dialog.
                            document.querySelector('.phone-unsaved-changes').classList.add('alert');
                            document.querySelector('.phone-unsaved-changes').classList.add('alert-danger');
                            document.querySelector('.phone-unsaved-changes').classList.remove('error-pulse');
                            window.setTimeout(() => {
                                document.querySelector('.phone-unsaved-changes').classList.add('error-pulse');
                            }, 0);
                        }
                        self.parent.deliveryAddressFactory.currentAddress().errors.showAllMessages(true);
                        self.parent.deliveryAddressFactory.currentAddress().phone.isValid.notifySubscribers(false);
                        if (window.isMobileSite()) {
                            window.scrollTo(0, 0);
                        }
                    }
                    break;
                //case 5:
                //    self.showCompletingMessage();

                //    self.applePaymentRequest = self.applePayInstance.createPaymentRequest({
                //        total: {
                //            label: 'Designers Guild',
                //            type: 'pending',
                //            amount: getBalanceNumber(),
                //            lineItems: [{
                //                label: 'Sub Total',
                //                type: 'final',
                //                amount: getItemSubTotalNumber()
                //            }, {
                //                label: 'Delivery',
                //                type: 'final',
                //                amount: getShippingNumber()
                //            }, {
                //                label: 'Tax',
                //                type: 'final',
                //                amount: getTaxNumber()
                //            }]
                //        },

                //        // We recommend collecting billing address information, at minimum
                //        // billing postal code, and passing that billing postal code with
                //        // all Apple Pay transactions as a best practice.
                //        applePayShippingType: 'delivery',
                //        requiredBillingContactFields: ["postalAddress"],
                //        requiredShippingContactFields: [
                //            "email"
                //        ]
                //    });

                //    //self.beginApplePay = () => {

                //    console.log('applePay window.beginApplePay called', self.applePaymentRequest);

                //    self.activeApplePaySession = new ApplePaySession(3, self.applePaymentRequest);

                //    self.activeApplePaySession.onvalidatemerchant = (e) => {
                //        console.log('applePay onvalidatemerchant', e);
                //        self.applePayInstance.performValidation({
                //            validationURL: e.validationURL,
                //            displayName: 'Designers Guild'
                //        }, function (err, merchantSession) {
                //            if (err) {
                //                self.hideCompletingMessage();
                //                window.showError(self.resources.ApplePayFailedToLoad);
                //                // You should show an error to the user, e.g. 'Apple Pay failed to load.'
                //                return;
                //            }
                //            console.log('applePay validatedmerchant', merchantSession);
                //            self.activeApplePaySession.completeMerchantValidation(merchantSession);
                //        });
                //    };

                //    self.activeApplePaySession.onshippingcontactselected = (e) => {
                //        console.log('applePay onshippingcontactselected', e);

                //        let errors = [];

                //        if (!self.parent.errors().length) {
                //            self.parent.errors().map(e => {
                //                errors.push(new ApplePayError('addressUnserviceable', null, e));
                //            })
                //        }

                //        self.paymentMethodFactory.activeApplePaySession.completeShippingContactSelection({
                //            newTotal: {
                //                type: 'final',
                //                label: 'Designers Guild',
                //                amount: getBalanceNumber()
                //            },
                //            newLineItems: [{
                //                label: 'Sub Total',
                //                type: 'pending',
                //                amount: getItemSubTotalNumber()

                //            }, {
                //                label: 'Delivery',
                //                type: 'pending',
                //                amount: getShippingNumber()

                //            }, {
                //                label: 'Tax',
                //                type: 'pending',
                //                amount: getTaxNumber()
                //            }],
                //            errors: errors
                //        });
                //    }

                //    self.activeApplePaySession.onpaymentauthorized = (e) => {
                //        console.log('applePay onpaymentauthorized', e);

                //        self.applePayInstance.tokenize({
                //            token: e.payment.token
                //        }, function (tokenizeErr, payload) {
                //            if (tokenizeErr) {
                //                self.hideCompletingMessage();
                //                console.error('applePay Error tokenizing:', tokenizeErr);
                //                self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, JSON.stringify(tokenizeErr));
                //                window.showError(self.resources.ApplePayFailureNotCharged);
                //                self.activeApplePaySession.completePayment(ApplePaySession.STATUS_FAILURE);
                //                return;
                //            }

                //            console.log('applePay tokenized', payload);

                //            // Send payload.nonce to your server.
                //            self.submitBraintreeCheckout('apple', payload.nonce, e.payment.shippingContact.emailAddress);

                //            // If requested, address information is accessible in event.payment
                //            // and may also be sent to your server.
                //            console.log('applyPay billingPostalCode:', e.payment.billingContact.postalCode);

                //            // After you have transacted with the payload.nonce,
                //            // call `completePayment` to dismiss the Apple Pay sheet.

                //            self.endApple = () => {
                //                self.activeApplePaySession.completePayment(ApplePaySession.STATUS_SUCCESS);
                //            }
                //        });
                //    };

                //    self.activeApplePaySession.oncancel = function (event) {
                //        self.hideCompletingMessage();
                //    }

                //    self.activeApplePaySession.begin();

                //    break;
            }
        }
        catch (ex) {
            window.setTimeout(() => {
                window.onErrorModalHide.push(() => {
                    self.hideCompletingMessage();
                });
                window.showError();
            }, 300);
            window.logClientSideError(JSON.stringify({
                message: e.toString(),
                source: e.stack,
                url: window.location.href
            }));
        }
    }

    self.validateCVV = function () {
        var state = self.braintreeCVVInstance.getState();
        if (state.fields['cvv'].isValid) {
            self.showCompletingMessage();
            self.braintreeCVVInstance.tokenize(function (tokenizeErr, payload) {
                if (tokenizeErr) {
                    self.hideCompletingMessage();
                    alert(self.resources.MajorError);
                    return;
                }
                else {
                    var selectedCard = self.getSelectedCardDetails();
                    self.asyncCheckCVV(selectedCard.vaultToken(), payload.nonce, payload.details.bin, self.completePayment, self.invalidCVV);
                }
            });
        }
        else {
            self.invalidCVV();
        }
    }

    self.invalidCVV = function () {
        var error = [self.resources.CheckoutErrorCVV];
        self.braintreeCVVInstance.focus('cvv');
        self.showCheckoutErrors(error);
        $('#cvv-validation').addClass('braintree-hosted-fields-invalid');
    }

    self.completePayment = function (nonce, bin) {
        if (true) // again, 3dSecureRequired ?
        {
            self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory completePayment');
            self.verify3DSecure(nonce, bin);
        }
        else {
            self.submitBraintreeCheckout('card');
        }
    }

    self.verify3DSecureInProgress = false;

    self.verify3DSecure = function (nonce, bin) {
        if (threeDSecure) {
            try {
                self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory verify3DSecure, window.threeDSecure was defined');
                var selectedCard = self.getSelectedCardDetails();

                if (self.verify3DSecureInProgress) {
                    threeDSecure.cancelVerifyCard();
                }

                self.verify3DSecureInProgress = true;

                var toPay = self.toPay();

                //console.log('paymentMethodFactory verify 3D Secure called, amount: ', toPay);
                self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory verify3DSecure verifying amount ' + toPay);

                var addr = self.parent.deliveryAddressFactory.currentAddress();

                var name = '';

                if (addr.UPSAPID == null) { // generally we only get business names from UPS APs
                    name = (addr.fullName() ?? '').trim().split(' ');
                } else {
                    addr = self.parent.deliveryAddressFactory.upsIdAddress();
                    name = (addr.fullName() ?? '').trim().split(' ');
                }

                var firstName = '';
                var lastName = '';

                if (name.length == 1) {
                    firstName = name[0]
                }

                if (name.length > 1) {
                    firstName = name[0];
                    lastName = name[name.length - 1];
                }

                var threeDSecureParameters = {
                    nonce: nonce,
                    amount: self.toPay().toString(),
                    bin: bin,
                    email: getEmail(),
                    additionalInformation: {
                        shippingGivenName: firstName,
                        shippingSurname: lastName,
                        shippingPhone: addr.phone(),
                        shippingAddress: {
                            streetAddress: addr.addressLine1(),
                            extendedAddress: addr.addressLine2(),
                            locality: addr.town(),
                            //region: addr.countyFromList() || addr.county(),
                            postalCode: addr.postcode(),
                            countryCodeAlpha2: addr.country().Code
                        }
                    },
                    onLookupComplete: function (data, next) {
                        // use 'data' here...

                        let msg = "";

                        try {
                            msg = JSON.stringify(data);
                        }
                        catch (e) {
                            msg = "failed to stringify data!";
                        }

                        //console.log('threeDSecure onLookupComplete', msg);

                        self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'threeDSecure onLookupComplete', msg);

                        next();
                    }
                };
                //console.log('threeDSecureParameters was', JSON.stringify(threeDSecureParameters));
                threeDSecure.verifyCard(threeDSecureParameters, function (err, payload) {
                    if (err) {

                        let errMsg = "";

                        try {
                            errMsg = JSON.stringify(err);
                        }
                        catch (e) {
                            errMsg = "failed to stringify err!";
                        }

                        self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory verify3DSecure ERROR' + errMsg);

                        $("#payment-processing-error-text").html(self.resources.CheckoutBraintree3DSecureMessage);

                        self.showPaymentProcessingError();

                        self.selectedCard(undefined);

                        return;
                    }

                    //console.log('threeDSecure.verifyCard returned', payload);

                    self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory verify3DSecure succeeded');

                    selectedCard.nonce = payload.nonce;

                    self.submitBraintreeCheckout('card');
                });
            }
            catch (e) {
                window.setTimeout(() => {
                    self.hideCompletingMessage();
                }, 300);
                window.logClientSideError(JSON.stringify({
                    message: e.toString(),
                    source: e.stack,
                    url: window.location.href
                }));
                window.showError();
                throw e;
            }
        }
        else {
            self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory verify3DSecure, window.threeDSecure WAS NOT DEFINED');
            self.submitBraintreeCheckout('card');
        }
    }

    self.getSelectedCardDetails = function () {
        var result = null;
        $.each(self.cards(), function (i, card) {
            if (card.token == self.selectedCard()) {
                result = card;
                return false;
            }
        });
        return result;
    }

    self.removeCard = function (toRemove) {
        window.showConfirm(self.resources.AreYouSure, self.resources.ThisWillRemoveCardFromVault, function () {
            var myCards = self.cards();
            $.each(myCards, function (i, card) {
                if (card.token == toRemove.token) {
                    if (card.token == self.selectedCard()) {
                        self.selectedCard(undefined);
                    }

                    if (toRemove.defaultCard()) {
                        localStorage.removeItem('defaultToken');
                    }

                    self.removeCardFromDb(toRemove);

                    myCards.splice(i, 1);
                    self.cards(myCards);

                    return false;
                }
            });
        }, null)
    }

    self.makeDefaultCard = function (makeDefault) {
        if (self.parent.checkoutType === 0) {
            $.each(self.cards(), function (i, card) {
                card.defaultCard(card.token == makeDefault.token);
                localStorage.setItem('defaultToken', makeDefault.token);
            });
        }
    }

    self.submitBraintreeCheckout = function (method, nonce, email) {

        self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory submitBraintreeCheckout called');

        self.showCompletingMessage();

        var defaultCard = false;

        if (method === 'card') {
            var card = self.getSelectedCardDetails();
            if (card.nonce) {
                nonce = card.nonce;
            }
            else {
                nonce = card.token;
            }
            defaultCard = card.defaultCard();
        }
        else if (method === 'apple') {

        } else {
            nonce = "paypal";
        }

        var data = JSON.stringify({
            device_data: null,
            payment_method_nonce: nonce,
            masterDefinition: getMasterDefinition(),
            basketCountry: self.siteCountry,
            marketingPermissions: ko.toJS(self.parent.marketingPermissions()),
            make_default: defaultCard,
            email: email
        });

        self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory submitBraintreeCheckout submitting: ' + data);

        $.ajax({
            url: '/api/checkout/checkoutBraintreeSubmit',
            type: 'POST',
            cache: false,
            async: true,
            data: data,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {

                console.log('paymentMethodFactory submitBraintreeCheckout returned', data);
                self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory submitBraintreeCheckout returned: ' + JSON.stringify(data));

                if (data != null) {
                    if (data.result === "OK") {
                        if (data.redirect === "confirm") {
                            // defunct and should never be hit
                            // window.location.href = '@dgFramework.buildUrl(Model.masterDefinition.UrlBreakdown.countryDetails, null, dgContentType.landingPage, (int)dgFixedPage.checkoutConfirmation, null, null)';
                        } else if (data.redirect === "orderGen") {
                            $('#checkout-complete-waiting .page-blocker').addClass('success');
                            $('#checkout-complete-waiting p').html(self.resources.CheckoutPaymentSuccessGeneratingOrder);
                            self.showCompletingMessage();
                            if (typeof self.endApple === 'function') {
                                self.endApple();
                            }
                            $.ajax({
                                url: '/api/checkout/checkoutOrderGenerate',
                                type: 'POST',
                                cache: false,
                                async: true,
                                timeout: 30000,
                                data: {},
                                contentType: 'application/json; charset=utf-8',
                                success: function (data) {
                                    window.location.href = '/' + getSiteCountry() + '/l271'; //'@dgFramework.buildUrl(Model.masterDefinition.UrlBreakdown.countryDetails, null, dgContentType.landingPage, (int)dgFixedPage.checkoutConfirmation, null, null)';
                                },
                                error: function (xhr, textStatus, errorThrown) {
                                    window.location.href = '/' + getSiteCountry() + '/l281'; //@dgFramework.buildUrl(Model.masterDefinition.UrlBreakdown.countryDetails, null, dgContentType.landingPage, (int)dgFixedPage.checkoutConfirmationSpartan, null, null)';
                                }
                            });
                        }
                    }
                    else if (data.result === "FAIL") {
                        //console.log("dg-checkout " + data.message);

                        // if guest, unselect card - otherwise potential for 3DS'ing same details regardless of change

                        // if ANYONE, unselect card.
                        self.selectedCard(undefined);
                        // what ramifications does this have for members?

                        //if (self.parent.checkoutType === 1) {
                        //    self.selectedCard(undefined);
                        //}

                        self.hideCompletingMessage();
                        switch (data.redirect) {
                            case "pay":
                                switch (data.message) {
                                    case "3dsecuremessage":
                                        $("#payment-processing-error-text").html(self.resources.CheckoutBraintree3DSecureMessage);
                                        self.showPaymentProcessingError();
                                        break;
                                    default:
                                        $("#payment-processing-error-text").html(data.message);
                                        //$("#payment-processing-error-text").html(self.resources.CheckoutBraintreePaymentDeclined);
                                        self.showPaymentProcessingError();
                                        break;
                                }
                                break;
                            case "basket":
                                window.location.href = '/l201'; //'@dgFramework.buildUrl(Model.masterDefinition.UrlBreakdown.countryDetails, null, dgContentType.landingPage, (int)dgFixedPage.maintainBasket, null, null)';
                                break;
                            default:
                                break;
                        }
                    }
                }
                else {
                    // if guest, unselect card - otherwise potential for 3DS'ing same details regardless of change
                    //if (self.parent.checkoutType === 1) {
                    self.selectedCard(undefined);
                    //}

                    self.hideCompletingMessage();
                    self.showPaymentProcessingError();
                }
            },
            error: function (e, s, a) {

                self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory submitBraintreeCheckout ERRORED: ' + JSON.stringify(e));

                // if guest, unselect card - otherwise potential for 3DS'ing same details regardless of change
                if (self.parent.checkoutType === 1) {
                    self.selectedCard(undefined);
                }
                self.hideCompletingMessage();
                self.showPaymentProcessingError();
                //console.log("dg-checkoutBraintreePay " + s + " " + e.responseText);
            },
            timeout: 90000
        });
    }

    self.removeCardFromDb = function (token) {
        $.ajax({
            url: '/api/checkout/checkoutRemoveCard',
            type: 'POST',
            cache: false,
            async: true,
            data: JSON.stringify({
                vaultToken: token.vaultToken(),
                cardToken: token.token,
                basketCountry: self.siteCountry
            }),
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                if (data.hasOwnProperty('message') && typeof jumpToSessionExpired === 'function') {
                    if (data.message === "checkoutBasketExpired") {
                        jumpToSessionExpired();
                        return;
                    }
                }
            },
            error: function (e, s, a) {
                //console.log("checkoutRemoveCard " + s + " " + e.responseText);
            },
            timeout: 90000
        });
    }

    self.asyncPlaceOrderNow = function () {
        self.showCompletingMessage();

        // this used is used for 0 value orders, e.g. vouchers / promo codes
        $.ajax({
            url: "/api/checkout/getURLforPaidInFull",
            type: "Post",
            cache: false,
            async: true,
            data: JSON.stringify({
                basketCountry: self.siteCountry,
                orderReference: self.parent.orderReference()
            }),
            contentType: 'application/json; charset=utf-8',
            success: function (data) {

                if (data != null) {
                    if (data.result === "OK") {
                        if (data.redirect === "confirm") {
                            // defunct and should never be hit
                            // window.location.href = '@dgFramework.buildUrl(Model.masterDefinition.UrlBreakdown.countryDetails, null, dgContentType.landingPage, (int)dgFixedPage.checkoutConfirmation, null, null)';
                        } else if (data.redirect === "orderGen") {
                            $('#checkout-complete-waiting .page-blocker').addClass('success');
                            if (self.parent.checkoutType === 2) {
                                $('#checkout-complete-waiting p').html(self.resources.CheckoutTradeSuccessGeneratingOrder);
                            } else {
                                $('#checkout-complete-waiting p').html(self.resources.CheckoutPaymentSuccessGeneratingOrder);
                            }
                            $.ajax({
                                url: '/api/checkout/checkoutOrderGenerate',
                                type: 'POST',
                                cache: false,
                                async: true,
                                timeout: 30000,
                                data: {},
                                contentType: 'application/json; charset=utf-8',
                                success: function (data) {
                                    window.location.href = '/' + getSiteCountry() + '/l271'; //'@dgFramework.buildUrl(Model.masterDefinition.UrlBreakdown.countryDetails, null, dgContentType.landingPage, (int)dgFixedPage.checkoutConfirmation, null, null)';
                                },
                                error: function (xhr, textStatus, errorThrown) {
                                    window.location.href = '/' + getSiteCountry() + '/l281'; //@dgFramework.buildUrl(Model.masterDefinition.UrlBreakdown.countryDetails, null, dgContentType.landingPage, (int)dgFixedPage.checkoutConfirmationSpartan, null, null)';
                                }
                            });
                        }
                    }
                    else if (data.result === "FAIL") {
                        //console.log("dg-checkout " + data.message);

                        // if guest, unselect card - otherwise potential for 3DS'ing same details regardless of change

                        // if ANYONE, unselect card.
                        self.selectedCard(undefined);
                        // what ramifications does this have for members?

                        //if (self.parent.checkoutType === 1) {
                        //    self.selectedCard(undefined);
                        //}

                        self.hideCompletingMessage();
                        switch (data.redirect) {
                            case "pay":
                                switch (data.message) {
                                    case "3dsecuremessage":
                                        $("#payment-processing-error-text").html(self.resources.CheckoutBraintree3DSecureMessage);
                                        self.showPaymentProcessingError();
                                        break;
                                    default:
                                        $("#payment-processing-error-text").html(data.message);
                                        //$("#payment-processing-error-text").html(self.resources.CheckoutBraintreePaymentDeclined);
                                        self.showPaymentProcessingError();
                                        break;
                                }
                                break;
                            case "basket":
                                window.location.href = '/l201'; //'@dgFramework.buildUrl(Model.masterDefinition.UrlBreakdown.countryDetails, null, dgContentType.landingPage, (int)dgFixedPage.maintainBasket, null, null)';
                                break;
                            default:
                                break;
                        }
                    }
                }
                else {
                    // if guest, unselect card - otherwise potential for 3DS'ing same details regardless of change
                    //if (self.parent.checkoutType === 1) {
                    self.selectedCard(undefined);
                    //}

                    self.hideCompletingMessage();
                    self.showPaymentProcessingError();
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                self.hideCompletingMessage();
                //console.log('dg-checkoutPay: ' + textStatus);
                alert(self.resources.MajorError);
            }
        });
    }

    // check CVV
    self.asyncCheckCVV = function (token, nonce, bin, onSuccess, onFailure) {
        $.ajax({
            url: '/api/checkout/checkoutCheckCVV',
            type: 'POST',
            cache: false,
            async: true,
            data: JSON.stringify({
                token: token,
                nonce: nonce,
                bin: bin,
                country: self.siteCountry
            }),
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                if (data)
                    onSuccess(data.nonce, data.bin);
                else
                    onFailure();
            },
            error: function () {
                onFailure();
            },
            timeout: 90000
        });
    }

    self.showCompletingMessage = function () {
        console.log('paymentMethodFactory showCompletingMessage');
        bootstrap.Modal.getOrCreateInstance(document.querySelector("#checkout-complete-waiting")).show();
        self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'completing message shown');
    }

    self.hideCompletingMessage = function () {
        console.log('paymentMethodFactory hideCompletingMessage');
        bootstrap.Modal.getOrCreateInstance(document.querySelector("#checkout-complete-waiting")).hide();
        self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'completing message hidden');
    }

    self.scrollToIdentificationAddress = () => {
        document.querySelector('#identification-address-heading').scrollIntoView({
            behavior: 'smooth'
        });
    }

    self.showCheckoutErrors = function (errors) {
        var container = $('#checkout-errors-list');
        container.empty();

        $(errors).each(function (i, error) {
            $("<li>").html("<p class='checkout-info-content'>" + error + "</p>").appendTo(container);
        });

        if (!self.parent.deliveryAddressFactory.identificationAddressConfirmed.isValid()) {
            $('#checkout-errors').one('hide.bs.modal', self.scrollToIdentificationAddress);
        }

        $('#checkout-errors').modal('show');
    }

    self.hideCheckoutErrors = function () {
        $('#checkout-errors').modal('hide');
    }

    self.getDisplayCard = function (card) {
        var cardType = card.cardType + " " + self.resources.CheckoutCardEnding + " " + card.endDigits;
        if (card.isExpired) {
            cardType += " (" + self.resources.CheckoutCardExpired + ")";
        }
        else if (card.expiry !== undefined && card.expiry !== null)
            cardType += " (" + self.resources.CheckoutCardExpires + " " + card.expiry + ")";

        return cardType;
    }

    self.braintreeLog = function (ms, type, detail) {
        $.ajax({
            url: '/api/checkout/asyncSaveBraintreeLog',
            type: 'POST',
            async: false,
            cache: false,
            data: {
                type: type,
                time: ms,
                detail: detail || "no detail provided"
            },
            success: function (data) {
                //console.log("braintree logged: " + JSON.stringify({ type: type, ms: ms, detail: detail }));
            },
            error: function (xhr, s, a) {
                //console.log("braintree error logging failed");
            }
        });
    }

    self.initialiseBraintree = function (onInitialised) {
        if (braintree) {
            //console.log('paymentMethodFactory initialiseBraintree');

            braintree.client.create({
                authorization: getBraintreeClientKey()
            },
                function (clientErr, clientInstance) {
                    if (clientErr) {
                        self.notifyError(clientErr);
                        return;
                    }

                    self.braintreeInstance = clientInstance;

                    if (self.parent.checkoutType === 4) {
                        braintree.paypalCheckout.create(
                            {
                                client: clientInstance
                            },
                            function (paypalCheckoutErr, paypalCheckoutInstance) {
                                if (paypalCheckoutErr) {
                                    notifyPaypalError(paypalCheckoutErr);
                                    return;
                                }

                                $("#paypal-button").empty();

                                paypal.Button.render(
                                    {
                                        env: getBraintreeEnvironment(),
                                        commit: false,
                                        payment: function () {
                                            return paypalCheckoutInstance.createPayment(
                                                {
                                                    flow: 'checkout',
                                                    amount: 0.01,
                                                    currency: getBraintreeCurrency(),
                                                    enableShippingAddress: true
                                                }
                                            );
                                        },
                                        style: {
                                            color: 'gold',
                                            size: 'responsive',
                                            shape: 'rect',
                                            label: 'paypal',
                                            tagline: false
                                        },
                                        onAuthorize: function (data, actions) {
                                            return paypalCheckoutInstance.tokenizePayment(data, function (err, payload) {
                                                self.parent.deliveryAddressFactory.updateFromPayPal(payload);
                                            });
                                        },
                                        onCancel: function (data) {
                                        },
                                        onError: function (err) {
                                            notifyPaypalError(err);
                                        }
                                    },
                                    '#paypal-button');
                            });
                    } else if (self.parent.checkoutType == 5) { // Apple
                        //if (!window.ApplePaySession) {
                        //    let errorString = self.resources.ApplePayDeviceNotSupported;
                        //    console.log(errorString);
                        //    notifyApplePayError(errorString);
                        //} else {
                        //    if (!ApplePaySession.canMakePayments()) {
                        //        let errorString = self.resources.ApplePayDeviceCannotPay;
                        //        console.log(errorString);
                        //        notifyApplePayError(errorString);
                        //    } else {
                        //        braintree.applePay.create({
                        //            client: clientInstance
                        //        }, function (applePayErr, applePayInstance) {
                        //            if (applePayErr) {
                        //                console.error('applePay error', applePayErr);
                        //                notifyApplePayError(applePayErr);
                        //                return;
                        //            }

                        //            self.applePayInstance = applePayInstance;
                        //        });
                        //    }
                        //}
                    }
                    else {
                        braintree.threeDSecure.create({
                            version: 2,
                            client: self.braintreeInstance
                        },
                            function (threeDSecureErr, threeDSecureInstance) {
                                if (threeDSecureErr) {
                                    self.notifyError(threeDSecureErr);
                                    return;
                                }

                                threeDSecure = threeDSecureInstance;
                            });
                    }

                    if (onInitialised) {
                        onInitialised();
                    }
                });
        } else {
            //console.log('paymentMethodFactory initialiseBraintree: braintree NOT DEFINED');
            self.braintreeLog(0, 'failure', '');
            $("#payment-processing-failure").modal('show');
        }
    }

    self.loadBraintreeHostedFields = function () {
        //console.log('paymentMethodFactory loadBraintreeHostedFields');
        $.ajax({
            url: "/api/checkout/checkoutBraintreeGenerate",
            type: "POST",
            cache: false,
            async: true,
            data: JSON.stringify({
                basketCountry: self.siteCountry
            }),
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                if (data.message === "checkoutBasketExpired") {
                    jumpToSessionExpired();
                    return;
                }

                $("#braintree-container").html(data);
                //$("#creditCardPayment-iframeContainer").html(data);

                //var permissions = document.getElementById('checkout-marketing-permissions');

                //// why is this being done?
                //if (permissions != null) {
                //    ko.cleanNode(permissions);
                //    ko.applyBindings(viewModel, permissions);
                //}

                var braintreeContainer = document.getElementById('braintree-form-container');
                if (braintreeContainer) {
                    ko.cleanNode(braintreeContainer);
                    ko.applyBindings(viewModel, braintreeContainer);
                }

                self.braintreeHostedFieldsLoaded();
            },
            error: function (xhr, textStatus, errorThrown) {
                //console.log('dg-checkoutPay: ' + textStatus);
                alert(self.resources.MajorError);
            }
        });
    }

    self.braintreeFullyLoaded = function () {
        $("#payment-loading-wrapper").hide();
    }

    self.braintreeHostedFieldsLoaded = function () {

        //console.log('paymentMethodFactory braintreeHostedFields loading')

        if (self.braintreeInstance != null) {

            self.braintreeLog(0, 'ready', 'onReady callback completed.');

            $('.payment-loading-wrapper').remove();

            if (braintree.hasOwnProperty('hostedFields')) {

                $('#creditCardPayment-iframeContainer').show();

                braintree.hostedFields.create({
                    client: self.braintreeInstance,
                    styles: {
                        'input': {
                            'font-family': 'Verdana',
                            'font-weight': 'normal',
                            'color': '#666',
                            'transition': 'color 100ms linear',
                            '-webkit-transition': 'color 100ms linear'
                        },
                        '.invalid': {
                            'color': 'red'
                        },
                        '.number': {
                        },
                        '.cvv': {
                        }
                    },
                    fields: {
                        number: {
                            selector: '#number',
                            placeholder: self.resources.CheckoutBraintreeCardNumber
                        },
                        cvv: {
                            selector: '#cvv',
                            placeholder: self.resources.CheckoutBraintreeCVV
                        },
                        expirationMonth: {
                            selector: '#expirationMonth',
                            placeholder: self.resources.CheckoutBraintreeMM
                        },
                        expirationYear: {
                            selector: '#expirationYear',
                            placeholder: self.resources.CheckoutBraintreeYYYY
                        },
                        postalCode: {
                            selector: '#postalCode',
                            placeholder: self.resources.FormsPostCode
                        }
                    }
                },
                    function (hostedFieldsErr, hostedFieldsInstance) {

                        if (hostedFieldsErr) {
                            $("#pay-button-blocker").hide();
                            self.notifyError(hostedFieldsErr);
                            return;
                        }

                        //console.log('paymentMethodFactory braintreeHostedFields loaded successfully');

                        self.braintreeFullInstance = hostedFieldsInstance;

                        hostedFieldsInstance.on('focus', function (event) {
                            $(event.fields[event.emittedBy].container).toggleClass('input-focused');
                        });

                        hostedFieldsInstance.on('blur', function (event) {
                            $(event.fields[event.emittedBy].container).toggleClass('input-focused');
                        });

                        hostedFieldsInstance.on('cardTypeChange', function (event) {
                            //console.log('cardTypeChange', event);
                            if (event.cards.length === 1 && (self.loggedCardType == null || self.loggedCardType != event.cards[0].type)) {
                                self.loggedCardType = event.cards[0].type;
                                self.braintreeLog(0, 'cardType', self.loggedCardType);
                            }
                        });

                        // now we've removed the popup thing, we need somewhere to click... to save the card to db & vault
                        // but... we should be able to work that into the buy now click, happy days
                        $('#braintree-add-card').click(function (event) {

                            self.addCardBusy(true);

                            //$('#braintree-add-card').attr('disabled', true);
                            //event.preventDefault();

                            hostedFieldsInstance.tokenize(
                                {
                                    vault: self.saveCardDetails(),
                                    cardholderName: self.billingFullName()
                                    //,
                                    // no can do; we're removing billing address...
                                    //billingAddress: {
                                    //    streetAddress: selectedBillingAddress.AddressLine1,
                                    //    extendedAddress: selectedBillingAddress.AddressLine2,
                                    //    locality: selectedBillingAddress.Town,
                                    //    region: selectedBillingAddress.County,
                                    //    postalCode: selectedBillingAddress.PostCode,
                                    //    countryCodeAlpha2: selectedBillingAddress.CountryCode,                                
                                    //    countryName: selectedBillingAddress.CountryName
                                    //}
                                },
                                function (tokenizeErr, payload) {
                                    if (tokenizeErr) {

                                        self.addCardBusy(false);

                                        if (tokenizeErr.code === "HOSTED_FIELDS_FIELDS_INVALID") {
                                            $.each(tokenizeErr.details.invalidFields, function (i, field) {
                                                $(field).addClass('braintree-hosted-fields-invalid')
                                            });
                                            self.showPaymentProcessingError();
                                        } else if (tokenizeErr.code === "HOSTED_FIELDS_FIELDS_EMPTY") {
                                            self.showPaymentProcessingError();
                                            $("#braintree-checkout-credit-card-wrapper div.form-control").addClass('braintree-hosted-fields-invalid');
                                        }
                                        else {
                                            $("#card-vaulting-failure").modal('show');
                                        }

                                        $("#pay-button-blocker").hide();
                                        self.notifyError(tokenizeErr);
                                        return;
                                    }
                                    var saveCardToDB = !self.saveCardDetails();
                                    self.addCard(payload.details, payload.nonce, payload.details.bin, saveCardToDB);
                                    $("#card-modal").modal('hide');
                                    self.braintreeFullInstance.clear('number');
                                    self.braintreeFullInstance.clear('cvv');
                                    self.braintreeFullInstance.clear('expirationMonth');
                                    self.braintreeFullInstance.clear('expirationYear');
                                    self.braintreeFullInstance.clear('postalCode');
                                }
                            );
                        });
                    });
            }
        }
    }

    self.initialisePaymentPartial = function () {
        // called by _paymentCardComponent upon KO binding

        //console.log('paymentMethodFactory initialisePaymentPartial');

        self.initialiseBraintree(self.onBraintreeInitialised);

        $('#divAddPaymentCardDetails').click(function (e) {
            e.stopPropagation();
        });
    }

    self.onBraintreeInitialised = function () {
        //console.log('paymentMethodFactory onBraintreeInitialised');

        self.loadBraintreeHostedFields();

        var selectedCard = localStorage.getItem('defaultToken');

        $.each(self.cards(), function (i, card) {
            var selectCard = false;

            if (selectedCard == null) {
                selectCard = card.defaultCard();
            }
            else {
                selectCard = card.token == selectedCard;
            }

            if (selectCard) {
                self.selectedCard(card.token);
                return false;
            }
        });

        if (self.parent.checkoutType === 4) {
            self.selectedCard("paypal");
        }

        // ???
        //if (self.parent.isMemberCheckout)
        //{
        //    // ?
        //    //self.parent.deliveryAddressFactory.addressItems.subscribe(function (updatedAddresses) {
        //    //    self.parent.deliveryAddressFactory.addressItems(updatedAddresses);
        //    //});
        //    //
        //}
        //else
        //{
        //    if (self.parent.deliveryAddress() != null) {
        //        if (self.parent.selectedAddress().hasOwnProperty('deliveryOptionID')) {
        //            if (self.parent.selectedAddress().deliveryOptionID() === 1) {
        //                paymentViewModel.addCheckoutAddresstoAddressItems(viewModel.selectedAddress());
        //            }
        //        }
        //    }
        //    self.parent.deliveryAddressFactory.selectedAddress.subscribe(function (address) {
        //        if (address) {
        //            if (address.hasOwnProperty('deliveryOptionID')) {
        //                if (address.deliveryOptionID() === 1 || address.deliveryOptionID() === 5)
        //                    paymentViewModel.addCheckoutAddresstoAddressItems(address);
        //            }
        //        }
        //    });
        //}
    }

    self.notifyError = function (error) {
        var errorMessage = "onError hit, but the parameter was passed incorrectly.";

        if (error != null) {
            if (error.hasOwnProperty('details')) {
                if (error.details) {
                    if (error.details.hasOwnProperty('invalidFields')) {
                        delete error.details.invalidFields;
                    }
                }
            }
            errorMessage = JSON.stringify(error)
        }

        //console.log('paymentMethodFactory error', errorMessage);

        $.ajax({
            url: '/api/checkout/asyncSaveBraintreeError',
            type: 'POST',
            async: false,
            cache: false,
            data: {
                type: "braintree error function error!",
                message: errorMessage
            },
            success: function (data) {
                self.hideCompletingMessage();
                //console.log("braintree error logged");
            },
            error: function (xhr, s, a) {
                self.hideCompletingMessage();
                //console.log("braintree error logging failed");
            }
        });
    }

    self.addCard = function (card, nonce, bin, save) {

        self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory addCard');

        // no expiry information available at this point. The card IS valid and has not expired; it has been successfully tokenised.
        var newCard = {
            cardType: card.cardType,
            endDigits: card.lastFour,
            token: nonce,
            defaultCard: ko.observable(false),
            isExpired: false
        };

        /*if (save) {
            newCard.vaultToken = ko.observable('');
        }
        else {        
            newCard.vaultToken = ko.observable('new');
        }*/

        newCard.vaultToken = ko.observable('');
        self.cards.push(newCard);
        self.selectedCard(nonce);

        if ($('#chkMakeCardDefault').prop('checked')) {
            self.makeDefaultCard(newCard);
        }

        if (save) {
            self.asyncAddPaymentToken(card, nonce, bin);
        } else {
            self.addCardBusy(false);
        }
    }

    self.asyncAddPaymentToken = function (card, nonce, bin) {
        //console.log('paymentMethodFactory asyncAddPaymentToken called');

        self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory asyncAddPaymentToken called');
        $.ajax({
            url: "/api/checkout/checkoutAttachPaymentToken",
            type: "Post",
            cache: false,
            async: true,
            data: {
                cardType: card.cardType,
                lastFourDigits: card.lastFour,
                token: nonce,
                bin: bin
            },
            success: function (data) {
                self.addCardBusy(false);
                //console.log('paymentMethodFactory asyncAddPaymentToken returned');
                self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory asyncAddPaymentToken returned');
            },
            error: function (xhr, textStatus, errorThrown) {
                self.addCardBusy(false);
                self.parent.checkoutLog(251, 'error saving card');
                self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory asyncAddPaymentToken errored: ' + JSON.stringify(xhr));
            }
        });
    }

    window.onAmazonLoginReady = function () {
        //console.log('window onAmazonLoginReady called');
        self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory onAmazonLoginReady called');
        amazon.Login.setClientId(getAmazonClientId());
    };

    window.onAmazonPaymentsReady = function () {
        //console.log('window onAmazonPaymentsReady called');
        self.amazonAddressBookRebind();
    };

    self.initialiseAmazonPaymentPartial = function () {
        //console.log('paymentMethodFactory initialiseAmazonPaymentPartial called');
        ko.applyBindings(self, $('#payment-method-content')[0]);
    }

    // whether or not we should skip the onAddressSelect behaviour for the address book
    // set when changing from readonly mode to edit mode, reset when read
    self.amazonAddressBookSkipSelect = ko.observable(false);
    self.amazonAddressBookSkipSelect.subscribe(function (newValue) {
        //console.log('paymentMethodFactory amazonAddressBookSkipSelect subscribe', newValue);
    });

    // whether or not we should skip the onPaymentSelect behaviour for the payment wallet
    // set when changing from readonly mode to edit mode, reset when read
    self.amazonWalletSkipSelect = ko.observable(false);
    self.amazonWalletSkipSelect.subscribe(function (newValue) {
        //console.log('paymentMethodFactory amazonWalletSkipSelect subscribe', newValue);
    });

    self.amazonReturnToEdit = function () {
        self.amazonHappy(false);
        self.amazonPaymentSelectHit(false);
        $("#amazon-address-book-edit").hide();
        $("#amazon-wallet-edit").hide();
        self.setShippingEditable();
        self.paymentMethodSelected(undefined);
        self.amazonAddressBookRebind();
    }

    self.amazonReview = function () {
        self.amazonHappy(true);
        self.setShippingReadOnly();
        //self.amazonWalletWidgetReadOnlyRebind();
        //self.amazonAddressBookReadOnlyRebind();
    }

    self.amazonAddressBookRebind = function () {
        self.amazonAddressBookWidget = new OffAmazonPayments.Widgets.AddressBook({
            sellerId: getAmazonMerchantId(),
            onAddressSelect: function (orderReference) {
                $("#payment-address-error").modal('hide');
                //console.log('paymentMethodFactory amazonAddressBookWidget onAddressSelect');
                self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory amazonAddressBookWidget onAddressSelect');

                //console.log('paymentMethodFactory Amazon onAddressSelect - amazonAddressBookSkipSelect() was', self.amazonAddressBookSkipSelect());
                //if (self.amazonAddressBookSkipSelect()) {
                //console.log('paymentMethodFactory Amazon onAddressSelect - select was skipped');
                //self.amazonAddressBookSkipSelect(false);
                //} else {
                //self.parent.busy(true);
                self.amazonHappy(false);
                self.parent.busy(true);
                self.getAmazonSelectedAddress(self.amazonOrderReferenceId());
                //}
            },
            design:
            {
                designMode: 'responsive'
            },
            onReady: function (orderReference) {
                //console.log('window onAmazonPaymentsReady.onReady called', orderReference, orderReference.getAmazonBillingAgreementId());
                self.amazonOrderReferenceId(orderReference.getAmazonBillingAgreementId());
            },
            onError: function (error) {
                console.error(error);
                console.error(error.getErrorCode(), error.getErrorMessage());
                if (error.getErrorCode() === "BuyerSessionExpired")
                    jumpToSessionExpired();
                else {
                    var errText = self.resources.AmazonPayAddressWidgetError;
                    alert(errText);
                }
            }
        });

        self.amazonAddressBookWidget.bind("addressBookWidgetDiv");
    }

    //self.amazonAddressBookReadOnlyRebind = function () {
    //    $("#amazon-address-book-edit").show();
    //    self.amazonAddressBookWidget = new OffAmazonPayments.Widgets.AddressBook({
    //        sellerId: getAmazonMerchantId(),
    //        onAddressSelect: function (orderReference) {
    //            //not required here?
    //            //self.getAmazonSelectedAddress(self.amazonOrderReferenceId());
    //        },
    //        design:
    //        {
    //            designMode: 'responsive'
    //        },
    //        onReady: function (orderReference) {
    //            //console.log('window onAmazonPaymentsReady.onReady called', orderReference, orderReference.getAmazonBillingAgreementId());
    //            self.amazonOrderReferenceId(orderReference.getAmazonBillingAgreementId());
    //        },
    //        onError: function (error) {
    //            if (error.getErrorCode() === "BuyerSessionExpired")
    //                jumpToSessionExpired();
    //            else {
    //                var errText = self.resources.AmazonPayAddressWidgetError;
    //                alert(errText);
    //            }
    //        },
    //        amazonOrderReferenceId: self.amazonOrderReferenceId(),
    //        displayMode: 'Read'
    //    });

    //    self.amazonAddressBookWidget.bind("addressBookWidgetDiv");
    //}

    self.amazonWalletWidgetRebind = function () {
        //console.log('paymentMethodFactory amazonWalletWidgetRebind called');
        var options = {
            sellerId: getAmazonMerchantId(),
            onPaymentSelect: function (orderReference) {

                //console.log('paymentMethodFactory amazonWalletWidget onPaymentSelect');
                self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory amazonWalletWidget onPaymentSelect');

                self.amazonHappy(true);
                self.amazonPaymentSelectHit(true);
                self.paymentMethodSelected('amazon');
                //if (self.amazonWalletSkipSelect()) {
                //console.log('paymentMethodFactory Amazon onPaymentSelect - select was skipped');
                //self.amazonWalletSkipSelect(false);
                //} else {
                //console.log('paymentMethodFactory Amazon onPaymentSelect - select occurred');

                //}
                //amazon should only be happy if both widgets are in readOnly mode
                //self.amazonHappy(true);
            },
            design: {
                designMode: 'responsive'
            },
            onError: function (error) {
                if (error.getErrorCode() === "BuyerSessionExpired")
                    jumpToSessionExpired();
                else {
                    var error = self.resources.AmazonPayWalletWidgetError;
                    alert(error);
                }
            }
        };

        if (self.amazonOrderReferenceId() != null) {
            options.amazonOrderReferenceId = self.amazonOrderReferenceId();
        }

        self.amazonWalletWidget = new OffAmazonPayments.Widgets.Wallet(options);

        self.amazonWalletWidget.setPresentmentCurrency(getBraintreeCurrency());
        self.amazonWalletWidget.bind("walletWidgetDiv");
    }

    //self.amazonWalletWidgetReadOnlyRebind = function () {
    //    $("#amazon-wallet-edit").show();
    //    //console.log('paymentMethodFactory amazonWalletWidgetReadOnlyRebind called');
    //    var options = {
    //        sellerId: getAmazonMerchantId(),
    //        onPaymentSelect: function (orderReference) {
    //            //console.log('paymentMethodFactory amazonWalletWidgetReadOnly onPaymentSelect');

    //            //not required here!
    //            //if (self.parent.deliveryAddressFactory.dispatchOptions().length) {
    //            //    self.updateAmazonValue();
    //            //}

    //            self.amazonPaymentSelectHit(true);
    //            self.paymentMethodSelected('amazon');
    //        },
    //        design: {
    //            designMode: 'responsive'
    //        },
    //        onError: function (error) {
    //            if (error.getErrorCode() === "BuyerSessionExpired")
    //                jumpToSessionExpired();
    //            else {
    //                var error = self.resources.AmazonPayWalletWidgetError;
    //                alert(error);
    //            }
    //        },
    //        displayMode: 'Read'
    //    };

    //    if (self.amazonOrderReferenceId() != null) {
    //        options.amazonOrderReferenceId = self.amazonOrderReferenceId();
    //    }

    //    self.amazonWalletWidget = new OffAmazonPayments.Widgets.Wallet(options);

    //    self.amazonWalletWidget.setPresentmentCurrency(getBraintreeCurrency());
    //    self.amazonWalletWidget.bind("walletWidgetDiv");
    //}

    self.getAmazonSelectedAddress = function (orderReferenceId) {
        //console.log('paymentMethodFactory getAmazonSelectedAddress called', orderReferenceId)

        self.parent.deliveryAddressFactory.addressConfirmed(false);

        self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory getAmazonSelectedAddress called');

        $.ajax({
            url: '/api/checkout/getAmazonDeliveryAddress',
            data: JSON.stringify({
                orderReferenceId: orderReferenceId,
                basketCountry: getSiteCountry()
            }),
            type: "POST",
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory getAmazonSelectedAddress returned' + JSON.stringify(data));

                //console.log('paymentMethodFactory getAmazonSelectedAddress returned', data);
                if (data.message === "checkoutBasketExpired") {
                    jumpToSessionExpired();
                    return;
                }
                else if (data.message === "invalidDeliveryAddress") {
                    self.parent.busy(false);
                    self.parent.checkoutLog(49, data.additionalInfo || 'No Additional Info!');
                    self.parent.deliveryAddressFactory.invalidDeliveryCountry();
                    return;
                }
                else {
                    var addr = self.parent.deliveryAddressFactory.createCheckoutDeliveryAddressFromAmazonAddress(data);
                    self.parent.deliveryAddressFactory.currentAddress(addr);
                    self.parent.deliveryAddressFactory.currentAddress().confirm();
                    self.amazonWalletWidgetRebind();
                }
            },
            error: function (data, status) {
                self.parent.busy(false);
                //console.log('dg-getAmazonSelectedAddress: ' + status);
                alert(self.resources.MajorError);
            }
        });
    }

    // must be called on first load out of delivery methods
    // why... can't we execute this functionality first thing in the submit method?
    self.updateAmazonValue = function () {
        //console.log('paymentMethodFactory updateAmazonValue called');
        var data = JSON.stringify({ orderReferenceId: self.amazonOrderReferenceId(), masterDefinition: getMasterDefinition() });

        self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory updateAmazonValue called: ' + data);
        $.ajax({
            url: '/api/checkout/updateAmazonPrice',
            data: data,
            type: "POST",
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                //console.log('paymentMethodFactory updateAmazonValue returned', data);

                self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory updateAmazonValue returned: ' + JSON.stringify(data));
                //self.setShippingReadOnly();
                //self.amazonAddressBookReadOnlyRebind();
                //self.amazonWalletWidgetReadOnlyRebind();

                if (data.message === "checkoutBasketExpired") {
                    jumpToSessionExpired();
                    return;
                } else {
                    self.amazonValueUpdated(true);
                }
            },
            error: function (data, status) {
                self.amazonValueUpdated(false);

                //console.log('dg-updateAmazonValue: ' + status);
                alert(self.resources.MajorError);
            }
        });
    }

    self.submitAmazonCheckout = function () {
        var data = {
            orderReferenceId: self.amazonOrderReferenceId(),
            masterDefinition: getMasterDefinition(),
            marketingPermissions: ko.toJS(self.parent.marketingPermissions())
        };
        //console.log('paymentMethodFactory submitAmazonCheckout called', data);
        self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory submitAmazonCheckout called: ' + JSON.stringify(data));
        $.ajax({
            url: '/api/checkout/checkoutAmazonSubmit',
            type: 'POST',
            cache: false,
            async: true,
            data: JSON.stringify(data),
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                //console.log('paymentMethodFactory submitAmazonCheckout returned', data);
                self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory submitAmazonCheckout returned: ' + JSON.stringify(data));
                if (data != null) {
                    if (data.result === "OK") {
                        if (data.redirect === "confirm") {
                            // defunct and should never be hit...
                            // window.location.href = '/l271';
                        } else if (data.redirect === "orderGen") {
                            $('#checkout-complete-waiting .checkout-loading-overlay').css('background-color', 'lightgreen');
                            $('#checkout-complete-waiting .popContent').css('background-color', 'lightgreen');
                            $('#checkout-complete-waiting .checkoutLoadingText').html(self.resources.CheckoutPaymentSuccessGeneratingOrder);
                            self.showCompletingMessage();
                            $.ajax({
                                url: '/api/checkout/checkoutOrderGenerate',
                                type: 'POST',
                                cache: false,
                                async: true,
                                timeout: 30000,
                                data: {},
                                contentType: 'application/json; charset=utf-8',
                                success: function (data) {
                                    window.location.href = '/l271';
                                },
                                error: function (xhr, textStatus, errorThrown) {
                                    window.location.href = '/l281';
                                }
                            });
                        }
                    }
                    else if (data.result === "FAIL") {
                        self.hideCompletingMessage();
                        switch (data.redirect) {
                            case "basket":
                                window.location.href = '/l201';
                                break;
                            case "amazonLogout":
                                //console.log('paymentMethodFactory submitAmazonCheckout: log user out of amazon, recreate checkout, bounce user to checkout welcome, provide information message');

                                $.ajax({
                                    url: "/api/checkout/recreateCheckout",
                                    type: "Post",
                                    cache: false,
                                    async: false,
                                    data: JSON.stringify({
                                        country: getCountryCode()
                                    }),
                                    contentType: 'application/json; charset=utf-8',
                                    success: function (data) {
                                        window.location.href = '/l211?a=0';
                                    },
                                    error: function (xhr, textStatus, errorThrown) {
                                        window.location.href = '/l201';
                                    }
                                });

                                break;
                            default:
                                $("#payment-processing-error-text").html(self.getAmazonErrorMessage(data.errorCode));
                                self.showPaymentProcessingError();

                                self.amazonReturnToEdit();

                                if (data.errorCode === 'InvalidPaymentMethod') {
                                    //console.log('paymentMethodFactory submitAmazonCheckout: InvalidPaymentMethod');
                                    // refresh wallet widget
                                    // bind all visible / accessible widgets to self.amazonOrderReferenceId();
                                    // set addressBook displayMode: 'read'
                                    // prevent any changes to shipping options
                                    // do NOT call setOrderReferenceDetails again (this would fail)
                                    // wait for onPaymentSelect callback to be hit
                                    // call ConfirmOrderReference (this is set in amazon submit)
                                    // OrderReferenceObject is now set back to Open
                                }
                                break;
                        }
                    }
                }
                else {
                    self.hideCompletingMessage();
                    $("#payment-processing-error-text").html(self.getAmazonErrorMessage(data.errorCode));
                    self.showPaymentProcessingError();
                    self.amazonWalletWidgetRebind();
                }
            },
            error: function (e, s, a) {
                self.parent.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory submitAmazonCheckout ERRORED: ' + JSON.stringify(e));
                self.hideCompletingMessage();
                $("#payment-processing-error-text").html(self.getAmazonErrorMessage("generic"));
                self.showPaymentProcessingError();
                //console.log("dg-checkoutAmazonSubmit " + s + " " + e.responseText);
                self.amazonWalletWidgetRebind();
            },
            timeout: 90000
        });
    }

    self.setShippingReadOnly = function () {
        $("#gift-voucher-div").hide();
        $(".voucher-action").hide();
        $('.deliveryradio').attr('disabled', true);
        $("#amazon-shipping-edit").show();
    }

    self.setShippingEditable = function () {
        $("#gift-voucher-div").show();
        $(".voucher-action").show();
        $('.deliveryradio').attr('disabled', false);
        $("#amazon-shipping-edit").hide();
    }

    self.getAmazonErrorMessage = function (code) {
        switch (code) {
            case "ConstraintsExist":
                return self.resources.AmazonPayConstraintsExist;
                break;
            case "InvalidPaymentMethod":
                return self.resources.AmazonPayInvalidPaymentMethod;
                break;
            case "AmazonRejected":
                return self.resources.AmazonPayRejected;
                break;
            case "ProcessingFailure":
                return self.resources.AmazonPayProcessingFailure;
                break;
            case "generic":
                return self.resources.AmazonPayGenericError;
                break;
            default:
                return self.resources.AmazonPayGenericError;
        }
    }

    window.pmf = self;

    return self;
});

const giftBoxFactory = ((parent) => {
    let self = {
        parent: parent,
        resources: parent.resources
    };

    self.serverModel = ko.observable();

    self.serverModel.subscribe(function (newValue) {
        if (newValue) {
            self.load();
        }
    });

    self.products = ko.pureComputed(function () {
        if (self.serverModel()) {
            return Enumerable.From(self.serverModel().checkoutGiftGroupedBaskets)
                .Select(function (x) {
                    return giftBoxingProduct({ record: x, parent: self });
                }).OrderBy(function (x) {
                    return x.ProductID
                }).ToArray()
        } else {
            return [];
        }
    }, self);

    self.giftBoxableProducts = ko.computed(function () {
        if (self.serverModel()) {
            return Enumerable.From(self.serverModel().checkoutGiftGroupedBaskets).Where(function (x) {
                return x.GiftWrappable
            }).OrderBy(function (x) {
                return x.ProductID
            }).ToArray()
        } else {
            return [];
        }
    }, self);

    self.giftBoxedProducts = ko.pureComputed(function () {
        return [...self.giftBoxes().flatMap(gb => gb.products())];
    }, self)

    self.maxGiftBoxCount = ko.pureComputed(function () {
        return Enumerable.From(self.giftBoxableProducts()).Select(function (x) {
            return x.Quantity
        }).Sum();
    }, self);

    self.giftBoxCountOptions = ko.pureComputed(function () {
        var options = [];
        for (var i = 0; i < self.maxGiftBoxCount(); i++) {
            options.push({ text: i + 1, value: i + 1 });
        }
        return options;
    }, self);

    self.desiredGiftBoxCount = ko.observable();

    self.desiredGiftBoxCount.subscribe(function (newValue) {
        if (newValue) {
            var boxes = [];
            for (var i = 0; i < newValue; i++) {
                boxes.push(giftBox({ record: { giftBoxId: (i + 1) }, parent: self }));
            }
            self.giftBoxes(boxes);
        } else {
            self.giftBoxes([]);
        }
    });

    self.giftBoxDesired = ko.observable(false);

    self.giftBoxes = ko.observableArray([]);

    self.giftBoxDesired.subscribe(function (newValue) {
        if (!newValue)
            self.desiredGiftBoxCount(undefined);
    });

    self.giftBoxPriceDisplay = ko.pureComputed(function () {
        if (self.serverModel()) {
            return self.resources.CheckoutGiftWouldYouLike + ' ' + formatCurrency(self.serverModel().checkoutGiftBoxPrices[0].GiftBoxPriceInc, 2, getCulture(), getCurrencyCode())
                + ' ' + self.resources.CheckoutGiftPerBox;
        }
    }, self);

    self.giftBoxTotalCostDisplay = ko.pureComputed(function () {
        if (self.serverModel())
            return formatCurrency(self.desiredGiftBoxCount() * self.serverModel().checkoutGiftBoxPrices[0].GiftBoxPriceInc, 2, getCulture(), getCurrencyCode());
    }, self);

    self.load = function () {
        if (self.serverModel()) {
            var giftBoxes = [];
            for (var i = 0; i < self.serverModel().checkoutGiftBoxs.length; i++) {
                var gb = giftBox({
                    record: {
                        giftBoxId: self.serverModel().checkoutGiftBoxs[i].CheckoutGiftBoxID,
                        message: self.serverModel().checkoutGiftBoxs[i].GiftMessage
                    },
                    parent: self
                });
                var products = Enumerable.From(self.serverModel().checkoutGiftBoxItems).Where(function (x) {
                    return x.CheckoutGiftBoxID == gb.giftBoxId;
                }).ToArray();
                var productObjs = [];
                for (var j = 0; j < products.length; j++) {
                    var rec = Enumerable.From(self.products()).Where(function (x) {
                        return x.CheckoutBasketID == products[j].CheckoutBasketID;
                    }).Single();
                    productObjs.push({
                        Code: rec.Code,
                        BasketName: rec.BasketName,
                        ProductID: rec.ProductID,
                        BasketLineTypeID: rec.BasketLineTypeID,
                        Quantity: ko.observable(products[j].Quantity),
                        CheckoutBasketID: rec.CheckoutBasketID
                    }
                    );
                }
                gb.products(productObjs);
                giftBoxes.push(gb);
            }
            self.giftBoxDesired(giftBoxes.length > 0);
            self.desiredGiftBoxCount(giftBoxes.length);
            for (var i = 0; i < giftBoxes.length; i++) {
                var thisGb = Enumerable.From(self.giftBoxes()).Where(function (x) {
                    return x.giftBoxId == giftBoxes[i].giftBoxId
                }).Single();
                thisGb.message(giftBoxes[i].message());
                thisGb.products(giftBoxes[i].products());
            }
        }
    }

    self.addGiftBox = function () {
        self.giftBoxes.push(giftBox({ parent: self }));
    }

    self.removeGiftBox = function (gb) {
        self.giftBoxes.remove(gb);
    }

    self.applyChoice = function (gb) {
        if (gb.addingQuantity.isValid()) {
            var extant = Enumerable.From(gb.addingToGiftBox().products()).Where(function (x) {
                return x.ProductID == gb.ProductID && x.BasketLineTypeID == gb.BasketLineTypeID
            }).SingleOrDefault();
            if (extant == null) {
                gb.addingToGiftBox().products.push({
                    Code: gb.Code,
                    BasketName: gb.BasketName,
                    ProductID: gb.ProductID,
                    BasketLineTypeID: gb.BasketLineTypeID,
                    Quantity: ko.observable(gb.addingQuantity()),
                    CheckoutBasketID: gb.CheckoutBasketID
                });
            } else {
                extant.Quantity(extant.Quantity() + gb.addingQuantity());
            }
        }
    }

    self.saveGiftBoxing = function () {
        if (Enumerable.From(self.giftBoxes()).Where(function (x) {
            return x.products().length == 0
        }).Any()) {
            window.showConfirm(self.resources.AreYouSure, self.resources.CheckoutGiftUnusedGiftBoxMessage + '<br/><br/>' + self.resources.CheckoutGiftNoChargeMessage, function () {
                var data = {
                    giftBoxes: self.toJs()
                };

                $.ajax({
                    url: '/api/checkout/saveGiftBoxing',
                    type: 'POST',
                    dataType: 'json',
                    data: data,
                    success: function (data) {
                        $.get('/' + getSiteCountry() + '/h251', function (result) {
                            $('#checkout-total-wrapper').html(result);
                            ko.cleanNode($('#checkout-total-wrapper')[0]);
                            ko.applyBindings(viewModel, $('#checkout-total-wrapper')[0]);
                        });
                        $('#gift-wrap-modal').modal('hide');
                    },
                    error: function (xhr, s, a) {
                        alert(self.resources.MajorError);
                    }
                });
            });
        } else {
            var data = {
                giftBoxes: self.toJs()
            };

            $.ajax({
                url: '/api/checkout/saveGiftBoxing',
                type: 'POST',
                dataType: 'json',
                data: data,
                success: function (data) {
                    $.get('/' + getSiteCountry() + '/h251', function (result) {
                        $('#checkout-total-wrapper').html(result);
                        ko.cleanNode($('#checkout-total-wrapper')[0]);
                        ko.applyBindings(viewModel, $('#checkout-total-wrapper')[0]);
                    });
                    $('#gift-wrap-modal').modal('hide');
                },
                error: function (xhr, s, a) {
                    alert(self.resources.MajorError);
                }
            });
        }
    }

    self.toJs = function () {
        return Enumerable.From(self.giftBoxes()).Where(function (x) {
            return x.products().length > 0
        }).Select(function (x) {
            return {
                giftBoxId: x.giftBoxId,
                giftBoxMessage: x.message(),
                products: Enumerable.From(x.products()).Select(function (y) {
                    return {
                        checkoutBasketId: y.CheckoutBasketID,
                        basketLineTypeId: y.BasketLineTypeID,
                        giftBoxId: x.giftBoxId,
                        quantity: y.Quantity()
                    }
                }).ToArray()
            }
        }).ToArray();
    }

    return self;
});

const giftBox = ((r) => {
    let self = Object.assign({
        resources: r.parent.resources
    }, r.record);

    self.name = self.resources.CheckoutGiftGiftBox + ' ' + self.giftBoxId;

    self.productQuantities = ko.observableArray([]);

    self.products = ko.observableArray([]);

    self.remove = function (product) {
        self.products.remove(product);
    }

    self.message = ko.observable(r.record.message);

    self.messageParams = {
        label: self.resources.CheckoutGiftGiftMessage,
        observable: self.message,
        useNativeValidation: false,
        decorateInputElement: true,
        insertValidationMessages: false,
        showClear: false,
        permitRevert: false,
        rows: 3,
        maxNewLines: 3,
        charsRemainingOptions: {
            maxLength: 500
        }
    }

    return self;
});

const giftBoxingProduct = ((r) => {

    let self = Object.assign({
        parent: r.parent,
        resources: r.parent.resources
    }, r.record);

    self.remainingQuantity = ko.pureComputed(function () {
        var alreadyGifted = Enumerable.From(self.parent.giftBoxes()).SelectMany(function (x) {
            return x.products()
        }).Where(function (y) {
            return y.ProductID == self.ProductID
        }).Select(function (x) {
            return x.Quantity();
        }).Sum();

        return self.Quantity - alreadyGifted;
    }, self);

    self.addingQuantity = ko.observable(1).extend({
        max: self.remainingQuantity
    });

    self.addingToGiftBox = ko.observable(1);

    return self;
});

const giftCardFactory = ((parent) => {
    let self = {
        parent: parent,
        resources: parent.resources,
        busy: ko.observable(false),
        giftCards: ko.observableArray([]),
        validateNow: ko.observable(false),
        serverModel: ko.observable()
    };

    self.serverModel.subscribe(sv => {
        self.giftCards(sv.map(gc => giftCard(gc, self)));
    })

    self.save = () => {
        self.busy(true);

        $.ajax({
            url: '/api/checkout/saveGiftCardsInBasket',
            type: 'POST',
            dataType: 'json',
            data: {
                giftCards: self.giftCards().map(gc => {
                    return {
                        CheckoutBasketID: gc.CheckoutBasketID,
                        Message: ko.unwrap(gc.message)
                    }
                })
            },
            success: function (data) {
                self.parent.hideGiftCardOptions();
                let products = self.parent.deliveryAddressFactory.dispatchOptions().flatMap(d => d.products);
                self.giftCards().forEach(gc => {
                    products.find(p => p.CheckoutBasketID == gc.CheckoutBasketID).giftCard.message(gc.message());
                });
            },
            error: function (xhr, s, a) {
                alert(self.resources.MajorError);
            }
        });

        self.busy(false);
    }

    return self;
});

const giftCard = ((record, parent) => {
    let self = Object.assign(record, {
        parent: parent,
        resources: parent.resources
    });

    self.message = ko.observable(self.GiftVoucherMessage).extend({
        charsRemainingOptions: {
            maxLength: 300
        }
    });

    self.messageParams = {
        label: self.resources.Message,
        observable: self.message,
        useNativeValidation: false,
        decorateInputElement: true,
        insertValidationMessages: false,
        showClear: false,
        permitRevert: false,
        rows: 3,
        maxNewLines: 3,
        charsRemainingOptions: {
            maxLength: 300
        }
    }

    return self;
});

const giftVoucherFactory = ((parent) => {
    let self = {
        parent: parent,
        resources: parent.resources
    };

    self.siteCountry = getSiteCountry();

    self.validateNow = ko.observable(false);

    self.voucherCodeOne = ko.observable().extend({
        minLength: {
            params: 4,
            message: self.parent.resources.VoucherCodeLengthMessage,
            onlyIf: function () {
                return self.validateNow();
            }
        }
    });

    self.voucherCodeTwo = ko.observable().extend({
        minLength: {
            params: 4,
            message: self.parent.resources.VoucherCodeLengthMessage,
            onlyIf: function () {
                return self.validateNow();
            }
        }
    });

    self.voucherCodeThree = ko.observable().extend({
        minLength: {
            params: 4,
            message: self.parent.resources.VoucherCodeLengthMessage,
            onlyIf: function () {
                return self.validateNow();
            }
        }
    });

    self.voucherCodeFour = ko.observable().extend({
        minLength: {
            params: 4,
            message: self.parent.resources.VoucherCodeLengthMessage,
            onlyIf: function () {
                return self.validateNow();
            }
        }
    });

    self.voucherCodeOne.subscribe(function (newValue) {
        if (newValue) {
            if (newValue.length == 4) {
                $(self.voucherCodeTwo.DOMElement).focus();
            }
        }
        self.clearServerError();
    });

    self.voucherCodeTwo.subscribe(function (newValue) {
        if (newValue) {
            if (newValue.length == 4) {
                $(self.voucherCodeThree.DOMElement).focus();
            }
        }
        self.clearServerError();
    });

    self.voucherCodeThree.subscribe(function (newValue) {
        if (newValue) {
            if (newValue.length == 4) {
                $(self.voucherCodeFour.DOMElement).focus();
            }
        }
        self.clearServerError();
    });

    self.voucherCodeFour.subscribe(function (newValue) {
        if (newValue) {
            if (newValue.length == 4) {
                $("#apply-gift-voucher").focus();
            }
        }
        self.clearServerError();
    });

    self.voucherPaste = function (data, event) {
        var p = null;
        if (event.originalEvent) {
            if (event.originalEvent.clipboardData) {
                p = event.originalEvent.clipboardData.getData('text/plain');
            } else if (window.clipboardData) {
                p = window.clipboardData.getData('text/plain');
            }
        }
        //console.log('voucherPaste', p);
        if (p.length == 19) {
            var arr = p.split('-');
            if (arr.length == 4) {
                arr.forEach(function (block) {
                    if (block.length != 4) {
                        return;
                    }
                });
                self.voucherCodeOne(arr[0]);
                self.voucherCodeTwo(arr[1]);
                self.voucherCodeThree(arr[2]);
                self.voucherCodeFour(arr[3]);
                $("#apply-gift-voucher").focus();
            }
        } else if (p.length == 16) {
            self.voucherCodeOne(p.substring(0, 4));
            self.voucherCodeTwo(p.substring(4, 8));
            self.voucherCodeThree(p.substring(8, 12));
            self.voucherCodeFour(p.substring(12, 16));
            $("#apply-gift-voucher").focus();
        }
    }

    self.vouchers = ko.observableArray([]);

    self.voucherToRemove = ko.observable();

    self.voucherToRemove.subscribe(function (newValue) {
        if (newValue) {
            $('#gift-voucher-modal').modal('hide');
            $("#voucher-remove-modal").modal('show');
        }
    });

    self.vouchersTotal = ko.computed(function () {
        var total = 0;
        self.vouchers().forEach(function (code) {
            total += code.ValueInc;
        });

        return total;
    });

    self.balance = ko.observable(getBalanceNumber());

    self.voucherTotalLength = ko.computed(function () {
        var one = self.voucherCodeOne() || '';
        var two = self.voucherCodeTwo() || '';
        var three = self.voucherCodeThree() || '';
        var four = self.voucherCodeFour() || '';
        var arr = [one, two, three, four];
        var str = arr.join('-');
        return str.length;
    }).extend({
        equal: {
            params: 19,
            message: self.parent.resources.VoucherCodeLengthMessage,
            onlyIf: function () {
                return self.validateNow();
            }
        }
    });

    self.clearServerError = function () {
        $("#gift-voucher-failure-message").empty();
    }

    self.errors = ko.validation.group([
        self.voucherCodeOne,
        self.voucherCodeTwo,
        self.voucherCodeThree,
        self.voucherCodeFour
        //,
        //self.voucherTotalLength
    ]);

    self.applyVoucherCode = function () {
        $('#gift-voucher-failure-message').empty();

        var voucherCode = self.voucherCodeOne() + "-" + self.voucherCodeTwo() + "-" + self.voucherCodeThree() + "-" + self.voucherCodeFour();

        self.validateNow(true);

        self.errors.showAllMessages(false);

        if (self.errors().length) {
            self.errors.showAllMessages(true);
        } else {
            $.ajax({
                url: "/api/checkout/insertGiftVoucher",
                type: "Post",
                cache: false,
                async: true,
                data: JSON.stringify({
                    Code: voucherCode,
                    Country: self.siteCountry
                }),
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    self.validateNow(false);
                    if (data.message === "checkoutBasketExpired") {
                        jumpToSessionExpired();
                        return;
                    }
                    if (data.isValid) {
                        self.clearVoucherInputs();
                        self.vouchers.push(data);
                        self.parent.vouchersUpdated();
                    } else {
                        $('#gift-voucher-failure-message').text(data.failureMessage);
                    }

                    self.balance(data.BalanceTotal);
                    self.parent.balanceNumber(data.BalanceTotal);

                    if (data.BalanceTotal === 0) {
                        self.parent.paymentMethodSelected('giftVoucherFull'); // @* If a voucher covers the total cost we've selected a payment method *@
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    self.validateNow(false);
                    //console.log('dg-checkoutPay: ' + textStatus);
                    alert(self.resources.MajorError);
                }
            });
        }
    };

    self.removeVoucher = function () {
        $("#voucher-remove-modal").modal('hide');
        $.ajax({
            url: "/api/checkout/deleteGiftVoucher",
            type: "Post",
            cache: false,
            async: true,
            data: JSON.stringify({
                GiftVoucherId: self.voucherToRemove().GiftVoucherID,
                Country: self.siteCountry
            }),
            contentType: 'application/json; charset=utf-8',
            success: function (data) {

                if (data.message === "checkoutBasketExpired") {
                    jumpToSessionExpired();
                    return;
                }

                var arrVouchers = self.vouchers();

                for (var i = 0; i < arrVouchers.length; i++) {
                    var voucher = arrVouchers[i];

                    if (voucher.GiftVoucherID === self.voucherToRemove().GiftVoucherID) {
                        arrVouchers.splice(i, 1);
                        self.voucherToRemove(undefined);
                        break;
                    }
                }

                self.vouchers(arrVouchers);
                self.balance(data.BalanceTotal);

                self.parent.vouchersUpdated();
            },
            error: function (xhr, textStatus, errorThrown) {
                //console.log('dg-checkoutPay: ' + textStatus);
                alert(self.resources.MajorError);
            }
        });
    };

    self.clearVoucherInputs = function () {
        self.voucherCodeOne(undefined);
        self.voucherCodeTwo(undefined);
        self.voucherCodeThree(undefined);
        self.voucherCodeFour(undefined);
    }

    self.reloadGiftVoucherDetails = function () {
        $.ajax({
            url: "/api/checkout/getGiftVouchersInBasket",
            type: "Post",
            cache: false,
            async: true,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                if (data.message === "checkoutBasketExpired") {
                    jumpToSessionExpired();
                    return;
                }

                self.buildVoucherCodeList(data.giftVoucherDetails);

                // what the hell is this?!
                if (self.parent.balanceNumber() <= 0) {
                    self.parent.showPlaceOrder(true);
                }

            },
            error: function (xhr, textStatus, errorThrown) {
                //console.log('dg-checkoutPay: ' + textStatus);
                alert(self.resources.MajorError);
            }
        });
    }

    self.buildVoucherCodeList = function (voucherCodes) {
        var arrVoucherCodes = [];
        voucherCodes.forEach(function (code) {
            arrVoucherCodes.push(code);
        });

        self.vouchers(arrVoucherCodes);
    }

    Number.prototype.formatCurrencyForDisplay = function () {
        if (getNumberFormattingSeparatorIsComma()) {
            return formatWithComma(Number(this.toFixed(2).replace(/\d(?=(\d{6})+\.)/g, '$&,'), 2, ','));
        }
        else {
            return this.toFixed(2).replace(/\d(?=(\d{6})+\.)/g, '$&,');
        }
    }

    self.formatWithComma = function (x, precision, seperator) {
        var options = {
            precision: precision || 2,
            seperator: seperator || ','
        }
        var formatted = x.toFixed(options.precision);
        var regex = new RegExp('^(\\d+)[^\\d](\\d{' + options.precision + '})$');
        formatted = formatted.replace(regex, '$1' + options.seperator + '$2');
        return formatted;
    }

    self.init = function () {
        //console.log('giftVoucherFactory init');
    }

    self.reset = function () {
        //console.log('giftVoucherFactory reset');

        self.vouchers([]);

        self.voucherCodeOne(undefined);
        self.voucherCodeTwo(undefined);
        self.voucherCodeThree(undefined);
        self.voucherCodeFour(undefined);
    }

    return self;
});

function upsApMarker({
    upsLocatorResult,
    map,
    parent
} = {
        upsLocatorResult: {
            geocodeField: {
                latitudeField: 0,
                longitudeField: 0
            },
            addressKeyFormatField: {
                consigneeNameField: 'error'
            }
        },
        map: null,
        parent: null
    }) {
    var self = $.extend(upsLocatorResult, {});

    self.parent = parent;

    self.map = map;

    self.infoDomHolder = ko.observable();

    self.marker = new google.maps.Marker({
        position: {
            lat: Number(upsLocatorResult.geocodeField.latitudeField),
            lng: Number(upsLocatorResult.geocodeField.longitudeField)
        },
        map: self.map,
        title: upsLocatorResult.addressKeyFormatField.consigneeNameField
    });

    self.showLargeImage = function () {
        if (self.accessPointInformationField.imageURLField)
            window.showInfo(self.addressKeyFormatField.consigneeNameField, `<img class="ap-img-lg" src="${self.accessPointInformationField.imageURLField}" />`);
    }

    let frag = document.createDocumentFragment();
    let con = document.createElement('div');

    frag.appendChild(con);

    ko.renderTemplate('ups-ap-detail-template', self, {}, con);

    self.info = con;

    self.infoWindow = new google.maps.InfoWindow({
        content: self.info
    });

    self.openInfoWindow = function () {
        self.infoWindow.open({
            anchor: self.marker,
            map: self.map,
            shouldFocus: true,
        });
        if (self.infoWindowOpenListener)
            google.maps.event.removeListener(self.infoWindowOpenListener);
        self.infoWindowOpenListener = google.maps.event.addListener(self.infoWindow, 'domready', function (e) {
            //console.log('infoWindow domready', e);
            ko.cleanNode(self.info);
            ko.applyBindings(self, self.info);
        });
    }

    self.marker.addListener("click", self.openInfoWindow);

    return self;
}

var marketingPermission = function (mp) {
    var self = this;

    self.MarketingPermissionID = mp.MarketingPermissionID;
    self.Name = mp.Name;
    self.Label = mp.Label;
    self.Hidden = mp.Hidden;
    self.Default = mp.Default;
    self.Checked = ko.observable(mp.Default);
}