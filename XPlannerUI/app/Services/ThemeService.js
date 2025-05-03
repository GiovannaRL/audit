xPlanner.factory('ThemeService', ['localStorageService', function (localStorageService) {

    var defaultTheme = 'grey';

    function _setTheme(theme_name) {
        //verifies if the browser support the localStorage
        if (localStorageService.isSupported) {
            localStorageService.set("theme", theme_name);
        } else if (localStorageService.cookie.isSupported) { // otherwise we use cookie
            localStorageService.cookie.set("theme", theme_name);
        }
    };

    function _getTheme() {

        if (localStorageService.isSupported) {
            var theme = localStorageService.get("theme") || defaultTheme;
        } else if (localStorageService.cookie.isSupported) {
            var theme = localStorageService.cookie.get("theme") || defaultTheme;
        }

        if (theme && theme !== 'default')
            return theme;

        return defaultTheme;
    }

    return {
        setTheme: _setTheme,
        getTheme: _getTheme
    };

}]);