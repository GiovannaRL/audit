xPlanner.controller('AssetOptionGridCtrl', ['$scope', 'HttpService', 'AuthService', 'GridService', 'toastr', 'WebApiService',
    'DialogService', 'ProgressService', '$stateParams', 'AudaxwareDataService', '$state',
    function ($scope, HttpService, AuthService, GridService, toastr, WebApiService, DialogService, ProgressService,
        $stateParams, AudaxwareDataService, $state) {

        /* kendo ui grid configurations*/
        var toolbar = {
            template:
                "<section layout=\"row\" ng-cloack ng-if=\"!asset.related_asset\">" +
                    "<section layout=\"row\" layout-align=\"start center\" class=\"gray-color\">" +
                        "<button class=\"md-icon-button md-button\" ng-click=\"openAddEditModal()\"><i class=\"material-icons\">add</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">Add Option</md-tooltip>" +
                        "</button>" +
                        "<button class=\"md-icon-button md-button\" ng-click=\"openAddEditModal(true)\"><i class=\"material-icons\">edit</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">Edit Option</md-tooltip>" +
                        "</button>" +
                        "<button class=\"md-icon-button md-button\" ng-click=\"delete()\"><i class=\"material-icons\">delete</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">Delete Option</md-tooltip>" +
                        "</button>" +
                    "</section>" +
                    //"<section layout=\"row\" layout-align=\"end center\" flex=\"100\">" +
                    //    "<button class=\"md-icon-button md-button\" ng-click=\"openAddEditModal()\">" +
                    //        "<md-icon class=\"material-icons md-accent\">add_circle</md-icon><div class=\"md-ripple-container\"></div>" +
                    //        "<md-tooltip md-direction=\"bottom\">Add New Option</md-tooltip>" +
                    //    "</button>" +
                    //"</section>" +
                "</section>"
        };

        var dataSource = {
            transport: {
                read: {
                    url: HttpService.asset_options("All", $stateParams.domain_id, $stateParams.asset_id),
                    headers: {
                        Authorization: "Bearer " + AuthService.getAccessToken()
                    }
                }
            },
            schema: {
                model: {
                    id: "asset_option_id"
                }
            }
        };
        var gridOptions = {
            reorderable: true,
            groupable: true,
            noRecords: "No options added",
            height: 475
        };
        var columns = [
                { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(assetOptionGrid)\" ng-checked=\"allPagesSelected(assetOptionGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, assetOptionGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, assetOptionGrid)\" ng-checked=\"isSelected(assetOptionGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
                { field: "code", title: "Code", width: 240 },
                { field: "description", title: "Description", width: 600 },
                { field: "data_type", title: "Type", template: "#= data_type == 'C' ? 'Color' : data_type == 'CO' ? 'Consumable' : data_type == 'A' ? 'Accessory' : data_type == 'D' ? 'Discount' : data_type == 'I' ? 'Installation' : data_type == 'W' ? 'Warranty' : data_type=='FI' ? 'Finish' : 'Frame' #", width: 150 },
                { field: "unit_budget", title: "Unit Price", width: 135, template: "<aw-currency value=\"#: unit_budget # \"></aw-currency>" },
                { field: "settings", title: "Settings", width: 135, attributes: { "class": "no-multilines" } }
                /*{ field: "min_cost", title: "Min", width: 100, template: "<aw-currency value=\"#: min_cost # \"></aw-currency>" },
                { field: "max_cost", title: "Max", width: 100, template: "<aw-currency value=\"#: max_cost # \"></aw-currency>" },
                { field: "avg_cost", title: "Avg", width: 100, template: "<aw-currency value=\"#: avg_cost # \"></aw-currency>" },
                { field: "last_cost", title: "Last", width: 100, template: "<aw-currency value=\"#: last_cost # \"></aw-currency>" }*/
        ];

        $scope.options = GridService.getStructure(dataSource, columns, toolbar, gridOptions);

        function setDbClick(grid) {
            if (grid) {
                grid.tbody.find("tr").dblclick(function () {
                    if (!$scope.asset.related_asset) {
                        var asset_option = grid.dataItem(this);
                        $scope.openAddEditModal(true, asset_option);
                    }
                });
            }
        };

        $scope.dataBound = function () {
            setDbClick($scope.assetOptionGrid);
            GridService.dataBound($scope.assetOptionGrid);
        };

        $scope.$on("kendoWidgetCreated", function (event, widget) {
            if (widget === $scope.assetOptionGrid) {
                setDbClick($scope.assetOptionGrid);
            }
        });
        /* END - kendo ui grid configurations*/

        /* Select the grid's rows */
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = GridService.select;
        $scope.allPagesSelected = GridService.allPagesSelected;
        /* END - Select the grid's rows */

        function _openOptionModal(asset, edit, item, duplicated) {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            var method = edit ? 'update' : 'save';
            var gridItems = $scope.assetOptionGrid.dataSource.data();

            DialogService.openModal('app/Assets/Modals/AddAssetOptionColor.html', 'AddAssetOptionColorCtrl', { option: item, params: $stateParams },
                    true).then(function (assetOption) {

                        duplicated ? $state.go('assetsWorkspace.assetsDetails', asset) : $scope.assetOptionGrid.dataSource.read();

                    }, function () {
                        $state.go('assetsWorkspace.assetsDetails', asset);
                    });
        }

        /* Open the add cost center modal*/
        $scope.openAddEditModal = function (edit, item) {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            if (edit) {
                if (!item) {
                    if (!GridService.verifySelected('edit', 'option', $scope.assetOptionGrid, true)) return;
                    item = GridService.getSelecteds($scope.assetOptionGrid)[0];
                }
            }

            AudaxwareDataService.CheckDuplicateAsset($scope.asset, edit).then(function (data) {
                if (data) {
                    $scope.saveAux($scope.asset, true).then(function (newAsset) {
                        edit ? $state.go('assetsWorkspace.assetsDetails', newAsset) : _openOptionModal(newAsset, edit, item, true);
                    });
                } else {
                    _openOptionModal($scope.asset, edit, item);
                }
            });
        };
        /* END - Open the add cost center modal*/

        /* Delete selected options */
        $scope.delete = function () {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            if (GridService.verifySelected('delete', 'option', $scope.assetOptionGrid)) {
                AudaxwareDataService.CheckDuplicateAsset($scope.asset, true, 'delete').then(function (data) {
                    if (data)
                        $scope.saveAux($scope.asset, true).then(function (newAsset) {
                            $state.go('assetsWorkspace.assetsDetails', newAsset);
                        });
                    else
                        DialogService.Confirm('Are you sure?', 'The option(s) will be deleted permanently').then(function () {
                            ProgressService.blockScreen();
                            GridService.deleteItems(WebApiService.asset_option,
                                function (item) {
                                    return { action: "Item", asset_option_id: item.asset_option_id, domain_id: item.domain_id, asset_id: item.asset_id };
                                },
                                $scope.assetOptionGrid).then(function (items) {
                                    ProgressService.unblockScreen();
                                    toastr.success('Option(s) Deleted');
                                }, function (error) {
                                    error.status === 409 ? toastr.info("Some, or all, of the options could not be deleted -- there is assigned asset") :
                                    toastr.error('Error to delete option(s), please contact the technical support');
                                    ProgressService.unblockScreen();
                                });
                            GridService.unselectAll($scope.assetOptionGrid);
                        });
                });
            }
        };
        /* END - Delete selected options */
    }]);
