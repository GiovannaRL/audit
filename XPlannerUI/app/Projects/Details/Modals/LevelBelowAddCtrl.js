xPlanner.controller('LevelBelowAddCtrl', ['$scope', '$mdDialog', 'WebApiService', 'toastr', 'local', 'AuthService', 'ProgressService',
    function ($scope, $mdDialog, WebApiService, toastr, local, AuthService, ProgressService) {

        $scope.controllerParams = angular.extend(local.params, { controller: local.type + 's', domain_id: AuthService.getLoggedDomain() });

        $scope.data = {
            domain_id: AuthService.getLoggedDomain(),
            project_id: local.params.project_id,
            phase_id: local.params.phase_id,
            department_id: local.params.department_id
        };

        $scope.add = function () {

            $scope.addForm.$setSubmitted();

            if ($scope.addForm.$valid) {
                ProgressService.blockScreen();
                WebApiService.genericController.save({
                    controller: local.type + 's', action: 'Item', domain_id: AuthService.getLoggedDomain(),
                    project_id: local.params.project_id, phase_id: local.params.phase_id, department_id: local.params.department_id
                }, $scope.data,
                    function (data) {
                        ProgressService.unblockScreen()
                        toastr.success(local.type + ' saved');
                        $mdDialog.hide(data);
                    }, function () {
                        ProgressService.unblockScreen()
                        toastr.error('Error to save ' + local.type + ', please contact the technincal support');
                        $scope.cancel();
                    });
            } else {
                toastr.error('Make sure you enter all the required fields');
            }

        };

        $scope.close = function () {
            $mdDialog.cancel();
        };

    }]);