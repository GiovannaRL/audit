xPlanner.directive('downloadFile', ['$http', function ($http) {
    return {
        restrict: 'E',
        templateUrl: 'app/Directives/Elements/downloadFile.html',
        scope: {
            url: '@',
            filename: '@'
        },
        transclude: true,
        link: function (scope, element, attr, ctrl, transclude) {

            transclude(scope.$parent, function (clone, scope) {
                element.find("a").append(clone);
            });

        }
    }
}]);