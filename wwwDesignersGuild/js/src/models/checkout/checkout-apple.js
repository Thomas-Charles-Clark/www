const LOG_ACTION_APPLE = 35,
    LOG_ACTION_PAY_NOW_CLICK = 136,
    LOG_ACTION_PAY_NOW_STEP = 138,
    LOG_ACTION_PAY_NOW_VALIDATION_ERROR = 139,
    LOG_ACTION_APPLE_BEGIN = 170,
    LOG_ACTION_APPLE_ONVALIDATEMERCHANT = 171,
    LOG_ACTION_APPLE_ONVALIDATEMERCHANT_ERROR = 172,
    LOG_ACTION_APPLE_ONVALIDATEMERCHANT_SUCCESS = 173,
    LOG_ACTION_APPLE_ONSHIPPINGCONTACTSELECTED = 174,
    LOG_ACTION_APPLE_COMPLETESHIPPINGCONTACTSELECTION = 175,
    LOG_ACTION_APPLE_ONSHIPPINGMETHODSELECTED = 176,
    LOG_ACTION_APPLE_COMPLETESHIPPINGMETHODSELECTION = 177,
    LOG_ACTION_APPLE_ONPAYMENTAUTHORIZED = 178,
    LOG_ACTION_APPLE_PAYMENTTOKENIZATION_ERROR = 179,
    LOG_ACTION_APPLE_PAYMENTTOKENIZATION_SUCCESS = 180,
    LOG_ACTION_APPLE_COMPLETEPAYMENT = 181,
    LOG_ACTION_APPLE_ONCANCEL = 182,
    LOG_ACTION_APPLE_SAVEDELIVERYADDRESS_ERROR = 183

/* 
    This is the Apple Pay View Model
    It is invoked by _Login.cshtml, if the current checkout is permitted by the server to attempt Apple Pay (ecm.CheckoutType_Select governs this)
    The Braintree client instance manifests us an applePayInstance, and 'parent' is the Checkout Login Model 
    (window.clm, also pageModel.fixedPage().this.checkoutLoginModel which is why the shortcut...
*/
class ApplePayViewModel { 

    //#region properties

    // these are the despatch options delivered via _checkoutdespatch.cshtml. jquery is used to render the html onto the hidden container element, 
    // so the script tags get processed into existence and we can call the functions therein to pull the data.
    dispatchoptionsfromserver = ko.observablearray([]);

    // these are necessary because apple in their wisdom don't allow merchants to require multiple delivery option selections.
    consolidateddispatchoptionsfromserver = ko.observablearray([]);

    // this is the subtotal, lifted from getbalancenumber which first exists on _login.cshtml and is then overwritten by _checkouttotal.cshtml
    subtotal = ko.observable(getbalancenumber());

    // this is the shipping, rendered unto us by _checkouttotal.cshtml eventually
    shipping = ko.observable(0);

    // this is the shipping, rendered unto us by _checkouttotal.cshtml eventually
    tax = ko.observable(0);

    // this is the shipping, rendered unto us by _checkouttotal.cshtml eventually
    total = ko.observable(0);

    // this is the currently selected shipping method
    selectedshippingmethod = null;

    // this is the currently selected delivery address
    selecteddeliveryaddress = null;

    //#endregion

