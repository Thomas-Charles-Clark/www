const moodboardViewModel = ((resources) => {
    let self = {
        resources: resources
    };

    //#region Properties

    self.kendoGridPagerMessages = {
        display: self.resources.KendoPagerDisplay,
        empty: self.resources.KendoPagerEmpty,
        page: self.resources.KendoPagerPage,
        of: self.resources.KendoPagerOf,
        itemsPerPage: self.resources.KendoPagerItemsPerPage,
        first: self.resources.KendoPagerFirstPage,
        last: self.resources.KendoPagerLastPage,
        next: self.resources.KendoPagerNextPage,
        previous: self.resources.KendoPagerPreviousPage,
        refresh: self.resources.KendoPagerRefresh,
        morePages: self.resources.KendoPagerMorePages,
    };

    self.busy = ko.observable(true);

    self.productsBusy = ko.observable(false);

    self.expanded = ko.observable(false);

    self.memberWrapperElement = document.querySelector('#member-wrapper');

    self.canvasElement = document.querySelector('#moodboard-canvas');

    self.scrapbookGridElement = document.querySelector('#scrapbook-grid');

    self.scrapbookGrid = $(self.scrapbookGridElement);

    self.canvas = self.canvasElement.getContext('2d');

    self.fabric = new fabric.Canvas(self.canvasElement, {
        width: self.memberWrapperElement.clientWidth - 2,
        height: 600,
        imageSmoothingEnabled: false,
        enableRetinaScaling: false,
        preserveObjectStacking: true
    });

    self.moodboards = ko.observableArray([]);

    self.scrapbook = ko.observableArray([]);

    self.masonryOptions = {};

    self.tabButtonElements = [...document.querySelectorAll('#moodboard-tabs-container button.nav-link')];

    self.tabs = self.tabButtonElements;

    self.selectedMoodboard = ko.observable();

    self.availableBrands = ko.observableArray(getBrands());

    self.availableRooms = ko.observableArray(getRooms());

    self.addingBrandLogo = ko.observable('');

    self.scrapbookFilterBrandId = ko.observable('');

    self.scrapbookFilterProductTypeId = ko.observable('');

    self.scrapbookFilterNew = ko.observable(false);

    self.includeProductData = ko.observable(true);

    self.saveCountdownTimer = null;

    self.savedTimer = null;

    self.saveTimerElement = document.querySelector('#save-timer');

    self.countdownElement = self.saveTimerElement.querySelector('.countdown');

    self.shareModalElement = document.querySelector('#moodboard-share-modal');

    self.shareModalElement.addEventListener('show.bs.modal', () => {
        window.setTimeout(() => {
            document.getElementById('email-collapse').addEventListener('show.bs.collapse', () => {
                self.emailer.initCaptcha();
            }, { once: true });
        }, 0);
    }, { once: true })

    self.productsModalElement = document.querySelector('#moodboard-products-modal');

    self.validateEmailNow = ko.observable(false);

    self.email = ko.observable().extend({
        required: {
            params: true,
            onlyIf: () => {
                return self.validateEmailNow()
            },
            message: self.resources.FormsErrorEmail
        },
        email: {
            params: true,
            onlyIf: () => {
                return self.validateEmailNow()
            },
            message: self.resources.FormsErrorEmailFormat
        },
        errorPulse: {}
    });

    // Whether or not to include detailed product info in the moodboard share email.
    self.detailedProductInfo = ko.observable(false);

    self.detailedProductInfoParams = {
        checkboxId: 'detailed-product-info',
        label: self.resources.MoodboardIncludeProductData,
        observable: self.detailedProductInfo,
        useNativeValidation: false,
        disabled: false
    }

    //#endregion


    //#region Undo / Redo stack stuff, with thanks to: https://stackoverflow.com/questions/19043219/undo-redo-feature-in-fabric-js#answer-32618231

    self.state = ko.observable();
    self.undo = ko.observableArray([]);
    self.redo = ko.observableArray([]);
    self.skipStateUpdate = ko.observable(true);

    //#endregion


    //#region Components

    self.moodboardGrid = {
        autoBind: false,
        busy: self.busy,
        widget: ko.observable(),
        data: self.moodboards,
        useKOTemplates: true,
        rowTemplate: 'moodboard-row-template',
        pageable: {
            pageSize: 20,
            pageSizes: [20, 50, 100, 500],
            messages: self.kendoGridPagerMessages
        },
        columns: [{
            field: 'CreatedDT',
            title: self.resources.MoodboardCreated,
            attributes: {
                'class': 'text-right'
            }
        }, {
            field: 'CustomerName',
            title: self.resources.MoodboardCustomerName
        }, {
            field: 'Room',
            title: self.resources.MoodboardRoomSelector
        }, {
            field: 'Reference',
            title: self.resources.MoodboardYourReference
        }, {
            field: 'ViewEdit',
            title: ' '
        }, {
            field: 'OrderSamples',
            title: ' '
        }, {
            field: 'Share',
            title: ' '
        }, {
            field: 'Delete',
            title: ' '
        }],
        width: 'auto',
        sortable: true,
        scrollable: true,
        selectable: 'row',
        noRecords: {
            template: self.resources.NoRecordsToDisplay
        },
        change: (e) => {
            //let mb = e.sender.dataItem();
            //self.edit(mb);

            let selectedRows = e.sender.select();
            let selectedDataItems = [];
            for (var i = 0; i < selectedRows.length; i++) {
                let dataItem = e.sender.dataItem(selectedRows[i]);
                selectedDataItems.push(dataItem);
            }

            // selectedDataItems contains all selected data items
            /* The result can be observed in the DevTools(F12) console of the browser. */
            //console.log("Selected data items' name: " + selectedDataItems.map(e => e.name).join(", "));
        },
        selectable: false
    }

    self.emailErrors = new ko.validation.group([
        self.email
    ]);

    // necessarily lifted
    self.emailerSend = (onComplete) => {

        let mb = self.selectedMoodboard();

        let captcha = null;

        try {
            captcha = grecaptcha.getResponse();
        }
        catch (e) {
            // grecaptcha.getResponse will throw if there isn't actually a captcha on the page.
        }

        mb.shareBusy(true);

        fetch('/api/moodboardapi/SendMoodboardShareEmail', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-cache',
            credentials: 'same-origin',
            body: JSON.stringify(Object.assign(self.emailer.toJS(), {
                countryId: getCountryId(),
                languageId: getLanguageId(),
                detailedProductInfo: self.detailedProductInfo(),
                userName: self.emailer.userName(),
                userEmail: self.emailer.replyTo(),
                recipientName: self.emailer.recipientName(),
                recipientEmail: self.emailer.to(),
                body: self.emailer.body(),
                moodboardId: mb.MoodboardID,
                imageUrl: mb.savedImageUrl(),
                gRecaptcha: captcha
            }))
        }).then(window.handleFetchResponse)
            .then(data => {
                mb.shareBusy(false);
                if (data.result)
                    window.showInfo(self.resources.ThankYou, self.resources.MoodboardShareSendEmailSuccess)
                else
                    window.showError();

                try {
                    grecaptcha.reset();
                }
                catch (e) {

                }

                if (typeof onComplete === 'function')
                    onComplete();
            })
            .catch(err => {
                mb.shareBusy(false);
                window.catchFetchErrors(err);
            })
    }

    self.emailer = emailerViewModel(self, self.resources.SomeoneSharedAMoodboardWithYou, 'email-share-captcha', self.emailerSend);

    //#endregion


    //#region Subscriptions

    self.addingBrandLogo.subscribe(nv => {
        if (nv) {
            self.addBrandLogo(nv);
        }
    });

    self.memberWrapperElement.addEventListener('fullscreenchange', (ev) => {
        self.expanded(!self.expanded());
    });

    self.unbusy = () => {
        self.busy(false);
        self.fabric.off('after:render', self.unbusy);
    }

    self.selectedMoodboard.subscribe(nv => {
        if (nv) {
            self.busy(true);
            document.querySelector('.canvas-container').clientHeight = nv.Height;
            self.fabric.setHeight(nv.Height);

            // problem with fonts not measuring correctly on load
            // apparently preloading might cure it

            let arial = new FontFaceObserver('Arial');
            let courierNew = new FontFaceObserver('Courier New');
            let garamond = new FontFaceObserver('Garamond');
            let georgia = new FontFaceObserver('Georgia');
            let lucidaCalligraphyItalic = new FontFaceObserver('Lucida Calligraphy Italic');
            let openSans = new FontFaceObserver('Open Sans');
            let tahoma = new FontFaceObserver('Tahoma');
            let timesNewRoman = new FontFaceObserver('Times New Roman');
            let trebuchetMs = new FontFaceObserver('Trebuchet MS');
            let trixie = new FontFaceObserver('Trixie');
            let verdana = new FontFaceObserver('Verdana');

            Promise.all([
                arial.load(),
                courierNew.load(),
                garamond.load(),
                georgia.load(),
                lucidaCalligraphyItalic.load(),
                openSans.load(),
                tahoma.load(),
                timesNewRoman.load(),
                trebuchetMs.load(),
                trixie.load(),
                verdana.load()
            ]).then(() => {
                self.fabric.loadFromJSON(nv.JSON, () => {
                    self.skipStateUpdate(false);
                    self.fabric.on('object:added', self.saveCountdown);
                    self.fabric.on('object:modified', self.saveCountdown);
                    self.fabric.on('object:removed', self.saveCountdown);
                });

                self.fabric.on('after:render', self.unbusy);

                self.fabric.renderAll();

                self.redo([]);
                self.undo([]);
                self.state(nv.JSON);

                if (nv.JSON != null &&
                    nv.JSON != "" &&
                    nv.JSON != `{"version":"5.2.1","objects":[],"background":"#ffffff"}`) {
                    $('#canvas-background-text').hide();
                } else {
                    $('#canvas-background-text').show();
                }

            });
        } else {
            self.undo([]);
            self.redo([]);

            self.fabric.off('object:added', self.saveCountdown);
            self.fabric.off('object:modified', self.saveCountdown);
            self.fabric.off('object:removed', self.saveCountdown);

            self.fabric.remove(...self.fabric.getObjects().concat())
            self.fabric.setBackgroundColor('#ffffff');

            self.fabric.on('object:added', self.saveCountdown);
            self.fabric.on('object:modified', self.saveCountdown);
            self.fabric.on('object:removed', self.saveCountdown);
        }
    });

    //#endregion


    //#region Computeds

    self.undoDisabled = ko.computed(() => {
        return self.undo().length === 0;
    }, self);

    self.redoDisabled = ko.computed(() => {
        return self.redo().length === 0;
    }, self);

    self.visibleScrapbookItems = ko.pureComputed(() => {
        let brandId = self.scrapbookFilterBrandId();
        let productTypeId = self.scrapbookFilterProductTypeId();
        let newFilter = self.scrapbookFilterNew();

        return self.scrapbook().filter(sb => {
            return (brandId == '' || sb.BrandID == brandId) &&
                (productTypeId == '' || sb.ProductTypeID == productTypeId) &&
                (newFilter === false || sb.New === newFilter);
        });
    }, self);

    self.scrapbookFilteredNoResults = ko.pureComputed(() => {
        if (self.scrapbook().length && !self.visibleScrapbookItems().length) {
            return true;
        }
        return false;
    }, self);

    self.scrapbookEmpty = ko.pureComputed(() => {
        if (!self.scrapbook().length) {
            return true;
        }
        return false;
    }, self);

    //#endregion


    //#region Methods

    // state management
    self.replay = (playStack, saveStack) => {
        saveStack.push(self.state());

        self.state(playStack().pop());

        self.skipStateUpdate(true);

        //self.fabric.clear();

        self.fabric.loadFromJSON(self.state(), () => {
            self.fabric.requestRenderAll();
            self.skipStateUpdate(false);
        })
    }

    self.saveCountdown = () => {
        // don't be starting the save countdown before finished loading or while otherwise busy
        if (self.busy()) 
            return;

        // can't do this here because this is function is invoked
        // every time the state changes - even when we're trying to undo / redo state
        // which involves a clearing of the canvas. That clear causes this to proc,
        // which stores the empty state of the canvas in self.state, which is then what gets
        // loaded.

         //state management
        if (self.skipStateUpdate() === false) {
            if (self.state()) {
                self.undo.push(self.state());
            }
            self.state(JSON.stringify(self.fabric));
        }

        self.saveTimerElement.classList.remove('complete');
        self.countdownElement.classList.remove('complete');

        window.clearTimeout(self.saveCountdownTimer);
        self.countdownElement.classList.remove('counting');

        window.clearTimeout(self.savedTimer);
        self.countdownElement.classList.remove('complete');

        window.setTimeout(() => {
            self.countdownElement.classList.add('counting');
            self.saveCountdownTimer = window.setTimeout(() => {
                self.save(() => {

                    let moodboard = self.moodboards().filter(mb => mb.MoodboardID === self.selectedMoodboard().MoodboardID)[0];
                    
                    moodboard.JSON = JSON.stringify(self.fabric);
                    moodboard.Height = self.fabric.height;
                    moodboard.CustomerName = self.selectedMoodboard().name();
                    moodboard.RoomID = self.selectedMoodboard().roomId();
                    moodboard.Reference = self.selectedMoodboard().reference();

                    self.countdownElement.classList.remove('counting');
                    self.saveTimerElement.classList.add('complete');
                    self.countdownElement.classList.add('complete');
                    self.savedTimer = window.setTimeout(() => {
                        self.saveTimerElement.classList.remove('complete');
                        self.countdownElement.classList.remove('complete');
                    }, 3000);
                });
            }, 5000);
        }, 0);
    }

    self.toggleFullscreen = () => {
        if (!document.fullscreenElement) {
            document.querySelector('#member-wrapper').requestFullscreen();
        } else {
            if (document.exitFullscreen) {
                document.exitFullscreen();
            }
        }
    }

    self.edit = (mb, ev) => {
        //console.trace(mb);

        if (self.selectedMoodboard() != mb) {
            if (self.selectedMoodboard()) {
                self.save(() => self.selectedMoodboard(mb));
            } else {
                self.selectedMoodboard(mb);
            }
        }
        bootstrap.Tab.getOrCreateInstance(self.tabs[1]).show();
    }

    self.fetchOrdering = (mb, ev) => {
        //console.trace(mb);
        self.productsBusy(true);
        bootstrap.Modal.getOrCreateInstance(self.productsModalElement).show(true);
        fetch('/api/MoodboardApi/MoodboardFetchProducts', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-cache',
            credentials: 'same-origin',
            body: JSON.stringify({
                moodboardId: self.selectedMoodboard().MoodboardID,
                countryId: getCountryId(),
                languageId: getLanguageId(),
                baseViewModel: getBaseViewModel()
            })
        })
            .then(window.handleFetchResponse)
            .then(data => {

                let results = data.result.json.products.flatMap(p => {

                    let product = p.product;

                    let products = [];

                    let mainProduct = productViewModel(product.productId, 1);

                    if (product.controls.allowSample) {
                        let sample = productViewModel(product.productId, 1);
                        sample.data(product.sample);
                        sample.orderFormTypeId = 0;
                        sample.quantity(sample.stockMessage.quantityMinimum);
                        products.push(sample);
                    }

                    if (product.controls.allowLargeSample) {
                        let largeSample = productViewModel(product.productId, 1);
                        largeSample.data(product.largeSample);
                        largeSample.orderFormTypeId = 1;
                        largeSample.quantity(largeSample.stockMessage.quantityMinimum);
                        products.push(largeSample);
                    }

                    if (product.controls.allowOrder) {
                        let order = productViewModel(product.productId, 1);
                        order.data(product);
                        order.orderFormTypeId = 2;
                        order.quantity(order.stockMessage.quantityMinimum);
                        products.push(order);
                    }

                    if (product.controls.allowReserve) {
                        let reserve = productViewModel(product.productId, 2);
                        reserve.data(product);
                        reserve.orderFormTypeId = 3;
                        reserve.quantity(reserve.stockMessage.quantityMinimum);
                        products.push(reserve);
                    }

                    if (product.controls.allowReserveWithBatchSample) {
                        let reserveWithBatch = productViewModel(product.productId, 3);
                        reserveWithBatch.data(product);
                        reserveWithBatch.orderFormTypeId = 4;
                        reserveWithBatch.quantity(reserveWithBatch.stockMessage.quantityMinimum);
                        products.push(reserveWithBatch);
                    }

                    mainProduct.data(product);

                    mainProduct.products(products);

                    return mainProduct;

                });

                self.selectedMoodboard().products(results);

                document.querySelector('#products-container').innerHTML = data.markup;

                window.setTimeout(() => {
                    ko.cleanNode(document.querySelector('#products-container'));
                    ko.applyBindings(self.selectedMoodboard(), document.querySelector('#products-container'));
                    self.productsBusy(false);

                    window.setTimeout(window.lazyImages, 0);
                }, 0);
            })
            .catch(err => {
                self.productsBusy(false);
                window.showError();
            });
    }

    self.download = (mb, ev) => {
        //console.trace(mb);
        let selection = self.fabric.getActiveObject();
        self.fabric.discardActiveObject().requestRenderAll();
        self.canvasElement.toBlob((b) => {
            let url = URL.createObjectURL(b);
            let anchor = document.createElement('a');
            anchor.href = url;
            anchor.download = 'Moodboard';
            if (self.selectedMoodboard().name())
                anchor.download += `-${self.selectedMoodboard().name()}`;
            if (self.selectedMoodboard().reference())
                anchor.download += `-${self.selectedMoodboard().reference()}`;
            anchor.download += '.jpeg';
            anchor.innerHTML = '';
            anchor.style.display = 'none';
            document.body.appendChild(anchor);
            anchor.click();
            document.body.removeChild(anchor);
            if (selection)
                self.fabric.setActiveObject(selection);
        }, 'image/jpeg', 100);
    }

    self.printFrameClose = () => {
        document.body.removeChild(self.printFrame);
    }

    self.printFrameLoaded = () => {
        self.printFrame.contentWindow.__container = self.printFrame;
        self.printFrame.contentWindow.onbeforeunload = self.printFrameClose;
        self.printFrame.contentWindow.onafterprint = self.printFrameClose;
        self.printFrame.contentWindow.focus(); // might not be required, used to be necessary for IE
        self.printFrame.contentWindow.print();
    }

    self.print = (mb, ev) => {
        //console.trace(mb);
        if (self.selectedMoodboard()) {
            self.selectedMoodboard().saveAsImage(() => {
                self.printFrame = document.createElement('iframe');
                self.printFrame.onload = self.printFrameLoaded;
                self.printFrame.style.visibility = 'hidden';
                self.printFrame.src = self.selectedMoodboard().savedImageUrl();
                document.body.appendChild(self.printFrame);
            });
        }
    }

    self.share = (mb, ev) => {
        //console.trace(mb);
        if (self.selectedMoodboard()) {
            self.selectedMoodboard().saveAsImage(() => {
                bootstrap.Modal.getOrCreateInstance(self.shareModalElement).show();
            })
        } else {
            window.showInfo(self.resources.Sorry, self.resources.NoMoodboardToShare)
        }
    }

    self.createNew = (mb, ev) => {
        //console.trace(mb);
        if (self.selectedMoodboard()) {
            self.save(() => {
                self.selectedMoodboard(undefined);
                self.fetchNew();
            });
        } else {
            self.fetchNew();
        }
    }

    self.fetchNew = () => {
        self.busy(true);
        fetch('/api/moodboardapi/CreateMoodboard', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-cache',
            credentials: 'same-origin',
            body: JSON.stringify({
                countryId: getCountryId()
            })
        }).then(window.handleFetchResponse)
        .then(data => {
            self.busy(false);
            let mb = moodboardDefinition(data, self);
            self.moodboards.push(mb);
            self.selectedMoodboard(mb);
            let height = document.querySelector('.canvas-container').clientHeight;
            self.fabric.setHeight(height);
        })
        .catch(err => {
            self.busy(false);
            window.catchFetchErrors(err);
        })
    }

    self.delete = (mb, ev) => {
        console.trace(mb);
        ev.stopPropagation();
        window.showConfirm(self.resources.AreYouSure, self.resources.ThisWillDeleteMoodboard, () => {
            self.busy(true);
            fetch('/api/moodboardapi/DeleteMoodboard', {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-cache',
                credentials: 'same-origin',
                body: JSON.stringify({
                    moodboardId: mb.MoodboardID
                })
            })
                .then(window.handleFetchResponse)
                .then(data => {
                    self.busy(false);
                    if (data.error) {
                        window.showError();
                    } else {
                        self.moodboards.remove(m => m.MoodboardID == mb.MoodboardID);
                        if (self.selectedMoodboard())
                            if (self.selectedMoodboard().MoodboardID == mb.MoodboardID)
                                self.selectedMoodboard(undefined);
                        self.moodboardGrid.widget().dataSource.read();
                    }
                })
                .catch(err => {
                    self.busy(false);
                    window.catchFetchErrors(err);
                })
        });
    }

    self.sortScrapbook = () => {
        //self.busy(true);
        let data = {
            countryId: getCountryId(),
            languageId: getLanguageId(),
            images: [...document.querySelectorAll('.scrapbook-pod')].map((img, idx) => {
                return {
                    imageId: ko.dataFor(img).ImageID,
                    sortOrder: idx
                }
            })
        };
        fetch('/api/moodboardapi/SortScrapbook', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-store',
            credentials: 'same-origin',
            body: JSON.stringify(data)
        })
        .then(window.handleFetchResponse)
        .then(data => {
            //self.busy(false);
        })
        .catch(err => {
            //self.busy(false);
            window.catchFetchErrors(err);
        });
    }
    
    self.addBrandLogo = (b) => {
        //console.trace(b);
        if (b.LogoURL != null) {
            fabric.Image.fromURL(b.LogoURL, (i) => {
                i.imageId = -1;
                i.set('left', 10);
                i.set('top', 10);
                self.fabric.add(i);
                self.fabric.setActiveObject(i);
            }, {
                crossOrigin: 'anonymous'
            });
            $('#canvas-background-text').hide();
        }
        self.addingBrandLogo('');
    }

    self.reflowLabels = () => {
        if (self.selectedMoodboard()) {
            if (self.selectedMoodboard().name.hasOwnProperty('mdcTextField'))
                self.selectedMoodboard().name.mdcTextField.layout();
            if (self.selectedMoodboard().reference.hasOwnProperty('mdcTextField'))
                self.selectedMoodboard().reference.mdcTextField.layout();
        }
    }

    self.removeSelectedImages = () => {

        let imageIds = [];

        self.fabric.getActiveObjects().forEach(o => {
            self.removalRecursion(o, imageIds);
            self.fabric.remove(o);
        });

        self.fabric.discardActiveObject();
        self.fabric.requestRenderAll();

        self.removeImagesFromMoodboard(imageIds);
        
        if (self.fabric.isEmpty()) {
            $('#canvas-background-text').show();
        } else {
            self.fabric.requestRenderAll();
        }
    }

    self.removalRecursion = (obj, imageIds) => {
        var items = obj._objects;

        if (items != null) {
            items.forEach(i => {
                if (i.type === 'image') {
                    imageIds.push(i.imageId);
                } else {
                    self.removalRecursion(i, imageIds);
                }
            })
        } else {
            if (obj.type === 'image') {
                imageIds.push(obj.imageId);
            }
        }
    }

    self.removeImagesFromMoodboard = (imageIds) => {
        fetch('/api/moodboardapi/removeImagesFromMoodboard', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-cache',
            credentials: 'same-origin',
            body: JSON.stringify({
                moodboardId: self.selectedMoodboard().MoodboardID,
                imageIds: imageIds
            })
        })
    }

    self.init = () => {

        self.moodboards(getMoodboards().map(mb => moodboardDefinition(mb, self)));

        self.scrapbook(getScrapbook().map(sb => scrapbookDefinition(sb, self)));

        if (self.moodboards().length >= 1) {
            self.edit(self.moodboards()[0]);
        }
        //self.tabs[0]._element.addEventListener('shown.bs.tab', (ev) => {
        //    self.moodboardGrid.widget().dataSource.read();
        //});
        bootstrap.Tab.getOrCreateInstance(self.tabs[1])._element.addEventListener('shown.bs.tab', (ev) => {
            if (self.selectedMoodboard() == null) {
                self.createNew();
            }
            self.reflowLabels();
        });
        $('.canvas-container').resizable({
            minHeight: 600,
            handles: {
                s: document.querySelector('#canvas-height-handle')
            },
            resize: function (ev, ui) {
                let height = document.querySelector('.canvas-container').clientHeight;
                self.fabric.setHeight(height);
            },
            stop: function (ev, ui) {
                self.saveCountdown();
            }
        });
        document.querySelector('body').addEventListener('keydown', (e) => {
            if (e.shiftKey) {
                self.fabric.getActiveObjects().forEach(o => {
                    o.snapAngle = 15;
                })
            } else if (e.ctrlKey) {
                self.fabric.getActiveObjects().forEach(o => {
                    o.snapAngle = 5;
                });
            }

            if (e.ctrlKey) {
                if (e.keyCode == 90)
                    self.replay(self.undo, self.redo);
                if (e.keyCode == 89)
                    self.replay(self.redo, self.undo);
            }
        })
        document.querySelector('body').addEventListener('keyup', (e) => {
            self.fabric.getActiveObjects().forEach(o => {
                o.snapAngle = 0;
            });
            if (e.keyCode === 8 || e.keyCode === 46) {

                self.removeSelectedImages();

                //console.log('save your progress');
            }
        });
        
        $("#scrapbook-grid").sortable({
            helper: (ev, ui) => {
                let helper = $(ui).clone();
                ko.applyBindings(ko.dataFor(ui[0]), helper[0]);
                return helper;
            },
            appendTo: 'body',
            placeholder: 'scrapbook-pod ui-state-highlight',
            opacity: '.5',
            tolerance: 'pointer',
            revert: 0,
            stop: function (event, ui) {
                self.sortScrapbook();
            },
            scroll: true,
            scrollSensitivity: 100,
            scrollSpeed: 100
        });

        $(".canvas-container").droppable({
            accept: ".scrapbook-pod",
            hoverClass: "ui-state-hover",
            drop: function (ev, ui) {
                let parent = $(ui.draggable).parent();
                let draggedElement = $(ui.helper);
                let dropZone = $(this);

                let left = Math.abs(draggedElement.offset().left - dropZone.offset().left);
                let top = draggedElement.offset().top - dropZone.offset().top;

                $("#canvas-background-text").hide();

                let data = ko.dataFor(draggedElement[0]);

                self.selectedMoodboard().addImage({
                    left: left,
                    top: top,
                    width: data.MaxWidth,
                    height: data.Height,
                    includeProductData: self.includeProductData(),
                    imgData: data
                });
            }
        });

        self.fabric.setBackgroundColor('#ffffff', self.fabric.requestRenderAll.bind(self.fabric));

        self.fabric.on('object:added', self.saveCountdown);

        self.fabric.on('object:modified', self.saveCountdown);

        self.fabric.on('object:removed', self.saveCountdown);

        self.moodboardHub = $.connection.moodboardHub;

        self.moodboardHub.client.receiveTwitterOAuth = (obj) => {
            console.trace(obj);
            self.selectedMoodboard().twitterOAuthToken = obj.OAuthToken;
            self.selectedMoodboard().twitterOAuthVerifier = obj.OAuthVerifier;
            self.selectedMoodboard().twitterAuthDialog.close();
            self.selectedMoodboard().postTweet();
        };

        self.moodboardHub.connection.start().done((obj) => {
            //console.trace(obj);
            self.busy(false);
        });
    }

    self.save = (onComplete) => {
        //console.trace(self);

        if (self.selectedMoodboard()) {

            let data = {
                moodboardId: self.selectedMoodboard().MoodboardID,
                height: self.fabric.height,
                JSON: JSON.stringify(self.fabric),
                customerName: self.selectedMoodboard().name(),
                roomId: self.selectedMoodboard().roomId(),
                reference: self.selectedMoodboard().reference()
            }

            fetch('/api/moodboardapi/SaveMoodboard', {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-cache',
                credentials: 'same-origin',
                body: JSON.stringify(data)
            })
                .then(window.handleFetchResponse)
                .then(data => {
                    //console.trace(data);
                    if (typeof onComplete === 'function')
                        onComplete();
                })
                .catch(err => {
                    window.catchFetchErrors(err);
                });
        }
    }

    self.resizeCanvas = () => {
        window.setTimeout(() => {
            self.fabric.setWidth(self.memberWrapperElement.querySelector('#moodboard-composer').clientWidth - 2);
            self.fabric.requestRenderAll();
            self.saveCountdown();
        }, 0);
    }

    window.addEventListener('resize', self.resizeCanvas, false);

    //#endregion

    return self;
});

