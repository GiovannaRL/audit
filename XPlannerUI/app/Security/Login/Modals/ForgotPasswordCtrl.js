xPlanner.controller('ForgotPasswordCtrl', ['$scope', '$mdDialog', 'WebApiService', 'toastr', 'ProgressService',
    function ($scope, $mdDialog, WebApiService, toastr, ProgressService) {

        $scope.data = null;

        $scope.send = function () {

            $scope.forgotPasswordForm.$setSubmitted();

            if ($scope.forgotPasswordForm.$valid) {
                ProgressService.blockScreen();
                WebApiService.genericController.save({ controller: 'account', action: 'forgotPassword' }, { email: $scope.data }, function () {
                    ProgressService.unblockScreen();
                    toastr.success('An email have been sent to you with the instructions');
                    $scope.close();
                }, function (error) {
                    ProgressService.unblockScreen();
                    if (error.status === 404) {
                        toastr.error('Username not found int the system');
                    } else {
                        toastr.error('Error sending email, please try again. If the error persist, please contact the technical support.');
                    }
                });
            } else {
                toastr.error('Please make sure you entered all the required fields');
            }
        };

        $scope.close = function () {
            $mdDialog.cancel();
        };
    }]);