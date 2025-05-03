xPlanner.factory('AuthService', ['OAuth', 'localStorageService', 'OAuthToken',
    function (OAuth, localStorageService, OAuthToken) {

        var notToClean = [/*'add-asset-grid-state'*/, 'theme'];
        var mapProjectsStatus = new Map();

        var _isAuthenticated = function () {
            return OAuth.isAuthenticated();
        };

        var _getAccessToken = function (user) {
            return user ? OAuth.getAccessToken(user) : OAuthToken.getAccessToken();
        };

        var _getRefreshToken = function () {
            return OAuth.getRefreshToken();
        };

        var _removeToken = function () {
            return OAuthToken.removeToken();
        };

        var _setProjectStatus = function (projects) {
            projects.forEach(({ id, status }) => {
                mapProjectsStatus.set(id, status);
            });
        };

        var _getProjectStatus = function (projectId) {
            return mapProjectsStatus.get(parseInt(projectId));
        };

        var _setLoggedDomain = function (domain) {

            //verifies if the browser support the localStorage
            if (localStorageService.isSupported) {
                localStorageService.set("domain", domain);
            } else if (localStorageService.cookie.isSupported) { // otherwise we use cookie
                localStorageService.cookie.set("domain", domain);
            } else {
                return false;
            }

            return true;
        };

        function _isProjectLocked() {
            return _getProjectStatus() === "L";
        }

        var _getLoggedDomain = function () {

            if (localStorageService.isSupported) {
                var domain = localStorageService.get("domain");
            } else if (localStorageService.cookie.isSupported) {
                var domain = localStorageService.cookie.get("domain");
            }

            return domain ? domain.domain_id : null;
        };

        var _getLoggedDomainName = function () {

            if (localStorageService.isSupported) {
                var domain = localStorageService.get("domain");
            } else if (localStorageService.cookie.isSupported) {
                var domain = localStorageService.cookie.get("domain");
            }

            if (domain) {
                return domain.name;
                //var index_of = domain.name.indexOf('.');
                //if (index_of > -1)
                //    return domain.name.substring(0, index_of);
            }

            return null;
        };

        function _getLoggedDomainFull() {
            if (localStorageService.isSupported) {
                return localStorageService.get("domain");
            } else if (localStorageService.cookie.isSupported) {
                return localStorageService.cookie.get("domain");
            }
        }

        var _getLoggedUserType = function () {
            var loggedDomain = _getLoggedDomainFull();
            return loggedDomain ? loggedDomain.role_id : null;
        };

        function _isManufacturerDomain() {
            var loggedDomain = _getLoggedDomainFull();
            return loggedDomain ? loggedDomain.type === 'M' : null;
        }

        function _isAdmin() {
            return _getLoggedUserType() === 1;
        }

        function _isViewer() {
            return _getLoggedUserType() === 3;
        }

        var _setLoggedUserEmail = function (email) {
            //verifies if the browser support the localStorage
            if (localStorageService.isSupported) {
                localStorageService.set("user_email", email);
            } else if (localStorageService.cookie.isSupported) { // otherwise we use cookie
                localStorageService.cookie.set("user_email", email);
            } else {
                return false;
            }

            return true;
        };

        var _getLoggedUserEmail = function () {

            if (localStorageService.isSupported) {
                return localStorageService.get("user_email");
            } else if (localStorageService.cookie.isSupported) {
                return localStorageService.cookie.get("user_email");
            }
        };

        var _isAudaxwareDomainAndEmail = function () {
            return _getLoggedDomain() === 1 && _getLoggedUserEmail().split('@')[1] === 'audaxware.com';
        }

        var _setAvailableDomains = function (domains) {

            if (localStorageService.isSupported) {
                localStorageService.set("availableDomains", domains);
            } else if (localStorageService.cookie.isSupported) {
                localStorageService.cookie.set("availableDomains", domains);
            } else {
                return false;
            }

            return true;
        };

        var _getAvailableDomains = function () {

            if (localStorageService.isSupported) {
                return localStorageService.get("availableDomains");
            } else if (localStorageService.cookie.isSupported) {
                return localStorageService.cookie.get("availableDomains");
            }
        };

        var _removeLoggedDomain = function () {

            if (localStorageService.isSupported) {
                localStorageService.remove("domain_id");
            } else if (localStorageService.cookie.isSupported) {
                localStorageService.cookie.remove("domain_id");
            } else {
                return false;
            }
            return true;
        };

        var _clearLocalStorageCompletely = function () {

            if (localStorageService.isSupported) {
                localStorageService.clearAll();
                return true;
            } else if (localStorageService.cookie.isSupported) {
                localStorageService.cookie.clearAll();
                return true;
            }
        }

        var _clearLocalStorage = function () {

            var keep = {};

            if (localStorageService.isSupported) {

                angular.forEach(notToClean, function (item) { keep[item] = localStorageService.get(item) });
                localStorageService.clearAll();
                angular.forEach(keep, function (value, key) { localStorageService.set(key, value); });

            } else if (localStorageService.cookie.isSupported) {

                angular.forEach(notToClean, function (item) { keep[item] = localStorageService.cookie.get(item) });
                localStorageService.cookie.clearAll();
                angular.forEach(keep, function (value, key) { localStorageService.cookie.set(key, value); });
            } else {
                return false;
            }
            return true;
        }

        return {
            isAuthenticated: _isAuthenticated,
            getAccessToken: _getAccessToken,
            getRefreshToken: _getRefreshToken,
            removeToken: _removeToken,
            setProjectStatus: _setProjectStatus,
            getProjectStatus: _getProjectStatus,
            setLoggedDomain: _setLoggedDomain,
            getLoggedDomain: _getLoggedDomain,
            getLoggedDomainName: _getLoggedDomainName,
            getLoggedDomainFull: _getLoggedDomainFull,
            getLoggedUserType: _getLoggedUserType,
            setLoggedUserEmail: _setLoggedUserEmail,
            getLoggedUserEmail: _getLoggedUserEmail,
            getAvailableDomains: _getAvailableDomains,
            setAvailableDomains: _setAvailableDomains,
            removeLoggedDomain: _removeLoggedDomain,
            clearLocalStorage: _clearLocalStorage,
            clearLocalStorageCompletely: _clearLocalStorageCompletely,
            isAdmin: _isAdmin,
            isViewer: _isViewer,
            isManufacturerDomain: _isManufacturerDomain,
            isProjectLocked: _isProjectLocked,
            isAudaxwareDomainAndEmail: _isAudaxwareDomainAndEmail
        }

    }])