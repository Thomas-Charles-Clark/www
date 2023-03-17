const coverageCalculatorViewModel = ((parent) => {
    let self = {
        parent: parent,
        resources: parent.resources
    };

    //#region Properties

    self.busy = ko.observable(false);

    self.validateNow = ko.observable(true);

    self.validateEmailNow = ko.observable(false);

    self.domElement = ko.observable();

    self.unit = ko.observable();

    self.coats = ko.observable(2);

    self.includeWastage = ko.observable(false);

    self.finish = ko.observable();

    self.availableFinishes = self.parent.allFinishes;

    // lifted so as to avoid including email
    self.errors = new ko.validation.group([
        self.units,
        self.walls,
        self.rooms
    ], {
        deep: true,
        live: true
    });

    self.email = ko.observable().extend({
        required: {
            params: true,
            onlyIf: () => {
                return self.validateEmailNow()
            },
            message: self.resources.FormsErrorEmail
        },
        email: {
            params: true,
            onlyIf: () => {
                return self.validateEmailNow()
            },
            message: self.resources.FormsErrorEmailFormat
        },
        errorPulse: {}
    });

    self.captchaPrepared = false;

    self.captchaFailed = ko.observable(false);

    // Lifted Necessarily

    self.removeRoom = (room) => {
        self.rooms.remove(room);
    }

    self.removeWall = (wall) => {
        self.walls.remove(wall);
    }

    // End of Lifted Necessarily

    self.rooms = ko.observableArray([
        //roomModel(self)
    ]);

    self.walls = ko.observableArray([
        //wallModel(self)
    ]);

    self.rooms.extend({
        minLength: {
            params: 1,
            onlyIf: () => {
                return self.validateNow() &&
                    self.walls().length == 0;
            }
        },
        errorPulse: {}
    });

    self.walls.extend({
        minLength: {
            params: 1,
            onlyIf: () => {
                return self.validateNow() &&
                    self.rooms().length == 0
            }
        },
        errorPulse: {}
    });

    self.coverageResult = ko.observable();

    //#endregion


    //#region Components

    self.emailErrors = new ko.validation.group([
        self.email
    ]);

    //#endregion


    //#region Subscriptions

    self.finish.subscribe(nv => {
        self.parent.finish(nv);
        self.rooms([]);
        self.coverageResult(undefined);
        if (nv === 615) {
            bootstrap.Tab.getOrCreateInstance(document.querySelector('#rooms-tab-btn')).show();
            document.querySelector('#walls-tab-btn').parentElement.style.display = 'none';
        } else {
            document.querySelector('#walls-tab-btn').parentElement.style.display = 'list-item';
        }
    });

    //#endregion


    //#region Computeds

    self.changeComputer = ko.computed(() => {
        let rooms = self.rooms();
        let walls = self.walls();
        let coats = self.coats();
        let includeWastage = self.includeWastage();

        rooms.forEach(r => {
            let rw = r.width();
            let rh = r.height();
            let rl = r.length();
        });

        walls.forEach(w => {
            let ww = w.width();
            let wh = w.height();
            if (w.hasOwnProperty('exclusions')) {
                let wExclusions = w.exclusions();
                wExclusions.forEach(e => {
                    let ew = e.width();
                    let eh = e.height();
                });
            }
        });

        self.coverageResult(undefined);

    }, self).extend({
        rateLimit: {
            timeout: 10,
            method: 'notifyWhenChangesStop'
        }
    });

    self.unitMinimumComputer = ko.computed(() => {
        switch (self.unit()) {
            case 'cm':
                return 1;
                break;
            case 'm':
                return 0.1;
                break;
            case '"':
                return 1;
                break;
            case 'ft':
                return 0.08;
                break;
            case 'yd':
                return 0.33;
                break;
        }
    }, self);

    //#endregion


    //#region Methods

    self.addRoom = () => {
        self.rooms.push(roomModel(self));
    }

    self.addWall = () => {
        self.walls.push(wallModel(self));
    }

    self.showModal = () => {
        bootstrap.Modal.getOrCreateInstance(self.domElement.DOMElement).show();
    }

    self.hideModal = () => {
        bootstrap.Modal.getOrCreateInstance(self.domElement.DOMElement).hide();
    }

    self.calculate = (includeCaptcha) => {
        self.captchaFailed(false);
        self.validateNow(false);
        self.validateNow(true);

        if (includeCaptcha) {
            self.validateEmailNow(false);
            self.validateEmailNow(true);
        }

        if (self.errors().length) {
            self.errors.showAllMessages(true);
            self.errors.filter(x => x.isValid.notifySubscribers(x.isValid()));
            return;
        }

        if (self.emailErrors().length) {
            self.emailErrors.showAllMessages(true);
            self.emailErrors.filter(x => x.isValid.notifySubscribers(x.isValid()));
            return;
        }

        window.setTimeout(() => {

            self.busy(true);

            let input = self.toJS(includeCaptcha);

            console.trace(input);

            fetch('/api/generalapi/CoverageCalculator', {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-cache',
                credentials: 'same-origin',
                body: JSON.stringify(input)
            })
                .then(window.handleFetchResponse)
                .then(data => {
                    self.busy(false);
                    if (data.error) {
                        if (data.message == 'InvalidCaptcha') {
                            self.captchaFailed(true);
                        }
                    } else {
                        console.trace(data);

                        try {
                            grecaptcha.reset();

                            self.captchaPrepared = false;

                            self.initCaptcha();
                        }
                        catch (e) {

                        }

                        if (!includeCaptcha) {
                            self.coverageResult(data);
                        } else {
                            bootstrap.Collapse.getOrCreateInstance(document.querySelector('#email-calc-collapse')).hide();
                            window.showInfo(self.resources.ThankYou, self.resources.ThankYouEmailSent);
                        }
                    }
                })
                .catch(err => {
                    self.busy(false);
                    console.trace(err);
                    window.showError();
                })
        }, 0);
    }

    self.addEstimateToBasket = () => {
        self.busy(true);
        let promises = [];
        let e = new Event('');
        self.coverageResult().ShoppingList.forEach(sl => {
            promises.push(basketModel.addToBasket({
                productId: sl.ProductID,
                quantity: ko.observable(sl.Quantity),
                basketLineTypeId: sl.BasketLineTypeID,
                productPageId: self.parent.Definition.json.productPageId,
                isValid: () => true,
                enabled: () => { }
            }, e));
        });
        Promise.allSettled(promises).then(() => {
            self.busy(false);
            self.hideModal();
        });
    }

    self.initCaptcha = () => {
        if (self.captchaPrepared == false) {
            try {
                grecaptcha.render('g-recaptcha', {
                    sitekey: '6LcHNDshAAAAAM05Kn3Y8tad1U4Sn7sWZW0Djzq8'
                });
            } catch (e) {

            }
            self.captchaPrepared = true;
        }
        return true;
    }

    self.toJS = (includeCaptcha) => {
        let result = {
            countryId: getCountryId(),
            languageId: getLanguageId(),
            unit: self.unit(),
            dimensions: JSON.stringify({
                rooms: self.rooms().flatMap(r => r.toJS()),
                walls: self.walls().flatMap(w => w.toJS())
            }),
            includeWastage: self.includeWastage(),
            productPageId: self.parent.Definition.json.productPageId,
            coats: +self.coats(),
            paintFinishAttributeId: self.finish()
        };
        if (includeCaptcha) {
            result.email = self.email();
            result.captcha = grecaptcha.getResponse();
        }
        return result;
    }

    //#endregion

    return self;
});

