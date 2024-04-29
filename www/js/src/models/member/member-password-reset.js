const passwordResetViewModel = ((resources) => {
    let self = {
        resources: resources
    };

    //#region Properties

    self.busy = ko.observable(false);

    self.showPassword = ko.observable(false);

    self.validateNow = ko.observable(false);

    self.token = ko.observable(getToken());

    self.tokenValid = ko.observable(getTokenValidity());

    self.validateEmailNow = ko.observable(false);

    self.email = {
        observable: ko.observable().extend({
            required: {
                params: true,
                onlyIf: () => {
                    return self.validateEmailNow();
                },
                message: self.resources.ManageAccountErrorEmail
            },
            email: {
                params: true,
                onlyIf: () => {
                    return self.validateEmailNow();
                },
                message: self.resources.ManageAccountErrorEmailFormat
            },
            minLength: {
                params: 5,
                onlyIf: () => {
                    return self.validateEmailNow();
                },
                message: self.resources.ManageAccountErrorEmailFormat
            },
        }),
        focus: ko.observable(false),
        charsRemainingOptions: {
            maxLength: 254
        },
        updated: ko.observable(false)
    };

    self.forgotSuccess = ko.observable(false);

    //#endregion


    //#region Components

    self.newPassword = {
        observable: ko.observable().extend({
            required: {
                params: true,
                onlyIf: function () {
                    return self.validateNow()
                },
                message: self.resources.ManageAccountErrorPassword
            },
            minLength: {
                params: 6,
                onlyIf: function () {
                    return self.validateNow()
                },
                message: self.resources.ManageAccountErrorPassword
            },
            zxcvbn: {},
            errorPulse: {}
        }),
        charsRemainingOptions: {
            maxLength: 50
        }
    }

    self.newPassword.observable.extend({
        zxcvbnValidator: {
            params: {
                selfReference: self.newPassword.observable,
                minScore: 2
            },
            message: self.resources.ManageAccountPasswordNotStrongEnough
        }
    });

    self.confirmNewPassword = {
        observable: ko.observable().extend({
            required: {
                params: true,
                onlyIf: function () {
                    return self.validateNow()
                },
                message: self.resources.ManageAccountErrorPassword
            },
            charsRemaining: {
                maxLength: 50,
                elementId: '#new-password'
            },
            minLength: {
                params: 6,
                onlyIf: function () {
                    return self.validateNow()
                },
                message: self.resources.ManageAccountErrorPassword
            },
            equal: {
                params: self.newPassword.observable,
                onlyIf: function () {
                    return self.validateNow()
                },
                message: self.resources.ManageAccountErrorPasswordCompare
            },
            errorPulse: {}
        }),
        charsRemainingOptions: {
            maxLength: 50
        }
    }

    self.errors = ko.validation.group([
        self.newPassword.observable,
        self.confirmNewPassword.observable
    ]);

    //#endregion


    //#region Subscriptions



    //#endregion


    //#region Computeds



    //#endregion


    //#region Methods

    self.save = (onComplete, sources) => {
        self.validateNow(false);
        self.validateNow(true);
        self.errors.showAllMessages(false);
        if (self.errors().length) {
            self.errors.showAllMessages(true);
            self.errors.filter(x => {
                x.isValid.notifySubscribers(x.isValid());
            });
        } else {
            self.busy(true);
            let data = {
                newPassword: self.newPassword.observable(),
                token: self.token()
            };
            fetch('/api/member/passwordReset', {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-store',
                credentials: 'same-origin',
                body: JSON.stringify(data)
            })
                .then(window.handleFetchResponse)
                .then(response => {
                    if (response.error) {
                        window.showError();
                    } else {
                        window.showInfo(self.resources.PasswordReset, self.resources.PasswordResetSuccessMessage, () => {
                            window.location.href = '/l102';
                        });
                    }
                    self.busy(false);
                })
                .catch(err => {
                    self.busy(false);
                    window.showError();
                })
        }
    }

    self.requestPasswordReset = () => {
        self.validateEmailNow(false);
        self.validateEmailNow(true);
        if (self.email.observable.isValid()) {
            self.busy(true);
            fetch('/api/member/requestPasswordReset', {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-store',
                credentials: 'same-origin',
                body: JSON.stringify({
                    email: self.email.observable(),
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

                    self.busy(false);
                })
                .catch(err => {
                    self.busy(false);
                    window.catchFetchErrors(err);
                });
        } else {
            self.email.observable.isValid.notifySubscribers(false);
        }
    }

    //#endregion

    return self;
});

fetchResources([
    'ManageAccountErrorPassword',
    'ManageAccountPasswordNotStrongEnough',
    'ManageAccountErrorPasswordCompare',
    'ManageAccountPasswordStrength',
    'Save',
    'FormsEmail',
    'ManageAccountErrorEmail',
    'ManageAccountErrorEmailFormat',
    'PasswordReset',
    'PasswordResetSuccessMessage',
    'MajorError'
])
    .then(window.handleResourcesArray)
    .then(resources => {
        window.passwordResetModel = passwordResetViewModel(resources);
        ko.applyBindings(window.passwordResetModel, document.querySelector('.member-wrapper'));
    });