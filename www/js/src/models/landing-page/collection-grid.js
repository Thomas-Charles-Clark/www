const collectionGridViewModel = ((pageSection) => {
    let self = {
        parent: pageSection
    };

    self.visibleCollections = ko.observableArray([]);

    self.createCollectionPod = () => collectionPodViewModel();

    return self;
});