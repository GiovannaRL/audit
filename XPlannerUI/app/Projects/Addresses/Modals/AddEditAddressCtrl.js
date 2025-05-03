xPlanner.controller('AddEditAddressCtrl', ['$scope', 'StatesList', 'toastr', '$mdDialog', 'local',
    function ($scope, StatesList, toastr, $mdDialog, local) {

        $scope.statesList = StatesList;

        $scope.saveButtonText = local.address ? 'Update' : 'Add';
        $scope.address = angular.copy(local.address) || {};

        $scope.anotherAddresses = local.items.filter(function (item) {
            return item.id != $scope.address.id || item.domain_id != local.params.domain_id || item.project_id != local.params.project_id;
        });

        $scope.add = function () {

            $scope.addAddressForm.$setSubmitted();

            if ($scope.addAddressForm.$valid) {

                $mdDialog.hide($scope.address);
            } else {
                toastr.error("Please make sure you entered correctly all the fields");
            }
        };

        $scope.close = function () {
            $mdDialog.cancel();
        };
    }]);