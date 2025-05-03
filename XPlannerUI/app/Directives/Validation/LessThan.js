xPlanner.directive('awLessThan', ['$parse', function ($parse) {
    var directive = {
        link: link,
        restrict: 'A',
        require: '?ngModel',
        scope: {
            allowEqual: '='
        }
    };
    return directive;

    function link(scope, elem, attrs, ctrl) {

        // if ngModel is not defined, we don't need to do anything
        if (!ctrl) return;
        if (!attrs['awLessThan']) return;

        

        var validator = function (value) {
            var temp = $parse(attrs['awLessThan'])(scope);

            ctrl.$setValidity('less-than', value < temp || (scope.allowEqual && value == temp));
            return value;
        }

        ctrl.$parsers.unshift(validator);
        ctrl.$formatters.push(validator);
        attrs.$observe('awLessThan', function () {
            validator(ctrl.$viewValue);
        });
    }
}]);