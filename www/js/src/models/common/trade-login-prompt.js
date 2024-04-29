const tradeLoginPromptViewModel = ((resources) => {
    let self = {
        resources: resources
    };

    //#region Properties

    self.loginPrompt = ko.observable();

    self.loginPrompt.DOMElement = document.querySelector('#trade-login-prompt');

    self.bodyInnerElement = document.querySelector('#body-inner');

    //#endregion


    //#region Components



    //#endregion


    //#region Subscriptions



    //#endregion


    //#region Computeds



    //#endregion


    //#region Methods

    self.hide = function () {

        self.loginPrompt.DOMElement.classList.remove('open');

        // get the height of the element's inner content, regardless of its actual size
        let sectionHeight = self.loginPrompt.DOMElement.scrollHeight;

        // temporarily disable all css transitions
        let elementTransition = self.loginPrompt.DOMElement.style.transition;
        self.loginPrompt.DOMElement.style.transition = '';

        // on the next frame (as soon as the previous style change has taken effect),
        // explicitly set the element's height to its current pixel height, so we 
        // aren't transitioning out of 'auto'
        requestAnimationFrame(function () {
            self.loginPrompt.DOMElement.style.height = sectionHeight + 'px';
            self.loginPrompt.DOMElement.style.transition = elementTransition;

            // on the next frame (as soon as the previous style change has taken effect),
            // have the element transition to height: 0
            requestAnimationFrame(function () {
                self.loginPrompt.DOMElement.style.height = 0 + 'px';
            });
        });

        // mark the section as "currently collapsed"
        self.loginPrompt.DOMElement.setAttribute('data-collapsed', 'true');

        self.dismiss();
    }

    self.hideTrade = function () {
        self.loginPrompt.DOMElement.classList.remove('open');

        // get the height of the element's inner content, regardless of its actual size
        let sectionHeight = self.loginPrompt.DOMElement.scrollHeight;

        // temporarily disable all css transitions
        let elementTransition = self.loginPrompt.DOMElement.style.transition;
        self.loginPrompt.DOMElement.style.transition = '';

        // on the next frame (as soon as the previous style change has taken effect),
        // explicitly set the element's height to its current pixel height, so we 
        // aren't transitioning out of 'auto'
        requestAnimationFrame(function () {
            self.loginPrompt.DOMElement.style.height = sectionHeight + 'px';
            self.loginPrompt.DOMElement.style.transition = elementTransition;

            // on the next frame (as soon as the previous style change has taken effect),
            // have the element transition to height: 0
            requestAnimationFrame(function () {
                self.loginPrompt.DOMElement.style.height = 0 + 'px';
            });
        });

        // mark the section as "currently collapsed"
        self.loginPrompt.DOMElement.setAttribute('data-collapsed', 'true');

    }

    self.toggle = function (ev) {

        if (self.loginPrompt.DOMElement.classList.contains('open'))
            self.hide();
        else
            self.show();

        ev.stopPropagation();

        return true;
    }

    self.transitionEnd = function (e) {
        var element = self.loginPrompt.DOMElement;

        // remove this event listener so it only gets triggered once
        element.removeEventListener('transitionend', self.transitionEnd);

        // remove "height" from the element's inline styles, so it can return to its initial value
        element.style.height = 'auto';
    }

    self.show = function () {
        self.loginPrompt.DOMElement.classList.add('open');

        var element = self.loginPrompt.DOMElement;

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
        fetch('/api/generalapi/TradeLoginPromptDismissed', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-cache',
            credentials: 'same-origin'
        });
    }

    self.init = () => {
        //self.bodyInnerElement.style.paddingTop = window.getComputedStyle(self.loginPrompt.DOMElement).height;

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
window.tradeLoginPromptModel = tradeLoginPromptViewModel();
window.tradeLoginPromptModel.init();
ko.applyBindings(window.tradeLoginPromptModel, document.querySelector('#trade-login-prompt'));