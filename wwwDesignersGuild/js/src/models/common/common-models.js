const productViewModel = ((productId, basketLineTypeId) => {
    let self = {};

    self.data = ko.observable();

    self.productId = productId;

    self.basketLineTypeId = basketLineTypeId;

    self.productPageId = ko.observable();

    self.enabled = ko.observable(true);

    //self.enabled.subscribe((nv) => {
    //    console.log('enabled', nv)
    //});

    self.quantity = ko.observable();

    self.resources = getPrefetchedResources();

    // This is the lovely magic hook which lets me lift my properties from where I've had Razor write them as JSON into a hidden element
    // as Knockout processes bindings LTR, the init: { convert: JSON.parse } kicks off first, text tells it to bind contents to self.data, and remove
    // then removes the extraneous element from the DOM!
    let dataSub = self.data.subscribe(function (newValue) {
        Object.assign(self, newValue);

        self.quantity(self.stockMessage?.quantityMinimum ?? 1);

        let qMax = 9999;

        switch (self.basketLineTypeId) {
            case 2:
            case 3:
                qMax = self.stockMessage.reserveQuantityMaximum;
                break;
            default:
                qMax = self.stockMessage.quantityMaximum;
                break;
        }

        self.quantity.extend({
            numberFormatted: {
                digits: countDecimals(self.stockMessage?.quantityIncrement),
                currency: {
                    FormattingCulture: getCulture()
                },
                minValue: self.stockMessage?.quantityMinimum,
                maxValue: qMax,
                unit: self.stockMessage?.unit
            },
            roundedNumeric: {
                precision: countDecimals(self.stockMessage?.quantityIncrement ?? 1)
            }
        });

        self.quantity.extend({
            required: {
                params: true,
                onlyIf: function () {
                    return self.enabled()
                },
                message: self.resources.QuantityRequired
            },
            errorPulse: {},
            min: {
                params: self.stockMessage?.quantityMinimum,
                onlyIf: function () {
                    return self.enabled()
                },
                message: `${self.resources.pleaseGreaterThanEqualTo} ${self.stockMessage?.quantityMinimum}`
            },
            max: {
                params: qMax,
                onlyIf: function () {
                    return self.enabled()
                },
                message: `${self.resources.pleaseLessThanEqualTo} ${qMax}`
            }
        });

        if (!qMax) {
            self.enabled(false);
        }

        dataSub.dispose();
    });

    self.image = ko.observable();

    self.products = ko.observableArray([]);

    self.errors = new ko.validation.group([
        self.quantity
    ]);

    self.isValid = function () {
        self.errors.showAllMessages(false);
        if (self.errors().length > 0) {
            self.errors.showAllMessages(true);
            self.errors.filter(function (x) {
                x.isValid.notifySubscribers(x.isValid());
            });
            return false;
        } else {
            return true;
        }
    }

    self.showLargeImage = () => {

    };

    self.primaryProductId = ko.observable();

    // utility function to permit "with" against the appropriate product entry.
    self.getProductByBasketLineType = (productId, basketLineTypeId) =>
        self.products().filter(p => p.productId == productId && p.basketLineTypeId == basketLineTypeId)[0];

    self.buildPopover = () => {
        //var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
        //var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        //    return new bootstrap.Popover(popoverTriggerEl)
        //})

        let orderFormItemElement = self.quantity.DOMElement.closest('.order-form-item');

        if (orderFormItemElement) {

            let reserveOverlayElement = orderFormItemElement.querySelector('.reserve-overlay');

            if (reserveOverlayElement) {

                if (reserveOverlayElement.dataset["bsToggle"] == "popover") {
                    bootstrap.Popover.getOrCreateInstance(reserveOverlayElement);
                }
            }
        }
    }

    return self;
});

