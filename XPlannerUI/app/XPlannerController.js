// Principal controller
xPlanner.controller('XPlannerController', ['$scope', '$state', '$mdSidenav', 'AuthService', '$mdMedia', 'DialogService',
    'ThemeService', 'WebApiService', '$timeout', 'UtilsService',
    function ($scope, $state, $mdSidenav, AuthService, $mdMedia, DialogService, ThemeService, WebApiService, $timeout, UtilsService) {

        $scope.theme = ThemeService.getTheme();
        $scope.current_year = new Date().getFullYear();

        $scope.auth = AuthService.isAuthenticated;
        $scope.mobileMedia = !$mdMedia('gt-xs');

        function UpdateEnterpriseOnView() {
            $scope.enterprise = AuthService.getLoggedDomainName();
        }

        UpdateEnterpriseOnView();

        function GoState(params) {
            $state.go(params.state, params.params, { reload: true });
            $mdSidenav('left').toggle();
            initMenu();
        };

        function OpenModal(params) {
            $mdSidenav('left').toggle().then(function () {
                initMenu();
                DialogService.openModal(params.template, params.ctrl, null, true);
            });
        };

        $scope.changePassword = function () {            
            DialogService.openModal('app/Security/Change Password/ChangePassword.html', 'ChangePasswordCtrl', null, true);
        };

        $scope.thirdParty = function () {
            $scope.showEnterprises = false;
            DialogService.openModal('app/Utils/Modals/ThirdPartyCopyright.html', 'ThirdPartyCopyrightCtrl', null, true);
        };

        function ChangeTheme(new_theme) {
            $scope.theme = new_theme;
            ThemeService.setTheme(new_theme);
        }

        $scope.toggleEnterprises = function (clickOutside) {

            if (!$scope.showEnterprises)
                $scope.userDomains = AuthService.getAvailableDomains();

            $scope.showEnterprises = !$scope.showEnterprises;
        }

        function DownloadUserGuide() {
            window.open('docs/Planner_Guide_v1.5.pdf', '_blank');
        };

        function Logoff() {
            // Ideally we would like to revoke our token, but we do not have this capability
            // so we remove it
            //OAuth.revokeToken();
            var logoff = function () {
                AuthService.removeToken();
                AuthService.clearLocalStorage();
                $state.go('login');
                $scope.showEnterprises = false;
            }
            WebApiService.genericController.save({
                controller: 'account', action: 'logout'
            }, null, logoff, logoff);
        };
        //
        // Checks if the token is still valid. When the token is invalid we should logoff. The invalid token
        // will cause 0 domains to be returned
        if (AuthService.isAuthenticated()) {
            WebApiService.genericController.query({ controller: "domains", action: "Available" }, function (data) {
                AuthService.setAvailableDomains(data.length > 0 ? data : null);
                if (!(data instanceof Array) || data.length == 0) {
                    Logoff();
                }
            }, Logoff
            );
        }

        function initMenu() {
            $scope.itemsMenu = [
            { title: 'Home', icon: 'home', toggle: false, visible: true, onclick: GoState, params: { state: 'index' } },
                { title: 'Projects', icon: 'projects', toggle: false, visible: !AuthService.isManufacturerDomain(), onclick: GoState, params: { state: 'index' } },
                { title: 'Assets Catalog', icon: 'assets', toggle: false, visible: !AuthService.isViewer() || AuthService.isManufacturerDomain(), onclick: GoState, params: { state: 'assetsWorkspace.assets' } },
                //{ title: 'Dashboard', icon: 'dashboard', toggle: false, visible: AuthService.isAdmin() && !AuthService.isManufacturerDomain(), onclick: GoState, params: { state: 'dashboard' } },
                { title: 'Room Templates', icon: 'room_templates', toggle: false, visible: !AuthService.isViewer() && !AuthService.isManufacturerDomain(), onclick: GoState, params: { state: 'room-templates' } },
              //{ title: 'Department Types', icon: 'department_types', toggle: false, onclick: GoState, params: { state: 'index' } },
              {
                  title: 'Administration', icon: 'admin', toggle: true, visible: AuthService.isAdmin(), subItems: [
                      { title: 'User Management', icon: 'user_management', visible: true, onclick: GoState, params: { state: 'users' } },
                      { title: 'Audit', icon: 'audit', visible: true, onclick: GoState, params: { state: 'audit' } },
                      { title: 'Enterprise Management', icon: 'enterprise', toggle: false, visible: AuthService.isAudaxwareDomainAndEmail(), onclick: GoState, params: { state: 'domains' } },
                      { title: 'Asset Code', icon: 'code', toggle: false, visible: AuthService.isAudaxwareDomainAndEmail(), onclick: GoState, params: { state: 'assetCode' } },
                      /*{ title: 'Add User', onclick: GoState, params: { state: 'index' } },
                      { title: 'Change password', onclick: OpenModal, params: { ctrl: 'ChangePasswordCtrl', template: 'app/Security/Change Password/ChangePassword.html' } },
                      { title: 'Billing', onclick: GoState, params: { state: 'index' } }*/
                  ]
              },
               {
                   title: 'Site Theme', icon: 'themes', toggle: true, visible: true, subItems: [
                       { title: 'Cyan', onclick: ChangeTheme, params: 'cyan', visible: true },
                       { title: 'Green', onclick: ChangeTheme, params: 'green', visible: true },
                       { title: 'Grey', onclick: ChangeTheme, params: 'grey', visible: true },
                       { title: 'Indigo', onclick: ChangeTheme, params: 'indigo', visible: true },
                       { title: 'Red', onclick: ChangeTheme, params: 'red', visible: true }
                   ]
               },
             { title: 'Contact Us', icon: 'contact_us', toggle: false, visible: true, onclick: OpenModal, params: { ctrl: 'ContactUsCtrl', template: 'app/Contact Us/Modals/ContactUs.html' } },
             //{ title: 'Users Guide', icon: 'user_guide', toggle: false, onclick: DownloadUserGuide },
             { title: 'Logout', icon: 'logout', toggle: false, visible: true, onclick: Logoff }
            ];
        };
                

        $scope.$watch(function () { return AuthService.getLoggedUserType() }, function (newValue) {
            initMenu();
        });

        $scope.changeDomain = function (domain, refreshSite) {
            WebApiService.genericController.update({ controller: 'account', action: 'ChangeLoggedDomain' }, domain,
                function () {
                    if (refreshSite) {
                        var email = AuthService.getLoggedUserEmail();
                        AuthService.clearLocalStorage();
                        AuthService.setLoggedUserEmail(email);
                        AuthService.setAvailableDomains($scope.userDomains);
                        AuthService.setLoggedDomain(domain);
                        $scope.enterprise = domain.name;
                        $scope.showEnterprises = false;
                        // Reinitiates menu so the items will reflect the new domain and role
                        initMenu();
                        $mdSidenav('left').close();
                        $state.go('index', null, { reload: true });

                        UtilsService.ShowExpiratedPOs();
                    }
                });
        }

        // If the server is restarted the user refreshes the page, the server will not have the user
        // Selected domain. All calls will return empty value. This makes sure we properly set the domain upon reload
        // TODO(JLT): We need to make the server return an error in case the domain is not set for the user and the controller
        // being called is not the accounts controller
        function refreshDomain() {
            if (AuthService.getLoggedDomain()) {
                if (!$scope.userDomains) {
                    $scope.userDomains = AuthService.getAvailableDomains();
                    if (!$scope.userDomains) {
                        Logoff();
                        return;
                    }
                }
                var domainId = AuthService.getLoggedDomain();

                var currentDomain = $scope.userDomains.find(function (domain) {
                    return (domainId == domain.domain_id)
                });
                if (!currentDomain) {
                    Logoff();
                }
                $scope.changeDomain(currentDomain);
            }
        }

        if (!$scope.userDomains) {
            refreshDomain();
        }

        $scope.toogleLeft = function () {
            $mdSidenav('left').toggle();
        };

        $scope.toggleMenuItem = function (index) {
            $scope.itemsMenu.map(function (i, ind) {
                i.open = index == ind ? !i.open : false;
                return i;
            });
        };

        $scope.logoff = Logoff;

        $scope.messages = [];
        var canAddNotifications = true;

        (function GetNotifications() {
            if (AuthService.isAuthenticated() && AuthService.getLoggedDomain() > 0) {
                WebApiService.genericController.query({ controller: 'Notification', action: 'All', domain_id: AuthService.getLoggedDomain() },
                       function (messages) {
                           if (canAddNotifications && messages.length > 0) {
                               $scope.messages = $scope.messages.concat(messages.filter(function (item) {
                                   return !$scope.messages.find(function (i) {
                                       return i.id == item.id;
                                   });
                               }));
                           } else if (canAddNotifications) {
                               $scope.messages = [];
                           }
                           $timeout(GetNotifications, 10000);
                       }, function (error) {
                           if (error.status === 401) {
                               refreshDomain();
                           }
                           console.log('error to get notifications');
                           $timeout(GetNotifications, 10000);
                       });
            } else {
                $timeout(GetNotifications, 10000);
            }
        })();

        $scope.toggleNotifications = function (clickOutside) {

            if (!clickOutside || $scope.showNotifications) {

                if (!$scope.showNotifications && $scope.messages.length > 0) {
                    $scope.showNotifications = true;
                    canAddNotifications = false;
                    $scope.noNotifications = false;
                } else if (!$scope.showNotifications) {

                    $scope.showNotifications = true;
                    canAddNotifications = false;
                    $scope.noNotifications = true;
                    $scope.messages = [{ id: -1, message: 'No notifications' }];
                } else {
                    $scope.showNotifications = false;
                    canAddNotifications = false;
                    WebApiService.genericController.save({ controller: 'Notification', action: 'Read', domain_id: AuthService.getLoggedDomain() },
                        $scope.messages.map(function (item) { if (item.id != -1) return item.id }), function () {
                            $scope.messages = [];
                            canAddNotifications = true;
                            $scope.noNotifications = false;
                        }, function () {
                            console.log('Error to set notifications as read');
                        });
                }
            }
        }

        /* to be fired when an user is logged and then 
            set in the view the chosen enterprise */
        $scope.$on('logged', function (event, domain) {
            UpdateEnterpriseOnView();
        })
    }]);