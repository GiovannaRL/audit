xPlanner.factory('RouteService', ['AuthService', '$q', function (AuthService, $q) {

    function _isNotManufaturerDomainType() {

        if (!$isAuthenticated()) {
            return $q.reject('UNAUTHENTICATED');
        } else if (AuthService.isManufacturerDomain()) {
            return $q.reject('FORBIDDEN_MANUFACTURER');
        }

        return $q.resolve();
    }

    function _isAdminUser() {

        if (!$isAuthenticated()) {
            return $q.reject('UNAUTHENTICATED')
        } else if (!AuthService.isAdmin()) {
            return $q.reject('FORBIDDEN')
        }

        return $q.resolve();
    }

    function _isAuthenticated() {

        if (!$isAuthenticated()) {
            return $q.reject('UNAUTHENTICATED')
        }

        return $q.resolve();

    }

    function $isAuthenticated() {
        return AuthService.isAuthenticated() && typeof AuthService.getLoggedDomain() == "number";
    }

    return {
        isNotManufaturerDomainType: _isNotManufaturerDomainType,
        isAuthenticated: _isAuthenticated,
        isAdminUser: _isAdminUser
    }

}])