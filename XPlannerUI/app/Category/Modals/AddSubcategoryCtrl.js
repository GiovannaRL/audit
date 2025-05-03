xPlanner.controller('AddSubcategoryCtrl', ['$scope', '$mdDialog', 'WebApiService', 'ProgressService', 'AuthService', 'toastr', 'local', 'DialogService',
    function ($scope, $mdDialog, WebApiService, ProgressService, AuthService, toastr, local, DialogService) {
        
        $scope.style_height = window.innerHeight - 388;
        $scope.subcategory = local && ($scope.edit = local.edit) ? angular.copy(local.subcategory) : {};

        $scope.controllerParams = { domain_id: AuthService.getLoggedDomain() };
        var added = [];

        if ($scope.edit) {

            WebApiService.genericController.get({
                controller: 'categories', action: 'Item', domain_id: local.subcategory.category_domain_id,
                project_id: local.subcategory.category_id
            }, function (category) {
                $scope.subcategory.assets_category = category
            });
        } else if (local) {
            $scope.subcategory = { assets_category: local.category && local.category.domain_id === AuthService.getLoggedDomain() ? local.category : null }
        }

        $scope.add = function (close) {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            $scope.addSubcategoryForm.$setSubmitted();

            if ($scope.addSubcategoryForm.$valid) {

                var method = $scope.edit ? 'update' : 'save';

                if ($scope.subcategory.asset_code != '' && $scope.subcategory.asset_code != undefined) {
                    $scope.subcategory.asset_code_domain_id = 1;
                }

                WebApiService.genericController[method]({
                    controller: "subCategories", action: "Item", domain_id: AuthService.getLoggedDomain(),
                    project_id: $scope.subcategory.category_domain_id || $scope.subcategory.assets_category.domain_id,
                    phase_id: $scope.subcategory.category_id || $scope.subcategory.assets_category.category_id,
                    department_id: $scope.subcategory.subcategory_id
                },
                    $scope.subcategory, function (subcategory) {

                        if ($scope.edit) {
                            toastr.success("Subcategory Saved");
                            $mdDialog.hide(subcategory);
                        } else {
                            toastr.success("Subcategory Added");
                            $scope.subcategory = { use_category_settings: true };
                            $scope.addSubcategoryForm.$setUntouched();
                            $scope.addSubcategoryForm.$setPristine();
                            added.push(subcategory);
                            if (close) {
                                $mdDialog.hide(added);
                            }
                        }
                    }, function (error) {
                        if (error.status === 409) {
                            toastr.error('The selected category already contains a subcategory with the name informed and it cannot be duplicated, please choose another name.');
                        } else {
                            toastr.error('Error to try save subcategory, please contact the tecnical support');
                            $mdDialog.cancel();
                        }
                    });
            } else {
                toastr.error("Please make sure you enter all the required fields");
            }
        };

        $scope.close = function () {
            added.length == 0 ? $mdDialog.cancel() : $mdDialog.hide(added);
        };

    }]);