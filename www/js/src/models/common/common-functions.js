window.logout = () => {
    fetch('/api/member/signout', {
        method: 'POST',
        headers: window.fetchHeaders,
        cache: 'no-store',
        credentials: 'same-origin',
        cache: 'no-store',
        credentials: 'same-origin',
        body: {}
    })
        .then(window.handleFetchResponse)
        .then((e) => {
            window.location.reload();
        }).catch((e) => {
            console.error(e);
        });
}

window.logClientSideError = (message) => {
    fetch('/api/generalapi/LogClientError', {
        method: 'POST',
        headers: window.fetchHeaders,
        cache: 'no-store',
        credentials: 'same-origin',
        body: message
    }).catch((e) => {
        console.error(e);
    });
}

window.parseBool = str => {
    var _str$toLowerCase;
    return (str === null || str === void 0 ? void 0 : (_str$toLowerCase = str.toLowerCase()) === null || _str$toLowerCase === void 0 ? void 0 : _str$toLowerCase.trim()) === "true";
};

window.parseString = str => {
    return str === null || str === void 0 ? void 0 : str.trim();
};

window.formatNumber = (value, digits, culture) => {
    var formatter = new Intl.NumberFormat(
        culture, {
        minimumFractionDigits: digits,
        maximumFractionDigits: digits
    });

    return formatter.format(value);
}

window.formatCurrency = (value, digits, culture, currencyCode) => {
    var formatter = new Intl.NumberFormat(
        culture, {
        minimumFractionDigits: digits,
        maximumFractionDigits: digits,
        style: 'currency',
        currency: currencyCode
    });

    return formatter.format(value);
}

window.copyToClipboard = (text, element) => {
    window.navigator.clipboard.writeText(text);

    element.querySelector('.copied').classList.add('show');

    window.setTimeout(() => {
        element.querySelector('.copied').classList.remove('show');
    }, 500);
}

window.handleFetchResponse = async (response, requestData) => {
    if (response.ok) {
        return await response.json();
    } else {
        let resp = {};
        try {
            resp = await response.json();
        }
        catch (e) {

        }
        throw {
            message: `${response.status} - ${response.url}`,
            exception: resp.Message,
            source: requestData
        }
    }
}

window.handleResourcesArray = (arrayResult) => {
    let resources = {};
    if (arrayResult) {
        Object.keys(arrayResult).forEach((r) => {
            resources[arrayResult[r].Name.replace(/\.$/, '')] = arrayResult[r].Value;
        });
    }
    return resources;
}

window.catchFetchErrors = (error) => {
    console.error(error);
    if (error instanceof TypeError)
        error = { message: error.message };
    showError();
    window.logClientSideError(JSON.stringify(error));
}

window.getResource = function (resources) {
    let singular = false;
    if (!(resources instanceof Array)) {
        singular = true;
        resources = [resources];
    }
    return fetch('/api/generalapi/FetchResources', {
        method: 'POST',
        headers: window.fetchHeaders,
        cache: 'no-store',
        credentials: 'same-origin',
        body: JSON.stringify({
            resources: resources,
            countryId: getCountryId(),
            languageId: getLanguageId(),
            useDate: getDate()
        })
    })
        .then(window.handleFetchResponse)
        .then((data) => {
            if (singular)
                return data[0]
            else
                return data
        })
}

window.getResources = window.fetchResources = window.getResource;

window.decodeBase64 = (b64) => b64 == null ? null : decodeURIComponent(escape(window.atob(b64)));

window.trackOutbound = function (elem) {
    let url = element.attributes['href'].value;
    let target = element.attributes['target'].value;

    var newWindow = (target == '_blank');

    var params = function () { };
    if (!newWindow) {
        params = function () {
            window.location.href = url;
        }
    }

    if (typeof gtag === 'function') {
        gtag('event', 'click', {
            'send_to': 'default',
            'event_category': 'outbound',
            'event_label': url,
            'transport_type': 'beacon',
            'event_callback': params
        });
    } else {
        window.location.href = url;
    }

    return newWindow;
}

window.lazyImages = () => {
    console.log('lazyImages');
    document.querySelectorAll('img[loading="lazy"]:not(.lazy-loaded)').forEach(img => {
        if (img.complete && img.naturalHeight !== 0) {
            img.classList.add('lazy-loaded')
        } else {
            img.addEventListener('load', (e) => {
                img.classList.add('lazy-loaded');
            });
        }
    });
}

