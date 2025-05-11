xPlanner.controller('EditAssetGlobalCtrl', ['$scope', 'local', 'HttpService', 'ProgressService', 'WebApiService', 'toastr',
        '$mdDialog', 'AuthService',
    function ($scope, local, HttpService, ProgressService, WebApiService, toastr, $mdDialog, AuthService) {

        $scope.multiple = local.multiple;

        function downloadFile(filename, asset_domain_id) {
            return HttpService.generic('filestream', 'file', asset_domain_id, filename, 'photo');
        };

        // Get the asset image
        if (!local.multiple)
            $scope.image = downloadFile(local.assets[0].photo, local.assets[0].asset_domain_id);

        // get the inventory ids
        if (local.multiple) {
            var inventory_ids = [];
            var id = local.assets[0].asset_id;
            if (local.assets[0].consolidated_view == 1) { //consolidated

                if (local.assets.length == 1)
                    $scope.data = angular.copy(local.assets[0]);

                angular.forEach(local.assets, function (asset) {
                    inventory_ids = inventory_ids.concat(asset.inventory_id.split(','));
                });
            } else { // not consolidated
                inventory_ids = local.assets.map(function (asset) { return asset.inventory_id; });
            }
        } else {
            $scope.data = angular.copy(local.assets[0]);
        }

        // Save the edited asset(s)
        function GetParams() {

            var params = angular.copy(local.params);
            params.domain_id = AuthService.getLoggedDomain();
            params.action = local.multiple ? "All" : "EditSingle";

            return params;
        };

        function GetBody() {
            return local.multiple ? { edited_data: $scope.data, inventories: inventory_ids } : $scope.data;
        }

        $scope.save = function () {
            $scope.editAssetForm.$setSubmitted();

            if ($scope.editAssetForm.$valid) {

                ProgressService.blockScreen();

                WebApiService.asset_inventory.update(GetParams(), GetBody(), function (data) {
                    ProgressService.unblockScreen();
                    $mdDialog.hide();
                }, function () {
                    ProgressService.unblockScreen();
                    toastr.error("Error to save asset(s), please contact technical support");
                    $mdDialog.cancel();
                });
            } else {
                toastr.error('Please make sure you enter the quantity');
            }
        }

        // Close modal
        $scope.close = function () {
            $mdDialog.cancel();
        };

    }]);