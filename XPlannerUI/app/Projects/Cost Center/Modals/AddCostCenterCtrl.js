xPlanner.controller('AddCostCenterCtrl', ['$scope', '$mdDialog', 'toastr', 'local',
    function ($scope, $mdDialog, toastr, local) {

        $scope.anotherCostCenters = local.items;

        $scope.edit = local.costCenter ? true : false;
        $scope.costCenter = angular.copy(local.costCenter) || {};

        $scope.anotherCostCenters = local.items.filter(function (item) {
            return item.id != $scope.costCenter.id || item.domain_id != local.params.domain_id || item.project_id != local.params.project_id;
        });

        $scope.add = function () {

            $scope.addCostCenterForm.$setSubmitted();

            if ($scope.addCostCenterForm.$valid) {
                $mdDialog.hide($scope.costCenter);
            } else {
                toastr.error("Please make sure you entered correctly all the fields");
            }
        };

        $scope.close = function () {
            $mdDialog.cancel();
        };
    }]);