    constructor(applepayinstance, parent) 
    {
        this.applepayinstance = applepayinstance;
        this.parent = parent;
        this.resources = parent.resources;

        this.onshippingcontactselected = this.onshippingcontactselected.bind(this);
        this.onshippingmethodselected = this.onshippingmethodselected.bind(this);
        this.onvalidatemerchant = this.onvalidatemerchant.bind(this);
        this.onvalidatemerchantresponse = this.onvalidatemerchantresponse.bind(this);
        this.onpaymentauthorized = this.onpaymentauthorized.bind(this);
        this.oncancel = this.oncancel.bind(this);

        let paymentrequestparams = {
            total: {
                label: 'designers guild',
                type: 'pending',
                amount: this.total(),
                lineitems: [{
                    label: 'sub total',
                    type: 'pending',
                    amount: this.subtotal()
                }, {
                    label: 'delivery',
                    type: 'pending',
                    amount: this.shipping()
                }]
            },
            shippingtype: 'delivery',
            shippingcontacteditingmode: 'enabled',
            // we recommend collecting billing address information, at minimum
            // billing postal code, and passing that billing postal code with
            // all apple pay transactions as a best practice.
            applepayshippingtype: 'delivery',
            requiredbillingcontactfields: ["postaladdress"],
            requiredshippingcontactfields: [
                "postaladdress",
                "email"
            ]
        }

        if (this.tax()) {
            paymentrequestparams.total.lineitems.push({
                label: 'tax',
                type: 'pending',
                amount: this.tax()
            });
        }

        this.applepaymentrequest = this.applepayinstance.createpaymentrequest(paymentrequestparams);

        this.checkoutlog(log_action_apple_begin, json.stringify(paymentrequestparams));

        // this is necessary because i have to await setcheckouttype returning successfully
        // before invoking the applepay sheet.
        document.body.addeventlistener('applego', () => {
            this.activeapplepaysession.begin();
        });

        console.log('applepayviewmodel ctor called', this);
    }

    //#region methods

    // this is necessary because you have to create an applepaysession from a user guesture handler,
    // see _login.cshtml proceedtocheckout
    init() {
        this.activeapplepaysession = new applepaysession(3, this.applepaymentrequest);

        this.activeapplepaysession.onvalidatemerchant = this.onvalidatemerchant;

        this.activeapplepaysession.onshippingcontactselected = this.onshippingcontactselected;

        this.activeapplepaysession.onshippingmethodselected = this.onshippingmethodselected;

        this.activeapplepaysession.onpaymentauthorized = this.onpaymentauthorized;

        this.activeapplepaysession.oncancel = this.oncancel;
    }

    // to log things with
    checkoutlog(type, detail) {
        console.log('applepayviewmodel checkoutlog', type, detail);

        $.ajax({
            url: '/api/checkout/asyncsavecheckoutlog',
            type: 'post',
            async: false,
            cache: false,
            data: {
                action: type,
                detail: detail || "no detail provided"
            },
            success: (function (data) {

            }).bind(this),
            error: (function (xhr, s, a) {
                console.error("checkout logging failed!");
            }).bind(this)
        });
    }

