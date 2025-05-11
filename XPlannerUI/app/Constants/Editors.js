xPlanner.constant('SettingsDropDown', function (container, options) {

    $('<input data-text-field="name" data-value-field="value" data-bind="value:' + options.field + '"/>')
                .appendTo(container)
                .kendoDropDownList({
                    dataSource: {
                        data: [{ name: "Enable", value: 'E' }, { name: 'Disable', value: 'D' }, { name: "Required", value: "R" }]
                    }
                });
});

xPlanner.constant('CheckboxEditor', function (container, options) {

    options.model.dirty = true;

    options.model[options.field] = typeof (options.model[options.field]) === 'string' ? options.model[options.field].toLowerCase() !== 'true' : !options.model[options.field];
    $('<section layout="column" layout-align="none center"><md-checkbox style="margin-top: 0px; margin-bottom: 0px; class="checkbox" ng-model="dataItem[\'' + options.field + '\']" aria-label="checkbox"></md-checkbox></section>').appendTo(container);

});

xPlanner.constant('ChbxAssetSettingEditor', function (container, options, item, ngModelString, marginLeft) {

    options.model.dirty = true;

    options.model[options.field] = item[options.model.asset_field] = typeof (options.model[options.field]) === 'string' ? options.model[options.field].toLowerCase() !== 'true' : !options.model[options.field];
    $('<section layout="column" layout-align="none center"><md-checkbox style="margin-top: 0px; margin-bottom: 0px;' + (marginLeft ? 'margin-left: ' + marginLeft + 'px;' : '') + '" class="checkbox" ng-model="dataItem[\'' + options.field + '\']" ng-change=\"' + ngModelString + ' = dataItem[\' + ' + options.field + '\']\" aria-label="checkbox"></md-checkbox></section>').appendTo(container);

});

xPlanner.constant('OptionsDropDown', function (container, options) {

    $('<input data-text-field="name" data-value-field="value" data-bind="value:' + options.field + '"/>')
                .appendTo(container)
                .kendoDropDownList({
                    dataSource: {
                        data: [{ name: "--", value: 0 }, { name: 'Yes', value: 1 }, { name: "Optional", value: 2 }]
                    }
                });
});

xPlanner.constant('HertzDropDown', function (container, options) {

    $('<input data-text-field="name" data-value-field="value" data-bind="value:' + options.field + '"/>')
                .appendTo(container)
                .kendoDropDownList({
                    dataSource: {
                        data: [{ name: "--", value: "" }, { name: '50', value: '50' }, { name: "50/60", value: '50/60' }, { name: "60", value: '60' }]
                    }
                });
});

xPlanner.constant('ConnectionTypeDropDown', function (container, options) {

    $('<input data-text-field="name" data-value-field="value" data-bind="value:' + options.field + '"/>')
        .appendTo(container)
        .kendoDropDownList({
            dataSource: {
                data: [{ name: "--", value: "" }, { name: 'Chemtron', value: 'Chemtron' }, { name: "DISS", value: 'DISS' }, { name: "Ohmeda", value: 'Ohmeda' }, { name: "Puritan Bennett", value: 'Puritan Bennett' }]
            }
        });
});


xPlanner.constant('PlugTypeDropDown', function (container, options) {

    $('<input data-text-field="name" data-value-field="value" data-bind="value:' + options.field + '"/>')
        .appendTo(container)
        .kendoDropDownList({
            dataSource: {
                data: [{ name: "--", value: "" }, { name: "NEMA 5-15P", value: 'NEMA 5-15P' }, { name: "NEMA 5-20P", value: 'NEMA 5-20P' }, { name: "NEMA 6-15P", value: 'NEMA 6-15P' }, { name: "NEMA 6-20P", value: 'NEMA 6-20P' }, { name: "NEMA 6-30LP", value: 'NEMA 6-30LP' }, { name: "NEMA 6-50P", value: 'NEMA 6-50P' }]
            }
        });
});

xPlanner.constant('NetworkTypeDropDown', function (container, options) {

    $('<input data-text-field="name" data-value-field="value" data-bind="value:' + options.field + '"/>')
        .appendTo(container)
        .kendoDropDownList({
            dataSource: {
                data: [{ name: "--", value: "" }, { name: "Cable", value: 'Cable' }, { name: "Wi-Fi", value: 'Wi-Fi' }, { name: "Cable & Wi-Fi", value: 'Cable & Wi-Fi' }]
            }
        });
});





xPlanner.constant('PhasesDropDown', function (container, options) {

    $('<input data-text-field="name" data-value-field="value" data-bind="value:' + options.field + '"/>')
                .appendTo(container)
                .kendoDropDownList({
                    dataSource: {
                        data: [{ name: "--", value: "" }, { name: '1', value: '1' }, { name: "2", value: '2' }, { name: "3", value: '3' }]
                    }
                });
});

xPlanner.constant('CategoryDropDown', function (container, options) {
    $('<input data-text-field="name" data-value-field="value" data-bind="value:' + options.field + '"/>')
                .appendTo(container)
                .kendoDropDownList({
                    dataSource: {
                        data: [{ name: "Fixed", value: "F" }, { name: 'Major Moveable', value: 'MJ' }, { name: "Minor Moveable", value: 'MN' }, {name: '', value: ""}]
                    }
                });
});


xPlanner.constant('NumericEditor', function (container, options, format) { // only positive values

    $('<input data-bind="value:' + options.field + '"/>')
                 .appendTo(container)
                 .kendoNumericTextBox({
                     format: format || "n2",
                     min: 0
                 });
});


xPlanner.constant('TextEditor', function (container, options, name) {

    $('<input class="k-input k-textbox"' + (name ? ' name="' + name + '"' : '') + ' data-bind="value:' + options.field + '"/>')
                 .appendTo(container);
});