window.addLoadFunction(window.lazyImages);

window.lazyImages();

window.keepAliveTimer = 0;

window.keepAliveFn = function (event) {

    fetch('/heartbeat/bradycardia', {
        method: 'POST',
        headers: window.fetchHeaders,
        cache: 'no-store',
        credentials: 'same-origin'
    });

    if (window.ecommerceGeneralPopupModel) {
        window.ecommerceGeneralPopupModel.cookieExpiryDate = window.addMinutes(new Date(), 60).toUTCString();
    }
};

window.keepAlive = (() => {
    clearTimeout(keepAliveTimer);
    keepAliveTimer = setInterval(window.keepAliveFn, 600000);
})();

// Thanks to: https://stackoverflow.com/questions/19305821/multiple-modals-overlay
document.addEventListener('show.bs.modal', function (event) {
    if (typeof Enumerable === 'undefined') {
        var self = event.target;

        var zIndex = 1040 + (10 * document.querySelectorAll('.modal.fade.show').length);

        var adj = 0;

        document.querySelectorAll('.modal.fade.show').forEach(function (i, ele) {
            if (+ele?.style?.zIndex >= zIndex) {
                adj++;
            }
        });

        zIndex += adj * 10;

        console.log(self.id + ' showing at', zIndex);

        self.style.zIndex = zIndex;

        window.setTimeout(function () {
            console.log(self.id + ' backdrop showing at', zIndex - 1);
            let backdropNodes = document.querySelectorAll('.modal-backdrop:not(.modal-stack)');
            let backdrops = [];
            backdropNodes.forEach(bd => {
                backdrops.push(bd);
            })
            if (backdrops.length) {
                backdrops[backdrops.length - 1].style.zIndex = zIndex - 1;
                backdrops[backdrops.length - 1].classList.add('modal-stack');
            }
        }, 0);
    } else {
        self = event.target;

        let zIndex = 1040;

        let openModals = [];

        document.querySelectorAll('.modal.fade.show').forEach(n => {
            if (n != self) {
                openModals.push(n);
            }
        })

        if (openModals.length) {
            zIndex = Enumerable.From(openModals).Select(function (x) {
                return +(x.style.zIndex);
            }).Max() + 10;
        }
        console.log(self.id + ' showing at', zIndex);

        self.style.zIndex = zIndex;

        window.setTimeout(function () {
            console.log(self.id + ' backdrop showing at', zIndex - 1);
            let backdropNodes = document.querySelectorAll('.modal-backdrop:not(.modal-stack)');
            let backdrops = [];
            backdropNodes.forEach(bd => {
                backdrops.push(bd);
            })
            if (backdrops.length) {
                backdrops[backdrops.length - 1].style.zIndex = zIndex - 1;
                backdrops[backdrops.length - 1].classList.add('modal-stack');
            }
        }, 0);
    }
});

document.addEventListener('hidden.bs.modal', function (event) {
    document.querySelectorAll('.modal.show').length && document.body.classList.add('modal-open');
});

// Cheers: https://stackoverflow.com/questions/64564266/check-if-a-piece-of-the-element-is-in-viewport#answer-64565158
window.elementIntersection = (r1, r2) => {
    const xOverlap = Math.max(0, Math.min(r1.x + r1.w, r2.x + r2.w) - Math.max(r1.x, r2.x));
    const yOverlap = Math.max(0, Math.min(r1.y + r1.h, r2.y + r2.h) - Math.max(r1.y, r2.y));
    const overlapArea = xOverlap * yOverlap;
    return overlapArea;
}

window.elementPercentInViewport = (div) => {
    const rect = div.getBoundingClientRect();
    const dimension = { x: rect.x, y: rect.y, w: rect.width, h: rect.height };
    const viewport = { x: 0, y: 0, w: window.innerWidth, h: window.innerHeight };
    const divsize = dimension.w * dimension.h;
    const overlap = window.elementIntersection(dimension, viewport);
    return overlap / divsize;
}

window.addDays = (date, days) => {
    var result = new Date(date);
    result.setDate(result.getDate() + days);
    return result;
}

window.addMinutes = (date, minutes) => {
    return new Date(date.getTime() + minutes * 60000);
}