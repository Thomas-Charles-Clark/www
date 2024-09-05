const basketViewModel = ((resources) => {
    let self = {
        resources: ko.observable(resources)
    };

    //#region Properties

    // Is this model ready?
    self.ready = ko.observable(false);

    // Is this model busy?
    self.busy = ko.observable();

    // Basket Items
    self.basketItems = ko.observableArray([]);

    // Int count of items currently in basket
    self.itemsInBasket = ko.observable();

    // Snippet? / promotional message contents from CMS
    self.promotionalMessage = ko.observable();

    // Summary message - snippet? Which of these two is actually fed by the snippet, and which SnippetLocationID is it?
    self.summaryMessage = ko.observable();

    //#endregion


    //#region Components



    //#endregion


    //#region Subscriptions


    //#endregion


    //#region Computeds

    self.samples = ko.computed(() => {
        return self.basketItems().filter(bi => bi.ProductTypeID == 55);
    }, self);

    self.samplesGrouped = ko.computed(() => {
        let samples = self.samples();

        let results = [{
            samples: []
        }];

        let counter = 0;

        for (let i = 0; i < samples.length; i++) {
            results[results.length - 1].samples.push(samples[i]);
            counter++;
            if (counter % 6 == 0 && i + 1 < samples.length) {
                counter = 0;
                results.push({
                    samples: []
                });
            }
        }

        return results;
    }, self);

    //#endregion


    //#region Methods

    self.nextClickHandler = (event) => {
        let menuElement = document.querySelector('#basket-menu');
        if (menuElement) {
            if (event.target !== menuElement && !menuElement.contains(event.target)) {
                menuElement.classList.remove('open');
                document.body.removeEventListener('click', self.nextClickHandler);
            }
        }
    }

    // Everyone's favourite add-to-basket function!
    self.addToBasket = (p, e) => {
        return new Promise((resolve, reject) => {
            if (p.isValid()) {
                p.enabled(false);
                self.busy(true);
                let target = e.currentTarget;
                let payload = JSON.stringify({
                    productId: p.productId,
                    quantity: p.quantity(),
                    basketLineTypeId: p.basketLineTypeId,
                    productPageId: ko.unwrap(p.productPageId),
                    basketCountry: getSiteCountry(),
                    priceListId: getPriceListId()
                });
                console.trace('calling addToBasket with', payload);
                fetch('/api/basket/addToBasket', {
                    method: 'POST',
                    headers: window.fetchHeaders,
                    cache: 'no-store',
                    credentials: 'same-origin',
                    body: payload
                })
                    .then(async (response) => {
                        p.enabled(true);
                        return await window.handleFetchResponse(response, payload);
                    })
                    .then((reply) => {
                        console.trace('addToBasket returned', reply);

                        self.itemsInBasket(reply.totalDisplayQuantity);

                        //target.parentElement == order-form-controls

                        //target.parentElement.parentElement == order-form-item


                        if (target) {
                            let modularFurnitureModal = target.closest('.module-option-preview') != null;

                            if (modularFurnitureModal == false) {

                                let extant = target.parentElement.parentElement.querySelector('.basket-message');

                                if (extant)
                                    target.parentElement.parentElement.removeChild(extant);

                                let bisr = target.parentElement.parentElement.querySelector('.back-in-stock-wrapper');

                                if (bisr) {
                                    target.parentElement.parentElement.removeChild(bisr);
                                }

                                target.parentElement.parentElement.insertAdjacentHTML('beforeend', `<span class="basket-message">${reply.basketLineText}</span>`);

                                if (bisr) {
                                    target.parentElement.parentElement.appendChild(bisr);
                                }

                            } else {

                                let parent = target.closest('.order-form-details').parentElement.querySelector('.order-form-item.relative');

                                let extant = parent.querySelector('.basket-message');

                                if (extant)
                                    parent.removeChild(extant);

                                let bisr = parent.querySelector('.back-in-stock-wrapper');

                                if (bisr) {
                                    parent.removeChild(bisr);
                                }

                                parent.insertAdjacentHTML('beforeend', `<span class="basket-message">${reply.basketLineText}</span>`);

                                if (bisr) {
                                    parent.appendChild(bisr);
                                }
                            }
                        }
                       
                        document.body.addEventListener('click', self.nextClickHandler);

                        self.refreshBasket();

                        if (typeof window.gTagAddToCart === 'function')
                            window.gTagAddToCart(p, p.quantity());

                        if (typeof fbq === 'function') {
                            fbq('track', 'AddToCart', {
                                content_ids: [p.productId.toString()],
                                content_type: 'product',
                                value: Number(reply.price),
                                currency: reply.currency
                            });
                        }

                        resolve();
                    })
                    .catch(err => {
                        self.busy(false);
                        window.catchFetchErrors(err);
                        reject();
                    });
            } else {
                reject();
            }
        });
    }

    // Refreshes the basket details (calls pub.QuickOrder_Basket_Generate)
    self.refreshBasket = () => {
        // we might not have called addToBasket to get here: we might have called showBasketSummary instead
        if (!window.hasOwnProperty('quickOrderingViewModel')) {
            self.busy(true);
            let menuElement = document.querySelector('#basket-menu');
            if (menuElement)
                menuElement.classList.add('open');
        }

        fetch('/api/basket/refreshBasket', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-store',
            credentials: 'same-origin',
            body: JSON.stringify(getMasterDefinition())
        })
            .then(window.handleFetchResponse)
            .then(data => {
                //console.trace(data);
                self.basketItems(data.basketItems);
                self.promotionalMessage(data.promotionalMessage);
                self.summaryMessage(data.summaryMessage);
                self.itemsInBasket(data.summaryTotal);
                self.busy(false);

                if (window.hasOwnProperty('basketMaintenanceModel')) {
                    window.basketMaintenanceModel.updateBasket(null, true);
                }
            })
            .catch(err => {
                self.busy(false);
                window.catchFetchErrors(err);
            });
    }
    // very handy toggle scrapbook functionality
    self.toggleScrapbook = (i, e) => {
        console.trace(i, e);
        let target = e.currentTarget;
        let payload = JSON.stringify({
            imageId: i.ImageID,
            productPageId: i.ProductPageID,
            countryId: getCountryId(),
            languageId: getLanguageId()
        });
        fetch('/api/member/toggleScrapbook', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-store',
            credentials: 'same-origin',
            body: payload
        })
            .then(async (response) => {
                return await window.handleFetchResponse(response, payload);
            })
            .then((reply) => {
                console.trace('toggleScrapbook returned', reply, target);
                self.scrapbookPopover(reply.addition, target);
            })
            .catch(window.catchFetchErrors);
    }

    // Default prevention for bootstrap popover utility, see scrapbookPopover
    self.preventPop = function (e) {
        e.preventDefault();
    }

    // Scrapbook popover function: used when toggling images into and out of the scrapbook
    self.scrapbookPopover = (addition, target) => {
        target.removeEventListener('show.bs.popover', self.preventPop);
        let pop = bootstrap.Popover.getInstance(target);
        if (pop)
            pop.dispose();
        if (addition) {
            let pop = new bootstrap.Popover(target, {
                trigger: 'focus',
                boundary: target.parentElement,
                container: target.parentElement,
                html: true,
                placement: 'bottom',
                content: document.querySelector('#add-to-scrapbook-content').innerHTML,
                template: document.querySelector('#toggle-scrapbook-template').innerHTML
            });
            pop.show();
            target.addEventListener('show.bs.popover', self.preventPop);
            target.classList.add('transitioning');
            setTimeout(() => {
                target.querySelector('.bi').classList.add('bi-bookmark-heart');
                target.querySelector('.bi').classList.remove('bi-bookmark-plus');
            }, 300);
            setTimeout(() => {
                target.classList.remove('transitioning');
            }, 600);
        } else {
            let pop = new bootstrap.Popover(target, {
                trigger: 'focus',
                boundary: target.parentElement,
                container: target.parentElement,
                html: true,
                placement: 'bottom',
                content: document.querySelector('#removed-from-scrapbook-content').innerHTML,
                template: document.querySelector('#toggle-scrapbook-template').innerHTML
            });
            pop.show();
            target.addEventListener('show.bs.popover', self.preventPop);
            target.classList.add('transitioning');
            setTimeout(() => {
                target.querySelector('.bi').classList.remove('bi-bookmark-heart');
                target.querySelector('.bi').classList.add('bi-bookmark-plus');
            }, 300);
            setTimeout(() => {
                target.classList.remove('transitioning');
            }, 600);
        }
    }


    // Fired from the desktop basket menu item in #shortcuts-navbar: avoids unnecessary calls to refresh if nothing has changed since the last
    self.showBasketSummary = () => {
        let menuElement = document.querySelector('#basket-menu');
        if (menuElement) {
            if (self.basketItems().length == 0) {
                self.refreshBasket();
            } else {
                // this is only called onmouseenter #basket-menu, so if we don't need to refresh the basket we can just show the dropdown
                menuElement.classList.add('open');
            }
        }
    }

    // Hide: verb. The opposite of show.
    self.hideBasketSummary = () => {
        let ele = document.querySelector('#basket-menu');
        if (ele != null) {
            ele.classList.remove('open');
        }
    }

    self.backInStockReminder = (product) => {
        fetch('/api/member/createBackInStockReminder', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-store',
            credentials: 'same-origin',
            body: JSON.stringify({
                productId: product.ProductID ?? product.productId,
                countryId: getCountryId(),
                languageId: getLanguageId()
            })
        })
            .then(window.handleFetchResponse)
            .then(data => {
                if (data.success) {
                    window.showBackInStockSuccess();
                } else {
                    if (data.mustLogin) {
                        window.showBackInStockLogin();
                    } else {
                        showError();
                    }
                }
            })
            .catch(window.catchFetchErrors)
    }

    //#endregion

    return self;
});

window.basketModel = basketViewModel(getPrefetchedResources());

if (document.querySelector('#navbar-basket')) {
    ko.cleanNode(document.querySelector('#navbar-basket'));
    ko.applyBindings(window.basketModel, document.querySelector('#navbar-basket'));
}

if (document.querySelector('#basket-menu')) {
    ko.cleanNode(document.querySelector('#basket-menu'));
    ko.applyBindings(window.basketModel, document.querySelector('#basket-menu'));
}

if (document.querySelector('#basket-menu-item')) {
    ko.cleanNode(document.querySelector('#basket-menu-item'));
    ko.applyBindings(window.basketModel, document.querySelector('#basket-menu-item'));
}

if (typeof window.onBasketLoad === 'function') {
    window.onBasketLoad();
}