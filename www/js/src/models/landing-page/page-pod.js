const pagePodViewModel = ((collectionGrid) => {
    let self = {
        parent: collectionGrid
    };

    self.data = ko.observable();

    let dataSub = self.data.subscribe((newValue) => {
        Object.assign(self, newValue);
        dataSub.dispose();
    });

    self.images = ko.observableArray([]);

    self.createImage = () => imageViewModel();

    return self;
});