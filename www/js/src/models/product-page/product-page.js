const productPageViewModel = ((resources) => {
    let self = {
        resources: resources
    };

    //#region Properties

    // Prerendered data binding point
    self.data = ko.observable();

    // main product page imagery: carousel, thumbnails
    self.images = ko.observableArray([]);

    // order form groups
    self.orderFormGroups = ko.observableArray([]);

    // html section - this holds the product type specific html section thing
    self.html = htmlViewModel(self);

    //#endregion


    //#region Components

    // stock info
    self.stockInfo = stockInfoViewModel(self);

    //#endregion


    //#region Subscriptions

    let dataSub = self.data.subscribe(function (newValue) {
        Object.assign(self, newValue);
        //self.jsonLd();
        self.thirdParty();
        dataSub.dispose();
        if (typeof window.lazyImages === 'function')
            window.lazyImages();
    });

    //#endregion


    //#region Computeds



    //#endregion


    //#region Methods

    // helper function for prerendered binding handler to know how to create an image object
    self.createImage = () => {
        return imageViewModel(self.Definition.productPageId);
    }

    // another prerendered binding handler helper function
    self.createOrderFormGroup = () => orderFormGroupViewModel(self);

    // This is now rendered server side
    //
    //self.jsonLd = () => {
    //    var permittedTemplates = [3, 4, 5, 6, 9];
    //    if (permittedTemplates.indexOf(self.Definition.templateId) > -1) {
    //        let products = self.AllProducts.map(p => {
    //            return {
    //                name: p.OrderFormName,
    //                description: self.Definition.json.description,
    //                imageId: p.Image.imageId,
    //                brand: p.Product.brand,
    //                code: p.Product.code,
    //                gtin13: p.Product.gtin13,
    //                reviews: self.Definition.json.reviews == null
    //                    ? []
    //                    : self.Definition.json.reviews.flatMap(r =>
    //                        r.productId == p.Product.productId ? [{ rating: r.rating, review: r.review }] : []
    //                    ),
    //                displayPrice: p.Product.displayPrice,
    //                displaySalePrice: p.Product.displaySalePrice,
    //                itemAvailability: p.Product.stockMessage?.basketQuantityThresholds[0]?.stockMessages[0]?.stockMessage,
    //                url: `${window.location.origin}${p.Product.url}`,
    //                rrp: p.Product.rRPMessage
    //            }
    //        })
    //        let data = {
    //            type: 'PrimaryProducts',
    //            description: self.Definition.json.description,
    //            json: btoa(unescape(encodeURIComponent(JSON.stringify(products)))),
    //            currency: getCurrencyCode()
    //        };
    //        fetch('/api/generalapi/jsonld', {
    //            method: 'POST',
    //            headers: window.fetchHeaders,
    //            cache: 'no-store',
    //            credentials: 'same-origin',
    //            body: JSON.stringify(data)
    //        }).then(window.handleFetchResponse)
//            .then(data => {
//                //console.trace(data);
//                let script = document.createElement('script');
//                script.type = 'application/ld+json';
//                script.text = data.jsonLd;
//                document.querySelector('head').appendChild(script);
//                script.insertAdjacentHTML('afterend', data.unfurling);
//            })
    //    }
    //}

    self.thirdParty = () => {
        if (typeof fbq === 'function' &&
            self.Definition &&
            self.AllProducts) {
            fbq('track', 'ViewContent', {
                content_name: self.Definition.Heading,
                content_type: 'product',
                content_ids: self.AllProducts.map(p => p.ProductCode)
            })
        }
    }

    //#endregion

    //#region Template specific stuff

    //#region Paint related stuff

    // Just don't, okay...? The two calls to foreachInit against orderFormGroups was causing double event handler bindings.
    // Add n to basket == add n*2 to basket!
    self.samplePotOrderFormGroups = ko.observableArray([]);
 
    self.allSurfaces = ko.observableArray([]);

    self.availableSurfaces = ko.pureComputed(() => self.allSurfaces(), self);

    self.surface = ko.observable();

    self.surfaceEnabled = ko.pureComputed(() => {
        return self.availableSurfaces().length > 0;
    }, self);

    self.allFinishes = ko.observableArray([]);

    self.availableFinishes = ko.pureComputed(() => {
        if (self.surface()) {
            let attributeIds = self.AllProducts.flatMap(p => {
                return p.Attributes.flatMap(a => a.AttributeID).indexOf(self.surface()) > -1 ? p.Attributes.flatMap(a => a.AttributeID) : []
            });

            return self.allFinishes().flatMap(f => {
                return attributeIds.indexOf(f.attributeId()) > -1 ? [f] : [];
            });
        }
        return [];
    }, self);

    self.finishEnabled = ko.pureComputed(() => {
        return self.availableFinishes().length > 0;
    }, self);

    self.finish = ko.observable();

    self.allSizes = ko.observableArray([]);

    self.availableSizes = ko.pureComputed(() => {
        if (self.finish()) {

            let attributeIds = self.AllProducts.flatMap(p => {
                return p.Attributes.flatMap(a => a.AttributeID).indexOf(self.finish()) > -1 ? p.Attributes.flatMap(a => a.AttributeID) : []
            });

            if (attributeIds.length) {
                return self.allSizes().flatMap(f => {
                    return attributeIds.indexOf(f.attributeId()) > -1 ? [f] : []
                });
            }
        }

        return [];
    }, self);

    self.sizeEnabled = ko.pureComputed(() => {
        return self.availableSizes().length > 0;
    }, self);

    self.size = ko.observable();

    self.visiblePaintProductIds = ko.pureComputed(() => {
        if (self.surface() &&
            self.finish() &&
            self.size()) {

            let productIds = self.AllProducts.flatMap(p => {
                let attributes = p.Attributes.flatMap(a => a.AttributeID);
                return (attributes.indexOf(self.surface()) > -1 &&
                    attributes.indexOf(self.finish()) > -1 &&
                    attributes.indexOf(self.size()) > -1)
                    ? p.ProductID
                    : [];
            });

            return productIds;
        }

        return [];
    }, self);

    //#endregion

    //#region Paint & Wallpaper related stuff

    self.coverageCalculator = coverageCalculatorViewModel(self);

    //#endregion

    //#region modular furniture related stuff

    self.moduleResult = ko.observable();

    self.moduleBusy = ko.observable(false);

    self.fabricRenderResult = ko.observable();

    self.modularFurnitureModuleSelected = (productPageId, fabricProductPageId) => {

        let params = {
            productPageId: productPageId,
            fabricProductPageId: fabricProductPageId,
            languageId: getLanguageId(),
            countryId: getCountryId(),
            masterDefinition: getMasterDefinition()
        };

        console.log("fetchModuleDetail calling with", params);

        self.moduleBusy(true);

        fetch('/api/productpageapi/fetchModuleDetail', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-store',
            credentials: 'same-origin',
            body: JSON.stringify(params)
        })
            .then(window.handleFetchResponse)
            .then(data => {
                console.log("fetchModuleDetail returned", data);

                self.moduleBusy(false);

                self.moduleResult(data.furnitureOrderForm.orderForm.products[0].product);

                self.fabricRenderResult(data.render);

                if (data.render) {

                    window.basketModel.refreshBasket();

                    window.setTimeout(() => {

                        document.querySelector(".module-option-preview .carousel-item.active .module-option-order-form")?.remove();

                        document.querySelectorAll(".basket-samples-panel").forEach(e => e.remove());

                        let moduleOptionGroupElement = document.createElement('div');

                        moduleOptionGroupElement.className = "module-option-order-form";

                        let orderFormDetailsElement = document.createElement('div');

                        orderFormDetailsElement.className = "order-form-details";

                        orderFormDetailsElement.setAttribute("data-bind", "html: fabricRenderResult, stopBinding");

                        moduleOptionGroupElement.appendChild(orderFormDetailsElement);

                        let basketSamplesPanel = document.createElement("div");

                        basketSamplesPanel.className = "basket-samples-panel";

                        basketSamplesPanel.setAttribute("data-bind", "template: { name: 'basket-samples-panel-template', data: window.basketModel }");

                        moduleOptionGroupElement.appendChild(basketSamplesPanel);

                        let basketAnchor = document.createElement('a');

                        basketAnchor.setAttribute("href", "/l201");

                        basketAnchor.setAttribute("target", "self");

                        basketAnchor.className = "btn btn-dark w-100 mt-5 mb-3";

                        basketAnchor.innerHTML = self.resources.ViewBasket;

                        moduleOptionGroupElement.appendChild(basketAnchor);

                        let closeBtn = document.createElement('button');

                        closeBtn.setAttribute("type", "button");

                        closeBtn.className = "btn btn-secondary w-100";

                        closeBtn.innerHTML = self.resources.CloseAndReturnToProductPage;

                        closeBtn.setAttribute("data-bs-dismiss", "modal");

                        moduleOptionGroupElement.appendChild(closeBtn);

                        document.querySelector(".module-option-preview .carousel-item.active")?.appendChild(moduleOptionGroupElement);

                        ko.applyBindings(productPageModel, moduleOptionGroupElement)

                        ko.cleanNode(moduleOptionGroupElement);

                        orderFormDetailsElement.removeAttribute("data-bind");

                        ko.applyBindings(orderFormGroupViewModel(self), orderFormDetailsElement);

                        ko.applyBindings(window.basketModel, basketSamplesPanel);

                    }, 0);
                }

                if (window.isMobileSite()) {
                    document.querySelector('.module-option-modal.fade.show').scrollTo({
                        top: 0,
                        left: 0,
                        behavior: 'smooth'
                    });
                }
            })
            .catch(err => {
                self.moduleBusy(false);
            })

        return true;
    }

    //#endregion
    

    //#endregion

    self.init = () => {
        const search = window.location.search;
        const params = new URLSearchParams(search);
        if (params.has('info')) {
            window.addLoadFunction(() => {
                document.querySelector('#information-panel-heading button').click();
            });
        }
    }

    self.init();

    return self;
});

// Does what it says on the tin. A mini attribute.
const miniAttribute = (() => {
    let self = {};

    self.attributeId = ko.observable();

    self.name = ko.observable();

    return self;
});

window.productPageModel = productPageViewModel(getPrefetchedResources());
ko.applyBindings(window.productPageModel, document.querySelector('#ko-wrapper'));