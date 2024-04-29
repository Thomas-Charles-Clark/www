// thanks: https://stackoverflow.com/questions/3925153/prevent-jquery-autocomplete-options-from-closing-after-each-selection#answer-22771511
// override the autocomplete widget
$.widget("ui.autocomplete", $.ui.autocomplete, {
    _close: function (event) {
        if (event !== undefined && event.keepOpen === true) {
            //trigger new search with current value
            this.search(null, event);
            return true;
        }
        //otherwise invoke the original
        return this._super(event);
    }
});