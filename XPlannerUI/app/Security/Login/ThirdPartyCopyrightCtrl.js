xPlanner.controller('ThirdPartyCopyrightCtrl', ['$scope', '$mdDialog', 'WebApiService', 'toastr', 'ProgressService',
    function ($scope, $mdDialog, WebApiService, toastr, ProgressService) {

       
        $scope.close = function () {
            $mdDialog.cancel();
        };

}]);