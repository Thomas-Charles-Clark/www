const productPageGridViewModel = ((pageSection) => {
    let self = {
        parent: pageSection,
        resources: pageSection.resources
    };

    //#region Properties

    self.domElement = ko.observable();

    //self.domElement.skipScroll = true;

    self.productType = ko.observable();

    self.productTypes = ko.observableArray([]);

    self.attributeGroupGroups = ko.observableArray([]);

    self.attributeGroups = ko.observableArray([]);

    self.attributes = ko.observableArray([]);

    self.attributeCount = ko.observable();

    self.selectedAttributes = ko.observableArray([]);

    self.selectedAttributes.subscribe(nv => {
        console.log('selectedAttributes subscribe hit');
    });

    self.searchable = ko.observable();

    self.searchable.subscribe(nv => {
        console.log('searchable subscribe hit', nv);
    });

    // Not in use by the search results product page grid.+
    self.allProductPages = ko.observableArray([]);

    self.allProductPages.subscribe(nv => {
        console.log('allProductPages subscribe hit', nv);
    });

    self.allProductPagesReady = ko.observable(false);

    self.visibleProductPages = ko.observableArray([]);

    self.filterCount = ko.observable();

    self.filteredContent = ko.observableArray([]); //this is used by the pager

    self.filteredContent.subscribe(nv => {
        console.log('filteredContent subscribe hit', nv);
    });

    self.lazyImageNonce = ko.observable();

    self.readyToCalculateAvailability = ko.observable(false);

    self.readyToCalculateAvailability.subscribe(nv => {
        console.log('readyToCalculateAvailability subscribe hit', nv);
    });

    self.offcanvas = ko.observable();

    self.offcanvasSwipeThreshold = 20;

    self.offcanvasTouchDeltaX = 0;

    self.urlParsingRegex = new RegExp(/(.*)(\/)(.*)\/([a-z][\d]+)(\??)(.*)$/g);
    //(.*)\/([^\d\/]+)(\/)([a-z][\d]+)(\??)(.*)$

    // Only in use for the search's use of this viewModel.
    // Because sorting should be done server side to avoid having to deliver n objects 
    // just so the client can sort
    // but we don't want to perform the necessary action on the first hit, as prerendering needs to
    // be allowed to fill the pager's sort value.
    self.skipSortSub = true;

    self.filterText = ko.observable('');

    self.productTypeText = ko.observable();

    self.productTypeTextPlural = ko.observable();

    self.originalTitle = ko.observable(getFilterIndexingTitle());

    self.attributeSignatures = null;

    self.filterTextComputer = ko.computed(() => {

        // grab dependencies
        let productTypeText = self.productTypeText();

        let productTypeTextPlural = self.productTypeTextPlural();

        let filterText = self.filterText();

        let selectedAttributeIdsSignature = self.selectedAttributes().map(sa => {
            return self.attributes().find(a => a.FormValue == sa).AttributeID;
        }).join('.');

        let filterIndexCombination = self.attributeSignatures && self.attributeSignatures.find(a => a.AttributeSignature == selectedAttributeIdsSignature);

        let filterIndexCombinationFound = filterIndexCombination != null;

        // FooterText is a misnomer: it could come anywhere on the page argh
        // can't change that easily as it's coming out of the database, try to remember to change it eventually
        let filterIndexCombinationText = filterIndexCombination?.FooterText;
        
        // parent doesn't necessarily have Definition yet; init might not have taken place yet
        if (self.parent.hasOwnProperty('Definition')) {

            // sanity check: filter indexing must be enabled on the page
            if (self.parent.Definition.FilterIndexingEnabled) {

                // bugger about with the product type text
                if (productTypeTextPlural) {
                    document.querySelectorAll('.product-type-propercase-text').forEach(e => {
                        e.innerHTML = `${productTypeTextPlural}`;
                    });
                    document.querySelectorAll('.product-type-text').forEach(e => {
                        e.innerHTML = `${productTypeTextPlural.toLowerCase()}`;
                    });
                } else {
                    document.querySelectorAll('.product-type-propercase-text').forEach(e => {
                        e.innerHTML = e.dataset['originalContents'];
                    });
                    document.querySelectorAll('.product-type-text').forEach(e => {
                        e.innerHTML = e.dataset['originalContents'];
                    });
                }

                // bugger about with the result count
                document.querySelectorAll('.filter-indexing-result-count').forEach(e => {
                    let r = self.allProductPages().length > 1 ? self.resources.AttributeFilterTextResultsFoundFor : self.resources.AttributeFilterTextResultFoundFor;
                    r = r.replace('{Count}', self.filteredContent().length);
                    if (productTypeTextPlural) {
                        r = r.replace('{AttributeFilterText}', `${filterText} ${productTypeTextPlural}`.trim());
                        e.innerHTML = r;
                    }
                });


                // do some seriously annoying shit
                // what's the objective here?
                // SIMPLIFYYYY, MAAAN!
                // Okay, we have two possible states. Either:
                //      we are filtered towards a combination which we wish Google to index
                //      or
                //      we are not
                //
                //      filterIndexCombinationEnabled... shouldn't be necessary to tell us that: the absence of a record corresponding to the current signature tells us
                //      // buuuut we might have a record with no footer text specified so might as well keep testing for it

                if (filterIndexCombinationFound) {
                    // great, swap all the smart tags out as appropriate
                } else {
                    // boo, get the original boilerplate for the... page...? back in place. The "unfiltered" state.
                }

                // this is BaseViewModel.AttributeFilterText, built from the URL by lex sort orders of attribute groups then attributes.
                if (filterText) {

                    document.querySelectorAll('.filter-text').forEach(e => {
                        e.innerHTML = `${filterText} `;
                    });

                    document.querySelectorAll('.no-filter-text').forEach(e => {
                        e.innerHTML = '';
                    });

                    if (filterIndexCombinationText) {
                        document.querySelectorAll('.no-filter-visible').forEach(e => {
                            e.classList.add('d-none');
                        });
                    } else {
                        document.querySelectorAll('.no-filter-visible').forEach(e => {
                            e.classList.remove('d-none');
                        });
                    }

                    // self.originalTitle will only ever be filled by FilterIndexCombination_Language if at all.
                    if (!self.originalTitle()) {
                        document.title = `${filterText} ${productTypeTextPlural} | Designers Guild`;
                    }

                    self.allProductPages().forEach(pp => {
                        if (typeof pp === 'object') {
                            if (ko.isObservable(pp.Heading)) {
                                if (pp.ShortHeading != null) {
                                    pp.Heading(`${pp.ShortHeading} ${filterText} ${productTypeText}`.replace(/\s\s/g, ' ').trim());
                                }
                            }
                        }
                    });

                } else {

                    document.querySelectorAll('.filter-text').forEach(e => {
                        e.innerHTML = e.dataset['originalContents'];
                    });

                    document.querySelectorAll('.no-filter-text').forEach(e => {
                        e.innerHTML = e.dataset['originalContents'];
                    });

                    document.querySelectorAll('.no-filter-visible').forEach(e => {
                        e.classList.remove('d-none');
                    });

                    window.lazyImages();

                    if (self.originalTitle())
                        document.title = self.originalTitle();

                    self.allProductPages().forEach(pp => {
                        if (typeof pp === 'object') {
                            if (ko.isObservable(pp.Heading)) {
                                pp.Heading(pp.OriginalHeading);
                            }
                        }
                    });
                }

                if (filterIndexCombinationText) {
                    document.querySelectorAll('.attribute-signature-text').forEach(e => {
                        e.innerHTML = `${filterIndexCombinationText}`;
                    });
                } else {
                    document.querySelectorAll('.attribute-signature-text').forEach(e => {
                        e.innerHTML = e.dataset['originalContents'] || null;
                    });
                }
            }
        }
    }, self).extend({
        rateLimit: {
            timeout: 20,
            method: 'notifyWhenChangesStop'
        }
    });

    //#endregion

    //#region Components

    self.pager = pagerViewModel(self);

    //#endregion

    //#region Subscriptions

    self.allProductPagesReady.subscribe(nv => {
        console.log('allProductPagesReady subscribe hit', nv);
        self.allProductPages().forEach(pp => {
            if (typeof pp === 'object') {
                pp.Heading = ko.observable(pp.Heading);
                pp.OriginalHeading = ko.unwrap(pp.Heading);
            }
        });
    });

    self.attributeCount.subscribe(nv => {
        self.readyToCalculateAvailability(true);
    });

    // Necessarily lifted
    // With thanks https://stackoverflow.com/questions/7802822/all-possible-combinations-of-a-list-of-values#answer-57058345
    // further explained by https://stackoverflow.com/questions/57948316/why-is-bitwise-operator-needed-in-this-powerset-generator#answer-57948404
    self.powerSet = (array) => {
        return Array.from({ length: 1 << array.length }).map((element, index) => {
            return array.flatMap((d, i) => {
                return (index & (1 << i)) != 0 ? [d] : [];
            });
        }).filter(x => x.length);
    }

    // This method has to be lifted too
    self.updateUrl = () => {
        if (self.parent.hasOwnProperty('PageSection')) {
            if (document.querySelector(`#page-section-${self.parent.PageSection.PageSectionID}-form`)) {

                // build the query string parameters from the page section form
                let e = new FormData(document.querySelector(`#page-section-${self.parent.PageSection.PageSectionID}-form`));

                let obj = {};

                console.log(`updateUrl #page-section-${self.parent.PageSection.PageSectionID}-form FormData.entries():`);

                for (const kvp of e.entries()) {
                    if (obj.hasOwnProperty(kvp[0])) {
                        obj[kvp[0]] = [obj[kvp[0]]];
                        obj[kvp[0]].push(kvp[1]);
                    } else {
                        obj[kvp[0]] = kvp[1];
                    }
                    console.log(kvp);
                }

                if (self.offcanvas.DOMElement && document.getElementById(`${self.offcanvas.DOMElement.id}-form`)) {
                    // and from the grid filters (this is now necessary because I've had to detach the filters and reattach them to body... because page section z-indexing, argh)
                    let f = new FormData(document.querySelector(`#${self.offcanvas.DOMElement.id}-form`));

                    console.log(`updateUrl #${self.offcanvas.DOMElement.id}-form FormData.entries():`);

                    for (const kvp of f.entries()) {
                        if (obj.hasOwnProperty(kvp[0])) {
                            obj[kvp[0]] = [obj[kvp[0]]];
                            obj[kvp[0]].push(kvp[1]);
                        } else {
                            obj[kvp[0]] = kvp[1];
                        }
                        console.log(kvp);
                    }
                }

                let qs = new URLSearchParams(obj);

                // end of building query string parameters

                let url = new URL(window.location);

                let urlString = url.href;

                url.search = qs;

                let foo = 'bar';

                // if we're doing the filter indexing dance
                if (self.parent.hasOwnProperty('Definition')) {
                    // [dbo].[Page].[EnableFilterIndexing]
                    if (self.parent.Definition.FilterIndexingEnabled) {

                        // we might not have any selected attributes, in which case we still need to do the rebuild beneath
                        // here, have an unhelpful variable name...
                        let alreadyProcessed = false;

                        // match the urlString against our urlParsingRegex:   (/(.*)(\/)(.*)\/([a-z][\d]+)(\??)(.*)$/g);
                        let matches = self.urlParsingRegex.exec(urlString);

                        // we definitely should have matches and can't proceed unless we do - and we must have at least 4
                        if (matches != null && matches.length >= 4) {

                            // the malleable segment is the segment directly preceeding the contentType and Id
                            // e.g. https://www.designersguild.com/uk/blah-blah-blah/malleable-segment/l1024
                            let malleableSegment = matches[3];

                            // remove the original semantic string from the malleable segment. In the above example
                            // this would be "malleable-segment". This must be re-added as a suffix once we're finished with the attribute dance
                            malleableSegment = malleableSegment.replace(window.pageModel.OriginalSemanticString, '');

                            // split the malleable segment on hyphens, removing empty entries
                            let originalSplit = malleableSegment.split('-').filter(x => x != '');

                            // produce the power set of the resultant array
                            // this gives all possible combinations of ordering of elements
                            // meaning we can provide e.g. english-heritage-cushions and one of the resultant elements in the power set is guaranteed to be english-heritage
                            let powerSet = self.powerSet(originalSplit);

                            let split = ko.observableArray();

                            // split the power segment remnants on hyphen: this gives us our attributes
                            powerSet.forEach(potentialCombination => {
                                split.push({
                                    attribute: potentialCombination.join('-'),
                                    lexSortOrder: null,
                                    logicalOperator: null
                                });
                            });

                            //console.trace(ko.unwrap(split));

                            // here we remove anything which is not a currently select attribute
                            // currently selected attributes are determined server-side by parsing the query string for attributeGroups and their value lists
                            // these are rendered into the response and thence into the client-side model(s)
                            split.remove(x => {
                                return self.selectedAttributes().indexOf(x.attribute) === -1;
                                //&& x.attribute != window.pageModel.OriginalSemanticString
                            });

                            // for each of our currently selected attributes (selectedAttributes is a simple array of FormValues)
                            self.selectedAttributes().forEach(a => {

                                // identify the full object from the FormValue - this is probably insufficient
                                // as we could have multiple attributes sharing the same FormValue. How else am I to do this?
                                var attribute = self.attributes().filter(x => x.FormValue == a)[0];

                                // If attribute's parent attributeGroup involves itself with filter indexing given the currently selected product type
                                // (this is just based on the product type having a non-null lexicographical sort order)
                                if (attribute.parent.involveInFilterIndexing(self.productType())) {

                                    // Then add the current selectedAttribute to our split array if it isn't already present 
                                    if (!split().filter(s => s.attribute == a).length) {
                                        let ag = self.attributeGroups().filter(ag => ag.attributeGroupId() == attribute.AttributeGroupID)[0];
                                        split.push({
                                            attribute: a,
                                            attributeName: attribute.Name,
                                            lexSortOrder: null,
                                            logicalOperator: ag.logicalOperator()
                                        })
                                    }

                                    // find those attribute groups which engage in filter indexing given the currently selected product type
                                    let attributeGroups = self.attributeGroups().filter(ag => ag.involveInFilterIndexing(self.productType()));

                                    // because we're forEaching over an array avoid modifying it within the iteration
                                    let toRemove = [];
                                    let toAdd = [];

                                    // iterate over the array of attributes lifted from the malleable segment
                                    // (which now includes the currently considered selectedAttribute if it didn't initially)
                                    split().forEach(s => {
                                        // only if this split is still contained within our attribute selection
                                        // [previously also considered whether or not it was part of the original URL]
                                        if (self.selectedAttributes().indexOf(s.attribute) > -1) {
                                            //|| s.attribute == window.pageModel.OriginalSemanticString) {

                                            // identify the attributeGroup parent to this split entry
                                            let thisAttributeGroup = attributeGroups.filter(ag => ag.attributes().flatMap(a => a.FormValue).indexOf(s.attribute) > -1)[0];

                                            // if we have an attributeGroup parent (we always should, but someone might have typed nonsense in there manually)
                                            if (thisAttributeGroup) {

                                                // pull the lexicographical sort order 
                                                let lexSortOrder = thisAttributeGroup.productTypes().filter(pt => pt.FormValue == self.productType())[0].LexicographicalSortOrder;

                                                // if that isn't null, set it on this split entry
                                                if (lexSortOrder != null) {
                                                    s.lexSortOrder = lexSortOrder;
                                                    s.logicalOperator = thisAttributeGroup.logicalOperator() // as well as the logical operator... might not need this
                                                }

                                                // if this attribute group performs a logical OR within itself
                                                //if (thisAttributeGroup.logicalOperator() == 'OR') {

                                                // sanity check, this should be true...
                                                if (attribute.AttributeGroupID == thisAttributeGroup.attributeGroupId()) {

                                                    // mark this current entry for removal
                                                    toRemove.push(s);

                                                    // and if our additions does not yet already contain this entry, insert it
                                                    // this avoids doubling up of attributes, as we're necessarily doing a nested loop dance
                                                    if (toAdd.filter(ta => {
                                                        return ta.attribute == attribute.FormValue &&
                                                            ta.attributeName == attribute.Name &&
                                                            ta.lexSortOrder == lexSortOrder &&
                                                            ta.logicalOperator == thisAttributeGroup.logicalOperator()
                                                    }).length == 0) {
                                                        toAdd.push({
                                                            attribute: a,
                                                            attributeName: attribute.Name,
                                                            lexSortOrder: lexSortOrder,
                                                            logicalOperator: thisAttributeGroup.logicalOperator()
                                                        })
                                                    }
                                                }
                                                //} else {
                                                //    toAdd.push({
                                                //        attribute: a,
                                                //        attributeName: attribute.Name,
                                                //        lexSortOrder: lexSortOrder,
                                                //        logicalOperator: thisAttributeGroup.logicalOperator()
                                                //    })
                                                //}
                                            }
                                        } else {
                                            // this split entry is no longer present in our selectedAttributes: remove it
                                            toRemove.push(s);
                                        }
                                    });

                                    // remove all of the removals
                                    toRemove.forEach(s => {
                                        split.remove(e => {
                                            return e.attribute == s.attribute &&
                                                e.attributeName == s.attributeName &&
                                                e.lexSortOrder == s.lexSortOrder &&
                                                e.logicalOperator == s.logicalOperator
                                        });
                                    });

                                    // add all of the additions
                                    toAdd.forEach(s => {
                                        split.push(s);
                                    });

                                    // sort by lexicographical sort order
                                    split(split.sorted((a, b) => {
                                        return a.lexSortOrder == b.lexSortOrder ? 0
                                            : a.lexSortOrder < b.lexSortOrder ? -1
                                                : 1;
                                    }));
                                        
                                }
                            });

                            // Set the filter text for text replacement funtimes
                            self.filterText(split().map(s => s.attributeName).join(' '));

                            // re-add the original semantic string
                            // if we have a singular product type defined for the page, great
                            if (window.pageModel.CategoryPageProductType) {
                                self.productTypeText(window.pageModel.CategoryPageProductType.Name);
                                self.productTypeTextPlural(window.pageModel.CategoryPageProductType.NamePlural);
                                split.push({
                                    attribute: window.pageModel.CategoryPageProductType.URLKeywords
                                });
                            } else {
                                split.push({
                                    attribute: window.pageModel.OriginalSemanticString
                                });
                            }

                            let segment = split().map(s => s.attribute).join('-');

                            urlString = urlString.replace(self.urlParsingRegex, `$1/${segment}/$4`);

                            // can't for the life of me think what this bullshit was for, commenting it out...
                            // "if we haven't processed at least once, we still need to do this rebuilding bit"
                            //if (!alreadyProcessed) {

                            //    // clear the filter text
                            //    self.filterText('');

                            //    // if we haven't processed at least once then we haven't had the chance to add the originalSemanticString to the split array
                            //    split.push({
                            //        attribute: window.pageModel.OriginalSemanticString
                            //    })

                            //    // update the url string
                            //    urlString = urlString.replace(self.urlParsingRegex, `$1/${split().map(s => s.attribute).join('-')}/$4`);

                            //    alreadyProcessed = true;
                            //}
                        }
                    }
                }

                //console.log(urlString);

                // 09/05/2024
                // We added some stuff to let AttributeSignatureText still be used and displayed whether or not a particular attribute set should be indexable by Google
                let selectedAttributeIdsSignature = self.selectedAttributes().map(sa => {
                    return self.attributes().find(a => a.FormValue == sa).AttributeID;
                }).join('.')

                let attributeCombinationEnabled = self.attributeSignatures && self.attributeSignatures.find(a => a.AttributeSignature == selectedAttributeIdsSignature)?.Enabled;

                // 09/05/2024
                // Amine doesn't want the URL updating if the thing is... something...
                if (attributeCombinationEnabled === true || self.selectedAttributes().length === 0) {
                    url = new URL(urlString);

                    url.search = qs;

                    //// avoid updating the URL initially
                    //if (self.parent.Definition &&
                    //    self.parent.Definition.FilterIndexingEnabled &&
                    //    self.skippedFirstUrlUpdate == null) {
                    //    self.skippedFirstUrlUpdate = true;
                    //} else {
                    //    console.log('updating URL', url);
                    //    window.history.replaceState({}, '', url);
                    //}

                    window.history.replaceState({}, '', url);

                }
                else
                {
                    // only update the query string, resetting the urlkeywords to the base for the page.



                    let urlKeywords = window.pageModel.CategoryPageProductType == null
                        ? window.pageModel.MasterDefinition.UrlBreakdown.SemanticString
                        : window.pageModel.CategoryPageProductType.URLKeywords;

                    url = new URL(window.location.href.replace(self.urlParsingRegex, `$1/${urlKeywords}/$4`));

                    url.search = qs;

                    window.history.replaceState({}, '', url);
                }
            }
        }
    }

    self.parentSub = self.parent.data.subscribe(d => {
        self.attributeCount(self.parent.Definition.AttributeGroups.flatMap(ag => ag.Attributes).length);

        if (self.parent.Definition.Searchable) {
            console.log('searchable');
            self.pager.sort.subscribe(nv => {
                if (self.skipSortSub) {
                    self.skipSortSub = false;
                } else {
                    self.pager.busy(true);
                    // this is necessary because we have two sets of controls: the topmost and the bottom.
                    // only the topmost can carry name="..." and be involved in the form submission / url forming.
                    // this means that if you change sort using the bottom control, we need to allow JS cycles to
                    // permit KO to update the topmost (the one which involves itself in the form & url) with the
                    // new value.
                    window.setTimeout(() => {
                        self.updateUrl();
                        window.location.reload();
                    }, 0);
                }
            });
        }

        self.parentSub.dispose();
    });

    self.attrSub = self.attributes.subscribe(nvs => {
        if (nvs.length == self.attributeCount()) {
            self.readyToCalculateAvailability(true);
            self.attrSub.dispose();
        }
    });

    self.attributeGroups.subscribe(nvs => {
        nvs.forEach(ag => {
            ag.attributes.subscribe(a => {
                self.attributes.push(...a);
            });
        });
    });

    self.productType.subscribe(nv => {
        if (getContentId() === 76) {
            if (typeof window.searchBoxModel === 'object') {
                window.searchBoxModel.productType(nv);
            }
        }
        // this is because some product page grids when you change product type
        // from the offcanvas immediately reveal the Colours attribute group opened
        // but with the images not yet lazily loaded in
        window.setTimeout(() => {
            window.lazyImages();
        }, 0);
        self.pager.page(1);
    });

    self.selectedAttributes.subscribe(nv => {
        self.pager.page(1);
    });

    self.offcanvas.subscribe(nv => {
        self.offcanvas.DOMElement.addEventListener('touchstart', self.offcanvasTouchStart, { passive: true });
        self.offcanvas.DOMElement.addEventListener('touchmove', self.offcanvasTouchMove, { passive: true });
        self.offcanvas.DOMElement.addEventListener('touchend', self.offcanvasTouchEnd, { passive: true });

        self.offcanvas.DOMElement.remove();
        let form = document.createElement("form");
        form.id = `${self.offcanvas.DOMElement.id}-form`;
        form.appendChild(self.offcanvas.DOMElement);
        document.body.appendChild(form);
    });

    //#endregion

    //#region Computeds

    self.selectedProductType = ko.pureComputed(() => {
        console.log('selectedProductType computing');
        if (self.productTypes().length)
            return self.productTypes().flatMap(pt => pt.FormValue == self.productType() ? [pt] : [])[0];
        else
            return null;
    }, self);

    self.productTypeId = ko.pureComputed(() => {
        console.log('productTypeId computing');
        return ko.unwrap(self.selectedProductType)?.ProductTypeID;
    }, self);

    document.querySelector('body').addEventListener('searchBoxReady', () => {
        self.labelNonce.valueHasMutated();
    });

    self.labelNonce = ko.observable();

    self.filteredContentLabel = ko.pureComputed(() => {
        let result = ``;
        let nonce = self.labelNonce();
        if (self.searchable() && window.hasOwnProperty('searchBoxModel') && window.searchBoxModel.searchTerm()) {
            result = `"${window.searchBoxModel.searchTerm()}"`;
        }
        if (self.selectedProductType() && self.selectedProductType().ProductTypeID != 0) {
            return `${result} ${self.selectedProductType().Name} (${self.filteredContent().length})`.trim();
        } else {
            return `${self.parent.PageSection.Heading}`.trim();
        }
    });

    self.attributeSelectionComputer = ko.computed(() => {
        let attributes = [];
        let attributeGroups = self.attributeGroups();

        //attributeGroups.sort((a, b) => {
        //    return a.lexicographicalSortOrder > b.lexicographicalSortOrder ? -1 : 1;
        //});

        // take dependencies on attribute groups' selection observables
        attributeGroups.forEach((ag) => {
            let selection = ko.unwrap(ag.selection);
            if (Array.isArray(selection))
                selection.forEach(a => {
                    if (attributes.indexOf(a) == -1)
                        attributes.push({ selection: a, lexicographicalSortOrder: ag.lexicographicalSortOrder });
                });
            else {
                if (selection != '')
                    attributes.push({ selection: selection, lexicographicalSortOrder: ag.lexicographicalSortOrder });
            }
        });

        attributes = attributes.sort((a, b) => {
            return a.lexicographicalSortOrder < b.lexicographicalSortOrder ? -1 : 1;
        }).map(a => a.selection);

        // I think we don't want to include Product Type as a clearable filter. 25/08/2022
        //self.filterCount(attributes.length + (self.productType() ? (self.productType() == 'all' ? 0 : 1) : 0));

        self.filterCount(attributes.length);

        if (!attributes.equals(self.selectedAttributes()))
            self.selectedAttributes(attributes);

        return attributes;
    }, self);

    self.determineAvailableProductPages = ko.computed(() => {
        console.log('determineAvailableProductPages computing');
        // We're only ready once we've built all attributes
        if (self.readyToCalculateAvailability()) {
            // Wait for a product type to be selected (one always will be)
            if (self.selectedProductType()) {
                // Then use that product type as the basis.
                let initialProductPageIds = self.selectedProductType().ProductPageIDs;

                let attributeProductPageIds = [];

                self.selectedAttributes().forEach(a => {
                    // Finding the attribute group whose selection contains this attribute (self.selectedAttributes contains FormValues)
                    let attributeGroup = self.attributeGroups.peek().flatMap(attrGrp => attrGrp.selection().indexOf(a) > -1 ? [attrGrp] : [])[0];

                    // Find the full attribute object corresponding to this selectedAttribute (a)
                    let attribute = attributeGroup.attributes.peek().flatMap(attr => attr.FormValue == a ? [attr] : [])[0];

                    switch (attributeGroup.logicalOperator()) {
                        // if this attributeGroup is using AND
                        case 'AND':
                            // gather the product page ids from this attribute in separate sets
                            attributeProductPageIds.push({
                                attributeGroupId: attributeGroup.attributeGroupId(),
                                productPageIds: new Set(attribute.allProductPageIds())
                            });
                            break;
                        // if this attributeGroup is using OR
                        case 'OR':
                            // if this attributeGroupId is already in the array we're putting together
                            let extant = attributeProductPageIds.filter(appid => appid.attributeGroupId == attributeGroup.attributeGroupId());

                            if (extant.length == 1) {
                                // concatenate
                                attributeProductPageIds[attributeProductPageIds.indexOf(extant[0])].productPageIds = new Set([...extant[0].productPageIds].concat(attribute.allProductPageIds()))
                            } else {
                                // if not, add to the array as normal
                                attributeProductPageIds.push({
                                    attributeGroupId: attributeGroup.attributeGroupId(),
                                    productPageIds: new Set(attribute.allProductPageIds())
                                });
                            }
                            break;
                        default:
                            break;
                    }
                });

                // The final resultant set of results is the product type set intersected with all our attribute groups
                let result = new Set(initialProductPageIds).multipleIntersect(...attributeProductPageIds.flatMap(p => p.productPageIds));

                self.attributeGroups.peek().forEach(ag => {
                    // Product Type selection affects all attribute groups
                    let productPageIdsAffectingThisAttributeGroup = [];

                    // this is the internal interaction operation (between attributes of the same group)
                    switch (ag.logicalOperator()) {
                        case 'AND':
                            // this attribute group's attributes can be affected by ALL attributes regardless of what group
                            productPageIdsAffectingThisAttributeGroup = new Set(initialProductPageIds)
                                .multipleIntersect(...attributeProductPageIds
                                    .flatMap(p => p.productPageIds)
                                );
                            break;
                        case 'OR':
                            // this attribute group's attributes cannot be affected by their siblings, only by other attribute groups' attributes
                            productPageIdsAffectingThisAttributeGroup = new Set(initialProductPageIds)
                                .multipleIntersect(...attributeProductPageIds
                                    .filter(f => f.attributeGroupId != ag.attributeGroupId())
                                    .flatMap(p => p.productPageIds)
                                );
                            break;
                        default: break;
                    }

                    // at this point we have the correct list of product page ids which are permitted to affect this attribute group
                    ag.attributes.peek().forEach(a => {
                        a.availableProductPageIds(a.ProductPageIDs.filter(ppid => productPageIdsAffectingThisAttributeGroup.has(ppid)));
                    });
                });

                if (result.size === 0) {
                    // find which attribute has 0 available product pages
                    let problemAttributes = self.attributeGroups().filter(ag => ag.selection() != '').flatMap(ag =>
                        ag.selectedAttributes().flatMap(a => a.availableProductPageIds().length == 0 ? [a] : [])
                    );
                    let deselectedAttributeNames = problemAttributes.flatMap(pa => pa.Name).join(', ');
                    window.showInfo(self.resources.Sorry, `${self.resources.ThereWereNoResultsForThatCombination} ${self.resources.WeHaveDeselectedTheFollowing}: <strong>${deselectedAttributeNames}</strong>`)
                    problemAttributes.forEach(pa => {
                        if (pa.parent.singular()) {
                            pa.parent.selection('');
                        } else {
                            pa.parent.selection.remove(s => s === pa.FormValue);
                        }
                    });
                    window.setTimeout(() => {
                        self.selectedAttributes.valueHasMutated();
                    }, 0);
                    return [];
                }

                if (self.searchable()) {
                    return [...result].sort((a, b) => {
                        return self.allProductPages().indexOf(a) - self.allProductPages().indexOf(b)
                    });
                } else {
                    return self.allProductPages().filter(pp => result.has(pp.ProductPageID));
                }
            }
        }
        return [];
    }, self).extend({
        rateLimit: {
            timeout: 50,
            method: 'notifyWhenChangesStop'
        }
    });

    // this because we want to skip the first re-render: the server has already done that render.
    self.skipProductPageRenderer = ko.observable(true);

    self.skipProductPageRenderer.subscribe(nv => {
        console.log('skipProductPageRenderer subscribe hit',)
    });

    self.productPageRenderer = ko.computed({
        read: () => {

            console.log('productPageRenderer computing');

            let page = self.pager.page();

            let pageSize = self.pager.pageSize();

            console.log('productPageRenderer page', page);
            console.log('productPageRenderer pageSize', pageSize);

            let sort = self.searchable() ? self.pager.sort.peek() : self.pager.sort();

            let results = [...self.determineAvailableProductPages()];

            // searchable grid does not want sorting client side
            if (!self.searchable()) {
                switch (sort) {
                    case "bestSellers":
                        results.sort((a, b) => {
                            if (a.SalesRank < b.SalesRank)
                                return -1;
                            if (a.SalesRank > b.SalesRank)
                                return 1;
                            if (a.MerchandisedSortOrder < b.MerchandisedSortOrder)
                                return -1;
                            if (a.MerchandisedSortOrder > b.MerchandisedSortOrder)
                                return 1;
                            return 0;
                        });
                        break;
                    case "alphaAZ":
                        results.sort((a, b) => {
                            if (a.AZSortOrder < b.AZSortOrder)
                                return -1;
                            if (a.AZSortOrder > b.AZSortOrder)
                                return 1;
                            if (a.MerchandisedSortOrder < b.MerchandisedSortOrder)
                                return -1;
                            if (a.MerchandisedSortOrder > b.MerchandisedSortOrder)
                                return 1;
                            return 0;
                        });
                        break;
                    case "alphaZA":
                        results.sort((a, b) => {
                            if (a.AZSortOrder < b.AZSortOrder)
                                return 1;
                            if (a.AZSortOrder > b.AZSortOrder)
                                return -1;
                            if (a.MerchandisedSortOrder < b.MerchandisedSortOrder)
                                return -1;
                            if (a.MerchandisedSortOrder > b.MerchandisedSortOrder)
                                return 1;
                            return 0;
                        });
                        break;
                    case "new":
                        results.sort((a, b) => {
                            if (a.NewSort < b.NewSort)
                                return -1;
                            if (a.NewSort > b.NewSort)
                                return 1;
                            if (a.MerchandisedSortOrder < b.MerchandisedSortOrder)
                                return -1;
                            if (a.MerchandisedSortOrder > b.MerchandisedSortOrder)
                                return 1;
                            return 0;
                        });
                        break;
                    case "priceHighToLow":
                        results.sort((a, b) => {
                            if (a.PriceSortOrder < b.PriceSortOrder)
                                return 1;
                            if (a.PriceSortOrder > b.PriceSortOrder)
                                return -1;
                            if (a.MerchandisedSortOrder < b.MerchandisedSortOrder)
                                return -1;
                            if (a.MerchandisedSortOrder > b.MerchandisedSortOrder)
                                return 1;
                            return 0;
                        });
                        break;
                    case "priceLowToHigh":
                        results.sort((a, b) => {
                            if (a.PriceSortOrder < b.PriceSortOrder)
                                return -1;
                            if (a.PriceSortOrder > b.PriceSortOrder)
                                return 1;
                            if (a.MerchandisedSortOrder < b.MerchandisedSortOrder)
                                return -1;
                            if (a.MerchandisedSortOrder > b.MerchandisedSortOrder)
                                return 1;
                            return 0;
                        });
                        break;
                    case "discountHighToLow":
                        results.sort((a, b) => {
                            if (a.DiscountSortOrder < b.DiscountSortOrder)
                                return 1;
                            if (a.DiscountSortOrder > b.DiscountSortOrder)
                                return -1;
                            if (a.MerchandisedSortOrder < b.MerchandisedSortOrder)
                                return -1;
                            if (a.MerchandisedSortOrder > b.MerchandisedSortOrder)
                                return 1;
                            return 0;
                        });
                        break;
                    case "discountLowToHigh":
                        results.sort((a, b) => {
                            if (a.DiscountSortOrder < b.DiscountSortOrder)
                                return -1;
                            if (a.DiscountSortOrder > b.DiscountSortOrder)
                                return 1;
                            if (a.MerchandisedSortOrder < b.MerchandisedSortOrder)
                                return -1;
                            if (a.MerchandisedSortOrder > b.MerchandisedSortOrder)
                                return 1;
                            return 0;
                        });
                        break;
                    case "stockHighToLow":
                        results.sort((a, b) => {
                            if (a.StockSortOrder < b.StockSortOrder)
                                return 1;
                            if (a.StockSortOrder > b.StockSortOrder)
                                return -1;
                            if (a.MerchandisedSortOrder < b.MerchandisedSortOrder)
                                return -1;
                            if (a.MerchandisedSortOrder > b.MerchandisedSortOrder)
                                return 1;
                            return 0;
                        });
                        break;
                    case "stockLowToHigh":
                        results.sort((a, b) => {
                            if (a.StockSortOrder < b.StockSortOrder)
                                return -1;
                            if (a.StockSortOrder > b.StockSortOrder)
                                return 1;
                            if (a.MerchandisedSortOrder < b.MerchandisedSortOrder)
                                return -1;
                            if (a.MerchandisedSortOrder > b.MerchandisedSortOrder)
                                return 1;
                            return 0;
                        });
                        break;
                    default:
                        results.sort((a, b) => {
                            if (a.MerchandisedSortOrder < b.MerchandisedSortOrder)
                                return -1;
                            if (a.MerchandisedSortOrder > b.MerchandisedSortOrder)
                                return 1;
                            return 0;
                        });
                        break;
                }
            }

            self.filteredContent(results);

            results = results.slice((page - 1) * pageSize, ((page - 1) * pageSize) + pageSize);

            // fortunately, the first time this computed evaluates the pager's ajax is always false
            // this works in my favour because I need to skip the first call to render a page, as it has
            // already been rendered by Razor
            if (self.pager.ajax()) {
                if (results.length) {
                    if (self.skipProductPageRenderer()) {
                        self.skipProductPageRenderer(false);
                    } else {
                        self.pager.renderPage(results, (renderedResults) => {
                            self.visibleProductPages(renderedResults);
                        });
                    }
                }
            } else {
                if (self.pager.skipRender()) {
                    self.pager.skipRender(false);
                } else {
                    if (self.skipProductPageRenderer()) {
                        self.skipProductPageRenderer(false);
                    } else {
                        self.visibleProductPages(results);
                    }
                }
            }

            self.updateUrl();

            return results;

        }
    }, self).extend({
        rateLimit: {
            method: 'notifyWhenChangesStop',
            timeout: 50
        }
    });

    self.lazyLoad = ko.computed(() => {
        let n = self.lazyImageNonce();
        document.querySelectorAll('img[loading="lazy"]:not(.lazy-loaded)').forEach(img => {
            if (img.complete && img.naturalHeight !== 0) {
                img.classList.add('lazy-loaded')
            } else {
                img.addEventListener('load', (e) => {
                    img.classList.add('lazy-loaded');
                });
            }
        });
    }, self).extend({
        rateLimit: {
            timeout: 50,
            method: 'notifyWhenChangesStop'
        }
    });

    //#endregion

    //#region Methods

    self.createProductPagePod = () => productPagePodViewModel();

    self.clearFilters = () => {
        self.pager.busy(true);
        setTimeout(() => { // is this really necessary?
            self.attributeGroups().forEach(ag => {
                ag.selection([]);
            });
            // I think we don't want to include Product Type as a clearable filter. 25/08/2022
            //self.productType('all');
            let offcanvas = bootstrap.Offcanvas.getOrCreateInstance(self.offcanvas.DOMElement);
            offcanvas.hide();
            self.pager.busy(false);
        }, 0);
    }

    self.lazyImages = () => {
        self.lazyImageNonce.valueHasMutated();
    }

    self.productTypeDisabler = (option, item) => {
        ko.applyBindingsToNode(option, { disable: item.disabled }, item);
    }

    self.allProductPagesInitialised = (changes) => {
        self.allProductPagesReady(true);
        return true;
    }

    self.offcanvasTouchStart = (event) => {
        self.offcanvasTouchDeltaX = event.touches[0].clientX;
    }

    self.offcanvasTouchMove = (event) => {
        self.offcanvasTouchDeltaX = event.touches && event.touches.length > 1 ? 0 : event.touches[0].clientX - self.offcanvasTouchDeltaX;

        //console.trace(self.offcanvasTouchDeltaX);
    }

    self.offcanvasTouchEnd = (event) => {
        self.offcanvasTouchDeltaX = event.changedTouches[0].clientX - self.offcanvasTouchDeltaX;

        let absDeltaX = Math.abs(self.offcanvasTouchDeltaX);

        if (absDeltaX <= self.offcanvasSwipeThreshold)
            return;

        const direction = absDeltaX / self.offcanvasTouchDeltaX;

        if (!direction)
            return;

        if (direction < 0) {
            let offcanvas = bootstrap.Offcanvas.getOrCreateInstance(self.offcanvas.DOMElement);
            offcanvas.hide();
        }
    }

    //#endregion

    return self;
});