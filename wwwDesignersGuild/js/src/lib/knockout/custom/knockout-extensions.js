if (typeof window.onKnockoutAvailable === 'function')
    window.onKnockoutAvailable();

ko.validation.init({
    decorateInputElement: true,
    decorateElementOnModified: true,
    registerExtenders: true,
    messagesOnModified: true,
    insertMessages: true,
    parseInputAttributes: false,
    errorElementClass: 'validation-element',
    errorMessageClass: 'validation-message'
}, true);

function assignVisibilityCallback(ele, cb) {
    var options = {
        root: null//document.documentElement
    }

    var observer = new IntersectionObserver(function (entries, observer) {
        entries.forEach(function (entry) {
            cb(entry.intersectionRatio > 0);
        });
    }, options);

    observer.observe(ele);
}

ko.extenders.postable = function (target, value) {
    target.postable = value;
}

ko.extenders.errorPulse = function (target, options) {
    if (target.hasOwnProperty('isValid')) {
        target.isValid.subscribe(function (newValue) {
            if (!newValue && target.hasOwnProperty('DOMElement')) {

                target.DOMElement.parentElement.querySelectorAll('.validation-element').forEach(ve => {
                    if (!ve.classList.contains('error-pulse'))
                        ve.classList.add('error-pulse');

                    setTimeout(function () {
                        ve.classList.remove('error-pulse');
                    }, 150);
                })
            }
        });
    }
}

// thanks: https://stackoverflow.com/questions/9465200/get-element-an-observable-is-bound-to-with-knockout#answer-12187273
ko.extenders.element = function (target, element) {
    target.DOMElement = element;
}

ko.bindingHandlers.keepElement = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        if (Array.isArray(valueAccessor())) {
            valueAccessor().forEach(va => {
                va.extend({ element: element });
            })
        } else {
            valueAccessor().extend({ element: element });
        }
    }
};

ko.bindingHandlers.enterKey = {
    init: function (element, valueAccessor, allBindings, data, context) {
        var wrapper = function (data, event) {
            if (event.keyCode === 13) {
                if (valueAccessor() && typeof valueAccessor()  === 'function')
                    valueAccessor().call();
            }
            return true;
        };
        ko.applyBindingsToNode(element, { event: { keyup: wrapper } }, context);
    }
}

ko.bindingHandlers.downArrowKey = {
    init: function (element, valueAccessor, allBindings, data, context) {
        var wrapper = function (data, event) {
            if (event.keyCode === 40) {
                if (valueAccessor() && typeof valueAccessor() === 'function')
                    valueAccessor().call();
            }
        };
        ko.applyBindingsToNode(element, { event: { keyup: wrapper } }, context);
    }
}

// Here's a custom Knockout binding that makes elements shown/hidden via jQuery's fadeIn()/fadeOut() methods
// Could be stored in a separate utility library
ko.bindingHandlers.fadeVisible = {
    init: function (element, valueAccessor) {
        // Initially set the element to be instantly visible/hidden depending on the value
        var value = ko.unwrap(valueAccessor());
        $(element).toggle(value); // Use "unwrapObservable" so we can handle values that may or may not be observable
    },
    update: function (element, valueAccessor, allBindingsAccessor) {
        // Whenever the value subsequently changes, slowly fade the element in or out
        var value = ko.unwrap(valueAccessor());

        var fadeSpeed = undefined;
        if (allBindingsAccessor().fadeSpeed) {
            fadeSpeed = allBindingsAccessor().fadeSpeed;
        }

        ko.unwrap(value) ?
            $(element).stop().fadeIn(fadeSpeed) :
            $(element).stop().fadeOut(fadeSpeed);
    }
};

// jquery-free alternative to the above. Underlying styling must have 0 opacity and ideally a transition.
ko.bindingHandlers.toggleOpacity = {
    init: function (element, valueAccessor) {
        var blockerVisible = ko.unwrap(valueAccessor());
        if (blockerVisible) {
            element.classList.remove('d-none');
            element.classList.add('opacity-100');
        }
    },
    update: function (element, valueAccessor, allBindingsAccessor) {
        // Whenever the value subsequently changes, slowly fade the element in or out
        var blockerVisible = ko.unwrap(valueAccessor());
        if (blockerVisible) {
            element.classList.remove('d-none');
            element.classList.add('opacity-100');
        } else {
            element.classList.remove('opacity-100');
            setTimeout(() => element.classList.add('d-none'), 150);
        }
    }
}

// 'Characters remaining' helper.
// I'd like very much to be able to say 'I want to display the remaining characters for this input property' in one simple binding.
ko.extenders.charsRemaining = function (observable, options) {

    if (options == null)
        console.error('charsRemaining extender - options was null');
    if (!options.hasOwnProperty('maxLength'))
        console.error('charsRemaining extender - options does not have property maxLength');
    if (!options.hasOwnProperty('elementId') && !options.hasOwnProperty('element'))
        console.error('charsRemaining extender - options does not have property elementId nor element');
    if (options.maxLength == null)
        console.error('charsRemaining extender - options.maxLength was null');
    if (isNaN(options.maxLength))
        console.error('charsRemaining extender - options.maxLength was NaN');

    observable.maxLength = options.maxLength;

    var label = $(options.element || options.elementId).closest('.form-group').find(".col-form-label");

    if (!label.length) {
        label = $(options.element || options.elementId).closest('.mdc-validating-field').find(".mdc-floating-label");
    }

    if (!label.length) {
        label = $(options.element || options.elementId).closest('.form-group').find(".field-label");
    }

    if (!label.length) {
        //console.error(options.element || options.elementId, ' charsRemaining extender - input element must be inside a .form-group containing a .col-form-label or a .field-label, or a .mdc-validating-field containing an .mdc-floating-label');
    }

    observable.charsRemaining = ko.pureComputed({
        read: function () {
            var value = ko.unwrap(observable());
            if (value == null)
                value = "";
            return '(' + (observable.maxLength - value.length) + ')';
        }
    }, this, { disposeWhenNodeIsRemoved: label });

    $(options.elementId).attr('maxlength', observable.maxLength);

    // upgraded to permit having duplicate input elements tied to same observable... lovely.
    $(label).each(function (index, ele) {
        var sup = document.createElement('sup');
        sup.className = "remaining";
        $(ele).find('sup').remove();
        $(ele).append(sup);
        ko.applyBindingsToNode(sup, { html: observable.charsRemaining });
    });
};

ko.bindingHandlers.charsRemaining = {
    init: function (element, valueAccessor, allBindings, data, context) {
        if (valueAccessor()) {
            $(element).prop('maxlength', data.charsRemainingOptions.maxLength);
            data.observable.extend({
                charsRemaining: {
                    maxLength: data.charsRemainingOptions.maxLength,
                    element: element
                }
            });
        }
    }
}

ko.bindingHandlers.maxNewLines = {
    init: function (element, valueAccessor, allBindings, viewModel, context) {
        var maxNewLines = ko.unwrap(valueAccessor());
        if (maxNewLines) {
            //console.log('maxNewLines bindingHandler init hit', maxNewLines);
        }
        $(element).on('keydown', function (event) {
            var text = ko.unwrap(allBindings.get('textInput')) || '';
            var newLines = text.split(/\r*\n/).length;
            if (event.keyCode === 13 && newLines >= 3) {
                event.preventDefault();
            }
        });
    },
    update: function (element, valueAccessor, allBindings, viewModel, context) {
        var maxNewLines = ko.unwrap(valueAccessor());
        if (maxNewLines) {
            var text = ko.unwrap(allBindings.get('textInput'));
            if (text) {
                var split = text.split(/\r*\n/);
                if (split.length > 3) {
                    text = split[0] + '\n' + split[1] + '\n' + split[2] + ' ' + split.splice(3).join(' ');
                }
                allBindings.get('textInput')(text);
            }
        }
    }
}

