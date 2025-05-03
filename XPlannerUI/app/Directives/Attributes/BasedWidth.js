xPlanner.directive('awBasedWidth', [function () {
    return {
        restrict: 'A',
        scope: {
            widthElementId: '@'
        },
        link: function (scope, element, attrs) {
            
            if (scope.widthElementId && scope.widthElementId != "") {

                var left = angular.element('#' + scope.widthElementId)[0].getBoundingClientRect().left;
                element[0].style.left = (left - 285) + "px"
            }
        }
    }
}]);