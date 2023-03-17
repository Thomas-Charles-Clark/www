const userRecordViewModel = ((data, resources) => {
    let self = {
        resources: resources
    };

    //#region Properties

    self.saved = true;

    self.rowClass = '';

    self.admin = false;

    self.editEmail = false;

    self.checkEmailBusy = ko.observable(false);

    self.permissionsAlreadyExist = ko.observable(false);

    self.memberId = data.MemberID;
    self.fullName = data.FullName;
    self.email = ko.observable(data.Email);
    self.daysRemaining = ko.observable(data.DaysRemainingToRegister);
    self.tradeAuthenticationComplete = data.TradeAuthenticationComplete;
    self.manageAccount = ko.observable(data.TradePermissionAccountAdmin);
    self.placeOrders = ko.observable(data.TradePermissionPlaceOrder);
    self.placeSamples = ko.observable(data.TradePermissionRequestSample);
    self.viewAccounts = ko.observable(data.TradePermissionViewAccounts);
    self.downloadPricelistsBrochures = ko.observable(data.TradePermissionDownloadPricelistsBrochures);
    self.downloadProductFeeds = ko.observable(data.TradePermissionDownloadProductFeeds);
    self.tradePricingDisplay = data.DisplayTradePrice;

    self.validateNow = ko.observable(false);

    self.emailParams = {
        label: self.resources.email,
        observable: self.email,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            $(self.observable.DOMElement).focus();
        },
        permitRevert: false,
        charsRemainingOptions: {
            maxLength: 254
        }
    };

    //#endregion


    //#region Components



    //#endregion


    //#region Subscriptions

    self.manageAccount.subscribe(function (newValue) {
        //viewModel.unsavedChanges(true);
        //self.unsavedChanges(true);
    });

    self.placeOrders.subscribe(function (newValue) {
        //viewModel.unsavedChanges(true);
        //self.unsavedChanges(true);
    });

    self.viewAccounts.subscribe(function (newValue) {
        //viewModel.unsavedChanges(true);
        //self.unsavedChanges(true);
    });

    self.downloadPricelistsBrochures.subscribe(function (newValue) {
        //viewModel.unsavedChanges(true);
        //self.unsavedChanges(true);
    });

    self.downloadProductFeeds.subscribe(function (newValue) {
        //viewModel.unsavedChanges(true);
        //self.unsavedChanges(true);
    });

    //#endregion


    //#region Computeds

    self.permissionsValid = ko.computed(function () {
        var pO = self.placeOrders();
        var pS = self.placeSamples();
        var vA = self.viewAccounts();
        var vSS = self.downloadPricelistsBrochures();
        var d = self.downloadProductFeeds();

        if (pO || vA || vSS || d || pS)
            return true;
        else
            return false;
    }, self).extend({
        equal: {
            params: true,
            onlyIf: function () {
                return self.validateNow()
            },
            message: ' '
        }
    });

    //#endregion


    //#region Methods

    self.get = () => {
        return {
            MemberID: self.memberId,
            FullName: self.fullName,
            Email: self.email,
            ManageAccount: self.manageAccount(),
            PlaceOrders: self.placeOrders(),
            PlaceSamples: self.placeSamples(),
            ViewAccounts: self.viewAccounts(),
            DownloadPricelistsBrochures: self.downloadPricelistsBrochures(),
            DownloadProductFeeds: self.downloadProductFeeds(),
            CountryID: getCountryId(),
            LanguageID: getLanguageId()
        }
    }
    
    //#endregion

    return self;
});