// Here's another home cooked one then:
ko.extenders.pcaPredict = function (observable, options) {

    if (options === null) {
        console.error('pcaPredict extender requires options');
    }
    if (!options.hasOwnProperty("elementId") && !options.hasOwnProperty('element')) {
        console.error('pcaPredict extender options object requires elementId OR element');
    }
    if (!options.hasOwnProperty("mapFunction")) {
        console.error('pcaPredict extender options object requires mapFunction to utilise results');
    }
    if (typeof options.mapFunction !== 'function') {
        console.error('pcaPredict extender options object requires mapFunction function taking params: line1, line2, line3, line4 & postalCode')
    }
    if (!options.hasOwnProperty("country")) {
        console.error('pcaPredict extender options object requires countryCode observable');
    }

    //observable.subscribe(function (newValue) {
    //    if (newValue === '') {
    //        newValue = null;
    //    }
    //    if (newValue != null) {
    //        if (!$(observable.mdcTextField.label_.root_).hasClass('mdc-floating-label--float-above')) {
    //            //window.setTimeout(function () {
    //            observable.mdcTextField.label_.float(true);
    //            observable.mdcTextField.getDefaultFoundation().adapter_.notchOutline(params.boundTo.mdcTextField.getDefaultFoundation().adapter_.getLabelWidth() * 0.75);
    //            //}, 50);
    //        }
    //    } else {
    //        if ($(observable.mdcTextField.label_.root_).hasClass('mdc-floating-label--float-above')) {
    //            //window.setTimeout(function () {
    //            observable.mdcTextField.label_.float(false);
    //            observable.mdcTextField.getDefaultFoundation().adapter_.closeOutline();
    //            //}, 50);
    //        }
    //    }
    //});

    observable.PCASearchId = ko.observable();
    observable.PCAContainer = ko.observable();

    $(options.elementId || options.element).autocomplete({
        appendTo: $(options.elementId || options.element).parent(),
        source: function (request, response) {
            if (ko.unwrap(options.country)) {
                $(options.elementId || options.element).parent().find('.pca-failure').remove();
                $.ajax({
                    url: "/api/member/addressLookupSearch",
                    type: "POST",
                    dataType: "json",
                    data: {
                        id: observable.PCASearchId(),
                        text: observable(),
                        container: observable.PCAContainer(),
                        origin: null,
                        countries: ko.unwrap(options.country).Code,
                    },
                    success: function (data) {
                        $(options.elementId || options.element).removeClass('ui-autocomplete-loading');
                        if (observable.PCASearchId() != null) {
                            if (data.length == 1) {
                                var county = data[0].Province || data[0].ProvinceName || data[0].ProvinceCode || data[0].AdminAreaName || data[0].AdminAreaCode;
                                if (data[0].City === "London" && ko.unwrap(options.country).Code == "GB")
                                    county = "London";
                                var line1 = data[0].Line1;
                                if (data[0].Company != "") {
                                    line1 = data[0].Company + ", " + line1;
                                }

                                options.mapFunction(
                                    data[0].Company, // companyName
                                    "", // contactName
                                    line1, // adressLine1
                                    data[0].Line2, // addressLine2
                                    data[0].City, // town
                                    county, // county
                                    data[0].PostalCode, // postcode
                                    null, // phone
                                    null, // email
                                    null, // deliveryReference
                                    true, // loqateAddress
                                    ko.unwrap(options.country).CountryID // countryId
                                );

                                $(options.elementId || options.element).autocomplete("close");

                                observable.PCASearchId(null);
                                observable.PCAContainer(null);

                                return false;
                            } else {
                                response($.map(data, function (item) {
                                    var county = item.Province || item.ProvinceName || item.ProvinceCode || item.AdminAreaName || item.AdminAreaCode;
                                    if (item.City === "London" && ko.unwrap(options.country).Code == "GB")
                                        county = "London";
                                    var line1 = data[0].Line1;
                                    if (item.Company != "") {
                                        line1 = data[0].Company + ", " + line1;
                                    }
                                    return {
                                        customCatch: true,
                                        label: item.Label,
                                        value: item.Id,
                                        addressLine1: line1,
                                        addressLine2: item.Line2,
                                        addressLine3: item.City,
                                        addressLine4: county,
                                        Postcode: item.PostalCode
                                    }
                                }));
                                return false;
                            }
                        }

                        observable.PCASearchId(null);
                        observable.PCAContainer(null);

                        response($.map(data, function (item) {
                            if (item.hasOwnProperty('Type')) {
                                if ((item.Type === "Address") || (item.Type === "Residential")) {
                                    return {
                                        id: item.Id,
                                        label: item.Text + ", " + item.Description,
                                        value: item.Id,
                                        type: item.Type,
                                        text: item.Text
                                        // highlight: item.Highlight
                                    };
                                } else {
                                    return {
                                        id: item.Id,
                                        label: item.Description,
                                        value: item.Id,
                                        type: item.Type,
                                        text: item.Text
                                        // highlight: item.Highlight
                                    };
                                }
                            } else {
                                return {
                                    label: item.Label,
                                    value: item.Id
                                }
                            }
                        }));

                        if (typeof options.afterSearch === 'function')
                            options.afterSearch();
                    },
                    error: function (xhr, textStatus, errorThrown) {
                        console.log('pcaPredict failure', errorThrown);
                        $(options.elementId || options.element).removeClass('ui-autocomplete-loading');
                        $(options.elementId || options.element).parent().find('.pca-failure').remove();
                        $(options.elementId || options.element).parent().append('<span class="pca-failure alert alert-danger">Lookup Failure. Please try again.</span>');
                    }
                });
            } else {
                $(options.elementId || options.element).removeClass('ui-autocomplete-loading');
                response($.map([{
                    id: '',
                    label: options.resources.PleaseSelectCountryBefore,
                    value: null,
                    type: null,
                    text: options.resources.PleaseSelectCountryBefore
                }], function (item) {
                    return {
                        id: item.id,
                        label: item.label,
                        value: item.value,
                        type: item.type,
                        text: item.text
                    }
                }));
            }
        },
        minLength: 1,
        select: function (event, ui) {
            if (ui.item.hasOwnProperty('type')) {
                switch (ui.item.type) {
                    case 'Address':
                    case 'Residential':
                        jQuery.extend(event.originalEvent, { keepOpen: true });
                        observable.PCASearchId(ui.item.id);
                        observable.PCAContainer(null);
                        break;
                    default:
                        jQuery.extend(event.originalEvent, { keepOpen: true });
                        //modify value as required
                        observable.PCASearchId(null);
                        observable.PCAContainer(ui.item.id);
                        //$("#pca-find").autocomplete('search');
                        return false;
                }
            } else if (ui.item.hasOwnProperty('customCatch')) {

                options.mapFunction(ui.item.addressLine1,
                    ui.item.addressLine2,
                    ui.item.addressLine3,
                    ui.item.addressLine4,
                    ui.item.Postcode);

                $(options.elementId || options.element).autocomplete("close");

                observable.PCASearchId(null);
                observable.PCAContainer(null);

                if (observable.hasOwnProperty('PCASearchTerm'))
                    observable.PCASearchTerm(null);
            }

            return false;
        },
        focus: function (event, ui) {
            event.preventDefault();
        },
        autoFocus: true
    });

    $(options.elementId || options.element).data("ui-autocomplete")._renderItem = function (ul, item) {
        return $('<li class="ui-menu-item">')
            .attr("data-value", item.value)
            .append('<div class="ui-menu-item-wrapper">' + item.label + '</div>')
            .appendTo(ul);
    }

    $(options.elementId || options.element).data("ui-autocomplete")._renderMenu = function (ul, items) {
        var that = this;
        $.each(items, function (index, item) {
            that._renderItemData(ul, item);
        });
        $(ul).addClass("address-autocomplete");
    };

    observable.performSearch = function () {
        $(options.elementId || options.element).autocomplete('search');
    }

    if (options.hasOwnProperty('readyNotice')) {
        window.setTimeout(() => { options.readyNotice(true); }, 0)
    }
}