    // callback handler for the onshippingcontactselected apple event
    onshippingcontactselected(e) {
        console.log('applepayviewmodel onshippingcontactselected', e);

        this.checkoutlog(log_action_apple_onshippingcontactselected, json.stringify(e));

        this.savedeliveryaddress({
            deliveryoptionid: 1,
            memberaddressid: null,
            fullname: 'temporary',
            companyname: null,
            addressline1: 'temporary',
            addressline2: null,

            town: e.shippingcontact.locality,
            county: e.shippingcontact.administrativearea,
            postcode: e.shippingcontact.postalcode,
            countrycode: e.shippingcontact.countrycode,
            basketcountry: getsitecountry(),
            phonenumber: null,
            upsapid: null,
            upsapmemberaddressid: null,
            istradeaddress: false,
            isclickcollect: false,
            masterdefinition: getmasterdefinition()
        }, (result) => {

            if (result.state === 'ok') {

                this.refreshdeliveryandtotals(() => {

                    if (this.availableshippingmethods.length == 1) {

                        this.selectedshippingmethod = this.availableshippingmethods[0];

                        this.savedispatch({
                            shippingmethod: this.selectedshippingmethod
                        }, () => {

                            this.checkoutlog(log_action_apple_completeshippingcontactselection, json.stringify(this.completeshippingcontactmodel));

                            console.log('applepayviewmodel calling completeshippingcontactselection', this.completeshippingcontactmodel);

                            this.activeapplepaysession.completeshippingcontactselection(this.completeshippingcontactmodel);
                        });

                    } else {

                        this.checkoutlog(log_action_apple_completeshippingcontactselection, json.stringify(this.completeshippingcontactmodel));

                        console.log('applepayviewmodel calling completeshippingcontactselection', this.completeshippingcontactmodel);

                        this.activeapplepaysession.completeshippingcontactselection(this.completeshippingcontactmodel);
                    }
                });
            } else {

                if (result.message === "checkoutbasketexpired") {
                    jumptosessionexpired();
                    return;
                }

                let errors = [];

                if (result.message === 'invalidcountry') {
                    errors.push(new applepayerror('shippingcontactinvalid', 'country', `${this.resources.cannotdelivertothatcountry}`));
                }

                if (errors.length == 0) {

                    this.refreshdeliveryandtotals(() => {

                        if (result.hasownproperty('issalestaxknown')) {

                            if (result.issalestaxknown) {
                                oncomplete.apply(this);
                                return;
                            } else {

                                if (result.message === "invalidzipcode") {
                                    errors.push({
                                        code: "shippingcontactinvalid",
                                        contactfield: 'postalcode',
                                        message: this.resources.checkoutaddresszipcodeinvalid
                                    });
                                } else if (result.message === "ziptaxservicefailed") {
                                    errors.push({
                                        code: "shippingcontactinvalid",
                                        contactfield: 'postalcode',
                                        message: this.resources.checkoutaddresszipcodeproblem
                                    });
                                } else if (result.salestaxaddresslist) {

                                    errors.push(new applepayerror(
                                        'shippingcontactinvalid',
                                        'postaladdress',
                                        `${this.resources.ziptaxcouldnotdetermine}`
                                    ));

                                    let addresslines = `"${result.salestaxaddresslist.flatmap(s => s.addressline).join('", "')}"`;
                                    let towns = `"${result.salestaxaddresslist.flatmap(s => s.town).join('", "')}"`;
                                    let states = `"${result.salestaxaddresslist.flatmap(s => s.state).join('", "')}"`;
                                    let counties = `"${result.salestaxaddresslist.flatmap(s => s.county).join('", "')}"`;

                                    errors.push(new applepayerror(
                                        'shippingcontactinvalid',
                                        'addresslines',
                                        `${addresslines}?`
                                    ));

                                    errors.push(new applepayerror(
                                        'shippingcontactinvalid',
                                        'locality',
                                        `${towns}?`
                                    ));

                                    errors.push(new applepayerror(
                                        'shippingcontactinvalid',
                                        'administrativearea',
                                        `${states}?`
                                    ));

                                    errors.push(new applepayerror(
                                        'shippingcontactinvalid',
                                        'subadministrativearea',
                                        `${counties}?`
                                    ));
                                }
                            }
                        }

                        let completeshippingcontactmodel = this.completeshippingcontactmodel;

                        completeshippingcontactmodel.errors = errors;

                        this.checkoutlog(log_action_apple_savedeliveryaddress_error, json.stringify(object.assign(result, completeshippingcontactmodel)));

                        console.log('applepayviewmodel calling completeshippingcontactselection', completeshippingcontactmodel);

                        this.activeapplepaysession.completeshippingcontactselection(completeshippingcontactmodel);
                    });
                } else {

                    let completeshippingcontactmodel = {
                        'status': applepaysession.status_success,
                        'newtotal': {
                            'type': 'pending',
                            'label': 'designers guild',
                            'amount': `${this.total().tofixed(2)}`
                        },
                        'newlineitems': [{
                            'label': 'sub total',
                            'type': 'pending',
                            'amount': `${this.subtotal().tofixed(2)}`
                        }, {
                            'label': 'delivery',
                            'type': 'pending',
                            'amount': `${this.shipping().tofixed(2)}`
                        }],
                        errors: errors
                    };

                    this.checkoutlog(log_action_apple_savedeliveryaddress_error, json.stringify(object.assign(result, completeshippingcontactmodel)));

                    console.log('applepayviewmodel calling completeshippingcontactselection', completeshippingcontactmodel);

                    this.activeapplepaysession.completeshippingcontactselection(completeshippingcontactmodel);

                }
            }
        });
    }

