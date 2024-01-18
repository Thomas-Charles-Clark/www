const tradeRequestAccessViewModel = ((resources) => {
    let self = {
        resources: resources
    };

    //#region Properties

    self.busy = ko.observable(false);

    if (window.loginModel) {
        self.busy = window.loginModel.busy;
    }

    self.success = ko.observable(false);

    self.notification = ko.observable();

    self.resultClass = ko.observable();

    self.validateNow = ko.observable(false);

    self.tradeAccountNumber = ko.observable();

    self.tradeAccountName = ko.observable();

    self.tradeAccountNumber.extend({
        required: {
            params: true,
            onlyIf: function () {
                return self.validateNow() && (self.tradeAccountName() == null)
            },
            message: self.resources.TradeAccountNumberOrName
        }
    });

    self.tradeAccountName.extend({
        required: {
            params: true,
            onlyIf: function () {
                return self.validateNow() && (self.tradeAccountNumber() == null)
            },
            message: self.resources.TradeAccountNumberOrName
        }
    });

    self.email = ko.observable().extend({
        required: {
            params: true,
            onlyIf: function () {
                return self.validateNow();
            },
            message: self.resources.emailValidation
        },
        email: {
            params: true,
            onlyIf: function () {
                return self.validateNow()
            },
            message: self.resources.emailValidationFormat
        }
    });

    self.userName = ko.observable().extend({
        required: {
            params: true,
            onlyIf: function () {
                return self.validateNow();
            },
            message: self.resources.nameValidation
        }
    });

    self.phone = ko.observable().extend({
        validation: {
            validator: function (val, parms) {
                let isValid = true;
                if (self.validateNow()) {
                    let p = self.phone();
                    if (p == null)
                        return isValid;
                    if (self.phone.hasOwnProperty('intlTel')) {
                        if (self.phone.intlTel.getNumber()) {
                            if (!self.phone.intlTel.isValidNumber()) {
                                hasPhoneErrored = true;
                                self.phone.isValid.notifySubscribers(false);
                                isValid = false;
                            }
                        } else {
                            if (!self.phone.isValid()) {
                                isValid = false;
                                self.phone.isValid.notifySubscribers(false);
                            }
                        }
                    }
                }
                return isValid;
            },
            message: self.resources.phoneInvalid
        },
        intTelMobile: true
    });

    //#endregion


    //#region Components

    self.tradeAccountNumberParams = {
        label: self.resources.RegistrationTradeAccountNumberLabel,
        observable: self.tradeAccountNumber,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            $(self.observable.DOMElement).focus();
        },
        permitRevert: false,
        charsRemainingOptions: {
            maxLength: 20
        }
    };

    self.tradeAccountNameParams = {
        label: self.resources.TradeAccountName,
        observable: self.tradeAccountName,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            $(self.observable.DOMElement).focus();
        },
        permitRevert: false,
        charsRemainingOptions: {
            maxLength: 100
        }
    };

    self.emailParams = {
        label: self.resources.MoodboardShareMyEmailLabel,
        observable: self.email,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            $(self.observable.DOMElement).focus();
        },
        permitRevert: false,
        charsRemainingOptions: {
            maxLength: 254
        }
    };

    self.userNameParams = {
        label: self.resources.TradeRegistrationNameLabel,
        observable: self.userName,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            $(self.observable.DOMElement).focus();
        },
        permitRevert: false,
        charsRemainingOptions: {
            maxLength: 100
        }
    };

    self.phoneParams = {
        label: self.resources.FormsPhoneNumber,
        observable: self.phone,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            $('body').focus();
        },
        permitRevert: false,
        charsRemainingOptions: {
            maxLength: 20
        },
        countryObservable: ko.observable(),
        preferredCountries: [getCountryCode()]
    };

    self.errors = new ko.validation.group([
        self.tradeAccountNumber,
        self.tradeAccountName,
        self.email,
        self.userName,
        self.phone
    ]);

    //#endregion


    //#region Subscriptions



    //#endregion


    //#region Computeds



    //#endregion


    //#region Methods

    self.submit = function () {
        self.validateNow(true);
        window.setTimeout(function () {
            if (self.errors().length) {
                self.errors.filter(function (x) {
                    x.isValid.notifySubscribers(x.isValid());
                });
            } else {
                self.busy(true);
                var data = {
                    tradeAccountName: self.tradeAccountName(),
                    tradeAccountCode: self.tradeAccountNumber(),
                    userName: self.userName(),
                    userEmail: self.email(),
                    userPhone: self.phone()
                };
                console.log('calling requestTradeAccess with', data);
                $.ajax({
                    url: '/api/tradeapi/RequestTradeAccess',
                    type: 'POST',
                    dataType: 'json',
                    data: data,
                    success: function (data) {
                        self.busy(false);

                        if (data.success) {
                            self.success(true);
                        }

                        if (data.notification) {
                            self.notification(data.notification);
                        }

                        if (data.resultClass) {
                            self.resultClass(data.resultClass);
                        }

                        //window.setTimeout(function () {
                        //    self.showSuccess(false);
                        //}, 10000);
                        console.log('requestTradeAccess returned', data);
                    },
                    error: function (xhr, s, a) {
                        self.busy(false);
                        alert(self.resources.MajorError);
                    }
                });
            }
        }, 0);
    }

    //#endregion

    return self;
});

fetchResources([
    'RegistrationTradeAccountNumberLabel',
    'TradeAccountName',
    'TradeAccountNumberOrName',
    'TradeRegistrationNameLabel',
    'MoodboardShareMyEmailLabel',
    'email',
    'emailValidation',
    'emailValidationFormat',
    'TradeRegistrationNameLabel',
    'nameValidation',
    'TradeAccountTelephoneNumber',
    'FormsPhoneNumber',
    'phoneInvalid',
    'MajorError'
])
    .then(window.handleResourcesArray)
    .then(resources => {
        window.tradeRequestAccessModel = tradeRequestAccessViewModel(resources);
        ko.applyBindings(window.tradeRequestAccessModel, document.querySelector('#trade-wrapper'));
    });