const newUserRecordViewModel = ((admin, resources) => {
    let self = {
        resources: resources
    };

    //#region Properties

    self.saved = false;

    self.admin = admin;

    self.editEmail = true;

    self.validateNow = ko.observable(false);

    self.memberId = ko.observable();

    self.email = ko.observable().extend({
        required: {
            params: true,
            onlyIf: function () {
                return self.validateNow()
            },
            message: self.resources.ManageTradeAccountErrorEmail
        },
        email: {
            params: true,
            onlyIf: function () {
                return self.validateNow();
            },
            message: self.resources.ManageAccountErrorEmailFormat
        }
    });

    self.emailParams = {
        label: self.resources.email,
        observable: self.email,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            $(self.observable.DOMElement).focus();
        },
        permitRevert: false,
        charsRemainingOptions: {
            maxLength: 254
        }
    };

    self.manageAccount = admin;
    self.placeOrders = ko.observable(admin);
    self.placeSamples = ko.observable(admin);
    self.viewAccounts = ko.observable(admin);
    self.downloadPricelistsBrochures = ko.observable(admin);
    self.downloadProductFeeds = ko.observable(admin);

    self.checkEmailBusy = ko.observable(false);

    self.extantTradePermissionsCheckPassed = ko.observable(false).extend({
        required: {
            params: true
        }
    });

    self.permissionsAlreadyExist = ko.observable(false);

    //#endregion


    //#region Components



    //#endregion


    //#region Subscriptions

    self.email.subscribe(function (newValue) {
        self.permissionsAlreadyExist(false);
        self.extantTradePermissionsCheckPassed(false);
    });

    //#endregion


    //#region Computeds

    self.permissionsValid = ko.computed(function () {
        var pO = self.placeOrders();
        var pS = self.placeSamples();
        var vA = self.viewAccounts();
        var vSS = self.downloadPricelistsBrochures();
        var d = self.downloadProductFeeds();

        if (pO || vA || vSS || d || pS)
            return true;
        else
            return false;
    }, self).extend({
        equal: {
            params: true,
            onlyIf: function () {
                return self.validateNow()
            },
            message: self.resources.ManageTradeAccount_SelectPermissions
        }
    });

    //#endregion


    //#region Methods

    self.initLateExtenders = function () {
        self.email.extend({
            charsRemaining: {
                maxLength: 254,
                element: self.email.DOMElement
            }
        });
    }

    self.performExtantTradePermissionsCheck = function () {

        self.checkEmailBusy(true);

        $.ajax({
            url: '/api/member/extantTradePermissionsCheck',
            type: 'POST',
            dataType: 'json',
            cache: false,
            data: {
                memberEmail: self.email()
            },
            success: function (data) {
                console.log('checkEmail returned', data);
                self.checkEmailBusy(false);
                if (data.hasOwnProperty('handleErrorMessage')) {
                    self.extantTradePermissionsCheckPassed(false);
                    self.extantTradePermissionsCheckPassed.isModified(true);
                    if (data.handleErrorMessage == 'TradePermissionsExist') {
                        self.permissionsAlreadyExist(true);
                    }
                } else if (data) {
                    self.memberId(data.MemberID);
                    self.permissionsAlreadyExist(false);
                    self.extantTradePermissionsCheckPassed(true);
                    self.extantTradePermissionsCheckPassed.isModified(true);
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                self.extantTradePermissionsCheckPassed(false);
                window.showError();
            }
        });
    }

    self.get = () => {
        return {
            MemberID: self.memberId(),
            //FullName: self.fullName,
            Email: self.email,
            ManageAccount: self.manageAccount,
            PlaceOrders: self.placeOrders(),
            PlaceSamples: self.placeSamples(),
            ViewAccounts: self.viewAccounts(),
            DownloadPricelistsBrochures: self.downloadPricelistsBrochures(),
            DownloadProductFeeds: self.downloadProductFeeds(),
            CountryID: getCountryId(),
            LanguageID: getLanguageId()
        }
    }

    //#endregion

    return self;
});

