const stockistsViewModel = ((parent) => {
    let self = {
        parent: parent,
        resources: parent.resources
    };

    //#region Properties

    // Are we ready? 
    self.ready = ko.observable(false);

    // Are we busy?
    self.busy = ko.observable(false);

    // If so what do we want to tell the user?
    self.busyMessage = ko.observable(self.resources.PleaseWait);

    // Is the detail modal busy?
    self.detailBusy = ko.observable(false);

    // The detail modal busy message
    self.detailBusyMessage = ko.observable(self.resources.LoadingStockistDetail);

    // The detail modal element
    self.detailModalElement = document.querySelector('#stockist-detail-modal');

    // Are we validating presently?
    self.validateNow = ko.observable(false);

    // Whether or not we have searched at least once
    self.haveSearched = ko.observable(false);

    self.premiumPartner = ko.observable(getPremiumPartner()).extend({
        postable: true
    });

    self.qr = ko.observable(getQr()).extend({
        postable: true
    });

    // The user's search term
    self.searchTerm = ko.observable().extend({
        required: {
            params: true,
            onlyIf: () => {
                return self.validateNow() &&
                    (self.latitude() == null && self.longitude() == null);
            },
            message: self.resources.PleaseProvideSearchTerm
        },
        errorPulse: {}
    });

    // The countryId given by the Country drop-down list
    self.countryCode = ko.observable().extend({
        required: {
            params: true,
            onlyIf: () => {
                return self.validateNow()
            },
            message: self.resources.PleaseSelectCountryBefore
        },
        postable: true,
        errorPulse: {}
    });

    // Featured stockists array
    self.featuredStockists = ko.observableArray([]);

    // Search results array
    self.searchResults = ko.observableArray([]);

    // Array of selected brand codes - tied to the Brand checkboxes
    self.selectedBrands = ko.observableArray(getSelectedBrands()).extend({
        postable: true
    });

    // Array of selected product types - tied to the product type checkboxes
    self.selectedProductTypes = ko.observableArray(getSelectedProductTypes()).extend({
        postable: true
    });

    // Latitude (either from browser geolocation or via Google)
    self.latitude = ko.observable().extend({
        postable: true
    });

    // Latitude (either from browser geolocation or via Google)
    self.longitude = ko.observable().extend({
        postable: true
    });

    // The stockist we're currently inspecting - bringing further details into the detail modal
    self.inspectingStockist = ko.observable();

    self.detailModalElement.addEventListener('hidden.bs.modal', () => {
        self.inspectingStockist(undefined);
    })

    // Do we need to FORCE a geocode lookup?
    self.forceGeocode = ko.observable(false);

    self.premiumPartner = ko.observable(getPremiumPartner()).extend({
        postable: true
    });

    self.qr = ko.observable(getQr()).extend({
        postable: true
    });

    // Error groups
    self.errors = new ko.validation.group([
        self.searchTerm,
        self.countryCode
    ]);

    //#endregion


    //#region Components

    // Google geocoder
    self.geocoder = new google.maps.Geocoder();

    //#endregion


    //#region Subscriptions

    self.searchTerm.subscribe(nv => {
        self.forceGeocode(true);
    });

    self.latitude.subscribe(nv => {
        console.log('lat', nv);
    });

    self.longitude.subscribe(nv => {
        console.log('lng', nv);
    });

    self.ready.subscribe(nv => {
        if (self.latitude() == null || self.longitude() == null)
            self.locationPrompt(false);
    });

    //#endregion


    //#region Computeds

    self.searchResultsDisplay = ko.pureComputed(() => {
        let count = self.searchResults().length;
        if (count == 1) {
            return `(${count} ${self.resources.SearchResult})`;
        } else {
            return `(${count} ${self.resources.SearchResults})`
        }
    }, self);

    self.queryStringComputer = ko.computed(() => {
        let searchTerm = self.searchTerm();

        let brands = self.selectedBrands();

        let productTypes = self.selectedProductTypes();
        
        let countryCode = self.countryCode();

        if (document.querySelector(`#stockist-search-form`)) {

            let e = new FormData(document.querySelector(`#stockist-search-form`));
            let obj = {};
            for (const kvp of e.entries()) {
                if (obj.hasOwnProperty(kvp[0])) {
                    obj[kvp[0]] = [obj[kvp[0]]];
                    obj[kvp[0]].push(kvp[1]);
                } else {
                    obj[kvp[0]] = kvp[1];
                }
            }
            let oqs = new URLSearchParams(window.location.search);
            let qs = new URLSearchParams(obj);
            oqs.forEach((v, k) => {
                if (!qs.has(k)) {
                    qs.append(k, v);
                }
            });
            let url = new URL(window.location);
            url.search = qs;
            window.history.pushState({ isFilterAction: true }, '', url);

            if (self.latitude() && self.longitude())
                self.search();
        }
    }, self).extend({
        rateLimit: {
            timeout: 500,
            method: 'notifyWhenChangesStop'
        }
    });

    //#endregion


    //#region Methods

    self.locationPrompt = (throwModal) => {
        self.validateNow(false);
        navigator.geolocation.getCurrentPosition((position) => {
            let lat = position.coords.latitude;
            let long = position.coords.longitude;

            self.latitude(parseFloat(lat.toFixed(7)));
            self.longitude(parseFloat(long.toFixed(7)));

            self.searchTerm(undefined);
        }, (err) => {
            if (throwModal !== false)
                self.showGeolocationFailure();
        });
    };

    self.search = () => {
        self.errors.showAllMessages(false);
        self.validateNow(true);
        if (self.errors().length) {
            self.errors.showAllMessages(true);
            self.errors.filter(x => {
                x.isValid.notifySubscribers(x.isValid());
            });
        } else {
            self.busy(true);

            if (self.latitude() == null ||
                self.longitude() == null ||
                self.forceGeocode() == true) {
                // need to geocode the search term

                let request = {
                    address: self.searchTerm(),
                    componentRestrictions: {
                        country: self.countryCode()
                    }
                }

                self.geocoder.geocode(request, (results, status) => {

                    //console.trace(results);

                    if (results.length > 0) {
                        if (status == google.maps.GeocoderStatus.OK) {
                            self.latitude(parseFloat(results[0].geometry.location.lat().toFixed(7)));
                            self.longitude(parseFloat(results[0].geometry.location.lng().toFixed(7)));
                        } else {
                            self.busy(false);
                            self.showGeocodingFailure();
                        }
                    } else {
                        self.busy(false);
                        self.showGeocodingFailure();
                    }
                    self.forceGeocode(false);
                });
            } else {
                self.actuallySearch();
                self.forceGeocode(false);
            }
        }
    }

    self.showGeolocationFailure = () => {
        window.showInfo(self.resources.Sorry, self.resources.BrowserCouldNotDetermine);
    }

    self.showGeocodingFailure = () => {
        window.showInfo(self.resources.Sorry, self.resources.GoogleMapsCouldNotDetermine);
    }

    self.actuallySearch = () => {

        // do the search
        fetch('/api/stockist/stockistSearch', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-store',
            credentials: 'same-origin',
            body: JSON.stringify(self.toJS())
        })
            .then(window.handleFetchResponse)
            .then(data => {
                console.log('search returned', data);
                //console.trace(data);
                if (data.error) {
                    switch (data.message) {
                        default:
                            showError();
                            break;
                    }
                } else {
                    self.featuredStockists(data.featuredStockists.map(d => stockistSearchResultViewModel(d)));
                    self.searchResults(data.stockistResults.map(d => stockistSearchResultViewModel(d)));
                    if (window.isMobileSite()) {
                        window.scrollTo(0, document.querySelector('#stockist-results').offsetTop);
                    }
                }
                self.haveSearched(true);
                self.busy(false);
            })
            .catch(err => {
                self.busy(false);
                window.catchFetchErrors(err);
            });
    }

    self.toJS = () => {
        return Object.assign(...Object.keys(self).flatMap(k => {
            let result = {};
            if (self[k].hasOwnProperty('postable')) {
                result[k] = ko.unwrap(self[k]);
                return [result];
            }
            else
                return [];
        }));
    }

    self.createStockistViewModel = (d) => stockistSearchResultViewModel(d);

    self.loadStockistDetail = (stockist) => {
        self.detailBusy(true);
        self.showStockistDetailModal();
        if (stockist.detail()) {
            setTimeout(() => {
                self.inspectingStockist(stockist);
                self.loadStockistMap();
                self.detailBusy(false);
            }, 0);
        } else {
            fetch('/api/stockist/stockistDetail', {
                headers: window.fetchHeaders,
                cache: 'no-store',
                credentials: 'same-origin',
                method: 'POST',
                body: JSON.stringify({
                    stockistId: stockist.StockistID
                })
            })
            .then(window.handleFetchResponse)
            .then(data => {
                stockist.detail(data);
                self.inspectingStockist(stockist);
                self.loadStockistMap();
                self.detailBusy(false);
            })
            .catch(err => {
                self.detailBusy(false);
                self.hideStockistDetailModal();
                window.catchFetchErrors(err);
            })
        }
    }

    self.loadStockistMap = () => {
        var request = self.inspectingStockist();
        var requestedLocation = new google.maps.LatLng(request.detail().Latitude, request.detail().Longitude);

        var myOptions = {
            zoom: 16,
            center: requestedLocation,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        }
        var map = new google.maps.Map(document.getElementById("map-canvas"), myOptions);

        var marker = new google.maps.Marker({
            position: requestedLocation,
            map: map
        });
    }

    self.showStockistDetailModal = () => {
        bootstrap.Modal.getOrCreateInstance(self.detailModalElement).show();
    }

    self.hideStockistDetailModal = () => {
        bootstrap.Modal.getOrCreateInstance(self.detailModalElement).hide();
    }

    //#endregion

    window.sm = self;

    return self;
});

const stockistSearchResultViewModel = ((data) => {
    let self = Object.assign({}, data);

    self.data = ko.observable();

    self.dataSub = self.data.subscribe(nv => {

        Object.assign(self, nv);

        self.dataSub.dispose();

        delete self.dataSub;
    });

    self.detail = ko.observable();

    return self;
});