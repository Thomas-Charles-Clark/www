const productTypeViewModel = ((productPageGrid, availableProductPageIds) => {
    let self = {
        parent: productPageGrid
    };

    self.data = ko.observable();

    let dataSub = self.data.subscribe((newValue) => {
        Object.assign(self, newValue);
        dataSub.dispose();
    });

    self.availableProductPageIds = ko.observableArray(availableProductPageIds);

    //self.parent.currentSet.subscribe(set => {
    //    if (self.hasOwnProperty('ProductPageIDs')) {
    //        if (set.length) {
    //            let x = [...new Set(self.ProductPageIDs.filter(p => set.has(p)))];
    //            self.availableProductPageIds(x);
    //        } else {
    //            self.availableProductPageIds(self.ProductPageIDs);
    //        }
    //    }
    //}, self);

    self.selected = ko.pureComputed(() => {
        return self.parent.selection().indexOf(self.FormValue) > -1
    }, self);

    self.deselect = (data, event) => {
        self.parent.selection.remove((s) => {
            return s == self.FormValue;
        });
    }

    self.label = ko.pureComputed(() => {
        return `${self.Name} (${self.availableProductPageIds().length})`
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