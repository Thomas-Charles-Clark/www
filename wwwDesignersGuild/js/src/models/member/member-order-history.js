const memberOrderHistoryViewModel = ((parent) => {
    let self = {
        parent: parent,
        resources: parent.resources
    };

    //#region Properties

    self.busy = ko.observable(false);

    self.skipFirstSearchComputation = true;

    self.searchTerm = ko.observable().extend({
        rateLimit: {
            timeout: 500,
            method: 'notifyWhenChangesStop'
        }
    });

    self.orderLines = ko.observableArray(getOrderLines());

    self.trackingInfoLines = ko.observableArray(getTrackingLines());

    self.returnedItems = ko.observableArray(ko.utils.arrayMap(getReturnedItems(), function (r) {
        return returnItem(r);
    }));

    self.orders = ko.observableArray(ko.utils.arrayMap(getOrderHeads(), function (oh) {
        return orderHead(oh, self);
    }));

    self.inspectingOrder = ko.observable();

    self.displayDateRanges = ko.observableArray(getDisplayDateRanges());

    self.selectedDateRange = ko.observable(self.displayDateRanges().find(x => x.NumberOfDays == 365));

    self.displayStatuses = ko.observableArray(getDisplayStatuses());

    self.returnReasons = ko.observableArray(getReturnReasons());

    self.selectedStatus = ko.observable(self.displayStatuses()[0]);

    self.returnModalElement = document.querySelector('#return-modal');

    //#endregion


    //#region Components



    //#endregion


    //#region Subscriptions


    //#endregion


    //#region Computeds



    //#endregion


    //#region Methods

    self.showReturnModal = function (order) {
        self.inspectingOrder(order);
        //$("#tab-menu").tabs({ active: 0 });
        bootstrap.Modal.getOrCreateInstance(self.returnModalElement).show();
    }

    self.hideReturnModal = function () {
        bootstrap.Modal.getOrCreateInstance(self.returnModalElement).hide();
        if (self.inspectingOrder()) {
            self.inspectingOrder().resetReturn();
            self.inspectingOrder(undefined);
        }
    }

    self.displayReturnPdf = function (order) {
        window.open('/pdf/Return?countryCode=' + getCountryCode() + '&returnIdentifier=' + encodeURIComponent(order.ReturnIdentifier), '_blank');
    }

    self.displayReceiptPdf = function (order) {
        window.open('/pdf/Receipt?countryCode=' + getCountryCode() + '&transactionIdentifier=' + encodeURIComponent(order.TransactionIdentifier), '_blank');
    }

    self.fetchOrderHistory = ko.computed(() => {
        let data = {
            countryCode: getCountryCode(),
            searchString: self.searchTerm(),
            historyDays: self.selectedDateRange().NumberOfDays,
            orderStatusId: self.selectedStatus().OrderStatusID
        };
        if (self.skipFirstSearchComputation) {
            self.skipFirstSearchComputation = false;
        } else {
            self.busy(true);
            $.ajax({
                url: "/api/Member/OrderHistoryLoad",
                type: "POST",
                dataType: 'json',
                cache: false,
                async: true,
                data: data,
                success: function (data) {

                    self.hideReturnModal();

                    self.trackingInfoLines(data.OrderLineTracking);

                    self.orderLines(data.OrderLines);

                    self.returnedItems(data.PreviousReturns.map(r => returnItem(r)));

                    self.orders(data.OrderHeads.map(oh => orderHead(oh, self)));

                    self.busy(false);
                },
                error: function (xhr, textStatus, errorThrown) {
                    window.showError();
                    self.busy(false);
                }
            });
        }
    }, self).extend({
        rateLimit: {
            timeout: 50,
            method: 'notifyWhenChangesStop'
        }
    });

    self.saveReturn = () => {
        let o = self.inspectingOrder();

        if (o != null) {
            if (!o.returningAnything()) {
                window.showInfo(self.resources.Sorry, self.resources.orderHistoryNotReturningAnything);
                return;
            }

            o.validateReturnNow(true);

            window.setTimeout(function () {
                if (o.errors().length) {
                    o.errors.showAllMessages(true);
                    o.errors.forEach(x => {
                        x.isValid.notifySubscribers(x.isValid());
                    });
                } else {

                    var detailArray = [];

                    ko.utils.arrayForEach(o.items(), function (i) {
                        if (i.quantityReturning()) {
                            detailArray.push({
                                orderLineId: i.OrderLineID,
                                returnedQuantity: i.quantityReturning(),
                                returnReasonId: i.returnReason().ReturnReasonID
                            });
                        }
                    });

                    var data = {
                        orderHeadId: self.inspectingOrder().OrderHeadID,
                        memberSaveReturnDetails: detailArray
                    }

                    console.log('calling saveReturn with', data);

                    $.ajax({
                        url: "/api/Member/saveReturn",
                        type: 'POST',
                        cache: false,
                        async: false,
                        data: data,
                        dataType: 'json',
                        success: function (data) {
                            if (data.error) {
                                window.showError();
                            } else {
                                self.displayReturnPdf(data);
                                self.searchTerm(undefined);
                                self.selectedDateRange(self.displayDateRanges().find(x => x.NumberOfDays == 365));
                                self.selectedStatus(self.displayStatuses().find(x => x.OrderStatusID == 99));
                                self.fetchOrderHistory();
                                window.showInfo(self.resources.Info, self.resources.orderHistoryReturnGenerated);
                            }
                        },
                        error: function (xhr, textStatus, errorThrown) {
                            console.log(xhr);
                            alert(getResource('majorError'))
                        }
                    });
                }
            }, 0);
        }
    }

    self.reset = function () {
        console.log('orderHistoryViewModel reset called');
        self.inspectingOrder(undefined);
        self.errors.showAllMessages(false);
    }

    //#endregion

    return self;
});

