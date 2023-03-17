const articleGridViewModel = ((pageSection) => {
    let self = {
        parent: pageSection
    };

    self.domElement = ko.observable();

    //self.domElement.skipScroll = true;

    self.allArticlesReady = ko.observable(false);

    self.articleTypeId = ko.observable();

    self.articleTypeId.skipSub = true; // skip the initial prerendered init binding sub

    self.articleTypeId.subscribe(nv => {
        if (self.articleTypeId.skipSub === true) {
            self.articleTypeId.skipSub = false;
        } else {
            window.location.href = nv;
        }
    });

    self.articleSubTypeId = ko.observable();

    self.articleSubTypeId.skipSub = true; // skip the initial prerendered init binding sub

    self.articleSubTypeId.subscribe(nv => {
        if (self.articleSubTypeId.skipSub === true) {
            self.articleSubTypeId.skipSub = false;
        } else {
            window.location.href = nv;
        }
    });

    self.createArticlePod = () => articlePodViewModel();

    self.pager = pagerViewModel(self);

    self.allArticles = ko.observableArray([]);

    self.selectedAttributes = ko.observableArray([]);

    self.visibleArticles = ko.observableArray([]);

    self.filteredContent = ko.observableArray([]);

    self.articleIdsForCurrentAttributes = ko.observableArray([]);

    self.articleRenderer = ko.pureComputed({
        read: () => {

            let page = self.pager.page();

            let pageSize = self.pager.pageSize();

            let sort = self.pager.sort();

            let attributes = self.selectedAttributes();

            let results = attributes.some(() => true)
                ? self.allArticles().flatMap(pp => self.articleIdsForCurrentAttributes().includes(pp.ArticleID) ? [pp] : [])
                : self.allArticles();

            self.filteredContent(results);

            let aposReg = /'/g;
            let empty = '';

            switch (sort) {
                case "alphaAZ":
                    results.sort((a, b) => {
                        if (a.Heading?.replace(aposReg, empty) < b.Heading?.replace(aposReg, empty))
                            return -1;
                        if (a.Heading?.replace(aposReg, empty) > b.Heading?.replace(aposReg, empty))
                            return 1;
                        if (a.SubHeading?.replace(aposReg, empty) < b.SubHeading?.replace(aposReg, empty))
                            return -1;
                        if (a.SubHeading?.replace(aposReg, empty) > b.SubHeading?.replace(aposReg, empty))
                            return 1;
                        return 0;
                    });
                    //results = Enumerable.From(results)
                    //    .OrderBy(p => p.AZSortOrder)
                    //    .ThenBy(p => p.MerchandisedSortOrder)
                    //    .ToArray();
                    break;
                case "alphaZA":
                    results.sort((a, b) => {
                        if (a.Heading?.replace(aposReg, empty) > b.Heading?.replace(aposReg, empty))
                            return -1;
                        if (a.Heading?.replace(aposReg, empty) < b.Heading?.replace(aposReg, empty))
                            return 1;
                        if (a.SubHeading?.replace(aposReg, empty) < b.SubHeading?.replace(aposReg, empty))
                            return -1;
                        if (a.SubHeading?.replace(aposReg, empty) > b.SubHeading?.replace(aposReg, empty))
                            return 1;
                        return 0;
                    });
                    //results = Enumerable.From(results)
                    //    .OrderByDescending(p => p.AZSortOrder)
                    //    .ThenBy(p => p.MerchandisedSortOrder)
                    //    .ToArray();
                    break;
                case "new":
                    results.sort((a, b) => {
                        if (a.CreatedDT < b.CreatedDT)
                            return -1;
                        if (a.CreatedDT > b.CreatedDT)
                            return 1;
                        if (a.Heading?.replace(aposReg, empty) < b.Heading?.replace(aposReg, empty))
                            return -1;
                        if (a.Heading?.replace(aposReg, empty) > b.Heading?.replace(aposReg, empty))
                            return 1;
                        return 0;
                    });
                    //results = Enumerable.From(results)
                    //    .OrderBy(p => p.NewSort)
                    //    .ToArray();
                    break;
                default:
                    //results = Enumerable.From(results)
                    //    .OrderBy(p => p.MerchandisedSortOrder)
                    //    .ToArray();
                    results.sort((a, b) => {
                        if (a.SortOrder < b.SortOrder)
                            return -1;
                        if (a.SortOrder > b.SortOrder)
                            return 1;
                    });
                    break;
            }

            //results = Enumerable.From(results)
            //    .Skip((page - 1) * pageSize)
            //    .Take(pageSize)
            //    .ToArray();

            results = results.slice((page - 1) * pageSize, ((page - 1) * pageSize) + pageSize);
            //.Skip((page - 1) * pageSize)
            //.Take(pageSize)
            //.ToArray();

            //console.log('articleRenderer', results);

            self.visibleArticles(results);

            //if (self.domElement.skipScroll == true) {
            //    if (self.allArticlesReady())
            //        self.domElement.skipScroll = false;
            //} else {
            //    if (self.domElement.hasOwnProperty('DOMElement')) {
            //        if (window.scrollY > self.domElement.DOMElement.offsetTop) {
            //            window.scrollTo({
            //                top: self.domElement.DOMElement.offsetTop,
            //                left: 0,
            //                behavior: 'smooth'
            //            });
            //        }
            //    }
            //}

            return results;
        }
    }, self)
    .extend({
        rateLimit: {
            method: 'notifyWhenChangesStop',
            timeout: 10
        }
    });

    self.articleRenderer.subscribe((changes) => {
        let formData = new FormData(document.querySelector(`#page-section-${self.parent.PageSection.PageSectionID}-form`));
        let qs = new URLSearchParams(formData);
        let url = new URL(window.location);
        url.search = qs;
        window.history.replaceState({}, '', url);
    });

    self.allArticlesInitialised = () => {
        self.allArticlesReady(true);
    }

    return self;
});