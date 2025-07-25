const formViewModel = ((pageSection) => {
    let self = {
        parent: pageSection,
        resources: pageSection.resources
    };

    //#region General Properties

    self.data = ko.observable();

    let dataSub = self.data.subscribe((newValue) => {
        Object.assign(self, newValue);
        dataSub.dispose();
        self.phoneNumberRequired(self.Form.PhoneNumberRequired);
        window[`form${self.Form.FormID}`] = self;
        self.init();
    });

    self.success = ko.observable(false);

    self.sourceId = ko.observable().extend({
        postable: true
    });

    self.brochureId = ko.observable().extend({
        postable: true
    });

    self.sourceGroupId = ko.observable().extend();

    self.thirdPartyId = ko.observable().extend({
        postable: true
    });

    // null / undefined: not gathered; false: optional; true: mandatory;
    self.phoneNumberRequired = ko.observable(undefined).extend({
        postable: false
    });

    self.phoneNumberRequired.subscribe(nv => {
        console.log("phoneNumberRequired", nv);
    });

    self.addressRequired = ko.observable(false).extend({
        postable: true
    });

    self.captchaRequired = ko.observable(false).extend({
        postable: true
    });

    self.submitButtonText = ko.observable().extend({
        postable: true
    });

    self.busy = ko.observable(false);

    self.success = ko.observable(false);

    self.validateNow = ko.observable(false);

    self.fullName = ko.observable().extend({
        postable: true,
        required: {
            params: true,
            onlyIf: () => self.validateNow(),
            message: self.resources.FormsErrorName
        },
        errorPulse: {}
    });

    self.emailConfirmation = ko.observable();

    self.email = ko.observable(typeof window.getMailingListEmail === 'function' ? window.getMailingListEmail() : undefined).extend({
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

    self.emailParams = {
        label: ko.observable(self.resources.FormsEmail),
        observable: self.email,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            self.observable.DOMElement.focus();
        },
        charsRemainingOptions: {
            maxLength: 254
        },
        readOnly: ko.observable(false),
        disabled: ko.observable(false)
    }

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

    self.emailConfirmationParams = {
        label: ko.observable(self.resources.FormsConfirmEmail),
        observable: self.emailConfirmation,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            self.observable.DOMElement.focus();
        },
        charsRemainingOptions: {
            maxLength: 254
        },
        readOnly: ko.observable(false),
        disabled: ko.observable(false)
    }

    self.availableCountries = ko.observableArray([]);

    self.marketingCountryId = ko.observable().extend({
        postable: true,
        required: {
            params: true,
            onlyIf: () => self.validateNow(),
            message: self.resources.FormsCountryRequired
        },
        errorPulse: {}
    });

    self.availableLanguages = ko.observableArray([]);

    self.marketingLanguageId = ko.observable().extend({
        postable: true,
        required: {
            params: true,
            message: self.resources.FormsLanguageRequired
        },
        errorPulse: {}
    });

    // This must be determined based on the below user related properties and/or the user's response to marketingPermission checks
    self.mustResubscribe = ko.observable(false);

    // This is fed by the marketingPermissionsCheck
    self.userEmailMarketingPermitted = ko.observable(false);

    // This is fed by the marketingPermissionsCheck
    self.userUnsubscribedFromEmailMarketing = ko.observable(false);

    self.phoneNumber = ko.observable().extend({
        postable: true,
    });

    self.phoneNumberParams = {
        label: self.resources.FormsPhoneNumber,
        observable: self.phoneNumber,
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

    //#endregion

    //#region Components

    self.marketingPermissionChecker = marketingPermissionCheckerViewModel(self);

    self.marketingPermissions = self.marketingPermissionChecker.marketingPermissions;

    self.deliveryAddress = addressViewModel({
        labelClass: 'delivery-address-label',
        controlClass: 'form-control mdc-text-field__input',
        phoneIntlMobile: true,
        email: false,
        defaultCountry: getCountryId()
    }, self.resources);

    self.deliveryAddressSearch = addressSearchFactory(
        ko.observable({
            CountryID: getCountryId()
        }),
        self.resources.AddressSearchPlaceholder,
        self.resources.AddressSearchPlaceholder,
        self.resources.MoodboardSave,
        null,
        null, // needs its own busy observable
        null, // message probably not required
        (onComplete) => {
            console.trace(self.deliveryAddress())
        },
        false,
        false,
        self.resources,
        'member');

    self.deliveryAddressSearch.setAddressObject(self.deliveryAddress);

    self.deliveryAddress.countryId.subscribe(nv => {
        self.marketingCountryId(nv);
    });

    //#endregion

    //#region Store Event Form Properties

    self.storeEventLocation = ko.observable().extend({
        required: {
            params: true,
            onlyIf: () => {
                return self.validateNow() && self.sourceId() == 72
            },
            message: self.resources.FormsErrorSelectLocation
        },
        errorPulse: {}
    });

    self.storeEventDate = ko.observable().extend({
        postable: true,
        required: {
            params: true,
            onlyIf: () => {
                return self.validateNow() && self.storeEventLocation() && self.sourceId() == 72
            },
            message: self.resources.FormsErrorSelectDate
        },
        errorPulse: {}
    });

    self.storeEvent = ko.observable().extend({
        required: {
            params: true,
            onlyIf: () => {
                return self.validateNow() && self.storeEventDate() && self.sourceId() == 72
            },
            message: self.resources.FormsErrorSelectTimeSlot
        },
        errorPulse: {}
    });

    self.storeEventDateFocus = ko.observable(false);

    self.storeEventDatePicker = {
        widget: ko.observable(),
        value: self.storeEventDate,
        format: 'dd/MM/yyyy',
        clear: function () {
            self.storeEventDate(undefined);
            self.storeEventDatePicker.widget().value(null);
            self.storeEventDatePicker.widget().trigger('change');
        }
    };

    //#endregion

    //#region Retail Appointment Properties

    self.appointmentAgendaId = ko.observable().extend({
        postable: true,
        required: {
            params: true,
            onlyIf: () => {
                return self.validateNow() && self.sourceId() == 71
            },
            message: self.resources.FormsErrorSelectAppointmentAgenda
        },
        errorPulse: {}
    });

    self.appointmentTypeId = ko.observable().extend({
        postable: true,
        required: {
            params: true,
            onlyIf: () => {
                return self.validateNow() && self.sourceId() == 71
            },
            message: self.resources.FormsErrorSelectAppointmentType
        },
        errorPulse: {}
    });

    self.appointmentTime = ko.observable().extend({
        postable: true,
        required: {
            params: true,
            onlyIf: () => {
                return self.validateNow() && self.sourceId() == 71
            },
            message: self.resources.FormsErrorSelectTimeSlot
        },
        errorPulse: {}
    });

    self.appointmentDate = ko.observable().extend({
        postable: true,
        required: {
            params: true,
            onlyIf: () => {
                return self.validateNow() && self.sourceId() == 71
            },
            message: self.resources.FormsErrorSelectDate
        },
        errorPulse: {}
    });

    self.appointmentDateFocus = ko.observable(false);

    let minDate = new Date();
    minDate.setDate(minDate.getDate() - 1);

    self.appointmentDatePicker = {
        widget: ko.observable(),
        value: self.appointmentDate,
        format: 'dd/MM/yyyy',
        clear: function () {
            self.appointmentDate(undefined);
            self.appointmentDatePicker.widget().value(null);
            self.appointmentDatePicker.widget().trigger('change');
        },
        minDate: minDate,
        disableDates: ['su', 'mo']
    };

    self.appointmentLocation = ko.observable(self.sourceId() == 71 ? 'Kings Road' : undefined).extend({
        postable: true,
        required: {
            params: true,
            onlyIf: () => {
                return self.validateNow() && self.sourceId() == 71
            },
            message: self.resources.FormsErrorSelectLocation
        },
        errorPulse: {}
    });

    self.appointmentComments = ko.observable().extend({
        postable: true
    });

    //self.defaultErrorGroup = new ko.validation.group([
    //    self.phoneNumber
    //]);

    //self.appointmentErrorGroup = new ko.validation.group([
    //    self.appointmentAgendaId,
    //    self.appointmentTypeId,
    //    self.appointmentTime,
    //    self.appointmentDate,
    //    self.appointmentLocation,
    //    self.appointmentComments,
    //    self.fullName,
    //    self.phoneNumber,
    //    self.email,
    //    self.emailConfirmation,
    //    self.marketingPermissionChecker.havePerformedMarketingPermissionCheck
    //]);

    //self.newsletterErrorGroup = new ko.validation.group([
    //    self.fullName,
    //    self.phoneNumber,
    //    self.email,
    //    self.emailConfirmation,
    //    self.marketingCountryId,
    //    self.marketingLanguageId
    //]);


    //self.brochureErrorGroup = new ko.validation.group([
    //    self.phoneNumber,
    //    self.email,
    //    self.emailConfirmation,
    //    self.marketingCountryId,
    //    self.marketingLanguageId,
    //    self.deliveryAddress.name.observable,
    //    self.deliveryAddress.companyName.observable,
    //    self.deliveryAddress.addressLine1.observable,
    //    self.deliveryAddress.addressLine2.observable,
    //    self.deliveryAddress.town.observable,
    //    self.deliveryAddress.county.observable,
    //    self.deliveryAddress.postcode.observable,
    //    self.deliveryAddress.country,
    //    self.deliveryAddress.countryId,
    //], {
    //    deep: true
    //});

    //self.competitionErrorGroup = new ko.validation.group([
    //    self.email,
    //    self.emailConfirmation,
    //    self.fullName,
    //    self.phoneNumber,
    //]);

    //#endregion

    //#region General Computeds

    self.errorGroup = ko.pureComputed(() => {
        switch (self.sourceId()) {
            case 0: // Newsletter Sign-Up
                return self.newsletterErrorGroup;
                break;
            case 1:
                return self.competitionErrorGroup;
                break;
            case 71: // Retail Appointment Request
                return self.appointmentErrorGroup;
                break;
            case 72: // Store Event Form
                return self.storeEventErrors;
                break;
            default:
                return self.defaultErrorGroup;
                break;
        }
        if (self.sourceGroupId() == 6) {
            return self.brochureErrorGroup;
        }
    }, self);

    //#endregion

    //#region Store Event Form Computeds

    self.storeEventDateStrings = ko.pureComputed(() => {
        let dateStrings = self.storeEventLocation() ? self.storeEventLocation().StoreEvents.map(se => se.StoreEventDate.replace(/T.*/g, '')) : [];
        return dateStrings;
    }, self);

    self.storeEvents = ko.pureComputed(() => {
        if (self.storeEventDate())
            return self.storeEventLocation().StoreEvents.flatMap(se => {
                return se.StoreEventDate.replace(/T.*/g, '') == self.storeEventDate().toISOString().replace(/T.*/g, '') ? [se] : [];
            })
    }, self);

    self.storeEventId = ko.pureComputed(() => self.storeEvent()
        ? self.storeEvent().StoreEventID
        : undefined,
        self)
        .extend({
            postable: true
        });

    self.storeEventLocationId = ko.pureComputed(() => self.storeEvent()
        ? self.storeEvent().StoreEventLocationID
        : undefined,
        self)
        .extend({
            postable: true
        });

    self.storeEventDateId = ko.pureComputed(() => self.storeEvent()
        ? self.storeEvent().StoreEvent_DateID
        : undefined,
        self)
        .extend({
            postable: true
        });

    self.storeEventTimeSlot = ko.pureComputed(() => self.storeEvent()
        ? self.storeEvent().StoreEvent_Date_TimeSlotID
        : undefined,
        self)
        .extend({
            postable: true
        });

    //#endregion

    //#region Retail Appointment Computeds

    //#endregion

    //#region Competition Computeds

    self.competitionId = ko.pureComputed(() => {
        return self.Form.CompetitionID
    }, self).extend({
        postable: true
    });

    //#endregion

    //#region General Subscriptions 

    self.marketingLanguageId.subscribe(nv => {
        console.log(`marketingLanguageId subscribe hit, ${nv}`);
    });

    self.availableLanguages.subscribe(nv => {
        console.log(`availableLanguages subscribe hit, ${nv}`);
        if (nv.length == 1) {
            setTimeout(() => {
                self.marketingLanguageId(nv[0].LanguageID);
            }, 0);
        }
    });

    //#endregion

    //#region Store Event Form Subscriptions

    self.storeEventLocation.subscribe(newValue => {
        self.storeEventDatePicker.widget().setOptions({
            disableDates: (date) => {
                if (date != null) {
                    if (new Date().getTimezoneOffset() == -60)
                        date.setUTCHours(date.getUTCHours() + 1);
                    let dateStrings = self.storeEventDateStrings();
                    let datePart = date.toISOString().replace(/T.*/g, '');
                    let result = dateStrings.indexOf(datePart) == -1;
                    return result;
                } else {
                    return false;
                }
            }
        });
    });

    self.storeEventDateFocus.subscribe(newValue => newValue ? self.storeEventDatePicker.widget().open() : null);

    self.storeEvents.subscribe(newValue => {
        self.storeEvent(undefined);
        if (newValue)
            if (newValue.length == 1)
                setTimeout(() => self.storeEvent(newValue[0]), 0);
    });

    //#endregion

    //#region Retail Appointment Subscriptions

    //#endregion

    //#region General Methods

    self.init = () => {
        // phoneNumberRequired triple state: null === not gathered; false === not required; true === required
        window.setTimeout(() => {
            if (self.phoneNumberRequired() != null) {
                let phoneElement = document.getElementById("forms-phone-wrapper");
                self.phoneNumber.extend({
                    required: {
                        params: self.phoneNumberRequired(),
                        onlyIf: () => self.validateNow(),
                        message: self.resources.phoneValidation
                    },
                    errorPulse: {}
                });
                ko.cleanNode(phoneElement);
                ko.applyBindings(self, phoneElement);
            }
        }, 0);

        if (self.Form.SourceID == 71) { // If this is an appointment request form
            self.appointmentLocation('Kings Road'); // force the location to KR; we no longer have > 1 locations and digging it out is harder than just hiding the control and setting the value.
        }

        self.buildErrorGroups();
    }

    self.buildErrorGroups = () => {

        self.defaultErrorGroup = new ko.validation.group([

        ]);

        self.appointmentErrorGroup = new ko.validation.group([
            self.appointmentAgendaId,
            self.appointmentTypeId,
            self.appointmentTime,
            self.appointmentDate,
            self.appointmentLocation,
            self.appointmentComments,
            self.fullName,
            self.phoneNumber,
            self.email,
            self.emailConfirmation,
            self.marketingPermissionChecker.havePerformedMarketingPermissionCheck
        ]);

        self.newsletterErrorGroup = new ko.validation.group([
            self.fullName,
            self.phoneNumber,
            self.email,
            self.emailConfirmation,
            self.marketingCountryId,
            self.marketingLanguageId
        ]);

        self.brochureErrorGroup = new ko.validation.group([
            self.phoneNumber,
            self.email,
            self.emailConfirmation,
            self.marketingCountryId,
            self.marketingLanguageId,
            self.deliveryAddress.name.observable,
            self.deliveryAddress.companyName.observable,
            self.deliveryAddress.addressLine1.observable,
            self.deliveryAddress.addressLine2.observable,
            self.deliveryAddress.town.observable,
            self.deliveryAddress.county.observable,
            self.deliveryAddress.postcode.observable,
            self.deliveryAddress.country,
            self.deliveryAddress.countryId,
        ], {
            deep: true
        });

        self.competitionErrorGroup = new ko.validation.group([
            self.email,
            self.emailConfirmation,
            self.fullName,
            self.phoneNumber,
        ]);

        self.storeEventErrors = new ko.validation.group([
            self.storeEventLocation,
            self.storeEventDate,
            self.storeEvent,
            self.fullName,
            self.phoneNumber,
            self.email,
            self.emailConfirmation,
            self.marketingPermissionChecker.havePerformedMarketingPermissionCheck
        ]);
    }

    self.toJS = () => {
        var result = Object.assign(...Object.keys(self).flatMap(k => {
            let result = {};
            if (self[k].hasOwnProperty('postable')) {
                if (ko.isObservableArray(self[k])) { // deep unwrap one level - specifcally for marketingPermissions, also for anything else
                    let arr = [];
                    ko.unwrap(self[k]()).forEach(i => {
                        let item = {};
                        Object.keys(i).forEach(ik => {
                            return item[ik] = ko.unwrap(i[ik]);
                        });
                        arr.push(item);
                    });
                    result[k] = arr;
                } else {
                    result[k] = ko.unwrap(self[k]);
                }
                return [result];
            }
            else
                return [];
        }));

        if (self.Form.CaptchaRequired) {
            try {
                result.gRecaptcha = grecaptcha.getResponse();
            } catch (e) {
                window.logClientSideError(e);
            }
        }

        if (self.sourceGroupId() == 6) {
            result = Object.assign(result, self.deliveryAddress.toPageSectionFormJS());
        }

        result.ContentID = window.getContentId();

        result.PageSectionID = self.Form.PageSectionID;

        result.FormID = self.Form.FormID;

        result.CountryID = getCountryId();

        result.LanguageID = getLanguageId();

        return result;
    }

    self.toJSON = () => JSON.stringify(self.toJS());

    self.isValid = () => {
        self.validateNow(true);

        let errorGroup = self.errorGroup();

        if (self.sourceGroupId() == 6) {
            self.deliveryAddress.validateNow(true);
        }

        if (errorGroup && errorGroup().length) {
            errorGroup.showAllMessages(false);
            if (errorGroup().length) {
                errorGroup.showAllMessages(true);
                errorGroup.filter(e => e.isValid.notifySubscribers(e.isValid()));
            }
            return false;
        }

        if (self.marketingPermissionChecker &&
            self.marketingPermissionChecker.havePerformedMarketingPermissionCheck() == false) {
            self.marketingPermissionChecker.checkMarketingPermissions();
            window.showInfo(self.resources.Sorry, self.resources.PleaseTryAgain);
            return false;
        }

        return true;
    }

    self.submit = () => {
        if (self.isValid()) {
            self.busy(true);

            fetch('/api/form/SavePageSectionFormNew', {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-store',
                credentials: 'same-origin',
                body: self.toJSON()
            })
                .then(window.handleFetchResponse)
                .then(data => {

                    try {
                        grecaptcha.reset();
                    }
                    catch (e) {

                    }
                    if (data === true) {

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
                                        'ResubscribeNotice',
                                        'ResubscribeFooter'])
                                        .then(window.handleResourcesArray)
                                        .then(resources => {
                                            window.showInfo(resources.ResubscribeNotice, resources.ResubscribeFooter);
                                        });
                                    break;
                            }
                            self.beginResubscribe();
                        }

                        self.success(data);

                        window.scrollTo(0, document.querySelector('#form-wrapper').offsetTop);

                        self.mustResubscribe(false);
                    }
                    else
                        showError();
                    self.busy(false);
                })
                .catch((error) => {
                    console.error(error);
                    try {
                        grecaptcha.reset();
                    }
                    catch (e) {

                    }
                    if (error instanceof TypeError)
                        error = { message: error.message };
                    window.logClientSideError(JSON.stringify(error));
                    self.busy(false);
                    showError();
                });
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

    //#endregion

    return self;
});