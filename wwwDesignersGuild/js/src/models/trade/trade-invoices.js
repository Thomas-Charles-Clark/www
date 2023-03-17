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

    self.accountSummaryBusy = ko.observable(false);

    self.outstandingInvoicesBusy = ko.observable(false);

    self.invoiceSearchBusy = ko.observable(false);

    self.recentPaymentsBusy = ko.observable(false);

    self.accountSummary = ko.observable(getAccountSummary());

    self.outstandingInvoices = ko.observableArray(getOutstandingInvoices());

    self.outstandingInvoicesAccountBalance = ko.observable(getAccountBalance());

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
                return self.validateInvoiceSearchNow() && self.dateRange() == -1 && self.customToDate() == null
            },
            message: self.resources.PleaseProvideFromToOrBoth
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
                return self.validateInvoiceSearchNow() && self.dateRange() == -1 && self.customFromDate() == null
            },
            message: self.resources.PleaseProvideFromToOrBoth
        }
    });

    self.showAccountSummaryInvoicesResult = ko.observable(false);

    self.accountSummaryDrilldownResult = ko.observableArray([]);

    self.showAccountSummaryOnAccountResult = ko.observable(false);

    self.accountSummaryOnAccountResult = ko.observableArray([]);

    self.accountSummaryGridTitle = ko.observable();

    self.valueDisplay = ko.observable(0); // 0 = base, 1 = currency

    self.panBusy = ko.pureComputed(() => {
        return self.accountSummaryBusy() ||
            self.invoiceSearchBusy() ||
            self.outstandingInvoicesBusy() ||
            self.recentPaymentsBusy() ||
            self.trackingBusy();
    }, self);

    self.invoiceColumnsSummary = [{
        field: 'DocumentNumber',
        title: self.resources.InvoiceNo,
        width: 120,
        template: '<a href="javascript:;" onclick="tradeInvoicesModel.accountSummaryDrilldownGrid.widget().select($(event.srcElement).closest(\'tr\')[0])">#: DocumentNumber #</a>'
    }, {
        field: 'DocumentDate',
        title: self.resources.InvoiceDate,
        width: 140,
        template: "#: DocumentDate ? kendo.toString(kendo.parseDate(DocumentDate, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') : '' #"
    }, {
        field: 'DueDate',
        title: self.resources.DueDate,
        width: 140,
        template: "#: DueDate ? kendo.toString(kendo.parseDate(DueDate, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') : '' #"
    }, {
        field: 'OrderNumber',
        title: self.resources["OrderNo."],
        width: 120,
        template: '<a href="javascript:;" onclick="tradeInvoicesModel.fetchOrderDetails(#: OrderNumber #, \'#: DocumentDate #\')">#: OrderNumber #</a>'
    }, {
        field: 'CustomerOrderReference',
        title: self.resources.CustOrderRef,
        width: 180,
        footerTemplate: '<span class="text-right text-bold display-block">' + self.resources.Total + ':</span>'
    }, {
        field: 'OriginalValue',
        title: self.resources["Orig.Value"],
        width: 120,
        attributes: {
            'class': 'text-right'
        },
        headerAttributes: {
            'class': 'text-right'
        },
        template: function (dataItem) {
            if (dataItem.OriginalValue <= 0) {
                return '<span class="text-danger">' + formatNumber(dataItem.OriginalValue, 2) + '</span>'
            } else {
                return formatNumber(dataItem.OriginalValue, 2);
            }
        },
        footerTemplate: function () {
            return '<span class="text-right display-block">' + formatNumber(self.accountSummaryDrilldownGridOriginalValueTotal(), 2) + '</span>'
        }
    }, {
        field: 'OutstandingAmount',
        title: self.resources.OutstandingAmount,
        width: 120,
        attributes: {
            'class': 'text-right'
        },
        headerAttributes: {
            'class': 'text-right'
        },
        template: function (dataItem) {
            if (dataItem.OutstandingAmount <= 0) {
                return '<span class="text-danger">' + formatNumber(dataItem.OutstandingAmount, 2) + '</span>'
            } else {
                return formatNumber(dataItem.OutstandingAmount, 2);
            }
        },
        footerTemplate: function () {
            return '<span class="text-right display-block">' + formatNumber(self.accountSummaryDrilldownGridOustandingAmountTotal(), 2) + '</span>'
        }
    }, {
        field: 'Disputed',
        title: self.resources.Disputed,
        width: 120,
        attributes: {
            'class': 'text-right'
        },
        headerAttributes: {
            'class': 'text-right'
        }
    }];


    self.invoiceColumns = [{
        field: 'DocumentNumber',
        width: 120,
        title: self.resources.InvoiceNo
    }, {
        field: 'DocumentDate',
        title: self.resources.InvoiceDate,
        width: 140,
        template: "#: kendo.toString(kendo.parseDate(DocumentDate, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') #"
    }, {
        field: 'DueDate',
        title: self.resources.DueDate,
        width: 140,
        template: "#: DueDate ? kendo.toString(kendo.parseDate(DueDate, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') : '' #"
    }, {
        field: 'OrderNumber',
        title: self.resources["OrderNo."],
        width: 120,
        template: '<a href="javascript:;" onclick="tradeInvoicesModel.fetchOrderDetails(#: OrderNumber #, \'#: DocumentDate #\')">#: OrderNumber #</a>'
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
        field: 'OriginalValue',
        title: self.resources.OrigValue,
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
            if (dataItem.OriginalValue <= 0) {
                return '<span class="text-danger">' + formatNumber(dataItem.OriginalValue, 2) + '</span>'
            } else {
                return formatNumber(dataItem.OriginalValue, 2);
            }
        },
        footerTemplate: function () {
            if (self.outstandingInvoices()) {
                return '<span class="text-right display-block">' + formatNumber(Enumerable.From(self.outstandingInvoices()).Select(function (x) {
                    return x.OriginalValue
                }).Sum(), 2) + '</span>';
            } else {
                return '';
            }
        }
    }, {
        field: 'OutstandingAmount',
        title: self.resources.OutstandingAmount,
        width: 120,
        attributes: {
            'class': 'text-right'
        },
        headerAttributes: {
            'class': 'text-right'
        },
        template: function (dataItem) {
            if (dataItem.OutstandingAmount <= 0) {
                return '<span class="text-danger">' + formatNumber(dataItem.OutstandingAmount, 2) + '</span>'
            } else {
                return formatNumber(dataItem.OutstandingAmount, 2);
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
            return self.customFromDate().getTime() <= self.customToDate().getTime();
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

    self.accountSummaryDrilldownGridOriginalValueTotal = ko.pureComputed(function () {
        return Enumerable.From(self.accountSummaryDrilldownResult()).Select(function (x) {
            return x.OriginalValue
        }).Sum();
    }, self);

    self.accountSummaryDrilldownGridOustandingAmountTotal = ko.pureComputed(function () {
        return Enumerable.From(self.accountSummaryDrilldownResult()).Select(function (x) {
            return x.OriginalValue
        }).Sum();
    }, self);

    self.outstandingInvoicesOutstandingTotal = ko.pureComputed(function () {
        return Enumerable.From(self.outstandingInvoices()).Select(function (x) {
            return x.OutstandingAmount
        }).Sum();
    }, self);

    self.accountSummaryOnAccountOutstandingAmountTotal = ko.computed(function () {
        return Enumerable.From(self.accountSummaryOnAccountResult()).Select(function (x) {
            return x.OutstandingAmount
        }).Sum();
    }, self);

    self.accountSummaryOnAccountOriginalValueTotal = ko.computed(function () {
        return Enumerable.From(self.accountSummaryOnAccountResult()).Select(function (x) {
            return x.OriginalValue
        }).Sum();
    }, self);

    self.recentPaymentsOriginalValueTotal = ko.pureComputed(function () {
        if (self.recentPayments()) {
            return formatNumber(Enumerable.From(self.recentPayments()).Select(function (x) {
                return x.OriginalValue
            }).Sum(), 2)
        } else {
            return '';
        }
    }, self)

    //#endregion


    //#region Components

    self.invoiceSearchErrors = ko.validation.group([
        self.customFromDate,
        self.customToDate,
        self.fromPreceedsTo
    ]);

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

    self.accountSummaryDrilldownGrid = {
        widget: ko.observable(),
        data: self.accountSummaryDrilldownResult,
        dataSource: new kendo.data.DataSource({
            schema: {
                model: {
                    id: 'DocumentNumber',
                    fields: {
                        DocumentNumber: {},
                        DocumentDate: { type: 'date' },
                        DueDate: { type: 'date' },
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


    self.accountSummaryOnAccountDrilldownGrid = {
        widget: ko.observable(),
        data: self.accountSummaryOnAccountResult,
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
            $('#on-account-os-amount-total').text(formatNumber(self.accountSummaryOnAccountOutstandingAmountTotal(), 2));

            if (self.accountSummaryOnAccountOutstandingAmountTotal() <= 0) {
                $('#on-account-os-amount-total').addClass('text-danger');
            } else {
                $('#on-account-os-amount-total').removeClass('text-danger');
            }

            $('#on-account-ov-amount-total').text(formatNumber(self.accountSummaryOnAccountOriginalValueTotal(), 2));

            if (self.accountSummaryOnAccountOriginalValueTotal() <= 0) {
                $('#on-account-ov-amount-total').addClass('text-danger');
            } else {
                $('#on-account-ov-amount-total').removeClass('text-danger');
            }
        },
        columns: [{
            field: 'DocumentReference',
            title: self.resources.PaymentReference
        }, {
            field: 'PaymentDate',
            title: self.resources.PaymentDate,
            template: "#: PaymentDate ? kendo.toString(kendo.parseDate(PaymentDate, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') : '' #",
            headerAttributes: {
                'class': 'text-bold display-block'
            },
            footerAttributes: {
                'class': 'text-bold display-block'
            },
            footerTemplate: '<span class="text-right text-bold display-block">' + self.resources.Totals + ':</span>'
        }, {
            field: 'OriginalValue',
            title: self.resources.OriginalValue,
            attributes: {
                'class': 'text-right'
            },
            headerAttributes: {
                'class': 'text-right'
            },
            template: function (dataItem) {
                if (dataItem.OriginalValue <= 0) {
                    return '<span class="text-danger">' + formatNumber(dataItem.OriginalValue, 2) + '</span>'
                } else {
                    return formatNumber(dataItem.OriginalValue, 2);
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
                if (dataItem.OutstandingAmount <= 0) {
                    return '<span class="text-danger">' + formatNumber(dataItem.OutstandingAmount, 2) + '</span>'
                } else {
                    return formatNumber(dataItem.OutstandingAmount, 2);
                }
            },
            footerTemplate: '<span id="on-account-os-amount-total" class="text-right text-bold display-block"></span>'
        }],
        sortable: true,
        scrollable: true,
        selectable: 'row',
        change: function (e) {
            var dataItem = this.dataItem(this.select());
            window.open('/downloads/trade-document?docref=' + dataItem.DocumentKey + '&c=' + getCountryId() + '&l=' + getLanguageId(), '_blank');
        },
        excelExport: self.exportToExcel
    };

    self.outstandingInvoicesGrid = {
        widget: ko.observable(),
        data: self.outstandingInvoices,
        dataSource: new kendo.data.DataSource({
            sort: {
                field: 'DocumentDate',
                dir: 'asc'
            },
            schema: {
                model: {
                    id: 'DocumentNumber',
                    fields: {
                        DocumentNumber: {},
                        DocumentDate: { type: 'date' },
                        DueDate: { type: 'date' },
                        OrderNumber: {},
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
                field: 'DocumentReference',
                dir: 'desc'
            }],
            schema: {
                model: {
                    id: 'DocumentReference',
                    fields: {
                        DocumentReference: {},
                        PaymentDate: { type: 'date' },
                        OriginalValue: {}
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
            field: 'DocumentReference',
            title: self.resources.PaymentReference
        }, {
            field: 'PaymentDate',
            title: self.resources.PaymentDate,
            template: "#: PaymentDate ? kendo.toString(kendo.parseDate(PaymentDate, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') : '' #",
            footerTemplate: '<span class="text-bold display-block text-right">' + self.resources.Totals + ':</span>'
        }, {
            field: 'OriginalValue',
            title: self.resources.OriginalValue,
            headerAttributes: {
                'class': 'text-right'
            },
            template: function (e) {
                if (e.OriginalValue <= 0) {
                    return '<span class="text-right text-danger display-block">' + formatNumber(e.OriginalValue, 2) + '</span>';
                } else {
                    return '<span class="text-right display-block">' + formatNumber(e.OriginalValue, 2) + '</span>';
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
                field: 'DocumentDate',
                dir: 'asc'
            }],
            schema: {
                model: {
                    id: 'DocumentNumber',
                    fields: {
                        DocumentNumber: {},
                        DocumentDate: { type: 'date' },
                        DueDate: { type: 'date' },
                        OrderNumber: {},
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
        columns: [{
            field: 'DocumentNumber',
            title: self.resources.InvoiceNo,
            width: 120
        }, {
            field: 'DocumentDate',
            title: self.resources.InvoiceDate,
            width: 140,
            template: "#: DocumentDate ? kendo.toString(kendo.parseDate(DocumentDate, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') : '' #"
        }, {
            field: 'DueDate',
            title: self.resources.DueDate,
            width: 140,
            template: "#: DueDate ? kendo.toString(kendo.parseDate(DueDate, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') : '' #"
        }, {
            field: 'OrderNumber',
            title: self.resources["OrderNo."],
            width: 120,
            template: '<a href="javascript:;" onclick="tradeInvoicesModel.fetchOrderDetails(#: OrderNumber #, \'#: DocumentDate #\')">#: OrderNumber #</a>'
        }, {
            field: 'CustomerOrderReference',
            title: self.resources.CustOrderRef,
            width: 180,
            footerTemplate: '<span class="text-bold display-block text-right">' + self.resources.Totals + ':</span>'
        }, {
            field: 'OriginalValue',
            title: self.resources["Orig.Value"],
            width: 120,
            attributes: {
                'class': 'text-right'
            },
            headerAttributes: {
                'class': 'text-right'
            },
            template: '#: formatNumber(OriginalValue, 2) #',
            footerAttributes: {
                'class': 'text-bold'
            },
            footerTemplate: function () {
                if (self.invoiceSearchResults()) {
                    return '<span class="text-right text-bold display-block">' + formatNumber(Enumerable.From(self.invoiceSearchResults()).Select(function (x) {
                        return x.OriginalValue
                    }).Sum(), 2) + '</span>';
                } else {
                    return '';
                }
            }
        }, {
            field: 'OutstandingAmount',
            title: self.resources.OutstandingAmount,
            width: 120,
            attributes: {
                'class': 'text-right'
            },
            headerAttributes: {
                'class': 'text-right'
            },
            template: '#: formatNumber(OutstandingAmount, 2) #',
            footerTemplate: function () {
                if (self.invoiceSearchResults()) {
                    return '<span class="text-right text-bold display-block">' + formatNumber(Enumerable.From(self.invoiceSearchResults()).Select(function (x) {
                        return x.OutstandingAmount
                    }).Sum(), 2) + '</span>';
                } else {
                    return '';
                }
            }
        }, {
            field: 'Disputed',
            title: self.resources.Disputed,
            width: 120,
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
            window.open('/downloads/trade-document?docref=' + dataItem.DocumentKey + '&c=' + getCountryId() + '&l=' + getLanguageId(), '_blank');
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


    self.dateRange.subscribe(function (newValue) {
        if (newValue != -1) {
            self.performInvoiceSearch();
        }
    });

    self.dateRangeType.subscribe(function (newValue) {
        if (self.dateRange() != -1) {
            self.performInvoiceSearch();
        }
    });

    //#endregion



    //#region Methods

    self.refreshAccountSummary = function () {
        self.accountSummaryBusy(true);
        self.showAccountSummaryInvoicesResult(false);
        self.showAccountSummaryOnAccountResult(false);
        self.accountSummaryDrilldownResult([]);
        self.accountSummaryOnAccountResult([]);
        $.ajax({
            url: '/api/tradeapi/refreshAccountSummary',
            type: 'POST',
            dataType: 'json',
            data: {},
            success: function (data) {
                self.accountSummaryBusy(false);
                self.accountSummary(data);
            },
            error: function (xhr, s, a) {
                window.location.href = '/l102?redirect=112'
            }
        });
    }

    self.accountSummaryDrilldown = (item, title, sort) => {
        self.showAccountSummaryInvoicesResult(false);
        self.showAccountSummaryOnAccountResult(false);
        self.accountSummaryDrilldownResult([]);
        self.accountSummaryOnAccountResult([]);
        self.accountSummaryBusy(true);
        data = {
            criteria: item
        };
        console.log('calling accountSummaryDrilldown with', data);
        $.ajax({
            url: '/api/tradeapi/accountSummaryDrilldown',
            type: 'POST',
            dataType: 'json',
            data: data,
            success: function (data) {
                console.log('accountSummaryDrilldown returned', data);
                self.accountSummaryBusy(false);
                self.accountSummaryGridTitle(title);
                if (data.hasOwnProperty('Invoices')) {
                    self.showAccountSummaryOnAccountResult(false);
                    self.showAccountSummaryInvoicesResult(true);
                    self.accountSummaryDrilldownResult(data.Invoices);
                    window.setTimeout(function () {
                        self.accountSummaryDrilldownGrid.widget().dataSource.query({
                            page: 1,
                            pageSize: self.accountSummaryDrilldownGrid.widget().dataSource.pageSize(),
                            sort: {
                                field: sort,
                                dir: 'asc'
                            }
                        });
                    }, 0);
                } else {
                    self.showAccountSummaryInvoicesResult(false);
                    self.showAccountSummaryOnAccountResult(true);
                    self.accountSummaryOnAccountResult(data.PaymentsOnAccount);
                }
            },
            error: function (xhr, s, a) {
                window.location.href = '/l102?redirect=112'
            }
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
        $.ajax({
            url: '/api/tradeapi/refreshOutstandingInvoices',
            type: 'POST',
            dataType: 'json',
            data: {},
            success: function (data) {
                console.log('refreshOutstandingInvoices returned', data);
                self.outstandingInvoicesBusy(false);
                self.outstandingInvoices(data.Invoices);
                self.outstandingInvoicesAccountBalance(data.OnAccountTotal)
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
            },
            error: function (xhr, s, a) {
                window.location.href = '/l102?redirect=112'
            }
        });
    }

    self.performInvoiceSearch = () => {
        self.invoiceSearchErrors.showAllMessages(false);
        self.validateInvoiceSearchNow(true);
        if (self.invoiceSearchErrors().length) {
            self.invoiceSearchErrors.showAllMessages(true);
        } else {
            self.invoiceSearchBusy(true);
            var data = {
                dateRange: self.dateRange(),
                dateRangeType: self.dateRangeType(),
                searchTerm: self.searchTerm() || ''
            }
            if (self.dateRange() == -1) {
                data.fromDate = self.customFromDate() ? self.customFromDate().toISOString() : null;
                data.toDate = self.customToDate() ? self.customToDate().toISOString() : null;
            }
            console.log('calling invoiceSearch with', data);
            $.ajax({
                url: '/api/tradeapi/invoiceSearch',
                type: 'POST',
                dataType: 'json',
                data: data,
                success: function (data) {
                    console.log('invoiceSearch returned', data);
                    self.invoiceSearchResults(data.SearchResults);
                    self.invoiceSearchBusy(false);
                },
                error: function (xhr, s, a) {
                    window.location.href = '/l102?redirect=112'
                }
            });
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
                affectingBusy = self.accountSummaryBusy;
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