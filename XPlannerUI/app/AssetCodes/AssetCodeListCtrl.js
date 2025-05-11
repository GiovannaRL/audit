xPlanner.controller('AssetCodeListCtrl', ['$scope', 'GridService', 'HttpService', 'AuthService', 'DialogService', 'ProgressService',
    'toastr', '$state',
    function ($scope, GridService, HttpService, AuthService, DialogService, ProgressService, toastr, $state) {

        if (!AuthService.isAuthenticated()) {

            AuthService.clearLocalStorage();
            $state.go('login');
            return;
        }

        /* kendo ui grid configurations*/
        var toolbar = {
            template:
                "<section layout=\"row\" ng-cloak >" +
                    "<section layout=\"row\" layout-align=\"start center\"  class=\"gray-color\">" +
                        "<button class=\"md-icon-button md-button\" ng-click=\"openAddEditModal()\"><i class=\"material-icons\">add</i><div class=\"md-ripple-container\"></div>" +
                        "<md-tooltip md-direction=\"bottom\">Add New Asset Code</md-tooltip>" +
                        "</button>" +
                        "<button class=\"md-icon-button md-button\" ng-click=\"openAddEditModal(true)\"><i class=\"material-icons\">edit</i><div class=\"md-ripple-container\"></div>" +
                        "<md-tooltip md-direction=\"bottom\">Edit Asset Code</md-tooltip>" +
                        "</button>" +
                        "<button class=\"md-icon-button md-button\" ng-click=\"delete()\"><i class=\"material-icons\">delete</i><div class=\"md-ripple-container\"></div>" +
                        "<md-tooltip md-direction=\"bottom\">Delete Asset Code</md-tooltip>" +
                        "</button>" +
                    "</section>" +
                "</section>"
        };
        var dataSource = {
            transport: {
                read: {
                    url: HttpService.generic("AssetCodes", "All", AuthService.getLoggedDomain()),
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
        var gridOptions = { groupable: true, noRecords: "No asset code available", height: 370 };

        var columns = [
            { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(codesGrid)\" ng-checked=\"allPagesSelected(codesGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, codesGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, codesGrid)\" ng-checked=\"isSelected(codesGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "5px" },
            { field: "prefix", title: "Prefix", width: "15px" },
            { field: "description", title: "Description", width: "20px" },
            { field: "next_seq", title: "Next Number", width: "10px" },
            { field: "added_by", title: "Added By", width: "10px" },
            { field: "date_added", title: "Date Added", width: "10px" },
        ];

        $scope.options = GridService.getStructure(dataSource, columns, toolbar, gridOptions);

        function setDbClick(grid) {
            if (grid) {
                grid.tbody.find("tr").dblclick(function () {
                    var assetcode = grid.dataItem(this);
                    $scope.openAddEditModal(true, assetcode);
                });
            }
        };

        $scope.dataBound = function () {
            setDbClick($scope.codesGrid);
            GridService.dataBound($scope.codesGrid);
        };

        $scope.$on("kendoWidgetCreated", function (event, widget) {
            if (widget === $scope.codesGrid) {
                setDbClick($scope.codesGrid);
            }
        });
        /* END - kendo ui grid configurations*/

        /* Select the grid's rows */
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = GridService.select;
        $scope.allPagesSelected = GridService.allPagesSelected;
        /* END - Select the grid's rows */

        /* Open the add/edit address modal*/
        $scope.openAddEditModal = function (edit, item) {
            var method = 'save';
            if (edit) {

                if (!item) {
                    if (!GridService.verifySelected('edit', 'assetcode', $scope.codesGrid, true)) return;
                }

                method = 'update';
                item = item || GridService.getSelecteds($scope.codesGrid)[0];
            }

            var params = { action: "Item", domain_id: AuthService.getLoggedDomain() };

            var gridItems = angular.copy($scope.codesGrid.dataSource.data());

            DialogService.openModal('app/AssetCodes/Modals/AddEditAssetCode.html', 'AddEditAssetCodeCtrl',
                { items: gridItems, params: params, assetcode: item }, true).then(function (assetcode) {

                    //WebApiService.project_address[method](params, assetcode, function (data) {

                        //if (address.is_default && gridItems.length > 0) {
                        //    $scope.codesGrid.dataSource.data(gridItems.map(function (i) { if (i.id == address.id) i.is_default = true; else i.is_default = false; return i }));
                        //}

                        //if (!item) {
                        //    if (gridItems.length == 0) {
                        //        data.is_default = true;
                        //    }

                        //    GridService.addItem($scope.codesGrid, JSON.parse(JSON.stringify(data)));
                        //    toastr.success('Address Saved');
                        //} else {

                        //    GridService.updateItems($scope.codesGrid, address);
                        //    toastr.success('Address Updated');
                        //}
                    //}, function () {
                    //    toastr.error("Erro to save address, please contact technical support");
                    //});
                });
        };
        /* END - Open the add/edit address modal*/

        /* Delete selected addresses */
        $scope.delete = function () {

            if (GridService.verifySelected('delete', 'assetcode', $scope.codesGrid)) {
                DialogService.Confirm('Are you sure?', 'The addresses will be deleted permanently!').then(function () {
                    ProgressService.blockScreen();
                    GridService.deleteItems(WebApiService.project_address,
                        function (item) { return { action: "Item", domain_id: item.domain_id, project_id: item.project_id, id: item.id }; },
                        $scope.codesGrid).then(function () {
                            ProgressService.unblockScreen();
                            toastr.success('Addresses Deleted!');
                        }, function (error) {
                            ProgressService.unblockScreen();

                            if (error.status === 409)
                                toastr.info('Some, or all, of the selected addresses could not be deleted because it\'s assigned to purchase orders');
                            else
                                toastr.error('Error to try delete addresses, please contact the technical support');
                        });
                    GridService.unselectAll($scope.codesGrid);
                });
            }
        };
        /* END - Delete selected addresses */

        

        $scope.options = GridService.getStructure(dataSource, columns, toolbar, gridOptions);


        function setDbClick(grid) {
            if (grid) {
                grid.tbody.find("tr").dblclick(function () {
                    var auditedData = grid.dataItem(this);
                    $scope.openAddEditModal(true, auditedData);
                });
            }
        };

        $scope.dataBound = function () {
            setDbClick($scope.codesGrid);
            GridService.dataBound($scope.codesGrid);
        };

        $scope.$on("kendoWidgetCreated", function (event, widget) {
            if (widget === $scope.codesGrid) {
                setDbClick($scope.codesGrid);
            }
        });
        /* END - kendo ui grid configurations*/




    }]);