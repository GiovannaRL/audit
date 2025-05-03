/*xPlanner.directive('awDateFormatter', function() {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function(scope, element, attr, ngModel) {
            function fromUser(dt) {
                return dt.toISOString();
            }
            function toUser(text) {
                var tmp = new Date(text);
                return new Date(tmp.getTime() + tmp.getTimezoneOffset() * 60 * 1000)
            }
            ngModel.$parsers.push(fromUser);
            ngModel.$formatters.push(toUser);

        }
    };
});*/

xPlanner.directive('awDateFormatter', ['$filter', function ($filter) {
    return {
        require: 'ngModel',
        link: function (scope, element, attrs, ngModelController) {
            ngModelController.$parsers.push(function (data) {
                data = $filter('date')(data, 'yyyy-MM-dd');
                return data;
            });

            ngModelController.$formatters.push(function (data) {
                data = $filter('date')(data, 'MM/dd/yyyy');
                return data;
            });
        }
    }
}]);

xPlanner.directive('awCurrencyModel', ['$filter', function ($filter) {
    return {
        require: 'ngModel',
        link: function (scope, element, attrs, ngModelController) {
            ngModelController.$parsers.push(function (data) {
                return data.replace(/[^0-9.-]/g, "");
            });

            ngModelController.$formatters.push(function (data) {
                return data ? $filter('currency')(data) : '$';
            });
        }
    }
}]);