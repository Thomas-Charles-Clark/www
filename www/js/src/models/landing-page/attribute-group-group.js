const attributeGroupGroupViewModel = ((record, productPageGrid) => {
    let self = Object.assign({
        parent: productPageGrid
    }, record);

    self.attributeGroups = ko.computed(() => {
        return self.parent.attributeGroups().filter(ag => ag.attributeGroupGroupId() === self.AttributeGroupGroupID);
    }, self);

    self.selectionDisplay = ko.computed(() => {
        let count = null;

        self.attributeGroups().forEach(ag => {
            if (Array.isArray(ag.selection())) {
                if (ag.selection().length)
                    count += ag.selection().length;
            } else {
                if (ag.selection()) {
                    count += 1
                }
            }   
        })

        if (count) {
            return `(${count})`;
        }
    }, self);

    self.visible = ko.pureComputed(() => {
        if (self.parent.productTypes().length) {
            if (self.parent.productType() == 'all') {
                return self.attributeGroups().find(ag => self.parent.parent.Definition.CommonAttributeGroups.flatMap(cag => [cag.AttributeGroupID]).includes(ag.attributeGroupId()));
            } else {
                return self.attributeGroups().flatMap(ag => ag.productTypes()).flatMap(pt => [pt.FormValue]).includes(self.parent.productType());
            }
        } else {
            return true;
        }
    }, self);

    return self;
});