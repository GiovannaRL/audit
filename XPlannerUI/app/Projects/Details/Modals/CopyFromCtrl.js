xPlanner.controller('CopyFromCtrl', ['$scope', 'StatusListProject', '$mdDialog', 'AuthService', 'local', 'WebApiService',
    'toastr',
    function ($scope, StatusListProject, $mdDialog, AuthService, local, WebApiService, toastr) {

        $scope.statusList = StatusListProject;

        $scope.copyLevel = local.params['room_id'] ? 'room' 
            : (local.params['department_id'] ? 'department'
                : (local.params['phase_id'] ? 'phase' : 
                    (local.params['project_id'] ? 'project' : null)))

        $scope.data = {
            status: 'A'
        };

        $scope.params = local.params;

        var GetParamsCB = function (data, action) {
            return {
                controller: data,
                action: action,
                domain_id: AuthService.getLoggedDomain(),
                status: $scope.data.status,
                project_id: $scope.data.cp_project_id,
                phase_id: $scope.data.cp_phase_id,
                department_id: $scope.data.cp_department_id,
                room_id: $scope.data.cp_room_id
            };
        };

        function getFilteredItemsByData(data, items) {
            return items.filter(function (i) { return i[data + '_id'] != local.params[data + '_id'] });
        }

        function getFilteredProjectsByData(data, items) {
            return items.filter(function (i) { return i.status == $scope.data.status });
        }

        $scope.getData = function (data) {
            WebApiService.genericController.query(GetParamsCB(data + 's', "All"), function (items) {
                switch (data) {
                    case 'project':
                        $scope.phases = [];
                        break;
                    case 'phase':
                        $scope.departments = [];
                    case 'department':
                        $scope.rooms = []
                }
                if (data === 'project' && $scope.copyLevel === data) {
                    $scope[data + 's'] = getFilteredProjectsByData(data, items);
                    return;
                }
                var isSameProject = $scope.data.cp_project_id === local.params['project_id'];
                if (data === 'phase' && isSameProject && $scope.copyLevel === data) {
                    $scope[data + 's'] = getFilteredItemsByData(data, items);
                    return;
                }

                var isSamePhase = $scope.data.cp_phase_id === local.params['phase_id'];
                if (data === 'department' && isSamePhase && $scope.copyLevel === data) {
                    $scope[data + 's'] = getFilteredItemsByData(data, items);
                    return;
                }

                var isSameDepartment = $scope.data.cp_department_id === local.params['department_id'];
                if (data === 'room' && isSameDepartment && $scope.copyLevel === data) {
                    $scope[data + 's'] = getFilteredItemsByData(data, items);
                    return;
                }

                $scope[data + 's'] = items;
            });
        };

        $scope.getData('project');

        function GetParamsCopy() {

            var params = angular.copy($scope.data);

            var selectedProject = $scope.projects.filter(function (i) { return i.project_id == $scope.data.cp_project_id });

            params.domain_id = AuthService.getLoggedDomain();
            params.cp_domain_id = selectedProject[0].domain_id;
            params.project_id = $scope.params.project_id;
            params.phase_id = $scope.params.phase_id || -1;
            params.department_id = $scope.params.department_id || -1;
            params.room_id = $scope.params.room_id || -1;
            params.cp_opt_col = $scope.data.cp_opt_col ? true : false;
            params.cp_phase_id = params.cp_phase_id || -1;
            params.cp_department_id = params.cp_department_id || -1;
            params.cp_room_id = params.cp_room_id || -1;

            return params;
        };

        $scope.copy = function () {

            $scope.copyFromForm.$setSubmitted();

            if ($scope.copyFromForm.$valid) {
                WebApiService.copy_from.save(GetParamsCopy(), null, function (data) {
                    toastr.success('Copied');
                    $mdDialog.hide();
                }, function (error) {
                    if (error.status === 409) {
                        toastr.info('The phases, departments, rooms and assets which already exist in this project were not copied');
                        $mdDialog.hide();
                    } else
                        toastr.error('Error to try copy items, please contact the technical support');
                    $mdDialog.cancel();
                });
            } else {
                toastr.error("Please make sure you entered correctly all the fields");
            }

        };

        $scope.close = function () {
            $mdDialog.cancel();
        };
    }]);