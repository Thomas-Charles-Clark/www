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

    return self;
});