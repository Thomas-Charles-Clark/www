const tradeLoginDetailsViewModel = ((resources) => {
    let self = {
        resources: resources
    };

    //#region Properties

    self.emailToCheckForMarketing = ko.observable()

    self.editingEmail = ko.observable(false);

    self.showPassword = ko.observable(false);

    self.editingPassword = ko.observable(false);

    self.checkEmailBusy = ko.observable(false);

    self.extantEmailCheckPassed = ko.observable().extend({
        required: {
            params: true,
            onlyIf: self.emailPreChecksPass,
            message: self.resources.RegistrationDuplicateText
        },
        equal: {
            params: true,
            onlyIf: self.emailPreChecksPass,
            message: self.resources.RegistrationDuplicateText
        }
    });

    self.saveBusy = ko.observable(false);

    //#endregion


    //#region Components

    self.fullName = {
        observable: ko.observable(getFullName()).extend({
            required: {
                params: true,
                message: self.resources.ManageAccountErrorName
            },
            minLength: {
                params: 2,
                message: self.resources.ManageAccountErrorName
            }
        }),
        originalValue: ko.observable(getFullName()),
        charsRemainingOptions: {
            maxLength: 50
        },
        updated: ko.observable(false)
    }

    self.email = {
        observable: ko.observable(getEmail()).extend({
            required: {
                params: true,
                onlyIf: self.editingEmail,
                message: self.resources.ManageAccountErrorEmail
            },
            email: {
                params: true,
                message: self.resources.ManageAccountErrorEmailFormat
            },
            minLength: {
                params: 5,
                message: self.resources.ManageAccountErrorEmailFormat
            },
        }),
        focus: ko.observable(false),
        originalValue: ko.observable(getEmail()),
        charsRemainingOptions: {
            maxLength: 254
        },
        updated: ko.observable(false)
    };

    self.confirmEmail = {
        observable: ko.observable().extend({
            equal: {
                params: self.email.observable,
                onlyIf: self.editingEmail,
                message: self.resources.ManageAccountErrorEmailCompare
            }
        }),
        focus: ko.observable(false),
        charsRemainingOptions: {
            maxLength: 254
        }
    }

    self.currentPassword = {
        observable: ko.observable('&#8226;&#8226;&#8226;').extend({
            required: {
                params: true,
                onlyIf: () => {
                    return self.editingPassword()
                },
                message: self.resources.ManageAccountErrorPassword
            }
        }),
        charsRemainingOptions: {
            maxLength: 50
        },
        updated: ko.observable(false)
    }

    self.newPassword = {
        observable: ko.observable().extend({
            required: {
                params: true,
                onlyIf: function () {
                    return self.editingPassword()
                },
                message: self.resources.ManageAccountErrorPassword
            },
            minLength: {
                params: 6,
                onlyIf: function () {
                    return self.editingPassword()
                },
                message: self.resources.ManageAccountErrorPassword
            },
            zxcvbn: {}
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
            onlyIf: function () {
                return self.editingPassword()
            },
            message: self.resources.ManageAccountPasswordNotStrongEnough
        }
    });

    self.confirmNewPassword = {
        observable: ko.observable().extend({
            required: {
                params: true,
                onlyIf: function () {
                    return self.editingPassword()
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
                    return self.editingPassword()
                },
                message: self.resources.ManageAccountErrorPassword
            },
            equal: {
                params: self.newPassword.observable,
                onlyIf: function () {
                    return self.editingPassword()
                },
                message: self.resources.ManageAccountErrorPasswordCompare
            }
        }),
        charsRemainingOptions: {
            maxLength: 50
        }
    }

    self.nameErrors = ko.validation.group([
        self.fullName.observable
    ]);

    self.emailErrors = ko.validation.group([
        self.email.observable,
        self.confirmEmail.observable,
        self.extantEmailCheckPassed
    ]);

    self.passwordErrors = ko.validation.group([
        self.currentPassword.observable,
        self.newPassword.observable,
        self.confirmNewPassword.observable
    ]);

    //#endregion


    //#region Subscriptions



    self.newPassword.observable.subscribe(function (newValue) {
        //self.unsavedChanges(true);
    });

    self.confirmNewPassword.observable.subscribe(function (newValue) {
        //self.unsavedChanges(true);
    });


    self.editingEmail.subscribe(function (newValue) {
        if (!newValue) {
            self.email.observable(self.email.originalValue());
            self.confirmEmail.observable(undefined);
            self.confirmEmail.observable.isModified(false);
        } else {
            self.email.updated(false);
            $(self.email.observable.DOMElement).closest('.member-account-row').removeClass('save-successful');
            window.setTimeout(function () {
                $(self.email.observable.DOMElement).select();
            }, 0);
        }
    });

    self.editingPassword.subscribe(function (newValue) {
        if (newValue) {
            $(self.currentPassword.observable.DOMElement).closest('.member-account-row').removeClass('save-successful');
            self.currentPassword.updated(false);
            self.currentPassword.observable(undefined);
            self.currentPassword.observable.isModified(false);
            self.newPassword.observable(undefined);
            self.newPassword.observable.isModified(false);
            self.confirmNewPassword.observable(undefined);
            self.confirmNewPassword.observable.isModified(false);
            window.setTimeout(function () {
                $(self.currentPassword.observable.DOMElement).focus();
            }, 0);
            self.passwordErrors.showAllMessages(true);
        } else {
            self.currentPassword.observable('&#8226;&#8226;&#8226;');
            self.newPassword.observable(undefined);
            self.newPassword.observable.isModified(false);
            self.showPassword(false);
            self.passwordErrors.showAllMessages(false);
        }
    });


    self.fullName.observable.subscribe(function (newValue) {
        self.fullName.updated(false);
        $(self.fullName.observable.DOMElement).closest('.member-account-row').removeClass('save-successful');
    });


    self.email.focus.subscribe(function (newValue) {
        if (!newValue) {
            if (self.confirmEmail.observable.isValid()) {
                self.checkEmail();
            }
        }
    });

    self.email.observable.subscribe(function (newValue) {
        self.extantEmailCheckPassed.isModified(false);
        if (self.confirmEmail.observable.isValid()) {
            self.checkEmail();
        }
    });

    self.confirmEmail.observable.subscribe(function (newValue) {
        self.extantEmailCheckPassed.isModified(false);
    });

    self.confirmEmail.focus.subscribe(function (newValue) {
        if (!newValue) {
            if (self.confirmEmail.observable.isValid()) {
                self.checkEmail();
            }
        }
    });

    //#endregion


    //#region Computeds


    self.emailPreChecksPass = ko.pureComputed(function () {
        return self.editingEmail() && self.email.observable.isValid() && self.confirmEmail.observable.isValid();
    }, self);

    //#endregion


    //#region Methods

    self.checkEmail = () => {
        self.checkEmailBusy(true);
        $.ajax({
            url: '/api/member/extantEmailCheck',
            type: 'POST',
            dataType: 'json',
            data: {
                email: self.confirmEmail.observable()
            },
            success: function (data) {
                self.checkEmailBusy(false);
                if (data.error) {
                    self.extantEmailCheckPassed(false);
                } else {
                    self.extantEmailCheckPassed(true);
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                self.extantEmailCheckPassed(false);
                window.showError();
            }
        });
    }
    self.saveName = function () {
        if (self.nameErrors().length > 0) {
            self.nameErrors.showAllMessages(true);
            return;
        } else {
            self.saveBusy(true);

            var data = {
                name: self.fullName.observable()
            };

            $.ajax({
                url: '/api/tradeapi/tradeLoginDetailsSaveFullname',
                type: 'POST',
                dataType: 'json',
                data: data,
                success: function (data) {
                    // show name save tick and fade it out

                    if (data.hasOwnProperty('handleErrorMessage')) {
                        switch (data.error) {
                            case 'MemberUpdateFailed':
                                self.fullName.observable.setError(self.resources.ManageAccountSaveFailed);
                                self.nameErrors.showAllMessages(true);
                                $(self.fullName.observable.DOMElement).select();
                                break;
                            default:
                                window.showError();
                                break;
                        }
                    } else {
                        $(self.fullName.observable.DOMElement).closest('.member-account-row').addClass('save-successful');
                        self.fullName.updated(true);
                        self.fullName.originalValue(self.fullName.observable());
                        $("#topnav-name").text(self.fullName.observable());
                        window.setTimeout(() => {
                            $(self.fullName.observable.DOMElement).closest('.member-account-row').removeClass('save-successful');
                        }, 10000);
                    }

                    self.saveBusy(false);
                },
                error: function (xhr, textStatus, errorThrown) {
                    self.saveBusy(false);
                    window.showError();
                }
            });
        }
    }

    self.saveEmail = function () {
        if (self.emailErrors().length > 0) {
            self.emailErrors.showAllMessages(true);
            return;
        } else {
            self.saveBusy(true);

            var data = {
                Email: self.email.observable()
            };

            $.ajax({
                url: '/api/tradeapi/tradeLoginDetailsSaveEmail',
                type: 'POST',
                dataType: 'json',
                data: data,
                success: function (data) {
                    // show email save tick and fade it out

                    if (data.hasOwnProperty('handleErrorMessage')) {
                        switch (data.handleErrorMessage) {
                            case 'MemberUpdateFailed':
                                self.email.observable.setError(self.resources.ManageAccountSaveFailed);
                                self.emailErrors.showAllMessages(true);
                                $(self.email.observable.DOMElement).select();
                                break;
                            default:
                                window.showError();
                                break;
                        }
                    } else {
                        if (self.editingEmail()) {
                            self.email.originalValue(self.email.observable());
                            self.editingEmail(false);
                            $(self.email.observable.DOMElement).closest('.member-account-row').addClass('save-successful');
                            self.email.updated(true);
                            window.setTimeout(() => {
                                $(self.email.observable.DOMElement).closest('.member-account-row').removeClass('save-successful');
                            }, 10000);
                        }
                    }

                    self.saveBusy(false);
                },
                error: function (xhr, textStatus, errorThrown) {
                    self.saveBusy(false);
                    window.showError();
                }
            });
        }
    }

    self.savePassword = function () {
        if (self.passwordErrors().length > 0) {
            self.passwordErrors.showAllMessages(true);
            $(self.currentPassword.observable.DOMElement).select();
            return;
        } else {
            self.saveBusy(true);

            var data = {
                CurrentPassword: self.currentPassword.observable(),
                NewPassword: self.newPassword.observable()
            };

            $.ajax({
                url: '/api/tradeapi/tradeLoginDetailsSavePassword',
                type: 'POST',
                dataType: 'json',
                data: data,
                success: function (data) {

                    if (data.hasOwnProperty('handleErrorMessage')) {
                        switch (data.handleErrorMessage) {
                            case 'MemberUpdateFailed':
                                self.currentPassword.observable.setError(self.resources.ManageAccountSaveFailed);
                                self.passwordErrors.showAllMessages(true);
                                $(self.currentPassword.observable.DOMElement).select();
                                break;
                            default:
                                window.showError();
                                break;
                        }
                    } else {
                        // show password save tick and fade it out
                        self.currentPassword.updated(true);
                        $(self.currentPassword.observable.DOMElement).closest('.member-account-row').addClass('save-successful');
                        if (self.editingPassword()) {
                            self.editingPassword(false);
                        }
                        window.setTimeout(() => {
                            $(self.currentPassword.observable.DOMElement).closest('.member-account-row').removeClass('save-successful');
                        }, 10000);
                    }

                    self.saveBusy(false);
                },
                error: function (xhr, textStatus, errorThrown) {
                    self.saveBusy(false);
                    window.showError();
                }
            });
        }
    }

    //#endregion

    return self;
});

window.tradeLoginDetailsModel = tradeLoginDetailsViewModel(getPrefetchedResources());

ko.applyBindings(window.tradeLoginDetailsModel, document.querySelector('#trade-wrapper'));