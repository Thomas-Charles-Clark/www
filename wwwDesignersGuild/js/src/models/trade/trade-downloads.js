const tradeDownloadsViewModel = ((resources) => {
    let self = {
        resources: resources
    };

    //#region Properties

    self.ready = ko.observable(false);

    //#endregion


    //#region Components



    //#endregion


    //#region Subscriptions



    //#endregion


    //#region Computeds



    //#endregion


    //#region Methods



    //#endregion

    return self;
});

window.tradeDownloadsModel = tradeDownloadsViewModel(getPrefetchedResources());

ko.applyBindings(window.tradeDownloadsModel, document.querySelector('#trade-wrapper'));

window.tradeDownloadsModel.ready(true);