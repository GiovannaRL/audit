xPlanner.controller('ChangePasswordCtrl', ['$scope', '$mdDialog', 'WebApiService', 'toastr', 'ProgressService',
    function ($scope, $mdDialog, WebApiService, toastr, ProgressService) {

        $scope.change = function () {

            $scope.changePasswordForm.$setSubmitted();

            if ($scope.changePasswordForm.$valid) {
                ProgressService.blockScreen();
                WebApiService.genericController.save({ controller: 'account', action: 'ChangePassword' }, $scope.data,
                    function () {
                        toastr.success('Password changed');
                        ProgressService.unblockScreen();
                        $mdDialog.hide();
                    }, function (error) {
                        ProgressService.unblockScreen();
                        if (error.data.modelState[""][0] === 'Incorrect password.') {
                            toastr.error('The currenct password is incorrect.');
                        } else {
                            toastr.error('Error to change password, please contact the technical support.');
                        }
                    });

            } else {
                toastr.error('Make sure you entered correctly all the fields');
            }

        };

        $scope.close = function () {
            $mdDialog.cancel();
        };

}]);