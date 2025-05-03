xPlanner.controller('AssetVendorGridCtrl', ['$scope', 'AuthService', 'HttpService', '$stateParams', 'toastr', 'GridService',
        'DialogService', 'AudaxwareDataService', '$state', 'ProgressService', 'WebApiService',
    function ($scope, AuthService, HttpService, $stateParams, toastr, GridService, DialogService, AudaxwareDataService,
        $state, ProgressService, WebApiService) {

        /* kendo ui grid configurations*/
        var dataSource = {
            transport: {
                read: {
                    url: HttpService.generic("assetsVendor", "All", $stateParams.domain_id, $stateParams.asset_id),
                    headers: {
                        Authorization: "Bearer " + AuthService.getAccessToken()
                    }
                }
            },
            schema: {
                model: {
                    fields: {
                        min_cost: { type: "number" }, max_cost: { type: "number" }, avg_cost: { type: "number" },
                        last_cost: { type: "number" }
                    }
                }
            },
            error: function () {
                toastr.error("Error to retrieve vendors from server, please contact the technical support");
            }
        };
        var gridOptions = {
            reorderable: true,
            noRecords: "No vendors available",
            height: 350
        };
        var columns = [
                { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(assetVendorGrid)\" ng-checked=\"allPagesSelected(assetVendorGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, assetVendorGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, assetVendorGrid)\" ng-checked=\"isSelected(assetVendorGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
                { field: "vendor.name", title: "Name", width: 230, template: '<span class="link" ng-click=\"GoToVendor(dataItem)\">#: vendor.name #</span>' },
                { field: "model_number", title: "Model No.", width: 120 },
                { field: "min_cost", title: "Min", width: 100, template: "<aw-currency value=\"#= min_cost # \"></aw-currency>" },
                { field: "max_cost", title: "Max", width: 100, template: "<aw-currency value=\"#= max_cost # \"></aw-currency>" },
                { field: "avg_cost", title: "Avg", width: 100, template: "<aw-currency value=\"#= avg_cost # \"></aw-currency>" },
                { field: "last_cost", title: "Last", width: 100, template: "<aw-currency value=\"#= last_cost # \"></aw-currency>" }
        ];

        $scope.options = GridService.getStructure(dataSource, columns, null, gridOptions);

        $scope.GoToVendor = function (vendor) {
            if (vendor) {
                $state.go('assetsWorkspace.vendorsDetails', { domain_id: vendor.vendor_domain_id, vendor_id: vendor.vendor_id });
            }
        }

        function setDbClick(grid) {
            if (grid) {
                grid.tbody.find("tr").dblclick(function () {
                    var asset_vendor = grid.dataItem(this);
                    $scope.addEdit(true, asset_vendor);
                });
            }
        };

        $scope.dataBound = function () {
            setDbClick($scope.assetVendorGrid);
            GridService.dataBound($scope.assetVendorGrid);
        };

        /* Select the grid's rows */
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = GridService.select;
        $scope.allPagesSelected = GridService.allPagesSelected;
        /* END - Select the grid's rows */

        /* END - kendo ui grid configurations*/

        function _openVendorModal(asset, duplicated, edit, item) {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            DialogService.openModal('app/Assets/Modals/AssignVendor.html', 'AssignVendorCtrl',
                            {
                                asset: asset, edit: edit, vendor: edit ? item : null
                            }, true)
                        .then(function (item) {
                            duplicated ? $scope.changeAsset(asset) : edit ? $scope.assetVendorGrid.dataSource.read() : $scope.assetVendorGrid.dataSource.add(
                                angular.extend(item, { min_cost: 0, max_cost: 0, avg_cost: 0, last_cost: null }));
                        }, function () {
                            if (duplicated)
                                $scope.changeAsset(asset);
                        });
        };

        $scope.addEdit = function (edit, item) {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            if (!edit || item || GridService.verifySelected('edit', 'vendor', $scope.assetVendorGrid, true)) {
                AudaxwareDataService.CheckDuplicateAsset($scope.asset).then(function (data) {
                    if (data) {
                        $scope.saveAux($scope.asset, true).then(function (data) {
                            _openVendorModal(data, true, edit, item || GridService.getSelecteds($scope.assetVendorGrid)[0]);
                        });
                    } else {
                        _openVendorModal($scope.asset, null, edit, item || GridService.getSelecteds($scope.assetVendorGrid)[0]);
                    }
                });
            }
        };

        $scope.delete = function () {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            if (GridService.verifySelected('delete', 'vendor', $scope.assetVendorGrid)) {
                AudaxwareDataService.CheckDuplicateAsset($scope.asset, true, 'delete').then(function (data) {
                    if (data)
                        $scope.saveAux($scope.asset, true).then(function (newAsset) {
                            $state.go('assetsWorkspace.assetsDetails', newAsset);
                        });
                    else {
                        DialogService.Confirm('Are you sure?', 'The vendor(s) will be deleted permanently').then(function () {
                            ProgressService.blockScreen();
                            GridService.deleteItems(WebApiService.genericController, function (item) {
                                return {
                                    controller: 'assetsVendor', action: 'Item', domain_id: item.asset_domain_id,
                                    project_id: item.asset_id, phase_id: item.vendor_domain_id, department_id: item.vendor_id
                                }
                            }, $scope.assetVendorGrid).then(function () {
                                ProgressService.unblockScreen();
                                toastr.success('Vendor(s) Deleted');
                            }, function (error) {
                                error.status === 409 ? toastr.info("Some, or all, of the vendors could not be deleted -- purchase orders have been issued") :
                                        toastr.error('Error to delete vendor(s), please contact the technical support');
                                ProgressService.unblockScreen();
                            });
                        });
                    }
                });
            }
        }
    }]);