// Now, this is a bit of a weird one so bear with me.
// Every "Add to Basket" button is actually representative of an individual product.
// Some products and circumstances have up to 5(!) different buttons (sample, large sample, order, reserve, reserve with batch cutting).
// I term these "order form groups": whether or not that aligns with what the CMS considers to be order form groups doesn't really matter.
const orderFormGroupViewModel = ((parent) => {
    let self = {
        parent: parent
    };

    // binding point for the OrderFormItemViewModel which might present here
    self.data = ko.observable();

    // data subscription
    self.dataSub = self.data.subscribe(nv => {
        Object.assign(self, nv);
        self.dataSub.dispose();
    });

    // Only used by the Paint template so far; is set via prerendered init
    self.primaryProductId = ko.observable();

    // certain templates (those with multiple products on one page)
    // present an image per order form group. Those templates which only present a single product
    // e.g. cushions don't display an image next to the order form group, but the binding point is still necessary.
    self.image = ko.observable();

    // this is a helper method which lets the knockout prerendered binding handler know how to create an image object.
    self.createImage = () => {
        return imageViewModel(self.Definition.productPageId);
    }

    // large image modal invocation
    self.showLargeImage = (data, event) => {
        if (window.hasOwnProperty('largeImageModal'))
            window.largeImageModal.load(self, event);
    };

    // some order form groups further group things by size, some don't
    self.sizeGroups = ko.observableArray([]);

    // another prerendered binding handler helper.
    self.createSizeGroup = () => sizeGroupViewModel(self);

    // those order form groups which don't group by size present products at this level instead
    self.products = ko.observableArray([]);

    // utility function to permit "with" against the appropriate product entry.
    self.getProductByBasketLineType = (productId, basketLineTypeId) =>
        self.products().filter(p => p.productId == productId && p.basketLineTypeId == basketLineTypeId)[0];

    return self;
});

// Some order form groups further group the products within them by their size, e.g. bedlinen.
const sizeGroupViewModel = ((parent) => {
    let self = {
        parent: parent
    }

    // binding point for the OrderFormItemViewModel which will present here
    self.data = ko.observable();

    // data subscription
    self.dataSub = self.data.subscribe(nv => {
        Object.assign(self, nv);
        self.dataSub.dispose();
    });

    // certain templates (those with multiple products on one page)
    // present an image per order form group. Those templates which only present a single product
    // e.g. cushions don't display an image next to the order form group, but the binding point is still necessary.
    self.image = ko.observable();

    // this is a helper method which lets the knockout prerendered binding handler know how to create an image object.
    self.createImage = () => {
        return imageViewModel(self.Definition.productPageId);
    }

    // large image modal invocation
    self.showLargeImage = (data, event) => {
        if (window.hasOwnProperty('largeImageModal'))
            window.largeImageModal.load(self, event);
    };

    // products within this size grouping
    self.products = ko.observableArray([]);

    // utility function to permit "with" against the appropriate product entry.
    self.getProductByBasketLineType = (productId, basketLineTypeId) =>
        self.products().filter(p => p.productId == productId && p.basketLineTypeId == basketLineTypeId)[0];
    
    return self;
});

const imageViewModel = ((productPageId) => {
    let self = {};

    self.ProductPageID = productPageId;

    self.data = ko.observable();

    let dataSub = self.data.subscribe(function (newValue) {
        Object.assign(self, newValue);
        dataSub.dispose();
        self.hotspots(self.Hotspots)
    });

    self.orderFormGroups = ko.observableArray([]);

    self.createOrderFormGroup = () => orderFormGroupViewModel(self);

    self.hotspots = ko.observableArray([]);

    return self;
});

const htmlViewModel = ((parent) => {
    let self = {
        parent: parent
    };

    self.images = ko.observableArray([]);

    self.getImageById = (imageId) => self.images().filter(i => i.ProductPageID == imageId)[0];

    //self.data = ko.observable();

    //let dataSub = self.data.subscribe(function (newValue) {
    //    Object.assign(self, newValue);
    //    dataSub.dispose();
    //    self.hotspots(self.Hotspots)
    //});

    //self.orderFormGroups = ko.observableArray([]);

    //self.createOrderFormGroup = () => orderFormGroupViewModel(self);

    //self.hotspots = ko.observableArray([]);

    return self;
});

const stockMessage = (() => {
    let self = {};

    self.cssClasses = ko.observable();

    self.stockMessage = ko.observable();

    return self;
});

