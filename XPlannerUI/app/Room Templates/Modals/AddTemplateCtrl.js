xPlanner.controller('AddTemplateCtrl', ['$scope', 'AuthService', '$mdDialog', 'toastr', 'WebApiService', 'ProgressService',
    function ($scope, AuthService, $mdDialog, toastr, WebApiService, ProgressService) {

        $scope.controllerParams = { domain_id: AuthService.getLoggedDomain() };

        var last;

        WebApiService.genericController.query({ controller: 'projects', action: 'All', domain_id: AuthService.getLoggedDomain() },
            function (projects) {
                $scope.projects = projects;
            });

        $scope.save = function (close) {
            $scope.addTemplateForm.$setSubmitted();

            if ($scope.addTemplateForm.$valid) {

                $scope.room_template.is_template = true;

                ProgressService.blockScreen();
                WebApiService.genericController.save({ controller: "templateRoom", action: "Item", domain_id: AuthService.getLoggedDomain() },
                    $scope.room_template, function (data) {
                        toastr.success('Template Added');
                        ProgressService.unblockScreen();
                        close ? $mdDialog.hide(data) : last = data;
                    }, function (error) {
                        error.status === 409 ? toastr.error(error.data + ' Please choose a different name.') :
                            toastr.error('Error to try add template, please contact the technical support');
                        ProgressService.unblockScreen();
                    });
            } else {
                toastr.error('Please make sure you enter all the required fields');
            }
        };

        $scope.close = function () {
            last ? $mdDialog.hide(last) : $mdDialog.cancel();
        };

    }]);