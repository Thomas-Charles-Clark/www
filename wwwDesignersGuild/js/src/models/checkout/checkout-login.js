const checkoutLoginViewModel = ((parent) => {
    let self = {
        parent: parent,
        resources: parent.resources
    };

    //#region Properties

    self.busy = ko.observable(false);

    self.validateGuestNow = ko.observable(false);

    self.guestEmail = ko.observable().extend({
        email: {
            params: true,
            onlyIf: () => {
                return self.validateGuestNow();
            },
            message: self.resources.FormsErrorEmailFormat
        },
        required: {
            params: true,
            onlyIf: () => {
                return self.validateGuestNow();
            },
            message: self.resources.emailValidation
        },
        errorPulse: {}
    });

    self.guestErrors = new ko.validation.group([
        self.guestEmail
    ]);

    self.loginModalElement = document.querySelector('#guest-member-login-modal');

    self.loginModal = bootstrap.Modal.getOrCreateInstance(self.loginModalElement);

    // PayPal, Amazon Pay
    self.loginToken = ko.observable();

    // PayPal? Amazon Pay?
    /*
        public string companyName { get; set; }
        public string addressName { get; set; }
        public string addressLineOne { get; set; }
        public string addressLineTwo { get; set; }
        public string addressTown { get; set; }
        public string addressCounty { get; set; }
        public string addressPostCode { get; set; }
        public string addressCountryCode { get; set; }
        public string addressPhone { get; set; }
        public string memberCountry { get; set; }
    */
    self.initialShippingAddress = ko.observable();

    //#endregion


    //#region Components


    //#endregion


    //#region Subscriptions



    //#endregion


    //#region Computeds



    //#endregion


    //#region Methods

    self.invokeTradeLogin = () => {
        self.loginModalElement.classList.add('trade');
        self.loginModalElement.querySelector('.modal-title').innerHTML = self.resources.CheckoutTradeCheckout;
        self.loginModalElement.addEventListener('bs.hide.modal', self.undoTradeMods);
        document.querySelector('#first-blocker').style.display = 'none';
        window.afterLogin = () => {
            window.setTimeout(function () {
                window.proceedToCheckout('Trade');
            }, 0);
        }
        self.loginModal.show();
    }

    self.undoTradeMods = () => {
        self.loginModalElement.classList.remove('trade');
        self.loginModalElement.querySelector('.modal-title').innerHTML = self.resources.PayWithCard;
        document.querySelector('#first-blocker').style.display = 'flex';
        window.afterLogin = () => {
            window.setTimeout(function () {
                window.proceedToCheckout('Member');
            }, 0);
        }
        self.loginModalElement.removeEventListener('bs.hide.modal', self.undoTradeMods);
    }

    self.proceedAsGuest = () => {
        self.validateGuestNow(false);
        setTimeout(() => {
            self.validateGuestNow(true);
            if (self.guestErrors().length) {
                self.guestErrors.showAllMessages(true);
                self.guestErrors.filter(e => {
                    e.isValid.notifySubscribers(e.isValid())
                })
            } else {
                self.busy(true);
                document.querySelector('#first-blocker').style.display = 'none';
                fetch('/api/checkout/setCheckoutType', {
                    method: 'POST',
                    headers: window.fetchHeaders,
                    cache: 'no-store',
                    credentials: 'same-origin',
                    body: JSON.stringify({
                        checkoutType: 'guest',
                        guestEmail: self.guestEmail(),
                        token: self.loginToken(),
                        basketCountry: window.getSiteCountry(),
                        initialShippingAddress: self.initialShippingAddress()
                    })
                })
                    .then(window.handleFetchResponse)
                    .then(data => {
                        //console.trace(data);

                        if (data.error) {
                            switch (data.message) {
                                case 'checkoutBasketExpired':
                                    jumpToSessionExpired();
                                    break;
                                case 'invalidDeliveryAddress': {
                                    if (shippingAddress) {
                                        var addressLine = [shippingAddress.addressLineOne, shippingAddress.addressPostCode, shippingAddress.addressCountryCode].join(', ');
                                        asyncCheckoutLog('invalidAddress', addressLine);
                                    }
                                    displayPaymentAddressError();
                                }
                                break;
                            }
                        } else {
                            window.location.href = data.redirect;
                        }
                    })
                    .catch(err => {
                        self.busy(false);
                        document.querySelector('#first-blocker').style.display = 'flex';
                        window.catchFetchErrors(err);
                    })
            }
        }, 0);
    }

    //#endregion

    window.clm = self;

    return self;
});