xPlanner.controller('AddRoomCtrl', ['$scope', 'StatusListProject', '$mdDialog', 'AuthService', 'local', 'WebApiService', 'toastr', 'ProgressService',
    function ($scope, StatusListProject, $mdDialog, AuthService, local, WebApiService, toastr, ProgressService) {

        $scope.data = {
            cp_project_id: local.params.project_id,
            
        };

        $scope.params = local.params;
        $scope.room_count = [];
        $scope.departments = [];
        $scope.template_options = [{ value: 1, description: 'No Template' }, { value: 3, description: 'Linked Template' }, { value: 4, description: 'Unlinked Template' }]
        $scope.data.room_number = { 0: '' };
        $scope.data.room_name = { 0: '' };

        var GetParamsCB = function (data, action, phase_id) {
            return {
                controller: data,
                action: action,
                domain_id: AuthService.getLoggedDomain(),
                project_id: $scope.data.cp_project_id,
                phase_id: (data == 'departments' ? phase_id : null)
            };
        };

        function addRows() {
            var row_number = 0;
            if ($scope.room_count.length > 0) {
                row_number = $scope.room_count[$scope.room_count.length - 1] + 1;
            }
            $scope.room_count.push(row_number);
            $scope.departments.push({});
            //if ($scope.data.room_number == undefined) {
            //    $scope.data.room_number = {0: ''};
            //}
            //if ($scope.data.room_name == undefined) {
            //    $scope.data.room_name = { 0: '' };
            //}
            $scope.data.room_name[row_number] = '';
            $scope.data.room_number[row_number] = '';

            return row_number;
        }

        addRows();


        $scope.getData = function (data, phase_id, pos) {
            WebApiService.genericController.query(GetParamsCB(data, "All", phase_id), function (items) {
                if (data == 'departments') {
                    $scope.departments[pos] = items;

                    if (pos == 0)
                        $scope.data.cp_department_id = { 0: local.params.department_id };
                    else {
                        $scope.data.cp_department_id[pos] = null;
                        $scope.data.cp_department_id[pos] = $scope.data.cp_department_id[pos - 1];
                    }
                }
                else {
                    //phase
                    $scope[data] = items;
                    $scope.data.cp_phase_id = { 0: local.params.phase_id };
                }
            });
        };

        $scope.getData('phases', 0, 0);
        $scope.getData('departments', local.params.phase_id, 0);

        $scope.addRoom = function (row) {
            var row_number = addRows();
            if (row_number == row + 1) {
                $scope.data.cp_phase_id[row_number] = $scope.data.cp_phase_id[$scope.room_count[row_number - 1]];
                $scope.getData('departments', $scope.data.cp_phase_id[row_number], row_number);
            }
            else {
                for (var i = row_number; i > row+1; i--) {
                    $scope.data.cp_phase_id[i] = $scope.data.cp_phase_id[i - 1];
                    $scope.departments[i] = $scope.departments[i - 1];
                    $scope.data.cp_department_id[i] = $scope.data.cp_department_id[i - 1];
                    $scope.data.room_name[i] = $scope.data.room_name[i - 1]; //($scope.data.room_name != undefined && $scope.data.room_name[i - 1] != undefined ? $scope.data.room_name[i - 1] : '');
                    $scope.data.room_number[i] = $scope.data.room_number[i - 1]; //($scope.data.room_number != undefined && $scope.data.room_number[i-1] != undefined ? $scope.data.room_number[i - 1] : '');
                }
                $scope.data.cp_phase_id[row + 1] = $scope.data.cp_phase_id[row];
                $scope.departments[row + 1] = $scope.departments[row];
                $scope.data.cp_department_id[row + 1] = $scope.data.cp_department_id[row];
                $scope.data.room_name[row + 1] = '';
                $scope.data.room_number[row + 1] = '';
            }
            

        }

        $scope.deleteRoom = function (row_number) {
            if ($scope.room_count.length > 1) {
                var index = $scope.room_count.indexOf(row_number);

                $scope.room_count.splice(index, 1);
                delete $scope.data.cp_phase_id[index];
                delete $scope.data.cp_department_id[index];
                delete $scope.data.room_name[index];
                delete $scope.data.room_number[index];
            }
        }

        $scope.$watch('data.is_linked_template', function (newValue) {
            if (newValue && newValue != 1) {
                if (newValue == 4)
                    newValue = null;

                WebApiService.template_room_filtered.query(getParams2('TemplateList', local, newValue), function (templates) {
                    $scope.data.template_id = null;
                    $scope.templates = templates;
                    $scope.templates.concat($scope.templates.map(function (item) {
                        item.name = item.department_type + ' - ' + item.description;
                    }));

                });
            }
        });


        function GetParams() {

            var params = {};

            params.domain_id = AuthService.getLoggedDomain();
            params.project_id = $scope.params.project_id;
            params.phase_id = $scope.params.phase_id;
            params.department_id = $scope.params.department_id;
            params.is_linked_template = $scope.data.is_linked_template == 3 ? true : false;
            params.template_id = $scope.data.template_id;

            return params;
        };

        function getParams2(action, data, template_type) {
            return {
                action: action,
                domain_id: AuthService.getLoggedDomain(),
                project_id: template_type ? data.params.project_id : 0,
                template_type: template_type
            };
        }

        function GetBodyParams() {

            var list_params = [];
            var rooms = $scope.room_count;

            rooms.forEach(function (i) {
                var params = {};
                params.project_id = $scope.params.project_id;
                params.phase_id = $scope.data.cp_phase_id[i];
                params.department_id = $scope.data.cp_department_id[i];
                params.room_name = $scope.data.room_name[i];
                params.room_number = $scope.data.room_number != undefined && $scope.data.room_number[i] != undefined ? $scope.data.room_number[i]: null;

                list_params.push(params);
            });

            return list_params;
        };

        $scope.copy = function () {
            $scope.addRoomForm.$setSubmitted();
            ProgressService.blockScreen();
            var body_params = GetBodyParams();
            if ($scope.addRoomForm.$valid) {
                WebApiService.add_multi_rooms.save(GetParams(), body_params, function (data) {
                    ProgressService.unblockScreen();
                    toastr.success('The rooms were successfully added');
                    $mdDialog.hide(data);
                }, function (error) {
                    toastr.error('Error trying to add rooms, please contact the technical support');
                    ProgressService.unblockScreen();
                    //$mdDialog.cancel();

                });
            } else {
                toastr.error("Please make sure you entered correctly all the fields");
                ProgressService.unblockScreen();
            }

        };

        $scope.close = function () {
            $mdDialog.cancel();
        };
    }]);