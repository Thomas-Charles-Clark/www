const largeImageModalViewModel = (() => {
    let self = {};

    self.carouselId = 'large-image-carousel';

    document.getElementById('large-image-modal').addEventListener('shown.bs.modal', () => {
        setTimeout(() => {
            window.lazyImages();
        }, 0);
    });

    self.title = ko.observable();

    self.carousel = ko.observable();

    self.images = ko.observableArray([]);

    self.modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('large-image-modal'));

    self.showModal = () => {
        self.modal.show();
    }

    self.hideModal = () => {
        self.modal.hide();
    }

    self.load = (data, event) => {
        if (data.hasOwnProperty('Definition') && data.Definition.heading) {
            self.title(data.Definition.heading);
        } else if (data.hasOwnProperty('MainProduct')) {
            self.title(`${data.MainProduct.Product.name} - ${data.MainProduct.Product.code}`);
        } else if (data.hasOwnProperty('Product')) {
            self.title(`${data.Product.name} - ${data.Product.code}`);
        } else {
            console.trace(event); 
            if (event.target.nodeName === 'IMG') {

                let imageModel = imageViewModel(null);

                //imageModel.htmlSectionImage = event.target.outerHTML.replace(/data-bind=".*"/, '');
                imageModel.htmlSectionImage = event.target.outerHTML.replace(/data-bind="[^"]*"/, '')
                    .replace(/srcset="[^"]*"/, '')
                    .replace(/sizes="[^"]*"/, '');

                imageModel.htmlSectionImage = imageModel.htmlSectionImage.replace('thumb', '').replace(/image\/\d*\//, 'image/1024/');
                imageModel.htmlSectionImage = imageModel.htmlSectionImage.replace('thumb', '').replace(/image-nonwebp\/\d*\//, 'image-nonwebp/1024/');

                self.title(event.target.alt);

                self.images([imageModel]);
            }
        }
        if (data.hasOwnProperty('Carousel')) {
            // good!
            self.carousel(data.Carousel);
            self.images(data.Carousel.Images.map(i => {
                let parentElement = data.images().filter(im => im.ImageID == i.ImageID)[0].data.DOMElement.parentElement;
                let result = imageViewModel(null);
                result = Object.assign(result, i);
                result.hotspots(data.Hotspots.flatMap(h => h.ImageID == i.ImageID ? h.Hotspots : []));
                result.hotspots().forEach(h => {
                    let ofg = orderFormGroupViewModel();

                    let product = h.Product.Product;

                    let products = [];

                    let mainProduct = productViewModel(product.productId, 1);

                    if (product.controls.allowSample) {
                        let sample = productViewModel(product.productId, 1);
                        sample.data(product.sample);
                        sample.orderFormTypeId = 0;
                        sample.quantity(sample.stockMessage.quantityMinimum);
                        products.push(sample);
                    }

                    if (product.controls.allowLargeSample) {
                        let largeSample = productViewModel(product.productId, 1);
                        largeSample.data(product.largeSample);
                        largeSample.orderFormTypeId = 1;
                        largeSample.quantity(largeSample.stockMessage.quantityMinimum);
                        products.push(largeSample);
                    }

                    if (product.controls.allowOrder) {
                        let order = productViewModel(product.productId, 1);
                        order.data(product);
                        order.orderFormTypeId = 2;
                        order.quantity(order.stockMessage.quantityMinimum);
                        products.push(order);
                    }

                    if (product.controls.allowReserve) {
                        let reserve = productViewModel(product.productId, 2);
                        reserve.data(product);
                        reserve.orderFormTypeId = 3;
                        reserve.quantity(reserve.stockMessage.quantityMinimum);
                        products.push(reserve);
                    }

                    if (product.controls.allowReserveWithBatchSample) {
                        let reserveWithBatch = productViewModel(product.productId, 3);
                        reserveWithBatch.data(product);
                        reserveWithBatch.orderFormTypeId = 4;
                        reserveWithBatch.quantity(reserveWithBatch.stockMessage.quantityMinimum);
                        products.push(reserveWithBatch);
                    }

                    mainProduct.data(product);

                    mainProduct.products(products);

                    ofg.products([mainProduct]);
                    ofg.markup = parentElement.querySelector(`.hotspot-marker #product-${product.productId}`).closest('.hotspot-marker').outerHTML;
                    ofg.markup = ofg.markup.replace(`product-${product.productId}`, `product-hotspot-${product.productId}`);
                    result.orderFormGroups.push(ofg);
                });
                //let result = Object.assign({}, i);
                //result.Hotspots = data.Hotspots.flatMap(h => h.ImageID == i.ImageID ? h.Hotspots : [])
                //result.Hotspots.forEach(h => {
                //    h.orderFormGroup = orderFormGroupViewModel();
                //});
                return result;
            }));
        }
        self.showModal();
        window.setTimeout(() => {
            window.lazyImages();
        }, 100);
    }

    return self;
});

window.largeImageModal = largeImageModalViewModel();

if (document.querySelector('#large-image-modal')) {
    ko.applyBindings(window.largeImageModal, document.querySelector('#large-image-modal'));
}