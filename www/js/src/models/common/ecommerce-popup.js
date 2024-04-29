const ecommercePopupViewModel = ((resources, banner) => {
    let self = {
        resources: resources,
        banner: banner
    };

    //#region Properties

    // Are we busy?
    self.busy = ko.observable(false);

    // Busy message
    self.busyMessage = ko.observable(self.resources.PleaseWait);

    // Are we complete?
    self.complete = ko.observable(false);

    // Are we validating?
    self.validateNow = ko.observable(false);

    // The pageSection object
    self.pageSectionForm = ko.observable();

    // Because we need to know your country
    self.marketingCountryId = ko.observable().extend({
        postable: true,
        required: {
            params: true,
            onlyIf: () => self.validateNow(),
            message: self.resources.FormsCountryRequired
        },
        errorPulse: {}
    });

    // A list of available countries
    self.availableCountries = ko.observableArray([]);

    // Selected language: some countries present with multiple languages
    self.languageId = ko.observable(getLanguageId()).extend({
        required: {
            params: true,
            onlyIf: () => {
                return self.validateNow()
            },
            message: self.resources.FormsLanguageRequired
        }
    });

    // A list of available languages
    self.availableLanguages = ko.observableArray([]);

    // User's email double-entry
    self.emailConfirmation = ko.observable();

    // User's email
    self.email = ko.observable().extend({
        postable: true,
        email: {
            params: true,
            onlyIf: () => self.validateNow(),
            message: self.resources.FormsErrorEmailFormat
        },
        equal: {
            params: self.emailConfirmation,
            onlyIf: () => self.validateNow(),
            message: self.resources.FormsErrorEmailCompare
        },
        required: {
            params: true,
            onlyIf: () => self.validateNow(),
            message: self.resources.FormsErrorEmail
        },
        errorPulse: {}
    });

    // email confirmation needed to exist for the above before being extended here
    self.emailConfirmation.extend({
        postable: true,
        email: {
            params: true,
            onlyIf: () => self.validateNow(),
            message: self.resources.FormsErrorEmailFormat
        },
        equal: {
            params: self.email,
            message: self.resources.FormsErrorEmailCompare
        },
        required: {
            params: true,
            message: self.resources.FormsErrorEmailCompare
        },
        errorPulse: {}
    });

    // the modal element
    self.modalElement = document.querySelector('#ecommerce-pop-modal');

    // the bootstrap modal itself
    self.modal = bootstrap.Modal.getOrCreateInstance(self.modalElement);

    // This must be determined based on the below user related properties and/or the user's response to marketingPermission checks
    self.mustResubscribe = ko.observable(false);

    // This is fed by the marketingPermissionsCheck
    self.userEmailMarketingPermitted = ko.observable(false);

    // This is fed by the marketingPermissionsCheck
    self.userUnsubscribedFromEmailMarketing = ko.observable(false);

    //#endregion



    //#region Subscriptions



    //#endregion


    //#region Computeds

    self.currentlyAvailableLanguages = ko.computed(() => {
        return self.availableLanguages().filter(l => l.CountryID == self.marketingCountryId());
    }, self);

    self.addressRequired = ko.computed(() => {
        return self.pageSectionForm()?.AddressRequired;
    });

    self.sourceId = ko.computed(() => {
        return self.pageSectionForm()?.SourceID;
    });

    //#endregion


    //#region Components

    // our marketing permission factory
    self.marketingPermissionChecker = marketingPermissionCheckerViewModel(self);

    self.marketingPermissions = self.marketingPermissionChecker.marketingPermissions;

    // Error collection
    self.errors = new ko.validation.group([
        self.email,
        self.emailConfirmation,
        self.marketingCountryId,
        self.languageCountryId,
        self.marketingPermissionChecker.havePerformedMarketingPermissionCheck
    ]);

    //#endregion

    //#region Methods

    self.alreadyDone = () => {
        let result = document.cookie.split('; ').find(row => row.startsWith('page-visit-counter='))?.split('=')[1] === 'done';
        //console.log('ecommerce-popup alreadyDone returns', result);
        return result;
    }

    self.setDone = () => {
        self.modal._config.backdrop = true;
        self.modal._config.keyboard = true;
        document.cookie = `page-visit-counter=done; expires=Fri, 31 Dec 9999 23:59:59 GMT; SameSite=Lax; Secure; Path=/`;
        //console.log('ecommerce-popup setDone called');
    }

    self.checkCookie = () => {
        let result = document.cookie.split('; ').some((item) => item.includes("page-visit-counter=2"));
        //console.log('ecommerce-popup checkCookie returns', result);
        return result;
    }

    self.setOrIncrementCookie = () => {
        let count = document.cookie.split('; ').find(row => row.startsWith('page-visit-counter='))?.split('=')[1];

        //console.log('ecommerce-popup setOrIncrementCookie count was', count);

        if (count === 'done')
            return;

        count = count ? +(++count) : 1;

        document.cookie = `page-visit-counter=${count}; expires=Fri, 31 Dec 9999 23:59:59 GMT; SameSite=Lax; Secure; Path=/`;

        //console.log('ecommerce-popup setOrIncrementCookie count is', count);
    }

    self.showModal = () => {
        self.modal.show();
    }

    self.hideModal = () => {
        self.modal.hide();
    }

    // the one that does the loading
    self.load = () => {
        if (self.alreadyDone() == false) {
            if (self.checkCookie() == true) {
                self.fetch();
            } else {
                self.setOrIncrementCookie();
            }
        } else {
            self.setOrIncrementCookie();
        }
    }

    self.fetch = () => {
        fetch('/api/form/LoadECPop', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-store',
            credentials: 'same-origin',
            body: JSON.stringify({
                countryId: getCountryId(),
                languageId: getLanguageId()
            })
        })
            .then(window.handleFetchResponse)
            .then(data => {
                console.trace(data);
                self.pageSectionForm(data.objForm);
                self.availableCountries(data.availableCountries);
                self.availableLanguages(data.availableLanguages);
                self.marketingCountryId(data.defaultCountryForDropDown);
                self.showModal();
                self.setDone();
            })
            .catch(window.catchFetchErrors)
    }

    self.submit = () => {
        self.errors.showAllMessages(false);
        self.validateNow(true);
        if (self.errors().length) {
            self.errors.showAllMessages(true);
            self.errors.filter(x => {
                x.isValid.notifySubscribers(x.isValid())
            });
        } else {
            self.busy(true);
            fetch('/api/form/savePageSectionForm', {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-store',
                credentials: 'same-origin',
                body: JSON.stringify(self.toJS())
            })
            .then(window.handleFetchResponse)
            .then(data => {
                self.busy(false);
                if (data.error) {
                    switch (data.message) {

                    }
                } else {
                    let emailOptIn = self.marketingPermissions().filter(mp => mp.MarketingPermissionID == 1);
                    let emailOptOut = self.marketingPermissions().filter(mp => mp.MarketingPermissionID == 2);

                    if (emailOptIn.length) {
                        if (emailOptIn[0].Checked()) {
                            // User has opted in to email marketing. If they were previously unsubscribed, we must resubscribe them.
                            if (self.userUnsubscribedFromEmailMarketing())
                                self.mustResubscribe(true);
                        } else {
                            // User has not opted in to email marketing. No action (user should already not be subscribed).
                        }
                    } else if (emailOptOut.length) {
                        if (emailOptOut[0].Checked()) {
                            // User has opted out of email marketing. No action (user should not already be opted in to email marketing).
                        } else {
                            // User has opted in to email marketing. If they were previously unsubscribed, we must resubscribe them.
                            if (self.userUnsubscribedFromEmailMarketing())
                                self.mustResubscribe(true);
                        }
                    }

                    if (emailOptIn.length == 0 &&
                        emailOptOut.length == 0) {
                        if (self.userEmailMarketingPermitted() &&
                            self.userUnsubscribedFromEmailMarketing()) {
                            // We have not presented any email marketing permission options to the user, because they have already granted permission.
                            // However, for whatever reason the user is also unsubscribed from email marketing. Consider a silent resubscribe?
                            self.mustResubscribe(true);
                        }
                    }

                    if (self.mustResubscribe()) {
                        switch (self.sourceId()) {
                            case 0:
                                fetchResources([
                                    'ResubscribeNotice',
                                    'ResubscribeFooter'])
                                    .then(window.handleResourcesArray)
                                    .then(resources => {
                                        document.querySelector('#form-wrapper > .alert.alert-success').innerHTML
                                            = `<i class="material-icons text-success">error</i><div>${resources.ResubscribeNotice}<br/><br/>${resources.ResubscribeFooter}</div>`
                                    });
                                break;
                            case 10:
                                fetchResources([
                                    'Information',
                                    'ResubscribeNotice',
                                    'ResubscribeFooter'])
                                    .then(window.handleResourcesArray)
                                    .then(resources => {
                                        window.showInfo(resources.Information, `${resources.ResubscribeNotice}<br/><br/>${resources.ResubscribeFooter}`);
                                    });
                                break;
                        }
                        self.beginResubscribe();
                    }

                    self.userUnsubscribedFromEmailMarketing(false);

                    self.complete(true);
                }
            })
            .catch(err => {
                self.busy(false);
                window.catchFetchErrors(err);
            })
        }
    }

    self.beginResubscribe = () => {
        fetch('/api/form/BeginMemberResubscribe', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-store',
            credentials: 'same-origin',
            body: JSON.stringify({
                email: ko.unwrap(self.email)
            })
        });
    }

    self.toJS = () => {
        let objForm = ko.unwrap(self.pageSectionForm);
        return {
            FormID: objForm.FormID,
            Email: self.email(),
            MarketingCountryID: self.marketingCountryId(),
            MarketingLanguageID: self.languageId(),
            CountryID: getCountryId(),
            LanguageID: getLanguageId(),
            AddressRequired: objForm.AddressRequired,
            CaptchaRequired: objForm.CaptchaRequired,
            SourceID: objForm.SourceID,
            SubmitButtonText: objForm.SubmitButtonText,
            HeaderText: objForm.HeaderText,
            FooterText: objForm.FooterText,
            PageSectionID: objForm.PageSectionID,
            ThirdPartyID: objForm.ThirdPartyID,
            CompetitionID: objForm.CompetitionID,
            BrochureID: objForm.BrochureID,
            MarketingPermissions: ko.toJS(self.marketingPermissionChecker.marketingPermissions())
        }
    }

    //#endregion

    return self;
});

window.addLoadFunction(() => {
    window.fetchResources(['FormsErrorEmailFormat',
        'FormsErrorEmailCompare',
        'FormsErrorEmail',
        'FormsCountryRequired',
        'FormsLanguageRequired',
        'PleaseWait'
    ])
    .then(window.handleResourcesArray)
    .then(resources => {
        window.ecommercePopModel = ecommercePopupViewModel(resources, window.ecommercePopBanners[0].banners[0]);

        if (document.querySelector('#ecommerce-pop-modal')) {
            ko.applyBindings(window.ecommercePopModel, document.querySelector('#ecommerce-pop-modal'));
        }

        window.ecommercePopModel.load();
    });
});