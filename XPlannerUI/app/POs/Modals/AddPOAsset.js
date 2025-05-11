xPlanner.controller('AddPOAsset', ['$scope', 'GridService', 'AuthService', 'HttpService', 'toastr', '$mdDialog',
    '$mdMedia', 'WebApiService', 'local', 'ProgressService', '$timeout',
    function ($scope, GridService, AuthService, HttpService, toastr, $mdDialog, $mdMedia, WebApiService, local, ProgressService, $timeout) {


        $scope.show_unapproved = false;
        $scope.showOnlyApproved = true;
        $scope.params = local.parasms;
        $scope.alreadyAssociated = [{ inventory_id: 0 }];


        $scope.changeGrid = function () {

            //This was the way i found to reload the directive
            $scope.showOnlyApproved = !$scope.show_unapproved;
            $scope.$broadcast("refreshGrid", { showOnlyApproved: $scope.showOnlyApproved });
        };

        /* Close modal */
        $scope.close = function () {
            $mdDialog.cancel();
        };

        /* Add assets and close modal */
        $scope.add = function () {

            if ($scope.selectedInventory.length > 0) {
                const inventoriesList = [...new Map($scope.selectedInventory.map(item => [item.inventory_id, item])).values()];
                ProgressService.blockScreen();

                WebApiService.genericController.save({
                    controller: 'InventoryPurchaseOrder', action: 'Item', domain_id: local.params.domain_id,
                    project_id: local.params.project_id, phase_id: local.params.po_id
                }, inventoriesList, function () {
                    toastr.success('Assets added');
                    ProgressService.unblockScreen();
                    $mdDialog.hide();
                }, function () {
                    toastr.error('Error to add one or more assets, please contact the technical support');
                    ProgressService.unblockScreen();
                });
            }
        };

    }]);