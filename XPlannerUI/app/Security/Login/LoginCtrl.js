xPlanner.controller('LoginCtrl', ['$scope', 'AuthService', '$state', 'toastr', '$mdDialog', 'WebApiService', 'ProgressService',
    'DialogService', '$q', 'UtilsService',
    function ($scope, AuthService, $state, toastr, $mdDialog, WebApiService, ProgressService, DialogService, $q, UtilsService) {
        if (AuthService.isAuthenticated()) {
            $state.go('index', { domain_id: AuthService.getLoggedDomain() }, {reload: true});
        } else {

            $scope.user = {};
            $scope.login = function () {

                $scope.loginForm.$setSubmitted();

                if ($scope.loginForm.$valid) {

                    ProgressService.blockScreen();

                    var user = { username: $scope.user.username, password: $scope.user.password };
                    AuthService.setLoggedUserEmail($scope.user.username);

                    AuthService.getAccessToken(user).then(function () {
                        if (AuthService.isAuthenticated()) {

                            WebApiService.genericController.query({ controller: "domains", action: "Available" }, function (data) {
                                ProgressService.unblockScreen();
                                AuthService.setAvailableDomains(data.length > 0 ? data : null);

                                var license_agreement = data[0].accept_user_license;
                                var user_id = data[0].user_id;
                                var domain_id = data[0].domain_id;
                                
                                verifyLicenseAgreement(license_agreement, user_id, domain_id).then(function () {
                                    if (data.length > 1) {
                                        $mdDialog.show({
                                            templateUrl: 'app/Security/Login/DomainsModal.html',
                                            controller: ['$mdDialog', 'toastr', function ($mdDialog, toastr) {
                                                this.domains = data;
                                                this.select = function () {
                                                    if (this.selected) {
                                                        $mdDialog.hide(this.selected);
                                                    }
                                                }
                                            }],
                                            controllerAs: 'ctrl',
                                            fullscreen: true,
                                        }).then(function (domain) {

                                            WebApiService.genericController.save({ controller: 'account', action: 'AddLoggedDomain' }, domain,
                                                function () {
                                                    // Inform the chosen domain to auth service
                                                    AuthService.setLoggedDomain(domain);

                                                    // Inform the chosen domain to the main controller
                                                    $scope.$emit('logged', domain)

                                                    UtilsService.ShowExpiratedPOs();

                                                    if (!AuthService.isManufacturerDomain()) {
                                                        // Go to the index page
                                                        $state.go('index', { domain_id: domain.domain_id });
                                                    } else {
                                                        $state.go('assetsWorkspace.assets');
                                                    }
                                                });
                                        }, function () {
                                            $scope.logoff();
                                        });
                                    } else if (data.length == 1) {
                                        WebApiService.genericController.save({ controller: 'account', action: 'AddLoggedDomain' }, data[0],
                                                function () {
                                                    // Inform the chosen domain to auth service
                                                    AuthService.setLoggedDomain(data[0]);

                                                    // Inform the chosen domain to the main controller
                                                    $scope.$emit('logged', data[0])

                                                    if (!AuthService.isManufacturerDomain()) {
                                                        // Go to the index page
                                                        $state.go('index', { domain_id: data[0].domain_id });
                                                    } else {
                                                        $state.go('assetsWorkspace');
                                                    }
                                                });
                                    } else {
                                        AuthService.removeToken();
                                        AuthService.clearLocalStorage();
                                        $state.go('no-domain');
                                    }
                                });
                            }, function () {
                                ProgressService.unblockScreen();
                                toastr.error("Error to retrieve data from server, please contact technical support");
                            });
                            

                        } else {
                            toastr.error("We could not store the token, are you using https?");
                        }
                    },
                    function (toastr) {
                        return function (response) {
                            if (response && response.data && response.data.error_description)
                                toastr.error(response.data.error_description);
                            else
                                toastr.error("Error to login");
                            ProgressService.unblockScreen();
                        }
                    }(toastr)
                    );
                }
            }

            function verifyLicenseAgreement(license_agreement, user_id, domain_id) {
                
                return $q(function (resolve, reject) {
                    if (license_agreement) {
                        resolve();
                    }
                    else {
                        $mdDialog.show({
                            templateUrl: 'app/Security/Login/LicenseAgreement.html',
                            controller: ['$mdDialog', 'toastr', function ($mdDialog, toastr) {
                                this.accept = function () {
                                    $mdDialog.hide('accept');
                                }
                                this.reject = function () {
                                    $mdDialog.hide();
                                }
                            }],
                            controllerAs: 'ctrl',
                            fullscreen: true,
                        }).then(function (license) {
                            if (license == 'accept') {
                                WebApiService.genericController.update({ controller: 'AspNetUsers', action: 'AcceptLicenseAgreement', domain_id: domain_id, project_id: user_id }, null,
                                    function () {
                                        resolve();
                                    });
                            }
                            else {
                                $scope.logoff();
                                reject();
                            };
                        });
                    }
                });
            };


            $scope.requestSubscription = function () {
                $state.go('subscription');
            };

            $scope.forgotPassword = function () {
                DialogService.openModal('app/Security/Login/Modals/ForgotPassword.html', 'ForgotPasswordCtrl', null, true);
            };
        }
    }]);