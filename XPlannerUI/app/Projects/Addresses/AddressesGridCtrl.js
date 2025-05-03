xPlanner.controller('AddressesGridCtrl', ['$scope', 'AuthService', 'HttpService', 'WebApiService', 'GridService',
    'DialogService', 'ProgressService', 'toastr',
    function ($scope, AuthService, HttpService, WebApiService, GridService, DialogService, ProgressService, toastr) {

        $scope.isNotViewer = (AuthService.getLoggedUserType() != "3");

        /* kendo ui grid configurations*/
        var toolbar = {
            template:
                "<section layout=\"row\" ng-cloak ng-if=\"" + $scope.isNotViewer + "\">" +
                    "<section layout=\"row\" layout-align=\"start center\"  class=\"gray-color\">" +
                        "<button class=\"md-icon-button md-button\" ng-click=\"openAddEditModal()\"><i class=\"material-icons\">add</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">Add New Address</md-tooltip>" +
                        "</button>" +
                        "<button class=\"md-icon-button md-button\" ng-click=\"openAddEditModal(true)\"><i class=\"material-icons\">edit</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">Edit Address</md-tooltip>" +
                        "</button>" +
                        "<button class=\"md-icon-button md-button\" ng-click=\"delete()\"><i class=\"material-icons\">delete</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">Delete Addresses</md-tooltip>" +
                        "</button>" +
                        
                    "</section>" +
                    //"<section layout=\"row\" layout-align=\"end center\" flex=\"100\">" +
                    //    "<button class=\"md-icon-button md-button\" ng-click=\"openAddEditModal()\">" +
                    //        "<md-icon class=\"md-accent\">add_circle</md-icon><div class=\"md-ripple-container\"></div>" +
                    //        "<md-tooltip md-direction=\"bottom\">Add New Address</md-tooltip>" +
                    //    "</button>" +
                    //"</section>" +
                "</section>"
        };
        var dataSource = {
            transport: {
                read: {
                    url: HttpService.project_addresses("All", AuthService.getLoggedDomain(), $scope.params.project_id),
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
        var gridOptions = { groupable: true, noRecords: "No addresses available", height: 370 };
        var columns = [
                { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(addressesGrid)\" ng-checked=\"allPagesSelected(addressesGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, addressesGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, addressesGrid)\" ng-checked=\"isSelected(addressesGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
                { field: "nickname", title: "Nickname", width: 120 },
                { field: "description", title: "Description", width: 150 },
                { field: "address1", title: "Address1", width: 200 },
                { field: "address2", title: "Address2", width: 120 },
                { field: "city", title: "City", width: 100 },
                { field: "state", title: "State", width: 95 },
                { field: "zip", title: "Zip", width: 100 },
                { field: "is_default", title: "Default", template: "#= is_default ?  '<span layout=\"row\" layout-align=\"center center\"><i class=\"material-icons no-button\" style=\"color: green;\">check</i></span>' : '' #", filterable: false, width: 75 }
        ];

        $scope.options = GridService.getStructure(dataSource, columns, toolbar, gridOptions);

        function setDbClick(grid) {
            if (grid) {
                grid.tbody.find("tr").dblclick(function () {
                    var address = grid.dataItem(this);
                    $scope.openAddEditModal(true, address);
                });
            }
        };

        $scope.dataBound = function () {
            setDbClick($scope.addressesGrid);
            GridService.dataBound($scope.addressesGrid);
        };

        $scope.$on("kendoWidgetCreated", function (event, widget) {
            if (widget === $scope.addressesGrid) {
                setDbClick($scope.addressesGrid);
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
            if (!validateAccess()) {
                return;
            }

            var method = 'save';
            if (edit) {

                if (!item) {
                    if (!GridService.verifySelected('edit', 'address', $scope.addressesGrid, true)) return;
                }

                method = 'update';
                item = item || GridService.getSelecteds($scope.addressesGrid)[0];
            }

            var addressId;
            if (method === 'update') {
                addressId = item.id;
            }

            var params = { action: "Item", project_id: $scope.params.project_id, domain_id: AuthService.getLoggedDomain(), id: addressId };

            var gridItems = angular.copy($scope.addressesGrid.dataSource.data());

            DialogService.openModal('app/Projects/Addresses/Modals/AddEditAddress.html', 'AddEditAddressCtrl',
                { items: gridItems, params: params, address: item }, true).then(function (address) {

                    WebApiService.project_address[method](params, address, function (data) {

                        if (address.is_default && gridItems.length > 0) {
                            $scope.addressesGrid.dataSource.data(gridItems.map(function (i) { if (i.id == address.id) i.is_default = true; else i.is_default = false; return i }));
                        }

                        if (!item) {
                            if (gridItems.length == 0) {
                                data.is_default = true;
                            }
                            
                            GridService.addItem($scope.addressesGrid, JSON.parse(JSON.stringify(data)));
                            toastr.success('Address Saved');
                        } else {

                            GridService.updateItems($scope.addressesGrid, address);
                            toastr.success('Address Updated');
                        }
                    }, function () {
                        toastr.error("Erro to save address, please contact technical support");
                    });
                });
        };
        /* END - Open the add/edit address modal*/

        /* Delete selected addresses */
        $scope.delete = function () {
            if (!validateAccess()) {
                return;
            }

            if (GridService.verifySelected('delete', 'address', $scope.addressesGrid)) {
                DialogService.Confirm('Are you sure?', 'The addresses will be deleted permanently!').then(function () {
                    ProgressService.blockScreen();
                    GridService.deleteItems(WebApiService.project_address,
                        function (item) { return { action: "Item", domain_id: item.domain_id, project_id: item.project_id, id: item.id }; },
                        $scope.addressesGrid).then(function () {
                            ProgressService.unblockScreen();
                            toastr.success('Addresses Deleted!');
                        }, function (error) {
                            ProgressService.unblockScreen();

                            if (error.status === 409)
                                toastr.info('Some, or all, of the selected addresses could not be deleted because it\'s assigned to purchase orders');
                            else
                                toastr.error('Error to try delete addresses, please contact the technical support');
                        });
                    GridService.unselectAll($scope.addressesGrid);
                });
            }
        };
        /* END - Delete selected addresses */

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