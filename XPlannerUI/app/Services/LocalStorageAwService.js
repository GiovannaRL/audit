xPlanner.factory('localStorageAwService', ['localStorageService',
    function (localStorageService) {

        function _get(name) {
            if (localStorageService.isSupported) {
                return localStorageService.get(name);
            } else if (localStorageService.cookie.isSupported) {
                return localStorageService.cookie.get(name);
            } else {
                return null;
            }
        }

        function _set(name, value) {
            if (localStorageService.isSupported) {
                localStorageService.set(name, value);
            } else if (localStorageService.cookie.isSupported) {
                localStorageService.cookie.set(name, value);
            } else {
                return false;
            }

            return true;
        }

        return {
            get: _get,
            set: _set
        }

    }])