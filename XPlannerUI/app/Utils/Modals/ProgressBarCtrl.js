xPlanner.controller('ProgressBarCtrl', ['$scope', '$mdDialog', 'local', function ($scope, $mdDialog, local) {
    $scope.progress = local.percentage;
    $scope.close = function () {
        $mdDialog.cancel();
    };
}]);