const pageGridViewModel = ((pageSection) => {
    let self = {
        parent: pageSection
    };

    self.visiblePages = ko.observableArray([]);

    self.createPagePod = () => pagePodViewModel();

    return self;
});