const productPagePodViewModel = ((productPageGrid) => {
    let self = {
        parent: productPageGrid
    };

    self.data = ko.observable();

    let dataSub = self.data.subscribe((newValue) => {
        Object.assign(self, newValue);
        dataSub.dispose();
    });

    return self;
});