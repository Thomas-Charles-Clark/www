const unsubscribeViewModel = ((resources) => {
    let self = {
        resources: resources
    };

    //#region Properties

    self.success = ko.observable(false);

    self.busy = ko.observable(false);

    self.validateEmailNow = ko.observable(false);

    self.email = {
        observable: ko.observable(getEmail()).extend({
            required: {
                params: true,
                onlyIf: () => {
                    return self.validateEmailNow();
                },
                message: self.resources.ManageAccountErrorEmail
            },
            email: {
                params: true,
                onlyIf: () => {
                    return self.validateEmailNow();
                },
                message: self.resources.ManageAccountErrorEmailFormat
            },
            minLength: {
                params: 5,
                onlyIf: () => {
                    return self.validateEmailNow();
                },
                message: self.resources.ManageAccountErrorEmailFormat
            },
        }),
        focus: ko.observable(false),
        charsRemainingOptions: {
            maxLength: 254
        },
        updated: ko.observable(false)
    };

    self.guid = ko.observable(getGuid());

    self.emailName = ko.observable(getEmailName());

    self.availableUnsubscribeReasons = ko.observableArray(getUnsubscribeReasons());

    self.unsubscribeReasonId = ko.observable();

    //#endregion


    //#region Components

    self.errors = new ko.validation.group([
        self.email.observable
    ]);

    //#endregion


    //#region Subscriptions



    //#endregion


    //#region Computeds



    //#endregion


    //#region Methods

    self.save = (onComplete, sources) => {
        self.validateEmailNow(false);
        self.validateEmailNow(true);
        self.errors.showAllMessages(false);
        if (self.errors().length) {
            self.errors.showAllMessages(true);
            self.errors.filter(x => {
                x.isValid.notifySubscribers(x.isValid());
            });
        } else {
            self.busy(true);
            let data = {
                emailName: self.emailName(),
                email: self.email.observable(),
                guid: self.guid(),
                reasonId: self.unsubscribeReasonId()
            };
            fetch('/api/form/UnsubscribeMember', {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-store',
                credentials: 'same-origin',
                body: JSON.stringify(data)
            })
                .then(window.handleFetchResponse)
                .then(response => {
                    if (response.error) {
                        window.showError();
                    } else {
                        self.success(true);
                    }
                    self.busy(false);
                })
                .catch(err => {
                    self.busy(false);
                    window.showError();
                })
        }
    }

    //#endregion

    return self;
});

fetchResources([
    'ManageAccountErrorEmail',
    'ManageAccountErrorEmailFormat',
    'FormsEmail',
    'PleaseSelect',
    'MajorError'
])
    .then(window.handleResourcesArray)
    .then(resources => {
        window.unsubscribeModel = unsubscribeViewModel(resources);
        ko.applyBindings(window.unsubscribeModel, document.querySelector('.member-wrapper'));
    });