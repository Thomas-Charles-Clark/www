const tradePricingChoiceViewModel = (() => {
    let self = {

    };

    self.busy = ko.observable(false);

    self.acknowledged = ko.observable(false);

    self.choice = ko.observable(getTradePriceDisplay());

    self.choice.subscribe(function (newValue) {

        document.querySelector('.trade-pricing-save.retail-pricing').classList.remove('successful');
        document.querySelector('.trade-pricing-save.trade-pricing').classList.remove('successful');

        self.busy(true);

        var data = {
            displayTradePrice: self.choice()
        }

        fetch('/api/tradeapi/setTradePricingDisplay', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-store',
            credentials: 'same-origin',
            body: JSON.stringify(data)
        }).then(window.handleFetchResponse)
            .then(data => {
                self.busy(false);
                switch (self.choice()) {
                    case true:
                        document.querySelector('.trade-pricing-save.trade-pricing').classList.add('successful');
                        window.setTimeout(function () {
                            document.querySelector('.trade-pricing-save.trade-pricing').classList.remove('successful');
                        }, 5000);
                        break;
                    case false:
                        if (document.querySelector('.trade-clearance-menu-item'))
                            document.querySelector('.trade-clearance-menu-item').remove();
                        document.querySelector('.trade-pricing-save.retail-pricing').classList.add('successful');
                        window.setTimeout(function () {
                            document.querySelector('.trade-pricing-save.retail-pricing').classList.remove('successful');
                        }, 5000);
                        break;
                    default:
                        break;
                }
            })
            .catch(err => {
                self.busy(false);
                window.showError();
            })
    });

    return self;
});

window.tradePricingChoiceModel = tradePricingChoiceViewModel();
ko.applyBindings(window.tradePricingChoiceModel, document.querySelector('#trade-wrapper'));