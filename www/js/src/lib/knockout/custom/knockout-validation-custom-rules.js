if (ko.validation) {
    ko.validation.rules['zxcvbnValidator'] = {
        validator: function (val, params) {

            if (params == null) {
                console.error('zxcvbn validation requires params { zxcvbn: observable, minScore: # }');
                return false;
            }

            if (!params.hasOwnProperty('selfReference') ||
                !params.hasOwnProperty('minScore')) {
                console.error('zxcvbn validation requires params { observable: property, minScore: # }');
                return false;
            }

            if (!ko.isObservable(params.selfReference)) {
                console.error('zxcvbnValidator for', params.zxcvbn, 'parameter zxcvbn is not an observable');
                return false;
            }

            if (!params.selfReference.hasOwnProperty('zxcvbn')) {
                console.error('zxcvbnValidator for', params.selfReference, 'does not utilize the zxcvbn extender');
                return false;
            }

            if (val) {
                if (params.selfReference) {
                    let result = params.selfReference.zxcvbn().score >= params.minScore;
                    if (params.selfReference.hasOwnProperty('mdcTextField')) {
                        if (params.selfReference.mdcTextField) {
                            params.selfReference.mdcTextField.valid = result;
                        }
                    }
                    return result;
                } else {
                    return false;
                }
            } else {
                return false;
            }
        },
        message: 'The field must equal {0}'
    }
}
ko.validation.registerExtenders();