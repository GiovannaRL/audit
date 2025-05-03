xPlanner.controller('ReceivePOCtrl', ['$scope', '$mdDialog', 'ProgressService', 'WebApiService', 'toastr', 'local',
    function ($scope, $mdDialog, ProgressService, WebApiService, toastr, local) {

        $scope.po = { po_requested_date: local.po.po_requested_date };

        $scope.save = function () {

            $scope.receivePOForm.$setSubmitted();
            $scope.receivePOForm.po_received_date.$setTouched();

            if ($scope.receivePOForm.$valid) {
                ProgressService.blockScreen();
                WebApiService.genericController.update({
                    controller: 'PurchaseOrders', action: 'ReceivePO',
                    domain_id: local.po.domain_id, project_id: local.po.project_id, phase_id: local.po.po_id
                }, $scope.po, function (data) {
                    toastr.success('PO Received');
                    ProgressService.unblockScreen();
                    $mdDialog.hide(data);
                }, function () {
                    toastr.error('Error to try receive PO, please contact the technical support');
                    ProgressService.unblockScreen();
                });

            } else {
                toastr.error('Please make sure you entered all the required fields');
            }

        };

        $scope.close = function () {
            $mdDialog.cancel();
        };

    }]);