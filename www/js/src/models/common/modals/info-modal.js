window.infoModal = document.querySelector('#info-modal');
window.infoModalInstance = bootstrap.Modal.getOrCreateInstance(window.infoModal);
window.onInfoModalShow = [];
window.infoModal.addEventListener('show.bs.modal', function (event) {
    window.onInfoModalShow.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.onInfoModalShown = [];
window.infoModal.addEventListener('shown.bs.modal', function (event) {
    window.onInfoModalShown.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.onInfoModalHide = [];
window.infoModal.addEventListener('hide.bs.modal', function (event) {
    window.onInfoModalHide.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.onInfoModalHidden = [];
window.infoModal.addEventListener('hidden.bs.modal', function (event) {
    window.onInfoModalHidden.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.showInfo = (title, message, close) => {
    document.querySelector('#info-modal .modal-title').innerHTML = title;
    document.querySelector('#info-modal .modal-body').innerHTML = message;
    window.infoModalInstance.closeFn = (() => {
        typeof close === 'function' ? close() : null;
        window.infoModalInstance.hide();
    });
    window.infoModalInstance.show();
};
window.hideInfo = () => {
    window.infoModalInstance.hide();
};