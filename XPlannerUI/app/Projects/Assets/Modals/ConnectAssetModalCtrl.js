xPlanner.controller('ConnectAssetModalCtrl', ['$scope', 'AuthService', 'local',
    'WebApiService', '$mdDialog', 'toastr', 'AssetsService', 'GridService', '$timeout', '$mdStepper',
    'KendoGridService', 'ConnectionTypeList',
    function ($scope, AuthService, local, WebApiService, $mdDialog, toastr, AssetsService, GridService,
        $timeout, $mdStepper, KendoGridService, ConnectionTypeList) {

        // Verify if is authenticate
        if (!AuthService.isAuthenticated()) {
            $state.go('login');
            return;
        }

        var allAssets;

        $scope.showAssetSourceStep = (local.items == null);

        $scope.connectionTypeList = ConnectionTypeList;

        $scope.wizardData = {
            gridHeight: window.innerHeight - 230
        };

        // FILTER BY CONNECTION TYPE

        $scope.changeConnectionType = function () {
            var items = allAssets;
            var connectionType = $scope.newConnection.connection_type.toLowerCase();

            if (connectionType === "none") {
                $scope.wizardData.assetsGrid.dataSource.data(allAssets);
                return;
            }
            console.log('--------------------------')
            console.log(items)
            var filteredItems = items.filter(item => item[connectionType] === true || item[connectionType] === '1');
            $scope.wizardData.assetsGrid.dataSource.data(filteredItems);

        };


        //THIS PAGE IS CALLED BY CONNECTIVITY AND ASSETS TAB
        if ($scope.showAssetSourceStep) {
            $scope.searchBoxInValue = null;

            $scope.wizardData.optionsIn = getGridConnectConfigurationAssetIn($scope.wizardData.gridHeight);

            AssetsService.GetNetworkAssetInToGrid(local.params.project_id).then(function (data) {
                $scope.wizardData.assetsInGrid.dataSource.data(data);
            }, function () {
                toastr.error("Error to retrieve assets from server, please contact technical support");

            });
        }
        else {
            fillTargetAssetVars(local.items[0], local.params.project_id);
            $scope.searchBoxValue = null;
            showAllAssetOutToConnect();
        }
                
        var steppers;
        $timeout(function () {
            steppers = $mdStepper('stepper-connect-asset');
            $scope.currentStep = 1;
        }, false);

   

        function fillTargetAssetVars(item, project_id) {
            $scope.assetIn = item;
            $scope.assetOut = null;
            allAssets = null;

            $scope.newConnection = {
                project_id: project_id,
                domain_id: AuthService.getLoggedDomain(),
                inventory_id_in: $scope.assetIn.inventory_id,
                inventory_id_out: null,
                connection_type: null
            };

            $scope.wizardData.options = getGridConnectConfiguration($scope.wizardData.gridHeight);

        }


        function getGridConnectConfigurationAssetIn(height) {
            var dataSource = new kendo.data.DataSource({
                pageSize: 50,
                data: [],
                schema: {
                    model: {
                        id: "inventory_id",
                        fields: {
                            phase_description: { editable: false },
                            department_description: { editable: false },
                            room_name: { editable: false },
                            room_number: { editable: false },
                            asset_code: { editable: false },
                            asset_description: { editable: false },
                            manufacturer_description: { editable: false },
                            ports: { editable: false },
                            it_connections: { editable: false }
                        }
                    }
                }
            })

            var gridOptions = {
                groupable: true, noRecords: "No assets available", reorderable: true, editable: true, columnMenu: {
                    columns: true,
                    sortable: false
                },
                selectable: 'single row',
                height: height
            };

            var columns = [
                { field: "phase_description", title: "Phase", width: 150 },
                { field: "department_description", title: "Department", width: 150 },
                { field: "room_name", title: "Room Name", width: 150 },
                { field: "room_number", title: "Room Number", width: 150 },
                { field: "asset_code", title: "Code", width: 120 },
                { field: "asset_description", title: "Description", width: 200 },
                { field: "manufacturer_description", title: "Manufacturer", width: 150 },
                { field: "ports", title: "Ports", width: 90 },
                { field: "it_connections", title: "IT Connections", width: 140 }
            ];


            var _toolbar = {
                template:
                    "<section layout=\"row\">" +
                    "<section layout=\"row\" layout-align=\"center center\" flex-offset=\"40\" flex=\"40\" style=\"margin-top: 10px;\">" +
                    "<md-input-container class=\"md-block no-md-errors-spacer\" style=\"margin: 0px;\" flex=\"90\">" +
                    "<label>Search</label>" +
                    "<input name=\"search\" ng-model=\"searchBoxInValue\" ng-enter=\"searchIn(searchBoxInValue)\">" +
                    "</md-input-container>" +
                    "<button class=\"md-icon-button md-button gray-color\" style=\"bottom: -0.5em; padding-left 0px; margin-left 0px\" ng-click=\"clearAllFiltersIn()\">" +
                    "<i class=\"material-icons\">delete_sweep</i><div class=\"md-ripple-container\"></div>" +
                    "<md-tooltip md-direction=\"bottom\">Clear all filters</md-tooltip>" +
                    "</button>" +
                    "</section>" +
                    "<section layout=\"row\" flex=\"40\" layout-align=\"end start\">" +
                    "<button class=\"md-icon-button md-button\" ng-click=\"collapseExpand(assetsInGrid)\" style=\"bottom: -18px\">" +
                    "<md-icon md-svg-icon=\"collapse_expand\"></md-icon>" +
                    "<md-tooltip md-direction=\"bottom\">Collapse/Expand All</md-tooltip>" +
                    "</button>" +
                    "</section>" +
                    "</section>"
            };

            return GridService.getStructure(dataSource, columns, _toolbar, gridOptions);
        };


        function getGridConnectConfiguration(height) {
            var dataSource = new kendo.data.DataSource({
                pageSize: 50,
                data: [],
                schema: {
                    model: {
                        id: "inventory_id",
                        fields: {
                            phase_description: { editable: false },
                            department_description: { editable: false },
                            room_name: { editable: false },
                            room_number: { editable: false },
                            asset_code: { editable: false },
                            asset_description: { editable: false },
                            connection_type: { editable: true },
                            manufacturer_description: { editable: false },
                            network_option: { editable: false },
                            bluetooth: { editable: false },
                            cat6: { editable: false },
                            displayport: { editable: false },
                            dvi: { editable: false },
                            hdmi: { editable: false },
                            ports: { editable: false },
                            wireless: { editable: false }
                        }
                    }
                }
            })

            var gridOptions = {
                groupable: true, noRecords: "No assets available", reorderable: true, editable: true, columnMenu: {
                    columns: true,
                    sortable: false
                },
                selectable: 'single row',
                height: height
            };

            var columns = [
                { field: "phase_description", title: "Phase", width: 150 },
                { field: "department_description", title: "Department", width: 150 },
                { field: "room_name", title: "Room Name", width: 150 },
                { field: "room_number", title: "Room Number", width: 150 },
                { field: "asset_code", title: "Code", width: 120 },
                { field: "asset_description", title: "Description", width: 200 },
                { field: "manufacturer_description", title: "Manufacturer", width: 150 },
                { field: "network_option", title: "Network Required", width: 150, template: "<div align=center>#: network_option ? 'Yes' : '' #</div>" },
                { field: "bluetooth", title: "Bluetooth", width: 150, template: "<div align=center>#: bluetooth ? 'Yes' : '--' #</div>" },
                { field: "cat6", title: "Cat 6", width: 100, template: "<div align=center>#: cat6 ? 'Yes' : '--' #</div>" },
                { field: "displayport", title: "Displayport", width: 150, template: "<div align=center>#: displayport ? 'Yes' : '--' #</div>" },
                { field: "dvi", title: "DVI", width: 100, template: "<div align=center>#: dvi ? 'Yes' : '--' #</div>" },
                { field: "hdmi", title: "HDMI", width: 100, template: "<div align=center>#: hdmi ? 'Yes' : '--' #</div>" },
                { field: "ports", title: "Ports", width: 100 },
                { field: "wireless", title: "Wireless", width: 150, template: "<div align=center>#: wireless ? 'Yes' : '--' #</div>" }

            ];



            var _toolbar = {
                template:
                    "<section layout=\"row\" class=\"grid-toolbar\" layout-align=\"space-between center\" style=\"margin-top: 20px\">" +
                    "<section flex=\"30\" flex-order=\"1\">" +
                    "<md-input-container class=\"md-block no-md-errors-spacer\" style=\"margin: 0px\" >" +
                    "<label>Connection Type</label>" +
                    "<md-select ng-model= \"newConnection.connection_type\" ng-change=\"changeConnectionType()\" required>" +
                    "<md-option><em>None</em></md-option>" +
                    "<md-option ng-repeat=\"connection in connectionTypeList\" value=\"{{connection.value}}\" >" +
                    "{{connection.name}}" +
                    "</md-option>" +
                    "</md-select>" +
                    "</md-input-container>" +
                    "</section>" +
                    "<section flex=\"35\" flex-order=\"2\" layout= \"row\">" +
                    "<md-input-container class=\"md-block no-md-errors-spacer\" style=\"margin: 0px;\" flex=\"90\">" +
                    "<label>Search</label>" +
                    "<input name=\"search\" ng-model=\"searchBoxValue\" ng-enter=\"search(searchBoxValue)\">" +
                    "</md-input-container>" +
                    "<button class=\"md-icon-button md-button gray-color\" style=\"bottom: -0.5em; padding-left 0px; margin-left 0px\" ng-click=\"clearAllFilters()\">" +
                    "<i class=\"material-icons\">delete_sweep</i><div class=\"md-ripple-container\"></div>" +
                    "<md-tooltip md-direction=\"bottom\">Clear all filters</md-tooltip>" +
                    "</button>" +                    
                    "</section>" +
                    "<section layout= \"row\" layout-align=\"end start\" class=\"grid-toolbar\" flex-order=\"3\">" +
                    "<button class=\"md-icon-button md-button\" ng-click=\"collapseExpand(assetsGrid)\">" +
                    "<md-icon md-svg-icon=\"collapse_expand\"></md-icon>" +
                    "<md-tooltip md-direction=\"bottom\">Collapse/Expand All</md-tooltip>" +
                    "</button>" +
                    "</section>" +
                    "</section>"          
            };

            return GridService.getStructure(dataSource, columns, _toolbar, gridOptions);
        };

        



        /* END - Grid configuration */


        $scope.dataBound = GridService.dataBound;
        $scope.collapseExpand = GridService.collapseExpand;
        $scope.removeItem = GridService.RemoveItem;
        $scope.updateItems = GridService.updateItems;

        $scope.next = function () {           
            steppers.clearError();
            if ($scope.currentStep == 1 && $scope.showAssetSourceStep) {
                if (!$scope.wizardData.assetsInGrid.select().length) {
                    steppers.error('Please select the source asset. ');
                    return;
                }

                var assetIn = $scope.wizardData.assetsInGrid.dataItem($scope.wizardData.assetsInGrid.select());

                fillTargetAssetVars(assetIn, assetIn.project_id);
                showAllAssetOutToConnect();

            }
            if (($scope.currentStep == 2 && $scope.showAssetSourceStep) || ($scope.currentStep == 1 && !$scope.showAssetSourceStep)) {
                if (!$scope.wizardData.assetsGrid.select().length) {
                    steppers.error('Please select the target asset. The assets you selected in the inventory will be connected to the selected target.');
                    return;
                }

                $scope.assetOut = $scope.wizardData.assetsGrid.dataItem($scope.wizardData.assetsGrid.select());
                $scope.assetOut.connection_type = $scope.newConnection.connection_type;

                if (!$scope.assetOut.connection_type || $scope.newConnection.connection_type.toLowerCase() === "none" ) {
                    steppers.error('Please select a connection type.');
                    return;
                }

                if ($scope.assetOut.ports && $scope.assetOut.it_connections >= $scope.assetOut.ports) {
                    steppers.error('There are no ports available for this connection.')
                    return;                
                }
            }

            if (($scope.currentStep == 3 && $scope.showAssetSourceStep) || ($scope.currentStep == 2 && !$scope.showAssetSourceStep)) {
                connect();
            } else {
                $scope.currentStep++;
                steppers.next();
            }
        }

        /* Search box */
        $scope.search = function (value) {

            var items = allAssets || $scope.wizardData.assetsGrid.dataSource.data();
            KendoGridService.UnselectAll($scope.wizardData.assetsGrid);

            if (!value) {
                $scope.wizardData.assetsGrid.dataSource.data(items);
            } else {
                value = value.toLowerCase();
                allAssets = items;
                var columns = $scope.wizardData.assetsGrid.columns;
                var filteredItems = items.filter(function (item) {
                    for (var i = 0; i < columns.length; i++) {
                        if (!columns[i].hidden && String(item[columns[i].field]).toLowerCase().indexOf(value) > -1) {
                            return true;
                        }
                    };
                });
                $scope.wizardData.assetsGrid.dataSource.data(filteredItems);
                if (filteredItems.length > 0) {
                    $scope.wizardData.assetsGrid.dataSource.page(1);
                }
            }
        };

        $scope.searchIn = function (value) {

            var items = allAssets || $scope.wizardData.assetsInGrid.dataSource.data();
            KendoGridService.UnselectAll($scope.wizardData.assetsInGrid);

            if (!value) {
                $scope.wizardData.assetsInGrid.dataSource.data(items);
            } else {
                value = value.toLowerCase();
                allAssets = items;
                var columns = $scope.wizardData.assetsInGrid.columns;
                var filteredItems = items.filter(function (item) {
                    for (var i = 0; i < columns.length; i++) {
                        if (!columns[i].hidden && String(item[columns[i].field]).toLowerCase().indexOf(value) > -1) {
                            return true;
                        }
                    };
                });
                $scope.wizardData.assetsInGrid.dataSource.data(filteredItems);
                if (filteredItems.length > 0) {
                    $scope.wizardData.assetsInGrid.dataSource.page(1);
                }
            }
        };

        /* BEGIN - Clear all filter*/
        $scope.clearAllFilters = function () {
            $scope.searchBoxValue = null;
            KendoGridService.ClearFilters($scope.wizardData.assetsGrid);
            $scope.search();
        };

        $scope.clearAllFiltersIn = function () {
            $scope.searchBoxInValue = null;
            KendoGridService.ClearFilters($scope.wizardData.assetsInGrid);
            $scope.searchIn();
        };
        /* END - Clear all filters */


        //Function to select assetOut
        function showAllAssetOutToConnect() { 
            $scope.searchBoxValue = null;

            AssetsService.GetNetworkAssetToGrid($scope.newConnection.project_id, $scope.newConnection.inventory_id_in).then(function (data) {
                $scope.wizardData.assetsGrid.dataSource.data(data.map(function (i) { i.network_option = 1; return i; }));
                allAssets = data;
                }, function () {
                    toastr.error("Error to retrieve assets from server, please contact technical support");

                })
        }

        /* Function which connect a asset */
        function connect() {
            $scope.newConnection.inventory_id_out = $scope.assetOut.inventory_id;
            $scope.newConnection.connection_type = $scope.assetOut.connection_type;

            WebApiService.genericController.save(
                { controller: "ITConnectivity", action: "Item", domain_id: AuthService.getLoggedDomain(), project_id: local.params.project_id },
                $scope.newConnection,
                function () {
                    toastr.info('Assets Connected!')
                    $mdDialog.hide();
                },
                function (error) {
                    if (error.status == 400) {
                        toastr.error(error.data);
                    } else {
                        toastr.error('Error to connect assets, please contact technical suport');
                    }
                }
            );
        }

        /* Function which closes the modal */
        $scope.back = function () {
            $scope.assetOut = null;
            steppers.clearError();
            steppers.back();
            $scope.currentStep--;
            if ($scope.showAssetSourceStep) {
                if ($scope.currentStep == 1) {
                    $scope.assetIn = null;
                }
            }
            
        };

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

    }]);