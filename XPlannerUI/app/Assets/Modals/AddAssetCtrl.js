xPlanner.controller('AddAssetCtrl', ['$scope', 'WebApiService', 'AuthService', '$mdDialog', 'toastr',
    'AssetsService', 'ProgressService', '$state',
    function ($scope, WebApiService, AuthService, $mdDialog, toastr, AssetsService, ProgressService, $state) {

        $scope.categoryControllerParams = {
            domain_id: AuthService.getLoggedDomain(), //subcategory_domain_id
            project_id: AuthService.getLoggedDomain() //category_domain_id
        }

        $scope.controllerParams = {
            domain_id: AuthService.getLoggedDomain()
        }

        function GetGenericParams(controller, action, id1, id2, id3) {
            return {
                controller: controller,
                action: action,
                domain_id: id1 || AuthService.getLoggedDomain(),
                project_id: id2,
                phase_id: id3
            };
        };

        $scope.asset = {
            default_resp: 'OFOI'
        };


        $scope.$watch('selectedCategory', function (newCategory) {
            if (newCategory) {

                //if (newCategory.asset_code != null) {
                    $scope.asset.asset_code = newCategory.asset_code;
                //}
            }
            
        });

        $scope.save = function () {

            $scope.addNewAssetForm.$setSubmitted();
            $scope.addNewAssetForm.asset_code.$setTouched();

            if ($scope.addNewAssetForm.$valid) {

                // Verifies if the min amount is not more than max amount
                if ($scope.asset.min_cost && $scope.asset.max_cost
                        && Number($scope.asset.min_cost) > Number($scope.asset.max_cost)) {
                    toastr.error('The Max value must be grather than Min value');
                    return;
                }

                ProgressService.blockScreen();
                $scope.asset.default_resp = $scope.asset.default_resp.name;
                $scope.asset.assets_subcategory = angular.extend({}, $scope.selectedCategory, { domain_id: $scope.selectedCategory.subcategory_domain_id, assets_category: { domain_id: $scope.selectedCategory.category_domain_id, category_id: $scope.selectedCategory.category_id } });
                if (typeof $scope.asset.asset_code === 'object') {
                    $scope.asset.asset_code = $scope.asset.asset_code.prefix;
                }
                AssetsService.AddAsset($scope.asset).then(function (data) {
                    $scope.showDetails(data);
                }, function () {
                    ProgressService.unblockScreen();
                    toastr.error('Error to try save asset, please contact the technical support');
                });

            } else {
                toastr.error('Please make sure you enter all the required fields');
            }
        };

        $scope.showDetails = function (asset) {

            asset = {
                asset_id: asset.asset_id,
                domain_id: asset.domain_id,
                asset_code: asset.asset_code,
                comment: asset.comment || null,
                model_number: asset.model_number,
                model_name: asset.model_name,
                asset_description: asset.asset_description,
                asset_category: asset.assets_subcategory.assets_category.description,
                asset_subcategory: asset.assets_subcategory.description,
                owner_name: AuthService.getLoggedDomainName(),
                //owner: { domain_id: AuthService.getLoggedDomain(), name: AuthService.getLoggedDomainName() },
                manufacturer_description: asset.manufacturer.manufacturer_description,
                cut_sheet: asset.cut_sheet,
                cad_block: asset.cad_block,
                revit: asset.revit,
                updated_at: asset.updated_at,
            };

            $state.go('assetsWorkspace.assetsDetails', { domain_id: asset.domain_id, asset_id: asset.asset_id });
            $mdDialog.hide();
        };

        $scope.close = function () {
            $mdDialog.cancel();
        };
    }]);