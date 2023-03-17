const RELEASE_AVAILABLE = 1,
    CHANGING_ADDRESS = 2

const tradeReserveOrdersViewModel = ((resources) => {
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

    self.ready = ko.observable(false);

    self.phase = ko.observable(RELEASE_AVAILABLE);

    self.reserveOrdersBusy = ko.observable(true);

    self.reserveOrders = ko.observable(getReserveOrders());

    self.orderDetailsUniVerseAccount = ko.observable();
    self.orderDetailsOrderNumber = ko.observable();
    self.orderDetailsOrderDate = ko.observable();
    self.orderDetailsOrderLines = ko.observableArray([]);
    self.invoiceAddress = ko.observable();
    self.deliveryAddress = ko.observable();
    self.inspectingLine = ko.observable();
    self.lineBusy = ko.observable(false);

    self.lineQuantity = ko.observable().extend({
        required: {
            params: true,
            message: self.resources.QuantityRequired
        },
        numberFormatted: {
            digits: 1,
            currency: {
                FormattingCulture: 'en-GB'
            },
            minValue: 0.5,
            maxValue: 999
        }
    });

    self.reserveDate = ko.observable();

    self.lineReserveDateOriginalValue = ko.observable();

    self.lineQuantityOriginalValue = ko.observable();

    self.lineUpdateErrorMessage = ko.observable();

    self.lineUpdateErrored = ko.observable(false).extend({
        mustEqual: {
            params: false,
            message: function () {
                return self.lineUpdateErrorMessage();
            }
        }
    });

    self.lineReference = ko.observable().extend({
        required: {
            params: true,
            message: self.resources.LineReference
        }
    });

    self.lineMemo = ko.observable();

    self.lineUnsavedChanges = ko.observable(false);

    self.viewingStatus = ko.observable('reserve');

    self.searchTerm = ko.observable();

    self.validateSearchNow = ko.observable(false);

    self.dateRange = ko.observable(12);

    self.dateRangeType = ko.observable(1);

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

    self.allDateRangeTypes = [{
        dateRangeTypeId: 1,
        name: self.resources.OrderDate
    }, {
        dateRangeTypeId: 2,
        name: self.resources.StatusDate
    }, {
        dateRangeTypeId: 3,
        name: self.resources.DueDate
    }];

    self.dateRangeTypes = ko.observableArray();

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
                return self.validateSearchNow() && self.dateRange() == -1 && self.customToDate() == null
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
                return self.validateSearchNow() && self.dateRange() == -1 && self.customFromDate() == null
            },
            message: self.resources.PleaseProvideFromToOrBoth
        }
    });

    self.schema = {
        model: {
            id: 'OrderNumber',
            fields: {
                OrderNumber: {},
                LineNumber: {},
                OrderDate: { type: 'date' },
                OrderQuantity: {},
                Product: {},
                Memo: {},
                Description: {},
                CustomerOrderReference: {},
                Status: {},
                Tracking: {},
                BaseProduct: {},
                DespatchStatusDate: { type: 'date' },
                DespatchStatusDateMessage: { type: 'date' },
                ReservedUntil: {},
                ReservedUntilMessage: {},
                ReserveAction: {},
                ImageID: {},
                ReserveFlag: {}
            }
        }
    };

    self.addressChangeModalElement = document.querySelector('#address-change-modal');

    self.orderDetailsModalElement = document.querySelector('#order-detail-modal');

    self.lineInspectModalElement = document.querySelector('#line-inspect-modal');

    self.lineInspectModalElement.addEventListener('shown.bs.modal', () => {
        self.lineQuantity.mdcTextField.layout();
        self.reserveDate.mdcTextField.layout();
        self.lineReference.mdcTextField.layout();
        self.lineMemo.mdcTextField.layout();
    });

    self.lineSaved = ko.observable(true);

    //#endregion

    //#region Lifted Methods 

    self.exportToExcel = function (e) {
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

    //#region Components

    self.reserveDatePicker = {
        widget: ko.observable(),
        min: new Date(),
        label: self.resources.Date,
        value: self.reserveDate,
        observable: self.reserveDate,
        culture: 'en-GB',
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: false,
        readOnly: false,
        clearFunction: null,
        dateTimePickerOptions: {
            locale: 'en-gb',
            format: 'DD/MM/YYYY',
            ignoreReadonly: true,
            showClear: false,
            allowInputToggle: true,
            sideBySide: false,
            useCurrent: false,
            minDate: new Date(),
            showClose: false,
            keepInvalid: false
        }
    }

    self.lineQuantityParams = {
        label: self.resources.Quantity,
        observable: self.lineQuantity,
        useNativeValidation: false,
        decorateInputElement: true,
        insertValidationMessages: false,
        showClear: false,
        permitRevert: false
    };

    self.lineReferenceParams = {
        label: self.resources.LineReference,
        observable: self.lineReference,
        useNativeValidation: false,
        decorateInputElement: true,
        insertValidationMessages: false,
        showClear: false,
        permitRevert: false,
        charsRemainingOptions: {
            maxLength: 100
        }
    }

    self.lineMemoParams = {
        label: self.resources.LineMemo,
        observable: self.lineMemo,
        useNativeValidation: false,
        decorateInputElement: true,
        insertValidationMessages: false,
        showClear: false,
        permitRevert: false,
        rows: 4,
        charsRemainingOptions: {
            maxLength: 500
        }
    }

    self.inspectingLineErrors = ko.validation.group([
        self.lineQuantity,
        self.lineReference
    ]);

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

    self.reserveOrdersGrid = {
        widget: ko.observable(),
        data: self.reserveOrders,
        useKOTemplates: true,
        rowTemplate: 'reserve-row-template',
        dataSource: new kendo.data.DataSource({
            schema: self.schema,
            serverFiltering: false,
            serverSorting: false,
            serverGrouping: false,
            serverPaging: false,
            pageSize: 20,
            sort: {
                field: 'OrderNumber',
                dir: 'desc'
            },
        }),
        pageable: {
            pageSize: 20,
            pageSizes: [20, 50, 100, 500],
            messages: self.kendoGridPagerMessages
        },
        columns: [{
            field: 'OrderNumber',
            title: self.resources['OrderNo'],
            width: 120
        }, {
            field: 'OrderDate',
            title: self.resources.OrderDate,
            width: 140
        }, {
            field: 'DeliveryReference',
            title: self.resources.DeliveryReference,
            width: 120
        }, {
            field: 'CustomerOrderReference',
            title: self.resources.YourReference,
            width: 180
        }, {
            field: 'ReserveAction',
            title: self.resources.Actions,
            width: 280
        }],
        sortable: true,
        scrollable: true,
        noRecords: {
            template: self.resources.NoRecordsToDisplay
        },
        selectable: 'row',
        change: function (e) {
            var dataItem = this.dataItem(this.select());
            if (dataItem)
                self.fetchReserveLines(dataItem);
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
        useKOTemplates: true,
        rowTemplate: 'order-details-grid-template',
        columns: [{
            field: 'LineNumber',
            title: self.resources['lineNo.'],
            hidden: true
        }, {
            field: 'Product',
            title: self.resources.Product,
            width: 120
        }, {
            field: 'Description',
            title: self.resources.Description
        }, {
            field: 'Quantity',
            title: self.resources.Quantity,
            width: 100,
            headerAttributes: {
                'class': 'text-right'
            },
            attributes: {
                'class': 'text-right'
            }
        }, {
            field: 'BatchNumber',
            title: self.resources['batchNo.'],
            width: 140
        }, {
            field: 'CustomerOrderReference',
            title: self.resources.YourReference,
            width: 115
        }, {
            field: 'DespatchStatus',
            title: self.resources.ReservedUntil,
            width: 140
        }, {
            field: 'Modify',
            title: 'Actions',
            template: '<a href="javascript:;" onclick="modifyReserveLine(this)">' + self.resources.Modify + '</a>',
            width: 180
        }, {
            field: 'ImageID',
            title: ' ',
            hidden: true
        }, {
            field: 'Memo',
            title: self.resources.Memo,
            template: function (dataItem) {
                if (dataItem.Memo) {
                    return dataItem.Memo.replace(/\u00FD/g, "<br/>")
                } else {
                    return '';
                }
            }
        }, {
            field: 'ReserveFlag',
            title: ' ',
            hidden: true
        }],
        sortable: true,
        scrollable: true,
        noRecords: {
            template: self.resources.NoRecordsToDisplay
        },
        //selectable: 'row',
        change: function (e) {
            var dataItem = this.dataItem(this.select());
            self.inspectingLine(dataItem);
            //window.open('/downloads/trade-document?docref=' + encodeURIComponent(dataItem.DocumentNumber) + '&c=' + getCountryId() + '&l=' + getLanguageId(), '_blank');
        }
    };

    self.addressManagementModel = tradeAddressManagementViewModel(self.resources, {
        displayHeading: false,
        displayInvoice: false,
        selectFunction: function (data) {
            if (self.testCountryMatch(data.country())) {
                window.showConfirm(self.resources.AreYouSure, self.resources.AreYouSureChangeDelivery, () => {
                    var delRef = ko.unwrap(data.deliveryReference);
                    self.reserveOrdersBusy(true);
                    bootstrap.Modal.getOrCreateInstance(self.addressChangeModalElement).hide();
                    $.ajax({
                        url: '/api/tradeapi/modifyReserveOrder',
                        type: 'POST',
                        dataType: 'json',
                        data: {
                            action: 'delref',
                            universeAccount: data.UniVerseAccount,
                            deliveryReference: delRef,
                            orderNumber: self.orderDetailsOrderNumber(),
                            universeAccount: self.orderDetailsUniVerseAccount()
                        },
                        success: function (data) {
                            console.log('modifyReserve retured', data);
                            window.showInfo(self.resources.ThankYou, '<p>' + self.resources.ReserveOrderAddressChangedText + '</p>');
                            self.deliveryAddress(data.deliveryAddress);
                            self.invoiceAddress(data.invoiceAddress);
                            self.reserveOrdersBusy(false);
                            self.phase(RELEASE_AVAILABLE);
                            self.search();
                        }
                    });
                }, null);
            } else {
                window.showConfirm(self.resources.AreYouSure, '<div class="alert alert-danger mb-0">\
                            <h4 class="alert-title mb-3">\
                                ' + self.resources.YouMayNotChangeCountryManually1 + '\
                            </h4>\
                             <p class="mb-0">' + self.resources.YouMayNotChangeCountryManually2 + '</p>\
                        </div>',
                    () => {
                        var delRef = ko.unwrap(data.deliveryReference);
                        self.reserveOrdersBusy(true);
                        $.ajax({
                            url: '/api/tradeapi/changeReserveOrderCountry',
                            type: 'POST',
                            dataType: 'json',
                            data: {
                                deliveryReference: delRef,
                                languageId: getLanguageId(),
                                orderNumber: self.orderDetailsOrderNumber(),
                                universeAccount: self.orderDetailsUniVerseAccount(),
                                desiredDestination: data.toBasketAddressJS()
                            },
                            success: function (data) {
                                self.reserveOrdersBusy(false);
                                bootstrap.Modal.getOrCreateInstance(self.addressChangeModalElement).hide();
                                window.showInfo(self.resources.ThankYou, '<p>' + self.resources.AddressChangeRequestSubmitted + '</p>');
                                self.addressManagementModel.addressSearch.addressSearchString(undefined);
                            }
                        });
                    }, null);
            }
        },
        selectPermitted: (address) => {
            return true;
            //if (ko.unwrap(self.deliveryAddress)) {
            //    return ko.unwrap(address.country) == ko.unwrap(self.deliveryAddress).Country
            //} else if (ko.unwrap(self.invoiceAddress)) {
            //    return ko.unwrap(address.country) == ko.unwrap(self.invoiceAddress).Country
            //}
        }
    });

    self.stockInfoModel = stockInfoViewModel({ parent: self });

    //#endregion


    //#region Subscriptions

    self.lineQuantity.subscribe(function (newValue) {
        self.lineUnsavedChanges(true);
    });

    self.lineReference.subscribe(function (newValue) {
        self.lineUnsavedChanges(true);
    });

    self.lineMemo.subscribe(function (newValue) {
        self.lineUnsavedChanges(true);
    });

    self.inspectingLine.subscribe(function (newValue) {
        if (newValue) {
            self.lineQuantity(newValue.Quantity);
            self.lineQuantityOriginalValue(newValue.Quantity);
            self.lineReference(newValue.CustomerOrderReference);

            let d = kendo.parseDate(newValue.DespatchStatus, 'dd/MM/yyyy', 'en-GB')

            if (d != null) {
                self.lineReserveDateOriginalValue(d);
                self.reserveDate(d);
            } else {
                alert(self.resources.ReserveDateError);
            }

            self.lineMemo((newValue.Memo || '').replace(/\u00FD/g, '\n'));
            self.lineUnsavedChanges(false);
            self.lineSaved(false);
            bootstrap.Modal.getOrCreateInstance(self.lineInspectModalElement).show();
        } else {
            self.lineQuantity(undefined);
            self.lineReference(undefined);
            self.lineMemo(undefined);
            self.lineUnsavedChanges(false);
            self.reserveDate(undefined);
            $('.save-message').fadeOut('slow');
            bootstrap.Modal.getOrCreateInstance(self.lineInspectModalElement).hide();
        }
    }, self);

    self.dateRange.subscribe(function (newValue) {
        if (newValue != -1) {
            self.refresh(self.viewingStatus(), true);
        }
    });

    self.dateRangeType.subscribe(function (newValue) {
        if (self.dateRange() != -1) {
            self.refresh(self.viewingStatus(), true);
        }
    });

    //#endregion


    //#region Computeds

    self.amChangingAddress = ko.computed(function () {
        return self.phase() == CHANGING_ADDRESS
    }, self);

    self.isDateExtension = ko.pureComputed(function () {
        if (self.lineReserveDateOriginalValue() && self.reserveDate()) {
            return self.lineReserveDateOriginalValue().toISOString() != self.reserveDate().toISOString();
        } else {
            return false;
        }
    }, self);

    self.isLineQuantityIncrease = ko.pureComputed(function () {
        if (self.lineQuantityOriginalValue() && self.lineQuantity())
            return +self.lineQuantityOriginalValue() < +self.lineQuantity();
        else
            return false;
    }, self);

    self.availableDateRangeTypes = ko.pureComputed(function () {
        switch (self.viewingStatus()) {
            case 'outstanding':
                return [{
                    dateRangeTypeId: 1,
                    name: self.resources.OrderDate
                }]
                break;
            case 'backOrders':
                return [{
                    dateRangeTypeId: 1,
                    name: self.resources.OrderDate
                }, {
                    dateRangeTypeId: 5,
                    name: self.resources.DueDate
                }]
                break;
            case 'reserveOrders':
                return [{
                    dateRangeTypeId: 1,
                    name: self.resources.OrderDate
                }, {
                    dateRangeTypeId: 4,
                    name: self.resources.ExpiryDate
                }]
                break;
            case 'despatchedOrders':
                return [{
                    dateRangeTypeId: 1,
                    name: self.resources.OrderDate
                }, {
                    dateRangeTypeId: 3,
                    name: self.resources.DespatchDate
                }]
                break;
            case 'ordersInProgress':
                return [{
                    dateRangeTypeId: 1,
                    name: self.resources.OrderDate
                }, {
                    dateRangeTypeId: 2,
                    name: self.resources.StatusDate
                }]
                break;
        }
    }, self);

    self.fromPreceedsTo = ko.pureComputed(function () {
        if (self.customFromDate() && self.customToDate()) {
            return self.customFromDate().getTime() <= self.customToDate().getTime();
        }
    }, self).extend({
        equal: {
            params: true,
            onlyIf: function () {
                return self.validateSearchNow() && self.customFromDate() && self.customToDate()
            },
            message: self.resources.FromMustPreceedTo
        }
    });

    self.reserveDestinationCountry = ko.pureComputed(function () {
        if (self.deliveryAddress()) {
            return self.deliveryAddress().Country;
        } else if (self.invoiceAddress()) {
            return self.invoiceAddress().Country;
        } else {
            return null;
        }
    }, self);

    //#endregion


    //#region Methods

    self.hideLineInspectModal = function () {
        self.inspectingLine(undefined);
    }

    self.saveLineInspect = function () {
        if (self.inspectingLineErrors().length) {
            self.inspectingLineErrors.filter(function (x) {
                x.isValid.notifySubscribers(x.isValid());
            });
        } else {
            console.log('saveLineInspect okay');
            self.lineUpdateErrored(false);
            self.lineBusy(true);
            $('.save-message').fadeOut('fast');
            $.ajax({
                url: '/api/tradeapi/updateReserveLine',
                type: 'POST',
                dataType: 'json',
                data: {
                    action: 'Q',
                    code: self.inspectingLine().code,
                    lineKey: self.inspectingLine().OrderNumber + '*' + self.inspectingLine().LineNumber,
                    quantity: self.isLineQuantityIncrease() ? self.lineQuantityOriginalValue() : self.lineQuantity(),
                    lineReference: self.lineReference(),
                    lineMemo: self.lineMemo(),
                    isLineQuantityIncrease: self.isLineQuantityIncrease(),
                    requestedIncreasedQuantity: self.isLineQuantityIncrease() ? self.lineQuantity() : null,
                    isDateExtension: self.isDateExtension(),
                    requestedExtension: self.reserveDate().toISOString(),
                    universeAccount: self.inspectingLine().UniVerseAccount
                },
                success: function (data) {
                    console.log('updateReserveLine returned', data)
                    self.lineUpdateErrored(false);
                    if (data.errorMessage != null) {
                        $.error(data.errorMessage);
                    } else {
                        if (self.isLineQuantityIncrease()) {
                            $.info(self.resources.ReserveQuantityChangeRequested, self.resources.ReserveQuantityPleaseWaitForCustomerServices)
                        }
                        self.lineUnsavedChanges(false);
                        var dataItem = self.reserveOrdersGrid.widget().dataItem(self.reserveOrdersGrid.widget().select());
                        self.fetchReserveLines(dataItem);
                        self.lineSaved(true);
                    }
                    self.lineBusy(false);
                },
                error: function (xhr, s, a) {
                    self.lineBusy(false);
                    if (xhr.responseJSON) {
                        if (xhr.responseJSON.ExceptionMessage.indexOf('alloc') > -1) {
                            $('.save-message.text-danger').fadeIn('fast');
                            self.lineUpdateErrorMessage(xhr.responseJSON.ExceptionMessage.replace(/"/g, ''));
                            self.lineUpdateErrored(true);
                        }
                    } else {
                        window.showError();
                    }
                }
            });
        }
    }

    self.resendBatchCutting = function (line) {
        $.ajax({
            url: '/api/tradeapi/resendBatchCutting',
            type: 'POST',
            dataType: 'json',
            data: {
                languageId: getLanguageId(),
                code: line.Product,
                batch: line.BatchNumber,
                lineKey: line.OrderNumber + '*' + line.LineNumber,
                quantity: line.Quantity,
                lineReference: line.CustomerOrderReference,
                lineMemo: line.Memo,
                isLineQuantityIncrease: false,
                requestedIncreasedQuantity: null
            },
            success: function (data) {
                console.log('resendBatchCutting returned', data)
                if (data.errorMessage != null) {
                    $.error(data.errorMessage);
                } else {
                    $.info(self.resources.RequestSubmitted, self.resources.BatchRequestSubmitted.replace('{0}', line.BatchNumber));
                }
            },
            error: function (xhr, s, a) {
                self.lineBusy(false);
                window.showError();
            }
        });
    }

    self.releaseLine = function (line) {
        window.showConfirm(self.resources.AreYouSure, self.resources.AreYouSureReleaseThisLine, function () {
            self.reserveOrdersBusy(true);
            $.ajax({
                url: '/api/tradeapi/updateReserveLine',
                type: 'POST',
                dataType: 'json',
                data: {
                    languageId: getLanguageId(),
                    action: 'R',
                    lineKey: line.OrderNumber + '*' + line.LineNumber,
                    quantity: line.Quantity,
                    lineReference: line.CustomerOrderReference,
                    lineMemo: line.Memo,
                    universeAccount: line.UniVerseAccount
                },
                success: function (data) {
                    console.log('releaseReserveLine returned', data)
                    if (data.errorMessage != null) {
                        $.error(data.errorMessage);
                    } else {
                        self.fetchReserveLines(self.reserveOrdersGrid.widget().dataItem(self.reserveOrdersGrid.widget().select()));

                        window.showInfo(self.resources.OrderLineReleased, self.resources.ThankYouOrderLineReleased.replace('{0}', line.LineNumber).replace('{1}', line.OrderNumber));
                    }
                    self.reserveOrdersBusy(false);
                },
                error: function (xhr, s, a) {
                    self.lineBusy(false);
                    window.showError();
                }
            });
        }, function () {

        });
    }

    self.cancelLine = function (line) {
        window.showConfirm(self.resources.AreYouSure, self.resources.AreYouSureCancelThisLine, function () {
            self.reserveOrdersBusy(true);
            $.ajax({
                url: '/api/tradeapi/updateReserveLine',
                type: 'POST',
                dataType: 'json',
                data: {
                    action: 'D',
                    lineKey: line.OrderNumber + '*' + line.LineNumber,
                    code: line.Product,
                    quantity: line.Quantity,
                    lineReference: null,
                    lineMemo: null,
                    universeAccount: line.UniVerseAccount
                },
                success: function (data) {
                    console.log('cancelReserveLine returned', data)
                    if (data.error) {
                        window.showError();
                    } else {
                        self.fetchReserveLines(self.reserveOrdersGrid.widget().dataItem(self.reserveOrdersGrid.widget().select()));
                        window.showInfo(self.resources.ReserveLineCancelledTitle, self.resources.ReserveLineCancelled.replace('{0}', line.LineNumber).replace('{1}', line.OrderNumber));
                    }
                    self.reserveOrdersBusy(false);
                },
                error: function (xhr, s, a) {
                    self.lineBusy(false);
                    window.showError();
                }
            });
        }, function () {

        });
    }

    self.search = function () {
        self.refresh(self.viewingStatus(), true);
    }

    self.refresh = function (orderStatus, force, clean) {
        if (clean) {
            //self.dateRange(12);
            //self.dateRangeType(?);
            self.searchTerm(undefined);
        }

        var data = {
            orderStatus: orderStatus,
            dateRange: self.dateRange(),
            dateRangeType: self.dateRangeType(),
            searchTerm: self.searchTerm()
        };

        if ((self.reserveOrders().length == 0) || force) {
            self.reserveOrders([]);
            self.reserveOrdersBusy(true);

            $.ajax({
                url: '/api/tradeapi/reserveOrderSearch',
                type: 'POST',
                dataType: 'json',
                data: data,
                success: function (data) {
                    console.log('reserveOrderSearch returned', data);
                    self.reserveOrders(data);
                    window.setTimeout(() => {
                        self.reserveOrdersBusy(false);
                    }, 0);
                    //self.reserveOrdersGrid.dataSource.filter({});
                },
                error: function (xhr, s, a) {
                    window.showError();
                }
            });
        }
    }

    self.showOrderDetailModal = () => {
        bootstrap.Modal.getOrCreateInstance(self.orderDetailsModalElement).show();
    }

    self.hideOrderDetailModal = () => {
        bootstrap.Modal.getOrCreateInstance(self.orderDetailsModalElement).hide();
    }

    self.fetchReserveLines = (dataItem, onComplete) => {
        self.reserveOrdersBusy(true);
        if (dataItem) {
            $.ajax({
                url: '/api/tradeapi/FetchReserveLines',
                type: 'POST',
                dataType: 'json',
                data: {
                    orderNumber: dataItem.OrderNumber,
                    universeAccount: dataItem.UniVerseAccount
                },
                success: function (data) {
                    console.log('fetchReserveLines returned', data);

                    if (data.OrderLines.length) {
                        self.fetchReserveDeliveryDetail(dataItem);
                        self.orderDetailsOrderDate(dataItem.OrderDate);
                        self.orderDetailsOrderNumber(dataItem.OrderNumber);
                        self.orderDetailsUniVerseAccount(dataItem.UniVerseAccount);
                        self.orderDetailsOrderLines(data.OrderLines);
                        if (!$('#order-detail-modal').is(':visible')) {
                            self.showOrderDetailModal(dataItem);
                        }
                        self.reserveOrdersBusy(false);
                    } else {
                        self.hideOrderDetailModal();
                        self.orderDetailsOrderDate(undefined);
                        self.orderDetailsOrderNumber(undefined);
                        self.orderDetailsUniVerseAccount(undefined);
                        self.orderDetailsOrderLines([]);
                        self.deliveryAddress(undefined);
                        self.invoiceAddress(undefined);
                        self.search();
                    }

                    if (typeof onComplete === 'function') {
                        onComplete();
                    }
                },
                error: function (xhr, s, a) {
                    window.location.href = '/l102?redirect=117'
                }
            });
        } else {
            onComplete();
        }
    }

    self.largeImage = (data, event) => {
        let d = Object.assign({}, data);
        delete d.Product;
        window.largeImageModal.load(d, event);
        //$('#large-image').attr('src', 'https://www.designersguild.com/image/1024/' + data.ImageID);
        //$('#large-image-title').text(data.Description);
        //$('#large-image-modal').modal('show');
    }

    self.testCountryMatch = (country) => {
        if (typeof country === 'object')
            country = country.Code;
        return self.reserveDestinationCountry() === country;
    }

    self.releaseOrder = (dataItem) => {
        if (dataItem == null) {
            dataItem = {
                OrderNumber: self.orderDetailsOrderNumber()
            }
        }
        window.showConfirm(self.resources.AreYouSure, self.resources.AreYouSureReleaseAllLines.replace('{0}', dataItem.OrderNumber), function () {
            //console.log('release order confirmed', dataItem.OrderNumber);
            self.reserveOrdersBusy(true);
            $.ajax({
                url: '/api/tradeapi/modifyReserveOrder',
                type: 'POST',
                dataType: 'json',
                data: {
                    action: 'release',
                    deliveryReference: dataItem.DeliveryReference,
                    orderNumber: self.orderDetailsOrderNumber(),
                    universeAccount: self.orderDetailsUniVerseAccount()
                },
                success: function (data) {
                    console.log('modifyReserve returned', data);
                    self.deliveryAddress(data.deliveryAddress);
                    self.invoiceAddress(data.invoiceAddress);
                    self.reserveOrdersBusy(false);
                    self.phase(RELEASE_AVAILABLE);
                    self.search();
                    self.hideOrderDetailModal();
                    window.showInfo(self.resources.OrderReleased, self.resources.ThankYouOrderReleased.replace('{0}', dataItem.OrderNumber));
                },
                error: (xhr, s, a) => {
                    window.showError();
                }
            });
        }, () => {
            console.log('release order refused', dataItem.OrderNumber);
        })
    }

    self.fetchReserveDeliveryDetail = (dataItem) => {
        console.log('fetchReserveDeliveryDetail calling with', dataItem);
        $.ajax({
            url: '/api/tradeapi/FetchReserveDeliveryDetail',
            type: 'POST',
            dataType: 'json',
            data: {
                orderNumber: dataItem.OrderNumber,
                deliveryReference: dataItem.DeliveryReference
            },
            success: (data) => {
                console.log('fetchReserveDeliveryDetail retured', data);
                self.deliveryAddress(data.deliveryAddress);
                self.invoiceAddress(data.invoiceAddress);
            },
            error: (xhr, s, a) => {
                window.location.href = '/l102?redirect=117'
            }
        });
    }

    self.changeAddress = () => {
        self.phase(CHANGING_ADDRESS);
        bootstrap.Modal.getOrCreateInstance(self.addressChangeModalElement).show();
    }

    self.cancelAddressChange = () => {
        self.phase(RELEASE_AVAILABLE);
        bootstrap.Modal.getOrCreateInstance(self.addressChangeModalElement).hide();
    }

    self.changeQuantity = (ele) => {
        self.ordersDetailsGrid.widget().select(ele);
        window.setTimeout(() => {
            $(self.lineQuantity.DOMElement).parent().addClass('mdc-text-field--focused');
            self.lineMemo.mdcTextField.label_.float(true);
            $(self.lineQuantity.DOMElement).select();
        }, 300);
    }

    self.changeLineReference = (ele) => {
        self.ordersDetailsGrid.widget().select(ele);
        window.setTimeout(() => {
            $(self.lineReference.DOMElement).parent().addClass('mdc-text-field--focused');
            self.lineMemo.mdcTextField.label_.float(true);
            $(self.lineReference.DOMElement).select();
        }, 300);
    }

    self.modifyMemo = (ele) => {
        self.ordersDetailsGrid.widget().select(ele);
        window.setTimeout(() => {
            $(self.lineMemo.DOMElement).parent().addClass('mdc-text-field--focused');
            self.lineMemo.mdcTextField.label_.float(true);
            $(self.lineMemo.DOMElement).select();
        }, 300);
    }

    self.requestExtension = (ele) => {
        self.ordersDetailsGrid.widget().select(ele);
        window.setTimeout(() => {
            $(self.reserveDate.DOMElement).parent().addClass('mdc-text-field--focused');
            self.reserveDate.mdcTextField.label_.float(true);
            $(self.reserveDate.DOMElement).select();
        }, 300);
    }

    //#endregion

    return self;
});

window.tradeReserveOrdersModel = tradeReserveOrdersViewModel(getPrefetchedResources());
ko.applyBindings(window.tradeReserveOrdersModel, document.querySelector('#trade-wrapper'));
window.tradeReserveOrdersModel.ready(true);
window.tradeReserveOrdersModel.refresh('reserve', true);