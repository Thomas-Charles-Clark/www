const tradeAccountContactDetailsViewModel = ((resources) => {
    let self = {
        resources: resources
    };

    //#region Properties

    self.memberList = ko.observableArray([]);

    self.orderConfirmations = ko.observable().extend({
        email: {
            params: true,
            message: self.resources.FormsErrorEmailFormat
        },
        required: {
            params: true,
            message: self.resources.ManageAccountErrorEmail
        },
        minLength: {
            params: 5,
            message: self.resources.ManageAccountErrorEmail
        }
    });

    self.invoicesStatements = ko.observable().extend({
        email: {
            params: true,
            message: self.resources.FormsErrorEmailFormat
        },
        required: {
            params: true,
            message: self.resources.ManageAccountErrorEmail
        },
        minLength: {
            params: 5,
            message: self.resources.ManageAccountErrorEmail
        }
    });

    self.productNewsEvents = ko.observable().extend({
        email: {
            params: true,
            message: self.resources.FormsErrorEmailFormat
        },
        required: {
            params: true,
            message: self.resources.ManageAccountErrorEmail
        },
        minLength: {
            params: 5,
            message: self.resources.ManageAccountErrorEmail
        }
    });

    self.pricelists = ko.observable().extend({
        email: {
            params: true,
            message: self.resources.FormsErrorEmailFormat
        },
        required: {
            params: true,
            message: self.resources.ManageAccountErrorEmail
        },
        minLength: {
            params: 5,
            message: self.resources.ManageAccountErrorEmail
        }
    });

    self.invoicesStatementsOriginal = ko.observable();

    self.orderConfirmationsOriginal = ko.observable();

    self.productNewsEventsOriginal = ko.observable();

    self.pricelistsOriginal = ko.observable();


    self.orderConfirmationsParams = {
        label: self.resources.ManageAccountTradeEmailOrderConfirmations,
        observable: self.orderConfirmations,
        originalValue: self.orderConfirmationsOriginal,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        charsRemainingOptions: {
            maxLength: 254
        },
        busy: ko.observable(false),
        updated: ko.observable(false),
        comboBoxSettings: {
            widget: ko.observable(),
            value: self.orderConfirmations,
            dataSource: self.memberList,
            dataValueField: 'Email',
            dataTextField: 'Email',
            template: '#: FullName # - #: Email #'
        }
    }

    self.invoicesStatementsParams = {
        label: self.resources.InvoicesStatements,
        observable: self.invoicesStatements,
        originalValue: self.invoicesStatementsOriginal,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        charsRemainingOptions: {
            maxLength: 254
        },
        busy: ko.observable(false),
        updated: ko.observable(false),
        comboBoxSettings: {
            widget: ko.observable(),
            value: self.invoicesStatements,
            dataSource: self.memberList,
            dataValueField: 'Email',
            dataTextField: 'Email',
            template: '#: FullName # - #: Email #'
        }
    }

    self.productNewsEventsParams = {
        label: self.resources.ManageAccountTradeEmailProductNews,
        observable: self.productNewsEvents,
        originalValue: self.productNewsEventsOriginal,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        charsRemainingOptions: {
            maxLength: 254
        },
        busy: ko.observable(false),
        updated: ko.observable(false),
        comboBoxSettings: {
            widget: ko.observable(),
            value: self.productNewsEvents,
            dataSource: self.memberList,
            dataValueField: 'Email',
            dataTextField: 'Email',
            template: '#: FullName # - #: Email #'
        }
    }

    self.pricelistsParams = {
        label: self.resources.ManageAccountTradeEmailPricelists,
        observable: self.pricelists,
        originalValue: self.pricelistsOriginal,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        charsRemainingOptions: {
            maxLength: 254
        },
        busy: ko.observable(false),
        updated: ko.observable(false),
        comboBoxSettings: {
            widget: ko.observable(),
            value: self.pricelists,
            dataSource: self.memberList,
            dataValueField: 'Email',
            dataTextField: 'Email',
            template: '#: FullName # - #: Email #'
        }
    }

    //#endregion


    //#region Components

    self.orderConfirmationErrors = ko.validation.group([
        self.orderConfirmations
    ]);

    self.invoicesStatementsErrors = ko.validation.group([
        self.invoicesStatements
    ]);

    self.productNewsEventsErrors = ko.validation.group([
        self.productNewsEvents
    ]);

    self.pricelistsErrors = ko.validation.group([
        self.pricelists
    ]);

    //#endregion


    //#region Subscriptions



    //#endregion


    //#region Computeds



    //#endregion


    //#region Methods

    self.saveOrderConfirmation = () => {
        self.orderConfirmationErrors.showAllMessages(false);
        //window.setTimeout()
        if (self.orderConfirmationErrors().length > 0) {
            self.orderConfirmationErrors.showAllMessages(true);
            self.orderConfirmationErrors.filter(function (x) {
                x.isValid.notifySubscribers(x.isValid());
            });
            return;
        } else {

            var data = {
                email: self.orderConfirmations()
            };

            self.orderConfirmationsParams.busy(true);

            $.ajax({
                url: '/api/tradeapi/tradeContactDetailsSaveOrderConfirmation',
                type: 'POST',
                dataType: 'json',
                data: data,
                success: function (data) {
                    self.orderConfirmationsParams.busy(false);
                    if (data.error) {
                        switch (data.message) {
                            case 'MemberUpdateFailed':
                                self.orderConfirmations.setError(self.resources.ManageAccountSaveFailed);
                                self.orderConfirmationErrors.showAllMessages(true);
                                $(self.orderConfirmations.DOMElement).select();
                                break;
                            default:
                                window.showError();
                                break;
                        }
                    } else {
                        $(self.orderConfirmations.DOMElement).closest('.member-account-row').addClass('save-successful');
                        self.orderConfirmationsParams.updated(true);
                        self.orderConfirmationsOriginal(self.orderConfirmations());

                        if (window.orderConfirmationsTimeout) {
                            window.clearTimeout(window.orderConfirmationsTimeout);
                        }

                        window.orderConfirmationsTimeout = window.setTimeout(function () {
                            self.orderConfirmationsParams.updated(false);
                            $(self.orderConfirmations.DOMElement).closest('.member-account-row').removeClass('save-successful');
                        }, 5000);
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    self.orderConfirmationsParams.busy(false);
                    window.showError();
                }
            });
        }
    }

    self.saveInvoicesStatements = () => {
        if (self.invoicesStatementsErrors().length > 0) {
            self.invoicesStatementsErrors.showAllMessages(true);
            self.invoicesStatementsErrors.filter(function (x) {
                x.isValid.notifySubscribers(false);
            });
            return;
        } else {

            var data = {
                email: self.invoicesStatements()
            };

            self.invoicesStatementsParams.busy(true);

            $.ajax({
                url: '/api/tradeapi/tradeContactDetailsSaveInvoicesStatements',
                type: 'POST',
                dataType: 'json',
                data: data,
                success: function (data) {
                    self.invoicesStatementsParams.busy(false);
                    // show name save tick and fade it out
                    if (data.error) {
                        switch (data.message) {
                            case 'MemberUpdateFailed':
                                self.invoicesStatements.setError(self.resources.ManageAccountSaveFailed);
                                self.invoicesStatementsErrors.showAllMessages(true);
                                $(self.invoicesStatements.DOMElement).select();
                                break;
                            default:
                                window.showError();
                                break;
                        }
                    } else {
                        $(self.invoicesStatements.DOMElement).closest('.member-account-row').addClass('save-successful');
                        self.invoicesStatementsParams.updated(true);
                        self.invoicesStatementsOriginal(self.invoicesStatements());

                        if (window.invoicesStatementsTimeout) {
                            window.clearTimeout(window.invoicesStatementsTimeout);
                        }

                        window.invoicesStatementsTimeout = window.setTimeout(function () {
                            self.invoicesStatementsParams.updated(false);
                            $(self.invoicesStatements.DOMElement).closest('.member-account-row').removeClass('save-successful');
                        }, 5000);
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    self.invoicesStatementsParams.busy(false);
                    window.showError();
                }
            });
        }
    }

    self.saveProductNewsEvents = () => {
        if (self.productNewsEventsErrors().length > 0) {
            self.productNewsEventsErrors.showAllMessages(true);
            self.productNewsEventsErrors.filter(function (x) {
                x.isValid.notifySubscribers(false);
            });
            return;
        } else {

            var data = {
                email: self.productNewsEvents()
            };

            self.productNewsEventsParams.busy(true);

            $.ajax({
                url: '/api/tradeapi/tradeContactDetailsSaveProductNewsEvents',
                type: 'POST',
                dataType: 'json',
                data: data,
                success: function (data) {
                    self.productNewsEventsParams.busy(false);
                    // show name save tick and fade it out
                    if (data.error) {
                        switch (data.message) {
                            case 'MemberUpdateFailed':
                                self.productNewsEvents.setError(self.resources.ManageAccountSaveFailed);
                                self.productNewsEventsErrors.showAllMessages(true);
                                $(self.productNewsEvents.DOMElement).select();
                                break;
                            default:
                                window.showError();
                                break;
                        }
                    } else {
                        $(self.productNewsEvents.DOMElement).closest('.member-account-row').addClass('save-successful');
                        self.productNewsEventsParams.updated(true);
                        self.productNewsEventsOriginal(self.productNewsEvents());

                        if (window.productNewsEventsTimeout) {
                            window.clearTimeout(window.productNewsEventsTimeout);
                        }

                        window.productNewsEventsTimeout = productNewsEventsTimeoutwindow.setTimeout(function () {
                            self.productNewsEventsParams.updated(false);
                            $(self.productNewsEvents.DOMElement).closest('.member-account-row').removeClass('save-successful');
                        }, 5000);
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    self.productNewsEventsParams.busy(false);
                    window.showError();
                }
            });
        }
    }

    self.savePricelists = () => {
        if (self.pricelistsErrors().length > 0) {
            self.pricelistsErrors.showAllMessages(true);
            self.pricelistsErrors.filter(function (x) {
                x.isValid.notifySubscribers(false);
            });
            return;
        } else {

            var data = {
                email: self.pricelists()
            };

            self.pricelistsParams.busy(true);

            $.ajax({
                url: '/api/tradeapi/tradeContactDetailsSavePricelists',
                type: 'POST',
                dataType: 'json',
                data: data,
                success: function (data) {
                    // show name save tick and fade it out
                    self.pricelistsParams.busy(false);
                    if (data.error) {
                        switch (data.message) {
                            case 'MemberUpdateFailed':
                                self.pricelists.setError(self.resources.ManageAccountSaveFailed);
                                self.pricelistsErrors.showAllMessages(true);
                                $(self.pricelists.DOMElement).select();
                                break;
                            default:
                                window.showError();
                                break;
                        }
                    }  else {
                        $(self.pricelists.DOMElement).closest('.member-account-row').addClass('save-successful');
                        self.pricelistsParams.updated(true);
                        self.pricelistsOriginal(self.pricelists());

                        if (window.priceListSaveTimeout) {
                            window.clearTimeout(window.priceListSaveTimeout);
                        }

                        window.priceListSaveTimeout = window.setTimeout(function () {
                            self.pricelistsParams.updated(false);
                            $(self.pricelists.DOMElement).closest('.member-account-row').removeClass('save-successful');
                        }, 5000);
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    self.pricelistsParams.busy(false);
                    window.showError();
                }
            });
        }
    }

    self.reflowLabels = () => {
        self.invoicesStatements.mdcSelect.label_.float(true);
        self.invoicesStatements.mdcSelect.getDefaultFoundation().adapter_.notchOutline(self.invoicesStatements.mdcSelect.label_.getWidth() * 0.8);

        self.productNewsEvents.mdcSelect.label_.float(true);
        self.productNewsEvents.mdcSelect.getDefaultFoundation().adapter_.notchOutline(self.productNewsEvents.mdcSelect.label_.getWidth() * 0.8);

        self.orderConfirmations.mdcSelect.label_.float(true);
        self.orderConfirmations.mdcSelect.getDefaultFoundation().adapter_.notchOutline(self.orderConfirmations.mdcSelect.label_.getWidth() * 0.8);

        self.pricelists.mdcSelect.label_.float(true);
        self.pricelists.mdcSelect.getDefaultFoundation().adapter_.notchOutline(self.pricelists.mdcSelect.label_.getWidth() * 0.8);
    }

    //#endregion

    return self;
});

window.tradeContactDetailsModel = tradeAccountContactDetailsViewModel(getPrefetchedResources());

ko.cleanNode(document.querySelector('#trade-wrapper'));

ko.applyBindings(window.tradeContactDetailsModel, document.querySelector('#trade-wrapper'));

window.setTimeout(() => {
    window.tradeContactDetailsModel.reflowLabels();
}, 0);