window.backInStockLoginModal = document.querySelector('#back-in-stock-login-modal');
window.backInStockLoginModalInstance = bootstrap.Modal.getOrCreateInstance(window.backInStockLoginModal);
window.onBackInStockLoginModalShow = [];
window.backInStockLoginModal.addEventListener('show.bs.modal', function (event) {
    window.onBackInStockLoginModalShow.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.onBackInStockLoginModalShown = [];
window.backInStockLoginModal.addEventListener('shown.bs.modal', function (event) {
    window.onBackInStockLoginModalShown.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.onBackInStockLoginModalHide = [];
window.backInStockLoginModal.addEventListener('hide.bs.modal', function (event) {
    window.onBackInStockLoginModalHide.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.onBackInStockLoginModalHidden = [];
window.backInStockLoginModal.addEventListener('hidden.bs.modal', function (event) {
    window.onBackInStockLoginModalHidden.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.showBackInStockLogin = () => {
    window.backInStockLoginModalInstance.closeFn = (() => {
        typeof close === 'function' ? close() : null;
        window.backInStockLoginModalInstance.hide();
    });
    window.backInStockLoginModalInstance.show();
};
window.hideBackInStockLogin = () => {
    window.backInStockLoginModalInstance.hide();
};