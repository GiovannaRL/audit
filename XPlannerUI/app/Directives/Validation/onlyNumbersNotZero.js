xPlanner.directive('awOnlyNumbersNotZero', function () {
    var directive = {
        link: link,
        restrict: 'A',
        require: '?ngModel'
    };
    return directive;

    function link(scope, elem, attrs, ctrl) {

        // if ngModel is not defined, we don't need to do anything
        if (!ctrl) return;

        var validator = function (value) {

            // regular expression which verifies not numbers
            var reg = /[^0-9]/g;

            var v = true;
            if (value) {
                v = !(reg.test(value));
            }

            if (v == true && value == 0) 
                v = false;

            ctrl.$setValidity('numberNotZero', v);
            return value;
        }

        ctrl.$parsers.unshift(validator);
        ctrl.$formatters.push(validator);
        attrs.$observe('awOnlyNumbersNotZero', function () {
            validator(ctrl.$viewValue);
        });
    }
});