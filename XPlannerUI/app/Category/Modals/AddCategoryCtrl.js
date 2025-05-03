xPlanner.controller('AddCategoryCtrl', ['$scope', '$mdDialog', 'WebApiService', 'ProgressService', 'AuthService', 'toastr', 'local', 'DialogService',
    function ($scope, $mdDialog, WebApiService, ProgressService, AuthService, toastr, local, DialogService) {

        $scope.style_height = window.innerHeight - 275;

        $scope.category = local && ($scope.edit = local.edit) ? angular.copy(local.category) : {};

        var added = [];

        function GetGenericParams(controller, action, id1, id2, id3) {
            return {
                controller: controller,
                action: action,
                domain_id: id1 || AuthService.getLoggedDomain(),
                project_id: id2,
                phase_id: id3
            };
        };

        WebApiService.genericController.query({controller: 'categories', action: 'All', domain_id: AuthService.getLoggedDomain()},
            function (categories) {
                $scope.anotherCategories = categories.filter(function (c) { return c.description !== $scope.category.description; })
                    .map(function (c) { return c.description });
            });

        WebApiService.genericController.query(GetGenericParams("AssetCodes", "All"), function (codes) {
            $scope.assetCodes = codes;
        }, function () {
            toastr.error("Error to retrieve data from server, please contact technical support");
        });

        $scope.add = function (close) {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            $scope.addCategoryForm.$setSubmitted();

            if ($scope.addCategoryForm.$valid) {

                var method = $scope.edit ? 'update' : 'save';

                if ($scope.category.asset_code != '' && $scope.category.asset_code != undefined) {
                    $scope.category.asset_code_domain_id = 1;
                }

                WebApiService.genericController[method]({ controller: "categories", action: "Item", domain_id: AuthService.getLoggedDomain(), project_id: $scope.category.category_id },
                    $scope.category, function (category) {
                        if ($scope.edit) {
                            toastr.success("Category Saved");
                            $mdDialog.hide(category);
                        } else {
                            toastr.success("Category Added");
                            $scope.category = {};
                            $scope.addCategoryForm.$setUntouched();
                            $scope.addCategoryForm.$setPristine();
                            added.push(category);
                            if (close) {
                                $mdDialog.hide(added);
                            }
                        }
                    }, function () {
                        toastr.error('Error to save category, please contact the technical support');
                        $mdDialog.cancel();
                    });
            } else {
                toastr.error("Please make sure you enter all the required fields");
            }
        };

        $scope.close = function () {
            added.length == 0 ? $mdDialog.cancel() : $mdDialog.hide(added);
        };

    }]);