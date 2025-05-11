xPlanner.directive('awAutocompleteValidate', ['$mdUtil', '$parse', function ($mdUtil, $parse) {
    return {
        restrict: 'A',
        require: ['^mdInputContainer', 'ngModel'],
        link: function (scope, elem, attrs, ctrl) {

            var ngModelCtrl = ctrl[1];
            var containerCtrl = ctrl[0];
            if (!containerCtrl || !ngModelCtrl) return;
            
            var value = $parse(attrs['mdSelectedItem'])(scope);

            elem.addClass('aw-autocomplete-validate');


            /* Validation messages */
            var isErrorGetter = containerCtrl.isErrorGetter || function () {
                return ngModelCtrl.$invalid && (
                  ngModelCtrl.$touched ||
                  (ngModelCtrl.$$parentForm && ngModelCtrl.$$parentForm.$submitted)
                );
            };

            scope.$watch(isErrorGetter, function (newValue) {
                containerCtrl.setInvalid(newValue);

            });
            scope.$watch(function () { return $parse(attrs['mdSelectedItem'])(scope) }, function (newValue, oldValue) {
                if (!newValue != !oldValue) containerCtrl.element.toggleClass('md-input-has-value');
                ngModelCtrl.$setViewValue(newValue);
            });

            elem.focusout(function () {
                if (containerCtrl && ngModelCtrl.$untouched) {
                    ngModelCtrl.$setTouched();
                    ngModelCtrl.$setDirty();
                }
            });

            scope.$on('$destroy', function () {
                containerCtrl.element.removeClass('aw-autocomplete');
                containerCtrl.element.removeClass('md-input-has-value');
            });

        }
    }
}]);