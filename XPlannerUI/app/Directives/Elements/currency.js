xPlanner.directive('awCurrency', ['$interpolate', function ($interpolate) {
    return {
        link: function (scope, elem, attrs, ctrl, transclusionFn) {

            
            if (scope.value == 0 || scope.value) scope.valueView = kendo.toString(Number(scope.value), 'n2');
        },
        restrict: 'E',
        //transclude: true,
        scope: {
            value: '='
        },
        templateUrl: 'app/Directives/Elements/currency.html'
    };
}]);