xPlanner.controller('ResetPasswordCtrl', ['$scope', '$stateParams', 'AuthService', '$state', 'WebApiService', 'toastr', '$timeout',
        'ProgressService',
    function ($scope, $stateParams, AuthService, $state, WebApiService, toastr, $timeout, ProgressService) {

        if (AuthService.isAuthenticated()) {

            $state.go('index');
            return;
        }

        $scope.data = { email: $stateParams.email, token: $stateParams.token };

        $scope.reset = function () {

            $scope.resetPasswordForm.$setSubmitted();

            if ($scope.resetPasswordForm.$valid) {
                ProgressService.blockScreen();
                WebApiService.genericController.save({ controller: 'account', action: 'ResetPassword' }, $scope.data,
                    function () {
                        toastr.success('Your password has been reset successfully! You will be redirected to the login page in 2 seconds.');
                        $timeout(function () {
                            ProgressService.unblockScreen();
                            $state.go('login');
                        }, 2000);
                    }, function (error) {
                        ProgressService.unblockScreen();
                        if (error.status === 404 || (error.status === 400 && error.data.message === 'Invalid token.')) {
                            toastr.error('Error to reset password. The link has expired.');
                        } else {
                            toastr.error('Error to reset password. Please contact the technical support.');
                        }
                    });
            } else {
                toastr.error('Please make sure you entered all the required fields');
            }

        };
    }]);