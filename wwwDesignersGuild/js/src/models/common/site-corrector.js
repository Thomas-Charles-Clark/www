const siteCorrectorViewModel = ((resources) => {
    let self = {
        resources: resources
    };

    //#region Properties

    self.siteSelection = ko.observable();

    self.siteSelection.DOMElement = document.querySelector('#site-corrector');

    self.bodyInnerElement = document.querySelector('#body-inner');

    //#endregion


    //#region Components



    //#endregion


    //#region Subscriptions



    //#endregion


    //#region Computeds



    //#endregion


    //#region Methods

    self.changeSite = () => {
        let regex = new RegExp(`(${window.location.protocol}\/\/${window.location.host}\/)([a-z]*)(\/.*)?$`);

        if (self.siteSelection() == 'country-picker') {
            window.location.href = `${window.location.protocol}//${window.location.host}/l50`;
            return;
        }

        let results = regex.exec(window.location.href);

        let destination = '';
        
        if (results.length == 4) {
            destination = window.location.href.replace(regex, `$1${self.siteSelection()}$3`);
        } else if (results.length == 3) {
            destination = window.location.href.replace(regex, `$1${self.siteSelection()}$2`);
        }

        window.location.href = destination;
    }

    self.dismiss = () => {
        fetch('/api/generalapi/CountryCorrectorDismissed', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-cache',
            credentials: 'same-origin'
        });

        self.bodyInnerElement.style.paddingTop = '0px';
        self.siteSelection.DOMElement.classList.toggle('dismissed');
        self.bodyInnerElement.classList.toggle('site-corrector-dismissed');
    }

    self.init = () => {
        self.bodyInnerElement.style.paddingTop = window.getComputedStyle(self.siteSelection.DOMElement).height;

        window.addLoadFunction(() => {
            window.setTimeout(() => {
                window.scrollTo(0, 0);
                window.requestAnimationFrame(() => {
                    self.bodyInnerElement.style.transition = 'padding-top 500ms ease-in-out'
                });
            }, 0);
        });
    }

    //#endregion

    return self;
});
window.siteCorrectorModel = siteCorrectorViewModel();
window.siteCorrectorModel.init();
ko.applyBindings(window.siteCorrectorModel, document.querySelector('#site-corrector'));