    onshippingmethodselected(e) {
        console.trace(e);

        this.checkoutlog(log_action_apple_onshippingmethodselected, json.stringify(e));

        this.selectedshippingmethod = e.shippingmethod;

        window.getshippingnumber = () => {
            return +e.shippingmethod.amount;
        }

        this.savedispatch(e, () => {
            this.fetchtotals(() => {

                let completionmodel = this.completeshippingcontactmodel;

                completionmodel.status = applepaysession.status_success;

                this.checkoutlog(log_action_apple_completeshippingmethodselection, json.stringify(completionmodel));

                this.activeapplepaysession.completeshippingmethodselection(completionmodel);
            });
        });
    }

    onvalidatemerchant(e) {
        console.trace(e);

        this.applepayinstance.performvalidation({
                validationurl: e.validationurl,
                displayname: 'designers guild'
            },
            this.onvalidatemerchantresponse);
    };

    onvalidatemerchantresponse(err, merchantsession) {
        if (err) {
            this.abortapple();
            window.showinfo(this.resources.sorry, this.resources.applepayfailedtoload);
            this.checkoutlog(log_action_apple_onvalidatemerchant_error, json.stringify(err));
            return;
        }
        console.trace(merchantsession);

        this.checkoutlog(log_action_apple_onvalidatemerchant_success, json.stringify(merchantsession));

        this.activeapplepaysession.completemerchantvalidation(merchantsession);
    }

    oncancel(e) {
        this.hidecompletingmessage();
        this.parent.busy(false);
        this.checkoutlog(log_action_apple_oncancel, this.resources.applepayfailurenotcharged);
        window.showinfo(this.resources.sorry, this.resources.applepayfailurenotcharged);
    }

    onpaymentauthorized(e) {
        console.trace(e);

        this.checkoutlog(log_action_apple_onpaymentauthorized, json.stringify(e));

        this.applepayinstance.tokenize({
            token: e.payment.token
        }, function (tokenizeerr, payload) {
            if (tokenizeerr) {
                this.hidecompletingmessage();
                console.error('applepayviewmodel error tokenizing:', tokenizeerr);

                this.checkoutlog(log_action_apple_paymenttokenization_error, json.stringify(tokenizeerr));

                window.showinfo(this.resources.sorry, this.resources.applepayfailurenotcharged);

                this.activeapplepaysession.completepayment(applepaysession.status_failure);

                return;
            }

            console.trace(payload);

            this.checkoutlog(log_action_apple_paymenttokenization_success, json.stringify(payload));

            let fullname = `${e.payment.shippingcontact.givenname} ${e.payment.shippingcontact.familyname}`.trim();
            let addressline1 = null;
            let addressline2 = null;

            if (e.payment.shippingcontact.addresslines.length >= 1) {
                addressline1 = e.payment.shippingcontact.addresslines[0];
            }

            if (e.payment.shippingcontact.addresslines.length >= 2) {
                addressline2 = e.payment.shippingcontact.addresslines[1];
            }

            this.savedeliveryaddress({
                deliveryoptionid: 1,
                memberaddressid: null,
                fullname: fullname,
                companyname: null,
                addressline1: addressline1,
                addressline2: addressline2,
                town: e.payment.shippingcontact.locality,
                county: e.payment.shippingcontact.administrativearea,
                postcode: e.payment.shippingcontact.postalcode,
                countrycode: e.payment.shippingcontact.countrycode,
                basketcountry: getsitecountry(),
                phonenumber: null,
                upsapid: null,
                upsapmemberaddressid: null,
                istradeaddress: false,
                isclickcollect: false,
                masterdefinition: getmasterdefinition(),
                secondsave: true
            }, (result) => {
                this.submit(payload.nonce, e.payment.shippingcontact.emailaddress);
            });
        });
    }