const roomModel = ((parent) => {
    let self = {
        resources: parent.resources,
        validateNow: parent.validateNow,
        parent: parent
    };

    self.id = `${new Date().valueOf()}`;

    self.width = ko.observable();
    self.widthId = `width-${self.id}`;

    self.length = ko.observable();
    self.lengthId = `length-${self.id}`;

    self.height = ko.observable();
    self.heightId = `height-${self.id}`;

    self.width.extend({
        required: {
            params: true,
            onlyIf: () => {
                return self.validateNow()
            },
            message: self.resources.PleaseProvideWidth
        },
        min: {
            params: self.parent.unitMinimumComputer,
            message: () => `${self.resources.Width} ${self.resources.CalculatorMustBeBetween.replace('1', self.parent.unitMinimumComputer())}`
        },
        max: {
            params: 99,
            message: `${self.resources.Width} ${self.resources.CalculatorMustBeBetween.replace('1', self.parent.unitMinimumComputer())}`
        },
        errorPulse: {}
    });

    self.height.extend({
        required: {
            params: true,
            onlyIf: () => {
                return self.validateNow()
            },
            message: self.resources.PleaseProvideHeight
        },
        min: {
            params: self.parent.unitMinimumComputer(),
            message: () => `${self.resources.Height} ${self.resources.CalculatorMustBeBetween.replace('1', self.parent.unitMinimumComputer())}`
        },
        max: {
            params: 99,
            message: `${self.resources.Height} ${self.resources.CalculatorMustBeBetween.replace('1', self.parent.unitMinimumComputer())}`
        },
        errorPulse: {}
    });

    self.length.extend({
        required: {
            params: true,
            onlyIf: () => {
                return self.validateNow()
            },
            message: self.resources.PleaseProvideLength
        },
        min: {
            params: self.parent.unitMinimumComputer,
            message: () => `${self.resources.Length} ${self.resources.CalculatorMustBeBetween.replace('1', self.parent.unitMinimumComputer())}`
        },
        max: {
            params: 99,
            message: `${self.resources.Length} ${self.resources.CalculatorMustBeBetween.replace('1', self.parent.unitMinimumComputer())}`
        },
        errorPulse: {}
    });

    self.remove = self.parent.removeRoom;

    self.toJS = () => {
        return {
            id: +self.id,
            width: +self.width(),
            height: +self.height(),
            length: +self.length()
        };
    };

    return self;
});

