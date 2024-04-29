const tradeRegistrationViewModel = ((resources) => {
    let self = {
        resources: resources
    };

    //#region Properties

    self.guid = getAuthGuid();

    self.validateNow = ko.observable(false);

    self.tradeEmail = ko.observable(getTradeEmail()).extend({
        required: {
            params: true,
            onlyIf: function () {
                return self.validateNow();
            },
            message: self.resources.RegistrationErrorEmail
        },
        minLength: {
            params: 2,
            onlyIf: function () {
                return self.validateNow();
            },
            message: self.resources.RegistrationErrorEmail
        },
        email: {
            params: true,
            onlyIf: function () {
                return self.validateNow();
            },
            message: self.resources.RegistrationErrorEmailFormat
        }
    });

    self.tradePostcode = ko.observable().extend({
        required: {
            params: true,
            onlyIf: function () {
                return self.validateNow();
            },
            message: self.resources.TradeRegistrationErrorPostcode
        }
    });

    self.tradePhone = ko.observable().extend({
        required: {
            params: true,
            onlyIf: function () {
                return self.validateNow()
            },
            message: self.resources.FormsErrorPhone
        }
    });

    self.tradeAccountNumber = ko.observable().extend({
        required: {
            params: true,
            onlyIf: function () {
                return self.validateNow();
            },
            message: self.resources.ManageAccountErrorTradeAccountNumber
        },
        minLength: {
            params: 2,
            onlyIf: function () {
                return self.validateNow();
            },
            message: self.resources.ManageAccountErrorTradeAccountNumber
        }
    });

    self.tradeCheckResultText = ko.observable();

    self.tradeCheckResultErrorText = ko.observable();

    self.displayTradePrice = ko.observable(true);

    self.tradeName = ko.observable().extend({
        required: {
            params: true,
            onlyIf: function () {
                return self.validateNow();
            },
            message: self.resources.RegistrationErrorName
        },
        minLength: {
            params: 2,
            onlyIf: function () {
                return self.validateNow();
            },
            message: self.resources.RegistrationErrorName
        }
    });

    self.showPassword = ko.observable(false);

    self.tradePassword = ko.observable().extend({
        required: {
            params: true,
            message: self.resources.PleaseProvideYourPassword
        },
        zxcvbn: {
            restrictions: [
                self.tradeName,
                self.tradeAccountNumber,
                self.tradeEmail,
                self.tradePhone
            ]
        }
    });

    self.tradePassword.extend({
        zxcvbnValidator: {
            params: {
                selfReference: self.tradePassword,
                minScore: 2
            },
            message: self.resources.ManageAccountPasswordNotStrongEnough
        },
        minLength: {
            params: 6,
            message: self.resources.ManageAccountErrorPassword
        }
    });

    self.tradeSubmitBusy = ko.observable(false);

    self.tradeComplete = ko.observable(false);

    //#endregion


    //#region Components


    self.tradeEmailParams = {
        label: self.resources.RegistrationEmailLabel,
        observable: self.tradeEmail,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: false,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            $(self.observable.DOMElement).focus();
        },
        permitRevert: false,
        charsRemainingOptions: {
            maxLength: 256
        },
        disabled: true
    };

    self.tradeAccountNumberParams = {
        label: self.resources.ManageAccountTradeAccountNumber,
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
            maxLength: 50
        },
        disabled: false
    };

    self.tradeAccountInvoicePostcodeParams = {
        label: self.resources.TradeAccountInvoiceAddressPostcode,
        observable: self.tradePostcode,
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
        },
        disabled: false
    };

    self.tradeAccountPhoneParams = {
        label: self.resources.TradeAccountBusinessPhoneNumber,
        observable: self.tradePhone,
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
        },
        disabled: false
    };

    self.tradeAccountNameParams = {
        label: self.resources.TradeRegistrationNameLabel,
        observable: self.tradeName,
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
            maxLength: 50
        },
        disabled: false
    };

    self.tradeAccountPasswordParams = {
        label: self.resources.RegistrationPasswordLabel,
        observable: self.tradePassword,
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
            maxLength: 50 
        },
        disabled: false,
        showPassword: self.showPassword
    };

    self.tradeRegisterErrors = ko.validation.group([
        self.tradeName,
        self.tradeAccountNumber,
        self.tradePostcode,
        self.tradePhone,
        self.tradeEmail,
        self.tradePassword
    ]);

    if (!getDisplayAdmin()) {
        self.tradeRegisterErrors = ko.validation.group([
            self.tradeName,
            self.tradeEmail,
            self.tradePassword
        ]);
    }

    //#endregion


    //#region Subscriptions



    //#endregion


    //#region Computeds



    //#endregion


    //#region Methods

    self.readOnly = function () {
        $("input.form-control").each(function (index, item) {
            $(item).attr('disabled', true);
        });
    }

    self.tradeRegistrationSubmit = function () {
        self.tradeAssociation();
    }

    self.tradeAssociation = function () {
        self.validateNow(true);
        self.tradeRegisterErrors.showAllMessages(false);
        if (self.tradeRegisterErrors().length) {
            self.tradeRegisterErrors.showAllMessages(true);
            self.tradeRegisterErrors.filter(x => {
                x.isValid.notifySubscribers(x.isValid());
            });
            return;
        }
        self.tradeSubmitBusy(true);
        $.ajax({
            url: "/api/member/memberTradeAccountCompleteAuthentication",
            type: "Post",
            cache: false,
            async: true,
            data: JSON.stringify({
                guid: self.guid,
                fullName: self.tradeName(),
                accountNumber: self.tradeAccountNumber(),
                postcode: self.tradePostcode(),
                phone: self.tradePhone(),
                email: self.tradeEmail(),
                password: self.tradePassword(),
                displayTradePrice: self.displayTradePrice(),
                masterDefinition: getMasterDefinition()
            }),
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                self.tradeSubmitBusy(false);
                if (data.error) {
                    switch (data.message) {
                        case 'MemberUpdateFailed':
                            self.tradeCheckResultText(null);
                            self.tradeCheckResultErrorText(self.resources.ManageAccountErrorTradeAccountNumberUnverified);
                            break;
                        case 'accountNotConfiguredCorrectly':
                            self.tradeCheckResultText(null);
                            self.tradeCheckResultErrorText(self.resources.ManageAccountErrorTradeAccountNumberConfiguration);
                            break;
                        case 'accountNotFoundInMertex':
                            self.tradeCheckResultText(null);
                            self.tradeCheckResultErrorText(self.resources.ErrorTradeAccountNumberNotFound);
                            break;
                        case 'mertexCommunicationError':
                            self.tradeCheckResultText(null);
                            self.tradeCheckResultErrorText(self.resources.ManageAccountErrorTradeAccountNumberTransient);
                            break;
                        default:
                            self.tradeCheckResultText(null);
                            self.tradeCheckResultErrorText(data.message);
                            break;
                    }
                } else {
                    self.tradeCheckResultErrorText(undefined);
                    self.tradeComplete(true);
                    self.tradeCheckResultText(self.resources.TradeRegistrationCompleteMessage);
                }

                $('#trade-menu').html(data);
            },
            error: function (xhr, textStatus, errorThrown) {
                self.tradeSubmitBusy(false);
                console.log('dg-memberTradeAccountNumberUpdate: ' + textStatus);
                window.showError();
            }
        });
    }

    //#endregion

    return self;
});

window.tradeRegistrationModel = tradeRegistrationViewModel(getPrefetchedResources());

ko.applyBindings(window.tradeRegistrationModel, document.querySelector('#trade-wrapper'));