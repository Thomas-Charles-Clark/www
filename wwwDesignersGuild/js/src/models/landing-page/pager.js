const pagerViewModel = ((grid) => {
    let self = {
        parent: grid,
        page: ko.observable(1),
        pageSize: ko.observable(),
        breadth: ko.observable(),
        sort: ko.observable()
    };

    self.page.subscribe(nv => {
        console.log('pager.page subscribe hit', nv);
        let koWrapper = document.querySelector('.page-section-grid-nav');
        if (koWrapper && koWrapper.offsetTop < window.scrollY) {
            console.log('pager.page.subscribe scrolling to ', koWrapper.offsetTop)
            window.scrollTo({
                top: koWrapper.offsetTop,
                left: 0,
                behavior: 'smooth'
            });
        }
    })

    // Only true for Search at the moment
    self.ajax = ko.observable();

    // Only present if ajax === true
    self.ajaxEndpoint = ko.observable();

    // Only in use if ajax === true
    self.busy = ko.observable(false);

    // We have no desire to re-render the first page
    self.skipRender = ko.observable(true);

    // What does this hold...?
    self.pages = ko.observable();

    // Where did this go...?
    self.pageSize = ko.observable();

    self.pageCount = ko.pureComputed({
        read: () => {
            if (self.parent.filteredContent().length) {

                let count = self.parent.filteredContent().length;
                let size = self.pageSize();

                if (count < size)
                    return 1;
                else
                    return Math.ceil(count / size);
            } else {
                return self.pages();
            }
        }
    }, self).extend({
        rateLimit: {
            timeout: 10,
            method: 'notifyWhenChangesStop'
        }
    });

    self.pageSize.subscribe((newValue) => {
        if (self.page() > self.pageCount()) {
            self.page(self.pageCount())
        }
    });

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
            fetch(self.ajaxEndpoint(), {
                method: 'POST',
                headers: window.fetchHeaders,
                cache: 'no-store',
                credentials: 'same-origin',
                credentials: 'include',
                body: JSON.stringify({
                    productPageIds: page,
                    objMaster: getMasterDefinition()
                })
            })
            .then(window.handleFetchResponse)
            .then(renderedResults => {
                self.busy(false);
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

    return self;
})