const returnItem = ((record) => {
    let self = Object.assign({}, record);

    //#region Properties



    //#endregion


    //#region Components



    //#endregion


    //#region Subscriptions



    //#endregion


    //#region Computeds



    //#endregion


    //#region Methods



    //#endregion

    return self;
});

const orderHead = ((record, parent) => {
    let self = Object.assign({}, record);

    self.parent = parent;

    self.resources = self.parent.resources;

    if (self.OrderStatusID == 5) {
        self.OrderStatusID = 4;
    }

    //#region Properties

    self.validateReturnNow = ko.observable(false);

    self.displayStatus = getDisplayStatuses().find(x => x.OrderStatusID == self.OrderStatusID).Name;

    self.items = ko.observableArray(self.parent.orderLines().flatMap(ol => ol.OrderHeadID == self.OrderHeadID ? [orderLine(ol, self)] : []));

    //#endregion


    //#region Components

    self.errors = ko.validation.group(
        self.items().flatMap(i => [i.quantityReturning, i.returnReason])
    );

    //#endregion


    //#region Subscriptions



    //#endregion


    //#region Computeds

    self.returnedItems = ko.pureComputed(() => {
        return self.parent.returnedItems().filter(x => x.OrderHeadID == self.OrderHeadID);
    }, self);

    self.returningAnything = ko.pureComputed(() => {
        return self.items().find(i => i.quantityReturning() != null) != null;
    }, self);

    //#endregion


    //#region Methods

    self.resetReturn = function () {
        self.validateReturnNow(false);
        self.items().forEach(i => {
            i.quantityReturning(undefined);
            i.returnReason(undefined);
        });
    }

    //#endregion

    return self;
});

const orderLine = ((record, parent) => {
    let self = Object.assign({}, record);

    self.parent = parent;

    self.resources = self.parent.resources;

    if (self.OrderStatusID == 5) {
        self.OrderStatusID = 4;
    }

    //#region Properties

    self.displayStatus = getDisplayStatuses().find(x => x.OrderStatusID == self.OrderStatusID).Name;

    self.trackingInfo = ko.observable(self.parent.parent.trackingInfoLines().filter(x => {
        return x.OrderHeadID == self.OrderHeadID &&
            x.OrderLineNo == self.OrderLineNo
    }));

    self.returnedQuantity = ko.observable(self.ReturnedQuantity);

    self.quantityReturning = ko.observable();

    self.returnReason = ko.observable().extend({
        required: {
            params: true,
            onlyIf: function () {
                return self.parent.validateReturnNow() && self.quantityReturning()
            },
            message: self.resources.orderHistoryReturnReasonError
        }
    });

    //#endregion


    //#region Components


    //#endregion


    //#region Subscriptions



    //#endregion


    //#region Computeds

    self.unreturnedQuantity = ko.pureComputed(function () {
        return self.Quantity - self.returnedQuantity();
    }, self);

    self.returnable = ko.pureComputed(function () {
        return self.returnedQuantity() < self.Quantity;
    }, self);

    //#endregion

    //#region Late Extenders

    self.quantityReturning.extend({
        numberFormatted: {
            digits: 0,
            nearest: 1,
            currency: {
                FormattingCulture: getCulture()
            }
        },
        min: {
            params: +0,
            onlyIf: function () {
                return self.parent.validateReturnNow()
            },
            message: self.resources.pleaseGreaterThanEqualTo + ' {0}'
        },
        max: {
            params: self.unreturnedQuantity,
            onlyIf: function () {
                return self.parent.validateReturnNow()
            },
            message: self.resources.pleaseGreaterThanEqualTo + ' {0}'
        }
    });

    //#endregion

    //#region Methods

    self.increaseQuantity = () => {
        let q = self.quantityReturning();

        if (q == null)
            q = 0;

        if (q < self.returnable()) {
            let nextValue = ((q + 1) * 100) / 100;
            self.quantityReturning(nextValue);
        }
    }

    self.decreaseQuantity = () => {
        let q = +self.quantityReturning();

        if (q === 1) {
            self.quantityReturning(null);
        } else {
            if (q > 0) {
                let nextValue = ((q - 1) * 100) / 100;
                self.quantityReturning(nextValue);
            }
        }
    }

    //#endregion

    return self;
});