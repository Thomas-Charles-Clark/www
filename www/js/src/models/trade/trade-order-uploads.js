const tradeOrderUploadsViewModel = ((resources) => {
    let self = {
        resources: resources
    };

    //#region Properties

    self.widget = ko.observable();

    self.enabled = ko.observable(true);

    //#endregion


    //#region Components

    self.uploader = {
        widget: self.widget,
        enabled: self.enabled,
        async: {
            autoUpload: false,
            saveUrl: '/api/tradeapi/UploadOrderFile',
        },
        upload: function (e) {
            e.data = { countryId: $("#countryId").val(), languageId: $('#languageId').val() };
        },
        validation: {
            allowedExtensions: ['.csv']
        }
    }

    //#endregion


    //#region Subscriptions



    //#endregion


    //#region Computeds



    //#endregion


    //#region Methods

    self.downloadExampleCSV = function () {
        fetch('/api/tradeapi/downloadexamplecsv', {
            method: 'POST',
            body: JSON.stringify(data),
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(function (response) {
            return response.blob();
        }).then(function (blob) {
            var link = document.createElement('a');
            link.href = window.URL.createObjectURL(blob);
            link.download = 'Example.csv';
            link.click();
        });
    }

    //#endregion

    return self;
});

window.tradeOrderUploadsModel = tradeOrderUploadsViewModel(getPrefetchedResources());

ko.applyBindings(window.tradeOrderUploadsModel, document.querySelector('#trade-wrapper'));