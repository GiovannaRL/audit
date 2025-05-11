xPlanner.directive('awExists', ['$parse', function ($parse) {
    var directive = {
        link: link,
        restrict: 'A',
        require: '?ngModel',
        scope: {
            property: '@',
            sensitivity: '='
        }
    };
    return directive;

    function link(scope, elem, attrs, ctrl) {

        // if ngModel is not defined, we don't need to do anything
        if (!ctrl) return;
        //if (!attrs['awExists']) return;

        var anotherValue;

        var validator = function (value) {
            anotherValue = $parse(attrs['awExists']);

            var temp = anotherValue(scope) || [];
            v = true;

            if (value && scope.sensitivity) {
                for (var i = 0; i < temp.length; i++) {
                    if (scope.property && temp[i][scope.property] === value) {
                        v = false;
                        break;
                    } else if (!scope.property && temp[i] === value) {
                        v = false;
                        break;
                    }
                };
            } else if (value) {
                for (var i = 0; i < temp.length; i++) {
                    if (scope.property && temp[i][scope.property].toLowerCase() === value.toLowerCase()) {
                        v = false;
                        break;
                    } else if (!scope.property && temp[i].toLowerCase() === value.toLowerCase()) {
                        v = false;
                        break;
                    }
                };
            }

            ctrl.$setValidity('exists', v);
            return value;
        }

        ctrl.$parsers.unshift(validator);
        ctrl.$formatters.push(validator);
        attrs.$observe('awExists', function () {
            validator(ctrl.$viewValue);
        });
    }
}]);