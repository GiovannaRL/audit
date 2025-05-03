xPlanner.controller('AddEditAssetCodeCtrl', ['$scope', 'StatesList', 'toastr', '$mdDialog', 'local',
    function ($scope, StatesList, toastr, $mdDialog, local) {

        $scope.statesList = StatesList;

        $scope.saveButtonText = local.assetcode ? 'Update' : 'Add';
        $scope.assetcode = angular.copy(local.assetcode) || {};

        if ($scope.assetcode == 'Add') {
            $scope.assetcode.next_seq = 1;
        }

        $scope.anotherAssetCodes = local.items.filter(function (item) {
            return item.prefix != $scope.assetcode.prefix || item.domain_id != local.params.domain_id ;
        });

        $scope.add = function () {

            alert('Under Construction');
            //$scope.addAssetCodeForm.$setSubmitted();

            //if ($scope.addAssetCodeForm.$valid) {

            //    $mdDialog.hide($scope.address);
            //} else {
            //    toastr.error("Please make sure you entered correctly all the fields");
            //}
        };

        $scope.close = function () {
            $mdDialog.cancel();
        };
    }]);