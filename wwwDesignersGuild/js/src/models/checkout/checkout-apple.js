const LOG_ACTION_APPLE = 35,
    LOG_ACTION_PAY_NOW_CLICK = 136,
    LOG_ACTION_PAY_NOW_STEP = 138,
    LOG_ACTION_PAY_NOW_VALIDATION_ERROR = 139,
    LOG_ACTION_APPLE_BEGIN = 170,
    LOG_ACTION_APPLE_ONVALIDATEMERCHANT = 171,
    LOG_ACTION_APPLE_ONVALIDATEMERCHANT_ERROR = 172,
    LOG_ACTION_APPLE_ONVALIDATEMERCHANT_SUCCESS = 173,
    LOG_ACTION_APPLE_ONSHIPPINGCONTACTSELECTED = 174,
    LOG_ACTION_APPLE_COMPLETESHIPPINGCONTACTSELECTION = 175,
    LOG_ACTION_APPLE_ONSHIPPINGMETHODSELECTED = 176,
    LOG_ACTION_APPLE_COMPLETESHIPPINGMETHODSELECTION = 177,
    LOG_ACTION_APPLE_ONPAYMENTAUTHORIZED = 178,
    LOG_ACTION_APPLE_PAYMENTTOKENIZATION_ERROR = 179,
    LOG_ACTION_APPLE_PAYMENTTOKENIZATION_SUCCESS = 180,
    LOG_ACTION_APPLE_COMPLETEPAYMENT = 181,
    LOG_ACTION_APPLE_ONCANCEL = 182,
    LOG_ACTION_APPLE_FETCHADDRESS = 183

