xPlanner.controller('AddJSNCtrl', ['$scope', 'WebApiService', 'AuthService', '$mdDialog', 'toastr',
    'AssetsService', 'ProgressService', 'local',
    function ($scope, WebApiService, AuthService, $mdDialog, toastr, AssetsService, ProgressService, local) {

        $scope.asset = { jsn: { jsn_code: local.jsn_code } };

        $scope.save = function () {

            $scope.addNewJSNForm.$setSubmitted();
            if ($scope.addNewJSNForm.$valid) {
                ProgressService.blockScreen();
                $scope.asset.jsn.asset_code = 'EQP';
                $scope.asset.jsn.asset_code_domain_id = 1;
                WebApiService.genericController.save({ controller: 'JSN', action: 'Item', domain_id: AuthService.getLoggedDomain() }, $scope.asset.jsn, function (data) {
                    ProgressService.unblockScreen();
                    toastr.success('JSN added sucessfully.');
                    $mdDialog.hide(data);
                }, function () {
                    ProgressService.unblockScreen();
                    toastr.error('Error trying to save JSN, please contact the technical support');
                });

            } else {
                toastr.error('Please make sure you enter all the required fields');
            }
        };

        $scope.close = function () {
            $mdDialog.cancel();
        };

    }]);