// same as the above, but instead of rendering results as a dropdown they're injected into a list-group ul
// named 'searchResultsList'
ko.extenders.pcaPredictList = function (observable, options) {

    if (options === null) {
        console.error('pcaPredictList extender requires options');
    }
    if (!options.hasOwnProperty('element')) {
        console.error('pcaPredictList extender options object requires element');
    }
    if (!options.hasOwnProperty("mapFunction")) {
        console.error('pcaPredictList extender options object requires mapFunction to utilise results');
    }
    if (typeof options.mapFunction !== 'function') {
        console.error('pcaPredictList extender options object requires mapFunction function taking params: line1, line2, line3, line4 & postalCode')
    }
    if (!options.hasOwnProperty("country")) {
        console.error('pcaPredictList extender options object requires countryCode observable');
    }
    if (!options.hasOwnProperty('searchResultsList'))
        console.error('pcaPredictList extender options object requires searchResultsList observableArray')

    if (!options.hasOwnProperty('searchEndpoint'))
        console.error('pcaPredictList extender options object requires searchEndpoint observable')

    //observable.subscribe(function (newValue) {
    //    if (newValue === '') {
    //        newValue = null;
    //    }
    //    if (newValue != null) {
    //        if (!$(observable.mdcTextField.label_.root_).hasClass('mdc-floating-label--float-above')) {
    //            //window.setTimeout(function () {
    //            observable.mdcTextField.label_.float(true);
    //            observable.mdcTextField.getDefaultFoundation().adapter_.notchOutline(params.boundTo.mdcTextField.getDefaultFoundation().adapter_.getLabelWidth() * 0.75);
    //            //}, 50);
    //        }
    //    } else {
    //        if ($(observable.mdcTextField.label_.root_).hasClass('mdc-floating-label--float-above')) {
    //            //window.setTimeout(function () {
    //            observable.mdcTextField.label_.float(false);
    //            observable.mdcTextField.getDefaultFoundation().adapter_.closeOutline();
    //            //}, 50);
    //        }
    //    }
    //});

    observable.PCASearchId = ko.observable();
    observable.PCAContainer = ko.observable();
    observable.searchEndpoint = options.searchEndpoint;
    observable.busy = options.busy;

    observable.performSearch = function (response) {

        if (!observable.busy()) {

            observable.busy(true);

            var data = {
                id: observable.PCASearchId(),
                text: observable(),
                container: observable.PCAContainer(),
                origin: null,
                countries: ko.unwrap(options.country) ? ko.unwrap(options.country).Code : null,
                mode: ko.unwrap(options.mode),
                language: getLanguageId()
            };

            if (observable.searchEndpoint() == '/api/generalapi/addressSearch') {
                data.countries = null;
            }

            $.ajax({
                url: observable.searchEndpoint(),//"/Products/Enquiry/PCAPredictFind",
                type: "POST",
                dataType: "json",
                data: data,
                success: function (data) {
                    if (data.error) {
                        window.showError();
                    } else {

                        $(options.elementId || options.element).removeClass('ui-autocomplete-loading');

                        //if (observable.PCASearchId() != null || (observable.searchEndpoint().indexOf('mertex') > -1)) {

                        var results = [];

                        if (data.mertexResults.length) {
                            results = $.map(data.mertexResults, function (item) {
                                var county = item.Province || item.ProvinceName || item.ProvinceCode || item.AdminAreaName || item.AdminAreaCode;
                                if (item.City === "London" && ko.unwrap(options.country).Code == "GB")
                                    county = "London";
                                var avm = addressViewModel({
                                    labelClass: 'delivery-address-label',
                                    controlClass: 'form-control mdc-text-field__input',
                                    phoneIntlMobile: false,
                                    email: true
                                }, options.resources, options.addressSearchWrapper);

                                avm.loadPCAResult(item);

                                item = $.extend(item, {
                                    customCatch: true,
                                    label: item.Label,
                                    value: item.Id,
                                    companyName: item.Company,
                                    name: item.Name,
                                    addressLine1: item.Line1,
                                    addressLine2: item.Line2,
                                    addressLine3: item.City,
                                    addressLine4: county,
                                    postcode: item.PostalCode,
                                    phone: item.Phone,
                                    email: item.Email,
                                    deliveryReference: item.DeliveryReference,
                                    obj: item,
                                    type: item.Type,
                                    countryId: item.CountryID,
                                    country: item.Country,
                                    loqateAddress: false
                                });
                                avm.item = item;
                                return avm;
                            });

                            observable.PCASearchId(null);
                            observable.PCAContainer(null);

                            options.searchResultsList(results);

                            observable.busy(false);

                            if (typeof options.afterSearch === 'function')
                                options.afterSearch();

                            return;
                        }

                        if (observable.PCASearchId() != null) {
                            results = results.concat($.map(data.results, function (item) {
                                var county = item.Province || item.ProvinceName || item.ProvinceCode || item.AdminAreaName || item.AdminAreaCode;
                                if (item.City === "London" && ko.unwrap(options.country).Code == "GB")
                                    county = "London";

                                return {
                                    customCatch: true,
                                    label: item.Label,
                                    value: item.Id,
                                    companyName: item.Company,
                                    name: item.Name,
                                    addressLine1: item.Line1,
                                    addressLine2: item.Line2,
                                    addressLine3: item.City,
                                    addressLine4: county,
                                    postcode: item.PostalCode,
                                    phone: item.Phone,
                                    email: item.Email,
                                    deliveryReference: item.DeliveryReference,
                                    obj: item,
                                    type: item.Type,
                                    loqateAddress: true
                                };
                            }));

                        } else {
                            results = results.concat($.map(data.results, function (item) {
                                if (item.hasOwnProperty('Type')) {
                                    switch (item.Type) {
                                        case 'Address':
                                        case 'Residential':
                                            return {
                                                id: item.Id,
                                                label: item.Text + ", " + item.Description,
                                                value: item.Id,
                                                type: item.Type,
                                                text: item.Text
                                            };
                                        case 'Commercial':
                                            return {
                                                id: item.Id,
                                                label: item.Label,
                                                value: item.Id,
                                                type: item.Type,
                                                text: item.Label
                                            };
                                            break;
                                        default:
                                            return {
                                                id: item.Id,
                                                label: item.Description,
                                                value: item.Id,
                                                type: item.Type,
                                                text: item.Text
                                            }
                                    }
                                } else {
                                    return {
                                        label: item.Label,
                                        value: item.Id
                                    }
                                }
                            }));
                        }

                        observable.PCASearchId(null);
                        observable.PCAContainer(null);

                        options.searchResultsList(results);
                    }

                    if (typeof options.afterSearch === 'function')
                        options.afterSearch();

                    observable.busy(false);
                },
                error: function (xhr, textStatus, errorThrown) {
                    observable.busy(false);
                    console.log('pcaPredict failure', errorThrown);
                    $(options.elementId || options.element).removeClass('ui-autocomplete-loading');
                    $(options.elementId || options.element).parent().find('.pca-failure').remove();
                    $(options.elementId || options.element).parent().append('<span class="pca-failure alert alert-danger">Lookup Failure. Please try again.</span>');
                }
            });
        }
    }

    $(options.element).autocomplete({
        appendTo: 'body',
        source: function (request, response) {

            if (ko.unwrap(options.country) || ko.unwrap(options.mode) == 'extant') {
                //if (ko.unwrap(options.country).hasOwnProperty('Code')) {
                $(options.elementId || options.element).parent().find('.pca-failure').remove();
                observable.performSearch(response);
                return;
                //}
            }

            $(options.elementId || options.element).removeClass('ui-autocomplete-loading');

            options.searchResultsList($.map([{
                id: '',
                label: options.resources.PleaseSelectCountryBefore,
                value: null,
                type: null,
                text: options.resources.PleaseSelectCountryBefore
            }], function (item) {
                return {
                    id: item.id,
                    label: item.label,
                    value: item.value,
                    type: item.type,
                    text: item.text,
                    obj: item
                }
            }));
            response();

        },
        open: function (event, ui) {
            $('.ui-autocomplete').off('menufocus hover mouseover');
        },
        minLength: typeof options.minLength === 'undefined' ? 1 : options.minLength, // this... this cannot be one character for Mertex purposes; we want the complete list.
        select: function (event, ui) {
            return false;
        },
        focus: function (event, ui) {
            event.preventDefault();
        },
        autoFocus: typeof options.autoFocus === 'undefined' ? true : options.autoFocus,
        delay: 500,
        position: options.position || { my: "left top", at: "left bottom", collision: "none" }
    });

    $(options.element).data("ui-autocomplete")._renderItem = function (ul, item) {
        return $('<li class="ui-menu-item">')
            .attr("data-value", item.value)
            .append('<div class="ui-menu-item-wrapper">' + item.label + '</div>')
            .appendTo(ul);
    }

    $(options.element).data("ui-autocomplete")._renderMenu = function (ul, items) {
        var that = this;
        $.each(items, function (index, item) {
            that._renderItemData(ul, item);
        });
        $(ul).addClass("address-autocomplete");
    };

}

