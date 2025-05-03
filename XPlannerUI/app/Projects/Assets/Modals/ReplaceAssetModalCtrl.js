xPlanner.controller('ReplaceAssetModalCtrl', ['$scope', 'AuthService', 'localStorageService', 'local',
    'WebApiService', '$mdDialog', 'toastr', 'AssetsService', 'GridService', '$timeout', '$mdStepper', 'CostFieldListReplace',
    'KendoGridService', 'ProgressService',
    function ($scope, AuthService, localStorageService, local, WebApiService, $mdDialog, toastr, AssetsService, GridService,
        $timeout, $mdStepper, CostFieldListReplace, KendoGridService, ProgressService) {

        // Verify if is authenticate
        if (!AuthService.isAuthenticated()) {
            $state.go('login');
            return;
        }

        var allAssets;
        
        $scope.wizardData = {
            selected_costField: 'default',
            gridHeight: window.innerHeight - 230,
            selected_Catalog: null
        };

        $scope.edit = function (e) {
            if ($scope.wizardData.selected_costField == '') {
                Object.keys(e.container.context.classList).forEach(function (key) {
                    if (e.container.context.classList[key] == 'unit-budget') {
                        toastr.info('When "keep current one" is selected is not possible to edit the unit budget value');
                        e.sender.closeCell();
                    }
                });
            }
        }

        $scope.costField = CostFieldListReplace;
        WebApiService.genericController.get({
            controller: 'projects', action: 'CostField', domain_id: AuthService.getLoggedDomain(),
            project_id: local.params.project_id
        }, function (data) {
            $scope.default_cost_field = data.cost_field;
        });

        if (local.items.length == 1) {
            $scope.wizardData.default_unit_budget = local.items[0].unit_budget;
        }
        var default_resp = local.items[0].resp;
        if (local.items.find(function (asset) { return asset.resp != default_resp })) {
            default_resp = null;
        }

        $scope.replacedAssets = local.items;
        $scope.replaceWith = null;


        $scope.catalogOptionChanged = function () {
            ProgressService.blockScreen();
            WebApiService.genericController.query(AssetsService.GetReplaceParams($scope.wizardData.selected_Catalog),
                function (data) {
                    var assets = data.filter((asset) => asset.domain_id == $scope.wizardData.selected_Catalog.domain_id);                    
                    $scope.wizardData.assetsGrid.dataSource.data(assets.map(function (i) { i.default_resp = default_resp || i.default_resp; return i; }));
                    AssetsService.SetColumns($scope.wizardData);
                    ProgressService.unblockScreen();
                }, function () {
                    ProgressService.unblockScreen();
                    toastr.error('Error to retrieve data from server, please contact the technical support.');
                });
        },

        //if (local.items.resp) local.items.resp = local.items.resp.replace(/[^A-Z](BR)*/g, "");

            $scope.wizardData.options = AssetsService.GetGridReplaceConfiguration(local.isTemplate, $scope.wizardData.gridHeight);

        var steppers;
        $timeout(function () {
            steppers = $mdStepper('stepper-replace-asset');
            $scope.currentStep = 1;
        }, false);

        /* Set Catalog Options */
        var loggedDomain = AuthService.getLoggedDomainFull();
        $scope.catalogOptions = [];
        $scope.catalogOptions = loggedDomain.show_audax_info ? [{ name: 'Audaxware', domain_id: 1, status: 'A' }] : [];
        $scope.wizardData.selected_Catalog = $scope.catalogOptions[0];
        if (loggedDomain.domain_id != 1) {
            $scope.catalogOptions.push({ name: loggedDomain.name.charAt(0).toUpperCase() + loggedDomain.name.slice(1), domain_id: loggedDomain.domain_id, status: 'A' });
        }

        /* Set Project Options */
        $scope.favProjectOptions = [];
        $scope.projectOptions = [];
        var allProjects = [];

        WebApiService.genericController.query({ controller: 'Projects', action: 'All', domain_id: loggedDomain.domain_id },
            function (data) {
                data = data.sort(function (a, b) {
                    return (a.project_description > b.project_description ? 1 : -1);
                });

                allProjects = allProjects.concat(data.map(function (item) {
                    item.name = 'Project: ' + item.project_description;
                    item.status = item.status;
                    return item;
                }));

                allProjects = allProjects.filter(function (el) {
                    return (el != null && el != undefined);
                });

                allProjects.forEach(function (item) {
                    if (item.user_project_mine.length > 0) {
                        for (var i = 0; i < item.user_project_mine.length; i++) {
                            if (item.user_project_mine[i].userId === loggedDomain.user_id) {
                                $scope.favProjectOptions.push(item);
                                return;
                            }
                        }
                    }
                    $scope.projectOptions.push(item);
                });

            }, function () {
                toastr.error('Error to retrieve data from server, please contact the technical support.');
            }
        );

        /* Grid configuration */
        $scope.$on("kendoWidgetCreated", function (event, widget) {
            if (widget === $scope.wizardData.assetsGrid) {

                var assets = localStorageService.get('assets');
                $scope.wizardData.searchBoxValue = null;

                if (!assets) {
                    AssetsService.GetToGrid("All").then(function (data) {
                        localStorageService.set('assets', data);
                        $scope.wizardData.assetsGrid.dataSource.data(data.map(function (i) { i.default_resp = default_resp || i.default_resp; return i; }));
                    }, function () {
                        toastr.error("Error to retrieve assets from server, please contact technical support");
                    });
                } else {
                    $scope.wizardData.assetsGrid.dataSource.data(assets.map(function (i) { i.default_resp = default_resp || i.default_resp; return i; }));
                }
            }
        });
        /* END - Grid configuration */

        $scope.dataBound = GridService.dataBound;
        $scope.collapseExpand = GridService.collapseExpand;

        $scope.next = function () {

            steppers.clearError();
            if ($scope.currentStep == 1) {
                if (!$scope.wizardData.assetsGrid.select().length) {
                    steppers.error('Please select the target asset. The assets you selected in the inventory will be replaced by the selected target.');
                    return;
                }

                $scope.replaceWith = $scope.wizardData.assetsGrid.dataItem($scope.wizardData.assetsGrid.select());
            }

            if ($scope.currentStep == 2) {
                replace();
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

        /* BEGIN - Clear all filter*/
        $scope.clearAllFilters = function () {
            $scope.wizardData.searchBoxValue = null;
            KendoGridService.ClearFilters($scope.wizardData.assetsGrid);
            $scope.search();
        };
        /* END - Clear all filters */

        /* Function which replaces a asset */
        function replace() {

            $scope.replaceWith.inventories_id = [];
            local.items.forEach(function (asset) {
                $scope.replaceWith.inventories_id = $scope.replaceWith.inventories_id.concat(String(asset.inventory_id).split(","));
            });

            $scope.replaceWith.new_asset_id = $scope.replaceWith.asset_id;
            $scope.replaceWith.new_asset_domain_id = !$scope.wizardData.selected_Catalog.project_id ? $scope.replaceWith.domain_id : $scope.replaceWith.asset_domain_id;
            $scope.replaceWith.budget = $scope.replaceWith.unit_budget || ($scope.wizardData.selected_costField == '' ? $scope.wizardData.default_unit_budget : $scope.replaceWith[$scope.wizardData.selected_costField === 'default' ? $scope.default_cost_field : $scope.wizardData.selected_costField]) || 0;
            //$scope.replaceWith.budget = $scope.replaceWith.unit_budget;
            $scope.replaceWith.resp = $scope.replaceWith.default_resp;
            $scope.replaceWith.cost_col = 'default';

            WebApiService.replace_inventory.update(
                { domain_id: AuthService.getLoggedDomain(), project_id: local.params.project_id },
                $scope.replaceWith,
                function () {
                    toastr.info('Asset replaced!');
                    $mdDialog.hide();
                },
                function (error) {
                    if (error.status == 400) {
                        toastr.error(error.data);
                    } else {
                        toastr.error('Error to replace asset, please contact technical support');
                    }
                });
        }

        /* Function which closes the modal */
        $scope.back = function () {
            $scope.replaceWith = null;
            steppers.clearError();
            steppers.back();
            $scope.currentStep--;
        };

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

    }]);