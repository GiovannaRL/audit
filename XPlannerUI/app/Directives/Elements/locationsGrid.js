xPlanner.directive('awLocationsGrid', ['GridService', 'HttpService', 'AuthService', 'ProgressService', 'toastr', 'KendoGridService',
function (GridService, HttpService, AuthService, ProgressService, toastr, KendoGridService) {
    return {
        restrict: 'E',
        scope: {
            params: '=',
            selecteds: '=',
            height: '=',
            selectAll: '=',
            notLinkedOnly: '='
        },
        link: function (scope, elem, attrs, ctrl) {

            scope.gridHeight = scope.height || (window.innerHeight - 168);

            var allLocations;
            var gridName = 'locationsGrid';
            var gridLoaded = false;

            // DataSource
            var _dataSource = new kendo.data.DataSource({
                transport: {
                    read: {
                        url: HttpService.project_locations('rooms', scope.notLinkedOnly ? 'unlinkedRooms' : 'locationsTable',
                            AuthService.getLoggedDomain(), scope.params.project_id, scope.params.phase_id,
                            scope.params.department_id, scope.params.room_id),
                        headers: {
                            Authorization: "Bearer " + AuthService.getAccessToken()
                        }
                    }
                },
                schema: {
                    model: {
                        id: "id"
                    },
                    parse: function (data) {
                        return data.map(function (dt) { dt.id = dt.domain_id.toString() + dt.project_id.toString() + dt.phase_id.toString() + dt.department_id.toString() + dt.room_id.toString(); return dt; });
                    }
                },
                error: function () {
                    ProgressService.unblockScreen();
                    toastr.error("Error to retrieve locations from server, please contact technical support");
                },
                change: function () {
                    ProgressService.unblockScreen();
                }
            });

            // Columns
            var _columns = [
                  //define template column with checkbox and attach click event handler
                  {
                      headerTemplate: KendoGridService.GetSelectAllTemplate(gridName),
                      template: function (dataItem) {
                          return KendoGridService.GetSelectRowTemplate(dataItem.id);
                      },
                      width: "4em"
                  },
                  {
                      field: "phase_desc",
                      title: "Phase"
                  },
                  {
                      field: "department_desc",
                      title: "Department"
                  },
                  {
                      field: "room_desc",
                      title: "Room"
                  }
            ];

            // Grid options
            var _gridOptions = {
                reorderable: true,
                groupable: true,
                noRecords: scope.notLinkedOnly ? "No locations available (the rooms linked to any template are not displayed because the user cannot modified it inventory)" : "No locations available",
                height: scope.gridHeight
            };

            function dataBound() {

                KendoGridService.DataBound(scope.locationsGrid);
                if (scope.params.room_id || scope.selectAll) {
                    KendoGridService.SelectAll(scope.locationsGrid, null, true);
                    scope.selecteds = KendoGridService.GetSelecteds(scope.locationsGrid);
                    scope.$apply();
                }
                gridLoaded = true;
            }

            function selectRow(ev) {
                KendoGridService.SelectRow(ev, this);
                scope.selecteds = KendoGridService.GetSelecteds(scope.locationsGrid);
                scope.$apply();
            }

            function selectAll(ev) {
                KendoGridService.SelectAllChange(ev);
                scope.selecteds = KendoGridService.GetSelecteds(scope.locationsGrid);
                scope.$apply();
            }

            //Grid definition
            scope.locationsGrid = $("#locationGrid").kendoGrid(angular.extend({}, { dataBound: dataBound }, GridService.getStructure(_dataSource, _columns, null, _gridOptions, false))).data("kendoGrid");
            scope.locationsGrid.table.on("click", ".k-checkbox", { grid: scope.locationsGrid, grid_name: gridName }, selectRow);
            $('#select-all-' + gridName).on('change', { grid: scope.locationsGrid }, selectAll);

            scope.collapseExpand = KendoGridService.CollapseExpand;

            scope.searchLocations = function (value) {
                if (gridLoaded) {
                    var items = allLocations || scope.locationsGrid.dataSource.data();

                    if (!value) {
                        scope.locationsGrid.dataSource.data(allLocations);
                    } else {
                        value = value.toLowerCase();
                        allLocations = items;
                        var columns = scope.locationsGrid.columns;
                        var filteredItems = items.filter(function (item) {
                            for (var i = 1; i < columns.length; i++) {
                                if (!columns[i].hidden && String(item[columns[i].field]).toLowerCase().indexOf(value) > -1) {
                                    return true;
                                }
                            };
                        });
                        scope.locationsGrid.dataSource.data(filteredItems);
                        if (filteredItems.length > 0) {
                            scope.locationsGrid.dataSource.page(1);
                        }
                    }
                }
            };

            scope.clearAllFilters = function () {
                scope.searchBoxValue = null;
                KendoGridService.ClearFilters(scope.locationsGrid);
                scope.searchLocations();
            };
        },
        templateUrl: 'app/Directives/Elements/locationsGrid.html'
    }
}]);