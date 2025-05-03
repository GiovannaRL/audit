xPlanner.controller('ToastrCtrl', ['$scope', '$mdToast', function ($scope, $mdToast) {
    $scope.close = function () {
        $mdToast.hide();
    };
}]);