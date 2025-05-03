xPlanner.controller('SaveChangesCtrl', ['$scope', '$mdDialog', function ($scope, $mdDialog) {

    $scope.confirm = function (saveChanges) {

        $mdDialog.hide(saveChanges);
    };

    $scope.cancel = function () {

        $mdDialog.cancel();

    };

}]);