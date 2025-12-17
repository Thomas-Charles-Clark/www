const tradeInvoicestradeInvoicesModel = ((resources) => {
    let self = {
        resources: resources
    };


    //#region Properties

    self.kendoGridPagerMessages = {
        display: self.resources.KendoPagerDisplay,
        empty: self.resources.KendoPagerEmpty,
        page: self.resources.KendoPagerPage,
        of: self.resources.KendoPagerOf,
        itemsPerPage: self.resources.KendoPagerItemsPerPage,
        first: self.resources.KendoPagerFirstPage,
        last: self.resources.KendoPagerLastPage,
        next: self.resources.KendoPagerNextPage,
        previous: self.resources.KendoPagerPreviousPage,
        refresh: self.resources.KendoPagerRefresh,
        morePages: self.resources.KendoPagerMorePages,
    };

    self.viewingStatus = ko.observable('summary');

    self.ready = ko.observable(false);

    self.accountBalanceBusy = ko.observable(false);

    self.outstandingInvoicesBusy = ko.observable(false);

    self.invoiceSearchBusy = ko.observable(false);

    self.recentPaymentsBusy = ko.observable(false);

    self.accountBalance = ko.observable(getAccountBalance());

    self.outstandingInvoices = ko.observableArray(getOutstandingInvoices());

    self.outstandingInvoicesAccountBalance = ko.observable(getOnAccountTotal());

    self.invoiceSearchResults = ko.observableArray([]);

    self.recentPayments = ko.observable(getRecentPayments());

    self.searchTerm = ko.observable();

    self.validateInvoiceSearchNow = ko.observable(false);

    self.orderDetailModalElement = document.getElementById('order-detail-modal');

    self.trackingModalElement = document.getElementById('tracking-modal');

    self.orderDetailsOrderDate = ko.observable();
    self.orderDetailsOrderNumber = ko.observable();
    self.orderDetailsDeliveryInfo = ko.observable();
    self.orderDetailsOrderLines = ko.observableArray([]);

    self.trackingBusy = ko.observable(false);
    self.trackingOrderNumber = ko.observable();
    self.trackingResults = ko.observableArray([]);

    self.dateRange = ko.observable(1);

    self.dateRangeType = ko.observable(2);

    self.dateRanges = [{
        dateRangeId: 12,
        name: self.resources.Last12Months
    }, {
        dateRangeId: 6,
        name: self.resources.Last6Months
    }, {
        dateRangeId: 1,
        name: self.resources.ThisMonth
    }, {
        dateRangeId: -1,
        name: self.resources.Custom
    }];

    self.dateRangeTypes = [{
        dateRangeTypeId: 1,
        name: self.resources.ShipmentDate
    }, {
        dateRangeTypeId: 2,
        name: self.resources.InvoiceDate
    }, {
        dateRangeTypeId: 3,
        name: self.resources.DueDate
    }];

    self.customFromDate = ko.observable();

    self.customToDate = ko.observable();


    self.customFromDate.extend({
        date: {
            params: true,
            message: self.resources.FromMustBeDate
        },
        required: {
            params: true,
            onlyIf: function () {
                return self.dateRange() == -1;
            },
            message: self.resources.PleaseProvideFromDate
        }
    });

    self.customToDate.extend({
        date: {
            params: true,
            message: self.resources.ToMustBeDate
        },
        required: {
            params: true,
            onlyIf: function () {
                return self.dateRange() == -1;
            },
            message: self.resources.PleaseProvideToDate
        }
    });

    self.showAccountBalanceInvoicesResult = ko.observable(false);

    self.accountBalanceDrilldownResult = ko.observableArray([]);

    self.showAccountBalanceOnAccountResult = ko.observable(false);

    self.accountBalanceOnAccountResult = ko.observableArray([]);

    self.accountBalanceGridTitle = ko.observable();

    self.valueDisplay = ko.observable(0); // 0 = base, 1 = currency

    self.panBusy = ko.pureComputed(() => {
        return self.accountBalanceBusy() ||
            self.invoiceSearchBusy() ||
            self.outstandingInvoicesBusy() ||
            self.recentPaymentsBusy() ||
            self.trackingBusy();
    }, self);

    self.invoiceColumnsSummary = [{
        field: 'InvoiceHead',
        title: self.resources.InvoiceNo,
        width: 140,
        template: '<a href="javascript:;" onclick="tradeInvoicesModel.accountBalanceDrilldownGrid.widget().select($(event.srcElement).closest(\'tr\')[0])">#: InvoiceHead #</a>'
    }, {
        field: 'InvoiceDT',
        title: self.resources.InvoiceDate,
        width: 140,
        template: "#: InvoiceDT ? kendo.toString(kendo.parseDate(InvoiceDT, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') : '' #"
    }, {
        field: 'DueDT',
        title: self.resources.DueDate,
        width: 140,
        template: "#: DueDT ? kendo.toString(kendo.parseDate(DueDT, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') : '' #"
    }, {
        field: 'DespatchedDT',
        title: self.resources.ShipmentDate,
        width: 140,
        template: "#: DespatchedDT ? kendo.toString(kendo.parseDate(DespatchedDT, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') : '' #"
    }, {
        field: 'OrderID',
        title: self.resources["OrderNo."],
        width: 140,
        template: '<a href="javascript:;" onclick="tradeInvoicesModel.fetchOrderDetails(#: OrderID #, \'#: InvoiceDT #\')">#: OrderID #</a>'
    }, {
        field: 'CustomerOrderReference',
        title: self.resources.CustOrderRef,
        //width: 160,
        footerTemplate: '<span class="text-right text-bold display-block">' + self.resources.Total + ':</span>'
    }, {
        field: 'OriginalValueInCurrency',
        title: self.resources["Orig.Value"],
        width: 160,
        attributes: {
            'class': 'text-right'
        },
        headerAttributes: {
            'class': 'text-right'
        },
        template: function (dataItem) {
            if (dataItem.OriginalValueInCurrency <= 0) {
                return '<span class="text-danger">' + formatNumber(dataItem.OriginalValueInCurrency, 2) + '</span>'
            } else {
                return formatNumber(dataItem.OriginalValueInCurrency, 2);
            }
        },
        footerTemplate: function () {
            return '<span class="text-right display-block">' + formatNumber(self.accountBalanceDrilldownGridOriginalValueTotal(), 2) + '</span>'
        }
    }, {
        field: 'OutstandingValueInCurrency',
        title: self.resources.OutstandingAmount,
        width: 160,
        attributes: {
            'class': 'text-right'
        },
        headerAttributes: {
            'class': 'text-right'
        },
        template: function (dataItem) {
            if (dataItem.OutstandingValueInCurrency <= 0) {
                return '<span class="text-danger">' + formatNumber(dataItem.OutstandingValueInCurrency, 2) + '</span>'
            } else {
                return formatNumber(dataItem.OutstandingValueInCurrency, 2);
            }
        },
        footerTemplate: function () {
            return '<span class="text-right display-block">' + formatNumber(self.accountBalanceDrilldownGridOustandingAmountTotal(), 2) + '</span>'
        }
    }, {
        field: 'Disputed',
        title: self.resources.Disputed,
        width: 120,
        template: (e) => e.Disputed ? 'Yes' : 'No',
        attributes: {
            'class': 'text-right'
        },
        headerAttributes: {
            'class': 'text-right'
        }
    }];


    self.invoiceColumns = [{
        field: 'InvoiceHead',
        width: 120,
        title: self.resources.InvoiceNo
    }, {
        field: 'InvoiceDT',
        title: self.resources.InvoiceDate,
        width: 140,
        template: "#: kendo.toString(kendo.parseDate(InvoiceDT, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') #"
    }, {
        field: 'DueDT',
        title: self.resources.DueDate,
        width: 140,
        template: "#: DueDT ? kendo.toString(kendo.parseDate(DueDT, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') : '' #"
    }, {
        field: 'DespatchedDT',
        title: self.resources.ShipmentDate,
        width: 140,
        template: "#: DespatchedDT ? kendo.toString(kendo.parseDate(DespatchedDT, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') : '' #"
    }, {
        field: 'OrderID',
        title: self.resources["OrderNo"],
        width: 120,
        template: '<a href="javascript:;" onclick="tradeInvoicesModel.fetchOrderDetails(#: OrderID #, \'#: InvoiceDT #\')">#: OrderID #</a>'
    }, {
        field: 'CustomerOrderReference',
        title: self.resources.CustOrderRef,
        width: 180,
        footerAttributes: {
            'style': 'overflow: visible; white-space: nowrap; text-align: right;'
        },
        footerTemplate: '<span class="text-right text-bold display-block">' + self.resources.Totals + ':</span>\
<span class="text-right text-bold display-block">' + self.resources.OnAccount + ':</span>\
<span class="text-right text-bold display-block">' + self.resources.Outstanding + ':</span>'
    }, {
        field: 'OriginalValueInCurrency',
        title: self.resources.OriginalValue,
        width: 120,
        attributes: {
            'class': 'text-right'
        },
        headerAttributes: {
            'class': 'text-right'
        },
        footerAttributes: {
            'style': 'vertical-align: top'
        },
        template: function (dataItem) {
            if (dataItem.OriginalValueInCurrency <= 0) {
                return '<span class="text-danger">' + formatNumber(dataItem.OriginalValueInCurrency, 2) + '</span>'
            } else {
                return formatNumber(dataItem.OriginalValueInCurrency, 2);
            }
        },
        footerTemplate: function () {
            if (self.outstandingInvoices().length) {
                return '<span class="text-right display-block">' + formatNumber(Enumerable.From(self.outstandingInvoices()).Select(function (x) {
                    return x.OriginalValueInCurrency
                }).Sum(), 2) + '</span>';
            } else {
                return '';
            }
        }
    }, {
        field: 'OutstandingValueInCurrency',
        title: self.resources.OutstandingAmount,
        width: 120,
        attributes: {
            'class': 'text-right'
        },
        headerAttributes: {
            'class': 'text-right'
        },
        template: function (dataItem) {
            if (dataItem.OutstandingValueInCurrency <= 0) {
                return '<span class="text-danger">' + formatNumber(dataItem.OutstandingValueInCurrency, 2) + '</span>'
            } else {
                return formatNumber(dataItem.OutstandingValueInCurrency, 2);
            }
        },
        footerTemplate: function () {
            if (self.outstandingInvoices()) {
                return '<span class="text-right display-block">' + formatNumber(self.outstandingInvoicesOutstandingTotal(), 2) + '</span>\
                        <span class="text-right display-block ' + (self.outstandingInvoicesAccountBalance() <= 0 ? 'text-danger' : '') + '">' + formatNumber(self.outstandingInvoicesAccountBalance(), 2) + '</span>\
                        <span class="text-right display-block">' + formatNumber(self.outstandingInvoicesOutstandingTotal() + self.outstandingInvoicesAccountBalance(), 2) + '</span>';
            } else {
                return '';
            }
        }
    }, {
        field: 'Disputed',
        title: self.resources.Disputed,
        width: 120,
        template: (e) => e.Disputed ? 'Yes' : 'No',
        attributes: {
            'class': 'text-right'
        },
        headerAttributes: {
            'class': 'text-right'
        }
    }];

    //#endregion

    //#region Lifted Methods

    // Because grids using this need it to exist a-priori
    self.exportToExcel = (e) => {
        var sheet = e.workbook.sheets[0];
        var grid = e.sender;
        var data = grid.dataSource.data();
        var gridColumns = grid.columns;
        var columns = gridColumns.map(function (col) {
            return {
                value: col.title ? col.title : col.field,
                autoWidth: true,
                background: "#7a7a7a",
                color: "#fff"
            };
        });

        var rows = [{ cells: columns, type: "header" }];

        for (var i = 0; i < data.length; i++) {
            var rowCells = [];
            for (var j = 0; j < gridColumns.length; j++) {
                var cellValue = data[i][gridColumns[j].field];
                if (typeof cellValue === 'string')
                    cellValue = cellValue.replace(/T00:00:00/g, '');
                rowCells.push({ value: cellValue });
            }
            rows.push({ cells: rowCells, type: "data" });
        }
        sheet.rows = rows;
    }

    //#endregion


    //#region Computeds

    self.fromPreceedsTo = ko.pureComputed(function () {
        if (self.customFromDate() && self.customToDate()) {
            return self.customFromDate().getTime() < self.customToDate().getTime();
        }
    }, self).extend({
        equal: {
            params: true,
            onlyIf: function () {
                return self.validateInvoiceSearchNow() && self.customFromDate() && self.customToDate()
            },
            message: self.resources.FromMustPreceedTo
        }
    });

    self.toFollowsFrom = ko.pureComputed(function () {
        if (self.customFromDate() && self.customToDate()) {
            return self.customFromDate().getTime() < self.customToDate().getTime();
        }
    }, self).extend({
        equal: {
            params: true,
            onlyIf: function () {
                return self.customFromDate() && self.customToDate()
            },
            message: self.resources.ToMustFollowFrom
        }
    });

    self.accountBalanceDrilldownGridOriginalValueTotal = ko.pureComputed(function () {
        return Enumerable.From(self.accountBalanceDrilldownResult()).Select(function (x) {
            return x.OriginalValueInCurrency
        }).Sum();
    }, self);

    self.accountBalanceDrilldownGridOustandingAmountTotal = ko.pureComputed(function () {
        return Enumerable.From(self.accountBalanceDrilldownResult()).Select(function (x) {
            return x.OutstandingValueInCurrency
        }).Sum();
    }, self);

    self.outstandingInvoicesOutstandingTotal = ko.pureComputed(function () {
        return Enumerable.From(self.outstandingInvoices()).Select(function (x) {
            return x.OutstandingValueInCurrency
        }).Sum();
    }, self);

    self.accountBalanceOnAccountOutstandingAmountTotal = ko.computed(function () {
        return Enumerable.From(self.accountBalanceOnAccountResult()).Select(function (x) {
            return x.OutstandingValueInCurrency
        }).Sum();
    }, self);

    self.accountBalanceOnAccountOriginalValueTotal = ko.computed(function () {
        return Enumerable.From(self.accountBalanceOnAccountResult()).Select(function (x) {
            return x.OriginalValueInCurrency
        }).Sum();
    }, self);

    self.recentPaymentsOriginalValueTotal = ko.pureComputed(function () {
        if (self.recentPayments()) {
            return formatNumber(Enumerable.From(self.recentPayments()).Select(function (x) {
                return x.OriginalValueInCurrency
            }).Sum(), 2)
        } else {
            return '';
        }
    }, self)

    //#endregion


    //#region Components

    self.customFromDatePicker = {
        widget: ko.observable(),
        value: self.customFromDate,
        format: 'dd/MM/yyyy',
        clear: function () {
            self.customFromDate(undefined);
            self.customFromDatePicker.widget().value(null);
            self.customFromDatePicker.widget().trigger('change');
        }
    };

    self.customToDatePicker = {
        widget: ko.observable(),
        value: self.customToDate,
        format: 'dd/MM/yyyy',
        clear: function () {
            self.customToDate(undefined);
            self.customToDatePicker.widget().value(null);
            self.customToDatePicker.widget().trigger('change');
        }
    };

    self.accountBalanceDrilldownGrid = {
        widget: ko.observable(),
        data: self.accountBalanceDrilldownResult,
        dataSource: new kendo.data.DataSource({
            schema: {
                model: {
                    id: 'DocumentNumber',
                    fields: {
                        DocumentNumber: {},
                        DocumentDate: { type: 'date' },
                        DueDT: { type: 'date' },
                        OrderNumber: {
                            type: 'string'
                        },
                        CustomerOrderReference: {},
                        OriginalValue: {},
                        OutstandingAmount: {},
                        Disputed: {}
                    }
                }
            },
            pageSize: 20
        }),
        pageable: {
            pageSize: 20,
            pageSizes: [20, 50, 100, 500],
            messages: self.kendoGridPagerMessages
        },
        columns: self.invoiceColumnsSummary,
        sortable: true,
        scrollable: true,
        noRecords: {
            template: self.resources.NoRecordsToDisplay
        },
        selectable: 'row',
        change: function (e) {
            var dataItem = this.dataItem(this.select());
            window.open('/downloads/trade-document?docref=' + dataItem.DocumentKey + '&c=' + getCountryId() + '&l=' + getLanguageId(), '_blank');
        },
        excelExport: self.exportToExcel
    };


    self.accountBalanceOnAccountDrilldownGrid = {
        widget: ko.observable(),
        data: self.accountBalanceOnAccountResult,
        noRecords: {
            template: self.resources.NoRecordsToDisplay
        },
        dataSource: new kendo.data.DataSource({
            sort: {
                field: 'PaymentDate',
                dir: 'asc'
            },
            aggregate: [
                { field: "OriginalValue", aggregate: "sum" },
                { field: "OutstandingAmount", aggregate: "sum" }
            ],
            schema: {
                model: {
                    id: 'DocumentReference',
                    fields: {
                        DocumentReference: {},
                        PaymentDate: { type: 'date' },
                        OriginalValue: {},
                        OutstandingAmount: {}
                    }
                }
            },
            pageSize: 20
        }),
        pageable: {
            pageSize: 20,
            pageSizes: [20, 50, 100, 500],
            messages: self.kendoGridPagerMessages
        },
        dataBound: function (e) {
            $('#on-account-os-amount-total').text(formatNumber(self.accountBalanceOnAccountOutstandingAmountTotal(), 2));

            if (self.accountBalanceOnAccountOutstandingAmountTotal() <= 0) {
                $('#on-account-os-amount-total').addClass('text-danger');
            } else {
                $('#on-account-os-amount-total').removeClass('text-danger');
            }

            $('#on-account-ov-amount-total').text(formatNumber(self.accountBalanceOnAccountOriginalValueTotal(), 2));

            if (self.accountBalanceOnAccountOriginalValueTotal() <= 0) {
                $('#on-account-ov-amount-total').addClass('text-danger');
            } else {
                $('#on-account-ov-amount-total').removeClass('text-danger');
            }
        },
        columns: [{
            field: 'InvoiceHead',
            title: self.resources.PaymentReference
        }, {
            field: 'InvoiceDT',
            title: self.resources.PaymentDate,
            template: "#: PaymentDate ? kendo.toString(kendo.parseDate(InvoiceDT, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') : '' #",
            headerAttributes: {
                'class': 'text-bold display-block'
            },
            footerAttributes: {
                'class': 'text-bold display-block'
            },
            footerTemplate: '<span class="text-right text-bold display-block">' + self.resources.Totals + ':</span>'
        }, {
            field: 'OriginalValueInCurrency',
            title: self.resources.OriginalValue,
            attributes: {
                'class': 'text-right'
            },
            headerAttributes: {
                'class': 'text-right'
            },
            template: function (dataItem) {
                if (dataItem.OriginalValueInCurrency <= 0) {
                    return '<span class="text-danger">' + formatNumber(dataItem.OriginalValueInCurrency, 2) + '</span>'
                } else {
                    return formatNumber(dataItem.OriginalValueInCurrency, 2);
                }
            },
            footerTemplate: '<span id="on-account-ov-amount-total" class="text-right text-bold display-block"></span>'
        }, {
            field: 'OutstandingAmount',
            title: self.resources.OutstandingAmount,
            attributes: {
                'class': 'text-right'
            },
            headerAttributes: {
                'class': 'text-right'
            },
            template: function (dataItem) {
                if (dataItem.OutstandingValueInCurrency <= 0) {
                    return '<span class="text-danger">' + formatNumber(dataItem.OutstandingValueInCurrency, 2) + '</span>'
                } else {
                    return formatNumber(dataItem.OutstandingValueInCurrency, 2);
                }
            },
            footerTemplate: '<span id="on-account-os-amount-total" class="text-right text-bold display-block"></span>'
        }],
        sortable: true,
        scrollable: true,
        selectable: 'row',
        change: function (e) {
            var dataItem = this.dataItem(this.select());
            window.open('/downloads/trade-document?docref=' + dataItem.InvoiceHead + '&c=' + getCountryId() + '&l=' + getLanguageId(), '_blank');
        },
        excelExport: self.exportToExcel
    };

    self.outstandingInvoicesGrid = {
        widget: ko.observable(),
        data: self.outstandingInvoices,
        dataSource: new kendo.data.DataSource({
            sort: {
                field: 'InvoiceDT',
                dir: 'asc'
            },
            schema: {
                model: {
                    id: 'InvoiceHead',
                    fields: {
                        InvoiceHead: {},
                        InvoiceDT: { type: 'date' },
                        DueDT: { type: 'date' },
                        OrderID: {},
                        CustomerOrderReference: {},
                        OriginalValueInCurrency: {},
                        OutstandingValueInCurrency: {},
                        Disputed: {}
                    }
                }
            },
            pageSize: 20
        }),
        pageable: {
            pageSize: 20,
            pageSizes: [20, 50, 100, 500],
            messages: self.kendoGridPagerMessages
        },
        columns: self.invoiceColumns,
        sortable: true,
        scrollable: true,
        noRecords: {
            template: self.resources.NoRecordsToDisplay
        },
        selectable: 'row',
        change: function (e) {
            var dataItem = this.dataItem(this.select());
            window.open('/downloads/trade-document?docref=' + dataItem.DocumentKey + '&c=' + getCountryId() + '&l=' + getLanguageId(), '_blank');
        },
        excelExport: self.exportToExcel
    }

    self.recentPaymentsGrid = {
        widget: ko.observable(),
        data: self.recentPayments,
        dataSource: new kendo.data.DataSource({
            sort: [{
                field: 'PaymentDate',
                dir: 'desc'
            }, {
                field: 'PaymentReference',
                dir: 'desc'
            }],
            schema: {
                model: {
                    id: 'PaymentReference',
                    fields: {
                        PaymentReference: { type: 'string' },
                        PaymentDate: { type: 'date' },
                        OriginalValueInCurrency: { type: 'number' }
                    }
                }
            },
            pageSize: 20
        }),
        pageable: {
            pageSize: 20,
            pageSizes: [20, 50, 100, 500],
            messages: self.kendoGridPagerMessages
        },
        columns: [{
            field: 'PaymentReference',
            title: self.resources.PaymentReference
        }, {
            field: 'PaymentDate',
            title: self.resources.PaymentDate,
            template: "#: PaymentDate ? kendo.toString(kendo.parseDate(PaymentDate, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') : '' #",
            footerTemplate: '<span class="text-bold display-block text-right">' + self.resources.Totals + ':</span>'
        }, {
            field: 'OriginalValueInCurrency',
            title: self.resources.OriginalValue,
            headerAttributes: {
                'class': 'text-right'
            },
            template: function (e) {
                if (e.OriginalValue <= 0) {
                    return '<span class="text-right text-danger display-block">' + formatNumber(e.OriginalValueInCurrency, 2) + '</span>';
                } else {
                    return '<span class="text-right display-block">' + formatNumber(e.OriginalValueInCurrency, 2) + '</span>';
                }
            },
            footerTemplate: '<span class="text-right display-block text-bold">' + self.recentPaymentsOriginalValueTotal() + '</span>'
        }],
        sortable: true,
        scrollable: true,
        noRecords: {
            template: self.resources.NoRecordsToDisplay
        },
        excelExport: self.exportToExcel
    }

    self.invoiceSearchGrid = {
        widget: ko.observable(),
        data: self.invoiceSearchResults,
        dataSource: new kendo.data.DataSource({
            sort: [{
                field: 'InvoiceDT',
                dir: 'asc'
            }],
            schema: {
                model: {
                    id: 'InvoiceHead',
                    fields: {
                        InvoiceHead: { type: 'string', editable: false },
                        InvoiceDT: { type: 'date', editable: false },
                        DueDT: { type: 'date', editable: false },
                        OrderID: { type: 'string', editable: false },
                        CustomerOrderReference: { type: 'string', editable: false },
                        OriginalValueInCurrency: { type: 'number', editable: false },
                        OutstandingValueInCurrency: { type: 'number', editable: false },
                        Disputed: { type: 'boolean', editable: false  }
                    }
                }
            },
            pageSize: 20
        }),
        pageable: {
            pageSize: 20,
            pageSizes: [20, 50, 100, 500],
            messages: self.kendoGridPagerMessages
        },
        columns: [{
            field: 'InvoiceHead',
            title: self.resources.InvoiceNo,
            width: 120
        }, {
            field: 'InvoiceDT',
            title: self.resources.InvoiceDate,
            width: 140,
            template: "#: InvoiceDT ? kendo.toString(kendo.parseDate(InvoiceDT, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') : '' #"
        }, {
            field: 'DueDT',
            title: self.resources.DueDate,
            width: 140,
            template: "#: DueDT ? kendo.toString(kendo.parseDate(DueDT, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') : '' #"
        }, {
            field: 'OrderID',
            title: self.resources["OrderNo"],
            width: 120,
            template: '<a href="javascript:;" onclick="tradeInvoicesModel.fetchOrderDetails(#: OrderID #, \'#: InvoiceDT #\')">#: OrderID #</a>'
        }, {
            field: 'CustomerOrderReference',
            title: self.resources.CustOrderRef,
            width: 180,
            footerTemplate: '<span class="text-bold display-block text-right">' + self.resources.Totals + ':</span>'
        }, {
            field: 'OriginalValueInCurrency',
            title: self.resources["Orig.Value"],
            width: 120,
            attributes: {
                'class': 'text-right'
            },
            headerAttributes: {
                'class': 'text-right'
            },
            template: '#: formatNumber(OriginalValueInCurrency, 2) #',
            footerAttributes: {
                'class': 'text-bold'
            },
            footerTemplate: function () {
                if (self.invoiceSearchResults()) {
                    return '<span class="text-right text-bold display-block">' + formatNumber(Enumerable.From(self.invoiceSearchResults()).Select(function (x) {
                        return x.OriginalValueInCurrency
                    }).Sum(), 2) + '</span>';
                } else {
                    return '';
                }
            }
        }, {
            field: 'OutstandingValueInCurrency',
            title: self.resources.OutstandingAmount,
            width: 120,
            attributes: {
                'class': 'text-right'
            },
            headerAttributes: {
                'class': 'text-right'
            },
            template: '#: formatNumber(OutstandingValueInCurrency, 2) #',
            footerTemplate: function () {
                if (self.invoiceSearchResults()) {
                    return '<span class="text-right text-bold display-block">' + formatNumber(Enumerable.From(self.invoiceSearchResults()).Select(function (x) {
                        return x.OutstandingValueInCurrency
                    }).Sum(), 2) + '</span>';
                } else {
                    return '';
                }
            }
        }, {
            field: 'Disputed',
            title: self.resources.Disputed,
            width: 120,
            template: (e) => e.Disputed ? 'Yes' : 'No',
            attributes: {
                'class': 'text-right'
            },
            headerAttributes: {
                'class': 'text-right'
            }
        }],
        sortable: true,
        scrollable: true,
        noRecords: {
            template: self.resources.NoRecordsToDisplay
        },
        selectable: 'row',
        change: function (e) {
            var dataItem = this.dataItem(this.select());
            //window.open('/downloads/trade-document?docref=' + dataItem.DocumentKey + '&c=' + getCountryId() + '&l=' + getLanguageId(), '_blank');
            window.open(`${dataItem.InvoiceURI}`, '_blank');
        },
        excelExport: self.exportToExcel
    };

    self.ordersDetailsGrid = {
        widget: ko.observable(),
        data: self.orderDetailsOrderLines,
        dataSource: new kendo.data.DataSource({
            schema: self.schema,
            serverFiltering: false,
            serverSorting: false,
            serverGrouping: false,
            serverPaging: false,
            pageSize: 20,
            sort: {
                field: 'LineNumber',
                dir: 'asc'
            },
        }),
        pageable: {
            pageSize: 20,
            pageSizes: [20, 50, 100, 500],
            messages: self.kendoGridPagerMessages
        },
        columns: [{
            field: 'Product',
            title: self.resources.Product,
            width: 120
        }, {
            field: 'Batch',
            title: self.resources.Batch,
            width: 120
        }, {
            field: 'Description',
            title: self.resources.Description,
            width: 100,
            attributes: {
                'style': 'min-width: 100px'
            }
        }, {
            field: 'Quantity',
            title: self.resources.Quantity,
            width: 100,
            attributes: {
                'class': 'text-right'
            }
        }, {
            field: 'BatchNumber',
            title: self.resources.BatchNo,
            width: 100
        }, {
            field: 'CustomerOrderReference',
            title: self.resources.YourReference,
            width: 115
        }, {
            field: 'OrderStatus',
            title: self.resources.Status,
            width: 90,
            //template: "#: DespatchStatusDate ? kendo.toString(kendo.parseDate(DespatchStatusDate, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') : DespatchStatusDateMessage #",
            attributes: {
                'class': 'text-right'
            }
        }, {
            field: 'DespatchStatus',
            title: self.resources.Despatch,
            width: 100,
            //template: "#: DespatchStatusDate ? kendo.toString(kendo.parseDate(DespatchStatusDate, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') : DespatchStatusDateMessage #",
            attributes: {
                'class': 'text-right'
            }
        }, {
            field: 'Tracking',
            title: self.resources.Track,
            width: 120,
            attributes: {
                'min-width': '100px'
            },
            template: function (dataItem) {
                if (dataItem.Tracking) {
                    return '<a href="javascript:;" onclick="tradeInvoicesModel.checkParcel(\'' + dataItem.Tracking + '\')">' + self.resources.Track + '</a>'
                } else {
                    return ''
                }
            }
        }],
        sortable: true,
        scrollable: true,
        noRecords: {
            template: self.resources.NoRecordsToDisplay
        },
        selectable: 'row',
        change: function (e) {
            var dataItem = this.dataItem(this.select());
            //window.open('/downloads/trade-document?docref=' + encodeURIComponent(dataItem.DocumentNumber) + '&c=' + getCountryId() + '&l=' + getLanguageId(), '_blank');
        },
        dataBound: function (e) {
            let grid = e.sender;
            grid.autoFitColumn(grid.columns);
        },
        excelExport: self.exportToExcel
    };

    //#endregion


    //#region Subscriptions

    self.customDateSubscriber = ko.computed(() => {
        let a = self.customFromDate(),
            b = self.customToDate();

        if (a && b) {
            window.setTimeout(() => {
                self.performInvoiceSearch();
            }, 0);
        }
    }, self).extend({
        rateLimt: {
            timeout: 0,
            method: 'notifyWhenChangesStop'
        }
    });

    self.dateRange.subscribe(function (newValue) {
        if (newValue != -1) {
            self.performInvoiceSearch();
        } else {
            if (!self.customFromDate() && !self.customToDate()) {

                let currentDate = new Date();

                let year = currentDate.getFullYear();
                let month = currentDate.getMonth();
                let day = currentDate.getDate();
                let hours = currentDate.getHours();
                let minutes = currentDate.getMinutes();
                let seconds = currentDate.getSeconds();
                let milliseconds = currentDate.getMilliseconds();

                let adjustedDate = new Date(year, month - 1, day, hours, minutes, seconds, milliseconds);

                self.customToDate(currentDate);
                self.customFromDate(adjustedDate);
            }
        }
    });

    self.dateRangeType.subscribe(function (newValue) {
        if (self.dateRange() != -1) {
            self.performInvoiceSearch();
        }
    });

    //#endregion



    //#region Methods

    // This would normally be lifted to e.g. components, but can't be because
    // fromPreceedsTo and toFollowsFrom don't get hoisted
    self.invoiceSearchErrors = ko.validation.group([
        self.fromPreceedsTo,
        self.toFollowsFrom,
        self.customFromDate,
        self.customToDate,
    ]);

    self.refreshAccountBalance = function () {
        self.accountBalanceBusy(true);
        self.showAccountBalanceInvoicesResult(false);
        self.showAccountBalanceOnAccountResult(false);
        self.accountBalanceDrilldownResult([]);
        self.accountBalanceOnAccountResult([]);
        console.log('calling refreshAccountBalance');
        $.ajax({
            url: '/api/tradeapi/RefreshAccountBalance',
            type: 'POST',
            dataType: 'json',
            data: {},
            success: function (data) {
                console.log('calling refreshAccountReturned');
                self.accountBalanceBusy(false);
                self.accountBalance(data);
            },
            error: function (xhr, s, a) {
                window.location.href = '/l102?redirect=112'
            }
        });
    }

    self.accountBalanceDrilldown = (item, title, sort) => {
        self.showAccountBalanceInvoicesResult(false);
        self.showAccountBalanceOnAccountResult(false);
        self.accountBalanceDrilldownResult([]);
        self.accountBalanceOnAccountResult([]);
        self.accountBalanceBusy(true);
        /*
        
		public int TradeAccountID { get; set; }
		public short CountryID { get; set; }
		public byte LanguageID { get; set; }
		public string DateQualifier { get; set; }
		public DateTime? DateRangeStart { get; set; }
		public DateTime? DateRangeEnd { get; set; }
		public string SearchTerm { get; set; }

        */
        let data = {
            tradeAccountId: 0,
            countryId: getCountryId(),
            languageId: getLanguageId(),
            dateQualifier: 'DocumentDate',
            dateRangeStart: item.RangeStartDate ? new Date(Date.parse(item.RangeStartDate)) : null,
            dateRangeEnd: item.RangeEndDate ? new Date(Date.parse(item.RangeEndDate)) : null,
            searchTerm: null,
            outstanding: item.OnAccount ?? false
        };
        console.log('calling accountBalanceDrilldown with', data);
        fetch('/api/tradeapi/AccountBalanceDrilldown', {
            method: 'POST',
            headers: window.fetchHeaders,
            credentials: 'same-origin',
            cache: 'no-cache',
            body: JSON.stringify(data)
        })
            .then(window.handleFetchResponse)
            .then((data) => {
                console.log('accountBalanceDrilldown returned', data);
                self.accountBalanceBusy(false);
                self.accountBalanceGridTitle(title);
                if (data.hasOwnProperty('Records')) {
                    self.showAccountBalanceOnAccountResult(false);
                    self.showAccountBalanceInvoicesResult(true);
                    self.accountBalanceDrilldownResult(data.Records);
                    window.setTimeout(function () {
                        self.accountBalanceDrilldownGrid.widget().dataSource.query({
                            page: 1,
                            pageSize: self.accountBalanceDrilldownGrid.widget().dataSource.pageSize(),
                            sort: {
                                field: sort,
                                dir: 'asc'
                            }
                        });
                    }, 0);
                } else {
                    self.showAccountBalanceInvoicesResult(false);
                    self.showAccountBalanceOnAccountResult(true);
                    self.accountBalanceOnAccountResult(data.PaymentsOnAccount);
                }
            }).catch(err => {
                self.busy(false);
                window.catchFetchError(err);
                window.location.href = '/l102?redirect=112'
            });
    }

    self.refreshRecentPayments = () => {
        self.recentPaymentsBusy(true);
        $.ajax({
            url: '/api/tradeapi/refreshRecentPayments',
            type: 'POST',
            dataType: 'json',
            data: {},
            success: function (data) {
                self.recentPaymentsBusy(false);
                self.recentPayments(data.PaymentsOnAccount);
            },
            error: function (xhr, s, a) {
                window.location.href = '/l102?redirect=112'
            }
        });
    }

    self.refreshOutstandingInvoices = () => {
        self.outstandingInvoicesBusy(true);

        var data = {
            dateQualifier: 'InvoiceDate',
            countryId: getCountryId(),
            languageId: getLanguageId(),
            dateRangeStart: null,
            dateRangeEnd: null,
            searchTerm: null,
            outstanding: true
        }

        console.log('calling refreshOutstandingInvoices with', data);

        fetch('/api/tradeapi/RefreshOutstandingInvoices', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-cache',
            credentials: 'same-origin',
            body: JSON.stringify(data)
        })
            .then(window.handleFetchResponse)
            .then(data => {
                console.log('refreshOutstandingInvoices returned', data);
                self.outstandingInvoicesBusy(false);
                self.outstandingInvoices(data.Records);
                self.outstandingInvoicesAccountBalance(data.OnAccount.Value)
                window.setTimeout(function () {
                    self.outstandingInvoicesGrid.widget().dataSource.query({
                        page: 1,
                        pageSize: self.outstandingInvoicesGrid.widget().dataSource.pageSize(),
                        sort: {
                            field: 'DocumentDate',
                            dir: 'asc'
                        }
                    });
                }, 0);
            })
            .catch(err => {
                self.busy(false);
                window.catchFetchError(err);
            })
    }

    self.performInvoiceSearch = () => {
        self.invoiceSearchErrors.showAllMessages(false);
        self.validateInvoiceSearchNow(true);
        if (self.invoiceSearchErrors().length) {
            self.invoiceSearchErrors.showAllMessages(true);
        } else {
            self.invoiceSearchBusy(true);

            var data = {
                dateQualifier: self.dateRangeTypes.find(d => d.dateRangeTypeId == self.dateRangeType()).name.replace(/\s/gm, ''),
                dateRange: self.dateRange(),
                dateRangeType: self.dateRangeType(),
                searchTerm: self.searchTerm() == '' ? null : self.searchTerm(),
                countryId: getCountryId(),
                languageId: getLanguageId(),
                outstanding: null
            }

            if (self.dateRange() == -1) {
                data.dateRangeStart = self.customFromDate() ? self.customFromDate().toISOString() : null;
                data.dateRangeEnd = self.customToDate() ? self.customToDate().toISOString() : null;
            }
            console.log('calling invoiceSearch with', data);

            fetch('/api/tradeapi/InvoiceSearch', {
                method: 'POST',
                headers: window.fetchHeaders,
                credentials: 'same-origin',
                cache: 'no-cache',
                body: JSON.stringify(data)
            })
                .then(window.handleFetchResponse)
                .then(data => {
                    console.log('invoiceSearch returned', data);
                    self.invoiceSearchResults(data.Records);
                    self.invoiceSearchBusy(false);
                })
                .catch(err => {
                    self.busy(false);
                    window.catchFetchError(err);
                })
        }
    }

    self.showOrderDetailModal = function () {
        bootstrap.Modal.getOrCreateInstance(self.orderDetailModalElement).show();
    }

    self.hideOrderDetailModal = function () {
        bootstrap.Modal.getOrCreateInstance(self.orderDetailModalElement).hide();
    }

    self.fetchOrderDetails = (orderNumber, orderDate) => {

        var affectingBusy = null;

        switch (self.viewingStatus()) {
            case 'outstanding':
                affectingBusy = self.outstandingInvoicesBusy;
                break;
            case 'summary':
                affectingBusy = self.accountBalanceBusy;
                break;
            case 'invoice':
                affectingBusy = self.invoiceSearchBusy;
                break;
            case 'recent':
                affectingBusy = self.recentPaymentsBusy;
                break;
        }

        affectingBusy(true);

        self.showOrderDetailModal();

        $.ajax({
            url: '/api/tradeapi/GetOrderDeliveryInfo',
            type: 'POST',
            dataType: 'json',
            data: {
                orderNumber: orderNumber
            },
            success: function (data) {
                console.log('GetOrderDeliveryInfo returned', data);

                self.orderDetailsOrderDate(orderDate);
                self.orderDetailsOrderNumber(orderNumber);
                self.orderDetailsDeliveryInfo(data.DeliveryInfo);
                self.orderDetailsOrderLines(data.OrderLines);

                affectingBusy(false);
            },
            error: function (xhr, s, a) {
                window.location.href = '/l102?redirect=115'
            }
        });
    }

    self.checkParcel = function (text) {

        // this because the way we pull tracking data changed - it can now potentially
        // directly return the sole tracking URL, but it can still also potentially
        // return an indication there were multiple parcels.
        if (text.indexOf('http') == 0) {
            window.open(text, '_blank');
            return;
        }

        self.trackingBusy(true);

        $.ajax({
            url: '/api/tradeapi/GetParcelCount',
            type: 'POST',
            dataType: 'json',
            data: {
                documentNumber: text
            },
            success: function (data) {
                self.trackingBusy(false);
                if (data.Results.length == 1) {
                    window.open(data.Results[0].TrackingURL, '_blank');
                } else {
                    self.trackingOrderNumber(text.split(',')[1]);
                    self.trackingResults(data.Results);
                    self.showTrackingModal();
                }
            },
            error: function (xhr, s, a) {
                self.trackingBusy(false);
                window.location.href = '/l102?redirect=115'
            }
        });
    }

    self.showTrackingModal = function () {
        bootstrap.Modal.getOrCreateInstance(self.trackingModalElement).show();
    }

    self.hideTrackingModal = function () {
        self.trackingResults([]);
        bootstrap.Modal.getOrCreateInstance(self.trackingModalElement).hide();
    }

    //#endregion


    return self;
});

window.tradeInvoicesModel = tradeInvoicestradeInvoicesModel(getPrefetchedResources());

ko.applyBindings(window.tradeInvoicesModel, document.querySelector('#trade-wrapper'));

window.tradeInvoicesModel.performInvoiceSearch();