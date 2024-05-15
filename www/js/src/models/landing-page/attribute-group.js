const attributeGroupViewModel = ((name, lexicographicalSortOrder, productPageGrid) => {
    let self = {
        name: name,
        lexicographicalSortOrder: lexicographicalSortOrder,
        parent: productPageGrid
    };

    self.attributeGroupId = ko.observable();

    self.attributeGroupGroupId = ko.observable();

    self.singular = ko.observable();

    self.logicalOperator = ko.observable();

    self.attributes = ko.observableArray([]);

    self.selection = ko.observableArray([]);

    self.selectionDisplay = ko.pureComputed(() => {
        if (Array.isArray(self.selection())) {
            if (self.selection().length)
                return `(${self.selection().length})`;
        } else {
            if (self.selection()) {
                return `(1)`
            }
        }   
    }, self);

    self.selectedAttributes = () => {
        if (self.singular()) {
            if (self.selection() != '') {
                return self.attributes().flatMap(a => {
                    return a.FormValue == self.selection() ? [a] : []
                });
            }
        } else {
            if (self.selection().length) {
                return self.attributes().flatMap(a => {
                    return self.selection().indexOf(a.FormValue) > -1 ? [a] : []
                });
            }
        }
    };

    self.productTypes = ko.observableArray([]);

    self.visible = ko.pureComputed(() => {
        if (self.parent.productTypes().length) {
            if (self.parent.productType() == 'all') {
                return self.parent.parent.Definition.CommonAttributeGroups.flatMap(cag => [cag.AttributeGroupID]).includes(self.attributeGroupId());
            } else {
                return self.productTypes().flatMap(pt => [pt.FormValue]).includes(self.parent.productType());
            }
        } else {
            return true;
        }
    }, self);

    self.involveInFilterIndexing = (productType) => {
        return self.productTypes().filter(pt => pt.FormValue == productType && pt.LexicographicalSortOrder != null).length;
    }

    return self;
});