xPlanner.directive('awOnlyNumbersAndLetters', function () {
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

            // regular expression which verifies not letters, numbers, - and _
            var reg = /[^A-Za-z0-9\-\_ ]/g;

            var v = true;
            if (value) {
                v = !(reg.test(value));
            }

            ctrl.$setValidity('number-and-letter', v);
            return value;
        }

        ctrl.$parsers.unshift(validator);
        ctrl.$formatters.push(validator);
        attrs.$observe('awOnlyNumbersAndLetters', function () {
            validator(ctrl.$viewValue);
        });
    }
});