ko.bindingHandlers.pcaPredict = {
    init: function (element, valueAccessor, allBindingsAccessor) {

        //console.log('pcaPredict binding handler init', element);

        var params = ko.unwrap(valueAccessor());
        var otherBindings = ko.unwrap(allBindingsAccessor());

        params.element = element;

        otherBindings.textInput.extend({
            pcaPredict: params
        });
    }
}

ko.bindingHandlers.removeSelf = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        $(element).remove();
    }
}

ko.bindingHandlers.pcaPredictList = {
    init: function (element, valueAccessor, allBindingsAccessor) {

        //console.log('pcaPredictList binding handler init', element);

        var params = ko.unwrap(valueAccessor());
        var otherBindings = ko.unwrap(allBindingsAccessor());

        params.element = element;

        otherBindings.textInput.extend({
            pcaPredictList: params
        });
    }
}

// Thanks to http://stackoverflow.com/questions/17048468/make-an-input-only-numeric-type-on-knockout
ko.bindingHandlers.numericWhole = {
    init: function (element, valueAccessor) {
        element.addEventListener('keydown', function (event) {

            // Allow: backspace, delete, tab, escape, and enter
            if (event.keyCode == 46 || event.keyCode == 8 || event.keyCode == 9 || event.keyCode == 27 || event.keyCode == 13 ||
                // Allow: Ctrl+A
                (event.keyCode == 65 && event.ctrlKey === true) ||
                // Allow: home, end, left, right, up, down
                (event.keyCode >= 35 && event.keyCode <= 40)) {

                // let the keypress occur
                return;
            }
            else {
                // Ensure that it is a number and stop the keypress
                if (event.shiftKey || (event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105)) {
                    event.preventDefault();
                }
            }
        });
    }
};

ko.bindingHandlers.phoneInput = {
    init: function (element, valueAccessor) {
        element.addEventListener('keydown', function (event) {

            // Allow: +, space, backspace, delete, tab, escape, and enter
            if (event.keyCode == 107 || (event.keyCode == 187 && event.shiftKey) || event.keyCode == 32 || event.keyCode == 46 || event.keyCode == 8 || event.keyCode == 9 || event.keyCode == 27 || event.keyCode == 13 ||
                // Allow: Ctrl+A
                (event.keyCode == 65 && (event.ctrlKey === true || event.metaKey === true)) ||
                // Allow: Ctrl+V
                (event.keyCode == 86 && (event.ctrlKey === true || event.metaKey === true)) ||
                // Allow: home, end, left, right, up, down
                (event.keyCode >= 35 && event.keyCode <= 40)) {

                // let the keypress occur
                return;
            }
            else {
                // Ensure that it is a number and stop the keypress
                if (event.shiftKey || (event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105)) {
                    event.preventDefault();
                }
            }
        });
    }
};

// Thanks to http://stackoverflow.com/questions/17048468/make-an-input-only-numeric-type-on-knockout
ko.bindingHandlers.numeric = {
    init: function (element, valueAccessor) {

        var val = valueAccessor();

        element.addEventListener('keydown', function (event) {

            let currentValue = ko.unwrap(val);

            // Allow: backspace, delete, tab, escape, and enter
            if (event.keyCode == 46 || event.keyCode == 8 || event.keyCode == 9 || event.keyCode == 27 || event.keyCode == 13 ||
                // Allow: Ctrl+A
                (event.keyCode == 65 && event.ctrlKey === true) ||
                // Allow: both . keys (comma is event.keyCode == 188) if the Europeans complain about formatting... 
                ((event.keyCode == 190 || event.keyCode == 110) && currentValue?.toString().indexOf('.') == -1) ||
                // Allow: home, end, left, right
                (event.keyCode >= 35 && event.keyCode <= 39)) {

                // let the keypress occur
                return;
            }
            else {
                // Ensure that it is a number and stop the keypress
                if (event.shiftKey || (event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105)) {
                    event.preventDefault();
                }
            }
        });
    }
};

//http://knockoutjs.com/documentation/extenders.html
ko.extenders.roundedNumeric = function (target, precision) {
    //create a writable computed observable to intercept writes to our observable
    var result = ko.pureComputed({
        read: target,  //always return the original observables value
        write: function (newValue) {

            if (newValue === "")
                newValue = null;

            var current = target(),
                roundingMultiplier = Math.pow(10, precision),
                newValueAsNum = isNaN(newValue) ? 0 : +newValue,
                valueToWrite = Math.round(newValueAsNum * roundingMultiplier) / roundingMultiplier;

            valueToWrite = +Number(valueToWrite).toFixed(precision);

            // only write if it has changed

            if (newValue === null && current === null)
                return;

            if (newValue === null)
                valueToWrite = null;

            if (isNaN(newValue) && newValue !== null) {
                return;
            }


            // and if both new and current are not null
            //if (newValue !== null && current !== null) {
            if (valueToWrite !== current) {
                target(valueToWrite);
            } else {
                //if the rounded value is the same, but a different value was written, force a notification for the current field
                if (newValue !== current) {
                    target.notifySubscribers(valueToWrite);
                }
            }
            //}
        }
    }).extend({ notify: 'always' });

    //initialize with current value to make sure it is rounded appropriately
    result(target());

    //return the new computed observable
    return result;
};

