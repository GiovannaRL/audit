xPlanner.controller('AddAssetCodeCtrl', ['$scope', 'WebApiService', 'AuthService', '$mdDialog', 'toastr',
    function ($scope, WebApiService, AuthService, $mdDialog, toastr) {

        $scope.anotherCodes = [];

        function GetControllerParams(action) {
            return {
                controller: "AssetCodes",
                action: action,
                domain_id: AuthService.getLoggedDomain()
            };
        };

        WebApiService.genericController.query(GetControllerParams("All"), function (codes) {
            $scope.anotherCodes = codes;
        });

        $scope.add = function (close) {

            $scope.addCodeForm.$setSubmitted();

            if ($scope.addCodeForm.$valid) {
                WebApiService.genericController.save(GetControllerParams("Item"), $scope.code, function (code) {
                    toastr.success("Code Added");
                    $scope.code = {};
                    $scope.addCodeForm.$setUntouched();
                    $scope.addCodeForm.$setPristine();
                    if (close) {
                        $mdDialog.hide();
                    } else {
                        $scope.anotherCodes.push(code);
                    }
                }, function () {
                    $mdDialog.cancel();
                });
            } else {
                toastr.error("Make sure you have entered all the fields correctly");
            }
        };

        $scope.close = function () {
            $mdDialog.cancel();
        };

    }]);