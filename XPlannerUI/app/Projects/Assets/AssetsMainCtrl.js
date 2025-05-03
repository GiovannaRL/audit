xPlanner.controller('AssetsMainCtrl', ['$scope', '$stateParams', 'WebApiService', 'AuthService',
    function ($scope, $stateParams, WebApiService, AuthService  ) {

    $scope.$emit('detailsParams', angular.copy($stateParams));

    $scope.params = $stateParams;

    $scope.emitButtons = function (buttons) {
        $scope.buttons = buttons;
    };

    if (!$scope.params.room_id) {
        $scope.linked_room = false;
    } else {
        WebApiService.genericController.get(angular.extend({ controller: 'rooms', action: 'Item', domain_id: AuthService.getLoggedDomain() }, $scope.params), function (data) {
            $scope.linked_room = data.linked_template == true;
        });
    }

}]);