ko.extenders.numberFormatted = function (target, options) {

    if (typeof options !== 'object') {
        console.error('numberFormatted extender requires options { digits: [N], currency: ko.observable({ FormattingCulture: "[en-GB]" }) }');
    }
    if (!options.hasOwnProperty('digits')) {
        console.error('numberFormatted extender requires options { digits: [N], currency: ko.observable({ FormattingCulture: "[en-GB]" }) }');
    }
    if (!options.hasOwnProperty('currency')) {
        console.error('numberFormatted extender requires options { digits: [N], currency: ko.observable({ FormattingCulture: "[en-GB]" }) }');
    }

    target.formatted = ko.computed({
        read: function () {
            var val = target();

            if (typeof val === 'undefined' || val == null)
                return val;

            if (ko.unwrap(options.currency)) {
                var formattedValue = formatNumber(val, options.digits, ko.unwrap(options.currency).FormattingCulture);

                return formattedValue;
            }
            else {
                return val;
            }
        },
        write: function (newValue) {
            var current = target();

            var valueToWrite = formatToNumberHelper(newValue, options.digits, options.maxValue, options.minValue, options.nearest);

            if (newValue === "")
                newValue = null;

            // only write if it has changed
            if (newValue === null && current === null)
                return;

            if (newValue === null)
                valueToWrite = null;


            //only write if it changed
            if (valueToWrite !== current) {
                target(valueToWrite);
                //target.notifySubscribers(valueToWrite);
            } else {
                if (newValue !== current) {
                    target.notifySubscribers(valueToWrite);
                }
            }
        }
    }).extend({ notify: 'always' });
    return target;
};

function formatToNumberHelper(str, decimals, maxValue, minValue, nearest) {
    var roundingMultiplier = Math.pow(10, decimals);

    //Strip out everything except numbers and decimals (also strip out '%' and 'R')
    var newValueAsNum = new String(str).replace(/[^0-9\.]/g, '');
    if (isNaN(newValueAsNum)) {
        //can happen with two decimals.
        newValueAsNum = 0;
    }

    var valueToWrite = null;

    if (nearest == null)
        valueToWrite = Math.round(newValueAsNum * roundingMultiplier) / roundingMultiplier;
    else {
        let offset = 1 / nearest;
        valueToWrite = (Math.round(newValueAsNum * offset) / offset).toFixed(decimals);
    }

    maxValue = ko.unwrap(maxValue);
    minValue = ko.unwrap(minValue);

    if (maxValue) {
        if (valueToWrite > maxValue)
            valueToWrite = maxValue;
    }

    if (minValue) {
        if (valueToWrite < minValue)
            valueToWrite = minValue;
    }

    return valueToWrite;
}

//http://knockoutjs.com/documentation/extenders.html
ko.extenders.zxcvbn = function (observable, options) {
    options.restrictions = options.restrictions || [];
    //create a writable computed observable to intercept writes to our observable
    var zx = ko.pureComputed({
        read: function () {
            if (window.zxcvbn) {
                let result = zxcvbn(ko.unwrap(observable) || '', options.restrictions.map(function (r) { return ko.unwrap(r) }));
                Object.assign(result, {
                    min: ko.observable(0),
                    low: ko.observable(2),
                    high: ko.observable(3),
                    max: ko.observable(4),
                    optimum: ko.observable(4)
                })
                return result;
            } else {
                return null;
            }
        }
    }).extend({ notify: 'always' });

    observable.zxcvbn = zx;

    //return the new computed observable
    return observable;
};




ko.bindingHandlers.mdcTextField = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        if (!window.mdc) {
            console.error('mdcTextField binding handler: global mdc was not defined', element)
            return;
        }

        var params = valueAccessor();

        if (params.boundTo == null) {
            console.error('mdcTextField binding handler: observable was null or undefined', params);
        }

        if (ko.isObservable(params)) {
            params.mdcTextField = new mdc.textField.MDCTextField(element);
            params.mdcTextField.layout();
        } else {
            element.MDCTextField = new mdc.textField.MDCTextField(element);
            params.boundTo.mdcTextField = element.MDCTextField;
            if (params.hasOwnProperty('useNativeValidation')) {
                if (params.useNativeValidation == false && params.boundTo.hasOwnProperty('isValid')) {
                    params.boundTo.mdcTextField.useNativeValidation = params.useNativeValidation;
                    params.boundTo.isValid.subscribe(function (newValue) {
                        //console.log('isValid subscribe hit', newValue, element);
                        if (newValue === true) {
                            params.boundTo.mdcTextField.valid = true;
                        }
                        if (newValue === false) {
                            params.boundTo.mdcTextField.valid = false;
                        }
                    });
                }
            }
            params.boundTo.mdcTextField.layout();
        }

        $('input', element).on('focus', function () {
            $('input', element).select();
        });

        //params.boundTo.subscribe(function (newValue) {
        //    if (newValue == null || newValue == '') {
        //        params.boundTo.mdcTextField.label_.float(false);
        //        params.boundTo.mdcTextField.getDefaultFoundation().adapter_.closeOutline();
        //    } else {
        //        params.boundTo.mdcTextField.label_.float(true);
        //        params.boundTo.mdcTextField.getDefaultFoundation().adapter_.notchOutline(params.boundTo.mdcTextField.getDefaultFoundation().adapter_.getLabelWidth() * 0.75);
        //    }
        //});

        //assignVisibilityCallback(element, function (visible) {
        //    //console.log('mdcTextField visibleCallback hit', visible, element);
        //    if (visible) {
        //        window.setTimeout(function () {
        //            if (params.boundTo() != null) {
        //                params.boundTo.mdcTextField.label_.float(true);
        //                params.boundTo.mdcTextField.getDefaultFoundation().adapter_.notchOutline(params.boundTo.mdcTextField.getDefaultFoundation().adapter_.getLabelWidth() * 0.75);
        //            } else {
        //                params.boundTo.mdcTextField.label_.float(false);
        //                params.boundTo.mdcTextField.getDefaultFoundation().adapter_.closeOutline();
        //            }
        //        }, 100);
        //    }
        //});
    }
}

