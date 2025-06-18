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
        $scope.copy_options_colors = true;

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

            var roomsToCopy = $scope.action === 'Copy' ? $scope.rooms.length : 1;

            var departments = GridService.getSelecteds($scope.departmentsGrid);
            var data = [];

            for (var i = 0; i < roomsToCopy; i++) {
                departments.forEach(item => {
                    var currentLocation = {
                        source_project_id: local.params.project_id,
                        source_phase_id: local.params.phase_id,
                        source_department_id: local.params.department_id,
                        source_room_id: local.params.room_id,
                        phase_id: item.phase_id,
                        phase_description: item.phase_desc,
                        department_id: item.department_id,
                        department_description: item.department_desc,
                        room_name: $scope.action === 'Copy' ? $scope.rooms[i].Item2 : local.params.drawing_room_name,
                        room_number: $scope.action === 'Copy' ? $scope.rooms[i].Item1 : local.params.drawing_room_number,
                        room_id: null
                    };

                    data.push(currentLocation);
                });                
            }

            return data;            
        }

        $scope.save = function () {

            $scope.copyMoveRoom.$setSubmitted();

            if ($scope.action === 'Move') {
                var selected = GridService.getSelecteds($scope.departmentsGrid);

                if (selected.length !== 1) {
                    toastr.error("Please select just one destination to move room to.");
                    return;
                }

                if (selected[0].department_id === local.params.department_id) {
                    toastr.error("Please select a different destination to move room to.");
                    return;
                }  

            }

            var copyRoomData = {
                domain_id: AuthService.getLoggedDomain(),
                project_id: local.params.project_id,
                copy: $scope.action === 'Copy',
                options: $scope.copy_options_colors
            };            

            if ($scope.copyMoveRoom.$valid && GridService.anySelected($scope.departmentsGrid)) {
                ProgressService.blockScreen();

                var rooms = GetData();

                WebApiService.copy_from.save(copyRoomData, rooms, function (response) {
                    toastr.success($scope.action === 'Move' ? 'Room moved' : 'Room copied');
                    ProgressService.unblockScreen();

                    // The destination to navigate to after the copy/move
                    var destinationRoom = {
                        project_id: local.params.project_id,
                        phase_id: rooms.at(-1).phase_id,
                        department_id: rooms.at(-1).department_id,
                    }

                    $mdDialog.hide(destinationRoom);
                }, function (error) {
                    ProgressService.unblockScreen();
                    toastr.error(`Error trying to ${$scope.action.toLowerCase()} items, please contact technical support`);
                    $mdDialog.cancel();
                });
            } else if (!GridService.anySelected($scope.departmentsGrid)) {
                ProgressService.unblockScreen();
                toastr.error('Please make sure you select a destination');
            }
        };
    }]);