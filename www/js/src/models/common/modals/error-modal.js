window.errorModal = document.querySelector('#error-modal');
window.errorModalInstance = bootstrap.Modal.getOrCreateInstance(window.errorModal);
window.onErrorModalShow = [];
window.errorModal.addEventListener('show.bs.modal', function (event) {
    window.onErrorModalShow.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.onErrorModalShown = [];
window.errorModal.addEventListener('shown.bs.modal', function (event) {
    window.onErrorModalShown.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.onErrorModalHide = [];
window.errorModal.addEventListener('hide.bs.modal', function (event) {
    window.onErrorModalHide.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.onErrorModalHidden = [];
window.errorModal.addEventListener('hidden.bs.modal', function (event) {
    window.onErrorModalHidden.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.showError = () => {
    window.errorModalInstance.show();
};
window.hideError = () => {
    window.errorModalInstance.hide();
};