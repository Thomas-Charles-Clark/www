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
    LOG_ACTION_APPLE_SAVEDELIVERYADDRESS_ERROR = 183

/* 
    This is the Apple Pay View Model
    It is invoked by _Login.cshtml, if the current checkout is permitted by the server to attempt Apple Pay (ecm.CheckoutType_Select governs this)
    The Braintree client instance manifests us an applePayInstance, and 'parent' is the Checkout Login Model 
    (window.clm, also pageModel.fixedPage().this.checkoutLoginModel which is why the shortcut...
*/
class ApplePayViewModel { 

    //#region Properties

    // These are the despatch options delivered via _CheckoutDespatch.cshtml. jQuery is used to render the HTML onto the hidden container element, 
    // so the script tags get processed into existence and we can call the functions therein to pull the data.
    dispatchOptionsFromServer = ko.observableArray([]);

    // These are necessary because Apple in their wisdom don't allow merchants to require multiple delivery option selections.
    consolidatedDispatchOptionsFromServer = ko.observableArray([]);

    // This is the subtotal, lifted from getBalanceNumber which first exists on _Login.cshtml and is then overwritten by _CheckoutTotal.cshtml
    subTotal = ko.observable(getBalanceNumber());

    // This is the shipping, rendered unto us by _CheckoutTotal.cshtml eventually
    shipping = ko.observable(0);

    // This is the shipping, rendered unto us by _CheckoutTotal.cshtml eventually
    tax = ko.observable(0);

    // This is the shipping, rendered unto us by _CheckoutTotal.cshtml eventually
    total = ko.observable(0);

    // This is the currently selected shipping method
    selectedShippingMethod = null;

    // This is the currently selected delivery address
    selectedDeliveryAddress = null;

    //#endregion

