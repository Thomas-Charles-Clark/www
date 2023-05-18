var addressViewModel = ((options, resources) => {
    let self = {
        resources: resources
    };

    options = options || {};

    self.manual = ko.observable(false); // when ELSE should I set  this to true,
    // besides "Enter Address Manually" click?
    // useful to know?

    self.manual.subscribe((newValue) => {
        if (newValue) {
            self.detectChanges();
        }
    });

    self.editModalBody = ko.observable(); // where to attach dropdowns to

    self.tradeAccountAddressId = ko.observable();

    self.cleansedAddress = ko.observable(false);

    self.defaultCountry = options.defaultCountry;

    self.labelClass = options.labelClass;

    self.controlClass = options.controlClass;

    self.deliveryReference = ko.observable();

    self.promptForOverwrite = ko.observable(false);

    self.deliveryReference.subscribe(function (newValue) {

    });

    self.validateNow = options.validateNow || ko.observable(false);

    self.includeEU = typeof options.includeEU === 'undefined' ? true : options.includeEU;

    self.subscriptions = [];

    self.clearCleanseSubscriptions = [];

    self.loqateAddress = ko.observable(false);

    self.loqateAddress.subscribe(function (newValue) {
        console.log('loqateAddress subscribe hit', newValue);
    });

    self.changesHappened = function () {
        if (!self.promptForOverwrite())
            self.promptForOverwrite(true);

        self.loqateAddress(false);
    }

    self.removeDeliveryReferenceSubscriptions = function () {
        for (var i = 0; i < self.subscriptions.length; i++) {
            self.subscriptions[i].dispose();
        }
    }

    self.removeCleanseSubscriptions = function () {
        for (var i = 0; i < self.clearCleanseSubscriptions.length; i++) {
            self.clearCleanseSubscriptions[i].dispose();
        }
    }

    if (options.companyNameValidation == null)
        options.companyNameValidation = {
            required: {
                params: true,
                onlyIf: function () {
                    return self.validateNow() && (self.name.observable() == null)
                },
                message: self.resources.PleaseProvideCompanyName,
            }
        };

    if (options.nameValidation == null)
        options.nameValidation = {
            required: {
                params: true,
                onlyIf: function () {
                    return self.validateNow() && (self.companyName.observable() == null)
                },
                message: self.resources.PleaseProvideName
            }
        };

    if (options.addressLine1Validation == null)
        options.addressLine1Validation = {
            required: {
                params: true,
                onlyIf: function () {
                    return self.validateNow();
                },
                message: self.resources.PleaseProvideAddressLine1
            }
        };

    if (options.townValidation == null)
        options.townValidation = {
            required: {
                params: true,
                onlyIf: function () {
                    return self.validateNow();
                },
                message: self.resources.PleaseProvideTown
            }
        };

    if (options.postcodeValidation == null)
        options.postcodeValidation = {
            required: {
                params: true,
                onlyIf: function () {
                    if (self.validateNow()) {
                        if (self.country()) {
                            // No postcodes:
                            // 4: UAE, 99: Ireland, 106: Hong Kong, 145: Saint Martin, 216: Sint Maarten.
                            if ([4, 99, 106, 145, 216].indexOf(self.country().CountryID) > -1) {
                                return false;
                            } else {
                                return true;
                            }
                        } else {
                            return true;
                        }
                    } else {
                        return false;
                    }
                },
                message: self.resources.PleaseProvidePostcode
            }
        };

    if (options.countryValidation == null)
        options.countryValidation = {
            required: {
                params: true,
                onlyIf: function () {
                    return self.validateNow();
                },
                message: self.resources.PleaseSelectCountry
            }
        };

    if (options.emailValidation == null)
        options.emailValidation = {
            email: true
        }

    self.invoiceAddress = false;

    if (options.invoiceAddress == true) {
        self.invoiceAddress = true;
    }

    self.companyName = {
        label: ko.observable(self.resources.FormsCompanyName),
        observable: ko.observable().extend(options.companyNameValidation),
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
            maxLength: 50
        },
        readOnly: ko.observable(false),
        disabled: ko.observable(false)
    };

    self.name = {
        label: ko.observable(self.resources.FormsFullName),
        observable: ko.observable().extend(options.nameValidation),
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
            maxLength: 50
        },
        readOnly: ko.observable(false),
        disabled: ko.observable(false)
    };

    self.addressLine1 = {
        label: ko.observable(self.resources.FormsAddressLineOne),
        observable: ko.observable().extend(options.addressLine1Validation),
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
            maxLength: 60
        },
        readOnly: ko.observable(false),
        disabled: ko.observable(false)
    };

    self.addressLine2 = {
        label: ko.observable(self.resources.FormsAddressLineTwo),
        observable: ko.observable().extend(options.addressLine2Validation),
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
            maxLength: 60
        },
        readOnly: ko.observable(false),
        disabled: ko.observable(false)
    };

    self.town = {
        label: ko.observable(self.resources.FormsTown),
        observable: ko.observable().extend(options.townValidation),
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
            maxLength: 50
        },
        readOnly: ko.observable(false),
        disabled: ko.observable(false)
    };

    self.county = {
        label: ko.observable(self.resources.FormsCounty),
        observable: ko.observable().extend(options.countyValidation),
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
            maxLength: 40
        },
        readOnly: ko.observable(false),
        disabled: ko.observable(false)
    };

    self.postcode = {
        label: ko.observable(self.resources.FormsPostCode),
        observable: ko.observable().extend(options.postcodeValidation),
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
            maxLength: 20
        },
        readOnly: ko.observable(false),
        disabled: ko.observable(false)
    };

    self.addressSearchCountry = ko.observable();

    self.addressSearch = ko.observable().extend();

    self.country = ko.observable().extend(options.countryValidation);

    self.phone = {
        label: ko.observable(self.resources.FormsPhoneNumber),
        observable: ko.observable().extend(options.phoneValidation),
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        visible: ko.observable(options.phoneIntlMobile),
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            self.observable.DOMElement.focus();
            self.observable.isValid.notifySubscribers(true);
        },
        charsRemainingOptions: {
            maxLength: 20
        },
        intlMobile: options.phoneIntlMobile,
        readOnly: ko.observable(false),
        disabled: ko.observable(false)
    };

    self.email = {
        label: ko.observable(self.resources.email),
        observable: ko.observable().extend(options.emailValidation),
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        visible: ko.observable(typeof options.email === 'undefined' ? false : options.email),
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            self.observable.DOMElement.focus();
            self.observable.isValid.notifySubscribers(true);
        },
        charsRemainingOptions: {
            maxLength: 256
        },
        readOnly: ko.observable(false),
        disabled: ko.observable(false)
    };

    if (options.phoneIntlMobile) {
        self.phone.intlMobile = options.phoneIntlMobile;
        self.phone.countryObservable = self.country;
        self.phone.preferredCountries = options.phoneIntlPreferredCountries || ['gb'];

        self.country.subscribe(function (newValue) {
            if (newValue) {
                window.setTimeout(function () {
                    if (self.phone.observable.hasOwnProperty('intlTel')) {
                        if (newValue.Code != null)
                            $(self.phone.observable.intlTel).intlTelInput('setCountry', newValue.Code.toLowerCase());
                    }
                }, 100);
            } else {
                window.setTimeout(function () {
                    if (self.phone.observable.hasOwnProperty('intlTel')) {
                        $(self.phone.observable.intlTel).intlTelInput('setCountry', 'gb');
                    }
                }, 100);
            }
        });
    }

    self.pcaLookup = {
        label: self.resources.SearchForAddress,
        observable: self.addressSearch,
        resources: self.resources,
        mapFunction: function (line1, line2, line3, line4, postcode) {
            self.name.observable(undefined);
            self.addressLine1.observable(undefined);
            self.addressLine2.observable(undefined);
            self.town.observable(undefined);
            self.county.observable(undefined);
            self.postcode.observable(undefined);

            if (line1) {
                self.addressLine1.observable(line1);
            }

            if (line2) {
                self.addressLine2.observable(line2);
            }

            if (line3) {
                self.town.observable(line3);
            }

            if (line4) {
                self.county.observable(line4);
            }

            if (postcode) {
                self.postcode.observable(postcode);
            }

            self.countryWidget().value(self.addressSearchCountryId());
            window.setTimeout(function () {
                self.countryWidget().trigger('change');
            }, 0);

            if (ko.isObservable(self.validateNow)) {
                self.validateNow(true);
            }
        },
        country: self.addressSearchCountry,
        position: { my: "right top", at: "right bottom", collision: "none" },
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            self.observable.DOMElement.focus();
        }
    };

    self.validateNow.subscribe(function (newValue) {
        if (newValue) {
            self.notifySubscribers();
        }
    });

    self.isValid = function () {

        let hasPhoneErrored = false;

        if (self.phone.observable.hasOwnProperty('intlTel')) {
            if ($(self.phone.observable.DOMElement).intlTelInput('getNumber')) {
                if (!$(self.phone.observable.DOMElement).intlTelInput('isValidNumber')) {
                    hasPhoneErrored = true;
                    self.phone.observable.isValid.notifySubscribers(false);
                }
            }
        }

        if ((self.errors().length == 0) && (hasPhoneErrored == false)) {
            return true;
        } else {
            self.notifySubscribers();
            return false;
        }
    }

    self.isValid.notifySubscribers = () => { self.notifySubscribers() }

    self.countryId = ko.observable(options.defaultCountry).extend(options.countryValidation);

    self.countryId.subscribe(function (newValue) {
        if (self.countryWidget()) {
            if (newValue) {
                if (!(self.country.mdcSelect.label_.root_).classList.contains('mdc-floating-label--float-above')) {
                    self.country.mdcSelect.label_.float(true);
                    self.country.mdcSelect.getDefaultFoundation().adapter_.notchOutline(self.country.mdcSelect.getDefaultFoundation().adapter_.getLabelWidth());
                }
            } else {
                self.country.mdcSelect.getDefaultFoundation().adapter_.closeOutline();
                self.country.mdcSelect.label_.float(false);
            }
        }
    });

    self.addressSearchCountryId = ko.observable(options.defaultCountry);

    self.addressSearchCountryId.subscribe(function (newValue) {
        if (newValue) {
            if (self.addressSearchCountryWidget()) {
                if (!(self.addressSearchCountryWidget()._widgetOptions.dataItem.mdcSelect.label_.root_).classList.contains('mdc-floating-label--float-above')) {
                    self.addressSearchCountryWidget()._widgetOptions.dataItem.mdcSelect.label_.float(true);
                    self.addressSearchCountryWidget()._widgetOptions.dataItem.mdcSelect.getDefaultFoundation().adapter_.notchOutline(self.addressSearchCountryWidget()._widgetOptions.dataItem.mdcSelect.getDefaultFoundation().adapter_.getLabelWidth());
                }
            }
        } else {
            if (self.addressSearchCountryWidget()) {
                self.addressSearchCountryWidget()._widgetOptions.dataItem.mdcSelect.getDefaultFoundation().adapter_.closeOutline();
                self.addressSearchCountryWidget()._widgetOptions.dataItem.mdcSelect.label_.float(false);
            }
        }
    });

    self.addressSearchCountryWidget = ko.observable();

    self.addressSearchCountryDropDownList = () => {
        //console.log('addressViewModel addressSearchCountryDropDownList fn exec', self.editModalBody.DOMElement);
        let result = {
            observable: self.addressSearchCountry,
            showClear: false,
            width: '180px',
            label: self.resources.FormsCountry,
            valuePrimitive: false,
            useNativeValidation: false,
            decorateInputElement: false,
            insertValidationMessages: false,
            controlClass: 'form-control',
            disabled: ko.observable(false),
            dropDownListSettings: {
                value: self.addressSearchCountryId,
                dataItem: self.addressSearchCountry,
                dataValueField: 'CountryID',
                dataTextField: 'Name',
                widget: self.addressSearchCountryWidget,
                open: function (e) {
                    (self.addressSearchCountry.mdcSelect.root_).classList.add('mdc-select--focused');
                },
                close: function (e) {
                    (self.addressSearchCountry.mdcSelect.root_).classList.remove('mdc-select--focused');
                },
                data: new kendo.data.DataSource({
                    serverFiltering: true,
                    serverSorting: true,
                    serverGrouping: true,
                    schema: Object.assign({
                        data: "Data",
                        total: "Total",
                        filters: "Filters",
                        aggregates: "Aggregates",
                        errors: "Errors"
                    }, kendo.data.schemas.webapi),
                    transport: {
                        read: function (options) {
                            var webapi = new kendo.data.transports.webapi({ prefix: "" });
                            var mapped = webapi.parameterMap(options.data);

                            var data = {};

                            for (var propertyName in mapped) {
                                data[propertyName] = mapped[propertyName];
                            }

                            data.includeEU = self.includeEU;

                            fetch(`/api/tradeapi/getcountries?${new URLSearchParams(data).toString()}`, {
                                method: 'GET',
                                headers: window.fetchHeaders,
                            })
                                .then(window.handleFetchResponse)
                                .then(data => {
                                    options.success(data);
                                })
                                .catch(err => {
                                    options.error();
                                    window.catchFetchErrors(err);
                                });
                        }
                    }
                }),
                valuePrimitive: false,
                autoWidth: false,
                autoBind: true,
                optionLabel: ' ',
                filter: 'startswith',
                popup: {
                    appendTo: self.editModalBody.DOMElement
                }
            },
            clearFunction: function () {
                var self = this;
                self.observable(undefined);
                self.dropDownListSettings.widget().value('');
                self.dropDownListSettings.widget()._widgetOptions.dataItem.mdcSelect.getDefaultFoundation().adapter_.closeOutline();
                self.dropDownListSettings.widget()._widgetOptions.dataItem.mdcSelect.label_.float(false);
            }
        }

        self.addressSearchCountryDropDownList = result;

        return result;
    }

    self.countryWidget = ko.observable();

    self.countryDropDownList = () => {
        //console.log('addressViewModel countryDropDownList fn exec', self.editModalBody.DOMElement);
        let result = {
            observable: self.country,
            showClear: false,
            width: '180px',
            label: self.resources.FormsCountry,
            valuePrimitive: false,
            useNativeValidation: false,
            decorateInputElement: false,
            insertValidationMessages: false,
            controlClass: 'form-control',
            disabled: ko.observable(false),
            dropDownListSettings: ko.observable({
                value: self.countryId,
                dataItem: self.country,
                dataValueField: 'CountryID',
                dataTextField: 'Name',
                widget: self.countryWidget,
                open: function (e) {
                    (self.country.mdcSelect.root_).classList.add('mdc-select--focused');
                },
                close: function (e, event) {
                    (self.country.mdcSelect.root_).classList.remove('mdc-select--focused');
                },
                data: new kendo.data.DataSource({
                    serverFiltering: true,
                    serverSorting: true,
                    serverGrouping: true,
                    schema: Object.assign({
                        data: "Data",
                        total: "Total",
                        filters: "Filters",
                        aggregates: "Aggregates",
                        errors: "Errors"
                    }, kendo.data.schemas.webapi),
                    transport: {
                        read: function (options) {
                            var webapi = new kendo.data.transports.webapi({ prefix: "" });
                            var mapped = webapi.parameterMap(options.data);

                            var data = {};

                            for (var propertyName in mapped) {
                                data[propertyName] = mapped[propertyName];
                            }

                            data.includeEU = self.includeEU;

                            fetch(`/api/tradeapi/getcountries?${new URLSearchParams(data).toString()}`, {
                                method: 'GET',
                                headers: window.fetchHeaders,
                            })
                                .then(window.handleFetchResponse)
                                .then(data => {
                                    options.success(data);
                                })
                                .catch(err => {
                                    options.error();
                                    window.catchFetchErrors(err);
                                });
                        }
                    }
                }),
                valuePrimitive: false,
                autoWidth: false,
                autoBind: true,
                optionLabel: ' ',
                filter: 'startswith',
                popup: {
                    appendTo: self.editModalBody.DOMElement
                }
            }),
            clearFunction: function () {
                var self = this;
                self.observable(undefined);
                self.dropDownListSettings.widget().value('');
                self.dropDownListSettings.widget()._widgetOptions.dataItem.mdcSelect.getDefaultFoundation().adapter_.closeOutline();
                self.dropDownListSettings.widget()._widgetOptions.dataItem.mdcSelect.label_.float(false);
            }
        };

        self.countryDropDownList = result;

        return result;
    }

    // the dom has to be ready before we can bind our country drop down
    // this is because it might feature in a modal
    self.domReady = ko.observable(false);

    self.domReady.subscribe(nv => {
        // great, the dom is ready. 
    })

    self.detectChanges = function () {

        console.log('address detectChanges called');

        self.subscriptions.push(
            self.companyName.observable.subscribe(function (newValue) {
                self.changesHappened();
            })
        );

        self.subscriptions.push(
            self.phone.observable.subscribe(function (newValue) {
                self.changesHappened();
            })
        );

        self.subscriptions.push(
            self.email.observable.subscribe(function (newValue) {
                self.changesHappened();
            })
        );

        self.subscriptions.push(
            self.name.observable.subscribe(function (newValue) {
                self.changesHappened();
            })
        );

        self.subscriptions.push(
            self.addressLine1.observable.subscribe(function (newValue) {
                self.changesHappened();
            })
        );

        self.subscriptions.push(
            self.addressLine2.observable.subscribe(function (newValue) {
                self.changesHappened();
            })
        );

        self.subscriptions.push(
            self.town.observable.subscribe(function (newValue) {
                self.changesHappened();
            })
        );

        self.subscriptions.push(
            self.county.observable.subscribe(function (newValue) {
                self.changesHappened();
            })
        );

        self.subscriptions.push(
            self.postcode.observable.subscribe(function (newValue) {
                self.changesHappened();
            })
        );

        self.subscriptions.push(
            self.countryId.subscribe(function (newValue) {
                self.changesHappened();
            })
        );

        if (!self.initialState().hasOwnProperty('DeliveryName'))
            self.recordInitialState();
    }

    self.initialState = ko.observable({});

    self.recordInitialState = function () {
        self.initialState({
            DeliveryName: self.name.observable(),
            DeliveryCompanyName: self.companyName.observable(),
            AddressLine1: self.addressLine1.observable(),
            AddressLine2: self.addressLine2.observable(),
            Town: self.town.observable(),
            County: self.county.observable(),
            Postcode: self.postcode.observable(),
            Country: self.country()
        });
    }

    self.load = function (address) {
        self.reset();

        self.deliveryReference(address.deliveryReference);

        if (self.deliveryReference() == 'C') {
            self.companyName.readOnly(true);
            self.companyName.disabled(true);
            self.name.readOnly(true);
            self.name.disabled(true);
            self.addressLine1.readOnly(true);
            self.addressLine1.disabled(true);
            self.addressLine2.readOnly(true);
            self.addressLine2.disabled(true);
            self.town.readOnly(true);
            self.town.disabled(true);
            self.county.readOnly(true);
            self.county.disabled(true);
            self.postcode.readOnly(true);
            self.postcode.disabled(true);
            self.phone.readOnly(true);
            self.phone.disabled(true);
            self.email.readOnly(true);
            self.email.disabled(true);
            if (typeof self.addressSearchCountryDropDownList === 'object')
                self.addressSearchCountryDropDownList.disabled(true);
            self.countryDropDownList.disabled(true);
        } else {
            self.companyName.readOnly(false);
            self.companyName.disabled(false);
            self.name.readOnly(false);
            self.name.disabled(false);
            self.addressLine1.readOnly(false);
            self.addressLine1.disabled(false);
            self.addressLine2.readOnly(false);
            self.addressLine2.disabled(false);
            self.town.readOnly(false);
            self.town.disabled(false);
            self.county.readOnly(false);
            self.county.disabled(false);
            self.postcode.readOnly(false);
            self.postcode.disabled(false);
            self.phone.readOnly(false);
            self.phone.disabled(false);
            self.email.readOnly(false);
            self.email.disabled(false);

            if (typeof self.addressSearchCountryDropDownList === 'object')
                self.addressSearchCountryDropDownList.disabled(false);

            if (typeof self.countryDropDownList === 'object')
                self.countryDropDownList.disabled(false);
        }

        self.name.observable(address.name);
        self.addressLine1.observable(address.addressLine1);
        self.addressLine2.observable(address.addressLine2);
        self.town.observable(address.town);
        self.county.observable(address.county);
        self.postcode.observable(address.postcode);

        window.setTimeout(function () {
            if (self.countryWidget()) {
                self.countryWidget().bind('dataBound', function () {
                    self.countryWidget().value(address.countryId);
                    self.countryWidget().trigger('change');
                    self.countryWidget().unbind('dataBound');
                    self.detectChanges();
                });
                self.countryWidget().dataSource.read();
            } else {
                self.countryId(address.countryId);
                self.country({
                    Name: address.CountryName,
                    Code: address.CountryCode,
                    EU: address.EU,
                    CountryID: address.CountryID
                });
                self.detectChanges();
            }
        }, 300);

        //if (self.countryWidget() && address.countryId)
        //    self.countryWidget().value(address.countryId);
        //else
        //    self.country(address.country);

        var tags = [];

        if (address.tags != null) {
            for (var i = 0; i < address.tags.length; i++) {
                tags.push(address.tags[i].Tag);
            }
            self.tags(tags);
        }
    }

    self.hide = function () {
        if (self.addressSearch.editModal.hasOwnProperty('DOMElement'))
            bootstrap.Modal.getOrCreateInstance(self.addressSearch.editModal.DOMElement).hide();
    }

    self.deletable = ko.observable(true);

    self.loadBasketMiscAddress = function (address) {
        return new Promise(function (resolve, reject) {

            self.reset();

            self.deletable(false);

            self.deliveryReference(address.DeliveryReference);

            if (self.deliveryReference() == 'C') {
                self.companyName.readOnly(true);
                self.companyName.disabled(true);
                self.name.readOnly(true);
                self.name.disabled(true);
                self.addressLine1.readOnly(true);
                self.addressLine1.disabled(true);
                self.addressLine2.readOnly(true);
                self.addressLine2.disabled(true);
                self.town.readOnly(true);
                self.town.disabled(true);
                self.county.readOnly(true);
                self.county.disabled(true);
                self.postcode.readOnly(true);
                self.postcode.disabled(true);
                self.phone.readOnly(true);
                self.phone.disabled(true);
                self.email.readOnly(true);
                self.email.disabled(true);
                if (typeof self.addressSearchCountryDropDownList === 'object')
                    self.addressSearchCountryDropDownList.disabled(true);
                self.countryDropDownList.disabled(true);
            } else {
                self.companyName.readOnly(false);
                self.companyName.disabled(false);
                self.name.readOnly(false);
                self.name.disabled(false);
                self.addressLine1.readOnly(false);
                self.addressLine1.disabled(false);
                self.addressLine2.readOnly(false);
                self.addressLine2.disabled(false);
                self.town.readOnly(false);
                self.town.disabled(false);
                self.county.readOnly(false);
                self.county.disabled(false);
                self.postcode.readOnly(false);
                self.postcode.disabled(false);
                self.phone.readOnly(false);
                self.phone.disabled(false);
                self.email.readOnly(false);
                self.email.disabled(false);
                if (typeof self.addressSearchCountryDropDownList === 'object')
                    self.addressSearchCountryDropDownList.disabled(false);

                if (typeof self.countryDropDownList === 'object')
                    self.countryDropDownList.disabled(false);
            }

            self.name.observable(address.FullName);
            self.companyName.observable(address.CompanyName);
            self.companyName.label = self.resources.FormsCompanyName;
            self.addressLine1.observable(address.AddressLine1);
            self.addressLine2.observable(address.AddressLine2);
            self.town.observable(address.Town);
            self.county.observable(address.County);
            self.postcode.observable(address.Postcode);
            self.phone.observable(address.PhoneNumber);
            self.phone.visible(true);
            self.email.observable(address.Email);

            if (self.countryWidget()) {
                self.countryWidget().bind('dataBound', function () {
                    self.countryWidget().value(address.CountryID);
                    self.countryWidget().trigger('change');
                    self.countryWidget().unbind('dataBound');
                    self.detectChanges();
                });
                self.countryWidget().dataSource.read();
            } else {
                self.countryId(address.CountryID);

                self.country({
                    Name: address.CountryName,
                    Code: address.CountryCode,
                    EU: address.EU,
                    CountryID: address.CountryID
                });

                self.detectChanges();
            }

            resolve();
        });
    }

    self.loadAddress = function (address) {

        self.reset();

        self.deletable(false);

        self.deliveryReference(address.DeliveryReference);

        if (self.deliveryReference() == 'C') {
            self.companyName.readOnly(true);
            self.companyName.disabled(true);
            self.name.readOnly(true);
            self.name.disabled(true);
            self.addressLine1.readOnly(true);
            self.addressLine1.disabled(true);
            self.addressLine2.readOnly(true);
            self.addressLine2.disabled(true);
            self.town.readOnly(true);
            self.town.disabled(true);
            self.county.readOnly(true);
            self.county.disabled(true);
            self.postcode.readOnly(true);
            self.postcode.disabled(true);
            self.phone.readOnly(true);
            self.phone.disabled(true);
            self.email.readOnly(true);
            self.email.disabled(true);
            if (typeof self.addressSearchCountryDropDownList === 'object')
                self.addressSearchCountryDropDownList.disabled(true);
            self.countryDropDownList.disabled(true);
        } else {
            self.companyName.readOnly(false);
            self.companyName.disabled(false);
            self.name.readOnly(false);
            self.name.disabled(false);
            self.addressLine1.readOnly(false);
            self.addressLine1.disabled(false);
            self.addressLine2.readOnly(false);
            self.addressLine2.disabled(false);
            self.town.readOnly(false);
            self.town.disabled(false);
            self.county.readOnly(false);
            self.county.disabled(false);
            self.postcode.readOnly(false);
            self.postcode.disabled(false);
            self.phone.readOnly(false);
            self.phone.disabled(false);
            self.email.readOnly(false);
            self.email.disabled(false);

            if (typeof self.addressSearchCountryDropDownList === 'object')
                self.addressSearchCountryDropDownList.disabled(false);

            if (typeof self.countryDropDownList === 'object')
                 self.countryDropDownList.disabled(false);
        }

        self.name.observable(address.DeliveryName);
        self.companyName.observable(address.DeliveryCompanyName);
        self.companyName.label = self.resources.FormsCompanyName;
        self.addressLine1.observable(address.DeliveryAddressLine1);
        self.addressLine2.observable(address.DeliveryAddressLine2);
        self.town.observable(address.DeliveryTown);
        self.county.observable(address.DeliveryCounty);
        self.postcode.observable(address.DeliveryPostcode);
        self.phone.observable(address.DeliveryPhoneNumber);
        self.phone.visible(true);
        self.email.observable(address.OrderConfirmationsEmail);
        self.email.visible(true);

        self.endCustomerTradeAccountName = address.EndCustomerTradeAccountName;

        if (self.countryWidget()) {
            self.countryWidget().bind('dataBound', function () {
                self.countryWidget().value(address.DeliveryCountryID);
                self.countryWidget().trigger('change');
                self.countryWidget().unbind('dataBound');
                self.detectChanges();
            });
            self.countryWidget().dataSource.read();
        } else {
            self.countryId(address.DeliveryCountryID);
            if (address.DeliveryCountry) {
                self.country({
                    Name: address.DeliveryCountry.Name,
                    Code: address.DeliveryCountry.Code,
                    EU: address.DeliveryCountry.EU,
                    CountryID: address.DeliveryCountryID
                });
            }
            self.detectChanges();
        }
    }

    self.toMemberAddressJs = function () {
        var data = {
            memberAddressId: self.deliveryReference(),
            companyName: self.companyName.observable(),
            name: self.name.observable(),
            addressLine1: self.addressLine1.observable(),
            addressLine2: self.addressLine2.observable(),
            town: self.town.observable(),
            county: self.county.observable(),
            postcode: self.postcode.observable(),
            countryId: self.country() ? self.country().CountryID : self.countryId(),
        }
        if (self.phone.observable.hasOwnProperty('intlTel')) {
            data.phone = $(self.phone.observable.intlTel).intlTelInput('getNumber');
        } else {
            data.phone = self.phone.observable();
        }

        if (data.phone === '')
            data.phone = null;

        return data;
    }

    self.toBasketAddressJS = function () {
        var data = {
            manual: self.manual(),
            deliveryReference: self.deliveryReference(),
            deliveryName: self.name.observable(),
            deliveryCompanyName: self.companyName.observable(),
            deliveryAddressLine1: self.addressLine1.observable(),
            deliveryAddressLine2: self.addressLine2.observable(),
            deliveryTown: self.town.observable(),
            deliveryCounty: self.county.observable(),
            deliveryPostcode: self.postcode.observable(),
            deliveryCountryId: self.country() ? self.country().CountryID : self.countryId(),
            deliveryCountryCode: self.country() ? (self.country().Code || self.country()) : null,
            email: self.email.observable()
        }
        if (self.phone.observable.hasOwnProperty('intlTel')) {
            data.deliveryPhoneNumber = $(self.phone.observable.intlTel).intlTelInput('getNumber');
        } else {
            data.deliveryPhoneNumber = self.phone.observable();
        }

        return data;
    }

    self.loadPCAResult = function (r) {

        //self.addressObject.removeCleanseSubscriptions();
        //self.addressObject.removeDeliveryReferenceSubscriptions();

        //self.addressObject.deliveryReference(undefined);
        //self.addressObject.companyName.observable(undefined);
        //self.addressObject.name.observable(undefined);
        //self.addressObject.addressLine1.observable(undefined);
        //self.addressObject.addressLine2.observable(undefined);
        //self.addressObject.town.observable(undefined);
        //self.addressObject.county.observable(undefined);
        //self.addressObject.postcode.observable(undefined);
        //self.addressObject.phone.observable(undefined);
        //self.addressObject.email.observable(undefined);
        //self.addressObject.loqateAddress(false);

        self.reset();
        self.removeCleanseSubscriptions();
        self.removeDeliveryReferenceSubscriptions();

        if (r.LoqateAddress)
            self.loqateAddress(true);

        if (r.DeliveryReference)
            self.deliveryReference(r.DeliveryReference);

        if (r.Company)
            self.companyName.observable(r.Company);

        if (r.Name)
            self.name.observable(r.Name);

        if (r.Line1) {
            self.addressLine1.observable(r.Line1);
        }

        if (r.Line2) {
            self.addressLine2.observable(r.Line2);
        }

        if (r.City) {
            self.town.observable(r.City);
        }

        if (r.Province) {
            self.county.observable(r.Province);
        }

        if (r.PostalCode) {
            self.postcode.observable(r.PostalCode);
        }

        if (r.Phone) {
            self.phone.observable(r.Phone);
        }

        if (r.Email) {
            self.email.observable(r.Email);
        }

        self.countryId(r.CountryID);

        self.country(r.Country);

        if (self.countryWidget()) {
            self.countryWidget().bind('dataBound', function () {
                self.countryWidget().value(self.countryId());
                self.countryWidget().trigger('change');
                self.countryWidget().unbind('dataBound');
                self.detectChanges();
            });

            self.countryWidget().dataSource.read();
        }
    }

    self.loadFromOtherAddressObject = function (other) {
        self.reset();

        self.deletable(other.deletable);

        self.deliveryReference(other.deliveryReference());

        if (self.deliveryReference() == 'C') {
            self.companyName.readOnly(true);
            self.companyName.disabled(true);
            self.name.readOnly(true);
            self.name.disabled(true);
            self.addressLine1.readOnly(true);
            self.addressLine1.disabled(true);
            self.addressLine2.readOnly(true);
            self.addressLine2.disabled(true);
            self.town.readOnly(true);
            self.town.disabled(true);
            self.county.readOnly(true);
            self.county.disabled(true);
            self.postcode.readOnly(true);
            self.postcode.disabled(true);
            self.phone.readOnly(true);
            self.phone.disabled(true);
            self.email.readOnly(true);
            self.email.disabled(true);

            if (typeof self.addressSearchCountryDropDownList === 'object')
                self.addressSearchCountryDropDownList.disabled(true);

            if (typeof self.countryDropDownList === 'object')
                self.countryDropDownList.disabled(true);

        } else {
            self.companyName.readOnly(false);
            self.companyName.disabled(false);
            self.name.readOnly(false);
            self.name.disabled(false);
            self.addressLine1.readOnly(false);
            self.addressLine1.disabled(false);
            self.addressLine2.readOnly(false);
            self.addressLine2.disabled(false);
            self.town.readOnly(false);
            self.town.disabled(false);
            self.county.readOnly(false);
            self.county.disabled(false);
            self.postcode.readOnly(false);
            self.postcode.disabled(false);
            self.phone.readOnly(false);
            self.phone.disabled(false);
            self.email.readOnly(false);
            self.email.disabled(false);

            if (typeof self.addressSearchCountryDropDownList === 'object')
                self.addressSearchCountryDropDownList.disabled(false);

            if (typeof self.countryDropDownList === 'object')
                self.countryDropDownList.disabled(false);
        }

        self.companyName.observable(other.companyName.observable());
        self.name.observable(other.name.observable());
        self.addressLine1.observable(other.addressLine1.observable());
        self.addressLine2.observable(other.addressLine2.observable());
        self.town.observable(other.town.observable());
        self.county.observable(other.county.observable());
        self.postcode.observable(other.postcode.observable());
        self.phone.observable(other.phone.observable());
        self.email.observable(other.email.observable());

        if (other.phone.intlMobile == false) {
            self.phone.intlMobile = false
        } else {
            self.phone.intlMobile = true;
        }

        if (self.countryWidget()) {
            self.countryWidget().bind('dataBound', function () {
                self.countryWidget().value(other.countryId());
                self.countryWidget().trigger('change');
                self.countryWidget().unbind('dataBound');
                self.detectChanges();
            });
            self.countryWidget().dataSource.read();
        } else {
            self.countryId(other.countryId());
            self.country({
                Name: other.country() ? other.country().Name : undefined,
                Code: other.country() ? other.country().hasOwnProperty('Code') ? other.country().Code : other.country() : undefined,
                CountryID: other.countryId(),
                EU: other.country() ? other.country().EU : undefined
            });
            self.detectChanges();
        }
    }

    self.reflowLabels = function () {
        console.log('reflowLabels');
        self.name.observable.mdcTextField.layout();
        self.companyName.observable.mdcTextField.layout();
        self.addressLine1.observable.mdcTextField.layout();
        self.addressLine2.observable.mdcTextField.layout();
        self.town.observable.mdcTextField.layout();
        self.county.observable.mdcTextField.layout();
        self.postcode.observable.mdcTextField.layout();
        if (self.country()) {
            self.country.mdcSelect.layout();
            self.country.mdcSelect.outline_.notch(self.country.mdcSelect.label_.getWidth() * 0.75);
        }
        if (self.email.observable.hasOwnProperty('mdcTextField'))
            self.email.observable.mdcTextField.layout();
        if (self.phone.observable.hasOwnProperty('mdcTextField'))
            self.phone.observable.mdcTextField.layout();
        self.addressSearcher.countryDropDownList.observable.mdcSelect.layout();
        self.addressSearcher.countryDropDownList.observable.mdcSelect.outline_.notch(self.addressSearcher.countryDropDownList.observable.mdcSelect.label_.getWidth() * 0.75);
    }

    self.errors = ko.validation.group([
        self.name.observable,
        self.companyName.observable,
        self.addressLine1.observable,
        self.addressLine2.observable,
        self.town.observable,
        self.county.observable,
        self.postcode.observable,
        self.country,
        self.countryId,
        self.email.observable,
        self.phone.observable
    ]);

    self.reset = function (keepCountry) {

        keepCountry = typeof keepCountry === 'undefined' ? false : keepCountry;

        self.deletable(true);

        if (ko.isObservable(self.validateNow)) {
            self.validateNow(false);
        }

        self.initialState({});

        self.removeCleanseSubscriptions();
        self.removeDeliveryReferenceSubscriptions();

        self.manual(false);
        self.tradeAccountAddressId(undefined);

        self.companyName.observable(undefined);
        self.name.observable(undefined);
        self.addressLine1.observable(undefined);
        self.addressLine2.observable(undefined);
        self.town.observable(undefined);
        self.county.observable(undefined);
        self.postcode.observable(undefined);
        self.phone.observable(undefined);

        self.deliveryReference(undefined);

        if (!keepCountry) {
            if (self.countryWidget()) {
                if (self.defaultCountry) {
                    self.countryWidget().value(self.defaultCountry);
                }
                else {
                    self.countryWidget().value('');
                }
                window.setTimeout(function () {
                    self.countryWidget().trigger('change');
                }, 0);
            }
        }

        self.companyName.readOnly(false);
        self.companyName.disabled(false);
        self.name.readOnly(false);
        self.name.disabled(false);
        self.addressLine1.readOnly(false);
        self.addressLine1.disabled(false);
        self.addressLine2.readOnly(false);
        self.addressLine2.disabled(false);
        self.town.readOnly(false);
        self.town.disabled(false);
        self.county.readOnly(false);
        self.county.disabled(false);
        self.postcode.readOnly(false);
        self.postcode.disabled(false);
        self.phone.readOnly(false);
        self.phone.disabled(false);
        self.email.readOnly(false);
        self.email.disabled(false);

        if (typeof self.addressSearchCountryDropDownList === 'object')
            self.addressSearchCountryDropDownList.disabled(false);

        if (typeof self.countryDropDownList === 'object')
                self.countryDropDownList.disabled(false);

        self.promptForOverwrite(false);

        self.errors.showAllMessages(false);
    }

    self.toJS = function () {
        var data = {
            isLoqateAddress: self.loqateAddress(),
            manual: self.manual(),
            deliveryReference: self.deliveryReference(),
            deliveryName: self.name.observable(),
            deliveryCompanyName: self.companyName.observable(),
            deliveryAddressLine1: self.addressLine1.observable(),
            deliveryAddressLine2: self.addressLine2.observable(),
            deliveryTown: self.town.observable(),
            deliveryCounty: self.county.observable(),
            deliveryPostcode: self.postcode.observable(),
            deliveryCountryId: self.country() ? self.country().CountryID : self.countryId(),
            deliveryCountryCode: self.country() ? (self.country().Code || self.country()) : null,
            email: self.email.observable()
        }
        if (self.phone.observable.hasOwnProperty('intlTel')) {
            data.deliveryPhoneNumber = $(self.phone.observable.intlTel).intlTelInput('getNumber');
        } else {
            data.deliveryPhoneNumber = self.phone.observable();
        }

        return data;
    }

    self.toPageSectionFormJS = function () {
        var data = {
            fullname: self.name.observable(),
            companyName: self.companyName.observable(),
            addressLine1: self.addressLine1.observable(),
            addressLine2: self.addressLine2.observable(),
            town: self.town.observable(),
            county: self.county.observable(),
            postcode: self.postcode.observable(),
            countryId: self.country() ? self.country().CountryID : self.countryId()
        }
        if (self.phone.observable.hasOwnProperty('intlTel')) {
            data.phoneNumber = $(self.phone.observable.intlTel).intlTelInput('getNumber');
        } else {
            data.phoneNumber = self.phone.observable();
        }

        return data;
    }

    self.displayLine = ko.pureComputed({
        read: function () {

            let domString = ``;

            if (self.hasOwnProperty('endCustomerTradeAccountName'))
                if (self.endCustomerTradeAccountName != null)
                    domString += `<label>${self.endCustomerTradeAccountName + (self.deliveryReference() ? ' <span class="text-transparent">' + self.deliveryReference() + '</span>' : '')}</label>`

            if (self.companyName.observable())
                domString += `<label>${self.companyName.observable() + (self.deliveryReference() ? ' <span class="text-transparent">' + self.deliveryReference() + '</span>' : '')}</label>`

            if (self.name.observable()) {
                domString += `<span>${self.name.observable()}</span>`;
            }
            if (self.addressLine1.observable()) {
                domString += `<span>${self.addressLine1.observable()}</span>`;
            }
            if (self.addressLine2.observable()) {
                domString += `<span>${self.addressLine2.observable()}</span>`;
            }
            if (self.town.observable()) {
                domString += `<span>${self.town.observable()}</span>`;
            }
            if (self.county.observable()) {
                domString += `<span>${self.county.observable()}</span>`;
            }
            if (self.postcode.observable()) {
                domString += `<span>${self.postcode.observable()}</span>`
            }
            if (self.country()) {
                domString += `<span>${self.country().Name}</span>`;
            }

            domString = `<div class="address-display-line">${domString}</div>`;

            return domString;
        }
    }, self);

    self.singleLineDisplay = ko.pureComputed({
        read: function () {

            let domString = ``;

            if (self.name.observable())
                domString += `<span class="name">${self.name.observable()}</span>`;
            if (self.addressLine1.observable())
                domString += `<span class="address">${self.addressLine1.observable()}</span>`;
            if (self.postcode.observable())
                domString += `<span class="postcode">${self.postcode.observable()}</span>`;
            if (self.country())
                domString += `<span class="countrycode">${self.country().Name}</span>`;

            domString = `<div class="address-display-single-line">${domString}</div>`;

            return domString;
        }
    }, self);

    self.displayBlock = ko.pureComputed({
        read: function () {

            let domString = ``;

            if (self.deliveryReference()) {
                domString += `<label>${self.deliveryReference()}</label>`;
            }

            if (self.companyName.observable()) {
                domString += `<label>${self.companyName.observable()}</label>`;
            }
            if (self.hasOwnProperty('endCustomerTradeAccountName'))
                if (self.endCustomerTradeAccountName != null)
                    domString += `<span class='address-block-span'>${self.endCustomerTradeAccountName}</span>`;

            if (self.name.observable()) {
                domString += `<span class='address-block-span'>${self.name.observable()}</span>`;
            }
            if (self.addressLine1.observable()) {
                domString += `<span class='address-block-span'>${self.addressLine1.observable()}</span>`;
            }
            if (self.addressLine2.observable()) {
                domString += `<span class='address-block-span'>${self.addressLine2.observable()}</span>`;
            }
            if (self.town.observable()) {
                domString += `<span class='address-block-span'>${self.town.observable()}</span>`;
            }
            if (self.county.observable()) {
                domString += `<span class='address-block-span'>${self.county.observable()}</span>`;
            }
            if (self.postcode.observable()) {
                domString += `<span class='address-block-span'>${self.postcode.observable()}</span>`;
            }
            if (self.country()) {
                if (self.country().hasOwnProperty('Name'))
                    domString += `<span class='address-block-span address-block-country'>${self.country().Name}</span>`;
                else
                    domString += `<span class='address-block-span address-block-country'>${self.country()}</span>`;
            }

            if (self.phone.observable()) {
                domString += `<span class="address-block-spacer"></span>`;
                domString += `<span class='address-block-span address-block-phone'>${self.phone.observable()}</span>`;
            }

            if (self.email.observable()) {
                if (self.phone.observable() == null)
                    domString += `<span class="address-block-spacer"></span>`;
                domString += `<a class="address-block-span address-block-email" href="mailto:${self.email.observable()}">${self.email.observable()}</span>`;
            }

            domString = `<div class="address-display-block">${domString}</div>`

            return domString;

            return result.prop('outerHTML');
        }
    }, self);

    self.displayText = ko.pureComputed({
        read: function () {
            var result = '';
            if (self.name.observable())
                result += self.name.observable() + ' ';
            if (self.addressLine1.observable())
                result += self.addressLine1.observable() + ' ';
            if (self.postcode.observable())
                result += self.postcode.observable() + ' ';
            if (self.country())
                result += self.country().Code
            return result;
        }
    }, self);

    self.notifySubscribers = function () {
        if (self.phone.observable.hasOwnProperty('intlTel')) {
            if ($(self.phone.observable.intlTel).intlTelInput('getNumber')) {
                if (!$(self.phone.observable.intlTel).intlTelInput('isValidNumber')) {
                    self.phone.observable.isValid.notifySubscribers(false);
                }
            }
        }

        self.errors.filter(function (x) {
            if (x.hasOwnProperty('isValid')) {
                if (!x.isValid())
                    x.isValid.notifySubscribers(x.isValid());
            } else if (x.hasOwnProperty('observable')) {
                if (x.observable.hasOwnProperty('isValid')) {
                    if (!x.observable.isValid())
                        x.observable.isValid.notifySubscribers(x.observable.isValid());
                }
            }
        });
    }

    return self;
});