const wallModel = ((parent) => {
    let self = {
        resources: parent.resources,
        validateNow: parent.validateNow,
        parent: parent,
        unit: parent.unit
    };

    self.id = `${new Date().valueOf()}`;

    self.width = ko.observable();
    self.widthId = `width-${self.id}`;

    self.height = ko.observable();
    self.heightId = `height-${self.id}`;

    self.width.extend({
        required: {
            params: true,
            onlyIf: () => {
                return self.validateNow()
            },
            message: self.resources.PleaseProvideWidth
        },
        min: {
            params: self.parent.unitMinimumComputer,
            message: () => `${self.resources.Width} ${self.resources.CalculatorMustBeBetween.replace('1', self.parent.unitMinimumComputer())}`
        },
        max: {
            params: 99,
            message: `${self.resources.Width} ${self.resources.CalculatorMustBeBetween.replace('1', self.parent.unitMinimumComputer())}`
        },
        errorPulse: {}
    });

    self.height.extend({
        required: {
            params: true,
            onlyIf: () => {
                return self.validateNow()
            },
            message: self.resources.PleaseProvideHeight
        },
        min: {
            params: self.parent.unitMinimumComputer,
            message: () => `${self.resources.Height} ${self.resources.CalculatorMustBeBetween.replace('1', self.parent.unitMinimumComputer())}`
        },
        max: {
            params: 99,
            message: `${self.resources.Height} ${self.resources.CalculatorMustBeBetween.replace('1', self.parent.unitMinimumComputer())}`
        },
        errorPulse: {}
    });

    if (self.parent.parent.Definition.json.productTypeId != 18) {
        self.exclusions = ko.observableArray([]);
    }

    self.remove = self.parent.removeWall;

    self.unitMinimumComputer = self.parent.unitMinimumComputer;

    self.addExclusion = () => {
        self.exclusions.push(exclusionModel(self));
    }

    self.removeExclusion = (exclusion) => {
        self.exclusions.remove(exclusion);
    }

    self.toJS = () => {
        return {
            id: +self.id,
            width: +self.width(),
            height: +self.height(),
            exclusions: self.hasOwnProperty('exclusions') ? self.exclusions().flatMap(e => e.toJS()) : []
        };
    };

    return self;
});

const exclusionModel = ((parent) => {
    let self = {
        resources: parent.resources,
        validateNow: parent.validateNow,
        parent: parent
    };

    self.id = `${new Date().valueOf()}`;

    self.width = ko.observable();
    self.widthId = `width-${self.id}`;

    self.height = ko.observable();
    self.heightId = `height-${self.id}`;

    self.width.extend({
        required: {
            params: true,
            onlyIf: () => {
                return self.validateNow()
            },
            message: self.resources.PleaseProvideWidth
        },
        min: {
            params: self.parent.unitMinimumComputer,
            message: () => `${self.resources.Width} ${self.resources.CalculatorMustBeBetween.replace('1', self.parent.unitMinimumComputer())}`
        },
        max: {
            params: 99,
            message: `${self.resources.Width} ${self.resources.CalculatorMustBeBetween.replace('1', self.parent.unitMinimumComputer())}`
        },
        errorPulse: {}
    });

    self.height.extend({
        required: {
            params: true,
            onlyIf: () => {
                return self.validateNow()
            },
            message: self.resources.PleaseProvideHeight
        },
        min: {
            params: self.parent.unitMinimumComputer,
            message: () => `${self.resources.Height} ${self.resources.CalculatorMustBeBetween.replace('1', self.parent.unitMinimumComputer())}`
        },
        max: {
            params: 99,
            message: `${self.resources.Height} ${self.resources.CalculatorMustBeBetween.replace('1', self.parent.unitMinimumComputer())}`
        },
        errorPulse: {}
    });

    self.remove = self.parent.removeExclusion;

    self.toJS = () => {
        return {
            id: +self.id,
            width: +self.width(),
            height: +self.height()
        };
    };

    return self;
});