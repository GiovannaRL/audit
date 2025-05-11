xPlanner.directive('awPassword', ['$parse', function ($parse) {

    var directive = {
        link: link,
        restrict: 'A',
        require: '?ngModel'
    };
    return directive;

    function link(scope, elem, attrs, ctrl) {

        // if ngModel is not defined, we don't need to do anything
        if (!ctrl) return;

        var v = false;

        var validator = function (value) {
            
            v = false;

            reg = /[a-z]/;
            if (reg.test(value)) {
                reg = /[A-Z]/;
                if (reg.test(value)) {
                    reg = /[0-9]/;
                    if (reg.test(value)) {
                        reg = /[^a-zA-Z0-9\s]/;
                        v = reg.test(value);
                    }
                }
            }

            ctrl.$setValidity('password', v);
            return value;
        }

        ctrl.$parsers.unshift(validator);
        ctrl.$formatters.push(validator);
        attrs.$observe('awPassword', function () {
            validator(ctrl.$viewValue);
        });

    }
}]);