window.backInStockSuccessModal = document.querySelector('#back-in-stock-success-modal');
window.backInStockSuccessModalInstance = bootstrap.Modal.getOrCreateInstance(window.backInStockSuccessModal);
window.onBackInStockSuccessModalShow = [];
window.backInStockSuccessModal.addEventListener('show.bs.modal', function (event) {
    window.onBackInStockSuccessModalShow.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.onBackInStockSuccessModalShown = [];
window.backInStockSuccessModal.addEventListener('shown.bs.modal', function (event) {
    window.onBackInStockSuccessModalShown.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.onBackInStockSuccessModalHide = [];
window.backInStockSuccessModal.addEventListener('hide.bs.modal', function (event) {
    window.onBackInStockSuccessModalHide.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.onBackInStockSuccessModalHidden = [];
window.backInStockSuccessModal.addEventListener('hidden.bs.modal', function (event) {
    window.onBackInStockSuccessModalHidden.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.showBackInStockSuccess = () => {
    window.backInStockSuccessModalInstance.closeFn = (() => {
        typeof close === 'function' ? close() : null;
        window.backInStockSuccessModalInstance.hide();
    });
    window.backInStockSuccessModalInstance.show();
};
window.hideBackInStockSuccess = () => {
    window.backInStockSuccessModalInstance.hide();
};