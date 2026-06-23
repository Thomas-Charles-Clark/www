const pagerViewModel = ((grid) => {
    let self = {
        parent: grid,
        page: ko.observable(1),
        pageSize: ko.observable(48),
        breadth: ko.observable(),
        sort: ko.observable()
    };

    self.filteredContent = grid.filteredContent;

    self.allProductPages = grid.allProductPages;

    self.totalCount = ko.computed(() => {
        let tpps = ko.unwrap(self.allProductPages);
        return tpps.length;
    });

    // This holds PageCount from the server. It is fed by prerendered binding.
    self.pages = ko.observable();

    self.pageCount = ko.pureComputed({
        read: () => {
            //if (self.parent.filteredContent().length) {

            let filteredCount = self.filteredContent().length;
            let totalCount = self.allProductPages().length;

            let countToUse = filteredCount > 0 ? filteredCount : totalCount;

            let size = self.pageSize();

            if (countToUse < size)
                return 1;
            else
                return Math.ceil(countToUse / size);

            //} else {
            //    return self.pages();
            //}
        }
    }, self).extend({
        rateLimit: {
            timeout: 10,
            method: 'notifyWhenChangesStop'
        }
    });

    self.page.subscribe(nv => {

        console.log('pager.page subscribe hit', nv);

        if (nv > self.pageCount()) {
            self.page(self.pageCount());
            return;
        }
        window.setTimeout(() => {
            let koWrapper = document.querySelector('.page-section-grid-nav');
            if (koWrapper && koWrapper.offsetTop < window.scrollY) {
                console.log('pager.page.subscribe scrolling to ', koWrapper.offsetTop)
                window.scrollTo({
                    top: koWrapper.offsetTop,
                    left: 0,
                    behavior: 'smooth'
                });
            }
        }, 100);
    })

    self.pageSize.subscribe(nv => {
        console.log('pager.pageSize subscribe hit', nv);
        if (self.page() > self.pageCount()) {
            self.page(self.pageCount());
        }
    });

    self.sort.subscribe(nv => {
        console.log('pager.sort subscribe hit', nv);
    });

    // Only true for Search at the moment
    self.ajax = ko.observable();

    // Only present if ajax === true
    self.ajaxEndpoint = ko.observable();

    // Only in use if ajax === true
    self.busy = ko.observable(false);

    // We have no desire to re-render the first page
    self.skipRender = ko.observable(true);

    self.previousPage = ko.pureComputed(() => self.page() + 1, self);

    self.nextPage = ko.pureComputed(() => self.page() + 1, self);

    self.navigatePrevious = () => {
        if (self.page() > 1) {
            self.page(self.page() - 1);
        }
    }

    self.navigateNext = () => {
        if (self.page() < self.pageCount()) {
            self.page(self.page() + 1);
        }
    }

    self.backEnabled = ko.pureComputed(() => self.page() > 1, self);

    self.forwardEnabled = ko.pureComputed(() => self.page() < self.pageCount(), self);

    self.breadthNegative = ko.observable(0);

    self.breadthPositive = ko.observable(0);

    self.rightEllipsis = ko.observable();

    self.leftEllipsis = ko.observable();

    self.rightEllipsisVisible = ko.pureComputed(() => {
        if (self.pageCount() == self.page())
            return false;
        if (self.pageCount() && self.page() && self.breadthPositive())
            return self.pageCount() > self.page() + self.breadthPositive();
        else
            return self.rightEllipsis();
    }, self);

    self.leftEllipsisVisible = ko.pureComputed(() => {
        //if (self.page() && self.breadthNegative()) {
            return (self.page() - self.breadthNegative()) > 1;
        //} else {
        //    return self.leftEllipsis();
        //}
    }, self);

    self.visiblePages = ko.pureComputed(() => {

        let page = self.page();
        let pageCount = self.pageCount();
        let breadth = 2;
        let breadthNegative = breadth;
        let breadthPositive = breadth;

        if (page + breadthPositive > pageCount) {
            breadthPositive = pageCount - page;
        }

        if (breadth < pageCount) {
            if (breadthPositive < breadth) {
                breadthNegative += breadth - breadthPositive;
            }
        }

        if (page < breadthNegative) {
            breadthNegative = page - 1;
        }

        while (page - breadthNegative < 1) {
            breadthNegative--;
        }

        if (breadthNegative === 0 && breadthPositive === 0)
            return [{
                page: 1,
                type: 'current'
            }];

        if (breadthNegative < breadth) {
            breadthPositive += breadth - breadthNegative;
        }

        while (page + breadthPositive > pageCount) {
            breadthPositive--;
        }

        self.breadthNegative(breadthNegative);

        self.breadthPositive(breadthPositive);

        let result = [];

        if (breadthNegative > 0) {
            result = result.concat(Array(breadthNegative).fill(0).map((fill, index) => {
                return {
                    page: page + -(index + 1),
                    type: 'prev'
                }
            }).reverse());
        }

        result.push({
            page: page,
            type: 'current'
        });

        if (breadthPositive > 0) {
            result = result.concat(Array(breadthPositive).fill(0).map((fill, index) => {
                return {
                    page: page + (index + 1),
                    type: 'next'
                }
            }));
        }

        return result;
    }, self).extend({
        rateLimit: {
            timeout: 10,
            method: 'notifyWhenChangesStop'
        }
    });

    self.renderPage = (page, completeFn) => {
        if (self.skipRender()) {
            self.skipRender(false);
        } else {
            self.busy(true);

            let data = {
                pageSectionId: self.parent.parent.pageSectionId,
                languageId: window.getLanguageId(),
                countryId: window.getCountryId(),
                page: self.page(),
                pageSize: self.pageSize(),
                sort: self.sort(),
                productTypeId: self.parent.productTypeId(),
                attributes: self.parent.wholeAttributeSelection().map(a => {
                    return {
                        AttributeID: a.attributeId,
                        AttributeGroupID: a.attributeGroupId,
                        AttributeOperator: a.attributeOperator,
                        ProductPageIDs: a.productPageIds
                    }
                }),
                visitor: window.getBase().Visitor,
                masterDefinition: window.getMasterDefinition()
            }

            if (self.parent.searchable()) {
                data.productPageIds = self.parent.filteredContent();
            }

            fetch(self.ajaxEndpoint(), {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-store',
                credentials: 'same-origin',
                credentials: 'include',
                body: JSON.stringify(data)
            })
            .then(window.handleFetchResponse)
            .then(renderedResults => {
                self.busy(false);

                window.setTimeout(() => {
                    self.parent.filterText.notifySubscribers();
                    self.parent.updateUrl();
                }, 0);

                completeFn(renderedResults.map(r => {
                    let a = productPagePodViewModel(self.parent);
                    a.data(r);
                    return a;
                }));
            })
            .catch(err => {
                self.busy(false);
                window.catchFetchErrors(err);
            })
        }
    }

    //self.changePage = () => {
    //    if (self.skipRender()) {
    //        self.skipRender(false);
    //    } else {
    //        self.busy(true);

    //        let data = {
    //            pageSectionId: self.parent.PageSectionID,
    //            languageId: window.getLanguageId(),
    //            countryId: window.getCountryId(),
    //            page: self.page(),
    //            pageSize: self.pageSize(),
    //            sort: self.sort(),
    //            productTypeId: self.parent.productTypeId(),
    //            attributes: self.parent.selectedAttributeItems.map(a => {
    //                return {
    //                    AttributeID: a.AttributeID,
    //                    AttributeGroupID: a.AttributeGroupID,
    //                    AttributeOperator: a.AttributeGroup.Operator
    //                }
    //            })
    //        }

    //        console.log('ProductPageGrid_Page calling with', data);

    //        fetch('/api/ProductPagesAPI/ProductPageGrid_Page', {
    //            method: 'POST',
    //            headers: window.fetchHeaders,
    //            cache: 'no-store',
    //            credentials: 'same-origin',
    //            body: JSON.stringify(data)
    //        })
    //            .then(window.handleFetchResponse)
    //            .then(data => {
    //                console.log('ProductPageGrid_Page returned', data);
    //            })
    //            .catch(err => {
    //                self.busy(false);
    //                window.catchFetchErrors(err);
    //            })
    //    }
    //}

    return self;
})