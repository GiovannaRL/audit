xPlanner.controller('CopyRoomToCtrl', ['$scope', 'AuthService', 'GridService', 'local', '$mdDialog', 'localStorageService',
        'ProgressService', 'HttpService', 'toastr', 'WebApiService',
    function ($scope, AuthService, GridService, local, $mdDialog, localStorageService, ProgressService, HttpService, toastr,
        WebApiService) {

        $scope.action = local.copy ? 'Copy' : 'Move';
        $scope.copyMove = local.copy && local.move;
        var lastProjectId = local.params.project_id;

        $scope.toProject = {
            domain_id: AuthService.getLoggedDomain(),
            project_id: local.params.project_id
        };

        $scope.rooms = [{}];

        /* kendo ui grid configurations*/
        $scope.options = {
            dataSource: {
                transport: {
                    read: {
                        url: HttpService.project_locations('Departments', 'DepartmentsTable', AuthService.getLoggedDomain(), local.params.project_id),
                        headers: {
                            Authorization: "Bearer " + AuthService.getAccessToken()
                        }
                    }
                },
                error: function () {
                    toastr.error("Error to retrieve departments info from server, please contact technical support");
                },
                schema: {
                    model: {
                        fields: {
                            project_id: { type: "string" }
                        }
                    }
                },
                filter: [{ field: "project_id", operator: "eq", value: local.params.project_id }]
                //data: localStorageService.get('AllDepartmentsGrid') || []
            },
            scrollable: true,
            sortable: true,
            pageable: {
                pageSizes: [5, 10, 20, 50],
                pageSize: 5,
                buttonCount: 5
            },
            filterable: true,
            resizable: true,
            groupable: true,
            mobile: true,
            noRecords: {
                template: "No departments available"
            },
            columns: [
                { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(departmentsGrid)\" ng-checked=\"allPagesSelected(departmentsGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, departmentsGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, departmentsGrid)\" ng-checked=\"isSelected(departmentsGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
                { field: "phase_desc", title: "Phase" },
                { field: "department_desc", title: "Department" }
            ]
        };

        $scope.dataBound = GridService.dataBound;

        /* Select the grid's rows */
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = GridService.select;
        $scope.allPagesSelected = GridService.allPagesSelected;
        /* END - Select the grid's rows */


        $scope.$watch(function () { return $scope.toProject }, function (newValue, oldValue) {
            if (newValue && newValue.project_id && (newValue.project_id != lastProjectId)) {
                lastProjectId = newValue.project_id;
                $scope.departmentsGrid.dataSource.filter({ field: "project_id", operator: "eq", value: newValue.project_id });
            }
        });



        $scope.reload = function () {
            var controller = ($scope.action == "Move") ? "phases" : "departments";
            var action = ($scope.action == "Move") ? "PhasesTable" : "DepartmentsTable";

            $scope.options.dataSource.transport.read.url = HttpService.project_locations(controller, action, AuthService.getLoggedDomain(), local.params.project_id);
            $scope.departmentsGrid.dataSource.options = $scope.options;
            $scope.departmentsGrid.dataSource.read();
        };

        $scope.addRoom = function () {
            $scope.rooms[$scope.rooms.length - 1].complete = true;
            $scope.rooms.push({});
        };

        $scope.removeRoom = function (index) {
            $scope.rooms.splice(index, 1);
        };

        $scope.close = function () {
            $mdDialog.cancel();
        };

        function GetData() {

            if (!$scope.rooms[$scope.rooms.length - 1].Item1 && !$scope.rooms[$scope.rooms.length - 1].Item2) {
                $scope.rooms.length = $scope.rooms.length - 1;
            }

            var data = {
                move: $scope.action === 'Move',
                to: GridService.getSelecteds($scope.departmentsGrid),
                from_domain_id: AuthService.getLoggedDomain(),
                from_project_id: local.params.project_id,
                from_phase_id: local.params.phase_id,
                from_department_id: local.params.department_id,
                from_room_id: local.params.room_id,
                to_room_number_name: $scope.rooms,
                copy_options_colors: $scope.copy_options_colors
            };

            if (data.move && data.to_room_number_name.length == 0) {
                var movedRoom = {
                    item1: local.params.drawing_room_number,
                    item2: local.params.drawing_room_name,
                }
                data.to_room_number_name.push(movedRoom);                
            }

            return data;
        }

        $scope.save = function () {

            $scope.copyMoveRoom.$setSubmitted();

            if ($scope.action == 'Move' && GridService.getSelecteds($scope.departmentsGrid).length > 1) {
                toastr.error("Please select just one destination to move room to.");
                return;
            }

            


            if ($scope.copyMoveRoom.$valid && GridService.anySelected($scope.departmentsGrid)) {
                ProgressService.blockScreen();

                WebApiService.genericController.save({
                    controller: 'copyRoom', action: 'Item',
                    domain_id: AuthService.getLoggedDomain(), project_id: local.params.project_id
                }, GetData(), function (room) {
                    if($scope.action === 'Move')
                        toastr.success('Room moved');
                    else
                        toastr.success('Room copied');                    

                    ProgressService.unblockScreen();
                    $mdDialog.hide(room);
                }, function (data) {
                    ProgressService.unblockScreen();
                    toastr.error(data.data);
                    $mdDialog.cancel();
                });
            } else if (!GridService.anySelected($scope.departmentsGrid)) {
                ProgressService.unblockScreen();
                toastr.error('Please make sure you select a destination');
            }
        };
    }]);