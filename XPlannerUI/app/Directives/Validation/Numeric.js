xPlanner.directive('awNumeric', ['$parse', function ($parse) {
    var directive = {
        link: link,
        restrict: 'A',
        require: '?ngModel'
    };
    return directive;

    function link(scope, elem, attrs, ctrl) {

        // if ngModel is not defined, we don't need to do anything
        if (!ctrl) return;

        if (attrs['awNumeric']) {
            var decimalDigits = $parse(attrs['awNumeric']);
            decimalDigits = decimalDigits(scope);
        }

        var positive = attrs['positive'] != undefined;
        var negative = attrs['negative'] != undefined;

        var sinal = negative ? '\\-' + (positive ? '?' : '') : (positive ? '' : '\\-?');

        var validator = function (value) {

            // regular expression which verifies not numbers
            var reg = new RegExp('^' + sinal + '[0-9]+(\.[0-9]' + (decimalDigits ? '{1,' + decimalDigits + '})' : '+)') + '?$');

            var v = true;
            if (value) {
                v = reg.test(value);

                if (!v && decimalDigits) {
                    reg = /^[0-9]+(\.[0-9]+)?$/;
                    v = reg.test(value);
                    ctrl.$setValidity('numeric', v);
                    ctrl.$setValidity('numeric-decimal', !v);
                    return value;
                }
            }

            ctrl.$setValidity('numeric-decimal', true);
            ctrl.$setValidity('numeric', v);
            return value;
        }

        ctrl.$parsers.unshift(validator);
        ctrl.$formatters.push(validator);
        attrs.$observe('awNumeric', function () {
            validator(ctrl.$viewValue);
        });
    }
}]);