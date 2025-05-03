xPlanner.controller('CloneTemplateCtrl', ['$scope', '$mdDialog', 'toastr', 'WebApiService', 'AuthService', 'local',
        'ProgressService',
    function ($scope, $mdDialog, toastr, WebApiService, AuthService, local, ProgressService) {

        $scope.projectParams = { domain_id: AuthService.getLoggedDomain() };

        $scope.save = function () {

            $scope.cloneTemplateForm.$setSubmitted();

            if ($scope.cloneTemplateForm.$valid) {
                ProgressService.blockScreen();
                WebApiService.genericController.save(angular.extend(local.params, {
                    controller: 'templateRoom', action: 'Clone', domain_id: AuthService.getLoggedDomain(),
                }), $scope.room_template, function (new_template) {
                    toastr.success('Template cloned');
                    ProgressService.unblockScreen();
                    $mdDialog.hide(new_template);
                }, function (error) {
                    ProgressService.unblockScreen();
                    if (error.status === 409) {
                        toastr.error(error.data + ' Please choose a different name.');
                    } else {
                        toastr.error('Error to try clone template, please contact the technical support');
                        $scope.close();
                    }
                });
            } else {
                toastr.error('Please make sure you enter all the required fields');
            }

        };

        $scope.close = function () {
            $mdDialog.cancel();
        };
    }]);