const applePayViewModel = ((applePayInstance, parent) => {
    let self = {
        applePayInstance,
        parent: parent,
        resources: parent.resources
    }

    self.showCompletingMessage = () => {
        bootstrap.Modal.getOrCreateInstance(document.querySelector("#checkout-complete-waiting")).show();
        self.parent.busy(true);
        asyncCheckoutLog(LOG_ACTION_PAY_NOW_STEP, 'completing message shown');
    }

    self.hideCompletingMessage = () => {
        bootstrap.Modal.getOrCreateInstance(document.querySelector("#checkout-complete-waiting")).hide();
        self.parent.busy(false);
        document.querySelectorAll('.proceed-btn').forEach(pb => {
            pb.removeAttribute('disabled');
            pb.classList.remove('disabled');
        });
        asyncCheckoutLog(LOG_ACTION_PAY_NOW_STEP, 'completing message hidden');
    }

    self.dispatchOptionsFromServer = ko.observableArray([]);

    self.subTotal = ko.observable(getBalanceNumber());

    self.shipping = ko.observable(0);

    self.tax = ko.observable(0);

    self.total = ko.observable(0);

    self.selectedShippingMethod = null;

    self.selectedDeliveryAddress = null;

    self.begin = () => {

        let paymentRequestParams = {
            total: {
                label: 'Designers Guild',
                type: 'pending',
                amount: self.total(),
                lineItems: [{
                    label: 'Sub Total',
                    type: 'pending',
                    amount: self.subTotal()
                }, {
                    label: 'Delivery',
                    type: 'pending',
                    amount: self.shipping()
                }]
            },
            shippingType: 'delivery',
            shippingContactEditingMode: 'enabled',
            // We recommend collecting billing address information, at minimum
            // billing postal code, and passing that billing postal code with
            // all Apple Pay transactions as a best practice.
            applePayShippingType: 'delivery',
            requiredBillingContactFields: ["postalAddress"],
            requiredShippingContactFields: [
                "postalAddress",
                "email"
            ]
        }

        if (self.tax()) {
            paymentRequestParams.total.lineItems.push({
                label: 'Tax',
                type: 'pending',
                amount: self.tax()
            })
        }

        asyncCheckoutLog(LOG_ACTION_APPLE_BEGIN, JSON.stringify(paymentRequestParams));

        self.applePaymentRequest = self.applePayInstance.createPaymentRequest(paymentRequestParams);

        console.log('applePay window.beginApplePay called', self.applePaymentRequest);

        self.activeApplePaySession = new ApplePaySession(3, self.applePaymentRequest);

        self.activeApplePaySession.onvalidatemerchant = (e) => {
            console.log('applePay onvalidatemerchant', e);
            self.applePayInstance.performValidation({
                validationURL: e.validationURL,
                displayName: 'Designers Guild'
            }, function (err, merchantSession) {
                if (err) {
                    self.abortApple();
                    window.showInfo(self.resources.Sorry, self.resources.ApplePayFailedToLoad);
                    asyncCheckoutLog(LOG_ACTION_APPLE_ONVALIDATEMERCHANT_ERROR, JSON.stringify(err));
                    return;
                }
                console.log('applePay validatedmerchant', merchantSession);
                asyncCheckoutLog(LOG_ACTION_APPLE_ONVALIDATEMERCHANT_SUCCESS, JSON.stringify(merchantSession));
                self.activeApplePaySession.completeMerchantValidation(merchantSession);
            });
        };

        self.activeApplePaySession.onshippingcontactselected = (e) => {
            console.log('applePay onshippingcontactselected', e);

            asyncCheckoutLog(LOG_ACTION_APPLE_ONSHIPPINGCONTACTSELECTED, JSON.stringify(e));

            self.saveDeliveryAddress({
                DeliveryOptionID: 1,
                MemberAddressID: null,
                FullName: 'TEMPORARY',
                CompanyName: null,
                AddressLine1: 'TEMPORARY',
                AddressLine2: null,
                Town: e.shippingContact.locality,
                County: e.shippingContact.administrativeArea,
                Postcode: e.shippingContact.postalCode,
                CountryCode: e.shippingContact.countryCode,
                BasketCountry: getSiteCountry(),
                PhoneNumber: null,
                UPSAPID: null,
                UPSAPMemberAddressId: null,
                IsTradeAddress: false,
                IsClickCollect: false,
                masterDefinition: getMasterDefinition()
            }, () => {
                self.selectCheckoutAddress(() => {
                    self.loadDeliveryMethods(() => {
                        self.loadTotals(() => {
                            let availableShippingMethods = self.dispatchOptionsFromServer().map((d) => {
                                let minDateSplit = d.MinDeliveryDate.split(' ').map(i => Number(i));
                                let maxDateSplit = d.MaxDeliveryDate.split(' ').map(i => Number(i));
                                console.log('applePay mapping to shippingMethod', d);
                                return {
                                    'label': d.DisplayName,
                                    'detail': '',
                                    'amount': `${Number(d.PriceInc).toFixed(2)}`,
                                    'identifier': `${d.DeliveryBandID}`,
                                    'dateComponentsRange': {
                                        'startDateComponents': {
                                            'years': `${minDateSplit[0]}`,
                                            'months': `${minDateSplit[1]}`,
                                            'days': `${minDateSplit[2]}`
                                        },
                                        'endDateComponents': {
                                            'years': `${maxDateSplit[0]}`,
                                            'months': `${maxDateSplit[1]}`,
                                            'days': `${maxDateSplit[2]}`
                                        }
                                    }
                                }
                            });
                            let completeShippingContactModel = {
                                'status': ApplePaySession.STATUS_SUCCESS,
                                'newTotal': {
                                    'type': 'final',
                                    'label': 'Designers Guild',
                                    'amount': `${self.total().toFixed(2)}`
                                },
                                'newLineItems': [{
                                    'label': 'Sub Total',
                                    'type': 'final',
                                    'amount': `${self.subTotal().toFixed(2)}`
                                }, {
                                    'label': 'Delivery',
                                    'type': 'final',
                                    'amount': `${self.shipping().toFixed(2)}`
                                }],
                                'errors': [],
                                newShippingMethods: availableShippingMethods
                            };

                            if (self.tax()) {
                                completeShippingContactModel.newLineItems.push({
                                    'label': 'Tax',
                                    'type': 'final',
                                    'amount': `${self.tax().toFixed(2)}`
                                });
                            }

                            if (availableShippingMethods.length == 1) {
                                self.selectedShippingMethod = availableShippingMethods[0];
                                self.saveDispatch({
                                    shippingMethod: self.selectedShippingMethod
                                }, () => {
                                    asyncCheckoutLog(LOG_ACTION_APPLE_COMPLETESHIPPINGCONTACTSELECTION, JSON.stringify(completeShippingContactModel));
                                    console.log('applePay calling completeShippingContactSelection', completeShippingContactModel);

                                    self.activeApplePaySession.completeShippingContactSelection(completeShippingContactModel);
                                });
                            } else {
                                asyncCheckoutLog(LOG_ACTION_APPLE_COMPLETESHIPPINGCONTACTSELECTION, JSON.stringify(completeShippingContactModel));
                                console.log('applePay calling completeShippingContactSelection', completeShippingContactModel);

                                self.activeApplePaySession.completeShippingContactSelection(completeShippingContactModel);
                            }
                        });
                    });
                })
            });
        }

        self.activeApplePaySession.onshippingmethodselected = (e) => {
            console.log('applePay onshippingmethodselected', e);

            asyncCheckoutLog(LOG_ACTION_APPLE_ONSHIPPINGMETHODSELECTED, JSON.stringify(e));

            self.selectedShippingMethod = e.shippingMethod;

            window.getShippingNumber = () => {
                return +e.shippingMethod.amount;
            }

            self.saveDispatch(e, () => {

                let completionModel = {
                    status: ApplePaySession.STATUS_SUCCESS,
                    newTotal: {
                        type: 'final',
                        label: 'Designers Guild',
                        amount: getItemSubTotalNumber() + getShippingNumber()
                    },
                    newLineItems: [{
                        label: 'Sub Total',
                        type: 'final',
                        amount: getItemSubTotalNumber()

                    }, {
                        label: 'Delivery',
                        type: 'final',
                        amount: getShippingNumber()
                    }, {
                        label: 'Tax',
                        type: 'final',
                        amount: '0.00'
                    }]
                };

                asyncCheckoutLog(LOG_ACTION_APPLE_COMPLETESHIPPINGMETHODSELECTION, JSON.stringify(completionModel));

                self.activeApplePaySession.completeShippingMethodSelection(completionModel);
            });
        }

        self.activeApplePaySession.onpaymentauthorized = (e) => {
            console.log('applePay onpaymentauthorized', e);

            asyncCheckoutLog(LOG_ACTION_APPLE_ONPAYMENTAUTHORIZED, JSON.stringify(e));

            self.applePayInstance.tokenize({
                token: e.payment.token
            }, function (tokenizeErr, payload) {
                if (tokenizeErr) {
                    self.hideCompletingMessage();
                    console.error('applePay Error tokenizing:', tokenizeErr);

                    asyncCheckoutLog(LOG_ACTION_APPLE_PAYMENTTOKENIZATION_ERROR, JSON.stringify(tokenizeErr));

                    window.showInfo(self.resources.Sorry, self.resources.ApplePayFailureNotCharged);

                    self.activeApplePaySession.completePayment(ApplePaySession.STATUS_FAILURE);

                    return;
                }

                console.log('applePay tokenized', payload);

                asyncCheckoutLog(LOG_ACTION_APPLE_PAYMENTTOKENIZATION_SUCCESS, JSON.stringify(payload));

                let fullName = `${e.payment.shippingContact.givenName} ${e.payment.shippingContact.familyName}`.trim();
                let addressLine1 = null;
                let addressLine2 = null;

                if (e.payment.shippingContact.addressLines.length >= 1) {
                    addressLine1 = e.payment.shippingContact.addressLines[0];
                }

                if (e.payment.shippingContact.addressLines.length >= 2) {
                    addressLine2 = e.payment.shippingContact.addressLines[1];
                }

                self.saveDeliveryAddress({
                    DeliveryOptionID: 1,
                    MemberAddressID: null,
                    FullName: fullName,
                    CompanyName: null,
                    AddressLine1: addressLine1,
                    AddressLine2: addressLine2,
                    Town: e.payment.shippingContact.locality,
                    County: e.payment.shippingContact.administrativeArea,
                    Postcode: e.payment.shippingContact.postalCode,
                    CountryCode: e.payment.shippingContact.countryCode,
                    BasketCountry: getSiteCountry(),
                    PhoneNumber: null,
                    UPSAPID: null,
                    UPSAPMemberAddressId: null,
                    IsTradeAddress: false,
                    IsClickCollect: false,
                    masterDefinition: getMasterDefinition(),
                    secondSave: true
                }, () => {
                    self.submit(payload.nonce, e.payment.shippingContact.emailAddress);
                });
            });
        };

        self.activeApplePaySession.oncancel = function (event) {
            self.hideCompletingMessage();
            self.parent.busy(false);
            asyncCheckoutLog(LOG_ACTION_APPLE_ONCANCEL, self.resources.ApplePayFailureNotCharged);
            window.showInfo(self.resources.Sorry, self.resources.ApplePayFailureNotCharged);
        }
    }

    self.endApple = () => {
        asyncCheckoutLog(LOG_ACTION_APPLE_COMPLETEPAYMENT, 'completePayment(ApplePaySession.STATUS_SUCCESS)');
        self.activeApplePaySession.completePayment(ApplePaySession.STATUS_SUCCESS);
    }

    self.abortApple = () => {
        self.parent.busy(false);
        self.hideCompletingMessage();
        try {
            self.activeApplePaySession.abort();
        } catch (e) {

        }
    }

    self.showPaymentProcessingError = () => {
        bootstrap.Modal.getOrCreateInstance(document.querySelector("#payment-processing-error")).show();
    }

    self.hidePaymentProcessingError = () => {
        bootstrap.Modal.getOrCreateInstance(document.querySelector("#payment-processing-error")).hide();
    }

    self.loadDeliveryMethods = (onComplete) => {

        let data = {};

        console.log('applePay loadDeliveryMethods calling with', data);

        $.ajax({
            url: getUrlCheckoutDeliveryOptions(),
            type: "GET",
            data: data,
            dataType: "html",
            success: function (data) {
                console.log('applePay loadDeliveryMethods returned', data);

                var deliveryOptions = $('#despatch-items-wrapper');

                deliveryOptions.html(data);

                ko.cleanNode(deliveryOptions[0]);

                let model = {
                    deliveryAddressFactory: {
                        dispatchBusy: ko.observable(false),
                        parent: {
                            dispatchBusy: ko.observable(false)
                        },
                        dispatchOptions: ko.observableArray([])
                    }
                };

                ko.applyBindings(model, deliveryOptions[0]);

                // this because the markup contains a call which binding the binding hits
                // which throws dispatchBusy true
                window.setTimeout(() => {
                    model.deliveryAddressFactory.dispatchBusy(false);
                    model.deliveryAddressFactory.parent.dispatchBusy(false);
                }, 0);

                self.dispatchOptionsFromServer(getDispatchOptions().checkoutDispatchDeliveryOptions);

                onComplete();
            },
            error: function (xhr, s, a) {
                self.hideCompletingMessage();
                self.hidePaymentProcessingError();
                self.endApple();
                window.showInfo(self.resources.Sorry, self.resources.ApplePayFailureNotCharged);
            }
        });
    };

    self.loadTotals = (onComplete) => {
        let data = {};
        console.log('applePay loadTotals calling with', data);
        $.ajax({
            url: getUrlCheckoutSummary(),
            type: "GET",
            data: data,
            dataType: "html",
            success: function (data) {
                console.log('applePay loadTotals returned', data);

                var totalsWrapper = $('#despatch-items-wrapper');

                totalsWrapper.html(data);

                self.total(getBalanceNumber());
                self.subTotal(getItemSubTotalNumber());
                self.tax(getTaxNumber());
                self.shipping(getShippingNumber());
                onComplete();
            },
            error: function (xhr, s, a) {
                self.hideCompletingMessage();
                self.hidePaymentProcessingError();
                self.endApple();
                window.showInfo(self.resources.Sorry, self.resources.ApplePayFailureNotCharged);
            }
        });
    }

    self.saveDeliveryAddress = (addressData, onComplete) => {
        console.log('applePay saveDeliveryAddress calling with', addressData);

        asyncCheckoutLog(LOG_ACTION_PAY_NOW_STEP, addressData);

        $.ajax({
            url: "/api/checkout/saveCheckoutAddress",
            type: "Post",
            cache: false,
            async: true,
            dataType: 'json',
            data: addressData,
            success: function (data) {
                console.log('applePay saveDeliveryAddress returned', data);

                if (data.state === 'OK') {
                    onComplete();
                } else {
                    if (data.message === "checkoutBasketExpired") {
                        jumpToSessionExpired();
                        return;
                    } else if (data.message === "invalidZipCode") {
                        document.querySelector('#sales-tax-text').innerHTML = `${self.resources.CheckoutAddressZipCodeProblem}<br/><br/>${self.resources.ApplePayFailureNotCharged}`;
                        self.showZipModal();
                    } else if (data.message === "zipTaxServiceFailed") {
                        document.querySelector('#sales-tax-text').innerHTML = `${self.resources.CheckoutAddressZipCodeProblem}<br/><br/>${self.resources.ApplePayFailureNotCharged}`;
                        self.showZipModal();
                    }
                    self.hideCompletingMessage();
                    self.hidePaymentProcessingError();
                    self.endApple();
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                self.abortApple();
                window.showInfo(self.resources.Sorry, self.resources.ApplePayFailureNotCharged);
            }
        });
    }

    self.selectDeliveryAddress = (onComplete) => {
        console.log('applePay selectDeliveryAddress calling');

        asyncCheckoutLog(LOG_ACTION_APPLE_FETCHADDRESS, addressData);

        $.ajax({
            url: "/api/checkout/selectCheckoutAddress",
            type: "Post",
            cache: false,
            async: true,
            dataType: 'json',
            data: addressData,
            success: function (data) {
                console.log('applePay selectDeliveryAddress returned', data);
                self.selectedDeliveryAddress = data;
                if (data.ShouldPerformSalesTaxCheck) {
                    self.salesTaxCheck(onComplete);
                } else {
                    onComplete();
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                self.hideCompletingMessage();
                self.hidePaymentProcessingError();
                self.endApple();
                window.showInfo(self.resources.Sorry, self.resources.ApplePayFailureNotCharged);
            }
        });
    }

    self.showZipModal = () => {
        bootstrap.Modal.getOrCreateInstance(document.querySelector("#zip-modal")).show();
    }

    self.hideZipModal = () => {
        bootstrap.Modal.getOrCreateInstance(document.querySelector("#zip-modal")).hide();
    }

    self.salesTaxCheck = (onComplete) => {
        async = typeof async === 'undefined' ? true : async;
        guest = typeof guest === 'undefined' ? true : guest;

        var url = '/api/checkout/checkSalesTaxZipCode';

        var data = {
            MemberAddressID: ko.unwrap(checkoutAddress.memberAddressId),
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

            },
            error: function (xhr, textStatus, errorThrown) {
                //console.log('deliveryAddressFactory checkSalesTaxZipCode error:', xhr, textStatus, errorThrown);
                alert(self.resources.MajorError);
            }
        });
    }

    self.saveDispatch = (e, onComplete) => {

        let minDate = new Date(Date.UTC(
            e.shippingMethod.dateComponentsRange.startDateComponents.years,
            e.shippingMethod.dateComponentsRange.startDateComponents.months - 1,
            e.shippingMethod.dateComponentsRange.startDateComponents.days,
            0,
            0,
            0
        ));

        let maxDate = new Date(Date.UTC(
            e.shippingMethod.dateComponentsRange.endDateComponents.years,
            e.shippingMethod.dateComponentsRange.endDateComponents.months - 1,
            e.shippingMethod.dateComponentsRange.endDateComponents.days,
            0,
            0,
            0
        ));

        let data = [{
            dispatchId: 1,
            deliveryBandId: e.shippingMethod.identifier,
            shouldPartShip: false,
            minDeliveryDate: minDate.toISOString(),
            maxDeliveryDate: maxDate.toISOString(),
            customerRequestsFullDespatch: false
        }];

        console.log('applePay saveDispatch calling with', data);

        $.ajax({
            url: "/api/checkout/saveDispatch",
            type: "Post",
            cache: false,
            async: true,
            data: JSON.stringify({
                dispatchLines: data
            }),
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                console.log('applePay saveDispatch returned', data)
                onComplete();
            },
            error: function (xhr, textStatus, errorThrown) {
                self.hideCompletingMessage();
                self.hidePaymentProcessingError();
                self.endApple();
                window.showInfo(self.resources.Sorry, self.resources.ApplePayFailureNotCharged);
            }
        });
    }

    self.submit = function (nonce, email) {

        self.showCompletingMessage();

        let defaultCard = false;

        let data = JSON.stringify({
            device_data: null,
            payment_method_nonce: nonce,
            masterDefinition: getMasterDefinition(),
            basketCountry: getSiteCountry(),
            marketingPermissions: [],
            make_default: defaultCard,
            email: email
        });

        asyncCheckoutLog(LOG_ACTION_PAY_NOW_STEP, 'applePay submit calling with', data);

        console.log('applePay submit calling with', data);

        $.ajax({
            url: '/api/checkout/checkoutBraintreeSubmit',
            type: 'POST',
            cache: false,
            async: true,
            data: data,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {

                console.log('applePay submit returned', data);

                asyncCheckoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory submitAppleCheckout returned: ' + JSON.stringify(data));

                if (data != null) {
                    if (data.result === "OK") {
                        if (data.redirect === "confirm") {
                            // defunct and should never be hit
                            // window.location.href = '@dgFramework.buildUrl(Model.masterDefinition.UrlBreakdown.countryDetails, null, dgContentType.landingPage, (int)dgFixedPage.checkoutConfirmation, null, null)';
                        } else if (data.redirect === "orderGen") {
                            $('#checkout-complete-waiting .page-blocker').addClass('success');
                            $('#checkout-complete-waiting p').html(self.parent.resources.CheckoutPaymentSuccessGeneratingOrder);
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
                        self.hideCompletingMessage();
                        self.showPaymentProcessingError();
                    }
                }
                else {
                    self.hideCompletingMessage();
                    self.showPaymentProcessingError();
                }
            },
            error: function (e, s, a) {

                asyncCheckoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory submitApple ERRORED: ' + JSON.stringify(e));

                self.hideCompletingMessage();
                self.hidePaymentProcessingError();
                self.endApple();
                window.showInfo(self.resources.Sorry, self.resources.ApplePayFailureNotCharged);
            },
            timeout: 90000
        });
    }

    document.body.addEventListener('appleGo', () => {
        self.activeApplePaySession.begin();
    });

    return self;
});