const articlePageViewModel = ((resources) => {
    let self = {
        resources: resources
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
let articleModel = articlePageViewModel({});
ko.applyBindings(articleModel, document.querySelector('#article-main'))