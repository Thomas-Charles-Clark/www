const memberAddressManagementViewModel = ((parent) => {
    let self = {
        parent: parent,
        resources: parent.resources
    };

    //#region Properties

    self.defaultAddress = addressViewModel({
        labelClass: 'delivery-address-label',
        controlClass: 'form-control mdc-text-field__input',
        phoneIntlMobile: true,
        email: false,
        defaultCountry: getCountryId()
    }, self.resources);

    let dda = getDefaultDeliveryAddress();

    if (dda != null)
        self.defaultAddress.loadPCAResult(dda);
    
    self.defaultDeliveryReference = ko.observable(getDefaultMemberAddressId());

    //#endregion


    //#region Components

    self.deliveryAddress = addressViewModel({
        labelClass: 'delivery-address-label',
        controlClass: 'form-control mdc-text-field__input',
        phoneIntlMobile: true,
        email: false,
        defaultCountry: getCountryId(),
        phoneValidation: {
            required: {
                params: true
            }
        }
    }, self.resources);

    self.addressSearch = addressSearchFactory(
        ko.observable({
            CountryID: getCountryId()
        }),
        self.resources.AddressSearch,
        self.resources.SearchYourAddressBook,
        self.resources.MoodboardSave,
        null,
        null, // needs its own busy observable
        null, // message probably not required
        (onComplete) => {
            self.addressSearch.busy(true);
            fetch('/api/member/editAddress', {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-store',
                credentials: 'same-origin',
                body: JSON.stringify(self.addressSearch.addressObject.toMemberAddressJs())
            })
            .then(window.handleFetchResponse)
            .then(data => {
                self.addressSearch.addressLookup.clearFunction();
                self.addressSearch.closeEditModal();
                self.addressSearch.busy(false);
                onComplete();
                window.setTimeout(() => {
                    $(self.addressSearch.addressLookup.observable.DOMElement).autocomplete('search');
                }, 0);
            })
            .catch(err => {
                self.addressSearch.busy(false);
            })
        },
        false,
        false,
        self.resources,
        'member');

    self.addressSearch.setAddressObject(self.deliveryAddress);

    //#endregion


    //#region Subscriptions



    //#endregion


    //#region Computeds



    //#endregion


    //#region Methods

    self.makeDefault = (item) => {
        if (self.defaultDeliveryReference()) {
            window.showConfirm(
                self.resources.AreYouSure,
                //`${self.resources.ThisWillMakeDeliveryReference} <strong>${item.deliveryReference()}</strong> ${self.resources.TheDefaultReplacing} <strong>${self.defaultDeliveryReference()}</strong>`,
                `${self.resources.ThisWillChangeTheDefaultDelivery}`,
                () => {
                    fetch('/api/member/makeAddressDefault', {
                        method: 'POST',
                        headers: window.fetchHeaders,
                        cache: 'no-store',
                        credentials: 'same-origin',
                        body: JSON.stringify({
                            memberAddressId: item.deliveryReference(),
                            country: item.country()
                        })
                    })
                        .then(window.handleFetchResponse)
                        .then(data => {
                            self.defaultAddress.loadPCAResult(item);
                            self.defaultDeliveryReference(item.deliveryReference());
                            self.addressSearch.pcaLookup.observable.performSearch();
                        })
                        .catch(window.catchFetchErrors)
                },
                null
            )
        }
        else {
            fetch('/api/member/makeAddressDefault', {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-store',
                credentials: 'same-origin',
                body: JSON.stringify({
                    memberAddressId: item.deliveryReference(),
                    country: item.country()
                })
            })
            .then(window.handleFetchResponse)
            .then(data => {
                self.defaultDeliveryReference(item.deliveryReference());
                self.addressSearch.pcaLookup.observable.performSearch();
            })
            .catch(window.catchFetchErrors)
        }
    }

    self.delete = (item) => {
        window.showConfirm(
            self.resources.AreYouSure,
            `${self.resources.ThisWillDeleteThisDeliveryAddress}`,
            () => {
                fetch('/api/member/deleteAddress', {
                    method: 'POST',
                    headers: window.fetchHeaders,
                    cache: 'no-store',
                    credentials: 'same-origin',
                    body: JSON.stringify({
                        memberAddressId: item.deliveryReference(),
                        country: item.country()
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

    //#endregion

    self.addressSearch.ready.subscribe(nv => {
        self.countrySub = self.addressSearch.addressLookup.country.subscribe(nv => {
            $(self.addressSearch.addressLookup.observable.DOMElement).autocomplete('search');
            self.countrySub.dispose();
        });
    });

    return self;
});
