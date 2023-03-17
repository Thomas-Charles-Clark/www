const collectionPageViewModel = (() => {
    let self = {};

    self.data = ko.observable();

    let dataSub = self.data.subscribe(function (newValue) {
        Object.assign(self, newValue);
        dataSub.dispose();
    });

    self.products = ko.observableArray([]);

    self.createImage = function () {
        return imageViewModel(self.Definition.productPageId);
    }

    self.images = ko.observableArray([]);

    return self;
});

var collectionModel = collectionPageViewModel();
ko.applyBindings(collectionModel, document.querySelector('#ko-wrapper'));