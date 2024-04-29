const emailerViewModel = ((parent, defaultSubject, gRecaptchaElementId, sendFnc) => {
    let self = {
        parent: parent,
        defaultSubject: defaultSubject,
        gRecaptchaElementId: gRecaptchaElementId,
        sendFnc: sendFnc
    };

    self.resources = self.parent.resources;

    self.sendLabel = ko.observable(self.resources.SendEmailLabel);

    self.validateSendNow = ko.observable(false);

    self.sentTo = ko.observable();

    self.showLastSentTo = ko.observable(false);

    self.captchaPrepared = false;

    if (document.querySelector(`#${self.gRecaptchaElementId}`) == null) {
        console.error('emailer demands gRecaptchaElementId, without # prefix');
        return;
    }

    self.to = ko.observable().extend({
        email: {
            params: true,
            onlyIf: function () {
                return self.validateSendNow;
            },
            message: self.resources.FormsErrorEmailFormat
        },
        required: {
            params: true,
            onlyIf: function () {
                return self.validateSendNow();
            },
            message: self.resources.RecipientEmailRequired
        },
        errorPulse: {},
        postable: true
    });

    self.toParams = {
        label: self.resources.MoodboardShareEmailRecipientLabel,
        originalValue: self.sentTo,
        observable: self.to,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            $(self.observable.DOMElement).focus();
        },
        permitRevert: false,
        charsRemainingOptions: {
            maxLength: 254
        }
    }

    self.userName = ko.observable(getUserName());

    self.userNameParams = {
        label: self.resources.MoodboardShareNameLabel,
        originalValue: self.userName(),
        observable: self.userName,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            $(self.observable.DOMElement).focus();
        },
        permitRevert: true,
        revertFunction: function () {
            self.userName(getUserName());
            $(self.userName.DOMElement).focus();
        },
        charsRemainingOptions: {
            maxLength: 50
        }
    }

    self.recipientName = ko.observable();

    self.recipientNameParams = {
        label: self.resources.MoodboardShareRecipientNameLabel,
        originalValue: self.recipientName(),
        observable: self.recipientName,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            $(self.observable.DOMElement).focus();
        },
        permitRevert: false,
        charsRemainingOptions: {
            maxLength: 50
        }
    }

    self.subject = ko.observable(self.defaultSubject).extend({
        required: {
            params: true,
            onlyIf: function () {
                return self.validateSendNow();
            },
            message: self.resources.SubjectRequired
        },
        errorPulse: {},
        postable: true
    });

    self.originalSubject = ko.observable(self.defaultSubject);

    self.subjectParams = {
        label: self.resources.Subject,
        originalValue: self.originalSubject,
        observable: self.subject,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            $(self.observable.DOMElement).focus();
        },
        permitRevert: true,
        revertFunction: function () {
            self.subject(self.originalSubject());
            $(self.subject.DOMElement).focus();
        },
        charsRemainingOptions: {
            maxLength: 1000
        }
    };

    self.originalReplyTo = ko.observable(getUserEmail());

    self.replyTo = ko.observable(getUserEmail()).extend({
        required: {
            params: true,
            onlyIf: function () {
                return self.validateSendNow();
            },
            message: self.resources.ReplyToEmailRequired
        },
        errorPulse: {},
        postable: true
    });

    self.replyToParams = {
        label: self.resources.ReplyTo,
        originalValue: self.originalReplyTo,
        observable: self.replyTo,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            $(self.observable.DOMElement).focus();
        },
        permitRevert: true,
        revertFunction: function () {
            self.replyTo(self.originalReplyTo());
            $(self.replyTo.DOMElement).focus();
        },
        charsRemainingOptions: {
            maxLength: 254
        }
    }

    self.body = ko.observable().extend({
        //required: {
        //    params: true,
        //    onlyIf: function () {
        //        return self.validateSendNow()
        //    },
        //    message: self.resources.EmailMessageRequired
        //},
        errorPulse: {},
        postable: true
    });

    self.originalBody = ko.observable();

    self.bodyParams = {
        label: self.resources.EmailMessage,
        originalValue: self.originalBody,
        observable: self.body,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        rows: 10,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            $(self.observable.DOMElement).focus();
        },
        permitRevert: true,
        revertFunction: function () {
            self.body(self.originalBody());
            $(self.body.DOMElement).focus();
        },
        charsRemainingOptions: {
            maxLength: 4000
        }
    };

    self.emailErrors = ko.validation.group([
        self.to,
        self.replyTo,
        self.subject,
        self.body
    ]);

    self.initCaptcha = () => {
        if (self.captchaPrepared == false) {
            try {
                grecaptcha.render(self.gRecaptchaElementId, {
                    sitekey: '6LcHNDshAAAAAM05Kn3Y8tad1U4Sn7sWZW0Djzq8'
                });
            } catch (e) {
                //console.error(e);
            }
            self.captchaPrepared = true;
        }
        return true;
    }

    self.send = () => {
        self.validateSendNow(false);
        self.validateSendNow(true);
        if (self.emailErrors().length) {
            self.emailErrors().filter(x => {
                if (x.hasOwnProperty('isValid'))
                    x.isValid.notifySubscribers(x.isValid())
            });
        } else {
            self.sendFnc(() => {

                try {
                    grecaptcha.reset();

                    self.captchaPrepared = false;

                    self.initCaptcha();
                }
                catch (e) {

                }

            });
        }
    };

    self.toJS = () => {
        var result = Object.assign(...Object.keys(self).flatMap(k => {
            let result = {};
            if (self[k].hasOwnProperty('postable')) {
                if (ko.isObservableArray(self[k])) { // deep unwrap one level - specifcally for marketingPermissions, also for anything else
                    let arr = [];
                    ko.unwrap(self[k]()).forEach(i => {
                        let item = {};
                        Object.keys(i).forEach(ik => {
                            return item[ik] = ko.unwrap(i[ik]);
                        });
                        arr.push(item);
                    });
                    result[k] = arr;
                } else {
                    result[k] = ko.unwrap(self[k]);
                }
                return [result];
            }
            else
                return [];
        }));

        try {
            result.gRecaptcha = grecaptcha.getResponse();
        } catch (e) {
            window.logClientSideError(e);
        }

        return result;
    }

    self.reflowLabels = () => {
        window.setTimeout(() => {
            if (self.to.hasOwnProperty('mdcTextField'))
                self.to.mdcTextField.layout();
            if (self.userName.hasOwnProperty('mdcTextField'))
                self.userName.mdcTextField.layout();
            if (self.subject.hasOwnProperty('mdcTextField'))
                self.subject.mdcTextField.layout();
            if (self.recipientName.hasOwnProperty('mdcTextField'))
                self.recipientName.mdcTextField.layout();
            if (self.replyTo.hasOwnProperty('mdcTextField'))
                self.replyTo.mdcTextField.layout();
            if (self.body.hasOwnProperty('mdcTextField'))
                self.body.mdcTextField.layout();
        }, 0);
    }

    return self;
});