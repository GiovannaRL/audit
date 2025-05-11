xPlanner.controller('AssetsWorkspaceCtrl', ['$scope', '$state', 'AuthService', 'FabService', '$stateParams', '$timeout', 'DialogService', '$q',
    function ($scope, $state, AuthService, FabService, $stateParams, $timeout, DialogService, $q) {

        if (!AuthService.isAuthenticated()) {
            AuthService.clearLocalStorage();
            $state.go('login');
            return;
        }

        hasChanges = false;

        $scope.$on('initialTab', function (event, value) {
            $scope.activeTab = value;
        });

        $scope.$on('itemHasChanges', function () {
            hasChanges = true;
        });

        $scope.$on('dataSaved', function (event, stateData) {
            hasChanges = false;
            if (stateData)
                $state.go('assetsWorkspace.' + stateData.params);
        });

        $scope.goToWorspaceState = function (tab) {
            verifyChanges({ params: tab }).then(function () {
                $state.go('assetsWorkspace.' + tab);
            });

        };

        $scope.tabs = [
            { name: 'assets', visible: true },
            { name: 'manufacturers', visible: !AuthService.isManufacturerDomain() },
            { name: 'vendors', visible: !AuthService.isManufacturerDomain() },
            { name: 'categories', visible: true }
        ];

        /* float button */
        $scope.chosen = FabService.chosen;
        /* end float button */



        // Verify if the current item has been modified and then ask user to save modifications before leaving the page
        function verifyChanges(tab) {
            return $q(function (resolve, reject) {
                if (hasChanges) {
                    DialogService.SaveChangesModal().then(function (answer) {
                        hasChanges = false;
                        if (answer) {
                            $scope.$broadcast('saveData', tab);
                            reject();
                        } else {
                            resolve();
                        }
                    }, function () {
                        reject();
                    });
                } else {
                    resolve();
                }
            });
        };

    }]);