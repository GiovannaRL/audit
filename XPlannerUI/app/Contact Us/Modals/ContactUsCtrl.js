xPlanner.controller('ContactUsCtrl', ['$scope', '$mdDialog', 'WebApiService', 'toastr', 'ProgressService',
    function ($scope, $mdDialog, WebApiService, toastr, ProgressService) {

        $scope.send = function () {

            $scope.contactUsForm.$setSubmitted();

            if ($scope.contactUsForm.$valid) {
                ProgressService.blockScreen();
                WebApiService.genericController.save({ controller: 'contactUs', action: 'Item' }, $scope.data, function () {
                    ProgressService.unblockScreen();
                    toastr.success('Email sent successfully');
                    $scope.close();
                }, function () {
                    ProgressService.unblockScreen();
                    toastr.error('Error sending email, please try again. If the error persist, please contact the technical support.');
                });

            } else {
                toastr.error('Make sure you entered all the required fields');
            }

        };

        $scope.close = function () {
            $mdDialog.cancel();
        };
    }]);