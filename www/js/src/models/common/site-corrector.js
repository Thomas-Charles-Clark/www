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
        let regex = new RegExp(`(${window.location.protocol}\/\/${window.location.host}\/)([a-z\-]*)(\/.*)?$`);

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

    self.hide = function () {

        self.siteSelection.DOMElement.classList.remove('open');

        // get the height of the element's inner content, regardless of its actual size
        let sectionHeight = self.siteSelection.DOMElement.scrollHeight;

        // temporarily disable all css transitions
        let elementTransition = self.siteSelection.DOMElement.style.transition;
        self.siteSelection.DOMElement.style.transition = '';

        // on the next frame (as soon as the previous style change has taken effect),
        // explicitly set the element's height to its current pixel height, so we 
        // aren't transitioning out of 'auto'
        requestAnimationFrame(function () {
            self.siteSelection.DOMElement.style.height = sectionHeight + 'px';
            self.siteSelection.DOMElement.style.transition = elementTransition;

            // on the next frame (as soon as the previous style change has taken effect),
            // have the element transition to height: 0
            requestAnimationFrame(function () {
                self.siteSelection.DOMElement.style.height = 0 + 'px';
            });
        });

        // mark the section as "currently collapsed"
        self.siteSelection.DOMElement.setAttribute('data-collapsed', 'true');

        self.dismiss();
    }

    self.hideTrade = function () {
        self.siteSelection.DOMElement.classList.remove('open');

        // get the height of the element's inner content, regardless of its actual size
        let sectionHeight = self.siteSelection.DOMElement.scrollHeight;

        // temporarily disable all css transitions
        let elementTransition = self.siteSelection.DOMElement.style.transition;
        self.siteSelection.DOMElement.style.transition = '';

        // on the next frame (as soon as the previous style change has taken effect),
        // explicitly set the element's height to its current pixel height, so we 
        // aren't transitioning out of 'auto'
        requestAnimationFrame(function () {
            self.siteSelection.DOMElement.style.height = sectionHeight + 'px';
            self.siteSelection.DOMElement.style.transition = elementTransition;

            // on the next frame (as soon as the previous style change has taken effect),
            // have the element transition to height: 0
            requestAnimationFrame(function () {
                self.siteSelection.DOMElement.style.height = 0 + 'px';
            });
        });

        // mark the section as "currently collapsed"
        self.siteSelection.DOMElement.setAttribute('data-collapsed', 'true');

    }

    self.toggle = function (ev) {

        if (self.siteSelection.DOMElement.classList.contains('open'))
            self.hide();
        else
            self.show();

        ev.stopPropagation();

        return true;
    }

    self.transitionEnd = function (e) {
        var element = self.siteSelection.DOMElement;

        // remove this event listener so it only gets triggered once
        element.removeEventListener('transitionend', self.transitionEnd);

        // remove "height" from the element's inline styles, so it can return to its initial value
        element.style.height = 'auto';
    }

    self.show = function () {
        self.siteSelection.DOMElement.classList.add('open');

        var element = self.siteSelection.DOMElement;

        // get the height of the element's inner content, regardless of its actual size
        var sectionHeight = element.scrollHeight;

        // have the element transition to the height of its inner content
        element.style.height = sectionHeight + 'px';

        // when the next css transition finishes (which should be the one we just triggered)
        element.addEventListener('transitionend', self.transitionEnd);

        // mark the section as "currently not collapsed"
        element.setAttribute('data-collapsed', 'false');
    }

    self.dismiss = () => {
        fetch('/api/generalapi/CountryCorrectorDismissed', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-cache',
            credentials: 'same-origin'
        });
    }

    self.init = () => {
        //self.bodyInnerElement.style.paddingTop = window.getComputedStyle(self.siteSelection.DOMElement).height;

        //window.addLoadFunction(() => {
        //    window.setTimeout(() => {
        //        window.scrollTo(0, 0);
        //        window.requestAnimationFrame(() => {
        //            self.bodyInnerElement.style.transition = 'padding-top 500ms ease-in-out'
        //        });
        //    }, 0);
        //});

        self.show();
        window.setTimeout(() => {
            window.scrollTo({ top: 0, left: 0, behavior: 'auto' });
        }, 300);
    }

    //#endregion

    return self;
});
window.siteCorrectorModel = siteCorrectorViewModel();
window.siteCorrectorModel.init();
ko.applyBindings(window.siteCorrectorModel, document.querySelector('#site-corrector'));