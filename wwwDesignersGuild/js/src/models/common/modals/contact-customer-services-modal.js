try {
    window.contactCustomerServicesModal = document.querySelector('#contact-customer-services-modal');
    window.contactCustomerServicesModalInstance = bootstrap.Modal.getOrCreateInstance(window.contactCustomerServicesModal);
    window.oncontactCustomerServicesModalShow = [];
    window.contactCustomerServicesModal.addEventListener('show.bs.modal', function (event) {
        window.oncontactCustomerServicesModalShow.forEach((f) => {
            if (f instanceof Function)
                f();
        });
    });
    window.onContactCustomerServicesModalShown = [];
    window.contactCustomerServicesModal.addEventListener('shown.bs.modal', function (event) {
        window.onContactCustomerServicesModalShown.forEach((f) => {
            if (f instanceof Function)
                f();
        });
    });
    window.onContactCustomerServicesModalHide = [];
    window.contactCustomerServicesModal.addEventListener('hide.bs.modal', function (event) {
        window.onContactCustomerServicesModalHide.forEach((f) => {
            if (f instanceof Function)
                f();
        });
    });
    window.onContactCustomerServicesModalHidden = [];
    window.contactCustomerServicesModal.addEventListener('hidden.bs.modal', function (event) {
        window.onContactCustomerServicesModalHidden.forEach((f) => {
            if (f instanceof Function)
                f();
        });
    });
    window.showContactCustomerServices = (close) => {
        window.contactCustomerServicesModalInstance.closeFn = (() => {
            typeof close === 'function' ? close() : null;
            window.contactCustomerServicesModalInstance.hide();
        });
        window.contactCustomerServicesModalInstance.show();
    };
    window.hideContactCustomerServices = () => {
        window.contactCustomerServicesModalInstance.hide();
    };
} catch (e) {

}