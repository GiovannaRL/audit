xPlanner.controller('ReceiveQuoteCtrl', ['$scope', '$mdDialog', 'local', 'WebApiService', 'ProgressService', 'toastr',
    function ($scope, $mdDialog, local, WebApiService, ProgressService, toastr) {

        //$scope.po = angular.copy(local.po);
        $scope.po = { quote_requested_date: new Date(local.po.quote_requested_date),/* tax: 0.0, freight: 0.0, misc: 0.0, warranty: 0.0,*/ quote_amount: 0.0 };

        $scope.save = function () {

            $scope.receiveQuoteForm.$setSubmitted();
            $scope.receiveQuoteForm.quote_received_date.$setTouched();

            if ($scope.receiveQuoteForm.$valid) {
                ProgressService.blockScreen();
                WebApiService.genericController.update({
                    controller: 'PurchaseOrders', action: 'ReceiveQuote',
                    domain_id: local.po.domain_id, project_id: local.po.project_id, phase_id: local.po.po_id
                }, $scope.po, function (data) {
                    toastr.success('Quote received');
                    ProgressService.unblockScreen();
                    $mdDialog.hide(data);
                }, function () {
                    toastr.error('Error to try receive quote, please contact the technical support');
                    ProgressService.unblockScreen();
                });
            } else {
                toastr.error('Please make sure you entered correctly all the fields');
            }
        };

        $scope.close = function () {
            $mdDialog.cancel();
        };

    }]);