xPlanner.directive('awAccordion', [function () {
    return {
        restrict: 'E',
        transclude:true,
        scope: {
            label: '@',
            expanded: '=',
        },
        templateUrl: function (elem, attrs) {
            if (attrs.expanded && eval(attrs.expanded))
                return 'app/Directives/Elements/accordionExpanded.html'
            else
                return 'app/Directives/Elements/accordion.html'
        }
    }
}]);