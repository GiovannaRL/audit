xPlanner.controller('EditAssetCodeCtrl', ['$scope', 'WebApiService', 'AuthService', '$mdDialog', 'toastr',
    'AssetsService', 'ProgressService',
    function ($scope, WebApiService, AuthService, $mdDialog, toastr, AssetsService, ProgressService) {


        $scope.controllerParams = {
            domain_id: AuthService.getLoggedDomain()
        }

        $scope.save = function () {
            if ($scope.addNewAssetForm.$valid) {
                if (typeof $scope.asset.asset_code === 'object') 
                    $mdDialog.hide($scope.asset.asset_code.prefix);
            }
        };

        $scope.close = function () {
            $mdDialog.cancel();
        };

    }]);