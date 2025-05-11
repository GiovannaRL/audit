xPlanner.controller('VendorsListCtrl', ['$scope', 'GridService', 'HttpService', 'AuthService', 'WebApiService', 'toastr', 'DialogService',
        'ProgressService', '$state',
    function ($scope, GridService, HttpService, AuthService, WebApiService, toastr, DialogService, ProgressService, $state) {

        $scope.$emit('initialTab', 'vendors');
        $scope.grid_content_height = window.innerHeight - 210;

        /* grid configuration */
        var columns = [
                    { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(vendorsListGrid)\" ng-checked=\"allPagesSelected(vendorsListGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, vendorsListGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, vendorsListGrid)\" ng-checked=\"isSelected(vendorsListGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
                    { field: "name", title: "Vendor", width: 170 },
                    { field: "territory", title: "Territory", width: 170 },
                    { field: "date_added", title: "Date Added", width: 150, template: "#: date_added ? kendo.toString(kendo.parseDate(date_added), \"MM-dd-yyyy\") : '' #" },
                    { field: "added_by", title: "Added By", width: 150 },
                    { field: "domain.name", title: "Owner", template: "{{ dataItem.domain.name | capitalize }}", width: 150 },
                    {
                        headerTemplate:
                        "<section layout=\"row\" layout-align=\"center end\"><button style=\"margin-left: -0.25em; padding-bottom: 0.2em;\" class=\"md-icon-button md-button no-button\"><i class=\"material-icons no-button\">comment</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">Comment</md-tooltip>" +
                        "</button></section>", template: "<div ng-if=\" #: comment != null # \" align=center><md-icon class=\"no-button grid-item-color\">comment</md-icon><md-tooltip md-direction=\"bottom\">#: comment #</md-tooltip></div>", width: 70
                    }
        ];

        /*var toolbar = {
            template:
                "<section layout=\"row\" ng-cloak>" +
                    "<section layout=\"row\" layout-align=\"start center\">" +
                        "<button class=\"md-icon-button md-button\" ng-click=\"delete()\"><i class=\"material-icons\">delete</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">Delete Vendor(s)</md-tooltip>" +
                        "</button>" +
                        "<button class=\"md-icon-button md-button\" ng-click=\"showDetails()\"><i class=\"material-icons\">visibility</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">View vendor details</md-tooltip>" +
                        "</button>" +
                    "</section>" +
                "</section>"
        };*/

        /*Get the data to dataSource*/
        var dataSource = {
            transport: {
                read: {
                    url: HttpService.generic("vendor", "All", AuthService.getLoggedDomain()),
                    headers: {
                        Authorization: "Bearer " + AuthService.getAccessToken()
                    }
                }
            },
            error: function () {
                toastr.error("Error to retrieve data from server, please contact technical support");
            }
        };
        $scope.options = GridService.getStructure(dataSource, columns, null, { noRecords: "No vendors available", height: $scope.grid_content_height, groupable: true });

        function setDbClick(grid) {
            if (grid) {
                grid.tbody.find("tr").dblclick(function () {
                    var vendor = grid.dataItem(this);
                    $scope.showDetails(vendor);
                });
            }
        };

        $scope.showDetails = function (vendor) {
            if (!vendor) {
                if (!GridService.verifySelected('view details', 'vendor', $scope.vendorsListGrid, true)) return;
                vendor = GridService.getSelecteds($scope.vendorsListGrid)[0];
            }

            ProgressService.blockScreen();
            $state.go('assetsWorkspace.vendorsDetails', { domain_id: vendor.domain_id, vendor_id: vendor.vendor_id });
        };

        $scope.dataBound = function () {
            setDbClick($scope.vendorsListGrid);
            GridService.dataBound($scope.vendorsListGrid);
        }

        /* Select the grid's rows */
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = GridService.select;
        $scope.allPagesSelected = GridService.allPagesSelected;
        /* END - Select the grid's rows */

        $scope.collapseExpand = GridService.collapseExpand;
        /* END - grid configuration */

        // Add a new asset
        $scope.openAddModal = function () {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            DialogService.openModal('app/Vendor/Modals/AddVendor.html', 'AddVendorCtrl').then(function (vendors) {
                angular.forEach(vendors, function (i) {
                    $scope.vendorsListGrid.dataSource.add({ added_by: i.added_by, comment: i.comment, date_added: i.date_added, domain_id: i.domain_id, hospitals: i.hospitals, name: i.name, territory: i.territory, vendor_id: i.vendor_id});
                });
            });
        };

        // delete vendors
        $scope.delete = function () {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            if (GridService.verifySelected('delete', 'vendor', $scope.vendorsListGrid)) {
                DialogService.Confirm('Are you sure?', 'The vendor(s) will be deleted permanently').then(function () {
                    ProgressService.blockScreen();
                    GridService.deleteItems(WebApiService.genericController, function (i) { return { controller: 'vendor', action: 'Item', domain_id: i.domain_id, project_id: i.vendor_id } },
                        $scope.vendorsListGrid, null, true).then(function (data) {
                            ProgressService.unblockScreen();
                            toastr.success('Vendor(s) Deleted');
                            GridService.unselectAll($scope.vendorsListGrid);
                        }, function (error) {
                            ProgressService.unblockScreen();
                            if (error.status === 409) toastr.info("Some, or all, of the vendors could not be deleted -- purchase orders have been issued");
                            else toastr.error('Error to delete one o more vendors, please contact the technical support');
                            GridService.unselectAll($scope.vendorsListGrid);
                        });
                });
            }
        };

    }]);