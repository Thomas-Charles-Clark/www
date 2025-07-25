const ecommercePopupViewModel = ({
    resources,
    banner,
    cookieIdentifier,
    showOnPageView,
    cookieExpiryDate
} = {
    resources: [],
    banner: {},
    cookieIdentifier: null,
    showOnPageView: 0,
    cookieExpiryDate: 'Fri, 31 Dec 9999 23:59:59 GMT'
}) => {

    let self = {
        resources: resources,
        banner: ko.observable(banner),
        cookieIdentifier: cookieIdentifier,
        showOnPageView: showOnPageView,
        cookieExpiryDate: cookieExpiryDate,
        loaded: ko.observable(false)
    };

    //#region Properties

    // banner might be undefined if there is no banner delivered for this particular instance
    self.bannerLocationId = self.banner()?.bannerLocationId;

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
    self.modalElement = document.querySelector(self.bannerLocationId == 54 ? '#ecommerce-general-pop-modal' : '#ecommerce-pop-modal');

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
        let result = document.cookie.split('; ').find(row => row.startsWith(`${self.cookieIdentifier}=`))?.split('=')[1] === 'done';
        //console.log('ecommerce-popup alreadyDone returns', result);
        return result;
    }

    self.setDone = () => {
        self.modal._config.backdrop = true;
        self.modal._config.keyboard = true;
        document.cookie = `${self.cookieIdentifier}=done; expires=${self.cookieExpiryDate}; SameSite=Lax; Secure; Path=/`;

        console.log(`${self.cookieIdentifier} was set to "done"`);
    }

    self.checkCookie = () => {
        let result = document.cookie.split('; ').some((item) => item.includes(`${self.cookieIdentifier}=done`));
        //console.log('ecommerce-popup checkCookie returns', result);
        return result;
    }

    self.setOrIncrementCookie = () => {
        let count = document.cookie.split('; ').find(row => row.startsWith(`${self.cookieIdentifier}=`))?.split('=')[1];

        //console.log('ecommerce-popup setOrIncrementCookie count was', count);

        if (count === 'done') {

            console.log(`${self.cookieIdentifier} was found to equal "done"`);

            document.cookie = `${self.cookieIdentifier}=${count}; expires=${self.cookieExpiryDate}; SameSite=Lax; Secure; Path=/`;

            console.log(`${self.cookieIdentifier} was set to expire "${self.cookieExpiryDate}"`);

            return;
        }

        count = count ? +(++count) : 1;

        document.cookie = `${self.cookieIdentifier}=${count}; expires=${self.cookieExpiryDate}; SameSite=Lax; Secure; Path=/`;

        console.log(`${self.cookieIdentifier} was set to "${count}"`);

        if (count >= self.showOnPageView) {
            self.fetch();
        }

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

            self.setOrIncrementCookie();

        } else {

            console.log(`${self.cookieIdentifier} was found to equal "done"`);

            document.cookie = `${self.cookieIdentifier}=done; expires=${self.cookieExpiryDate}; SameSite=Lax; Secure; Path=/`;

            console.log(`${self.cookieIdentifier} was set to expire "${self.cookieExpiryDate}"`);

        }
    }

    self.loadFetch = () => {
        self.complete(false);
        self.validateNow(false);
        self.email(document.querySelector('#footer-mailing-email').value);

        let afterFetch = null;
            
        if (self.email() && self.email() != '') {
            afterFetch = () => {
                self.emailConfirmation.DOMElement.focus()
            }
        } else {
            afterFetch = () => {
                self.email.DOMElement.focus()
            }
        }
        self.fetch(afterFetch);
    }

    self.fetch = (afterFetch) => {

        // eCommerce Email Capture Popup loads this:

        if (self.bannerLocationId == 55) {

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
                    self.banner(data.banner);
                    self.showModal();
                    self.setDone();
                    self.loaded(true);
                    window.setTimeout(() => {
                        window.lazyImages();
                    }, 0);
                    if (typeof afterFetch === 'function') {
                        window.setTimeout(() => {
                            afterFetch();
                        }, 300);
                    }
                })
                .catch(window.catchFetchErrors)

        } else {
            self.showModal();
            self.setDone();
            self.loaded(true);
            window.lazyImages();
        }
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
            fetch('/api/form/JoinMarketingListPopup', {
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
                                        document.querySelector('#form-wrapper > div').innerHTML
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
            //FormID: objForm.FormID,
            Email: self.email(),
            //MarketingCountryID: self.marketingCountryId(),
            //MarketingLanguageID: self.languageId(),
            CountryID: getCountryId(),
            LanguageID: getLanguageId(),
            Source: 'Popup',
            URL: window.location.href
            //AddressRequired: objForm.AddressRequired,
            //CaptchaRequired: objForm.CaptchaRequired,
            //SourceID: objForm.SourceID,
            //SubmitButtonText: objForm.SubmitButtonText,
            //HeaderText: objForm.HeaderText,
            //FooterText: objForm.FooterText,
            //PageSectionID: objForm.PageSectionID,
            //ThirdPartyID: objForm.ThirdPartyID,
            //CompetitionID: objForm.CompetitionID,
            //BrochureID: objForm.BrochureID,
            //MarketingPermissions: ko.toJS(self.marketingPermissionChecker.marketingPermissions())
        }
    }

    //#endregion

    return self;
};

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


        //if (window.ecommerceEmailCapturePopupBanners && window.ecommerceEmailCapturePopupBanners.length) {
        // 31/03/3025 riding roughshod, hurrha
        if (true) {

            let banner = {
                BannerID: 'ecom-email',
                bannerLocationId: 55
            };

            window.ecommerceEmailCapturePopupModel = ecommercePopupViewModel({
                resources: resources,
                banner: banner,
                cookieIdentifier: `banner-${banner.BannerID}-visit-counter`,
                showOnPageView: 3,
                cookieExpiryDate: 'Fri, 31 Dec 9999 23:59:59 GMT'
            });

            if (document.querySelector('#ecommerce-pop-modal')) {
                ko.applyBindings(window.ecommerceEmailCapturePopupModel, document.querySelector('#ecommerce-pop-modal'));
            }

            window.ecommerceEmailCapturePopupModel.load();
        }



        if (window.ecommerceGeneralPopupBanners && window.ecommerceGeneralPopupBanners.length) {

            let banner = window.ecommerceGeneralPopupBanners[0].banners[0];

            window.ecommerceGeneralPopupModel = ecommercePopupViewModel({
                resources: resources,
                banner: banner,
                cookieIdentifier: `banner-${banner.bannerId}-visit-counter`,
                showOnPageView: 1,
                cookieExpiryDate: window.addMinutes(new Date(), 60).toUTCString()
            });

            if (document.querySelector('#ecommerce-general-pop-modal')) {
                ko.applyBindings(window.ecommerceGeneralPopupModel, document.querySelector('#ecommerce-general-pop-modal'));
            }

            window.ecommerceGeneralPopupModel.load();
        }


    });
});