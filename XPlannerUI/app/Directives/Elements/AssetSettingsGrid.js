xPlanner.directive('awAssetSettingsGrid', ['GridService', 'SettingsDropDown', 'AuthService', 'HttpService', 'ProgressService',
    'ChbxAssetSettingEditor', 'OptionsDropDown', 'HertzDropDown', 'NumericEditor', 'TextEditor', 'CategoryDropDown', 'toastr',
    'PhasesDropDown','WebApiService', 'ConnectionTypeDropDown', 'PlugTypeDropDown', 'NetworkTypeDropDown',
function (GridService, SettingsDropDown, AuthService, HttpService, ProgressService, ChbxAssetSettingEditor, OptionsDropDown,
    HertzDropDown, NumericEditor, TextEditor, CategoryDropDown, toastr, PhasesDropDown, WebApiService, ConnectionTypeDropDown, PlugTypeDropDown, NetworkDropDown) {
    return {
        restrict: 'E',
        transclude: true,
        scope: {
            asset: '=',
            requiredSettings: '=',
            updatedJsnData: '=',
            origin: '@'
        },
        link: function (scope, elem, attrs, ctrl) {

            if (!scope.asset)
                return;

            var hasData;
            var first = true;
            var itemRow = null;

            if (scope.asset.consolidated_view == 1 && scope.asset.inventory_ids.length > 1) {
                scope.asset.inventory_id = parseInt(scope.asset.inventory_ids.substring(0, 7));
            };

            function _getValueTemplate(item) {
                item.value = item.value || '';
                switch (item.editor_type) {
                    case 'checkbox':
                        return '<div align=center><md-checkbox style=\"margin-top: 0px; margin-bottom: 0px; margin-left: 27px;\" class=\"checkbox\" ng-checked=\"' + (item.value === 'True' || item.value == true) + '\" aria-label=\"checkbox\"></md-checkbox></div>'
                        break;
                    case 'dropDown':
                        return '<div align=center>' + (item.asset_field === 'hertz' || item.asset_field === 'plug_type' || item.asset_field === 'connection_type' || item.asset_field === 'network_type' || item.asset_field === 'phases' ? (item.value === '' ? '--' : item.value)
                            : item.asset_field === 'category_attribute' ? (item.value === 'F' ? 'Fixed' : item.value === 'MJ' ? 'Major Moveable' : item.value === 'MN' ? 'Minor Moveable' : '') :
                            (item.value == 0 ? '--' : item.value == 1 ? 'Yes' : 'Optional')) + '</div>';
                        break;
                    default:
                        return '<div align=center>' + (item.value || '') + '</div>';
                }
            };

            function isLocked(item) {
                var ow = item + "_ow";
                return scope.asset[ow] ? false : true;
            };        

            function _getLockTemplate(item) {
                var lockIcon = isLocked(item.asset_field)? "lock":"lock_open";
                var LockToolTip = (lockIcon == "lock" ? "Unlock to customize the value" : "Lock to pick the value from the catalog");
                
                return "<section layout=\"row\" layout-align=\"center center\" class=\"grid-buttons\">"
                    + "<button class=\"md-icon-button md-button\" ng-click=\"toggleLockAttributes(dataItem)\">" +
                    "<i class=\"material-icons\">" + lockIcon + "<md-tooltip md-direction=\"bottom\">"
                    + LockToolTip + "</md-tooltip>" + "</button></section>";
            };

            function _getEditor(container, options) {
                
                var field_name = options.model.asset_field + '_ow';
                if (!scope.asset[field_name] && scope.origin) {
                    $('<div align=center>' + (options.model.value || '') + '</div>')
                        .appendTo(container); 
                }
                else {
                    switch (options.model.editor_type) {
                        case 'checkbox':
                            ChbxAssetSettingEditor(container, options, scope.asset, 'asset[\'' + options.model.asset_field + '\']', 25);
                            break;
                        case 'dropDown':
                            options.model.asset_field === 'hertz' ? HertzDropDown(container, options) :
                            options.model.asset_field === 'category_attribute' ? CategoryDropDown(container, options) :
                            options.model.asset_field === 'phases' ? PhasesDropDown(container, options) :
                            options.model.asset_field === 'connection_type' ? ConnectionTypeDropDown(container, options) :
                            options.model.asset_field === 'plug_type' ? PlugTypeDropDown(container, options) :
                            options.model.asset_field === 'network_type' ? NetworkDropDown(container, options) :
                            OptionsDropDown(container, options);
                            break;
                        case 'numeric':
                            NumericEditor(container, options);
                            break;
                        case 'regularExpression':
                            TextEditor(container, options, options.model.asset_field);
                            break;
                        default:
                            TextEditor(container, options);
                    }
                }
            };

            var _toolbar = {
                template:
                        '<section layout="row" layout-align="end start">' +
                            '<button class="md-icon-button md-button" ng-click="collapseExpand(assetSettingsGrid)">' +
                                '<md-icon md-svg-icon="collapse_expand"></md-icon>' +
                                '<md-tooltip md-direction="bottom">Collapse/Expand All</md-tooltip>' +
                            '</button>' +
                        '</section>'
            };

            scope.$watch(function () { return scope.updatedJsnData }, function (newValue, oldValue) {
                if (scope.asset && newValue && newValue != oldValue) {
                    scope.assetSettingsGrid.dataSource.transport.options.read.url = HttpService.generic('JSN', 'JSNUtilities', newValue.domain_id, newValue.id, scope.asset.domain_id, scope.asset.asset_id);
                    scope.assetSettingsGrid.dataSource.read();
                }            });

            scope.toggleLockAttributes = function (item) {

                itemRow = item;
                var field = item.asset_field;
                var ow = field + "_ow";
                scope.asset[ow] = !scope.asset[ow];

                if (!scope.asset[ow]) {
                    ProgressService.blockScreen();
                    WebApiService.genericController.get({ controller: "assets", action: "SummarizedSingle", domain_id: scope.asset.asset_domain_id, project_id: scope.asset.asset_id },
                        function (assetData) {
                            for (var i = 0; i < scope.assetSettingsGrid.dataSource._data.length; i++) {
                                if (scope.assetSettingsGrid.dataSource._data[i].asset_field == item.asset_field) {
                                    scope.assetSettingsGrid.dataSource._data[i].value = assetData[field];

                                }

                            }
                            ProgressService.unblockScreen();
                            GridService.updateItems(scope.assetSettingsGrid);

                            
                            

                        },
                        function () {
                            toastr.error("Error to retrieve asset data");
                            ProgressService.unblockScreen();                   
                        }
                    );
                                        
                }
                else {
                    GridService.updateItems(scope.assetSettingsGrid);
                }

            };
            


            scope.$watch(function () {
                return scope.asset;
            }, function (newValue) {
                if (first && newValue.domain_id) {
                    first = false;
                    ProgressService.blockScreen();

                    var _dataSource = new kendo.data.DataSource({
                        //pageSize: 10,
                        transport: {
                            read: {
                                url: HttpService.generic('assetSettings', (scope.origin ? 'InventorySettings' : 'Settings') , newValue.domain_id, scope.asset.inventory_id || newValue.asset_id, (scope.origin ? scope.origin : null)),
                                headers: {
                                    Authorization: 'Bearer ' + AuthService.getAccessToken()
                                }
                            },
                        },
                        sort: { field: 'property_name', dir: 'asc' },
                        schema: {
                            model: {
                                fields: {
                                    group_name: { type: 'string', editable: false },
                                    property_name: { type: 'string', editable: false },
                                    asset_field: { type: 'string' },
                                    required: { type: 'boolean', editable: false },
                                    lock: { type: 'boolean', editable: false },
                                    value: {
                                        validation: {
                                            valuevalidation: function (input) {
                                                if ((input.is("[name='depth']") || input.is("[name='mounting_height']") || input.is("[name='weight']") || input.is("[name='width']") || input.is("[name='height']") || input.is("[name='volts']")) && input.val() != '') {
                                                    input.attr('data-valuevalidation-msg', "Only the 'Number-Number' and 'Number/Number' formats are allowed");
                                                    return /^[0-9]+(\.[0-9]{1,3})?((\-|\/)[0-9]+(\.[0-9]{1,3})?){0,1}$/.test(input.val());
                                                }
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        error: function () {
                            ProgressService.unblockScreen();
                            toastr.error('Error to retrieve data from server, please contact the technical support');
                        },
                        change: function (e) {
                            ProgressService.unblockScreen();
                            if (e.action && e.action === 'itemchange') {
                                scope.asset[e.items[e.items.length - 1].asset_field] = e.items[e.items.length - 1].value;
                            }
                        },
                        group: [
                           { field: 'group_name' }
                        ]
                    });

                    var _columns = [
                        { field: 'group_name', title: 'Group', width: 250 },
                        { field: 'property_name', title: 'Setting', width: 250 },
                        { field: 'value', title: 'Value', filterable: false, editable: false, attributes: { "class": 'editable-cell' } , template: _getValueTemplate, editor: _getEditor, width: 150 },
                        { field: 'required', title: 'Required', template: "#= required ? '<span layout=\"row\" layout-align=\"center center\"><i class=\"material-icons no-button\" style=\"color: green;\">check</i></span>' : '' #", width: 100 },
                        { field: 'lock', title: '  ', editable: false, filterable: false, template: _getLockTemplate, width: 100 }
                    ];

                    // Remove columns
                    if (scope.origin) {
                        var removed = _columns.splice(3, 1);
                    } else {
                        var removed = _columns.splice(4, 1);
                    };

                    //var _pageable = {
                    //    pageSizes: [5, 10, 20, 30],
                    //    pageSizeDefault: 10
                    //}
                    scope.options = GridService.getStructure(_dataSource, _columns, _toolbar, { editable: true, noRecords: 'This asset has no settings to be set' }, false);
                }
            });

            scope.dataBound = function () {
                if (!hasData) {
                    hasData = true;
                    scope.assetSettingsGrid.dataSource.data().map(function (i) {
                        if (i.required) {
                            scope.requiredSettings.push(i.asset_field);
                        }
                    });
                }
                                                
                GridService.dataBound(scope.assetSettingsGrid);

                if (scope.origin != undefined && itemRow != null) {
                    GridService.expandAll(scope.assetSettingsGrid);
                }

            };

            scope.collapseExpand = GridService.collapseExpand;

        },
        templateUrl: 'app/Directives/Elements/AssetSettingsGrid.html'
    }
}]);