    // this function fetches _checkoutdespatch.cshtml from the server, renders it into a hidden element and then lifts delivery options.
    // oncomplete is called as a callback if provided upon success.
    fetchdeliverymethods(oncomplete) {

        let data = {};

        console.trace(data);

        $.ajax({
            url: geturlcheckoutdeliveryoptions(),
            type: "get",
            data: data,
            datatype: "html",
            success: (function (data) {

                console.log('applepayviewmodel fetchdeliverymethods returned', data);

                var deliveryoptions = $('#despatch-items-wrapper');

                deliveryoptions.html(data);

                this.dispatchoptionsfromserver(getdispatchoptions().checkoutdispatchdeliveryoptions);
                this.consolidateddispatchoptionsfromserver(getdispatchoptions().checkoutdispatchappledeliveryoptions);

                oncomplete.apply(this);
            }).bind(this),
            error: (function (xhr, s, a) {
                console.error('applepayviewmodel fetchdeliverymethods errored', xhr, s, a);

                this.hidecompletingmessage();
                this.hidepaymentprocessingerror();
                this.abortapple();
                window.showinfo(this.resources.sorry, this.resources.applepayfailurenotcharged);
            }).bind(this)
        });
    };

    // this function fetches _checkouttotal.cshtml from the server, renders it into a hidden element and then lifts totals.
    // oncomplete is called as a callback if provided upon success.
    fetchtotals(oncomplete) {

        let data = {};

        console.trace(data);

        $.ajax({
            url: geturlcheckoutsummary(),
            type: "get",
            data: data,
            datatype: "html",
            success: (function (data) {
                console.trace(data);

                var totalswrapper = $('#despatch-items-wrapper');

                totalswrapper.html(data);

                this.total(getbalancenumber());
                this.subtotal(getitemsubtotalnumber());
                this.tax(gettaxnumber());
                this.shipping(getshippingnumber());
                oncomplete.apply(this);
            }).bind(this),
            error: (function (xhr, s, a) {
                this.hidecompletingmessage();
                this.hidepaymentprocessingerror();
                this.abortapple();
                window.showinfo(this.resources.sorry, this.resources.applepayfailurenotcharged);
            }).bind(this)
        });
    }

    // this function chains fetchdeliverymethods and fetchtotals.
    // the callback to the fetchtotals call sets this.availableshippingmethods and this.completeshippingcontactmodel,
    // ready for handing this.completeshippingcontactmodel to the apple session.
    refreshdeliveryandtotals (oncomplete) {
        console.log('applepayviewmodel refreshdeliveryandtotals');

        this.fetchdeliverymethods(() => {
            console.log('applepayviewmodel fetchdeliverymethods oncomplete');

            this.fetchtotals(() => {
                console.log('applepayviewmodel fetchtotals oncomplete');

                let availableshippingmethods = this.consolidateddispatchoptionsfromserver().map((d) => {
                    let mindatesplit = d.mindeliverydate.split(' ').map(i => number(i));
                    let maxdatesplit = d.maxdeliverydate.split(' ').map(i => number(i));
                    console.log('applepayviewmodel mapping to shippingmethod', d);
                    return {
                        'label': d.displayname,
                        'detail': '',
                        'amount': `${number(d.priceinc).tofixed(2)}`,
                        'identifier': `${d.sortorder}`,
                        'datecomponentsrange': {
                            'startdatecomponents': {
                                'years': `${mindatesplit[0]}`,
                                'months': `${mindatesplit[1]}`,
                                'days': `${mindatesplit[2]}`
                            },
                            'enddatecomponents': {
                                'years': `${maxdatesplit[0]}`,
                                'months': `${maxdatesplit[1]}`,
                                'days': `${maxdatesplit[2]}`
                            }
                        }
                    }
                });

                let completeshippingcontactmodel = {
                    'status': applepaysession.status_success,
                    'newtotal': {
                        'type': 'final',
                        'label': 'designers guild',
                        'amount': `${this.total().tofixed(2)}`
                    },
                    'newlineitems': [{
                        'label': 'sub total',
                        'type': 'final',
                        'amount': `${this.subtotal().tofixed(2)}`
                    }, {
                        'label': 'delivery',
                        'type': 'final',
                        'amount': `${this.shipping().tofixed(2)}`
                    }],
                    'errors': [],
                    newshippingmethods: availableshippingmethods
                };

                if (this.tax()) {
                    completeshippingcontactmodel.newlineitems.push({
                        'label': 'tax',
                        'type': 'final',
                        'amount': `${this.tax().tofixed(2)}`
                    });
                }

                this.availableshippingmethods = availableshippingmethods;

                this.completeshippingcontactmodel = completeshippingcontactmodel;

                if (typeof oncomplete === 'function')
                    oncomplete.apply(this);
            });
        });
    }

