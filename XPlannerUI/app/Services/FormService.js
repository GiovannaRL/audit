xPlanner.factory('FormService', [function () {

    function _resetForm(form) {

        if (!form) return;

        form.$setPristine();
        form.$setUntouched();
        angular.forEach(form, function (value, key) {
            if (typeof value === 'object' && value.hasOwnProperty('$modelValue')) {
                value.$setPristine();
                value.$setUntouched();
            }
        });
    };

    return {
        ResetForm: _resetForm
    };
}]);