ko.bindingHandlers.mdcSelect = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        if (!window.mdc) {
            console.error('mdcSelect binding handler: global mdc was not defined', element)
            return;
        }

        var params = valueAccessor();

        if (ko.isObservable(params)) {
            params.mdcSelect = new mdc.select.MDCSelect(element);
            params.mdcSelect.layout();
        } else {
            params.boundTo.mdcSelect = new mdc.select.MDCSelect(element);
            if (params.hasOwnProperty('useNativeValidation')) {
                if (params.useNativeValidation == false && params.boundTo.hasOwnProperty('isValid')) {
                    params.boundTo.mdcSelect.useNativeValidation = params.useNativeValidation;

                    params.boundTo.isValid.subscribe(function (newValue) {
                        if (newValue === true) {
                            params.boundTo.mdcSelect.valid = true;
                        }
                        if (newValue === false) {
                            params.boundTo.mdcSelect.valid = false;
                        }
                    });
                }
            }
            params.boundTo.mdcSelect.layout();
        }

        params.boundTo.subscribe(function (newValue) {
            if (newValue != null && newValue !== '') {
                if (!$(params.boundTo.mdcSelect.label_.root_).hasClass('mdc-floating-label--float-above')) {
                    window.setTimeout(function () {
                        params.boundTo.mdcSelect.label_.float(true);
                        params.boundTo.mdcSelect.getDefaultFoundation().adapter_.notchOutline(params.boundTo.mdcSelect.getDefaultFoundation().adapter_.getLabelWidth() * 0.75);
                    }, 50);
                }
            } else {
                if ($(params.boundTo.mdcSelect.label_.root_).hasClass('mdc-floating-label--float-above')) {
                    window.setTimeout(function () {
                        params.boundTo.mdcSelect.label_.float(false);
                        params.boundTo.mdcSelect.getDefaultFoundation().adapter_.closeOutline();
                    }, 50);
                }
            }
        });

        //assignVisibilityCallback(element, function (visible) {
        //    //console.log('mdcSelectComponent visibleCallback hit', visible, element);
        //    if (visible) {
        //        window.setTimeout(function () {
        //            if (params.boundTo() != null) {
        //                params.boundTo.mdcSelect.label_.float(true);
        //                params.boundTo.mdcSelect.getDefaultFoundation().adapter_.notchOutline(params.boundTo.mdcSelect.getDefaultFoundation().adapter_.getLabelWidth() * 0.75);
        //            } else {
        //                params.boundTo.mdcSelect.label_.float(false);
        //                params.boundTo.mdcSelect.getDefaultFoundation().adapter_.closeOutline();
        //            }
        //        }, 100);
        //    }
        //});
    }
}

/** New - see /home/testbed **/

ko.bindingHandlers.mdcTextFieldComponent = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        if (!window.mdc) {
            console.error('mdcTextFieldNew binding handler: global mdc was not defined', element)
            return;
        }

        var params = valueAccessor();

        element.MDCTextField = new mdc.textField.MDCTextField(element);

        params.boundTo.mdcTextField = element.MDCTextField;

        params.boundTo.mdcTextField.useNativeValidation = params.useNativeValidation;

        if (params.hasOwnProperty('disabled')) {
            if (ko.isObservable(params.disabled)) {
                params.disabled.subscribe(function (newValue) {
                    params.boundTo.mdcTextField.disabled = newValue;
                });
            }
            window.setTimeout(() => {
                params.boundTo.mdcTextField.disabled = ko.unwrap(params.disabled);
            }, 0);
        }

        if (params.useNativeValidation == false) {
            if (!params.boundTo.hasOwnProperty('isValid')) {
                //console.warn('mdcTextFieldComponent observable has no isValid - did you forget some validation?', element);
            } else {
                params.boundTo.isValid.extend({
                    notify: 'always'
                });
                params.boundTo.isValid.subscribe(function (newValue) {
                    if (newValue === true) {
                        params.boundTo.mdcTextField.valid = true;
                    }
                    if (newValue === false) {
                        params.boundTo.mdcTextField.valid = false;
                        if (!$(params.boundTo.mdcTextField.label_.root_).hasClass('mdc-floating-label--float-above')) {
                            params.boundTo.mdcTextField.label_.shake(true);
                        }
                    }
                });
            }
        }

        params.boundTo.subscribe(function (newValue) {
            if (newValue === '') {
                newValue = null;
            }
            if (newValue != null) {
                //if (!$(params.boundTo.mdcTextField.label_.root_).hasClass('mdc-floating-label--float-above')) {
                //window.setTimeout(function () {
                if (params.boundTo.mdcTextField.label_) {
                    params.boundTo.mdcTextField.label_.float(true);
                    params.boundTo.mdcTextField.getDefaultFoundation().adapter_.notchOutline(params.boundTo.mdcTextField.getDefaultFoundation().adapter_.getLabelWidth() * 0.8);
                }
                //}, 50);
                //}
            } else {
                //if ($(params.boundTo.mdcTextField.label_.root_).hasClass('mdc-floating-label--float-above')) {
                //window.setTimeout(function () {
                if (params.boundTo.mdcTextField.label_) {
                    params.boundTo.mdcTextField.label_.float(false);
                    params.boundTo.mdcTextField.getDefaultFoundation().adapter_.closeOutline();
                }
                //}, 50);
                //}
            }
        });

        if (params.boundTo() != null && params.boundTo() != '') {
            // We're starting with some value; float the label!
            if (params.boundTo.mdcTextField.label_) {
                params.boundTo.mdcTextField.label_.float(true);
            }
        }

        window.setTimeout(function () {
            params.boundTo.mdcTextField.layout();
        }, 0);

        //assignVisibilityCallback(element, function (visible) {
        //    //console.log('mdcTextField visibleCallback hit', visible, element);
        //    if (visible) {
        //        window.setTimeout(function () {
        //            if (params.boundTo() != null && params.boundTo() != '') {
        //                params.boundTo.mdcTextField.label_.float(true);
        //                params.boundTo.mdcTextField.getDefaultFoundation().adapter_.notchOutline(params.boundTo.mdcTextField.getDefaultFoundation().adapter_.getLabelWidth() * 0.75);
        //            } else {
        //                params.boundTo.mdcTextField.label_.float(false);
        //                params.boundTo.mdcTextField.getDefaultFoundation().adapter_.closeOutline();
        //            }
        //        }, 100);
        //    }
        //});

        //$('input', element).on('focus', function () {
        //    $('input', element).select();
        //});
    }
}