    // this function saves the delivery address, then executes the oncomplete callback function if provided.
    savedeliveryaddress(data, oncomplete) {
        console.trace(data);

        this.checkoutlog(log_action_pay_now_step, data);

        $.ajax({
            url: "/api/checkout/savecheckoutaddress",
            type: "post",
            cache: false,
            async: true,
            datatype: 'json',
            data: data,
            success: (function (data) {
                console.log('applepayviewmodel savedeliveryaddress returned', data);
                oncomplete.apply(this, [data]);
            }).bind(this),
            error: (function (xhr, textstatus, errorthrown) {
                this.abortapple();
                window.showinfo(this.resources.sorry, this.resources.applepayfailurenotcharged);
            }).bind(this)
        });
    }

    // this is to be used when things go wrong
    // unsets parent.busy
    // calls hidecompletingmessage 
    // calls session.abort wrapped in a try catch: the session might already be closed and no longer in an abortable state, which is fine.
    abortapple() {
        this.parent.busy(false);
        this.hidecompletingmessage();
        try {
            this.activeapplepaysession.abort();
        } catch (e) {

        }
    }

    // this is the sucessful end, should only be called from submit success branch
    endapplesuccessfully() {
        this.checkoutlog(log_action_apple_completepayment, 'completepayment(applepaysession.status_success)');
        this.activeapplepaysession.completepayment(applepaysession.status_success);
    }

    showpaymentprocessingerror () {
        bootstrap.modal.getorcreateinstance(document.queryselector("#payment-processing-error")).show();
    }

    hidepaymentprocessingerror () {
        bootstrap.modal.getorcreateinstance(document.queryselector("#payment-processing-error")).hide();
    }

    showcompletingmessage() {
        // the completing message modal reads "completing your order \n please do not refresh the page"
        bootstrap.modal.getorcreateinstance(document.queryselector("#checkout-complete-waiting")).show();

        // this blocks the entire page with an overlay which reads "please wait..." and has a spinner-border.
        this.parent.busy(true);

        // this logs the fact that we've displayed the completing message.
        this.checkoutlog(log_action_pay_now_step, 'completing message shown');
    }

    hidecompletingmessage() {
        bootstrap.modal.getorcreateinstance(document.queryselector("#checkout-complete-waiting")).hide();

        this.parent.busy(false);

        // somewhere out there something disables the proceed-btn elements, preventing you from mashing buttons to try and hurt our checkout.
        // when we're no longer showing the completing message, apparently we want to re-enable these buttons so people can have another go.
        document.queryselectorall('.proceed-btn').foreach(pb => {
            pb.removeattribute('disabled');
            pb.classlist.remove('disabled');
        });

        // this logs the fact that we've hidden the completing message.
        this.checkoutlog(log_action_pay_now_step, 'completing message hidden');
    }

    savedispatch(e, oncomplete) {

        let mindate = new date(date.utc(
            e.shippingmethod.datecomponentsrange.startdatecomponents.years,
            e.shippingmethod.datecomponentsrange.startdatecomponents.months - 1,
            e.shippingmethod.datecomponentsrange.startdatecomponents.days,
            0,
            0,
            0
        ));

        let maxdate = new date(date.utc(
            e.shippingmethod.datecomponentsrange.enddatecomponents.years,
            e.shippingmethod.datecomponentsrange.enddatecomponents.months - 1,
            e.shippingmethod.datecomponentsrange.enddatecomponents.days,
            0,
            0,
            0
        ));

        let deliverybandid = this.dispatchoptionsfromserver().find(x => x.sortorder == e.shippingmethod.identifier).deliverybandid;

        let data = [{
            dispatchid: 1,
            deliverybandid: deliverybandid,
            shouldpartship: false,
            mindeliverydate: mindate.toisostring(),
            maxdeliverydate: maxdate.toisostring(),
            customerrequestsfulldespatch: false
        }];

        console.log('applepayviewmodel savedispatch calling with', data);

        $.ajax({
            url: "/api/checkout/savedispatch",
            type: "post",
            cache: false,
            async: true,
            data: json.stringify({
                dispatchlines: data
            }),
            contenttype: 'application/json; charset=utf-8',
            success: (function (data) {
                console.log('applepayviewmodel savedispatch returned', data)
                oncomplete.apply(this);
            }).bind(this),
            error: (function (xhr, textstatus, errorthrown) {
                this.hidecompletingmessage();
                this.hidepaymentprocessingerror();
                this.abortapple();
                window.showinfo(this.resources.sorry, this.resources.applepayfailurenotcharged);
            }).bind(this)
        });
    }

