try {
    window.stockInfoViewModel = ((parent) => {
        var self = {
            parent: parent
        };

        self.busy = ko.observable(false);

        self.product = ko.observable();
        self.totalStock = ko.observable();
        self.totalPurchaseOrderStock = ko.observable();
        self.totalSupplierPurchaseOrderStock = ko.observable();
        self.unit = ko.observable();
        self.stockStatus = ko.observable();
        self.inStockData = ko.observable();
        self.dueInData = ko.observable();
        self.supplierDueInData = ko.observable();
        self.supplierData = ko.observable();
        self.supplierStockAvailabilityMessage = ko.observable();

        self.stockModalElement = document.querySelector('#stock-modal');

        self.totalInStock = ko.pureComputed(function () {
            if (self.inStockData()) {
                let subTotal = 0;
                self.inStockData().flatMap(isd => isd.Pieces).forEach(p => {
                    subTotal += +(p.Length.replace(/[^\d.]/, ''));
                });
                return Number(Math.round((subTotal) * 10) / 10).toFixed(2) + self.unit();
            } else {
                return 0
            }
        }, self);

        self.totalDueIn = ko.pureComputed(function () {
            if (self.dueInData()) {
                let subTotal = 0;
                self.dueInData().forEach(p => {
                    subTotal += +(p.Length.replace(/[^\d.]/, ''));
                });
                return Number(Math.round((subTotal) * 10) / 10).toFixed(2) + self.unit();
            } else {
                return 0
            }
        }, self);

        self.totalSupplier = ko.pureComputed(function () {
            if (self.supplierData()) {
                let subTotal = 0;
                self.supplierData().flatMap(sd => sd.Pieces).forEach(p => {
                    subTotal += +(p.Length.replace(/[^\d.]/, ''));
                });
                return Number(Math.round((subTotal) * 10) / 10).toFixed(2) + self.unit();
            } else {
                return 0
            }
        }, self);

        self.totalSupplierDueIn = ko.pureComputed(function () {
            if (self.supplierDueInData()) {
                let subTotal = 0;
                self.supplierDueInData().forEach(p => {
                    subTotal += +(p.Length.replace(/[^\d.]/, ''));
                });
                return Number(Math.round((subTotal) * 10) / 10).toFixed(2) + self.unit();
            } else {
                return 0
            }
        }, self);

        self.supplierHasStock = ko.pureComputed(function () {
            return self.totalSupplier() != '0.00m' && self.totalSupplier() != 0
        }, self);

        self.sufficientSinglePieceAvailable = ko.pureComputed(function () {
            let pieces = self.supplierData().flatMap(sd => [sd.Pieces]);

            return pieces.find(p => {
                return +(p.Length.replace(/[^\d.]/, '')) <= +self.Quantity
            });
        }, self);

        self.showStockInfo = function (product, tab) {

            tab = typeof tab === 'undefined' ? 0 : tab;

            self.busy(true);

            self.product(product);

            if (product.hasOwnProperty('stockMessage')) {
                self.unit(product.stockMessage.unit);
                self.stockStatus(product.statusMessage);
            } else {
                self.unit('');
                self.stockStatus('');
            }

            //$('#stock-modal').off('show.bs.modal');

            //$('#stock-modal').on('show.bs.modal', function () {
            //$('#stock-tab-menu').tabs({ active: +tab });
            //});

            if (self.stockModalElement == null) {
                self.stockModalElement = document.querySelector('#stock-modal');
            }

            if (self.stockModalElement != null) {

                let d = ko.dataFor(self.stockModalElement);

                if (d == null) {
                    ko.cleanNode(self.stockModalElement);

                    ko.applyBindings({
                        stockInfoModel: self
                    }, self.stockModalElement);
                }

                if (typeof bootstrap !== 'undefined') {
                    bootstrap.Modal.getOrCreateInstance(self.stockModalElement).show();
                } else {
                    window.addBootstrapLoadFunction(() => {
                        bootstrap.Modal.getOrCreateInstance(self.stockModalElement).show();
                    });
                }

                self.busy(true);

                if (self.parent.hasOwnProperty('deliveryAddressFactory')) {
                    if (self.product().PermitCutLengthBreakdown || self.product().PermitBatch) {
                        self.parent.deliveryAddressFactory.actingCutLength(self.product());
                    } else {
                        self.parent.deliveryAddressFactory.actingCutLength(undefined);
                    }
                }

                self.supplierStockAvailabilityMessage(undefined);

                fetch('/api/stockist/GetPieceLevelStockInfo', {
                    method: 'POST',
                    headers: window.fetchHeaders,
                    cache: 'no-store',
                    credentials: 'same-origin',
                    body: JSON.stringify({
                        productId: ko.unwrap(self.product().productId) || self.product().ProductID,
                        countryId: getCountryId(),
                        languageId: getLanguageId()
                    })
                }).then(window.handleFetchResponse)
                    .then(data => {
                        console.log('getpiecelevelstockinfo returned', data);

                        if (data != null) {

                            self.totalStock(data.TotalStock);

                            ko.utils.arrayForEach(data.Batches, function (b) {
                                let subTotal = 0;
                                b.Pieces.forEach(p => {
                                    subTotal += Math.round((Number(p.Length) * Number(p.Quantity)) * 10) / 10;
                                });
                                b.totalStock = Number(Math.round(subTotal * 10) / 10).toFixed(2);
                            });

                            self.inStockData(data.Batches);

                            self.supplierData(data.SupplierStock);

                            self.dueInData(data.PurchaseOrders);

                            self.supplierDueInData(data.SupplierPurchaseOrders);

                            self.totalPurchaseOrderStock(data.TotalPurchaseOrderLength);

                            self.totalSupplierPurchaseOrderStock(data.TotalSupplierPurchaseOrderLength);

                            self.supplierStockAvailabilityMessage(data.SupplierAvailabilityMessage);

                            if (tab == 2 && !self.supplierHasStock()) {
                                let el = document.querySelector('#stock-tab-menu li:first-child button');
                                let bsTab = bootstrap.Tab.getOrCreateInstance(el);
                                bsTab.show();
                                //$('#stock-tab-menu').tabs({ active: 0 });
                            } else {
                                let el = document.querySelector('#stock-tab-menu li:nth-child(' + (tab + 1) + ') button');
                                let bsTab = bootstrap.Tab.getOrCreateInstance(el);
                                bsTab.show();
                                //$('#stock-tab-menu').tabs({ active: +tab });
                            }

                            //$('#stock-tab-menu').tabs("refresh");
                            //$('#stock-modal').modal('handleUpdate');
                        }

                        if (self.parent.hasOwnProperty('deliveryAddressFactory')) {
                            if (self.product().hasOwnProperty('cutLengthBreakdown')) {
                                self.product().cutLengthBreakdown.selectedBatch(undefined);
                                self.product().cutLengthBreakdown.availableBatches(self.inStockData().map(x => {
                                    return {
                                        batch: x.BatchNumber,
                                        text: x.BatchNumber
                                    }
                                }));
                            }
                        }

                        self.busy(false);
                    })
                    .catch(err => {
                        self.busy(false);
                        window.catchFetchErrors(err);
                    });
            } else {
                console.error('#stock-modal was not found!');
            }
        }

        self.init = () => {
            const search = window.location.search;
            const params = new URLSearchParams(search);
            if (params.has('stock')) {
                const productCode = params.get('stock');
                self.parent.data.subscribe(nv => {
                    window.setTimeout(() => {
                        const product = self.parent.AllProducts.find(p => p.ProductCode == productCode);
                        if (product)
                            self.showStockInfo(product.Product);
                    }, 0);
                });
            }
        }

        self.init();

        return self;
    });
} catch (e) {

}