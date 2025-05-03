xPlanner.directive('awCategorySettingsGrid', ['GridService', 'WebApiService', 'AuthService', 'SettingsDropDown', '$parse',
function (GridService, WebApiService, AuthService, SettingsDropDown, $parse) {
    return {
        restrict: 'E',
        transclude: true,
        scope: {
            selectedSettings: '=?',
            data: "=",
            notEditable: '='
        },
        link: function (scope, elem, attrs, ctrl) {

            if (attrs.subcategory !== null && attrs.subcategory !== undefined)
                scope.isSubcategory = true;

            scope.data = scope.data || { use_category_settings: false };
            //scope.selectedSettings = scope.selectedSettings || {};

            var _dataSource = new kendo.data.DataSource({
                data: [
                    { description: "HVAC", configuration: scope.data.hvac || 'E' },
                    { description: "Plumbing", configuration: scope.data.plumbing || 'E' },
                    { description: "Gases", configuration: scope.data.gases || 'E' },
                    { description: "IT", configuration: scope.data.it || 'E' },
                    { description: "Electrical", configuration: scope.data.electrical || 'E' },
                    { description: "Support", configuration: scope.data.support || 'E' },
                    { description: "Physical", configuration: scope.data.physical || 'E' },
                    { description: "Environmental", configuration: scope.data.environmental || 'E' }
                ],
                schema: {
                    model: { fields: { description: { editable: false } } }
                },
                change: function (e) {
                    if (e.action && e.action === "itemchange") {
                        scope.data[e.items[0].description.toLowerCase()] = e.items[0].configuration;
                        scope.selectedSettings = scope.settingsGrid.dataSource.data();
                    }
                }
            });

            var _columns = [
                { field: "description", title: "Description", width: "55%" },
                {
                    field: "configuration", title: "Configuration", template: "#: configuration === 'E' ? 'Enabled' : configuration === 'D' ? 'Disabled' : 'Required' #", editor: SettingsDropDown, attributes: {
                        "class": "editable-cell"
                    }, width: "45%"
                }
            ];

            scope.$watch(function () {
                return scope.settingsGrid && scope.settingsGrid.dataSource ? scope.settingsGrid.dataSource.data() : null;
            }, function (newValue) {
                scope.selectedSettings = newValue;
            });

            scope.$watch(function () { return Object.getOwnPropertyNames(scope.data).length }, function (newValue, oldValue) {
                if (newValue === 0 && oldValue > 0) {
                    scope.settingsGrid.dataSource.cancelChanges();
                }
            }, true);

            scope.options = GridService.getStructure(_dataSource, _columns, null, { editable: true, height: window.innerHeight - 170, filterable: false }, false);


            function GetGenericParams(controller, action, id1, id2, id3) {
                return {
                    controller: controller,
                    action: action,
                    domain_id: id1 || AuthService.getLoggedDomain(),
                    project_id: id2,
                    phase_id: id3
                };
            };

            scope.EnableDisableAll = function (value) {
                if (value && value.toUpperCase() === 'E' || value.toUpperCase() === 'D') {
                    value = value.toUpperCase();
                    var data = scope.settingsGrid.dataSource.data();
                    if (data) {
                        for (var i = 0; i < data.length; i++) {
                            data[i].configuration = value;
                        }
                    }
                    scope.settingsGrid.dataSource.data(data);
                }
            }

            WebApiService.genericController.query(GetGenericParams("AssetCodes", "All"), function (codes) {
                scope.assetCodes = codes;
            }, function () {
                toastr.error("Error to retrieve data from server, please contact technical support");
            });

        },
        templateUrl: 'app/Directives/Elements/CategorySettingsGrid.html'
    }
}]);