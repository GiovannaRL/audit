xPlanner.run(['$rootScope', '$location', 'AuthService',
    function ($rootScope, $location, AuthService) {

        // Catch authentication error so we can properly redirect to the login page
        $rootScope.$on('oauth:error', function (event, rejection) {
            //// Ignore `invalid_grant` error - should be catched on LoginCtrl.js.
            //if ('invalid_grant' === rejection.data.error) {
            //    return;
            //}

            //// Refresh token when a `invalid_token` error occurs.
            //if ('invalid_token' === rejection.data.error) {
            //    console.log("invalid_token");
            //    return OAuth.getRefreshToken();
            //}

            //console.log('login');
            //// Otherwise we go to the login page
            //AuthService.clearLocalStorage();
            //$state.go('login');
        });

        // Send not authorized users to the home page
        $rootScope.$on('$stateChangeError', function (event, toState, toParams, fromState, fromParams, error) {

            event.preventDefault();

            if (error == 'UNAUTHENTICATED') {
                AuthService.clearLocalStorage();
                AuthService.removeToken();
                $location.path('/login');
            } else if (error == 'FORBIDDEN') {
                $location.path('/');
            } else if (error == 'FORBIDDEN_MANUFACTURER') {
                $location.path('/workspace/assets');
            }
        });
    }]);

