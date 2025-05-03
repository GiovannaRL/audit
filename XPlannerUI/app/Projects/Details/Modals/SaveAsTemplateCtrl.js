xPlanner.controller('SaveAsTemplateCtrl', ['$scope', '$mdDialog', 'WebApiService', 'local', 'AuthService', 'toastr',
    function ($scope, $mdDialog, WebApiService, local, AuthService, toastr) {

        local.params.domain_id = AuthService.getLoggedDomain();
        $scope.controllerParams = local.params;

        $scope.template = {};

        $scope.save = function () {
            $scope.saveAsTemplateForm.$setSubmitted();

            if ($scope.saveAsTemplateForm.$valid) {
                $scope.template.project_id_template = $scope.template.scope == 1 ? null : local.params.project_id;
                $scope.template.description = typeof $scope.template.saveAs === 'object' ? $scope.template.saveAs.description : $scope.template.saveAs;
                WebApiService.genericController.save(angular.extend(local.params, { controller: 'templateRoom', action: 'SaveRoom', domain_id: AuthService.getLoggedDomain() }),
                    $scope.template, function (data) {
                        toastr.success("Room saved as template");
                        $mdDialog.hide();
                    }, function (error) {
                        if (error.status == 409)
                            toastr.error(error.data);
                        else
                            toastr.error("Error to save template, please contact technical support");
                        $mdDialog.cancel();
                    });
            } else {
                toastr.error("Please make sure you enter all the required fields");
            }
        };

        $scope.close = function () {
            $mdDialog.cancel();
        }

    }]);