var addressSearchFactory = ((
    tradeAccount,
    title,
    label,
    verb,
    addressObject,
    busy,
    busyMessage,
    saveFunction,
    isMiscTradeAccount,
    isInvoiceSearch,
    resources,
    mode,
    afterSearch) => {

    var self = {
        resources: resources
    };

    self.ready = ko.observable(false);

    self.tradeAccount = ko.observable(tradeAccount());

    self.isMiscTradeAccount = isMiscTradeAccount;

    self.isInvoiceSearch = isInvoiceSearch;

    self.afterSearch = afterSearch;

    //self.isMiscTradeAccount.subscribe(function (newValue) {
    //    if (newValue) {
    //        self.pcaLookup.searchEndpoint(self.pcaEndpoint);
    //    }
    //});

    self.searchEndpoint = '/api/generalapi/addressSearch';

    self.editModalBody = ko.observable(); // where to attach dropdowns to

    self.mode = ko.observable(mode ?? 'extant');

    self.title = ko.observable(title);

    self.verb = ko.observable(verb);

    self.label = ko.observable(label);

    self.saveFunction = saveFunction;

    self.addressObject = addressObject;

    if (self.addressObject) {
        self.addressObject.tradeAccount = self.tradeAccount;
        self.addressObject.addressSearcher = self;
    }
    
    self.includeEU = ko.observable(true); // by default, permit searching for countries within the EU.

    self.tradeAccount.subscribe(function (newValue) {
        console.log('addressSearcher tradeAccount subscribe hit', newValue);
        if (newValue)
            self.countryId(newValue.CountryID);
        else
            self.countryId(undefined);
    });

    self.countryId = ko.observable(self.tradeAccount() ? self.tradeAccount().CountryID : undefined); // why store both...? because the list needs to bind to a single value, not an object.

    self.country = ko.observable(); // the full Country object.

    self.widget = ko.observable(); // the Kendo widget for the list.

    self.addressSearchString = ko.observable().extend({
        rateLimit: {
            timeout: 300,
            method: 'notifyWhenChangesStop'
        }
    }); // the string provided for the autocompleter to search on

    self.addressSearchStringHasFocus = ko.observable(false); // whether or not the autocompleter has focus

    self.secondaryAddressSearchString = ko.observable().extend({
        rateLimit: {
            timeout: 300,
            method: 'notifyWhenChangesStop'
        }
    }); // the string provided for the autocompleter to search on

    self.secondaryAddressSearchStringHasFocus = ko.observable(false); // whether or not the autocompleter has focus

    self.pcaInputGroup = ko.observable(); // the input group itself, to provide class switching

    self.editModal = ko.observable(); // this searcher's edit modal

    self.busy = busy || ko.observable(false); // parent's busy, or own

    self.busyMessage = busyMessage || ko.observable(''); // parent's busy message, or own

    self.country.subscribe(function (newValue) {
        if (newValue) {
            if (self.pcaLookup.addressRequired)
                $(self.addressSearchString.DOMElement).autocomplete('search');
        } else {
            self.pcaLookup.clearFunction();
        }
    });

    self.countryDropDownList = () => {
        //console.log('countryDropDownList fn exec', self.editModalBody.DOMElement);
        let result = {
            observable: self.country,
            showClear: false,
            label: self.resources.FormsCountry,
            valuePrimitive: false,
            useNativeValidation: false,
            decorateInputElement: false,
            insertValidationMessages: false,
            controlClass: 'form-control',
            dropDownListSettings: {
                value: self.countryId,
                dataItem: self.country,
                dataValueField: 'CountryID',
                dataTextField: 'Name',
                widget: self.widget,
                open: function (e) {
                    self.country.mdcSelect.root_.classList.add('mdc-select--focused');
                },
                close: function (e) {
                    self.country.mdcSelect.root_.classList.add('mdc-select--focused');
                },
                dataBound: function (e) {
                    self.country.mdcSelect.layout();
                },
                data: new kendo.data.DataSource({
                    serverFiltering: true,
                    serverSorting: true,
                    serverGrouping: true,
                    schema: $.extend({}, kendo.data.schemas.webapi, {
                        data: "Data",
                        total: "Total",
                        filters: "Filters",
                        aggregates: "Aggregates",
                        errors: "Errors"
                    }),
                    transport: {
                        read: function (options) {
                            var webapi = new kendo.data.transports.webapi({ prefix: "" });
                            var mapped = webapi.parameterMap(options.data);
                            var data = {};
                            for (var propertyName in mapped) {
                                data[propertyName] = mapped[propertyName];
                            }
                            data.includeEU = self.includeEU;

                            fetch(`/api/tradeapi/getcountries?${new URLSearchParams(data).toString()}`, {
                                method: 'GET',
                                headers: window.fetchHeaders,
                            })
                                .then(window.handleFetchResponse)
                                .then(data => {
                                    options.success(data);
                                })
                                .catch(err => {
                                    options.error();
                                    window.catchFetchErrors(err);
                                });
                        }
                    }
                }),
                valuePrimitive: false,
                autoWidth: false,
                autoBind: true,
                optionLabel: ' ',
                filter: 'startswith',
                popup: {
                    appendTo: self.editModalBody.DOMElement
                }
            },
            clearFunction: function () {
                var self = this;
                self.observable(undefined);
                self.dropDownListSettings.widget().value('');
                self.dropDownListSettings.widget()._widgetOptions.dataItem.mdcSelect.getDefaultFoundation().adapter_.closeOutline();
                self.dropDownListSettings.widget()._widgetOptions.dataItem.mdcSelect.label_.float(false);
            }
        }

        self.countryDropDownList = result;

        return result;
    };

    self.pcaLookup = {
        mode: self.mode,
        label: self.label,
        busy: self.busy,
        readyNotice: self.ready,
        inputGroup: self.pcaInputGroup,
        searchResultsList: ko.observableArray([]),
        searchEndpoint: ko.observable(self.searchEndpoint), // current endpoint to search against; default to Mertex. Only Misc accounts should default to pca.
        afterSearch: self.afterSearch,
        tradeAccount: self.tradeAccount,
        miscTradeAccount: self.isMiscTradeAccount,
        resources: self.resources,
        addressSearchWrapper: {
            editModal: self.editModal
        },
        observable: self.secondaryAddressSearchString,
        focusObservable: self.secondaryAddressSearchStringHasFocus,
        mapFunction: function (companyName, name, line1, line2, line3, line4, postcode, phone, email, deliveryReference, loqateAddress, countryId) {

            console.log('addressSearcher mapFunction hit');

            if (countryId == null)
                countryId = self.countryId();

            self.addressObject.removeCleanseSubscriptions();
            self.addressObject.removeDeliveryReferenceSubscriptions();

            self.addressObject.reset();

            if (loqateAddress)
                self.addressObject.loqateAddress(true);

            if (deliveryReference)
                self.addressObject.deliveryReference(deliveryReference);

            if (companyName)
                self.addressObject.companyName.observable(companyName);

            if (name)
                self.addressObject.name.observable(name);

            if (line1) {
                self.addressObject.addressLine1.observable(line1);
            }

            if (line2) {
                self.addressObject.addressLine2.observable(line2);
            }

            if (line3) {
                self.addressObject.town.observable(line3);
            }

            if (line4) {
                self.addressObject.county.observable(line4);
            }

            if (postcode) {
                self.addressObject.postcode.observable(postcode);
            }

            if (phone) {
                self.addressObject.phone.observable(phone);
            }

            if (email) {
                self.addressObject.email.observable(email);
            }

            if (countryId) {
                self.addressObject.countryId(countryId);
                self.addressObject.countryWidget().value(countryId);
                self.addressObject.countryWidget().dataSource.read();
                self.countryId(countryId); // set the search as well, why not - also means we don't float and notch the label while unselected
            }

            self.showEditModal();
        },
        country: self.country,
        position: { my: "right top", at: "right bottom", collision: "none" },
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        minLength: 0,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            self.observable.PCASearchId(null);
            self.observable.PCAContainer(null);
            self.observable.DOMElement.focus();

            $(self.observable.DOMElement).autocomplete('search', '');

            //if (self.searchEndpoint().indexOf('mertex') > -1 && $(self.observable.DOMElement).is(':visible')) {
            //    $(self.observable.DOMElement).autocomplete('search', ' ');
            //}
        },
        itemSelect: function (item) {
            console.log('addressSearcher itemSelect hit', item);

            if (item.hasOwnProperty('type')) {
                switch (item.type) {
                    case 'Mertex':
                    case 'Commercial':
                    case 'Residential':
                    case 'Unknown': // yeah I know, I know...
                        self.pcaLookup.mapFunction(
                            item.companyName,
                            item.name,
                            item.addressLine1,
                            item.addressLine2,
                            item.addressLine3,
                            item.addressLine4,
                            item.postcode,
                            item.phone,
                            item.email,
                            item.deliveryReference,
                            item.loqateAddress,
                            item.countryId);

                        self.addressObject.promptForOverwrite(false);

                        self.secondaryAddressSearchString.PCASearchId(null);
                        self.secondaryAddressSearchString.PCAContainer(null);

                        if (self.secondaryAddressSearchString.hasOwnProperty('PCASearchTerm'))
                            self.secondaryAddressSearchString.PCASearchTerm(null);

                        break;
                    case 'Address':
                        self.secondaryAddressSearchString.PCASearchId(item.id);
                        self.secondaryAddressSearchString.PCAContainer(null);
                        // at this point we KNOW we're drilling down for the minutae of a particular address ID in Loqate
                        self.secondaryAddressSearchString.performSearch(function () {

                        });
                        break;
                    case 'Postcode':
                    // 'Postcode' may well not be the only "container" type object which we need to open when clicked...
                    default:
                        self.secondaryAddressSearchString.PCASearchId(null);
                        self.secondaryAddressSearchString.PCAContainer(item.id);
                        $(self.secondaryAddressSearchString.DOMElement).autocomplete('search');
                        return false;
                }
            }
        }
    };

    self.addressLookup = {
        mode: self.mode,
        label: self.label,
        busy: self.busy,
        inputGroup: self.pcaInputGroup,
        searchResultsList: ko.observableArray([]),
        searchEndpoint: ko.observable(self.searchEndpoint), // current endpoint to search against; default to Mertex. Only Misc accounts should default to pca.
        afterSearch: self.afterSearch,
        tradeAccount: self.tradeAccount,
        miscTradeAccount: self.isMiscTradeAccount,
        resources: self.resources,
        addressSearchWrapper: {
            editModal: self.editModal
        },
        observable: self.addressSearchString,
        focusObservable: self.addressSearchStringHasFocus,
        mapFunction: function (companyName, name, line1, line2, line3, line4, postcode, phone, email, deliveryReference, loqateAddress, countryId) {


            if (countryId == null)
                countryId = self.countryId();

            self.addressObject.removeCleanseSubscriptions();
            self.addressObject.removeDeliveryReferenceSubscriptions();

            self.addressObject.reset();

            if (loqateAddress)
                self.addressObject.loqateAddress(true);

            if (deliveryReference)
                self.addressObject.deliveryReference(deliveryReference);

            if (companyName)
                self.addressObject.companyName.observable(companyName);

            if (name)
                self.addressObject.name.observable(name);

            if (line1) {
                self.addressObject.addressLine1.observable(line1);
            }

            if (line2) {
                self.addressObject.addressLine2.observable(line2);
            }

            if (line3) {
                self.addressObject.town.observable(line3);
            }

            if (line4) {
                self.addressObject.county.observable(line4);
            }

            if (postcode) {
                self.addressObject.postcode.observable(postcode);
            }

            if (phone) {
                self.addressObject.phone.observable(phone);
            }

            if (email) {
                self.addressObject.email.observable(email);
            }

            if (countryId) {
                self.addressObject.countryId(countryId);
                //self.addressObject.countryWidget().dataSource.read();
            }

            self.showEditModal();
        },
        country: self.country,
        position: { my: "right top", at: "right bottom", collision: "none" },
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        minLength: 0,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            self.observable.PCASearchId(null);
            self.observable.PCAContainer(null);
            self.observable.DOMElement.focus();

            $(self.observable.DOMElement).autocomplete('search', '');

        },
        itemSelect: function (item) {
            console.log('addressSearcher itemSelect hit', item);

            if (item.hasOwnProperty('type')) {
                switch (item.type) {
                    case 'Mertex':
                    case 'Commercial':
                    case 'Residential':
                    case 'Unknown': // yeah I know, I know...
                        self.pcaLookup.mapFunction(
                            item.companyName,
                            item.name,
                            item.addressLine1,
                            item.addressLine2,
                            item.addressLine3,
                            item.addressLine4,
                            item.postcode,
                            item.phone,
                            item.email,
                            item.deliveryReference,
                            item.loqateAddress,
                            item.countryId);

                        self.addressObject.promptForOverwrite(false);

                        self.addressSearchString.PCASearchId(null);
                        self.addressSearchString.PCAContainer(null);

                        if (self.addressSearchString.hasOwnProperty('PCASearchTerm'))
                            self.addressSearchString.PCASearchTerm(null);

                        break;
                    case 'Address':
                        self.addressSearchString.PCASearchId(item.id);
                        self.addressSearchString.PCAContainer(null);
                        // at this point we KNOW we're drilling down for the minutae of a particular address ID in Loqate
                        self.addressSearchString.performSearch(function () {

                        });
                        break;
                    case 'Postcode':
                    // 'Postcode' may well not be the only "container" type object which we need to open when clicked...
                    default:
                        self.addressSearchString.PCASearchId(null);
                        self.addressSearchString.PCAContainer(item.id);
                        $(self.addressSearchString.DOMElement).autocomplete('search');
                        return false;
                }
            }
        }
    };

    self.descendantsComplete = function (node) {
        console.log('descendantsComplete');
    }

    self.addressLookup.searchResultsList.subscribe(function (newValues) {
        if (self.pcaInputGroup.hasOwnProperty('DOMElement')) {
            if (newValues.length) {
                self.pcaInputGroup.DOMElement.classList.add('has-results');
            } else {
                self.pcaInputGroup.DOMElement.classList.remove('has-results');
            }
        }
    });

    self.showEditModal = function (addingNew) {
        addingNew = typeof addingNew === 'undefined' ? false : addingNew;
        if (addingNew)
            self.addressObject.reset(true);

        if (self.editModal.hasOwnProperty('DOMElement'))
            bootstrap.Modal.getOrCreateInstance(self.editModal.DOMElement).show();

        if (self.country()) {
            self.country.mdcSelect.layout();
            self.country.mdcSelect.outline_.notch(self.country.mdcSelect.label_.getWidth() * 0.75);
        }

        if (self.editModal.hasOwnProperty('DOMElement'))
            self.editModal.DOMElement.addEventListener('shown.bs.modal', self.addressObject.reflowLabels);
    }

    self.closeEditModal = function () {
        self.addressObject.validateNow(false);
        if (self.editModal.hasOwnProperty('DOMElement'))
            bootstrap.Modal.getOrCreateInstance(self.editModal.DOMElement).hide();
    }

    self.validateAddress = function () {
        self.addressObject.validateNow(true);
        window.setTimeout(function () {
            if (self.addressObject.isValid()) {
                if (typeof self.saveFunction === 'function') {
                    self.saveFunction(function () {
                        if (self.editModal.hasOwnProperty('DOMElement'))
                            bootstrap.Modal.getOrCreateInstance(self.editModal.DOMElement).hide();
                    });
                }
                else {
                    if (self.editModal.hasOwnProperty('DOMElement'))
                        bootstrap.Modal.getOrCreateInstance(self.editModal.DOMElement).hide();
                }
            }
        }, 0);
    }

    self.deleteTradeAccountAddressCleanseRecord = function (rec) {
        window.showConfirm(
            self.resources.AreYouSure,
            self.resources.ThisWillMarkAddressDeleted,
            () => {
                fetch('/api/tradeapi/DeleteTradeAccountAddressCleanseRecord', {
                    method: 'POST',
                    headers: window.fetchHeaders,
                    cache: 'no-store',
                    credentials: 'same-origin',
                    body: JSON.stringify({
                        deliveryReference: rec.obj.DeliveryReference
                    })
                })
                    .then(window.handleFetchResponse)
                    .then(data => {
                        self.addressLookup.searchResultsList.remove(rec);
                    })
                    .catch(err => {
                        window.catchFetchErrors(err);
                    })
            },
            null
        )
    }

    self.reset = function (keepCountry) {
        self.cleanseResult(undefined);
        self.addressObject.reset(keepCountry);
        self.countryId(undefined); // this one action should cause the country observable to be nullified...?
        self.addressSearchString(undefined);
        self.secondaryAddressSearchString(undefined);
        //self.pcaLookup.searchEndpoint(self.searchEndpoint);
        self.addressLookup.searchResultsList([]);
        self.cleanseResult(undefined);
        self.havePerformedLoqateSearch(false);
        self.cleanseResultAccepted(false);
        self.showManualCleanse(false);
        self.cleanseBusy(false);
    }

    self.setAddressObject = (addressObject) => {
        self.addressObject = addressObject;
        self.addressObject.tradeAccount = self.tradeAccount;
        self.addressObject.addressSearcher = self;
    }

    // when should the addressObject be reset.
    // upon RESET... duh... but... how many problems will THAT cause.

    return self;
});