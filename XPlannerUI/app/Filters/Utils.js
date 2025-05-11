xPlanner.filter('stringToDate', function () {
    return function (input) {

        if (input && input.indexOf('-') == -1) {
            return input.substring(0, 2) + '-' + input.substring(2, 4) + '-' + input.substring(4);
        }

        return input;
    };
});

xPlanner.filter('capitalize', function () {
    return function (input) {

        if (input) {
            var index = /[a-zA-Z]/i.exec(input).index;
            if (index > -1) {
                return input.substring(0, index) + input.charAt(index).toUpperCase() +
                    (index < input.length - 1 ? input.substring(index + 1).toLowerCase() : '');
            }
        }
        return input;
    };
});

xPlanner.filter('enterpriseName', function () {
    return function (input) {

        if (input) {
            var index = input.indexOf('.');
            if (index != -1)
                return input.substring(0, index);
            return input;
        }

        return input;
    };
});