xPlanner.controller('CopyFromCtrl', ['$scope', 'StatusListProject', '$mdDialog', 'AuthService', 'local', 'WebApiService',
    'toastr', 'HttpService', 'GridService', 'ProgressService',
    function ($scope, StatusListProject, $mdDialog, AuthService, local, WebApiService, toastr, HttpService, GridService, ProgressService) {

        $scope.statusList = StatusListProject;

        $scope.copyLevel = local.params['room_id'] ? 'room' 
            : (local.params['department_id'] ? 'department'
                : (local.params['phase_id'] ? 'phase' : 
                    (local.params['project_id'] ? 'project' : null)))

        $scope.data = {
            status: 'A'
        };    

        $scope.params = local.params;
        $scope.addedRooms = [];
        $scope.movedRooms = [];
        $scope.action = 'Copy';
        $scope.copy_options_colors = true;

        $scope.searchTerm = ''; 

        // Clear Search
        $scope.clearSearchTerm = function () {
            $scope.searchTerm = '';
        };


        $scope.changeAction = function (action) {
            if ($scope.data.source_project_id && $scope.data.source_project_id != local.params.project_id && action === 'Move') {
                toastr.error('You can only move rooms within the same project. You must select the same project for source and target before switching to move.');
                $scope.action = "Copy";
                return;
            }         
            $scope.reloadSourceGrid();            
        };

        $scope.isProcessing = false;

        // Filter Project
        $scope.searchProjects = function (searchTerm) {
            return function (proj) {
                if (!searchTerm) {
                    return true; 
                }
                var projectText = (proj.client_project_number ? proj.client_project_number + ' - ' : '') + proj.project_description;
                return projectText.toLowerCase().includes(searchTerm.toLowerCase());
            };
        };        

        // get local phases and departments
        function getTargetData(controllerName, targetScopeProperty) {
            WebApiService.genericController.query(
                { controller: controllerName, action: 'All', domain_id: AuthService.getLoggedDomain(), project_id: local.params.project_id },
                function (data) {
                    $scope[targetScopeProperty] = data;
                },
                function (error) {
                    toastr.error(`Error to retrieve ${controllerName} info from server, please contact technical support`);
                }
            );
        }

        getTargetData('Phases', 'targetPhases');
        getTargetData('Departments', 'targetDepartments');   


        $scope.targetData = {
            phase_id: local.params.phase_id ? local.params.phase_id : null,
            department_id: local.params.department_id ? local.params.department_id : null,
        };


        // Source grid configurations
        $scope.options = {
            dataSource: {
                transport: {
                    read: {
                        url: function () {
                            return HttpService.project_locations('rooms', 'SourceRoom', AuthService.getLoggedDomain(), $scope.data.source_project_id || -1, $scope.data.source_phase_id || null, $scope.data.source_department_id || null);
                        },
                        headers: {
                            Authorization: "Bearer " + AuthService.getAccessToken()
                        }
                    }
                },
                error: function () {
                    toastr.error("Error to retrieve rooms info from server, please contact technical support");
                },
                schema: {
                    model: {
                        fields: {
                            project_id: { type: "string" },
                            phase_description: { type: "string" },
                            department_description: { type: "string" },
                            room_name: { type: "string" },
                            room_number: { type: "string" }
                        }
                    }
                },
                filter: [],
            },
            scrollable: true,
            sortable: true,
            pageable: {
                pageSizes: [10, 20, 50],
                pageSize: 10,
                buttonCount: 10
            },
            filterable: true,
            resizable: true,
            groupable: true,
            mobile: true,
            noRecords: {
                template: "No rooms available"
            },
            columns: [
                { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(departmentsGrid)\" ng-checked=\"allPagesSelected(departmentsGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, departmentsGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, departmentsGrid)\" ng-checked=\"isSelected(departmentsGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
                { field: "project_department.project_phase.description", title: "Phase" },
                { field: "project_department.description", title: "Department" },
                { field: "drawing_room_number", title: "Room No." },
                { field: "drawing_room_name", title: "Room Name" }
                
            ]
        };      

        //Target Grid
        $scope.roomsOptions = {
            dataSource: {
                data: $scope.addedRooms
            },
            scrollable: true,
            sortable: true,
            pageable: {
                pageSizes: [10, 10, 20, 50],
                pageSize: 10,
                buttonCount: 5
            },
            filterable: true,
            resizable: true,
            groupable: true,
            mobile: true,
            noRecords: {
                template: "No room added"
            },
            columns: [
                {
                    headerTemplate: `<md-checkbox class="checkbox" 
                     md-indeterminate="allSelected(roomsGrid)" 
                     ng-checked="allPagesSelected(roomsGrid)" 
                     aria-label="checkbox" 
                     ng-click="select($event, roomsGrid, true)"></md-checkbox>`,
                    template: `<md-checkbox class="checkbox" 
                       ng-click="select($event, roomsGrid)" 
                       ng-checked="isSelected(roomsGrid, dataItem)" 
                       aria-label="checkbox"></md-checkbox>`,
                    width: "3em"
                },
                { field: "phase_description", title: "Phase" },
                {
                    field: "department_description",
                    title: "Department",
                    width: "150px",
                    template: `<input type="text" 
                         ng-init="dataItem.department_description = dataItem.department_description || ''" 
                         ng-model="dataItem.department_description" 
                         ng-change="updateAddedDepartments(dataItem)"
                         class="editable-cell" 
                         style="border: none; background: transparent; width: 100%; padding: 0; outline: none;" />`,
                    attributes: { "class": "editable-cell" }
                },
                {
                    field: "room_number",
                    title: "Room No.",
                    template: GetEditableTemplate('room_number'),
                    attributes: { "class": "editable-cell" }
                },
                {
                    field: "room_name",
                    title: "Room Name",
                    template: GetEditableTemplate('room_name'),
                    attributes: { "class": "editable-cell" }
                }
            ],
            change: function (e) {
            }
        };

        function GetEditableTemplate(field) {
            return `<input type="text" 
           ng-init="dataItem.${field} = dataItem.${field} || ''" 
           ng-model="dataItem.${field}" 
           ng-change="updateAddedRooms(dataItem)"
           class="editable-cell" 
           style="border: none; background: transparent; width: 100%; padding: 0; outline: none;" />`;
        }

        $scope.updateAddedDepartments = function (dataItem) {
            dataItem.department_id = -1;
            $scope.addedRooms.forEach((item) => {
                if (item.source_room_id === dataItem.source_room_id) {
                    item.department_id = -1;
                    item.department_description = dataItem.department_description;
                }
            });
        };

        $scope.updateAddedRooms = function (dataItem) {
            $scope.addedRooms.forEach((item) => {
                if (item.source_room_id === dataItem.source_room_id) {
                    item.room_number = dataItem.room_number;
                    item.room_name = dataItem.room_name;
                }
            });
        }

        $scope.dataBound = GridService.dataBound;

        /* Select the grid's rows */
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = GridService.select;
        $scope.allPagesSelected = GridService.allPagesSelected;
        /* END - Select the grid's rows */


        $scope.reloadSourceGrid = async function () {
            try {
                ProgressService.blockScreen();
                var grid = $("#sourceGrid").data("kendoGrid");

                if (grid) {
                    grid.dataSource.transport.read.url = function () {
                        return HttpService.project_locations('rooms', 'SourceRoom', AuthService.getLoggedDomain(), $scope.data.source_project_id, $scope.data.source_phase_id, $scope.data.source_department_id);
                    };

                    await new Promise((resolve, reject) => {
                        grid.one("dataBound", resolve);
                        grid.dataSource.read();
                    });
                }
            } catch (error) {
                toastr.error("Error to retrieve rooms info from server, please contact technical support");
            } finally {
                if ($scope.action === 'Move')
                    $scope.updateSourceGrid();
                
                ProgressService.unblockScreen();
            }
        };        
        
        $scope.updateSourceGrid = async function () {
            ProgressService.blockScreen();

            if ($scope.action === "Copy" && $scope.movedRooms.length > 0) {
                $scope.reloadSourceGrid(); 
            }

            var grid = $("#sourceGrid").data("kendoGrid");

            if ($scope.action === "Move") {
                $scope.movedRooms.forEach(function (movedItem) {
                    var dataItem = grid.dataSource.data().find(item => item.room_id === movedItem.room_id);
                    if (dataItem) {
                        grid.dataSource._data.remove(dataItem);
                    }
                });
            }

            ProgressService.unblockScreen();
        };


                

        //Add room from Source Grid to Target Grid
        $scope.addRoom = function () {
            var selectedItems = $scope.departmentsGrid.select();

            selectedItems.each(function () {
                var item = $scope.departmentsGrid.dataItem(this);
                var exists = $scope.addedRooms.some(function (room) {
                    return room.room_id === item.room_id;
                });

                if (!exists) {
                    var selectedPhase = null;
                    var selectedDepartment = null;

                    if ($scope.targetData.phase_id) {
                        $scope.targetPhases.forEach((item) => {
                            if (item.phase_id === parseInt($scope.targetData.phase_id)) {
                                selectedPhase = item.description;
                            }
                        });
                    }

                    if ($scope.targetData.department_id) {
                        $scope.targetDepartments.forEach((item) => {
                            if (item.department_id === parseInt($scope.targetData.department_id)) {
                                selectedDepartment = item.description;
                            }
                        })
                    }
                    
                    $scope.addedRooms.push({
                        source_project_id: item.project_id,
                        source_phase_id: item.phase_id,
                        source_department_id: item.department_id,
                        source_room_id: item.room_id,
                        phase_id: $scope.targetData.phase_id ?? (item.project_id === local.params.project_id ? item.phase_id : -1),
                        phase_description: selectedPhase ?? item.project_department.project_phase.description,
                        department_id: $scope.targetData.department_id ?? (item.project_id === local.params.project_id ? item.department_id : -1),
                        department_description: selectedDepartment ?? item.project_department.description,
                        room_name: item.drawing_room_name,
                        room_number: item.drawing_room_number,
                        room_id: item.room_id
                    });

                    $scope.movedRooms.push(item);
                }
            });

            $scope.roomsGrid.dataSource.data($scope.addedRooms);
            GridService.unselectAll($scope.departmentsGrid);
            $scope.updateSourceGrid();
        };

        // Remove room from source grid 
        $scope.removeRoom = function () {    
            var grid = $("#sourceGrid").data("kendoGrid");
            var selectedItems = $scope.roomsGrid.select();

            selectedItems.each(function () {
                var item = $scope.roomsGrid.dataItem(this);

                $scope.addedRooms = $scope.addedRooms.filter(room => room.room_id !== item.room_id);

                var actualRoom = $scope.movedRooms.find(room => room.room_id == item.room_id);
                $scope.movedRooms = $scope.movedRooms.filter(room => room != actualRoom);
                

                if ($scope.action === "Move") {
                    if ((!$scope.data.source_phase_id || actualRoom.phase_id === parseInt($scope.data.source_phase_id)) &&
                        (!$scope.data.source_department_id || actualRoom.department_id === parseInt($scope.data.source_department_id))) {
                        AddIndexItem(actualRoom);                    
                    }
                }

            });

            $scope.roomsGrid.dataSource.data($scope.addedRooms);
            GridService.unselectAll($scope.roomsGrid);
        };

        //Return item to Source Grid
        function AddIndexItem(movedRoom) {
            var grid = $("#sourceGrid").data("kendoGrid");
            var movedItem = movedRoom.project_department.project_phase.description + movedRoom.project_department.description + movedRoom.drawing_room_name + movedRoom.drawing_room_number;
            var gridItems = grid.dataSource._data.map((item) => {
                var combinedString = item.project_department.project_phase.description + item.project_department.description + item.drawing_room_name + item.drawing_room_number;
                return combinedString;
            });

            gridItems.push(movedItem);
            gridItems.sort();
            var index = gridItems.indexOf(movedItem);
            grid.dataSource._data.splice(index, 0, movedRoom);
        }



        var GetSourceParams = function (data, action) {
            return {
                controller: data,
                action: action,
                domain_id: AuthService.getLoggedDomain(),
                status: $scope.data.status,
                project_id: $scope.data.source_project_id,
                phase_id: $scope.data.source_phase_id,
                department_id: $scope.data.source_department_id,
                room_id: $scope.data.source_room_id
            };
        };

        function getFilteredItemsByData(data, items) {
            return items.filter(function (i) { return i[data + '_id'] != local.params[data + '_id'] });
        }

        function getFilteredProjectsByData(data, items) {
            return items.filter(function (i) { return i.status == $scope.data.status });
        }

        $scope.getData = function (data) {
            if (data === 'phase' || data === 'department') {
                $scope.data.source_phase_id = (data === 'phase') ? null : $scope.data.source_phase_id;
                $scope.data.source_department_id = null;
            }

            if ($scope.data.source_project_id != local.params.project_id && $scope.action === 'Move') {
                toastr.info('You can only move rooms within the same project. You must select the same project for source and target before switching to move.');
                $scope.action = 'Copy';                
            }

            WebApiService.genericController.query(GetSourceParams(data + 's', "All"), function (items) {
                switch (data) {
                    case 'project':
                        $scope.phases = [];
                        $scope[data + 's'] = getFilteredProjectsByData(data, items);
                        break;
                    case 'phase':
                        $scope.departments = [];
                        $scope.rooms = [];
                        $scope[data + 's'] = getFilteredItemsByData(data, items);
                        break;
                    case 'department':
                        $scope.rooms = [];
                        $scope[data + 's'] = getFilteredItemsByData(data, items);
                        break;
                    case 'room':
                        $scope[data + 's'] = getFilteredItemsByData(data, items);
                        break;
                    default:
                        $scope[data + 's'] = items;
                }
            });

            $scope.reloadSourceGrid();
        };
              

        $scope.getData('project');


        // Set to default project.
        $scope.$watch("projects", function (newValue) {
            if (newValue && newValue.length > 0) {
                newValue.forEach(function (item) {
                    if (item.project_id == local.params.project_id) {
                        $scope.data.source_project_id = item.project_id;                        
                    }
                });

                if ($scope.data.source_project_id) {
                    $scope.getData('phase');
                }
            }
        });

        function GetParamsCopy() {
            var params = {}; 
            params.domain_id = AuthService.getLoggedDomain();
            params.project_id = local.params.project_id;
            params.action = $scope.action === 'Copy';
            params.options = $scope.copy_options_colors;
            params.added_by = null;

            return params;
        }

        $scope.copy = function () {
            if ($scope.addedRooms.length < 1) {
                toastr.error('There is no room to ' + $scope.action + '.');
                return;
            }

            // To avoid double click
            if ($scope.isProcessing) 
                return;
            
            $scope.isProcessing = true;

            var copyRoomData = {
                domain_id: AuthService.getLoggedDomain(),
                project_id: local.params.project_id,
                copy: $scope.action === 'Copy',
                options: $scope.copy_options_colors                
            };

            WebApiService.copy_from.save(copyRoomData, $scope.addedRooms,  function (response) {
                toastr.success($scope.action === 'Move' ? 'Room moved' : 'Room copied');
                ProgressService.unblockScreen();
                $scope.isProcessing = false;
                $mdDialog.hide(response);
            }, function (error) {
                ProgressService.unblockScreen();
                toastr.error('Error to try copy items, please contact the technical support');
                $scope.isProcessing = false;
                $mdDialog.cancel();
            });
        };

        $scope.close = function () {
            $mdDialog.cancel();
        };
    }]);