const moodboardDefinition = ((data, parent) => {
    let self = Object.assign(data, {});

    self.fabric = parent.fabric;

    self.canvasElement = parent.canvasElement;

    self.removeSelectedImages = parent.removeSelectedImages;

    self.availableRooms = parent.availableRooms;

    self.undoDisabled = parent.undoDisabled;

    self.redoDisabled = parent.redoDisabled;

    self.undo = parent.undo;

    self.redo = parent.redo;

    self.resources = parent.resources;

    self.replay = parent.replay;

    self.saveCountdown = parent.saveCountdown;

    self.blob = ko.observable();

    self.savedImageUrl = ko.observable();

    self.name = ko.observable(self.CustomerName);

    self.products = ko.observableArray([]);

    self.shareBusy = ko.observable(false);

    self.shareText = ko.observable();

    self.shareTextParams = {
        label: self.resources.MoodboardShareText,
        observable: self.shareText,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            $(self.observable.DOMElement).focus();
        },
        permitRevert: false,
        charsRemainingOptions: {
            maxLength: 280
        },
        rows: 3,
    }

    self.nameParams = {
        label: self.resources.MoodboardCustomerName,
        observable: self.name,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            $(self.observable.DOMElement).focus();
        },
        permitRevert: false,
        charsRemainingOptions: {
            maxLength: 50
        }
    };

    self.name.subscribe(nv => {
        self.saveCountdown();
    });

    self.roomId = ko.observable(self.RoomID ? `${self.RoomID}` : ``);

    self.roomId.subscribe(nv => {
        self.saveCountdown();
    });

    self.room = ko.pureComputed(() => {
        return self.availableRooms().find(r => r.RoomID == self.roomId())
    }, self);

    self.reference = ko.observable(self.Reference);

    self.reference.subscribe(nv => {
        self.saveCountdown();
    });

    self.referenceParams = {
        label: self.resources.MoodboardYourReference,
        observable: self.reference,
        useNativeValidation: false,
        decorateInputElement: false,
        insertValidationMessages: false,
        showClear: true,
        clearFunction: function () {
            var self = this;
            self.observable(undefined);
            $(self.observable.DOMElement).focus();
        },
        permitRevert: false,
        charsRemainingOptions: {
            maxLength: 50
        }
    };

    self.twitterContent = ko.pureComputed(() => {
        let text = self.resources.Moodboard;

        if (self.name())
            text += ` - ${self.name()}`;

        if (self.reference())
            text += ` ${self.reference()}`;

        if (self.room())
            text += ` ${self.room().Name}`;

        return text;
    }, self);

    self.shareOnFacebook = () => {
        if (typeof window.FB === 'object') {
            FB.ui({
                method: 'share',
                hashtag: '#designersguild',
                href: self.savedImageUrl(),
            }, (r) => {
                
            });
        } else {
            window.showError();
        }
    }

    self.shareViaEmail = () => {
        //console.trace(mb);
        window.showInfo('Sorry', 'Due to publishing deadlines, this feature is not yet complete. Please check back soon! In the meantime, we suggest that you save your moodboard image to your device and email it manually.');
    }

    /*
     Nope. The only way we're going to get these damned images into peoples' tweets is via the API.
     Which means having an App, having them authenticate with Twitter, having them grant our app
     permission to act on their behalf, creating the media (on their account?) via the 1.1 call, 
     then creating the tweet via a 2.0 call containing the media: {  media_ids: ["..."] }     
     see the related Postman collections
     */
     /* 
      
        The workflow would be:

        POST to https://api.twitter.com/oauth/request_token, giving OAuth 1.0 headers appropriately HMAC-SHA1 encrypted

        That returns markup for either logging into Twitter or, if the user is already logged in, for authorising the application

        Once permission is granted, the form returns the callback url (currently https://www.designersguild.com) with oauth_token and oauth_secret as query string parameters.

        We need to update the Twitter app description to be more meaningful

        Need to update the callback URL to some endpoint which can consume the query string parameters

        In order to upload the media and create the tweet for the user, we need to get their oauth token and secret delivered stored somewhere
        and inform the moodboard page that they're available
        then perform the two API calls
        then show the user that their tweet has been created
       
        So the user clicks the Twitter share button, we POST to our server which then does a server-side POST to /request_token

        The server-side POST we just did returns just the KVPs oauth_token, oauth_token_secret and oauth_callback_confirmed

        We return these to our client which performs a window.open(url, _blank, height=640,width=480)

        The user grants access

        The callback URL is notified, providing oauth_token and oauth_token_secret

        We need this callback URL to be publically available, via GET, returning an empty response.

        On the server-side, it must SignalR the oauth_token and oauth_token_secret back to the user on the moodboard page.

        When the user receives that SignalR push, the Twitter auth modal must be closed and the oauth tokens stored somewhere (localStorage?).

        Then the user must be allowed to write a bit of text for their tweet, then click Tweet, then we POST off to our server with the oauth tokens, file URL and Tweet text.
        Our server then POSTs to upload the media, followed immediately by the Tweet creation call. This call returns an object of the form:

        {
            "data": {
                "id": "1554018164991791106",
                "text": "Hello World! https://t.co/JFZVPfb2IV"
            }
        }

        We must parse out the URL from the text property, deliver that down to our client, and pop a final window.open with it.
      
    */


    self.fetchTwitterAuth = () => {
        self.shareBusy(true);
        fetch('/api/moodboardapi/FetchTwitterAuth', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-cache',
            credentials: 'same-origin'
        })
        .then(window.handleFetchResponse)
        .then(data => {
            self.shareBusy(false);
            if (data.error) {
                window.showError();
            } else {
                self.twitterOAuthRequestId = data.AuthRequestID;
                self.twitterAuthDialog = window.open(data.AuthorizationURL, '_blank', 'width=720,height=820')
            }
        })
        .catch(err => {
            self.shareBusy(false);
            window.catchFetchErrors(err);
        })
    }

    self.postTweet = () => {
        if (self.twitterOAuthRequestId && self.twitterOAuthToken && self.twitterOAuthVerifier) {

            let confirmBodyText = self.shareText() ? self.resources.MoodboardThisWillTweet.replace('{0}', self.shareText()) : self.resources.MoodboardThisWillTweetImageOnly;

            window.showConfirm(self.resources.AreYouSure, confirmBodyText, () => {
                let data = {
                    shareText: self.shareText(),
                    imageUrl: self.savedImageUrl(),
                    authTokens: `oauth_token=${self.twitterOAuthToken}&oauth_verifier=${self.twitterOAuthVerifier}&tweetinvi_auth_request_id=${self.twitterOAuthRequestId}`,
                };

                self.shareBusy(true);

                fetch('/api/moodboardapi/TweetMoodboard', {
                    method: 'POST',
                    headers: window.fetchHeaders,
                    cache: 'no-cache',
                    credentials: 'same-origin',
                    body: JSON.stringify(data)
                })
                    .then(window.handleFetchResponse)
                    .then(data => {
                        console.trace(data);
                        if (data.error) {

                        } else {
                            window.open(data.url, '_blank', 'width=720,height=820');
                        }
                        self.shareBusy(false);
                    })
                    .catch(err => {
                        self.shareBusy(false);
                        window.catchFetchErrors(err);
                    })
            })
        } else {
            window.showError();
        }
    }

    self.twitterUrl = ko.pureComputed(() => {
        let text = self.resources.Moodboard;

        if (self.name())
            text += ` - ${self.name()}`;

        if (self.reference())
            text += ` ${self.reference()}`;

        if (self.room())
            text += ` ${self.room().Name}`;

        text = encodeURIComponent(text);

        return `https://twitter.com/intent/tweet?text=${text}&url=${window.location.href}&hashtags=designersguild&via=DesignersGuild`
    }, self);

    self.font = ko.observable('Open Sans');
    self.fontSize = ko.observable('24');
    self.fontStyle = ko.observable('normal');

    self.setFont = (font, event) => {
        self.font(font);
        if (self.fabric.getActiveObject()) {
            let fontObserver = new FontFaceObserver(font);
            fontObserver.load()
                .then(() => {
                    // when font is loaded, use it.
                    self.fabric.getActiveObject().set("fontFamily", font);
                    self.fabric.requestRenderAll();
                    self.saveCountdown();
                }).catch(function (e) {
                    console.error(e)
                });
        } else {
            self.addText();
        }
    }

    self.fontStyle.subscribe(nv => {
        if (self.fabric.getActiveObject()) {
            self.fabric.getActiveObject().set("fontStyle", nv);
            self.fabric.requestRenderAll();
            self.saveCountdown();
        }
    });

    self.fontSize.subscribe(nv => {
        if (self.fabric.getActiveObject()) {
            self.fabric.getActiveObject().set('fontSize', +nv);
            self.fabric.requestRenderAll();
        }
    });

    self.addText = () => {
        //console.trace(mb); 

        let fontObserver = new FontFaceObserver(self.font());

        fontObserver.load()
            .then(() => {
                // when font is loaded, use it.
                self.fabric.add(new fabric.IText(self.resources.MoodboardClickToType, {
                    fontFamily: self.font(),
                    left: 100,
                    top: 100,
                    fontSize: +self.fontSize(),
                    fontStyle: self.fontStyle(),
                    lockUniScaling: true,
                }));
            }).catch(function (e) {
                console.error(e)
            });


        //console.log('save your progress');
    }

    self.addImage = (i) => {
        //console.trace(i);

        $('#canvas-background-text').hide();

        let data = {
            imageId: i.imgData.ImageID,
            moodboardId: self.MoodboardID,
            productPageId: i.imgData.ProductPageID
        };

        fetch('/api/moodboardapi/AddImageToMoodboard', {
            method: 'POST',
            headers: window.fetchHeaders,
            cache: 'no-store',
            credentials: 'same-origin',
            body: JSON.stringify(data)
        })
        .then(window.handleFetchResponse)
        .then(data => {
            fabric.Image.fromURL(`/image${getWebPFallback()}/${i.width}/${i.imgData.ImageID}`, (oImg) => {

                let initialScaling = i.width > 1024 ? 0.25 : 0.5;

                if (i.width == i.height) {
                    initialScaling *= 0.5;
                }

                oImg.set({
                    left: i.left,
                    top: i.top,
                    scaleX: initialScaling,
                    scaleY: initialScaling,
                    width: i.width,
                    height: i.height,
                    imageId: i.imgData.ImageID
                });

                let lanczosFilter = new fabric.Image.filters.Resize({
                    scaleX: 1,
                    scaleY: 1,
                    resizeType: 'lanczos',
                    lanczosLobes: 3,
                });

                let r = self.fabric.getRetinaScaling();

                lanczosFilter.scaleX = lanczosFilter.scaleY = oImg.scaleX * r;

                oImg.lockScalingFlip = true;
                oImg.minScaleLimit = 0.025;
                oImg.filters = [lanczosFilter];

                oImg.on('scaling', function (opt) {
                    let filters = [];
                    let sX = Math.abs(this.scaleX) * r, sY = Math.abs(this.scaleY) * r;
                    if (sX > 0.01 && sY > 0.01 && sX < 1 && sY < 1) {
                        if (sX <= 0.2 || sY <= 0.2) {
                            lanczosFilter.lanczosLobes = 2;
                        } else if (sX <= 0.05 || sY <= 0.05) {
                            lanczosFilter.lanczosLobes = 1;
                        } else {
                            lanczosFilter.lanczosLobes = 3;
                        }
                        lanczosFilter.scaleX = sX;
                        lanczosFilter.scaleY = sY;
                        filters.push(lanczosFilter);
                    }
                    this.filters = filters;
                });

                let t = null;
                let c = null;

                if (i.includeProductData) {
                    if (i.imgData.Heading != null) {
                        t = new fabric.IText(i.imgData.Heading, {
                            fontFamily: "Open Sans",
                            fontSize: 16,
                            left: oImg.left,
                            top: oImg.top + oImg.getBoundingRect().height + 5
                        });
                    }
                    if (i.imgData.ProductCode != null) {
                        c = new fabric.IText(i.imgData.ProductCode, {
                            fontFamily: "Open Sans",
                            fontSize: 16,
                            left: oImg.left,
                            top: oImg.top + oImg.getBoundingRect().height + 25
                        });
                    }
                }

                let objList = [oImg];
                if (t != null)
                    objList.push(t);
                if (c != null)
                    objList.push(c);

                let group = new fabric.Group(objList);
                self.fabric.add(group);
                self.fabric.setActiveObject(group);

            }, {
                crossOrigin: 'anonymous'
            });
        })
        .catch(err => {
            window.catchFetchErrors(err);
        })
    }

    self.toBlob = () => {
        return new Promise((resolve, reject) => {
            self.canvasElement.toBlob((b) => {
                self.blob(b);
                resolve(self.blob());
            }, 'image/jpeg', 100);
        })
    }

    self.blobToBase64 = () => {
        let reader = new FileReader();

        reader.readAsDataURL(self.blob());

        return new Promise((resolve, reject) => {
            reader.onloadend = () => {
                resolve(reader.result);
            }
        })
    }

    self.saveAsImage = (onComplete) => {
        self.toBlob().then(blob => {
            self.blobToBase64().then(base64 => {
                let data = {
                    moodboardId: self.MoodboardID,
                    file: base64,
                    name: self.name(),
                    reference: self.reference(),
                    room: self.room() ? self.room().Name : null
                }
                fetch('/api/moodboardapi/SaveMoodboardImage', {
                    method: 'POST',
                    headers: window.fetchHeaders,
                    cache: 'no-cache',
                    credentials: 'same-origin',
                    body: JSON.stringify(data)
                })
                    .then(window.handleFetchResponse)
                    .then(data => {
                        console.trace(data);
                        if (data.error) {
                            window.showError();
                        } else {
                            self.savedImageUrl(`${data.url}?t=${new Date().valueOf()}`);
                            if (typeof onComplete === 'function')
                                onComplete();
                        }
                    })
                    .catch(window.catchFetchErrors)
            });
        })
    }

    self.moveToFront = (mb, ev) => {
        //console.trace(mb);

        if (self.fabric.getActiveObject()) {
            self.fabric.bringToFront(self.fabric.getActiveObject());
            self.fabric.requestRenderAll();
        }

        //console.log('save your progress');
    }

    self.moveToBack = (mb, ev) => {
        //console.trace(mb);

        if (self.fabric.getActiveObject()) {
            self.fabric.sendToBack(self.fabric.getActiveObject());
            self.fabric.requestRenderAll();
        }

        //console.log('save your progress');
    }

    self.moveForward = (mb, ev) => {
        //console.trace(mb);

        if (self.fabric.getActiveObject()) {
            self.fabric.bringForward(self.fabric.getActiveObject());
            self.fabric.requestRenderAll();
        }

        //console.log('save your progress');
    }

    self.moveBackward = (mb, ev) => {
        //console.trace(mb);

        if (self.fabric.getActiveObject()) {
            self.fabric.sendBackwards(self.fabric.getActiveObject());
            self.fabric.requestRenderAll();
        }

        //console.log('save your progress');
    }

    self.group = (mb, ev) => {
        //console.trace(mb);

        let objList = [];

        let items = self.fabric.getActiveObjects();

        self.fabric.discardActiveObject().requestRenderAll();

        items.forEach(i => {
            objList.push(i);
            self.fabric.remove(i);
        });

        let group = new fabric.Group(objList);

        self.fabric.add(group);

        self.fabric.setActiveObject(group).requestRenderAll();
        
        //console.log('save your progress');
    }

    self.ungroup = (mb, ev) => {
        //console.trace(mb);

        let group = self.fabric.getActiveObjects();

        group.forEach(g => {
            let items = g._objects;
            if (items.length) {
                g._restoreObjectsState();
                self.fabric.remove(g);
                items.forEach(i => self.fabric.add(i));
            }
        });

        //console.log('save your progress');
    }

    self.remove = (mb, ev) => {
        self.removeSelectedImages();
    }

    self.clear = (mb, ev) => {
        //console.trace(mb);
        window.showConfirm(self.resources.AreYouSure, self.resources.MoodboardResetAreYouSure, () => {
            self.fabric.clear();
            $('#canvas-background-text').show();
        });
    }

    // utility function to permit "with" against the appropriate product entry.
    self.getProductById = (productId) =>
        self.products().filter(p => p.productId == productId)[0];

    return self;
});

const scrapbookDefinition = ((data, parent) => {
    let self = Object.assign(data, {});

    self.parent = parent;

    self.resources = parent.resources;

    return self;
})

window.moodboardModel = moodboardViewModel(getPrefetchedResources());

ko.applyBindings(window.moodboardModel, document.querySelector('#member-wrapper'));

window.moodboardModel.init();