xPlanner.controller('AddVendorCtrl', ['$scope', 'WebApiService', '$mdDialog', 'toastr', 'AuthService', 'ProgressService',
    function ($scope, WebApiService, $mdDialog, toastr, AuthService, ProgressService) {

        var added = [];

        WebApiService.genericController.query({ controller: "vendor", action: "All", domain_id: AuthService.getLoggedDomain() },
            function (vendors) {
                $scope.anotherVendors = vendors.filter(function (item) { return item.domain_id == AuthService.getLoggedDomain() })
                    .map(function (item) { return item.name });

                //$scope.anotherVendors = vendors;
            });

        $scope.add = function (close) {

            $scope.addVendorForm.$setSubmitted();

            if ($scope.addVendorForm.$valid) {

                ProgressService.blockScreen();
                WebApiService.genericController.save({
                    controller: "vendor", action: "Item", domain_id: AuthService.getLoggedDomain()
                }, $scope.vendor,
                    function (data) {
                        toastr.success('vendor Added');
                        $scope.vendor = {};
                        $scope.addVendorForm.$setUntouched();
                        $scope.addVendorForm.$setPristine();

                        added.push(data);

                        ProgressService.unblockScreen();
                        if (close) {
                            $mdDialog.hide(added);
                        }
                    }, function (error) {
                        ProgressService.unblockScreen();
                        toastr.error('Error to try add vendor, please contact the technical support');
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