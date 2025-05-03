xPlanner.directive('awUploadFileSize', ['$parse', function ($parse) {

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

        if (attrs.firefox !== undefined) {
            elem.bind("change", function (changeEvent) {
                var files = changeEvent.target.files;
                if (files.length > 0) {
                    validator(files[0]);
                } else {
                    validator();
                }
            });
        }

        var validator = function (value) {

            // passed in megabytes
            var limitSize = $parse(attrs['awUploadFileSize']);
            limitSize = limitSize(scope) * 1048576; // covert to Bytes

            v = !value || value.size <= (limitSize || 7340032); // 7Mb as default

            ctrl.$setValidity('awUploadFileSize', v);
            return value;
        }

        ctrl.$parsers.unshift(validator);
        ctrl.$formatters.push(validator);
        attrs.$observe('awUploadFileSize', function () {
            validator(ctrl.$viewValue);
        });

    }
}]);