const tradeUserManagementViewModel = ((resources) => {
    let self = {
        resources: resources
    };

    //#region Properties

    self.ready = ko.observable(false);

    self.busy = ko.observable(false);

    self.userToRemove = ko.observable();
    self.resendRecipient = ko.observable();

    self.tradeAdmins = ko.observableArray([]);
    self.tradeUsers = ko.observableArray([]);

    self.editingUser = ko.observable();

    self.newTradeAdmins = ko.observableArray([]);
    self.newTradeUsers = ko.observableArray([]);

    self.tradeInfo = ko.observable(getTradeInfo());

    self.userEditModalElement = document.querySelector('#user-edit-modal');

    self.removeConfirmElement = document.querySelector('#remove-confirm-modal');

    self.resentConfirmElement = document.querySelector('#resent-confirm-modal');

    self.userEditModalElement.addEventListener('shown.bs.modal', () => {
        if (self.editingUser()) {
            self.editingUser().email.mdcTextField.layout();
        }
    });

    //#endregion

    //#region Components

    self.errors = ko.validation.group([
        self.tradeAdmins,
        self.tradeUsers,
        self.newTradeUsers,
        self.newTradeAdmins
    ], {
        deep: true
    });

    //#endregion


    //#region Subscriptions

    self.newTradeAdmins.subscribe(function (newValues) {
        //self.unsavedChanges(true);
    })

    self.newTradeUsers.subscribe(function (newValues) {
        //self.unsavedChanges(true);
    });

    //#endregion


    //#region Computeds

    self.haveCheckedAllNewEmails = ko.computed(function () {
        var result = [true];
        ko.utils.arrayForEach(self.newTradeUsers(), function (item) {
            result.push(item.extantTradePermissionsCheckPassed());
        });
        ko.utils.arrayForEach(self.newTradeAdmins(), function (item) {
            result.push(item.extantTradePermissionsCheckPassed());
        });

        var results = Enumerable.From(result).Distinct().ToArray();

        if (results.length != 1) {
            return false;
        }
        else {
            return results[0];
        }
    }, self);

    self.haveCheckedAllNewEmails.subscribe(function (newValue) {
        if ((self.newTradeAdmins().length || self.newTradeUsers().length) && newValue) {
            self.save();
        }
    });

    //#endregion


    //#region Methods

    self.buildTradeInfo = function (tradeInfo) {
        var tradeAdmins = [];
        var tradeUsers = [];

        if (tradeInfo) {
            for (var i = 0; i < tradeInfo.Administrators.length; i++) {
                tradeAdmins.push(userRecordViewModel(tradeInfo.Administrators[i], self.resources));
            }

            for (var i = 0; i < tradeInfo.Users.length; i++) {
                tradeUsers.push(userRecordViewModel(tradeInfo.Users[i], self.resources));
            }

            self.tradeAdmins(tradeAdmins);
            self.tradeUsers(tradeUsers);
        }
    }

    self.showUserEditModal = () => {
        bootstrap.Modal.getOrCreateInstance(self.userEditModalElement).show();
    }

    self.hideUserEditModal = () => {
        bootstrap.Modal.getOrCreateInstance(self.userEditModalElement).hide();
    }

    self.showRemovalConfirmationModal = () => {
        bootstrap.Modal.getOrCreateInstance(self.removeConfirmElement).show();
    }

    self.hideRemovalConfirmationModal = () => {
        bootstrap.Modal.getOrCreateInstance(self.removeConfirmElement).hide();
    }

    self.showResentConfirmationModal = () => {
        bootstrap.Modal.getOrCreateInstance(self.resentConfirmElement).show();
    }

    self.hideResentConfirmationModal = () => {
        bootstrap.Modal.getOrCreateInstance(self.resentConfirmElement).hide();
    }

    self.addAdmin = function () {
        var r = newUserRecordViewModel(true, self.resources);
        self.editingUser(r);
        self.newTradeAdmins.push(r);
        self.showUserEditModal();

        self.errors = ko.validation.group([
            self.tradeAdmins,
            self.tradeUsers,
            self.newTradeUsers,
            self.newTradeAdmins
        ], {
            deep: true
        });
    }

    self.addUser = function () {
        var r = newUserRecordViewModel(false, self.resources);
        self.editingUser(r);
        self.newTradeUsers.push(r);
        self.showUserEditModal();

        self.errors = ko.validation.group([
            self.tradeAdmins,
            self.tradeUsers,
            self.newTradeUsers,
            self.newTradeAdmins
        ], {
            deep: true
        });
    }

    self.editUser = function (user) {
        self.editingUser(user);
        self.showUserEditModal();

        self.errors = ko.validation.group([
            self.tradeAdmins,
            self.tradeUsers,
            self.newTradeUsers,
            self.newTradeAdmins
        ], {
            deep: true
        });
    }

    self.closeUserEditModal = function () {
        self.newTradeAdmins.remove(self.editingUser());
        self.newTradeUsers.remove(self.editingUser());
        self.editingUser(undefined);
        self.hideUserEditModal();

        self.errors = ko.validation.group([
            self.tradeAdmins,
            self.tradeUsers,
            self.newTradeUsers,
            self.newTradeAdmins
        ], {
            deep: true
        });
    }

    self.save = () => {
        ko.utils.arrayForEach(self.tradeUsers(), function (item) {
            item.validateNow(false);
        });
        ko.utils.arrayForEach(self.tradeAdmins(), function (item) {
            item.validateNow(false);
        });
        ko.utils.arrayForEach(self.newTradeUsers(), function (item) {
            item.validateNow(false);
        });
        ko.utils.arrayForEach(self.newTradeAdmins(), function (item) {
            item.validateNow(false);
        });

        ko.utils.arrayForEach(self.tradeUsers(), function (item) {
            item.validateNow(true);
        });
        ko.utils.arrayForEach(self.tradeAdmins(), function (item) {
            item.validateNow(true);
        });
        ko.utils.arrayForEach(self.newTradeUsers(), function (item) {
            item.validateNow(true);
        });
        ko.utils.arrayForEach(self.newTradeAdmins(), function (item) {
            item.validateNow(true);
        });

        window.setTimeout(function () {

            if (self.errors().length > 0) {
                self.errors.showAllMessages(true);
                return;
            }
            else {
                if (self.haveCheckedAllNewEmails() == false) {
                    self.beginAllNewEmailChecks();
                    return;
                }

                var data = {};

                if (self.editingUser()) {
                    data = self.editingUser().get();
                }

                self.busy(true);

                $.ajax({
                    url: '/api/tradeapi/userManagementSave',
                    type: 'POST',
                    dataType: 'json',
                    data: data,
                    success: function (data) {
                        $("#save-success").show();

                        self.busy(false);

                        window.setTimeout(function () {
                            $("#save-success").fadeOut('fast');
                        }, 3000);

                        $('#admin-scroll').scrollLeft(0);
                        $('#user-scroll').scrollLeft(0);

                        self.closeUserEditModal();

                        self.newTradeAdmins([]);
                        self.newTradeUsers([]);

                        ko.utils.arrayForEach(self.tradeUsers(), function (item) {
                            //item.unsavedChanges(false);
                        });

                        ko.utils.arrayForEach(self.tradeAdmins(), function (item) {
                            //item.unsavedChanges(false);
                        });

                        if (data.hasOwnProperty('tradeAccountModel')) {
                            if (data.tradeAccountModel != null) {
                                self.buildTradeInfo(data.tradeAccountModel);
                            }
                        }

                        //self.unsavedChanges(false);
                    },
                    error: function (xhr, textStatus, errorThrown) {
                        window.showError();
                    }
                });
            }
        }, 0);
    }

    self.deleteAdmin = function (admin) {
        self.deleteUser(admin);
    }

    self.deleteNewAdmin = function (r) {
        self.newTradeAdmins.remove(r);
    }

    self.deleteUser = function (user) { // /api/member/deleteTradeUser
        self.userToRemove(user);
        self.showRemovalConfirmationModal();
    }

    self.deleteConfirmed = function () {
        $.ajax({
            url: '/api/tradeapi/deleteTradeUser',
            type: 'POST',
            dataType: 'json',
            data: {
                MemberID: ko.unwrap(self.userToRemove().memberId)
            },
            success: function (data) {
                self.tradeAdmins.remove(self.userToRemove());
                self.tradeUsers.remove(self.userToRemove());
            },
            error: function (xhr, textStatus, errorThrown) {
                window.showError();
            }
        });
    }

    self.deleteCancelled = function () {
        self.userToRemove(undefined);
    }

    self.deleteNewUser = function (r) {
        self.newTradeUsers.remove(r);
    }

    self.beginAllNewEmailChecks = function () {
        ko.utils.arrayForEach(self.newTradeAdmins(), function (item) {
            if (!item.extantTradePermissionsCheckPassed()) {
                item.performExtantTradePermissionsCheck();
            }
        });
        ko.utils.arrayForEach(self.newTradeUsers(), function (item) {
            if (!item.extantTradePermissionsCheckPassed()) {
                item.performExtantTradePermissionsCheck();
            }
        });
    }

    self.resendInvite = function (user) {
        $.ajax({
            url: '/api/tradeapi/resendTradeAuthenticationInvite',
            type: 'POST',
            dataType: 'json',
            data: user.get(),
            success: function (data) {
                self.resendRecipient(user.email);
                user.daysRemaining(data.daysRemaining);
                self.showResentConfirmationModal();
            },
            error: function (xhr, textStatus, errorThrown) {
                window.showError();
            }
        });
    }

    self.resendAdminInvite = function (user) {
        $.ajax({
            url: '/api/tradeapi/resendTradeAdminAuthenticationInvite',
            type: 'POST',
            dataType: 'json',
            data: user.get(),
            success: function (data) {
                self.resendRecipient(user.email);
                user.daysRemaining(data.daysRemaining);
                self.showResentConfirmationModal();
            },
            error: function (xhr, textStatus, errorThrown) {
                window.showError();
            }
        });
    }

    self.init = () => {
        self.buildTradeInfo(self.tradeInfo());
    }

    //#endregion

    return self;
});

window.tradeUserManagementModel = tradeUserManagementViewModel(getPrefetchedResources());

ko.applyBindings(window.tradeUserManagementModel, document.querySelector('#trade-wrapper'));

window.tradeUserManagementModel.init();