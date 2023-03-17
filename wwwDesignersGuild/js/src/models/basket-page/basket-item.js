const basketItemViewModel = ((parent, quantity) => {
    let self = {
        parent: parent
    };

    //#region Properties

    self.busy = ko.observable(false);

    self.data = ko.observable();

    self.productId = ko.observable();

    self.quantity = ko.observable(quantity).extend({
        required: {
            params: true,
            message: self.parent.resources.QuantityRequired
        },
        rateLimit: {
            timeout: 50,
            method: 'notifyWhenChangesStop'
        }
    });

    self.quantityMaximum = ko.observable();

    self.quantityMinimum = ko.observable();

    // comes into play on basket maintenance page when updating google etc
    self.previousQuantity = ko.observable();

    self.saveForLater = ko.observable();

    //#endregion

    //#region Components

    //#endregion

    //#region Subscriptions

    self.dataSub = self.data.subscribe(d => {
        Object.assign(self, d);
        self.dataSub.dispose();
        self.quantity.extend({
            roundedNumeric: {
                precision: countDecimals(self.StockMessaging.QuantityIncrement)
            },
            min: {
                params: self.StockMessaging.QuantityMinimum
            }
        });

        self.saveForLater(self.SaveForLater);

        self.productId(self.ProductID);

        self.quantity(self.Quantity);

        self.quantityMinimum(self.StockMessaging.QuantityMinimum);

        self.quantityMaximum(self.StockMessaging.QuantityMaximum);

        self.previousQuantity(self.Quantity);

        self.quantity.subscribe(nv => {
            if (self.quantity.isValid())
                self.parent.updateBasket(self);
        });

        self.code = self.Code;

        self.name = self.BasketName;
    });

    //#endregion

    //#region Computeds

    self.deliveryOptions = ko.pureComputed(() => {
        return self.parent.productDeliveryOptions().filter(pdo => pdo.ProductID == self.productId())
    }, self);

    self.basketItemErrors = ko.pureComputed(function () {
        if (self.productId() && self.parent.basketItemErrors()) {
            return self.parent.basketItemErrors().filter(pe => {
                return pe.productId == self.productId() &&
                    pe.basketLineTypeId == self.BasketLineTypeID
            });
        } else {
            return [];
        }
    }, self);

    //#endregion

    //#region Methods

    self.increaseQuantity = () => {
        if (self.quantity() < self.StockMessaging.QuantityMaximum) {
            let nextValue = Math.round((self.quantity() + self.StockMessaging.QuantityIncrement) * 100) / 100;
            self.quantity(nextValue);
        }
    }

    self.canIncreaseQuantity = ko.computed(() => {
        return self.quantity() < self.quantityMaximum();
    }, self);

    self.decreaseQuantity = () => {
        if (self.quantity() > self.StockMessaging.QuantityMinimum) {
            let nextValue = Math.round((self.quantity() - self.StockMessaging.QuantityIncrement) * 100) / 100;
            self.quantity(nextValue);
        }
    }

    self.canDecreaseQuantity = ko.computed(() => {
        return self.quantity() > self.quantityMinimum();
    }, self);

    self.deleteItem = () => {
        showConfirm(self.parent.resources.AreYouSure, self.parent.resources.ConfirmBasketItemDelete, () => {
            self.SaveForLater = false;
            self.quantity(0);
        }, null);
    }

    self.saveItem = () => {
        self.saveForLater(true);
        self.parent.updateBasket(self);
    }

    self.restoreItem = () => {
        self.saveForLater(false);
        self.parent.updateBasket(self);
    }

    self.backInStockReminder = () => {
        console.error('basketItemViewModel.backInStockReminder not implemented')
    }

    //#endregion

    return self;
});