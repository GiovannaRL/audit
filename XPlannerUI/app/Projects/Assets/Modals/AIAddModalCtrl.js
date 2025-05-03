xPlanner.controller('AIAddModalCtrl', ['$scope', 'AuthService', '$mdDialog', 'local', 'WebApiService', '$q', 'toastr',
        'ProgressService', '$timeout', '$mdStepper',
    function ($scope, AuthService, $mdDialog, local, WebApiService, $q, toastr, ProgressService, $timeout, $mdStepper) {

        // Verify if is authenticate
        if (!AuthService.isAuthenticated()) {
            $state.go('login');
            return;
        }

        $scope.isTemplate = local.isTemplate;
        $scope.isToGlobalTemplate = local.isToGlobalTemplate;
        $scope.isLink = local.isLink;
        if (local.selected_asset) {
            $scope.linkBudgetQty = local.selected_asset.budget_qty;
            toastr.warning('Only asset entries with matching quantities will appear when linking');
        }
        else {
            $scope.linkBudgetQty = null;
        }

        $scope.currentStep = 1;
        $scope.wizardData = { selectedAssets: [], projectAction: 1, isRelocate: false };
        $scope.modalTitle = local.isLink ? 'Link Asset' : 'Add New Asset';
        $scope.confirmMessage = local.isLink ? 'linking the asset' : 'adding the assets';

        var steppers;
        $timeout(function () {
            steppers = $mdStepper('stepper-add-asset');
        }, false);

        $scope.wizardData.params = local.isLink ? local.selected_asset : local.params;
        $scope.wizardData.style_height = window.innerHeight - (local.isLink ? 340 : 310); // When linking we have an extra element on page with source item

        var adds = [];

        function buildSourceDescription() {
            if (local.selected_asset) {
                $scope.sourceDescription = local.selected_asset.phase_description + ' > '
                    + local.selected_asset.department_description + ' > '
                    + local.selected_asset.room_name + ' - ' + local.selected_asset.room_number + ' > ';

                if (local.selected_asset.jsn_code) {
                    $scope.sourceDescription += local.selected_asset.jsn_code + ' > ';
                }

                $scope.sourceDescription += local.selected_asset.asset_code + ' - ' + local.selected_asset.asset_description;
            }  
        }

        function updateInfo(item, roomParams) {
            item = angular.copy(item);
            item.project_id = $scope.wizardData.params.project_id;
            item.phase_id = roomParams.phase_id || $scope.wizardData.params.phase_id;
            item.department_id = roomParams.department_id || $scope.wizardData.params.department_id;
            item.room_id = roomParams.room_id || $scope.wizardData.params.room_id;
            item.asset_domain_id = item.domain_id;
            item.domain_id = AuthService.getLoggedDomain();
            item.resp = item.default_resp;
            item.comment = 1;
            item.current_location = 'Plan';
            item.unit_budget = item.unit_budget || 0;
            return item;
        }

        if ($scope.isLink) {
            buildSourceDescription();
        }

        /* Add assets */
        $scope.add = function () {

            if (!$scope.wizardData.selectedAssets || $scope.wizardData.selectedAssets.length < 1) {
                steppers.error('Please modify the qty to select at least one element to add');
                return;
            };

            if (!$scope.wizardData.params.room_id && (!$scope.wizardData.selecteds || $scope.wizardData.selecteds.length == 0)) {
                // this should actually never happen
                steppers.error('Please select at least one room');
                return;
            } else if ($scope.wizardData.params.room_id) {
                $scope.wizardData.selecteds = [$scope.wizardData.params];
            }

            ProgressService.blockScreen();

            var promiseArray = [];

            if (!$scope.wizardData.selectedAssets[0].inventory_id) {
                angular.forEach($scope.wizardData.selectedAssets, function (item) { // looping for assets
                    angular.forEach($scope.wizardData.selecteds, function (location) { // looping for locations
                        promiseArray.push(WebApiService.asset_inventory_consolidated_save.save({
                            domain_id: AuthService.getLoggedDomain(),
                            project_id: location.project_id,
                            cost_field: ($scope.wizardData.selected_costField ? $scope.wizardData.selected_costField : 'default')
                        }, updateInfo(item, location), function (data) {
                            adds.push(data);
                            //update progress bar
                            $scope.$emit('updateProgressBar', $scope.params);
                        }).$promise);
                    });
                });
            } else {
                if ($scope.isLink) {
                    //var location = $scope.wizardData.selecteds[0];
                    promiseArray.push(WebApiService.link_asset_from_project.save({
                        domain_id: AuthService.getLoggedDomain(), project_id: $scope.wizardData.params.project_id, target_inventory_id: local.selected_asset.inventory_id
                    }, $scope.wizardData.selectedAssets[0], function (data) {
                        adds.push(data);
                        //update progress bar
                        $scope.$emit('updateProgressBar', $scope.params);
                    }).$promise);
                } else {
                // angular.forEach($scope.wizardData.selecteds, function (location) { // looping for locations
                    promiseArray.push(WebApiService.add_asset_from_project_multiple_rooms.save({
                        domain_id: AuthService.getLoggedDomain(),
                        project_id: $scope.wizardData.params.project_id,
                        action: $scope.wizardData.projectAction,
                        withBudgets: $scope.wizardData.selected_costField == 'source'
                    }, {
                        items: $scope.wizardData.selectedAssets,
                        locations: $scope.wizardData.selecteds
                    }, function (data) {
                        adds = adds.concat(data);
                        //update progress bar
                        $scope.$emit('updateProgressBar', $scope.params);
                    }).$promise);
                // });
                }
            }

            $q.all(promiseArray).then(function () {
                ProgressService.unblockScreen();
                //GridService.unselectAll($scope.wizardData.assetsGrid);
                if ($scope.isLink) {
                    toastr.success('The selected asset has been linked!');
                }
                else {
                    toastr.success('All the selected assets have been added!');
                }
                
                $scope.close();
            }, function () {
                ProgressService.unblockScreen();
                toastr.error('Error to try add one, or all, the selected assets. Please contact technical support');
            });
        };
        /* END - Add assets */
        $scope.confirm = function () {
            if (!$scope.wizardData.selecteds || $scope.wizardData.selecteds.length == 0) {
                // this should actually never happen
                steppers.error('Please select ' + ($scope.wizardData.isRelocate ? '' : 'at last') + ' one room');
                return;
            } else if (!$scope.wizardData.params.room_id && $scope.wizardData.isRelocate && $scope.wizardData.selecteds.length > 1) {
                steppers.error('When selecting "relocate" mode only one room can be selected');
                return;
            }

            var rooms;
            if (!$scope.wizardData.params.room_id) {
                for (var i = 0; i < $scope.wizardData.selecteds.length; ++i) {
                    var room = $scope.wizardData.selecteds[i];
                    if (rooms)
                        rooms += ', "' + room.room_desc + '"';
                    else
                        rooms = '"' + room.room_desc + '"';
                }
                $scope.wizardData.addingRooms = rooms;
            }
            else {
                for (var i = 0; i < $scope.wizardData.selectedAssets.length; ++i) {
                    if (rooms)
                        rooms += ', "' + $scope.wizardData.selectedAssets[i].asset_code + ' - ' + $scope.wizardData.selectedAssets[i].asset_description + '"';
                    else
                        rooms = '"' + $scope.wizardData.selectedAssets[i].asset_code + ' - ' + $scope.wizardData.selectedAssets[i].asset_description + '"';
                }
                $scope.wizardData.addingRooms = rooms;
            }
            $scope.currentStep = 3;
            steppers.next();
        }

        // close the modal
        $scope.close = function () {
            ProgressService.blockScreen();
            adds.length ? $mdDialog.hide(adds) : $mdDialog.cancel();
            ProgressService.unblockScreen();
        };

        $scope.next = function () {
            steppers.clearError();
            switch ($scope.currentStep) {
                case 1:
                    ProgressService.blockScreen();

                    if ((!$scope.wizardData.selectedAssets || $scope.wizardData.selectedAssets.length > 1) && $scope.isLink) {
                        steppers.error('Please select only one asset to be linked');
                        ProgressService.unblockScreen();
                        return;
                    }
                    if (!$scope.wizardData.selectedAssets || $scope.wizardData.selectedAssets.length < 1) {
                        if ($scope.wizardData.isRelocate) {
                            steppers.error('Please select at least one asset to be relocated');
                        } else {
                            steppers.error('Please modify the qty to select at least one element to add');
                        }
                        ProgressService.unblockScreen();
                        return;
                    } else if (!$scope.isLink && $scope.projectAction == 1 && $scope.wizardData.selectedAssets.find(function (item) {
                        return !item.avg_cost && !item.min_cost && !item.max_cost && !item.last_cost && (!item.unit_budget || item.unit_budget == 0);
                        })) {
                            steppers.error('The asset(s) to be added must have a budget different from 0');
                            ProgressService.unblockScreen();
                            return;
                    }
                    steppers.next();
                    $scope.currentStep = 2;
                    ProgressService.unblockScreen();
                    break;
                case 2:
                    $scope.confirm();
                    break;
                case 3:
                    $scope.add();
                    break;
            }
        }

        $scope.back = function () {
            if ($scope.currentStep > 1) {
                // We want to keep the elements, but there is a bug in the
                // locationsGrid directive where the elements are not kept
                // so we need to clear this. When we fix the grid we should remove this
                $scope.wizardData.selecteds = [];

                if ($scope.currentStep === 1) {
                    $scope.wizardData.selectedAssets = [];
                }

                steppers.clearError();
                steppers.back();
                $scope.currentStep -= 1;
            }
        }

    }]);