ko.bindingHandlers.mdcSelectComponent = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        if (!window.mdc) {
            console.error('mdcSelectComponent binding handler: global mdc was not defined', element)
            return;
        }

        var params = valueAccessor();

        //console.log('init mdcSelectComponent for', $(element).parent());

        element.MDCSelect = new mdc.select.MDCSelect(element);

        params.boundTo.mdcSelect = element.MDCSelect;

        params.boundTo.mdcSelect.useNativeValidation = params.useNativeValidation;

        if (params.hasOwnProperty('disabled')) {
            if (ko.isObservable(params.disabled)) {
                params.disabled.subscribe(function (newValue) {
                    params.boundTo.mdcSelect.disabled = newValue;
                });
            }
            params.boundTo.mdcSelect.disabled = ko.unwrap(params.disabled);
        }

        if (params.useNativeValidation == false) {
            if (!params.boundTo.hasOwnProperty('isValid')) {
                //console.warn('mdcSelectComponent observable has no isValid - did you forget some validation?', element);
            } else {
                params.boundTo.isValid.extend({
                    notify: 'always'
                });
                params.boundTo.isValid.subscribe(function (newValue) {
                    if (newValue === true) {
                        params.boundTo.mdcSelect.valid = true;
                    }
                    if (newValue === false) {
                        params.boundTo.mdcSelect.valid = false;
                        // this works but it looks wrong if the label is already floated...
                        // can we get isFloating?
                        if (!$(params.boundTo.mdcSelect.label_.root_).hasClass('mdc-floating-label--float-above')) {
                            params.boundTo.mdcSelect.label_.shake(true);
                        }
                    }
                });
            }
        }

        params.boundTo.subscribe(function (newValue) {
            if (newValue != null && newValue !== '') {
                //if (!$(params.boundTo.mdcSelect.label_.root_).hasClass('mdc-floating-label--float-above')) {
                    window.setTimeout(function () {
                        params.boundTo.mdcSelect.label_.float(true);
                        params.boundTo.mdcSelect.getDefaultFoundation().adapter_.notchOutline(params.boundTo.mdcSelect.getDefaultFoundation().adapter_.getLabelWidth() * 0.8);
                    }, 50);
                //}
            } else {
                if ($(params.boundTo.mdcSelect.label_.root_).hasClass('mdc-floating-label--float-above')) {
                    window.setTimeout(function () {
                        params.boundTo.mdcSelect.label_.float(false);
                        params.boundTo.mdcSelect.getDefaultFoundation().adapter_.closeOutline();
                    }, 50);
                }
            }
        });

        //window.setTimeout(function () {
        //    params.boundTo.mdcSelect.layout();
        //}, 0);

        //assignVisibilityCallback(element, function (visible) {
        //    //console.log('mdcSelectComponent visibleCallback hit', visible, element);
        //    if (visible) {
        //        window.setTimeout(function () {
        //            if (params.boundTo() != null && params.boundTo() != '') {
        //                if (params.boundTo().hasOwnProperty(params.optionsValue)) {
        //                    if (params.boundTo()[params.optionsValue] != null) {
        //                        params.boundTo.mdcSelect.label_.float(true);
        //                        params.boundTo.mdcSelect.getDefaultFoundation().adapter_.notchOutline(params.boundTo.mdcSelect.getDefaultFoundation().adapter_.getLabelWidth() * 0.75);
        //                    } else {
        //                        params.boundTo.mdcSelect.label_.float(false);
        //                        params.boundTo.mdcSelect.getDefaultFoundation().adapter_.closeOutline();
        //                    }
        //                } else {
        //                    if (params.boundTo() != null) {
        //                        params.boundTo.mdcSelect.label_.float(true);
        //                        params.boundTo.mdcSelect.getDefaultFoundation().adapter_.notchOutline(params.boundTo.mdcSelect.getDefaultFoundation().adapter_.getLabelWidth() * 0.75);
        //                    } else {
        //                        params.boundTo.mdcSelect.label_.float(false);
        //                        params.boundTo.mdcSelect.getDefaultFoundation().adapter_.closeOutline();
        //                    }
        //                }
        //            }
        //        }, 100);
        //    }
        //});
    }
}

ko.bindingHandlers.mdcKendoComboBox = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        if (!window.mdc) {
            console.error('mdcKendoComboBox binding handler: global mdc was not defined', element)
            return;
        }

        // this has already let the kendoComboBox binding handler do its thing, knockout-kendo to the rescue
        // now need to tackle the gobetween from kendoComboBox widget to neighbouring mdcSelect

        var params = valueAccessor();

        params.value.DOMElement = params.widget().input;

        function labelHandler(event) {
            params.value(event.currentTarget.value);

            if (!$(params.value.mdcSelect.label_.root_).hasClass('mdc-floating-label--float-above')) {
                window.setTimeout(function () {
                    params.value.mdcSelect.label_.float(true);
                    params.value.mdcSelect.getDefaultFoundation().adapter_.notchOutline(params.value.mdcSelect.getDefaultFoundation().adapter_.getLabelWidth() * 0.75);
                }, 50);
            }
            if (params.value.hasOwnProperty('isValid')) {
                if (params.value.isValid())
                    $(params.value.mdcSelect.label_.root_).removeClass('mdc-select--invalid');
                else
                    $(params.value.mdcSelect.label_.root_).addClass('mdc-select--invalid');
            }
        }

        function labelBlurHandler(event) {
            if (!params.value()) {
                params.value.mdcSelect.label_.float(false);
                params.value.mdcSelect.getDefaultFoundation().adapter_.closeOutline();
            }
        }

        ko.utils.registerEventHandler(params.widget().input, 'keyup', labelHandler);
        ko.utils.registerEventHandler(params.widget().input, 'focus', labelHandler);
        ko.utils.registerEventHandler(params.widget().input, 'blur', labelBlurHandler);


        if (params.value.hasOwnProperty('isValid')) {
            params.value.isValid.subscribe(function (newValue) {
                if (newValue) {

                } else {
                    params.value.mdcSelect.label_.shake(true);
                }
            });

        }
    }
}

ko.bindingHandlers.intlMobile = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        var params = valueAccessor();

        //params.phone.intlTel = $(element).intlTelInput({
        if (window.intlTelInput) {
            params.phone.intlTel = window.intlTelInput(element, {
                //utilsScript: '/Scripts/int-tel/utils.js',
                dropdownContainer: document.body,
                initialCountry: params.country() ? params.country().Code : undefined,
                preferredCountries: params.preferredCountries,
                //customPlaceholder: function (selectedCountryPlaceholder, selectedCountryData) {
                //    return '';
                //}
            });
        }

        params.phone.subscribe(function (newValue) {
            params.phone.isValid.notifySubscribers(true);
        });
    }
}

ko.bindingHandlers['dynamicHtml'] = {
    'update': function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        // setHtml will unwrap the value if needed
        ko.utils.setHtml(element, valueAccessor());
        ko.applyBindingsToDescendants(bindingContext, element);
    }
};

function countDecimals(value) {
    if (Math.floor(value) !== value) {
        return value.toString().split('.')[1].length || 0;
    }
    return 0;
}

ko.bindingHandlers.fadeBetweenMany = {
    init: function (element, valueAccessor) {
        var value = valueAccessor();
        var unwrapped = ko.unwrap(value);
        // so inside the valueAccessor we get the object provided for this bindingHandler
        // I've defined "condition" which is the function to evaluate
        //              "visibleWhen" which is the state we need element to be visible in
        //              "swapWith" which is the element which needs to be hidden when we're visible.
        //              - because you might be providing part of the selector at binding time, I'm no longer testing for
        //                the existence of $(swapWith) in the init function.
        if (!ko.isObservable(unwrapped.observable)) {
            console.error('fadeBetweenMany bindingHandler error - parameter "observable" is not an observable', element, unwrapped);
        }
        if (!typeof ko.hasOwnProperty(unwrapped.visibleWhen)) {
            console.error('fadeBetweenMany bindingHandler error - parameter "visibleWhen" is not present', element, unwrapped);
        }
        if (!typeof ko.hasOwnProperty(unwrapped.swapWith)) {
            console.error('fadeBetweenMany bindingHandler error - parameter "swapWith" is not present', element, unwrapped);
        }
    },
    update: function (element, valueAccessor) {
        var value = ko.unwrap(valueAccessor());

        if (typeof value.visibleWhen === 'function') {
            if (value.visibleWhen(ko.unwrap(value.observable))) {
                $(element).parent().find(value.swapWith).css('position', 'absolute').hide().stop().fadeOut(100);
                $(element).css('position', 'static').stop().fadeIn('fast');
            } else {
                // do not fade others in: that's their business, not yours
                //$(value.swapWith).css('position', 'static').stop().fadeIn();
                $(element).css('position', 'absolute').hide().stop().fadeOut(100);
            }
        } else {
            if (ko.unwrap(value.observable) == value.visibleWhen) {
                $(element).parent().find(value.swapWith).css('position', 'absolute').hide().stop().fadeOut(100);
                $(element).css('position', 'static').stop().fadeIn('fast');
            } else {
                $(element).css('position', 'absolute').hide().stop().fadeOut(100);
            }
        }
    }
}