    submit(nonce, email) {

        this.showcompletingmessage();

        let defaultcard = false;

        let data = json.stringify({
            device_data: null,
            payment_method_nonce: nonce,
            masterdefinition: getmasterdefinition(),
            basketcountry: getsitecountry(),
            marketingpermissions: [],
            make_default: defaultcard,
            email: email
        });

        this.checkoutlog(log_action_pay_now_step, 'applepayviewmodel submit calling with', data);

        console.log('applepayviewmodel submit calling with', data);

        $.ajax({
            url: '/api/checkout/checkoutbraintreesubmit',
            type: 'post',
            cache: false,
            async: true,
            data: data,
            contenttype: 'application/json; charset=utf-8',
            success: (function (data) {

                console.log('applepayviewmodel submit returned', data);

                this.checkoutlog(log_action_pay_now_step, 'paymentmethodfactory submitapplecheckout returned: ' + json.stringify(data));

                if (data != null) {
                    if (data.result === "ok") {
                        if (data.redirect === "confirm") {
                            // defunct and should never be hit
                            // window.location.href = '@dgframework.buildurl(model.masterdefinition.urlbreakdown.countrydetails, null, dgcontenttype.landingpage, (int)dgfixedpage.checkoutconfirmation, null, null)';
                        } else if (data.redirect === "ordergen") {
                            $('#checkout-complete-waiting .page-blocker').addclass('success');
                            $('#checkout-complete-waiting p').html(this.parent.resources.checkoutpaymentsuccessgeneratingorder);

                            this.showcompletingmessage();

                            this.endapplesuccessfully();

                            $.ajax({
                                url: '/api/checkout/checkoutordergenerate',
                                type: 'post',
                                cache: false,
                                async: true,
                                timeout: 30000,
                                data: {},
                                contenttype: 'application/json; charset=utf-8',
                                success: function (data) {
                                    window.location.href = '/' + getsitecountry() + '/l271'; //'@dgframework.buildurl(model.masterdefinition.urlbreakdown.countrydetails, null, dgcontenttype.landingpage, (int)dgfixedpage.checkoutconfirmation, null, null)';
                                },
                                error: function (xhr, textstatus, errorthrown) {
                                    window.location.href = '/' + getsitecountry() + '/l281'; //@dgframework.buildurl(model.masterdefinition.urlbreakdown.countrydetails, null, dgcontenttype.landingpage, (int)dgfixedpage.checkoutconfirmationspartan, null, null)';
                                }
                            });
                        }
                    }
                    else if (data.result === "fail") {
                        this.hidecompletingmessage();
                        this.showpaymentprocessingerror();
                    }
                }
                else {
                    this.hidecompletingmessage();
                    this.showpaymentprocessingerror();
                }
            }).bind(this),
            error: (function (e, s, a) {

                this.checkoutlog(log_action_pay_now_step, 'paymentmethodfactory submitapple errored: ' + json.stringify(e));

                this.hidecompletingmessage();

                this.hidepaymentprocessingerror();

                this.abortapple();

                window.showinfo(this.resources.sorry, this.resources.applepayfailurenotcharged);

            }).bind(this),
            timeout: 90000
        });
    }

    //#endregion
}