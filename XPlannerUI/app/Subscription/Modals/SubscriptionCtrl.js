xPlanner.controller('SubscriptionCtrl', ['$scope', '$state', '$mdDialog', 'WebApiService', 'toastr', 'ProgressService',
    function ($scope, $state, $mdDialog, WebApiService, toastr, ProgressService) {

        $scope.send = function () {
            $scope.requestSubscriptionForm.$setSubmitted();

            if ($scope.requestSubscriptionForm.$valid) {
                ProgressService.blockScreen();
                WebApiService.genericController.save({controller: 'Subscription', action: 'Item'}, $scope.data,
                    function () {
                        ProgressService.unblockScreen();
                        toastr.success('Email sent successfully');
                        $state.go('notice');
                        

                    }, function () {
                        ProgressService.unblockScreen();
                        toastr.error('Error sending email, please try again. If the error persist, please contact the technical support.');
                    });
            } else {
                toastr.error('Please make sure you entered all the required fields');
            }
        }

        $scope.close = function () {
            $mdDialog.cancel();
        };
    }]);