// ta: https://stackoverflow.com/questions/17983118/change-observable-but-dont-notify-subscribers-in-knockout-js#answer-37961239
ko.observable.fn.silentUpdate = function (value) {
    this.notifySubscribers = function () { };
    this(value);
    this.notifySubscribers = function () {
        ko.subscribable.fn.notifySubscribers.apply(this, arguments);
    };
};

// http://eonasdan.github.io/bootstrap-datetimepicker/Installing/#knockout
// modified to handle nulls.
ko.bindingHandlers.dateTimePicker = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        //initialize datepicker with some optional 

        var options = allBindingsAccessor().dateTimePickerOptions || {
            keepOpen: true
        };

        $(element).datetimepicker(options);

        //when a user changes the date, update the view model
        ko.utils.registerEventHandler(element, "dp.change", function (event) {
            var value = valueAccessor();
            if (ko.isObservable(value)) {
                if (event.date != null && !(event.date instanceof Date)) {
                    if (event.date.hasOwnProperty("toDate")) {
                        value(event.date.toDate());
                    } else {
                        if (event.date != false) {
                            value(event.date);
                        } else {
                            // clear() in bootstrap-datetimepicker.js sets event.date to false.
                            value(undefined);
                        }
                    }
                } else {
                    if (event.date != false) {
                        value(event.date);
                    }
                }
            }
        });

        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
            var picker = $(element).data("DateTimePicker");
            if (picker) {
                picker.destroy();
            }
        });
    },
    update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {

        var picker = $(element).data("DateTimePicker");
        //when the view model is updated, update the widget
        if (picker) {
            var koDate = ko.utils.unwrapObservable(valueAccessor());

            if (koDate != null && koDate !== false) {

                if (koDate.toString().indexOf('Date') > -1)
                    koDate = (typeof (koDate) !== 'object') ? new Date(parseFloat(koDate.replace(/[^0-9]/g, ''))) : koDate;
                else
                    koDate = moment(koDate).format(allBindings().dateTimePickerOptions.format);
                //in case return from server datetime i am get in this form for example /Date(93989393)/ then fomat this
                // koDate = (typeof (koDate) !== 'object') ? new Date(parseFloat(koDate.replace(/[^0-9]/g, ''))) : koDate;
            }

            if (typeof koDate === "undefined") {
                koDate = null;
            }

            if (koDate === false) {
                koDate = null;
            }

            picker.date(koDate);
        }
    }
};

ko.bindingHandlers.fadeSlideVisible = {
    init: function (element, valueAccessor) {
        // Initially set the element to be instantly visible/hidden depending on the value
        var value = valueAccessor();
        $(element).toggle(ko.unwrap(value)); // Use "unwrapObservable" so we can handle values that may or may not be observable
    },
    update: function (element, valueAccessor) {
        // Whenever the value subsequently changes, slowly fade the element in or out
        var value = valueAccessor();
        ko.unwrap(value) ?
            $(element).stop(true, true).fadeIn({ queue: false }).css('display', 'none').slideDown() :
            $(element).stop(true, true).fadeOut({ queue: false }).slideUp();
    }
};

if (typeof ko.validation !== 'undefined') {
    ko.validation.rules['noPOBox'] = {
        validator: function (val, validate, message) {
            if (!validate) return true;
            if (ko.validation.utils.isEmptyVal(val)) return true;

            if (val) {
                val = val.toUpperCase();
            }
            var regexes = [
                /PO BOX/,
                /P\.O\. BOX/,
                /P\.O BOX/,
                /PO\.BOX/,
                /PO\. BOX/,
                /P\.O/,
                /P\.O\./,
                /P O BOX/,
                /POBOX/,
                /P\.O\.BOX/,
                /POST OFFICE BOX/,
                /POSTOFFICEBOX/,
                /P\.O\./,
            ];

            var firstMatch = ko.utils.arrayFirst(regexes, function (r) {
                return val.match(r);
            });

            return firstMatch == null;
        },
        message: 'No P.O. Boxes!'
    };
    ko.validation.addExtender('noPOBox');
}

ko.bindingHandlers.fadeSwapVisible = {
    init: function (element, valueAccessor) {
        var value = valueAccessor();
        var unwrapped = ko.unwrap(value);
        // so inside the valueAccessor we get the object provided for this bindingHandler
        // I've defined "boolean" which is the value to watch
        //              "visibleWhen" which is the state we need element to be visible in
        //              "swapWith" which is the element which needs to be hidden when we're visible.
        if (!typeof ko.unwrap(unwrapped.boolean) === 'boolean') {
            console.error('fadeSwapVisible bindingHandler error - parameter "boolean" is not a boolean', element, unwrapped);
        }
        if (!typeof ko.unwrap(unwrapped.visibleWhen) === 'boolean') {
            console.error('fadeSwapVisible bindingHandler error - parameter "visibleWhen" is not a boolean', element, unwrapped);
        }
        if ($(unwrapped.swapWith).length == 0) {
            console.error('fadeSwapVisible bindingHandler error - parameter "swapWith" was not a found element', element, unwrapped);
        }
    },
    update: function (element, valueAccessor) {
        var value = ko.unwrap(valueAccessor());
        if (ko.unwrap(value.boolean) == value.visibleWhen) {
            $(element).parent().find(value.swapWith).css('position', 'absolute').hide().stop().fadeOut();
            $(element).css('position', 'static').stop().fadeIn();
        } else {
            $(element).parent().find(value.swapWith).css('position', 'static').stop().fadeIn();
            $(element).css('position', 'absolute').hide().stop().fadeOut();
        }
    }
}

ko.bindingHandlers.selectOnFocus = {
    init: function (element) {
        element.addEventListener('focusin', function () {
            element.select();
        });
    }
}

ko.bindingHandlers.remove = {
    init: function (element) {
        element.remove();
    }
}

ko.bindingHandlers.flexVisible = {
    init: function (element, valueAccessor) {
        var value = valueAccessor();
        var unwrapped = ko.unwrap(value);
        if (!typeof ko.unwrap(unwrapped.boolean) === 'boolean') {
            console.error('flexVisible bindingHandler error - parameter "boolean" is not a boolean', element, unwrapped);
        }
        if (value) {
            element.style.display = 'flex';
        }
    },
    update: function (element, valueAccessor) {
        var value = ko.unwrap(valueAccessor());
        if (value) {
            element.style.display = 'flex';
        } else {
            element.style.display = 'none';
        }
    }
}


ko.bindingHandlers.inlineFlexVisible = {
    init: function (element, valueAccessor) {
        var value = valueAccessor();
        var unwrapped = ko.unwrap(value);
        if (!typeof ko.unwrap(unwrapped.boolean) === 'boolean') {
            console.error('inlineFlexVisible bindingHandler error - parameter "boolean" is not a boolean', element, unwrapped);
        }
        if (value) {
            element.style.display = 'inline-flex';
        }
    },
    update: function (element, valueAccessor) {
        var value = ko.unwrap(valueAccessor());
        if (value) {
            element.style.display = 'inline-flex';
        } else {
            element.style.display = 'none';
        }
    }
}

ko.bindingHandlers.addressAutocompleteKeyPress = {
    init: function (element, valueAccessor) {
        element.addEventListener('keydown', function (event) {
            // tab and down arrow should focus the first li
            if (event.keyCode == 9) {
                if (element.parentElement.querySelectorAll('.address-autocomplete > li').length) {
                    element.parentElement.querySelector('.address-autocomplete > li:first-of-type').focus();
                }
            }
            return;
        });
    }
}


ko.bindingHandlers.stopBinding = {
    init: function () {
        return { controlsDescendantBindings: true };
    }
}