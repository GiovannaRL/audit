xPlanner.directive('awMatch', ['$parse', function ($parse) {

    var directive = {
        link: link,
        restrict: 'A',
        require: '?ngModel'
    };
    return directive;

    function link(scope, elem, attrs, ctrl) {

        // if ngModel is not defined, we don't need to do anything
        if (!ctrl) return;
        if (!attrs['awMatch']) return;

        var firstPassword = $parse(attrs['awMatch']);

        var validator = function (value) {
            var temp = firstPassword(scope),
            v = value === temp;
            ctrl.$setValidity('match', v);
            return value;
        }

        ctrl.$parsers.unshift(validator);
        ctrl.$formatters.push(validator);
        attrs.$observe('awMatch', function () {
            validator(ctrl.$viewValue);
        });

    }
}]);