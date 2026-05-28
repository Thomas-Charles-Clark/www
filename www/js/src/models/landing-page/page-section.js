const pageSectionViewModel = ((pageSectionType, resources, pageSectionId) => {
    let self = {
        type: pageSectionType,
        resources: resources,
        pageSectionId: pageSectionId
    };

    self.images = ko.observableArray([]);

    self.data = ko.observable();

    let dataSub = self.data.subscribe((newValue) => {
        Object.assign(self, newValue);
        dataSub.dispose();
    });

    self.createImage = function () {
        return imageViewModel();
    }

    switch (self.type) {
        case 'HTML':
            self.html = htmlViewModel(self);
            break;
        case 'Slideshow':
        case 'Animation':
            self.toggleCarousel = (data, event) => {
                let c = bootstrap.Carousel.getOrCreateInstance(self.images.DOMElement);
                if (!c.productsPaused) {
                    c.productsPaused = true;
                    clearTimeout(c.touchTimeout);
                    if (self.textTimeout)
                        clearTimeout(self.textTimeout);
                    event.target.innerHTML = self.resources.HideImageProducts;
                } else {
                    c.productsPaused = false;
                    c.cycle();
                    self.textTimeout = setTimeout(() => {
                        event.target.innerHTML = self.resources.ShowImageProducts;
                    }, 300)
                }
                //    c.cycle()
                //else
                //    c.pause()
            }
            break;
        case 'Form':
            self.form = formViewModel(self, resources);
            break;
        case 'ProductPageGrid':
            // This is necessary for product page grids doing massive index filtering
            self.PageSection = {
                PageSectionID: self.pageSectionId
            }
            self.productPageGrid = productPageGridViewModel(self);
            break;
        case 'CollectionGrid':
            self.collectionGrid = collectionGridViewModel(self);
            break;
        case 'PageGrid':
            self.pageGrid = pageGridViewModel(self);
            break;
        case 'Brochure':
            break;
        case 'BannerLocationGroup':
            self.bannerGrid = bannerGridViewModel(self);
            break;
        case 'ArticleGrid':
            self.articleGrid = articleGridViewModel(self);
            break;
        case 'Video':
            break;
        case 'Masonry':
            break;
        case 'BespokeContent':
            break;

        // FixedPages
        case 'Login': {
                self.loginModel = loginViewModel(self);
            }
            break;
        case 'MemberAddressManagement':
            self.memberAddressModel = memberAddressManagementViewModel(self);
            break;
        case 'MemberLoginDetails':
            self.memberLoginModel = memberLoginDetailsViewModel(self);
            break;
        case 'MemberOrderHistory':
        case 'ReturnsPaperwork':
            self.memberOrderHistoryModel = memberOrderHistoryViewModel(self);
            break;
        case 'SearchResults': {
                // Necessary for the productPageRenderer computed to identify the correct form
                self.PageSection = {
                    PageSectionID: -1
                }
                self.productPageGrid = productPageGridViewModel(self);
            }
            break;
        case 'Basket': {
            self.basketMaintenanceModel = basketMaintenanceViewModel(self);
        }
            break;
        case 'CheckoutLogin': {
            if (typeof checkoutLoginViewModel === 'function') {
                self.checkoutLoginModel = checkoutLoginViewModel(self);
                document.querySelector('#body-content').dispatchEvent(new Event('checkoutLoginReady'));
            }
            window.afterLogin = () => {
                window.setTimeout(function () {
                    window.proceedToCheckout('Member');
                }, 0);
            }
        }
        break;
        case 'CheckoutConfirm': {
            self.checkoutConfirmViewModel = checkoutConfirmViewModel({
                parent: self
            });
        }
        break;
        case 'Stockists': {
            self.stockistsModel = stockistsViewModel(self);
        }
        break;
        case 'TradeAccountRequest': {
            self.tradeAccountRequestModel = window.tradeAccountRequestModel;
        }
        break;
    }

    return self;
});