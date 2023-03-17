const basketMaintenanceViewModel = ((resources) => {
    let self = {
        resources: resources
    };

    //#region Properties

    // Whether or not our viewModel is busy
    self.busy = ko.observable(false);

    // Resolution busy?
    self.resolveBusy = ko.observable(false);

    // Whether or not our viewModel is ready for use
    self.ready = ko.observable(false);

    // The data holder, for Object.assign
    self.data = ko.observable();

    // The items in this basket
    self.basketItems = ko.observableArray([]);

    // Items saved for later
    self.savedForLaterItems = ko.observableArray([]);

    // Any errors this basket has run into
    self.errors = ko.observableArray([]);

    // The promotion code being inputted
    self.promoCode = ko.observable();

    // The promotion code to apply when you've logged in... or something?
    self.loginPromoCode = ko.observable();

    // Whether or not to show the promo code login modal
    self.showPromoCodeLogin = ko.observable(false);

    // The promotion object which may be present 
    self.promotion = ko.observable();

    // If failed, failed promotion object goes here
    self.failedPromotion = ko.observable();

    // Promotional message
    self.promotionalMessage = ko.observable();

    // The total price of this basket (excludes delivery)
    self.totalPrice = ko.observable();

    // Product delivery options
    self.productDeliveryOptions = ko.observableArray([]);

    // Basket notices (Freeform HTML coming from the DB)
    self.notices = ko.observable();

    //Need a feature which consumes a promotion code from query string parameter "a",
    //adds it to this visitor's basket if possible before the initial render
    //upon failure reports to the user the nature of the failure
    //if login is required presents user with a dialog stating as much, 
    //with a link to take them to login and redirect back to adding


    //#endregion

    //#region Components

    // Stock info model. Handy.
    self.stockInfoModel = stockInfoViewModel(self);

    //#endregion

    //#region Subscriptions

    self.ready.subscribe(nv => {
        self.init()
    });

    // The data subscription which knockout prerendered bindings come along and fill
    self.dataSub = self.data.subscribe(d => {
        Object.assign(self, d);
        self.dataSub.dispose();

        if (self.hasOwnProperty('BasketErrors'))
            self.errors(self.BasketErrors.errors);
        else if (self.hasOwnProperty("Errors") &&
            self.Errors.hasOwnProperty('errors')) {
            self.errors(self.Errors.errors);
        }

        self.totalPrice(self.Total.TotalPrice);

        self.buildDeliveryOptions(self.ProductDeliveryOptions);

        self.notices(self.Notices);

        if (self.hasOwnProperty('PromotionCodeDetails')) {
            if (self.PromotionCodeDetails.hasOwnProperty('json')) {
                if (self.PromotionCodeDetails.json != null) {
                    if (self.PromotionCodeDetails.json.length) {
                        var validationJson = [];
                        if (self.hasOwnProperty('Errors') && self.Errors.hasOwnProperty('errors')) {
                            if (self.Errors.errors.hasOwnProperty('promotionCodeErrors')) {
                                validationJson = self.Errors.errors.promotionCodeErrors;
                            }

                            if (validationJson.filter(x => {
                                return x.action == 'login'
                            }).length) {
                                self.showPromoCodeLogin(true);
                                self.loginPromoCode(self.promoCodeDetails.json[0].promotionCode);
                            }
                            if (validationJson.filter(x => {
                                return x.action == 'delete'
                            }).length) {

                            }
                        }
                        let promo = promotionViewModel({
                            passPromotionCheck: {
                                promoCodeDetailsJson: self.PromotionCodeDetails.json,
                                validationJson: validationJson
                            }
                        },
                            self);

                        if (validationJson.length == 0) {
                            promo.displayValue(true);
                            self.totalPrice(self.totalPrice() - promo.totalDiscount());
                        }

                        self.promotion(promo);
                    }
                }
            }
        }
    });

    //#endregion

    //#region Computeds

    self.isBasketEmpty = ko.pureComputed(() => {
        return self.basketItems().length == 0;
    }, self);

    // Total Price (Formatted)
    self.totalPriceDisplay = ko.pureComputed(() => {
        return formatCurrency(self.totalPrice(), 2, getCulture(), getCurrencyCode());
    }, self);

    // Any errors our basket items have
    self.basketItemErrors = ko.pureComputed(() => {
        if (self.errors() && self.errors().hasOwnProperty('productErrors')) {
            return self.errors().productErrors;
        } else {
            return []
        }
    }, self);

    // Now, why would this be... just the count of basket items? Why not the sum of quantities?
    self.totalQuantity = ko.pureComputed(() => {
        return self.basketItems().length;
    }, self);

    // Basket items with errors on them, either 'edit' or 'delete
    self.erroneousBasketItems = ko.pureComputed(() => {
        return self.basketItems().flatMap(bi => {
            return bi.basketItemErrors().flatMap(bie => {
                if (bie.errors.filter(e => e.action == 'delete' || e.action == 'edit').length) {
                    var basketItem = self.basketItems().filter(bim => bim.ProductID == bie.productId)[0];
                    return [basketItem];
                } else {
                    return [];
                }
            })
        });
    }, self);

    // Basket items with errors on them, only those which are 'hard errors', with the action type 'delete'.
    self.terminalBasketItems = ko.pureComputed(() => {
        return self.basketItems().flatMap(bi => {
            return bi.basketItemErrors().flatMap(bie => {
                if (bie.errors.filter(e => e.action == 'delete').length) {
                    var basketItem = self.basketItems().filter(bim => bim.ProductID == bie.productId)[0];
                    return [basketItem];
                } else {
                    return [];
                }
            })
        });
    }, self);

    // Error display
    self.basketErrorDisplay = ko.pureComputed(() => {
        var result = '';
        if (self.errors()) {
            if (self.errors().hasOwnProperty('basketErrors')) {
                result = self.errors().basketErrors.flatMap(be => be.display === 'visible' && be.action == 'edit' ? [be.message] : []).join('<br/><br/>');
            }
            if (self.errors().hasOwnProperty('promotionCodeErrors')) {
                if (result != '')
                    result += '<br/><br/>';

                if (typeof self.errors().promotionCodeErrors === 'string') {
                    result = (result || '') + JSON.parse(self.errors().promotionCodeErrors).flatMap(pce => pce.display === 'visible' && pce.action == 'edit' ? [pce.message] : []).join('<br/><br/>');
                } else {
                    result = (result || '') + self.errors().promotionCodeErrors.flatMap(pce => pce.display === 'visible' && pce.action == 'edit' ? [pce.message] : []).join('<br/><br/>');
                }
            }
        }
        return result.replace(/(<br\/>)*$/, '');
    }, self);

    // Promotion Code errors
    self.promoCodeErrorDisplay = ko.pureComputed(() => {
        if (self.errors()) {
            if (self.errors().hasOwnProperty('promotionCodeErrors')) {
                return self.errors().promotionCodeErrors.flatMap(pce => pce.display === 'visible' ? [pce] : []);
            } else {
                return [];
            }
        } else {
            return [];
        }
    }, self);

    // Has the basket been emptied, or was it originally empty?
    self.basketHasHadItems = ko.pureComputed(() => {
        return self.basketItems().length ? true : false;
    }, self);

    // Display of item counts...
    self.totalNumberOfItems = ko.pureComputed(function () {
        return self.totalQuantity() == 1 ? '(1' + self.resources.Item + ')' : '(' + self.totalQuantity() + ' ' + self.resources.Items + ')';
    }, self);

    self.promoCodeLoginUrl = ko.pureComputed(() => {
        if (self.loginPromoCode()) {
            return `/${getSiteCountry()}/l102?redirect=201&a=${encodeURIComponent(self.loginPromoCode())}`;
        } else {
            return null;
        }
    }, self);

    //#endregion


    //#region Methods

    // Automatically resolve basket item errors. Witchcraft!
    self.resolveAllBasketItemErrors = () => {
        self.resolveBusy(true);
        let promises = [];

        var erroredBasketItems = self.terminalBasketItems();

        for (var i = 0; i < erroredBasketItems.length - 1; i++) {
            erroredBasketItems[i].quantity.silentUpdate(0);
            promises.push(self.updateBasket(erroredBasketItems[i], false))
        }

        var erroredPromoCodes = self.promoCodeErrorDisplay();

        if (erroredPromoCodes.length) {
            promises.push(self.deletePromoCode())
        }

        Promise.allSettled(promises).then(function () {
            if (erroredBasketItems.length) {
                erroredBasketItems[erroredBasketItems.length - 1].quantity.silentUpdate(0);
                self.updateBasket(erroredBasketItems[erroredBasketItems.length - 1], true);
            }
            if (self.basketErrorDisplay()) {
                //noop
            } else {
                let ele = document.querySelector('#basket-errors-modal');
                let modal = bootstrap.Modal.getInstance(ele);
                modal.hide();
            }
            self.resolveBusy(false);
        });
    }

    // Attempts to add the given promotion code
    self.addPromoCode = (onComplete) => {
        if (self.promoCode() != null) {
            self.failedPromotion(undefined);
            self.showPromoCodeLogin(false);
            self.busy(true);
            fetch('/api/promotion/AddPromotionCode', {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-store',
                credentials: 'same-origin',
                body: JSON.stringify({
                    promotionCode: self.promoCode(),
                    basketCountry: getSiteCountry()
                })
            })
                .then(window.handleFetchResponse)
                .then(data => {
                    //console.trace(data);
                    if (data.passPromotionCheck.passed) {

                        self.promotion(promotionViewModel(data, self));

                        if (data.passPromotionCheck.validationJson.length) {
                            if (data.passPromotionCheck.validationJson.filter(x => {
                                return x.action == 'login'
                            }).length) {
                                self.showPromoCodeLogin(true);
                            }
                            if (data.passPromotionCheck.validationJson.filter(x => {
                                return x.action == 'edit'
                            }).length) {
                                console.log('not sure what I should be doing with this.');
                            }
                        } else {
                            self.totalPrice(self.totalPrice() - self.promotion().totalDiscount());
                            self.promotion().displayValue(true);

                            let fragment = document.createDocumentFragment(),
                                container = document.createElement('div');

                            fragment.appendChild(container);

                            ko.renderTemplate(
                                'ko-applied-promotion-template',
                                self,
                                {
                                    afterRender: function () {
                                        window.showInfo(self.resources.PromotionCode,
                                            `<p>${self.resources.PromotionCodeAppliedSuccessfully}</p>${container.innerHTML}`
                                        )
                                        container.innerHTML
                                    },
                                },
                                container
                            );

                            // remove the input value and query string parameter if present.
                            self.promoCode(undefined);

                            let queryString = window.location.search,
                                urlParams = new URLSearchParams(queryString),
                                url = new URL(window.location);

                            urlParams.delete('a');

                            url.search = urlParams;

                            window.history.replaceState({}, '', url);
                        };
                    } else {
                        self.failedPromotion(promotionViewModel(data, self));

                        let failedModalElement = document.querySelector('#failed-promotion-modal');

                        let failedModal = bootstrap.Modal.getOrCreateInstance(failedModalElement);

                        failedModal.show();

                        if (data.passPromotionCheck.validationJson.length) {
                            if (data.passPromotionCheck.validationJson.filter(x => {
                                return x.action == 'login'
                            }).length) {
                                self.showPromoCodeLogin(true);
                                self.loginPromoCode(self.promoCode());
                                window.setTimeout(function () {
                                    self.promoCode(undefined);
                                }, 0);
                            }
                            if (data.passPromotionCheck.validationJson.filter(x => {
                                return x.action == 'edit'
                            }).length) {
                                console.log('not sure what I should be doing with this.');
                            }
                        } else {
                            self.totalPrice(self.totalPrice() - self.promotion().totalDiscount());
                            self.promotion().displayValue(true);
                        }
                    }
                    self.busy(false);
                })
                .catch(err => {
                    self.busy(false);
                    window.catchFetchErrors(err);
                });
        }
    };

    // Attempts to remove the current promotion
    self.deletePromoCode = () => {
        return new Promise(function (resolve, reject) {
            if (self.promotion()) {
                self.busy(true);
                fetch('/api/promotion/DeletePromotionCode', {
                    method: "POST",
                    headers: window.fetchHeaders,
                    cache: 'no-store',
                    credentials: 'same-origin',
                    body: JSON.stringify({
                        promotionCodeId: self.promotion().promotionCodeId,
                        basketCountry: getSiteCountry(),
                        languageId: getLanguageId(),
                        countryId: getCountryId()
                    })
                })
                    .then(window.handleFetchResponse)
                    .then(data => {
                        //console.trace(data);
                        if (data) {
                            self.promoCode(undefined);
                            self.promotion(undefined);
                        }
                        self.reload(data);
                        self.busy(false);
                        resolve();
                    })
                    .catch(err => {
                        self.busy(false);
                        window.catchFetchErrors(err);
                        reject();
                    })
            }
        })
    };

    self.updateBasket = (basketItem, reload) => {
        return new Promise(function (resolve, reject) {

            reload = typeof reload === 'undefined' ? true : reload

            var data = {
                objMaster: getMasterDefinition(),
                productId: basketItem?.ProductID,
                basketLineTypeId: basketItem?.BasketLineTypeID,
                quantity: basketItem?.quantity() ? +basketItem?.quantity() : 0,
                basketCountry: getSiteCountry(),
                savedForLaterFlag: basketItem?.saveForLater()
            }

            console.log('updateBasket calling with', data);

            self.busy(true);

            fetch('/api/basket/UpdateBasket', {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-store',
                credentials: 'same-origin',
                body: JSON.stringify(data)
            })
                .then(window.handleFetchResponse)
                .then((data) => {
                    console.log('updateBasket returned', data);

                    if (data.handleErrorMessage) {
                        alert(getResource('majorError'));
                        return;
                    }

                    if (typeof updateDotDigital === 'function') {
                        updateDotDigital();
                    }

                    if (basketItem
                        && typeof gTagAddToCart === 'function'
                        && typeof gTagRemoveFromCart === 'function') {

                        let difference = +basketItem.quantity() - +basketItem.previousQuantity();

                        if (difference > 0) {

                            let price = ko.unwrap(basketItem.displaySalePrice);

                            if (price == null) {
                                price = ko.unwrap(basketItem.displayPrice);
                                if (price == null) {
                                    price = ko.unwrap(basketItem.Price);
                                }
                            }

                            basketItem.priceOverride = +(price) * +basketItem.quantity();

                            gTagAddToCart(basketItem, difference);
                        }

                        if (difference < 0) {

                            let price = ko.unwrap(basketItem.displaySalePrice);

                            if (price == null) {
                                price = ko.unwrap(basketItem.displayPrice);
                                if (price == null) {
                                    price = ko.unwrap(basketItem.Price);
                                }
                            }

                            basketItem.priceOverride = +(price) / +basketItem.quantity();

                            gTagRemoveFromCart(basketItem, Math.abs(difference));
                        }
                        //gTagRemoveFromCart(basketItem, basketItem.previousQuantity(), () => {
                        //    if (basketItem.quantity() > 0) {
                        //        gTagAddToCart(basketItem, basketItem.quantity());
                        //    }
                        //});
                    }

                    if (basketItem) {
                        basketItem.previousQuantity(basketItem.quantity());
                    }

                    if (reload) {
                        self.reload(data);
                        self.busy(false);
                    } else {
                        self.busy(false);
                    }

                    resolve();
                })
                .catch((err) => {
                    window.catchFetchErrors(err);
                    self.busy(false);
                    reject();
                });
        });
    }

    self.buildDeliveryOptions = async (deliveryOptions) => {
        fetchResources([...new Set(deliveryOptions.flatMap(d => d.ResourceName))])
            .then(window.handleResourcesArray)
            .then(resources => {
                self.productDeliveryOptions(deliveryOptions.map(d => {
                    let dovm = deliveryOptionViewModel(d);
                    dovm.resourceValue(resources[Object.keys(resources).filter(k => k == d.ResourceName)[0]]);
                    return dovm;
                }));
            });
    }

    self.reload = (data) => {
        var basketItemObjs = [];
        var savedForLaterObjs = [];

        data.BasketItems.sort((a, b) => {
            if (a.SortOrder < b.SortOrder)
                return -1;
            if (a.SortOrder > b.SortOrder)
                return 1;
            return 0;
        }).forEach(br => {
            let b = basketItemViewModel(self, br.Quantity);
            b.data(br);
            if (!br.SaveForLater)
                basketItemObjs.push(b);
            else
                savedForLaterObjs.push(b);
        });

        if (data.hasOwnProperty('Errors') &&
            data.Errors.hasOwnProperty('errors')) {
            self.errors(data.Errors.errors);
        } else {
            self.errors([]);
        }

        self.basketItems(basketItemObjs);
        self.savedForLaterItems(savedForLaterObjs);
        self.totalPrice(data.Total.TotalPrice);

        self.buildDeliveryOptions(data.ProductDeliveryOptions);

        if (window.hasOwnProperty('basketModel'))
            window.basketModel.itemsInBasket(data.Total.TotalQuantity);

        if (data.PromotionCodeDetails) {
            if (data.PromotionCodeDetails.PromotionCodeDetailsJSON) {
                if (data.PromotionCodeDetails.json) {
                    var validationJson = [];
                    if (data.Errors.hasOwnProperty('errors')) {
                        if (data.Errors.errors.hasOwnProperty('promotionCodeErrors')) {
                            validationJson = typeof data.Errors.errors.promotionCodeErrors == 'string' ? JSON.parse(data.Errors.errors.promotionCodeErrors) : data.Errors.errors.promotionCodeErrors
                        }

                        if (validationJson.filter(x => x.action == 'login').length) {
                            self.showPromoCodeLogin(true);
                        }
                        if (validationJson.filter(x => x.action == 'edit').length) {
                            //console.log('not sure what I should be doing with this.');
                        }
                    }

                    self.promotion(promotionViewModel({
                        passPromotionCheck: {
                            promoCodeDetailsJson: data.PromotionCodeDetails.json,
                            validationJson: validationJson
                        }
                    },
                        self)
                    );

                    if (validationJson.length == 0) {
                        self.totalPrice(self.totalPrice() - self.promotion().totalDiscount());
                        self.promotion().displayValue(true);
                    }
                }
            }
        }
    }

    self.confirmBasket = function () {
        fetch('/api/basket/ConfirmBasket', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-store',
            credentials: 'same-origin',
            body: JSON.stringify({
                totalQuantity: self.totalQuantity(),
                totalPrice: self.displayTotalPrice(),
                basketLines: Enumerable.From(self.basketItems())
                    .Select(function (x) {
                        return x.toJs()
                    })
                    .ToArray(),
                country: getSiteCountry()
            })
        })
            .then(window.handleFetchResponse)
            .then(data => {
                if (data === "") {
                    window.location.href = getProceedToCheckoutUrl();
                }
                else {
                    if (data.handleErrorMessage === "validateBasketFailed") {
                        self.failedConfirmBasket(true);
                        return;
                    }
                    else {
                        self.failedConfirmBasketMessage(data);
                        self.failedConfirmBasketOther(true);
                        return;
                    }
                }
            })
            .catch(err => {
                self.busy(false);
                window.handleFetchResponse(err);
            });

        //var allowProceedToCheckout = true;
        //var isPromotionValid = true;
        //var basketLines = [];

        //self.basketItems().forEach(function (basketItem) {

        //    if (basketItem.Quantity === basketItem.quantity()) {
        //        basketLines.push(new BasketLine(basketItem));
        //    } else {
        //        allowProceedToCheckout = false;
        //        displayWarningValidation(basketItem.quantityInputName, getResource("basketQuantityNotApplied") + " (" + getResource("basketOriginalQuantity") + " " + basketItem.Quantity + ")");
        //    }
        //});

        //if (self.promotion()) {
        //    isPromotionValid = self.promotion().passed() ? true : false;
        //}

        //if (allowProceedToCheckout && isPromotionValid) {

        //    asyncConfirmBasket(self.totalQuantity(), self.displayTotalPrice(), basketLines);

        //} else if (allowProceedToCheckout === false) {

        //    $('html, body').animate({ scrollTop: $("#shoppingBasket").offset().top }, "fast");

        //} else if (isPromotionValid === false) {

        //    $("#promotionCode").attr("title", getResource("promotionDeleteMessage"));

        //    $("#promotionCode").tooltip("open");

        //    window.setTimeout(function () {
        //        $("#promotionCode").tooltip("close");
        //    }, 5000);

        //    $('html, body').animate({ scrollTop: $("#promotionCode").offset().top - 50 }, "fast");
        //}
    };

    self.refreshPage = function () {
        self.failedConfirmBasket(false);
        window.location.href = window.location.href;
    };

    self.closeFailedBasketMessage = function () {
        self.failedConfirmBasketMessage("");
        self.failedConfirmBasketOther(false);
    };

    //Knockout prerendered foreachInit wants to know how to create basketItems
    self.createBasketItemViewModel = () => basketItemViewModel(self);

    //The above mentioned foreachInit calls this afterQueueFlush: that means every time the basket contents re-render
    self.basketReady = () => {
        self.ready(true); // any subscription against ready will only fire once though, upon going it initially going from false to true
        window.bm = self;
    }

    // init is fired via the subscription to ready.
    self.init = () => {
        if (!self.isBasketEmpty()) {
            let queryString = window.location.search;
            let urlParams = new URLSearchParams(queryString);

            if (typeof urlParams !== 'undefined') {
                if (urlParams.get('a') != null) {
                    if (self.promotion() == null) {
                        self.promoCode(urlParams.get('a'));
                        self.addPromoCode();
                    }
                }
            }

            if (self.erroneousBasketItems().length || self.promoCodeErrorDisplay().length || self.basketErrorDisplay()) {
                if (typeof bootstrap === 'object') {
                    let ele = document.querySelector('#basket-errors-modal');
                    let modal = bootstrap.Modal.getOrCreateInstance(ele);
                    modal.show();
                } else {
                    window.addBootstrapLoadFunction(() => {
                        let ele = document.querySelector('#basket-errors-modal');
                        let modal = bootstrap.Modal.getOrCreateInstance(ele);
                        modal.show();
                    })
                }
            }
        }
    }

    //#endregion

    return self;
});

const deliveryOptionViewModel = ((r) => {
    let self = Object.assign({}, r);

    self.resourceValue = ko.observable();

    return self;
});

const promotionViewModel = ((data, parent) => {
    let self = {
        parent: parent
    };

    Object.assign(self, data);

    self.passed = self.passPromotionCheck.passed;

    self.promotionCodeId = self.passPromotionCheck.promoCodeDetailsJson[0].promotionCodeId;

    self.promotionCode = self.passPromotionCheck.promoCodeDetailsJson[0].promotionCode;

    self.description = self.passPromotionCheck.promoCodeDetailsJson[0].description;

    self.validMessage = ko.observable(self.passPromotionCheck.validationJson.flatMap(x => {
        return x.message.replace('[PromotionCode]', self.promotionCode)
    }).join('<br/><br/>'));

    self.totalDiscount = ko.observable(self.passPromotionCheck.promoCodeDetailsJson[0].result.effectiveDiscount);

    self.displayValue = ko.observable(false);

    return self;
});

window.basketMaintenanceModel = basketMaintenanceViewModel(getPrefetchedResources());

ko.applyBindings(window.basketMaintenanceModel, document.querySelector('#basket-wrapper'));