    constructor(applePayInstance, parent) 
    {
        this.applePayInstance = applePayInstance;
        this.parent = parent;
        this.resources = parent.resources;

        this.onShippingContactSelected = this.onShippingContactSelected.bind(this);
        this.onShippingMethodSelected = this.onShippingMethodSelected.bind(this);
        this.onValidateMerchant = this.onValidateMerchant.bind(this);
        this.onValidateMerchantResponse = this.onValidateMerchantResponse.bind(this);
        this.onPaymentAuthorized = this.onPaymentAuthorized.bind(this);
        this.onPaymentTokenized = this.onPaymentTokenized.bind(this);
        this.onCancel = this.onCancel.bind(this);

        let paymentRequestParams = {
            total: {
                label: 'Designers Guild',
                type: 'pending',
                amount: this.total(),
                lineItems: [{
                    label: 'Sub Total',
                    type: 'pending',
                    amount: this.subTotal()
                }, {
                    label: 'Delivery',
                    type: 'pending',
                    amount: this.shipping()
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
                "phone",
                "email"
            ]
        }

        if (this.tax()) {
            paymentRequestParams.total.lineItems.push({
                label: 'Tax',
                type: 'pending',
                amount: this.tax()
            });
        }

        this.applePaymentRequest = this.applePayInstance.createPaymentRequest(paymentRequestParams);

        this.checkoutLog(LOG_ACTION_APPLE_BEGIN, JSON.stringify(paymentRequestParams));

        // This is necessary because I have to await setCheckoutType returning successfully
        // before invoking the ApplePay sheet.
        document.body.addEventListener('appleGo', () => {
            this.activeApplePaySession.begin();
        });

        console.log('applePayViewModel ctor called', this);
    }

    //#region Methods

    // this is necessary because you have to create an ApplePaySession from a user guesture handler,
    // see _Login.cshtml proceedToCheckout
    init() {
        console.log('applePayViewModel init called');

        this.activeApplePaySession = new ApplePaySession(3, this.applePaymentRequest);

        this.activeApplePaySession.onvalidatemerchant = this.onValidateMerchant;

        this.activeApplePaySession.onshippingcontactselected = this.onShippingContactSelected;

        this.activeApplePaySession.onshippingmethodselected = this.onShippingMethodSelected;

        this.activeApplePaySession.onpaymentauthorized = this.onPaymentAuthorized;

        this.activeApplePaySession.oncancel = this.onCancel;
    }

    // to log things with
    checkoutLog(type, detail) {
        console.log('applePayViewModel checkoutLog calling with', type, detail);

        $.ajax({
            url: '/api/checkout/asyncSaveCheckoutLog',
            type: 'POST',
            async: false,
            cache: false,
            data: {
                action: type,
                detail: detail || "no detail provided"
            },
            success: (function (data) {

            }).bind(this),
            error: (function (xhr, s, a) {
                console.error("checkout logging failed!");
            }).bind(this)
        });
    }

    // callback handler for the onshippingcontactselected apple event
    onShippingContactSelected(e) {
        console.log('applePayViewModel onShippingContactSelected called', e);

        this.checkoutLog(LOG_ACTION_APPLE_ONSHIPPINGCONTACTSELECTED, JSON.stringify(e));

        this.saveDeliveryAddress({
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
        }, (result) => {
            console.log('applePayViewModel saveDeliveryAddress onComplete called with', result);

            if (result.state === 'OK') {

                this.refreshDeliveryAndTotals(() => {
                    console.log('applePayViewModel refreshDeliveryAndTotals onComplete called');

                    if (this.availableShippingMethods.length == 1) {

                        console.log('applePayViewModel only one shipping method was available: it will be selected, saved, and presented to the ApplePay sheet');

                        this.selectedShippingMethod = this.availableShippingMethods[0];

                        this.saveDispatch({
                            shippingMethod: this.selectedShippingMethod
                        }, () => {

                            this.checkoutLog(LOG_ACTION_APPLE_COMPLETESHIPPINGCONTACTSELECTION, JSON.stringify(this.completeShippingContactModel));

                            console.log('applePayViewModel calling completeShippingContactSelection', this.completeShippingContactModel);

                            this.activeApplePaySession.completeShippingContactSelection(this.completeShippingContactModel);
                        });

                    } else {

                        console.log('applePayViewModel multiple shipping methods are available, they will be presented to the ApplePay sheet');

                        this.checkoutLog(LOG_ACTION_APPLE_COMPLETESHIPPINGCONTACTSELECTION, JSON.stringify(this.completeShippingContactModel));

                        console.log('applePayViewModel calling completeShippingContactSelection', this.completeShippingContactModel);

                        this.activeApplePaySession.completeShippingContactSelection(this.completeShippingContactModel);
                    }
                });
            } else {

                if (result.message === "checkoutBasketExpired") {
                    jumpToSessionExpired();
                    return;
                }

                let errors = [];

                if (result.message === 'invalidCountry') {
                    errors.push(new ApplePayError('shippingContactInvalid', 'country', `${this.resources.CannotDeliverToThatCountry}`));
                }

                if (errors.length == 0) {

                    this.refreshDeliveryAndTotals(() => {

                        if (result.hasOwnProperty('isSalesTaxKnown')) {

                            if (result.isSalesTaxKnown) {
                                onComplete.apply(this);
                                return;
                            } else {

                                if (result.message === "invalidZipCode") {
                                    errors.push({
                                        code: "shippingContactInvalid",
                                        contactField: 'postalCode',
                                        message: this.resources.CheckoutAddressZipCodeInvalid
                                    });
                                } else if (result.message === "zipTaxServiceFailed") {
                                    errors.push({
                                        code: "shippingContactInvalid",
                                        contactField: 'postalCode',
                                        message: this.resources.CheckoutAddressZipCodeProblem
                                    });
                                } else if (result.salesTaxAddressList) {

                                    errors.push(new ApplePayError(
                                        'shippingContactInvalid',
                                        'postalAddress',
                                        `${this.resources.ZipTaxCouldNotDetermine}`
                                    ));

                                    let addressLines = `"${result.salesTaxAddressList.flatMap(s => s.addressLine).join('", "')}"`;
                                    let towns = `"${result.salesTaxAddressList.flatMap(s => s.town).join('", "')}"`;
                                    let states = `"${result.salesTaxAddressList.flatMap(s => s.state).join('", "')}"`;
                                    let counties = `"${result.salesTaxAddressList.flatMap(s => s.county).join('", "')}"`;

                                    errors.push(new ApplePayError(
                                        'shippingContactInvalid',
                                        'addressLines',
                                        `${addressLines}?`
                                    ));

                                    errors.push(new ApplePayError(
                                        'shippingContactInvalid',
                                        'locality',
                                        `${towns}?`
                                    ));

                                    errors.push(new ApplePayError(
                                        'shippingContactInvalid',
                                        'administrativeArea',
                                        `${states}?`
                                    ));

                                    errors.push(new ApplePayError(
                                        'shippingContactInvalid',
                                        'subAdministrativeArea',
                                        `${counties}?`
                                    ));
                                }
                            }
                        }

                        let completeShippingContactModel = this.completeShippingContactModel;

                        completeShippingContactModel.errors = errors;

                        this.checkoutLog(LOG_ACTION_APPLE_SAVEDELIVERYADDRESS_ERROR, JSON.stringify(Object.assign(result, completeShippingContactModel)));

                        console.log('applePayViewModel calling completeShippingContactSelection', completeShippingContactModel);

                        this.activeApplePaySession.completeShippingContactSelection(completeShippingContactModel);
                    });
                } else {

                    let completeShippingContactModel = {
                        'status': ApplePaySession.STATUS_SUCCESS,
                        'newTotal': {
                            'type': 'pending',
                            'label': 'Designers Guild',
                            'amount': `${this.total().toFixed(2)}`
                        },
                        'newLineItems': [{
                            'label': 'Sub Total',
                            'type': 'pending',
                            'amount': `${this.subTotal().toFixed(2)}`
                        }, {
                            'label': 'Delivery',
                            'type': 'pending',
                            'amount': `${this.shipping().toFixed(2)}`
                        }],
                        errors: errors
                    };

                    this.checkoutLog(LOG_ACTION_APPLE_SAVEDELIVERYADDRESS_ERROR, JSON.stringify(Object.assign(result, completeShippingContactModel)));

                    console.log('applePayViewModel calling completeShippingContactSelection', completeShippingContactModel);

                    this.activeApplePaySession.completeShippingContactSelection(completeShippingContactModel);

                }
            }
        });
    }

    onShippingMethodSelected(e) {
        console.log('applePayViewModel onShippingMethodSelected called', e);

        this.checkoutLog(LOG_ACTION_APPLE_ONSHIPPINGMETHODSELECTED, JSON.stringify(e));

        this.selectedShippingMethod = e.shippingMethod;

        window.getShippingNumber = () => {
            return +e.shippingMethod.amount;
        }

        this.saveDispatch(e, () => {
            console.log('applePayViewModel saveDispatch onComplete called');

            this.fetchTotals(() => {
                console.log('applePayViewModel fetchTotals onComplete called');

                let completionModel = this.completeShippingContactModel;

                delete completionModel.newShippingMethods;

                completionModel.newTotal = {
                    'type': 'final',
                    'label': 'Designers Guild',
                    'amount': `${this.total().toFixed(2)}`
                }

                completionModel.newLineItems = [{
                    'label': 'Sub Total',
                    'type': 'final',
                    'amount': `${this.subTotal().toFixed(2)}`
                }, {
                    'label': 'Delivery',
                    'type': 'final',
                    'amount': `${this.shipping().toFixed(2)}`
                }];

                if (this.tax()) {
                    completeShippingContactModel.newLineItems.push({
                        'label': 'Tax',
                        'type': 'final',
                        'amount': `${this.tax().toFixed(2)}`
                    });
                }

                completionModel.status = ApplePaySession.STATUS_SUCCESS;

                this.checkoutLog(LOG_ACTION_APPLE_COMPLETESHIPPINGMETHODSELECTION, JSON.stringify(completionModel));

                console.log('applePayViewModel completeShippingMethodSelection calling with', completionModel);

                this.activeApplePaySession.completeShippingMethodSelection(completionModel);
            });
        });
    }

    onValidateMerchant(e) {
        console.log('applePayViewModel onValidateMerchant called', e);

        this.applePayInstance.performValidation({
                validationURL: e.validationURL,
                displayName: 'Designers Guild'
            },
            this.onValidateMerchantResponse);
    };

    onValidateMerchantResponse(err, merchantSession) {
        console.log('applePayViewModel onValidateMerchantResponse called', err, merchantSession);

        if (err) {
            this.abortApple();
            window.showInfo(this.resources.Sorry, this.resources.ApplePayFailedToLoad);
            this.checkoutLog(LOG_ACTION_APPLE_ONVALIDATEMERCHANT_ERROR, JSON.stringify(err));
            return;
        }

        this.checkoutLog(LOG_ACTION_APPLE_ONVALIDATEMERCHANT_SUCCESS, JSON.stringify(merchantSession));

        this.activeApplePaySession.completeMerchantValidation(merchantSession);
    }

    onCancel(e) {
        console.log('applePayViewModel onCancel called', e);

        this.hideCompletingMessage();
        this.parent.busy(false);
        this.checkoutLog(LOG_ACTION_APPLE_ONCANCEL, this.resources.ApplePayFailureNotCharged);
        window.showInfo(this.resources.Sorry, this.resources.ApplePayFailureNotCharged);
    }

    onPaymentAuthorized(e) {
        console.log('applePayViewModel onPaymentAuthorized called', e);

        this.checkoutLog(LOG_ACTION_APPLE_ONPAYMENTAUTHORIZED, JSON.stringify(e));

        this.paymentAuthorization = e.payment;

        this.applePayInstance.tokenize({
                token: e.payment.token
            },
            this.onPaymentTokenized);
    }

    onPaymentTokenized(tokenizeErr, payload) {
        if (tokenizeErr) {
            this.hideCompletingMessage();
            console.error('applePayViewModel Error tokenizing:', tokenizeErr);

            this.checkoutLog(LOG_ACTION_APPLE_PAYMENTTOKENIZATION_ERROR, JSON.stringify(tokenizeErr));

            window.showInfo(this.resources.Sorry, this.resources.ApplePayFailureNotCharged);

            this.activeApplePaySession.completePayment(ApplePaySession.STATUS_FAILURE);

            return;
        }

        console.log(payload);

        this.checkoutLog(LOG_ACTION_APPLE_PAYMENTTOKENIZATION_SUCCESS, JSON.stringify(payload));

        let fullName = `${this.paymentAuthorization.shippingContact.givenName} ${this.paymentAuthorization.shippingContact.familyName}`.trim();
        let addressLine1 = null;
        let addressLine2 = null;

        if (this.paymentAuthorization.shippingContact.addressLines.length >= 1) {
            addressLine1 = this.paymentAuthorization.shippingContact.addressLines[0];
        }

        if (this.paymentAuthorization.shippingContact.addressLines.length >= 2) {
            addressLine2 = this.paymentAuthorization.shippingContact.addressLines[1];
        }

        this.saveDeliveryAddress({
            DeliveryOptionID: 1,
            MemberAddressID: null,
            FullName: fullName,
            CompanyName: null,
            AddressLine1: addressLine1,
            AddressLine2: addressLine2,
            Town: this.paymentAuthorization.shippingContact.locality,
            County: this.paymentAuthorization.shippingContact.administrativeArea,
            Postcode: this.paymentAuthorization.shippingContact.postalCode,
            CountryCode: this.paymentAuthorization.shippingContact.countryCode,
            BasketCountry: getSiteCountry(),
            PhoneNumber: null,
            UPSAPID: null,
            UPSAPMemberAddressId: null,
            IsTradeAddress: false,
            IsClickCollect: false,
            masterDefinition: getMasterDefinition(),
            secondSave: true
        }, (result) => {
            this.submit(payload.nonce, this.paymentAuthorization.shippingContact.emailAddress, this.paymentAuthorization.shippingContact.phoneNumber);
        });
    }

    // This function fetches _CheckoutDespatch.cshtml from the server, renders it into a hidden element and then lifts delivery options.
    // onComplete is called as a callback if provided upon success.
    fetchDeliveryMethods(onComplete) {
        let data = {};

        console.log('applePayViewModel fetchDeliveryMethods calling with', data);

        $.ajax({
            url: getUrlCheckoutDeliveryOptions(),
            type: "GET",
            data: data,
            dataType: "html",
            success: (function (data) {

                var deliveryOptions = $('#despatch-items-wrapper');

                deliveryOptions.html(data);

                this.dispatchOptionsFromServer(getDispatchOptions().checkoutDispatchDeliveryOptions);
                this.consolidatedDispatchOptionsFromServer(getDispatchOptions().checkoutDispatchAppleDeliveryOptions);

                console.log('applePayViewModel fetchDeliveryMethods returned', this.dispatchOptionsFromServer(), this.consolidatedDispatchOptionsFromServer());

                onComplete.apply(this);
            }).bind(this),
            error: (function (xhr, s, a) {
                console.error('applePayViewModel fetchDeliveryMethods errored', xhr, s, a);

                this.hideCompletingMessage();
                this.hidePaymentProcessingError();
                this.abortApple();
                window.showInfo(this.resources.Sorry, this.resources.ApplePayFailureNotCharged);
            }).bind(this)
        });
    };

    // This function fetches _CheckoutTotal.cshtml from the server, renders it into a hidden element and then lifts totals.
    // onComplete is called as a callback if provided upon success.
    fetchTotals(onComplete) {

        let data = {};

        console.log('applePayViewModel fetchTotals calling with', data);

        $.ajax({
            url: getUrlCheckoutSummary(),
            type: "GET",
            data: data,
            dataType: "html",
            success: (function (data) {
                var totalsWrapper = $('#despatch-items-wrapper');

                totalsWrapper.html(data);

                this.total(getBalanceNumber());
                this.subTotal(getItemSubTotalNumber());
                this.tax(getTaxNumber());
                this.shipping(getShippingNumber());

                console.log('applePayViewModel fetchTotals returned',
                    `total: ${this.total()}`,
                    `subTotal: ${this.subTotal()}`,
                    `tax: ${this.tax()}`,
                    `shipping: ${this.shipping()}`);

                onComplete.apply(this);
            }).bind(this),
            error: (function (xhr, s, a) {
                console.error('applePayViewModel fetchTotals errored', xhr, s, a);

                this.hideCompletingMessage();
                this.hidePaymentProcessingError();
                this.abortApple();
                window.showInfo(this.resources.Sorry, this.resources.ApplePayFailureNotCharged);
            }).bind(this)
        });
    }

    // This function chains fetchDeliveryMethods and fetchTotals.
    // The callback to the fetchTotals call sets this.availableShippingMethods and this.completeShippingContactModel,
    // ready for handing this.completeShippingContactModel to the apple session.
    refreshDeliveryAndTotals (onComplete) {
        console.log('applePayViewModel refreshDeliveryAndTotals called');

        this.fetchDeliveryMethods(() => {
            console.log('applePayViewModel fetchDeliveryMethods onComplete called');

            this.fetchTotals(() => {
                console.log('applePayViewModel fetchTotals onComplete called');

                let availableShippingMethods = this.consolidatedDispatchOptionsFromServer().map((d) => {
                    let minDateSplit = d.MinDeliveryDate.split(' ').map(i => Number(i));
                    let maxDateSplit = d.MaxDeliveryDate.split(' ').map(i => Number(i));
                    console.log('applePayViewModel mapping to shippingMethod', d);
                    return {
                        'label': d.DisplayName,
                        'detail': '',
                        'amount': `${Number(d.PriceInc).toFixed(2)}`,
                        'identifier': `${d.SortOrder}`,
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
                        'amount': `${this.total().toFixed(2)}`
                    },
                    'newLineItems': [{
                        'label': 'Sub Total',
                        'type': 'final',
                        'amount': `${this.subTotal().toFixed(2)}`
                    }, {
                        'label': 'Delivery',
                        'type': 'final',
                        'amount': `${this.shipping().toFixed(2)}`
                    }],
                    'errors': [],
                    newShippingMethods: availableShippingMethods
                };

                if (this.tax()) {
                    completeShippingContactModel.newLineItems.push({
                        'label': 'Tax',
                        'type': 'final',
                        'amount': `${this.tax().toFixed(2)}`
                    });
                }

                this.availableShippingMethods = availableShippingMethods;

                this.completeShippingContactModel = completeShippingContactModel;

                if (typeof onComplete === 'function')
                    onComplete.apply(this);
            });
        });
    }

    // This function saves the delivery address, then executes the onComplete callback function if provided.
    saveDeliveryAddress(data, onComplete) {
        console.log('applePayViewModel saveDeliveryAddress calling with', data);

        this.checkoutLog(LOG_ACTION_PAY_NOW_STEP, data);

        $.ajax({
            url: "/api/checkout/saveCheckoutAddress",
            type: "Post",
            cache: false,
            async: true,
            dataType: 'json',
            data: data,

            success: (function (data) {

                console.log('applePayViewModel saveDeliveryAddress returned', data);

                onComplete.apply(this, [data]);

            }).bind(this),

            error: (function (xhr, s, a) {

                console.error('applePayViewModel saveDeliveryAddress errored', xhr, s, a);

                this.abortApple();

                window.showInfo(this.resources.Sorry, this.resources.ApplePayFailureNotCharged);

            }).bind(this)
        });
    }

    // This is to be used when things go wrong
    // Unsets parent.busy
    // Calls hideCompletingMessage 
    // Calls session.abort wrapped in a try catch: the session might already be closed and no longer in an abortable state, which is fine.
    abortApple() {
        console.log('applePayViewModel abortApple called');

        this.parent.busy(false);

        this.hideCompletingMessage();

        try {
            this.activeApplePaySession.abort();
        } catch (e) {

        }
    }

    // This is the sucessful end, should only be called from submit success branch
    endAppleSuccessfully() {
        console.log('applePayViewModel endAppleSuccessfully called');
        this.checkoutLog(LOG_ACTION_APPLE_COMPLETEPAYMENT, 'completePayment(ApplePaySession.STATUS_SUCCESS)');
        this.activeApplePaySession.completePayment(ApplePaySession.STATUS_SUCCESS);
    }

    showPaymentProcessingError() {
        console.log('applePayViewModel showPaymentProcessingError called');
        bootstrap.Modal.getOrCreateInstance(document.querySelector("#payment-processing-error")).show();
    }

    hidePaymentProcessingError() {
        console.log('applePayViewModel hidePaymentProcessingError called');
        bootstrap.Modal.getOrCreateInstance(document.querySelector("#payment-processing-error")).hide();
    }

    showCompletingMessage() {
        console.log('applePayViewModel showCompletingMessage called');
        // The Completing Message modal reads "Completing your order \n PLEASE DO NOT REFRESH THE PAGE"
        bootstrap.Modal.getOrCreateInstance(document.querySelector("#checkout-complete-waiting")).show();

        // This blocks the entire page with an overlay which reads "Please wait..." and has a spinner-border.
        this.parent.busy(true);

        // This logs the fact that we've displayed the completing message.
        this.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'completing message shown');
    }

    hideCompletingMessage() {
        console.log('applePayViewModel hideCompletingMessage called');

        bootstrap.Modal.getOrCreateInstance(document.querySelector("#checkout-complete-waiting")).hide();

        this.parent.busy(false);

        // Somewhere out there something disables the proceed-btn elements, preventing you from mashing buttons to try and hurt our checkout.
        // When we're no longer showing the completing message, apparently we want to re-enable these buttons so people can have another go.
        document.querySelectorAll('.proceed-btn').forEach(pb => {
            pb.removeAttribute('disabled');
            pb.classList.remove('disabled');
        });

        // This logs the fact that we've hidden the completing message.
        this.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'completing message hidden');
    }

    saveDispatch(e, onComplete) {

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

        let data = this.dispatchOptionsFromServer().flatMap((x, i) => {
            if (x.SortOrder == e.shippingMethod.identifier) {
                return [{
                    dispatchId: x.DispatchID,
                    deliveryBandId: x.DeliveryBandID,
                    shouldPartShip: false,
                    minDeliveryDate: minDate.toISOString(),
                    maxDeliveryDate: maxDate.toISOString(),
                    customerRequestsFullDespatch: false
                }]
            } else {
                return []
            }
        });

        console.log('applePayViewModel saveDispatch calling with', data);

        $.ajax({
            url: "/api/checkout/saveDispatch",
            type: "Post",
            cache: false,
            async: true,
            data: JSON.stringify({
                dispatchLines: data
            }),
            contentType: 'application/json; charset=utf-8',
            success: (function (data) {
                console.log('applePayViewModel saveDispatch returned', data)
                onComplete.apply(this);
            }).bind(this),
            error: (function (xhr, s, a) {
                console.error('applePayViewModel saveDispatch errored', xhr, s, a)
                this.hideCompletingMessage();
                this.hidePaymentProcessingError();
                this.abortApple();
                window.showInfo(this.resources.Sorry, this.resources.ApplePayFailureNotCharged);
            }).bind(this)
        });
    }

    submit(nonce, email, phone) {

        this.showCompletingMessage();

        let defaultCard = false;

        let data = JSON.stringify({
            device_data: null,
            payment_method_nonce: nonce,
            masterDefinition: getMasterDefinition(),
            basketCountry: getSiteCountry(),
            marketingPermissions: [],
            make_default: defaultCard,
            email: email,
            phone: phone
        });

        this.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'applePayViewModel submit calling with', data);

        console.log('applePayViewModel submit calling with', data);

        $.ajax({
            url: '/api/checkout/checkoutBraintreeSubmit',
            type: 'POST',
            cache: false,
            async: true,
            data: data,
            contentType: 'application/json; charset=utf-8',
            success: (function (data) {

                console.log('applePayViewModel submit returned', data);

                this.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory submitAppleCheckout returned: ' + JSON.stringify(data));

                if (data != null) {
                    if (data.result === "OK") {
                        if (data.redirect === "confirm") {
                            // defunct and should never be hit
                            // window.location.href = '@dgFramework.buildUrl(Model.masterDefinition.UrlBreakdown.countryDetails, null, dgContentType.landingPage, (int)dgFixedPage.checkoutConfirmation, null, null)';
                        } else if (data.redirect === "orderGen") {
                            $('#checkout-complete-waiting .page-blocker').addClass('success');
                            $('#checkout-complete-waiting p').html(this.parent.resources.CheckoutPaymentSuccessGeneratingOrder);

                            this.showCompletingMessage();

                            this.endAppleSuccessfully();

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
                        this.hideCompletingMessage();
                        this.showPaymentProcessingError();
                    }
                }
                else {
                    this.hideCompletingMessage();
                    this.showPaymentProcessingError();
                }
            }).bind(this),
            error: (function (e, s, a) {

                console.error('applePayViewModel submit errored', e, s, a);

                this.checkoutLog(LOG_ACTION_PAY_NOW_STEP, 'paymentMethodFactory submitApple ERRORED: ' + JSON.stringify(e));

                this.hideCompletingMessage();

                this.hidePaymentProcessingError();

                this.abortApple();

                window.showInfo(this.resources.Sorry, this.resources.ApplePayFailureNotCharged);

            }).bind(this),
            timeout: 90000
        });
    }

    //#endregion
}