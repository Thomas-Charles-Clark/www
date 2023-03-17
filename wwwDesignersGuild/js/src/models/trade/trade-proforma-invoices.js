const tradeProformaInvoicesViewModel = ((resources) => {
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

    self.outstandingInvoicesBusy = ko.observable(false);

    self.invoiceSearchBusy = ko.observable(false);

    self.proformaInvoices = ko.observableArray(getProformaInvoices());

    self.searchTerm = ko.observable();

    self.validateSearchNow = ko.observable(false);

    self.dateRange = ko.observable(12);

    self.dateRangeType = ko.observable(1);

    self.proformaStatus = ko.observable('');

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
                return self.validateSearchNow() && self.dateRange() == -1 && self.customToDate() != null
            },
            message: self.resources.PleaseProvideFromDate
        }
    });

    self.customToDate.extend({
        date: {
            params: true,
            message: self.resources.ToMustBeDate
        }
    });

    self.valueDisplay = ko.observable(0); // 0 = base, 1 = currency

    self.columns = [{
        field: 'OrderNumber',
        title: self.resources['orderNo.'],
        width: 120
    }, {
        field: 'DocumentDate',
        title: self.resources.InvoiceDate,
        width: 140,
        template: "#: kendo.toString(kendo.parseDate(DocumentDate, 'yyyy-MM-ddTHH:mm:ss'), 'dd/MM/yyyy') #"
    }, {
        field: 'CustomerOrderReference',
        title: self.resources.YourOrderReference,
        width: 180,
        footerTemplate: '<span class="text-right text-bold display-block">' + self.resources.Total + ':</span>'
    }, {
        field: 'Value',
        title: self.resources.Value,
        width: 120,
        template: function (dataItem) {
            if (dataItem.Value <= 0) {
                return '<span class="text-danger">' + formatNumber(dataItem.Value, 2) + '</span>'
            } else {
                return formatNumber(dataItem.Value, 2);
            }
        },
        attributes: {
            'class': 'text-right'
        },
        headerAttributes: {
            'class': 'text-right'
        },
        footerAttributes: {
            'style': 'vertical-align: top'
        },
        footerTemplate: function () {
            if (self.proformaInvoices()) {
                var val = Enumerable.From(self.proformaInvoices()).Select(function (x) {
                    return x.Value
                }).Sum();
                if (val <= 0) {
                    return '<span class="text-right text-danger display-block">' + formatNumber(val, 2) + '</span>';
                } else {
                    return '<span class="text-right display-block">' + formatNumber(val, 2) + '</span>';
                }
            } else {
                return '';
            }
        }
    }, {
        field: 'OrderType',
        title: self.resources.Status,
        width: 140,
        template: `#: OrderType == "P" ? '${self.resources.Outstanding}' : '${self.resources.Released}' #`
    }];

    //#endregion

    //#region Lifted Methods

    self.exportToExcel = async function (e) {
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



        // Get a reference to the Excel sheet we are working on
        var sheet = e.workbook.sheets[0];
        var hiddenColsShift = 0;
        var hasHeader = false;

        // loop through all cols to check if there is a template in columns
        for (var i = 0; i < this.columns.length; i++) {

            // if a column is hidden, it appears in the columns array but not in the Excel columns collection
            // we must shift -1 each time we find a hidden column in the columns array
            // befor to the set the hyperlink formula in the column next to the hidden one
            if (this.columns[i].hidden === true) {
                hiddenColsShift++;
            }
            else {

                if (this.columns[i].template) {
                    //if (typeof this.columns[i].template === 'function') {

                    //this.columns[i].template.indexOf('http') > -1) {

                    // if the column has a template, it can be a hyperlink
                    var template = kendo.template(this.columns[i].template);

                    // loop through all the rows in the Excel sheet
                    for (var j = 0; j < sheet.rows.length; j++) {

                        // And check that the row is not the header row
                        if (sheet.rows[j].type !== "header") {

                            // get the value of the cell
                            var value = sheet.rows[j].cells[i].value;

                            var templateResult = null;
                            // get the data of the row and execute the template
                            if (hasHeader === true) {
                                templateResult = template(e.sender.dataSource.data()[j - 1]);
                            }
                            else {
                                templateResult = template(e.sender.dataSource.data()[j]);
                            }

                            try {
                                // check if there is an url defined in the template
                                var url = $(templateResult).attr("href");

                                if (url !== null && typeof url !== 'undefined') {
                                    if (url.indexOf(String.fromCodePoint(253)) > -1) {
                                        let a = url.split(String.fromCodePoint(253));
                                        for (var ctr = 0; ctr < a.length; ctr++) {
                                            let url = a[ctr];

                                            if (ctr > 0) {
                                                sheet.rows[j].cells.push({ value: url });
                                            }

                                            // we create a formula hyperlink
                                            sheet.rows[j].cells[i + ctr].formula = '=HYPERLINK("' + url + '","' + self.resources.Track + '")';

                                            // We set the color property as blue, underlined and right aligned to make it look like a link
                                            sheet.rows[j].cells[i + ctr - hiddenColsShift].format = '[Blue]';
                                            sheet.rows[j].cells[i + ctr - hiddenColsShift].underline = true;
                                            sheet.rows[j].cells[i + ctr - hiddenColsShift].textAlign = 'left';
                                        }
                                    } else {
                                        // we create a formula hyperlink
                                        sheet.rows[j].cells[i].formula = '=HYPERLINK("' + url + '","' + self.resources.Track + '")';

                                        // We set the color property as blue, underlined and right aligned to make it look like a link
                                        sheet.rows[j].cells[i - hiddenColsShift].format = '[Blue]';
                                        sheet.rows[j].cells[i - hiddenColsShift].underline = true;
                                        sheet.rows[j].cells[i - hiddenColsShift].textAlign = 'left';
                                    }
                                }
                            }
                            catch {
                                sheet.rows[j].cells[i].value = templateResult;
                            }
                        }
                        else {
                            hasHeader = true;
                        }
                    }
                    //}
                }
            }
        }
    }

    //#endregion


    //#region Components

    self.searchErrors = ko.validation.group([
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

    self.proformaInvoicesGrid = {
        widget: ko.observable(),
        data: self.proformaInvoices,
        dataSource: new kendo.data.DataSource({
            sort: {
                field: 'DocumentDate',
                dir: 'asc'
            },
            schema: {
                model: {
                    id: 'OrderNumber',
                    fields: {
                        OrderNumber: {},
                        DocumentDate: { type: 'date' },
                        CustomerOrderReference: {},
                        Value: {},
                        OrderType: {}
                    }
                }
            },
            pageSize: 20
        }),
        columns: self.columns,
        sortable: true,
        noRecords: {
            template: self.resources.NoRecordsToDisplay
        },
        selectable: 'row',
        scrollable: true,
        change: function (e) {
            var dataItem = this.dataItem(this.select());
            window.open('/downloads/trade-proforma?docref=' + dataItem.DocumentKey + '&c=' + getCountryId() + '&l=' + getLanguageId(), '_blank');
        },
        pageable: {
            pageSize: 20,
            pageSizes: [20, 50, 100, 500],
            messages: self.kendoGridPagerMessages
        },
        excelExport: self.exportToExcel
    }

    //#endregion


    //#region Subscriptions

    self.proformaStatus.subscribe(function (newValue) {
        self.performInvoiceSearch();
    });

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


    //#region Computeds

    self.fromPreceedsTo = ko.pureComputed(function () {
        if (self.customFromDate() && self.customToDate()) {
            return self.customFromDate().getTime() <= self.customToDate().getTime();
        }
    }, self).extend({
        equal: {
            params: true,
            onlyIf: function () {
                return self.customFromDate() && self.customToDate()
            },
            message: self.resources.FromMustPreceedTo
        }
    });

    self.proformaInvoicesTotal = ko.pureComputed(function () {
        return Enumerable.From(self.proformaInvoices()).Select(function (x) {
            return x.Value
        }).Sum();
    }, self);

    //#endregion


    //#region Methods

    self.refreshProformaInvoices = () => {
        self.outstandingInvoicesBusy(true);
        $.ajax({
            url: '/api/tradeapi/refreshProformaInvoices',
            type: 'POST',
            dataType: 'json',
            data: {},
            success: function (data) {
                self.invoiceSearchBusy(false);
                self.proformaInvoices(data.Invoices);
                window.setTimeout(function () {
                    self.proformaInvoicesGrid.widget().dataSource.sort({
                        field: 'DocumentDate',
                        dir: 'asc'
                    });
                }, 0);
            },
            error: function (xhr, s, a) {
                window.location.href = '/l102?redirect=113'
            }
        });
    }

    self.performInvoiceSearch = () => {
        self.searchErrors.showAllMessages(false);
        self.validateSearchNow(true);
        window.setTimeout(function () {
            if (self.searchErrors().length) {
                self.searchErrors.showAllMessages(true);
                self.searchErrors.filter(function (x) {
                    x.isValid.notifySubscribers(x.isValid());
                })
            } else {
                self.invoiceSearchBusy(true);
                var data = {
                    dateRange: self.dateRange(),
                    dateRangeType: self.dateRangeType(),
                    searchTerm: self.searchTerm() || '',
                    orderStatus: self.proformaStatus()
                }
                if (self.dateRange() == -1) {
                    data.fromDate = self.customFromDate() ? self.customFromDate().toISOString() : null;
                    data.toDate = self.customToDate() ? self.customToDate().toISOString() : null;
                }
                console.log('calling invoiceSearch with', data);
                $.ajax({
                    url: '/api/tradeapi/proformaInvoiceSearch',
                    type: 'POST',
                    dataType: 'json',
                    data: data,
                    success: function (data) {
                        console.log('invoiceSearch returned', data);
                        self.proformaInvoices(data.Invoices);
                        self.invoiceSearchBusy(false);
                    },
                    error: function (xhr, s, a) {
                        window.location.href = '/l102?redirect=113'
                    }
                });
            }
        }, 0);
    }

    //#endregion

    return self;
});

window.tradeProformaInvoicesModel = tradeProformaInvoicesViewModel(getPrefetchedResources());

ko.applyBindings(window.tradeProformaInvoicesModel, document.querySelector('#trade-wrapper'));

window.tradeProformaInvoicesModel.performInvoiceSearch();