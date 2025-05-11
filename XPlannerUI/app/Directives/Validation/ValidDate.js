xPlanner.directive('awValidDate', function () {
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
            var v = true;
            if (value) {
                var tmpValue = typeof (value) === 'string' ? value : typeof(value) === 'object' && value.getMonth ? ((value.getMonth() + 1) + "/" + value.getDate() + "/" + value.getFullYear()) : null;

                // regular expression which verifies not numbers
                if (tmpValue) {
                    var reg = /(0?[1-9]|1[012])[- /.](0?[1-9]|[12][0-9]|3[01])[- /.](19|20)\d\d/;
                    v = reg.test(tmpValue.substring(0, 10));
                }
            }

            ctrl.$setValidity('valid-date', v);
            return value;
        }

        ctrl.$parsers.unshift(validator);
        ctrl.$formatters.push(validator);
        attrs.$observe('awValidDate', function () {
            validator(ctrl.$viewValue);
        });
    }
});