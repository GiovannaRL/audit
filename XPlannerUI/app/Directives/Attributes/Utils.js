xPlanner.directive('ngRightClick', ['$parse', function ($parse) {
    return function (scope, element, attrs) {
        var fn = $parse(attrs.ngRightClick);
        element.bind('contextmenu', function (event) {
            scope.$apply(function () {
                event.preventDefault();
                fn(scope, { $event: event });
            });
        });
    };
}]);


xPlanner.directive('ngEnter', function () {
    return function (scope, element, attrs) {
        element.bind("keydown keypress", function (event) {
            if (event.which === 13) {
                scope.$apply(function () {
                    scope.$eval(attrs.ngEnter);
                });

                event.preventDefault();
            }
        });
    };
});


xPlanner.directive('ngIgnoreEnter', function () {
    return {
        link: function (scope, element, attrs) {
            $(element).keypress(function (e) {
                if (e.keyCode == 13) {
                    return false;
                }
            });
        }
    }
});


xPlanner.directive('parse-style', function ($interpolate) {
    return function (scope, elem) {
        var exp = $interpolate(elem.html()),
            watchFunc = function () { return exp(scope); };

        scope.$watch(watchFunc, function (html) {
            elem.html(html);
        });
    };
});

xPlanner.directive('capitalize', function () {
    return {
        require: 'ngModel',
        link: function(scope, element, attrs, modelCtrl) {
            var capitalize = function(inputValue) {
                if (inputValue == undefined) inputValue = '';
                var capitalized = inputValue.toUpperCase();
                if (capitalized !== inputValue) {
                    // see where the cursor is before the update so that we can set it back
                    var selection = element[0].selectionStart;
                    modelCtrl.$setViewValue(capitalized);
                    modelCtrl.$render();
                    // set back the cursor after rendering
                    element[0].selectionStart = selection;
                    element[0].selectionEnd = selection;
                }
                return capitalized;
            }
            modelCtrl.$parsers.push(capitalize);
            capitalize(scope[attrs.ngModel]); // capitalize initial value
        }
    };
});