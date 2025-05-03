xPlanner.controller('AddPOCtrl', ['$scope', 'AuthService', '$mdDialog', 'toastr', 'WebApiService', 'local', 'ProgressService',
    function ($scope, AuthService, $mdDialog, toastr, WebApiService, local, ProgressService) {

        $scope.vendorCtrlParams = { domain_id: AuthService.getLoggedDomain() };

        $scope.data = {
            project_id: local.params.project_id, phase_id: local.params.phase_id, department_id:
                local.params.department_id, room_id: local.params.room_id
        }

        $scope.add = function () {

            $scope.addPOForm.$setSubmitted();

            if ($scope.addPOForm.$valid) {
                ProgressService.blockScreen();
                WebApiService.genericController.save({
                    controller: 'PurchaseOrders', action: 'Item', domain_id: AuthService.getLoggedDomain(), project_id: local.params.project_id
                }, $scope.data, function (data) {
                    toastr.success('Purchase Order Added');
                    ProgressService.unblockScreen();
                    $mdDialog.hide(data);
                }, function () {
                    toastr.error('Error to add purchase order, please contact the technical support');
                    ProgressService.unblockScreen();
                });
            } else {
                toastr.error('Please make sure you enter all the required fields');
            }
        };

        $scope.close = function () {
            $mdDialog.cancel();
        };

    }]);