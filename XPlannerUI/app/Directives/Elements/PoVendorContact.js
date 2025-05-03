xPlanner.directive('awPoVendorContact', ['WebApiService', function (WebApiService) {

    return {
        restrict: 'E',
        scope: {
            data: '='
        },
        link: function (scope, elem, attrs, ctrl) {

        },
        templateUrl: 'app/Directives/Elements/POVendorContact.html'
    };

}]);