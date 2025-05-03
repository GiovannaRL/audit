xPlanner.controller('CopyProjectCtrl', ['$scope', '$mdDialog', 'AuthService', 'local', 'WebApiService', 'toastr', 'ProgressService',
    function ($scope, $mdDialog, AuthService, local, WebApiService, toastr, ProgressService) {
        
        var today = new Date();
        $scope.project_name = local.project_description + ' ' + today.getFullYear() + '-' + (today.getMonth()+1) + '-' + today.getDate();
        $scope.params = local.params;

        var getParams = function (data, action) {
            return {
                controller: data,
                action: action,
                domain_id: AuthService.getLoggedDomain(),
                project_id: $scope.params.project_id,
                phase_id: $scope.copyUsers
            };
        };


        $scope.copy = function () {

            $scope.copyProjectForm.$setSubmitted();

            if ($scope.copyProjectForm.$valid) {
                ProgressService.blockScreen();
                WebApiService.genericController.save(getParams("Projects", "CopyProjectAsync"), { text: $scope.project_name }, function (data) {
                    toastr.success('Copy project process has started, you will be notified when it finishes');
                    ProgressService.unblockScreen();
                    $mdDialog.hide();
                }, function (error) {
                    ProgressService.unblockScreen();
                    toastr.error('Error to intialize copy project. The server might be too busy. Please contact technical support or try again later.');
                    //$mdDialog.cancel();
                });
            } else {
                toastr.error("Please make sure you entered correctly all the fields");
            }

        };

        $scope.close = function () {
            $mdDialog.cancel();
        };
    }]);