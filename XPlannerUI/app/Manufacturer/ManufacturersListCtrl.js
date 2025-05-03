xPlanner.controller('ManufacturersListCtrl', ['$scope', 'GridService', 'ProgressService', 'toastr', 'AuthService', 'DialogService',
        'HttpService', 'WebApiService', '$state',
    function ($scope, GridService, ProgressService, toastr, AuthService, DialogService, HttpService, WebApiService, $state) {

        $scope.$emit('initialTab', 'manufacturers');
        $scope.grid_content_height = window.innerHeight - 210;

        /* grid configuration */
        var columns = [
                    { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(manufacturersListGrid)\" ng-checked=\"allPagesSelected(manufacturersListGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, manufacturersListGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, manufacturersListGrid)\" ng-checked=\"isSelected(manufacturersListGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
                    { field: "manufacturer_description", title: "Manufacturer", width: 450 },
                    { field: "date_added", title: "Date Added", width: 180, template: "#: date_added ? kendo.toString(kendo.parseDate(date_added), \"MM-dd-yyyy\") : '' #" },
                    { field: "added_by", title: "Added By", width: 180 },
                    { field: "domain.name", title: "Owner", template: "{{ dataItem.domain.name | capitalize }}", width: 180 },
                    {
                        headerTemplate:
                        "<section layout=\"row\" layout-align=\"center end\"><button style=\"margin-left: -0.25em; padding-bottom: 0.2em;\" class=\"md-icon-button md-button no-button\"><i class=\"material-icons no-button\">comment</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">Comment</md-tooltip>" +
                        "</button></section>", template: "<div ng-if=\" #: comment != null && comment != '' # \" align=center><md-icon class=\"no-button grid-item-color\">comment</md-icon><md-tooltip md-direction=\"bottom\">#: comment #</md-tooltip></div>", width: 70
                    }
        ];

        /*Get the data to dataSource*/
        var dataSource = {
            transport: {
                read: {
                    url: HttpService.generic("manufacturer", "All", AuthService.getLoggedDomain()),
                    headers: {
                        Authorization: "Bearer " + AuthService.getAccessToken()
                    }
                },
                error: function () {
                    toastr.error("Error to retrieve data from server, please contact technical support");
                }
            },
            schema: {
                model: {
                    id: "manufacturer_id"
                }
            }
        };

        
        $scope.options = GridService.getStructure(dataSource, columns, null, { noRecords: "No manufacturers available", height: $scope.grid_content_height, groupable: true });

        function setDbClick(grid) {
            if (grid) {
                grid.tbody.find("tr").dblclick(function () {
                    var manufacturer = grid.dataItem(this);
                    $scope.showDetails(manufacturer);
                });
            }
        };

        $scope.dataBound = function () {
            setDbClick($scope.manufacturersListGrid);
            GridService.dataBound($scope.manufacturersListGrid);
        }

        /* Select the grid's rows */
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = GridService.select;
        $scope.allPagesSelected = GridService.allPagesSelected;
        /* END - Select the grid's rows */

        $scope.collapseExpand = GridService.collapseExpand;
        /* END - grid configuration */

        /* Show manufacturer details*/
        $scope.showDetails = function (manufacturer) {
            if (!manufacturer) {
                if (!GridService.verifySelected('view details', 'manufacturer', $scope.manufacturersListGrid, true)) return;
                manufacturer = GridService.getSelecteds($scope.manufacturersListGrid)[0];
            }

            GridService.unselectAll($scope.manufacturersListGrid);
            $state.go('assetsWorkspace.manufacturersDetails', { domain_id: manufacturer.domain_id, manufacturer_id: manufacturer.manufacturer_id });
        };

        // Add a new manufacturer
        $scope.openAddModal = function () {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            DialogService.openModal('app/Manufacturer/Modals/AddManufacturer.html', 'AddManufacturerCtrl').then(function (manufacturers) {
                angular.forEach(manufacturers, function (i) {
                    $scope.manufacturersListGrid.dataSource.add({ added_by: i.added_by, comment: i.comment, date_added: i.date_added, domain_id: i.domain_id, manufacturer_description: i.manufacturer_description, manufacturer_id: i.manufacturer_id });
                });
            });
        };

        // delete manufacturers
        $scope.delete = function () {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            if (GridService.verifySelected('delete', 'manufacturer', $scope.manufacturersListGrid)) {
                DialogService.Confirm('Are you sure?', 'The manufacturer(s) will be deleted permanently!').then(function () {
                    ProgressService.blockScreen();
                    GridService.deleteItems(WebApiService.genericController, function (i) { return { controller: 'manufacturer', action: 'Item', domain_id: i.domain_id, project_id: i.manufacturer_id } },
                        $scope.manufacturersListGrid, null, true).then(function (data) {
                            ProgressService.unblockScreen();
                            toastr.success('Manufacturer(s) Deleted');
                        }, function (error) {
                            ProgressService.unblockScreen();
                            if (error.status === 409) toastr.info("Some, or all, of the manufacturers could not be deleted -- there is assigned asset");
                            else toastr.error('Error to delete one o more manufacturers, please contact the technical support');
                            GridService.unselectAll($scope.manufacturersListGrid);
                        });
                });
            }
        };

    }]);