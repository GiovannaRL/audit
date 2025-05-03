xPlanner.controller('AddManufacturerCtrl', ['$scope', 'WebApiService', 'AuthService', '$mdDialog', 'toastr', 'ProgressService',
    function ($scope, WebApiService, AuthService, $mdDialog, toastr, ProgressService) {

        var added = [];

        WebApiService.genericController.query({ controller: "manufacturer", action: "All", domain_id: AuthService.getLoggedDomain() },
            function (manufacturers) {

                $scope.anotherManufacturers = manufacturers.filter(function (item) { return item.domain_id == AuthService.getLoggedDomain() })
                    .map(function (item) { return item.manufacturer_description });
            });

        $scope.add = function (close) {

            $scope.addManufacturerForm.$setSubmitted();

            if ($scope.addManufacturerForm.$valid) {

                ProgressService.blockScreen();
                WebApiService.genericController.save({
                    controller: "manufacturer", action: "Item", domain_id: AuthService.getLoggedDomain()
                }, $scope.manufacturer,
                    function (data) {
                        toastr.success('Manufacturer Added');
                        $scope.manufacturer = {};
                        $scope.addManufacturerForm.$setUntouched();
                        $scope.addManufacturerForm.$setPristine();
                        
                        added.push(data);
                        ProgressService.unblockScreen();

                        if (close) {
                            $mdDialog.hide(added);
                        }
                    }, function (error) {
                        ProgressService.unblockScreen();
                        toastr.error('Error to try add manufacturer, please contact the technical support');
                    }
                );
            } else {
                toastr.error("Please make sure you enter correctly all the fields");
            }
        };

        $scope.close = function () {
            added.length > 0 ? $mdDialog.hide(added) : $mdDialog.cancel();
        };

    }]);