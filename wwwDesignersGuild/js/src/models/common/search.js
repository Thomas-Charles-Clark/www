const searchBoxViewModel = ((searchTerm, productType) => {
    let self = {

    };

    //#region Properties

    // Are we busy?
    self.busy = ko.observable(false);

    // Do we want to hold busy (because we've submitted the search form and don't want the hints to clear the busy state)
    self.holdBusy = ko.observable(false);

    // Available product types
    self.availableProductTypes = ko.observableArray([]);

    // Selected product type
    self.productType = ko.observable(productType);

    // The searched for term
    self.searchTerm = ko.observable(searchTerm).extend({
        rateLimit: {
            method: 'notifyWhenChangesStop',
            timeout: 300
        }
    });

    // The search form
    self.productSearchForm = document.querySelector('#product-search-form');

    // The search form
    self.productSearchInput = document.querySelector('#product-search-control');

    self.productSearchInput.addEventListener('click', (event) => {
        event.stopPropagation();
    });

    // The main search container wrapper
    self.productSearchContentElement = document.querySelector('#product-search-content');

    // The (hidden) drop down element
    self.dropdownToggleElement = document.querySelector('#predictive-search-hints');

    // The product search form element
    self.productSearchFormElement = document.querySelector('#product-search-form');

    // The logo search container element
    self.logoSearchContainerElement = document.querySelector('#logo-search-container');

    // The drop down ul element
    self.dropdownElement = document.querySelector('#predictive-search-hints-dropdown');

    self.dropdownToggleElement.addEventListener('hide.bs.dropdown', () => {
        self.productSearchFormElement.classList.remove('predicting');
        if (self.logoSearchContainerElement)
            self.logoSearchContainerElement.classList.remove('predicting');
    }, false);

    self.dropdownToggleElement.addEventListener('show.bs.dropdown', () => {
        window.lazyImages();
    }, false);

    // Product Type hints
    self.predictiveSearchProductTypes = ko.observableArray([]);

    // Brand hints
    self.predictiveSearchBrands = ko.observableArray([]);

    // Collection hints
    self.predictiveSearchCollections = ko.observableArray([]);

    // Design hints
    self.predictiveSearchDesigns = ko.observableArray([]);

    // Discontinued hints
    self.predictiveSearchDiscontinueds = ko.observableArray([]);

    // Product hints
    self.predictiveSearchProducts = ko.observableArray([]);

    // Search result hints
    self.predictiveSearchResults = ko.observableArray([]);

    //#endregion


    //#region Components



    //#endregion


    //#region Subscriptions

    // if we're busy, add the busy class to the search form. This holds it open, should the user
    // happen to click away. Necessary to prevent the barberpole effect appearing without the form
    // expanded.
    self.busy.subscribe(nv => {
        console.log('search.js busy subscribe hit', nv);
        if (nv) {
            self.productSearchForm.classList.add('busy');
            self.productSearchContentElement.classList.add('busy');
        } else {
            self.productSearchForm.classList.remove('busy');
            self.productSearchContentElement.classList.remove('busy');
        }
    });

    self.showDropdown = () => {
        self.productSearchFormElement.classList.add('open');
        self.productSearchFormElement.classList.add('predicting');
        if (self.logoSearchContainerElement)
            self.logoSearchContainerElement.classList.add('predicting');
        bootstrap.Dropdown.getOrCreateInstance(self.dropdownToggleElement).show();
        window.lazyImages();
    }

    self.hideDropdown = () => {
        self.productSearchFormElement.classList.remove('open');
        self.productSearchFormElement.classList.remove('predicting');
        if (self.logoSearchContainerElement)
            self.logoSearchContainerElement.classList.remove('predicting');
        bootstrap.Dropdown.getOrCreateInstance(self.dropdownToggleElement).hide();
    }


    self.productType.subscribe(nv => {
        //if (window.hasOwnProperty('pageModel') && window.pageModel.ContentID == 76) {
        //    window.pageModel.fixedPage().productPageGrid.productType(nv);
        //}
        console.log('search.js productType subscribe hit', nv);

        if (nv && window.getContentId() == 76) {
            if (window.elementPercentInViewport(document.querySelector('#search-overlay')) > 0) {
                document.querySelector('#product-search-control').focus();
            }
        }
    });

    // Fetch predictive hints
    self.searchTerm.subscribe(nv => {

        if (!self.holdBusy()) {

            if (nv && nv.length > 3) {

                self.busy(true);

                fetch('/api/search/getPredictiveSearch', {
                    method: 'POST',
                    headers: window.fetchHeaders,
                    cache: 'no-store',
                    credentials: 'same-origin',
                    body: JSON.stringify({
                        productType: self.productType(),
                        searchTerm: self.searchTerm(),
                        siteCountry: getSiteCountry(),
                        countryId: getCountryId(),
                        languageId: getLanguageId()
                    })
                })
                    .then(window.handleFetchResponse)
                    .then(data => {
                        //console.trace(data);

                        if (data.error) {
                            // noop
                        } else {
                            self.predictiveSearchResults(data.predictiveSearchResults.results);
                            self.predictiveSearchProductTypes(data.predictiveSearchHints.productTypes);
                            self.predictiveSearchBrands(data.predictiveSearchHints.brands);
                            self.predictiveSearchCollections(data.predictiveSearchHints.collections);
                            self.predictiveSearchDesigns(data.predictiveSearchHints.designs);
                            self.predictiveSearchDiscontinueds(data.predictiveSearchHints.discontinueds);
                            self.predictiveSearchProducts(data.predictiveSearchHints.products);

                            if (self.anyPredictiveHints()) {
                                self.showDropdown();
                                self.productSearchContentElement.classList.add('open');
                            }
                        }

                        if (!self.holdBusy())
                            self.busy(false);
                    })
                    .catch(err => {
                        if (!self.holdBusy())
                            self.busy(false);
                        window.catchFetchErrors(err);
                    })
            }
        }
    });

    //#endregion


    //#region Computeds

    // These index offsets allow me to provide tabindex to the dropdown's li > a items

    self.brandsIndexOffset = ko.computed(() => {
        return self.predictiveSearchProductTypes().length;
    }, self);

    self.collectionsIndexOffset = ko.computed(() => {
        return self.brandsIndexOffset() +
            self.predictiveSearchBrands().length;
    }, self);

    self.designsIndexOffset = ko.computed(() => {
        return self.collectionsIndexOffset() +
            self.predictiveSearchCollections().length;
    }, self);

    self.resultsIndexOffset = ko.computed(() => {
        return self.designsIndexOffset() +
            self.predictiveSearchDesigns().length;
    }, self);

    //self.discontinuedIndexOffset = ko.computed(() => {
    //    return self.brandsIndexOffset() +
    //        self.predictiveSearchBrands().length;
    //}, self);

    self.productsIndexOffset = ko.computed(() => {
        return self.resultsIndexOffset() +
            self.predictiveSearchResults().length;
    }, self);

    //#endregion


    //#region Methods

    // have we got any predictive hints?
    // more efficient to have this be a simple function
    self.anyPredictiveHints = () => {
        return (self.predictiveSearchResults().length > 0 ||
            self.predictiveSearchProductTypes().length > 0 ||
            self.predictiveSearchBrands().length > 0 ||
            self.predictiveSearchCollections().length > 0 ||
            self.predictiveSearchDesigns().length > 0 ||
            self.predictiveSearchDiscontinueds().length > 0 ||
            self.predictiveSearchProducts().length > 0);
    };

    // clear the results if necessary with this
    self.clearPredictiveResults = () => {
        self.predictiveSearchResults([]);
        self.predictiveSearchProductTypes([]);
        self.predictiveSearchBrands([]);
        self.predictiveSearchCollections([]);
        self.predictiveSearchDesigns([]);
        self.predictiveSearchDiscontinueds([]);
        self.predictiveSearchProducts([]);
    }

    // Guess what this one does...
    self.focusFirstDropdownItem = () => {
        if (self.anyPredictiveHints()) {
            self.showDropdown();
            let firstChild = self.dropdownElement.querySelector('ul:first-of-type li:first-child a');
            if (firstChild)
                firstChild.focus();
        }
    }

    // Appropriately enough, this one uses the result selected by the user to search with.
    self.useResult = (result) => {
        self.formSubmit();
        document.querySelector('#product-search-control').value = result.Keyword;
        self.searchTerm(result.Keyword);
        window.setTimeout(() => {
            self.submit();
        }, 0);
    }

    // potentially submits the form
    self.submit = () => {
        if (self.productType() === '' && !self.searchTerm()) {
            self.searchTerm.DOMElement.classList.remove('shake');
            window.setTimeout(() => {
                self.searchTerm.DOMElement.classList.add('shake');
                window.setTimeout(() => {
                    self.searchTerm.DOMElement.classList.remove('shake');
                }, 300);
            }, 0);
            return false;
        } else {
            document.querySelector('#product-search-form').submit();
        }
    }

    self.formSubmit = (data, event) => {
        if (self.productType() === '' && !self.searchTerm()) {
            self.searchTerm.DOMElement.classList.remove('shake');
            window.setTimeout(() => {
                self.searchTerm.DOMElement.classList.add('shake');
                window.setTimeout(() => {
                    self.searchTerm.DOMElement.classList.remove('shake');
                }, 300);
            }, 0);
            return false;
        } else {
            self.busy(true);
            self.holdBusy(true);
            return true;
        }
    }

    //#endregion


    return self;
});

if (document.querySelector('#product-search-content')) {
    window.searchBoxModel = searchBoxViewModel(
        typeof window.getSearchTerm === 'function' ? window.getSearchTerm() : undefined,
        typeof window.getProductType === 'function' ? window.getProductType() : undefined
    );

    ko.applyBindings(window.searchBoxModel, document.querySelector('#product-search-content'));
    ko.applyBindings(window.searchBoxModel, document.querySelector('#predictive-search-hints-dropdown'));

    let ready = new Event('searchBoxReady');
    document.querySelector('body').dispatchEvent(ready);
}