xPlanner.controller('AssignVendorCtrl', ['$scope', 'AuthService', 'local', 'toastr', 'WebApiService', '$mdDialog',
        'ProgressService',
    function ($scope, AuthService, local, toastr, WebApiService, $mdDialog, ProgressService) {

        $scope.edit = local.edit;
        $scope.asset_id = local.asset.asset_id;

        $scope.controllerParams = {
            domain_id: AuthService.getLoggedDomain(), project_id: local.asset.domain_id, phase_id: local.asset.asset_id
        };

        var costs = {};

        function copyCosts(source, destination) {

            destination['max_cost'] = source['max_cost'];
            destination['min_cost'] = source['min_cost'];
            destination['last_cost'] = source['last_cost'];
            destination['avg_cost'] = source['avg_cost'];

            return destination;
        }

        if (local.edit && local.vendor) {
            local.vendor.name = local.vendor.vendor.name;
            $scope.asset_vendor = local.vendor;
            $scope.model_number = local.vendor.model_number;
            $scope.asset_vendor.domain_id = $scope.asset_vendor.vendor_domain_id;
            $scope.controllerParams.department_id = local.vendor.vendor_domain_id;
            $scope.controllerParams.room_id = local.vendor.vendor_id;

            costs = copyCosts($scope.asset_vendor, costs);
        }

        $scope.assign = function () {
            $scope.assignVendorForm.$setSubmitted();

            if ($scope.assignVendorForm.$valid) {
                $scope.asset_vendor.asset_id = local.asset.asset_id;
                $scope.asset_vendor.asset_domain_id = local.asset.domain_id;
                $scope.asset_vendor.vendor_domain_id = $scope.asset_vendor.domain_id;
                $scope.asset_vendor.model_number = $scope.model_number;
                $scope.asset_vendor = copyCosts(costs, $scope.asset_vendor);

                ProgressService.blockScreen();
                WebApiService.genericController[local.edit ? 'update' : 'save']({
                    controller: 'assetsVendor', action: 'Item', domain_id: local.asset.domain_id,
                    project_id: local.asset.asset_id,
                    phase_id: $scope.asset_vendor.vendor_domain_id,
                    department_id: $scope.asset_vendor.vendor_id
                }, $scope.asset_vendor, function (data) {
                    $scope.asset_vendor.vendor = { name: $scope.asset_vendor.name };
                    local.edit ? toastr.success("Vendor Updated") : toastr.success("Vendor Added");
                    ProgressService.unblockScreen();
                    $scope.close(true, $scope.asset_vendor);
                }, function (error) {
                    if (local.edit && error.status === 409) {
                        toastr.error('The vendor could not be deleted -- purchase orders have been issued');
                    } else {
                        toastr.error('Error to assign vendor to asset, please contact the technical support');
                    }
                    ProgressService.unblockScreen();
                });
            } else {
                toastr.error("Please make sure you enter all the required fields");
            }
        };

        $scope.close = function (hide, data) {
            hide ? $mdDialog.hide(data) : $mdDialog.cancel();
        };
    }]);