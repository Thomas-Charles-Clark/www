const attributeViewModel = ((attributeGroup, availableProductPageIds) => {
    let self = {
        parent: attributeGroup,
        ProductPageIDs: availableProductPageIds
    };

    console.log(`attributeViewModel being instantiated with ${self.ProductPageIDs.length} productPageIds`);

    self.data = ko.observable();

    // rather than setting this all the damned time, have it... calculate itself?
    // based on the parent presenting with a list of product page ids currently "in"?

    self.availableProductPageIds = ko.observableArray(availableProductPageIds);

    self.allProductPageIds = ko.observableArray(availableProductPageIds);

    //self.parent.parent.currentSet.subscribe(set => {
    //    self.availableProductPageIds(self.allProductPageIds().filter(pp => set.has(pp)));
    //});

    let dataSub = self.data.subscribe((newValue) => {
        Object.assign(self, newValue);
        dataSub.dispose();
    });

    self.selected = ko.pureComputed(() => {
        if (Array.isArray(self.parent.selection())) {
            return self.parent.selection().indexOf(self.FormValue) > -1
        } else {
            return self.parent.selection() == self.FormValue;
        }
    }, self);

    self.deselect = (data, event) => {
        if (self.parent.logicalOperator() === 'OR') {
            self.parent.selection('');
        } else {
            if (self.parent.singular()) {
                self.parent.selection('');
            } else {
                self.parent.selection.remove((s) => {
                    return s == self.FormValue;
                });
            }
        }
    }

    self.availableProductPagesDisplay = ko.pureComputed(() => {
        return `(${self.availableProductPageIds().length})`
    }, self).extend({
        rateLimit: {
            timeout: 10,
            method: 'notifyWhenChangesStop'
        }
    });

    self.disabled = ko.pureComputed(() => {
        return self.availableProductPageIds().length === 0;
    }, self).extend({
        rateLimit: {
            timeout: 10,
            method: 'notifyWhenChangesStop'
        }
    });

    return self;
});