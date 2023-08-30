const tradeAddressManagementViewModel = ((resources, params) => {
    var self = {
        resources: resources
    };

    self.ready = ko.observable(false);

    self.displayHeading = ko.observable(typeof params.displayHeading == 'undefined' ? true : params.displayHeading);
    self.displayInvoice = ko.observable(typeof params.displayInvoice == 'undefined' ? true : params.displayInvoice);

    if (typeof params != 'undefined' && typeof params.selectFunction === 'function') {
        self.selectFunction = params.selectFunction;
        self.selectPermitted = params.selectPermitted;
    } else {
        self.selectFunction = null;
        self.selectPermitted = false;
    }

    self.invoiceAddressSearch = addressSearchFactory(
        ko.observable({
            CountryID: getCountryId()
        }),
        self.resources.AddressSearch,
        self.resources.SearchYourAddressBook,
        self.resources.MoodboardSave,
        null, // must be set after factory creation, because editModal
        null, // needs its own busy observable
        null, // message probably not required
        function () {
            self.invoiceAddressSearch.busy(true);

            console.log('invoiceAddressChangeRequest submit');

            fetch('/api/tradeapi/saveInvoiceAddressChangeRequest', {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-store',
                credentials: 'same-origin',
                body: JSON.stringify({
                    languageId: getLanguageId(),
                    currentJson: JSON.stringify(self.invoiceAddress.toBasketAddressJS()),
                    currentDisplayBlock: self.invoiceAddress.displayBlock(),
                    requestedJson: JSON.stringify(self.invoiceAddressChange.toBasketAddressJS()),
                    requestedDisplayBlock: self.invoiceAddressChange.displayBlock()
                })
            })
                .then(window.handleFetchResponse)
                .then(data => {
                    self.invoiceAddressSearch.closeEditModal();
                    self.invoiceAddressSearch.pcaLookup.clearFunction();
                    self.requestStored();
                    self.invoiceAddressSearch.busy(false);
                })
                .catch(err => {
                    self.invoiceAddressSearch.busy(false);
                    window.catchFetchErrors(err);
                })
        },
        false,
        true,
        self.resources);

    self.invoiceAddressSearch.mode('pca');

    self.invoiceAddress = addressViewModel({
            labelClass: 'delivery-address-label',
            controlClass: 'form-control mdc-text-field__input',
            phoneIntlMobile: false,
            email: true
        }, 
        self.resources);

    self.invoiceAddressChange = addressViewModel({
            labelClass: 'delivery-address-label',
            controlClass: 'form-control mdc-text-field__input',
            phoneIntlMobile: true,
            email: true,
            invoiceAddress: true
        },
        self.resources);

    self.invoiceAddressSearch.setAddressObject(self.invoiceAddressChange);

    self.invoiceAddress.loadPCAResult(getInvoiceAddress());

    self.invoiceAddressChange.loadPCAResult(getInvoiceAddress());

    self.addresses = ko.observableArray([]);

    self.defaultDeliveryReference = ko.observable(getDefaultDeliveryReference())

    self.addressSearch = addressSearchFactory(
        ko.observable(),
        self.resources.AddressSearch,
        self.resources.SearchYourAddressBook,
        self.resources.MoodboardSave,
        null, // must be set after factory creation, for editModal hooking
        null, // needs its own busy observable
        null, // message probably not required
        function () {
            self.addressSearch.busy(true);
            fetch('/api/tradeapi/saveDeliveryAddress', {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-store',
                credentials: 'same-origin',
                body: JSON.stringify(this.addressObject.toBasketAddressJS())
            })
                .then(window.handleFetchResponse)
                .then(data => {
                    self.addressSearch.closeEditModal();
                    self.addressSearch.pcaLookup.clearFunction();
                    self.addressSearch.addressLookup.clearFunction();
                    self.addressSearch.addressLookup.observable.performSearch();
                    let searchResult = this.addressLookup.searchResultsList().find(sr => sr.deliveryReference() == this.addressObject.deliveryReference());
                    if (searchResult) {
                        searchResult.loadFromOtherAddressObject(this.addressObject);
                    } else {
                        this.addressObject.deliveryReference(data.deliveryReference);
                        self.addressSearch.addressLookup.searchResultsList.unshift(this.addressObject);
                    }
                    self.addressSearch.busy(false);
                })
                .catch(err => {
                    self.addressSearch.busy(false);
                    window.catchFetchErrors(err);
                })
        },
        false,
        false,
        self.resources,
        null,
        () => {
            self.ready(true);
        }
    );

    self.currentAddress = addressViewModel({
            labelClass: 'delivery-address-label',
            controlClass: 'form-control mdc-text-field__input',
            phoneIntlMobile: true,
            email: true
        },
        self.resources);

    self.addressSearch.setAddressObject(self.currentAddress);

    self.addressSearch.mode.subscribe(function (newValue) {
        self.addressSearch.pcaLookup.searchResultsList([]);
        if (newValue === 'extant') {
            if (self.addressSearch.widget()) {
                self.addressSearch.widget().value(null);
                self.addressSearch.widget().trigger('change');
            }
            self.addressSearch.pcaLookup.label(self.resources.SearchYourAddressBook);
            self.addressSearch.pcaLookup.observable.performSearch(); 
        } else {
            self.addressSearch.showEditModal();
            self.addressSearch.pcaLookup.label();
        }
    });

    self.makeDefault = function (item) {
        if (self.defaultDeliveryReference()) {
            window.showConfirm(
                self.resources.AreYouSure,
                //`${self.resources.ThisWillMakeDeliveryReference} <strong>${item.deliveryReference()}</strong> ${self.resources.TheDefaultReplacing} <strong>${self.defaultDeliveryReference()}</strong>`,
                `${self.resources.ThisWillChangeTheDefaultDelivery}`,
                () => {
                    fetch('/api/tradeapi/SetDefaultDeliveryReference', {
                        method: 'POST',
                        headers: window.fetchHeaders,
                        cache: 'no-store',
                        credentials: 'same-origin',
                        body: JSON.stringify({
                            deliveryReference: item.deliveryReference()
                        })
                    })
                        .then(window.handleFetchResponse)
                        .then(data => {
                            self.defaultDeliveryReference(item.deliveryReference());
                            self.addressSearch.pcaLookup.observable.performSearch();
                        })
                        .catch(window.catchFetchErrors)
                },
                null
            )
        }
        else {
            window.showConfirm(
                self.resources.AreYouSure,
                `${self.resources.ThisWillMakeDeliveryReference} <strong>${item.deliveryReference()}</strong> ${self.resources.TheDefaultOnYourAccount}`,
                () => {
                    fetch('/api/tradeapi/SetDefaultDeliveryReference', {
                        method: 'POST',
                        headers: window.fetchHeaders,
                        cache: 'no-store',
                        credentials: 'same-origin',
                        body: JSON.stringify({
                            deliveryReference: item.deliveryReference()
                        })
                    })
                        .then(window.handleFetchResponse)
                        .then(data => {
                            self.defaultDeliveryReference(item.deliveryReference());
                            self.addressSearch.pcaLookup.observable.performSearch();
                        })
                        .catch(window.catchFetchErrors)
                },
                null
            )
        }
    }

    self.delete = function (item) {
        window.showConfirm(
            self.resources.AreYouSure,
            `${self.resources.ThisWillDeleteDeliveryReference} <strong>${item.deliveryReference()}</strong>`,
            () => {
                fetch('/api/tradeapi/deleteDeliveryReference', {
                    method: 'POST',
                    headers: window.fetchHeaders,
                    cache: 'no-store',
                    credentials: 'same-origin',
                    body: JSON.stringify({
                        deliveryReference: item.deliveryReference()
                    })
                })
                    .then(window.handleFetchResponse)
                    .then(data => {
                        self.addressSearch.addressLookup.observable.performSearch();
                    })
                    .catch(window.catchFetchErrors)
            },
            null
        )
    }

    self.requestInvoiceAddressChange = function (item) {
        console.log('requestInvoiceAddressChange hit');
        document.querySelector('#invoice-address-change-requested').style.display = 'none';
        self.invoiceAddressSearch.showEditModal();
    }

    self.requestStored = function () {
        document.querySelector('#invoice-address-change-requested').style.display = 'flex';
    }

    return self;
});

window.tradeAddressModel = tradeAddressManagementViewModel(getPrefetchedResources(), {});
if (document.getElementById("trade-account-contact-details")) {
    ko.applyBindings(window.tradeAddressModel, document.getElementById("trade-account-contact-details"));
}
window.tradeAddressModel.addressSearch.ready.subscribe(nv => {
    window.tradeAddressModel.addressSearch.addressLookup.clearFunction();
});