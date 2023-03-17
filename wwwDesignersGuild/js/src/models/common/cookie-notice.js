const cookieNoticeViewModel = ((parent) => {
    let self = {
        parent: parent
    };

    //#region Properties

    self.visible = ko.observable(false);

    //#endregion


    //#region Components



    //#endregion


    //#region Subscriptions

    //self.visible.subscribe(nv => {
    //    console.log(nv);
    //});

    //#endregion


    //#region Computeds



    //#endregion


    //#region Methods

    self.show = () => {
        self.visible(true);
    }

    self.hide = () => {
        self.visible(false);
    }

    self.init = () => {
        self.show();
    }

    self.accept = () => {
        document.cookie = `cookie-notice=done; expires=Fri, 31 Dec 9999 23:59:59 GMT; SameSite=Lax; Secure; Path=/`;
        self.hide();
    }

    //#endregion

    return self;
});

window.addLoadFunction(() => {
    if (document.cookie.split('; ').find(row => row.startsWith('cookie-notice='))?.split('=')[1] == null) {
        window.cookieNoticeModel = cookieNoticeViewModel();
        ko.applyBindings(window.cookieNoticeModel, document.querySelector('#cookie-notice'));
        // this 200ms delay is to permit my toggleOpacity bindingHandler to complete its setTimeout 150. See
        // knockout-extensions.js:121
        window.setTimeout(() => {
            window.cookieNoticeModel.init();
        }, 200);
    }
});