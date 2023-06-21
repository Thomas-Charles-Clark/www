const tradeAccountRequestViewModel = ((resources) => {
    let self = {
        resources: resources
    };

    //#region Properties

    self.submitBusy = ko.observable(false);

    self.ignoreWarning = ko.observable(false);

    self.availableTitles = ko.observable(getAvailableTitles());

    self.availableCompanyTypes = ko.observable(getAvailableCompanyTypes());

    self.availableBusinessTypes = ko.observable(getAvailableBusinessTypes());

    self.availableTradingForOptions = ko.observable(getAvailableTradingForOptions());

    self.availableProductTypeGroupBrands = ko.observable(getAvailableProductTypeGroupBrands());

    self.patternBookInterest = ko.observable(false);

    self.availableContactTypes = ko.observable(getContactTypes());

    self.availableChannels = ko.observable(getChannels());

    self.availableLanguages = ko.observable(getAvailableLanguages());

    self.deliverySameAsInvoice = ko.observable(true);

    self.visitSameAsInvoice = ko.observable(true);

    self.immediateOrderRequested = ko.observable(getImmediateOrderState());

    self.warningModalElement = document.querySelector('#warning-modal');

    self.titleId = ko.observable().extend({
        required: {
            params: true,
            message: self.resources.PleaseProvideTitle
        }
    });

    self.companyName = ko.observable().extend({
        required: {
            params: true,
            message: self.resources.companyNameValidation
        }
    });

    self.channelId = ko.observable().extend({
        required: {
            params: true,
            message: self.resources.PleaseProvideBusinessType
        }
    });

    self.contactTypeId = ko.observable().extend({
        required: {
            params: true,
            message: self.resources.PleaseProvideYourPosition
        }
    });

    self.companyRegistration = ko.observable().extend({
        required: {
            params: true,
            message: self.resources.PleaseProvideCompanyRegistration
        }
    });

    self.vatRegistered = ko.observable(false);

    self.vatCountryId = ko.observable();

    self.vatCountry = ko.observable();

    self.vatCountryWidget = ko.observable();

    self.companyVATNumber = ko.observable().extend({
        required: {
            params: true,
            onlyIf: self.vatRegistered,
            message: self.resources.PleaseProvideVATNumber
        }
    });

    self.vatNumberDisplay = ko.observable();

    // Swiss only
    self.taxId = ko.observable();

    self.autoCorrespondenceLanguageCode = ko.observable().extend({
        required: {
            params: true,
            onlyIf: self.validateNow,
            message: self.resources.PleaseSelectLanguage
        }
    });

    self.eoriRegistered = ko.observable(false);

    self.companyEORINumber = ko.observable().extend({
        charsRemaining: {
            maxLength: 20,
            elementId: '#company-eori'
        },
        required: {
            params: true,
            onlyIf: self.eoriRegistered,
            message: self.resources.PleaseProvideYourEORI
        }
    });

    self.companyTypeId = ko.observable().extend({
        required: {
            params: true,
            message: self.resources.PleaseProvideCompanyType
        }
    });

    self.tradingSinceId = ko.observable().extend({
        required: {
            params: true,
            message: self.resources.PleaseProvideTradingFor
        }
    });

    self.fullName = ko.observable().extend({
        required: {
            params: true,
            message: self.resources.nameValidation
        }
    });

    self.phone = ko.observable().extend({
        required: {
            params: true,
            message: self.resources.phoneValidation
        }
    });

    self.email = ko.observable().extend({
        email: {
            params: true,
            message: self.resources.emailValidationFormat
        },
        required: {
            params: true,
            message: self.resources.emailValidation
        }
    });

    self.emailHasFocus = ko.observable(false);

    self.orderConfirmationsSameAsPrimary = ko.observable(true);

    self.invoicesStatementsSameAsPrimary = ko.observable(true);

    self.productNewsEventsSameAsPrimary = ko.observable(true);

    self.pricelistsSameAsPrimary = ko.observable(true);

    self.orderConfirmationsEmail = ko.observable().extend({
        email: {
            params: true,
            message: self.resources.emailValidationFormat
        },
        required: {
            params: true,
            onlyIf: function () {
                return !self.orderConfirmationsSameAsPrimary();
            },
            message: self.resources.emailValidation
        }
    });

    self.invoicesStatementsEmail = ko.observable().extend({
        email: {
            params: true,
            message: self.resources.emailValidationFormat
        },
        required: {
            params: true,
            onlyIf: function () {
                return !self.invoicesStatementsSameAsPrimary();
            },
            message: self.resources.emailValidation
        }
    });

    self.productNewsEventsEmail = ko.observable().extend({
        email: {
            params: true,
            message: self.resources.emailValidationFormat
        },
        required: {
            params: true,
            onlyIf: function () {
                return !self.productNewsEventsSameAsPrimary();
            },
            message: self.resources.emailValidation
        }
    });

    self.pricelistsEmail = ko.observable().extend({
        email: {
            params: true,
            message: self.resources.emailValidationFormat
        },
        required: {
            params: true,
            onlyIf: function () {
                return !self.pricelistsSameAsPrimary();
            },
            message: self.resources.emailValidation
        }
    });

    self.sellOnline = ko.observable(false);

    self.website = ko.observable().extend({
        required: {
            params: true,
            onlyIf: self.sellOnline,
            message: self.resources.WebsiteRequired
        }
    });

    self.invoiceAddress = addressViewModel({
        labelClass: 'delivery-address-label',
        controlClass: 'form-control mdc-text-field__input',
        phoneIntlMobile: false,
        email: false,
        defaultCountry: getCountryId()
    }, self.resources);

    self.invoiceAddressSearch = addressSearchFactory(
        ko.observable({
            CountryID: getCountryId()
        }),
        self.resources.AddressSearch,
        self.resources.SearchYourAddressBook,
        self.resources.MoodboardSave,
        self.invoiceAddress,
        null, // needs its own busy observable
        null, // message probably not required
        null, // save function not necessary
        false,
        true,
        self.resources);

    self.deliveryAddress = addressViewModel({
        labelClass: 'delivery-address-label',
        controlClass: 'form-control mdc-text-field__input',
        phoneIntlMobile: false,
        email: false,
        defaultCountry: getCountryId()
    }, self.resources);

    self.deliveryAddressSearch = addressSearchFactory(
        ko.observable({
            CountryID: getCountryId()
        }),
        self.resources.AddressSearch,
        self.resources.SearchYourAddressBook,
        self.resources.MoodboardSave,
        self.deliveryAddress,
        null, // needs its own busy observable
        null, // message probably not required
        null, // save function not necessary
        false,
        true,
        self.resources);

    self.visitAddress = addressViewModel({
        labelClass: 'delivery-address-label',
        controlClass: 'form-control mdc-text-field__input',
        phoneIntlMobile: false,
        email: false,
        defaultCountry: getCountryId()
    }, self.resources);

    self.visitAddressSearch = addressSearchFactory(
        ko.observable({
            CountryID: getCountryId()
        }),
        self.resources.AddressSearch,
        self.resources.SearchYourAddressBook,
        self.resources.MoodboardSave,
        self.visitAddress,
        null, // needs its own busy observable
        null, // message probably not required
        null, // save function not necessary
        false,
        true,
        self.resources);

    self.notes = ko.observable().extend({
        required: {
            params: true,
            message: self.resources.PleaseProvideNotes
        }
    });

    self.whyWorkWithDG = ko.observable().extend({
        required: {
            params: true,
            message: self.resources.PleaseProvideNotes
        }
    });

    // Lifted because it has to be, sorry
    self.availableProductTypeGroups = ko.pureComputed({
        read: function () {

            var grouped = Enumerable.From(Enumerable.From(self.availableProductTypeGroupBrands()).Where("$.CountryID == " + self.invoiceAddress.countryId()).OrderBy("$.ProductTypeGroupSortOrder").ToArray())
                .GroupBy(
                    "{ Name: $.ProductTypeGroupName, ProductTypeGroupID: $.ProductTypeGroupID, CountryID: $.CountryID }", // Group on ProductTypeGroup object
                    "", // Select everything
                    "key, e => e", // Standard result selector
                    "$.ProductTypeGroupID" // Compare just on ProductTypeGroupID property of ProductTypeGroup object
                ).ToArray();

            return Enumerable.From(grouped).Where(function (x) {
                if (Enumerable.From(x).Select("$.Available").Sum())
                    return x;
            }).ToArray()
        }
    }, self);

    self.leadInterests = ko.observableArray([]).extend({
        minArrayLength: {
            params: {
                minLength: 1
            },
            onlyIf: function () {
                return self.availableProductTypeGroups().length > 0;
            },
            message: self.resources.PleaseSpecifyAtLeastOne
        }
    });

    self.dataSharingCountryIDs = [170, 51, 16, 175, 238, 196, 74];

    // lifted because it has to be
    self.mustOptIn = ko.computed(function () {
        var interested = Enumerable.From(self.leadInterests()).Where("($.ProductTypeGroupID == 11) || ($.ProductTypeGroupID == 12)").Any();
        if (interested) {
            if (self.dataSharingCountryIDs.indexOf(Number(getCountryId())) > -1) {
                return true;
            }
        }
        return false;
    }, self);

    self.potentiallyMandatoryOptIn = ko.observable(false).extend({
        equal: {
            params: true,
            message: self.resources.YouMustConsent,
            onlyIf: function () {
                return self.mustOptIn();
            }
        }
    });

    self.businessTypeOther = ko.observable().extend({
        required: {
            params: true,
            onlyIf: function () {
                return self.channelId() === 0
            },
            message: self.resources.PleaseSpecify
        }
    });

    self.positionOther = ko.observable().extend({
        required: {
            params: true,
            onlyIf: function () {
                return self.contactTypeId() === 0
            },
            message: self.resources.PleaseSpecify
        }
    });

    self.toValidate = ko.observableArray([
        self.companyName,
        self.companyTypeId,
        self.companyRegistration,
        self.companyVATNumber,
        self.companyEORINumber,
        self.tradingSinceId,
        self.titleId,
        self.channelId,
        self.contactTypeId,
        self.fullName,
        self.phone,
        self.email,
        self.orderConfirmationsEmail,
        self.invoicesStatementsEmail,
        self.productNewsEventsEmail,
        self.pricelistsEmail,
        self.invoiceAddress,
        self.notes,
        self.whyWorkWithDG,
        self.potentiallyMandatoryOptIn,
        self.website,
        self.businessTypeOther,
        self.positionOther
    ]);

    self.leadId = ko.observable(getLeadId());

    self.success = ko.observable(false);

    self.warningModalElement = document.querySelector('#warning-modal');

    self.thankYouModalElement = document.querySelector('#thank-you-modal');

    //#endregion


    //#region Components

    self.companyNameParams = {
        label: self.resources.FormsCompanyName,
        observable: self.companyName,
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
            maxLength: 35
        }
    };

    self.businessTypeOtherParams = {
        label: self.resources.PleaseSpecify,
        observable: self.businessTypeOther,
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
        }
    };

    self.companyRegistrationParams = {
        label: self.resources.CompanyRegistrationNumber,
        observable: self.companyRegistration,
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

    self.areYouVATRegisteredParams = {
        checkboxId: 'are-you-vat-registered',
        label: self.resources.AreYouVATRegistered,
        observable: self.vatRegistered,
        useNativeValidation: false,
        disabled: false
    };

    self.vatNumberParams = {
        label: self.resources.VATNumber,
        observable: self.companyVATNumber,
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

    self.vatNumberDisplayParams = {
        label: self.resources.VATInvoiceFormat,
        observable: self.vatNumberDisplay,
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

    self.vatCountryDropDownListParams = {
        observable: self.vatCountry,
        showClear: false,
        width: '180px',
        label: self.resources.VATCountry,
        valuePrimitive: false,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        controlClass: 'form-control',
        disabled: ko.observable(false),
        dropDownListSettings: ko.observable({
            value: self.vatCountryId,
            dataItem: self.vatCountry,
            dataValueField: 'CountryID',
            dataTextField: 'Name',
            widget: self.vatCountryWidget,
            open: function (e) {
                (self.vatCountry.mdcSelect.root_).classList.add('mdc-select--focused');
            },
            close: function (e, event) {
                (self.vatCountry.mdcSelect.root_).classList.remove('mdc-select--focused');
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

                        data.includeEU = true;

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
            filter: 'startswith'
        }),
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            self.dropDownListSettings.widget().value('');
            self.dropDownListSettings.widget()._widgetOptions.dataItem.mdcSelect.getDefaultFoundation().adapter_.closeOutline();
            self.dropDownListSettings.widget()._widgetOptions.dataItem.mdcSelect.label_.float(false);
        }
    }

    self.taxIdParams = {
        label: self.resources.TaxID,
        observable: self.taxId,
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
        }
    };

    self.autoCorrespondenceLanguageDropDownList = {
        observable: self.autoCorrespondenceLanguageCode,
        showClear: true,
        width: 'auto',
        label: self.resources.WhichLanguageForAutomatedCorrespondence,
        valuePrimitive: false,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        controlClass: 'form-control',
        dropDownListSettings: {
            value: self.autoCorrespondenceLanguageCode,
            dataItem: ko.observable(),
            dataValueField: 'LanguageCode',
            dataTextField: 'Name',
            widget: ko.observable(),
            dataBound: function (e) {
                self.autoCorrespondenceLanguageCode.mdcSelect.layout();
            },
            data: self.availableLanguages,
            valuePrimitive: false,
            autoWidth: false,
            autoBind: true,
            optionLabel: ' '
        },
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            self.dropDownListSettings.widget().value('');
            self.observable.mdcSelect.getDefaultFoundation().adapter_.closeOutline();
            self.observable.mdcSelect.label_.float(false);
        }
    }

    self.autoCorrespondenceLanguageDropDownList.dropDownListSettings.widget.subscribe(nv => {
        let defaultCode = getAvailableLanguages().find(l => l.LanguageID == getLanguageId())?.LanguageCode ?? "01";
        self.autoCorrespondenceLanguageCode(defaultCode);
        nv.value(defaultCode);
    });

    self.doYouHaveEORIParams = {
        checkboxId: 'do-you-have-eori',
        label: self.resources.DoYouHaveEORI,
        observable: self.eoriRegistered,
        useNativeValidation: false,
        disabled: false
    };

    self.eoriNumberParams = {
        label: self.resources.EORINumber,
        observable: self.companyEORINumber,
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
    
    self.doYouSellOnlineParams = {
        checkboxId: 'do-you-sell-online',
        label: self.resources.SellOnline,
        observable: self.sellOnline,
        useNativeValidation: false,
        disabled: false
    };

    self.websiteParams = {
        label: self.resources.Website,
        observable: self.website,
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
            maxLength: 256
        }
    };

    self.positionOtherParams = {
        label: self.resources.PleaseSpecify,
        observable: self.positionOther,
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
        }
    };

    self.fullNameParams = {
        label: self.resources.FormsFullName,
        observable: self.fullName,
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
        }
    };

    self.phoneNumberParams = {
        label: self.resources.FormsPhoneNumber,
        observable: self.phone,
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

    self.emailParams = {
        label: self.resources.PrimaryEmailAddress,
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
            maxLength: 256
        }
    };

    self.orderConfirmationSameAsPrimaryParams = {
        checkboxId: 'confirmation-same-as-primary',
        label: self.resources.SameAsPrimary,
        observable: self.orderConfirmationsSameAsPrimary,
        useNativeValidation: false,
        disabled: false
    };

    self.orderConfirmationEmailParams = {
        label: self.resources.OrderConfirmations,
        observable: self.orderConfirmationsEmail,
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
            maxLength: 256
        },
        disabled: self.orderConfirmationsSameAsPrimary
    };

    self.invoicesStatementsSameAsPrimaryParams = {
        checkboxId: 'invoices-same-as-primary',
        label: self.resources.SameAsPrimary,
        observable: self.invoicesStatementsSameAsPrimary,
        useNativeValidation: false,
        disabled: false
    };

    self.invoicesStatementsEmailParams = {
        label: self.resources.InvoicesStatements,
        observable: self.invoicesStatementsEmail,
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
            maxLength: 256
        },
        disabled: self.invoicesStatementsSameAsPrimary
    };

    self.productNewsEventsSameAsPrimaryParams = {
        checkboxId: 'product-news-same-as-primary',
        label: self.resources.SameAsPrimary,
        observable: self.productNewsEventsSameAsPrimary,
        useNativeValidation: false,
        disabled: false
    };

    self.productNewsEventsEmailParams = {
        label: self.resources.ProductsNewsEvents,
        observable: self.productNewsEventsEmail,
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
            maxLength: 256
        },
        disabled: self.productNewsEventsSameAsPrimary
    };

    self.pricelistsSameAsPrimaryParams = {
        checkboxId: 'pricelists-same-as-primary',
        label: self.resources.SameAsPrimary,
        observable: self.pricelistsSameAsPrimary,
        useNativeValidation: false,
        disabled: false
    };

    self.pricelistsEmailParams = {
        label: self.resources.Pricelists,
        observable: self.pricelistsEmail,
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
            maxLength: 256
        },
        disabled: self.pricelistsSameAsPrimary
    };

    self.deliverySameAsInvoiceParams = {
        checkboxId: 'delivery-same-as-invoice',
        label: self.resources.DeliverySameAsInvoiceAddress,
        observable: self.deliverySameAsInvoice,
        useNativeValidation: false,
        disabled: false
    };

    self.visitSameAsInvoiceParams = {
        checkboxId: 'visit-same-as-invoice',
        label: self.resources.DeliverySameAsInvoiceAddress,
        observable: self.visitSameAsInvoice,
        useNativeValidation: false,
        disabled: false
    };

    self.brandsCurrentlyWorkedWithParams = {
        label: self.resources.Notes,
        observable: self.notes,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: false,
        permitRevert: false,
        rows: 3,
        maxNewLines: 3,
        charsRemainingOptions: {
            maxLength: 500
        }
    };

    self.whyWorkWithDGParams = {
        label: self.resources.WhyWorkWithDG,
        observable: self.whyWorkWithDG,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: false,
        permitRevert: false,
        rows: 3,
        maxNewLines: 3,
        charsRemainingOptions: {
            maxLength: 500
        }
    };

    self.potentiallyMandatoryOptInParams = {
        checkboxId: 'data-sharing-acknowledgement',
        label: self.resources.TARFIUnderstand,
        observable: self.potentiallyMandatoryOptIn,
        useNativeValidation: false,
        disabled: false
    };

    self.companyTypeDropDownList = {
        observable: self.companyTypeId,
        showClear: true,
        width: 'auto',
        label: self.resources.CompanyType,
        valuePrimitive: false,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        controlClass: 'form-control',
        dropDownListSettings: {
            value: self.companyTypeId,
            dataItem: ko.observable(),
            dataValueField: 'CompanyTypeID',
            dataTextField: 'Name',
            widget: ko.observable(),
            dataBound: function (e) {
                self.companyTypeId.mdcSelect.layout();
            },
            data: self.availableCompanyTypes,
            valuePrimitive: false,
            autoWidth: false,
            autoBind: true,
            optionLabel: ' '
        },
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            self.dropDownListSettings.widget().value('');
            self.observable.mdcSelect.getDefaultFoundation().adapter_.closeOutline();
            self.observable.mdcSelect.label_.float(false);
        }
    }

    self.businessTypeDropDownList = {
        observable: self.channelId,
        showClear: true,
        width: 'auto',
        label: self.resources.BusinessType,
        valuePrimitive: false,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        controlClass: 'form-control',
        dropDownListSettings: {
            value: self.channelId,
            dataItem: ko.observable(),
            dataValueField: 'ChannelID',
            dataTextField: 'Name',
            widget: ko.observable(),
            dataBound: function (e) {
                self.channelId.mdcSelect.layout();
            },
            data: self.availableChannels,
            valuePrimitive: false,
            autoWidth: false,
            autoBind: true,
            optionLabel: ' '
        },
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            self.dropDownListSettings.widget().value('');
            self.observable.mdcSelect.getDefaultFoundation().adapter_.closeOutline();
            self.observable.mdcSelect.label_.float(false);
        }
    }

    self.tradingForDropDownList = {
        observable: self.tradingSinceId,
        showClear: true,
        width: 'auto',
        label: self.resources.HowLongTradingFor,
        valuePrimitive: false,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        controlClass: 'form-control',
        dropDownListSettings: {
            value: self.tradingSinceId,
            dataItem: ko.observable(),
            dataValueField: 'Value',
            dataTextField: 'Name',
            widget: ko.observable(),
            dataBound: function (e) {
                self.tradingSinceId.mdcSelect.layout();
            },
            data: self.availableTradingForOptions,
            valuePrimitive: false,
            autoWidth: false,
            autoBind: true,
            optionLabel: ' '
        },
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            self.dropDownListSettings.widget().value('');
            self.observable.mdcSelect.getDefaultFoundation().adapter_.closeOutline();
            self.observable.mdcSelect.label_.float(false);
        }
    }

    self.contactTypeDropDownList = {
        observable: self.contactTypeId,
        showClear: true,
        width: 'auto',
        label: self.resources.TradeAccountYourPosition,
        valuePrimitive: false,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        controlClass: 'form-control',
        dropDownListSettings: {
            value: self.contactTypeId,
            dataItem: ko.observable(),
            dataValueField: 'LeadContactTypeID',
            dataTextField: 'Name',
            widget: ko.observable(),
            dataBound: function (e) {
                self.contactTypeId.mdcSelect.layout();
            },
            data: self.availableContactTypes,
            valuePrimitive: false,
            autoWidth: false,
            autoBind: true,
            optionLabel: ' '
        },
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            self.dropDownListSettings.widget().value('');
            self.observable.mdcSelect.getDefaultFoundation().adapter_.closeOutline();
            self.observable.mdcSelect.label_.float(false);
        }
    }

    self.titleDropDownList = {
        observable: self.titleId,
        showClear: true,
        width: 'auto',
        label: self.resources.Title,
        valuePrimitive: false,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        controlClass: 'form-control',
        dropDownListSettings: {
            value: self.titleId,
            dataItem: ko.observable(),
            dataValueField: 'TitleID',
            dataTextField: 'Title',
            widget: ko.observable(),
            dataBound: function (e) {
                self.titleId.mdcSelect.layout();
            },
            data: self.availableTitles,
            valuePrimitive: false,
            autoWidth: false,
            autoBind: true,
            optionLabel: ' '
        },
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            self.dropDownListSettings.widget().value('');
            self.observable.mdcSelect.getDefaultFoundation().adapter_.closeOutline();
            self.observable.mdcSelect.label_.float(false);
        }
    }

    self.formErrors = ko.validation.group(self.toValidate());

    self.interestErrors = ko.validation.group([
        self.leadInterests
    ]);

    //#endregion


    //#region Subscriptions

    self.vatRegistered.subscribe(nv => {
        if (!self.vatCountryId()) {
            window.setTimeout(() => {
                self.vatCountryId(getCountryId());
            }, 0);
        }
    });

    self.vatCountry.subscribe(nv => {
        if (nv) {
            let vn = self.companyVATNumber() || '';
            self.vatNumberDisplay(`${nv.Code}${vn}`);
        }
    });

    self.companyVATNumber.subscribe(nv => {
        if (self.vatCountry() && nv) {
            self.vatNumberDisplay(`${self.vatCountry().Code}${nv}`);
        }
    });

    self.email.subscribe((nv) => {
        if (self.email.isValid()) {
            if (self.orderConfirmationsSameAsPrimary())
                self.orderConfirmationsEmail(nv);
            if (self.invoicesStatementsSameAsPrimary())
                self.invoicesStatementsEmail(nv);
            if (self.productNewsEventsSameAsPrimary())
                self.productNewsEventsEmail(nv);
            if (self.pricelistsSameAsPrimary())
                self.pricelistsEmail(nv);
        }
    })

    self.emailHasFocus.subscribe(function (newValue) {
        if (!newValue) {
            if (self.email.isValid()) {
                if (self.orderConfirmationsSameAsPrimary()) {
                    self.orderConfirmationsEmail(self.email());
                }
                if (self.invoicesStatementsSameAsPrimary()) {
                    self.invoicesStatementsEmail(self.email());
                }
                if (self.productNewsEventsSameAsPrimary()) {
                    self.productNewsEventsEmail(self.email());
                }
                if (self.pricelistsSameAsPrimary()) {
                    self.pricelistsEmail(self.email());
                }
            }
        }
    });

    self.orderConfirmationsSameAsPrimary.subscribe(function (newValue) {
        if (newValue) {
            if (self.email.isValid())
                self.orderConfirmationsEmail(self.email());
            else {
                self.orderConfirmationsEmail(undefined);
                self.orderConfirmationsEmail.isModified(false);
            }
        } else {
            self.orderConfirmationsEmail(undefined);
            self.orderConfirmationsEmail.isModified(false);
        }
    });

    self.invoicesStatementsSameAsPrimary.subscribe(function (newValue) {
        if (newValue) {
            if (self.email.isValid())
                self.invoicesStatementsEmail(self.email());
            else {
                self.invoicesStatementsEmail(undefined);
                self.invoicesStatementsEmail.isModified(false);
            }
        } else {
            self.invoicesStatementsEmail(undefined);
            self.invoicesStatementsEmail.isModified(false);
        }
    });

    self.productNewsEventsSameAsPrimary.subscribe(function (newValue) {
        if (newValue) {
            if (self.email.isValid())
                self.productNewsEventsEmail(self.email());
            else {
                self.productNewsEventsEmail(undefined);
                self.productNewsEventsEmail.isModified(false);
            }
        } else {
            self.productNewsEventsEmail(undefined);
            self.productNewsEventsEmail.isModified(false);
        }
    });

    self.pricelistsSameAsPrimary.subscribe(function (newValue) {
        if (newValue) {
            if (self.email.isValid())
                self.pricelistsEmail(self.email());
            else {
                self.pricelistsEmail(undefined);
                self.pricelistsEmail.isModified(false);
            }
        } else {
            self.pricelistsEmail(undefined);
            self.pricelistsEmail.isModified(false);
        }
    });

    self.deliverySameAsInvoice.subscribe(function (newValue) {
        if (newValue) {
            self.toValidate.remove(self.deliveryAddress);
            self.formErrors = ko.validation.group(self.toValidate());
        } else {
            self.toValidate.push(self.deliveryAddress);
            self.formErrors = ko.validation.group(self.toValidate());
            if (self.invoiceAddress.validateNow())
                self.deliveryAddress.validateNow(true);
            window.setTimeout(() => {
                self.deliveryAddress.reflowLabels();
            }, 0);
        }
    });

    self.visitSameAsInvoice.subscribe(function (newValue) {
        if (newValue) {
            self.toValidate.remove(self.visitAddress);
            self.formErrors = ko.validation.group(self.toValidate());
        } else {
            self.toValidate.push(self.visitAddress);
            self.formErrors = ko.validation.group(self.toValidate());
            if (self.invoiceAddress.validateNow())
                self.visitAddress.validateNow(true);
            window.setTimeout(() => {
                self.visitAddress.reflowLabels();
            }, 0);
        }
    });

    //#endregion


    //#region Computeds

    self.availableBrandGroups = ko.pureComputed({
        read: function () {
            return Enumerable.From(Enumerable.From(self.availableProductTypeGroupBrands()).Where("$.CountryID == " + self.invoiceAddress.countryId()).OrderBy("$.BrandSortOrder").ToArray())
                .GroupBy(
                    "{ Name: $.BrandName, BrandID: $.BrandID }",
                    "",
                    "key, e => e",
                    "$.BrandID"
                ).ToArray()
        }
    }, self);


    //#endregion


    //#region Methods

    self.setImmediateOrderRequest = function () {
        bootstrap.Modal.getOrCreateInstance(self.thankYouModalElement).show();
        return;
        $.ajax({
            url: '/api/form/SaveTradeAccountImmediateOrderFlag',
            type: 'POST',
            data: {
                leadId: self.leadId()
            },
            success: function () {
                self.immediateOrderRequested(true);
            }
        });
    }

    self.getBrandProductTypeGroupObject = function (brandId, productTypeGroupId) {
        var result = Enumerable.From(self.availableProductTypeGroupBrands()).Where("$.BrandID == " + brandId + " && $.ProductTypeGroupID == " + productTypeGroupId + " && $.CountryID == " + self.invoiceAddress.countryId()).SingleOrDefault();
        if (result) {
            return result;
        }
    }

    self.interestedInAllProductTypesForBrand = function (brandId) {

        var anyFound = false;
        for (var i in self.availableProductTypeGroups()) {
            var ptg = self.availableProductTypeGroups()[i];
            var avail = Enumerable.From(ptg.source).Where("$.BrandID == " + brandId + " && $.Available").ToArray();

            for (var j in avail) {

                var rec = avail[j];

                var obj = self.getBrandProductTypeGroupObject(rec.BrandID, rec.ProductTypeGroupID);

                var ext = Enumerable.From(self.leadInterests()).Where(function (item) {
                    return item == obj
                }).SingleOrDefault();

                if (ext) {
                    anyFound = true;
                    self.leadInterests.remove(ext);
                }
            }
        }

        if (!anyFound) {
            for (var i in self.availableProductTypeGroups()) {
                var ptg = self.availableProductTypeGroups()[i];
                var avail = Enumerable.From(ptg.source).Where("$.BrandID == " + brandId + " && $.Available").ToArray();
                for (var j in avail) {
                    var rec = avail[j];

                    var obj = self.getBrandProductTypeGroupObject(rec.BrandID, rec.ProductTypeGroupID);

                    self.leadInterests.push(obj);
                }
            }
        }
    }

    self.interestedInAllBrandsForProductType = function (productTypeGroupId) {

        for (var i in self.availableProductTypeGroups()) {
            var ptg = self.availableProductTypeGroups()[i];
            var avail = Enumerable.From(ptg.source).Where("$.ProductTypeGroupID == " + productTypeGroupId + " && $.Available").ToArray();
            var anyFound = false;
            for (var j in avail) {
                var rec = avail[j];

                var obj = self.getBrandProductTypeGroupObject(rec.BrandID, rec.ProductTypeGroupID);

                var ext = Enumerable.From(self.leadInterests()).Where(function (item) {
                    return item == obj
                }).SingleOrDefault();

                if (ext) {
                    anyFound = true;
                    self.leadInterests.remove(ext);
                }
            }
            if (!anyFound) {
                for (var j in avail) {
                    var rec = avail[j];

                    var obj = self.getBrandProductTypeGroupObject(rec.BrandID, rec.ProductTypeGroupID);

                    self.leadInterests.push(obj);
                }
            }
        }
    }

    self.interestedInEverything = function () {
        if (self.leadInterests().length == 0) {
            for (var i in self.availableProductTypeGroups()) {
                var ptg = self.availableProductTypeGroups()[i];
                var avail = Enumerable.From(ptg.source).Where("$.Available").ToArray();

                for (var j in avail) {
                    var rec = avail[j];
                    var obj = self.getBrandProductTypeGroupObject(rec.BrandID, rec.ProductTypeGroupID);
                    self.leadInterests.push(obj);
                }
            }
        } else {
            self.leadInterests([]);
        }
    }
    
    self.submit = function () {

        $("#g-recaptcha-tip").hide();

        self.formErrors.showAllMessages(false);
        self.invoiceAddress.validateNow(false);
        self.deliveryAddress.validateNow(false);
        self.visitAddress.validateNow(false);

        self.invoiceAddress.validateNow(true);
        if (!self.deliverySameAsInvoice())
            self.deliveryAddress.validateNow(true);
        if (!self.visitSameAsInvoice())
            self.visitAddress.validateNow(true);
        if (self.formErrors().length) {
            self.formErrors.showAllMessages(true);

            self.formErrors.filter(x => {
                x.isValid.notifySubscribers(x.isValid());
            })
        }

        if (self.formErrors().length == 0 &&
            self.interestErrors().length == 0 &&
            self.invoiceAddress.isValid() &&
            self.deliveryAddress.isValid() &&
            self.visitAddress.isValid()) {
            if (!self.ignoreWarning()) {
                if (!self.companyRegistration() || !self.companyVATNumber() || !self.companyEORINumber() || !self.website()) {
                    bootstrap.Modal.getOrCreateInstance(self.warningModalElement).show();
                    return;
                }
            }

            var captcha = null;

            try {
                captcha = grecaptcha.getResponse();
            }
            catch(e) {
                // grecaptcha.getResponse will throw if there isn't actually a captcha on the page.
            }

            var invoiceAddress = {
                addressLine1: self.invoiceAddress.addressLine1.observable(),
                addressLine2: self.invoiceAddress.addressLine2.observable(),
                town: self.invoiceAddress.town.observable(),
                county: self.invoiceAddress.county.observable(),
                postcode: self.invoiceAddress.postcode.observable(),
                countryId: self.invoiceAddress.countryId()
            };

            var deliveryAddress = {};

            if (!self.deliverySameAsInvoice()) {
                deliveryAddress = {
                    addressLine1: self.deliveryAddress.addressLine1.observable(),
                    addressLine2: self.deliveryAddress.addressLine2.observable(),
                    town: self.deliveryAddress.town.observable(),
                    county: self.deliveryAddress.county.observable(),
                    postcode: self.deliveryAddress.postcode.observable(),
                    countryId: self.deliveryAddress.countryId()
                }
            }

            var visitAddress = {};

            if (!self.visitSameAsInvoice()) {
                visitAddress = {
                    addressLine1: self.visitAddress.addressLine1.observable(),
                    addressLine2: self.visitAddress.addressLine2.observable(),
                    town: self.visitAddress.town.observable(),
                    county: self.visitAddress.county.observable(),
                    postcode: self.visitAddress.postcode.observable(),
                    countryId: self.visitAddress.countryId()
                }
            }

            var params = {
                gRecaptcha: captcha,
                objMaster: getMasterDefinition(),
                companyName: self.companyName(),
                channelId: self.channelId(),
                companyRegistration: self.companyRegistration(),
                companyVATNumber: self.companyVATNumber(),
                vatCountryId: self.vatCountryId(),
                vatNumberDisplay: getCountryId() == 238 ? null : self.vatNumberDisplay(),
                taxId: self.taxId(),
                autoCorrespondenceLanguageCode: self.autoCorrespondenceLanguageCode(),
                companyEORINumber: self.companyEORINumber(),
                companyTypeId: self.companyTypeId(),
                website: self.website(),
                tradingSinceId: self.tradingSinceId(),
                contactTypeId: self.contactTypeId(),
                titleId: self.titleId(),
                fullName: self.fullName(),
                phone: self.phone(),
                email: self.email(),
                orderConfirmationsEmail: self.orderConfirmationsEmail(),
                invoicesStatementsEmail: self.invoicesStatementsEmail(),
                productNewsEventsEmail: self.productNewsEventsEmail(),
                pricelistsEmail: self.pricelistsEmail(),
                notes: self.notes() + "\n\n" + self.whyWorkWithDG(),
                leadInterests: self.leadInterests(),
                invoiceAddress: invoiceAddress,
                deliveryAddress: deliveryAddress,
                visitAddress: visitAddress,
                deliverySameAsInvoice: self.deliverySameAsInvoice(),
                visitSameAsInvoice: self.visitSameAsInvoice(),
                patternBookInterest: self.patternBookInterest(),
                distributorConsent: self.potentiallyMandatoryOptIn(),
                sellOnline: self.sellOnline(),
                businessTypeOther: self.businessTypeOther(),
                positionOther: self.positionOther(),
                leadId: self.leadId()
            }

            self.submitBusy(true);

            $.ajax({
                url: '/api/form/SaveTradeAccountRequestForm',
                type: 'POST',
                dataType: 'json',
                data: params,
                success: function(data) {
                    self.submitBusy(false);
                    if (data.hasOwnProperty('handleErrorMessage')) {
                        if (data.handleErrorMessage === "InvalidCaptcha") {
                            $("#g-recaptcha-tip").show();
                        }
                    } else {
                        if (data != null) {
                            self.success(true);
                            self.leadId(data);
                        } else {
                            window.showError();
                        }
                    }

                    try {
                        grecaptcha.reset();
                    }
                    catch (e) {

                    }
                },
                error: function(xhr, e, s) {
                    self.submitBusy(false);
                    try {
                        grecaptcha.reset();
                    }
                    catch (e) {

                    }
                }
            });

        } else {
            self.formErrors.showAllMessages(true);
            self.interestErrors.showAllMessages(true);
        }
    }

    self.ignoreWarningModal = () => {
        self.ignoreWarning(true);
        self.submit();
        bootstrap.Modal.getOrCreateInstance(self.warningModalElement).hide();
    }

    self.closeWarningModal = () => {
        bootstrap.Modal.getOrCreateInstance(self.warningModalElement).hide();
    }

    self.reset = function () {
        self.ignoreWarning(false);

        self.invoiceAddress.reset();
        self.deliveryAddress.reset();
        self.visitAddress.reset();

        self.companyName(undefined);
        self.channelId(undefined);
        self.companyRegistration(undefined);
        self.businessType(undefined);
        self.companyVATNumber(undefined);
        self.vatNumberDisplay(undefined);
        self.vatCountryWidget().value(getCountryId());
        self.companyEORINumber(undefined);
        self.companyTypeId(undefined);
        self.taxId(undefined);
        self.autoCorrespondenceLanguageCode(undefined);
        self.website(undefined);
        self.tradingSinceId(undefined);
        self.contactTypeId(undefined);
        self.titleId(undefined);
        self.fullName(undefined);
        self.email(undefined);
        self.phone(undefined);
        self.notes(undefined);
        self.whyWorkWithDG(undefined);
        self.leadInterests([]);
        self.patternBookInterest(false);
        self.potentiallyMandatoryOptIn(false);
        self.deliverySameAsInvoice(true);
        self.sellOnline(false);
        self.businessTypeOther(undefined);
        self.positionOther(undefined);

        self.formErrors.showAllMessages(false);
        self.interestErrors.showAllMessages(false);
    }

    //#endregion

    return self;
});

window.tradeAccountRequestModel = tradeAccountRequestViewModel(getPrefetchedResources());
ko.applyBindings(window.tradeAccountRequestModel, document.querySelector('#trade-wrapper'));