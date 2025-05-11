xPlanner.controller('EditDomainCtrl', ['$scope', 'WebApiService', '$mdDialog', 'AuthService', 'toastr', 'local', 'ProgressService',
    function ($scope, WebApiService, $mdDialog, AuthService, toastr, local, ProgressService) {

        $scope.anotherDomains = local.existingDomains;
        $scope.domain = angular.copy(local.domain);
        $scope.edit = local.edit;

        if (!local.edit) {
            $scope.domain = { type: 'E' };
        }

        function loadManufacturers() {

            if ($scope.availableManufacturers && $scope.availableManufacturers.length < 1) {
                return;
            }

            ProgressService.blockScreen();
            WebApiService.genericController.query({
                controller: 'Manufacturer', action: 'AllSorted', domain_id: AuthService.getLoggedDomain()
            }, function (data) {
                $scope.availableManufacturers = data;
                ProgressService.unblockScreen();
            }, function () {
                toastr.error('Error to try get the manufacturers');
                ProgressService.unblockScreen();
            });
        };

        if (local.edit && $scope.domain.type === 'M') {
            loadManufacturers();
        }

        $scope.save = function () {

            $scope.addDomainForm.$setSubmitted();

            if ($scope.addDomainForm.$valid) {
                ProgressService.blockScreen();

                // Due to the different models that update and save controllers need, we need to make sure to send manufacturers in the correct property:
                if (!local.edit) {
                    $scope.domain.manufacturers1 = $scope.domain.manufacturers;
                    $scope.domain.manufacturers = null;
                    $scope.domain.enabled = true;
                }


                WebApiService.genericController[local.edit ? 'update' : 'save']({ controller: "domains", action: "Item", domain_id: $scope.domain.domain_id },
                    $scope.domain, function (data) {
                        console.log(data);
                        if (!local.edit) {
                            var user = { email: $scope.domain.email, first_name: 'Auto', last_name: 'Created' };
                            user.aspNetUserRoles = null;
                            user.AspNetUserRoles = [{ userId: null, roleId: 1, domain_id: data.domain_id }];
                            var enterprise = { domain_id: data.domain_id, name: data.name }
                            var sendData = { user: user, enterprise: enterprise }

                            WebApiService.genericController.save({
                                controller: 'Users', action: 'InviteUser', domain_id: AuthService.getLoggedDomain()
                            }, sendData, function (data) {
                            }, function (error) {
                                if (error.status === 409) {
                                    toastr.error('The enterprise already has an associated user with the informed Email!');
                                } else {
                                    toastr.error('Error trying to associate enterprise with user, please contact the technical support.');
                                }
                            });
                        }

                        toastr.success('Enterprise successfully saved.');
                        ProgressService.unblockScreen();
                        $mdDialog.hide();
                    }, function () {
                        toastr.error("Error to try save domain, please contact technical support");
                        ProgressService.unblockScreen();
                    });
            }
        };

        $scope.close = function () {
            $mdDialog.cancel();
        };

        $scope.changeDomain = function (newValue) {
            if (newValue === 'M') {
                $scope.domain.show_audax_info = true;
                loadManufacturers();
            }
        }
    }]);