const checkoutConfirmViewModel = ({ parent } = { parent: {} }) => {
    let self = Object.assign({}, {
        parent
    });

    //#region Properties

    self.busy = ko.observable(false);

    self.signupSuccess = ko.observable();

    //#endregion


    //#region Components



    //#endregion


    //#region Subscriptions



    //#endregion


    //#region Computeds



    //#endregion


    //#region Methods

    self.submitSignup = () => {
        self.busy(true); 

        fetch("/api/form/RejoinMarketingListReceipt", {
            method: "POST",
            headers: window.fetchHeaders,
            cache: 'no-cache',
            credentials: 'same-origin',
            body: JSON.stringify({
                email: getCheckoutEmail(),
                countryId: getCountryId(),
                languageId: getLanguageId(),
                url: window.location.href
            })
        }).then(window.handleFetchResponse)
            .then(data => {
                //fetchResources([
                //    'CheckoutResubscribeNotice',
                //    'CheckoutResubscribeFooter'])
                //    .then(window.handleResourcesArray)
                //    .then(resources => {
                        
                //    });

                window.setTimeout(() => {
                    document.querySelector('#checkout-hero').classList.add('alert');
                    document.querySelector('#checkout-hero').classList.add('alert-success');
                    document.querySelector('#checkout-hero').innerHTML = getThanksHtml();
                    self.busy(false);

                    self.signupSuccess(data);
                }, 1000)
            })
            .catch(err => {
                self.busy(false);
                window.catchFetchErrors(err);
            })
    }

    //#endregion


    //#region Events



    //#endregion

    return self;
};