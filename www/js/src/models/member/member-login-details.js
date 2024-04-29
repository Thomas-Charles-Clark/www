// yet another override, see common-models.js:364 and checkoutsinglepage.js:6684
var marketingPermission = ((mp) => {
    let self = Object.assign({}, mp);

    self.Checked = ko.observable(self.Checked);

    return self;
});


const memberLoginDetailsViewModel = ((parent) => {
    let self = {
        parent: parent,
        resources: parent.resources
    };

    //#region Properties

    self.emailToCheckForMarketing = ko.observable()

    self.editingEmail = ko.observable(false);

    self.showPassword = ko.observable(false);

    self.editingPassword = ko.observable(false);

    self.checkEmailBusy = ko.observable(false);

    self.saveBusy = ko.observable(false);

    self.marketingCountryId = ko.observable(getMarketingCountryId());

    self.marketingPermissions = ko.observableArray(getMarketingPermissions().Permissions.map(mp => marketingPermission(mp)));

    self.availableMarketingCountries = ko.observableArray(getMarketingCountries());
    
    // A list of available languages
    self.availableLanguages = ko.observableArray(getMarketingLanguages());

    // Selected language: some countries present with multiple languages
    self.marketingLanguageId = ko.observable(getMarketingLanguageId());

    // We have a computed marshalling marketingCountryId and marketingLanguageId subscriptions,
    // but we don't want it to fire the save just because it ran the initial scrape for dependencies
    self.skipFirstMarketingComputation = true;

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

    // I know, I know, wrong place - necessary, sorry.
    self.emailPreChecksPass = ko.pureComputed(function () {
        return self.editingEmail() && self.email.observable.isValid() && self.confirmEmail.observable.isValid();
    }, self);

    // I know, I know, wrong place - necessary, sorry.
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

    self.marketingPermissions().forEach(mp => {
        mp.Checked.subscribe(nv => {
            self.save(null, [mp.Checked]);
        });
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

    self.marketingLanguageId.subscribe(nv => {
        console.log('marketingLanguageId', nv);
    });

    self.marketingCountryId.subscribe(nv => {
        console.log('marketingCountryId', nv);
    });

    //#endregion


    //#region Computeds


    self.currentlyAvailableLanguages = ko.computed(() => {
        return self.availableLanguages().filter(l => l.CountryID == self.marketingCountryId());
    }, self);

    self.marketingLanguageId.extend({
        required: {
            params: true,
            onlyIf: () => {
                return self.currentlyAvailableLanguages().length > 1
            },
            message: self.resources.FormsLanguageRequired
        }
    });

    //#endregion

    //#region Error Group

    self.errors = ko.validation.group([
        self.fullName.observable,
        self.email.observable,
        self.confirmEmail.observable,
        self.extantEmailCheckPassed,
        self.currentPassword.observable,
        self.newPassword.observable,
        self.confirmNewPassword.observable,
        self.marketingLanguageId
    ]);

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

    self.refreshMarketingPermissions = () => {
        fetch('/api/form/CheckMarketingPermission', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-store',
            credentials: 'same-origin',
            body: JSON.stringify({
                memberId: null,
                email: self.email.observable(),
                addressRequired: true,
                countryId: getCountryId(),
                languageId: getLanguageId(),
                sourceId: 50,
                marketingCountryId: self.marketingCountryId(),
                thirdPartyId: null
            })
        })
        .then(window.handleFetchResponse)
        .then(mps => {
            self.marketingPermissions(mps.map(p => marketingPermission(p)));
            self.marketingPermissions().forEach(mp => {
                mp.Checked.subscribe(nv => {
                    self.save(null, [mp.Checked]);
                });
            });
            self.saveBusy(false);
        });
    }

    self.save = (onComplete, sources) => {
        self.errors.showAllMessages(false);
        if (self.errors().length) {
            self.errors.showAllMessages(true);
            self.errors.filter(x => {
                x.isValid.notifySubscribers(x.isValid());
            });
        } else {
            self.saveBusy(true);
            let data = {
                countryId: getCountryId(),
                languageId: getLanguageId(),
                fullName: self.fullName.observable(),
                email: self.email.observable(),
                marketingCountryId: self.marketingCountryId(),
                marketingLanguageId: self.marketingLanguageId(),
                currentPassword: self.currentPassword.observable(),
                newPassword: self.newPassword.observable(),
                marketingPermissions: ko.toJS(self.marketingPermissions())
            };
            fetch('/api/member/myAccountSave', {
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
                    sources.forEach(s => {
                        s.DOMElement.closest('.member-account-row').classList.add('save-successful');
                    });
                    self.fiveSecondTimeout = window.setTimeout(() => {
                        self.fullName.originalValue(self.fullName.observable());
                        self.email.originalValue(self.email.observable());
                        document.querySelectorAll('.member-account-row.save-successful').forEach(e => {
                            e.classList.add('hide');
                        });
                        self.pointThreeSecondTimeout = window.setTimeout(() => {
                            document.querySelectorAll('.member-account-row.save-successful').forEach(e => {
                                e.classList.remove('save-successful');
                                e.classList.remove('hide');
                            });
                        }, 300)
                    }, 5000);
                    //self.fullName.observable.DOMElement.closest('.member-account-row').classList.add('save-successful');
                    //self.email.observable.DOMElement.closest('.member-account-row').classList.add('save-successful');
                    //self.currentPassword.observable.DOMElement.closest('.member-account-row').classList.add('save-successful');
                    //self.marketingCountryId.DOMElement.closest('.member-account-row').classList.add('save-successful');
                    //self.marketingLanguageId.DOMElement.closest('.member-account-row').classList.add('save-successful');
                    //self.marketingPermissions().forEach(mp => {
                    //    mp.Checked.DOMElement.closest('.member-account-row').classList.add('save-successful');
                    //});
                    //window.setTimeout(() => {
                    //    document.querySelectorAll('.member-account-row.save-successful').forEach(e => {
                    //        e.classList.add('hide');
                    //    });
                    //    window.setTimeout(() => {
                    //        document.querySelectorAll('.member-account-row.save-successful').forEach(e => {
                    //            e.classList.remove('save-successful');
                    //            e.classList.remove('hide');
                    //        });
                    //    }, 300)
                    //}, 5000);
                }
                if (typeof onComplete === 'function')
                    onComplete();
                else
                    self.saveBusy(false);
            })
            .catch(err => {
                self.saveBusy(false);
                window.showError();
            })
        }
    }

    self.marketingComputer = ko.computed(() => {
        let mcid = self.marketingCountryId();
        let mlid = self.marketingLanguageId();
        if (self.skipFirstMarketingComputation) {
            self.skipFirstMarketingComputation = false;
        } else {
            ko.ignoreDependencies(
                self.save,
                self,
                [
                    self.refreshMarketingPermissions,
                    [self.marketingCountryId, self.marketingLanguageId]
                ]);
        }
    }).extend({
        rateLimit: {
            timeout: 50,
            method: 'notifyWhenChangesStop'
        }
    });

    //#endregion

    return self;
});