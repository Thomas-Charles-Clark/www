window.confirmModal = document.querySelector('#confirm-modal');
window.confirmModalInstance = bootstrap.Modal.getOrCreateInstance(window.confirmModal);
window.onConfirmModalShow = [];
window.confirmModal.addEventListener('show.bs.modal', function (event) {
    window.onConfirmModalShow.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.onConfirmModalShown = [];
window.confirmModal.addEventListener('shown.bs.modal', function (event) {
    window.onConfirmModalShown.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.onConfirmModalHide = [];
window.confirmModal.addEventListener('hide.bs.modal', function (event) {
    window.onConfirmModalHide.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.onConfirmModalHidden = [];
window.confirmModal.addEventListener('hidden.bs.modal', function (event) {
    window.onConfirmModalHidden.forEach((f) => {
        if (f instanceof Function)
            f();
    });
});
window.showConfirm = (title, message, confirm, reject) => {
    document.querySelector('#confirm-modal .modal-title').innerHTML = title;
    document.querySelector('#confirm-modal .modal-body').innerHTML = message;
    window.confirmModalInstance.confirmFn = (() => {
        typeof confirm === 'function' ? confirm() : null;
        window.confirmModalInstance.hide();
    });
    window.confirmModalInstance.rejectFn = (() => {
        typeof reject === 'function' ? reject() : null;
        window.confirmModalInstance.hide();
    });
    window.confirmModalInstance.show();
};
window.hideConfirm = () => {
    window.confirmModalInstance.hide();
};