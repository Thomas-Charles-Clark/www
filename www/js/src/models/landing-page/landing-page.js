const landingPageViewModel = ((resources) => {
    let self = {
        resources: resources
    };

    self.data = ko.observable();

    let dataSub = self.data.subscribe((newValue) => {
        Object.assign(self, newValue);
        dataSub.dispose();
    });

    self.createImage = () => imageViewModel(self.Definition.productPageId);
    
    self.images = ko.observableArray([]);

    self.createPageSection = () => pageSectionViewModel();

    self.pageSections = [];


    //#region Fixed Page

    self.fixedPage = ko.observable();

    self.fixedPage.subscribe(nv => {
        if (typeof window.onFixedPageLoad === 'function') {
            window.onFixedPageLoad();
        }
    })

    //#endregion

    return self;
});

window.pageModel = landingPageViewModel(getPrefetchedResources());
ko.applyBindings(window.pageModel, document.querySelector('#ko-wrapper'));