xPlanner.controller('CostCenterGridCtrl', ['$scope', 'HttpService', 'AuthService', 'GridService', 'toastr', 'WebApiService',
    'DialogService', 'ProgressService',
    function ($scope, HttpService, AuthService, GridService, toastr, WebApiService, DialogService, ProgressService) {

        $scope.isNotViewer = (AuthService.getLoggedUserType() != "3");

        /* kendo ui grid configurations*/
        var toolbar = {
            template:
                "<section layout=\"row\" ng-cloack ng-if=\"" + $scope.isNotViewer + "\">" +
                    "<section layout=\"row\" layout-align=\"start center\" class=\"gray-color\">" +
                        "<button class=\"md-icon-button md-button\" ng-click=\"openAddEditModal()\"><i class=\"material-icons\">add</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">Add New Cost Center</md-tooltip>" +
                        "</button>" +
                        "<button class=\"md-icon-button md-button\" ng-click=\"openAddEditModal(true)\"><i class=\"material-icons\">edit</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">Edit Cost Center</md-tooltip>" +
                        "</button>" +
                        "<button class=\"md-icon-button md-button\" ng-click=\"delete()\"><i class=\"material-icons\">delete</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">Delete Cost Center</md-tooltip>" +
                        "</button>" +
                    "</section>" +
                    //"<section layout=\"row\" layout-align=\"end center\" flex=\"100\">" +
                    //    "<button class=\"md-icon-button md-button\" ng-click=\"openAddEditModal()\">" +
                    //        "<md-icon class=\"material-icons md-accent\">add_circle</md-icon><div class=\"md-ripple-container\"></div>" +
                    //        "<md-tooltip md-direction=\"bottom\">Add New Cost Center</md-tooltip>" +
                    //    "</button>" +
                    //"</section>" +
                "</section>"
        };
        var dataSource = {
            transport: {
                read: {
                    url: HttpService.cost_center("All", AuthService.getLoggedDomain(), $scope.params.project_id),
                    headers: {
                        Authorization: "Bearer " + AuthService.getAccessToken()
                    }
                }
            },
            schema: {
                model: {
                    id: "id"
                }
            }
        };
        var gridOptions = {
            reorderable: true,
            groupable: true,
            noRecords: "No cost centers available",
            height: 370
        };
        var columns = [
                { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(costCenterGrid)\" ng-checked=\"allPagesSelected(costCenterGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, costCenterGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, costCenterGrid)\" ng-checked=\"isSelected(costCenterGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
                { field: "code", title: "Code", width: 200 },
                { field: "description", title: "Description", width: 650 },
                { field: "is_default", title: "Default", template: "#= is_default ?  '<span layout=\"row\" layout-align=\"center center\"><i class=\"material-icons no-button\" style=\"color: green;\">check</i></span>' : '' #", filterable: false, width: 75 }
        ];

        $scope.options = GridService.getStructure(dataSource, columns, toolbar, gridOptions);

        function setDbClick(grid) {
            if (grid) {
                grid.tbody.find("tr").dblclick(function () {
                    var costCenter = grid.dataItem(this);
                    $scope.openAddEditModal(true, costCenter);
                });
            }
        };

        $scope.dataBound = function () {
            setDbClick($scope.costCenterGrid);
            GridService.dataBound($scope.costCenterGrid);
        };

        $scope.$on("kendoWidgetCreated", function (event, widget) {
            if (widget === $scope.costCenterGrid) {
                setDbClick($scope.costCenterGrid);
            }
        });
        /* END - kendo ui grid configurations*/

        /* Select the grid's rows */
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = GridService.select;
        $scope.allPagesSelected = GridService.allPagesSelected;
        /* END - Select the grid's rows */

        /* Open the add cost center modal*/
        $scope.openAddEditModal = function (edit, item) {
            if (!validateAccess()) {
                return;
            }

            var method = 'save';
            if (edit) {

                if (!item) {
                    if (!GridService.verifySelected('edit', 'cost center', $scope.costCenterGrid, true)) return;
                }

                method = 'update';
                item = item || GridService.getSelecteds($scope.costCenterGrid)[0];
            }

            var costCenterId;
            if (method === 'update') {
                costCenterId = item.id
            }


            var gridItems = angular.copy($scope.costCenterGrid.dataSource.data());
            var params = { controller: 'costCenters', action: "Item", domain_id: AuthService.getLoggedDomain(), project_id: $scope.params.project_id, id: costCenterId };

            DialogService.openModal('app/Projects/Cost Center/Modals/AddCostCenter.html', 'AddCostCenterCtrl', { items: gridItems, params: params, costCenter: item },
                true).then(function (costCenter) {

                    params.phase_id = costCenter.updateAll ? 1 : null;


                    WebApiService.cost_center[method](params, costCenter, function (data) {

                        if (costCenter.is_default && gridItems.length > 0) {
                            $scope.costCenterGrid.dataSource.data(gridItems.map(function (i) { if (i.id == costCenter.id) i.is_default = true; else i.is_default = false; return i }));
                        }

                        if (!item) {
                            
                            GridService.addItem($scope.costCenterGrid, JSON.parse(JSON.stringify(data)));
                            toastr.success('Cost center Saved');
                        } else {
                            GridService.updateItems($scope.costCenterGrid, costCenter);
                            toastr.success('Cost center Updated');
                        }
                    }, function () {
                        toastr.error("Erro to save cost center, please contact technical support");
                    });
                });
        };
        /* END - Open the add cost center modal*/

        /* Delete selected cost centers */
        $scope.delete = function () {
            if (!validateAccess()) {
                return;
            }

            if (GridService.verifySelected('delete', 'cost center', $scope.costCenterGrid)) {
                DialogService.Confirm('Are you sure?', 'The cost center(s) will be deleted permanently').then(function () {
                    ProgressService.blockScreen();
                    GridService.deleteItems(WebApiService.cost_center,
                        function (item) { return { action: "Item", domain_id: item.domain_id, project_id: item.project_id, id: item.id }; },
                        $scope.costCenterGrid).then(function (items) {
                            ProgressService.unblockScreen();
                            toastr.success('Cost Centers Deleted');
                        }, function () {
                            toastr.error('Error to delete cost center(s), please contact the technical support');
                            ProgressService.unblockScreen();
                        });
                    GridService.unselectAll($scope.costCenterGrid);
                });
            }
        };
        /* END - Delete selected cost centers */

        function validateAccess() {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return false;
            }
            if (AuthService.getProjectStatus($scope.params.project_id) == "L") {
                DialogService.LockedProjectModal();
                return false;
            }

            return true;
        }
    }]);