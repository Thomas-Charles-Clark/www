const loginViewModel = ((resources) => {
    var self = {
        resources: resources
    };

    //#region Properties

    self.sourceId = 62; // SourceID 62: Register (My Account / Moodboard)

    self.busy = ko.observable(false);

    self.registrationMode = ko.observable(false);

    self.redirect = ko.observable();

    self.validateLoginNow = ko.observable(false);

    self.validateForgottenNow = ko.observable(false);

    self.validateRegistrationNow = ko.observable(false);

    self.validLogin = ko.observable(false);

    //#region Login-Specific Stuff

    self.invalidLogin = ko.observable(false);

    self.loginEmailAddress = ko.observable().extend({
        email: {
            params: true,
            onlyIf: () => {
                return (self.validateLoginNow() || self.validateForgottenNow()) && self.registrationMode() == false
            },
            message: self.resources.FormsErrorEmailFormat
        },
        required: {
            params: true,
            onlyIf: () => {
                return (self.validateLoginNow() || self.validateForgottenNow()) && self.registrationMode() == false
            },
            message: self.resources.emailValidation
        },
        errorPulse: {}
    });

    self.loginPassword = ko.observable().extend({
        required: {
            params: true,
            onlyIf: () => {
                return self.validateLoginNow() && self.registrationMode() == false
            },
            message: self.resources.PleaseProvideYourPassword
        },
        errorPulse: {}
    });

    self.loginErrors = new ko.validation.group([
        self.loginEmailAddress,
        self.loginPassword
    ]);

    //#endregion

    //#region Forgotten Password Stuff

    self.forgotBusy = ko.observable(false);

    self.forgotSuccess = ko.observable(false);

    self.forgottenPasswordErrors = new ko.validation.group([
        self.loginEmailAddress
    ]);

    //#endregion

    //#region Registration related stuff

    self.registrationName = ko.observable().extend({
        required: {
            params: true,
            onlyIf: () => {
                return self.validateRegistrationNow() && self.registrationMode() == true
            },
            message: self.resources.nameValidation
        },
        errorPulse: {}
    });

    self.registrationEmailConfirm = ko.observable();

    self.registrationEmail = ko.observable().extend({
        email: {
            params: true,
            onlyIf: () => {
                return self.validateRegistrationNow() && self.registrationMode() == true
            },
            message: self.resources.FormsErrorEmailFormat
        },
        required: {
            params: true,
            onlyIf: () => {
                return self.validateRegistrationNow() && self.registrationMode() == true
            },
            message: self.resources.emailValidation
        },
        equal: {
            params: self.registrationEmailConfirm,
            onlyIf: () => self.validateRegistrationNow() && self.registrationMode() == true,
            message: self.resources.FormsErrorEmailCompare
        },
        errorPulse: {}
    });

    self.registrationEmailClone = ko.pureComputed(() => {
        return self.registrationEmail()
    }).extend({
        email: {
            params: true,
            message: self.resources.FormsErrorEmailFormat
        }
    });

    self.registrationEmailConfirm.extend({
        email: {
            params: true,
            onlyIf: () => {
                return self.validateRegistrationNow() && self.registrationMode() == true
            },
            message: self.resources.FormsErrorEmailFormat
        },
        required: {
            params: true,
            message: self.resources.emailValidation
        },
        equal: {
            params: self.registrationEmail,
            message: self.resources.FormsErrorEmailCompare
        },
        errorPulse: {}
    });

    self.registrationShowPassword = ko.observable(false);

    self.registrationPassword = ko.observable().extend({
        required: {
            params: true,
            onlyIf: () => {
                return self.validateRegistrationNow() && self.registrationMode() == true
            },
            message: self.resources.PleaseProvideYourPassword
        },
        minLength: {
            params: 6,
            message: self.resources.ManageAccountErrorPassword
        },
        zxcvbn: {
            restrictions: [
                self.registrationName,
                self.registrationEmail
            ]
        },
        errorPulse: {}
    });

    self.registrationAllCountries = ko.observableArray([]);

    self.registrationCountry = ko.observable();

    self.registrationAllLanguages = ko.observableArray([]);

    self.registrationLanguage = ko.observable();

    self.registrationExtantEmailCheckPerformed = ko.observable(false);

    self.registrationExtantEmailCheckPassed = ko.observable(false).extend({
        equal: {
            params: true,
            onlyIf: () => {
                return self.registrationExtantEmailCheckPerformed()
            },
            message: self.resources.RegistrationDuplicateText
        }
    });

    self.captchaFailed = ko.observable(false);

    // This must be determined based on the below user related properties and/or the user's response to marketingPermission checks
    self.mustResubscribe = ko.observable(false);

    // This is fed by the marketingPermissionsCheck
    self.userEmailMarketingPermitted = ko.observable(false);

    // This is fed by the marketingPermissionsCheck
    self.userUnsubscribedFromEmailMarketing = ko.observable(false);

    //#endregion

    //#endregion


    //#region Subscriptions

    self.loginEmailAddress.subscribe(nv => self.invalidLogin(false));

    self.registrationMode.subscribe(nv => {
        let carouselElement = document.querySelector('#login-form-carousel');
        let carousel = bootstrap.Carousel.getOrCreateInstance(carouselElement);
        carousel.to(nv ? 1 : 0);
    });

    self.registrationAllCountries.subscribe(nvs => {
        self.registrationCountry(nvs.filter(c => c.CountryID == getCountryId())[0]);
    })

    self.registrationCountry.subscribe(nv => {
        fetch('/api/generalapi/FetchLanguages', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-store',
            credentials: 'same-origin',
            body: JSON.stringify({
                countryId: nv.CountryID
            })
        })
        .then(window.handleFetchResponse)
        .then(data => self.registrationAllLanguages(data))
        .catch(window.catchFetchErrors);
    });

    self.registrationEmail.subscribe(nv => {
        self.marketingPermissions([]);
        self.marketingPermissionChecker.havePerformedMarketingPermissionCheck(false);
        self.registrationExtantEmailCheckPerformed(false);
    })

    //#endregion


    //#region Computeds

    self.marketingCountryId = ko.pureComputed(() => {
        if (self.registrationCountry()) {
            return self.registrationCountry().CountryID;
        } else {
            return undefined;
        }
    });

    self.checkRegistrationEmail = ko.computed(function () {

        if (self.registrationEmailClone() &&
            self.registrationEmailClone.isValid()) {
            var email = self.registrationEmailClone();
            if (email) {
                //self.busy(true);

                fetch('/api/member/extantEmailCheck', {
                    method: 'POST',
                    headers: window.fetchHeaders,
                    cache: 'no-store',
                    credentials: 'same-origin',
                    body: JSON.stringify({
                        email: self.registrationEmail()
                    })
                })
                    .then(window.handleFetchResponse)
                    .then(data => {
                        self.registrationExtantEmailCheckPerformed(true);
                        self.busy(false);
                        if (data.hasOwnProperty('error')) {
                            self.registrationExtantEmailCheckPassed(false);
                            self.registrationEmail.DOMElement.classList.add('validation-element');
                            self.registrationExtantEmailCheckPassed.isModified(true);
                        } else {
                            if (data) {
                                self.registrationExtantEmailCheckPassed(true);
                                self.registrationEmail.DOMElement.classList.remove('validation-element');
                                self.registrationExtantEmailCheckPassed.isModified(true);
                            }
                        }
                    })
                    .catch(err => {
                        self.busy(false);
                        self.registrationExtantEmailCheckPerformed(true);
                        self.registrationExtantEmailCheckPassed(false);
                        self.registrationExtantEmailCheckPassed.isModified(true);
                    });
            }
        }
    }, self).extend({
        rateLimit: {
            timeout: 300,
            method: 'notifyWhenChangesStop'
        }
    });

    //#endregion

    //#region Components -- Necessarily bumped down because marketingCountryId must exist before the permission checker

    // aliases: mpc requires email and emailConfirm

    self.email = self.registrationEmail;

    self.emailConfirmation = self.registrationEmailConfirm;

    self.marketingPermissionChecker = marketingPermissionCheckerViewModel(self);

    self.marketingPermissions = self.marketingPermissionChecker.marketingPermissions;

    self.registrationErrors = new ko.validation.group([
        self.registrationName,
        self.registrationEmail,
        self.registrationEmailConfirm,
        self.registrationPassword,
        self.registrationCountry,
        self.registrationLanguage,
        self.marketingPermissionChecker.havePerformedMarketingPermissionCheck,
        self.registrationExtantEmailCheckPassed
    ]);

    //#endregion

    //#region Methods

    self.signIn = () => {
        self.invalidLogin(false);
        self.validateLoginNow(true);
        if (self.loginErrors().length) {
            self.loginErrors.filter(x => {
                x.isValid.notifySubscribers(x.isValid())
            });
        } else {

            self.busy(true);
            
            fetch('/api/member/signIn', {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-store',
                credentials: 'same-origin',
                body: JSON.stringify({
                    email: self.loginEmailAddress(),
                    password: self.loginPassword(),
                    countryId: getCountryId(),
                    languageId: getLanguageId(),
                    accountType: 0 // "either"
                })
            })
                .then(window.handleFetchResponse)
                .then(data => {
                    //console.trace(data);

                    if (data.error) {
                        switch (data.message) {
                            case 'InvalidEmailFormat':
                                self.registrationEmail.isValid.notifySubscribers(false);
                                break;
                            case 'EmailDuplicate':
                                self.registrationEmail.isValid.notifySubscribers(false);
                                break;
                            case 'InvalidLogin':
                                self.invalidLogin(true);
                                break;
                            case 'checkoutTradeInvalidPermission':
                                window.showError();
                                break;
                            case 'InvalidCaptcha':
                                self.captchaFailed(true);
                                break;
                        }
                        self.busy(false);
                    } else {
                        if (data === true) {

                            self.validLogin(true);

                            if (typeof window.afterLogin === 'function') {
                                window.afterLogin();
                            } else {
                                if (self.redirect())
                                    window.location.href = self.redirect();
                                else
                                    window.location.href = '/' + getSiteCountry() + '/l0';
                            }
                        } else {
                            if (typeof data === 'object' && data.hasOwnProperty('redirect')) {

                                self.validLogin(true);

                                if (typeof window.afterLogin === 'function') {
                                    window.afterLogin();
                                } else {
                                    if (self.redirect())
                                        window.location.href = self.redirect();
                                    else
                                        window.location.href = data.redirect;
                                }
                            } else {
                                showError();
                                self.busy(false);
                            }
                        }
                    }
                })
                .catch(err => {
                    self.busy(false);
                    window.catchFetchErrors(err);
                });
        }
    }

    self.showForgottenPassword = () => {
        let forgottenPasswordElement = document.querySelector('#forgotten-password-modal');
        let forgottenPasswordModal = bootstrap.Modal.getOrCreateInstance(forgottenPasswordElement);
        forgottenPasswordModal.show();
    };

    self.submitForgottenPassword = () => {
        self.validateForgottenNow(true);
        if (self.loginErrors().length) {
            self.loginErrors.filter(x => {
                x.isValid.notifySubscribers(x.isValid())
            });
        } else {

            self.forgotBusy(true);

            fetch('/api/member/requestPasswordReset', {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-store',
                credentials: 'same-origin',
                body: JSON.stringify({
                    email: self.loginEmailAddress(),
                    countryId: getCountryId(),
                    languageId: getLanguageId()
                })
            })
                .then(window.handleFetchResponse)
                .then(data => {
                    console.trace(data);

                    if (data === true) {
                        self.forgotSuccess(true);
                    }

                    self.forgotBusy(false);
                })
                .catch(err => {
                    self.forgotBusy(false);
                    window.catchFetchErrors(err);
                });
        }
    }

    self.closeForgottenPassword = () => {
        let forgottenPasswordElement = document.querySelector('#forgotten-password-modal');
        let forgottenPasswordModal = bootstrap.Modal.getOrCreateInstance(forgottenPasswordElement);
        forgottenPasswordModal.hide();
        self.forgotSuccess(false);
    };
    
    self.register = () => {
        self.validateRegistrationNow(false);
        self.captchaFailed(false);
        setTimeout(() => {
            self.validateRegistrationNow(true);
            if (self.registrationErrors().length) {
                self.registrationErrors.showAllMessages(true);
                self.registrationErrors.filter(x => {
                    x.isValid.notifySubscribers(x.isValid())
                });
            } else {
                self.busy(true);

                let payload = {
                    name: self.registrationName(),
                    email: self.registrationEmail(),
                    password: self.registrationPassword(),
                    countryId: self.registrationCountry().CountryID,
                    languageId: self.registrationLanguage().LanguageID
                }

                try {
                    payload.gRecaptcha = grecaptcha.getResponse();
                } catch (e) {
                    window.logClientSideError(e);
                }

                fetch('/api/member/signUpNewMember', {
                    method: 'POST',
                    headers: window.fetchHeaders,
                    cache: 'no-store',
                    credentials: 'same-origin',
                    body: JSON.stringify(payload)
                })
                    .then(window.handleFetchResponse)
                    .then(data => {

                        try {
                            grecaptcha.reset();
                        }
                        catch (e) {

                        }

                        if (data.error) {
                            switch (data.message) {
                                case 'InvalidEmailFormat':
                                    self.registrationEmail.isValid.notifySubscribers(false);
                                    break;
                                case 'EmailDuplicate':
                                    self.registrationEmail.isValid.notifySubscribers(false);
                                    break;
                                case 'InvalidLogin':
                                    self.invalidLogin(true);
                                    break;
                                case 'checkoutTradeInvalidPermission':
                                    window.showError();
                                    break;
                                case 'InvalidCaptcha':
                                    self.captchaFailed(true);
                                    break;
                            }

                            self.busy(false);
                        } else {
                            if (data === true) {

                                self.validLogin(true);

                                if (typeof window.afterLogin === 'function') {
                                    window.afterLogin();
                                } else {
                                    window.setTimeout(() => {
                                        if (self.redirect())
                                            window.location.href = self.redirect();
                                        else
                                            window.location.href = '/' + getSiteCountry() + '/l0';
                                    }, 0);
                                }

                            } else {
                                showError();
                            }
                        }
                    })
                    .catch(err => {
                        try {
                            grecaptcha.reset();
                        }
                        catch (e) {

                        }
                        window.handleErrorMessage(err);
                    });
            }
        }, 0);
    }

    //#endregion

    window._lg = self;

    return self;
});
//fetchResources(['FormsErrorEmailFormat',
//    'nameValidation',
//    'emailValidation',
//    'PleaseProvideYourPassword',
//    'ManageAccountErrorPassword',
//    'ManageAccountPasswordStrength0',
//    'ManageAccountPasswordStrength1',
//    'ManageAccountPasswordStrength2',
//    'ManageAccountPasswordStrength3',
//    'ManageAccountPasswordStrength4',
//    'FormsErrorEmailCompare',
//    'RegistrationDuplicateText',
//    'LoginInvalid',
//    'PaypalInvalidAddress',
//    'PayWithCard',
//    'CheckoutTradeCheckout'
//])
//.then(window.handleResourcesArray)
//.then(resources => {
    window.loginModel = loginViewModel(window.getLoginResources());
    ko.applyBindings(window.loginModel, document.querySelector('#login-wrapper'));
    document.querySelector('#body-content').dispatchEvent(new Event('memberGuestLoginReady'));
//});