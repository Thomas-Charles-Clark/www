const bannerGridViewModel = ((pageSection) => {
    let self = {
        parent: pageSection
    };

    self.visibleBanners = ko.observableArray([]);

    self.createBannerPod = () => bannerPodViewModel();

    return self;
});