const marketingPermissionCheckerViewModel = ((parent) => {
    let self = {
        parent: parent
    };

    //#region Properties

    self.busy = parent.busy || ko.observable(false);

    self.email = parent.email || ko.observable().extend({
        required: {
            params: true,
            message: 'marketingPermissionChecker email required - email was not inherited from parent'
        },
        email: {
            params: true,
            message: 'marketingPermissionChecker email format - email was not inherited from parent'
        }
    });

    self.emailConfirmation = parent.emailConfirmation || ko.observable().extend({
        required: {
            params: true,
            message: 'marketingPermissionChecker emailConfirmation required - emailConfirmation was not inherited from parent'
        },
        email: {
            params: true,
            message: 'marketingPermissionChecker emailConfirmation format - emailConfirmation was not inherited from parent'
        }
    });

    self.addressRequired = parent.addressRequired || ko.observable(false);

    self.sourceId = parent.sourceId || 0;

    self.thirdPartyId = parent.thirdPartyId || null;

    self.availableLanguages = parent.availableLanguages || ko.observableArray([]);

    self.languageId = parent.languageId || window.getLanguageId();

    self.countryId = parent.countryId || window.getCountryId();

    self.marketingCountryId = parent.marketingCountryId || window.getCountryId();

    self.marketingPermissions = ko.observableArray([]).extend({
        postable: true
    });

    self.havePerformedMarketingPermissionCheck = ko.observable(false).extend({
        equal: {
            params: true,
            message: 'marketingPermissionChecker has not checked for marketing permissions'
        }
    });

    //#endregion

    //#region Subscriptions

    self.email.subscribe(_ => self.havePerformedMarketingPermissionCheck(false));

    self.emailConfirmation.subscribe(_ => self.havePerformedMarketingPermissionCheck(false));

    //self.havePerformedMarketingPermissionCheck.subscribe(nv => console.log('havePerformed', nv));

    self.emailConfirmation.isValid.subscribe(function (newValue) {
        if (newValue) {
            setTimeout(() => self.checkMarketingPermissions(), 0);
        }
    });

    self.marketingCountryId.subscribe(newValue => {
        self.loadLanguages();
        self.checkMarketingPermissions();
    });

    //#endregion

    //#region Methods

    self.loadLanguages = function () {
        fetch('/api/member/LoadLanguagesForCountry', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-store',
            credentials: 'same-origin',
            body: JSON.stringify({
                countryId: self.marketingCountryId()
            })
        })
        .then(window.handleFetchResponse)
        .then(languages => {
            self.availableLanguages(languages);
        });
    }

    self.checkMarketingPermissions = () => {
        if (self.email() && self.email.isValid()) {
            self.busy(true);
            fetch('/api/form/CheckMarketingPermission', {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-store',
                credentials: 'same-origin',
                body: self.toJSON()
            })
                .then(window.handleFetchResponse)
                .then(result => {
                    self.parent.userEmailMarketingPermitted(result.EmailMarketingPermitted);
                    self.parent.userUnsubscribedFromEmailMarketing(result.UnsubscribedFromEmailMarketing);
                    self.marketingPermissions(result.Permissions.map(p => marketingPermission(p)));
                    self.havePerformedMarketingPermissionCheck(true);
                    self.busy(false);
                })
                .catch(err => {
                    self.parent.userEmailMarketingPermitted(false);
                    self.parent.userUnsubscribedFromEmailMarketing(false);
                    self.marketingPermissions([]);
                    self.havePerformedMarketingPermissionCheck(false);
                    self.busy(false);
                    window.showError();
                })
        } else {
            self.busy(false);
        }
    };

    self.toJS = () => {
        let languageId = ko.unwrap(self.languageId);
        return {
            Email: ko.unwrap(self.email),
            AddressRequired: ko.unwrap(self.addressRequired),
            CountryID: ko.unwrap(self.countryId),
            LanguageID: languageId || 1,
            SourceID: ko.unwrap(self.sourceId),
            MarketingCountryID: ko.unwrap(self.marketingCountryId),
            ThirdPartyID: ko.unwrap(self.thirdPartyId)
        }
    };

    self.toJSON = () => JSON.stringify(self.toJS());

    //#endregion

    return self;
});

// not const because the checkoutsinglepagejs bundle has a prior implementation of marketingPermission
// and the override remains necessary until such a time as the checkout is fully rewritten
var marketingPermission = ((mp) => {
    let self = Object.assign({}, mp);

    self.Checked = ko.observable(self.Default);

    return self;
});