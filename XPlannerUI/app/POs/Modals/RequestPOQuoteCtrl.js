xPlanner.controller('RequestPOQuoteCtrl', ['$scope', '$mdDialog', 'toastr', 'local', 'ProgressService', 'WebApiService',
    function ($scope, $mdDialog, toastr, local, ProgressService, WebApiService) {

        $scope.type = local.type.toLowerCase();
        $scope.po = { date: null };

        $scope.minDate = $scope.type === 'po' && local.po.quote_received_date ? new Date(local.po.quote_received_date) : new Date(0, 0, 0);

        $scope.save = function () {

            $scope.requestPOForm.$setSubmitted();
            $scope.requestPOForm.requested_date.$setTouched();

            if ($scope.requestPOForm.$valid) {
                ProgressService.blockScreen();

                $scope.po[$scope.type + '_requested_date'] = $scope.po.date;

                WebApiService.genericController.update({
                    controller: 'PurchaseOrders', action: 'Request' + local.type,
                    domain_id: local.po.domain_id, project_id: local.po.project_id, phase_id: local.po.po_id
                }, $scope.po, function (data) {
                    toastr.success(local.type + ' requested');
                    ProgressService.unblockScreen();
                    $mdDialog.hide(data);
                }, function () {
                    toastr.error('Error to try request PO, please contact the technical support');
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