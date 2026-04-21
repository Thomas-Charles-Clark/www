ko.bindingHandlers.tradeClearanceAddition = {
    init: (element, valueAccessor, allBindingsAccessor) => {
        var self = valueAccessor();

        self.tradeClearanceAdditionReady = ko.observable(false);

        self.selectMode = ko.observable(false); // true == browse mode, false == select mode

        self.selectMode.subscribe(nv => {
            if (nv) {
                document.querySelector('.product-page-grid').classList.add('trade-clearance-select-mode');
                self.filteredContent().forEach(pp => pp.selected(true));
            } else {
                document.querySelector('.product-page-grid').classList.remove('trade-clearance-select-mode');
            }
        });

        self.allProductPages.subscribe(nvs => {

            self.allProductPages(self.allProductPages().map(pp => {
                pp.selected = ko.observable(false);
                return pp;
            }));

            // this should cause determineAvailableProductPages to fire
            self.readyToCalculateAvailability.valueHasMutated();

            window.setTimeout(() => {
                self.tradeClearanceAdditionReady(true);
            }, 0);
        }, self, 'arrayChange');

        self.allSelected = ko.computed(() => {
            if (self.tradeClearanceAdditionReady()) {
                return !(self.filteredContent().find(pp => pp.selected() == false));
            } else {
                return false;
            }
        }, self);

        self.toggleSelectAll = () => {
            console.log('selectAll called');
            if (self.allSelected()) {
                self.filteredContent().forEach(pp => pp.selected(false));
            } else {
                self.filteredContent().forEach(pp => pp.selected(true));
                self.selectMode(true);
                document.querySelector('.product-page-grid').classList.add('trade-clearance-select-mode');
            }
        }

        self.fetchClearanceSpreadsheet = () => {
            let productPageIds = self.filteredContent().flatMap(pp => pp.selected() ? [pp.ProductPageID] : []);

            var headers = Object.assign({}, window.fetchHeaders);

            headers.accept = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            self.pager.busy(true);

            fetch('/api/tradeapi/downloadClearanceSpreadsheet', {
                method: 'POST',
                headers: headers,
                cache: 'no-store',
                credentials: 'same-origin',
                body: JSON.stringify({
                    productPageIds: productPageIds
                })
            })
                .then(async res => ({
                    filename: 'Designers-Guild-Clearance.xlsx',
                    blob: await res.blob()
                }))
                .then(resObj => {
                    const newBlob = new Blob([resObj.blob], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });

                    // MS Edge and IE don't allow using a blob object directly as link href, instead it is necessary to use msSaveOrOpenBlob
                    if (window.navigator && window.navigator.msSaveOrOpenBlob) {
                        window.navigator.msSaveOrOpenBlob(newBlob);
                    } else {
                        // For other browsers: create a link pointing to the ObjectURL containing the blob.
                        const objUrl = window.URL.createObjectURL(newBlob);

                        let link = document.createElement('a');
                        link.href = objUrl;
                        link.download = resObj.filename;
                        link.click();

                        // For Firefox it is necessary to delay revoking the ObjectURL.
                        setTimeout(() => { window.URL.revokeObjectURL(objUrl); }, 250);
                    }
                    self.pager.busy(false);
                })
                .catch(err => {
                    window.catchFetchErrors(err);
                    self.pager.busy(false);
                });
        }

        // big thanks: https://gist.github.com/devloco/5f779216c988438777b76e7db113d05c

        self.fetchClearancePDF = () => {
            let productPageIds = self.filteredContent().flatMap(pp => pp.selected() ? [pp.ProductPageID] : []);

            var headers = Object.assign({}, window.fetchHeaders);

            headers.accept = "application/pdf";

            self.pager.busy(true);

            fetch('/api/tradeapi/downloadClearancePDF', {
                method: 'POST',
                headers: headers,
                cache: 'no-store',
                credentials: 'same-origin',
                body: JSON.stringify({
                    productPageIds: productPageIds
                })
            })
                .then(async res => ({
                    filename: 'Designers-Guild-Clearance.pdf',
                    blob: await res.blob()
                }))
                .then(resObj => {
                    const newBlob = new Blob([resObj.blob], { type: 'application/pdf' });

                    // MS Edge and IE don't allow using a blob object directly as link href, instead it is necessary to use msSaveOrOpenBlob
                    if (window.navigator && window.navigator.msSaveOrOpenBlob) {
                        window.navigator.msSaveOrOpenBlob(newBlob);
                    } else {
                        // For other browsers: create a link pointing to the ObjectURL containing the blob.
                        const objUrl = window.URL.createObjectURL(newBlob);

                        let link = document.createElement('a');
                        link.href = objUrl;
                        link.download = resObj.filename;
                        link.click();

                        // For Firefox it is necessary to delay revoking the ObjectURL.
                        setTimeout(() => { window.URL.revokeObjectURL(objUrl); }, 250);
                    }

                    self.pager.busy(false);
                })
                .catch(err => {
                    window.catchFetchErrors(err);
                    self.pager.busy(false);
                });
        }

        self.selectedAttributes.subscribe(nv => {
            self.selectMode(false);
            self.filteredContent().forEach(pp => pp.selected(false));
            // Tom 21/04/2026 - why was the below line written? It appears to be extremely unhelpful.
            // bootstrap.Collapse.getOrCreateInstance(document.querySelector('#trade-clearance-control-panel')).hide();
        }, self, 'arrayChange');

        //self.getFileNameFromContentDispostionHeader = (header) => {
        //    let contentDispostion = header.split(';');
        //    const fileNameToken = `filename*=UTF-8''`;

        //    let fileName = ;
        //    for (let thisValue of contentDispostion) {
        //        if (thisValue.trim().indexOf(fileNameToken) === 0) {
        //            fileName = decodeURIComponent(thisValue.trim().replace(fileNameToken, ''));
        //            break;
        //        }
        //    }

        //    return fileName;
        //}

        self.handleOffCanvasShow = (e) => {
            console.log(`#grid-filters${self.parent.pageSectionId} offCanvas show`, e);
            let productPageIds = self.filteredContent().flatMap(pp => pp.selected() ? [pp.ProductPageID] : []);
            if (productPageIds.length) {
                console.log(`#grid-filters${self.parent.pageSectionId} offCanvas show interrupted`);
                e.preventDefault();
                window.showConfirm(self.resources.AreYouSure, self.resources.ChangingFiltersClearsSelection, () => {
                    console.log(`#grid-filters${self.parent.pageSectionId} offCanvas show confirmed`);
                    self.filteredContent().forEach(fc => {
                        fc.selected(false);
                    });
                    self.offCanvas._element.removeEventListener('show.bs.offcanvas', self.handleOffCanvasShow);
                    self.offCanvas._element.addEventListener('show.bs.offcanvas', self.setupOffCanvasEvents, {
                        once: true
                    });
                    self.offCanvas.show();
                }, () => {
                    console.log(`#grid-filters${self.parent.pageSectionId} offCanvas show refused`);
                });
            }
        }

        self.setupOffCanvasEvents = () => {
            console.log('setupOffCanvasEvents');
            self.offCanvasElementId = `grid-filters-${self.parent.pageSectionId}`;

            self.offCanvasElement = document.getElementById(self.offCanvasElementId);

            self.offCanvas = bootstrap.Offcanvas.getOrCreateInstance(self.offCanvasElement);

            self.offCanvas._element.addEventListener('show.bs.offcanvas', self.handleOffCanvasShow);
        }

        window.addBootstrapLoadFunction(() => {
            self.setupOffCanvasEvents();
        });
    }
}