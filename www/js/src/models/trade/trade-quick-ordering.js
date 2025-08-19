const tradeQuickOrderingViewModel = ((resources) => {
    let self = {
        resources: resources
    };

    //#region Properties

    self.busy = ko.observable(false);

    self.basketBusy = ko.observable(false);

    self.haveSearched = ko.observable(false);

    self.validateBasketNow = ko.observable(false);

    self.productTypeId = ko.observable('');

    self.results = ko.observableArray([]);

    self.searchTerm = ko.observable();

    //#endregion


    //#region Components

    self.stockInfoModel = stockInfoViewModel(self);

    //#endregion


    //#region Subscriptions



    //#endregion


    //#region Computeds




    //#endregion


    //#region Methods

    self.search = function () {
        if (self.searchTerm()) {
            self.busy(true);
            self.results([]);
            fetch('/api/tradeapi/quicksearch', {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-store',
                credentials: 'same-origin',
                body: JSON.stringify({
                    searchTerm: self.searchTerm(),
                    countryId: getCountryId(),
                    languageId: getLanguageId(),
                    productTypeId: self.productTypeId()
                })
            })
                .then(window.handleFetchResponse)
                .then(data => {
                    console.trace(data.products);

                    if (data.hasOwnProperty('products')) {
                        let results = data.products.flatMap(p => {

                            let product = p.product;

                            let products = [];

                            let mainProduct = productViewModel(product.productId, 1);

                            if (product.controls.allowSample) {
                                let sample = productViewModel(product.productId, 1);
                                sample.data(product.sample);
                                sample.orderFormTypeId = 0;
                                sample.quantity(sample.stockMessage.quantityMinimum);
                                products.push(sample);
                            }

                            if (product.controls.allowLargeSample) {
                                let largeSample = productViewModel(product.productId, 1);
                                largeSample.data(product.largeSample);
                                largeSample.orderFormTypeId = 1;
                                largeSample.quantity(largeSample.stockMessage.quantityMinimum);
                                products.push(largeSample);
                            }

                            if (product.controls.allowOrder) {
                                let order = productViewModel(product.productId, 1);
                                order.data(product);
                                order.orderFormTypeId = 2;
                                order.quantity(order.stockMessage.quantityMinimum);
                                products.push(order);
                            }

                            if (product.controls.allowReserve) {
                                let reserve = productViewModel(product.productId, 2);
                                reserve.data(product);
                                reserve.orderFormTypeId = 3;
                                reserve.quantity(reserve.stockMessage.quantityMinimum);
                                products.push(reserve);
                            }

                            if (product.controls.allowReserveWithBatchSample) {
                                let reserveWithBatch = productViewModel(product.productId, 3);
                                reserveWithBatch.data(product);
                                reserveWithBatch.orderFormTypeId = 4;
                                reserveWithBatch.quantity(reserveWithBatch.stockMessage.quantityMinimum);
                                products.push(reserveWithBatch);
                            }

                            mainProduct.data(product);

                            mainProduct.products(products);

                            return mainProduct;

                        });

                        // This to allow the subscriptions fired above to actually fire
                        window.setTimeout(() => {
                            self.results(results);
                            self.busy(false);
                            self.haveSearched(true);
                        }, 0);
                    } else {
                        window.setTimeout(() => {
                            self.results([]);
                            self.busy(false);
                            self.haveSearched(true);
                        }, 0);
                    }
                })
                .catch(err => {
                    self.busy(false);
                    window.catchFetchErrors(err);
                });
        }
    }

    // utility function to permit "with" against the appropriate product entry.
    self.getProductByBasketLineType = (productId, basketLineTypeId) =>
        self.results().filter(p => p.productId == productId && p.basketLineTypeId == basketLineTypeId)[0];


    //#endregion

    return self;
});

window.quickOrderingViewModel = tradeQuickOrderingViewModel(getPrefetchedResources());
ko.applyBindings(window.quickOrderingViewModel, document.querySelector('#trade-wrapper'));