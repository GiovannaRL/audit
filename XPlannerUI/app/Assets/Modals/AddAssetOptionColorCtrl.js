xPlanner.controller('AddAssetOptionColorCtrl', ['$scope', '$mdDialog', 'toastr', 'local', '$filter',
    function ($scope, $mdDialog, toastr, local, $filter) {

        $scope.optionCtrl = {};
        $scope.params = local.params;

        $scope.option = local.option;
        $scope.isEdit = local.option ? true : false;

        $scope.saveOption = function () {
            $scope.optionCtrl.saveOption().then(function () {
                $mdDialog.hide();
            }, function (err) { console.log(err); });
        }

        $scope.close = function () {
            $mdDialog.cancel();
        }
    }]);