xPlanner.controller('SplitRoomCtrl', ['$scope', 'StatusListProject', '$mdDialog', 'AuthService', 'local', 'WebApiService',
    'toastr', 'ProgressService',
    function ($scope, StatusListProject, $mdDialog, AuthService, local, WebApiService, toastr, ProgressService) {


        $scope.data = {
            cp_project_id: local.params.project_id
        };

        $scope.params = local.params;
        $scope.room_count = [];
        $scope.departments = [];

        for (var i = 1; i < local.params.room_quantity; i++) {
            $scope.room_count.push(i);
            $scope.departments.push({});
        }

        WebApiService.genericController.query({ controller: "TemplateRoom", action: "All", domain_id: AuthService.getLoggedDomain() },
            function (templates) {
                $scope.anotherTemplates = templates.filter(function (item) { return item.domain_id == AuthService.getLoggedDomain() && item.is_template == 1 })
                    .map(function (item) { return item.drawing_room_name });
            });


        var GetParamsCB = function (data, action, phase_id) {
            return {
                controller: data,
                action: action,
                domain_id: AuthService.getLoggedDomain(),
                project_id: $scope.data.cp_project_id,
                phase_id: (data == 'departments' ? phase_id : null)
            };
        };


        $scope.getData = function (data, phase_id, pos) {
            WebApiService.genericController.query(GetParamsCB(data + 's', "All", phase_id), function (items) {
                if (data == 'department') {
                    if (pos == 0) {
                        if (phase_id == local.params.phase_id) {
                            $scope.actual_departments = items;
                        }

                        for (var i = 1; i < local.params.room_quantity; i++) {
                            $scope.departments[i] = [];
                            $scope.departments[i] = items;

                        }
                    }
                    else {
                        $scope.departments[pos] = items;
                        $scope.data.cp_department_id[pos] = null;
                    }
                }
                else
                    $scope[data + 's'] = items;
            });
        };

        $scope.getData('phase', 0, 0);
        $scope.getData('department', local.params.phase_id, 0);


        function GetParams() {

            var params = {};

            params.domain_id = AuthService.getLoggedDomain();
            params.project_id = $scope.params.project_id;
            params.phase_id = $scope.params.phase_id;
            params.department_id = $scope.params.department_id;
            params.room_id = $scope.params.room_id;
            params.is_linked_template = $scope.data.link_template ? true : false;
            params.template_name = $scope.data.template_name;

            return params;
        };

        function GetBodyParams() {

            var list_params = [];

            for (var i = 1; i < local.params.room_quantity; i++) {
                var params = {};
                params.project_id = $scope.params.project_id;
                params.phase_id = $scope.data.cp_phase_id[i];
                params.department_id = $scope.data.cp_department_id[i];
                params.room_name = $scope.data.room_name[i];
                params.room_number = $scope.data.room_number[i];

                list_params.push(params);
            }

            return list_params;
        };

        $scope.copy = function () {

            $scope.copyFromForm.$setSubmitted();
            ProgressService.blockScreen();
            if ($scope.copyFromForm.$valid) {
                WebApiService.split_rooms.save(GetParams(), GetBodyParams(), function (data) {
                    ProgressService.unblockScreen();
                    toastr.success('The rooms were successfully splitted');
                    $mdDialog.hide();
                }, function (error) {
                    ProgressService.unblockScreen();
                    toastr.error('Error trying to split rooms, please contact the technical support');
                    $mdDialog.cancel();
                });
            } else {
                ProgressService.unblockScreen();
                toastr.error("Please make sure you entered correctly all the fields");
            }

        };

        $scope.close = function () {
            $mdDialog.cancel();
        };
    }]);